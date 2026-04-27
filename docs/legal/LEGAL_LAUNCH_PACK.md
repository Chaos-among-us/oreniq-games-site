# Launch Legal Pack

## Purpose
- Working launch-legal packet for the current `com.oreniq.endlessdodge` Android project.
- Built from the repo state on `2026-04-25`.
- Not legal advice. Treat this as production prep and have counsel review before public release.

## What Is Required Before Google Play Launch
- Public privacy policy URL:
  - Google Play requires a privacy policy for apps on Play, and the policy must be at an active, publicly accessible, non-geofenced, non-editable URL.
  - The policy must clearly identify the developer or app, explain what data is collected, used, shared, retained, and deleted, and provide a privacy contact.
- Accurate Data safety form:
  - All developers must complete the Play Console Data safety form and include data handled by third-party SDKs.
  - The form must match the actual release build, not the temporary QA build.
- App content declarations:
  - Complete the App content page in Play Console.
  - Declare whether the app contains ads.
  - Provide target audience and app-content details.
  - Complete the content rating questionnaire.
  - Provide app-access instructions only if any part of the app is restricted.
- Support contact:
  - A valid user-facing support email is required for publishing on Google Play.
  - Paid-app and in-app purchase support questions must be answered on time.
- Payments compliance:
  - Digital goods and features sold in the Android app must use Google Play Billing unless a documented policy exception applies.
- Prominent disclosure and consent:
  - If the production app continues to collect personal or sensitive user data in a way that is not obvious from the feature itself, Google Play requires an in-app disclosure immediately before the permission/capability request.
  - The disclosure cannot live only in the privacy policy or store listing.

## Strongly Recommended Before Launch
- Terms of Service / EULA:
  - Not a core Google Play listing requirement, but strongly recommended for licenses, virtual currency, purchase rules, disclaimers, refunds, and dispute language.
- Support and refund page:
  - Helps handle billing questions and refund routing cleanly.
- Naming clearance:
  - Trademark and store-title clearance should be done before final assets and store copy are locked.
- Copyright notice:
  - Add a copyright line for `Oreniq Games` in the store listing, website, and any credits/legal page you publish.

## Repo-Specific Status On 2026-04-25
- Privacy policy URL:
  - Prepared in `Assets/Resources/AppLegalConfig.json` as the GitHub Pages path:
    - `https://chaos-among-us.github.io/Block-dodger1/privacy-policy.html`
  - Still needs the placeholder text in `docs/privacy-policy.html` to be finalized and published.
- In-app privacy / legal link:
  - Main-menu privacy button is now wired in `Assets/Scripts/MainMenu.cs`.
  - Terms URL support is also wired through `Assets/Scripts/AppLegalLinks.cs`.
- Support contact:
  - Public support and privacy email are now wired as `oreniqgames@gmail.com`.
  - Public mailing address is now wired as `822 South 480 West, Ogden, UT 84404`.
- Data safety worksheet:
  - Added in `docs/legal/GOOGLE_PLAY_DATA_SAFETY_WORKSHEET.md`.
- Terms draft:
  - Added in `docs/legal/TERMS_OF_SERVICE_DRAFT.md`.
- Privacy policy draft:
  - Added in `docs/legal/PRIVACY_POLICY_DRAFT.md`.
- Public legal-site pages:
  - Added in `docs/index.html`, `docs/privacy-policy.html`, and `docs/terms-of-service.html`.
- Billing implementation:
  - The project uses `com.unity.purchasing` and sells digital items such as coin packs and a starter pack through Unity IAP.
- Analytics / authentication:
  - The project uses `com.unity.services.analytics` and `com.unity.services.authentication`.
- Ads:
  - No live ad SDK was found in `Packages/manifest.json`, but rewarded-ad behavior is simulated in debug/editor flows.
  - Store declarations should stay `No ads` unless a real ad SDK and live ad serving are added.
- QA-only sensitive data:
  - The temporary QA build records screen video, collects tester names and notes, and uploads QA bundles over a LAN HTTP endpoint.
  - These flows should not ship to production unless you intentionally keep them, switch them to production-grade transport, and reflect them in the privacy policy, Data safety answers, and in-app disclosure flow.

## Remaining Placeholders To Fill Before Publishing
- `[effective-date]`
- `[legal-entity-name]`
- `[website-privacy-policy-url]`
- `[governing-law]`
- `[dispute-venue]`

## Recommended Next Steps
1. Pick the public game name and the exact legal entity string you want on the store listing.
2. Finalize the remaining privacy-policy and terms placeholders:
   - effective date
   - legal entity string if it should differ from `Oreniq Games`
   - governing law
   - dispute venue
   - final public privacy-policy hosting URL
3. Decide whether the release build will keep any QA capture or upload code. If not, remove it before Data safety submission.
4. Publish the privacy policy at a public HTTPS URL.
5. Add an in-app Legal / Privacy entry point once the final URLs exist.
6. Complete the Play Console App content and Data safety forms from the production build, not the QA build.

## Official Google Play Sources
- [User Data - Play Console Help](https://support.google.com/googleplay/android-developer/answer/10144311?hl=en)
- [Provide information for Google Play's Data safety section - Play Console Help](https://support.google.com/googleplay/android-developer/answer/10787469?hl=en)
- [Prepare your app for review - Play Console Help](https://support.google.com/googleplay/android-developer/answer/9859455?hl=en)
- [Manage target audience and app content settings - Play Console Help](https://support.google.com/googleplay/android-developer/answer/9867159?hl=en)
- [Content Ratings - Play Console Help](https://support.google.com/googleplay/android-developer/answer/9898843?hl=en)
- [Payments - Play Console Help](https://support.google.com/googleplay/android-developer/answer/9858738?hl=en)
- [How to support your app's users - Play Console Help](https://support.google.com/googleplay/android-developer/answer/113477?hl=en)
- [Best practices for prominent disclosure and consent - Play Console Help](https://support.google.com/googleplay/android-developer/answer/11150561?hl=en)
