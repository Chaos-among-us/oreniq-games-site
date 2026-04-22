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
- Real phone readability and feel are now partially verified, but the current Android debug build still needs another focused on-device pass for four remaining issues:
  - `Inventory` cards still need a final real-phone check after being rebuilt to follow the shop-style card structure
  - the `MainMenu` daily challenge card still has text overlap
  - verify the newest spacing change actually clears the daily challenge status line from the CTA button on device
- Audio, feedback, particles, and transitions are still light or missing.
- The rewarded flow still needs a real ad provider.
- Coin-pack IAPs and starter-offer surfaces now exist, but they still need production store configuration and polish.
- Store assets, screenshots, privacy policy, and Play Console setup are still pending.
- Local Android signing data is intentionally machine-local and must not be committed:
  - `UserSettings/Android/oreniq-release.keystore`
  - `UserSettings/Android/release-signing.json`
- When changing machines, re-verify Unity modules, package restore, and release-signing setup.

## Current Focus
1. Retest the newest Android debug build on a real phone and verify the rebuilt inventory cards plus the daily challenge panel spacing.
2. Finish the remaining real-phone UI/layout cleanup in `MainMenu`, `Inventory`, and the in-run HUD before moving on to broader launch polish.
3. Replace the simulated rewarded flow with a real ad provider.
4. Add audio and feedback polish needed for launch quality.
5. Finish production IAP/store configuration, listing assets, screenshots, privacy policy, and Play Console metadata.

## When The Other PC Is Available
1. On the other PC, check the project copy that was actually used there.
2. Look inside that repo's `UserSettings/Android` folder for:
   - `release-signing.json`
   - `oreniq-release.keystore`
3. If they are not there, search the other PC for:
   - `oreniq-release.keystore`
   - `release-signing.json`
4. Copy both files into this machine's repo folder:
   - `UserSettings/Android/`
5. On this machine, reopen Unity if needed and click:
   - `Tools/Android/Apply Local Release Signing`
6. Verify the local signing warning is gone or that Unity reports signing was applied successfully.
7. Do not create a new release keystore unless the old one is truly gone and you intentionally want a new Android signing identity.

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
