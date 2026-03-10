// Decompiled with JetBrains decompiler
// Type: NuclearOption.Workshop.SteamWorkshopItem
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Cysharp.Threading.Tasks;
using Steamworks;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

#nullable disable
namespace NuclearOption.Workshop;

public class SteamWorkshopItem
{
  public readonly string Name;
  public readonly string Tag;
  public string Description;
  public string ImagePath;
  public bool Public;
  public string ContentPath;
  public string PreviewURL;
  public bool Subscribed;
  public CSteamID OwnerId;

  public string OwnerName { get; private set; }

  public event Action OwnerNameChanged;

  public PublishedFileId_t WorkshopId { get; private set; }

  public SteamWorkshopItem(string name, string tag, PublishedFileId_t itemId)
  {
    this.Name = name;
    this.Tag = tag;
    this.WorkshopId = itemId;
  }

  public void OnItemCreated(PublishedFileId_t itemId) => this.WorkshopId = itemId;

  public void SetOwnerName(string name)
  {
    this.OwnerName = name;
    Action ownerNameChanged = this.OwnerNameChanged;
    if (ownerNameChanged == null)
      return;
    ownerNameChanged();
  }

  public UniTask<Sprite> GetPreview(CancellationToken cancellationToken = default (CancellationToken))
  {
    return SteamWorkshop.DownloadImage(this.PreviewURL, cancellationToken);
  }

  public UniTask SetPreviewImageAsync(Image image, ref CancellationTokenSource cancellationSource)
  {
    cancellationSource?.Cancel();
    cancellationSource = (CancellationTokenSource) null;
    if (this.PreviewURL != null)
    {
      cancellationSource = new CancellationTokenSource();
      return this.SetPreviewAsync(image, cancellationSource.Token);
    }
    image.sprite = (Sprite) null;
    image.color = new Color(0.2f, 0.2f, 0.2f);
    return UniTask.CompletedTask;
  }

  private async UniTask SetPreviewAsync(Image image, CancellationToken cancellationToken)
  {
    Sprite preview = await this.GetPreview(cancellationToken);
    if ((UnityEngine.Object) preview == (UnityEngine.Object) null || (UnityEngine.Object) image == (UnityEngine.Object) null || cancellationToken.IsCancellationRequested)
      return;
    image.color = Color.white;
    image.sprite = preview;
  }

  public void OpenSteamPage() => SteamWorkshopItem.OpenSteamPage(this.WorkshopId);

  public static void OpenSteamPage(PublishedFileId_t itemId)
  {
    string pchURL = $"steam://url/CommunityFilePage/{itemId}";
    Debug.Log((object) ("Opening steam " + pchURL));
    SteamFriends.ActivateGameOverlayToWebPage(pchURL);
  }

  public void OpenLocalContent()
  {
    Debug.Log((object) ("Opening local content " + this.ContentPath));
    Application.OpenURL(this.ContentPath);
  }

  public bool IsOwner() => SteamUser.GetSteamID() == this.OwnerId;
}
