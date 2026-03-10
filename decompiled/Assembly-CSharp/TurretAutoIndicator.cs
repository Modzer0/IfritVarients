// Decompiled with JetBrains decompiler
// Type: TurretAutoIndicator
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;
using UnityEngine.UI;

#nullable disable
public class TurretAutoIndicator : HUDApp
{
  [SerializeField]
  private Image safetyIcon;

  public override void Initialize(Aircraft aircraft)
  {
    CombatHUD.onSetTurretAuto += new Action(this.TurretAutoIndicator_OnToggleAuto);
    this.safetyIcon.enabled = !SceneSingleton<CombatHUD>.i.turretAutoControl;
  }

  public new void RefreshSettings()
  {
  }

  private void OnDestroy()
  {
    CombatHUD.onSetTurretAuto -= new Action(this.TurretAutoIndicator_OnToggleAuto);
  }

  private void TurretAutoIndicator_OnToggleAuto()
  {
    this.safetyIcon.enabled = !SceneSingleton<CombatHUD>.i.turretAutoControl;
  }
}
