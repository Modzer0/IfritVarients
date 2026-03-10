// Decompiled with JetBrains decompiler
// Type: NuclearOption.Workshop.WorkshopMenu
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Cysharp.Threading.Tasks;
using NuclearOption.ModScripts;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

#nullable disable
namespace NuclearOption.Workshop;

public class WorkshopMenu : MonoBehaviour
{
  [SerializeField]
  private Button updateAll;
  [SerializeField]
  private Button uploadItem;
  [SerializeField]
  private Button openWorkshopPage;
  [SerializeField]
  private Button backToListButton;
  [SerializeField]
  private ListWorkshopPanel listPanel;
  [SerializeField]
  private ItemDetailsWorkshopPanel detailsPanel;
  [SerializeField]
  private UploadWorkshopPanel uploadPanel;
  private WorkshopMenu.WorkshopPanel activePanel;

  private void SetActivePanel(WorkshopMenu.WorkshopPanel behaviour)
  {
    if ((UnityEngine.Object) this.activePanel == (UnityEngine.Object) behaviour)
      return;
    if ((UnityEngine.Object) this.activePanel != (UnityEngine.Object) null)
      this.activePanel.gameObject.SetActive(false);
    this.activePanel = behaviour;
    behaviour.gameObject.SetActive(true);
    this.uploadItem.interactable = (UnityEngine.Object) this.activePanel != (UnityEngine.Object) this.uploadPanel;
    this.backToListButton.interactable = (UnityEngine.Object) this.activePanel != (UnityEngine.Object) this.listPanel;
  }

  public void OpenListPanel() => this.SetActivePanel((WorkshopMenu.WorkshopPanel) this.listPanel);

  public void OpenUploadPanel()
  {
    this.SetActivePanel((WorkshopMenu.WorkshopPanel) this.uploadPanel);
  }

  public void OpenUploadPanel(WorkshopUploadItem item, SteamWorkshopItem details)
  {
    this.SetActivePanel((WorkshopMenu.WorkshopPanel) this.uploadPanel);
    this.uploadPanel.OpenWithItem(item, details);
  }

  public void OpenItemDetailsPanel(SteamWorkshopItem item)
  {
    this.SetActivePanel((WorkshopMenu.WorkshopPanel) this.detailsPanel);
    this.detailsPanel.ShowItem(item);
  }

  private void Start()
  {
    this.updateAll.onClick.AddListener(new UnityAction(this.UpdateAllClicked));
    this.uploadItem.onClick.AddListener(new UnityAction(this.OpenUploadPanel));
    this.backToListButton.onClick.AddListener(new UnityAction(this.OpenListPanel));
    this.openWorkshopPage.onClick.AddListener(new UnityAction(SteamWorkshop.OpenWorkshopPage));
    this.detailsPanel.gameObject.SetActive(false);
    this.uploadPanel.gameObject.SetActive(false);
    this.SetActivePanel((WorkshopMenu.WorkshopPanel) this.listPanel);
    this.updateAll.interactable = SteamWorkshop.AnyNeedUpdates();
  }

  private void UpdateAllClicked()
  {
    UniTask.Void((Func<UniTaskVoid>) (async () =>
    {
      this.updateAll.interactable = false;
      await SteamWorkshop.UpdateAllSubscribedItems();
      await UniTask.Delay(500);
      this.updateAll.interactable = SteamWorkshop.AnyNeedUpdates();
    }));
  }

  public void CloseMenu() => UnityEngine.Object.Destroy((UnityEngine.Object) this.gameObject);

  public class WorkshopPanel : MonoBehaviour
  {
  }
}
