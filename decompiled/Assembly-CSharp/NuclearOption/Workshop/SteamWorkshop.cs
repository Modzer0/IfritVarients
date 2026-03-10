// Decompiled with JetBrains decompiler
// Type: NuclearOption.Workshop.SteamWorkshop
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Cysharp.Threading.Tasks;
using NuclearOption.ModScripts;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;

#nullable disable
namespace NuclearOption.Workshop;

public class SteamWorkshop : MonoBehaviour
{
  private const int MAX_PATH_LENGTH = 1000;
  public const string UNKNOWN_NAME = "[unknown]";
  private static PublishedFileId_t[] subscribedCache = new PublishedFileId_t[100];
  private static ArraySegment<PublishedFileId_t> Subscribed;
  private static readonly Dictionary<string, Sprite> previewCache = new Dictionary<string, Sprite>();
  private Callback<PersonaStateChange_t> _personaStateChangeCallback;
  private static readonly Dictionary<ulong, SteamWorkshopItem> ownerNamesToUpdate = new Dictionary<ulong, SteamWorkshopItem>();
  [SerializeField]
  private SteamErrorPopup errorPopup;

  private void Awake()
  {
    SteamWorkshop.ClearSteamImageCache.Create();
    this._personaStateChangeCallback = Callback<PersonaStateChange_t>.Create(new Callback<PersonaStateChange_t>.DispatchDelegate(this.OnPersonaStateChange));
  }

  private void OnDestroy()
  {
    this._personaStateChangeCallback?.Dispose();
    SteamWorkshop.ownerNamesToUpdate.Clear();
  }

  public static void ClearPreviewCache()
  {
    foreach (Sprite sprite in SteamWorkshop.previewCache.Values)
    {
      UnityEngine.Object.Destroy((UnityEngine.Object) sprite.texture);
      UnityEngine.Object.Destroy((UnityEngine.Object) sprite);
    }
    SteamWorkshop.previewCache.Clear();
  }

  private void OnPersonaStateChange(PersonaStateChange_t param)
  {
    SteamWorkshopItem steamWorkshopItem;
    if (!SteamWorkshop.ownerNamesToUpdate.TryGetValue(param.m_ulSteamID, out steamWorkshopItem))
      return;
    SteamWorkshop.ownerNamesToUpdate.Remove(param.m_ulSteamID);
    steamWorkshopItem.SetOwnerName(SteamFriends.GetFriendPersonaName(new CSteamID(param.m_ulSteamID)));
  }

  private async UniTask<(bool, EResult)> CreateItem(
    SteamWorkshopItem item,
    IProgress<(float overall, float part)> progress,
    IWorkshopCreateCallbacks createCallbacks)
  {
    (PublishedFileId_t, bool, EResult) async = await this.CreateAsync();
    PublishedFileId_t id = async.Item1;
    int num = async.Item2 ? 1 : 0;
    EResult eresult = async.Item3;
    if (num == 0)
      return (false, eresult);
    createCallbacks.OnCreateOrFail(id);
    (bool, EResult) uploadResult = await this.UpdateItem(id, item, "First Version", progress);
    if (uploadResult.Item1)
    {
      item.OnItemCreated(id);
    }
    else
    {
      createCallbacks.OnCreateOrFail(PublishedFileId_t.Invalid);
      (DeleteItemResult_t, bool) valueTuple = await SteamWorkshop.WaitAsync<DeleteItemResult_t>(SteamUGC.DeleteItem(id));
      DeleteItemResult_t deleteItemResultT = valueTuple.Item1;
      if (valueTuple.Item2 || deleteItemResultT.m_eResult != EResult.k_EResultOK)
        Debug.LogError((object) $"Failed to clear up create item, {id} after failing to update");
    }
    return uploadResult;
  }

  private void AssertSuccess(string tag, bool success)
  {
    if (success)
      return;
    Debug.LogError((object) $"Failed to set {tag} on SteamUGC");
  }

