// Decompiled with JetBrains decompiler
// Type: CameraStateManager
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.MissionEditorScripts;
using NuclearOption.SavedMission;
using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

#nullable disable
public class CameraStateManager : SceneSingleton<CameraStateManager>
{
  public static CameraMode cameraMode;
  public CameraFreeState freeState = new CameraFreeState();
  public CameraOrbitState orbitState = new CameraOrbitState();
  public CameraTVState TVState = new CameraTVState();
  public CameraCockpitState cockpitState = new CameraCockpitState();
  public CameraSelectionState selectionState = new CameraSelectionState();
  public CameraRelativeState relativeState = new CameraRelativeState();
  public CameraEncyclopediaState encyclopediaState = new CameraEncyclopediaState();
  public CameraChaseState chaseState = new CameraChaseState();
  public CameraControlledState controlledState = new CameraControlledState();
  [NonSerialized]
  public Transform cameraPivot;
  public Light nightVisLight;
  public Light Illuminator;
  public Light sunLight;
  public Light moonLight;
  public Vector2 cloudSpeed;
  public float minExposure;
  public float maxExposure;
  public float lightSensitivity;
  private UniversalAdditionalLightData sunLightData;
  private UniversalAdditionalLightData moonLightData;
  public Material atmosphereSky;
  public Material waterSky;
  public GameObject cockpitCam;
  [NonSerialized]
  public Camera mainCamera;
  [NonSerialized]
  public float gForce;
  [NonSerialized]
  public float jerk;
  public string viewStatus = "noTarget";
  public static bool enableMouseLook;
  public float panView;
  public float tiltView;
  private FloatingOrigin floatingOrigin;
  [NonSerialized]
  public float shakeFactor;
  public float shakeThreshold = 5f;
  public Rigidbody followingRB;
  public AudioSource cockpitRattle;
  public AudioSource windNoiseExternal;
  public AudioSource soundEffectSource;
  public Camera cockpitCamRender;
  public Camera selectionCam;
  public GameObject externalViewCenter;
  private Vector3 deathVelocity;
  [HideInInspector]
  public Vector3 deathPosition;
  [HideInInspector]
  public float deathTimer;
  public TrackIRComponent trackIRComponent;
  public Texture2D mapTexture;
  public bool freeCam;
  public Color oceanColor;
  private bool underwater;
  [SerializeField]
  private UnityEngine.Renderer sunObject;
  [SerializeField]
  private UnityEngine.Renderer moonObject;
  [SerializeField]
  private Volume volume;
  [SerializeField]
  private Image blackoutImage;
  public Unit followingUnit;
  public Unit previousFollowingUnit;
  public float desiredFOV;
  public bool allowInputs = true;
  public float desiredTransSpeed = 1f;
  public float desiredRotSpeed = 1f;
  public float fovChangeSpeed = 0.5f;
  public float fovChangeInertia = 0.5f;
  private int detailUnitIndex;
  private float lastFollowCheck;
  public Vector3 cameraVelocity;

  public CameraBaseState currentState { get; private set; }

  public static event Action<Unit> onFollowingUnitSet;

  public event Action onSwitchCamera;

  public void SwitchState(CameraBaseState state)
  {
    this.lastFollowCheck = Time.timeSinceLevelLoad;
    if (this.currentState != null)
      this.currentState.LeaveState(this);
    this.currentState = state;
    if (this.currentState == this.freeState)
      this.freeState.DontResetRotationFlag = true;
    this.desiredFOV = this.mainCamera.fieldOfView;
    this.currentState.EnterState(this);
    Aircraft localAircraft;
    if (GameManager.GetLocalAircraft(out localAircraft) && (UnityEngine.Object) localAircraft == (UnityEngine.Object) this.followingUnit)
      SceneSingleton<DynamicMap>.i.Minimize();
    this.cockpitRattle.Stop();
    Action onSwitchCamera = this.onSwitchCamera;
    if (onSwitchCamera != null)
      onSwitchCamera();
    if (this.currentState == this.cockpitState)
      AudioMixerVolume.SetEffectsAudioFilterStrength(8000f, 0.7f);
    else
      AudioMixerVolume.SetEffectsAudioFilterStrength(22000f, 1f);
  }

  public Volume GetPostProcessVolume() => this.volume;

  public Image GetBlackoutImage() => this.blackoutImage;

