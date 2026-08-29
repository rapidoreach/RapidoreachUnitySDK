using System.IO;
using NUnit.Framework;
using RapidoReach.Unity;
using UnityEngine;

public sealed class RapidoReachV2ContractTests
{
    [Test]
    public void CanonicalFixtureDecodesWithoutClientRewardAuthority()
    {
        var path = Path.Combine(Application.dataPath, "..", "contracts", "performance-marketing-v2", "sdk-golden.json");
        var json = File.ReadAllText(path);
        StringAssert.Contains("\"contractVersion\": \"2.0.0\"", json);
        StringAssert.Contains("\"rewardedVideo\": false", json);
        Assert.False(json.Contains("rewardHashSalt"));
        Assert.AreEqual("2.0.0", RapidoReachVersions.Contract);

        var clicked = json.IndexOf("\"type\": \"offerClicked\"");
        var pending = json.IndexOf("\"type\": \"rewardPending\"");
        var confirmed = json.IndexOf("\"type\": \"rewardConfirmed\"");
        var duplicate = json.IndexOf("\"duplicate\": true");
        var reversed = json.IndexOf("\"type\": \"rewardReversed\"");
        Assert.That(clicked, Is.GreaterThan(0));
        Assert.That(pending, Is.GreaterThan(clicked));
        Assert.That(confirmed, Is.GreaterThan(pending));
        Assert.That(duplicate, Is.GreaterThan(confirmed));
        Assert.That(reversed, Is.GreaterThan(duplicate));
    }

    [Test]
    public void EditorReturnsExplicitUnsupportedError()
    {
        RapidoReachResponse response = null;
        RapidoReachV2.Initialize("placement", "user", value => response = value, environment: "development");
        Assert.NotNull(response);
        Assert.False(response.Ok);
        Assert.AreEqual("unsupported_platform", response.Error.code);
    }

    [Test]
    public void RewardedVideoFacadeExistsButRemainsDeviceAndCapabilityGated()
    {
        Assert.NotNull(typeof(RapidoReachV2).GetMethod("IsRewardedVideoAvailable"));
        Assert.NotNull(typeof(RapidoReachV2).GetMethod("ShowRewardedVideo"));
        RapidoReachResponse response = null;
        RapidoReachV2.IsRewardedVideoAvailable("slot_video_1", value => response = value);
        Assert.NotNull(response);
        Assert.False(response.Ok);
        Assert.AreEqual("unsupported_platform", response.Error.code);
    }
}