  public UniTask<(bool success, EResult)> UpdateItem(
    PublishedFileId_t itemId,
    SteamWorkshopItem item,
    string changeLog,
    IProgress<(float overall, float part)> progress = null)
  {
    UGCUpdateHandle_t ugcUpdateHandleT = SteamUGC.StartItemUpdate(SteamUtils.GetAppID(), itemId);
    this.AssertSuccess("title", SteamUGC.SetItemTitle(ugcUpdateHandleT, item.Name));
    this.AssertSuccess("description", SteamUGC.SetItemDescription(ugcUpdateHandleT, item.Description));
    this.AssertSuccess("visiblity", SteamUGC.SetItemVisibility(ugcUpdateHandleT, item.Public ? ERemoteStoragePublishedFileVisibility.k_ERemoteStoragePublishedFileVisibilityPublic : ERemoteStoragePublishedFileVisibility.k_ERemoteStoragePublishedFileVisibilityUnlisted));
    this.AssertSuccess("tag", (SteamUGC.SetItemTags(ugcUpdateHandleT, (IList<string>) new List<string>()
    {
      item.Tag
    }) ? 1 : 0) != 0);
    this.AssertSuccess("content", SteamUGC.SetItemContent(ugcUpdateHandleT, item.ContentPath));
    if (!string.IsNullOrEmpty(item.ImagePath))
      SteamUGC.SetItemPreview(ugcUpdateHandleT, item.ImagePath);
    return this.SubmitAsync(ugcUpdateHandleT, changeLog, progress);
  }

  private async UniTask<(PublishedFileId_t, bool, EResult)> CreateAsync()
  {
    (CreateItemResult_t createItemResultT, bool flag) = await SteamWorkshop.WaitAsync<CreateItemResult_t>(SteamUGC.CreateItem(SteamUtils.GetAppID(), EWorkshopFileType.k_EWorkshopFileTypeFirst));
    this.CheckLegal(createItemResultT.m_bUserNeedsToAcceptWorkshopLegalAgreement);
    string message = $"Create IOFailur={flag} Result={createItemResultT.m_eResult} item={createItemResultT.m_nPublishedFileId} need Legal={createItemResultT.m_bUserNeedsToAcceptWorkshopLegalAgreement}";
    if (flag || createItemResultT.m_eResult != EResult.k_EResultOK)
      ColorLog<SteamWorkshop>.LogError(message);
    return !flag ? (createItemResultT.m_eResult == EResult.k_EResultOK ? (createItemResultT.m_nPublishedFileId, true, createItemResultT.m_eResult) : (new PublishedFileId_t(), false, createItemResultT.m_eResult)) : (new PublishedFileId_t(), false, EResult.k_EResultNone);
  }

  private async UniTask<(bool, EResult)> SubmitAsync(
    UGCUpdateHandle_t handle,
    string changeLog,
    IProgress<(float overall, float part)> progress)
  {
    UniTask<(SubmitItemUpdateResult_t, bool)> task = SteamWorkshop.WaitAsync<SubmitItemUpdateResult_t>(SteamUGC.SubmitItemUpdate(handle, changeLog));
    float percent = 0.0f;
    while (task.Status == UniTaskStatus.Pending)
    {
      ulong punBytesProcessed;
      ulong punBytesTotal;
      EItemUpdateStatus itemUpdateProgress = SteamUGC.GetItemUpdateProgress(handle, out punBytesProcessed, out punBytesTotal);
      percent = Mathf.MoveTowards(percent, itemUpdateProgress == EItemUpdateStatus.k_EItemUpdateStatusInvalid ? 1f : (float) (itemUpdateProgress - 1) / 5f, 0.02f);
      float num = (float) punBytesProcessed / (float) punBytesTotal;
      if (!float.IsFinite(num))
        num = 0.0f;
      progress?.Report((percent, num));
      await UniTask.Yield();
    }
    (SubmitItemUpdateResult_t itemUpdateResultT, bool flag1) = await task;
    bool flag2 = !flag1 && itemUpdateResultT.m_eResult == EResult.k_EResultOK;
    string message = $"Submit bIOFailure={flag1} Result={itemUpdateResultT.m_eResult} item={itemUpdateResultT.m_nPublishedFileId} need Legal={itemUpdateResultT.m_bUserNeedsToAcceptWorkshopLegalAgreement}";
    if (!flag2)
      ColorLog<SteamWorkshop>.LogError(message);
    this.CheckLegal(itemUpdateResultT.m_bUserNeedsToAcceptWorkshopLegalAgreement);
    if (flag2)
      progress?.Report((1f, 1f));
    (bool, EResult) valueTuple = (flag2, itemUpdateResultT.m_eResult);
    task = new UniTask<(SubmitItemUpdateResult_t, bool)>();
    return valueTuple;
  }

