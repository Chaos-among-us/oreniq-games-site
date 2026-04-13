# EndlessDodge1 Logs

## Project Path
Use this project:
- `C:\Users\antho\Documents\UnityProjects\Block-dodger1`

## Source Of Truth Correction
- The earlier "stale clone" warning in this file was wrong.
- That came from a malfunctioning handoff thread and should not be followed.
- The current files in `C:\Users\antho\Documents\UnityProjects\Block-dodger1` are the source of truth.
- Continue logging all major changes in this file.

## Handoff Note
- This log intentionally does **not** include hidden system instructions, hidden chain-of-thought, or private tool internals.
- Instead, it includes the strongest public handoff possible:
  - project scope
  - user preferences
  - implemented systems
  - major decisions
  - reversions
  - current rough edges
  - next recommended work
  - a visible-turn conversation log
  - roadmap
  - full scripts
- A new thread should be able to continue from this file without needing any hidden context.

## Seamless Handoff Instructions
- In a new thread, tell the assistant to read:
  - `C:\Users\antho\Documents\UnityProjects\Block-dodger1\logs.md`
- Tell it to treat that file as the source of truth for:
  - current project state
  - user preferences
  - what has already been built
  - what was reverted
  - what still needs work
- Tell it to continue working in:
  - `C:\Users\antho\Documents\UnityProjects\Block-dodger1`

## Copy-Paste Prompt For A New Thread
Use the file `C:\Users\antho\Documents\UnityProjects\Block-dodger1\logs.md` as the full project handoff and source of truth. Read it before making assumptions. Continue development in `C:\Users\antho\Documents\UnityProjects\Block-dodger1`. Follow the user preferences documented in the log, preserve the current systems and reversions already made, and continue from the current project state rather than restarting or re-architecting unnecessarily.

## Current Local File Status
- `logs.md` is the live handoff and working change log for this project.
- New work from `2026-04-11` onward should be appended here as changes are made.

## Current Highest-Priority Next Work
- install Android Build Support for Unity `6000.4.0f1` on this machine
- open Unity and let Package Manager restore the new Analytics/Auth/IAP packages
- verify the local Android release-signing setup loads from `UserSettings/Android`
- add real ad mediation and coin-pack IAPs on top of the simulated rewarded flow
- test readability, feel, and monetization flow on a real phone
- finish icons, screenshots, store copy, privacy policy, and Play Console setup

## Important Recent Reversion State
- The project is currently on a **better restored baseline** after undoing the later clunky main-menu/shop cleanup batch.
- That means:
  - the later overlapping/clunky challenge-panel layout change was undone
  - the later shop-header/layout behavior from that batch was undone
  - broken dead legacy shop junk was **not** reintroduced
- Future work should continue from the current look, not from the reverted clunkier version.

## Engine / Setup
- Unity 6
- 2D mobile game
- Portrait orientation
- Current scenes:
  - `MainMenu`
  - `Game`
  - `Shop`
  - `Inventory`

## User Workflow Preferences
- Give one short Unity step at a time when asking the user to do something manually.
- Always include full scripts when providing code edits.
- Be very explicit with Unity clicks.
- Assume the user is a beginner and needs very direct instructions.
- Do as much of the heavy lifting as possible in code and scene files.

## Core Design Direction
- This is an endless dodge mobile arcade game.
- Permanent upgrades were replaced with consumable upgrades.
- The player buys consumables in `Shop`.
- Consumables are stored in `UpgradeInventory`.
- The player equips up to 3 upgrade types in `Inventory`.
- Equipped upgrades appear in the `Game` scene.
- Upgrades can be activated in-run, and some now auto-chain.
- `Shield` blocks a hit.
- `Extra Life` revives once.

## Business / Product Goal
The long-term goal is to turn this into a polished, highly replayable mobile game with strong retention, organic competition, and eventual monetization. The target is not just working gameplay but a launchable product with:
- strong first-session clarity
- daily return hooks
- competitive and shareable loops
- Google Play-ready quality
- future support for leaderboards, achievements, rewarded systems, and growth features

## Current Launch Targets
- Release target: Friday, `2026-04-17`
- Near-term revenue target: reach roughly `$1,000/day` within the first month after release
- Year-one ambition: scale toward `$3,000,000` in the first year through retention, ads, IAP, and rapid iteration

## What Is Implemented Right Now

### Core Gameplay
- Player movement in a portrait play area.
- Border clamping so the player stays inside the lane.
- Obstacle spawning and difficulty scaling.
- Coins spawn and are collected.
- Coins now render as circular gold pickups instead of square-looking placeholders.
- Game over and restart loop exists.
- Best score saving exists.
- Post-run summary exists.

### Consumable Upgrade System
- `UpgradeInventory` persists across scenes.
- Shop purchases add consumables to inventory.
- Inventory allows equipping up to 3 upgrade types.
- Game builds in-run upgrade controls from the equipped loadout.
- Shield works during gameplay and can prevent death.
- Extra Life works as a revive.
- Additional consumables have gameplay effects implemented:
  - Speed Boost
  - Coin Magnet
  - Double Coins
  - Slow Time
  - Smaller Player
  - Score Booster
  - Bomb
  - Rare Coin Boost
- In-run upgrade buttons support persistent and auto-use behavior for the appropriate buffs.

### UI / Scene Progress
- `Game` has scene-backed HUD elements and a post-run summary panel.
- `Game` has a tutorial overlay and pause/settings overlay.
- `MainMenu` has a daily reward panel, daily missions summary, and daily challenge summary.
- `Shop` is a scrollable consumable store.
- `Inventory` is a scrollable equipped-loadout scene with a back button.
- A lot of UI was moved toward scene-owned objects instead of script-only temporary text, because the user prefers to tweak things directly in Unity.

### Retention Systems
- Daily login reward and streak reward system exists.
- Daily missions system exists with 3 daily missions.
- Daily challenge system exists and is distinct from daily missions.

### Daily Challenge Notes
- The daily challenge is a separate featured run, not the same thing as missions.
- Challenge runs disable consumables for fairness.
- Challenge completion can award coins plus a buff reward.
- Challenge reward claiming exists.
- Challenge tracking and daily best storage exist.
- Challenge variety exists through rotating templates, but more templates would help later.

## Current Known Rough Edges / Likely Next Polish Tasks
- Some UI text is still too small in desktop preview, even if it may be acceptable on phone.
- Main menu challenge and mission panels still need polish so they feel cleaner and less clunky.
- Some scene-owned UI migration is still incomplete or inconsistent depending on scene.
- Shop header and body readability has been sensitive to layout changes, so future tweaks should be conservative and scene-first.
- Daily challenge presentation and menu card polish still need refinement.
- Audio, music, juice, and transitions are still missing or incomplete.
- Leaderboards, achievements, and social sharing are not integrated yet.
- Device testing on a real phone still needs to happen before a serious push.

## Recommended Next Build Order
1. Finish scene-owned UI polish in all scenes.
2. Improve text readability and spacing across Game, MainMenu, Shop, and Inventory.
3. Add audio, music, feedback, particles, and better juice.
4. Improve daily challenge presentation and daily mission UX.
5. Add leaderboard, social, and sharing scaffolding.
6. Add Android device testing and release prep.

## Important Context For A Fresh Chat
- The user likes scene-owned Unity UI more than script-generated UI.
- The user wants edits done directly when possible, not just suggestions.
- The user cares about making this commercially successful long-term, but wants practical steps first.
- The user is open to creative license.
- The user prefers not to spend messages on tiny back-and-forth when a reasonable assumption can be made.

## Session Timeline
- Main development in this log happened across `2026-04-09` and `2026-04-10`.
- The project was resumed from a user-provided checkpoint after a previous long ChatGPT thread.
- Early work focused on restoring the known code state and getting the inventory/loadout system working.
- Mid-session work focused on making upgrades actually function in gameplay, then fixing the UI/readability fallout.
- Later work added retention systems: daily reward, daily missions, and a separate daily challenge mode.
- The most recent work before this log request involved reverting a clunky main-menu/shop cleanup batch back to a better baseline.
- Work resumed again on `2026-04-11` for a launch sprint focused on release readiness, monetization, analytics, and Android setup.

## Detailed Change Log

### 2026-04-11 launch sprint continuation
- Confirmed the active project is `C:\Users\antho\Documents\UnityProjects\Block-dodger1`.
- Corrected this log so it no longer tells future threads to use the wrong project path.
- Added Unity package entries for:
  - `com.unity.services.analytics`
  - `com.unity.services.authentication`
  - `com.unity.purchasing`
- Added launch/release service scripts:
  - `Assets/Scripts/Services/UnityServicesBootstrap.cs`
  - `Assets/Scripts/Services/LaunchAnalytics.cs`
  - `Assets/Scripts/Services/MonetizationManager.cs`
- Added first monetization surface:
  - post-run rewarded double-coins flow
  - currently simulated in-editor until a real ad provider is integrated
- Instrumented launch analytics in core run and economy flows:
  - run start
  - run finish
  - shop purchase
  - daily reward claim
  - daily mission reward claim
  - daily challenge start
  - daily challenge reward claim
  - rewarded offer request/result
- Replaced the old roadmap with a release-focused `ROADMAP.md`.
- Added `RELEASE_SPRINT.md` for the `2026-04-17` launch push.
- Locked release identity:
  - company/developer name: `Oreniq Games`
  - Android package: `com.oreniq.endlessdodge`
- Created a local Android release keystore at:
  - `UserSettings/Android/oreniq-release.keystore`
- Created a local signing config at:
  - `UserSettings/Android/release-signing.json`
- Important:
  - the keystore and signing JSON are intentionally local-only and should stay out of committed source control
  - do not paste keystore passwords into this log
- Added editor-side release-signing helper:
  - `Assets/Editor/AndroidReleaseSigningConfigurator.cs`
  - automatically reapplies local signing from `UserSettings/Android/release-signing.json` when Unity opens
  - also adds menu items under `Tools/Android`
- Updated `ProjectSettings/ProjectSettings.asset` to use the local release keystore path and alias.
- Verified:
  - script assembly compiled successfully after the launch-sprint code changes
- Current blocker:
  - this machine's Unity `6000.4.0f1` install currently has Windows, WebGL, Android, and iOS support installed
- Follow-up progress:
  - Android Build Support, Android SDK & NDK Tools, OpenJDK, and Visual Studio Community were installed through Unity Hub
  - the requested Unity packages restored successfully:
    - `com.unity.services.analytics`
    - `com.unity.services.authentication`
    - `com.unity.purchasing`
  - both assemblies compiled cleanly after restore:
    - `Assembly-CSharp`
    - `Assembly-CSharp-Editor`
  - local Android release signing was verified in Unity through:
    - `Tools/Android/Apply Local Release Signing`
    - success dialog: `Applied local Android release signing from UserSettings/Android/release-signing.json.`
  - Android build profile was switched and is now the active profile in Unity
- Next recommended step:
  - verify the release-signing helper loads correctly from the Unity menu
  - then switch the active build target to Android and continue release configuration

### Initial restore / baseline
- Restored the project to the checkpoint the user provided.
- Confirmed the active project should be `C:\Users\antho\EndlessDodge1`.
- Noted that `C:\Users\antho\OneDrive\Documents\GitHub\Block-dodger1` is stale and should not be used.
- Ensured the `Inventory` scene existed in build settings.

### Inventory / loadout system
- Added `InventoryMenu.cs`.
- Added `InventorySlotUI.cs`.
- Expanded `UpgradeInventory.cs` to support equipped upgrades as a 3-slot loadout.
- Built the inventory selection flow so upgrades can be equipped/unequipped and persisted.
- Iterated on the inventory layout several times:
  - broken grid attempt
  - broken narrow column attempt
  - fixed scroll-list version
- Converted the inventory scene toward scene-owned UI and preserved a visible back button.

### Gameplay / upgrade behavior
- Fixed the shield-bypass problem by moving collision resolution to the player side.
- Fixed player border clamping so the player stays inside the lane.
- Added support for equipped upgrades to appear in `Game`.
- Implemented gameplay effects for:
  - Shield
  - Extra Life
  - Speed Boost
  - Coin Magnet
  - Double Coins
  - Slow Time
  - Smaller Player
  - Score Booster
  - Bomb
  - Rare Coin Boost
- Added persistent/auto-use behavior for appropriate in-run buffs.

### Main menu / navigation
- Added an inventory entry path from `MainMenu`.
- Added daily reward UI and later adjusted it multiple times for placement and readability.
- Added daily missions summary UI and later converted it to a simpler summary + pop-out approach.
- Added daily challenge summary UI.
- Multiple layout passes were done on menu panels because text size and overlap were persistent issues.

### Shop
- Replaced the old shop logic that only really exposed the old leftover buttons.
- Built a scrollable 10-upgrade consumable store.
- Iterated on shop button layout repeatedly because readability was poor in the desktop phone preview.
- Moved the shop closer to scene-owned UI for title/header/back button/content area.
- Removed dead legacy shop objects from the scene.
- Later reverted a clunkier shop/main-menu cleanup batch after user feedback.

### Game UI
- Added scene-backed HUD elements in `Game`.
- Added a scene-backed post-run summary panel.
- Added a pause button and pause/settings panel.
- Added tutorial overlay and haptics toggle support.
- Enlarged and cleaned up multiple game HUD elements after overlap/readability complaints.

### Rewards / retention systems
- Added daily login reward system via `DailyRewardSystem.cs`.
- Added daily missions via `DailyMissionSystem.cs`.
- Added a separate daily challenge system via `DailyChallengeSystem.cs`.
- Fixed the daily challenge naming/goal mismatch so challenge type and goal align better.
- Added daily challenge claiming and repeat-run reward locking.

### Coins / visuals
- Coins were changed to render as circular gold-looking pickups instead of square-looking placeholders.

### Files added during the project
- `Assets/Scripts/DailyChallengeSystem.cs`
- `Assets/Scripts/DailyMissionSystem.cs`
- `Assets/Scripts/DailyRewardSystem.cs`
- `Assets/Scripts/GameSettings.cs`
- `Assets/Scripts/InventoryMenu.cs`
- `Assets/Scripts/InventorySlotUI.cs`
- `ROADMAP.md`
- This current log file, originally created as `FRESH_START_PROTOCOL.md` and then renamed to `logs.md`

### Files heavily modified during the project
- `Assets/Scripts/GameManager.cs`
- `Assets/Scripts/MainMenu.cs`
- `Assets/Scripts/ShopManager.cs`
- `Assets/Scripts/InventoryMenu.cs`
- `Assets/Scripts/InventorySlotUI.cs`
- `Assets/Scripts/UpgradeInventory.cs`
- `Assets/Scripts/PlayerController.cs`
- `Assets/Scripts/Coin.cs`
- `Assets/Scenes/Game.unity`
- `Assets/Scenes/MainMenu.unity`
- `Assets/Scenes/Shop.unity`
- `Assets/Scenes/Inventory.unity`

### Latest revert state
- The last reverted batch was the cleanup/integration work that happened after the user message:
  - `the look is clunky and the panel overlaps other buttons on the menu so if we can move them around and resize a little wuuld be good. Please also integrate it with unity so each thing can be controlled like we did the other scenes. please also do the cleanup tasks.`
- That revert restored the previous `MainMenu` challenge/mission card layout and the previous `ShopManager.cs` behavior.
- Dead corrupt legacy shop junk was not reintroduced, because it was broken baggage rather than useful UI.

## Conversation Log
This section is the visible-turn project log for this thread. It captures the user-visible conversation and the major work done at each step.

1. User asked what help was possible for finishing the game.
   Assistant explained it could implement features, fix bugs, polish UI/UX, balance gameplay, clean code, review the project, and help with release prep.

2. User asked if other ChatGPT chats could be accessed.
   Assistant said no, unless the user pasted their contents, and offered to reconstruct the roadmap from the project.

3. User said they were pushing the latest update manually to GitHub.
   Assistant confirmed the local repo looked clean and the user could push manually.

4. User supplied the main project checkpoint:
   - Unity 6
   - 2D portrait mobile game
   - scenes: MainMenu, Game, Shop, Inventory
   - consumable-upgrade design
   - current scripts
   - current UI state
   - known shield bug
   - loadout system in progress

5. Assistant resumed from that checkpoint and directed the user to ensure `Inventory.unity` existed and was added to build settings.

6. User completed the scene/build setup and asked what was next.
   Assistant began building the 3-upgrade loadout system.

7. Assistant added `InventoryMenu.cs` and `InventorySlotUI.cs`.
   User could not see the component in Unity.

8. Assistant first suggested refresh, then found Unity was pointed at the wrong folder.
   After correcting the project path, the user could add the component.

9. The user manually wired the first inventory slot through several step-by-step messages:
   - set `Slot UIs` size
   - add `Inventory Slot UI`
   - set `Upgrade Type` to Shield
   - assign the `Inventory Menu` reference
   - assign the slot into `Slot UIs`

10. User asked whether all 10 upgrades should be set up now.
    Assistant updated the system so one slot could act as a template and the rest would be generated/runtime-bound.

11. The inventory UI looked terrible through several iterations.
    Multiple layout passes were attempted:
    - ugly compressed layout
    - worse broken vertical layout
    - still broken
    - eventually fixed by making it a readable scroll list

12. User confirmed the inventory scroll worked and then asked to stop doing one-step-at-a-time messages because of chat limits.
    Assistant took more initiative from that point onward.

13. Assistant implemented the larger consumable loop:
    - up to 3 equipped upgrades
    - only equipped upgrades appear in game
    - shield no longer bypassed
    - extra life and other buffs implemented

14. User reported major issues:
    - no way to reach inventory from main menu
    - shield still did nothing in gameplay
    - player could move outside borders

15. Assistant patched:
    - runtime `Inventory` button in `MainMenu`
    - stronger collision handling for shield
    - player border clamping against borders/camera bounds

16. User confirmed those fixes worked and then requested:
    - matching border visuals
    - bigger activate buttons
    - toggle behavior that persists until buffs run out

17. Assistant updated the game:
    - border visuals improved
    - buff buttons larger
    - auto-on / auto-off behavior for repeat-consumed buffs

18. User asked about long-term scope and whether internet research was possible.
    Assistant said yes, and later did browse-backed product/launch guidance.

19. Shop work began in earnest.
    User reported shop text/readability issues repeatedly.
    Assistant made many layout passes:
    - full scrollable 10-item shop
    - various failed text layouts
    - scene root fix
    - simplified centered card layout

20. User asked for a roadmap to completion.
    Assistant created `ROADMAP.md`.

21. Assistant implemented best score saving and a post-run summary.
    User then asked what `ROADMAP.md` was and how to access it.

22. Assistant explained `ROADMAP.md` and continued into daily reward work.

23. Assistant added `DailyRewardSystem.cs` and a runtime daily reward panel on the main menu.

24. User repeatedly asked for the daily reward panel to be moved down and the text enlarged.
    Assistant iterated many times until it was “good enough for now.”

25. Assistant then added daily missions.
    The missions panel was initially unreadable and overlapping.

26. User asked whether real Unity objects/TextMeshPro objects could be used instead of script-generated blocks.
    Assistant agreed that scene-owned UI would be better and changed the missions flow so the main menu used a compact summary and a separate pop-out for readable mission detail.

27. User approved the mission summary enough to move on.

28. Assistant added:
    - tutorial overlay
    - pause button
    - pause/settings panel
    - `GameSettings.cs`
    - haptics toggle and first-run tutorial tracking

29. User reported more issues:
    - text too small across the board
    - tutorial listed wrong colors / coins
    - post-run summary overlapped restart
    - coins should look circular

30. Assistant moved more of the `Game` scene UI into scene-owned objects, enlarged HUD text, fixed tutorial wording, moved the summary lower, and made coins look circular and gold.

31. User liked the result and asked to use the same Unity-owned approach in other scenes.

32. Assistant converted the `Game` top HUD to scene-owned objects and laid out:
    - pause button
    - coins
    - score
    - best score
    - level
    - upgrade/challenge text
    - menu button

33. User asked to do the same for `MainMenu`, `Shop`, and `Inventory`, noting:
    - reward claimed panel was hard to read
    - inventory lost the back button
    - UI felt cluttered

34. Assistant moved more persistent UI scaffolding into those scenes and updated the scripts to bind to scene-owned elements where possible.

35. User specifically requested scene-owned setup for inventory and shop.
    Assistant built that direction further:
    - inventory slot cards and back button area improved
    - shop used real card objects under content

36. User reported shop card proportions/title problems, plus warnings and greyed-out objects.
    Assistant explained that greyed-out objects were disabled legacy leftovers, and identified the active scene-owned content under `ShopContent`.

37. User asked if disabled legacy objects should just be deleted.
    Assistant agreed, removed them from `Shop.unity`, and cleaned the scene further.

38. User said the shop looked great after that cleanup and asked what was next.
    Assistant suggested inventory polish and then a real daily challenge.

39. Assistant polished inventory further and confirmed local phone testing would be possible later via Android build/sideload.

40. User pointed out that daily missions and daily rewards already existed, and asked if daily challenge was something different.
    Assistant clarified the distinction:
    - daily reward
    - daily missions
    - separate daily challenge mode

41. Assistant implemented the actual daily challenge system in `DailyChallengeSystem.cs` and wired it into the menu and gameplay.

42. User tested it and reported:
    - small text
    - challenge mode starts correctly and disables buffs
    - reward claiming works
    - `Gold Rush` name did not match the objective
    - it could be replayed indefinitely after completion
    - `Transform child can't be loaded` warning still appeared

43. Assistant fixed:
    - `Gold Rush` to actually behave like a coin challenge
    - challenge HUD/readability
    - challenge reward locking for the day
    - another corrupted scene block contributing to warnings

44. User then said the challenge/menu look was clunky and overlapping, and asked for it to be integrated with Unity objects and cleaned up.

45. Assistant attempted a cleanup/integration pass:
    - made challenge and mission summary cards smaller/lower in `MainMenu.unity`
    - cleaned `Shop.unity`
    - made `ShopManager.cs` respect scene-owned header objects more strongly

46. That cleanup batch was not well received.
    The user said it made the shop worse and later clarified they wanted everything after that request undone.

47. Assistant first only reverted the last header tweak, which was not enough.
    User clarified that the full later batch should be reverted.

48. Assistant then reverted the later visible/layout behavior changes from that batch:
    - restored previous `MainMenu.unity` challenge and mission panel layout
    - restored previous `ShopManager.cs` shop layout behavior
    - kept stale broken legacy objects out of `Shop.unity`

49. User requested a fresh start file with scope, goals, full scripts, and everything done so far.
    Assistant created `FRESH_START_PROTOCOL.md`.

50. User then asked for a better idea:
    - rename the file to `logs`
    - include every change, every message, every bit of data available
    - include scope, goals, timeline, etc.

51. Assistant renamed the file to `logs.md` and expanded it into this fuller project log.

## Current File / Scene Snapshot

### Scenes
- `Assets/Scenes/Game.unity`
- `Assets/Scenes/Inventory.unity`
- `Assets/Scenes/MainMenu.unity`
- `Assets/Scenes/Shop.unity`

### Scripts
- `Assets/Scripts/Coin.cs`
- `Assets/Scripts/DailyChallengeSystem.cs`
- `Assets/Scripts/DailyMissionSystem.cs`
- `Assets/Scripts/DailyRewardSystem.cs`
- `Assets/Scripts/GameManager.cs`
- `Assets/Scripts/GameSettings.cs`
- `Assets/Scripts/GameUI.cs`
- `Assets/Scripts/InventoryMenu.cs`
- `Assets/Scripts/InventorySlotUI.cs`
- `Assets/Scripts/MainMenu.cs`
- `Assets/Scripts/Obstacle.cs`
- `Assets/Scripts/ObstacleZigZag.cs`
- `Assets/Scripts/PlayerController.cs`
- `Assets/Scripts/SceneLoader.cs`
- `Assets/Scripts/ShopManager.cs`
- `Assets/Scripts/Spawner.cs`
- `Assets/Scripts/UpgradeInventory.cs`

## Current Roadmap File
Below is the current `ROADMAP.md` content.
```md
# EndlessDodge1 Roadmap

## Core Goal
Build a polished mobile arcade game with:
- fast restart loops
- readable UI on phones
- satisfying consumable strategy
- strong retention hooks
- organic sharing and competition

## Phase 1: Ship-Ready Core
Status: In progress

### Must finish
- polish the `Game`, `Shop`, and `Inventory` UI for phone readability
- add a simple first-run tutorial overlay
- improve score/coin/loadout HUD readability
- tighten obstacle pacing and early-game fairness
- add pause/settings panel
- add audio and haptics
- save and display best score
- remove scene leftovers and inconsistent legacy UI

### Success check
- a new player understands the game in under 30 seconds
- a full run feels fair, readable, and restart-friendly
- no obvious collision, border, or scene-navigation bugs remain

## Phase 2: Retention Systems
Status: In progress

### Build next
- daily reward
- daily missions
- streak rewards
- post-run summary with score, coins earned, and reward progress
- beginner reward track for the first 7 days
- better economy balancing for shop prices and run rewards

### Reward direction
- use in-game rewards only: coins, shield packs, mixed buff packs
- do not use real-money prizes
- weekly milestone rewards can scale from coins to premium consumable bundles

## Phase 3: Social and Organic Growth
Status: Planned

### Core features
- daily challenge mode with the same seed for everyone
- leaderboard support
- weekly leaderboard reset with in-game rewards
- share result screen with score and challenge result
- friend brag hooks like "beat my run"
- seasonal events with temporary reward tracks

### Why this matters
- daily challenge creates a reason to return
- leaderboards create competition without paid acquisition
- share results create organic discovery loops

## Phase 4: Launch and Store Readiness
Status: Planned

### Store and release
- closed test
- crash and performance pass
- store icon, screenshots, short description, full description
- Google Play Games setup
- analytics setup
- update cadence plan for the first 30 days after launch

### Launch targets
- stable build with no major blocker bugs
- strong first-session clarity
- enough progression to support repeat play before content fatigue

## Recommended Build Order
1. finish UI readability and run polish
2. add best score, tutorial, pause/settings, and audio
3. add daily rewards and daily missions
4. add post-run summary and streak rewards
5. add daily challenge and leaderboard support
6. prepare store listing assets and closed testing

## Immediate Next Slice
This is the next highest-value implementation block:
- HUD readability pass
- beginner reward track
- economy balancing pass
```

## Full Scripts

