// Decompiled with JetBrains decompiler
// Type: NuclearOption.MissionEditorScripts.EditorTabs
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Cysharp.Threading.Tasks;
using NuclearOption.MissionEditorScripts.Buttons;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#nullable disable
namespace NuclearOption.MissionEditorScripts;

public class EditorTabs : MonoBehaviour
{
  [SerializeField]
  private UnitSelection unitSelection;
  [SerializeField]
  private RectTransform leftPanel;
  [SerializeField]
  private PanelScrollView scrollPrefab;
  private HighlightButton currentTabButton;
  private bool rebuildRequested;
  private Dictionary<GameObject, PanelScrollView> panels = new Dictionary<GameObject, PanelScrollView>();
  private List<GameObject> enabledMenus = new List<GameObject>();

  public bool TryGetOpenTab<T>(out T foundPanel) where T : MonoBehaviour
  {
    foreach (PanelScrollView panelScrollView in this.panels.Values)
    {
      if ((Object) panelScrollView != (Object) null)
      {
        T componentInChildren = panelScrollView.GetComponentInChildren<T>();
        if ((Object) componentInChildren != (Object) null)
        {
          foundPanel = componentInChildren;
          return true;
        }
      }
    }
    foundPanel = default (T);
    return false;
  }

  public void ChangeTabMap(HighlightButton tabButton)
  {
    if (!this.SelectOrHideTab(tabButton, true))
      return;
    SceneSingleton<DynamicMap>.i.Maximize();
  }

  public void ToggleTabPrefab(HighlightButton tabButton, GameObject tabPrefab, bool clearUnit)
  {
    if ((Object) tabButton != (Object) null)
    {
      if (!this.SelectOrHideTab(tabButton, clearUnit))
        return;
      this.AddPanel(tabPrefab);
    }
    else
    {
      this.HideTab(clearUnit);
      this.AddPanel(tabPrefab);
    }
  }

  public void ToggleTabOpen(HighlightButton tabButton, GameObject menu)
  {
    if ((Object) tabButton != (Object) null)
    {
      if (!this.SelectOrHideTab(tabButton, true))
        return;
      this.EnableMenu(menu);
    }
    else
    {
      this.HideTab(true);
      this.EnableMenu(menu);
    }
  }

  private void EnableMenu(GameObject menu)
  {
    this.enabledMenus.Add(menu);
    menu.SetActive(true);
  }

  private void DisableAllMenu()
  {
    foreach (GameObject enabledMenu in this.enabledMenus)
      enabledMenu.SetActive(false);
    this.enabledMenus.Clear();
  }

  public T ChangeTab<T>(T tabPrefab, bool clearUnit) where T : Component
  {
    this.HideTab(clearUnit);
    return this.AddPanel<T>(tabPrefab);
  }

  public GameObject ChangeTab(GameObject tabPrefab, bool clearUnit)
  {
    this.HideTab(clearUnit);
    return this.AddPanel(tabPrefab);
  }

  public T AddPanel<T>(T tabPrefab) where T : Component
  {
    PanelScrollView panelScrollView = Object.Instantiate<PanelScrollView>(this.scrollPrefab, (Transform) this.leftPanel);
    T obj = panelScrollView.AddChild<T>(tabPrefab);
    this.panels.Add(obj.gameObject, panelScrollView);
    this.RequestRebuild();
    return obj;
  }

  public GameObject AddPanel(GameObject tabPrefab)
  {
    PanelScrollView panelScrollView = Object.Instantiate<PanelScrollView>(this.scrollPrefab, (Transform) this.leftPanel);
    GameObject key = panelScrollView.AddChild(tabPrefab);
    this.panels.Add(key, panelScrollView);
    this.RequestRebuild();
    return key;
  }

  public void DestroyPanel(GameObject key)
  {
    PanelScrollView panelScrollView;
    if (this.panels.TryGetValue(key, out panelScrollView))
    {
      if ((Object) panelScrollView != (Object) null)
        Object.Destroy((Object) panelScrollView.gameObject);
      this.panels.Remove(key);
    }
    else
      Debug.LogError((object) "Trying to destroy a panel, but key was not in panels Dictionary");
  }

  public void RequestRebuild()
  {
    if (this.rebuildRequested)
      return;
    this.RebuildAtEndOfFrame().Forget();
  }

  private async UniTask RebuildAtEndOfFrame()
  {
    this.rebuildRequested = true;
    await UniTask.Yield(PlayerLoopTiming.PostLateUpdate);
    this.rebuildRequested = false;
    foreach (PanelScrollView panelScrollView in this.panels.Values)
    {
      if ((Object) panelScrollView != (Object) null)
        panelScrollView.Rebuild();
    }
    LayoutRebuilder.ForceRebuildLayoutImmediate(this.leftPanel);
  }

  private bool SelectOrHideTab(HighlightButton tabButton, bool clearUnit)
  {
    int num = (Object) this.currentTabButton == (Object) tabButton ? 1 : 0;
    this.HideTab(clearUnit);
    if (num != 0)
      return false;
    this.currentTabButton = tabButton;
    this.currentTabButton.Highlight(true);
    return true;
  }

  public void HideTab(bool clearUnit)
  {
    foreach (PanelScrollView panelScrollView in this.panels.Values)
    {
      if ((Object) panelScrollView != (Object) null)
        Object.Destroy((Object) panelScrollView.gameObject);
    }
    this.panels.Clear();
    if ((Object) this.currentTabButton != (Object) null)
    {
      this.currentTabButton.Highlight(false);
      this.currentTabButton = (HighlightButton) null;
    }
    this.DisableAllMenu();
    if (clearUnit)
      this.unitSelection.ClearSelection();
    this.RequestRebuild();
  }
}