  private void CheckLegal(bool needLegal)
  {
    if (!needLegal)
      return;
    this.errorPopup.Show("Legal Agreement Required", "You need to accept the Steam Workshop Legal Agreement.", "Open Agreement", (Action) (() => Application.OpenURL("http://steamcommunity.com/sharedfiles/workshoplegalagreement")));
  }

  private static async UniTask<(T result, bool ioFailure)> WaitAsync<T>(SteamAPICall_t request)
  {
    if (request == SteamAPICall_t.Invalid)
      throw new ArgumentException("Steam Api call is invalid");
    CallResult<T> callResult = CallResult<T>.Create();
    UniTaskCompletionSource<(T, bool)> completionSource = new UniTaskCompletionSource<(T, bool)>();
    SteamAPICall_t hAPICall = request;
    CallResult<T>.APIDispatchDelegate func = (CallResult<T>.APIDispatchDelegate) ((a, b) => completionSource.TrySetResult((a, b)));
    callResult.Set(hAPICall, func);
    return await completionSource.Task;
  }

  private static EUGCQuery OrderByToQuery(OrderBy orderBy)
  {
    switch (orderBy)
    {
      case OrderBy.Trend30Days:
        return EUGCQuery.k_EUGCQuery_RankedByTrend;
      case OrderBy.TopAllTime:
        return EUGCQuery.k_EUGCQuery_RankedByVote;
      case OrderBy.New:
        return EUGCQuery.k_EUGCQuery_RankedByPublicationDate;
      default:
        throw new ArgumentException("Invalid OrderBy value");
    }
  }

  public async UniTask<bool> RefreshItems(
    OrderBy orderBy,
    string tag,
    List<SteamWorkshopItem> results,
    uint page)
  {
    AppId_t appId = SteamUtils.GetAppID();
    UGCQueryHandle_t queryAllUgcRequest = SteamUGC.CreateQueryAllUGCRequest(SteamWorkshop.OrderByToQuery(orderBy), EUGCMatchingUGCType.k_EUGCMatchingUGCType_Items, appId, appId, page);
    SteamUGC.AddRequiredTag(queryAllUgcRequest, tag);
    await SteamWorkshop.RunItemQuery(new OrderBy?(orderBy), true, results, queryAllUgcRequest);
    return results.Count == 50;
  }

