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
- If switching machines while the repo is dirty, preserve work on a safety branch before syncing.

## Quick Resume Prompt
Read `logs.md` first, then `ROADMAP.md`, then `RELEASE_SPRINT.md`. Treat `logs.md` as the operational handoff source of truth. Work in the current local checkout path instead of a hardcoded machine path. Preserve the scene-owned UI direction, the consumable upgrade loop, the current restored menu and shop baseline, and the Android release push toward April 17, 2026. For a fresh machine, also run `scripts/bootstrap-workstation.ps1`.

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
  - current branch on this workstation is `master`
  - current local head on this workstation is `8270e89`
  - prior multi-computer sync cleanup happened on `2026-04-13`
  - workstation role clarification on `2026-04-22`: the desktop remains the primary machine and this laptop is the secondary machine
  - a secondary phone-test package is now available for this laptop workflow: `com.oreniq.endlessdodge.secondary` (`Endless Dodge Test`)
  - this workstation currently has the local release-signing files in `UserSettings/Android`
  - this workstation does not currently have `%USERPROFILE%\.android\debug.keystore`
  - this workstation does not currently have the shared external signing folder at `Documents/EndlessDodge1/SharedSigning/Android/`
  - this workstation does not currently have `%ENDLESSDODGE_SIGNING_CONFIG%` or `%ENDLESSDODGE_SIGNING_ROOT%` configured
  - the older notes about `Builds/Android/EndlessDodge1-debug.apk` build 15 do not match the files currently present in this checkout on this workstation
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
- Current release target: Friday, `2026-04-17`
- Current phase: launch sprint and release-readiness pass
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
- A first monetization surface exists:
  - post-run rewarded double-coins flow
  - currently simulated until a real ad provider is integrated
- Analytics instrumentation was added for core run, economy, reward, and challenge events.

## Important Decisions And Reversions
- Consumables replaced the earlier permanent-upgrade direction.
- The daily challenge is separate from daily missions.
- The project is currently on a restored baseline after reverting a later clunky menu and shop cleanup batch.
- Continue from the restored menu and shop baseline, not from the reverted clunkier version.
- Do not treat one machine-specific absolute path as the global source of truth. The repo content is the source of truth; the working path depends on the current machine.

## Open Risks And Blockers
- The newer repo docs explain the cross-PC signing workflow, but the actual other-PC signing material is still not present on this workstation: no `%USERPROFILE%\.android\debug.keystore`, no shared signing folder, and no signing override environment variables.
- This laptop can now install and update a side-by-side secondary phone app (`com.oreniq.endlessdodge.secondary`) for testing, but it still cannot update the original desktop-owned package (`com.oreniq.endlessdodge`) in place until both machines share the same signing identity.
- Real phone validation is still needed on the newest laptop-test Android build (`Builds/Android/EndlessDodge1-secondary-debug.apk`, installed as `com.oreniq.endlessdodge.secondary`) for:
  - whether the slight hitch on every biome change / every third level-up is now fully gone after prewarming music + all biome background themes ahead of time
  - whether the cave-tinted side borders feel like the right width/opacity in gameplay after being stretched to the full visible play area
  - whether the new longer 4-section background track finally reduces the remaining repetition enough to feel comfortable over repeated runs
  - whether the gameplay music is loud enough on phone without drowning out important SFX
  - whether the loop seam / end-of-loop click is gone or at least much smaller
  - whether the cave backgrounds are sharp enough and still readable against dark obstacles
  - Some phone UI polish may still remain in `MainMenu` and `Inventory`; do not assume the earlier layout issues are fully closed until the newest APK is retested.
  - Cross-PC Android signing must stay out of Git, so the repo needs to point to a safe shared external signing folder instead of storing signing secrets in `logs.md`.
  - Runtime audio/visual feedback now exists, but it is still procedural placeholder content and may need stronger authored polish.
  - The post-run rewarded double-coins offer was restored for debug/dev builds and must be re-verified on phone after a run.
  - A one-time rewarded revive prompt was added and must be re-verified on phone to decide whether it feels good enough to keep.
  - The rewarded flow still needs a real ad provider before release; current phone testing still uses simulation in debug/dev builds.
  - Coin-pack IAPs and starter-offer surfaces now exist, but they still need production store configuration and polish.
  - The new post-run `Share Result` button and conditional `Rate on Google Play` prompt are live and phone-verified on the secondary package, but the review action currently validates only Android's market/store handoff path, not a live public Play listing or a true in-app review API flow yet.
  - Store assets, screenshots, privacy policy, and Play Console setup are still pending.
  - Local Android signing data is intentionally machine-local and must not be committed:
    - `UserSettings/Android/oreniq-release.keystore`
    - `UserSettings/Android/release-signing.json`
  - Secret material must not be pasted into `logs.md`; use the shared external signing folder or the supported environment-variable overrides instead.
  - Two Android build paths now exist in `Assets/Editor`, and they are not equivalent:
    - `AndroidBuildUtility` matches the newer shared-signing/debug-fallback workflow in the docs
    - `AndroidBuildAutomation` still uses the older local-release-signing-only behavior and can produce the wrong signer for phone update testing on this workstation
  - When changing machines, re-verify Unity modules, package restore, and release-signing setup.

## Current Focus
1. On the primary desktop, pull the latest repo, keep the original package signer intact, then build/install the original app (`com.oreniq.endlessdodge`) onto the phone so the laptop-tested changes become the real phone baseline.
2. Keep the desktop as the primary machine for the original production package (`com.oreniq.endlessdodge`) and use this laptop's side-by-side secondary package (`com.oreniq.endlessdodge.secondary`) for rapid phone testing until signing is intentionally shared.
3. Use the newer `AndroidBuildUtility` workflow on this laptop for phone testing:
   - `Tools/Android/Build Secondary Test APK`
   - `Tools/Android/Build And Install Secondary Test APK`
4. Pre-launch product scope should stay narrow now:
   - real rewarded ads instead of simulated rewarded flow
   - real IAP / starter-pack readiness
   - first-session clarity / polish on phone
   - post-run growth surfaces that spread well without widening scope
   - release/store/compliance tasks
5. After the desktop installs the original app again, retest these exact items on the real package: biome-change hitch, cave border feel, music/SFX balance, post-run double-coins visibility, rewarded revive feel, and whether the new share/review post-run actions still feel right outside the secondary test package.
6. If both machines need to update the same installed original phone app, choose one of these supported paths:
   - temporary bridge: copy the desktop's `%USERPROFILE%\.android\debug.keystore` to the same path on this laptop
   - preferred long-term path: configure `Documents/EndlessDodge1/SharedSigning/Android/` so both machines use the same shared signing identity
7. After the desktop signer is present on this workstation, rebuild and reinstall the original debug APK, then retest on phone with sound on. First checks:
   - music identity and loudness
   - loop-end click/pop
   - music feel / energy
   - post-run double-coins button visibility
   - rewarded revive prompt behavior
8. Finish any remaining real-phone UI/layout cleanup in `MainMenu`, `Inventory`, and the in-run HUD after the audio/rewarded retest is checked.
9. Replace the simulated rewarded flow with a real ad provider.
10. Finish production IAP/store configuration, listing assets, screenshots, privacy policy, Data safety declarations, and Play Console metadata.
11. Avoid adding broad new gameplay systems until the above launch-critical items are done.

## When The Other PC Is Available
1. Resolve the actual repo root on that PC, pull the latest changes, then read:
   - `logs.md`
   - `ROADMAP.md`
   - `RELEASE_SPRINT.md`
   - `docs/COMPUTER_SWITCH_CHECKLIST.md`
2. Run:
   - `powershell -ExecutionPolicy Bypass -File scripts/bootstrap-workstation.ps1`
