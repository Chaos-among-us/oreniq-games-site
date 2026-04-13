# Release Sprint

Target release date: Friday, April 17, 2026

## Current repo status
- Core gameplay loop exists and compiles.
- Shop, inventory, daily reward, daily missions, and daily challenge are already in place.
- Analytics, IAP, and release metadata were not fully wired before this sprint.

## Locked release identity
- Company/developer name: `Oreniq Games`
- Android package: `com.oreniq.endlessdodge`

## Changes started in this sprint
- Added package manifest entries for:
  - `com.unity.services.analytics`
  - `com.unity.services.authentication`
  - `com.unity.purchasing`
- Added runtime bootstrap for Unity Services:
  - [UnityServicesBootstrap.cs](C:/Users/antho/Documents/UnityProjects/Block-dodger1/Assets/Scripts/Services/UnityServicesBootstrap.cs)
- Added launch analytics event router:
  - [LaunchAnalytics.cs](C:/Users/antho/Documents/UnityProjects/Block-dodger1/Assets/Scripts/Services/LaunchAnalytics.cs)
- Added monetization entry point and first rewarded-offer surface:
  - [MonetizationManager.cs](C:/Users/antho/Documents/UnityProjects/Block-dodger1/Assets/Scripts/Services/MonetizationManager.cs)
  - post-run double-coins flow is now wired for immediate editor testing
- Instrumented high-value events:
  - run start
  - run finish
  - soft-currency shop purchase
  - daily reward claim
  - daily mission reward claim
  - daily challenge start
  - daily challenge reward claim
  - rewarded offer request/result

## Unity dashboard setup next
1. Open the project in Unity and let Package Manager restore the new packages.
2. Link the project to Unity Services.
3. Enable Authentication, Analytics, and IAP in the Services window.
4. In Analytics Event Manager, create these custom events:
   - `session_started`
   - `run_started`
   - `run_finished`
   - `soft_shop_purchase`
   - `daily_reward_claimed`
   - `daily_mission_rewards_claimed`
   - `daily_challenge_started`
   - `daily_challenge_reward_claimed`
   - `rewarded_offer_requested`
   - `rewarded_offer_result`

## Launch blockers still outside code
- Keystore creation and backup
- App icon and feature graphic
- Privacy policy URL
- Ad mediation choice and ad-unit IDs
- Google Play Console listing and content rating setup

## Monetization recommendation
- Fastest path for launch:
  - ship coin packs with Unity IAP
  - ship one rewarded ad placement
- Best first rewarded placement:
  - post-run double coins
- Best first paid offers:
  - starter pack
  - small coin pack
  - medium coin pack
  - large coin pack

## External requirements I am using
- Google Play currently requires new apps and updates to target Android 15 / API level 35 or higher.
- Unity's docs note that as of April 1, 2026, direct Unity Ads legacy integration can see reduced performance, and Unity recommends LevelPlay mediation for best ad revenue.

## Next implementation slice after package restore
1. Replace the simulated rewarded-ad path with a real ad SDK provider.
2. Add runtime UI hooks for post-run double coins and starter-pack surfaces.
3. Add Unity IAP-backed coin packs.
4. Finish Android release metadata and device QA.