  private static async UniTask RunItemQuery(
    OrderBy? orderBy,
    bool refreshSubscribed,
    List<SteamWorkshopItem> results,
    UGCQueryHandle_t handle)
  {
    try
    {
      OrderBy? nullable = orderBy;
      OrderBy orderBy1 = OrderBy.Trend30Days;
      if (nullable.GetValueOrDefault() == orderBy1 & nullable.HasValue)
        SteamUGC.SetRankedByTrendDays(handle, 30U);
      (SteamUGCQueryCompleted_t, bool) valueTuple = await SteamWorkshop.WaitAsync<SteamUGCQueryCompleted_t>(SteamUGC.SendQueryUGCRequest(handle));
      SteamUGCQueryCompleted_t ugcQueryCompletedT = valueTuple.Item1;
      if (valueTuple.Item2)
        throw new Exception("IO Failed");
      if (ugcQueryCompletedT.m_eResult != EResult.k_EResultOK)
        throw new Exception($"Failed to refresh workshop list, result={ugcQueryCompletedT.m_eResult}");
      if (refreshSubscribed)
        SteamWorkshop.RefreshSubscribed();
      results.Clear();
      for (uint index = 0; index < ugcQueryCompletedT.m_unNumResultsReturned; ++index)
      {
        SteamUGCDetails_t pDetails;
        if (!SteamUGC.GetQueryUGCResult(handle, index, out pDetails))
          Debug.LogError((object) "Failed to get details");
        else if (pDetails.m_eResult == EResult.k_EResultFileNotFound)
          Debug.Log((object) $"Item not found {pDetails.m_nPublishedFileId} (maybe it is deleted)");
        else if (pDetails.m_eResult != EResult.k_EResultOK)
        {
          Debug.LogError((object) $"Details result not ok: {pDetails.m_eResult}");
        }
        else
        {
          CSteamID csteamId = new CSteamID(pDetails.m_ulSteamIDOwner);
          SteamWorkshopItem steamWorkshopItem = new SteamWorkshopItem(pDetails.m_rgchTitle, pDetails.m_rgchTags, pDetails.m_nPublishedFileId)
          {
            Description = pDetails.m_rgchDescription,
            OwnerId = csteamId,
            Public = pDetails.m_eVisibility == ERemoteStoragePublishedFileVisibility.k_ERemoteStoragePublishedFileVisibilityPublic,
            Subscribed = SteamWorkshop.Subscribed.Array != null && SteamWorkshop.Subscribed.Contains<PublishedFileId_t>(pDetails.m_nPublishedFileId)
          };
          string friendPersonaName = SteamFriends.GetFriendPersonaName(csteamId);
          if (friendPersonaName == "[unknown]")
          {
            SteamWorkshop.ownerNamesToUpdate.Add(steamWorkshopItem.WorkshopId.m_PublishedFileId, steamWorkshopItem);
            SteamFriends.RequestUserInformation(csteamId, true);
          }
          steamWorkshopItem.SetOwnerName(friendPersonaName);
          string pchFolder;
          if (SteamUGC.GetItemInstallInfo(steamWorkshopItem.WorkshopId, out ulong _, out pchFolder, 1000U, out uint _))
            steamWorkshopItem.ContentPath = pchFolder;
          results.Add(steamWorkshopItem);
          string pchURL;
          if (SteamUGC.GetQueryUGCPreviewURL(handle, index, out pchURL, 1000U))
            steamWorkshopItem.PreviewURL = pchURL;
          else
            Debug.Log((object) "No preview");
        }
      }
    }
    finally
    {
      SteamUGC.ReleaseQueryUGCRequest(handle);
    }
  }

  public static async UniTask<(bool success, SteamWorkshopItem details)> GetDetails(
    PublishedFileId_t id)
  {
    UGCQueryHandle_t ugcDetailsRequest = SteamUGC.CreateQueryUGCDetailsRequest(new PublishedFileId_t[1]
    {
      id
    }, 1U);
    List<SteamWorkshopItem> resultsList = new List<SteamWorkshopItem>(1);
    resultsList.Clear();
    await SteamWorkshop.RunItemQuery(new OrderBy?(OrderBy.Trend30Days), false, resultsList, ugcDetailsRequest);
    (bool, SteamWorkshopItem) details = resultsList.Count != 0 ? (true, resultsList.First<SteamWorkshopItem>()) : (false, (SteamWorkshopItem) null);
    resultsList = (List<SteamWorkshopItem>) null;
    return details;
  }

  public async UniTask DownloadItem(SteamWorkshopItem item, IProgress<float> progress = null)
  {
    (RemoteStorageSubscribePublishedFileResult_t, bool) valueTuple = await SteamWorkshop.WaitAsync<RemoteStorageSubscribePublishedFileResult_t>(SteamUGC.SubscribeItem(item.WorkshopId));
    RemoteStorageSubscribePublishedFileResult_t publishedFileResultT = valueTuple.Item1;
    if (valueTuple.Item2 || publishedFileResultT.m_eResult != EResult.k_EResultOK)
      throw new Exception("Failed to subscribe to item");
    await SteamWorkshop.DownloadAsync(item.WorkshopId, progress);
    item.Subscribed = true;
  }

