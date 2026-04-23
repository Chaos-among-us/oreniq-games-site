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
- Preferred shared path for cross-PC builds:
  - `Documents/EndlessDodge1/SharedSigning/Android/`
- Supported overrides:
  - `%ENDLESSDODGE_SIGNING_CONFIG%`
  - `%ENDLESSDODGE_SIGNING_ROOT%`
- `Tools/Android/Build Debug APK` now reuses the resolved shared signing config automatically when available, so the same phone install can be updated from different PCs without committing secrets.
- If the phone already has an older machine-debug-signed build installed, uninstall it once before moving over to the shared-signed build identity.
- Do not paste signing values or other secrets into the repo-backed handoff documents even for temporary convenience.
- Temporary fallback if the currently installed phone app came from one PC's debug keystore:
  - copy that PC's `%USERPROFILE%\.android\debug.keystore` to the same path on the other PC
  - this lets both PCs update the same debug-installed app while shared signing is still being recovered
  - do not commit the debug keystore to Git
  - move back to the shared-signing workflow once the real signing files are recovered

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
