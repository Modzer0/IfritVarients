// Decompiled with JetBrains decompiler
// Type: TargetCam
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

#nullable disable
public class TargetCam : MonoBehaviour
{
  private Camera cam;
  private Camera UICam;
  [SerializeField]
  private Transform currentMount;
  [SerializeField]
  private Transform camMountForward;
  [SerializeField]
  private Transform camMountRear;
  [SerializeField]
  private Transform camMountLanding;
  [SerializeField]
  private UnitPart attachedPart;
  [SerializeField]
  private UnityEngine.Renderer targetScreenRenderer;
  [SerializeField]
  private float landingCamFoV = 90f;
  private Volume screenVolume;
  private ColorAdjustments colorAdjustments;
  private GlobalPosition targetPosition;
  private GlobalPosition targetPositionPrev;
  private float camTimeout;
  private float timeOnTarget;
  private GameObject canvasObjectTarget;
  private GameObject canvasObjectLanding;
  private TargetScreenUI targetScreenUI;
  private LandingScreenUI landingScreenUI;
  private Aircraft aircraft;
  private bool IRMode;
  private float targetFOV = 1f;
  private float targetDist;
  private Vector3 rotationalVelocity;
  private TargetCam.CamMode currentMode;
  private List<Unit> targetList;
  private float lastExposureUpdate;
  private Vector3 landingCamVector = Vector3.zero;
  [SerializeField]
  private bool enableLandingCam;

  public event Action<TargetCam.OnCamToggle> onCamToggle;

  public event Action<bool> onSetup;

  private void OnBeginCameraRendering(ScriptableRenderContext context, Camera camera)
  {
    if (!((UnityEngine.Object) camera == (UnityEngine.Object) this.cam))
      return;
    RenderSettings.fog = !this.IRMode;
  }

  private void OnEndCameraRendering(ScriptableRenderContext context, Camera camera)
  {
    if (!((UnityEngine.Object) camera == (UnityEngine.Object) this.cam))
      return;
    RenderSettings.fog = true;
  }

  private void Awake()
  {
    this.aircraft = this.attachedPart.parentUnit as Aircraft;
    this.targetList = this.aircraft.weaponManager.GetTargetList();
    this.aircraft.Identity.OnStartClient.AddListener(new Action(this.Initialize));
    this.landingCamVector = this.camMountLanding.transform.localEulerAngles;
  }

  public void Initialize()
  {
    this.aircraft = this.attachedPart.parentUnit as Aircraft;
    if (!((UnityEngine.Object) this.aircraft != (UnityEngine.Object) null) || !this.aircraft.Identity.HasAuthority)
      return;
    this.camMountForward.localEulerAngles = Vector3.zero;
    this.camMountRear.localEulerAngles = new Vector3(0.0f, 180f, 0.0f);
    this.currentMount = this.camMountForward;
    Camera[] componentsInChildren = UnityEngine.Object.Instantiate<GameObject>(GameAssets.i.targetCam, this.currentMount).GetComponentsInChildren<Camera>();
    this.cam = componentsInChildren[0];
    this.UICam = componentsInChildren[1];
    this.screenVolume = this.cam.GetComponentInChildren<Volume>();
    this.screenVolume.enabled = false;
    this.attachedPart.onParentDetached += new Action<UnitPart>(this.TargetCam_OnDetach);
    this.aircraft.targetCam = this;
    this.aircraft.OnTouchdown += new Action(this.TargetCam_OnTouchdown);
    this.aircraft.onDisableUnit += new Action<Unit>(this.TargetCam_OnUnitDisable);
    this.aircraft.onSetGear += new Action<Aircraft.OnSetGear>(this.TargetCam_OnSetGear);
    this.enabled = false;
    this.SwitchIRState(false);
    RenderPipelineManager.beginCameraRendering += new Action<ScriptableRenderContext, Camera>(this.OnBeginCameraRendering);
    RenderPipelineManager.endCameraRendering += new Action<ScriptableRenderContext, Camera>(this.OnEndCameraRendering);
  }

  private void SwitchIRState(bool IR)
  {
    this.IRMode = IR;
    if (!this.screenVolume.profile.TryGet<ColorAdjustments>(out this.colorAdjustments))
      throw new NullReferenceException("colorAdjustments");
    this.colorAdjustments.saturation.overrideState = IR;
  }

