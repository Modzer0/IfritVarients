// Decompiled with JetBrains decompiler
// Type: HUDBoresightState
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#nullable disable
public class HUDBoresightState : HUDWeaponState
{
  private HUDBoresightState.DisplayState displayState;
  [SerializeField]
  private Image projectedPosition;
  [SerializeField]
  private Image targetPosition;
  [SerializeField]
  private Image boresight;
  [SerializeField]
  private Image line;
  [SerializeField]
  private Text hint;
  private Vector3 gunDirectionRelative;
  private Image targetDesignator;
  private WeaponStation weaponStation;
  private Vector3 targetVel;
  private Vector3 targetVelPrev;
  private Vector3 targetAccel;
  private Vector3 targetAccelSmoothed;
  private Vector3 targetAccelSmoothingVel;
  private ControlsFilter controlsFilter;

  public override void UpdateWeaponDisplay(Aircraft aircraft, List<Unit> targetList)
  {
    int displayState1 = (int) this.displayState;
    this.transform.localPosition = Vector3.zero;
    SceneSingleton<FlightHud>.i.velocityVector.color = new Color(0.0f, 1f, 0.0f, Mathf.Clamp01((float) ((double) FastMath.Distance(SceneSingleton<FlightHud>.i.velocityVector.transform.position, this.boresight.transform.position) * 0.05000000074505806 - 0.10000000149011612)));
    Vector3 rhs = aircraft.transform.TransformDirection(this.gunDirectionRelative);
    Vector3 position = aircraft.transform.position + rhs * 3000f;
    Vector3 screenPoint = SceneSingleton<CameraStateManager>.i.mainCamera.WorldToScreenPoint(position) with
    {
      z = 0.0f
    };
    this.boresight.transform.position = screenPoint;
    this.targetDesignator.color = new Color(0.0f, 1f, 0.0f, Mathf.Clamp01((float) ((double) Vector3.Distance(this.targetDesignator.transform.position, screenPoint) * 0.05000000074505806 - 0.10000000149011612)));
    if (aircraft.gearDeployed)
    {
      this.boresight.enabled = false;
      SceneSingleton<FlightHud>.i.waterline.enabled = true;
    }
    else
    {
      this.boresight.enabled = (double) Vector3.Dot(SceneSingleton<CameraStateManager>.i.transform.forward, rhs) > 0.0;
      SceneSingleton<FlightHud>.i.waterline.enabled = false;
    }
    this.displayState = HUDBoresightState.DisplayState.Disabled;
    bool lookingAtTarget = false;
    this.boresight.color = Color.gray;
    if (this.weaponStation.Ammo > 0 && targetList.Count > 0)
    {
      Unit target = targetList[0];
      float num = FastMath.Distance(target.GlobalPosition(), aircraft.GlobalPosition()) / this.weaponInfo.targetRequirements.maxRange;
      this.displayState = HUDBoresightState.DisplayState.OutOfRange;
      if (this.DisplayLead(aircraft, target, out lookingAtTarget))
        this.displayState = (double) num >= 0.5 ? HUDBoresightState.DisplayState.ShowingLead : HUDBoresightState.DisplayState.Shoot;
    }
    if (PlayerSettings.zoomOnBoresight)
      SceneSingleton<CameraStateManager>.i.SetDesiredFoV(lookingAtTarget ? PlayerSettings.defaultFoV * 0.7f : PlayerSettings.defaultFoV, 0.0f);
    int displayState2 = (int) this.displayState;
    if (displayState1 == displayState2)
      return;
    this.UpdateDisplayState();
  }

