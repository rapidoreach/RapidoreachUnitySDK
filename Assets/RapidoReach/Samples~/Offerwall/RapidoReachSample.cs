using RapidoReach.Unity;
using UnityEngine;

public sealed class RapidoReachSample : MonoBehaviour
{
    [SerializeField] private string placementId = "DEVELOPMENT_PLACEMENT_ID";
    [SerializeField] private string externalUserId = "DEVELOPMENT_USER_ID";
    [SerializeField] private string adSlotId = "DEVELOPMENT_AD_SLOT_ID";

    private void Start()
    {
        RapidoReachV2.EventReceived += value => Debug.Log($"RapidReach v2 event: {value.type}");
        RapidoReachV2.Initialize(placementId, externalUserId, result =>
            Debug.Log(result.Ok ? "RapidReach v2 ready" : result.Error?.code), environment: "development", adSlotId: adSlotId);
    }

    public void ShowOfferwall() => RapidoReachV2.ShowOfferwall(result =>
        Debug.Log(result.Ok ? "Offerwall opened" : result.Error?.code));

    public void LoadOffers() => RapidoReachV2.GetOffers(adSlotId, result =>
        Debug.Log(result.Ok ? $"Offers: {result.Data<RapidoReachOfferPage>().items.Length}" : result.Error?.code));

    private void OnDestroy() => RapidoReachV2.Destroy();
}
