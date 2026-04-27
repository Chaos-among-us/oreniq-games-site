# Google Play Data Safety Worksheet

## Purpose
- Working worksheet for the Play Console Data safety form.
- Based on the codebase on `2026-04-25`.
- Not legal advice.
- Final answers must be based on the actual release build you upload.

## Read This First
- The current repo contains both production-intent systems and temporary QA-only capture/upload flows.
- Do not submit Data safety answers from the QA build if those features will be removed before launch.
- Google Play requires Data safety answers to include data handled by third-party libraries and SDKs.

## Current Code Signals
- Unity Analytics package present:
  - `Packages/manifest.json`
  - `Assets/Scripts/Services/LaunchAnalytics.cs`
- Unity Authentication package present:
  - `Packages/manifest.json`
  - `Assets/Scripts/Services/UnityServicesBootstrap.cs`
- Unity IAP package present:
  - `Packages/manifest.json`
  - `Assets/Scripts/Services/MonetizationManager.cs`
- QA-only data capture and upload present:
  - `Assets/Scripts/QaTestingSystem.cs`
  - `Assets/Resources/QaSubmissionConfig.json`
  - `Assets/Plugins/Android/EndlessDodgeQaRecorder.androidlib/src/main/AndroidManifest.xml`

## Likely Production Data Categories

### 1. App activity
- Likely yes.
- Evidence:
  - `run_started`
  - `run_finished`
  - reward, challenge, share, review, and purchase-result events in `LaunchAnalytics.cs`
- Working interpretation:
  - Likely maps to gameplay and in-app interaction analytics.

### 2. Device or other identifiers
- Likely yes.
- Evidence:
  - service-based anonymous authentication
  - app-generated `session_id`
  - likely SDK-managed identifiers used by Unity services
- Working interpretation:
  - Verify exact SDK behavior in the Unity SDK Index / vendor Data safety guidance before submitting.

### 3. Purchase history or in-app purchase data
- Likely yes.
- Evidence:
  - `MonetizationManager.cs` records product IDs, purchase requests, purchase results, and entitlement state for digital goods.
- Working interpretation:
  - Payment card details are not handled directly by the app, but transaction and purchase-state data likely still need disclosure.

### 4. Personal info
- Likely no in the intended public launch build.
- Evidence:
  - no user-facing account profile, name field, or contact form found in the gameplay app
- Caveat:
  - support email interactions outside the app are still real-world personal data, but they are usually handled in your support process rather than the binary itself

### 5. App info and performance
- Unknown from code alone.
- Evidence:
  - no dedicated crash-reporting package was identified in the repo snapshot
- Action:
  - confirm whether Unity services or any later-added SDK auto-collect diagnostic or performance data in the final build

## QA-Only Categories That Should Not Leak Into Production Answers Unless Shipped

### 1. Name
- QA build collects tester names.
- Evidence:
  - tester-name gate and submission field in `QaTestingSystem.cs`

### 2. User-generated content
- QA build collects freeform tester notes and survey responses.

### 3. Photos and videos
- QA build can record gameplay screen video and upload it in a QA ZIP bundle.

### 4. Device metadata tied to QA submissions
- QA build includes device model, operating system, run data, recording status, and build metadata in the uploaded package.

## Working Draft For Play Console Review

### If you remove QA recording/upload before launch
- Start from:
  - App activity: likely `Collected`
  - Device or other IDs: likely `Collected`
  - Purchase history / in-app purchase activity: likely `Collected`
  - Personal info: likely `Not collected in-app`
  - Photos and videos: `Not collected`
  - Audio files: `Not collected`
  - User-generated content: likely `Not collected`
- Still verify:
  - whether analytics data is shared with service providers
  - whether data is encrypted in transit
  - whether users can request deletion of any account-linked data

### If you keep QA recording/upload in the public build
- Expect a much broader Data safety disclosure.
- You will likely need to declare collection of:
  - names
  - user-generated content
  - photos and videos
  - device identifiers / device metadata
  - app activity
- You should also keep the in-app disclosure and consent flow aligned with the data collection.

## Release Decision Gate
- Before filing the final Data safety form, answer this:
  - Does the shipping build still include `QaTestingSystem`, screen recording, tester names, notes, or upload endpoints?
- If `no`:
  - remove those systems from the release build and answer Data safety from the cleaned build.
- If `yes`:
  - keep the disclosure flow, move uploads to secure production infrastructure, and expand the privacy policy accordingly.

## Official Sources
- [Provide information for Google Play's Data safety section - Play Console Help](https://support.google.com/googleplay/android-developer/answer/10787469?hl=en)
- [User Data - Play Console Help](https://support.google.com/googleplay/android-developer/answer/10144311?hl=en)
- [Prepare your app for review - Play Console Help](https://support.google.com/googleplay/android-developer/answer/9859455?hl=en)
