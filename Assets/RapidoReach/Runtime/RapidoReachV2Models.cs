using System;

namespace RapidoReach.Unity
{
    public static class RapidoReachVersions
    {
        public const string Contract = "2.0.0";
        public const string Sdk = "2.0.0";
    }

    [Serializable] public sealed class RapidoReachCapabilities
    {
        public bool offerwall;
        public bool offerApi;
        public bool rewardStatus;
        public bool surveywall;
        public bool rewardedVideo;
    }

    [Serializable] public sealed class RapidoReachAdSlot
    {
        public string adSlotId;
        public string name;
        public string type;
        public bool available;
        public string unavailableReason;
    }

    [Serializable] public sealed class RapidoReachSession
    {
        public string sessionId;
        public string issuedAt;
        public string expiresAt;
        public string contractVersion;
        public string minimumSdkVersion;
        public string recommendedSdkVersion;
        public RapidoReachCapabilities capabilities;
        public string hostedOfferwallUrl;
        public string hostedRewardedVideoUrl;
        public RapidoReachAdSlot[] adSlots;
    }

    [Serializable] public sealed class RapidoReachReward
    {
        public long minorUnits;
        public string currency;
        public int decimals;
    }

    [Serializable] public sealed class RapidoReachTask
    {
        public string goalId;
        public string title;
        public string state;
        public bool required;
        public string[] prerequisiteGoalIds;
        public string deadline;
        public RapidoReachReward reward;
    }

    [Serializable] public sealed class RapidoReachProgress
    {
        public string state;
        public RapidoReachTask[] tasks;
        public RapidoReachReward pending;
        public RapidoReachReward earned;
        public RapidoReachReward reversed;
        public string updatedAt;
    }

    [Serializable] public sealed class RapidoReachOffer
    {
        public string offerId;
        public string campaignId;
        public string revisionId;
        public string title;
        public string description;
        public RapidoReachTask[] tasks;
        public RapidoReachReward totalReward;
        public RapidoReachProgress progress;
        public string expiresAt;
    }

    [Serializable] public sealed class RapidoReachOfferPage
    {
        public RapidoReachOffer[] items;
        public string nextCursor;
        public string generatedAt;
        public string expiresAt;
        public string contractVersion;
    }

    [Serializable] public sealed class RapidoReachRewardStatus
    {
        public string offerId;
        public RapidoReachProgress progress;
        public bool supportEligible;
        public string updatedAt;
    }

    [Serializable] public sealed class RapidoReachSdkError
    {
        public string code;
        public string message;
        public bool retryable;
        public int retryAfterSeconds;
        public string traceId;
    }

    [Serializable] public sealed class RapidoReachEvent
    {
        public string type;
        public string offerId;
        public string transactionId;
        public string status;
        public RapidoReachReward reward;
        public RapidoReachSdkError error;
    }

    [Serializable] internal sealed class RapidoReachEnvelope<T>
    {
        public bool ok;
        public T data;
        public RapidoReachSdkError error;
    }

    public sealed class RapidoReachResponse
    {
        public bool Ok { get; internal set; }
        public string Json { get; internal set; }
        public RapidoReachSdkError Error { get; internal set; }

        public T Data<T>()
        {
            return UnityEngine.JsonUtility.FromJson<RapidoReachEnvelope<T>>(Json).data;
        }
    }
}