### Coin.cs
```csharp
using UnityEngine;

public class Coin : MonoBehaviour
{
    public float fallSpeed = 5f;
    public float destroyY = -6f;
    public float magnetMoveSpeed = 14f;

    private static Sprite runtimeCoinSprite;

    void Awake()
    {
        EnsureCircularCoinVisual();
    }

    void Update()
    {
        float worldSpeedMultiplier = 1f;

        if (GameManager.instance != null)
            worldSpeedMultiplier = GameManager.instance.GetWorldSpeedMultiplier();

        Transform magnetTarget = null;

        if (GameManager.instance != null)
            magnetTarget = GameManager.instance.GetCoinMagnetTarget(transform.position);

        if (magnetTarget != null)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                magnetTarget.position,
                magnetMoveSpeed * Time.deltaTime);
        }
        else
        {
            transform.Translate(Vector3.down * fallSpeed * worldSpeedMultiplier * Time.deltaTime);
        }

        if (transform.position.y < destroyY)
        {
            Destroy(gameObject);
        }
    }

    void EnsureCircularCoinVisual()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
            return;

        spriteRenderer.sprite = GetRuntimeCoinSprite();
        spriteRenderer.color = Color.white;
    }

    Sprite GetRuntimeCoinSprite()
    {
        if (runtimeCoinSprite != null)
            return runtimeCoinSprite;

        const int size = 64;
        Texture2D texture = new Texture2D(size, size, TextureFormat.ARGB32, false);
        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;

        Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
        float outerRadius = size * 0.45f;
        float innerRadius = size * 0.34f;
        Vector2 highlightCenter = center + new Vector2(-10f, 10f);
        float highlightRadius = size * 0.14f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);

                if (distance > outerRadius)
                {
                    texture.SetPixel(x, y, Color.clear);
                    continue;
                }

                Color color = Color.Lerp(
                    new Color(0.76f, 0.42f, 0.02f, 1f),
                    new Color(1f, 0.9f, 0.22f, 1f),
                    Mathf.InverseLerp(outerRadius, 0f, distance));

                if (distance > innerRadius)
                {
                    float ringBlend = Mathf.InverseLerp(innerRadius, outerRadius, distance);
                    color = Color.Lerp(color, new Color(0.58f, 0.28f, 0.01f, 1f), ringBlend);
                }

                float highlightDistance = Vector2.Distance(new Vector2(x, y), highlightCenter);

                if (highlightDistance < highlightRadius)
                {
                    float highlightBlend = Mathf.InverseLerp(highlightRadius, 0f, highlightDistance) * 0.55f;
                    color = Color.Lerp(color, new Color(1f, 0.98f, 0.75f, 1f), highlightBlend);
                }

                texture.SetPixel(x, y, color);
            }
        }

        texture.Apply();
        runtimeCoinSprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, size, size),
            new Vector2(0.5f, 0.5f),
            size);
        runtimeCoinSprite.name = "RuntimeCoinSprite";
        return runtimeCoinSprite;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.instance.AddCoin();
            Destroy(gameObject);
        }
    }
}
```

### DailyChallengeSystem.cs
```csharp
using System;
using UnityEngine;

public enum DailyChallengeType
{
    PrecisionRun,
    GoldRush,
    RushHour,
    EnduranceTest
}

public enum DailyChallengeGoalType
{
    ReachScore,
    CollectCoins
}

public struct DailyChallengeData
{
    public DailyChallengeType type;
    public DailyChallengeGoalType goalType;
    public int targetScore;
    public int bestScore;
    public int rewardCoins;
    public UpgradeType rewardUpgrade;
    public int rewardUpgradeAmount;
    public float worldSpeedMultiplier;
    public float coinSpawnMultiplier;
    public bool disableConsumables;
    public bool rewardClaimed;
    public string title;
    public string description;
}

public static class DailyChallengeSystem
{
    private const string ChallengeDateKey = "DailyChallenge_Date";
    private const string ChallengePrefix = "DailyChallenge_";
    private const string ActiveRunDateKey = "DailyChallenge_ActiveDate";
    private const string ActiveRunFlagKey = "DailyChallenge_ActiveFlag";
    private const string TotalCoinsKey = "TotalCoins";

    public static void EnsureInitializedForToday()
    {
        string todayKey = DateTime.Today.ToString("yyyy-MM-dd");

        if (PlayerPrefs.GetString(ChallengeDateKey, string.Empty) == todayKey)
            return;

        CreateTodayChallenge(todayKey);
    }

    public static DailyChallengeData GetTodayChallenge()
    {
        EnsureInitializedForToday();
        DailyChallengeData challenge = LoadChallenge();
        ApplyPresentation(ref challenge);
        return challenge;
    }

    public static void BeginTodayChallengeRun()
    {
        EnsureInitializedForToday();

        DailyChallengeData challenge = LoadChallenge();

        if (challenge.rewardClaimed)
            return;

        PlayerPrefs.SetString(ActiveRunDateKey, DateTime.Today.ToString("yyyy-MM-dd"));
        PlayerPrefs.SetInt(ActiveRunFlagKey, 1);
        PlayerPrefs.Save();
    }

    public static bool IsDailyChallengeRunActive()
    {
        EnsureInitializedForToday();

        string todayKey = DateTime.Today.ToString("yyyy-MM-dd");

        return PlayerPrefs.GetInt(ActiveRunFlagKey, 0) == 1 &&
               PlayerPrefs.GetString(ActiveRunDateKey, string.Empty) == todayKey;
    }

    public static void ClearActiveRun()
    {
        PlayerPrefs.DeleteKey(ActiveRunDateKey);
        PlayerPrefs.SetInt(ActiveRunFlagKey, 0);
        PlayerPrefs.Save();
    }

    public static DailyChallengeData RegisterRunResult(int score, int coinsCollected)
    {
        EnsureInitializedForToday();

        DailyChallengeData challenge = LoadChallenge();
        int progressValue = GetProgressValue(challenge, score, coinsCollected);
        challenge.bestScore = Mathf.Max(challenge.bestScore, progressValue);
        SaveChallenge(challenge);
        ClearActiveRun();
        ApplyPresentation(ref challenge);
        return challenge;
    }

    public static bool HasCompletedToday()
    {
        DailyChallengeData challenge = GetTodayChallenge();
        return challenge.bestScore >= challenge.targetScore;
    }

    public static bool CanClaimReward()
    {
        DailyChallengeData challenge = GetTodayChallenge();
        return challenge.bestScore >= challenge.targetScore && !challenge.rewardClaimed;
    }

    public static string GetObjectiveLabel(DailyChallengeData challenge)
    {
        if (UsesCoinGoal(challenge))
            return "Collect " + challenge.targetScore + " coins";

        return "Reach score " + challenge.targetScore;
    }

    public static bool TryClaimReward(out int coinsGranted, out UpgradeType rewardUpgrade, out int rewardAmount)
    {
        coinsGranted = 0;
        rewardUpgrade = UpgradeType.Shield;
        rewardAmount = 0;

        if (!CanClaimReward())
            return false;

        DailyChallengeData challenge = LoadChallenge();
        challenge.rewardClaimed = true;
        SaveChallenge(challenge);

        coinsGranted = challenge.rewardCoins;
        rewardUpgrade = challenge.rewardUpgrade;
        rewardAmount = challenge.rewardUpgradeAmount;

        int totalCoins = PlayerPrefs.GetInt(TotalCoinsKey, 0);
        totalCoins += coinsGranted;
        PlayerPrefs.SetInt(TotalCoinsKey, totalCoins);

        if (UpgradeInventory.Instance != null && rewardAmount > 0)
            UpgradeInventory.Instance.AddUpgrade(rewardUpgrade, rewardAmount);

        PlayerPrefs.Save();
        return true;
    }

    public static string GetRewardLabel(DailyChallengeData challenge)
    {
        return challenge.rewardCoins + " coins + " +
               challenge.rewardUpgradeAmount + " " +
               UpgradeInventory.GetDisplayName(challenge.rewardUpgrade);
    }

    public static string GetMenuStatusLabel(DailyChallengeData challenge)
    {
        if (challenge.rewardClaimed)
            return "Reward claimed for today";

        if (challenge.bestScore >= challenge.targetScore)
            return "Reward ready";

        if (challenge.bestScore > 0)
            return GetBestProgressLabel(challenge);

        return GetObjectiveLabel(challenge);
    }

    public static string GetRunModifierLabel(DailyChallengeData challenge)
    {
        string speedLabel = challenge.worldSpeedMultiplier > 1f ? "Fast lanes" : "Steady lanes";
        string coinLabel = challenge.coinSpawnMultiplier > 1f ? "more coins" : "normal coins";
        return speedLabel + "   |   " + coinLabel + "   |   no consumables";
    }

    public static string GetCurrentRunLabel(DailyChallengeData challenge, int score, int coinsCollected)
    {
        int progressValue = GetProgressValue(challenge, score, coinsCollected);
        string unitLabel = UsesCoinGoal(challenge) ? "coins" : "score";
        return progressValue + " / " + challenge.targetScore + " " + unitLabel;
    }

    public static string GetBestProgressLabel(DailyChallengeData challenge)
    {
        string unitLabel = UsesCoinGoal(challenge) ? "coins" : "score";
        return "Best " + challenge.bestScore + " / " + challenge.targetScore + " " + unitLabel;
    }

    public static string GetNeedsMoreLabel(DailyChallengeData challenge)
    {
        int neededValue = Mathf.Max(0, challenge.targetScore - challenge.bestScore);
        string unitLabel = UsesCoinGoal(challenge)
            ? (neededValue == 1 ? "coin" : "coins")
            : "score";
        return "Need " + neededValue + " more " + unitLabel;
    }

    private static void CreateTodayChallenge(string todayKey)
    {
        int seed = DateTime.Today.DayOfYear + (DateTime.Today.Year * 37);
        DailyChallengeData challenge = new DailyChallengeData();
        challenge.type = (DailyChallengeType)(seed % 4);
        challenge.goalType = DailyChallengeGoalType.ReachScore;
        challenge.bestScore = 0;
        challenge.disableConsumables = true;
        challenge.rewardClaimed = false;

        int targetOffset = (seed % 5) * 5;

        switch (challenge.type)
        {
            case DailyChallengeType.PrecisionRun:
                challenge.targetScore = 45 + targetOffset;
                challenge.rewardCoins = 140;
                challenge.rewardUpgrade = UpgradeType.Shield;
                challenge.rewardUpgradeAmount = 1;
                challenge.worldSpeedMultiplier = 1.05f;
                challenge.coinSpawnMultiplier = 1.1f;
                break;
            case DailyChallengeType.GoldRush:
                challenge.goalType = DailyChallengeGoalType.CollectCoins;
                challenge.targetScore = 18 + ((seed % 4) * 2);
                challenge.rewardCoins = 165;
                challenge.rewardUpgrade = UpgradeType.CoinMagnet;
                challenge.rewardUpgradeAmount = 1;
                challenge.worldSpeedMultiplier = 1.08f;
                challenge.coinSpawnMultiplier = 1.75f;
                break;
            case DailyChallengeType.RushHour:
                challenge.targetScore = 55 + targetOffset;
                challenge.rewardCoins = 190;
                challenge.rewardUpgrade = UpgradeType.SlowTime;
                challenge.rewardUpgradeAmount = 1;
                challenge.worldSpeedMultiplier = 1.28f;
                challenge.coinSpawnMultiplier = 0.92f;
                break;
            default:
                challenge.targetScore = 50 + targetOffset;
                challenge.rewardCoins = 210;
                challenge.rewardUpgrade = UpgradeType.ExtraLife;
                challenge.rewardUpgradeAmount = 1;
                challenge.worldSpeedMultiplier = 1.18f;
                challenge.coinSpawnMultiplier = 1.2f;
                break;
        }

        PlayerPrefs.SetString(ChallengeDateKey, todayKey);
        SaveChallenge(challenge);
        ClearActiveRun();
        PlayerPrefs.Save();
    }

    private static DailyChallengeData LoadChallenge()
    {
        DailyChallengeData challenge = new DailyChallengeData();
        challenge.type = (DailyChallengeType)PlayerPrefs.GetInt(GetKey("Type"), 0);
        challenge.goalType = (DailyChallengeGoalType)PlayerPrefs.GetInt(GetKey("GoalType"), 0);
        challenge.targetScore = PlayerPrefs.GetInt(GetKey("TargetScore"), 40);
        challenge.bestScore = PlayerPrefs.GetInt(GetKey("BestScore"), 0);
        challenge.rewardCoins = PlayerPrefs.GetInt(GetKey("RewardCoins"), 100);
        challenge.rewardUpgrade = (UpgradeType)PlayerPrefs.GetInt(GetKey("RewardUpgrade"), 0);
        challenge.rewardUpgradeAmount = PlayerPrefs.GetInt(GetKey("RewardUpgradeAmount"), 1);
        challenge.worldSpeedMultiplier = PlayerPrefs.GetFloat(GetKey("WorldSpeedMultiplier"), 1f);
        challenge.coinSpawnMultiplier = PlayerPrefs.GetFloat(GetKey("CoinSpawnMultiplier"), 1f);
        challenge.disableConsumables = PlayerPrefs.GetInt(GetKey("DisableConsumables"), 1) == 1;
        challenge.rewardClaimed = PlayerPrefs.GetInt(GetKey("RewardClaimed"), 0) == 1;
        NormalizeLegacyChallenge(ref challenge);
        return challenge;
    }

    private static void SaveChallenge(DailyChallengeData challenge)
    {
        PlayerPrefs.SetInt(GetKey("Type"), (int)challenge.type);
        PlayerPrefs.SetInt(GetKey("GoalType"), (int)challenge.goalType);
        PlayerPrefs.SetInt(GetKey("TargetScore"), challenge.targetScore);
        PlayerPrefs.SetInt(GetKey("BestScore"), challenge.bestScore);
        PlayerPrefs.SetInt(GetKey("RewardCoins"), challenge.rewardCoins);
        PlayerPrefs.SetInt(GetKey("RewardUpgrade"), (int)challenge.rewardUpgrade);
        PlayerPrefs.SetInt(GetKey("RewardUpgradeAmount"), challenge.rewardUpgradeAmount);
        PlayerPrefs.SetFloat(GetKey("WorldSpeedMultiplier"), challenge.worldSpeedMultiplier);
        PlayerPrefs.SetFloat(GetKey("CoinSpawnMultiplier"), challenge.coinSpawnMultiplier);
        PlayerPrefs.SetInt(GetKey("DisableConsumables"), challenge.disableConsumables ? 1 : 0);
        PlayerPrefs.SetInt(GetKey("RewardClaimed"), challenge.rewardClaimed ? 1 : 0);
    }

    private static string GetKey(string suffix)
    {
        return ChallengePrefix + suffix;
    }

    private static bool UsesCoinGoal(DailyChallengeData challenge)
    {
        return challenge.goalType == DailyChallengeGoalType.CollectCoins;
    }

    private static int GetProgressValue(DailyChallengeData challenge, int score, int coinsCollected)
    {
        return UsesCoinGoal(challenge) ? coinsCollected : score;
    }

    private static void NormalizeLegacyChallenge(ref DailyChallengeData challenge)
    {
        if (challenge.type == DailyChallengeType.GoldRush)
        {
            challenge.goalType = DailyChallengeGoalType.CollectCoins;

            int seed = DateTime.Today.DayOfYear + (DateTime.Today.Year * 37);
            int expectedTarget = 18 + ((seed % 4) * 2);

            challenge.targetScore = expectedTarget;
            challenge.bestScore = Mathf.Clamp(challenge.bestScore, 0, expectedTarget);
            challenge.rewardCoins = Mathf.Max(challenge.rewardCoins, 165);
            challenge.rewardUpgrade = UpgradeType.CoinMagnet;
            challenge.rewardUpgradeAmount = Mathf.Max(challenge.rewardUpgradeAmount, 1);
            challenge.worldSpeedMultiplier = Mathf.Max(challenge.worldSpeedMultiplier, 1.08f);
            challenge.coinSpawnMultiplier = Mathf.Max(challenge.coinSpawnMultiplier, 1.75f);
            return;
        }

        challenge.goalType = DailyChallengeGoalType.ReachScore;
    }

    private static void ApplyPresentation(ref DailyChallengeData challenge)
    {
        switch (challenge.type)
        {
            case DailyChallengeType.PrecisionRun:
                challenge.title = "Precision Run";
                challenge.description = "Reach the target score with no consumables.";
                break;
            case DailyChallengeType.GoldRush:
                challenge.title = "Gold Rush";
                challenge.description = "Collect coins under pressure with no consumables.";
                break;
            case DailyChallengeType.RushHour:
                challenge.title = "Rush Hour";
                challenge.description = "Reach the target score in faster lanes.";
                break;
            default:
                challenge.title = "Endurance Test";
                challenge.description = "Survive long enough to hit the target score.";
                break;
        }
    }
}
```

### DailyMissionSystem.cs
```csharp
using System;
using System.Collections.Generic;
using UnityEngine;

public enum DailyMissionType
{
    CollectCoins,
    ReachScore,
    PlayRuns
}

public struct DailyMissionData
{
    public DailyMissionType type;
    public int target;
    public int progress;
    public int rewardCoins;
    public UpgradeType rewardUpgrade;
    public int rewardUpgradeAmount;
    public bool claimed;
}

public static class DailyMissionSystem
{
    private const int MissionCount = 3;
    private const string MissionDateKey = "DailyMission_Date";
    private const string TotalCoinsKey = "TotalCoins";
    private const string MissionPrefix = "DailyMission_";

    public static void EnsureInitializedForToday()
    {
        string todayKey = DateTime.Today.ToString("yyyy-MM-dd");

        if (PlayerPrefs.GetString(MissionDateKey, string.Empty) == todayKey)
            return;

        CreateTodayMissions(todayKey);
    }

    public static DailyMissionData[] GetMissions()
    {
        EnsureInitializedForToday();

        DailyMissionData[] missions = new DailyMissionData[MissionCount];

        for (int i = 0; i < MissionCount; i++)
            missions[i] = LoadMission(i);

        return missions;
    }

    public static void RegisterCoinsCollected(int amount)
    {
        EnsureInitializedForToday();
        UpdateMissionProgress(DailyMissionType.CollectCoins, amount, false);
    }

    public static void RegisterRunFinished(int score, int levelReached)
    {
        EnsureInitializedForToday();
        UpdateMissionProgress(DailyMissionType.PlayRuns, 1, false);
        UpdateMissionProgress(DailyMissionType.ReachScore, score, true);
    }

    public static int GetClaimableCount()
    {
        DailyMissionData[] missions = GetMissions();
        int count = 0;

        for (int i = 0; i < missions.Length; i++)
        {
            if (missions[i].progress >= missions[i].target && !missions[i].claimed)
                count += 1;
        }

        return count;
    }

    public static int GetCompletedCount()
    {
        DailyMissionData[] missions = GetMissions();
        int count = 0;

        for (int i = 0; i < missions.Length; i++)
        {
            if (missions[i].progress >= missions[i].target)
                count += 1;
        }

        return count;
    }

    public static bool ClaimCompletedRewards(out int coinsGranted, out List<string> upgradesGranted)
    {
        EnsureInitializedForToday();

        coinsGranted = 0;
        upgradesGranted = new List<string>();
        bool claimedAny = false;

        for (int i = 0; i < MissionCount; i++)
        {
            DailyMissionData mission = LoadMission(i);

            if (mission.claimed || mission.progress < mission.target)
                continue;

            mission.claimed = true;
            SaveMission(i, mission);

            coinsGranted += mission.rewardCoins;

            if (mission.rewardUpgradeAmount > 0 && UpgradeInventory.Instance != null)
            {
                UpgradeInventory.Instance.AddUpgrade(mission.rewardUpgrade, mission.rewardUpgradeAmount);
                upgradesGranted.Add(mission.rewardUpgradeAmount + " " + UpgradeInventory.GetDisplayName(mission.rewardUpgrade));
            }

            claimedAny = true;
        }

        if (claimedAny)
        {
            int totalCoins = PlayerPrefs.GetInt(TotalCoinsKey, 0);
            totalCoins += coinsGranted;
            PlayerPrefs.SetInt(TotalCoinsKey, totalCoins);
            PlayerPrefs.Save();
        }

        return claimedAny;
    }

    public static string GetMissionSummary(DailyMissionData mission)
    {
        switch (mission.type)
        {
            case DailyMissionType.CollectCoins:
                return "Collect " + mission.target + " coins";
            case DailyMissionType.ReachScore:
                return "Reach score " + mission.target;
            default:
                return "Play " + mission.target + " runs";
        }
    }

    public static string GetMissionProgressLabel(DailyMissionData mission)
    {
        switch (mission.type)
        {
            case DailyMissionType.CollectCoins:
                return "Coins " + mission.progress + "/" + mission.target;
            case DailyMissionType.ReachScore:
                return "Score " + mission.progress + "/" + mission.target;
            default:
                return "Runs " + mission.progress + "/" + mission.target;
        }
    }

    public static string GetMissionRewardLabel(DailyMissionData mission)
    {
        return mission.rewardCoins + "c + " + GetShortUpgradeName(mission.rewardUpgrade);
    }

    public static string GetCompactMissionLabel(DailyMissionData mission)
    {
        switch (mission.type)
        {
            case DailyMissionType.CollectCoins:
                return "Collect " + mission.target;
            case DailyMissionType.ReachScore:
                return "Score " + mission.target;
            default:
                return "Play " + mission.target + " runs";
        }
    }

    static void CreateTodayMissions(string todayKey)
    {
        int seed = DateTime.Today.DayOfYear + (DateTime.Today.Year * 17);
        PlayerPrefs.SetString(MissionDateKey, todayKey);

        DailyMissionData collectCoins = new DailyMissionData();
        collectCoins.type = DailyMissionType.CollectCoins;
        collectCoins.target = 20 + ((seed % 3) * 10);
        collectCoins.progress = 0;
        collectCoins.rewardCoins = 80;
        collectCoins.rewardUpgrade = UpgradeType.CoinMagnet;
        collectCoins.rewardUpgradeAmount = 1;
        collectCoins.claimed = false;

        DailyMissionData reachScore = new DailyMissionData();
        reachScore.type = DailyMissionType.ReachScore;
        reachScore.target = 40 + ((seed % 4) * 10);
        reachScore.progress = 0;
        reachScore.rewardCoins = 100;
        reachScore.rewardUpgrade = UpgradeType.Shield;
        reachScore.rewardUpgradeAmount = 1;
        reachScore.claimed = false;

        DailyMissionData playRuns = new DailyMissionData();
        playRuns.type = DailyMissionType.PlayRuns;
        playRuns.target = 3 + (seed % 2);
        playRuns.progress = 0;
        playRuns.rewardCoins = 120;
        playRuns.rewardUpgrade = UpgradeType.SpeedBoost;
        playRuns.rewardUpgradeAmount = 1;
        playRuns.claimed = false;

        SaveMission(0, collectCoins);
        SaveMission(1, reachScore);
        SaveMission(2, playRuns);
        PlayerPrefs.Save();
    }

    static void UpdateMissionProgress(DailyMissionType missionType, int amount, bool keepHighestValue)
    {
        for (int i = 0; i < MissionCount; i++)
        {
            DailyMissionData mission = LoadMission(i);

            if (mission.type != missionType)
                continue;

            if (keepHighestValue)
                mission.progress = Mathf.Max(mission.progress, amount);
            else
                mission.progress += amount;

            mission.progress = Mathf.Min(mission.progress, mission.target);
            SaveMission(i, mission);
            PlayerPrefs.Save();
            return;
        }
    }

    static DailyMissionData LoadMission(int index)
    {
        DailyMissionData mission = new DailyMissionData();
        mission.type = (DailyMissionType)PlayerPrefs.GetInt(GetKey(index, "Type"), 0);
        mission.target = PlayerPrefs.GetInt(GetKey(index, "Target"), 1);
        mission.progress = PlayerPrefs.GetInt(GetKey(index, "Progress"), 0);
        mission.rewardCoins = PlayerPrefs.GetInt(GetKey(index, "RewardCoins"), 0);
        mission.rewardUpgrade = (UpgradeType)PlayerPrefs.GetInt(GetKey(index, "RewardUpgrade"), 0);
        mission.rewardUpgradeAmount = PlayerPrefs.GetInt(GetKey(index, "RewardUpgradeAmount"), 0);
        mission.claimed = PlayerPrefs.GetInt(GetKey(index, "Claimed"), 0) == 1;
        return mission;
    }

    static void SaveMission(int index, DailyMissionData mission)
    {
        PlayerPrefs.SetInt(GetKey(index, "Type"), (int)mission.type);
        PlayerPrefs.SetInt(GetKey(index, "Target"), mission.target);
        PlayerPrefs.SetInt(GetKey(index, "Progress"), mission.progress);
        PlayerPrefs.SetInt(GetKey(index, "RewardCoins"), mission.rewardCoins);
        PlayerPrefs.SetInt(GetKey(index, "RewardUpgrade"), (int)mission.rewardUpgrade);
        PlayerPrefs.SetInt(GetKey(index, "RewardUpgradeAmount"), mission.rewardUpgradeAmount);
        PlayerPrefs.SetInt(GetKey(index, "Claimed"), mission.claimed ? 1 : 0);
    }

    static string GetKey(int index, string suffix)
    {
        return MissionPrefix + index + "_" + suffix;
    }

    static string GetShortUpgradeName(UpgradeType type)
    {
        switch (type)
        {
            case UpgradeType.SpeedBoost:
                return "Speed";
            case UpgradeType.CoinMagnet:
                return "Magnet";
            case UpgradeType.DoubleCoins:
                return "Double";
            case UpgradeType.SlowTime:
                return "Slow";
            case UpgradeType.SmallerPlayer:
                return "Small";
            case UpgradeType.ScoreBooster:
                return "Score";
            case UpgradeType.RareCoinBoost:
                return "Rare Coin";
            default:
                return UpgradeInventory.GetDisplayName(type);
        }
    }
}
```

