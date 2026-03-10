// Decompiled with JetBrains decompiler
// Type: Refueler
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.Networking;
using System;
using UnityEngine;

#nullable disable
public class Refueler : MonoBehaviour
{
  [SerializeField]
  private Unit attachedUnit;
  [SerializeField]
  private float range = 300f;
  [SerializeField]
  private bool singleUse;
  [Tooltip("in seconds")]
  [SerializeField]
  private float checkInterval = 5f;

  private void Start()
  {
    if (!NetworkManagerNuclearOption.i.Server.Active)
      return;
    this.attachedUnit.StartSlowUpdateDelayed(this.checkInterval, new Action(this.SlowUpdate));
  }

  private void SlowUpdate()
  {
    if (this.attachedUnit.disabled)
      return;
    foreach (Unit unitsInRange in BattlefieldGrid.GetUnitsInRangeEnumerable(this.attachedUnit.GlobalPosition(), this.range))
    {
      if (!((UnityEngine.Object) unitsInRange.NetworkHQ != (UnityEngine.Object) this.attachedUnit.NetworkHQ) && unitsInRange is IRefuelable refuelable && refuelable.CanRefuel() && FastMath.InRange(unitsInRange.transform.position, this.transform.position, this.range))
      {
        bool flag = false;
        float refillValue = 0.0f;
        Aircraft aircraft = (Aircraft) null;
        if (this.attachedUnit is Container attachedUnit)
        {
          Unit unit;
          if (UnitRegistry.TryGetUnit(new PersistentID?(attachedUnit.ownerID), out unit))
            aircraft = unit as Aircraft;
          if ((UnityEngine.Object) aircraft != (UnityEngine.Object) null && (UnityEngine.Object) aircraft.Player != (UnityEngine.Object) null)
            flag = true;
        }
        refuelable.Refuel(this.attachedUnit);
        if (this.singleUse)
          this.attachedUnit.Networkdisabled = true;
        if (refuelable != aircraft & flag)
          refillValue += 0.15f * Mathf.Sqrt(unitsInRange.definition.value);
        if ((double) refillValue > 0.0)
          aircraft.NetworkHQ.ReportSupplyAction(aircraft.Player, (Unit) null, refillValue, this.singleUse);
      }
    }
  }
}