  public void SetFollowingUnit(Unit unit)
  {
    Action<Unit> followingUnitSet = CameraStateManager.onFollowingUnitSet;
    if (followingUnitSet != null)
      followingUnitSet(unit);
    this.currentState.LeaveState(this);
    if ((UnityEngine.Object) this.followingUnit != (UnityEngine.Object) null)
    {
      if (this.followingUnit is Aircraft followingUnit)
        followingUnit.SetDoppler(true);
      this.previousFollowingUnit = this.followingUnit;
      this.followingUnit.onDisableUnit -= new Action<Unit>(this.Cam_OnFollowingUnitDisabled);
    }
    if ((UnityEngine.Object) unit != (UnityEngine.Object) null)
    {
      this.followingUnit = unit;
      this.followingUnit.onDisableUnit += new Action<Unit>(this.Cam_OnFollowingUnitDisabled);
      this.followingUnit.displayDetail = 1000f;
      if ((UnityEngine.Object) unit.rb != (UnityEngine.Object) null)
      {
        this.followingRB = unit.rb;
        this.SwitchState((CameraBaseState) this.orbitState);
      }
      else
      {
        this.transform.position = unit.transform.position - unit.transform.forward * (unit.maxRadius * 2f) + Vector3.up * unit.definition.height * 0.8f;
        this.transform.LookAt(unit.transform);
        this.SwitchState((CameraBaseState) this.freeState);
      }
    }
    else
    {
      this.followingUnit = (Unit) null;
      this.SwitchState((CameraBaseState) this.freeState);
    }
    NetworkSceneSingleton<LevelInfo>.i.UpdateReflectionProbe(true);
  }

  public void FocusPosition(Vector3 position, Quaternion? rotation, float distance)
  {
    this.SetFollowingUnit((Unit) null);
    this.previousFollowingUnit = (Unit) null;
    if (rotation.HasValue)
    {
      Vector3 vector3 = rotation.Value * Vector3.forward;
      this.transform.SetPositionAndRotation(position - vector3 * distance, rotation.Value);
    }
    else
      this.transform.position = position - this.transform.forward * distance;
    this.freeState.DontResetRotationFlag = true;
    this.SwitchState((CameraBaseState) this.freeState);
  }

  public void FocusAirbase(
    Airbase airbase,
    bool allowMoveToDropFocus,
    float viewDistance = 20f,
    float upDistance = 1.75f)
  {
    this.selectionState.FocusAirbase(this, airbase, allowMoveToDropFocus, viewDistance, upDistance);
  }

  public void SetCameraPosition(PositionRotation positionRotation)
  {
    this.SetCameraPosition(positionRotation.Position, positionRotation.Rotation);
  }

  public void SetCameraPosition(GlobalPosition position, Quaternion rotation)
  {
    this.SetFollowingUnit((Unit) null);
    this.previousFollowingUnit = (Unit) null;
    this.transform.SetPositionAndRotation(position.ToLocalPosition(), rotation);
    this.freeState.DontResetRotationFlag = true;
    this.SwitchState((CameraBaseState) this.freeState);
  }

  public void GetCameraPosition(out PositionRotation positionRotation)
  {
    this.GetCameraPosition(out positionRotation.Position, out positionRotation.Rotation);
  }

  public void GetCameraPosition(out GlobalPosition position, out Quaternion rotation)
  {
    Vector3 position1;
    this.transform.GetPositionAndRotation(out position1, out rotation);
    position = position1.ToGlobalPosition();
  }

  private void Cam_OnFollowingUnitDisabled(Unit unit)
  {
    if (GameManager.gameState == GameState.Menu)
      return;
    Aircraft localAircraft;
    if (GameManager.GetLocalAircraft(out localAircraft) && (UnityEngine.Object) localAircraft == (UnityEngine.Object) this.followingUnit)
      SoundManager.PlayInterfaceOneShot(GameAssets.i.deathSound);
    if ((UnityEngine.Object) SceneSingleton<GameplayUI>.i.hurt != (UnityEngine.Object) null)
    {
      SceneSingleton<GameplayUI>.i.hurt.color = new Color(0.0f, 0.0f, 0.0f, 0.0f);
      SceneSingleton<GameplayUI>.i.hurt.gameObject.SetActive(false);
    }
    this.SetFollowingUnit((Unit) null);
  }

  private void Start()
  {
    this.atmosphereSky = UnityEngine.Object.Instantiate<Material>(this.atmosphereSky);
    this.waterSky = UnityEngine.Object.Instantiate<Material>(this.waterSky);
    RenderSettings.skybox = this.atmosphereSky;
    this.SetDesiredSpeed(1f, 1f);
    this.SetDesiredFoV(PlayerSettings.defaultFoV, PlayerSettings.defaultFoV);
    this.allowInputs = true;
  }

  private void OnEnable()
  {
    this.mainCamera = this.GetComponent<Camera>();
    this.floatingOrigin = this.GetComponent<FloatingOrigin>();
    this.sunLightData = this.sunLight.gameObject.GetComponent<UniversalAdditionalLightData>();
    this.moonLightData = this.moonLight.gameObject.GetComponent<UniversalAdditionalLightData>();
    CameraStateManager.enableMouseLook = true;
    if (GameManager.gameState == GameState.Encyclopedia)
    {
      this.currentState = (CameraBaseState) this.encyclopediaState;
      this.currentState.EnterState(this);
    }
    else
    {
      if (this.currentState == null)
      {
        this.cameraPivot = new GameObject("cameraPivot").transform;
        this.currentState = (CameraBaseState) this.freeState;
        this.currentState.EnterState(this);
        this.gameObject.AddComponent<ExplosionAudioManager>();
        this.oceanColor = new Color(0.32f, 0.45f, 0.61f);
      }
      this.chaseState.Initialize();
    }
  }

