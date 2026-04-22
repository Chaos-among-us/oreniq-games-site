# Block Dodger / EndlessDodge1 Project Handoff

## Document Role
- This file is the operational source of truth for project state, handoffs, and session continuity.
- Read this file first when resuming work on any machine or in any new chat.
- Use `ROADMAP.md` for product strategy and `RELEASE_SPRINT.md` for the active launch checklist.
- Do not assume one fixed absolute path is correct on every machine. Always work from the current local checkout path for the machine you are on.

## Standard Structure Contract
- Keep this heading order stable so future sessions stay easy to scan.
- Update these sections after each meaningful work session:
  - `Project Snapshot`
  - `Current Focus`
  - `Open Risks And Blockers`
  - `Structured Change Log`
- Append new dated entries instead of dumping full chat transcripts.
- Record reversions and decision changes explicitly inside the dated entry that made them.
- Do not paste secrets into this file.
- Do record non-secret signing metadata here whenever Android signing changes:
  - which keystore identity is used
  - where that keystore lives
  - alias name
  - signer certificate fingerprint
  - which machine created or last verified it
  - whether it matches the active phone build
  - where the secure backup lives
- If switching machines while the repo is dirty, preserve work on a safety branch before syncing.

## Quick Resume Prompt
Read `logs.md` first, then `ROADMAP.md`, then `RELEASE_SPRINT.md`. Treat `logs.md` as the operational handoff source of truth. Work in the current local checkout path instead of a hardcoded machine path. Preserve the scene-owned UI direction, the consumable upgrade loop, the current restored menu and shop baseline, and the phone-verified monetization build that was installed on the Galaxy S26 Ultra on `2026-04-21`. For a fresh machine, also run `scripts/bootstrap-workstation.ps1`.

## Project Snapshot
- Repo folder name: `Block-dodger1` or `EndlessDodge1` depending on the machine
- Existing project/game naming still used in code and docs: `EndlessDodge1`
- Genre: portrait 2D endless dodge mobile arcade game
- Engine: Unity 6
- Current scenes:
  - `MainMenu`
  - `Game`
  - `Shop`
  - `Inventory`
- Current baseline:
  - current branch on this machine is `master`
  - current local head on this machine is commit `682a8a6` (`4-15-2026 1`), pulled locally on `2026-04-21`
  - the connected Galaxy S26 Ultra currently has a debug build of `com.oreniq.endlessdodge` first installed on `2026-04-21 14:20` and last updated on `2026-04-21 16:12`
  - the installed Galaxy build is signed by `Android Debug` with SHA-256 fingerprint `b5a482c7ba1b832b8deef10b7170faa0b080f9a409453152320023edf7496601`
  - the installed phone build contains the April 15 monetization/shop-offer feature set, including `Coin Packs & Offers`, starter pack, coin packs, and the post-run double-coins flow
  - prior multi-computer sync cleanup happened on `2026-04-13`
  - local Android signing files are currently present on this workstation under `UserSettings/Android`
  - this workstation's local `Oreniq Games` keystore currently has SHA-256 fingerprint `7abda6c18c6396264270bfa0dc34400181a28f58fd59f7da6adc78e77e97e47e`
- Release identity:
  - company name: `Oreniq Games`
  - Android package: `com.oreniq.endlessdodge`

## Project Goals
- Ship a launchable Android version quickly, then iterate hard based on real usage.
- Deliver a clean first session with readable UI, satisfying moment-to-moment feel, and fast restart loops.
- Build retention through daily rewards, daily missions, and a distinct daily challenge.
- Add launch-safe monetization and analytics before release instead of treating them as post-launch TODOs.
- Grow the game toward a commercially viable mobile product over time.

## Scope
### In Scope
- Core endless dodge gameplay and fairness
- Consumable upgrades, inventory, and loadout flow
- Scene-owned UI polish
- Daily reward, daily mission, and daily challenge loops
- Android release prep
- Analytics, rewarded flow, and starter monetization

### Out Of Scope Before Launch Unless Needed
- Major re-architecture
- Large feature pivots away from the consumable-upgrade direction
- Deep social systems beyond lightweight scaffolding
- Nice-to-have polish that delays release-critical work

## Timeline
- Previous release target in planning docs: Friday, `2026-04-17`
- Current phase: active on-device QA and tuning against the Galaxy S26 Ultra build
- Near-term business target: work toward roughly `$1,000/day` within the first month after launch

