export default {
  async fetch(request, env) {
    if (request.method === "OPTIONS") {
      return withCors(new Response(null, { status: 204 }));
    }

    const url = new URL(request.url);

    if (request.method === "GET" && url.pathname === "/") {
      return withCors(jsonResponse(200, {
        ok: true,
        service: "Cavern Veerfall GitHub QA Upload Worker",
        uploadPath: "/qa-upload",
        repo: `${env.GITHUB_OWNER || ""}/${env.GITHUB_REPO || ""}`.replace(/^\/|\/$/g, "")
      }));
    }

    if (request.method !== "POST" || url.pathname !== "/qa-upload") {
      return withCors(jsonResponse(404, { ok: false, error: "Use POST /qa-upload" }));
    }

    try {
      const formData = await request.formData();
      const packageFile = formData.get("package");

      if (!(packageFile instanceof File)) {
        return withCors(jsonResponse(400, { ok: false, error: "Missing QA package file." }));
      }

      const maxUploadBytes = Number(env.MAX_UPLOAD_BYTES || 120 * 1024 * 1024);

      if (packageFile.size > maxUploadBytes) {
        return withCors(jsonResponse(413, {
          ok: false,
          error: `QA package is too large. Limit is ${maxUploadBytes} bytes.`
        }));
      }

      const repoOwner = must(env.GITHUB_OWNER, "Missing GITHUB_OWNER");
      const repoName = must(env.GITHUB_REPO, "Missing GITHUB_REPO");
      const branch = env.GITHUB_BRANCH || "master";
      const releaseTag = env.GITHUB_RELEASE_TAG || "qa-submissions";
      const releaseName = env.GITHUB_RELEASE_NAME || "QA Submissions";
      const manifestPrefix = env.GITHUB_MANIFEST_PREFIX || "Builds/QaCollectorInboxRemote";

      const runId = sanitize(formData.get("run_id") || "qa-run");
      const testerName = sanitize(formData.get("tester_name") || "unknown-tester");
      const packageId = stringValue(formData.get("package_id"));
      const buildVersion = stringValue(formData.get("build_version"));
      const surveyJsonRaw = stringValue(formData.get("survey_json"));
      const reportText = stringValue(formData.get("report_text"));
      const surveyData = tryParseJson(surveyJsonRaw);
      const uploadedAt = new Date();
      const timestamp = formatTimestamp(uploadedAt);
      const datedPrefix = `${uploadedAt.getUTCFullYear()}/${pad(uploadedAt.getUTCMonth() + 1)}/${pad(uploadedAt.getUTCDate())}`;
      const baseName = `${timestamp}-${testerName}-${runId}`;
      const assetName = `${baseName}.zip`;
      const repoFolderPath = `${manifestPrefix}/${datedPrefix}/${baseName}`;
      const manifestPath = `${repoFolderPath}/manifest.json`;
      const fieldsPath = `${repoFolderPath}/fields.json`;
      const surveyPath = `${repoFolderPath}/survey.json`;
      const reportPath = `${repoFolderPath}/report.txt`;

      const release = await ensureRelease(env, repoOwner, repoName, releaseTag, releaseName);
      const packageBytes = await packageFile.arrayBuffer();
      const asset = await uploadReleaseAsset(env, release.upload_url, assetName, packageBytes);

      const manifest = {
        schemaVersion: "1",
        uploadedAtUtc: uploadedAt.toISOString(),
        runId,
        testerName,
        packageId,
        buildVersion,
        packageSizeBytes: packageFile.size,
        assetName,
        assetDownloadUrl: asset.browser_download_url,
        releaseHtmlUrl: release.html_url,
        manifestPath,
        surveyPath,
        reportPath,
        fieldsPath,
        survey: surveyData,
        surveyJsonRaw,
        reportText
      };

      const fields = {
        schemaVersion: "1",
        uploadedAtUtc: uploadedAt.toISOString(),
        runId,
        testerName,
        packageId,
        buildVersion,
        packageSizeBytes: packageFile.size,
        assetName,
        assetDownloadUrl: asset.browser_download_url,
        releaseHtmlUrl: release.html_url,
        manifestPath
      };

      const repoFiles = [
        {
          path: manifestPath,
          content: withTrailingNewline(JSON.stringify(manifest, null, 2))
        },
        {
          path: fieldsPath,
          content: withTrailingNewline(JSON.stringify(fields, null, 2))
        },
        {
          path: surveyPath,
          content: withTrailingNewline(
            surveyData == null
              ? (surveyJsonRaw || "{}")
              : JSON.stringify(surveyData, null, 2))
        },
        {
          path: reportPath,
          content: withTrailingNewline(reportText || "")
        }
      ];

      await writeRepoFiles(
        env,
        repoOwner,
        repoName,
        branch,
        repoFiles,
        `qa: add ${baseName} package metadata`
      );

      return withCors(jsonResponse(200, {
        ok: true,
        savedTo: manifestPath,
        assetDownloadUrl: asset.browser_download_url,
        releaseHtmlUrl: release.html_url,
        runId,
        testerName
      }));
    } catch (error) {
      return withCors(jsonResponse(500, {
        ok: false,
        error: error instanceof Error ? error.message : String(error)
      }));
    }
  }
};