  private static async UniTask DownloadAsync(PublishedFileId_t itemId, IProgress<float> progress = null)
  {
    if (!SteamUGC.DownloadItem(itemId, true))
      throw new Exception("Failed to start download");
    ulong punBytesDownloaded;
    ulong punBytesTotal;
    while (!SteamUGC.GetItemDownloadInfo(itemId, out punBytesDownloaded, out punBytesTotal))
    {
      progress?.Report((float) punBytesDownloaded / (float) punBytesTotal);
      await UniTask.Yield();
    }
  }

  public async UniTask Unsubscribe(SteamWorkshopItem item)
  {
    (RemoteStorageUnsubscribePublishedFileResult_t, bool) valueTuple = await SteamWorkshop.WaitAsync<RemoteStorageUnsubscribePublishedFileResult_t>(SteamUGC.UnsubscribeItem(item.WorkshopId));
    RemoteStorageUnsubscribePublishedFileResult_t publishedFileResultT = valueTuple.Item1;
    if (valueTuple.Item2 || publishedFileResultT.m_eResult != EResult.k_EResultOK)
      throw new Exception("Failed to subscribe to item");
    item.Subscribed = false;
  }

  public UniTask<(bool success, EResult)> CreateOrUpdateItem(
    SteamWorkshopItem item,
    string changeLog,
    IWorkshopCreateCallbacks createCallbacks,
    IProgress<(float overall, float part)> progress = null)
  {
    progress?.Report((0.0f, 0.0f));
    return item.WorkshopId == PublishedFileId_t.Invalid ? this.CreateItem(item, progress, createCallbacks) : this.UpdateItem(item.WorkshopId, item, changeLog, progress);
  }

  public static void RefreshSubscribed()
  {
    int count = SteamWorkshop.RefreshSubscribedInternal();
    SteamWorkshop.Subscribed = new ArraySegment<PublishedFileId_t>(SteamWorkshop.subscribedCache, 0, count);
  }

  private static int RefreshSubscribedInternal()
  {
    if (!SteamManager.ClientInitialized)
      return 0;
    uint numSubscribedItems = SteamUGC.GetNumSubscribedItems();
    Debug.Log((object) $"{numSubscribedItems} Subscribed Items");
    int num = checked ((int) numSubscribedItems);
    if (num > SteamWorkshop.subscribedCache.Length)
    {
      if (num > 1000000)
      {
        Debug.LogError((object) "Subscribed Count was greater than max. Clamping value");
        num = 1000000;
      }
      Array.Resize<PublishedFileId_t>(ref SteamWorkshop.subscribedCache, checked (num + 20));
    }
    return checked ((int) SteamUGC.GetSubscribedItems(SteamWorkshop.subscribedCache, (uint) SteamWorkshop.subscribedCache.Length));
  }

  public static List<SubscribedItem> GetSubscribedItems(
    bool refresh,
    SubscribedItemType? filterType)
  {
    if (refresh)
      SteamWorkshop.RefreshSubscribed();
    List<SubscribedItem> subscribedItems = new List<SubscribedItem>();
    for (int index = 0; index < SteamWorkshop.Subscribed.Count; ++index)
    {
      PublishedFileId_t publishedFileIdT = SteamWorkshop.subscribedCache[index];
      string pchFolder;
      if (!SteamUGC.GetItemInstallInfo(publishedFileIdT, out ulong _, out pchFolder, 1000U, out uint _))
      {
        Debug.LogWarning((object) $"Failed to get item info for id {publishedFileIdT.m_PublishedFileId}");
      }
      else
      {
        WorkshopJson workshop = WorkshopJson.ReadFileOrInvalid(pchFolder);
        if (!filterType.HasValue || workshop.Type == filterType.Value)
          subscribedItems.Add(new SubscribedItem(publishedFileIdT, pchFolder, workshop));
      }
    }
    return subscribedItems;
  }

