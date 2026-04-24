# QA Tester Pipeline

Use this when you want outside testers to:
- download the current build easily
- play with auto-record + in-app survey
- send the finished QA package back in one tap

## What The Game Now Supports
- QA mode can already:
  - require a tester name before recording starts
  - auto-record Android gameplay after capture consent
  - show an in-app QA tutorial/disclaimer before the Android capture prompt
  - collect the post-run survey, including freeform tester notes
  - build a report file
  - save a ZIP bundle locally
  - upload through the one-tap QA data button
- `Assets/Resources/QaSubmissionConfig.json` is currently pointed at the local repo collector on `http://10.0.0.7:8787/qa-upload`.

## Config File
Edit:
- `Assets/Resources/QaSubmissionConfig.json`

Fields:
- `uploadUrl`
  - endpoint that accepts a multipart POST from the app
  - for the temporary LAN QA bridge, start `scripts/start-qa-collector.ps1` before testers submit
  - when blank, the app falls back to the Android share sheet
- `submissionButtonLabel`
  - button text shown in the post-run QA survey
- `testerBuildUrl`
  - the hosted download link you send to testers for the APK
  - included in the generated QA metadata for reference
- `testerSurveyUrl`
  - optional external survey link for your own tracking
  - included in the generated QA metadata for reference

## Upload Contract
When `uploadUrl` is configured, the app sends a multipart form POST with:
- `package`
  - ZIP file containing the QA bundle
- `run_id`
- `package_id`
- `tester_name`
- `build_version`
- `survey_json`
- `report_text`

## Local Repo Collector
For the current temporary QA build, run this on the repo computer before a tester taps `Send QA Data`:

```powershell
powershell -ExecutionPolicy Bypass -File scripts\start-qa-collector.ps1
```

The collector listens on port `8787` and stores each received submission under:
- `Builds/QaCollectorInbox/<timestamp>-<tester>-<run-id>/`

Each submission folder contains:
- the uploaded QA ZIP
- `fields.json`
- `survey.json`
- `report.txt`

The phone and repo computer must be on the same network, and Windows Firewall must allow the local Node.js server to receive the connection. If the computer's Wi-Fi IP changes, update `Assets/Resources/QaSubmissionConfig.json` and rebuild the QA app.

The ZIP contains:
- recorded gameplay video when available
- the text QA report
- `qa-metadata.json`

Current in-game survey fields:
- collision feel
- difficulty curve
- reward feel
- one-more-run pull
- shop prices
- shop reward value
- ad offer balance
- danger multiplier balance
- feature clarity
- optional tester notes

## Suggested Tester Flow
1. Send testers the hosted APK link from `testerBuildUrl`.
2. Tell them to open `QA Tester Setup`, enter their tester name, read the disclosure, and run the tutorial practice once.
3. For real runs, they enable QA recording and accept Android's screen-capture prompt.
4. After each real QA run, they answer the in-app survey, add notes if needed, and tap `Send QA Data`.
5. The app uploads the package directly to the repo collector if it is running.
6. If the upload fails, start or check the collector, then have the tester tap `Send QA Data` again.

## Pull Packages From A Connected Phone
When the phone is connected by USB or wireless ADB, pull the saved QA handoff files into a timestamped desktop folder:

```powershell
powershell -ExecutionPolicy Bypass -File scripts\pull-qa-artifacts.ps1
```

Output goes under:
- `Builds/PhoneQaArtifacts/<timestamp>/`

Use this lighter report/package-only pull when you do not need large gameplay videos:

```powershell
powershell -ExecutionPolicy Bypass -File scripts\pull-qa-artifacts.ps1 -SkipVideos
```

## Notes
- The local repo collector is a temporary LAN bridge for QA builds. For remote Play testers who are not on the same network, replace it with a hosted endpoint before relying on one-tap uploads.
- Keep the upload URL simple and stable. A direct webhook or lightweight upload worker is easier for testers than email-style workflows.
- The current code does not require any secret auth token in the repo config. Prefer an endpoint URL that is safe to embed in the tester build.
- Do not embed a GitHub token in the app to write directly to the repo. If repo-backed storage is desired, use a small serverless endpoint or GitHub Action dispatch proxy that owns the secret outside the app.
- QA mode, screen-recording permissions, QA reports, upload config, and tester-name capture are temporary internal-test features. Remove them before a production release unless the production privacy policy and Data safety answers intentionally cover them.
