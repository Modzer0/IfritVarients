// Decompiled with JetBrains decompiler
// Type: PilotPlayerState
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Rewired;
using Unity.Profiling;
using UnityEngine;

#nullable disable
public class PilotPlayerState : PilotBaseState
{
  private static readonly ProfilerMarker fixedUpdateStateMarker = new ProfilerMarker("PilotPlayerState.FixedUpdateState");
  private float airspeed;
  private float maxG;
  private bool collective = true;
  private GLOC gloc;
  private float pilotStrength;
  private Player player;
  private float simulatedThrottle;
  private float simulatedCustomAxis1;
  public float pitchInput;
  public float rollInput;
  public float yawInput;

  public override void EnterState(Pilot pilot)
  {
    this.stateDisplayName = "player controlled";
    this.player = ReInput.players.GetPlayer(0);
    SceneSingleton<FlightHud>.i.virtualJoystickPos.transform.localPosition = Vector3.zero;
    this.pilot = pilot;
    this.controlInputs = pilot.aircraft.GetInputs();
    this.gloc = pilot.gameObject.AddComponent<GLOC>();
    this.simulatedThrottle = (double) pilot.aircraft.radarAlt > (double) pilot.aircraft.definition.spawnOffset.y + 1.0 ? 0.4f : -1f;
    this.collective = (double) pilot.aircraft.GetAircraftParameters().takeoffDistance == 0.0;
  }

  public override void LeaveState()
  {
    this.gloc.ResetGLOC();
    SceneSingleton<CombatHUD>.i.ClearIcons();
    SceneSingleton<CombatHUD>.i.aircraft = (Aircraft) null;
    FlightHud.EnableCanvas(false);
  }

  public override void UpdateState(Pilot pilot)
  {
    if (!((Object) pilot.aircraft != (Object) null))
      return;
    this.PlayerControls();
  }

  public override void FixedUpdateState(Pilot pilot)
  {
    using (PilotPlayerState.fixedUpdateStateMarker.Auto())
    {
      this.pilotStrength = this.gloc.SimulateGLOC(pilot.gForce);
      if ((Object) pilot.aircraft == (Object) null)
        return;
      this.HUDTelemetry();
      this.PlayerAxisControls();
      pilot.aircraft.FilterInputs();
    }
  }

  private void HUDTelemetry()
  {
    Rigidbody rb = this.pilot.GetRB();
    this.airspeed = rb.velocity.magnitude;
    float num = Mathf.Max(this.pilot.transform.position.GlobalY() - this.pilot.aircraft.definition.spawnOffset.y, 0.0f);
    this.maxG = Mathf.Max(this.maxG, this.pilot.gForce);
    float climbRate = Vector3.Dot(rb.velocity, Vector3.up);
    float angleOnAxis = TargetCalc.GetAngleOnAxis(rb.transform.forward, rb.velocity, rb.transform.right);
    float radarAlt = Mathf.Min(this.pilot.aircraft.radarAlt, num);
    SceneSingleton<FlightHud>.i.SetHUDInfo(this.pilot.aircraft, this.airspeed, num, radarAlt, this.pilot.GetAccel(), this.maxG, climbRate, angleOnAxis, rb.velocity, this.controlInputs);
  }