  private void UpdateExposure()
  {
    if ((UnityEngine.Object) this.aircraft == (UnityEngine.Object) null)
      return;
    float t = Mathf.InverseLerp(0.02f, 0.4f, NetworkSceneSingleton<LevelInfo>.i.GetAmbientLight());
    if (this.IRMode)
    {
      this.colorAdjustments.postExposure.value = Mathf.Lerp(3f, -0.5f, t);
      this.colorAdjustments.contrast.value = 1f;
    }
    else
    {
      this.colorAdjustments.postExposure.value = Mathf.Lerp(0.5f, -1f, t);
      this.colorAdjustments.contrast.value = 5f;
    }
  }

  private void TargetCam_OnDetach(UnitPart part)
  {
    this.targetScreenRenderer.material = GameAssets.i.BSOD;
    UnityEngine.Object.Destroy((UnityEngine.Object) this);
  }

  private void TargetCam_OnUnitDisable(Unit unit)
  {
    UnityEngine.Object.Destroy((UnityEngine.Object) this);
    this.cam.enabled = false;
    this.UICam.enabled = false;
    UnityEngine.Object.Destroy((UnityEngine.Object) this.screenVolume);
  }

  private void TargetCam_OnTouchdown()
  {
    this.cam.enabled = false;
    this.currentMode = TargetCam.CamMode.targetForward;
    Action<TargetCam.OnCamToggle> onCamToggle1 = this.onCamToggle;
    if (onCamToggle1 != null)
      onCamToggle1(new TargetCam.OnCamToggle()
      {
        enabled = false,
        camMode = TargetCam.CamMode.landingMode
      });
    WeaponManager weaponManager = this.aircraft.weaponManager;
    if ((weaponManager != null ? (weaponManager.GetTargetList().Count > 0 ? 1 : 0) : 0) == 0)
      return;
    Action<TargetCam.OnCamToggle> onCamToggle2 = this.onCamToggle;
    if (onCamToggle2 == null)
      return;
    onCamToggle2(new TargetCam.OnCamToggle()
    {
      enabled = true,
      camMode = TargetCam.CamMode.targetForward
    });
  }

  private void TargetCam_OnSetGear(Aircraft.OnSetGear g)
  {
    if (!this.enableLandingCam && PlayerSettings.landingCam == 1 || PlayerSettings.landingCam == 0)
      return;
    if (g.gearState == LandingGear.GearState.Extending || g.gearState == LandingGear.GearState.LockedExtended)
    {
      WeaponManager weaponManager = this.aircraft.weaponManager;
      if ((weaponManager != null ? (weaponManager.GetTargetList().Count > 0 ? 1 : 0) : 0) != 0)
      {
        this.cam.enabled = false;
        this.currentMode = TargetCam.CamMode.landingMode;
        Action<TargetCam.OnCamToggle> onCamToggle = this.onCamToggle;
        if (onCamToggle != null)
          onCamToggle(new TargetCam.OnCamToggle()
          {
            enabled = false,
            camMode = TargetCam.CamMode.targetForward
          });
      }
      this.SetLandingCam();
    }
    else
    {
      if (g.gearState != LandingGear.GearState.Retracting && g.gearState != LandingGear.GearState.LockedRetracted)
        return;
      this.cam.enabled = false;
      this.currentMode = TargetCam.CamMode.targetForward;
      Action<TargetCam.OnCamToggle> onCamToggle1 = this.onCamToggle;
      TargetCam.OnCamToggle onCamToggle2;
      if (onCamToggle1 != null)
      {
        onCamToggle2 = new TargetCam.OnCamToggle();
        onCamToggle2.enabled = false;
        onCamToggle2.camMode = TargetCam.CamMode.landingMode;
        onCamToggle1(onCamToggle2);
      }
      WeaponManager weaponManager = this.aircraft.weaponManager;
      if ((weaponManager != null ? (weaponManager.GetTargetList().Count > 0 ? 1 : 0) : 0) == 0)
        return;
      Action<TargetCam.OnCamToggle> onCamToggle3 = this.onCamToggle;
      if (onCamToggle3 == null)
        return;
      onCamToggle2 = new TargetCam.OnCamToggle();
      onCamToggle2.enabled = true;
      onCamToggle2.camMode = TargetCam.CamMode.targetForward;
      onCamToggle3(onCamToggle2);
    }
  }

