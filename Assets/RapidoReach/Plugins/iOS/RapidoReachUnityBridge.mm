#import <Foundation/Foundation.h>
#import <RapidoReach/RapidoReach-Swift.h>

extern void UnitySendMessage(const char *object, const char *method, const char *message);

static NSString *RRString(const char *value) {
    return value ? [NSString stringWithUTF8String:value] : @"";
}

static void RRSend(NSString *target, NSString *method, NSString *requestId, NSString *payload) {
    NSDictionary *message = @{ @"requestId": requestId ?: @"", @"payload": payload ?: @"" };
    NSData *data = [NSJSONSerialization dataWithJSONObject:message options:0 error:nil];
    NSString *json = [[NSString alloc] initWithData:data encoding:NSUTF8StringEncoding];
    dispatch_async(dispatch_get_main_queue(), ^{ UnitySendMessage(target.UTF8String, method.UTF8String, json.UTF8String); });
}

static RapidoReachV2UnityAdapter *RRAdapter(NSString *baseURL) {
    static RapidoReachV2UnityAdapter *adapter;
    static NSString *configuredURL;
    if (!adapter || (baseURL.length && ![configuredURL isEqualToString:baseURL])) {
        configuredURL = baseURL.length ? baseURL : @"https://rorapps.rapidoreach.com";
        adapter = [[RapidoReachV2UnityAdapter alloc] initWithBaseURLString:configuredURL];
    }
    return adapter;
}

extern "C" {
void rrUnityInitialize(const char *placementId, const char *userId, const char *environment,
                       const char *language, const char *consent, const char *adSlotId,
                       const char *target, const char *method, const char *requestId, const char *baseUrl) {
    NSString *t = RRString(target), *m = RRString(method), *r = RRString(requestId);
    [RRAdapter(RRString(baseUrl)) initializeWithPlacementId:RRString(placementId) externalUserId:RRString(userId)
        environment:RRString(environment) language:RRString(language) consent:RRString(consent)
        adSlotId:RRString(adSlotId) completion:^(NSString *payload) { RRSend(t, m, r, payload); }];
}
void rrUnityRefresh(const char *target, const char *method, const char *requestId) {
    NSString *t=RRString(target), *m=RRString(method), *r=RRString(requestId);
    [RRAdapter(@"") refreshSessionWithCompletion:^(NSString *payload){ RRSend(t,m,r,payload); }];
}
void rrUnityRevoke(const char *target, const char *method, const char *requestId) {
    NSString *t=RRString(target), *m=RRString(method), *r=RRString(requestId);
    [RRAdapter(@"") revokeSessionWithCompletion:^(NSString *payload){ RRSend(t,m,r,payload); }];
}
void rrUnityGetOffers(const char *adSlotId, const char *cursor, const char *target, const char *method, const char *requestId) {
    NSString *t=RRString(target), *m=RRString(method), *r=RRString(requestId);
    [RRAdapter(@"") getOffersWithAdSlotId:RRString(adSlotId) cursor:RRString(cursor)
        completion:^(NSString *payload){ RRSend(t,m,r,payload); }];
}
void rrUnityGetRewardStatus(const char *offerId, const char *target, const char *method, const char *requestId) {
    NSString *t=RRString(target), *m=RRString(method), *r=RRString(requestId);
    [RRAdapter(@"") getRewardStatusWithOfferId:RRString(offerId)
        completion:^(NSString *payload){ RRSend(t,m,r,payload); }];
}
void rrUnityShowOfferwall(const char *target, const char *method, const char *requestId) {
    NSString *t=RRString(target), *m=RRString(method), *r=RRString(requestId);
    [RRAdapter(@"") showOfferwallWithCompletion:^(NSString *payload){ RRSend(t,m,r,payload); }];
}
void rrUnityIsRewardedVideoAvailable(const char *adSlotId, const char *target, const char *method, const char *requestId) {
    NSString *t=RRString(target), *m=RRString(method), *r=RRString(requestId);
    [RRAdapter(@"") isRewardedVideoAvailableWithAdSlotId:RRString(adSlotId)
        completion:^(NSString *payload){ RRSend(t,m,r,payload); }];
}
void rrUnityShowRewardedVideo(const char *adSlotId, const char *target, const char *method, const char *requestId) {
    NSString *t=RRString(target), *m=RRString(method), *r=RRString(requestId);
    [RRAdapter(@"") showRewardedVideoWithAdSlotId:RRString(adSlotId)
        completion:^(NSString *payload){ RRSend(t,m,r,payload); }];
}
void rrUnitySetEventTarget(const char *target, const char *method) {
    NSString *t=RRString(target), *m=RRString(method);
    RRAdapter(@"").eventHandler = ^(NSString *payload) {
        dispatch_async(dispatch_get_main_queue(), ^{ UnitySendMessage(t.UTF8String, m.UTF8String, payload.UTF8String); });
    };
}
void rrUnityDestroy(void) { [RRAdapter(@"") destroy]; }
}