3. Open the project in Unity `6000.4.0f1` and wait for import/compile to finish.
4. On the other PC, check the project copy that was actually used there for Android signing material.
5. Look inside that repo's `UserSettings/Android` folder for:
   - `release-signing.json`
   - `oreniq-release.keystore`
6. If they are not there, search the other PC for:
   - `oreniq-release.keystore`
   - `release-signing.json`
7. Put both files in the shared external signing folder if possible:
   - `Documents/EndlessDodge1/SharedSigning/Android/`
8. If needed, local project fallback is still:
   - `UserSettings/Android/`
9. Supported secret-location overrides also exist:
   - `%ENDLESSDODGE_SIGNING_CONFIG%`
   - `%ENDLESSDODGE_SIGNING_ROOT%`
10. On that machine, reopen Unity if needed and click:
   - `Tools/Android/Apply Local Release Signing`
11. Verify Unity reports signing was applied successfully, then use:
   - `Tools/Android/Build Debug APK`
   - or `Tools/Android/Build And Install Debug APK`
12. Important: the desktop should use `Build Debug APK` / `Build And Install Debug APK` for the original phone app, not the secondary test build. The temporary secondary package workflow was only for this laptop.
13. After install on the desktop, verify the original package was updated:
   - package should still be `com.oreniq.endlessdodge`
   - use `adb shell dumpsys package com.oreniq.endlessdodge` and check `lastUpdateTime`
   - launch the original app and confirm the new cave background/music/border/share-review changes are present there
14. First manual retest on the desktop-installed original app should be:
   - music loudness / annoyance level
   - whether the ambient loop still ends with a loud click
   - whether the slight every-third-level hitch is now acceptably small
   - whether the full-height cave borders still feel right on the original package
   - whether the post-run `Watch Ad` double-coins button appears again
   - whether the mid-run rewarded revive prompt appears and feels good enough to keep
   - whether the post-run `Share Result` button feels worth keeping
   - whether the conditional `Rate on Google Play` prompt should stay as a store-link prompt or be replaced with a true in-app review flow
15. Build 15 is the current known-good baseline from this machine:
   - log: `Logs/codex-android-build-15.log`
   - artifact: `Builds/Android/EndlessDodge1-debug.apk`
   - install status on this machine: succeeded on phone via `adb install -r`
   - important signing note: build 15 did **not** use shared signing yet; it fell back to this machine's local Android debug keystore because shared signing was not found
   - current machine debug-keystore path:
     - `C:\Users\antho\.android\debug.keystore`
16. If the other PC needs to update the exact same currently installed app **before** shared/release signing is recovered:
   - copy this machine's debug keystore file:
     - `C:\Users\antho\.android\debug.keystore`
   - place it on the other PC at:
     - `%USERPROFILE%\.android\debug.keystore`
   - this makes the other PC's debug builds use the same signing identity as the app currently installed from this PC
   - do **not** commit this file to Git
   - this is a temporary bridge only; the preferred long-term fix is still shared signing via:
     - `Documents/EndlessDodge1/SharedSigning/Android/`
17. Important secret rule:
   - do not paste signing keys, keystore passwords, or other secrets into `logs.md` or any repo-tracked file
   - keep them in the shared external signing folder or environment variables only
18. Do not create a new release keystore unless the old one is truly gone and you intentionally want a new Android signing identity.

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
### 2026-04-23 - End-of-night desktop handoff and launch outlook
- Goal:
  - Leave one clean desktop-ready handoff that summarizes tonight's work, explains how to move the repo changes onto the original phone app, and sets a realistic launch / earnings expectation.
- What changed:
  - Consolidated tonight's laptop work into one resume point for the primary desktop:
    - cave-background / biome progression work
    - multiple gameplay-music rewrites toward a longer, less annoying loop
    - reduced biome-transition hitch and cave-tinted full-height borders
    - side-by-side secondary phone-test app workflow
    - post-run share / review growth actions
  - Added explicit primary-PC instructions in `Current Focus` and `When The Other PC Is Available` for:
    - pulling the latest repo
    - preserving the original signer
    - using `Tools/Android/Build And Install Debug APK` for the real package (`com.oreniq.endlessdodge`)
    - verifying the original package update on phone
    - retesting the newly added music / hitch / border / post-run growth features on the production package
  - Added finish-the-game priorities for tomorrow:
    - real rewarded ads
    - real IAP / starter-pack configuration
    - store assets / privacy / Data safety / Play Console release setup
    - one more real-phone polish pass on the original package after desktop install
  - Added a realistic launch-window note:
    - best case if credentials/signing/setup go smoothly: roughly `3-7` focused workdays from `2026-04-23`
    - more conservative if Play Console / signing / store-review friction appears: roughly `1-2` weeks
  - Added a realistic commercial-outlook note:
    - this project is much more launchable now, but it should still be treated as a small indie casual / hybrid-casual release, not a top-chart expectation
    - compared with stronger casual competitors, the current game is lighter on content depth, brand power, and polish volume, so retention and organic spread will need to outperform expectations to become a major earner
    - the earlier `$1,000/day` goal should stay aspirational for now, not the planning baseline
- Decisions / reversions:
  - Use the desktop as the machine that turns the laptop-tested repo state into the real phone app state.
  - Treat commercial estimates as scenario planning until soft-launch data exists; do not assume launch features alone guarantee meaningful revenue.
- Verification:
  - The detailed build/install/phone verification for tonight's feature work is recorded in the `2026-04-22` entries above, including the successful secondary APK deploy and temporary screenshot QA pass.
  - `logs.md` now contains desktop install instructions, finishing priorities, and the launch / commercial outlook in one place for tomorrow's handoff.
- Next best action:
  - On the primary PC tomorrow: `git pull`, verify signing, run `Tools/Android/Build And Install Debug APK`, confirm `com.oreniq.endlessdodge` updates on the phone, then finish the credential-gated monetization / store tasks.

### 2026-04-22 - Post-run share/review growth pass and phone QA
- Goal:
  - Add the highest-leverage spread / review features that can be shipped tonight without dashboard credentials, then verify them on the laptop-managed secondary phone app.
- What changed:
  - Added `Assets/Scripts/MobileGrowthActions.cs` as a runtime Android handoff helper for share-sheet launches and Play Store / market-view launches, with clipboard / URL fallback outside native Android runtime.
  - Expanded the `GameManager` post-run summary panel with a permanent `Share Result` CTA plus a conditional `Rate on Google Play` CTA that appears only after stronger runs (`Growth_CompletedRuns` plus a happy-moment threshold).
  - Added review-prompt state tracking in `GameSettings` and new share/review analytics events in `LaunchAnalytics`.
  - Kept the new growth actions wired into the existing runtime-built post-run UI instead of adding a separate results scene or duplicate overlay.
  - Built and installed a fresh secondary test APK on the connected Samsung phone:
    - build log: `Logs/android-secondary-growth-build.log`
    - artifact: `Builds/Android/EndlessDodge1-secondary-debug.apk`
- Decisions / reversions:
  - Use the lighter store-link / market-intent review flow for now instead of a full Play in-app review API integration, because the live listing / dashboard setup still depends on tomorrow's credential work.
  - Keep screenshot QA temporary only: use local phone screenshots to inspect layout and intent handoff, restore any temporary on-device QA state, then delete the screenshots instead of leaving them in the repo or handoff.
