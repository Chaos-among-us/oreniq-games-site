# QA Remote GitHub Upload Setup

Use this to replace the laptop-only LAN collector with a hosted upload endpoint that works for remote testers and saves QA review data back into GitHub.

## What This Setup Does
- Testers still tap one in-app `Send QA Data` button.
- The app uploads to a Cloudflare Worker instead of `http://192.168.88.8:8787/qa-upload`.
- The Worker stores the uploaded ZIP as a GitHub release asset.
- The Worker writes the lightweight analysis files into the repo under:
  - `Builds/QaCollectorInboxRemote/YYYY/MM/DD/<timestamp>-<tester>-<run-id>/`
- Each uploaded folder gets:
  - `manifest.json`
  - `fields.json`
  - `survey.json`
  - `report.txt`
- Both PCs can pull the repo and immediately review the survey/report data.
- The manifest includes a GitHub download URL for the actual QA ZIP and recorded gameplay.
- Because the upload lands on Cloudflare + GitHub, testers do not need to be on your Wi-Fi and this laptop can be off.

## Important Privacy Note
- If `Chaos-among-us/Block-dodger1` is public, uploaded QA assets in that same repo will also be public.
- If you want private tester recordings, create a separate private GitHub repo such as `Block-dodger1-qa` and point the Worker there.

## Click-By-Click Setup

### 1. Create or choose the GitHub repo that will hold QA uploads
1. Open [GitHub](https://github.com).
2. If you want private QA storage, click `+` in the top-right.
3. Click `New repository`.
4. Name it something like `Block-dodger1-qa`.
5. Set visibility to `Private`.
6. Click `Create repository`.

### 2. Create a GitHub token for the Worker
1. In GitHub, click your profile picture.
2. Click `Settings`.
3. In the left sidebar, click `Developer settings`.
4. Click `Personal access tokens`.
5. Click `Fine-grained tokens`.
6. Click `Generate new token`.
7. Give it a name like `Cavern Veerfall QA Worker`.
8. Set the resource owner to your GitHub account.
9. Limit repository access to the repo from step 1, or to `Block-dodger1` if you are using the main repo.
10. Grant these permissions:
    - `Contents`: `Read and write`
    - `Metadata`: `Read-only`
    - `Releases`: `Read and write`
11. Click `Generate token`.
12. Copy the token and keep it ready.

### 3. Create the Cloudflare Worker
1. Open [Cloudflare Dashboard](https://dash.cloudflare.com).
2. In the left sidebar, click `Workers & Pages`.
3. Click `Create`.
4. Under Workers, click `Create Worker`.
5. Give it a simple name like `endless-dodge-qa-upload`.
6. Click `Deploy`.
7. Click `Edit code`.
8. Open this repo file:
   - `deploy/cloudflare/qa-github-upload-worker.js`
9. Copy the entire file.
10. Paste it into the Cloudflare editor, replacing the starter code.
11. Click `Save and deploy`.

### 4. Add the Worker secrets and variables
1. In the Worker dashboard, click `Settings`.
2. Click `Variables`.
3. Under `Environment Variables`, add:
   - `GITHUB_OWNER`
   - Value: `Chaos-among-us`
4. Add:
   - `GITHUB_REPO`
   - Value:
     - `Block-dodger1` if you are using the main repo
     - or your private QA repo name if you created one
5. Add:
   - `GITHUB_BRANCH`
   - Value: `master`
6. Add:
   - `GITHUB_RELEASE_TAG`
   - Value: `qa-submissions`
7. Add:
   - `GITHUB_RELEASE_NAME`
   - Value: `QA Submissions`
8. Add:
   - `GITHUB_MANIFEST_PREFIX`
   - Value: `Builds/QaCollectorInboxRemote`
9. Under `Secrets`, add:
   - `GITHUB_TOKEN`
   - Value: paste the token from step 2
10. Click `Save and deploy` again.

### 5. Test the Worker URL
1. In the Worker overview, copy the Worker URL.
2. Open the URL in a browser.
3. You should see JSON with:
   - `"ok": true`
   - `"uploadPath": "/qa-upload"`

### 6. Point the app at the hosted Worker
1. Open:
   - `Assets/Resources/QaSubmissionConfig.json`
2. Replace the current `uploadUrl` value with:
   - `https://YOUR-WORKER-URL/qa-upload`
3. Save the file.
4. Rebuild the Android QA APK.
5. Install it on the test phone.

### 7. Verify one real QA upload
1. Run the updated QA build on the phone.
2. Finish one recorded run.
3. Tap `Send QA Data`.
4. In GitHub, open the target repo.
5. Confirm a new folder appears under:
   - `Builds/QaCollectorInboxRemote/.../`
6. Confirm that folder contains:
   - `manifest.json`
   - `fields.json`
   - `survey.json`
   - `report.txt`
7. Open the repo's `Releases` page.
8. Confirm a new QA ZIP asset appears under the `qa-submissions` prerelease.

## What To Change On The App Side
- Only `uploadUrl` must change.
- The same app upload contract already works with this Worker.
- No secret token is stored in the app.

## Current Recommended Values
- Worker endpoint:
  - `https://<your-worker-subdomain>.workers.dev/qa-upload`
- Repo manifest path:
  - `Builds/QaCollectorInboxRemote`
- Release tag:
  - `qa-submissions`

## After Setup
- Leave the LAN collector docs in place only as a local fallback.
- Remote testers should use the hosted Worker path, not the laptop LAN path.
- After this is live, the answer to "what if the laptop is off?" becomes "nothing breaks; the upload still succeeds."
