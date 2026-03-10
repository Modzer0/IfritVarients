// Decompiled with JetBrains decompiler
// Type: HUDOptions_Item
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System.Collections.Generic;
using UnityEngine;

#nullable disable
public class HUDOptions_Item : MonoBehaviour
{
  public List<HUDOptions_ToggleButton> prioButtons;
  public bool friendlyFaction;
  public List<UnitDefinition> listUnitTypes = new List<UnitDefinition>();
  public float transparency = 1f;
  public float size = 1f;

  public void Initialize()
  {
  }

  public void SetPriority(int index)
  {
    this.transparency = (float) (0.75 + 0.125 * (double) index);
    this.size = (float) (0.75 + 0.125 * (double) index);
    if (this.prioButtons.Count == 0)
      return;
    for (int index1 = 0; index1 < this.prioButtons.Count; ++index1)
      this.prioButtons[index1].Set(index1 == index);
    SceneSingleton<HUDOptions>.i.NeedUpdateIcons();
  }

  public int ButtonIndex(HUDOptions_ToggleButton button)
  {
    int num = 0;
    if (this.prioButtons.Count > 0)
      num = this.prioButtons.IndexOf(button);
    return num;
  }

  public bool CheckFaction(FactionHQ hq)
  {
    bool flag = false;
    if ((Object) SceneSingleton<DynamicMap>.i.HQ != (Object) null && ((Object) hq == (Object) SceneSingleton<DynamicMap>.i.HQ && this.friendlyFaction || (Object) hq != (Object) SceneSingleton<DynamicMap>.i.HQ && !this.friendlyFaction))
      flag = true;
    return flag;
  }

  public bool CheckType(UnitDefinition unitType)
  {
    bool flag = false;
    if (this.listUnitTypes.Count > 0 && unitType.GetType() == this.listUnitTypes[0].GetType())
      flag = true;
    return flag;
  }
}