  public static bool TryGetInstallFolder(PublishedFileId_t itemId, out string folder)
  {
    if (itemId == PublishedFileId_t.Invalid)
    {
      Debug.LogWarning((object) "Invalid Id");
      folder = (string) null;
      return false;
    }
    if (SteamManager.ClientInitialized)
    {
      if (SteamUGC.GetItemInstallInfo(itemId, out ulong _, out folder, 1000U, out uint _))
        return true;
      Debug.LogWarning((object) $"Failed to resolve steam folder for ({itemId})");
      return false;
    }
    if (SteamManager.ServerInitialized)
    {
      folder = (string) null;
      return false;
    }
    Debug.LogError((object) "No Steam not initialized");
    folder = (string) null;
    return false;
  }

  public static bool AnyNeedUpdates()
  {
    foreach (SubscribedItem subscribedItem in SteamWorkshop.GetSubscribedItems(true, new SubscribedItemType?()))
    {
      if (((EItemState) SteamUGC.GetItemState(subscribedItem.Id)).HasFlag((Enum) EItemState.k_EItemStateNeedsUpdate))
        return true;
    }
    return false;
  }

  public static UniTask UpdateAllSubscribedItems()
  {
    List<SubscribedItem> subscribedItems = SteamWorkshop.GetSubscribedItems(true, new SubscribedItemType?());
    List<UniTask> tasks = new List<UniTask>();
    foreach (SubscribedItem subscribedItem in subscribedItems)
    {
      if (((EItemState) SteamUGC.GetItemState(subscribedItem.Id)).HasFlag((Enum) EItemState.k_EItemStateNeedsUpdate))
        tasks.Add(SteamWorkshop.DownloadAsync(subscribedItem.Id));
    }
    return UniTask.WhenAll((IEnumerable<UniTask>) tasks);
  }

  public static async UniTask<Sprite> DownloadImage(string url, CancellationToken cancellationToken = default (CancellationToken))
  {
    if (string.IsNullOrEmpty(url))
    {
      Debug.LogError((object) "Image url was null");
      return (Sprite) null;
    }
    Sprite sprite1;
    if (SteamWorkshop.previewCache.TryGetValue(url, out sprite1))
      return sprite1;
    UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
    UnityWebRequest uniTask = await UnityAsyncExtensions.ToUniTask(request.SendWebRequest(), cancellationToken: cancellationToken);
    if (request.result == UnityWebRequest.Result.Success)
    {
      Texture2D texture = ((DownloadHandlerTexture) request.downloadHandler).texture;
      Sprite sprite2 = Sprite.Create(texture, new Rect(0.0f, 0.0f, (float) texture.width, (float) texture.height), new Vector2(0.5f, 0.5f));
      SteamWorkshop.previewCache[url] = sprite2;
      return sprite2;
    }
    Debug.LogError((object) ("Failed to download image: " + request.error));
    return (Sprite) null;
  }

  public static void OpenWorkshopPage()
  {
    SteamFriends.ActivateGameOverlayToWebPage($"steam://url/SteamWorkshopPage/{SteamUtils.GetAppID()}");
  }

  public class ClearSteamImageCache : MonoBehaviour
  {
    private static SteamWorkshop.ClearSteamImageCache instance;

    public static void Create()
    {
      if ((UnityEngine.Object) SteamWorkshop.ClearSteamImageCache.instance != (UnityEngine.Object) null)
        return;
      GameObject gameObject = new GameObject(nameof (ClearSteamImageCache), new System.Type[1]
      {
        typeof (SteamWorkshop.ClearSteamImageCache)
      });
    }

    private void Awake() => SteamWorkshop.ClearSteamImageCache.instance = this;

    private void OnDestroy() => SteamWorkshop.ClearPreviewCache();
  }
}
