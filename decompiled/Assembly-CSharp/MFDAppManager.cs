// Decompiled with JetBrains decompiler
// Type: MFDAppManager
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;

#nullable disable
public class MFDAppManager : SceneSingleton<MFDAppManager>
{
  private Aircraft aircraft;
  [SerializeField]
  private HUDApp[] apps;

  private void Start()
  {
    this.aircraft = SceneSingleton<CombatHUD>.i.aircraft;
    this.aircraft.onDisableUnit += new Action<Unit>(this.HUDAppManager_OnUnitDisable);
    PlayerSettings.OnApplyOptions += new Action(this.RefreshSettings);
    foreach (HUDApp app in this.apps)
    {
      app.Initialize(this.aircraft);
      app.RefreshSettings();
    }
  }

  private void Update()
  {
    for (int index = 0; index < this.apps.Length; ++index)
      this.apps[index].Refresh();
  }

  public void RefreshSettings()
  {
    foreach (HUDApp app in this.apps)
      app.RefreshSettings();
  }

  private void HUDAppManager_OnUnitDisable(Unit unit)
  {
    this.aircraft.onDisableUnit -= new Action<Unit>(this.HUDAppManager_OnUnitDisable);
    PlayerSettings.OnApplyOptions -= new Action(this.RefreshSettings);
    UnityEngine.Object.Destroy((UnityEngine.Object) this.gameObject);
  }

  private void OnDestroy() => PlayerSettings.OnApplyOptions -= new Action(this.RefreshSettings);
}
