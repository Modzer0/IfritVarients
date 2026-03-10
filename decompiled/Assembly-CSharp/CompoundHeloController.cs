// Decompiled with JetBrains decompiler
// Type: CompoundHeloController
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;

#nullable disable
public class CompoundHeloController : MonoBehaviour
{
  private CompoundHeloController.ThrustMode thrustMode = CompoundHeloController.ThrustMode.neutral;
  [SerializeField]
  private Aircraft aircraft;
  [SerializeField]
  private float failsafeThreshold = 50f;
  [SerializeField]
  private UnitPart[] failsafeSources;
  private float inputTarget;
  private ControlInputs inputs;

  private void Awake()
  {
    this.enabled = this.aircraft.flightAssist;
    this.aircraft.onSetFlightAssist += new Action<Aircraft.OnFlightAssistToggle>(this.CompoundHeloController_OnFlightAssist);
    this.inputs = this.aircraft.GetInputs();
    this.UpdateThrustMode(CompoundHeloController.ThrustMode.neutral);
    foreach (UnitPart failsafeSource in this.failsafeSources)
      failsafeSource.onApplyDamage += new Action<UnitPart.OnApplyDamage>(this.CompoundHeloController_OnDamage);
  }

  private void CompoundHeloController_OnFlightAssist(Aircraft.OnFlightAssistToggle e)
  {
    this.enabled = e.enabled;
  }

  private void CompoundHeloController_OnDamage(UnitPart.OnApplyDamage e)
  {
    if ((double) e.hitPoints >= (double) this.failsafeThreshold)
      return;
    this.UpdateThrustMode(CompoundHeloController.ThrustMode.failsafe);
    foreach (UnitPart failsafeSource in this.failsafeSources)
      failsafeSource.onApplyDamage -= new Action<UnitPart.OnApplyDamage>(this.CompoundHeloController_OnDamage);
  }

  private void UpdateThrustMode(CompoundHeloController.ThrustMode newThrustMode)
  {
    this.thrustMode = newThrustMode;
    if (GameManager.gameState == GameState.Encyclopedia)
      return;
    switch (this.thrustMode)
    {
      case CompoundHeloController.ThrustMode.forward:
        this.inputTarget = 1f;
        if (!((UnityEngine.Object) this.aircraft == (UnityEngine.Object) SceneSingleton<CombatHUD>.i.aircraft))
          break;
        SceneSingleton<AircraftActionsReport>.i.ReportText("Thrust mode set to Forward", 3f);
        break;
      case CompoundHeloController.ThrustMode.reverse:
        this.inputTarget = 0.0f;
        if (!((UnityEngine.Object) this.aircraft == (UnityEngine.Object) SceneSingleton<CombatHUD>.i.aircraft))
          break;
        SceneSingleton<AircraftActionsReport>.i.ReportText("Thrust mode set to Reverse", 3f);
        break;
      case CompoundHeloController.ThrustMode.neutral:
        this.inputTarget = 0.5f;
        if (!((UnityEngine.Object) this.aircraft == (UnityEngine.Object) SceneSingleton<CombatHUD>.i.aircraft))
          break;
        SceneSingleton<AircraftActionsReport>.i.ReportText("Thrust mode set to Neutral", 3f);
        break;
      case CompoundHeloController.ThrustMode.failsafe:
        this.inputTarget = 0.5f;
        if (!((UnityEngine.Object) this.aircraft == (UnityEngine.Object) SceneSingleton<CombatHUD>.i.aircraft))
          break;
        SceneSingleton<AircraftActionsReport>.i.ReportText("Thrust mode set to Fail-safe", 3f);
        break;
    }
  }

  private void FixedUpdate()
  {
    CompoundHeloController.ThrustMode newThrustMode = this.thrustMode;
    if ((double) this.aircraft.speed < 35.0)
    {
      newThrustMode = CompoundHeloController.ThrustMode.neutral;
    }
    else
    {
      double num = (double) Vector3.Dot(this.transform.forward, Vector3.up);
      if (num > 0.0 && (double) this.inputs.throttle < 0.25)
        newThrustMode = CompoundHeloController.ThrustMode.reverse;
      if (num < 0.0 && (double) this.inputs.throttle > 0.25)
        newThrustMode = CompoundHeloController.ThrustMode.forward;
    }
    if (this.thrustMode == CompoundHeloController.ThrustMode.failsafe)
      newThrustMode = CompoundHeloController.ThrustMode.failsafe;
    if (newThrustMode != this.thrustMode)
      this.UpdateThrustMode(newThrustMode);
    this.inputs.customAxis1 += Mathf.Clamp(this.inputTarget - this.inputs.customAxis1, -0.3f * Time.fixedDeltaTime, 0.3f * Time.fixedDeltaTime);
  }

  private enum ThrustMode
  {
    forward,
    reverse,
    neutral,
    failsafe,
  }
}