### DailyRewardSystem.cs
```csharp
using System;
using UnityEngine;

public struct DailyRewardPackage
{
    public int rewardDay;
    public int coins;
    public UpgradeType bonusUpgrade;
    public int bonusAmount;
}

public static class DailyRewardSystem
{
    private const string LastClaimDateKey = "DailyReward_LastClaimDate";
    private const string StreakKey = "DailyReward_Streak";
    private const string TotalCoinsKey = "TotalCoins";

    public static bool CanClaimToday()
    {
        if (!TryGetLastClaimDate(out DateTime lastClaimDate))
            return true;

        return (DateTime.Today - lastClaimDate.Date).Days >= 1;
    }

    public static int GetCurrentStreak()
    {
        return PlayerPrefs.GetInt(StreakKey, 0);
    }

    public static DailyRewardPackage GetPreviewReward()
    {
        int nextStreak = GetNextStreakValue();
        return GetRewardForStreak(nextStreak);
    }

    public static string GetNextClaimCountdownText()
    {
        DateTime now = DateTime.Now;
        DateTime nextClaimTime = now.Date.AddDays(1);
        TimeSpan remaining = nextClaimTime - now;

        if (remaining.TotalMinutes < 1)
            return "Ready soon";

        int hours = Mathf.Max(0, (int)remaining.TotalHours);
        int minutes = Mathf.Max(0, remaining.Minutes);

        if (hours > 0)
            return hours + "h " + minutes + "m";

        return minutes + "m";
    }

    public static bool TryClaimReward(out DailyRewardPackage rewardPackage)
    {
        rewardPackage = default;

        if (!CanClaimToday())
            return false;

        int nextStreak = GetNextStreakValue();
        rewardPackage = GetRewardForStreak(nextStreak);

        int totalCoins = PlayerPrefs.GetInt(TotalCoinsKey, 0);
        totalCoins += rewardPackage.coins;
        PlayerPrefs.SetInt(TotalCoinsKey, totalCoins);
        PlayerPrefs.SetString(LastClaimDateKey, DateTime.Today.ToString("yyyy-MM-dd"));
        PlayerPrefs.SetInt(StreakKey, nextStreak);

        if (UpgradeInventory.Instance != null && rewardPackage.bonusAmount > 0)
            UpgradeInventory.Instance.AddUpgrade(rewardPackage.bonusUpgrade, rewardPackage.bonusAmount);

        PlayerPrefs.Save();
        return true;
    }

    static int GetNextStreakValue()
    {
        int currentStreak = GetCurrentStreak();

        if (!TryGetLastClaimDate(out DateTime lastClaimDate))
            return 1;

        int daysSinceLastClaim = (DateTime.Today - lastClaimDate.Date).Days;

        if (daysSinceLastClaim <= 0)
            return Mathf.Max(1, currentStreak);

        if (daysSinceLastClaim == 1)
            return currentStreak + 1;

        return 1;
    }

    static DailyRewardPackage GetRewardForStreak(int streakValue)
    {
        int rewardDay = ((Mathf.Max(streakValue, 1) - 1) % 7) + 1;

        switch (rewardDay)
        {
            case 1:
                return CreateReward(rewardDay, 60, UpgradeType.Shield, 1);
            case 2:
                return CreateReward(rewardDay, 75, UpgradeType.SpeedBoost, 1);
            case 3:
                return CreateReward(rewardDay, 90, UpgradeType.CoinMagnet, 1);
            case 4:
                return CreateReward(rewardDay, 110, UpgradeType.DoubleCoins, 1);
            case 5:
                return CreateReward(rewardDay, 130, UpgradeType.ExtraLife, 1);
            case 6:
                return CreateReward(rewardDay, 155, UpgradeType.SlowTime, 1);
            default:
                return CreateReward(rewardDay, 220, UpgradeType.Bomb, 1);
        }
    }

    static DailyRewardPackage CreateReward(int rewardDay, int coins, UpgradeType bonusUpgrade, int bonusAmount)
    {
        DailyRewardPackage rewardPackage = new DailyRewardPackage();
        rewardPackage.rewardDay = rewardDay;
        rewardPackage.coins = coins;
        rewardPackage.bonusUpgrade = bonusUpgrade;
        rewardPackage.bonusAmount = bonusAmount;
        return rewardPackage;
    }

    static bool TryGetLastClaimDate(out DateTime lastClaimDate)
    {
        string savedDate = PlayerPrefs.GetString(LastClaimDateKey, string.Empty);

        if (!string.IsNullOrEmpty(savedDate) && DateTime.TryParse(savedDate, out lastClaimDate))
            return true;

        lastClaimDate = default;
        return false;
    }
}
```

### GameManager.cs
```csharp
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI totalCoinsText;
    public TextMeshProUGUI upgradeText;
    public TextMeshProUGUI levelText;

    public GameObject gameOverText;
    public GameObject restartButton;
    public Spawner spawner;
    public PlayerController player;

    private float score = 0f;
    private int totalCoins = 0;
    private int runCoinsEarned = 0;
    private int bestScore = 0;
    private bool gameEnded = false;
    private bool newBestScore = false;

    private float runTime = 0f;
    public float difficultyStepTime = 15f;

    private int currentLevel = 1;
    public int activeShields = 0;
    public int armedExtraLives = 0;
    private bool isDailyChallengeRun = false;
    private DailyChallengeData activeDailyChallenge;

    private readonly List<UpgradeType> equippedUpgrades = new List<UpgradeType>();
    private readonly Dictionary<UpgradeType, float> activeUpgradeEndTimes = new Dictionary<UpgradeType, float>();
    private readonly Dictionary<UpgradeType, RunUpgradeButtonUI> runUpgradeButtons = new Dictionary<UpgradeType, RunUpgradeButtonUI>();
    private readonly HashSet<UpgradeType> autoEnabledUpgrades = new HashSet<UpgradeType>();

    private RectTransform runUpgradePanel;
    private TMP_FontAsset runtimeFont;
    private GameObject postRunPanel;
    private TextMeshProUGUI postRunSummaryText;
    private TextMeshProUGUI bestScoreHudText;
    private Button pauseButton;
    private TextMeshProUGUI pauseButtonText;
    private GameObject pauseOverlayRoot;
    private TextMeshProUGUI pauseOverlayTitleText;
    private TextMeshProUGUI hapticsStatusText;
    private Button hapticsToggleButton;
    private TextMeshProUGUI hapticsToggleButtonText;
    private GameObject tutorialOverlayRoot;
    private TextMeshProUGUI tutorialBodyText;
    private Button tutorialPrimaryButton;
    private TextMeshProUGUI tutorialPrimaryButtonText;
    private bool markTutorialSeenOnClose;

    private const float RunUpgradeButtonWidth = 230f;
    private const float RunUpgradeButtonHeight = 78f;
    private const float ShieldProtectionDuration = 0.35f;
    private const float ExtraLifeReviveDelay = 0.2f;
    private const float ExtraLifeInvulnerabilityDuration = 1.4f;
    private const float SpeedBoostDuration = 10f;
    private const float CoinMagnetDuration = 12f;
    private const float DoubleCoinsDuration = 12f;
    private const float SlowTimeDuration = 10f;
    private const float SmallerPlayerDuration = 12f;
    private const float ScoreBoosterDuration = 12f;
    private const float RareCoinBoostDuration = 15f;
    private const string BestScoreKey = "BestScore";

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        DailyChallengeSystem.EnsureInitializedForToday();
        DailyMissionSystem.EnsureInitializedForToday();
        isDailyChallengeRun = DailyChallengeSystem.IsDailyChallengeRunActive();

        if (isDailyChallengeRun)
            activeDailyChallenge = DailyChallengeSystem.GetTodayChallenge();

        if (gameOverText != null)
            gameOverText.SetActive(false);

        if (restartButton != null)
            restartButton.SetActive(false);

        totalCoins = PlayerPrefs.GetInt("TotalCoins", 0);
        bestScore = PlayerPrefs.GetInt(BestScoreKey, 0);

        if (totalCoinsText != null)
            totalCoinsText.text = "Coins: " + totalCoins;

        if (isDailyChallengeRun)
            UpdateUpgradeText();

        runtimeFont = GetRuntimeFont();
        EnsureRuntimeFeedbackUI();
        RefreshPauseOverlayState();
        UpdateBestScoreHud();
        SyncLoadoutFromInventory(true);
        ApplyContinuousEffects();
        UpdateUpgradeText();
        ShowTutorialIfNeeded();

        currentLevel = GetDifficultyLevel();
        UpdateLevelText();
    }

    void Update()
    {
        SyncLoadoutFromInventory(false);

        if (gameEnded) return;

        ProcessAutoUpgrades();
        runTime += Time.deltaTime;
        score += Time.deltaTime * GetScoreMultiplier();

        if (scoreText != null)
            scoreText.text = "Score: " + Mathf.FloorToInt(score);

        if (isDailyChallengeRun)
        {
            UpdateUpgradeText();
            UpdateBestScoreHud();
        }

        ApplyContinuousEffects();
        RefreshRunUpgradeButtons();
        int newLevel = GetDifficultyLevel();

        if (newLevel != currentLevel)
        {
            currentLevel = newLevel;
            UpdateLevelText();
            StartCoroutine(LevelFlash());
        }
    }

    void UpdateUpgradeText()
    {
        if (upgradeText == null)
            return;

        if (isDailyChallengeRun)
        {
            upgradeText.text =
                activeDailyChallenge.title +
                "\n" + DailyChallengeSystem.GetObjectiveLabel(activeDailyChallenge);
            return;
        }

        if (equippedUpgrades.Count == 0)
        {
            upgradeText.text = "No loadout selected";
            return;
        }

        List<string> lines = new List<string>();

        for (int i = 0; i < equippedUpgrades.Count; i++)
        {
            lines.Add(BuildUpgradeSummary(equippedUpgrades[i]));
        }

        upgradeText.text = string.Join("\n", lines.ToArray());
    }

    void UpdateLevelText()
    {
        if (levelText != null)
            levelText.text = "Level " + currentLevel;
    }

    IEnumerator LevelFlash()
    {
        if (levelText == null)
            yield break;

        Color originalColor = levelText.color;
        levelText.color = Color.yellow;
        yield return new WaitForSeconds(0.3f);
        levelText.color = originalColor;
    }

    public int GetDifficultyLevel()
    {
        return Mathf.FloorToInt(runTime / difficultyStepTime) + 1;
    }

    public void AddCoin()
    {
        int coinValue = GetCoinValue();
        totalCoins += coinValue;
        runCoinsEarned += coinValue;
        DailyMissionSystem.RegisterCoinsCollected(coinValue);
        PlayerPrefs.SetInt("TotalCoins", totalCoins);
        PlayerPrefs.Save();

        if (totalCoinsText != null)
            totalCoinsText.text = "Coins: " + totalCoins;

        if (isDailyChallengeRun)
        {
            UpdateUpgradeText();
            UpdateBestScoreHud();
        }
    }

    public void ActivateShield()
    {
        if (UpgradeInventory.Instance == null)
            return;

        bool used = UpgradeInventory.Instance.UseUpgrade(UpgradeType.Shield, 1);

        if (used)
        {
            activeShields += 1;
            RefreshRunUpgradeButtons();
            UpdateUpgradeText();
        }
    }

    public bool ConsumeShieldIfAvailable()
    {
        if (activeShields > 0)
        {
            activeShields -= 1;
            RefreshRunUpgradeButtons();
            UpdateUpgradeText();
            return true;
        }

        return false;
    }

    void SyncLoadoutFromInventory(bool forceRebuild)
    {
        if (isDailyChallengeRun)
        {
            bool shouldClear = forceRebuild || equippedUpgrades.Count > 0 || runUpgradeButtons.Count > 0;

            if (shouldClear)
            {
                equippedUpgrades.Clear();
                autoEnabledUpgrades.Clear();
                activeUpgradeEndTimes.Clear();
                activeShields = 0;
                armedExtraLives = 0;
                RebuildRunUpgradeButtons();
                UpdateUpgradeText();
            }

            return;
        }

        if (UpgradeInventory.Instance == null)
            return;

        List<UpgradeType> savedUpgrades = UpgradeInventory.Instance.GetEquippedUpgrades();
        bool loadoutChanged = forceRebuild || HaveEquippedUpgradesChanged(savedUpgrades);

        if (!loadoutChanged)
            return;

        equippedUpgrades.Clear();

        for (int i = 0; i < savedUpgrades.Count; i++)
        {
            equippedUpgrades.Add(savedUpgrades[i]);
        }

        RemoveUnavailableAutoUpgrades();
        RebuildRunUpgradeButtons();
        UpdateUpgradeText();
    }

    bool HaveEquippedUpgradesChanged(List<UpgradeType> savedUpgrades)
    {
        if (savedUpgrades.Count != equippedUpgrades.Count)
            return true;

        for (int i = 0; i < savedUpgrades.Count; i++)
        {
            if (savedUpgrades[i] != equippedUpgrades[i])
                return true;
        }

        return false;
    }

    void RebuildRunUpgradeButtons()
    {
        if (runUpgradePanel != null)
            Destroy(runUpgradePanel.gameObject);

        runUpgradePanel = null;
        runUpgradeButtons.Clear();
        BuildRunUpgradeButtons();
    }

    void RemoveUnavailableAutoUpgrades()
    {
        List<UpgradeType> autoTypes = new List<UpgradeType>(autoEnabledUpgrades);

        for (int i = 0; i < autoTypes.Count; i++)
        {
            UpgradeType type = autoTypes[i];

            if (!equippedUpgrades.Contains(type) || !IsAutoToggleUpgrade(type))
                autoEnabledUpgrades.Remove(type);
        }
    }

    void ProcessAutoUpgrades()
    {
        if (UpgradeInventory.Instance == null)
            return;

        for (int i = 0; i < equippedUpgrades.Count; i++)
        {
            UpgradeType type = equippedUpgrades[i];

            if (!autoEnabledUpgrades.Contains(type) || !IsAutoToggleUpgrade(type))
                continue;

            switch (type)
            {
                case UpgradeType.Shield:
                    SustainShield();
                    break;
                case UpgradeType.ExtraLife:
                    SustainExtraLife();
                    break;
                case UpgradeType.SpeedBoost:
                    SustainTimedUpgrade(type, SpeedBoostDuration);
                    break;
                case UpgradeType.CoinMagnet:
                    SustainTimedUpgrade(type, CoinMagnetDuration);
                    break;
                case UpgradeType.DoubleCoins:
                    SustainTimedUpgrade(type, DoubleCoinsDuration);
                    break;
                case UpgradeType.SlowTime:
                    SustainTimedUpgrade(type, SlowTimeDuration);
                    break;
                case UpgradeType.SmallerPlayer:
                    SustainTimedUpgrade(type, SmallerPlayerDuration);
                    break;
                case UpgradeType.ScoreBooster:
                    SustainTimedUpgrade(type, ScoreBoosterDuration);
                    break;
                case UpgradeType.RareCoinBoost:
                    SustainTimedUpgrade(type, RareCoinBoostDuration);
                    break;
            }
        }
    }

    void SustainShield()
    {
        if (activeShields > 0)
            return;

        if (GetUpgradeOwnedCount(UpgradeType.Shield) > 0)
            ActivateShield();
        else
            autoEnabledUpgrades.Remove(UpgradeType.Shield);
    }

    void SustainExtraLife()
    {
        if (armedExtraLives > 0)
            return;

        if (GetUpgradeOwnedCount(UpgradeType.ExtraLife) > 0)
            ArmExtraLife();
        else
            autoEnabledUpgrades.Remove(UpgradeType.ExtraLife);
    }

    void SustainTimedUpgrade(UpgradeType type, float duration)
    {
        if (IsUpgradeActive(type))
            return;

        if (GetUpgradeOwnedCount(type) > 0)
            ActivateTimedUpgrade(type, duration);
        else
            autoEnabledUpgrades.Remove(type);
    }

    public void HandlePlayerHit(GameObject obstacle)
    {
        if (gameEnded)
            return;

        if (player != null && player.IsInvulnerable())
        {
            if (obstacle != null)
                Destroy(obstacle);

            return;
        }

        if (ConsumeShieldIfAvailable())
        {
            if (obstacle != null)
                Destroy(obstacle);

            if (player != null)
                player.TriggerInvulnerability(ShieldProtectionDuration);

            GameSettings.TriggerHaptic();

            ProcessAutoUpgrades();
            return;
        }

        if (armedExtraLives > 0)
        {
            armedExtraLives -= 1;

            if (obstacle != null)
                Destroy(obstacle);

            GameSettings.TriggerHaptic();
            StartCoroutine(RevivePlayerRoutine());
            ProcessAutoUpgrades();
            RefreshRunUpgradeButtons();
            UpdateUpgradeText();
            return;
        }

        GameOver();
    }

    IEnumerator RevivePlayerRoutine()
    {
        if (player != null)
            player.Die();

        yield return new WaitForSeconds(ExtraLifeReviveDelay);

        if (player != null)
            player.Revive(ExtraLifeInvulnerabilityDuration);
    }

    public void UseRunUpgrade(UpgradeType type)
    {
        if (gameEnded || !equippedUpgrades.Contains(type) || UpgradeInventory.Instance == null)
            return;

        if (IsAutoToggleUpgrade(type))
        {
            ToggleAutoUpgrade(type);
            ProcessAutoUpgrades();
        }
        else if (type == UpgradeType.Bomb)
        {
            ActivateBomb();
        }

        RefreshRunUpgradeButtons();
        UpdateUpgradeText();
    }

    void ToggleAutoUpgrade(UpgradeType type)
    {
        if (autoEnabledUpgrades.Contains(type))
        {
            autoEnabledUpgrades.Remove(type);
            return;
        }

        if (GetUpgradeOwnedCount(type) > 0 || HasBufferedUpgradeState(type))
            autoEnabledUpgrades.Add(type);
    }

    bool IsAutoToggleUpgrade(UpgradeType type)
    {
        return type != UpgradeType.Bomb;
    }

    bool IsUpgradeAutoEnabled(UpgradeType type)
    {
        return autoEnabledUpgrades.Contains(type);
    }

    bool HasBufferedUpgradeState(UpgradeType type)
    {
        switch (type)
        {
            case UpgradeType.Shield:
                return activeShields > 0;
            case UpgradeType.ExtraLife:
                return armedExtraLives > 0;
            case UpgradeType.Bomb:
                return false;
            default:
                return IsUpgradeActive(type);
        }
    }

    void ArmExtraLife()
    {
        if (UpgradeInventory.Instance.UseUpgrade(UpgradeType.ExtraLife, 1))
        {
            armedExtraLives += 1;
        }
    }

    void ActivateBomb()
    {
        if (!UpgradeInventory.Instance.UseUpgrade(UpgradeType.Bomb, 1))
            return;

        GameObject[] obstacles = GameObject.FindGameObjectsWithTag("Obstacle");

        for (int i = 0; i < obstacles.Length; i++)
        {
            Destroy(obstacles[i]);
        }
    }

    void ActivateTimedUpgrade(UpgradeType type, float duration)
    {
        if (!UpgradeInventory.Instance.UseUpgrade(type, 1))
            return;

        float startTime = Time.time;

        if (IsUpgradeActive(type))
            startTime = activeUpgradeEndTimes[type];

        activeUpgradeEndTimes[type] = startTime + duration;
    }

    void LoadEquippedUpgrades()
    {
        equippedUpgrades.Clear();

        if (UpgradeInventory.Instance == null)
            return;

        List<UpgradeType> savedUpgrades = UpgradeInventory.Instance.GetEquippedUpgrades();

        for (int i = 0; i < savedUpgrades.Count; i++)
        {
            equippedUpgrades.Add(savedUpgrades[i]);
        }
    }

    void BuildRunUpgradeButtons()
    {
        if (equippedUpgrades.Count == 0)
            return;

        RectTransform parentRect = GetRuntimeUIParent();

        if (parentRect == null)
            return;

        GameObject panelObject = new GameObject(
            "RunUpgradePanel",
            typeof(RectTransform),
            typeof(VerticalLayoutGroup),
            typeof(ContentSizeFitter));

        panelObject.transform.SetParent(parentRect, false);
        runUpgradePanel = panelObject.GetComponent<RectTransform>();
        runUpgradePanel.anchorMin = new Vector2(1f, 0f);
        runUpgradePanel.anchorMax = new Vector2(1f, 0f);
        runUpgradePanel.pivot = new Vector2(1f, 0f);
        runUpgradePanel.anchoredPosition = new Vector2(-18f, 26f);
        runUpgradePanel.sizeDelta = new Vector2(RunUpgradeButtonWidth, 0f);

        VerticalLayoutGroup layoutGroup = panelObject.GetComponent<VerticalLayoutGroup>();
        layoutGroup.spacing = 10f;
        layoutGroup.childAlignment = TextAnchor.LowerRight;
        layoutGroup.childControlHeight = false;
        layoutGroup.childControlWidth = true;
        layoutGroup.childForceExpandHeight = false;
        layoutGroup.childForceExpandWidth = true;

        ContentSizeFitter fitter = panelObject.GetComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        for (int i = 0; i < equippedUpgrades.Count; i++)
        {
            UpgradeType type = equippedUpgrades[i];
            RunUpgradeButtonUI buttonUI = CreateRunUpgradeButton(type);

            if (buttonUI != null)
                runUpgradeButtons[type] = buttonUI;
        }
    }

    RunUpgradeButtonUI CreateRunUpgradeButton(UpgradeType type)
    {
        if (runUpgradePanel == null)
            return null;

        GameObject buttonObject = new GameObject(
            UpgradeInventory.GetDisplayName(type) + "Button",
            typeof(RectTransform),
            typeof(Image),
            typeof(Button),
            typeof(LayoutElement),
            typeof(RunUpgradeButtonUI));

        buttonObject.transform.SetParent(runUpgradePanel, false);

        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        buttonRect.sizeDelta = new Vector2(RunUpgradeButtonWidth, RunUpgradeButtonHeight);

        LayoutElement layoutElement = buttonObject.GetComponent<LayoutElement>();
        layoutElement.preferredHeight = RunUpgradeButtonHeight;
        layoutElement.preferredWidth = RunUpgradeButtonWidth;

        Image buttonImage = buttonObject.GetComponent<Image>();
        buttonImage.color = new Color(0.16f, 0.2f, 0.3f, 0.92f);

        GameObject labelObject = new GameObject("Label", typeof(RectTransform));
        labelObject.transform.SetParent(buttonObject.transform, false);

        RectTransform labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = new Vector2(8f, 6f);
        labelRect.offsetMax = new Vector2(-8f, -6f);

        TextMeshProUGUI label = labelObject.AddComponent<TextMeshProUGUI>();
        label.alignment = TextAlignmentOptions.Center;
        label.enableAutoSizing = true;
        label.fontSizeMin = 12f;
        label.fontSizeMax = 22f;
        label.color = Color.white;

        if (runtimeFont != null)
            label.font = runtimeFont;

        RunUpgradeButtonUI buttonUI = buttonObject.GetComponent<RunUpgradeButtonUI>();
        buttonUI.Initialize(this, type, label, buttonImage, buttonObject.GetComponent<Button>());

        return buttonUI;
    }

    RectTransform GetRuntimeUIParent()
    {
        if (scoreText != null && scoreText.rectTransform.parent != null)
            return scoreText.rectTransform.parent as RectTransform;

        Canvas canvas = FindAnyObjectByType<Canvas>();

        if (canvas != null)
            return canvas.GetComponent<RectTransform>();

        return null;
    }

    TMP_FontAsset GetRuntimeFont()
    {
        if (scoreText != null)
            return scoreText.font;

        if (upgradeText != null)
            return upgradeText.font;

        return null;
    }

    void RefreshRunUpgradeButtons()
    {
        if (runUpgradeButtons.Count == 0)
            return;

        List<UpgradeType> buttonTypes = new List<UpgradeType>(runUpgradeButtons.Keys);

        for (int i = 0; i < buttonTypes.Count; i++)
        {
            UpgradeType type = buttonTypes[i];
            RunUpgradeButtonUI buttonUI = runUpgradeButtons[type];

            if (buttonUI == null)
                continue;

            string stateText = BuildButtonStateText(type);
            string modeText = BuildButtonModeText(type);
            bool hasEffect = HasBufferedUpgradeState(type);
            bool isSelected = IsUpgradeAutoEnabled(type);
            bool canUse = CanUseRunUpgradeButton(type);

            buttonUI.RefreshView(
                GetUpgradeOwnedCount(type),
                stateText,
                modeText,
                isSelected,
                hasEffect,
                canUse,
                !IsAutoToggleUpgrade(type));
        }
    }

    string BuildButtonStateText(UpgradeType type)
    {
        switch (type)
        {
            case UpgradeType.Shield:
                if (activeShields > 0)
                    return "Shield ready";

                if (GetUpgradeOwnedCount(type) > 0)
                    return "Waiting to arm";

                return "Out";
            case UpgradeType.ExtraLife:
                if (armedExtraLives > 0)
                    return "Life armed";

                if (GetUpgradeOwnedCount(type) > 0)
                    return "Waiting to arm";

                return "Out";
            case UpgradeType.Bomb:
                return "Clear screen";
            default:
                if (IsUpgradeActive(type))
                    return Mathf.CeilToInt(GetUpgradeRemainingTime(type)) + "s active";

                if (GetUpgradeOwnedCount(type) > 0)
                    return "Ready";

                return "Out";
        }
    }

    string BuildButtonModeText(UpgradeType type)
    {
        if (!IsAutoToggleUpgrade(type))
            return "Tap to use";

        return IsUpgradeAutoEnabled(type) ? "Auto ON" : "Auto OFF";
    }

    bool CanUseRunUpgradeButton(UpgradeType type)
    {
        if (gameEnded)
            return false;

        if (IsAutoToggleUpgrade(type))
            return GetUpgradeOwnedCount(type) > 0 || HasBufferedUpgradeState(type) || IsUpgradeAutoEnabled(type);

        return GetUpgradeOwnedCount(type) > 0;
    }

    int GetUpgradeOwnedCount(UpgradeType type)
    {
        if (UpgradeInventory.Instance == null)
            return 0;

        return UpgradeInventory.Instance.GetAmount(type);
    }

    string BuildUpgradeSummary(UpgradeType type)
    {
        switch (type)
        {
            case UpgradeType.Shield:
                return "Shield: " + activeShields + " active / " + GetUpgradeOwnedCount(type) + " owned";
            case UpgradeType.ExtraLife:
                return "Extra Life: " + armedExtraLives + " armed / " + GetUpgradeOwnedCount(type) + " owned";
            case UpgradeType.Bomb:
                return "Bomb: " + GetUpgradeOwnedCount(type) + " ready";
            default:
                if (IsUpgradeActive(type))
                {
                    return UpgradeInventory.GetDisplayName(type) + ": " +
                           Mathf.CeilToInt(GetUpgradeRemainingTime(type)) + "s";
                }

                return UpgradeInventory.GetDisplayName(type) + ": " + GetUpgradeOwnedCount(type) + " owned";
        }
    }

    void ApplyContinuousEffects()
    {
        if (player == null)
            return;

        player.SetSizeMultiplier(GetPlayerSizeMultiplier());
        UpdateUpgradeText();
    }

    public bool IsUpgradeActive(UpgradeType type)
    {
        if (!activeUpgradeEndTimes.ContainsKey(type))
            return false;

        return activeUpgradeEndTimes[type] > Time.time;
    }

    public float GetUpgradeRemainingTime(UpgradeType type)
    {
        if (!activeUpgradeEndTimes.ContainsKey(type))
            return 0f;

        return Mathf.Max(0f, activeUpgradeEndTimes[type] - Time.time);
    }

    public float GetWorldSpeedMultiplier()
    {
        float multiplier = 1f;

        if (isDailyChallengeRun)
            multiplier *= activeDailyChallenge.worldSpeedMultiplier;

        if (IsUpgradeActive(UpgradeType.SlowTime))
            multiplier *= 0.55f;

        return multiplier;
    }

    public float GetPlayerMoveSpeedMultiplier()
    {
        if (IsUpgradeActive(UpgradeType.SpeedBoost))
            return 1.55f;

        return 1f;
    }

    public float GetPlayerSizeMultiplier()
    {
        if (IsUpgradeActive(UpgradeType.SmallerPlayer))
            return 0.68f;

        return 1f;
    }

    public float GetScoreMultiplier()
    {
        if (IsUpgradeActive(UpgradeType.ScoreBooster))
            return 2f;

        return 1f;
    }

    public int GetCoinValue()
    {
        if (IsUpgradeActive(UpgradeType.DoubleCoins))
            return 2;

        return 1;
    }

    public float GetCurrentCoinSpawnChance(float baseChance)
    {
        float spawnChance = baseChance;

        if (isDailyChallengeRun)
            spawnChance *= activeDailyChallenge.coinSpawnMultiplier;

        if (IsUpgradeActive(UpgradeType.RareCoinBoost))
            spawnChance += 0.3f;

        return Mathf.Clamp01(spawnChance);
    }

    public Transform GetCoinMagnetTarget(Vector3 coinPosition)
    {
        if (!IsUpgradeActive(UpgradeType.CoinMagnet) || player == null)
            return null;

        float magnetRadius = 2.8f;

        if (Vector3.Distance(coinPosition, player.transform.position) <= magnetRadius)
            return player.transform;

        return null;
    }

    public void GameOver()
    {
        if (gameEnded) return;

        gameEnded = true;
        int finalScore = Mathf.FloorToInt(score);
        DailyMissionSystem.RegisterRunFinished(finalScore, currentLevel);

        if (isDailyChallengeRun)
            activeDailyChallenge = DailyChallengeSystem.RegisterRunResult(finalScore, runCoinsEarned);

        SetPauseOverlayVisible(false);
        SetTutorialOverlayVisible(false);
        GameSettings.TriggerHaptic();

        if (finalScore > bestScore)
        {
            bestScore = finalScore;
            newBestScore = true;
            PlayerPrefs.SetInt(BestScoreKey, bestScore);
            PlayerPrefs.Save();
        }
        else
        {
            newBestScore = false;
        }

        if (spawner != null)
            spawner.StopSpawning();

        if (player != null)
            player.Die();

        if (gameOverText != null)
            gameOverText.SetActive(true);

        if (restartButton != null)
            restartButton.SetActive(true);

        ShowPostRunSummary(finalScore);
        UpdateBestScoreHud();
        RefreshRunUpgradeButtons();
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;

        if (isDailyChallengeRun)
            DailyChallengeSystem.BeginTodayChallengeRun();

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void EnsureRuntimeFeedbackUI()
    {
        RectTransform parentRect = GetRuntimeUIParent();

        if (parentRect == null)
            return;

        if (bestScoreHudText == null)
        {
            bestScoreHudText = TryBindSceneText(parentRect, "BestScoreText");

            if (bestScoreHudText == null)
            {
                bestScoreHudText = CreateRuntimeLabel(
                    parentRect,
                    "BestScoreText",
                    new Vector2(0.5f, 1f),
                    new Vector2(0.5f, 1f),
                    new Vector2(0.5f, 1f),
                    new Vector2(360f, 42f),
                    new Vector2(0f, -118f),
                    TextAlignmentOptions.Center,
                    20,
                    32,
                    new Color(0.9f, 0.94f, 1f, 1f));
            }
        }

        if (postRunPanel == null || postRunSummaryText == null)
        {
            if (!TryBindScenePostRunPanel(parentRect))
                CreatePostRunPanel(parentRect);
        }

        ConfigurePostRunSummaryText(postRunSummaryText);

        if (pauseButton == null)
        {
            TryBindScenePauseButton(parentRect);

            if (pauseButton == null)
                CreatePauseButton(parentRect);
        }

        if (pauseOverlayRoot == null)
            CreatePauseOverlay(parentRect);

        if (tutorialOverlayRoot == null)
            CreateTutorialOverlay(parentRect);
    }

    TextMeshProUGUI TryBindSceneText(RectTransform parentRect, string objectName)
    {
        if (parentRect == null)
            return null;

        Transform childTransform = parentRect.Find(objectName);

        if (childTransform == null)
            return null;

        return childTransform.GetComponent<TextMeshProUGUI>();
    }

    void TryBindScenePauseButton(RectTransform parentRect)
    {
        if (parentRect == null)
            return;

        Transform pauseTransform = parentRect.Find("PauseButton");

        if (pauseTransform == null)
            return;

        pauseButton = pauseTransform.GetComponent<Button>();
        pauseButtonText = pauseTransform.GetComponentInChildren<TextMeshProUGUI>(true);

        if (pauseButton != null)
        {
            pauseButton.onClick.RemoveAllListeners();
            pauseButton.onClick.AddListener(TogglePause);
        }
    }

    bool TryBindScenePostRunPanel(RectTransform parentRect)
    {
        if (parentRect == null)
            return false;

        Transform panelTransform = parentRect.Find("PostRunSummaryPanel");

        if (panelTransform == null)
            return false;

        postRunPanel = panelTransform.gameObject;

        Transform summaryTransform = panelTransform.Find("PostRunSummaryText");

        if (summaryTransform != null)
            postRunSummaryText = summaryTransform.GetComponent<TextMeshProUGUI>();

        if (postRunPanel != null)
            postRunPanel.SetActive(false);

        return postRunPanel != null && postRunSummaryText != null;
    }

    void ConfigurePostRunSummaryText(TextMeshProUGUI summaryText)
    {
        if (summaryText == null)
            return;

        summaryText.enableAutoSizing = true;
        summaryText.fontSizeMin = 26f;
        summaryText.fontSizeMax = 44f;
        summaryText.lineSpacing = 10f;
        summaryText.alignment = TextAlignmentOptions.Center;
        summaryText.color = new Color(0.96f, 0.98f, 1f, 1f);

        if (runtimeFont != null)
            summaryText.font = runtimeFont;
    }

    TextMeshProUGUI CreateRuntimeLabel(
        RectTransform parent,
        string objectName,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 pivot,
        Vector2 size,
        Vector2 anchoredPosition,
        TextAlignmentOptions alignment,
        float minSize,
        float maxSize,
        Color color)
    {
        GameObject labelObject = new GameObject(objectName, typeof(RectTransform));
        labelObject.transform.SetParent(parent, false);

        RectTransform labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.anchorMin = anchorMin;
        labelRect.anchorMax = anchorMax;
        labelRect.pivot = pivot;
        labelRect.sizeDelta = size;
        labelRect.anchoredPosition = anchoredPosition;

        TextMeshProUGUI label = labelObject.AddComponent<TextMeshProUGUI>();
        label.alignment = alignment;
        label.enableAutoSizing = true;
        label.fontSizeMin = minSize;
        label.fontSizeMax = maxSize;
        label.color = color;

        if (runtimeFont != null)
            label.font = runtimeFont;

        return label;
    }

    void CreatePostRunPanel(RectTransform parentRect)
    {
        postRunPanel = new GameObject("PostRunPanel", typeof(RectTransform), typeof(Image));
        postRunPanel.transform.SetParent(parentRect, false);

        RectTransform panelRect = postRunPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(560f, 250f);
        panelRect.anchoredPosition = new Vector2(0f, -290f);

        Image panelImage = postRunPanel.GetComponent<Image>();
        panelImage.color = new Color(0.12f, 0.18f, 0.3f, 0.9f);

        postRunSummaryText = CreateRuntimeLabel(
            panelRect,
            "PostRunSummaryText",
            Vector2.zero,
            Vector2.one,
            new Vector2(0.5f, 0.5f),
            Vector2.zero,
            Vector2.zero,
            TextAlignmentOptions.Center,
            26f,
            44f,
            new Color(0.97f, 0.98f, 1f, 1f));

        ConfigurePostRunSummaryText(postRunSummaryText);

        if (postRunSummaryText != null)
        {
            RectTransform summaryRect = postRunSummaryText.rectTransform;
            summaryRect.offsetMin = new Vector2(28f, 24f);
            summaryRect.offsetMax = new Vector2(-28f, -24f);
        }

        postRunPanel.SetActive(false);
    }

    void CreatePauseButton(RectTransform parentRect)
    {
        pauseButton = CreateRuntimeButton(
            parentRect,
            "PauseButton",
            new Vector2(0f, 1f),
            new Vector2(0f, 1f),
            new Vector2(0f, 1f),
            new Vector2(190f, 78f),
            new Vector2(28f, -28f),
            "Pause",
            new Color(0.18f, 0.26f, 0.42f, 0.96f),
            out pauseButtonText);

        if (pauseButton != null)
        {
            pauseButton.onClick.RemoveAllListeners();
            pauseButton.onClick.AddListener(TogglePause);
        }
    }

    void CreatePauseOverlay(RectTransform parentRect)
    {
        pauseOverlayRoot = new GameObject("PauseOverlayRoot", typeof(RectTransform), typeof(Image));
        pauseOverlayRoot.transform.SetParent(parentRect, false);

        RectTransform rootRect = pauseOverlayRoot.GetComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;

        Image overlayImage = pauseOverlayRoot.GetComponent<Image>();
        overlayImage.color = new Color(0.03f, 0.05f, 0.09f, 0.82f);

        GameObject panelObject = new GameObject("PausePanel", typeof(RectTransform), typeof(Image));
        panelObject.transform.SetParent(pauseOverlayRoot.transform, false);

        RectTransform panelRect = panelObject.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(720f, 700f);
        panelRect.anchoredPosition = new Vector2(0f, -10f);

        Image panelImage = panelObject.GetComponent<Image>();
        panelImage.color = new Color(0.11f, 0.17f, 0.29f, 1f);

        pauseOverlayTitleText = CreateRuntimeLabel(
            panelRect,
            "PauseTitle",
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(520f, 60f),
            new Vector2(0f, -28f),
            TextAlignmentOptions.Center,
            30f,
            40f,
            Color.white);

        Button resumeButton = CreateRuntimeButton(
            panelRect,
            "ResumeButton",
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(420f, 72f),
            new Vector2(0f, -122f),
            "Resume",
            new Color(0.25f, 0.53f, 0.34f, 1f),
            out _);

        if (resumeButton != null)
        {
            resumeButton.onClick.RemoveAllListeners();
            resumeButton.onClick.AddListener(ResumeGameplay);
        }

        hapticsStatusText = CreateRuntimeLabel(
            panelRect,
            "HapticsStatus",
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(520f, 44f),
            new Vector2(0f, -220f),
            TextAlignmentOptions.Center,
            20f,
            28f,
            new Color(0.88f, 0.94f, 1f, 1f));

        hapticsToggleButton = CreateRuntimeButton(
            panelRect,
            "HapticsToggleButton",
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(420f, 68f),
            new Vector2(0f, -282f),
            "Haptics",
            new Color(0.93f, 0.73f, 0.24f, 1f),
            out hapticsToggleButtonText);

        if (hapticsToggleButton != null)
        {
            hapticsToggleButton.onClick.RemoveAllListeners();
            hapticsToggleButton.onClick.AddListener(ToggleHaptics);
        }

        Button tutorialButton = CreateRuntimeButton(
            panelRect,
            "ReplayTutorialButton",
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(420f, 68f),
            new Vector2(0f, -364f),
            "How To Play",
            new Color(0.24f, 0.36f, 0.56f, 1f),
            out _);

        if (tutorialButton != null)
        {
            tutorialButton.onClick.RemoveAllListeners();
            tutorialButton.onClick.AddListener(ReplayTutorial);
        }

        Button restartButtonOverlay = CreateRuntimeButton(
            panelRect,
            "RestartFromPauseButton",
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(420f, 68f),
            new Vector2(0f, -446f),
            "Restart Run",
            new Color(0.72f, 0.45f, 0.19f, 1f),
            out _);

        if (restartButtonOverlay != null)
        {
            restartButtonOverlay.onClick.RemoveAllListeners();
            restartButtonOverlay.onClick.AddListener(RestartGame);
        }

        Button menuButtonOverlay = CreateRuntimeButton(
            panelRect,
            "MenuFromPauseButton",
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(420f, 68f),
            new Vector2(0f, -528f),
            "Main Menu",
            new Color(0.32f, 0.42f, 0.56f, 1f),
            out _);

        if (menuButtonOverlay != null)
        {
            menuButtonOverlay.onClick.RemoveAllListeners();
            menuButtonOverlay.onClick.AddListener(ReturnToMainMenu);
        }

        pauseOverlayRoot.SetActive(false);
    }

    void CreateTutorialOverlay(RectTransform parentRect)
    {
        tutorialOverlayRoot = new GameObject("TutorialOverlayRoot", typeof(RectTransform), typeof(Image));
        tutorialOverlayRoot.transform.SetParent(parentRect, false);

        RectTransform rootRect = tutorialOverlayRoot.GetComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;

        Image overlayImage = tutorialOverlayRoot.GetComponent<Image>();
        overlayImage.color = new Color(0.02f, 0.05f, 0.1f, 0.88f);

        GameObject panelObject = new GameObject("TutorialPanel", typeof(RectTransform), typeof(Image));
        panelObject.transform.SetParent(tutorialOverlayRoot.transform, false);

        RectTransform panelRect = panelObject.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(860f, 900f);
        panelRect.anchoredPosition = new Vector2(0f, -10f);

        Image panelImage = panelObject.GetComponent<Image>();
        panelImage.color = new Color(0.1f, 0.18f, 0.31f, 1f);

        CreateRuntimeLabel(
            panelRect,
            "TutorialTitle",
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(700f, 86f),
            new Vector2(0f, -34f),
            TextAlignmentOptions.Center,
            38f,
            56f,
            Color.white).text = "How To Play";

        tutorialBodyText = CreateRuntimeLabel(
            panelRect,
            "TutorialBody",
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(740f, 460f),
            new Vector2(0f, -170f),
            TextAlignmentOptions.Center,
            34f,
            48f,
            new Color(0.93f, 0.97f, 1f, 1f));

        if (tutorialBodyText != null)
        {
            tutorialBodyText.text =
                "Avoid every obstacle, no matter the color." +
                "\nCollect the gold coins to buy consumables." +
                "\nEquip up to 3 upgrades in Inventory before each run." +
                "\nTap a run upgrade once to keep shields, buffs, and extra lives auto-armed.";
            tutorialBodyText.lineSpacing = 18f;
        }

        tutorialPrimaryButton = CreateRuntimeButton(
            panelRect,
            "TutorialPrimaryButton",
            new Vector2(0.5f, 0f),
            new Vector2(0.5f, 0f),
            new Vector2(0.5f, 0f),
            new Vector2(420f, 92f),
            new Vector2(0f, 42f),
            "Start Run",
            new Color(0.25f, 0.53f, 0.34f, 1f),
            out tutorialPrimaryButtonText);

        if (tutorialPrimaryButton != null)
        {
            tutorialPrimaryButton.onClick.RemoveAllListeners();
            tutorialPrimaryButton.onClick.AddListener(CloseTutorialOverlay);
        }

        tutorialOverlayRoot.SetActive(false);
    }

    Button CreateRuntimeButton(
        RectTransform parent,
        string objectName,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 pivot,
        Vector2 size,
        Vector2 anchoredPosition,
        string text,
        Color backgroundColor,
        out TextMeshProUGUI label)
    {
        GameObject buttonObject = new GameObject(objectName, typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);

        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        buttonRect.anchorMin = anchorMin;
        buttonRect.anchorMax = anchorMax;
        buttonRect.pivot = pivot;
        buttonRect.sizeDelta = size;
        buttonRect.anchoredPosition = anchoredPosition;

        Image buttonImage = buttonObject.GetComponent<Image>();
        buttonImage.color = backgroundColor;

        GameObject labelObject = new GameObject("Label", typeof(RectTransform));
        labelObject.transform.SetParent(buttonObject.transform, false);

        RectTransform labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = new Vector2(10f, 8f);
        labelRect.offsetMax = new Vector2(-10f, -8f);

        label = labelObject.AddComponent<TextMeshProUGUI>();
        label.text = text;
        label.alignment = TextAlignmentOptions.Center;
        label.enableAutoSizing = true;
        label.fontSizeMin = 28f;
        label.fontSizeMax = 40f;
        label.color = Color.white;

        if (runtimeFont != null)
            label.font = runtimeFont;

        return buttonObject.GetComponent<Button>();
    }

    void RefreshPauseOverlayState()
    {
        if (pauseOverlayTitleText != null)
            pauseOverlayTitleText.text = "Paused";

        if (hapticsStatusText != null)
            hapticsStatusText.text = "Haptics: " + (GameSettings.IsHapticsEnabled() ? "On" : "Off");

        if (hapticsToggleButtonText != null)
            hapticsToggleButtonText.text = GameSettings.IsHapticsEnabled() ? "Turn Haptics Off" : "Turn Haptics On";

        if (pauseButtonText != null)
            pauseButtonText.text = IsPauseOverlayVisible() ? "Paused" : "Pause";
    }

    void UpdateBestScoreHud()
    {
        if (bestScoreHudText != null)
        {
            if (isDailyChallengeRun)
            {
                bestScoreHudText.text = gameEnded
                    ? DailyChallengeSystem.GetBestProgressLabel(activeDailyChallenge)
                    : DailyChallengeSystem.GetCurrentRunLabel(activeDailyChallenge, Mathf.FloorToInt(score), runCoinsEarned);
            }
            else
                bestScoreHudText.text = "Best: " + bestScore;
        }
    }

    void ShowPostRunSummary(int finalScore)
    {
        if (postRunPanel == null || postRunSummaryText == null)
            return;

        int completedMissionCount = DailyMissionSystem.GetCompletedCount();
        int claimableMissionCount = DailyMissionSystem.GetClaimableCount();

        string summaryText;

        if (isDailyChallengeRun)
        {
            summaryText =
                activeDailyChallenge.title +
                "\n" + DailyChallengeSystem.GetCurrentRunLabel(activeDailyChallenge, finalScore, runCoinsEarned) +
                "\n" + DailyChallengeSystem.GetBestProgressLabel(activeDailyChallenge);

            if (DailyChallengeSystem.CanClaimReward())
            {
                summaryText += "\n<color=#FFD876>Challenge reward ready in menu</color>";
            }
            else if (activeDailyChallenge.rewardClaimed)
            {
                summaryText += "\n<color=#7FF0A6>Reward already claimed today</color>";
            }
            else if (activeDailyChallenge.bestScore >= activeDailyChallenge.targetScore)
            {
                summaryText += "\n<color=#7FF0A6>Challenge cleared</color>";
            }
            else
            {
                summaryText += "\n" + DailyChallengeSystem.GetNeedsMoreLabel(activeDailyChallenge);
            }
        }
        else
        {
            summaryText =
                "Run Score " + finalScore +
                "\nBest " + bestScore + "   Coins +" + runCoinsEarned +
                "\nLevel " + currentLevel + "   Missions " + completedMissionCount + "/3";

            if (claimableMissionCount > 0)
            {
                summaryText +=
                    "\n<color=#FFD876>" +
                    claimableMissionCount +
                    (claimableMissionCount == 1 ? " reward ready in menu" : " rewards ready in menu") +
                    "</color>";
            }
        }

        if (newBestScore)
            summaryText += "\n<color=#7FF0A6>New Best!</color>";

        postRunSummaryText.text = summaryText;
        postRunPanel.SetActive(true);
    }

    void ShowTutorialIfNeeded()
    {
        if (GameSettings.HasSeenTutorial())
            return;

        markTutorialSeenOnClose = true;
        SetTutorialOverlayVisible(true);
    }

    public void TogglePause()
    {
        if (gameEnded || IsTutorialOverlayVisible())
            return;

        SetPauseOverlayVisible(!IsPauseOverlayVisible());
    }

    public void ResumeGameplay()
    {
        SetPauseOverlayVisible(false);
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;

        if (isDailyChallengeRun)
            DailyChallengeSystem.ClearActiveRun();

        SceneManager.LoadScene("MainMenu");
    }

    public void ReplayTutorial()
    {
        markTutorialSeenOnClose = false;
        SetTutorialOverlayVisible(true);
    }

    public void CloseTutorialOverlay()
    {
        if (markTutorialSeenOnClose)
        {
            GameSettings.MarkTutorialSeen();
            markTutorialSeenOnClose = false;
        }

        SetTutorialOverlayVisible(false);
    }

    public void ToggleHaptics()
    {
        GameSettings.SetHapticsEnabled(!GameSettings.IsHapticsEnabled());
        RefreshPauseOverlayState();
        GameSettings.TriggerHaptic();
    }

    void SetPauseOverlayVisible(bool isVisible)
    {
        if (pauseOverlayRoot != null)
            pauseOverlayRoot.SetActive(isVisible);

        RefreshPauseOverlayState();
        UpdateOverlayTimeScale();
    }

    void SetTutorialOverlayVisible(bool isVisible)
    {
        if (tutorialOverlayRoot != null)
            tutorialOverlayRoot.SetActive(isVisible);

        UpdateOverlayTimeScale();
    }

    bool IsPauseOverlayVisible()
    {
        return pauseOverlayRoot != null && pauseOverlayRoot.activeSelf;
    }

    bool IsTutorialOverlayVisible()
    {
        return tutorialOverlayRoot != null && tutorialOverlayRoot.activeSelf;
    }

    void UpdateOverlayTimeScale()
    {
        Time.timeScale = (IsPauseOverlayVisible() || IsTutorialOverlayVisible()) ? 0f : 1f;
    }
}

public class RunUpgradeButtonUI : MonoBehaviour
{
    private GameManager gameManager;
    private UpgradeType upgradeType;
    private TextMeshProUGUI label;
    private Image backgroundImage;
    private Button button;

    private readonly Color readyColor = new Color(0.18f, 0.33f, 0.56f, 0.96f);
    private readonly Color selectedColor = new Color(0.17f, 0.46f, 0.52f, 0.98f);
    private readonly Color activeColor = new Color(0.18f, 0.58f, 0.38f, 0.98f);
    private readonly Color manualColor = new Color(0.6f, 0.36f, 0.13f, 0.98f);
    private readonly Color disabledColor = new Color(0.3f, 0.32f, 0.37f, 0.88f);

    public void Initialize(
        GameManager manager,
        UpgradeType type,
        TextMeshProUGUI labelText,
        Image image,
        Button sourceButton)
    {
        gameManager = manager;
        upgradeType = type;
        label = labelText;
        backgroundImage = image;
        button = sourceButton;

        if (button != null)
        {
            button.onClick.RemoveListener(OnPressed);
            button.onClick.AddListener(OnPressed);
        }
    }

    public void RefreshView(
        int ownedAmount,
        string stateText,
        string modeText,
        bool isSelected,
        bool hasEffect,
        bool canUse,
        bool isManualUse)
    {
        if (label != null)
        {
            label.text =
                UpgradeInventory.GetDisplayName(upgradeType) +
                "\n<size=80%>" + modeText + "</size>" +
                "\n<size=68%>" + stateText + "  |  Owned " + ownedAmount + "</size>";
        }

        if (backgroundImage != null)
        {
            if (hasEffect)
                backgroundImage.color = activeColor;
            else if (isSelected)
                backgroundImage.color = selectedColor;
            else if (isManualUse && canUse)
                backgroundImage.color = manualColor;
            else if (canUse)
                backgroundImage.color = readyColor;
            else
                backgroundImage.color = disabledColor;
        }

        if (button != null)
            button.interactable = canUse;
    }

    void OnPressed()
    {
        if (gameManager != null)
            gameManager.UseRunUpgrade(upgradeType);
    }
}
```

