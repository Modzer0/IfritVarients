// Decompiled with JetBrains decompiler
// Type: CameraCockpitState
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;

#nullable disable
public class CameraCockpitState : CameraBaseState
{
  private float panView;
  private float tiltView;
  private float gForce;
  private float gForcePrev;
  private float jerk;
  private float lowFreqShake;
  private float highFreqShake;
  private Vector3 velocityPrev;
  private Vector3 accelPrev;
  private Aircraft aircraft;
  private Rigidbody targetRB;
  private float FOVAdjustment;
  private float minFOV = 20f;
  private float maxFOV = 120f;
  private float maxSpeed;
  private float antiSlump;
  private Vector3 lowFreqShakeOffset;
  private Vector3 highFreqShakeOffset;
  private Vector3 accel;
  private Vector3 camRelativePos;
  private Vector3 camRelativeVel;
  private bool padLock;

  public override void EnterState(CameraStateManager cam)
  {
    this.aircraft = cam.followingUnit as Aircraft;
    this.targetRB = cam.followingUnit.rb;
    this.velocityPrev = Vector3.zero;
    if ((UnityEngine.Object) this.aircraft != (UnityEngine.Object) null)
    {
      foreach (Pilot pilot in this.aircraft.pilots)
        pilot.TogglePilotVisibility(false);
      this.aircraft.SetCockpitRenderers(true);
      if (!this.aircraft.disabled)
      {
        this.aircraft.onShake += new Action<Aircraft.OnShake>(this.CockpitCam_OnShake);
        Aircraft localAircraft;
        if (GameManager.GetLocalAircraft(out localAircraft) && (UnityEngine.Object) this.aircraft == (UnityEngine.Object) localAircraft)
          FlightHud.EnableCanvas(true);
      }
      this.aircraft.SetDoppler(false);
      this.maxSpeed = this.aircraft.definition.aircraftInfo.maxSpeed / 3.6f;
      cam.transform.rotation = this.aircraft.transform.rotation;
    }
    cam.followingUnit.cockpitViewPoint.transform.rotation = cam.followingUnit.transform.rotation;
    cam.cameraPivot.SetParent((UnityEngine.Object) cam.followingUnit.cockpitViewPoint != (UnityEngine.Object) null ? cam.followingUnit.cockpitViewPoint : cam.followingUnit.transform);
    cam.cameraPivot.localPosition = Vector3.zero;
    cam.cameraPivot.localRotation = Quaternion.identity;
    cam.transform.SetParent(cam.cameraPivot);
    cam.transform.localPosition = Vector3.zero;
    cam.transform.localRotation = Quaternion.identity;
    this.panView = 0.0f;
    this.tiltView = 0.0f;
    cam.mainCamera.nearClipPlane = 0.2f;
    cam.cockpitCamRender.enabled = true;
    CameraStateManager.cameraMode = CameraMode.cockpit;
    this.FOVAdjustment = 0.0f;
    cam.SetDesiredFoV(PlayerSettings.defaultFoV, PlayerSettings.defaultFoV);
    cam.cockpitCamRender.fieldOfView = cam.mainCamera.fieldOfView;
    this.gForcePrev = 0.0f;
    if ((UnityEngine.Object) this.aircraft != (UnityEngine.Object) null)
      this.velocityPrev = Vector3.zero;
    this.lowFreqShake = 0.0f;
  }

  public override void LeaveState(CameraStateManager cam)
  {
    cam.SetDesiredFoV(PlayerSettings.defaultExternalFoV, PlayerSettings.defaultExternalFoV);
    if ((UnityEngine.Object) this.aircraft != (UnityEngine.Object) null)
    {
      foreach (Pilot pilot in this.aircraft.pilots)
        pilot.TogglePilotVisibility(true);
      this.aircraft.onShake -= new Action<Aircraft.OnShake>(this.CockpitCam_OnShake);
      this.aircraft.SetDoppler(true);
      this.aircraft.SetCockpitRenderers(false);
    }
    AoAFeedback.RunAoAFeedback((Aircraft) null);
    cam.cockpitCamRender.enabled = false;
  }

