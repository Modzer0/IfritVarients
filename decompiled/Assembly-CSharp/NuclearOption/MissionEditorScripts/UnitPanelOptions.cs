// Decompiled with JetBrains decompiler
// Type: NuclearOption.MissionEditorScripts.UnitPanelOptions
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.SavedMission;
using UnityEngine;

#nullable disable
namespace NuclearOption.MissionEditorScripts;

public abstract class UnitPanelOptions : MonoBehaviour
{
  protected UnitPanel unitMenu;
  protected NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<SavedUnit> targets;

  public void Setup(UnitPanel unitMenu, NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<SavedUnit> targets)
  {
    this.unitMenu = unitMenu;
    this.targets = targets;
    this.SetupInner();
  }

  protected void OnDestroy()
  {
    if (this.targets == null)
      return;
    this.Cleanup();
    this.targets = (NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<SavedUnit>) null;
  }

  protected abstract void SetupInner();

  public abstract void OnTargetsChanged();

  public abstract void Cleanup();
}
