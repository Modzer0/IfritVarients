// Decompiled with JetBrains decompiler
// Type: Radar
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.Jobs;
using System;
using UnityEngine;

#nullable disable
public class Radar : TargetDetector
{
  [Range(0.0f, 1f)]
  [SerializeField]
  private float jamTolerance;
  private float jamAccumulation;
  private Aircraft aircraft;
  private PowerSupply powerSupply;
  [SerializeField]
  private float radarCone;
  [SerializeField]
  private float powerDraw;
  public RadarParams RadarParameters;
  private float powerRatio = 1f;

  public bool IsJammed() => (double) this.jamAccumulation > (double) this.jamTolerance;

  protected override void Awake()
  {
    if ((UnityEngine.Object) this.attachedUnit == (UnityEngine.Object) null)
      return;
    base.Awake();
    this.attachedUnit.onJam += new Action<Unit.JamEventArgs>(this.Radar_OnJam);
    this.powerSupply = this.attachedUnit.GetPowerSupply();
    if ((UnityEngine.Object) this.powerSupply != (UnityEngine.Object) null)
      this.powerSupply.AddUser();
    this.enabled = true;
    this.activated = true;
    this.attachedUnit.radar = (TargetDetector) this;
    if (!GameManager.IsHeadless)
      return;
    this.enabled = false;
  }

  public void ResetRotators()
  {
    foreach (TargetDetector.Rotator rotator in this.rotators)
      rotator.Reset();
  }

  protected override void OnDestroy()
  {
    base.OnDestroy();
    if ((UnityEngine.Object) this.attachedUnit != (UnityEngine.Object) null)
      this.attachedUnit.onJam -= new Action<Unit.JamEventArgs>(this.Radar_OnJam);
    if ((UnityEngine.Object) this.aircraft != (UnityEngine.Object) null)
      this.aircraft.SetRadar((Radar) null);
    if (!((UnityEngine.Object) this.powerSupply != (UnityEngine.Object) null))
      return;
    this.powerSupply.RemoveUser();
  }

  public void AttachToUnit(Unit unit)
  {
    if ((UnityEngine.Object) this.attachedUnit != (UnityEngine.Object) null)
      this.attachedUnit.onJam -= new Action<Unit.JamEventArgs>(this.Radar_OnJam);
    this.attachedUnit = unit;
    this.TargetDetector_OnInitialize();
    this.attachedUnit.onJam += new Action<Unit.JamEventArgs>(this.Radar_OnJam);
    this.powerSupply = unit.GetPowerSupply();
    if ((UnityEngine.Object) this.powerSupply != (UnityEngine.Object) null)
    {
      this.enabled = true;
      this.powerSupply.AddUser();
    }
    if (!(unit is Aircraft aircraft))
      return;
    if ((UnityEngine.Object) aircraft.radar != (UnityEngine.Object) null)
      aircraft.radar.activated = false;
    this.aircraft = aircraft;
    aircraft.SetRadar(this);
    this.activated = true;
  }

  protected override void TargetDetector_OnUnitDisabled(Unit unit)
  {
    if ((UnityEngine.Object) this.attachedUnit.NetworkHQ != (UnityEngine.Object) null && this.shared)
      this.attachedUnit.NetworkHQ.DeregisterRadar(this);
    base.TargetDetector_OnUnitDisabled(unit);
  }

  public override float GetRadarRange() => this.RadarParameters.maxRange;

  public float GetMinSignal() => this.RadarParameters.minSignal;

  private void Radar_OnJam(Unit.JamEventArgs e)
  {
    this.enabled = true;
    this.jamAccumulation += e.jamAmount / Mathf.Max(this.jamTolerance, 0.1f);
    this.jamAccumulation = Mathf.Clamp01(this.jamAccumulation);
  }

  protected override void TargetSearch()
  {
    if ((double) this.RadarParameters.maxRange > 0.0)
      this.RadarCheck();
    if ((double) this.visualRange <= 0.0)
      return;
    this.VisualCheck();
  }

  protected override void TargetDetector_OnApplyDamage(UnitPart.OnApplyDamage e)
  {
    if (!e.detached && (double) e.hitPoints > 0.0)
      return;
    this.DisableTargetDetector();
  }

  private void RadarCheck()
  {
    GlobalPosition b = this.scanner.GlobalPosition();
    foreach (FactionHQ allHq in FactionRegistry.GetAllHQs())
    {
      if (!((UnityEngine.Object) this.attachedUnit.NetworkHQ == (UnityEngine.Object) allHq))
      {
        for (int i = 0; i < allHq.factionRadarReturn.Count; ++i)
        {
          Unit unit;
          if (UnitRegistry.TryGetUnit(new PersistentID?(allHq.factionRadarReturn[i]), out unit))
          {
            IRadarReturn radarReturn = unit as IRadarReturn;
            if (FastMath.InRange(unit.GlobalPosition(), b, this.RadarParameters.maxRange * 2f) && ((double) this.radarCone <= 0.0 || (double) Vector3.Angle(unit.transform.position - this.scanner.position, this.scanner.transform.forward) <= (double) this.radarCone))
              DetectorManager.RequestRadarCheck((TargetDetector) this, unit, radarReturn);
          }
        }
      }
    }
  }

  public bool CanSeeRadarReturn(IRadarReturn radarReturn, float dist, float clutterFactor)
  {
    return (double) radarReturn.GetRadarReturn(this.scanner.position, this, this.attachedUnit, dist, clutterFactor, this.RadarParameters, true) >= (double) this.RadarParameters.minSignal && !this.IsJammed();
  }

  private void FixedUpdate()
  {
    if (!((UnityEngine.Object) this.powerSupply != (UnityEngine.Object) null) || !this.activated)
      return;
    float num = this.powerSupply.DrawPower(this.powerDraw);
    this.powerRatio = (double) this.powerDraw != 0.0 ? num / this.powerDraw : 1f;
  }

  private void Update()
  {
    for (int index = 0; index < this.rotators.Length; ++index)
      this.rotators[index].transform.localEulerAngles += this.rotators[index].axis * Time.deltaTime;
    this.jamAccumulation -= Mathf.Max(this.jamAccumulation, 0.2f) * Time.deltaTime;
    if ((double) this.jamAccumulation > 0.0)
      return;
    this.jamAccumulation = 0.0f;
    if (this.rotators.Length != 0 || !((UnityEngine.Object) this.powerSupply == (UnityEngine.Object) null))
      return;
    this.enabled = false;
  }

  public struct RadarDetectionParameters
  {
    public readonly IRadarReturn targetReturn;
    public readonly Radar radar;
    public readonly Unit detectorUnit;
    public readonly float maxRange;
    public readonly float maxSignal;
    public readonly float minSignal;
    public readonly float dopplerFactor;
    public readonly bool triggerWarning;
  }
}