  public override void UpdateState(CameraStateManager cam)
  {
    this.targetRB = this.aircraft.cockpit.rb;
    if (!GameManager.flightControlsEnabled)
      return;
    if (GameManager.playerInput.GetButtonTimedPressUp("Switch View", 0.0f, PlayerSettings.clickDelay))
    {
      if ((UnityEngine.Object) this.aircraft != (UnityEngine.Object) null)
        this.aircraft.onShake -= new Action<Aircraft.OnShake>(this.CockpitCam_OnShake);
      cam.SwitchState((CameraBaseState) cam.orbitState);
    }
    if (!DynamicMap.mapMaximized)
      this.FOVAdjustment -= 5f * GameManager.playerInput.GetAxis("Zoom View");
    this.FOVAdjustment = Mathf.Clamp(this.FOVAdjustment, this.minFOV - cam.desiredFOV, this.maxFOV - cam.desiredFOV);
    float b = Mathf.Clamp(cam.desiredFOV + this.FOVAdjustment, this.minFOV, this.maxFOV);
    cam.mainCamera.fieldOfView = Mathf.Lerp(cam.mainCamera.fieldOfView, b, 0.2f);
    cam.cockpitCamRender.fieldOfView = cam.mainCamera.fieldOfView;
    if (PlayerSettings.virtualJoystickEnabled)
    {
      if (GameManager.playerInput.GetButton("Free Look"))
      {
        this.panView += GameManager.playerInput.GetAxis("Pan View") * 120f * PlayerSettings.viewSensitivity * Time.unscaledDeltaTime;
        this.tiltView += (float) ((double) GameManager.playerInput.GetAxis("Tilt View") * 120.0 * (double) PlayerSettings.viewSensitivity * (double) Time.unscaledDeltaTime * (PlayerSettings.viewInvertPitch ? -1.0 : 1.0));
        CursorManager.Refresh();
      }
      else
      {
        this.panView = 0.0f;
        this.tiltView = 0.0f;
      }
    }
    else if (!Cursor.visible && !RadialMenuMain.IsInUse())
    {
      this.panView += GameManager.playerInput.GetAxis("Pan View") * 120f * PlayerSettings.viewSensitivity * Time.unscaledDeltaTime;
      this.tiltView += (float) ((double) GameManager.playerInput.GetAxis("Tilt View") * 120.0 * (double) PlayerSettings.viewSensitivity * (double) Time.unscaledDeltaTime * (PlayerSettings.viewInvertPitch ? -1.0 : 1.0));
    }
    if (GameManager.playerInput.GetButtonDown("Center"))
    {
      if (PlayerSettings.padLockTarget && (UnityEngine.Object) SceneSingleton<CombatHUD>.i.aircraft != (UnityEngine.Object) null && SceneSingleton<CombatHUD>.i.GetTargetList().Count > 0)
        this.padLock = !this.padLock;
      if (!this.padLock || SceneSingleton<CombatHUD>.i.GetTargetList().Count == 0)
      {
        this.padLock = false;
        this.panView = 0.0f;
        this.tiltView = 0.0f;
      }
    }
    if (PlayerSettings.padLockTarget && this.padLock && (UnityEngine.Object) SceneSingleton<CombatHUD>.i.aircraft != (UnityEngine.Object) null && SceneSingleton<CombatHUD>.i.GetTargetList().Count > 0)
    {
      Unit target = SceneSingleton<CombatHUD>.i.GetTargetList()[0];
      GlobalPosition knownPosition;
      if ((UnityEngine.Object) target != (UnityEngine.Object) null && SceneSingleton<CombatHUD>.i.aircraft.NetworkHQ.TryGetKnownPosition(target, out knownPosition))
      {
        Vector3 normalized = (knownPosition.ToLocalPosition() - this.aircraft.transform.position).normalized;
        Vector3 vector3_1 = Vector3.Dot(normalized, this.aircraft.transform.forward) * this.aircraft.transform.forward + Vector3.Dot(normalized, this.aircraft.transform.right) * this.aircraft.transform.right;
        Vector3 vector3_2 = Vector3.Cross(vector3_1.normalized, this.aircraft.transform.up);
        this.panView = Vector3.SignedAngle(vector3_1.normalized, this.aircraft.transform.forward, -this.aircraft.transform.up);
        this.tiltView = Vector3.SignedAngle(normalized, this.aircraft.transform.up, vector3_2.normalized) - 90f;
      }
      this.panView = Mathf.Clamp(this.panView, -165f, 165f);
      this.tiltView = Mathf.Clamp(this.tiltView, -65f, 45f);
    }
    this.panView = Mathf.Clamp(this.panView, -165f, 165f);
    this.tiltView = Mathf.Clamp(this.tiltView, -65f, 65f);
    float a = (double) cam.transform.localEulerAngles.y <= 180.0 ? cam.transform.localEulerAngles.y : cam.transform.localEulerAngles.y - 360f;
    double x = (double) Mathf.Lerp((double) cam.transform.localEulerAngles.x <= 180.0 ? cam.transform.localEulerAngles.x : cam.transform.localEulerAngles.x - 360f, this.tiltView, Mathf.Min(2f * Time.unscaledDeltaTime / Mathf.Max(PlayerSettings.viewSmoothing, 0.01f), 1f));
    float f = Mathf.Lerp(a, this.panView, Mathf.Min(2f * Time.unscaledDeltaTime / Mathf.Max(PlayerSettings.viewSmoothing, 0.01f), 1f));
    double y = (double) f;
    Quaternion baseOrientation = Quaternion.Euler((float) x, (float) y, 0.0f);
    if ((UnityEngine.Object) this.aircraft != (UnityEngine.Object) null && (UnityEngine.Object) this.aircraft.CockpitRB() != (UnityEngine.Object) null)
    {
      cam.cameraVelocity = this.aircraft.CockpitRB().velocity;
      this.camRelativePos += this.camRelativeVel * Mathf.Min(Time.deltaTime, 0.01666667f);
      if ((double) this.camRelativePos.magnitude > 0.15000000596046448)
      {
        this.camRelativeVel = Vector3.zero;
        this.camRelativePos = Vector3.ClampMagnitude(this.camRelativePos, 0.15f);
      }
    }
    cam.cameraPivot.position = cam.followingUnit.cockpitViewPoint.position + this.camRelativePos * PlayerSettings.cockpitCamInertia + this.CameraShake();
    float num = Mathf.Lerp(0.0f, 0.2f, Mathf.Abs(f * 0.015f) - 0.5f) * Mathf.Sign(f);
    if (PlayerSettings.useTrackIR)
    {
      Tuple<Vector3, Quaternion> trackIrOffset = TrackIRComponent.i.GetTrackIROffset(Vector3.right * num, baseOrientation);
      cam.transform.localPosition = new Vector3(Mathf.Clamp(trackIrOffset.Item1.x, -0.25f, 0.25f), Mathf.Clamp(trackIrOffset.Item1.y, -0.15f, 0.15f), Mathf.Clamp(trackIrOffset.Item1.z, -0.1f, 0.45f));
      cam.transform.localRotation = trackIrOffset.Item2;
    }
    else
    {
      cam.transform.localPosition = Vector3.right * num;
      cam.transform.localRotation = baseOrientation;
    }
  }

