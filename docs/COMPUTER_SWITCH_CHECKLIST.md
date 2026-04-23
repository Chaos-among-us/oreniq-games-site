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
1. First check the shared external signing folder:
   - `Documents/EndlessDodge1/SharedSigning/Android/`
2. The expected primary files are:
   - `release-signing.json`
   - `oreniq-release.keystore`
3. Temporary QA bridge files may instead be:
   - `release-signing.json`
   - `shared-debug.keystore`
4. Supported override environment variables:
   - `%ENDLESSDODGE_SIGNING_CONFIG%`
   - `%ENDLESSDODGE_SIGNING_ROOT%`
5. In Unity, click `Tools/Android/Apply Local Release Signing`.
6. If the phone already has an older machine-debug-signed build, uninstall it once before installing the new shared-signed build.
7. Do not paste signing keys, passwords, or other secrets into `logs.md` or any repo-tracked file.
8. Do not create a replacement keystore unless the original is truly gone and you intentionally want a new signing identity.

## If You Need To Match The Current Phone Install Exactly
1. On the primary PC, run:
   - `powershell -ExecutionPolicy Bypass -File scripts/setup-shared-android-signing.ps1`
2. That seeds the shared signing folder from the best available source:
   - recovered release keystore if present
   - otherwise the current machine's Android debug keystore as a temporary QA bridge
3. Wait for OneDrive to sync `Documents/EndlessDodge1/SharedSigning/Android/` to the other PC.
4. On the other PC, run `scripts/bootstrap-workstation.ps1`, open Unity, and use `Tools/Android/Build And Install Debug APK`.
5. The debug build helper will automatically prefer the shared signing folder, so both PCs can update the same phone install without copying `.android\debug.keystore` by hand.
6. Replace the temporary shared debug bridge with the real release keystore as soon as it is recovered.

## If OneDrive Sync Keeps Failing
1. On the primary PC, run:
   - `powershell -ExecutionPolicy Bypass -File scripts/setup-network-shared-android-signing.ps1`
2. Note the printed UNC path, for example:
   - `\\HOLLAND_WORK_PC\EndlessDodgeSigning`
3. Make sure both PCs are on the same private network and the primary PC stays awake while the secondary PC is building.
4. On the secondary PC, run:
   - `powershell -ExecutionPolicy Bypass -File scripts/use-network-shared-android-signing.ps1 -ShareRoot "\\HOLLAND_WORK_PC\EndlessDodgeSigning"`
5. Restart Unity if it was already open, then use `Tools/Android/Build And Install Debug APK`.
6. This makes the secondary PC read the signing config directly from the primary PC over LAN instead of waiting for OneDrive.

## If You Need A No-Admin Fallback
1. On the primary PC, run:
   - `powershell -ExecutionPolicy Bypass -File scripts/start-network-signing-server.ps1`
2. Note the printed server URL, for example:
   - `http://HOLLAND_WORK_PC:8765/`
3. On the secondary PC, run:
   - `powershell -ExecutionPolicy Bypass -File scripts/sync-network-shared-android-signing.ps1 -ServerRoot "http://HOLLAND_WORK_PC:8765"`
4. Restart Unity if it was already open, then run `scripts/bootstrap-workstation.ps1`.
5. Build with `Tools/Android/Build And Install Debug APK`.
6. After the sync is complete, stop the temporary server on the primary PC with:
   - `powershell -ExecutionPolicy Bypass -File scripts/stop-network-signing-server.ps1`

## If The Repo Is Dirty On Two Computers
1. Preserve the dirty state on a safety branch.
2. Sync `master`.
3. Compare and merge intentionally.
