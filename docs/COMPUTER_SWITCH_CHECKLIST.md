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
7. If continuing the current mobile pass, start with the newest verified Android build context:
   - `Logs/codex-android-build-15.log`
   - `Builds/Android/EndlessDodge1-debug.apk`
8. First phone retest should be:
   - gameplay music loudness
   - ambient loop click/pop at the end of the cycle
   - music tempo/energy
   - post-run double-coins rewarded button
   - mid-run rewarded revive prompt

## If You Need Android Release Signing
1. Open `UserSettings/Android`.
2. Confirm whether these files exist:
   - `release-signing.json`
   - `oreniq-release.keystore`
3. Also check the shared external signing folder:
   - `Documents/EndlessDodge1/SharedSigning/Android/`
4. On the previous machine, first check that repo's `UserSettings/Android` folder.
5. If they are not there, search the previous machine for:
   - `oreniq-release.keystore`
   - `release-signing.json`
6. Put both files either in the current machine's `UserSettings/Android` or in the shared external signing folder.
7. Supported override environment variables:
   - `%ENDLESSDODGE_SIGNING_CONFIG%`
   - `%ENDLESSDODGE_SIGNING_ROOT%`
8. In Unity, click `Tools/Android/Apply Local Release Signing`.
9. If the phone already has an older machine-debug-signed build, uninstall it once before installing the new shared-signed build.
10. Do not paste signing keys, passwords, or other secrets into `logs.md` or any repo-tracked file.
11. Do not create a replacement keystore unless the original is truly gone and you intentionally want a new signing identity.

## If You Need To Match This PC's Current Debug Install Exactly
1. Current verified phone build from this PC used the local Android debug keystore, not shared signing yet.
2. On this PC, the current debug-keystore file is:
   - `C:\Users\antho\.android\debug.keystore`
3. Copy that file to the other PC here:
   - `%USERPROFILE%\.android\debug.keystore`
4. Do not commit this file to Git.
5. After copying it, debug APKs built on the other PC should be able to update the same currently installed phone app without uninstalling first.
6. Treat this as a temporary bridge only until the shared/release signing files are recovered and configured.

## If The Repo Is Dirty On Two Computers
1. Preserve the dirty state on a safety branch.
2. Sync `master`.
3. Compare and merge intentionally.