- Verification:
  - `dotnet build Assembly-CSharp.csproj -nologo /p:UseSharedCompilation=false` succeeded with `0` warnings and `0` errors.
  - `dotnet build Assembly-CSharp-Editor.csproj -nologo /p:UseSharedCompilation=false` succeeded with `0` warnings and `0` errors.
  - Unity batch Android build completed successfully and logged `Android secondary debug APK created at: C:\Users\antho\Documents\UnityProjects\Block-dodger1\Builds\Android\EndlessDodge1-secondary-debug.apk`.
  - `adb install -r -t Builds/Android/EndlessDodge1-secondary-debug.apk` returned `Success`.
  - `adb shell dumpsys package com.oreniq.endlessdodge.secondary` reported `lastUpdateTime=2026-04-23 00:02:14`.
  - Used `12` temporary phone screenshots during QA to check:
    - main menu restored cleanly after install
    - gameplay still launches normally
    - rewarded revive still appears before post-run summary
    - low-score post-run layout shows the new share button cleanly
    - a forced QA review state shows both `Rate on Google Play` and `Share Result` without overlap
    - Android chooser / resolver handoff really occurs for the new growth buttons
  - Temporarily edited the secondary app's PlayerPrefs only to force the review-prompt state for QA, then restored the original values and relaunched the app to confirm the normal menu state came back (`Coins 540`, `Best 128`, `Streak 1`).
- Next best action:
  - Tomorrow, finish the credential-gated side: real rewarded ads, production IAP/store wiring, and decide whether to keep the store-link review prompt as-is or replace it with a true Google Play in-app review flow once the listing path is ready.

### 2026-04-22 - Revenue and spread priority pass
- Goal:
  - Capture which remaining features are most likely to increase launch revenue and organic spread so the sprint stays commercially focused.
- What changed:
  - Prioritized the highest expected launch-impact items as:
    - real rewarded ads on the existing post-run double-coins and mid-run revive surfaces
    - strong starter-pack / coin-pack purchase flow with real billing verification
    - in-app review timing after a satisfying player moment, not immediately
    - lightweight share loop built around daily challenge / score results
    - optional Play Games achievements / leaderboards if time allows, especially for retention and discovery leverage
    - stronger Play Store asset / listing work and later experiment hooks instead of more broad gameplay systems
  - Explicitly treated broader feature expansion as lower ROI than monetization reality, review/share loops, and store-conversion work before launch.
- Decisions / reversions:
  - Revenue and organic spread should come first from polished monetization + store conversion + share/review loops, not from piling on more gameplay systems right before launch.
  - If time gets tight, cut optional systems before cutting rewarded ads, IAP readiness, store assets, or review/share hooks.
- Verification:
  - Rechecked the current repo state against launch docs before setting the priority order.
- Next best action:
  - Implement real rewarded ads first, then add one clean share/result flow and one well-timed in-app review prompt before spending time on lower-ROI additions.

### 2026-04-22 - Pre-launch feature triage
- Goal:
  - Decide which remaining features are actually worth doing before launch so time stays focused on what will matter most for the first release.
- What changed:
  - Narrowed the recommended pre-launch feature scope to a small launch-critical set instead of continuing to add broad new systems.
  - Prioritized these remaining feature/product items ahead of new content:
    - real rewarded ads replacing the current simulated rewarded flow
    - real starter-pack / coin-pack purchase readiness and verification
    - first-session clarity and phone-feel polish
    - release/store/compliance work required to actually ship
  - Explicitly pushed broader feature expansion like extra modes, larger progression systems, or big content additions to post-launch unless a release blocker appears.
- Decisions / reversions:
  - Do not turn the pre-launch sprint into a feature-creep phase now that the game already has core loop, shop, inventory, daily reward, daily missions, and daily challenge systems.
  - Treat launch-readiness, monetization reality, and phone polish as more important than adding more systems before the first release.
- Verification:
  - Re-read `ROADMAP.md`, `RELEASE_SPRINT.md`, and `docs/RELEASE_COMPLIANCE_CHECKLIST.md` against the current repo state before setting the priority list.
- Next best action:
  - Work the remaining launch items in this order: real rewarded ads, real IAP/store readiness, final phone polish / hitch cleanup, then release/store/compliance packaging.

### 2026-04-22 - Biome hitch smoothing and cave-border pass
- Goal:
  - Remove the slight gameplay hitch that happens on every biome change / every third level-up, restyle the bright blue side borders to match the cave direction, and confirm the result with real phone screenshots.
- What changed:
  - Updated `Assets/Scripts/EndlessDodgeAudioDirector.cs` so biome music loops are prewarmed ahead of time instead of being synthesized on the level-up frame that first needs them.
  - Updated `Assets/Scripts/CaveBackgroundController.cs` so all biome theme variants are queued for prewarm early, not just the immediate next couple of transitions.
  - Added `Assets/Scripts/PlayfieldBorderController.cs` and wired it through `GameManager` so the existing `LeftBorder`, `RightBorder`, and glow sprites are recolored from the live cave theme blend and stretched to the full visible play area on each device.
  - Rebuilt the secondary Android APK, reinstalled it to the connected Samsung phone as `Endless Dodge Test`, and pulled fresh device screenshots into `Builds/PhoneScreenshots/` to verify the border pass from the actual phone.
- Decisions / reversions:
  - Treat the every-third-level hitch as a biome-prewarm problem instead of trying to hide it with a later crossfade timing tweak only.
  - Keep the existing border objects, but style and size them at runtime so the phone layout stays correct across aspect ratios instead of baking one fixed scene scale.
- Verification:
  - `dotnet build Assembly-CSharp.csproj -nologo /p:UseSharedCompilation=false` succeeded with `0` warnings and `0` errors.
  - `dotnet build Assembly-CSharp-Editor.csproj -nologo /p:UseSharedCompilation=false` succeeded with `0` warnings and `0` errors.
  - Unity batch build `AndroidBuildUtility.BuildSecondaryDebugApkBatchmode` completed successfully and wrote `Builds/Android/EndlessDodge1-secondary-debug.apk` (`Logs/android-secondary-hitch-borders-build.log`).
  - `adb shell dumpsys package com.oreniq.endlessdodge.secondary` showed `lastUpdateTime=2026-04-22 22:43:20`.
  - `adb shell monkey -p com.oreniq.endlessdodge.secondary -c android.intent.category.LAUNCHER 1` launched the updated secondary app.
  - Real device screenshots were captured and pulled successfully:
    - `Builds/PhoneScreenshots/endlessdodge-test-shot.png`
    - `Builds/PhoneScreenshots/endlessdodge-gameplay-shot.png`
- Next best action:
  - Play through at least one biome transition on `Endless Dodge Test` and confirm two things first: the hitch is gone, and the new cave-colored side borders feel right in width and brightness during an actual run.

### 2026-04-22 - Longer-form music variation pass
- Goal:
  - Reduce the remaining repetition after the pop-leaning rewrite by making the song evolve more over time instead of simply looping the same pleasant idea.
- What changed:
  - Extended the generated gameplay music again to a `32` second loop and expanded the chord progressions to a longer bar structure so the return point takes longer to come around.
  - Reworked `Assets/Scripts/EndlessDodgeAudioDirector.cs` into a clearer 4-section form with a lighter opening, fuller middle, added response phrases later in the loop, section-dependent drum density, and a different end-of-loop turnaround.
  - Kept the cleaner pop/listenable direction from the previous pass rather than sliding back toward cave ambience or character-theme weirdness.
  - Rebuilt the secondary Android APK and reinstalled it to the connected Samsung phone as `Endless Dodge Test`.
- Decisions / reversions:
  - Attack repetition with song-form variation and arrangement changes, not just by nudging notes.
  - Keep the soundtrack goal centered on “pleasant over many runs” rather than trying to tightly mirror the cave art direction.