## User Workflow Preferences
- Give one short Unity step at a time when manual editor work is required.
- Always include full scripts when providing code edits.
- Be explicit with Unity clicks and beginner-friendly instructions.
- Do as much work as possible directly in code and scene files.
- Prefer scene-owned UI over script-generated throwaway UI when practical.
- Make reasonable assumptions instead of wasting turns on tiny back-and-forth.

## Current Product State
### Core Gameplay
- Player movement exists in a portrait play area.
- Border clamping exists so the player stays inside the lane.
- Obstacle spawning and difficulty scaling exist.
- A runtime-generated moving background now runs behind gameplay and rotates themes as the run levels up.
- Coins spawn and are collected.
- Coins were updated away from square-looking placeholders toward circular gold pickups.
- Game over, restart, best score saving, and post-run summary all exist.

### Consumable Upgrade Loop
- Permanent upgrades were replaced with consumable upgrades.
- The player buys consumables in `Shop`.
- Consumables persist in `UpgradeInventory`.
- The player can equip up to 3 upgrade types in `Inventory`.
- Equipped upgrades appear in the `Game` scene.
- Implemented upgrade effects include:
  - `Shield`
  - `Extra Life`
  - `Speed Boost`
  - `Coin Magnet`
  - `Double Coins`
  - `Slow Time`
  - `Smaller Player`
  - `Score Booster`
  - `Bomb`
  - `Rare Coin Boost`
- In-run upgrade buttons support persistent and auto-use behavior where appropriate.

### UI And Scene Direction
- `Game` uses scene-backed HUD elements, tutorial overlay, pause/settings overlay, and post-run summary UI.
- `MainMenu` includes daily reward, daily missions summary, and daily challenge summary.
- `Shop` is a scrollable consumable store.
- `Inventory` is a scrollable equipped-loadout scene with a back button.
- Scene-owned objects are preferred so the user can keep tuning layout directly in Unity.

### Retention Systems
- Daily login reward and streak logic exist.
- Daily missions exist with a 3-mission structure.
- Daily challenge exists as a separate featured run.
- Daily challenge intentionally disables consumables for fairness.
- Challenge rewards, challenge progress, and daily best tracking exist.

### Launch Sprint Systems
- Unity package additions were made for:
  - `com.unity.services.analytics`
  - `com.unity.services.authentication`
  - `com.unity.purchasing`
- Launch and release service scripts were added:
  - `Assets/Scripts/Services/UnityServicesBootstrap.cs`
  - `Assets/Scripts/Services/LaunchAnalytics.cs`
  - `Assets/Scripts/Services/MonetizationManager.cs`
- Shop now surfaces a `Coin Packs & Offers` section with:
  - starter pack
  - small coin pack
  - medium coin pack
  - large coin pack
- Unity IAP purchase hooks and IAP analytics exist, with editor-safe simulated purchases still enabled for testing.
- A post-run rewarded double-coins surface exists and is still simulated until a real ad provider is integrated.
- Analytics instrumentation was added for core run, economy, reward, and challenge events.

## Important Decisions And Reversions
- Consumables replaced the earlier permanent-upgrade direction.
- The daily challenge is separate from daily missions.
- The project is currently on a restored baseline after reverting a later clunky menu and shop cleanup batch.
- Continue from the restored menu and shop baseline, not from the reverted clunkier version.
- Do not treat one machine-specific absolute path as the global source of truth. The repo content is the source of truth; the working path depends on the current machine.

## Open Risks And Blockers
- The current Galaxy S26 Ultra install of `com.oreniq.endlessdodge` is signed with an `Android Debug` certificate from the other PC, while this workstation's local signing path uses the `Oreniq Games` keystore, so in-place updates from this machine currently fail with `INSTALL_FAILED_UPDATE_INCOMPATIBLE`.
- Until the original debug keystore from the other PC is recovered, phone installs from this machine require uninstalling the current app first, which would wipe local device save data.
- Some April 21 phone-side tuning may live in scene-owned Unity content and is not fully described in the older handoff entries yet.
- Unity import, compile, and scene verification still need a live editor pass on this workstation; the current repo review was file-based plus phone package inspection.
- Real phone readability and feel still need a side-by-side check against the current Unity scenes.
- The new dynamic background themes need a live Unity and phone pass to tune contrast, motion speed, and distraction level against gameplay readability.
- Audio, feedback, particles, and transitions are still light or missing.
- The rewarded flow still needs a real ad provider.
- Coin-pack IAP offers exist in code but still need real store-product and device verification.
- Store assets, screenshots, privacy policy, and Play Console setup are still pending.
- Local Android signing data is intentionally machine-local and must not be committed:
  - `UserSettings/Android/oreniq-release.keystore`
  - `UserSettings/Android/release-signing.json`
