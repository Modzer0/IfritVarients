// Decompiled with JetBrains decompiler
// Type: Repairer
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.Networking;
using System;
using UnityEngine;

#nullable disable
public class Repairer : MonoBehaviour
{
  [SerializeField]
  private Unit attachedUnit;
  [SerializeField]
  private float radius = 1000f;
  [SerializeField]
  private float repairRate = 5f;
  [Tooltip("in seconds")]
  [SerializeField]
  private float checkInterval = 30f;
  private float lastRepairCheck;
  private Unit unitToRepair;
  private IRepairable repairInProgress;
  private float repairValue;

  private void Awake() => this.attachedUnit.onInitialize += new Action(this.Repairer_OnInitialize);

  private void Repairer_OnInitialize()
  {
    if (!NetworkManagerNuclearOption.i.Server.Active)
      return;
    this.StartSlowUpdateDelayed(5f, new Action(this.Repair));
  }

  private void SearchForRepair()
  {
    if ((double) Time.timeSinceLevelLoad - (double) this.lastRepairCheck < 30.0)
      return;
    if (this.repairInProgress != null)
    {
      if (this.repairInProgress.NeedsRepair())
        return;
      this.repairInProgress = (IRepairable) null;
      this.unitToRepair = (Unit) null;
    }
    this.lastRepairCheck = Time.timeSinceLevelLoad;
    Unit unitToRepair = this.unitToRepair;
    this.attachedUnit.NetworkHQ.TryGetUnitNeedingRepair(this.attachedUnit, this.radius, out this.unitToRepair);
    if (!((UnityEngine.Object) this.unitToRepair != (UnityEngine.Object) null) || !((UnityEngine.Object) this.unitToRepair != (UnityEngine.Object) unitToRepair) || !(this.attachedUnit is ICommandable attachedUnit))
      return;
    GlobalPosition waypoint = this.unitToRepair.startPosition - this.unitToRepair.maxRadius * 2f * ((this.unitToRepair.GlobalPosition() - this.attachedUnit.GlobalPosition()) with
    {
      y = 0.0f
    }).normalized;
    attachedUnit.UnitCommand.SetDestination(waypoint, false);
  }

  private void Repair()
  {
    if (this.attachedUnit.disabled || (double) this.attachedUnit.radarAlt > 2.0)
      return;
    this.SearchForRepair();
    if ((UnityEngine.Object) this.unitToRepair == (UnityEngine.Object) null)
      return;
    this.repairInProgress = FastMath.InRange(this.unitToRepair.startPosition, this.attachedUnit.GlobalPosition(), this.radius) ? this.unitToRepair as IRepairable : (IRepairable) null;
    if (this.repairInProgress == null || !this.repairInProgress.NeedsRepair())
    {
      this.unitToRepair = (Unit) null;
    }
    else
    {
      this.repairInProgress.Repair(this.attachedUnit, this.repairRate);
      this.repairValue += 0.01f * this.repairRate * Mathf.Sqrt(this.unitToRepair.definition.value);
      if (this.repairInProgress.NeedsRepair())
        return;
      this.lastRepairCheck = Time.timeSinceLevelLoad - 29f;
      this.repairInProgress = (IRepairable) null;
      if (!(this.attachedUnit is GroundVehicle attachedUnit))
        return;
      Player networkowner = attachedUnit.Networkowner;
      if (!((UnityEngine.Object) networkowner != (UnityEngine.Object) null) || !((UnityEngine.Object) networkowner.HQ != (UnityEngine.Object) null))
        return;
      this.repairValue = Mathf.Sqrt(this.repairValue);
      attachedUnit.Networkowner.HQ.ReportRepairAction(networkowner, this.repairValue, this.repairValue);
      this.repairValue = 0.0f;
    }
  }
}
