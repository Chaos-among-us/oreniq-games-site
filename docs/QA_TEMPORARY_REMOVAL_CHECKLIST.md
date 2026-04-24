# Temporary QA Removal Checklist

Use this before any closed, open, or production release that is not explicitly a QA recording build.

## Remove Or Disable
- Remove the Main Menu `QA Tester Setup` entry point.
- Disable `QaTestingSystem.SetQaModeEnabled(true)` access in production builds.
- Remove Android screen-recording bridge code and manifest entries for media projection / foreground capture services if production does not record screens.
- Remove `Assets/Resources/QaSubmissionConfig.json` upload configuration from production builds.
- Remove tester-name and freeform QA-notes collection unless the production privacy policy and Data safety form intentionally cover them.
- Remove QA report ZIP creation, Downloads export, and share-sheet copy that references QA packages.
- Remove any QA-only copy in Play release notes and store listing.

## Keep If Still Useful
- General post-run player feedback can stay, but only if it is redesigned as a normal player feedback feature.
- Internal build scripts and `scripts/pull-qa-artifacts.ps1` can remain in the repo because they are not shipped inside the app.

## Production Paperwork
- Complete the Play Data safety form based on the production build only.
- Publish a privacy policy URL before collecting personally identifiable tester data, gameplay recordings, analytics IDs, purchases, or ad identifiers from non-internal users.
- Complete content rating, target audience, ads, and in-app product declarations before a non-internal rollout.
