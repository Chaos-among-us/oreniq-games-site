# Workstation Sync

## Goal
- Make this project easy to resume on any computer without depending on chat history or memory.
- Keep shared project state in Git.
- Keep machine-local secrets and signing files out of Git.

## Source Of Truth
Read files in this order after switching machines:
1. `logs.md`
2. `ROADMAP.md`
3. `RELEASE_SPRINT.md`
4. `docs/COMPUTER_SWITCH_CHECKLIST.md`

## What Must Sync Through Git
- All code under `Assets/`
- Scene files and project settings
- Strategy and handoff docs
- Templates and workstation setup scripts

## What Must Stay Local
- `Library/`
- `Logs/`
- `UserSettings/`
- Android release signing files:
  - `UserSettings/Android/release-signing.json`
  - `UserSettings/Android/oreniq-release.keystore`
  - or the shared external equivalents in `Documents/EndlessDodge1/SharedSigning/Android/`
- Any signing passwords, license keys, or other secret values used by release/signing services:
  - keep these out of `logs.md`
  - keep these out of all repo-tracked files
  - use the shared external signing folder or environment variables instead

## Standard Workflow
### Leaving A Computer
1. Run `git status --short --branch`.
2. Update `logs.md` with a short structured entry.
3. Commit and push all shared changes.
4. If the repo is dirty and you cannot finish the sync cleanly, create a safety branch like `codex/local-snapshot-YYYY-MM-DD`.

### Arriving On A New Computer
1. Pull the latest repo state.
2. Run:
   - `powershell -ExecutionPolicy Bypass -File scripts/bootstrap-workstation.ps1`
3. Open the project in Unity `6000.4.0f1`.
4. Let packages restore and scripts compile.
5. If you need the same Android app identity across multiple PCs, put the signing files in the shared external signing folder or set the signing environment variables before building.

## Android Signing Rule
- Never create a new release keystore if the old one still exists anywhere.
- A new keystore means a different Android signing identity.
- Missing local signing data should not block day-to-day editor work, only signed release builds.
- Canonical shared path for cross-PC builds:
  - `Documents/EndlessDodge1/SharedSigning/Android/`
- Seed or refresh that folder from the primary PC with:
  - `powershell -ExecutionPolicy Bypass -File scripts/setup-shared-android-signing.ps1`
- Optional lower-latency LAN fallback if OneDrive sync is misbehaving:
  - on the primary PC, share that same folder with:
    - `powershell -ExecutionPolicy Bypass -File scripts/setup-network-shared-android-signing.ps1`
  - on the secondary PC, point Unity at the desktop's UNC share with:
    - `powershell -ExecutionPolicy Bypass -File scripts/use-network-shared-android-signing.ps1 -ShareRoot "\\PRIMARY-PC\EndlessDodgeSigning"`
  - note:
    - creating the SMB share usually needs an elevated PowerShell window on the primary PC
- No-admin fallback that works from this repo today:
  - on the primary PC, start a temporary local-network file server with:
    - `powershell -ExecutionPolicy Bypass -File scripts/start-network-signing-server.ps1`
  - on the secondary PC, download a local cache and set `%ENDLESSDODGE_SIGNING_ROOT%` with:
    - `powershell -ExecutionPolicy Bypass -File scripts/sync-network-shared-android-signing.ps1 -ServerRoot "http://PRIMARY-PC:8765"`
  - after the laptop syncs, stop the server on the primary PC with:
    - `powershell -ExecutionPolicy Bypass -File scripts/stop-network-signing-server.ps1`
- Resolver priority is now:
  - `%ENDLESSDODGE_SIGNING_CONFIG%`
  - `%ENDLESSDODGE_SIGNING_ROOT%`
  - shared signing folder
  - project-local `UserSettings/Android`
- This is intentional so a stale machine-local file cannot silently override the team-shared signing identity.
- Supported overrides:
  - `%ENDLESSDODGE_SIGNING_CONFIG%`
  - `%ENDLESSDODGE_SIGNING_ROOT%`
- `Tools/Android/Build Debug APK` now reuses the resolved shared signing config automatically when available, so the same phone install can be updated from different PCs without committing secrets.
- If the phone already has an older machine-debug-signed build installed, uninstall it once before moving over to the shared-signed build identity.
- Do not paste signing values or other secrets into the repo-backed handoff documents even for temporary convenience.
- Temporary fallback if the currently installed phone app came from one PC's debug keystore:
  - run `scripts/setup-shared-android-signing.ps1` on the primary PC
  - this copies the current machine's debug keystore into the shared external signing folder as `shared-debug.keystore`
  - the shared config then lets both PCs update the same debug-installed app without copying `.android\debug.keystore` by hand
  - if OneDrive sync itself is the fragile part, expose that same folder as a read-only LAN share and point the secondary PC at the UNC path with `%ENDLESSDODGE_SIGNING_ROOT%`
  - do not commit the debug keystore to Git
  - this is only a QA bridge; move back to the shared release-signing workflow once the real signing files are recovered

## Shared Templates
- Repo-tracked template:
  - `config/templates/android-release-signing.template.json`
- Local generated copy:
  - `UserSettings/Android/release-signing.template.json`

## Recovery Rule
- If two computers drift and you are not sure what is newest:
  1. preserve the dirty state on a safety branch
  2. sync `master`
  3. compare the safety branch against `master`
