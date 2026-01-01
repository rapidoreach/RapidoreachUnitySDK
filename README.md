# RapidoReach Unity SDK

Rewarded survey offerwall SDK for Unity. Show the RapidoReach offerwall, receive reward and lifecycle events, and customize the in-SDK navigation UI.

## Get an API key

Create an app in the RapidoReach dashboard and copy your API key.

## Requirements

- Unity: works with the Unity Editor, but the offerwall only runs on device (Android/iOS).
- Android: minSdk `23` (the bundled `RapidoReach-1.1.0.aar` requires it).
- iOS: minimum deployment target `12.0` (bundled `RapidoReach.xcframework`).

## Install

1) Download the latest `RapidoreachUnitySDK.unitypackage` from GitHub Releases:
`https://github.com/rapidoreach/RapidoreachUnitySDK/releases/latest`

2) Import into Unity:
- `Assets` → `Import Package` → `Custom Package...` → select `RapidoreachUnitySDK.unitypackage`

3) Verify the plugin files exist after import:
- `Assets/Plugins/RapidoReach.cs`
- Android: `Assets/Plugins/Android/RapidoReach-1.1.0.aar`
- iOS: `Assets/Plugins/iOS/RapidoReach/Source/RapidoReach.xcframework`

## Quick start

Create a GameObject (for example, `RapidoReachManager`) and attach a script like this:

```csharp
using UnityEngine;

public class RapidoReachManager : MonoBehaviour
{
    [SerializeField] private string apiKey = "YOUR_API_TOKEN";
    [SerializeField] private string userId = "YOUR_USER_ID";

    private void Start()
    {
#if UNITY_ANDROID || UNITY_IOS
        // Optional: configure staging/regional backend before init.
        // RapidoReach.UpdateBackend("https://your-backend.example", null);

        RapidoReach.Initialize(apiKey, userId);

        // Required to receive callbacks (OnReward / OnRewardCenterOpened / etc).
        RapidoReach.SetListener(gameObject.name);

        // Optional UI customization.
        RapidoReach.SetNavigationBarText(Application.productName);
        RapidoReach.SetNavigationBarColor("#211548");
        RapidoReach.SetNavigationBarTextColor("#FFFFFF");
#else
        Debug.Log("RapidoReach: supported on Android/iOS only (build & run on device).");
#endif
    }

    public void ShowOfferwall()
    {
#if UNITY_ANDROID || UNITY_IOS
        RapidoReach.ShowRewardCenter();
#endif
    }

    // --- Callbacks (UnitySendMessage) ---

    private void OnReward(string quantity)
    {
        Debug.Log($"RapidoReach OnReward: {quantity}");
    }

    private void OnRewardCenterOpened()
    {
        Debug.Log("RapidoReach OnRewardCenterOpened");
    }

    private void OnRewardCenterClosed()
    {
        Debug.Log("RapidoReach OnRewardCenterClosed");
    }

    private void RapidoReachSurveyAvailable(string available)
    {
        // "true" / "false"
        Debug.Log($"RapidoReach SurveyAvailable: {available}");
    }
}
```

## Reward attribution (recommended)

For production reward attribution, use server-to-server callbacks whenever possible.
The `userId` you pass to `RapidoReach.Initialize(apiKey, userId)` is returned in the server callback so you can credit the correct user.

## Unity API reference

All APIs live on the static `RapidoReach` class inside `Assets/Plugins/RapidoReach.cs`.

### Offerwall

- `RapidoReach.ShowRewardCenter()`: shows the offerwall.
- `RapidoReach.ShowRewardCenter(string placementId)`: Android supports `placementId`; iOS ignores the placement and shows the default offerwall.
- `RapidoReach.IsSurveyAvailable()`: returns whether surveys are available (based on latest SDK state).

### User identity

If your user logs in/out, update the user identifier:

```csharp
RapidoReach.SetUserIdentifier("NEW_USER_ID");
```

Notes:
- On iOS, `SetUserIdentifier` updates the stored user id, but you should re-run `RapidoReach.Initialize(apiKey, userId)` to refresh state.

### Backends / environments

If you use staging/regional backends:

```csharp
RapidoReach.Initialize(apiKey, userId);
```

Notes:
- Android currently ignores `rewardHashSalt` (parameter is present for parity); iOS forwards it to the native SDK.

### Customizing SDK options (navigation UI)

```csharp
RapidoReach.SetNavigationBarText("Earn Rewards");
RapidoReach.SetNavigationBarColor("#211548");
RapidoReach.SetNavigationBarTextColor("#FFFFFF");
```

Android-only (iOS no-ops in this Unity wrapper):

```csharp
RapidoReach.SetOverrideCloseButtonURL("https://example.com/close.png");
RapidoReach.SetOverrideRefreshButtonURL("https://example.com/refresh.png");
```

### Moments (Android only)

The Unity wrapper supports Moments on Android:

```csharp
RapidoReach.EnableMoments(true);
RapidoReach.ShowMomentSurvey();
```

Notes:
- iOS Moments are not supported by this Unity wrapper (these calls are no-ops on iOS).

### Profiling/testing helpers

```csharp
bool profiled = RapidoReach.IsProfiled();  // Android only; iOS always returns false in this wrapper
RapidoReach.ResetProfiler(true);           // Android only; iOS no-op
```

## Android lifecycle (recommended)

Some apps prefer to explicitly forward lifecycle:

```csharp
private void OnApplicationPause(bool paused)
{
#if UNITY_ANDROID
    if (paused) RapidoReach.OnPause();
    else RapidoReach.OnResume();
#endif
}
```

## Troubleshooting

- No callbacks firing: call `RapidoReach.SetListener(gameObject.name)` and ensure the GameObject is active and not destroyed.
- Unity Editor: offerwall won’t open in Editor; use `Build And Run` on Android/iOS.
- Android build errors: ensure your project minSdk is `23` or higher.
