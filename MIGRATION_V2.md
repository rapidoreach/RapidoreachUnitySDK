# Migrating the Unity SDK from 1.x to 2.0.0

Remove the old package before importing 2.0.0 so the 1.0.2/1.1.0 AAR,
XCFramework, `UnityPluginBridge`, and global `RapidoReach` C# class cannot shadow
the v2 artifacts.

| 1.x API | 2.0.0 API |
| --- | --- |
| `RapidoReach.Initialize(apiKey, userId)` | `RapidoReachV2.Initialize(placementId, externalUserId, callback, ...)` |
| `ShowRewardCenter()` | `ShowOfferwall(callback)` |
| `IsSurveyAvailable()` | Read the server session capability/ad-slot state |
| `OnReward(quantity)` | `EventReceived` with `rewardConfirmed`; never credit locally |
| client reward hash/salt | Removed; server callbacks and reward status are authoritative |
| `SetUserIdentifier` | Reinitialize/rebind through a new short-lived v2 session |

Keep legacy survey integrations on the supported native compatibility APIs while
migrating. Do not treat an SDK event as financial authority, and do not add a
publisher callback secret, provider token, or reward calculation to Unity code.

Before release, run the golden scenario on signed Android and iOS device sample
builds: session, offers, launch, pending, confirmed, duplicate confirmation, and
reversal. Verify package/bundle mismatch, minimum/blocked version, no network,
background/foreground, close/reopen, and the editor `unsupported_platform` path.