  private void PlayerAxisControls()
  {
    if (this.pilot.aircraft.cockpit.IsDetached())
      return;
    float num1 = PlayerSettings.virtualJoystickInvertPitch ? -1f : 1f;
    if ((!PlayerSettings.virtualJoystickEnabled ? 0 : (DynamicMap.mapMaximized ? 1 : (RadialMenuMain.IsInUse() ? 1 : 0))) != 0)
    {
      this.controlInputs.pitch = Mathf.Clamp(this.pitchInput, -1f, 1f);
      this.controlInputs.roll = Mathf.Clamp(this.rollInput, -1f, 1f);
      this.controlInputs.yaw = Mathf.Clamp(this.yawInput, -1f, 1f);
    }
    else if ((double) this.pilotStrength < 0.2)
    {
      this.controlInputs.pitch = 0.0f;
      this.controlInputs.roll = 0.0f;
      this.controlInputs.yaw = 0.0f;
    }
    else
    {
      this.pitchInput = 0.0f;
      this.rollInput = 0.0f;
      this.yawInput = 0.0f;
      if (PlayerSettings.virtualJoystickEnabled)
      {
        if (!SceneSingleton<FlightHud>.i.virtualJoystickPos.gameObject.activeSelf)
          SceneSingleton<FlightHud>.i.virtualJoystickPos.gameObject.SetActive(true);
        if (!this.player.GetButton("Free Look") && CameraStateManager.cameraMode == CameraMode.cockpit)
        {
          Vector3 joystickPos = Vector3.Lerp(Vector3.ClampMagnitude(SceneSingleton<FlightHud>.i.virtualJoystickPos.transform.localPosition + (float) ((double) PlayerSettings.virtualJoystickSensitivity * (double) Mathf.Min(Time.unscaledDeltaTime, 0.1f) * 30.0) * new Vector3(GameManager.playerInput.GetAxis("Pan View"), -num1 * GameManager.playerInput.GetAxis("Tilt View"), 0.0f), 150f), Vector3.zero, PlayerSettings.virtualJoystickCentering * 2f * Time.deltaTime);
          SceneSingleton<FlightHud>.i.SetVirtualJoystick(joystickPos);
        }
        if (!DynamicMap.mapMaximized && !RadialMenuMain.IsInUse() && !Leaderboard.IsOpen())
        {
          this.pitchInput = (float) (-(double) SceneSingleton<FlightHud>.i.virtualJoystickPos.transform.localPosition.y / 150.0);
          this.rollInput = SceneSingleton<FlightHud>.i.virtualJoystickPos.transform.localPosition.x / 150f;
          if ((double) this.pilot.aircraft.radarAlt < (double) this.pilot.aircraft.definition.spawnOffset.y + 1.0)
            this.yawInput = SceneSingleton<FlightHud>.i.virtualJoystickPos.transform.localPosition.x / 150f;
        }
      }
      else if (SceneSingleton<FlightHud>.i.virtualJoystickPos.gameObject.activeSelf)
        SceneSingleton<FlightHud>.i.virtualJoystickPos.gameObject.SetActive(false);
      this.pitchInput += this.player.GetAxis("Pitch");
      this.rollInput += this.player.GetAxis("Roll");
      this.yawInput += this.player.GetAxis("Yaw");
      this.controlInputs.pitch = Mathf.Clamp(this.pitchInput, -1f, 1f);
      this.controlInputs.roll = Mathf.Clamp(this.rollInput, -1f, 1f);
      this.controlInputs.yaw = Mathf.Clamp(this.yawInput, -1f, 1f);
      float f1 = Mathf.Clamp(this.player.GetAxisRaw("Throttle"), -1f, 1f);
      float f2 = Mathf.Clamp(this.player.GetAxisRawPrev("Throttle"), -1f, 1f);
      if (PlayerSettings.throttleUseRelative)
      {
        f1 = (double) Mathf.Abs(f1) > 0.10000000149011612 ? Mathf.Sign(f1) * 1f : 0.0f;
        f2 = (double) Mathf.Abs(f2) > 0.10000000149011612 ? Mathf.Sign(f2) * 1f : 0.0f;
      }
      float f3 = Mathf.Clamp(this.player.GetAxisRaw("Custom Axis 1"), -1f, 1f);
      if (this.player.GetButton("Axis Modifier"))
      {
        f3 += f1;
        f1 = 0.0f;
      }
      float num2 = Mathf.Abs(f1 - f2);
      if ((double) num2 > 0.0 && (double) num2 < 0.5)
        this.simulatedThrottle = f1;
      else if ((double) Mathf.Abs(f1) > 0.5)
        this.simulatedThrottle += Mathf.Clamp(f1 - this.simulatedThrottle, -Time.deltaTime, Time.deltaTime);
      float num3 = this.simulatedThrottle;
      float num4 = Mathf.Clamp(this.player.GetAxisRawPrev("Custom Axis 1"), -1f, 1f);
      float num5 = Mathf.Abs(f3 - num4);
      float num6 = this.controlInputs.customAxis1;
      if ((double) num5 > 0.0 && (double) num5 < 0.5)
        num6 = f3;
      else if ((double) Mathf.Abs(f3) > 0.5)
        num6 += Mathf.Clamp(f3 - num6, -Time.deltaTime, Time.deltaTime);
      if ((double) this.controlInputs.customAxis1 != (double) num6)
        this.controlInputs.customAxis1 = Mathf.Clamp01(num6);
      if (PlayerSettings.throttleUseNegative)
        num3 = (float) (0.5 * ((double) num3 + 1.0));
      if (this.collective && PlayerSettings.invertCollective)
        num3 = 1f - num3;
      this.controlInputs.throttle = Mathf.Clamp01(num3);
    }
  }

