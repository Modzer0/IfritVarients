// Decompiled with JetBrains decompiler
// Type: NuclearOption.Workshop.ItemDetailsWorkshopPanel
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Cysharp.Threading.Tasks;
using NuclearOption.ModScripts;
using System;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

#nullable disable
namespace NuclearOption.Workshop;

public class ItemDetailsWorkshopPanel : WorkshopMenu.WorkshopPanel
{
  [Header("References")]
  [SerializeField]
  private SteamWorkshop steamWorkshop;
  [SerializeField]
  private WorkshopMenu menu;
  [Header("UI")]
  [SerializeField]
  private TextMeshProUGUI missionName;
  [SerializeField]
  private TextMeshProUGUI owner;
  [SerializeField]
  private TextMeshProUGUI description;
  [SerializeField]
  private Image previewImage;
  [SerializeField]
  private TextMeshProUGUI subscribeText;
  [SerializeField]
  private Button subscribe;
  [SerializeField]
  private Button openSteamPage;
  [SerializeField]
  private Button showLocalFiles;
  [SerializeField]
  private GameObject progressHolder;
  [SerializeField]
  private TextMeshProUGUI progressText;
  [SerializeField]
  private Button updateButton;
  private SteamWorkshopItem item;
  private ItemDetailsWorkshopPanel.DownloadProgress downloadProgress;
  private CancellationTokenSource cancellationTokenSource;

  private void Awake()
  {
    this.subscribe.onClick.AddListener(new UnityAction(this.SubscribedClicked));
    this.openSteamPage.onClick.AddListener(new UnityAction(this.OpenSteamPage));
    this.showLocalFiles.onClick.AddListener(new UnityAction(this.ShowLocalFolder));
    this.updateButton.onClick.AddListener(new UnityAction(this.SubmitUpdate));
    if (!((UnityEngine.Object) this.steamWorkshop == (UnityEngine.Object) null))
      return;
    this.steamWorkshop = this.GetComponentInParent<SteamWorkshop>();
  }

  public void ShowItem(SteamWorkshopItem item)
  {
    if (this.item != null)
      this.item.OwnerNameChanged -= new Action(this.OnOwnerNameChanged);
    this.item = item;
    this.missionName.text = item.Name;
    item.OwnerNameChanged += new Action(this.OnOwnerNameChanged);
    this.OnOwnerNameChanged();
    this.description.text = item.Description;
    this.subscribeText.text = item.Subscribed ? "Unsubscribe" : "Subscribe";
    this.progressHolder.SetActive(false);
    this.showLocalFiles.gameObject.SetActive(item.Subscribed);
    this.updateButton.gameObject.SetActive(item.IsOwner());
    this.updateButton.interactable = true;
    item.SetPreviewImageAsync(this.previewImage, ref this.cancellationTokenSource).Forget();
  }

  private void OnOwnerNameChanged() => this.owner.text = this.item.OwnerName;

  private void SubscribedClicked()
  {
    if (this.item.Subscribed)
      this.Unsubscribe().Forget();
    else
      this.DownloadMission();
  }

  private void DownloadMission()
  {
    UniTask.Void((Func<UniTaskVoid>) (async () =>
    {
      this.downloadProgress = new ItemDetailsWorkshopPanel.DownloadProgress();
      this.SetProgressText();
      this.progressHolder.SetActive(true);
      try
      {
        await this.steamWorkshop.DownloadItem(this.item, (IProgress<float>) this.downloadProgress);
        this.RefreshSubscribed();
      }
      finally
      {
        this.progressHolder.SetActive(false);
        this.downloadProgress = (ItemDetailsWorkshopPanel.DownloadProgress) null;
      }
    }));
  }

  private async UniTaskVoid Unsubscribe()
  {
    try
    {
      await this.steamWorkshop.Unsubscribe(this.item);
      this.RefreshSubscribed();
    }
    catch (Exception ex)
    {
      Debug.LogException(ex);
    }
  }

  private void RefreshSubscribed()
  {
    this.subscribeText.text = this.item.Subscribed ? "Unsubscribe" : "Subscribe";
    this.showLocalFiles.gameObject.SetActive(this.item.Subscribed);
  }

  private void OpenSteamPage() => this.item.OpenSteamPage();

  private void ShowLocalFolder() => this.item.OpenLocalContent();

  private void SubmitUpdate()
  {
    WorkshopUploadItem workshopUploadItem;
    if (ModTypes.FromTag(this.item.Tag).TryGetLocalItem(this.item, out workshopUploadItem))
      this.menu.OpenUploadPanel(workshopUploadItem, this.item);
    else
      this.updateButton.interactable = false;
  }

  private void Update()
  {
    if (this.downloadProgress == null)
      return;
    this.SetProgressText();
  }

  private void SetProgressText()
  {
    this.progressText.text = MissionManagerDebugGui.CreateProgressBar(this.downloadProgress.Progress, 40);
  }

  internal class DownloadProgress : IProgress<float>
  {
    public float Progress;

    void IProgress<float>.Report(float value) => this.Progress = value;
  }
}
