// Decompiled with JetBrains decompiler
// Type: NuclearOption.MissionEditorScripts.SidePanel
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

#nullable disable
namespace NuclearOption.MissionEditorScripts;

public class SidePanel
{
  private readonly EditorTabs editorTabs;
  public SidePanel Child;
  public SidePanel Parent;
  private GameObject panelGO;
  private ISidePanel panel;
  private bool hasPanel;

  public SidePanel(EditorTabs editorTabs, ISidePanel startingPanel = null)
  {
    this.editorTabs = editorTabs;
    this.panel = startingPanel;
  }

  public T Create<T>(T prefab) where T : Component, ISidePanel
  {
    this.Destroy();
    T obj = this.editorTabs.AddPanel<T>(prefab);
    this.panel = (ISidePanel) obj;
    this.panelGO = obj.gameObject;
    obj.Panel = this;
    this.hasPanel = true;
    return obj;
  }

  public void Refresh() => this.panel.PanelRefresh();

  public void Destroy()
  {
    if (!this.hasPanel)
      return;
    this.Child?.Destroy();
    this.editorTabs.DestroyPanel(this.panelGO);
    this.hasPanel = false;
  }
}