### GameSettings.cs
```csharp
using UnityEngine;

public static class GameSettings
{
    private const string HapticsEnabledKey = "Settings_HapticsEnabled";
    private const string TutorialSeenKey = "Settings_TutorialSeen";

    public static bool IsHapticsEnabled()
    {
        return PlayerPrefs.GetInt(HapticsEnabledKey, 1) == 1;
    }

    public static void SetHapticsEnabled(bool enabled)
    {
        PlayerPrefs.SetInt(HapticsEnabledKey, enabled ? 1 : 0);
        PlayerPrefs.Save();
    }

    public static bool HasSeenTutorial()
    {
        return PlayerPrefs.GetInt(TutorialSeenKey, 0) == 1;
    }

    public static void MarkTutorialSeen()
    {
        PlayerPrefs.SetInt(TutorialSeenKey, 1);
        PlayerPrefs.Save();
    }

    public static void TriggerHaptic()
    {
        if (!IsHapticsEnabled())
            return;

#if UNITY_ANDROID || UNITY_IOS
        Handheld.Vibrate();
#endif
    }
}
```

### GameUI.cs
```csharp
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameUI : MonoBehaviour
{
    public string menuSceneName = "MainMenu";

    public void GoToMenu()
    {
        Time.timeScale = 1f; // ensure game isn't paused
        SceneManager.LoadScene(menuSceneName);
    }
}
```

