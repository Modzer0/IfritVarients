// Decompiled with JetBrains decompiler
// Type: Rearmer
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.Networking;
using System;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
public class Rearmer : MonoBehaviour
{
  private readonly List<Unit> needsRearmCache = new List<Unit>();
  [SerializeField]
  private Unit unit;
  [SerializeField]
  private float range;
  [SerializeField]
  private bool aircraft;
  [SerializeField]
  private bool vehicle;
  [SerializeField]
  private bool naval;
  [SerializeField]
  private bool singleUse;
  [Tooltip("in seconds")]
  [SerializeField]
  private float checkInterval;

  private void Start()
  {
    if (!NetworkManagerNuclearOption.i.Server.Active)
      return;
    this.unit.StartSlowUpdateDelayed(this.checkInterval, new Action(this.RearmingCheck));
  }

  private void RearmingCheck()
  {
    if (this.unit.disabled || (UnityEngine.Object) this.unit.NetworkHQ == (UnityEngine.Object) null)
      return;
    bool flag = false;
    Aircraft aircraft = (Aircraft) null;
    List<Unit> listUnitsNeedingRearm;
    if (!this.unit.NetworkHQ.GetListUnitsNeedingRearm(out listUnitsNeedingRearm))
      return;
    if (this.unit is Container unit1)
    {
      Unit unit;
      if (UnitRegistry.TryGetUnit(new PersistentID?(unit1.ownerID), out unit))
        aircraft = unit as Aircraft;
      if ((UnityEngine.Object) aircraft != (UnityEngine.Object) null && (UnityEngine.Object) aircraft.Player != (UnityEngine.Object) null)
        flag = true;
    }
    this.needsRearmCache.Clear();
    foreach (Unit unit2 in listUnitsNeedingRearm)
    {
      if (!unit2.disabled && unit2 is IRearmable rearmable && !FastMath.OutOfRange(unit2.transform.position, this.transform.position, this.range) && rearmable.CanRearm(this.aircraft, this.vehicle, this.naval))
        this.needsRearmCache.Add(unit2);
    }
    if (this.needsRearmCache.Count == 0)
      return;
    float refillValue = 0.0f;
    foreach (Unit unit3 in this.needsRearmCache)
    {
      if ((UnityEngine.Object) unit3 != (UnityEngine.Object) aircraft & flag)
      {
        AmmoValue ammoValue = unit3.GetAmmoValue();
        refillValue += ammoValue.Missing;
      }
      ((IRearmable) unit3).Rearm(new RearmEventArgs()
      {
        rearmer = this.unit
      });
      if (this.singleUse)
        this.unit.Networkdisabled = true;
    }
    if ((double) refillValue > 0.0)
      aircraft.NetworkHQ.ReportSupplyAction(aircraft.Player, (Unit) null, refillValue, this.singleUse);
    this.needsRearmCache.Clear();
  }
}