async function ensureRelease(env, owner, repo, tag, name) {
  const existing = await githubFetch(
    env,
    `https://api.github.com/repos/${owner}/${repo}/releases/tags/${encodeURIComponent(tag)}`,
    { method: "GET", allow404: true }
  );

  if (existing.status !== 404) {
    return existing.json;
  }

  const created = await githubFetch(
    env,
    `https://api.github.com/repos/${owner}/${repo}/releases`,
    {
      method: "POST",
      body: JSON.stringify({
        tag_name: tag,
        name,
        draft: false,
        prerelease: true,
        generate_release_notes: false
      })
    }
  );

  return created.json;
}

async function uploadReleaseAsset(env, uploadUrlTemplate, assetName, assetBuffer) {
  const uploadUrl = uploadUrlTemplate.replace(/\{[^}]+\}$/, "");
  const response = await fetch(`${uploadUrl}?name=${encodeURIComponent(assetName)}`, {
    method: "POST",
    headers: {
      "Authorization": `Bearer ${must(env.GITHUB_TOKEN, "Missing GITHUB_TOKEN")}`,
      "User-Agent": "endless-dodge-qa-worker",
      "Accept": "application/vnd.github+json",
      "Content-Type": "application/zip",
      "Content-Length": String(assetBuffer.byteLength)
    },
    body: assetBuffer
  });

  if (!response.ok) {
    throw new Error(`GitHub asset upload failed: ${response.status} ${await response.text()}`);
  }

  return response.json();
}

async function writeRepoFiles(env, owner, repo, branch, files, message) {
  const refName = encodeGitRefPart(branch);
  const headRef = await githubFetch(
    env,
    `https://api.github.com/repos/${owner}/${repo}/git/ref/heads/${refName}`,
    {
      method: "GET"
    }
  );
  const headCommitSha = headRef.json.object.sha;
  const headCommit = await githubFetch(
    env,
    `https://api.github.com/repos/${owner}/${repo}/git/commits/${headCommitSha}`,
    {
      method: "GET"
    }
  );
  const tree = await githubFetch(
    env,
    `https://api.github.com/repos/${owner}/${repo}/git/trees`,
    {
      method: "POST",
      body: JSON.stringify({
        base_tree: headCommit.json.tree.sha,
        tree: files.map(file => ({
          path: file.path,
          mode: "100644",
          type: "blob",
          content: file.content
        }))
      })
    }
  );
  const commit = await githubFetch(
    env,
    `https://api.github.com/repos/${owner}/${repo}/git/commits`,
    {
      method: "POST",
      body: JSON.stringify({
        message,
        tree: tree.json.sha,
        parents: [headCommitSha]
      })
    }
  );

  await githubFetch(
    env,
    `https://api.github.com/repos/${owner}/${repo}/git/refs/heads/${refName}`,
    {
      method: "PATCH",
      body: JSON.stringify({
        sha: commit.json.sha,
        force: false
      })
    }
  );
}

async function githubFetch(env, url, options) {
  const response = await fetch(url, {
    method: options.method || "GET",
    headers: {
      "Authorization": `Bearer ${must(env.GITHUB_TOKEN, "Missing GITHUB_TOKEN")}`,
      "User-Agent": "endless-dodge-qa-worker",
      "Accept": "application/vnd.github+json",
      "Content-Type": "application/json"
    },
    body: options.body
  });

  if (options.allow404 && response.status === 404) {
    return { status: 404, json: null };
  }

  if (!response.ok) {
    throw new Error(`GitHub API failed: ${response.status} ${await response.text()}`);
  }

  return { status: response.status, json: await response.json() };
}

function encodeGitRefPart(value) {
  return value
    .split("/")
    .map(encodeURIComponent)
    .join("/");
}

function tryParseJson(value) {
  if (!value) {
    return null;
  }

  try {
    return JSON.parse(value);
  } catch {
    return null;
  }
}

function stringValue(value) {
  return value == null ? "" : String(value);
}

function sanitize(value) {
  return stringValue(value)
    .trim()
    .replace(/[^a-z0-9._-]+/gi, "-")
    .replace(/^-+|-+$/g, "")
    .slice(0, 80) || "unknown";
}

function formatTimestamp(date) {
  return `${date.getUTCFullYear()}${pad(date.getUTCMonth() + 1)}${pad(date.getUTCDate())}-${pad(date.getUTCHours())}${pad(date.getUTCMinutes())}${pad(date.getUTCSeconds())}`;
}

function pad(value) {
  return String(value).padStart(2, "0");
}

function must(value, message) {
  if (!value) {
    throw new Error(message);
  }

  return value;
}

function jsonResponse(status, payload) {
  return new Response(JSON.stringify(payload, null, 2), {
    status,
    headers: {
      "Content-Type": "application/json; charset=utf-8",
      "Cache-Control": "no-store"
    }
  });
}

function withCors(response) {
  response.headers.set("Access-Control-Allow-Origin", "*");
  response.headers.set("Access-Control-Allow-Methods", "GET,POST,OPTIONS");
  response.headers.set("Access-Control-Allow-Headers", "Content-Type");
  return response;
}

function withTrailingNewline(text) {
  return text.endsWith("\n") ? text : `${text}\n`;
}