### InventoryMenu.cs
```csharp
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryMenu : MonoBehaviour
{
    public TextMeshProUGUI equippedCountText;
    public TextMeshProUGUI feedbackText;
    public InventorySlotUI[] slotUIs;
    public InventorySlotUI slotTemplate;
    public RectTransform slotParent;

    public int columnCount = 1;
    public float sidePadding = 20f;
    public float topPadding = 154f;
    public float bottomPadding = 92f;
    public float columnSpacing = 18f;
    public float rowSpacing = 12f;
    public float slotHeight = 92f;
    public float cardHorizontalInset = 38f;
    public float cardMaxWidth = 560f;

    private RectTransform titleRect;
    private TextMeshProUGUI titleText;
    private RectTransform backButtonRect;
    private TextMeshProUGUI backButtonText;
    private RectTransform slotViewport;
    private RectTransform slotContent;
    private ScrollRect slotScrollRect;

    private readonly UpgradeType[] slotOrder =
    {
        UpgradeType.Shield,
        UpgradeType.SpeedBoost,
        UpgradeType.ExtraLife,
        UpgradeType.CoinMagnet,
        UpgradeType.DoubleCoins,
        UpgradeType.SlowTime,
        UpgradeType.SmallerPlayer,
        UpgradeType.ScoreBooster,
        UpgradeType.Bomb,
        UpgradeType.RareCoinBoost
    };

    void Awake()
    {
        NormalizeLegacyPanelLayout();

        if (slotParent == null)
            slotParent = transform as RectTransform;

        if (slotTemplate == null)
            slotTemplate = GetComponentInChildren<InventorySlotUI>(true);

        FindStaticReferences();
        EnsureRuntimeLabels();
        EnsureScrollArea();
        RebuildSlotArray();
    }

    void Start()
    {
        BuildSlotsFromTemplate();
        RefreshUI();
    }

    void OnEnable()
    {
        RefreshUI();
    }

    void BuildSlotsFromTemplate()
    {
        if (slotTemplate == null)
            return;

        if (slotParent == null)
            slotParent = transform as RectTransform;

        EnsureScrollArea();

        if (slotTemplate.transform.parent != slotContent)
            slotTemplate.transform.SetParent(slotContent, false);

        RebuildSlotArray();

        while (slotUIs.Length < slotOrder.Length)
        {
            InventorySlotUI newSlot = Instantiate(slotTemplate, slotContent);
            newSlot.gameObject.SetActive(true);
            RebuildSlotArray();
        }

        Canvas.ForceUpdateCanvases();
        LayoutStaticElements();
        Canvas.ForceUpdateCanvases();
        LayoutSlotList();
        Canvas.ForceUpdateCanvases();

        for (int i = 0; i < slotOrder.Length; i++)
        {
            InventorySlotUI slot = slotUIs[i];

            if (slot == null)
                continue;

            slot.Initialize(this, slotOrder[i]);
            slot.gameObject.name = UpgradeInventory.GetDisplayName(slotOrder[i]).Replace(" ", "") + "Slot";
            PositionSlot(slot.GetComponent<RectTransform>(), i);
        }
    }

    void PositionSlot(RectTransform slotRect, int index)
    {
        if (slotRect == null || slotViewport == null || slotContent == null)
            return;

        float y = -(index * (slotHeight + rowSpacing));
        float availableWidth = Mathf.Max(320f, slotViewport.rect.width - (cardHorizontalInset * 2f));
        float slotWidth = Mathf.Min(availableWidth, cardMaxWidth);

        slotRect.anchorMin = new Vector2(0.5f, 1f);
        slotRect.anchorMax = new Vector2(0.5f, 1f);
        slotRect.pivot = new Vector2(0.5f, 1f);
        slotRect.sizeDelta = new Vector2(slotWidth, slotHeight);
        slotRect.anchoredPosition = new Vector2(0f, y);
    }

    void LayoutSlotList()
    {
        if (slotViewport == null || slotContent == null)
            return;

        int rowCount = slotOrder.Length;
        float contentHeight = (rowCount * slotHeight) + ((rowCount - 1) * rowSpacing);
        float viewportHeight = slotViewport.rect.height;

        if (contentHeight < viewportHeight)
            contentHeight = viewportHeight;

        slotContent.anchorMin = new Vector2(0f, 1f);
        slotContent.anchorMax = new Vector2(1f, 1f);
        slotContent.pivot = new Vector2(0.5f, 1f);
        slotContent.sizeDelta = new Vector2(0f, contentHeight);
        slotContent.anchoredPosition = Vector2.zero;
    }

    void FindStaticReferences()
    {
        if (titleRect == null)
        {
            Transform titleTransform = transform.Find("InventoryTitle");

            if (titleTransform != null)
            {
                titleRect = titleTransform as RectTransform;
                titleText = titleTransform.GetComponent<TextMeshProUGUI>();
            }
        }

        if (backButtonRect == null)
        {
            Transform backTransform = transform.Find("BackButton");

            if (backTransform != null)
            {
                backButtonRect = backTransform as RectTransform;
                backButtonText = backTransform.GetComponentInChildren<TextMeshProUGUI>(true);
            }
        }
    }

    void NormalizeLegacyPanelLayout()
    {
        Canvas canvas = FindAnyObjectByType<Canvas>();

        if (canvas != null)
        {
            RectTransform canvasRect = canvas.GetComponent<RectTransform>();

            if (canvasRect != null)
            {
                canvasRect.localScale = Vector3.one;
                canvasRect.anchorMin = Vector2.zero;
                canvasRect.anchorMax = Vector2.zero;
                canvasRect.sizeDelta = Vector2.zero;
            }

            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();

            if (scaler != null)
            {
                scaler.referenceResolution = new Vector2(1080f, 1920f);
                scaler.matchWidthOrHeight = 0.5f;
            }
        }

        RectTransform panelRect = transform as RectTransform;

        if (panelRect != null)
        {
            panelRect.localScale = Vector3.one;
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
        }
    }

    void EnsureRuntimeLabels()
    {
        if (equippedCountText == null)
            equippedCountText = CreateRuntimeLabel("EquippedCountText");

        if (feedbackText == null)
            feedbackText = CreateRuntimeLabel("FeedbackText");
    }

    TextMeshProUGUI CreateRuntimeLabel(string objectName)
    {
        Transform existing = transform.Find(objectName);

        if (existing != null)
            return existing.GetComponent<TextMeshProUGUI>();

        GameObject labelObject = new GameObject(objectName, typeof(RectTransform));
        labelObject.transform.SetParent(transform, false);

        TextMeshProUGUI label = labelObject.AddComponent<TextMeshProUGUI>();
        label.fontSize = 20;
        label.enableAutoSizing = true;
        label.fontSizeMin = 14;
        label.fontSizeMax = 20;
        label.alignment = TextAlignmentOptions.Center;
        label.color = new Color(0.16f, 0.18f, 0.24f, 1f);

        return label;
    }

    void EnsureScrollArea()
    {
        if (slotParent == null)
            return;

        if (slotViewport == null)
        {
            Transform viewportTransform = transform.Find("SlotViewport");

            if (viewportTransform != null)
            {
                slotViewport = viewportTransform as RectTransform;
            }
            else
            {
                GameObject viewportObject = new GameObject("SlotViewport", typeof(RectTransform), typeof(RectMask2D));
                viewportObject.transform.SetParent(transform, false);
                slotViewport = viewportObject.GetComponent<RectTransform>();
            }
        }

        if (slotViewport.GetComponent<RectMask2D>() == null)
            slotViewport.gameObject.AddComponent<RectMask2D>();

        if (slotContent == null)
        {
            Transform contentTransform = slotViewport.Find("SlotContent");

            if (contentTransform != null)
            {
                slotContent = contentTransform as RectTransform;
            }
            else
            {
                GameObject contentObject = new GameObject("SlotContent", typeof(RectTransform));
                contentObject.transform.SetParent(slotViewport, false);
                slotContent = contentObject.GetComponent<RectTransform>();
            }
        }

        if (slotScrollRect == null)
        {
            slotScrollRect = GetComponent<ScrollRect>();

            if (slotScrollRect == null)
                slotScrollRect = gameObject.AddComponent<ScrollRect>();
        }

        slotScrollRect.viewport = slotViewport;
        slotScrollRect.content = slotContent;
        slotScrollRect.horizontal = false;
        slotScrollRect.vertical = true;
        slotScrollRect.movementType = ScrollRect.MovementType.Clamped;
        slotScrollRect.scrollSensitivity = 30f;
    }

    void LayoutStaticElements()
    {
        FindStaticReferences();

        float panelWidth = slotParent.rect.width;
        float panelHeight = slotParent.rect.height;

        if (titleRect != null)
        {
            titleRect.anchorMin = new Vector2(0.5f, 1f);
            titleRect.anchorMax = new Vector2(0.5f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.sizeDelta = new Vector2(panelWidth - (sidePadding * 2f), 62f);
            titleRect.anchoredPosition = new Vector2(0f, -18f);
        }

        if (titleText != null)
        {
            titleText.text = "Choose 3 Upgrades";
            titleText.enableAutoSizing = true;
            titleText.fontSizeMin = 26;
            titleText.fontSizeMax = 40;
            titleText.alignment = TextAlignmentOptions.Center;
        }

        if (equippedCountText != null)
        {
            RectTransform countRect = equippedCountText.rectTransform;
            countRect.anchorMin = new Vector2(0.5f, 1f);
            countRect.anchorMax = new Vector2(0.5f, 1f);
            countRect.pivot = new Vector2(0.5f, 1f);
            countRect.sizeDelta = new Vector2(panelWidth - (sidePadding * 2f), 38f);
            countRect.anchoredPosition = new Vector2(0f, -76f);
            equippedCountText.enableAutoSizing = true;
            equippedCountText.fontSizeMin = 20;
            equippedCountText.fontSizeMax = 30;
        }

        if (feedbackText != null)
        {
            RectTransform feedbackRect = feedbackText.rectTransform;
            feedbackRect.anchorMin = new Vector2(0.5f, 1f);
            feedbackRect.anchorMax = new Vector2(0.5f, 1f);
            feedbackRect.pivot = new Vector2(0.5f, 1f);
            feedbackRect.sizeDelta = new Vector2(panelWidth - (sidePadding * 2f), 34f);
            feedbackRect.anchoredPosition = new Vector2(0f, -114f);
            feedbackText.enableAutoSizing = true;
            feedbackText.fontSizeMin = 18;
            feedbackText.fontSizeMax = 24;
        }

        if (backButtonRect != null)
        {
            backButtonRect.anchorMin = new Vector2(0.5f, 0f);
            backButtonRect.anchorMax = new Vector2(0.5f, 0f);
            backButtonRect.pivot = new Vector2(0.5f, 0f);
            backButtonRect.sizeDelta = new Vector2(260f, 62f);
            backButtonRect.anchoredPosition = new Vector2(0f, 18f);
        }

        if (backButtonText != null)
        {
            backButtonText.enableAutoSizing = true;
            backButtonText.fontSizeMin = 18;
            backButtonText.fontSizeMax = 28;
        }

        if (slotViewport != null)
        {
            slotViewport.anchorMin = new Vector2(0f, 0f);
            slotViewport.anchorMax = new Vector2(1f, 1f);
            slotViewport.pivot = new Vector2(0.5f, 0.5f);
            slotViewport.offsetMin = new Vector2(sidePadding, bottomPadding);
            slotViewport.offsetMax = new Vector2(-sidePadding, -topPadding);
        }
    }

    void RebuildSlotArray()
    {
        if (slotContent != null)
            slotUIs = slotContent.GetComponentsInChildren<InventorySlotUI>(true);
        else
            slotUIs = GetComponentsInChildren<InventorySlotUI>(true);
    }

    public void OnSlotPressed(UpgradeType type)
    {
        if (UpgradeInventory.Instance == null)
            return;

        EquipToggleResult result = UpgradeInventory.Instance.ToggleEquippedUpgrade(type);
        UpdateFeedback(result, type);
        RefreshUI();
    }

    public void RefreshUI()
    {
        int equippedCount = 0;

        if (UpgradeInventory.Instance != null)
            equippedCount = UpgradeInventory.Instance.GetEquippedCount();

        if (equippedCountText != null)
            equippedCountText.text = "Equipped: " + equippedCount + "/" + UpgradeInventory.MaxEquippedUpgrades;

        if (feedbackText != null && string.IsNullOrEmpty(feedbackText.text))
            feedbackText.text = "Tap a card to select it";

        RebuildSlotArray();
        Canvas.ForceUpdateCanvases();
        LayoutStaticElements();
        Canvas.ForceUpdateCanvases();
        LayoutSlotList();
        Canvas.ForceUpdateCanvases();

        for (int i = 0; i < slotUIs.Length; i++)
        {
            if (slotUIs[i] != null)
                PositionSlot(slotUIs[i].GetComponent<RectTransform>(), i);
        }

        foreach (InventorySlotUI slot in slotUIs)
        {
            if (slot != null)
                slot.Refresh();
        }
    }

    void UpdateFeedback(EquipToggleResult result, UpgradeType type)
    {
        if (feedbackText == null)
            return;

        string upgradeName = UpgradeInventory.GetDisplayName(type);

        switch (result)
        {
            case EquipToggleResult.Equipped:
                feedbackText.text = upgradeName + " selected";
                break;
            case EquipToggleResult.Unequipped:
                feedbackText.text = upgradeName + " removed";
                break;
            case EquipToggleResult.NotOwned:
                feedbackText.text = "Buy " + upgradeName + " first";
                break;
            case EquipToggleResult.LoadoutFull:
                feedbackText.text = "Only 3 upgrades can be selected";
                break;
        }
    }
}
```

### InventorySlotUI.cs
```csharp
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour
{
    public UpgradeType upgradeType;
    public InventoryMenu inventoryMenu;

    public TextMeshProUGUI nameText;
    public TextMeshProUGUI ownedText;
    public TextMeshProUGUI statusText;
    public TextMeshProUGUI combinedText;
    public Image backgroundImage;

    public Color normalColor = new Color(1f, 1f, 1f, 0.92f);
    public Color selectedColor = new Color(0.58f, 0.9f, 0.68f, 0.98f);
    public Color unavailableColor = new Color(0.82f, 0.82f, 0.84f, 0.78f);

    private Button button;
    private bool clickBound = false;

    void Awake()
    {
        CacheReferences();
    }

    void Start()
    {
        BindButton();
        Refresh();
    }

    void OnEnable()
    {
        Refresh();
    }

    public void Initialize(InventoryMenu menu, UpgradeType type)
    {
        inventoryMenu = menu;
        upgradeType = type;
        CacheReferences();
        BindButton();
        Refresh();
    }

    void CacheReferences()
    {
        if (inventoryMenu == null)
            inventoryMenu = GetComponentInParent<InventoryMenu>();

        if (backgroundImage == null)
            backgroundImage = GetComponent<Image>();

        if (button == null)
            button = GetComponent<Button>();

        if (combinedText == null && nameText == null && ownedText == null && statusText == null)
            combinedText = GetComponentInChildren<TextMeshProUGUI>(true);
    }

    void BindButton()
    {
        if (button == null || clickBound)
            return;

        button.onClick.RemoveListener(OnSlotPressed);
        button.onClick.AddListener(OnSlotPressed);
        clickBound = true;
    }

    public void OnSlotPressed()
    {
        if (inventoryMenu != null)
            inventoryMenu.OnSlotPressed(upgradeType);
    }

    public void Refresh()
    {
        CacheReferences();

        int ownedAmount = 0;
        bool isEquipped = false;

        if (UpgradeInventory.Instance != null)
        {
            ownedAmount = UpgradeInventory.Instance.GetAmount(upgradeType);
            isEquipped = UpgradeInventory.Instance.IsEquipped(upgradeType);
        }

        if (nameText != null)
            nameText.text = UpgradeInventory.GetDisplayName(upgradeType);

        if (ownedText != null)
            ownedText.text = "Owned: " + ownedAmount;

        if (statusText != null)
        {
            if (isEquipped)
                statusText.text = "Selected";
            else if (ownedAmount > 0)
                statusText.text = "Tap to Select";
            else
                statusText.text = "Not Owned";
        }

        if (combinedText != null)
        {
            RectTransform textRect = combinedText.rectTransform;
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(20f, 12f);
            textRect.offsetMax = new Vector2(-20f, -12f);

            combinedText.enableAutoSizing = true;
            combinedText.fontSizeMin = 18;
            combinedText.fontSizeMax = 28;
            combinedText.alignment = TextAlignmentOptions.CenterGeoAligned;
            combinedText.color = new Color(0.16f, 0.18f, 0.24f, 1f);

            string statusLine;

            if (isEquipped)
                statusLine = "Selected";
            else if (ownedAmount > 0)
                statusLine = "Ready";
            else
                statusLine = "Not Owned";

            combinedText.text =
                UpgradeInventory.GetDisplayName(upgradeType) +
                "\n<size=78%>Owned: " + ownedAmount + "   " + statusLine + "</size>";
        }

        if (backgroundImage != null)
        {
            if (isEquipped)
                backgroundImage.color = selectedColor;
            else if (ownedAmount > 0)
                backgroundImage.color = normalColor;
            else
                backgroundImage.color = unavailableColor;
        }
    }
}
```