- On this workstation, both local Android signing files are currently present.
- When changing machines, re-verify Unity modules, package restore, and release-signing setup.

## Current Focus
1. Tomorrow, recover the other PC's original Android debug keystore, verify it matches the phone fingerprint `b5a482c7ba1b832b8deef10b7170faa0b080f9a409453152320023edf7496601`, and copy it to a secure backup plus this workstation.
2. Record the recovered debug signing metadata in `logs.md` without storing raw secret values, so future machine switches do not lose the phone update path again.
3. Open Unity on the current machine and tune the new level-based dynamic backgrounds for readability, speed, and theme cadence during real gameplay.
4. Confirm the `682a8a6` repo state still imports, compiles, and restores packages cleanly in Unity on this workstation.
5. Compare the current Unity scenes against the Galaxy S26 Ultra build that was installed and updated on `2026-04-21`.
6. Capture any scene-owned UI, balance, or feel tweaks that exist on the phone build but are not yet spelled out in the handoff docs.
7. Verify the local Android release-signing helper still works from `Tools/Android` on this workstation.
8. Verify the shop `Coin Packs & Offers` section, starter pack, simulated IAP flows, and post-run double-coins surface on-device once the phone signing path is resolved.
9. Replace the simulated rewarded flow with a real ad provider.
10. Add audio and feedback polish needed for launch quality, then finish store listing assets and release metadata.

## When The Other PC Is Available
1. On the other PC, first look for the Android debug keystore that likely signed the current phone build:
   - `%USERPROFILE%\.android\debug.keystore`
2. If it is not there, search the other PC for:
   - `debug.keystore`
3. If a candidate debug keystore is found, verify its signer fingerprint matches the phone's current signer:
   - expected phone signer SHA-256: `b5a482c7ba1b832b8deef10b7170faa0b080f9a409453152320023edf7496601`
4. Copy that debug keystore to:
   - a secure backup location outside the repo
   - this workstation so future phone updates can use the same signer
5. Also check the project copy that was actually used there for the current local release-signing files:
   - `UserSettings/Android/release-signing.json`
   - `UserSettings/Android/oreniq-release.keystore`
6. If those release files are not there, search the other PC for:
   - `oreniq-release.keystore`
   - `release-signing.json`
7. Copy the release-signing files into this machine's repo folder:
   - `UserSettings/Android/`
8. After the debug keystore and release files are recovered, update `logs.md` with all non-secret signing metadata:
   - keystore type (`Android Debug` or `Oreniq Games` release)
   - file path on the machine where it was recovered
   - alias name
   - signer SHA-256 fingerprint
   - which machine it came from
   - date verified
   - whether it supports in-place updates on the Galaxy phone
   - secure backup location
9. Do not paste passwords or raw secret values into `logs.md`.
10. Do not create a new debug or release keystore unless the old one is truly gone and you intentionally want a new Android signing identity.

## Cross-Computer And Cross-Chat Handoff Rules
- Start by resolving the current repo root on the machine you are using.
- Read documents in this order:
  1. `logs.md`
  2. `ROADMAP.md`
  3. `RELEASE_SPRINT.md`
  4. `docs/COMPUTER_SWITCH_CHECKLIST.md`
- Use `docs/WORKSTATION_SYNC.md` as the permanent switching workflow reference.
- On a fresh clone, run `powershell -ExecutionPolicy Bypass -File scripts/bootstrap-workstation.ps1`.
- Check `git status --short --branch` before making assumptions.
- If the repo is dirty and you need to sync from another machine:
  1. preserve the dirty state on a safety branch
  2. sync `master`
  3. compare preserved work against the new baseline
- If local Android signing is missing on a fresh machine, use:
  - `Tools/Android/Open Local Signing Folder`
  - `Tools/Android/Create Local Signing Config Template`