  public void SetDesiredFoV(float FOVTarget, float FOVCurrent)
  {
    this.desiredFOV = FOVTarget;
    if ((double) FOVCurrent == 0.0)
      return;
    this.mainCamera.fieldOfView = FOVCurrent;
  }

  public void SetDesiredSpeed(float speed, float rot)
  {
    this.desiredTransSpeed = speed;
    this.desiredRotSpeed = rot;
  }

  public void ShakeCamera(float lowFreqShake, float highFreqShake)
  {
    if (this.currentState != this.cockpitState)
      return;
    this.cockpitState.AddShake(lowFreqShake, highFreqShake);
  }

  private void FixedUpdate()
  {
    this.currentState.FixedUpdateState(this);
    SonicBoomManager.ManageSonicBooms();
  }

  private void Update()
  {
    if ((double) this.deathTimer > 0.0)
    {
      this.deathTimer -= Time.fixedUnscaledDeltaTime;
      this.deathPosition += this.deathVelocity * Time.deltaTime;
    }
    ExposureController.UpdateExposure();
    this.SetUnitDetail();
    if ((UnityEngine.Object) NetworkSceneSingleton<LevelInfo>.i == (UnityEngine.Object) null)
      return;
    this.sunObject.transform.position = this.transform.position - NetworkSceneSingleton<LevelInfo>.i.sun.gameObject.transform.forward * 40000f;
    this.moonObject.transform.position = this.transform.position - NetworkSceneSingleton<LevelInfo>.i.moon.gameObject.transform.forward * 40000f;
    this.sunLightData.lightCookieOffset += this.cloudSpeed * Time.deltaTime;
    this.moonLightData.lightCookieOffset += this.cloudSpeed * Time.deltaTime;
    float num = Mathf.Clamp01((this.cameraVelocity - NetworkSceneSingleton<LevelInfo>.i.GetWind(this.transform.GlobalPosition())).magnitude / LevelInfo.GetSpeedOfSound(this.transform.GlobalPosition().y));
    this.windNoiseExternal.volume = num;
    this.windNoiseExternal.pitch = (float) ((double) num * 0.5 + 0.5);
    if (GameManager.gameState == GameState.Encyclopedia || (UnityEngine.Object) SceneSingleton<CameraControlUI>.i == (UnityEngine.Object) null || !SceneSingleton<CameraControlUI>.i.isOpen || InputFieldChecker.InsideInputField)
      return;
    SceneSingleton<CameraControlUI>.i.GetValues();
  }

  private void SetUnitDetail()
  {
    if (UnitRegistry.allUnits.Count == 0)
      return;
    this.detailUnitIndex = this.detailUnitIndex < UnitRegistry.allUnits.Count - 1 ? this.detailUnitIndex + 1 : 0;
    Unit allUnit = UnitRegistry.allUnits[this.detailUnitIndex];
    if (!((UnityEngine.Object) allUnit == (UnityEngine.Object) null) || !((UnityEngine.Object) allUnit != (UnityEngine.Object) this.followingUnit))
      return;
    allUnit.displayDetail = 1000f / FastMath.Distance(allUnit.transform.position, this.transform.position);
  }

  private void FollowCheck()
  {
    if ((double) Time.timeSinceLevelLoad - (double) this.lastFollowCheck < 1.0)
      return;
    this.lastFollowCheck = Time.timeSinceLevelLoad;
    FactionHQ localHq;
    if (GameManager.IsLocalAircraft(this.followingUnit) || !GameManager.GetLocalHQ(out localHq) || !((UnityEngine.Object) this.followingUnit.NetworkHQ != (UnityEngine.Object) null) || !((UnityEngine.Object) localHq != (UnityEngine.Object) this.followingUnit.NetworkHQ) || localHq.IsTargetPositionAccurate(this.followingUnit, 100f))
      return;
    this.SetFollowingUnit((Unit) null);
  }

  private void LateUpdate()
  {
    if ((UnityEngine.Object) this.followingUnit != (UnityEngine.Object) null)
      this.FollowCheck();
    this.currentState.UpdateState(this);
    this.CheckOriginShift();
    if (!this.underwater && (double) this.transform.position.y < (double) Datum.LocalSeaY)
    {
      RenderSettings.skybox = this.waterSky;
      RenderSettings.fogColor = new Color(0.0f, 0.25f, 0.35f);
      RenderSettings.fogDensity = 0.05f;
      this.underwater = true;
    }
    if (!this.underwater || (double) this.transform.position.y <= (double) Datum.LocalSeaY)
      return;
    RenderSettings.skybox = this.atmosphereSky;
    this.underwater = false;
  }

  public void CheckOriginShift()
  {
    if (!((UnityEngine.Object) this.floatingOrigin != (UnityEngine.Object) null))
      return;
    this.floatingOrigin.OriginShift(this.transform.position);
  }
}
