// Decompiled with JetBrains decompiler
// Type: HUDNoWeaponState
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#nullable disable
public class HUDNoWeaponState : HUDWeaponState
{
  public override void UpdateWeaponDisplay(Aircraft aircraft, List<Unit> targetList)
  {
    SceneSingleton<FlightHud>.i.velocityVector.color = new Color(0.0f, 1f, 0.0f, Mathf.Clamp01((float) ((double) Vector3.Distance(SceneSingleton<FlightHud>.i.velocityVector.transform.position, SceneSingleton<CombatHUD>.i.targetDesignator.transform.position) * 0.014999999664723873 - 0.15000000596046448)));
    SceneSingleton<CombatHUD>.i.targetDesignator.color = Color.Lerp(Color.black, Color.green, Mathf.Clamp01((float) ((double) Vector3.Distance(SceneSingleton<FlightHud>.i.GetHUDCenter().position, SceneSingleton<CombatHUD>.i.targetDesignator.transform.position) * 0.014999999664723873 - 0.15000000596046448)));
    SceneSingleton<CombatHUD>.i.targetDesignator.enabled = !aircraft.gearDeployed;
  }

  public override void SetHUDWeaponState(
    Image targetDesignator,
    Aircraft aircraft,
    WeaponStation weaponStation)
  {
    SceneSingleton<FlightHud>.i.waterline.enabled = true;
    targetDesignator.transform.localScale = Vector3.one;
    SceneSingleton<FlightHud>.i.velocityVector.transform.localScale = Vector3.one;
  }
}
