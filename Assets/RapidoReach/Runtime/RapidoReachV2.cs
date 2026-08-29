using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace RapidoReach.Unity
{
    public static class RapidoReachV2
    {
        public static event Action<RapidoReachEvent> EventReceived;
        public static bool IsSupported => !Application.isEditor &&
            (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer);

        private const string DefaultBaseUrl = "https://rorapps.rapidoreach.com";
        private static readonly Dictionary<string, Action<RapidoReachResponse>> Callbacks = new Dictionary<string, Action<RapidoReachResponse>>();
        private static readonly object CallbackLock = new object();
#if UNITY_ANDROID && !UNITY_EDITOR
        private static AndroidJavaObject adapter;
        private static AndroidEventProxy eventProxy;
#endif
#if UNITY_IOS && !UNITY_EDITOR
        private static RapidoReachNativeReceiver receiver;
#endif

        public static void Initialize(string placementId, string externalUserId,
            Action<RapidoReachResponse> callback, string environment = "production",
            string language = null, string consent = "UNKNOWN", string adSlotId = null,
            string baseUrl = DefaultBaseUrl)
        {
            if (!RequirePlatform(callback)) return;
            var id = AddCallback(callback);
#if UNITY_ANDROID && !UNITY_EDITOR
            EnsureAndroid(baseUrl);
            adapter.Call("initialize", placementId, externalUserId, environment, language ?? "",
                consent, adSlotId ?? "", new AndroidResultProxy(id));
#elif UNITY_IOS && !UNITY_EDITOR
            EnsureIos();
            rrUnityInitialize(placementId, externalUserId, environment, language ?? "", consent,
                adSlotId ?? "", receiver.gameObject.name, nameof(RapidoReachNativeReceiver.OnNativeResult), id, baseUrl);
#endif
        }

        public static void RefreshSession(Action<RapidoReachResponse> callback) => InvokeSimple("refresh", callback);
        public static void RevokeSession(Action<RapidoReachResponse> callback) => InvokeSimple("revoke", callback);

        public static void GetOffers(string adSlotId, Action<RapidoReachResponse> callback, string cursor = null)
        {
            if (!RequirePlatform(callback)) return;
            var id = AddCallback(callback);
#if UNITY_ANDROID && !UNITY_EDITOR
            EnsureAndroid(DefaultBaseUrl);
            adapter.Call("getOffers", adSlotId, cursor ?? "", new AndroidResultProxy(id));
#elif UNITY_IOS && !UNITY_EDITOR
            EnsureIos();
            rrUnityGetOffers(adSlotId, cursor ?? "", receiver.gameObject.name,
                nameof(RapidoReachNativeReceiver.OnNativeResult), id);
#endif
        }

        public static void GetRewardStatus(string offerId, Action<RapidoReachResponse> callback)
        {
            if (!RequirePlatform(callback)) return;
            var id = AddCallback(callback);
#if UNITY_ANDROID && !UNITY_EDITOR
            EnsureAndroid(DefaultBaseUrl);
            adapter.Call("getRewardStatus", offerId, new AndroidResultProxy(id));
#elif UNITY_IOS && !UNITY_EDITOR
            EnsureIos();
            rrUnityGetRewardStatus(offerId, receiver.gameObject.name,
                nameof(RapidoReachNativeReceiver.OnNativeResult), id);
#endif
        }

        public static void ShowOfferwall(Action<RapidoReachResponse> callback)
        {
            if (!RequirePlatform(callback)) return;
            var id = AddCallback(callback);
#if UNITY_ANDROID && !UNITY_EDITOR
            EnsureAndroid(DefaultBaseUrl);
            using var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            using var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            adapter.Call("showOfferwall", activity, new AndroidResultProxy(id));
#elif UNITY_IOS && !UNITY_EDITOR
            EnsureIos();
            rrUnityShowOfferwall(receiver.gameObject.name, nameof(RapidoReachNativeReceiver.OnNativeResult), id);
#endif
        }

        public static void IsRewardedVideoAvailable(string adSlotId, Action<RapidoReachResponse> callback)
        {
            if (!RequirePlatform(callback)) return;
            var id = AddCallback(callback);
#if UNITY_ANDROID && !UNITY_EDITOR
            EnsureAndroid(DefaultBaseUrl);
            adapter.Call("isRewardedVideoAvailable", adSlotId, new AndroidResultProxy(id));
#elif UNITY_IOS && !UNITY_EDITOR
            EnsureIos();
            rrUnityIsRewardedVideoAvailable(adSlotId, receiver.gameObject.name,
                nameof(RapidoReachNativeReceiver.OnNativeResult), id);
#endif
        }

        public static void ShowRewardedVideo(string adSlotId, Action<RapidoReachResponse> callback)
        {
            if (!RequirePlatform(callback)) return;
            var id = AddCallback(callback);
#if UNITY_ANDROID && !UNITY_EDITOR
            EnsureAndroid(DefaultBaseUrl);
            using var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            using var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            adapter.Call("showRewardedVideo", activity, adSlotId, new AndroidResultProxy(id));
#elif UNITY_IOS && !UNITY_EDITOR
            EnsureIos();
            rrUnityShowRewardedVideo(adSlotId, receiver.gameObject.name,
                nameof(RapidoReachNativeReceiver.OnNativeResult), id);
#endif
        }

        public static void Destroy()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            adapter?.Call("destroy");
            adapter?.Dispose();
            adapter = null;
            eventProxy = null;
#elif UNITY_IOS && !UNITY_EDITOR
            rrUnityDestroy();
#endif
            lock (CallbackLock) Callbacks.Clear();
        }

        private static void InvokeSimple(string operation, Action<RapidoReachResponse> callback)
        {
            if (!RequirePlatform(callback)) return;
            var id = AddCallback(callback);
#if UNITY_ANDROID && !UNITY_EDITOR
            EnsureAndroid(DefaultBaseUrl);
            adapter.Call(operation == "refresh" ? "refreshSession" : "revokeSession", new AndroidResultProxy(id));
#elif UNITY_IOS && !UNITY_EDITOR
            EnsureIos();
            if (operation == "refresh") rrUnityRefresh(receiver.gameObject.name, nameof(RapidoReachNativeReceiver.OnNativeResult), id);
            else rrUnityRevoke(receiver.gameObject.name, nameof(RapidoReachNativeReceiver.OnNativeResult), id);
#endif
        }

        private static bool RequirePlatform(Action<RapidoReachResponse> callback)
        {
            if (IsSupported) return true;
            callback?.Invoke(Parse("{\"ok\":false,\"error\":{\"code\":\"unsupported_platform\",\"message\":\"The RapidReach offerwall runs only in Android or iOS device players.\",\"retryable\":false}}"));
            return false;
        }

        private static string AddCallback(Action<RapidoReachResponse> callback)
        {
            var id = Guid.NewGuid().ToString("N");
            lock (CallbackLock) Callbacks[id] = callback;
            return id;
        }

        internal static void Complete(string id, string json)
        {
            Action<RapidoReachResponse> callback = null;
            lock (CallbackLock)
            {
                if (Callbacks.TryGetValue(id, out callback)) Callbacks.Remove(id);
            }
            callback?.Invoke(Parse(json));
        }

        internal static void Emit(string json)
        {
            var envelope = JsonUtility.FromJson<RapidoReachEnvelope<RapidoReachEvent>>(json);
            if (envelope != null && envelope.ok && envelope.data != null) EventReceived?.Invoke(envelope.data);
        }

        private static RapidoReachResponse Parse(string json)
        {
            var envelope = JsonUtility.FromJson<RapidoReachEnvelope<object>>(json);
            return new RapidoReachResponse { Ok = envelope != null && envelope.ok, Json = json, Error = envelope?.error };
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        private static void EnsureAndroid(string baseUrl)
        {
            if (adapter != null) return;
            using var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            using var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            adapter = new AndroidJavaObject("com.rapidoreach.rapidoreachsdk.v2.RapidoReachV2UnityAdapter", activity, baseUrl);
            eventProxy = new AndroidEventProxy();
            adapter.Call("setEventCallback", eventProxy);
        }

        private sealed class AndroidResultProxy : AndroidJavaProxy
        {
            private readonly string id;
            internal AndroidResultProxy(string id) : base("com.rapidoreach.rapidoreachsdk.v2.RrV2UnityCallback") { this.id = id; }
            public void onResult(string json) => RapidoReachUnityDispatcher.Post(() => Complete(id, json));
        }

        private sealed class AndroidEventProxy : AndroidJavaProxy
        {
            internal AndroidEventProxy() : base("com.rapidoreach.rapidoreachsdk.v2.RrV2UnityCallback") { }
            public void onResult(string json) => RapidoReachUnityDispatcher.Post(() => Emit(json));
        }
#endif

#if UNITY_IOS && !UNITY_EDITOR
        private static void EnsureIos()
        {
            if (receiver != null) return;
            var target = new GameObject("RapidoReachV2NativeReceiver");
            UnityEngine.Object.DontDestroyOnLoad(target);
            receiver = target.AddComponent<RapidoReachNativeReceiver>();
            rrUnitySetEventTarget(target.name, nameof(RapidoReachNativeReceiver.OnNativeEvent));
        }

        [DllImport("__Internal")] private static extern void rrUnityInitialize(string placementId, string userId, string environment, string language, string consent, string adSlotId, string target, string method, string requestId, string baseUrl);
        [DllImport("__Internal")] private static extern void rrUnityRefresh(string target, string method, string requestId);
        [DllImport("__Internal")] private static extern void rrUnityRevoke(string target, string method, string requestId);
        [DllImport("__Internal")] private static extern void rrUnityGetOffers(string adSlotId, string cursor, string target, string method, string requestId);
        [DllImport("__Internal")] private static extern void rrUnityGetRewardStatus(string offerId, string target, string method, string requestId);
        [DllImport("__Internal")] private static extern void rrUnityShowOfferwall(string target, string method, string requestId);
        [DllImport("__Internal")] private static extern void rrUnityIsRewardedVideoAvailable(string adSlotId, string target, string method, string requestId);
        [DllImport("__Internal")] private static extern void rrUnityShowRewardedVideo(string adSlotId, string target, string method, string requestId);
        [DllImport("__Internal")] private static extern void rrUnitySetEventTarget(string target, string method);
        [DllImport("__Internal")] private static extern void rrUnityDestroy();
#endif
    }

    internal sealed class RapidoReachUnityDispatcher : MonoBehaviour
    {
        private static readonly ConcurrentQueue<Action> Queue = new ConcurrentQueue<Action>();
        private static RapidoReachUnityDispatcher instance;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Install()
        {
            if (instance != null) return;
            var target = new GameObject("RapidoReachV2Dispatcher");
            DontDestroyOnLoad(target);
            instance = target.AddComponent<RapidoReachUnityDispatcher>();
        }

        internal static void Post(Action action) => Queue.Enqueue(action);
        private void Update() { while (Queue.TryDequeue(out var action)) action(); }
    }

#if UNITY_IOS && !UNITY_EDITOR
    internal sealed class RapidoReachNativeReceiver : MonoBehaviour
    {
        [Serializable] private sealed class NativeMessage { public string requestId; public string payload; }
        public void OnNativeResult(string json)
        {
            var message = JsonUtility.FromJson<NativeMessage>(json);
            if (message != null) RapidoReachV2.Complete(message.requestId, message.payload);
        }
        public void OnNativeEvent(string json) => RapidoReachV2.Emit(json);
    }
#endif
}