### MainMenu.cs
```csharp
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    private const string ExpectedSceneName = "MainMenu";
    public string gameSceneName = "Game";
    public string shopSceneName = "Shop";
    public string inventorySceneName = "Inventory";
    public bool createInventoryButtonAtRuntime = true;
    public bool createDailyRewardPanelAtRuntime = true;
    public string playButtonObjectName = "PlayButton";
    public string shopButtonObjectName = "ShopButton";
    public string exitButtonObjectName = "ExitButton";
    public string inventoryButtonObjectName = "InventoryButton";
    public string titleObjectName = "TitleText";
    public float menuButtonSpacing = 150f;

    private TMP_FontAsset runtimeFont;
    private RectTransform menuRootRect;
    private TextMeshProUGUI profileStatsText;
    private bool profileStatsSceneOwned;

    private GameObject dailyRewardPanel;
    private TextMeshProUGUI dailyRewardTitleText;
    private TextMeshProUGUI dailyRewardBodyText;
    private TextMeshProUGUI dailyRewardStatusText;
    private Button claimRewardButton;
    private TextMeshProUGUI claimRewardButtonText;
    private bool dailyRewardPanelSceneOwned;

    private GameObject missionSummaryPanel;
    private TextMeshProUGUI missionSummaryTitleText;
    private TextMeshProUGUI missionSummaryStatusText;
    private Button missionSummaryOpenButton;
    private TextMeshProUGUI missionSummaryOpenButtonText;
    private bool missionSummaryPanelSceneOwned;

    private GameObject challengeSummaryPanel;
    private TextMeshProUGUI challengeSummaryTitleText;
    private TextMeshProUGUI challengeSummaryBodyText;
    private TextMeshProUGUI challengeSummaryStatusText;
    private Button challengeSummaryActionButton;
    private TextMeshProUGUI challengeSummaryActionButtonText;
    private bool challengeSummaryPanelSceneOwned;

    private GameObject missionOverlayRoot;
    private GameObject missionOverlayPanel;
    private TextMeshProUGUI missionOverlayTitleText;
    private readonly Image[] missionCardImages = new Image[3];
    private readonly TextMeshProUGUI[] missionCardTitleTexts = new TextMeshProUGUI[3];
    private readonly TextMeshProUGUI[] missionCardProgressTexts = new TextMeshProUGUI[3];
    private readonly TextMeshProUGUI[] missionCardRewardTexts = new TextMeshProUGUI[3];
    private TextMeshProUGUI missionOverlayStatusText;
    private Button claimMissionButton;
    private TextMeshProUGUI claimMissionButtonText;
    private Button closeMissionButton;
    private TextMeshProUGUI closeMissionButtonText;
    private string missionClaimFeedback = string.Empty;

    private const string ProfileStatsObjectName = "ProfileStatsText";
    private const string DailyRewardPanelObjectName = "DailyRewardPanel";
    private const string ChallengeSummaryPanelObjectName = "DailyChallengeSummaryPanel";
    private const string MissionSummaryPanelObjectName = "DailyMissionSummaryPanel";
    private const string MissionOverlayRootObjectName = "DailyMissionOverlayRoot";

    void Start()
    {
        if (SceneManager.GetActiveScene().name != ExpectedSceneName)
        {
            enabled = false;
            return;
        }

        DailyRewardSystem.GetPreviewReward();
        DailyMissionSystem.EnsureInitializedForToday();
        DailyChallengeSystem.EnsureInitializedForToday();

        runtimeFont = ResolveRuntimeFont();
        menuRootRect = GetMenuRoot();

        if (createInventoryButtonAtRuntime)
            EnsureInventoryButtonExists();

        EnsureProfileStatsLabel();

        if (createDailyRewardPanelAtRuntime)
            EnsureDailyRewardPanel();

        EnsureChallengeSummaryPanel();
        EnsureMissionSummaryPanel();
        EnsureMissionOverlayPanel();
        NormalizeMenuLayout();
        RefreshMenuHud();
        SetMissionOverlayVisible(false);
    }

    public void PlayGame()
    {
        DailyChallengeSystem.ClearActiveRun();
        SceneManager.LoadScene(gameSceneName);
    }

    public void OpenShop()
    {
        SceneManager.LoadScene(shopSceneName);
    }

    public void OpenInventory()
    {
        SceneManager.LoadScene(inventorySceneName);
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit Game");
    }

    public void ClaimDailyReward()
    {
        if (DailyRewardSystem.TryClaimReward(out DailyRewardPackage rewardPackage))
        {
            GameSettings.TriggerHaptic();
            RefreshMenuHud();

            if (dailyRewardStatusText != null)
                dailyRewardStatusText.text = "Claimed today";
        }
        else
        {
            RefreshDailyRewardPanel();
        }
    }

    public void HandleDailyChallengeAction()
    {
        DailyChallengeData challenge = DailyChallengeSystem.GetTodayChallenge();

        if (challenge.rewardClaimed)
        {
            RefreshMenuHud();
            return;
        }

        if (DailyChallengeSystem.TryClaimReward(out int coinsGranted, out UpgradeType rewardUpgrade, out int rewardAmount))
        {
            GameSettings.TriggerHaptic();
            RefreshMenuHud();

            if (challengeSummaryStatusText != null)
            {
                challengeSummaryStatusText.text = "Reward claimed for today";
            }

            return;
        }

        DailyChallengeSystem.BeginTodayChallengeRun();
        SceneManager.LoadScene(gameSceneName);
    }

    public void OpenMissionOverlay()
    {
        RefreshDailyMissionPanel();
        SetMissionOverlayVisible(true);
    }

    public void CloseMissionOverlay()
    {
        SetMissionOverlayVisible(false);
    }

    public void ClaimMissionRewards()
    {
        if (DailyMissionSystem.ClaimCompletedRewards(out int coinsGranted, out List<string> upgradesGranted))
        {
            GameSettings.TriggerHaptic();
            missionClaimFeedback = "Claimed " + coinsGranted + " coins";

            if (upgradesGranted.Count > 0)
                missionClaimFeedback += " + " + upgradesGranted.Count + " buff" + (upgradesGranted.Count == 1 ? "" : "s");

            RefreshMenuHud();
        }
        else
        {
            RefreshDailyMissionPanel();
        }
    }

    void EnsureInventoryButtonExists()
    {
        if (GameObject.Find(inventoryButtonObjectName) != null)
            return;

        Button shopButton = FindButton(shopButtonObjectName);

        if (shopButton == null)
            return;

        Button inventoryButton = Instantiate(shopButton, shopButton.transform.parent);
        inventoryButton.gameObject.name = inventoryButtonObjectName;

        RectTransform inventoryRect = inventoryButton.GetComponent<RectTransform>();
        RectTransform shopRect = shopButton.GetComponent<RectTransform>();

        if (inventoryRect != null && shopRect != null)
        {
            inventoryRect.anchoredPosition = new Vector2(
                shopRect.anchoredPosition.x,
                shopRect.anchoredPosition.y - menuButtonSpacing);
        }

        Button exitButton = FindButton(exitButtonObjectName);

        if (exitButton != null)
        {
            RectTransform exitRect = exitButton.GetComponent<RectTransform>();

            if (exitRect != null && inventoryRect != null)
            {
                exitRect.anchoredPosition = new Vector2(
                    exitRect.anchoredPosition.x,
                    inventoryRect.anchoredPosition.y - menuButtonSpacing);
            }
        }

        TMP_Text label = inventoryButton.GetComponentInChildren<TMP_Text>(true);

        if (label != null)
            label.text = "Inventory";

        inventoryButton.onClick.RemoveAllListeners();
        inventoryButton.onClick.AddListener(OpenInventory);
    }

    void EnsureProfileStatsLabel()
    {
        if (menuRootRect == null)
            return;

        Transform existing = menuRootRect.Find(ProfileStatsObjectName);

        if (existing != null)
        {
            profileStatsSceneOwned = true;
            profileStatsText = existing.GetComponent<TextMeshProUGUI>();
        }
        else
        {
            profileStatsSceneOwned = false;
            GameObject statsObject = new GameObject(ProfileStatsObjectName, typeof(RectTransform));
            statsObject.transform.SetParent(menuRootRect, false);
            profileStatsText = statsObject.AddComponent<TextMeshProUGUI>();
        }

        if (!profileStatsSceneOwned)
        {
            RectTransform statsRect = profileStatsText.rectTransform;
            statsRect.anchorMin = new Vector2(0.5f, 1f);
            statsRect.anchorMax = new Vector2(0.5f, 1f);
            statsRect.pivot = new Vector2(0.5f, 1f);
            statsRect.sizeDelta = new Vector2(760f, 60f);
            statsRect.anchoredPosition = new Vector2(0f, -340f);
        }

        profileStatsText.alignment = TextAlignmentOptions.Center;
        profileStatsText.enableAutoSizing = true;
        profileStatsText.fontSizeMin = 26;
        profileStatsText.fontSizeMax = 36;
        profileStatsText.color = new Color(0.92f, 0.96f, 1f, 1f);

        if (runtimeFont != null)
            profileStatsText.font = runtimeFont;
    }

    void EnsureDailyRewardPanel()
    {
        if (menuRootRect == null)
            return;

        Transform existing = menuRootRect.Find(DailyRewardPanelObjectName);

        if (existing != null)
        {
            dailyRewardPanelSceneOwned = true;
            dailyRewardPanel = existing.gameObject;
            CacheDailyRewardPanelReferences();
            return;
        }

        dailyRewardPanelSceneOwned = false;
        dailyRewardPanel = new GameObject(DailyRewardPanelObjectName, typeof(RectTransform), typeof(Image));
        dailyRewardPanel.transform.SetParent(menuRootRect, false);

        RectTransform panelRect = dailyRewardPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 1f);
        panelRect.anchorMax = new Vector2(0.5f, 1f);
        panelRect.pivot = new Vector2(0.5f, 1f);
        panelRect.sizeDelta = new Vector2(760f, 300f);
        panelRect.anchoredPosition = new Vector2(0f, -440f);

        Image panelImage = dailyRewardPanel.GetComponent<Image>();
        panelImage.color = new Color(0.1f, 0.18f, 0.32f, 0.96f);

        dailyRewardTitleText = CreatePanelText(
            dailyRewardPanel.transform,
            "DailyRewardTitle",
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(680f, 44f),
            new Vector2(0f, -22f),
            TextAlignmentOptions.Center,
            28f,
            38f,
            new Color(1f, 0.94f, 0.72f, 1f));

        dailyRewardBodyText = CreatePanelText(
            dailyRewardPanel.transform,
            "DailyRewardBody",
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(680f, 68f),
            new Vector2(0f, -102f),
            TextAlignmentOptions.Center,
            20f,
            28f,
            new Color(0.94f, 0.97f, 1f, 1f));

        dailyRewardStatusText = CreatePanelText(
            dailyRewardPanel.transform,
            "DailyRewardStatus",
            new Vector2(0.5f, 0f),
            new Vector2(0.5f, 0f),
            new Vector2(0.5f, 0f),
            new Vector2(680f, 28f),
            new Vector2(0f, 102f),
            TextAlignmentOptions.Center,
            17f,
            24f,
            new Color(0.79f, 0.88f, 1f, 1f));

        claimRewardButton = CreatePanelButton(
            dailyRewardPanel.transform,
            "ClaimRewardButton",
            new Vector2(0.5f, 0f),
            new Vector2(0.5f, 0f),
            new Vector2(0.5f, 0f),
            new Vector2(340f, 66f),
            new Vector2(0f, 18f),
            "Claim Reward",
            out claimRewardButtonText);

        if (claimRewardButton != null)
        {
            claimRewardButton.onClick.RemoveAllListeners();
            claimRewardButton.onClick.AddListener(ClaimDailyReward);
        }
    }

    void EnsureChallengeSummaryPanel()
    {
        if (menuRootRect == null)
            return;

        Transform existing = menuRootRect.Find(ChallengeSummaryPanelObjectName);

        if (existing != null)
        {
            challengeSummaryPanelSceneOwned = true;
            challengeSummaryPanel = existing.gameObject;
            CacheChallengeSummaryReferences();
            return;
        }

        challengeSummaryPanelSceneOwned = false;
        challengeSummaryPanel = new GameObject(ChallengeSummaryPanelObjectName, typeof(RectTransform), typeof(Image));
        challengeSummaryPanel.transform.SetParent(menuRootRect, false);

        RectTransform panelRect = challengeSummaryPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0f);
        panelRect.anchorMax = new Vector2(0.5f, 0f);
        panelRect.pivot = new Vector2(0.5f, 0f);
        panelRect.sizeDelta = new Vector2(760f, 220f);
        panelRect.anchoredPosition = new Vector2(0f, 244f);

        Image panelImage = challengeSummaryPanel.GetComponent<Image>();
        panelImage.color = new Color(0.1f, 0.19f, 0.34f, 0.97f);

        challengeSummaryTitleText = CreatePanelText(
            challengeSummaryPanel.transform,
            "ChallengeSummaryTitle",
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(680f, 40f),
            new Vector2(0f, -18f),
            TextAlignmentOptions.Center,
            24f,
            34f,
            new Color(1f, 0.94f, 0.72f, 1f));

        challengeSummaryBodyText = CreatePanelText(
            challengeSummaryPanel.transform,
            "ChallengeSummaryBody",
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(680f, 64f),
            new Vector2(0f, -78f),
            TextAlignmentOptions.Center,
            20f,
            28f,
            new Color(0.94f, 0.97f, 1f, 1f));

        challengeSummaryStatusText = CreatePanelText(
            challengeSummaryPanel.transform,
            "ChallengeSummaryStatus",
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(680f, 34f),
            new Vector2(0f, -138f),
            TextAlignmentOptions.Center,
            18f,
            24f,
            new Color(0.86f, 0.93f, 1f, 1f));

        challengeSummaryActionButton = CreatePanelButton(
            challengeSummaryPanel.transform,
            "ChallengeSummaryActionButton",
            new Vector2(0.5f, 0f),
            new Vector2(0.5f, 0f),
            new Vector2(0.5f, 0f),
            new Vector2(420f, 60f),
            new Vector2(0f, 18f),
            "Play Challenge",
            out challengeSummaryActionButtonText);

        if (challengeSummaryActionButton != null)
        {
            challengeSummaryActionButton.onClick.RemoveAllListeners();
            challengeSummaryActionButton.onClick.AddListener(HandleDailyChallengeAction);
        }
    }

    void EnsureMissionSummaryPanel()
    {
        if (menuRootRect == null)
            return;

        Transform existing = menuRootRect.Find(MissionSummaryPanelObjectName);

        if (existing != null)
        {
            missionSummaryPanelSceneOwned = true;
            missionSummaryPanel = existing.gameObject;
            CacheMissionSummaryReferences();
            return;
        }

        missionSummaryPanelSceneOwned = false;
        missionSummaryPanel = new GameObject(MissionSummaryPanelObjectName, typeof(RectTransform), typeof(Image));
        missionSummaryPanel.transform.SetParent(menuRootRect, false);

        RectTransform panelRect = missionSummaryPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0f);
        panelRect.anchorMax = new Vector2(0.5f, 0f);
        panelRect.pivot = new Vector2(0.5f, 0f);
        panelRect.sizeDelta = new Vector2(760f, 200f);
        panelRect.anchoredPosition = new Vector2(0f, 18f);

        Image panelImage = missionSummaryPanel.GetComponent<Image>();
        panelImage.color = new Color(0.09f, 0.16f, 0.28f, 0.96f);

        missionSummaryTitleText = CreatePanelText(
            missionSummaryPanel.transform,
            "MissionSummaryTitle",
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(680f, 40f),
            new Vector2(0f, -20f),
            TextAlignmentOptions.Center,
            24f,
            34f,
            new Color(1f, 0.94f, 0.72f, 1f));

        missionSummaryStatusText = CreatePanelText(
            missionSummaryPanel.transform,
            "MissionSummaryStatus",
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(680f, 54f),
            new Vector2(0f, -80f),
            TextAlignmentOptions.Center,
            20f,
            28f,
            new Color(0.93f, 0.97f, 1f, 1f));

        missionSummaryOpenButton = CreatePanelButton(
            missionSummaryPanel.transform,
            "MissionSummaryOpenButton",
            new Vector2(0.5f, 0f),
            new Vector2(0.5f, 0f),
            new Vector2(0.5f, 0f),
            new Vector2(360f, 60f),
            new Vector2(0f, 20f),
            "View Missions",
            out missionSummaryOpenButtonText);

        if (missionSummaryOpenButton != null)
        {
            missionSummaryOpenButton.onClick.RemoveAllListeners();
            missionSummaryOpenButton.onClick.AddListener(OpenMissionOverlay);
        }
    }

    void EnsureMissionOverlayPanel()
    {
        if (menuRootRect == null)
            return;

        Transform existing = menuRootRect.Find(MissionOverlayRootObjectName);

        if (existing != null)
        {
            missionOverlayRoot = existing.gameObject;
            CacheMissionOverlayReferences();
            return;
        }

        missionOverlayRoot = new GameObject(MissionOverlayRootObjectName, typeof(RectTransform), typeof(Image));
        missionOverlayRoot.transform.SetParent(menuRootRect, false);

        RectTransform rootRect = missionOverlayRoot.GetComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;

        Image rootImage = missionOverlayRoot.GetComponent<Image>();
        rootImage.color = new Color(0.02f, 0.04f, 0.08f, 0.86f);

        missionOverlayPanel = new GameObject("MissionOverlayPanel", typeof(RectTransform), typeof(Image));
        missionOverlayPanel.transform.SetParent(missionOverlayRoot.transform, false);

        RectTransform panelRect = missionOverlayPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(860f, 980f);
        panelRect.anchoredPosition = new Vector2(0f, -20f);

        Image panelImage = missionOverlayPanel.GetComponent<Image>();
        panelImage.color = new Color(0.11f, 0.18f, 0.31f, 1f);

        missionOverlayTitleText = CreatePanelText(
            missionOverlayPanel.transform,
            "MissionOverlayTitle",
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(720f, 54f),
            new Vector2(0f, -28f),
            TextAlignmentOptions.Center,
            34f,
            44f,
            Color.white);

        for (int i = 0; i < 3; i++)
            CreateMissionCard(i);

        missionOverlayStatusText = CreatePanelText(
            missionOverlayPanel.transform,
            "MissionOverlayStatus",
            new Vector2(0.5f, 0f),
            new Vector2(0.5f, 0f),
            new Vector2(0.5f, 0f),
            new Vector2(720f, 40f),
            new Vector2(0f, 132f),
            TextAlignmentOptions.Center,
            22f,
            30f,
            new Color(0.84f, 0.92f, 1f, 1f));

        claimMissionButton = CreatePanelButton(
            missionOverlayPanel.transform,
            "ClaimMissionButton",
            new Vector2(0.5f, 0f),
            new Vector2(0.5f, 0f),
            new Vector2(1f, 0f),
            new Vector2(320f, 72f),
            new Vector2(-20f, 34f),
            "Claim Rewards",
            out claimMissionButtonText);

        if (claimMissionButton != null)
        {
            claimMissionButton.onClick.RemoveAllListeners();
            claimMissionButton.onClick.AddListener(ClaimMissionRewards);
        }

        closeMissionButton = CreatePanelButton(
            missionOverlayPanel.transform,
            "CloseMissionButton",
            new Vector2(0.5f, 0f),
            new Vector2(0.5f, 0f),
            new Vector2(0f, 0f),
            new Vector2(220f, 72f),
            new Vector2(20f, 34f),
            "Close",
            out closeMissionButtonText);

        if (closeMissionButton != null)
        {
            closeMissionButton.onClick.RemoveAllListeners();
            closeMissionButton.onClick.AddListener(CloseMissionOverlay);

            Image closeImage = closeMissionButton.GetComponent<Image>();

            if (closeImage != null)
                closeImage.color = new Color(0.32f, 0.42f, 0.56f, 1f);
        }
    }

    void CreateMissionCard(int index)
    {
        GameObject cardObject = new GameObject("MissionCard" + index, typeof(RectTransform), typeof(Image));
        cardObject.transform.SetParent(missionOverlayPanel.transform, false);

        RectTransform cardRect = cardObject.GetComponent<RectTransform>();
        cardRect.anchorMin = new Vector2(0.5f, 1f);
        cardRect.anchorMax = new Vector2(0.5f, 1f);
        cardRect.pivot = new Vector2(0.5f, 1f);
        cardRect.sizeDelta = new Vector2(760f, 170f);
        cardRect.anchoredPosition = new Vector2(0f, -116f - (index * 186f));

        Image cardImage = cardObject.GetComponent<Image>();
        cardImage.color = new Color(0.18f, 0.27f, 0.43f, 1f);
        missionCardImages[index] = cardImage;

        missionCardTitleTexts[index] = CreatePanelText(
            cardObject.transform,
            "Title",
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(680f, 48f),
            new Vector2(0f, -20f),
            TextAlignmentOptions.Center,
            26f,
            34f,
            Color.white);

        missionCardProgressTexts[index] = CreatePanelText(
            cardObject.transform,
            "Progress",
            new Vector2(0f, 0f),
            new Vector2(0f, 0f),
            new Vector2(0f, 0f),
            new Vector2(330f, 36f),
            new Vector2(28f, 28f),
            TextAlignmentOptions.Left,
            22f,
            28f,
            new Color(0.9f, 0.96f, 1f, 1f));

        missionCardRewardTexts[index] = CreatePanelText(
            cardObject.transform,
            "Reward",
            new Vector2(1f, 0f),
            new Vector2(1f, 0f),
            new Vector2(1f, 0f),
            new Vector2(330f, 36f),
            new Vector2(-28f, 28f),
            TextAlignmentOptions.Right,
            22f,
            28f,
            new Color(1f, 0.9f, 0.7f, 1f));
    }

    void CacheDailyRewardPanelReferences()
    {
        if (dailyRewardPanel == null)
            return;

        dailyRewardTitleText = FindTextInParent(dailyRewardPanel.transform, "DailyRewardTitle");
        dailyRewardBodyText = FindTextInParent(dailyRewardPanel.transform, "DailyRewardBody");
        dailyRewardStatusText = FindTextInParent(dailyRewardPanel.transform, "DailyRewardStatus");

        Transform buttonTransform = dailyRewardPanel.transform.Find("ClaimRewardButton");

        if (buttonTransform != null)
        {
            claimRewardButton = buttonTransform.GetComponent<Button>();
            claimRewardButtonText = buttonTransform.GetComponentInChildren<TextMeshProUGUI>(true);
        }
    }

    void CacheChallengeSummaryReferences()
    {
        if (challengeSummaryPanel == null)
            return;

        challengeSummaryTitleText = FindTextInParent(challengeSummaryPanel.transform, "ChallengeSummaryTitle");
        challengeSummaryBodyText = FindTextInParent(challengeSummaryPanel.transform, "ChallengeSummaryBody");
        challengeSummaryStatusText = FindTextInParent(challengeSummaryPanel.transform, "ChallengeSummaryStatus");

        Transform buttonTransform = challengeSummaryPanel.transform.Find("ChallengeSummaryActionButton");

        if (buttonTransform != null)
        {
            challengeSummaryActionButton = buttonTransform.GetComponent<Button>();
            challengeSummaryActionButtonText = buttonTransform.GetComponentInChildren<TextMeshProUGUI>(true);
        }
    }

    void CacheMissionSummaryReferences()
    {
        if (missionSummaryPanel == null)
            return;

        missionSummaryTitleText = FindTextInParent(missionSummaryPanel.transform, "MissionSummaryTitle");
        missionSummaryStatusText = FindTextInParent(missionSummaryPanel.transform, "MissionSummaryStatus");

        Transform buttonTransform = missionSummaryPanel.transform.Find("MissionSummaryOpenButton");

        if (buttonTransform != null)
        {
            missionSummaryOpenButton = buttonTransform.GetComponent<Button>();
            missionSummaryOpenButtonText = buttonTransform.GetComponentInChildren<TextMeshProUGUI>(true);
        }
    }

    void CacheMissionOverlayReferences()
    {
        if (missionOverlayRoot == null)
            return;

        Transform panelTransform = missionOverlayRoot.transform.Find("MissionOverlayPanel");

        if (panelTransform != null)
            missionOverlayPanel = panelTransform.gameObject;

        if (missionOverlayPanel == null)
            return;

        missionOverlayTitleText = FindTextInParent(missionOverlayPanel.transform, "MissionOverlayTitle");
        missionOverlayStatusText = FindTextInParent(missionOverlayPanel.transform, "MissionOverlayStatus");

        for (int i = 0; i < 3; i++)
        {
            Transform cardTransform = missionOverlayPanel.transform.Find("MissionCard" + i);

            if (cardTransform == null)
                continue;

            missionCardImages[i] = cardTransform.GetComponent<Image>();
            missionCardTitleTexts[i] = FindTextInParent(cardTransform, "Title");
            missionCardProgressTexts[i] = FindTextInParent(cardTransform, "Progress");
            missionCardRewardTexts[i] = FindTextInParent(cardTransform, "Reward");
        }

        Transform claimTransform = missionOverlayPanel.transform.Find("ClaimMissionButton");

        if (claimTransform != null)
        {
            claimMissionButton = claimTransform.GetComponent<Button>();
            claimMissionButtonText = claimTransform.GetComponentInChildren<TextMeshProUGUI>(true);
        }

        Transform closeTransform = missionOverlayPanel.transform.Find("CloseMissionButton");

        if (closeTransform != null)
        {
            closeMissionButton = closeTransform.GetComponent<Button>();
            closeMissionButtonText = closeTransform.GetComponentInChildren<TextMeshProUGUI>(true);
        }
    }

    TextMeshProUGUI FindTextInParent(Transform parent, string objectName)
    {
        if (parent == null)
            return null;

        Transform textTransform = parent.Find(objectName);

        if (textTransform == null)
            return null;

        return textTransform.GetComponent<TextMeshProUGUI>();
    }

    TextMeshProUGUI CreatePanelText(
        Transform parent,
        string objectName,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 pivot,
        Vector2 size,
        Vector2 anchoredPosition,
        TextAlignmentOptions alignment,
        float minSize,
        float maxSize,
        Color color)
    {
        GameObject textObject = new GameObject(objectName, typeof(RectTransform));
        textObject.transform.SetParent(parent, false);

        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = anchorMin;
        textRect.anchorMax = anchorMax;
        textRect.pivot = pivot;
        textRect.sizeDelta = size;
        textRect.anchoredPosition = anchoredPosition;

        TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
        text.alignment = alignment;
        text.enableAutoSizing = true;
        text.fontSizeMin = minSize;
        text.fontSizeMax = maxSize;
        text.color = color;
        text.lineSpacing = 0f;

        if (runtimeFont != null)
            text.font = runtimeFont;

        return text;
    }

    Button CreatePanelButton(
        Transform parent,
        string objectName,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 pivot,
        Vector2 size,
        Vector2 anchoredPosition,
        string labelText,
        out TextMeshProUGUI label)
    {
        GameObject buttonObject = new GameObject(objectName, typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);

        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        buttonRect.anchorMin = anchorMin;
        buttonRect.anchorMax = anchorMax;
        buttonRect.pivot = pivot;
        buttonRect.sizeDelta = size;
        buttonRect.anchoredPosition = anchoredPosition;

        Image buttonImage = buttonObject.GetComponent<Image>();
        buttonImage.color = new Color(0.93f, 0.73f, 0.24f, 1f);

        GameObject labelObject = new GameObject("Label", typeof(RectTransform));
        labelObject.transform.SetParent(buttonObject.transform, false);

        RectTransform labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;

        label = labelObject.AddComponent<TextMeshProUGUI>();
        label.text = labelText;
        label.alignment = TextAlignmentOptions.Center;
        label.enableAutoSizing = true;
        label.fontSizeMin = 20;
        label.fontSizeMax = 28;
        label.color = new Color(0.15f, 0.18f, 0.22f, 1f);

        if (runtimeFont != null)
            label.font = runtimeFont;

        return buttonObject.GetComponent<Button>();
    }

    void RefreshMenuHud()
    {
        RefreshProfileStats();
        RefreshDailyRewardPanel();
        RefreshDailyChallengePanel();
        RefreshMissionSummaryPanel();
        RefreshDailyMissionPanel();
    }

    void RefreshProfileStats()
    {
        if (profileStatsText == null)
            return;

        int totalCoins = PlayerPrefs.GetInt("TotalCoins", 0);
        int bestScore = PlayerPrefs.GetInt("BestScore", 0);
        int streak = DailyRewardSystem.GetCurrentStreak();

        profileStatsText.text =
            "Coins: " + totalCoins +
            "   |   Best: " + bestScore +
            "   |   Streak: " + streak;
    }

    void RefreshDailyRewardPanel()
    {
        if (dailyRewardPanel == null)
            return;

        DailyRewardPackage rewardPackage = DailyRewardSystem.GetPreviewReward();
        bool canClaimToday = DailyRewardSystem.CanClaimToday();
        int currentStreak = DailyRewardSystem.GetCurrentStreak();
        Image panelImage = dailyRewardPanel.GetComponent<Image>();
        Image rewardButtonImage = claimRewardButton != null ? claimRewardButton.GetComponent<Image>() : null;

        if (panelImage != null)
        {
            panelImage.color = canClaimToday
                ? new Color(0.1f, 0.18f, 0.32f, 0.96f)
                : new Color(0.1f, 0.18f, 0.32f, 0.56f);
        }

        if (dailyRewardTitleText != null)
        {
            dailyRewardTitleText.text = canClaimToday ? "Reward Ready" : "Reward Claimed";
            dailyRewardTitleText.color = canClaimToday
                ? new Color(1f, 0.94f, 0.72f, 1f)
                : new Color(0.86f, 0.92f, 1f, 0.9f);
        }

        if (dailyRewardBodyText != null)
        {
            dailyRewardBodyText.text = canClaimToday
                ? rewardPackage.coins + " Coins" +
                  "\n+ " + rewardPackage.bonusAmount + " " +
                  UpgradeInventory.GetDisplayName(rewardPackage.bonusUpgrade)
                : "Come back tomorrow";

            dailyRewardBodyText.color = canClaimToday
                ? new Color(0.94f, 0.97f, 1f, 1f)
                : new Color(0.9f, 0.95f, 1f, 0.84f);
        }

        if (dailyRewardStatusText != null)
        {
            dailyRewardStatusText.text = canClaimToday
                ? "Day " + rewardPackage.rewardDay + "   |   Streak " + currentStreak
                : "Next " + DailyRewardSystem.GetNextClaimCountdownText();

            dailyRewardStatusText.color = canClaimToday
                ? new Color(0.9f, 0.96f, 1f, 1f)
                : new Color(0.84f, 0.9f, 0.98f, 0.8f);
        }

        if (claimRewardButton != null)
            claimRewardButton.interactable = canClaimToday;

        if (claimRewardButtonText != null)
        {
            claimRewardButtonText.text = canClaimToday ? "Claim Reward" : "Come Back";
            claimRewardButtonText.color = canClaimToday
                ? new Color(0.15f, 0.18f, 0.22f, 1f)
                : new Color(0.2f, 0.24f, 0.32f, 0.76f);
        }

        if (rewardButtonImage != null)
        {
            rewardButtonImage.color = canClaimToday
                ? new Color(0.93f, 0.73f, 0.24f, 1f)
                : new Color(0.7f, 0.58f, 0.33f, 0.6f);
        }
    }

    void RefreshMissionSummaryPanel()
    {
        if (missionSummaryPanel == null)
            return;

        int completedCount = DailyMissionSystem.GetCompletedCount();
        int claimableCount = DailyMissionSystem.GetClaimableCount();

        if (missionSummaryTitleText != null)
            missionSummaryTitleText.text = "Daily Missions";

        if (missionSummaryStatusText != null)
        {
            missionSummaryStatusText.text = claimableCount > 0
                ? claimableCount + (claimableCount == 1 ? " reward ready" : " rewards ready")
                : completedCount + "/3 complete today";
        }

        if (missionSummaryOpenButtonText != null)
            missionSummaryOpenButtonText.text = claimableCount > 0 ? "Open + Claim" : "Open Missions";
    }

    void RefreshDailyChallengePanel()
    {
        if (challengeSummaryPanel == null)
            return;

        DailyChallengeData challenge = DailyChallengeSystem.GetTodayChallenge();
        bool canClaimReward = DailyChallengeSystem.CanClaimReward();
        bool rewardClaimed = challenge.rewardClaimed;
        Image panelImage = challengeSummaryPanel.GetComponent<Image>();
        Image buttonImage = challengeSummaryActionButton != null ? challengeSummaryActionButton.GetComponent<Image>() : null;

        if (panelImage != null)
        {
            panelImage.color = rewardClaimed
                ? new Color(0.14f, 0.2f, 0.29f, 0.82f)
                : canClaimReward
                    ? new Color(0.15f, 0.3f, 0.23f, 0.97f)
                    : new Color(0.1f, 0.19f, 0.34f, 0.97f);
        }

        if (challengeSummaryTitleText != null)
            challengeSummaryTitleText.text = "Daily Challenge";

        if (challengeSummaryBodyText != null)
        {
            challengeSummaryBodyText.text = challenge.title;
        }

        if (challengeSummaryStatusText != null)
        {
            if (rewardClaimed)
            {
                challengeSummaryStatusText.text = "Completed today";
            }
            else if (canClaimReward)
            {
                challengeSummaryStatusText.text = "Reward: " + DailyChallengeSystem.GetRewardLabel(challenge);
            }
            else
            {
                challengeSummaryStatusText.text = DailyChallengeSystem.GetObjectiveLabel(challenge);
            }
        }

        if (challengeSummaryActionButton != null)
            challengeSummaryActionButton.interactable = !rewardClaimed;

        if (challengeSummaryActionButtonText != null)
        {
            challengeSummaryActionButtonText.text = rewardClaimed
                ? "Done Today"
                : canClaimReward
                    ? "Claim Reward"
                    : "Play Challenge";
        }

        if (buttonImage != null)
        {
            buttonImage.color = rewardClaimed
                ? new Color(0.42f, 0.46f, 0.5f, 0.72f)
                : canClaimReward
                    ? new Color(0.3f, 0.75f, 0.46f, 1f)
                    : new Color(0.93f, 0.73f, 0.24f, 1f);
        }
    }

    void RefreshDailyMissionPanel()
    {
        if (missionOverlayPanel == null)
            return;

        DailyMissionData[] missions = DailyMissionSystem.GetMissions();
        int claimableCount = DailyMissionSystem.GetClaimableCount();
        int completedCount = DailyMissionSystem.GetCompletedCount();

        if (missionOverlayTitleText != null)
            missionOverlayTitleText.text = "Daily Missions";

        for (int i = 0; i < 3; i++)
        {
            if (i >= missions.Length)
                continue;

            DailyMissionData mission = missions[i];
            Color cardColor;
            string progressLabel;

            if (mission.claimed)
            {
                cardColor = new Color(0.26f, 0.33f, 0.43f, 1f);
                progressLabel = "Claimed";
            }
            else if (mission.progress >= mission.target)
            {
                cardColor = new Color(0.2f, 0.42f, 0.28f, 1f);
                progressLabel = "Ready to claim";
            }
            else
            {
                cardColor = new Color(0.18f, 0.27f, 0.43f, 1f);
                progressLabel = DailyMissionSystem.GetMissionProgressLabel(mission);
            }

            if (missionCardImages[i] != null)
                missionCardImages[i].color = cardColor;

            if (missionCardTitleTexts[i] != null)
                missionCardTitleTexts[i].text = DailyMissionSystem.GetMissionSummary(mission);

            if (missionCardProgressTexts[i] != null)
                missionCardProgressTexts[i].text = progressLabel;

            if (missionCardRewardTexts[i] != null)
                missionCardRewardTexts[i].text = "Reward: " + DailyMissionSystem.GetMissionRewardLabel(mission);
        }

        if (missionOverlayStatusText != null)
        {
            if (!string.IsNullOrEmpty(missionClaimFeedback) && claimableCount == 0)
            {
                missionOverlayStatusText.text = missionClaimFeedback;
            }
            else if (claimableCount > 0)
            {
                missionOverlayStatusText.text =
                    claimableCount +
                    (claimableCount == 1 ? " reward ready to claim" : " rewards ready to claim");
            }
            else
            {
                missionOverlayStatusText.text = completedCount + "/3 completed today";
            }
        }

        if (claimMissionButton != null)
            claimMissionButton.interactable = claimableCount > 0;

        if (claimMissionButtonText != null)
            claimMissionButtonText.text = claimableCount > 0 ? "Claim Rewards" : "No Rewards Yet";

        if (closeMissionButtonText != null)
            closeMissionButtonText.text = "Close";
    }

    void SetMissionOverlayVisible(bool isVisible)
    {
        if (missionOverlayRoot != null)
            missionOverlayRoot.SetActive(isVisible);
    }

    TMP_FontAsset ResolveRuntimeFont()
    {
        TMP_Text titleText = FindText(titleObjectName);

        if (titleText != null)
            return titleText.font;

        Button playButton = FindButton(playButtonObjectName);

        if (playButton != null)
        {
            TMP_Text label = playButton.GetComponentInChildren<TMP_Text>(true);

            if (label != null)
                return label.font;
        }

        return null;
    }

    void NormalizeMenuLayout()
    {
        NormalizeDailyRewardPanel();
        NormalizeChallengeSummaryPanel();
        NormalizeMissionSummaryPanel();
        NormalizeMissionOverlayPanel();
        NormalizeButtons();
    }

    void NormalizeDailyRewardPanel()
    {
        if (dailyRewardPanel == null)
            return;

        if (dailyRewardPanelSceneOwned)
            return;

        RectTransform panelRect = dailyRewardPanel.GetComponent<RectTransform>();

        if (panelRect == null)
            return;

        panelRect.anchorMin = new Vector2(0.5f, 1f);
        panelRect.anchorMax = new Vector2(0.5f, 1f);
        panelRect.pivot = new Vector2(0.5f, 1f);
        panelRect.sizeDelta = new Vector2(760f, 260f);
        panelRect.anchoredPosition = new Vector2(0f, -440f);

        if (dailyRewardTitleText != null)
        {
            RectTransform titleRect = dailyRewardTitleText.rectTransform;
            titleRect.sizeDelta = new Vector2(680f, 42f);
            titleRect.anchoredPosition = new Vector2(0f, -18f);
            dailyRewardTitleText.fontSizeMin = 26f;
            dailyRewardTitleText.fontSizeMax = 36f;
        }

        if (dailyRewardBodyText != null)
        {
            RectTransform bodyRect = dailyRewardBodyText.rectTransform;
            bodyRect.sizeDelta = new Vector2(680f, 64f);
            bodyRect.anchoredPosition = new Vector2(0f, -92f);
            dailyRewardBodyText.lineSpacing = -4f;
            dailyRewardBodyText.fontSizeMin = 22f;
            dailyRewardBodyText.fontSizeMax = 32f;
        }

        if (dailyRewardStatusText != null)
        {
            RectTransform statusRect = dailyRewardStatusText.rectTransform;
            statusRect.sizeDelta = new Vector2(680f, 24f);
            statusRect.anchoredPosition = new Vector2(0f, 90f);
            dailyRewardStatusText.fontSizeMin = 20f;
            dailyRewardStatusText.fontSizeMax = 26f;
        }

        if (claimRewardButton != null)
        {
            RectTransform buttonRect = claimRewardButton.GetComponent<RectTransform>();

            if (buttonRect != null)
            {
                buttonRect.sizeDelta = new Vector2(320f, 56f);
                buttonRect.anchoredPosition = new Vector2(0f, 16f);
            }
        }

        if (claimRewardButtonText != null)
        {
            claimRewardButtonText.fontSizeMin = 20f;
            claimRewardButtonText.fontSizeMax = 28f;
        }
    }

    void NormalizeMissionSummaryPanel()
    {
        if (missionSummaryPanel == null)
            return;

        if (missionSummaryPanelSceneOwned)
            return;

        RectTransform panelRect = missionSummaryPanel.GetComponent<RectTransform>();

        if (panelRect == null)
            return;

        panelRect.anchorMin = new Vector2(0.5f, 0f);
        panelRect.anchorMax = new Vector2(0.5f, 0f);
        panelRect.pivot = new Vector2(0.5f, 0f);
        panelRect.sizeDelta = new Vector2(760f, 200f);
        panelRect.anchoredPosition = new Vector2(0f, 18f);

        if (missionSummaryTitleText != null)
        {
            RectTransform titleRect = missionSummaryTitleText.rectTransform;
            titleRect.sizeDelta = new Vector2(680f, 40f);
            titleRect.anchoredPosition = new Vector2(0f, -20f);
        }

        if (missionSummaryStatusText != null)
        {
            RectTransform statusRect = missionSummaryStatusText.rectTransform;
            statusRect.sizeDelta = new Vector2(680f, 40f);
            statusRect.anchoredPosition = new Vector2(0f, -82f);
            missionSummaryStatusText.fontSizeMin = 24f;
            missionSummaryStatusText.fontSizeMax = 32f;
            missionSummaryStatusText.lineSpacing = 0f;
        }

        if (missionSummaryOpenButton != null)
        {
            RectTransform buttonRect = missionSummaryOpenButton.GetComponent<RectTransform>();

            if (buttonRect != null)
            {
                buttonRect.sizeDelta = new Vector2(380f, 64f);
                buttonRect.anchoredPosition = new Vector2(0f, 18f);
            }
        }

        if (missionSummaryOpenButtonText != null)
        {
            missionSummaryOpenButtonText.fontSizeMin = 22f;
            missionSummaryOpenButtonText.fontSizeMax = 30f;
        }
    }

    void NormalizeChallengeSummaryPanel()
    {
        if (challengeSummaryPanel == null)
            return;

        if (challengeSummaryPanelSceneOwned)
            return;

        RectTransform panelRect = challengeSummaryPanel.GetComponent<RectTransform>();

        if (panelRect == null)
            return;

        panelRect.anchorMin = new Vector2(0.5f, 0f);
        panelRect.anchorMax = new Vector2(0.5f, 0f);
        panelRect.pivot = new Vector2(0.5f, 0f);
        panelRect.sizeDelta = new Vector2(760f, 240f);
        panelRect.anchoredPosition = new Vector2(0f, 244f);

        if (challengeSummaryTitleText != null)
        {
            RectTransform titleRect = challengeSummaryTitleText.rectTransform;
            titleRect.sizeDelta = new Vector2(700f, 46f);
            titleRect.anchoredPosition = new Vector2(0f, -18f);
            challengeSummaryTitleText.fontSizeMin = 30f;
            challengeSummaryTitleText.fontSizeMax = 42f;
        }

        if (challengeSummaryBodyText != null)
        {
            RectTransform bodyRect = challengeSummaryBodyText.rectTransform;
            bodyRect.sizeDelta = new Vector2(700f, 50f);
            bodyRect.anchoredPosition = new Vector2(0f, -82f);
            challengeSummaryBodyText.fontSizeMin = 28f;
            challengeSummaryBodyText.fontSizeMax = 40f;
            challengeSummaryBodyText.lineSpacing = 0f;
        }

        if (challengeSummaryStatusText != null)
        {
            RectTransform statusRect = challengeSummaryStatusText.rectTransform;
            statusRect.sizeDelta = new Vector2(700f, 46f);
            statusRect.anchoredPosition = new Vector2(0f, -138f);
            challengeSummaryStatusText.fontSizeMin = 24f;
            challengeSummaryStatusText.fontSizeMax = 34f;
            challengeSummaryStatusText.lineSpacing = 0f;
        }

        if (challengeSummaryActionButton != null)
        {
            RectTransform buttonRect = challengeSummaryActionButton.GetComponent<RectTransform>();

            if (buttonRect != null)
            {
                buttonRect.sizeDelta = new Vector2(420f, 64f);
                buttonRect.anchoredPosition = new Vector2(0f, 20f);
            }
        }

        if (challengeSummaryActionButtonText != null)
        {
            challengeSummaryActionButtonText.fontSizeMin = 24f;
            challengeSummaryActionButtonText.fontSizeMax = 34f;
        }
    }

    void NormalizeMissionOverlayPanel()
    {
        if (missionOverlayRoot == null || missionOverlayPanel == null)
            return;

        RectTransform rootRect = missionOverlayRoot.GetComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;

        RectTransform panelRect = missionOverlayPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(860f, 980f);
        panelRect.anchoredPosition = new Vector2(0f, -20f);

        if (missionOverlayTitleText != null)
        {
            RectTransform titleRect = missionOverlayTitleText.rectTransform;
            titleRect.sizeDelta = new Vector2(720f, 54f);
            titleRect.anchoredPosition = new Vector2(0f, -28f);
        }

        for (int i = 0; i < 3; i++)
        {
            if (missionCardImages[i] == null)
                continue;

            RectTransform cardRect = missionCardImages[i].rectTransform;
            cardRect.sizeDelta = new Vector2(760f, 170f);
            cardRect.anchoredPosition = new Vector2(0f, -116f - (i * 186f));

            if (missionCardTitleTexts[i] != null)
            {
                RectTransform titleRect = missionCardTitleTexts[i].rectTransform;
                titleRect.sizeDelta = new Vector2(680f, 48f);
                titleRect.anchoredPosition = new Vector2(0f, -20f);
            }

            if (missionCardProgressTexts[i] != null)
            {
                RectTransform progressRect = missionCardProgressTexts[i].rectTransform;
                progressRect.sizeDelta = new Vector2(330f, 36f);
                progressRect.anchoredPosition = new Vector2(28f, 28f);
            }

            if (missionCardRewardTexts[i] != null)
            {
                RectTransform rewardRect = missionCardRewardTexts[i].rectTransform;
                rewardRect.sizeDelta = new Vector2(330f, 36f);
                rewardRect.anchoredPosition = new Vector2(-28f, 28f);
            }
        }

        if (missionOverlayStatusText != null)
        {
            RectTransform statusRect = missionOverlayStatusText.rectTransform;
            statusRect.sizeDelta = new Vector2(720f, 40f);
            statusRect.anchoredPosition = new Vector2(0f, 132f);
        }

        if (claimMissionButton != null)
        {
            RectTransform buttonRect = claimMissionButton.GetComponent<RectTransform>();

            if (buttonRect != null)
            {
                buttonRect.sizeDelta = new Vector2(320f, 72f);
                buttonRect.anchoredPosition = new Vector2(-20f, 34f);
            }
        }

        if (closeMissionButton != null)
        {
            RectTransform buttonRect = closeMissionButton.GetComponent<RectTransform>();

            if (buttonRect != null)
            {
                buttonRect.sizeDelta = new Vector2(220f, 72f);
                buttonRect.anchoredPosition = new Vector2(20f, 34f);
            }
        }
    }

    void NormalizeButtons()
    {
        SetButtonLayout(playButtonObjectName, 50f);
        SetButtonLayout(shopButtonObjectName, -88f);
        SetButtonLayout(inventoryButtonObjectName, -226f);
        SetButtonLayout(exitButtonObjectName, -364f);
    }

    void SetButtonLayout(string objectName, float anchoredY)
    {
        Button button = FindButton(objectName);

        if (button == null)
            return;

        Image buttonImage = button.GetComponent<Image>();

        if (buttonImage != null)
            buttonImage.color = new Color(0.96f, 0.97f, 1f, 0.96f);

        TMP_Text label = button.GetComponentInChildren<TMP_Text>(true);

        if (label != null)
        {
            label.enableAutoSizing = true;
            label.fontSizeMin = 24;
            label.fontSizeMax = 34;
            label.color = new Color(0.18f, 0.22f, 0.3f, 1f);
        }
    }

    TMP_Text FindText(string objectName)
    {
        GameObject textObject = GameObject.Find(objectName);

        if (textObject == null)
            return null;

        return textObject.GetComponent<TMP_Text>();
    }

    RectTransform GetMenuRoot()
    {
        Button playButton = FindButton(playButtonObjectName);

        if (playButton != null && playButton.transform.parent != null)
            return playButton.transform.parent as RectTransform;

        Canvas canvas = FindAnyObjectByType<Canvas>();

        if (canvas != null)
            return canvas.GetComponent<RectTransform>();

        return null;
    }

    Button FindButton(string objectName)
    {
        GameObject buttonObject = GameObject.Find(objectName);

        if (buttonObject == null)
            return null;

        return buttonObject.GetComponent<Button>();
    }
}
```

