# RapidReach Unity SDK 2.0.0

Unity facade for the RapidReach v2 offerwall, offer API, and server-authoritative
reward status contract. Android and iOS delegate to the corresponding native
2.0.0 SDKs; the C# layer contains no delivery, attribution, or reward logic.

## Requirements

- Unity 2022.3 or newer (prepared and syntax-checked with Unity 6000.3.1f1)
- Android API 23 or newer
- iOS 12 or newer
- A dashboard placement whose Android package or iOS bundle identifier exactly
  matches the built player

The real offerwall is not available in the Unity Editor. Editor calls return
`unsupported_platform` so inventory is never simulated as live.

## Install

Import the versioned `RapidoreachUnitySDK-2.0.0.unitypackage`. Its native pins
are `RapidoReach-2.0.0.aar` and `RapidoReach.xcframework` 2.0.0. Do not combine
it with the legacy 1.x Unity package or a different native SDK version.

## Development quick start

```csharp
using RapidoReach.Unity;
using UnityEngine;

public sealed class Offers : MonoBehaviour
{
    public void Start()
    {
        RapidoReachV2.EventReceived += value => Debug.Log(value.type);
        RapidoReachV2.Initialize(
            "DEVELOPMENT_PLACEMENT_ID",
            "DEVELOPMENT_EXTERNAL_USER_ID",
            result => Debug.Log(result.Ok ? "ready" : result.Error.code),
            environment: "development",
            consent: "GRANTED",
            adSlotId: "DEVELOPMENT_AD_SLOT_ID");
    }

    public void Show() => RapidoReachV2.ShowOfferwall(result =>
        Debug.Log(result.Ok ? "opened" : result.Error.code));
}
```

Use only non-secret placement, ad-slot, and external-user identifiers in a
client. The session token is short-lived and owned internally by the native
facade. Reward amount, completion, provider credentials, callback secrets, and
ledger state are always server-owned.

## Public operations

- `Initialize`
- `RefreshSession`
- `RevokeSession`
- `GetOffers`
- `GetRewardStatus`
- `ShowOfferwall`
- `Destroy`
- `EventReceived`

Responses expose typed session, capability, offer/progress, reward-status,
event, and safe-error models. `rewardedVideo` remains false and has no public
Unity playback method until the separate secure native-video gate passes.

## Samples and verification

`Assets/RapidoReach/Samples~/Offerwall/RapidoReachSample.cs` contains a device
sample. The editor tests check the shared golden contract and explicit editor
limitation. Android and iOS player builds still require the normal Unity license,
platform modules, signing setup, and attached devices; none are changed or
activated by this package.

See [MIGRATION_V2.md](MIGRATION_V2.md) for 1.x migration details.