- Keep machine-local secrets and signing files out of source control.
- Keep `logs.md` updated with non-secret signing metadata any time a new machine, new keystore, or new phone signer is introduced.
- After every meaningful session, append a structured entry below.

## Structured Change Log Template
Use this exact format for new entries:

### YYYY-MM-DD - Short Session Title
- Goal:
- What changed:
- Decisions / reversions:
- Verification:
- Next best action:

## Structured Change Log
### 2026-04-21 - Night handoff and tomorrow signing recovery plan
- Goal:
  - End the night with a clean handoff that explains exactly why the phone could not be updated from this workstation and what to recover tomorrow on the other PC.
- What changed:
  - Added the phone signer's exact SHA-256 fingerprint and this workstation's current release-keystore fingerprint to the handoff snapshot.
  - Expanded the `When The Other PC Is Available` checklist to prioritize recovering the old Android debug keystore before anything else.
  - Added an explicit rule that future signing metadata should be written into `logs.md` in non-secret form so future machine switches do not lose the signing trail again.
- Decisions / reversions:
  - Keep secrets out of `logs.md`, but do store non-secret signing metadata there from now on.
  - Treat signing identity history as handoff-critical project state, not as disposable local machine trivia.
- Verification:
  - Confirmed the current phone signer fingerprint is `b5a482c7ba1b832b8deef10b7170faa0b080f9a409453152320023edf7496601`.
  - Confirmed this workstation's current `Oreniq Games` keystore fingerprint is `7abda6c18c6396264270bfa0dc34400181a28f58fd59f7da6adc78e77e97e47e`.
- Next best action:
  - Tomorrow, recover the original debug keystore from the other PC, verify its fingerprint, back it up securely, and then log its non-secret metadata here before trying another in-place phone update.

### 2026-04-21 - Android deploy automation and phone signing blocker
- Goal:
  - Make phone deployment part of the normal coding loop on this workstation so gameplay changes can be pushed to the connected Galaxy device right after implementation.
- What changed:
  - Added `Assets/Editor/AndroidBuildAutomation.cs` with repeatable Unity menu and batch entry points for Android APK output from this repo.
  - Built `Builds/Android/EndlessDodge-dev.apk` successfully from the current repo state after the new dynamic background work landed.
  - Attempted to install the new APK onto the connected Galaxy S26 Ultra and captured the real Android failure instead of assuming the local signing setup matched the phone.
  - Verified the currently installed phone APK is signed with an `Android Debug` certificate, while this workstation's keystore is the local `Oreniq Games` release identity, so Android blocks the update with `INSTALL_FAILED_UPDATE_INCOMPATIBLE`.
  - Stopped a follow-up phone-signed rebuild once the certificate mismatch was confirmed, to avoid wasting more build time on an install path that still could not replace the phone's current package.
- Decisions / reversions:
  - From this point forward, treat "deploy to phone after changes" as the default workflow goal, but only when the signing identity supports a non-destructive update.
  - Do not uninstall the current phone build automatically, because that would wipe local app data and needs an explicit user decision.
  - The current workstation can build Android successfully, but it cannot update the phone in place until the signing identity mismatch is resolved.
- Verification:
  - `dotnet build Assembly-CSharp-Editor.csproj -nologo` succeeded with `0` warnings and `0` errors after adding the Android build automation script.
  - Unity batch build produced `Builds/Android/EndlessDodge-dev.apk`.
  - `adb install -r -t -d Builds\\Android\\EndlessDodge-dev.apk` failed with `INSTALL_FAILED_UPDATE_INCOMPATIBLE`.
  - APK certificate inspection showed the installed Galaxy build uses the `Android Debug` signer, while this workstation's `UserSettings/Android/oreniq-release.keystore` uses the `Oreniq Games` signer.
- Next best action:
  - Choose whether to uninstall and reinstall the app on the phone now, or recover the original debug keystore from the other PC so this workstation can resume in-place updates without wiping device data.

### 2026-04-21 - Dynamic level backgrounds
- Goal:
  - Give the `Game` scene a stronger sense of motion and progression by adding animated backgrounds that change as the player levels up.
- What changed:
  - Added `Assets/Scripts/DynamicBackgroundController.cs` to generate a full-screen backdrop and drifting foreground-safe scenery at runtime without waiting on imported art.
  - Implemented multiple rotating background themes that now cycle by difficulty level, including cloud, cave, crystal, storm, and starfield-inspired looks.
  - Wired `GameManager` to auto-create the dynamic background controller at run start so the effect is present in normal gameplay without scene hand-editing first.
  - Updated `Assembly-CSharp.csproj` so local command-line compilation includes the new background script until Unity next regenerates project files.
