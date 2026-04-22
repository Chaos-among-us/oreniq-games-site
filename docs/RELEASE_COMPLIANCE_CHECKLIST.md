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

## Naming Clearance Workflow
- Do not lock the final game name until a shortlist is reviewed.
- For each candidate name:
  - Search Google Play and the App Store for close game titles.
  - Search the USPTO trademark database for similar marks in software / games.
  - Reject names that are too generic, too close to an existing game, or too close to an active trademark in games/software.
- Only after the shortlist survives that check should package/display renaming happen in code, store assets, and docs.

## Current Game-Specific Notes
- The current code/package identity is still `EndlessDodge1` / `com.oreniq.endlessdodge`.
- The game uses analytics and monetization-related SDKs, so privacy-policy and Data-safety work should be treated as launch-critical.
- Shared release signing should stay outside Git in the external shared-signing folder workflow.

## Next Best Action
- Draft the privacy policy and Data safety answers in parallel with the final naming shortlist, then validate the final chosen name before store assets are exported.
