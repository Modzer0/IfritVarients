// Decompiled with JetBrains decompiler
// Type: FlightHud
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;
using UnityEngine.UI;

#nullable disable
public class FlightHud : SceneSingleton<FlightHud>
{
  [SerializeField]
  private Canvas canvas;
  [SerializeField]
  private Transform HUDCenter;
  [SerializeField]
  private RawImage compass;
  [SerializeField]
  private GameObject pitchCompassCenter;
  [SerializeField]
  private RawImage pitchCompass;
  public Transform statusAnchor;
  public Transform HMDCenter;
  public Image waterline;
  private Aircraft playerVehicle;
  public Image virtualJoystickPos;
  [SerializeField]
  private Image virtualJoystickVector;
  public Image velocityVector;

  public static void ResetAircraft()
  {
  }

  public static void EnableCanvas(bool enable)
  {
    if ((UnityEngine.Object) SceneSingleton<FlightHud>.i == (UnityEngine.Object) null)
    {
      if (!enable)
        return;
      Debug.LogWarning((object) "FlightHud enabled after it was destroyed");
    }
    else
    {
      SceneSingleton<FlightHud>.i.canvas.gameObject.SetActive(enable);
      if (!enable)
        return;
      if ((UnityEngine.Object) SceneSingleton<HeadMountedDisplay>.i != (UnityEngine.Object) null)
        SceneSingleton<HeadMountedDisplay>.i.RefreshSettings();
      if ((UnityEngine.Object) SceneSingleton<FlightHud>.i != (UnityEngine.Object) null)
        SceneSingleton<FlightHud>.i.RefreshSettings();
      if ((UnityEngine.Object) SceneSingleton<CombatHUD>.i != (UnityEngine.Object) null)
        SceneSingleton<CombatHUD>.i.RefreshSettings();
      if ((UnityEngine.Object) SceneSingleton<HUDAppManager>.i != (UnityEngine.Object) null)
        SceneSingleton<HUDAppManager>.i.RefreshSettings();
      if (!((UnityEngine.Object) SceneSingleton<MFDAppManager>.i != (UnityEngine.Object) null))
        return;
      SceneSingleton<MFDAppManager>.i.RefreshSettings();
    }
  }

  protected override void Awake()
  {
    base.Awake();
    this.virtualJoystickPos.transform.localPosition = Vector3.zero;
    PlayerSettings.OnApplyOptions += new Action(this.RefreshSettings);
  }

  public Transform GetHUDCenter() => this.HUDCenter;

  public void RefreshSettings()
  {
  }

  public void SetHUDInfo(
    Aircraft playerVehicle,
    float airSpeed,
    float altitude,
    float radarAlt,
    Vector3 accel,
    float maxG,
    float climbRate,
    float AoA,
    Vector3 velocity,
    ControlInputs inputs)
  {
    this.playerVehicle = playerVehicle;
    this.velocityVector.gameObject.SetActive((double) airSpeed > 10.0);
    this.HUDCenter.transform.eulerAngles = new Vector3(0.0f, 0.0f, (float) -((double) SceneSingleton<CameraStateManager>.i.mainCamera.transform.eulerAngles.z - (double) playerVehicle.cockpit.transform.eulerAngles.z));
    this.pitchCompassCenter.transform.eulerAngles = new Vector3(0.0f, 0.0f, -SceneSingleton<CameraStateManager>.i.mainCamera.transform.eulerAngles.z);
    this.pitchCompass.transform.localScale = Vector3.one * (50f / SceneSingleton<CameraStateManager>.i.mainCamera.fieldOfView);
    this.pitchCompass.uvRect = new Rect(1f, (float) (-(double) playerVehicle.CockpitRB().transform.eulerAngles.x / 180.0 + 0.43700000643730164), 1f, 0.126f);
    this.compass.uvRect = new Rect((float) (((double) playerVehicle.CockpitRB().transform.eulerAngles.y + 135.0) / 360.0), 0.0f, 0.25f, 1f);
  }

  public void SetVirtualJoystick(Vector3 joystickPos)
  {
    this.virtualJoystickPos.transform.localPosition = joystickPos;
    this.virtualJoystickPos.transform.eulerAngles = new Vector3(0.0f, 0.0f, (float) (-(double) Mathf.Atan2(this.virtualJoystickPos.transform.localPosition.x, this.virtualJoystickPos.transform.localPosition.y) * 57.295780181884766 + 180.0));
    float magnitude = this.virtualJoystickPos.transform.localPosition.magnitude;
    this.virtualJoystickVector.transform.localScale = new Vector3(1f, magnitude, 1f) * (1f / this.virtualJoystickPos.transform.localScale.x);
    this.virtualJoystickPos.color = new Color(0.0f, 1f, 0.0f, Mathf.Clamp01(magnitude * 0.01f));
  }

  private void Update()
  {
    if (!((UnityEngine.Object) this.playerVehicle != (UnityEngine.Object) null) || this.playerVehicle.disabled || !((UnityEngine.Object) this.playerVehicle.pilots[0] != (UnityEngine.Object) null))
      return;
    Vector3 position = this.playerVehicle.CockpitRB().transform.position + this.playerVehicle.CockpitRB().transform.forward * 4000f;
    Vector3 a1 = SceneSingleton<CameraStateManager>.i.mainCamera.WorldToScreenPoint(position);
    if ((double) Vector3.Dot(SceneSingleton<CameraStateManager>.i.transform.forward, position - SceneSingleton<CameraStateManager>.i.transform.position) > 0.0)
      a1 = Vector3.Scale(a1, new Vector3(1f, 1f, 0.0f));
    this.HUDCenter.transform.position = a1;
    if (!this.velocityVector.gameObject.activeSelf)
      return;
    Vector3 a2 = SceneSingleton<CameraStateManager>.i.mainCamera.WorldToScreenPoint(this.playerVehicle.CockpitRB().transform.position + this.playerVehicle.CockpitRB().velocity * 1000f);
    if ((double) Vector3.Dot(SceneSingleton<CameraStateManager>.i.transform.forward, this.playerVehicle.CockpitRB().velocity) > 0.0)
      a2 = Vector3.Scale(a2, new Vector3(1f, 1f, 0.0f));
    this.velocityVector.transform.position = a2;
  }

  private void OnDestroy() => PlayerSettings.OnApplyOptions -= new Action(this.RefreshSettings);
}