- Verification:
  - `dotnet build Assembly-CSharp.csproj -nologo /p:UseSharedCompilation=false` succeeded with `0` warnings and `0` errors.
  - `dotnet build Assembly-CSharp-Editor.csproj -nologo /p:UseSharedCompilation=false` succeeded with `0` warnings and `0` errors when rerun sequentially after transient parallel temp-file races during verification.
  - Unity batch build `AndroidBuildUtility.BuildSecondaryDebugApkBatchmode` completed successfully and wrote `Builds/Android/EndlessDodge1-secondary-debug.apk` (`Logs/android-secondary-music-variation-build.log`).
  - `adb shell dumpsys package com.oreniq.endlessdodge.secondary` showed `lastUpdateTime=2026-04-22 22:08:07`.
  - `adb shell monkey -p com.oreniq.endlessdodge.secondary -c android.intent.category.LAUNCHER 1` launched the updated secondary app.
- Next best action:
  - Listen to the updated `Endless Dodge Test` build and decide whether the repetition problem is now mostly solved or whether the next pass needs even bigger contrast between sections.

### 2026-04-22 - Longer pop-leaning soundtrack rewrite
- Goal:
  - Fix the remaining music problems called out on phone: too short, too repetitive, too cave-themed, and too close to a weird circus/character cue instead of something pleasant and catchy.
- What changed:
  - Reworked `Assets/Scripts/EndlessDodgeAudioDirector.cs` again so the generated music now uses a longer `24` second loop, a cleaner major-key song structure, more conventional pop-style chord progressions, lighter percussion, and a softer hook instead of the earlier cave/minor/tension-heavy direction.
  - Dropped the cave-noise layer from the main music build path so the soundtrack no longer needs to sell a cavern mood and can simply aim to be enjoyable background music for repeated runs.
  - Rebuilt the secondary Android APK and reinstalled it to the connected Samsung phone as `Endless Dodge Test`.
- Decisions / reversions:
  - Stop treating the gameplay soundtrack as something that must be cave-themed just because the visuals are cave-themed.
  - Favor a longer, cleaner, more listenable loop even if that means the music feels less tightly tied to the biome art direction.
- Verification:
  - `dotnet build Assembly-CSharp.csproj -nologo /p:UseSharedCompilation=false` succeeded with `0` warnings and `0` errors.
  - `dotnet build Assembly-CSharp-Editor.csproj -nologo /p:UseSharedCompilation=false` succeeded with `0` warnings and `0` errors when rerun sequentially after a transient parallel verification race in Unity's temp files.
  - Unity batch build `AndroidBuildUtility.BuildSecondaryDebugApkBatchmode` completed successfully and wrote `Builds/Android/EndlessDodge1-secondary-debug.apk` (`Logs/android-secondary-music-pop-build.log`).
  - `adb shell dumpsys package com.oreniq.endlessdodge.secondary` showed `lastUpdateTime=2026-04-22 21:46:53`.
  - `adb shell monkey -p com.oreniq.endlessdodge.secondary -c android.intent.category.LAUNCHER 1` launched the updated secondary app.
- Next best action:
  - Listen to the updated `Endless Dodge Test` build and answer only this first: is the new loop finally pleasant enough to live with for repeated runs, or should it move further toward either “more upbeat and catchy” or “more calm and unobtrusive” next?

### 2026-04-22 - Soundtrack retune away from melodic hook
- Goal:
  - Keep the new “actual music” direction, but remove the specific tune that felt wrong on phone and push a darker, less sing-song retune immediately.
- What changed:
  - Reworked `Assets/Scripts/EndlessDodgeAudioDirector.cs` again so the loop now leans more on darker bass pulses, shorter tension ostinatos, and accent stabs instead of the earlier more obvious lead melody and brighter arpeggio feel.
  - Pulled the pad voicing darker and softer so the track reads more like momentum and pressure inside a cave run instead of a tune that wants to sit in the foreground.
  - Rebuilt the secondary Android APK and reinstalled it to the connected Samsung phone as `Endless Dodge Test`.
- Decisions / reversions:
  - Keep the idea of “real music” instead of reverting to the old low hum, but move away from memorable lead-hook writing unless later feedback specifically asks for something more melodic.
  - Continue iterating through the side-by-side secondary package first so music taste testing does not disturb the desktop-owned original app.
- Verification:
  - `dotnet build Assembly-CSharp.csproj -nologo /p:UseSharedCompilation=false` succeeded with `0` warnings and `0` errors.
  - `dotnet build Assembly-CSharp-Editor.csproj -nologo /p:UseSharedCompilation=false` succeeded with `0` warnings and `0` errors when rerun sequentially after Unity `Temp` file locking during parallel verification.
  - Unity batch build `AndroidBuildUtility.BuildSecondaryDebugApkBatchmode` completed successfully and wrote `Builds/Android/EndlessDodge1-secondary-debug.apk` (`Logs/android-secondary-music-retune-build.log`).
  - `adb shell dumpsys package com.oreniq.endlessdodge.secondary` showed `lastUpdateTime=2026-04-22 21:25:27`.
  - `adb shell monkey -p com.oreniq.endlessdodge.secondary -c android.intent.category.LAUNCHER 1` launched the updated secondary app.
- Next best action:
  - Listen to the updated `Endless Dodge Test` build and answer only this first: is this darker direction closer, or should the music move even further toward one of these poles next time: heavier / more intense, more ambient / less musical, or more arcade / energetic?

### 2026-04-22 - Procedural soundtrack pass and secondary-phone deploy
- Goal:
  - Replace the low cave-hum background with something that feels like actual gameplay music, then push the result to the side-by-side phone test app immediately.
- What changed:
  - Reworked `Assets/Scripts/EndlessDodgeAudioDirector.cs` so the runtime loop now builds a structured 12-second procedural soundtrack with bass, chord pads, arpeggios, lead hooks, and kick/snare/hat rhythm instead of leaning mostly on drones and rumble.
  - Kept the cave ambience underneath as a lighter texture layer so the run still feels subterranean without the music collapsing back into a low humming bed.
  - Built a fresh secondary Android APK from this laptop and reinstalled it to the connected Samsung phone as `Endless Dodge Test` (`com.oreniq.endlessdodge.secondary`).
  - Captured the longer Android packaging path in the log: this laptop's Unity secondary builds can spend many minutes inside IL2CPP / Gradle packaging even when the code change is mainly audio generation, so long waits do not automatically mean the build is frozen.
- Decisions / reversions:
  - Keep the soundtrack procedural for now so iteration stays fast and repo-safe instead of blocking on imported/licensed music files.
  - Continue using the side-by-side secondary package for rapid phone feedback while leaving the desktop-owned original app untouched.
- Verification:
  - `dotnet build Assembly-CSharp.csproj -nologo /p:UseSharedCompilation=false` succeeded with `0` warnings and `0` errors.
  - `dotnet build Assembly-CSharp-Editor.csproj -nologo /p:UseSharedCompilation=false` succeeded with `0` warnings and `0` errors.
  - Unity batch build `AndroidBuildUtility.BuildSecondaryDebugApkBatchmode` completed successfully and wrote `Builds/Android/EndlessDodge1-secondary-debug.apk` (`Logs/android-secondary-music-build.log`).
  - `adb shell dumpsys package com.oreniq.endlessdodge.secondary` showed `lastUpdateTime=2026-04-22 21:15:09`.
  - `adb shell monkey -p com.oreniq.endlessdodge.secondary -c android.intent.category.LAUNCHER 1` launched the updated secondary app.
- Next best action:
  - Listen to `Endless Dodge Test` on the phone and decide three things first: whether the new loop finally feels like music, whether it is the right style for the game, and whether the volume balance against SFX is close enough to keep iterating from this direction.

