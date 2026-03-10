// Decompiled with JetBrains decompiler
// Type: LaserDesignator
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

#nullable disable
public class LaserDesignator : MonoBehaviour
{
  [SerializeField]
  private UnitPart unitPart;
  [SerializeField]
  private int maxTargets;
  [SerializeField]
  private float range;
  [SerializeField]
  private Aircraft aircraft;
  private Transform xform;
  private List<Unit> targetList;
  private List<Unit> lasedTargets;

  private void Awake()
  {
    this.aircraft.SetLaserDesignator(this);
    this.unitPart.onPartDetached += new Action<UnitPart>(this.LaserDesignator_OnPartDetach);
    this.aircraft.onDisableUnit += new Action<Unit>(this.LaserDesignator_OnUnitDisabled);
    this.targetList = this.aircraft.weaponManager.GetTargetList();
    this.xform = this.transform;
    this.lasedTargets = new List<Unit>();
    this.LaseTargets().Forget();
    this.enabled = false;
  }

  public int GetMaxTargets() => this.maxTargets;

  private void LaserDesignator_OnPartDetach(UnitPart part)
  {
    this.unitPart.onPartDetached -= new Action<UnitPart>(this.LaserDesignator_OnPartDetach);
    this.aircraft.onDisableUnit -= new Action<Unit>(this.LaserDesignator_OnUnitDisabled);
    UnityEngine.Object.Destroy((UnityEngine.Object) this);
  }

  private void LaserDesignator_OnUnitDisabled(Unit unit)
  {
    this.unitPart.onPartDetached -= new Action<UnitPart>(this.LaserDesignator_OnPartDetach);
    this.aircraft.onDisableUnit -= new Action<Unit>(this.LaserDesignator_OnUnitDisabled);
    UnityEngine.Object.Destroy((UnityEngine.Object) this);
  }

  public Unit GetLasedTarget()
  {
    return this.lasedTargets.Count == 0 ? (Unit) null : this.lasedTargets[Mathf.FloorToInt(UnityEngine.Random.Range(0.0f, (float) this.lasedTargets.Count - 1E-05f))];
  }

  private void OnDestroy()
  {
    foreach (Unit lasedTarget in this.lasedTargets)
    {
      FactionHQ networkHq = this.aircraft?.NetworkHQ;
      if ((UnityEngine.Object) networkHq != (UnityEngine.Object) null)
        networkHq.UpdateLasedState(lasedTarget, false);
    }
  }

  private async UniTask LaseTargets()
  {
    LaserDesignator laserDesignator = this;
    CancellationToken cancel = laserDesignator.destroyCancellationToken;
    while (!cancel.IsCancellationRequested)
    {
      foreach (Unit lasedTarget in laserDesignator.lasedTargets)
        laserDesignator.aircraft.NetworkHQ.UpdateLasedState(lasedTarget, false);
      laserDesignator.lasedTargets.Clear();
      for (int index = 0; index < laserDesignator.targetList.Count && index < laserDesignator.maxTargets; ++index)
      {
        Unit target = laserDesignator.targetList[index];
        if (!((UnityEngine.Object) target == (UnityEngine.Object) null) && laserDesignator.aircraft.NetworkHQ.IsTargetPositionAccurate(target, 100f) && FastMath.InRange(target.GlobalPosition(), laserDesignator.xform.GlobalPosition(), laserDesignator.range) && target.LineOfSight(laserDesignator.xform.position, 1000f))
        {
          laserDesignator.lasedTargets.Add(target);
          laserDesignator.aircraft.NetworkHQ.UpdateLasedState(target, true);
          if (laserDesignator.aircraft.LocalSim && (UnityEngine.Object) target.NetworkHQ != (UnityEngine.Object) laserDesignator.aircraft.NetworkHQ && (double) Time.timeSinceLevelLoad - (double) laserDesignator.aircraft.NetworkHQ.GetTrackingData(target.persistentID).lastSpottedTime > 2.0)
            laserDesignator.aircraft.NetworkHQ.CmdUpdateTrackingInfo(target.persistentID);
        }
      }
      await UniTask.Delay(250);
      await UniTask.WaitForFixedUpdate();
    }
    cancel = new CancellationToken();
  }

  public List<Unit> GetLasedTargets() => this.lasedTargets;

  public int LasedTargetCount() => this.lasedTargets.Count;

  public bool IsLased(Unit unit) => this.lasedTargets.Contains(unit);
}