  private void PlayerControls()
  {
    if (!GameManager.flightControlsEnabled || (double) this.pilotStrength < 0.2)
      return;
    this.controlInputs.brake = this.player.GetButton("Brake") ? 1f : 0.0f;
    if (this.player.GetButtonTimedPressUp("Next Weapon", 0.0f, PlayerSettings.clickDelay))
      this.pilot.NextWeapon();
    if (this.player.GetButtonTimedPressUp("Previous Weapon", 0.0f, PlayerSettings.clickDelay))
      this.pilot.PreviousWeapon();
    if (this.player.GetButtonTimedPressUp("Turret Control", 0.0f, PlayerSettings.clickDelay))
      SceneSingleton<CombatHUD>.i.ToggleAutoControl();
    if (this.player.GetButtonDown("Gear") && (double) this.pilot.aircraft.radarAlt > 0.20000000298023224)
    {
      if (this.pilot.aircraft.gearState == LandingGear.GearState.LockedExtended)
        this.pilot.aircraft.SetGear(false);
      if (this.pilot.aircraft.gearState == LandingGear.GearState.LockedRetracted)
        this.pilot.aircraft.SetGear(true);
    }
    if (this.player.GetButtonDown("Eject"))
    {
      this.pilot.aircraft.StartEjectionSequence();
      this.pilot.SwitchState((PilotBaseState) this.pilot.parkedState);
    }
    if (this.player.GetButton("Fire") && (!PlayerSettings.menuWeaponSafety || !Cursor.visible))
      this.pilot.Fire();
    if (this.player.GetButton("Countermeasures") && (double) this.pilot.aircraft.radarAlt > 0.20000000298023224)
    {
      if (!this.pilot.aircraft.countermeasureTrigger)
        this.pilot.aircraft.Countermeasures(true, this.pilot.aircraft.countermeasureManager.activeIndex);
    }
    else if (this.pilot.aircraft.countermeasureTrigger)
      this.pilot.aircraft.Countermeasures(false, this.pilot.aircraft.countermeasureManager.activeIndex);
    if (this.player.GetButtonTimedPressUp("Next Countermeasure", 0.0f, PlayerSettings.clickDelay))
      this.pilot.aircraft.countermeasureManager.NextCountermeasure();
    if (this.player.GetButtonTimedPressUp("Flight Assist", 0.0f, PlayerSettings.clickDelay))
      this.pilot.aircraft.TogglePitchLimiter();
    else if (this.player.GetButtonTimedPressDown("Flight Assist", PlayerSettings.pressDelay))
      this.pilot.aircraft.GetControlsFilter().ToggleAutoHover();
    if (this.player.GetButtonTimedPressUp("Radar", 0.0f, PlayerSettings.clickDelay))
    {
      if ((Object) this.pilot.aircraft.radar == (Object) null)
        return;
      this.pilot.aircraft.CmdToggleRadar();
    }
    if (this.player.GetButtonTimedPressUp("Nav Lights", 0.0f, PlayerSettings.clickDelay))
      this.pilot.aircraft.ToggleNavLights();
    if (this.player.GetButtonTimedPressUp("Toggle Engine", 0.0f, PlayerSettings.clickDelay))
      this.pilot.aircraft.CmdToggleIgnition();
    if (!this.player.GetButtonTimedPressUp("Link Guns", 0.0f, PlayerSettings.clickDelay))
      return;
    this.pilot.aircraft.weaponManager.ToggleGunsLinked();
  }
}