### 2026-04-22 - Secondary phone-test app workflow
- Goal:
  - Make this laptop able to install and update a safe side-by-side test build on the phone without overwriting the desktop-owned original app.
- What changed:
  - Extended `Assets/Editor/AndroidBuildUtility.cs` with a secondary-test build path that temporarily builds as `com.oreniq.endlessdodge.secondary` with the device label `Endless Dodge Test`, then restores the normal project identifiers after the build.
  - Added `Tools/Android/Build Secondary Test APK`, `Tools/Android/Build And Install Secondary Test APK`, and the matching batch entry point so this workflow is repeatable from either the editor or automation.
  - Built `Builds/Android/EndlessDodge1-secondary-debug.apk` on this laptop, installed it over ADB to the connected Samsung phone, and confirmed both `com.oreniq.endlessdodge` and `com.oreniq.endlessdodge.secondary` are now present side by side on the device.
  - Launched the secondary app once from ADB so the phone is ready for visual/feel feedback against the latest laptop-side changes.
- Decisions / reversions:
  - Keep the desktop as the primary machine for the original package and use the secondary package only as the laptop testing lane until shared signing is intentionally set up.
  - Do not permanently rename the real app or change the repo's default package identity; the secondary package name is a build-time override only for test installs.
- Verification:
  - `adb devices -l` detected the connected phone as `SM_S948U`.
  - `adb install -r -t Builds/Android/EndlessDodge1-secondary-debug.apk` returned `Success`.
  - `adb shell pm list packages | Select-String 'com.oreniq.endlessdodge'` showed both `com.oreniq.endlessdodge` and `com.oreniq.endlessdodge.secondary`.
  - `adb shell monkey -p com.oreniq.endlessdodge.secondary -c android.intent.category.LAUNCHER 1` launched the secondary package.
- Next best action:
  - Test the `Endless Dodge Test` app on the phone, give feedback on the cave presentation and gameplay feel, then keep iterating here while leaving the desktop-owned original package untouched until the signing identities are shared.

### 2026-04-22 - Desktop primary / laptop secondary clarification
- Goal:
  - Correct the handoff so it reflects the real workstation roles and the actual requirement for working on the same phone app from both machines.
- What changed:
  - Recorded that the desktop remains the primary machine and this laptop is the secondary machine.
  - Added the explicit cross-machine rule that if both computers need to update the same installed phone app, they must share the same signing identity.
  - Clarified the two supported ways to do that:
    - temporary debug-keystore bridge from the desktop to the laptop
    - preferred shared external signing folder for long-term use
- Decisions / reversions:
  - Do not treat this laptop as the new primary machine.
  - If signing secrets are intentionally not copied to the laptop, then this laptop remains a code-and-sync workstation, not a direct phone-update workstation.
- Verification:
  - Re-read the current Android signing docs and editor scripts in this checkout.
  - Confirmed this laptop still has no `%USERPROFILE%\.android\debug.keystore`, no shared signing folder, and no signing override environment variables configured.
- Next best action:
  - Decide whether the desktop's debug keystore can be copied here as a temporary bridge; if not, keep building/installing from the desktop and use the laptop only for repo work until shared signing is intentionally set up.

### 2026-04-22 - Current workstation signing-path inspection
- Goal:
  - Determine whether the newer repo files contain enough information to switch this workstation over to the other PC's Android signing path for phone updates.
- What changed:
  - Read the updated `logs.md`, `docs/WORKSTATION_SYNC.md`, `docs/COMPUTER_SWITCH_CHECKLIST.md`, and the Android editor scripts.
  - Confirmed the newer intended workflow is `AndroidBuildUtility` plus `AndroidSigningConfigResolver`, which prefers shared external signing and otherwise falls back to the machine-local Android debug keystore for debug phone builds.
  - Verified this workstation currently has only the local release-signing files in `UserSettings/Android`, but does not have `%USERPROFILE%\.android\debug.keystore`.
  - Verified this workstation also does not currently have the shared signing folder at `Documents/EndlessDodge1/SharedSigning/Android/`, and the `%ENDLESSDODGE_SIGNING_CONFIG%` / `%ENDLESSDODGE_SIGNING_ROOT%` overrides are unset.
  - Verified the older `AndroidBuildAutomation` script still exists beside the newer workflow and still applies local release signing directly, which can conflict with the current documented cross-PC phone-testing flow.
- Decisions / reversions:
  - The repo files are enough to explain the intended cross-PC signing workflow, but not enough to actually adopt the other PC's signing identity on this workstation without the missing keystore or shared signing files.
  - Prefer the `AndroidBuildUtility` path for future phone testing on this workstation, not the older `AndroidBuildAutomation` path.
  - Do not invent or recreate a debug keystore here, because that would create yet another signing identity and would not match the other PC.
- Verification:
  - `.git/HEAD` on this workstation points to `refs/heads/master`, and the current local head is `8270e8929159b870de7a06e2fdb08d09ef761650`.
  - `UserSettings/Android` contains `release-signing.json` and `oreniq-release.keystore`.
  - `%USERPROFILE%\.android\debug.keystore` is currently missing on this workstation.
  - `Documents/EndlessDodge1/SharedSigning/Android/` and `OneDrive/Documents/EndlessDodge1/SharedSigning/Android/` are currently missing on this workstation.
  - `%ENDLESSDODGE_SIGNING_CONFIG%` and `%ENDLESSDODGE_SIGNING_ROOT%` are currently unset on this workstation.
- Next best action:
  - Bring over the other PC's actual debug keystore or shared signing files, then use the newer `Tools/Android/Build Debug APK` path to verify this workstation can update the phone with the intended signing identity.

### 2026-04-22 - Rewarded-flow recovery and cave-audio tempo pass
- Goal:
  - Restore the missing rewarded ad surfaces on phone, add a proper one-time rewarded revive prompt, and make the cave ambience louder, faster, and less sleepy for real device testing.
- What changed:
  - Updated `MonetizationManager` so rewarded-ad simulation works on development phone builds again instead of being editor-only, which restores the post-run double-coins offer during device testing.
  - Added a one-time mid-run rewarded revive prompt in `GameManager` with `Watch Ad: Revive` and `End Run` actions, plus analytics events for request/result.
  - Reworked `EndlessDodgeAudioDirector` toward a shorter, more rhythmic cave loop with stronger percussion/noise texture, higher music gain, and loop-edge fades to reduce the loud end-of-cycle click.
  - Built a fresh Android debug APK and installed it to the connected phone from this machine.
- Decisions / reversions:
  - Keep rewarded ads simulated in debug/dev builds until a real ad provider is integrated, so monetization UI can still be validated on-device without blocking on SDK setup.
  - Treat the new rewarded revive as a single-use continue per non-daily run for now so it does not undermine challenge fairness or become an infinite continue loop.
- Verification:
  - `dotnet build Assembly-CSharp.csproj -nologo /p:UseSharedCompilation=false` succeeded with `0` warnings and `0` errors.
  - `dotnet build Assembly-CSharp-Editor.csproj -nologo /p:UseSharedCompilation=false` succeeded with `0` warnings and `0` errors.
  - Unity batch build `Logs/codex-android-build-15.log` finished with `Build Finished, Result: Success.`
  - Fresh artifact written to `Builds/Android/EndlessDodge1-debug.apk` at `2026-04-22 4:53 PM` local time.
  - `adb install -r` succeeded and the app was relaunched on the phone.
- Next best action:
  - Retest on phone specifically for music loudness/tempo/loop smoothness, confirm the double-coins button is back after a run, and verify whether the new rewarded revive prompt feels worth keeping as a release feature.

