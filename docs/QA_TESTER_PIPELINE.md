# QA Tester Pipeline

Use this when you want outside testers to:
- download the current build easily
- play with auto-record + in-app survey
- send the finished QA package back in one tap

## What The Game Now Supports
- QA mode can already:
  - auto-record Android gameplay after capture consent
  - collect the post-run survey
  - build a report file
  - save a ZIP bundle locally
  - share through Android's share sheet
- If `Assets/Resources/QaSubmissionConfig.json` contains an `uploadUrl`, the existing `Share QA Package` button turns into a direct `Send QA Package` upload button.

## Config File
Edit:
- `Assets/Resources/QaSubmissionConfig.json`

Fields:
- `uploadUrl`
  - HTTPS endpoint that accepts a multipart POST from the app
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
- `build_version`
- `survey_json`
- `report_text`

The ZIP contains:
- recorded gameplay video when available
- the text QA report
- `qa-metadata.json`

## Suggested Tester Flow
1. Send testers the hosted APK link from `testerBuildUrl`.
2. Tell them to enable QA mode once, then run the tutorial practice once.
3. After each real QA run, they answer the in-app survey and tap `Send QA Package`.
4. The app uploads the package directly if `uploadUrl` is set.
5. If the upload target is blank or unavailable, testers can still use the Android share sheet fallback.

## Notes
- The one-tap return flow depends on the upload endpoint. The app-side button is ready, but the endpoint still needs to exist.
- Keep the upload URL simple and stable. A direct webhook or lightweight upload worker is easier for testers than email-style workflows.
- The current code does not require any secret auth token in the repo config. Prefer an endpoint URL that is safe to embed in the tester build.