### Obstacle.cs
```csharp
using UnityEngine;

public class Obstacle : MonoBehaviour
{
    public float baseFallSpeed = 6f;
    public float destroyY = -6f;

    void Update()
    {
        int difficulty = 1;
        float worldSpeedMultiplier = 1f;

        if (GameManager.instance != null)
        {
            difficulty = GameManager.instance.GetDifficultyLevel();
            worldSpeedMultiplier = GameManager.instance.GetWorldSpeedMultiplier();
        }

        float speedMultiplier = 1f + (difficulty * 0.15f);
        float finalSpeed = baseFallSpeed * speedMultiplier;

        transform.Translate(Vector3.down * finalSpeed * worldSpeedMultiplier * Time.deltaTime, Space.World);

        if (transform.position.y < destroyY)
        {
            Destroy(gameObject);
        }
    }
}
```

### ObstacleZigZag.cs
```csharp
using UnityEngine;

public class ObstacleZigZag : MonoBehaviour
{
    public float baseFallSpeed = 5f;
    public float baseHorizontalSpeed = 2f;
    public float frequency = 2f;
    public float destroyY = -6f;

    private float timeOffset;

    void Start()
    {
        // Prevent all zig-zags syncing together
        timeOffset = Random.Range(0f, 10f);
    }

    void Update()
    {
        int difficulty = 1;
        float worldSpeedMultiplier = 1f;

        if (GameManager.instance != null)
        {
            difficulty = GameManager.instance.GetDifficultyLevel();
            worldSpeedMultiplier = GameManager.instance.GetWorldSpeedMultiplier();
        }

        float speedMultiplier = 1f + (difficulty * 0.15f);

        float fallSpeed = baseFallSpeed * speedMultiplier;

        // Limit horizontal scaling so it doesn't become unfair
        float horizontalSpeed = baseHorizontalSpeed * (1f + (difficulty * 0.05f));

        float xOffset = Mathf.Sin((Time.time + timeOffset) * frequency) * horizontalSpeed;

        transform.position = new Vector3(
            transform.position.x + xOffset * worldSpeedMultiplier * Time.deltaTime,
            transform.position.y - fallSpeed * worldSpeedMultiplier * Time.deltaTime,
            0f
        );

        if (transform.position.y < destroyY)
        {
            Destroy(gameObject);
        }
    }
}
```

### PlayerController.cs
```csharp
using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 8f;

    public float minX = -4.15f;
    public float maxX = 4.15f;

    public float minY = -6f;
    public float maxY = 3f;
    public float borderPadding = 0.3f;
    public float cameraSidePadding = 0.15f;
    public float cameraBottomPadding = 0.15f;
    public float cameraTopPadding = 1f;
    public string leftBorderObjectName = "LeftBorder";
    public string rightBorderObjectName = "RightBorder";

    private bool isDead = false;
    private bool isInvulnerable = false;
    private SpriteRenderer spriteRenderer;
    private Vector3 originalScale;
    private Coroutine invulnerabilityRoutine;
    private Rigidbody2D playerBody;
    private Collider2D playerCollider;
    private Camera mainCamera;
    private Transform leftBorderTransform;
    private Transform rightBorderTransform;
    private Vector2 moveInput;
    private float configuredMinX;
    private float configuredMaxX;
    private float configuredMinY;
    private float configuredMaxY;
    private float lastObstacleHitTime = -10f;
    private int cachedScreenWidth;
    private int cachedScreenHeight;

    private const float ObstacleHitCooldown = 0.08f;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalScale = transform.localScale;
        playerBody = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<Collider2D>();
        mainCamera = Camera.main;

        configuredMinX = minX;
        configuredMaxX = maxX;
        configuredMinY = minY;
        configuredMaxY = maxY;
    }

    void Start()
    {
        CacheScreenSize();
        RefreshMovementBounds();
        ClampInsideBoundsImmediate();
    }

    void Update()
    {
        if (DidScreenSizeChange())
        {
            CacheScreenSize();
            RefreshMovementBounds();
            ClampInsideBoundsImmediate();
        }

        if (isDead)
        {
            moveInput = Vector2.zero;
            return;
        }

        moveInput = ReadMovementInput();

        if (playerBody == null)
            ApplyMovement(Time.deltaTime, false);
    }

    void FixedUpdate()
    {
        if (isDead || playerBody == null)
            return;

        ApplyMovement(Time.fixedDeltaTime, true);
    }

    Vector2 ReadMovementInput()
    {
        float moveX = 0f;
        float moveY = 0f;

#if UNITY_EDITOR || UNITY_STANDALONE
        moveX = Input.GetAxisRaw("Horizontal");
        moveY = Input.GetAxisRaw("Vertical");
#else
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            Camera touchCamera = Camera.main != null ? Camera.main : mainCamera;
            Vector3 touchPos = touch.position;

            if (touchCamera != null)
                touchPos = touchCamera.ScreenToWorldPoint(touch.position);

            Vector3 direction = touchPos - transform.position;
            moveX = Mathf.Sign(direction.x);
            moveY = Mathf.Sign(direction.y);
        }
#endif

        return new Vector2(moveX, moveY);
    }

    void ApplyMovement(float deltaTime, bool useRigidbody)
    {
        Vector2 position = useRigidbody ? playerBody.position : (Vector2)transform.position;
        float currentSpeed = moveSpeed;

        if (GameManager.instance != null)
            currentSpeed *= GameManager.instance.GetPlayerMoveSpeedMultiplier();

        position.x += moveInput.x * currentSpeed * deltaTime;
        position.y += moveInput.y * currentSpeed * deltaTime;

        position = ClampToBounds(position);

        if (useRigidbody)
        {
            playerBody.MovePosition(position);
        }
        else
        {
            transform.position = new Vector3(position.x, position.y, transform.position.z);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryHandleObstacleTrigger(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TryHandleObstacleTrigger(other);
    }

    void TryHandleObstacleTrigger(Collider2D other)
    {
        if (isDead || other == null || !other.CompareTag("Obstacle"))
            return;

        if (isInvulnerable)
        {
            Destroy(other.gameObject);
            return;
        }

        if (Time.time < lastObstacleHitTime + ObstacleHitCooldown)
            return;

        lastObstacleHitTime = Time.time;

        if (GameManager.instance != null)
        {
            GameManager.instance.HandlePlayerHit(other.gameObject);
        }
        else
        {
            Die();
        }
    }

    public void Die()
    {
        if (invulnerabilityRoutine != null)
        {
            StopCoroutine(invulnerabilityRoutine);
            invulnerabilityRoutine = null;
        }

        isInvulnerable = false;
        isDead = true;
        moveInput = Vector2.zero;

        if (spriteRenderer != null)
            spriteRenderer.enabled = true;
    }

    public void Revive(float invulnerabilityDuration)
    {
        isDead = false;

        if (spriteRenderer != null)
            spriteRenderer.enabled = true;

        TriggerInvulnerability(invulnerabilityDuration);
    }

    public void SetSizeMultiplier(float multiplier)
    {
        transform.localScale = originalScale * multiplier;
        RefreshMovementBounds();
        ClampInsideBoundsImmediate();
    }

    public bool IsInvulnerable()
    {
        return isInvulnerable;
    }

    public void TriggerInvulnerability(float duration)
    {
        lastObstacleHitTime = Time.time;

        if (invulnerabilityRoutine != null)
            StopCoroutine(invulnerabilityRoutine);

        invulnerabilityRoutine = StartCoroutine(InvulnerabilityRoutine(duration));
    }

    IEnumerator InvulnerabilityRoutine(float duration)
    {
        isInvulnerable = true;
        float elapsed = 0f;
        bool visible = true;

        while (elapsed < duration)
        {
            elapsed += 0.1f;
            visible = !visible;

            if (spriteRenderer != null)
                spriteRenderer.enabled = visible;

            yield return new WaitForSeconds(0.1f);
        }

        if (spriteRenderer != null)
            spriteRenderer.enabled = true;

        isInvulnerable = false;
        invulnerabilityRoutine = null;
    }

    Vector2 ClampToBounds(Vector2 position)
    {
        position.x = Mathf.Clamp(position.x, minX, maxX);
        position.y = Mathf.Clamp(position.y, minY, maxY);
        return position;
    }

    void ClampInsideBoundsImmediate()
    {
        Vector2 clampedPosition = ClampToBounds(playerBody != null ? playerBody.position : (Vector2)transform.position);

        if (playerBody != null)
            playerBody.position = clampedPosition;

        transform.position = new Vector3(clampedPosition.x, clampedPosition.y, transform.position.z);
    }

    void RefreshMovementBounds()
    {
        mainCamera = Camera.main != null ? Camera.main : mainCamera;

        if (leftBorderTransform == null)
        {
            GameObject leftBorderObject = GameObject.Find(leftBorderObjectName);

            if (leftBorderObject != null)
                leftBorderTransform = leftBorderObject.transform;
        }

        if (rightBorderTransform == null)
        {
            GameObject rightBorderObject = GameObject.Find(rightBorderObjectName);

            if (rightBorderObject != null)
                rightBorderTransform = rightBorderObject.transform;
        }

        Vector2 halfExtents = GetPlayerHalfExtents();
        float computedMinX = configuredMinX;
        float computedMaxX = configuredMaxX;
        float computedMinY = configuredMinY;
        float computedMaxY = configuredMaxY;

        if (leftBorderTransform != null)
            computedMinX = leftBorderTransform.position.x + halfExtents.x + borderPadding;

        if (rightBorderTransform != null)
            computedMaxX = rightBorderTransform.position.x - halfExtents.x - borderPadding;

        if (mainCamera != null)
        {
            float cameraDistance = Mathf.Abs(mainCamera.transform.position.z - transform.position.z);
            Vector3 bottomLeft = mainCamera.ViewportToWorldPoint(new Vector3(0f, 0f, cameraDistance));
            Vector3 topRight = mainCamera.ViewportToWorldPoint(new Vector3(1f, 1f, cameraDistance));

            computedMinX = Mathf.Max(computedMinX, bottomLeft.x + halfExtents.x + cameraSidePadding);
            computedMaxX = Mathf.Min(computedMaxX, topRight.x - halfExtents.x - cameraSidePadding);
            computedMinY = Mathf.Max(computedMinY, bottomLeft.y + halfExtents.y + cameraBottomPadding);
            computedMaxY = Mathf.Min(computedMaxY, topRight.y - halfExtents.y - cameraTopPadding);
        }

        minX = computedMinX;
        maxX = computedMaxX;
        minY = computedMinY;
        maxY = computedMaxY;
    }

    Vector2 GetPlayerHalfExtents()
    {
        if (playerCollider != null)
            return playerCollider.bounds.extents;

        if (spriteRenderer != null)
            return spriteRenderer.bounds.extents;

        return new Vector2(0.3f, 0.3f);
    }

    bool DidScreenSizeChange()
    {
        return Screen.width != cachedScreenWidth || Screen.height != cachedScreenHeight;
    }

    void CacheScreenSize()
    {
        cachedScreenWidth = Screen.width;
        cachedScreenHeight = Screen.height;
    }
}
```

### SceneLoader.cs
```csharp
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
```