### 2026-04-22 - Inventory text geometry and gameplay-audio recovery pass
- Goal:
  - Fix the remaining inventory illegibility/overlap on phone and restore clearly audible gameplay ambience in the current Android build.
- What changed:
  - Overrode `InventoryMenu` layout values at runtime so old serialized scene values stop silently forcing the smaller legacy inventory sizing.
  - Reworked `InventorySlotUI` text regions again with explicit top/bottom label bands instead of the broken stretch math that was collapsing and overlapping text on device.
  - Shortened the inventory description and status copy so each card reads more cleanly at phone scale.
  - Increased gameplay music gain in `EndlessDodgeAudioDirector`, added a more audible midrange chamber-resonance layer, raised air/drip presence, and added a playback keepalive so the active music source restarts if it ever drops out.
- Decisions / reversions:
  - Keep the inventory on a compact two-line card model rather than trying to fit full shop-style detail density on every inventory card.
  - Favor more audible cave ambience even if it is slightly less subtle, because silence on the phone is a worse failure than a mix that needs another balancing pass.
- Verification:
  - `dotnet build Assembly-CSharp.csproj -nologo /p:UseSharedCompilation=false` succeeded with `0` errors.
  - Unity batch Android build `codex-android-build-14.log` completed successfully.
  - Fresh APK `Builds/Android/EndlessDodge1-debug.apk` was written at `2026-04-22 4:25 PM` and installed successfully over ADB.
- Next best action:
  - Retest only the inventory readability and the gameplay music presence/volume on the phone, then capture only any remaining misses.

### 2026-04-22 - Android batch-build recovery and phone UI follow-up pass
- Goal:
  - Recover the batch Android build path on this PC, then use the newest phone screenshots to fix the inventory regression, finally move the main-menu buttons, and make the cave presentation read more sharply on device.
- What changed:
  - Fixed `Assets/Editor/AndroidBuildUtility.cs` so batch Android debug builds now force the supported `IL2CPP + ARM64` configuration instead of the earlier invalid fast-debug backend combination.
  - Verified the Android batch build pipeline now completes end-to-end and writes `Builds/Android/EndlessDodge1-debug.apk` successfully on this machine.
  - Pulled the latest phone screenshots directly from the connected device and used them as the source of truth for this UI pass.
  - Reworked `InventorySlotUI` into a clearer large-card layout with separate title, description, owned-count, and status regions rather than a single collapsing TMP block.
  - Updated `InventoryMenu` with larger slot sizing, darker cave-tinted viewport chrome, and a generated cave backdrop so the inventory screen no longer sits on a flat gray field.
  - Added the same generated cave-backdrop treatment to `MainMenu` and `Shop` so the menus share the cave direction instead of reading like flat tinted panels.
  - Moved the main menu button stack upward again in `MainMenu` so the spacing change is more visible on phone.
  - Increased `CaveBackgroundController` cave-sprite resolution and reduced soft fog blending so gameplay caves read less blurry.
- Decisions / reversions:
  - Keep the menu cave backdrops procedural for now; they are meant to provide sharper atmosphere quickly, not lock the final art direction.
  - Continue using the connected phone screenshots as the deciding QA artifact for UI/layout fixes instead of trusting the desktop Game view.
- Verification:
  - `dotnet build Assembly-CSharp.csproj -nologo /p:UseSharedCompilation=false` succeeded with `0` warnings and `0` errors.
  - `dotnet build Assembly-CSharp-Editor.csproj -nologo /p:UseSharedCompilation=false` succeeded with `0` warnings and `0` errors.
  - Unity batch Android build `codex-android-build-13.log` finished with `Build Finished, Result: Success.`
  - Fresh APK installed successfully over ADB and the app was relaunched on the connected Samsung phone.
- Next best action:
  - Retest the inventory first, then the main menu spacing, then the sharper gameplay cave background, and only capture the remaining misses.

### 2026-04-22 - Biome transition, visibility, and menu-theme follow-up pass
- Goal:
  - Remove the remaining level-up hitch, brighten cave readability, make the inventory cards meaningfully larger, extend the cave theme into the remaining menu surfaces, and capture release-compliance work in repo docs instead of chat only.
- What changed:
  - Switched `CaveBackgroundController` from effectively changing presentation every level to biome-based presentation blocks, so heavy sprite swaps now happen on real biome changes instead of each level tick.
  - Brightened `CaveThemeLibrary`, background blending, and obstacle tinting so hazards stand out more clearly against the cave backdrop without going back to flat blue menus.
  - Rebalanced `EndlessDodgeAudioDirector` toward louder ambient presence and less obviously melodic / synthetic content by leaning more on rumble, air, and drip texture.
  - Enlarged the inventory list cards again in `InventoryMenu` / `InventorySlotUI` with taller slots, larger text, more padding, and clearer stacked status text.
  - Applied cave-scene styling to `Shop` and `Inventory` surfaces so they stop reading like the older default-blue baseline.
  - Added `docs/RELEASE_COMPLIANCE_CHECKLIST.md` to track privacy policy, Data safety, content rating, support/store metadata, and naming-clearance work.
- Decisions / reversions:
  - Do not rename the app in code yet; first generate a shortlist and do store + trademark knockout checks before touching package/display identity.
  - Keep release-signing secrets out of the repo and out of `logs.md`; use the shared external signing-folder workflow instead.
- Verification:
  - `dotnet build Assembly-CSharp.csproj -nologo` succeeded with `0` warnings and `0` errors.
  - `dotnet build Assembly-CSharp-Editor.csproj -nologo` succeeded with `0` warnings and `0` errors.
- Next best action:
  - Build and install a fresh Android APK, retest the cave transition / audio / inventory changes on phone, then draft the privacy policy and naming shortlist against the new compliance checklist.

### 2026-04-22 - Transition hitch and cave-direction refinement pass
- Goal:
  - Remove the jarring level-up hitch, stop existing obstacles from changing look mid-run, and push the cave presentation away from soft pastel gradients toward a darker cavern feel.
- What changed:
  - Reworked `CaveBackgroundController` so cave themes are prewarmed ahead of time instead of being generated on the level-up frame, which should remove the transition-time freeze.
  - Broadened the crossfade window in `GameManager` so the visual handoff starts earlier and feels less like a late hard swap.
  - Changed `CaveThemeLibrary` so one cave biome now lasts across multiple levels instead of effectively re-skinning the whole world every single level, while still allowing progression through subtler palette shifts.
  - Locked `CaveHazardVisuals` to the theme present when an obstacle spawns, so live hazards no longer recolor or reshape themselves during gameplay.
  - Reworked the default rock obstacle art toward sharper, chunkier silhouettes and switched runtime obstacle textures to crisper sampling so the start-of-run hazards stop reading like tumbleweeds.
  - Darkened and desaturated the cave palette, reduced the soft fog treatment, and made the cave layers more silhouette-driven so the backgrounds read less like a pastel wash.
  - Rebalanced `EndlessDodgeAudioDirector` toward heavier rumble / air / drip ambience with much less melodic shimmer so the loop feels less obviously synthetic.
- Decisions / reversions:
  - Keep the cave presentation procedural for now, but use it as a stepping stone toward later authored sprites and music instead of treating it as final art.
  - Preserve the continuous difficulty ramp; the main issue this pass targeted was the transition hitch and visual/audio presentation, not the ramping model itself.
- Verification:
  - `dotnet build Assembly-CSharp.csproj -nologo` succeeded with `0` warnings and `0` errors.
  - `dotnet build Assembly-CSharp-Editor.csproj -nologo` succeeded with `0` warnings and `0` errors.
  - Unity batch Android build completed successfully and produced a fresh `Builds/Android/EndlessDodge1-debug.apk` at `2026-04-22 11:38 AM`.
  - `adb install -r Builds/Android/EndlessDodge1-debug.apk` returned `Success` on the connected Samsung phone.
  - The app was relaunched over ADB after install.
