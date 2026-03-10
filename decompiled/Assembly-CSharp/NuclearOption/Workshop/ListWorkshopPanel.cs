// Decompiled with JetBrains decompiler
// Type: NuclearOption.Workshop.ListWorkshopPanel
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Cysharp.Threading.Tasks;
using NuclearOption.Chat;
using NuclearOption.MissionEditorScripts;
using NuclearOption.ModScripts;
using NuclearOption.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

#nullable disable
namespace NuclearOption.Workshop;

public class ListWorkshopPanel : WorkshopMenu.WorkshopPanel
{
  [Header("References")]
  [SerializeField]
  private SteamWorkshop steamWorkshop;
  [SerializeField]
  private ListWorkshopPanelControls controls;
  [SerializeField]
  private Button nextPageButton;
  [Header("UI")]
  [SerializeField]
  private WorkshopList workshopList;
  [SerializeField]
  private TabController tabController;
  [Space]
  [SerializeField]
  private LoadingImage loading;
  [Header("Steam tags")]
  [SerializeField]
  private bool automaticTags = true;
  [SerializeField]
  private List<string> modTags;
  private string currentTag;
  private readonly RateLimiter rateLimiter = new RateLimiter(5, 30f);
  private bool queryInProgress;
  private readonly List<SteamWorkshopItem> drawItems = new List<SteamWorkshopItem>();
  private readonly List<SteamWorkshopItem> allItems = new List<SteamWorkshopItem>();
  private int currentPage;

  private void OnValidate()
  {
    if (!this.automaticTags)
      return;
    this.modTags.Clear();
    this.modTags.Add(ModTypes.Missions.Tag);
    this.modTags.Add(ModTypes.AircraftLivery.Tag);
  }

  private void Awake()
  {
    this.controls.RefreshButton.onClick.AddListener(new UnityAction(this.GetFirstPage));
    this.controls.ClearFilterButton.onClick.AddListener(new UnityAction(this.ClearFilter));
    this.controls.FilterNameInput.onValueChanged.AddListener((UnityAction<string>) (_ => this.RedrawList()));
    this.controls.OrderByToggleGroup.OnChangeValue += new BetterToggleGroup.ToggleIndexOn(this.OrderByToggleGroup_OnChangeValue);
    this.nextPageButton.onClick.AddListener(new UnityAction(this.GetNextPage));
    this.nextPageButton.gameObject.SetActive(false);
    this.loading.SetActive(false);
    this.tabController.TabChanged += new TabController.TabChangedDelegate(this.TabController_TabChanged);
    this.tabController.Setup(this.modTags, 0);
  }

  private void TabController_TabChanged(string tag, int index)
  {
    this.currentTag = tag;
    this.GetFirstPage();
  }

  private void OrderByToggleGroup_OnChangeValue(int index) => this.GetFirstPage();

  private void OnEnable()
  {
    this.controls.Holder.SetActive(true);
    if (this.queryInProgress)
      return;
    this.GetFirstPage();
  }

  private void OnDisable()
  {
    this.controls.Holder.SetActive(false);
    this.loading.gameObject.SetActive(false);
  }

  private void SetRequestingUI(bool value)
  {
    this.queryInProgress = value;
    this.controls.RefreshButton.interactable = !value;
    this.nextPageButton.interactable = !value;
    this.loading.SetActive(value);
  }

  private void GetFirstPage()
  {
    if (string.IsNullOrEmpty(this.currentTag))
      throw new InvalidOperationException("No tag set when refreshing items");
    if (this.rateLimiter.ShouldLimit(Time.time))
      Debug.LogWarning((object) "RefreshItems rate limit");
    else
      this.GetPage(1).Forget();
  }

  private void GetNextPage() => this.GetPage(this.currentPage + 1).Forget();

  private async UniTask GetPage(int page)
  {
    ListWorkshopPanel listWorkshopPanel = this;
    if (listWorkshopPanel.queryInProgress)
    {
      Debug.LogError((object) "RefreshItems in progress");
    }
    else
    {
      try
      {
        listWorkshopPanel.SetRequestingUI(true);
        OrderBy orderBy = (OrderBy) listWorkshopPanel.controls.OrderByToggleGroup.Value;
        listWorkshopPanel.currentPage = page;
        UniTask<bool> refreshTask = listWorkshopPanel.steamWorkshop.RefreshItems(orderBy, listWorkshopPanel.currentTag, listWorkshopPanel.allItems, (uint) page);
        while (refreshTask.Status == UniTaskStatus.Pending)
        {
          await UniTask.Yield();
          if (!listWorkshopPanel.gameObject.activeInHierarchy)
            break;
        }
        bool flag = await refreshTask;
        listWorkshopPanel.RedrawList();
        if (listWorkshopPanel.nextPageButton.gameObject.activeSelf != flag)
          listWorkshopPanel.nextPageButton.gameObject.SetActive(flag);
        FixLayout.ForceRebuildRecursive((RectTransform) listWorkshopPanel.transform);
        refreshTask = new UniTask<bool>();
      }
      finally
      {
        listWorkshopPanel.SetRequestingUI(false);
      }
    }
  }

  private void ClearFilter() => this.controls.FilterNameInput.text = string.Empty;

  private void RedrawList()
  {
    IEnumerable<SteamWorkshopItem> collection = !string.IsNullOrEmpty(this.controls.FilterNameInput.text) ? this.allItems.Where<SteamWorkshopItem>((Func<SteamWorkshopItem, bool>) (x => x.Name.Contains(this.controls.FilterNameInput.text, StringComparison.OrdinalIgnoreCase))) : (IEnumerable<SteamWorkshopItem>) this.allItems;
    this.drawItems.Clear();
    this.drawItems.AddRange(collection);
    this.workshopList.UpdateList((IReadOnlyList<SteamWorkshopItem>) this.drawItems);
  }
}