  private Vector3 CameraShake()
  {
    this.lowFreqShake = Mathf.Min(this.lowFreqShake, 1f);
    this.highFreqShake = Mathf.Min(this.highFreqShake, 1f);
    if ((double) this.lowFreqShake < 0.0099999997764825821 && (double) this.highFreqShake < 0.05000000074505806)
      return Vector3.zero;
    float num1 = 8f;
    float num2 = 16f;
    this.lowFreqShakeOffset = 0.03f * new Vector3(Mathf.PerlinNoise1D((float) ((double) Time.timeSinceLevelLoad * (double) num1 * 2.0)) - 0.5f, Mathf.PerlinNoise1D((float) ((double) Time.timeSinceLevelLoad * (double) num1 * 1.6666667461395264)) - 0.5f, Mathf.PerlinNoise1D((float) ((double) Time.timeSinceLevelLoad * (double) num1 * 1.2107000350952148)) - 0.5f);
    this.highFreqShakeOffset = 0.01f * new Vector3(Mathf.PerlinNoise1D((float) ((double) Time.timeSinceLevelLoad * (double) num2 * 2.0)) - 0.5f, Mathf.PerlinNoise1D((float) ((double) Time.timeSinceLevelLoad * (double) num2 * 1.6666667461395264)) - 0.5f, Mathf.PerlinNoise1D((float) ((double) Time.timeSinceLevelLoad * (double) num2 * 1.2107000350952148)) - 0.5f);
    this.lowFreqShakeOffset *= Mathf.Max(this.lowFreqShake - 0.01f, 0.0f);
    this.highFreqShakeOffset *= Mathf.Max(this.highFreqShake - 0.05f, 0.0f);
    return this.lowFreqShakeOffset + this.highFreqShakeOffset;
  }