- Decisions / reversions:
  - Use procedural runtime visuals first so we can tune feel immediately and swap to hand-authored art later if needed.
  - Keep the generated scenery low-opacity and mostly edge-biased so the main dodge lane stays readable.
  - Continue running the Unity-generated `dotnet build` targets serially on this workstation because parallel builds still fight over shared `Temp` outputs.
- Verification:
  - `dotnet build Assembly-CSharp.csproj -nologo` succeeded with `0` warnings and `0` errors after adding the new script to the project file.
  - `dotnet build Assembly-CSharp-Editor.csproj -nologo` succeeded with `0` warnings and `0` errors when rerun serially.
- Next best action:
  - Open the `Game` scene in Unity and on the Galaxy phone, then tune background contrast, spawn density, and per-theme motion so the new visuals feel alive without making dodging harder.

### 2026-04-21 - Galaxy S26 Ultra build verification and log correction
- Goal:
  - Use the connected phone as the stronger source of truth so `logs.md` matches the build that was actually installed and tested today.
- What changed:
  - Inspected the connected Galaxy S26 Ultra over ADB and confirmed `com.oreniq.endlessdodge` is installed as a debuggable build that was first installed on `2026-04-21 14:20` and last updated on `2026-04-21 16:12`.
  - Pulled the installed `base.apk` from the phone and verified its metadata contains the April 15 monetization/shop-offer strings, including `Coin Packs & Offers`, starter pack, small/medium/large coin packs, `Editor purchase simulation enabled`, and the post-run double-coins labels.
  - Confirmed the phone package name matches the current repo release identity (`Oreniq Games` / `com.oreniq.endlessdodge`) rather than the older `DefaultCompany` copies found in other local folders.
  - Read the phone's Unity player prefs through `run-as` and confirmed active device testing data exists on `2026-04-21` (for example coins, score, daily systems, and tutorial state).
  - Corrected `logs.md` so it stays aligned with the phone-verified April 15 feature baseline instead of falling back to the older April 13 snapshot.
- Decisions / reversions:
  - Treat the connected phone build as the stronger source of truth than stale handoff assumptions when the docs and lived device state disagree.
  - Do not roll `logs.md` back before the April 15 monetization entry, because the phone build clearly contains that feature set.
  - Keep using the current checkout and `.git` metadata to anchor repo history, but treat phone verification as the tie-breaker for current active state.
  - Keep Android signing material local-only even when it is present on this workstation.
  - On this workstation, run the Unity-generated `dotnet build` targets serially instead of in parallel because both projects write into the same `Temp` outputs.
- Verification:
  - Read the current repo files directly.
  - Verified `.git/HEAD` points to `refs/heads/master` and the current local head hash is `682a8a6757b400aadbb5fc7c99585c0d8b791288` by reading `.git` metadata directly on this workstation.
  - Verified `ProjectSettings/ProjectVersion.txt` is `6000.4.0f1`, `ProjectSettings.asset` still uses `Oreniq Games` and `com.oreniq.endlessdodge`, and `UserSettings/Android` currently contains both local signing files on this workstation.
  - `adb devices -l` detected the Galaxy S26 Ultra (`SM-S948U`).
  - `adb shell dumpsys package com.oreniq.endlessdodge` reported the April 21 install/update times and confirmed the package is debuggable.
  - `adb shell run-as com.oreniq.endlessdodge cat shared_prefs/com.oreniq.endlessdodge.v2.playerprefs.xml` returned active player prefs from phone testing.
  - `dotnet build Assembly-CSharp.csproj -nologo` succeeded with `0` warnings and `0` errors after rerunning it serially.
  - `dotnet build Assembly-CSharp-Editor.csproj -nologo` succeeded with `0` warnings and `0` errors.
- Next best action:
  - Open Unity on this machine, compare the scene-owned UI and feel directly against the Galaxy S26 Ultra build, and capture any unlogged April 21 tuning changes before making the next code or scene edit.

### 2026-04-13 - Workstation sync and handoff standardization
- Goal:
  - Make the project easier to move between computers and chats without losing track of the active repo or local-only Android signing setup.
