import { createServer } from "node:http";
import { mkdirSync, writeFileSync } from "node:fs";
import { dirname, join, resolve } from "node:path";
import { fileURLToPath } from "node:url";

const scriptDirectory = dirname(fileURLToPath(import.meta.url));
const repoRoot = resolve(scriptDirectory, "..");
const args = parseArgs(process.argv.slice(2));
const port = Number(args.port || process.env.QA_COLLECTOR_PORT || 8787);
const host = args.host || process.env.QA_COLLECTOR_HOST || "0.0.0.0";
const outputRoot = resolve(args.output || process.env.QA_COLLECTOR_OUTPUT || join(repoRoot, "Builds", "QaCollectorInbox"));
const maxBytes = Number(args.maxBytes || process.env.QA_COLLECTOR_MAX_BYTES || 450 * 1024 * 1024);

mkdirSync(outputRoot, { recursive: true });

const server = createServer((request, response) => {
  if (request.method === "GET" && request.url === "/") {
    sendJson(response, 200, {
      ok: true,
      service: "Endless Dodge QA Collector",
      uploadPath: "/qa-upload",
      outputRoot
    });
    return;
  }

  if (request.method !== "POST" || request.url !== "/qa-upload") {
    sendJson(response, 404, { ok: false, error: "Use POST /qa-upload" });
    return;
  }

  const contentType = request.headers["content-type"] || "";
  const boundaryMatch = /boundary=([^;]+)/i.exec(contentType);

  if (!boundaryMatch) {
    sendJson(response, 400, { ok: false, error: "Missing multipart boundary" });
    return;
  }

  const chunks = [];
  let totalBytes = 0;

  request.on("data", (chunk) => {
    totalBytes += chunk.length;

    if (totalBytes > maxBytes) {
      request.destroy();
      return;
    }

    chunks.push(chunk);
  });

  request.on("end", () => {
    try {
      const body = Buffer.concat(chunks);
      const parts = parseMultipart(body, boundaryMatch[1]);
      const fields = {};
      let packagePart = null;

      for (const part of parts) {
        if (!part.name) {
          continue;
        }

        if (part.filename) {
          packagePart = part;
        } else {
          fields[part.name] = part.body.toString("utf8");
        }
      }

      if (!packagePart) {
        sendJson(response, 400, { ok: false, error: "No QA package file was attached" });
        return;
      }

      const runId = sanitize(fields.run_id || "qa-run");
      const testerName = sanitize(fields.tester_name || "unknown-tester");
      const timestamp = formatTimestamp(new Date());
      const submissionDirectory = join(outputRoot, `${timestamp}-${testerName}-${runId}`);
      mkdirSync(submissionDirectory, { recursive: true });

      const packageName = sanitize(packagePart.filename || `${runId}.zip`);
      const packagePath = join(submissionDirectory, packageName.endsWith(".zip") ? packageName : `${packageName}.zip`);
      writeBuffer(packagePath, packagePart.body);

      writeFileSync(join(submissionDirectory, "fields.json"), JSON.stringify(fields, null, 2), "utf8");

      if (fields.survey_json) {
        writeFileSync(join(submissionDirectory, "survey.json"), fields.survey_json, "utf8");
      }

      if (fields.report_text) {
        writeFileSync(join(submissionDirectory, "report.txt"), fields.report_text, "utf8");
      }

      console.log(`[${new Date().toISOString()}] Received ${packageName} from ${testerName} (${runId})`);
      sendJson(response, 200, {
        ok: true,
        savedTo: submissionDirectory,
        package: packagePath,
        runId,
        testerName
      });
    } catch (error) {
      console.error(error);
      sendJson(response, 500, { ok: false, error: error.message || String(error) });
    }
  });

  request.on("error", (error) => {
    console.error(error);
  });
});

server.listen(port, host, () => {
  console.log("Endless Dodge QA collector is running.");
  console.log(`Listening on http://${host}:${port}/qa-upload`);
  console.log(`Saving submissions to ${outputRoot}`);
});

function parseArgs(rawArgs) {
  const result = {};

  for (let i = 0; i < rawArgs.length; i++) {
    const arg = rawArgs[i];

    if (!arg.startsWith("--")) {
      continue;
    }

    const key = arg.slice(2);
    const value = rawArgs[i + 1] && !rawArgs[i + 1].startsWith("--") ? rawArgs[++i] : "true";
    result[key] = value;
  }

  return result;
}

function parseMultipart(buffer, boundary) {
  const delimiter = Buffer.from(`--${boundary}`);
  const segments = splitBuffer(buffer, delimiter);
  const parts = [];

  for (const segment of segments) {
    let part = trimMultipartSegment(segment);

    if (part.length === 0 || part.equals(Buffer.from("--"))) {
      continue;
    }

    const headerEnd = part.indexOf(Buffer.from("\r\n\r\n"));

    if (headerEnd < 0) {
      continue;
    }

    const headerText = part.subarray(0, headerEnd).toString("latin1");
    let body = part.subarray(headerEnd + 4);

    if (body.length >= 2 && body[body.length - 2] === 13 && body[body.length - 1] === 10) {
      body = body.subarray(0, body.length - 2);
    }

    const disposition = /content-disposition:\s*form-data;([^\r\n]+)/i.exec(headerText);

    if (!disposition) {
      continue;
    }

    const name = /name="([^"]+)"/i.exec(disposition[1])?.[1] || "";
    const filename = /filename="([^"]+)"/i.exec(disposition[1])?.[1] || "";
    parts.push({ name, filename, body });
  }

  return parts;
}

function splitBuffer(buffer, delimiter) {
  const result = [];
  let start = 0;

  while (true) {
    const index = buffer.indexOf(delimiter, start);

    if (index < 0) {
      result.push(buffer.subarray(start));
      break;
    }

    result.push(buffer.subarray(start, index));
    start = index + delimiter.length;
  }

  return result;
}

function trimMultipartSegment(buffer) {
  let start = 0;
  let end = buffer.length;

  while (start < end && (buffer[start] === 13 || buffer[start] === 10)) {
    start++;
  }

  while (end > start && (buffer[end - 1] === 13 || buffer[end - 1] === 10)) {
    end--;
  }

  return buffer.subarray(start, end);
}

function writeBuffer(path, buffer) {
  writeFileSync(path, buffer);
}

function formatTimestamp(date) {
  const pad = (value) => String(value).padStart(2, "0");
  return `${date.getFullYear()}${pad(date.getMonth() + 1)}${pad(date.getDate())}-${pad(date.getHours())}${pad(date.getMinutes())}${pad(date.getSeconds())}`;
}

function sanitize(value) {
  const cleaned = String(value || "")
    .replace(/[^a-z0-9._-]+/gi, "-")
    .replace(/^-+|-+$/g, "")
    .slice(0, 80);

  return cleaned || "unknown";
}

function sendJson(response, status, payload) {
  const body = JSON.stringify(payload, null, 2);
  response.writeHead(status, {
    "content-type": "application/json; charset=utf-8",
    "cache-control": "no-store"
  });
  response.end(body);
}