  public void AddShake(float lowFreqShake, float highFreqShake)
  {
    this.lowFreqShake += lowFreqShake;
    this.highFreqShake += highFreqShake;
  }

  private void CockpitCam_OnShake(Aircraft.OnShake e)
  {
    this.lowFreqShake += e.lowFreqShake;
    this.highFreqShake += e.highFreqShake;
  }

  public override void FixedUpdateState(CameraStateManager cam)
  {
    this.gForce = 0.0f;
    this.jerk = 0.0f;
    if (!((UnityEngine.Object) this.aircraft != (UnityEngine.Object) null) || !((UnityEngine.Object) this.aircraft.CockpitRB() != (UnityEngine.Object) null) || (double) Time.deltaTime <= 0.0)
      return;
    Vector3 pointVelocity = this.aircraft.CockpitRB().GetPointVelocity(cam.transform.position);
    this.accel = this.velocityPrev == Vector3.zero ? Vector3.zero : (pointVelocity - this.velocityPrev) / Time.deltaTime;
    Vector3 vector3_1 = -500f * this.camRelativePos.magnitude * this.camRelativePos.normalized;
    this.antiSlump += Vector3.Dot(cam.transform.up, -this.camRelativePos) * 1000f * Time.deltaTime;
    Vector3 vector3_2 = vector3_1 + cam.transform.up * this.antiSlump;
    this.camRelativeVel += (-Vector3.ClampMagnitude(this.accel, 500f) + vector3_2) * Time.deltaTime;
    this.camRelativeVel -= Vector3.ClampMagnitude(this.camRelativeVel * 20f * Time.deltaTime, this.camRelativeVel.magnitude);
    this.gForce = this.accel.magnitude / 9.81f;
    this.jerk = (double) this.gForcePrev == 0.0 ? 0.0f : (this.gForce - this.gForcePrev) / Time.deltaTime;
    this.velocityPrev = pointVelocity;
    this.gForcePrev = this.gForce;
    this.accelPrev = this.accel;
    this.lowFreqShake = Mathf.Clamp(this.jerk * 0.005f, this.lowFreqShake, 1f);
    this.lowFreqShake = Mathf.Lerp(this.lowFreqShake, 0.0f, 5f * Time.fixedDeltaTime);
    this.highFreqShake = Mathf.Lerp(this.highFreqShake, 0.0f, 4f * Time.fixedDeltaTime);
    cam.cockpitRattle.volume = this.lowFreqShake;
    if (!cam.cockpitRattle.isPlaying)
    {
      if ((double) this.lowFreqShake > 0.0)
        cam.cockpitRattle.Play();
    }
    else if ((double) this.lowFreqShake == 0.0)
      cam.cockpitRattle.Stop();
    AoAFeedback.RunAoAFeedback(this.aircraft);
  }
}