- What changed:
  - Replaced the transcript-heavy handoff file with this standardized structure.
  - Added explicit cross-computer rules and a specific checklist for recovering the Android signing files from the other PC.
  - Added workstation bootstrap and checklist docs plus a repo-tracked signing template.
  - Updated the Android signing editor helper so a fresh machine without local signing data no longer warns during editor load and can generate a local template.
- Decisions / reversions:
  - The project handoff should no longer depend on one hardcoded machine path.
  - `logs.md` should stay concise and structured; deeper planning belongs in `ROADMAP.md` and `RELEASE_SPRINT.md`.
- Verification:
  - Unity opened on this machine with warnings only, not blocking compile errors.
  - The missing-signing warning was traced to an empty local `UserSettings/Android` folder on this PC.
- Next best action:
  - When the other PC is available, recover the local Android signing files and apply them on this machine.

### 2026-04-15 - Shop monetization wiring
- Goal:
  - Connect the new monetization manager to the existing `Shop` scene so starter-offer and coin-pack purchases have a visible in-game surface.
- What changed:
  - Expanded `MonetizationManager` into a real offer catalog with starter pack and three coin packs, Unity IAP store connection, editor-safe purchase simulation, reward granting, and purchase callbacks.
  - Added IAP analytics events to `LaunchAnalytics` for purchase request and purchase result tracking.
  - Updated `ShopManager` to render a `Coin Packs & Offers` section above consumables, refresh from monetization state changes, and keep the soft-currency total in sync after purchases.
  - Added editor-only shop verification helpers so the shop snaps to the premium section on open, seeds a temporary test coin balance when empty, and uses larger card text for Game view checks.
- Decisions / reversions:
  - Keep editor purchase simulation enabled for now so the offer flow can be tested before Android signing and store products are fully recovered on every machine.
  - Leave the rewarded-ad flow simulated until a real ad provider is integrated.
- Verification:
  - `dotnet build Assembly-CSharp.csproj -nologo` succeeded with `0` warnings and `0` errors.
  - `dotnet build Assembly-CSharp-Editor.csproj -nologo` succeeded with `0` warnings and `0` errors.
- Next best action:
  - Reopen the `Shop` scene in Unity, verify the new offer cards render cleanly, and click through simulated purchases to confirm coin and consumable grants update live.

### 2026-04-11 - Launch sprint continuation
- Goal:
  - Move the project from "game systems mostly exist" toward "Android release preparation is real."
- What changed:
  - Added Unity Analytics, Authentication, and IAP packages.
  - Added `UnityServicesBootstrap`, `LaunchAnalytics`, and `MonetizationManager`.
  - Added a simulated post-run rewarded double-coins flow for immediate testing.
  - Instrumented analytics events for runs, economy, rewards, and challenge flows.
  - Added release-oriented docs in `ROADMAP.md` and `RELEASE_SPRINT.md`.
  - Locked release identity to `Oreniq Games` and `com.oreniq.endlessdodge`.
  - Added local Android signing helper support through `Assets/Editor/AndroidReleaseSigningConfigurator.cs`.
- Decisions / reversions:
  - Keep keystore material and signing JSON local-only under `UserSettings/Android`.
- Verification:
  - The session log for this sprint says package restore, compile, and local signing verification succeeded on the machine used for that session.
- Next best action:
  - Re-verify the same setup on any machine that was not used during that session.

### 2026-04-09 to 2026-04-10 - Core systems restore and expansion
- Goal:
  - Restore the project baseline and make the core gameplay, inventory, UI, and retention loops actually work together.
- What changed:
  - Built the `Inventory` scene flow and 3-slot equipped loadout system.
  - Connected equipped consumables to in-run buttons and gameplay behavior.
  - Fixed shield handling, player border clamping, and several gameplay integration issues.
  - Added and iterated on `MainMenu`, `Shop`, `Game`, and `Inventory` scene UI.
  - Added daily rewards, daily missions, and a separate daily challenge system.
- Decisions / reversions:
  - Reverted a later clunky cleanup batch that made the menu and shop layout worse.
  - Kept dead legacy shop junk out even after reverting the clunky batch.
- Verification:
  - Core loop, inventory flow, upgrade activation, and retention systems existed in the restored project state after this work.
- Next best action:
  - Continue polishing scene-owned UI carefully without undoing the restored layout baseline.