- Next best action:
  - Retest the phone build and focus on five things only: whether the level-up freeze is gone, whether backgrounds now read more like caves, whether the obstacle silhouettes feel better, whether the biome transition feels smoother, and whether the ambient audio is closer to the right direction.

### 2026-04-22 - Cave atmosphere and pacing polish pass
- Goal:
  - Make the run feel more like moving through a cave instead of sliding over flat gradients, smooth out progression between levels, and give the player stronger audio/visual context during gameplay.
- What changed:
  - Rebuilt `CaveBackgroundController` so the runtime background uses layered tunnel silhouettes, spikes, pillars, crystals, and floating cave motes instead of mostly blurry color washes.
  - Added background crossfading between the current and next cave theme so level changes blend rather than snapping abruptly.
  - Added `CaveThemeLibrary` plus themed runtime hazard styling in `Obstacle` / `ObstacleZigZag`, so hazards now read more like cave rocks, ledges, crystals, and bats.
  - Added a cave-themed runtime player visual in `PlayerController` so the character reads more like a cave creature / glow-bug than a generic placeholder.
  - Smoothed gameplay pacing by adding continuous difficulty helpers in `GameManager` and switching obstacle speed scaling to ramp through the level instead of jumping only on level-up.
  - Expanded `EndlessDodgeAudioDirector` with a louder, fuller ambient loop, slower theme crossfades, and level-progress-aware music swell so the cave ambience has more presence.
- Decisions / reversions:
  - Keep the new cave art direction procedural for now so the game can keep moving without blocking on imported art assets.
  - Treat authored sprites, obstacle variants, and higher-fidelity music as a later polish step if the direction tests well on phone.
- Verification:
  - `dotnet build Assembly-CSharp.csproj -nologo` succeeded with `0` warnings and `0` errors.
  - `dotnet build Assembly-CSharp-Editor.csproj -nologo` succeeded with `0` warnings and `0` errors.
  - Unity batch Android build completed successfully and produced a fresh `Builds/Android/EndlessDodge1-debug.apk` at `2026-04-22 11:13 AM`.
  - `adb install -r Builds/Android/EndlessDodge1-debug.apk` returned `Success` on the connected Samsung phone.
  - The app was relaunched over ADB after install.
- Next best action:
  - Retest the phone build with sound on and focus on five things only: cave detail readability, level-transition smoothness, ambient mix quality, whether obstacle/player theming feels right, and whether the new difficulty ramp feels smoother through each level.

### 2026-04-22 - Shared signing and immersion systems pass
- Goal:
  - Improve cross-PC Android build continuity without committing secrets, while also starting a proper immersion pass with scrolling cave backgrounds, level-based visual progression, runtime audio, and visible power-up feedback.
- What changed:
  - Added runtime cave-theme generation plus a scrolling layered cave background system that changes palette and mood by difficulty level.
  - Added a runtime audio director for ambient level music, coin pickup SFX, obstacle-hit feedback, level-up cues, and upgrade activation sounds without blocking on imported audio assets.
  - Added player power-up visuals for shield, magnet, slow time, speed boost, extra life, and other active buffs so the player's state reads during gameplay.
  - Wired the new background, audio, and power-up visual systems into `GameManager` so they react to real level changes, coin pickups, collisions, revives, bombs, and upgrade activations.
  - Added a reusable Android signing resolver plus editor menu support for a shared external signing folder / environment-variable workflow, and updated the Android debug build path to reuse that shared signing automatically when available.
- Decisions / reversions:
  - Do not put signing passwords or keystore contents in `logs.md` or Git, even for convenience.
  - Cross-PC phone continuity should come from a shared external signing location, not from storing secrets in the repo.
- Verification:
  - `dotnet build Assembly-CSharp.csproj -nologo` succeeded with `0` warnings and `0` errors after Unity regenerated the project files for the new scripts.
  - `dotnet build Assembly-CSharp-Editor.csproj -nologo` succeeded with `0` warnings and `0` errors.
  - Unity batch build completed successfully and produced a fresh `Builds/Android/EndlessDodge1-debug.apk` at `2026-04-22 10:31 AM`.
  - `adb install -r Builds/Android/EndlessDodge1-debug.apk` returned `Success` on the connected Samsung phone.
- Next best action:
  - Retest on phone with sound enabled and focus on four things: moving cave background feel, level-to-level progression readability, audio balance, and whether the active power-up visuals communicate state cleanly without becoming distracting.

### 2026-04-21 - Inventory rollback and menu-balance pass
- Goal:
  - Undo the bad inventory presentation regression from the previous pass and lightly rebalance the main menu so the larger challenge card does not crowd the core navigation buttons.
- What changed:
  - Replaced the failed multi-label inventory card experiment with a single centered card text block in `InventorySlotUI`, keeping the bigger cards but returning to a cleaner shop-like stacked presentation.
  - Tightened the `Inventory` header stack and viewport spacing so the screen budget goes back to the cards instead of oversized header text.
  - Fixed `MainMenu.SetButtonLayout` so it actually applies the intended Y offsets, then moved the `Play / Shop / Inventory / Exit` button stack upward slightly to create more breathing room below.
  - Rebuilt and reinstalled a fresh Android debug APK over ADB on the connected Samsung phone.
- Decisions / reversions:
  - The previous three-text inventory card layout was a regression on real hardware and should not be iterated further.
  - Keep the daily challenge panel improvements, but solve the remaining menu crowding by adjusting the button stack rather than shrinking the challenge card again immediately.
- Verification:
  - `dotnet build Assembly-CSharp.csproj -nologo` succeeded with `0` warnings and `0` errors.
  - `dotnet build Assembly-CSharp-Editor.csproj -nologo` succeeded with `0` warnings and `0` errors.
  - A fresh batch build completed successfully at `2026-04-21 4:11 PM` and produced an updated `Builds/Android/EndlessDodge1-debug.apk`.
  - `adb install -r Builds/Android/EndlessDodge1-debug.apk` returned `Success`.
- Next best action:
  - Retest only `Inventory` and the `MainMenu` vertical balance on phone. If either still feels off, capture one fresh screenshot of that exact screen and tune only that surface.

### 2026-04-21 - Inventory shop-style parity pass
- Goal:
  - Stop nudging the old inventory list layout around and instead rebuild it toward the same larger card structure that already works well in the shop, while also finally separating the daily challenge status line from its CTA button.
- What changed:
  - Reworked `InventorySlotUI` away from the old single combined label into a structured three-line card: title, description, and meta/status, using the same kind of padded stretch-text layout that the shop cards use.
  - Enlarged the `Inventory` list again with taller cards, narrower centered card width, tighter row spacing, and a lighter header stack so more of the screen budget goes to the actual cards.
  - Updated the daily challenge status text in `MainMenu` to use shorter labels such as `Best: x/y` and `Goal: x`, then expanded the challenge panel height and spacing so the action button no longer rides on top of the status line.
  - Rebuilt a fresh Android debug APK and reinstalled it over ADB onto the connected Samsung phone.
- Decisions / reversions:
  - The user feedback was right: inventory should follow the same card pattern as the shop instead of staying a compressed list with tiny text.
  - Keep using phone screenshots as the authority for readability issues instead of assuming editor changes are visible enough on-device.
