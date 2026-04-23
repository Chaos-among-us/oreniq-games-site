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
  - current active repo on this machine reached remote commit `9b065da` on `2026-04-13`
  - prior multi-computer sync cleanup happened on `2026-04-13`
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
- Real phone validation is still needed on the newest Android build (`Logs/codex-android-build-15.log`, artifact `Builds/Android/EndlessDodge1-debug.apk`) for:
  - whether the gameplay music is finally loud enough
  - whether the loud click at the end of the ambient loop is gone or at least much smaller
  - whether the new ambient loop feels faster and less synthetic/boring
  - whether the cave backgrounds are sharp enough and still readable against dark obstacles
  - Some phone UI polish may still remain in `MainMenu` and `Inventory`; do not assume the earlier layout issues are fully closed until the newest APK is retested.
  - Cross-PC Android signing must stay out of Git, so the repo needs to point to a safe shared external signing folder instead of storing signing secrets in `logs.md`.
  - Runtime audio/visual feedback now exists, but it is still procedural placeholder content and may need stronger authored polish.
  - The post-run rewarded double-coins offer was restored for debug/dev builds and must be re-verified on phone after a run.
  - A one-time rewarded revive prompt was added and must be re-verified on phone to decide whether it feels good enough to keep.
  - The rewarded flow still needs a real ad provider before release; current phone testing still uses simulation in debug/dev builds.
  - Coin-pack IAPs and starter-offer surfaces now exist, but they still need production store configuration and polish.
  - Store assets, screenshots, privacy policy, and Play Console setup are still pending.
  - Local Android signing data is intentionally machine-local and must not be committed:
    - `UserSettings/Android/oreniq-release.keystore`
    - `UserSettings/Android/release-signing.json`
  - Secret material must not be pasted into `logs.md`; use the shared external signing folder or the supported environment-variable overrides instead.
  - When changing machines, re-verify Unity modules, package restore, and release-signing setup.

## Current Focus
1. Retest Android build 15 on phone with sound on. First checks:
   - music loudness
   - loop-end click/pop
   - music tempo/energy
   - post-run double-coins button visibility
   - rewarded revive prompt behavior
2. Tune the cave atmosphere pass after that retest: background detail, theme transitions, audio mix, and obstacle/player theming.
3. Finish any remaining real-phone UI/layout cleanup in `MainMenu`, `Inventory`, and the in-run HUD after the audio/rewarded retest is checked.
4. Use a shared external Android signing folder so different PCs can update the same phone install without committing signing secrets.
5. Replace the simulated rewarded flow with a real ad provider.
6. Finish production IAP/store configuration, listing assets, screenshots, privacy policy, Data safety declarations, and Play Console metadata.

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
12. Build 15 is the current known-good baseline from this machine:
   - log: `Logs/codex-android-build-15.log`
   - artifact: `Builds/Android/EndlessDodge1-debug.apk`
   - install status on this machine: succeeded on phone via `adb install -r`
   - important signing note: build 15 did **not** use shared signing yet; it fell back to this machine's local Android debug keystore because shared signing was not found
   - current machine debug-keystore path:
     - `C:\Users\antho\.android\debug.keystore`
13. First manual retest on the other PC should be:
   - music loudness during gameplay
   - whether the ambient loop still ends with a loud click
   - whether the music feels faster and less synthetic
   - whether the post-run `Watch Ad` double-coins button appears again
   - whether the mid-run rewarded revive prompt appears and feels good enough to keep
14. If the other PC needs to update the exact same currently installed app **before** shared/release signing is recovered:
   - copy this machine's debug keystore file:
     - `C:\Users\antho\.android\debug.keystore`
   - place it on the other PC at:
     - `%USERPROFILE%\.android\debug.keystore`
   - this makes the other PC's debug builds use the same signing identity as the app currently installed from this PC
   - do **not** commit this file to Git
   - this is a temporary bridge only; the preferred long-term fix is still shared signing via:
     - `Documents/EndlessDodge1/SharedSigning/Android/`
15. Important secret rule:
   - do not paste signing keys, keystore passwords, or other secrets into `logs.md` or any repo-tracked file
   - keep them in the shared external signing folder or environment variables only
16. Do not create a new release keystore unless the old one is truly gone and you intentionally want a new Android signing identity.

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