  private void OnDestroy()
  {
    if ((UnityEngine.Object) this.attachedPart.parentUnit != (UnityEngine.Object) null)
      this.attachedPart.parentUnit.onDisableUnit -= new Action<Unit>(this.TargetCam_OnUnitDisable);
    this.attachedPart.onParentDetached -= new Action<UnitPart>(this.TargetCam_OnDetach);
    if ((UnityEngine.Object) this.aircraft != (UnityEngine.Object) null)
    {
      this.aircraft.OnTouchdown -= new Action(this.TargetCam_OnTouchdown);
      this.aircraft.onSetGear -= new Action<Aircraft.OnSetGear>(this.TargetCam_OnSetGear);
    }
    if ((UnityEngine.Object) this.targetScreenUI != (UnityEngine.Object) null)
      UnityEngine.Object.Destroy((UnityEngine.Object) this.targetScreenUI);
    if ((UnityEngine.Object) this.landingScreenUI != (UnityEngine.Object) null)
      UnityEngine.Object.Destroy((UnityEngine.Object) this.landingScreenUI);
    if ((UnityEngine.Object) this.canvasObjectTarget != (UnityEngine.Object) null)
      UnityEngine.Object.Destroy((UnityEngine.Object) this.canvasObjectTarget);
    if ((UnityEngine.Object) this.canvasObjectLanding != (UnityEngine.Object) null)
      UnityEngine.Object.Destroy((UnityEngine.Object) this.canvasObjectLanding);
    RenderPipelineManager.beginCameraRendering -= new Action<ScriptableRenderContext, Camera>(this.OnBeginCameraRendering);
    RenderPipelineManager.endCameraRendering -= new Action<ScriptableRenderContext, Camera>(this.OnEndCameraRendering);
  }

  public void SetTargetCam()
  {
    if (this.currentMode == TargetCam.CamMode.landingMode)
      return;
    this.screenVolume.enabled = true;
    foreach (Unit target in this.targetList)
      target.displayDetail = 1f;
    if ((UnityEngine.Object) this.canvasObjectLanding != (UnityEngine.Object) null && this.canvasObjectLanding.activeSelf)
      this.canvasObjectLanding.SetActive(false);
    if ((UnityEngine.Object) this.canvasObjectTarget == (UnityEngine.Object) null)
    {
      this.canvasObjectTarget = UnityEngine.Object.Instantiate<GameObject>(GameAssets.i.targetScreenCanvas, (Transform) null);
      this.targetScreenUI = this.canvasObjectTarget.GetComponent<TargetScreenUI>();
      this.targetScreenUI.SetupCamera(this.cam, this.UICam, this.aircraft);
    }
    else if ((UnityEngine.Object) this.canvasObjectTarget != (UnityEngine.Object) null && !this.canvasObjectTarget.activeSelf)
      this.canvasObjectTarget.SetActive(true);
    this.enabled = true;
    if (!this.cam.enabled)
    {
      this.currentMount = this.camMountForward;
      this.cam.transform.parent = this.camMountForward;
      this.cam.transform.localEulerAngles = Vector3.zero;
      this.cam.transform.localPosition = Vector3.zero;
      this.camMountForward.localEulerAngles = Vector3.zero;
      this.cam.fieldOfView = 10f;
      this.cam.nearClipPlane = 2f;
      this.cam.farClipPlane = 60000f;
      this.currentMode = TargetCam.CamMode.targetForward;
      this.cam.enabled = true;
      Action<TargetCam.OnCamToggle> onCamToggle = this.onCamToggle;
      if (onCamToggle != null)
        onCamToggle(new TargetCam.OnCamToggle()
        {
          enabled = true,
          camMode = TargetCam.CamMode.targetForward
        });
    }
    this.camTimeout = 3f;
    float size;
    this.GetPositionAndSize(this.targetList, out this.targetPosition, out size);
    this.targetDist = FastMath.Distance(this.targetPosition, this.transform.GlobalPosition());
    if ((double) NetworkSceneSingleton<LevelInfo>.i.timeOfDay < 6.0 || (double) NetworkSceneSingleton<LevelInfo>.i.timeOfDay > 18.0 || (double) this.targetDist > 10000.0 || PlayerSettings.tacScreenIR)
    {
      if (!this.IRMode)
        this.SwitchIRState(true);
    }
    else if (this.IRMode)
      this.SwitchIRState(false);
    this.targetFOV = Mathf.Clamp(size * 75f / this.targetDist, 0.25f, 20f);
    this.timeOnTarget += Time.deltaTime;
    if (FastMath.OutOfRange(this.targetPosition, this.targetPositionPrev, 50f))
      this.timeOnTarget = 0.0f;
    this.targetPositionPrev = this.targetPosition;
    this.AimCamera();
  }