  private bool DisplayLead(Aircraft aircraft, Unit firstTarget, out bool lookingAtTarget)
  {
    lookingAtTarget = false;
    if (!aircraft.NetworkHQ.IsTargetPositionAccurate(firstTarget, 10f))
      return false;
    GlobalPosition? aimPoint;
    this.controlsFilter.GetAim(firstTarget, out aimPoint, out GlobalPosition? _);
    if (!aimPoint.HasValue)
      return false;
    Vector3 vector3_1 = Vector3.Scale(SceneSingleton<CameraStateManager>.i.mainCamera.WorldToScreenPoint(firstTarget.transform.position), new Vector3(1f, 1f, 0.0f));
    this.targetPosition.transform.position = vector3_1;
    Vector3 vector3_2 = Vector3.Scale(SceneSingleton<CameraStateManager>.i.mainCamera.WorldToScreenPoint(aimPoint.Value.ToLocalPosition()), new Vector3(1f, 1f, 0.0f));
    Vector3 vector3_3 = Vector3.Scale(this.boresight.transform.position - vector3_2 + vector3_1, new Vector3(1f, 1f, 0.0f));
    this.projectedPosition.transform.position = PlayerSettings.lagPip ? vector3_3 : vector3_2;
    Vector3 vector3_4 = PlayerSettings.lagPip ? Vector3.zero : this.targetPosition.transform.localPosition;
    Vector3 localPosition = this.projectedPosition.transform.localPosition;
    this.line.transform.localPosition = (vector3_4 + localPosition) / 2f;
    Vector3 vector3_5 = vector3_4 - localPosition;
    this.line.transform.localRotation = Quaternion.Euler(0.0f, 0.0f, Mathf.Atan2(vector3_5.y, vector3_5.x) * 57.29578f);
    float magnitude = vector3_5.magnitude;
    this.boresight.color = FastMath.InRange(this.projectedPosition.transform.position, this.boresight.transform.position, 15f) ? Color.green : Color.yellow;
    if ((double) magnitude > 14.0)
    {
      if (!this.line.enabled)
        this.line.enabled = true;
      magnitude -= 14f;
    }
    else if (this.line.enabled)
      this.line.enabled = false;
    this.line.transform.localScale = new Vector3(magnitude, 1f, 1f);
    lookingAtTarget = (double) Vector3.Angle(aimPoint.Value - aircraft.GlobalPosition(), SceneSingleton<CameraStateManager>.i.transform.forward) < 10.0;
    return true;
  }

  private void UpdateDisplayState()
  {
    switch (this.displayState)
    {
      case HUDBoresightState.DisplayState.Disabled:
        this.projectedPosition.enabled = false;
        this.line.enabled = false;
        this.hint.text = "";
        break;
      case HUDBoresightState.DisplayState.OutOfRange:
        this.projectedPosition.enabled = false;
        this.line.enabled = false;
        this.hint.text = "OUT OF RANGE";
        break;
      case HUDBoresightState.DisplayState.ShowingLead:
        this.projectedPosition.enabled = true;
        this.hint.text = "";
        break;
      case HUDBoresightState.DisplayState.Shoot:
        this.hint.text = "SHOOT";
        this.projectedPosition.enabled = true;
        break;
    }
  }

  public override void SetHUDWeaponState(
    Image targetDesignator,
    Aircraft aircraft,
    WeaponStation weaponStation)
  {
    this.weaponStation = weaponStation;
    this.weaponInfo = weaponStation.WeaponInfo;
    this.targetDesignator = targetDesignator;
    targetDesignator.transform.localScale = Vector3.one * 0.6f;
    SceneSingleton<FlightHud>.i.waterline.enabled = false;
    SceneSingleton<FlightHud>.i.velocityVector.transform.localScale = Vector3.one * 0.7f;
    this.controlsFilter = aircraft.GetControlsFilter();
    Vector3 zero = Vector3.zero;
    foreach (Weapon weapon in weaponStation.Weapons)
      zero += weapon.transform.forward;
    this.gunDirectionRelative = aircraft.transform.InverseTransformDirection(zero);
  }

  public override void HUDFixedUpdate(Aircraft aircraft, List<Unit> targetList)
  {
    if (targetList.Count == 0)
      return;
    this.targetVel = (Object) targetList[0].rb != (Object) null ? targetList[0].rb.velocity : Vector3.zero;
    this.targetAccel = (this.targetVel - this.targetVelPrev) / Time.fixedDeltaTime;
    this.targetAccelSmoothed = Vector3.SmoothDamp(this.targetAccelSmoothed, this.targetAccel, ref this.targetAccelSmoothingVel, 0.5f);
    this.targetVelPrev = this.targetVel;
  }

  private enum DisplayState
  {
    Disabled,
    OutOfRange,
    ShowingLead,
    Shoot,
  }
}