### ShopManager.cs
```csharp
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    public TextMeshProUGUI totalCoinsText;

    public TextMeshProUGUI speedUpgradeText;
    public TextMeshProUGUI shieldUpgradeText;
    public TextMeshProUGUI extraLifeUpgradeText;
    public TextMeshProUGUI coinMagnetUpgradeText;
    public TextMeshProUGUI doubleCoinsUpgradeText;
    public TextMeshProUGUI slowTimeUpgradeText;
    public TextMeshProUGUI smallerPlayerUpgradeText;
    public TextMeshProUGUI scoreBoosterUpgradeText;
    public TextMeshProUGUI bombUpgradeText;
    public TextMeshProUGUI rareCoinBoostUpgradeText;

    public string titleObjectName = "ShopTitle";
    public string backButtonObjectName = "BackButton";

    public float sidePadding = 18f;
    public float topPadding = 140f;
    public float bottomPadding = 84f;
    public float rowSpacing = 12f;
    public float itemHeight = 208f;

    private int totalCoins;
    private TMP_FontAsset runtimeFont;
    private TextMeshProUGUI shopTitleText;
    private RectTransform shopTitleRect;
    private RectTransform backButtonRect;
    private TextMeshProUGUI backButtonText;
    private TextMeshProUGUI feedbackText;
    private RectTransform viewportRect;
    private RectTransform contentRect;
    private ScrollRect scrollRect;
    private RectTransform uiRootRect;
    private readonly UpgradeType[] shopOrder =
    {
        UpgradeType.Shield,
        UpgradeType.SpeedBoost,
        UpgradeType.ExtraLife,
        UpgradeType.CoinMagnet,
        UpgradeType.DoubleCoins,
        UpgradeType.SlowTime,
        UpgradeType.SmallerPlayer,
        UpgradeType.ScoreBooster,
        UpgradeType.Bomb,
        UpgradeType.RareCoinBoost
    };

    private readonly Dictionary<UpgradeType, ShopUpgradeButtonUI> runtimeButtons =
        new Dictionary<UpgradeType, ShopUpgradeButtonUI>();

    private const string FeedbackObjectName = "ShopFeedbackText";
    private const string ViewportObjectName = "ShopViewport";
    private const string ContentObjectName = "ShopContent";

    void Awake()
    {
        FindStaticReferences();
        NormalizeLegacyRootLayout();
        runtimeFont = GetRuntimeFont();
        EnsureRuntimeUI();
    }

    void Start()
    {
        totalCoins = PlayerPrefs.GetInt("TotalCoins", 0);
        BuildShopButtons();
        RefreshUI();
    }

    void OnEnable()
    {
        totalCoins = PlayerPrefs.GetInt("TotalCoins", 0);
        RefreshUI();
    }

    void OnRectTransformDimensionsChange()
    {
        if (!isActiveAndEnabled)
            return;

        RefreshUI();
    }

    void FindStaticReferences()
    {
        if (totalCoinsText == null)
        {
            totalCoinsText = FindTextObject("TotalCoinsText");
        }
        if (shopTitleText == null)
        {
            shopTitleText = FindTextObject(titleObjectName);
        }

        if (shopTitleText != null)
            shopTitleRect = shopTitleText.rectTransform;

        if (backButtonRect == null)
        {
            GameObject backButtonObject = GameObject.Find(backButtonObjectName);

            if (backButtonObject != null)
            {
                backButtonRect = backButtonObject.GetComponent<RectTransform>();
                backButtonText = backButtonObject.GetComponentInChildren<TextMeshProUGUI>(true);
            }
        }
        if (uiRootRect == null)
            uiRootRect = GetUIRootRect();
    }

    TextMeshProUGUI FindTextObject(string objectName)
    {
        GameObject textObject = GameObject.Find(objectName);

        if (textObject == null)
            return null;

        return textObject.GetComponent<TextMeshProUGUI>();
    }

    RectTransform GetUIRootRect()
    {
        if (totalCoinsText != null && totalCoinsText.rectTransform.parent != null)
            return totalCoinsText.rectTransform.parent as RectTransform;

        Canvas canvas = FindAnyObjectByType<Canvas>();

        if (canvas != null)
            return canvas.GetComponent<RectTransform>();

        return null;
    }

    TMP_FontAsset GetRuntimeFont()
    {
        if (totalCoinsText != null)
            return totalCoinsText.font;

        if (shopTitleText != null)
            return shopTitleText.font;

        return null;
    }

    void NormalizeLegacyRootLayout()
    {
        Canvas canvas = FindAnyObjectByType<Canvas>();

        if (canvas != null)
        {
            RectTransform canvasRect = canvas.GetComponent<RectTransform>();

            if (canvasRect != null)
            {
                canvasRect.localScale = Vector3.one;
                canvasRect.anchorMin = Vector2.zero;
                canvasRect.anchorMax = Vector2.zero;
                canvasRect.sizeDelta = Vector2.zero;
            }
        }

        if (uiRootRect == null && totalCoinsText != null)
            uiRootRect = totalCoinsText.rectTransform.parent as RectTransform;

        if (uiRootRect != null)
        {
            uiRootRect.localScale = Vector3.one;
            uiRootRect.anchorMin = Vector2.zero;
            uiRootRect.anchorMax = Vector2.one;
            uiRootRect.pivot = new Vector2(0.5f, 0.5f);
            uiRootRect.anchoredPosition = Vector2.zero;
            uiRootRect.sizeDelta = Vector2.zero;
            uiRootRect.offsetMin = Vector2.zero;
            uiRootRect.offsetMax = Vector2.zero;
        }
    }

    void EnsureRuntimeUI()
    {
        HideLegacyShopButtons();
        HideLegacyShopLabels();
        EnsureFeedbackText();
        EnsureScrollArea();
        LayoutStaticElements();
    }

    void HideLegacyShopButtons()
    {
        string[] legacyButtonNames =
        {
            "SpeedUpgradeButton",
            "ShieldUpgradeButton",
            "CoinUpgradeButton"
        };

        for (int i = 0; i < legacyButtonNames.Length; i++)
        {
            GameObject legacyObject = GameObject.Find(legacyButtonNames[i]);

            if (legacyObject != null)
                legacyObject.SetActive(false);
        }
    }

    void HideLegacyShopLabels()
    {
        TextMeshProUGUI[] labels = FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include);

        for (int i = 0; i < labels.Length; i++)
        {
            if (labels[i] != null && labels[i].text == "New Text")
                labels[i].gameObject.SetActive(false);
        }
    }

    void EnsureFeedbackText()
    {
        if (uiRootRect == null)
            return;

        GameObject existingObject = GameObject.Find(FeedbackObjectName);

        if (existingObject != null)
        {
            feedbackText = existingObject.GetComponent<TextMeshProUGUI>();
            return;
        }

        GameObject labelObject = new GameObject(FeedbackObjectName, typeof(RectTransform));
        labelObject.transform.SetParent(uiRootRect, false);

        feedbackText = labelObject.AddComponent<TextMeshProUGUI>();
        feedbackText.alignment = TextAlignmentOptions.Center;
        feedbackText.enableAutoSizing = true;
        feedbackText.fontSizeMin = 14;
        feedbackText.fontSizeMax = 18;
        feedbackText.color = new Color(0.16f, 0.18f, 0.24f, 1f);

        if (runtimeFont != null)
            feedbackText.font = runtimeFont;
    }

    void EnsureScrollArea()
    {
        if (uiRootRect == null)
            return;

        Transform existingViewport = uiRootRect.Find(ViewportObjectName);

        if (existingViewport != null)
        {
            viewportRect = existingViewport as RectTransform;
        }
        else
        {
            GameObject viewportObject = new GameObject(
                ViewportObjectName,
                typeof(RectTransform),
                typeof(Image),
                typeof(RectMask2D),
                typeof(ScrollRect));

            viewportObject.transform.SetParent(uiRootRect, false);
            viewportRect = viewportObject.GetComponent<RectTransform>();

            Image viewportImage = viewportObject.GetComponent<Image>();
            viewportImage.color = new Color(1f, 1f, 1f, 0.02f);
        }

        if (viewportRect.GetComponent<RectMask2D>() == null)
            viewportRect.gameObject.AddComponent<RectMask2D>();

        Transform existingContent = viewportRect.Find(ContentObjectName);

        if (existingContent != null)
        {
            contentRect = existingContent as RectTransform;
        }
        else
        {
            GameObject contentObject = new GameObject(ContentObjectName, typeof(RectTransform));
            contentObject.transform.SetParent(viewportRect, false);
            contentRect = contentObject.GetComponent<RectTransform>();
        }

        scrollRect = viewportRect.GetComponent<ScrollRect>();

        if (scrollRect == null)
            scrollRect = viewportRect.gameObject.AddComponent<ScrollRect>();

        scrollRect.viewport = viewportRect;
        scrollRect.content = contentRect;
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.scrollSensitivity = 32f;
    }

    void LayoutStaticElements()
    {
        if (uiRootRect == null)
            return;

        Canvas.ForceUpdateCanvases();
        float rootWidth = uiRootRect.rect.width;

        if (shopTitleRect != null)
        {
            shopTitleRect.anchorMin = new Vector2(0.5f, 1f);
            shopTitleRect.anchorMax = new Vector2(0.5f, 1f);
            shopTitleRect.pivot = new Vector2(0.5f, 1f);
            shopTitleRect.sizeDelta = new Vector2(rootWidth - (sidePadding * 2f), 52f);
            shopTitleRect.anchoredPosition = new Vector2(0f, -18f);
        }

        if (shopTitleText != null)
        {
            shopTitleText.text = "Consumable Shop";
            shopTitleText.alignment = TextAlignmentOptions.Center;
            shopTitleText.enableAutoSizing = true;
            shopTitleText.fontSizeMin = 20;
            shopTitleText.fontSizeMax = 30;
        }

        if (totalCoinsText != null)
        {
            RectTransform coinsRect = totalCoinsText.rectTransform;
            coinsRect.anchorMin = new Vector2(0.5f, 1f);
            coinsRect.anchorMax = new Vector2(0.5f, 1f);
            coinsRect.pivot = new Vector2(0.5f, 1f);
            coinsRect.sizeDelta = new Vector2(rootWidth - (sidePadding * 2f), 32f);
            coinsRect.anchoredPosition = new Vector2(0f, -60f);
            totalCoinsText.alignment = TextAlignmentOptions.Center;
            totalCoinsText.enableAutoSizing = true;
            totalCoinsText.fontSizeMin = 16;
            totalCoinsText.fontSizeMax = 24;
        }

        if (feedbackText != null)
        {
            RectTransform feedbackRect = feedbackText.rectTransform;
            feedbackRect.anchorMin = new Vector2(0.5f, 1f);
            feedbackRect.anchorMax = new Vector2(0.5f, 1f);
            feedbackRect.pivot = new Vector2(0.5f, 1f);
            feedbackRect.sizeDelta = new Vector2(rootWidth - (sidePadding * 2f), 28f);
            feedbackRect.anchoredPosition = new Vector2(0f, -92f);
            feedbackText.fontSizeMin = 16;
            feedbackText.fontSizeMax = 22;
        }

        if (backButtonRect != null)
        {
            backButtonRect.anchorMin = new Vector2(0.5f, 0f);
            backButtonRect.anchorMax = new Vector2(0.5f, 0f);
            backButtonRect.pivot = new Vector2(0.5f, 0f);
            backButtonRect.sizeDelta = new Vector2(220f, 56f);
            backButtonRect.anchoredPosition = new Vector2(0f, 16f);
        }

        if (backButtonText != null)
        {
            backButtonText.enableAutoSizing = true;
            backButtonText.fontSizeMin = 18;
            backButtonText.fontSizeMax = 28;
        }

        if (viewportRect != null)
        {
            viewportRect.anchorMin = new Vector2(0f, 0f);
            viewportRect.anchorMax = new Vector2(1f, 1f);
            viewportRect.pivot = new Vector2(0.5f, 0.5f);
            viewportRect.offsetMin = new Vector2(sidePadding, bottomPadding);
            viewportRect.offsetMax = new Vector2(-sidePadding, -topPadding);
        }
    }

    void BuildShopButtons()
    {
        if (contentRect == null)
            return;

        BindSceneShopButtons();

        if (runtimeButtons.Count > 0)
        {
            LayoutShopList();
            return;
        }

        for (int i = 0; i < shopOrder.Length; i++)
        {
            UpgradeType type = shopOrder[i];
            ShopUpgradeButtonUI buttonUI = CreateShopButton(type);

            if (buttonUI != null)
                runtimeButtons[type] = buttonUI;
        }

        LayoutShopList();
    }

    void BindSceneShopButtons()
    {
        if (contentRect == null)
            return;

        for (int i = 0; i < contentRect.childCount; i++)
        {
            Transform child = contentRect.GetChild(i);
            UpgradeType type;

            if (!TryGetUpgradeTypeFromObjectName(child.name, out type))
                continue;

            Button sceneButton = child.GetComponent<Button>();
            Image sceneImage = child.GetComponent<Image>();

            if (sceneButton == null || sceneImage == null)
                continue;

            ShopUpgradeButtonUI buttonUI = child.GetComponent<ShopUpgradeButtonUI>();

            if (buttonUI == null)
                buttonUI = child.gameObject.AddComponent<ShopUpgradeButtonUI>();

            buttonUI.Initialize(this, type, sceneImage, sceneButton, runtimeFont);
            runtimeButtons[type] = buttonUI;
        }
    }

    bool TryGetUpgradeTypeFromObjectName(string objectName, out UpgradeType type)
    {
        switch (objectName)
        {
            case "ShieldShopButton":
                type = UpgradeType.Shield;
                return true;
            case "SpeedBoostShopButton":
                type = UpgradeType.SpeedBoost;
                return true;
            case "ExtraLifeShopButton":
                type = UpgradeType.ExtraLife;
                return true;
            case "CoinMagnetShopButton":
                type = UpgradeType.CoinMagnet;
                return true;
            case "DoubleCoinsShopButton":
                type = UpgradeType.DoubleCoins;
                return true;
            case "SlowTimeShopButton":
                type = UpgradeType.SlowTime;
                return true;
            case "SmallerPlayerShopButton":
                type = UpgradeType.SmallerPlayer;
                return true;
            case "ScoreBoosterShopButton":
                type = UpgradeType.ScoreBooster;
                return true;
            case "BombShopButton":
                type = UpgradeType.Bomb;
                return true;
            case "RareCoinBoostShopButton":
                type = UpgradeType.RareCoinBoost;
                return true;
            default:
                type = UpgradeType.Shield;
                return false;
        }
    }

    ShopUpgradeButtonUI CreateShopButton(UpgradeType type)
    {
        GameObject buttonObject = new GameObject(
            UpgradeInventory.GetDisplayName(type) + "ShopButton",
            typeof(RectTransform),
            typeof(Image),
            typeof(Button),
            typeof(ShopUpgradeButtonUI));

        buttonObject.transform.SetParent(contentRect, false);

        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0f, 1f);
        buttonRect.anchorMax = new Vector2(1f, 1f);
        buttonRect.pivot = new Vector2(0.5f, 1f);
        buttonRect.sizeDelta = new Vector2(0f, itemHeight);

        Image buttonImage = buttonObject.GetComponent<Image>();
        buttonImage.color = new Color(1f, 1f, 1f, 0.94f);

        ShopUpgradeButtonUI buttonUI = buttonObject.GetComponent<ShopUpgradeButtonUI>();
        buttonUI.Initialize(this, type, buttonImage, buttonObject.GetComponent<Button>(), runtimeFont);
        return buttonUI;
    }

    void LayoutShopList()
    {
        if (viewportRect == null || contentRect == null)
            return;

        Canvas.ForceUpdateCanvases();

        float contentHeight = (shopOrder.Length * itemHeight) + ((shopOrder.Length - 1) * rowSpacing);
        float viewportHeight = viewportRect.rect.height;

        if (contentHeight < viewportHeight)
            contentHeight = viewportHeight;

        contentRect.anchorMin = new Vector2(0f, 1f);
        contentRect.anchorMax = new Vector2(1f, 1f);
        contentRect.pivot = new Vector2(0.5f, 1f);
        contentRect.sizeDelta = new Vector2(0f, contentHeight);
        contentRect.anchoredPosition = Vector2.zero;

        for (int i = 0; i < shopOrder.Length; i++)
        {
            if (!runtimeButtons.TryGetValue(shopOrder[i], out ShopUpgradeButtonUI buttonUI) || buttonUI == null)
                continue;

            RectTransform buttonRect = buttonUI.GetComponent<RectTransform>();

            if (buttonRect == null)
                continue;

            float y = -(i * (itemHeight + rowSpacing));
            buttonRect.anchorMin = new Vector2(0f, 1f);
            buttonRect.anchorMax = new Vector2(1f, 1f);
            buttonRect.pivot = new Vector2(0.5f, 1f);
            buttonRect.sizeDelta = new Vector2(0f, itemHeight);
            buttonRect.anchoredPosition = new Vector2(0f, y);
        }
    }

    void RefreshUI()
    {
        FindStaticReferences();
        LayoutStaticElements();
        LayoutShopList();

        if (totalCoinsText != null)
            totalCoinsText.text = "Coins: " + totalCoins;

        if (feedbackText != null && string.IsNullOrEmpty(feedbackText.text))
            feedbackText.text = "Tap a card to buy a consumable";

        for (int i = 0; i < shopOrder.Length; i++)
        {
            UpgradeType type = shopOrder[i];

            if (!runtimeButtons.TryGetValue(type, out ShopUpgradeButtonUI buttonUI) || buttonUI == null)
                continue;

            int owned = 0;
            int cost = GetUpgradeCost(type);

            if (UpgradeInventory.Instance != null)
                owned = UpgradeInventory.Instance.GetAmount(type);

            buttonUI.RefreshView(
                UpgradeInventory.GetDisplayName(type),
                GetUpgradeDescription(type),
                owned,
                cost,
                totalCoins >= cost);
        }
    }

    bool TryBuyUpgrade(UpgradeType type)
    {
        int cost = GetUpgradeCost(type);

        if (totalCoins < cost)
        {
            SetFeedback("Not enough coins for " + UpgradeInventory.GetDisplayName(type));
            RefreshUI();
            return false;
        }

        totalCoins -= cost;
        PlayerPrefs.SetInt("TotalCoins", totalCoins);
        PlayerPrefs.Save();

        if (UpgradeInventory.Instance != null)
            UpgradeInventory.Instance.AddUpgrade(type, 1);

        SetFeedback(UpgradeInventory.GetDisplayName(type) + " purchased");
        RefreshUI();
        return true;
    }

    string GetUpgradeDescription(UpgradeType type)
    {
        switch (type)
        {
            case UpgradeType.Shield:
                return "Block one hit";
            case UpgradeType.SpeedBoost:
                return "Move faster";
            case UpgradeType.ExtraLife:
                return "Revive once";
            case UpgradeType.CoinMagnet:
                return "Pull in coins";
            case UpgradeType.DoubleCoins:
                return "Double coin value";
            case UpgradeType.SlowTime:
                return "Slow obstacles";
            case UpgradeType.SmallerPlayer:
                return "Shrink your hitbox";
            case UpgradeType.ScoreBooster:
                return "Double score gain";
            case UpgradeType.Bomb:
                return "Clear the screen";
            case UpgradeType.RareCoinBoost:
                return "More coin spawns";
            default:
                return "Consumable upgrade";
        }
    }

    public void BuyUpgradeCard(UpgradeType type)
    {
        TryBuyUpgrade(type);
    }

    int GetUpgradeCost(UpgradeType type)
    {
        switch (type)
        {
            case UpgradeType.SpeedBoost:
                return 12;
            case UpgradeType.Shield:
                return 15;
            case UpgradeType.ExtraLife:
                return 24;
            case UpgradeType.CoinMagnet:
                return 18;
            case UpgradeType.DoubleCoins:
                return 20;
            case UpgradeType.SlowTime:
                return 18;
            case UpgradeType.SmallerPlayer:
                return 16;
            case UpgradeType.ScoreBooster:
                return 14;
            case UpgradeType.Bomb:
                return 28;
            case UpgradeType.RareCoinBoost:
                return 20;
            default:
                return 15;
        }
    }

    void SetFeedback(string message)
    {
        if (feedbackText != null)
            feedbackText.text = message;
    }

    public void BuySpeedUpgrade()
    {
        TryBuyUpgrade(UpgradeType.SpeedBoost);
    }

    public void BuyShieldUpgrade()
    {
        TryBuyUpgrade(UpgradeType.Shield);
    }

    public void BuyExtraLifeUpgrade()
    {
        TryBuyUpgrade(UpgradeType.ExtraLife);
    }

    public void BuyCoinMagnetUpgrade()
    {
        TryBuyUpgrade(UpgradeType.CoinMagnet);
    }

    public void BuyDoubleCoinsUpgrade()
    {
        TryBuyUpgrade(UpgradeType.DoubleCoins);
    }

    public void BuySlowTimeUpgrade()
    {
        TryBuyUpgrade(UpgradeType.SlowTime);
    }

    public void BuySmallerPlayerUpgrade()
    {
        TryBuyUpgrade(UpgradeType.SmallerPlayer);
    }

    public void BuyScoreBoosterUpgrade()
    {
        TryBuyUpgrade(UpgradeType.ScoreBooster);
    }

    public void BuyBombUpgrade()
    {
        TryBuyUpgrade(UpgradeType.Bomb);
    }

    public void BuyRareCoinBoostUpgrade()
    {
        TryBuyUpgrade(UpgradeType.RareCoinBoost);
    }

    public void BuyCoinUpgrade()
    {
        TryBuyUpgrade(UpgradeType.DoubleCoins);
    }

    public void GoBack()
    {
        SceneManager.LoadScene("MainMenu");
    }
}

public class ShopUpgradeButtonUI : MonoBehaviour
{
    private ShopManager shopManager;
    private UpgradeType upgradeType;
    private TextMeshProUGUI titleText;
    private TextMeshProUGUI descriptionText;
    private TextMeshProUGUI metaText;
    private Image backgroundImage;
    private Button button;

    private readonly Color affordableColor = new Color(1f, 1f, 1f, 0.96f);
    private readonly Color expensiveColor = new Color(0.87f, 0.88f, 0.92f, 0.88f);
    private readonly Color titleColor = new Color(0.13f, 0.17f, 0.26f, 1f);
    private readonly Color bodyColor = new Color(0.2f, 0.24f, 0.32f, 1f);

    public void Initialize(
        ShopManager manager,
        UpgradeType type,
        Image image,
        Button sourceButton,
        TMP_FontAsset runtimeFont)
    {
        shopManager = manager;
        upgradeType = type;
        backgroundImage = image;
        button = sourceButton;

        CreateLabels(runtimeFont);

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnPressed);
        }
    }

    void CreateLabels(TMP_FontAsset runtimeFont)
    {
        titleText = FindOrCreateStretchText(
            "Title",
            new Vector2(22f, 136f),
            new Vector2(-22f, -18f),
            TextAlignmentOptions.Top,
            30f,
            42f,
            runtimeFont);

        descriptionText = FindOrCreateStretchText(
            "Description",
            new Vector2(22f, 78f),
            new Vector2(-22f, -86f),
            TextAlignmentOptions.Center,
            22f,
            30f,
            runtimeFont);

        metaText = FindOrCreateStretchText(
            "Meta",
            new Vector2(22f, 18f),
            new Vector2(-22f, -136f),
            TextAlignmentOptions.Bottom,
            20f,
            28f,
            runtimeFont);
    }

    TextMeshProUGUI FindOrCreateStretchText(
        string objectName,
        Vector2 leftBottom,
        Vector2 rightTop,
        TextAlignmentOptions alignment,
        float minSize,
        float maxSize,
        TMP_FontAsset runtimeFont)
    {
        Transform existing = transform.Find(objectName);
        GameObject textObject;

        bool created = existing == null;

        if (existing != null)
        {
            textObject = existing.gameObject;
        }
        else
        {
            textObject = new GameObject(objectName, typeof(RectTransform));
            textObject.transform.SetParent(transform, false);
        }

        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(leftBottom.x, leftBottom.y);
        textRect.offsetMax = new Vector2(rightTop.x, rightTop.y);

        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();

        if (text == null)
            text = textObject.AddComponent<TextMeshProUGUI>();

        text.alignment = alignment;
        text.color = bodyColor;

        if (created)
        {
            text.enableAutoSizing = true;
            text.fontSizeMin = minSize;
            text.fontSizeMax = maxSize;
        }

        if (runtimeFont != null && text.font == null)
            text.font = runtimeFont;

        return text;
    }

    public void RefreshView(string displayName, string description, int ownedAmount, int cost, bool canAfford)
    {
        if (titleText != null)
        {
            titleText.text = displayName;
            titleText.color = titleColor;
        }

        if (descriptionText != null)
        {
            descriptionText.text = description;
            descriptionText.color = bodyColor;
        }

        if (metaText != null)
        {
            string affordText = canAfford ? "Tap to buy" : "Need more coins";
            string priceColorHex = canAfford ? "E9AA33" : "8A8FA0";
            metaText.text =
                "Owned " + ownedAmount +
                "   <color=#" + priceColorHex + ">" + cost + " coins</color>" +
                "\n" + affordText;
        }

        if (backgroundImage != null)
        {
            if (canAfford)
                backgroundImage.color = affordableColor;
            else
                backgroundImage.color = expensiveColor;
        }
    }

    void OnPressed()
    {
        if (shopManager != null)
            shopManager.BuyUpgradeCard(upgradeType);
    }
}
```

### Spawner.cs
```csharp
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject[] obstaclePrefabs;
    public GameObject coinPrefab;

    public float spawnInterval = 0.9f;
    public float minX = -4.0f;
    public float maxX = 4.0f;
    public float spawnY = 6f;
    public float coinSpawnChance = 0.25f;

    public float difficultyStepTime = 15f;

    private float timer;
    private float runTime;
    private bool stopSpawning = false;

    void Update()
    {
        if (stopSpawning) return;

        float worldSpeedMultiplier = 1f;

        if (GameManager.instance != null)
            worldSpeedMultiplier = GameManager.instance.GetWorldSpeedMultiplier();

        timer += Time.deltaTime * worldSpeedMultiplier;
        runTime += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            timer = 0f;
            SpawnPattern();
        }
    }

    void SpawnPattern()
    {
        float currentCoinSpawnChance = coinSpawnChance;

        if (GameManager.instance != null)
            currentCoinSpawnChance = GameManager.instance.GetCurrentCoinSpawnChance(coinSpawnChance);

        if (Random.value < currentCoinSpawnChance)
        {
            SpawnCoin();
            return;
        }

        int difficultyLevel = Mathf.FloorToInt(runTime / difficultyStepTime);

        if (difficultyLevel < 2)
        {
            SpawnSingle();
        }
        else if (difficultyLevel < 4)
        {
            if (Random.value < 0.5f)
                SpawnSingle();
            else
                SpawnDouble();
        }
        else
        {
            float r = Random.value;

            if (r < 0.4f)
                SpawnSingle();
            else if (r < 0.75f)
                SpawnDouble();
            else
                SpawnGap();
        }
    }

    void SpawnSingle()
    {
        float x = Random.Range(minX, maxX);
        SpawnObstacleAt(x);
    }

    void SpawnDouble()
    {
        float gap = 2.2f;
        float center = Random.Range(minX + gap, maxX - gap);

        SpawnObstacleAt(center - gap);
        SpawnObstacleAt(center + gap);
    }

    void SpawnGap()
    {
        float gapSize = 1.4f;
        float gapCenter = Random.Range(minX + gapSize, maxX - gapSize);

        int lanes = 7;
        float laneWidth = (maxX - minX) / lanes;

        for (int i = 0; i < lanes; i++)
        {
            float x = minX + laneWidth * i + laneWidth / 2f;

            if (Mathf.Abs(x - gapCenter) > gapSize)
            {
                SpawnObstacleAt(x);
            }
        }
    }

    void SpawnObstacleAt(float x)
    {
        if (obstaclePrefabs == null || obstaclePrefabs.Length == 0)
            return;

        int difficultyLevel = Mathf.FloorToInt(runTime / difficultyStepTime);
        int unlockedCount = 1 + (difficultyLevel / 2);
        unlockedCount = Mathf.Clamp(unlockedCount, 1, obstaclePrefabs.Length);

        GameObject prefab = obstaclePrefabs[Random.Range(0, unlockedCount)];
        Vector3 pos = new Vector3(x, spawnY, 0f);

        Instantiate(prefab, pos, prefab.transform.rotation);
    }

    void SpawnCoin()
    {
        float x = Random.Range(minX, maxX);
        Vector3 pos = new Vector3(x, spawnY, 0f);

        Instantiate(coinPrefab, pos, coinPrefab.transform.rotation);
    }

    public void StopSpawning()
    {
        stopSpawning = true;
    }
}
```

### UpgradeInventory.cs
```csharp
using System.Collections.Generic;
using UnityEngine;

public enum UpgradeType
{
    SpeedBoost,
    Shield,
    ExtraLife,
    CoinMagnet,
    DoubleCoins,
    SlowTime,
    SmallerPlayer,
    ScoreBooster,
    Bomb,
    RareCoinBoost
}

public enum EquipToggleResult
{
    Equipped,
    Unequipped,
    NotOwned,
    LoadoutFull
}

public class UpgradeInventory : MonoBehaviour
{
    public static UpgradeInventory Instance;
    public const int MaxEquippedUpgrades = 3;

    private Dictionary<UpgradeType, int> ownedUpgrades = new Dictionary<UpgradeType, int>();
    private List<UpgradeType> equippedUpgrades = new List<UpgradeType>();

    private const string EquippedCountKey = "EquippedUpgradeCount";
    private const string EquippedKeyPrefix = "EquippedUpgrade_";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void AutoCreate()
    {
        if (Instance == null)
        {
            GameObject obj = new GameObject("UpgradeInventory");
            obj.AddComponent<UpgradeInventory>();
        }
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeInventory();
            LoadInventory();
            LoadEquippedUpgrades();
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    void InitializeInventory()
    {
        foreach (UpgradeType type in System.Enum.GetValues(typeof(UpgradeType)))
        {
            if (!ownedUpgrades.ContainsKey(type))
            {
                ownedUpgrades[type] = 0;
            }
        }
    }

    public int GetAmount(UpgradeType type)
    {
        if (ownedUpgrades.ContainsKey(type))
            return ownedUpgrades[type];

        return 0;
    }

    public void AddUpgrade(UpgradeType type, int amount)
    {
        if (!ownedUpgrades.ContainsKey(type))
            ownedUpgrades[type] = 0;

        ownedUpgrades[type] += amount;
        SaveInventory();
    }

    public bool UseUpgrade(UpgradeType type, int amount = 1)
    {
        if (GetAmount(type) >= amount)
        {
            ownedUpgrades[type] -= amount;
            SaveInventory();
            return true;
        }

        return false;
    }

    public bool IsEquipped(UpgradeType type)
    {
        return equippedUpgrades.Contains(type);
    }

    public int GetEquippedCount()
    {
        return equippedUpgrades.Count;
    }

    public List<UpgradeType> GetEquippedUpgrades()
    {
        List<UpgradeType> result = new List<UpgradeType>();

        for (int i = 0; i < equippedUpgrades.Count && i < MaxEquippedUpgrades; i++)
        {
            result.Add(equippedUpgrades[i]);
        }

        return result;
    }

    public EquipToggleResult ToggleEquippedUpgrade(UpgradeType type)
    {
        if (IsEquipped(type))
        {
            equippedUpgrades.Remove(type);
            SaveEquippedUpgrades();
            return EquipToggleResult.Unequipped;
        }

        if (GetAmount(type) <= 0)
            return EquipToggleResult.NotOwned;

        if (equippedUpgrades.Count >= MaxEquippedUpgrades)
            return EquipToggleResult.LoadoutFull;

        equippedUpgrades.Add(type);
        SaveEquippedUpgrades();
        return EquipToggleResult.Equipped;
    }

    public void SaveInventory()
    {
        foreach (UpgradeType type in System.Enum.GetValues(typeof(UpgradeType)))
        {
            PlayerPrefs.SetInt("Upgrade_" + type.ToString(), GetAmount(type));
        }

        PlayerPrefs.Save();
    }

    public void LoadInventory()
    {
        foreach (UpgradeType type in System.Enum.GetValues(typeof(UpgradeType)))
        {
            ownedUpgrades[type] = PlayerPrefs.GetInt("Upgrade_" + type.ToString(), 0);
        }
    }

    void SaveEquippedUpgrades()
    {
        int previousCount = PlayerPrefs.GetInt(EquippedCountKey, 0);

        PlayerPrefs.SetInt(EquippedCountKey, equippedUpgrades.Count);

        for (int i = 0; i < equippedUpgrades.Count; i++)
        {
            PlayerPrefs.SetString(EquippedKeyPrefix + i, equippedUpgrades[i].ToString());
        }

        for (int i = equippedUpgrades.Count; i < previousCount; i++)
        {
            PlayerPrefs.DeleteKey(EquippedKeyPrefix + i);
        }

        PlayerPrefs.Save();
    }

    void LoadEquippedUpgrades()
    {
        equippedUpgrades.Clear();

        int equippedCount = PlayerPrefs.GetInt(EquippedCountKey, 0);

        for (int i = 0; i < equippedCount; i++)
        {
            string savedValue = PlayerPrefs.GetString(EquippedKeyPrefix + i, "");

            if (string.IsNullOrEmpty(savedValue))
                continue;

            if (System.Enum.TryParse(savedValue, out UpgradeType loadedType) &&
                !equippedUpgrades.Contains(loadedType) &&
                equippedUpgrades.Count < MaxEquippedUpgrades)
            {
                equippedUpgrades.Add(loadedType);
            }
        }
    }

    public static string GetDisplayName(UpgradeType type)
    {
        switch (type)
        {
            case UpgradeType.SpeedBoost:
                return "Speed Boost";
            case UpgradeType.ExtraLife:
                return "Extra Life";
            case UpgradeType.CoinMagnet:
                return "Coin Magnet";
            case UpgradeType.DoubleCoins:
                return "Double Coins";
            case UpgradeType.SlowTime:
                return "Slow Time";
            case UpgradeType.SmallerPlayer:
                return "Smaller Player";
            case UpgradeType.ScoreBooster:
                return "Score Booster";
            case UpgradeType.RareCoinBoost:
                return "Rare Coin Boost";
            default:
                return type.ToString();
        }
    }
}
```