  public void SetLandingCam()
  {
    if (!this.enableLandingCam && PlayerSettings.landingCam == 1 || PlayerSettings.landingCam == 0)
      return;
    this.screenVolume.enabled = true;
    if ((UnityEngine.Object) this.canvasObjectTarget != (UnityEngine.Object) null && this.canvasObjectTarget.activeSelf)
      this.canvasObjectTarget.SetActive(false);
    if ((UnityEngine.Object) this.canvasObjectLanding == (UnityEngine.Object) null)
    {
      this.canvasObjectLanding = UnityEngine.Object.Instantiate<GameObject>(GameAssets.i.LandingScreenCanvas, (Transform) null);
      this.landingScreenUI = this.canvasObjectLanding.GetComponent<LandingScreenUI>();
      this.landingScreenUI.SetupCamera(this.cam, this.UICam);
    }
    else if ((UnityEngine.Object) this.canvasObjectLanding != (UnityEngine.Object) null)
      this.canvasObjectLanding.SetActive(true);
    this.enabled = true;
    if (!this.cam.enabled)
    {
      this.currentMount = this.camMountLanding;
      this.camMountLanding.transform.localEulerAngles = this.landingCamVector;
      this.cam.transform.parent = this.camMountLanding;
      this.cam.transform.localEulerAngles = Vector3.zero;
      this.cam.transform.localPosition = Vector3.zero;
      this.cam.fieldOfView = this.landingCamFoV;
      this.cam.nearClipPlane = 0.1f;
      this.cam.farClipPlane = 30000f;
      this.currentMode = TargetCam.CamMode.landingMode;
      this.cam.enabled = true;
      Action<TargetCam.OnCamToggle> onCamToggle = this.onCamToggle;
      if (onCamToggle != null)
        onCamToggle(new TargetCam.OnCamToggle()
        {
          enabled = true,
          camMode = TargetCam.CamMode.landingMode
        });
    }
    this.landingScreenUI.SetInfo(10f / this.cam.fieldOfView, this.IRMode);
    if ((double) NetworkSceneSingleton<LevelInfo>.i.timeOfDay < 6.0 || (double) NetworkSceneSingleton<LevelInfo>.i.timeOfDay > 18.0 || PlayerSettings.tacScreenIR)
    {
      if (this.IRMode)
        return;
      this.SwitchIRState(true);
    }
    else
    {
      if (!this.IRMode)
        return;
      this.SwitchIRState(false);
    }
  }

  private void GetPositionAndSize(List<Unit> targets, out GlobalPosition position, out float size)
  {
    if (targets.Count == 1)
      this.SingleTargetPositionAndSize(targets, out position, out size);
    else
      this.MultipleTargetPositionAndSize(targets, out position, out size);
  }

  private void SingleTargetPositionAndSize(
    List<Unit> targets,
    out GlobalPosition position,
    out float size)
  {
    size = targets[0].definition.length;
    TrackingInfo trackingData = this.aircraft.NetworkHQ.GetTrackingData(targets[0].persistentID);
    position = trackingData != null ? trackingData.GetPosition() : targets[0].GlobalPosition();
  }

  private void MultipleTargetPositionAndSize(
    List<Unit> targets,
    out GlobalPosition position,
    out float size)
  {
    Vector3 targetsSouthWest;
    Vector3 targetsNorthEast;
    this.GetMultipleTargetBounds(targets, out targetsSouthWest, out targetsNorthEast);
    position = ((targetsSouthWest + targetsNorthEast) * 0.5f).ToGlobalPosition();
    float a = Mathf.Max(targetsNorthEast.x - targetsSouthWest.x, targetsNorthEast.z - targetsSouthWest.z);
    size = Mathf.Max(a, targetsNorthEast.y - targetsSouthWest.y);
    size *= 0.75f;
  }

  private void GetMultipleTargetBounds(
    List<Unit> targets,
    out Vector3 targetsSouthWest,
    out Vector3 targetsNorthEast)
  {
    targetsSouthWest = Vector3.one * float.MaxValue;
    targetsNorthEast = -Vector3.one * float.MaxValue;
    for (int index = 0; index < targets.Count; ++index)
    {
      TrackingInfo trackingData = this.aircraft.NetworkHQ.GetTrackingData(targets[index].persistentID);
      Vector3 vector3 = trackingData != null ? trackingData.GetPosition().ToLocalPosition() : targets[index].transform.position;
      if (!targets[index].disabled)
      {
        targetsSouthWest.x = Mathf.Min(vector3.x, targetsSouthWest.x);
        targetsSouthWest.z = Mathf.Min(vector3.z, targetsSouthWest.z);
        targetsSouthWest.y = Mathf.Min(vector3.y, targetsSouthWest.y);
        targetsNorthEast.x = Mathf.Max(vector3.x, targetsNorthEast.x);
        targetsNorthEast.z = Mathf.Max(vector3.z, targetsNorthEast.z);
        targetsNorthEast.y = Mathf.Max(vector3.y, targetsNorthEast.y);
      }
    }
  }

