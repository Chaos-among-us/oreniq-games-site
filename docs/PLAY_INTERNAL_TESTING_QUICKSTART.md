# Play Internal Testing Quickstart

Use this when the goal is to get the current Android test build onto Google Play Internal testing fast.

Official references:
- [Create and set up your app](https://support.google.com/googleplay/android-developer/answer/9859152)
- [Use Play App Signing](https://support.google.com/googleplay/android-developer/answer/9842756)
- [Upload your app to the Play Console](https://developer.android.com/studio/publish/upload-bundle)
- [Set up an open, closed, or internal test](https://support.google.com/googleplay/android-developer/answer/9845334)
- [Provide app privacy and security information](https://support.google.com/googleplay/android-developer/answer/10787469)
- [User Data policy](https://support.google.com/googleplay/android-developer/answer/10144311)
- [Test in-app billing with application licensing](https://support.google.com/googleplay/android-developer/answer/6062777)
- [Google Play Developer API getting started](https://developers.google.com/android-publisher/getting_started)

## Current Project Facts
- App package: `com.oreniq.endlessdodge`
- Unity version: `6000.4.0f1`
- Public game title: `Cavern Veerfall`
- Play upload artifact: `Builds/Android/CavernVeerfall-internal-test.aab`
- Current local blocker: shared signing is still the `shared-debug.keystore` QA bridge. Replace it before building a Play upload.
- QA build note: current tester flow collects tester names, screen recordings, survey answers, and optional notes. Treat this as user data even for testing.

## 1. Create Or Confirm The Play App
1. Open [Play Console](https://play.google.com/console).
2. Click **All apps**.
3. Click **Create app**.
4. Enter the app name.
5. Set default language.
6. Select **Game**.
7. Select **Free** unless you are intentionally launching a paid app.
8. Complete the declarations shown on that page.
9. Click **Create app**.

## 2. Create The Real Upload Keystore
Run this from PowerShell, changing passwords when prompted:

```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.4.0f1\Editor\Data\PlaybackEngines\AndroidPlayer\OpenJDK\bin\keytool.exe" -genkeypair -v -keystore "C:\Users\antho\OneDrive\Documents\EndlessDodge1\SharedSigning\Android\oreniq-release.keystore" -alias oreniq-release -keyalg RSA -keysize 4096 -validity 10000
```

Then update this local-only file:

```text
C:\Users\antho\OneDrive\Documents\EndlessDodge1\SharedSigning\Android\release-signing.json
```

Use this shape:

```json
{
  "keystoreName": "oreniq-release.keystore",
  "keystorePassword": "REAL_KEYSTORE_PASSWORD",
  "keyaliasName": "oreniq-release",
  "keyaliasPassword": "REAL_KEY_PASSWORD"
}
```

Do not commit the keystore or signing JSON.

## 3. Build The AAB
In Unity:
1. Open `C:\Users\antho\EndlessDodge1`.
2. Wait for scripts to finish compiling.
3. Click **Tools > Android > Build Play Internal Test AAB**.
4. Confirm the output exists:
   - `Builds/Android/CavernVeerfall-internal-test.aab`

Batchmode option:

```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.4.0f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\antho\EndlessDodge1" -executeMethod AndroidBuildUtility.BuildPlayInternalTestAabBatchmode -logFile "C:\Users\antho\EndlessDodge1\Logs\android-play-aab-2026-04-24.log"
```

## 4. Enable App Signing And Upload Internal Test
1. In Play Console, open the app.
2. Go to **Testing > Internal testing**.
3. Open the **Releases** tab.
4. Click **Create new release**.
5. In the app signing or app integrity section, accept Play App Signing if prompted.
6. Choose the Google-generated app signing key unless you have a specific reason to bring your own app signing key.
7. Upload `Builds/Android/CavernVeerfall-internal-test.aab`.
8. Let Play Console inspect the bundle.
9. Enter release notes, for example:

```text
First internal QA build. Includes QA mode, auto-recording, post-run survey, and cave-shop tuning.
```

10. Click **Save**.
11. Click **Review release**.
12. Fix any blocking warnings.
13. Click **Start rollout to Internal testing**.

## 4a. App Content And Privacy Checks
For a fast internal-only test, Play Console may let you proceed before every production questionnaire is complete, but do not treat that as production readiness.

Before inviting testers outside the immediate dev team:
1. Add a privacy policy URL if the build collects tester names, recordings, analytics identifiers, crash data, ad identifiers, purchase data, or survey notes.
2. Complete or draft Play Console **App content** sections:
   - Privacy policy
   - App access
   - Ads declaration
   - Content rating
   - Target audience and content
   - Data safety
   - Financial features only if Play asks because of billing setup
3. Make sure the in-app QA disclosure matches the actual capture behavior.
4. Keep the QA recording build limited to internal testing until the privacy policy and Data safety answers are accurate.

## 5. Add Testers And Share The Link
1. Go to **Testing > Internal testing**.
2. Open the **Testers** tab.
3. Click **Create email list** if no list exists.
4. Name it, for example `Cavern Veerfall QA`.
5. Add tester Google Account emails, separated by commas, or upload a CSV.
6. Click **Save changes**.
7. Select the tester list for the internal test.
8. Click **Save changes**.
9. Copy the opt-in link from the internal testing page.
10. Send testers the link and remind them to open it with the same Google account that is on the tester list.

## 6. Billing Test Credentials
Use this after the internal test release exists.

1. In Play Console, go to **Settings > License testing**.
2. Add the same tester Google accounts.
3. Click **Save changes**.
4. In the app, create one-time products for the current product IDs:
   - `com.oreniq.endlessdodge.starter_pack`
   - `com.oreniq.endlessdodge.coin_pack_small`
   - `com.oreniq.endlessdodge.coin_pack_medium`
   - `com.oreniq.endlessdodge.coin_pack_large`
5. Set prices matching the in-game labels for now:
   - `$1.99`
   - `$0.99`
   - `$2.99`
   - `$5.99`
6. Activate the products before testing purchases.

## 7. Optional Publishing API Credentials
This is useful later for automated uploads, but it is not required for the first two-hour internal test.

1. Open [Google Cloud Console](https://console.cloud.google.com/).
2. Create or select a project for Play publishing.
3. Go to **APIs & Services > Library**.
4. Search for **Google Play Android Developer API**.
5. Click **Enable**.
6. Go to **IAM & Admin > Service Accounts**.
7. Click **Create service account**.
8. Name it, for example `endlessdodge-play-publisher`.
9. Create a JSON key only if an automation script needs it.
10. Store the JSON outside the repo.
11. In Play Console, go to **Users and permissions**.
12. Click **Invite new users**.
13. Paste the service account email.
14. Grant only the app/release permissions needed for internal testing uploads.
15. Click **Invite user**.