- Verification:
  - `dotnet build Assembly-CSharp.csproj -nologo` succeeded with `0` warnings and `0` errors after the inventory and challenge layout pass.
  - `dotnet build Assembly-CSharp-Editor.csproj -nologo` succeeded with `0` warnings and `0` errors.
  - A fresh batch build completed successfully at `2026-04-21 4:00 PM` and produced an updated `Builds/Android/EndlessDodge1-debug.apk`.
  - `adb devices -l` detected the connected phone (`SM_S948U`), and `adb install -r Builds/Android/EndlessDodge1-debug.apk` returned `Success`.
- Next best action:
  - Retest only two screens now: `Inventory` and the `MainMenu` daily challenge card. If either still fails, capture one fresh screenshot of that exact screen and tune only that surface.

### 2026-04-21 - Third phone polish pass
- Goal:
  - Fix the remaining real-device blockers after the second phone retest: laggy mobile controls, inventory cards that still felt too small, daily challenge text overlap in `MainMenu`, and the in-run loadout label crowding the top-right menu area.
- What changed:
  - Reworked `PlayerController` mobile input again so active touch drag now moves the player directly by world-space drag delta instead of using the slower velocity-style floating-joystick model.
  - Enlarged the `Inventory` layout again with taller rows, slightly tighter outer padding, and roomier `InventorySlotUI` typography/padding so the cards read more like tappable shop cards on phone.
  - Tightened `MainMenu` daily challenge copy and expanded the challenge summary panel layout again so the title, status, and CTA fit more reliably on tall Samsung screens.
  - Lowered and deconflicted the in-game loadout HUD text in `GameManager`, and disabled its raycast target so it cannot steal taps from the top-right menu button.
  - Rebuilt a fresh Android debug APK and reinstalled it over ADB after USB debugging became available again.
- Decisions / reversions:
  - Reverted away from the slower velocity-style touch model because the real phone test said it felt delayed.
  - Keep validating from real device screenshots and APK installs rather than trusting the Unity Game view for portrait-phone readability.
- Verification:
  - `dotnet build Assembly-CSharp.csproj -nologo` succeeded with `0` warnings and `0` errors after the latest code pass.
  - `dotnet build Assembly-CSharp-Editor.csproj -nologo` succeeded with `0` warnings and `0` errors.
  - A fresh batch build completed successfully at `2026-04-21 3:34 PM` and produced an updated `Builds/Android/EndlessDodge1-debug.apk`.
  - `adb devices -l` detected the connected phone (`SM_S948U`), and `adb install -r Builds/Android/EndlessDodge1-debug.apk` returned `Success`.
- Next best action:
  - Retest the newest phone build and confirm four things only: touch responsiveness, inventory card size, daily challenge card readability, and whether the top-right menu button is now clear and tappable during play.

### 2026-04-21 - Second phone polish pass
- Goal:
  - Address the next round of real-device issues after the first phone retest: shop and inventory headers still sitting too close to the punch-hole area, inventory presentation still feeling rough, and touch controls still not matching the intended floating-joystick feel.
- What changed:
  - Reworked `PlayerController` mobile input again so drag now drives a virtual joystick-style movement vector instead of using touch delta as an absolute player target position.
  - Lowered the top header stack in `Shop` and widened the shop cards slightly so the camera area has more breathing room on tall portrait phones.
  - Reworked the `Inventory` mobile layout with more top clearance, wider cards, taller rows, clearer feedback copy, and stronger card formatting in `InventorySlotUI`.
  - Removed the old "skip normalization for scene-owned panels" behavior in `MainMenu` so the daily reward, mission summary, and challenge summary panels all go through the same phone layout pass.
  - Nudged the in-game HUD lower again so the next device check can confirm the runtime build is actually using the latest layout code.
- Decisions / reversions:
  - Use real phone screenshots as the source of truth when device behavior disagrees with the editor.
  - Prefer velocity-style floating-joystick controls over absolute drag-target controls for this game.
- Verification:
  - `dotnet build Assembly-CSharp.csproj -nologo` succeeded with `0` warnings and `0` errors.
  - `dotnet build Assembly-CSharp-Editor.csproj -nologo` succeeded with `0` warnings and `0` errors.
  - A fresh batch build completed successfully at `2026-04-21 3:02 PM` and produced an updated `Builds/Android/EndlessDodge1-debug.apk`.
  - `adb install -r Builds/Android/EndlessDodge1-debug.apk` returned `Success`.
- Next best action:
  - Retest the newest phone build and confirm four things only: shop header clearance, inventory header clearance, inventory card readability, and whether the new floating-joystick movement finally feels correct.

### 2026-04-21 - Phone readability and touch-control pass
- Goal:
  - Fix the real-device issues found during the first Android phone check: top text colliding with the camera area, crowded main menu panels, and touch controls that felt like jagged chase movement instead of direct drag movement.
- What changed:
  - Pulled phone screenshots into `Builds/PhoneScreenshots/` and used them as the source of truth for the next UI pass instead of relying on the tiny Unity Game view.
  - Reworked `PlayerController` mobile input so touch now behaves like a floating joystick / relative drag: the player mirrors the finger movement from the touch-start position instead of stepping toward the raw touch point with `Mathf.Sign`.
  - Added safe-area-aware layout normalization to the main menu, shop, inventory, and in-game HUD paths so the top content respects the phone cutout area more reliably.
  - Tightened the `MainMenu` daily reward, daily challenge, and daily mission summary panel layout so the reward/status text stacks cleanly on phone.
  - Shortened the in-run loadout fallback label from `No loadout selected` to `No loadout` to reduce top-right HUD crowding on narrow portrait screens.
  - Rebuilt a fresh Android debug APK and installed it over ADB onto the connected Samsung phone.
- Decisions / reversions:
  - Keep fixing the scene-owned UI and runtime normalizers instead of adding editor-only readability hacks.
  - Prefer direct phone screenshots and APK installs as the validation loop for mobile UI and controls.
- Verification:
  - `dotnet build Assembly-CSharp.csproj -nologo` succeeded with `0` warnings and `0` errors after the UI/control pass.
  - A closed-editor batch build succeeded and produced an updated `Builds/Android/EndlessDodge1-debug.apk`.
  - `adb devices -l` detected the connected phone (`SM_S948U`), and `adb install -r Builds/Android/EndlessDodge1-debug.apk` returned `Success`.
- Next best action:
  - Retest the phone build and confirm three things only: the top text no longer collides with the camera area, the main menu panels are readable, and the new drag controls feel closer to the intended joystick-style movement.

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

### 2026-04-21 - Android phone-test helper
- Goal:
  - Make it possible to get a phone-installable Android build quickly even before the release keystore is recovered on this machine.
- What changed:
  - Added `Assets/Editor/AndroidBuildUtility.cs` with `Tools/Android/Build Debug APK` and `Tools/Android/Build And Install Debug APK`.
  - The helper builds `Builds/Android/EndlessDodge1-debug.apk`, uses the enabled Build Settings scenes, temporarily disables custom release signing for the build itself, then resolves Unity's generated Gradle output into the stable repo output path.
  - The install action uses the Unity-bundled `adb.exe` and installs with `adb install -r` when a USB-debuggable phone is connected.
- Decisions / reversions:
  - Prefer debug APKs for immediate phone testing instead of blocking on the missing release keystore.
  - Remove the earlier editor-only seeded-coin workaround from the shop and keep the readability changes focused on responsive layout behavior.
- Verification:
  - `dotnet build Assembly-CSharp-Editor.csproj -nologo` succeeded with `0` warnings and `0` errors after adding the helper.
  - A closed-editor batch build succeeded and produced `Builds/Android/EndlessDodge1-debug.apk`.
  - `adb devices` returned no connected Android devices during this session.
- Next best action:
  - Connect a phone with USB debugging enabled, then use `Tools/Android/Build And Install Debug APK`, or manually copy/install `Builds/Android/EndlessDodge1-debug.apk`.

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
