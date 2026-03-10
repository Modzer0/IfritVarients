// Decompiled with JetBrains decompiler
// Type: HUDWeaponState
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#nullable disable
public class HUDWeaponState : MonoBehaviour
{
  protected WeaponInfo weaponInfo;
  protected List<Unit> targetList;

  public virtual void UpdateWeaponDisplay(Aircraft aircraft, List<Unit> targetList)
  {
  }

  public virtual void SetHUDWeaponState(
    Image targetDesignator,
    Aircraft aircraft,
    WeaponStation weaponStation)
  {
    this.weaponInfo = weaponStation.WeaponInfo;
  }

  public virtual void HUDFixedUpdate(Aircraft aircraft, List<Unit> targetList)
  {
  }
}
