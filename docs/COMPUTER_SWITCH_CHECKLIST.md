# Computer Switch Checklist

## Before Leaving A Computer
1. Save work in Unity and wait for imports to finish.
2. Run `git status --short --branch`.
3. Update `logs.md` with:
   - goal
   - what changed
   - verification
   - next best action
4. Commit and push shared changes.
5. If you must leave with uncommitted work, create a safety branch before switching devices.

## On The New Computer
1. Pull the latest repo state.
2. Run `powershell -ExecutionPolicy Bypass -File scripts/bootstrap-workstation.ps1`.
3. Open the project in Unity `6000.4.0f1`.
4. Wait for import and compile to finish.
5. Check the Console for blocking errors.
6. Read `logs.md` before deciding what to work on next.

## If You Need Android Release Signing
1. Open `UserSettings/Android`.
2. Confirm whether these files exist:
   - `release-signing.json`
   - `oreniq-release.keystore`
3. On the previous machine, first check that repo's `UserSettings/Android` folder.
4. If they are not there, search the previous machine for:
   - `oreniq-release.keystore`
   - `release-signing.json`
5. Copy both files into the current machine's `UserSettings/Android`.
6. In Unity, click `Tools/Android/Apply Local Release Signing`.
7. Do not create a replacement keystore unless the original is truly gone and you intentionally want a new signing identity.

## If The Repo Is Dirty On Two Computers
1. Preserve the dirty state on a safety branch.
2. Sync `master`.
3. Compare and merge intentionally.
