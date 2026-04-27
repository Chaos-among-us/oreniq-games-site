# Release Compliance Checklist

## Purpose
- This is the practical release-readiness checklist for Play Store launch work.
- It is not legal advice.
- Use it to track the minimum privacy, policy, naming, and store-prep tasks that should be complete before publishing.

## Required Before Google Play Release
- Release artifact / platform basics:
  - Ship a release Android App Bundle (`.aab`) for Play upload, not only a debug APK.
  - Confirm Play App Signing is configured for the release track.
  - Confirm the release build targets the current Play-required target API level.
- Privacy policy:
  - Publish a public privacy policy URL for the game.
  - Include the app name or developer entity (`Oreniq Games`) in the policy.
  - Explain analytics, authentication, IAP, crash/performance data, retention, deletion/contact, and any third-party SDK use.
- Data safety:
  - Complete the Play Console Data safety form accurately for all SDKs and app behavior.
  - Recheck the declarations whenever analytics, ads, auth, cloud save, or social features change.
- Content rating:
  - Complete the IARC content-rating questionnaire in Play Console.
  - Re-submit it if game content or monetization changes materially.
- Target audience / content:
  - Set the intended audience range in Play Console accurately.
  - If children become a target audience later, re-check Families policy constraints before shipping.
- Store listing basics:
  - Final title
  - Short description
  - Full description
  - Feature graphic / icon
  - Phone screenshots
  - Support contact email
  - Privacy policy URL
- Monetization disclosures:
  - Keep IAP metadata accurate.
  - If ads are added, declare `Contains ads` in Play Console.
  - If ads are added, update Data safety / policy declarations and check ad-content requirements.
  - Make sure rewarded ads are appropriate for the app's content rating.

## Confirmed Google Play Requirements Checked On 2026-04-26
- Privacy policy:
  - Google Play's User Data policy says all apps must post a privacy policy link in Play Console and a privacy policy link or text within the app itself.
  - The privacy policy must be on an active, public, non-geofenced URL and should include developer information, a privacy contact method, data use/sharing details, security handling, and retention/deletion information.
- Data safety:
  - Google Play says every published app package on closed, open, or production tracks must complete the Data safety form.
  - Even apps that say they collect no user data still must complete the form and provide a privacy policy link.
- App content:
  - App content declarations should be treated as launch blockers until complete: privacy policy, ads declaration, app access instructions if needed, target audience/content, content rating, and any sensitive-permission declarations.
- Support contact:
  - Play requires a valid user-facing support email on the store listing.
  - Google also strongly recommends a website for user support.
- Developer account verification:
  - All Play developer accounts need verified Google-contact email and phone details.
  - Personal accounts must provide a developer website.
  - Organization accounts must provide an organization website and additional public contact details.
- API level:
  - The current Play help article says app updates must target Android 14 / API level 34 or higher.

## Not Strictly A Play Requirement But Still Strongly Recommended
- Terms of Service:
  - A public Terms of Service page is useful for IP ownership, license scope, refunds, virtual items, disclaimers, and dispute language.
  - I did not find an official Play article that makes a standalone Terms of Service page mandatory for a standard mobile game launch, so treat this as recommended legal posture rather than a Play-specific checkbox.

## Naming Clearance Workflow
- Do not lock the final game name until a shortlist is reviewed.
- For each candidate name:
  - Search Google Play and the App Store for close game titles.
  - Search the USPTO trademark database for similar marks in software / games.
  - Reject names that are too generic, too close to an existing game, or too close to an active trademark in games/software.
- Only after the shortlist survives that check should package/display renaming happen in code, store assets, and docs.

## Current Game-Specific Notes
- The chosen public title is now `Cavern Veerfall`.
- The Android package id remains `com.oreniq.endlessdodge` for install/update continuity.
- The game uses analytics and monetization-related SDKs, so privacy-policy and Data-safety work should be treated as launch-critical.
- Shared release signing should stay outside Git in the external shared-signing folder workflow.
- Launch-legal working drafts now live in:
  - `docs/legal/LEGAL_LAUNCH_PACK.md`
  - `docs/legal/PRIVACY_POLICY_DRAFT.md`
  - `docs/legal/TERMS_OF_SERVICE_DRAFT.md`
  - `docs/legal/GOOGLE_PLAY_DATA_SAFETY_WORKSHEET.md`
- Public website and legal pages now live in:
  - `docs/index.html`
  - `docs/games/cavern-veerfall.html`
  - `docs/support.html`
  - `docs/privacy-policy.html`
  - `docs/terms-of-service.html`
- In-app privacy-link config now lives in:
  - `Assets/Resources/AppLegalConfig.json`
- Public support / privacy email chosen:
  - `oreniqgames@gmail.com`
- Public mailing address chosen for support/legal use:
  - `822 South 480 West, Ogden, UT 84404`

## Next Best Action
- Finalize the remaining legal placeholders, decide whether QA capture survives into release, then publish the privacy policy URL and complete the Play Console Data safety and App content declarations from the production build.
