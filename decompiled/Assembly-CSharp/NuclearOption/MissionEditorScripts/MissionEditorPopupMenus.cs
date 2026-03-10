// Decompiled with JetBrains decompiler
// Type: NuclearOption.MissionEditorScripts.MissionEditorPopupMenus
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

#nullable disable
namespace NuclearOption.MissionEditorScripts;

public class MissionEditorPopupMenus : MonoBehaviour
{
  [SerializeField]
  private GameObject setupMenu;
  [SerializeField]
  private GameObject newMenu;
  [SerializeField]
  private GameObject loadMenu;
  [SerializeField]
  private GameObject saveMenu;
  private GameObject previous;
  private GameObject activeMenu;
  private bool hasSetup;

  private void Awake()
  {
    if (this.hasSetup)
      return;
    this.Setup();
  }

  private void Setup()
  {
    this.hasSetup = true;
    this.setupMenu.SetActive(false);
    this.newMenu.SetActive(false);
    this.loadMenu.SetActive(false);
    this.saveMenu.SetActive(false);
    this.gameObject.SetActive(false);
  }

  private void SetActive(GameObject active)
  {
    if (!this.hasSetup)
      this.Setup();
    if ((Object) this.activeMenu != (Object) null)
      this.activeMenu.SetActive(false);
    this.previous = this.activeMenu;
    this.activeMenu = active;
    bool flag = (Object) this.activeMenu != (Object) null;
    this.gameObject.SetActive(flag);
    CursorManager.SetFlag(CursorFlags.EditorWindow, flag);
    DynamicMap.AllowedToOpen = !flag;
    if (!flag)
      return;
    this.activeMenu.SetActive(true);
    FixLayout.ForceRebuildRecursive((RectTransform) this.activeMenu.transform);
    if (!DynamicMap.mapMaximized)
      return;
    SceneSingleton<DynamicMap>.i.Minimize();
  }

  public void ShowSetupMenu() => this.SetActive(this.setupMenu);

  public void ShowNewMenu() => this.SetActive(this.newMenu);

  public void ShowLoadMenu() => this.SetActive(this.loadMenu);

  public void ShowSaveMenu() => this.SetActive(this.saveMenu);

  public void CloseMenu() => this.SetActive((GameObject) null);

  public void GoPrevious()
  {
    this.SetActive(this.previous);
    this.previous = (GameObject) null;
  }
}