  public float GetMag() => 10f / this.cam.fieldOfView;

  public float GetDist() => this.targetDist;

  public string GetGrid()
  {
    return SceneSingleton<DynamicMap>.i.gridLabels.GetGridPosition(this.targetPosition);
  }

  public bool UsingIR() => this.IRMode;

  public Transform GetCamMount() => this.currentMount;

  public void CancelTarget()
  {
    if (this.cam.enabled)
    {
      this.cam.enabled = false;
      Action<TargetCam.OnCamToggle> onCamToggle = this.onCamToggle;
      if (onCamToggle != null)
        onCamToggle(new TargetCam.OnCamToggle()
        {
          enabled = false,
          camMode = TargetCam.CamMode.targetForward
        });
    }
    this.targetPosition = new GlobalPosition();
  }

  private void AimCamera()
  {
    if (this.currentMode == TargetCam.CamMode.landingMode)
      return;
    if ((double) this.camTimeout < 2.5)
    {
      this.cam.fieldOfView = Mathf.Min(this.cam.fieldOfView + 2f * Time.deltaTime, 20f);
      if ((UnityEngine.Object) this.targetScreenUI == (UnityEngine.Object) null)
        return;
    }
    if ((double) this.camTimeout <= 0.0)
    {
      this.timeOnTarget = 0.0f;
      if (this.cam.enabled)
      {
        this.cam.enabled = false;
        Action<TargetCam.OnCamToggle> onCamToggle = this.onCamToggle;
        if (onCamToggle != null)
          onCamToggle(new TargetCam.OnCamToggle()
          {
            enabled = false,
            camMode = TargetCam.CamMode.targetForward
          });
      }
      if ((double) Vector3.Angle(this.transform.forward, this.attachedPart.transform.forward) >= 0.0099999997764825821)
        return;
      this.cam.enabled = false;
      this.enabled = false;
    }
    else if ((double) this.timeOnTarget < 1.0)
      this.currentMount.transform.rotation = Quaternion.Slerp(this.currentMount.transform.rotation, Quaternion.LookRotation(this.targetPosition - this.currentMount.transform.GlobalPosition(), Vector3.up), this.timeOnTarget);
    else
      this.currentMount.transform.rotation = Quaternion.LookRotation(this.targetPosition - this.currentMount.transform.GlobalPosition(), Vector3.up);
  }

  private void Update()
  {
    if ((UnityEngine.Object) this.aircraft == (UnityEngine.Object) null || (UnityEngine.Object) this.aircraft.Player == (UnityEngine.Object) null || !this.aircraft.Player.IsLocalPlayer)
    {
      UnityEngine.Object.Destroy((UnityEngine.Object) this);
    }
    else
    {
      if ((double) Time.timeSinceLevelLoad - (double) this.lastExposureUpdate > 1.0)
        this.UpdateExposure();
      if (this.currentMode == TargetCam.CamMode.landingMode)
        return;
      this.cam.fieldOfView = Mathf.Lerp(this.cam.fieldOfView, this.targetFOV, Time.deltaTime);
      float num = Vector3.Angle(this.aircraft.transform.forward, FastMath.Direction(this.aircraft.GlobalPosition(), this.targetPosition));
      if ((double) num < 135.0 && this.currentMode == TargetCam.CamMode.targetRear)
      {
        this.currentMount = this.camMountForward;
        this.currentMode = TargetCam.CamMode.targetForward;
        this.cam.transform.parent = this.camMountForward;
        this.cam.transform.localEulerAngles = Vector3.zero;
        this.cam.transform.localPosition = Vector3.zero;
        this.camMountForward.localEulerAngles = Vector3.zero;
        this.camMountRear.localEulerAngles = new Vector3(0.0f, 180f, 0.0f);
      }
      else if ((double) num > 135.0 && this.currentMode == TargetCam.CamMode.targetForward)
      {
        this.currentMount = this.camMountRear;
        this.currentMode = TargetCam.CamMode.targetRear;
        this.cam.transform.parent = this.camMountRear;
        this.cam.transform.localEulerAngles = Vector3.zero;
        this.cam.transform.localPosition = Vector3.zero;
        this.camMountForward.localEulerAngles = Vector3.zero;
        this.camMountRear.localEulerAngles = new Vector3(0.0f, 180f, 0.0f);
      }
      if ((double) this.camTimeout < 3.0)
        this.AimCamera();
      this.camTimeout -= Time.deltaTime;
    }
  }

  public enum CamMode
  {
    targetForward,
    targetRear,
    landingMode,
  }

  public struct OnCamToggle
  {
    public bool enabled;
    public TargetCam.CamMode camMode;
  }
}
