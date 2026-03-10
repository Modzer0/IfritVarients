// Decompiled with JetBrains decompiler
// Type: CameraChaseState
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

#nullable disable
public class CameraChaseState : CameraBaseState
{
  private RaycastHit hit;
  private float orbitDist;
  private Vector3 posVector;
  private Vector3 targetVector;
  private Quaternion cameraRotationPrev;
  private Quaternion cameraCustomRotation;
  private float viewDistAdjust;
  private int layerMask = 64 /*0x40*/;
  private float zoomSpeed;
  private float FOVAdjustment;
  private float minFOV = 20f;
  private float maxFOV = 120f;
  private Transform transitionTarget;
  private float transitionTimer;
  private CameraChaseState.ChasePos currentPos;
  private bool showHUD;
  private Dictionary<AircraftDefinition, CameraChaseState.CameraPos> AircraftCameraPos = new Dictionary<AircraftDefinition, CameraChaseState.CameraPos>();

  public void Initialize()
  {
    foreach (AircraftDefinition key in Encyclopedia.i.aircraft)
    {
      CameraChaseState.CameraPos cameraPos = new CameraChaseState.CameraPos();
      float num = Mathf.Max(key.length, key.width * 0.7f) + Mathf.Max(key.width, key.length * 0.7f);
      cameraPos.back.position = new Vector3(0.0f, 0.1f * num, -num);
      cameraPos.back.orientation = Vector3.forward;
      cameraPos.front.position = new Vector3(0.0f, 0.0f, num);
      cameraPos.front.orientation = -Vector3.forward;
      cameraPos.top.position = new Vector3(0.0f, num, 0.0f);
      cameraPos.top.orientation = -Vector3.up + 0.1f * Vector3.forward;
      cameraPos.bottom.position = new Vector3(0.0f, -num, 0.0f);
      cameraPos.bottom.orientation = Vector3.up + 0.1f * Vector3.forward;
      cameraPos.belly.position = new Vector3(0.0f, -0.1f * num, -0.25f * num);
      cameraPos.belly.orientation = Vector3.forward;
      cameraPos.tail.position = new Vector3(0.0f, 0.1f * num, -0.25f * num);
      cameraPos.tail.orientation = Vector3.forward;
      cameraPos.wingL.position = new Vector3(-num, 0.0f, 0.0f);
      cameraPos.wingL.orientation = Vector3.right;
      cameraPos.wingR.position = new Vector3(num, 0.0f, 0.0f);
      cameraPos.wingR.orientation = -Vector3.right;
      cameraPos.wingRootL.position = new Vector3(-0.1f * num, 0.05f * num, 0.0f);
      cameraPos.wingRootL.orientation = Vector3.forward;
      cameraPos.wingRootR.position = new Vector3(0.1f * num, 0.05f * num, 0.0f);
      cameraPos.wingRootR.orientation = Vector3.forward;
      this.AircraftCameraPos.Add(key, cameraPos);
    }
  }

  public override void EnterState(CameraStateManager cam)
  {
    if (!(cam.followingUnit is Aircraft))
    {
      cam.SwitchState((CameraBaseState) cam.orbitState);
    }
    else
    {
      this.viewDistAdjust = 0.0f;
      this.zoomSpeed = 0.0f;
      this.transitionTimer = 0.0f;
      cam.cameraPivot.SetParent(cam.followingUnit.transform);
      cam.transform.SetParent(cam.cameraPivot);
      cam.cameraPivot.localPosition = Vector3.zero;
      cam.cameraPivot.localRotation = Quaternion.identity;
      cam.transform.localEulerAngles = Vector3.zero;
      this.FOVAdjustment = 0.0f;
      this.orbitDist = Mathf.Max(cam.followingUnit.definition.length, cam.followingUnit.definition.width * 0.7f) + Mathf.Max(cam.followingUnit.definition.width, cam.followingUnit.definition.length * 0.7f);
      if (cam.followingUnit is Aircraft followingUnit)
      {
        followingUnit.SetDoppler(false);
        if (followingUnit.cockpit.IsDetached())
        {
          cam.followingRB = followingUnit.cockpit.rb;
          cam.cameraPivot.SetParent(followingUnit.cockpit.transform);
          cam.cameraPivot.localPosition = Vector3.zero;
        }
        else
          followingUnit.cockpit.onParentDetached += new Action<UnitPart>(this.CameraChaseState_OnCockpitDetach);
      }
      cam.mainCamera.nearClipPlane = 0.2f;
      this.currentPos = CameraChaseState.ChasePos.Back;
      CameraChaseState.CameraPos cameraPos;
      if (this.AircraftCameraPos.TryGetValue(cam.followingUnit.definition as AircraftDefinition, out cameraPos))
      {
        this.posVector = cameraPos.back.position;
        this.targetVector = cameraPos.back.orientation;
      }
      else
      {
        this.posVector = -cam.followingUnit.transform.forward * this.orbitDist;
        this.targetVector = cam.followingUnit.transform.forward;
      }
      this.cameraRotationPrev = cam.cameraPivot.rotation;
      this.cameraCustomRotation = cam.transform.localRotation;
      this.showHUD = false;
      this.CheckHUD();
      CameraStateManager.cameraMode = CameraMode.chase;
      ((UniversalRenderPipelineAsset) GraphicsSettings.currentRenderPipeline).shadowDistance = Mathf.Max(2000f, (float) (2000.0 * (double) this.orbitDist * 2.0 / 30.0));
    }
  }

  public override void LeaveState(CameraStateManager cam)
  {
    cam.cameraPivot.SetParent((Transform) null);
    ((UniversalRenderPipelineAsset) GraphicsSettings.currentRenderPipeline).shadowDistance = 2000f;
    if (!(cam.followingUnit is Aircraft followingUnit))
      return;
    followingUnit.SetDoppler(true);
    followingUnit.cockpit.onParentDetached -= new Action<UnitPart>(this.CameraChaseState_OnCockpitDetach);
  }

  private void CameraChaseState_OnCockpitDetach(UnitPart cockpitPart)
  {
    this.transitionTimer = 1f / 1000f;
    this.transitionTarget = cockpitPart.transform;
    SceneSingleton<CameraStateManager>.i.cameraPivot.transform.SetParent((Transform) null);
  }

  public override void UpdateState(CameraStateManager cam)
  {
    if ((UnityEngine.Object) cam.followingRB == (UnityEngine.Object) null)
      return;
    Vector3 a1 = cam.followingRB.velocity;
    this.CheckInput(cam);
    if ((double) this.transitionTimer > 0.0)
    {
      cam.cameraPivot.SetParent((Transform) null);
      Aircraft followingUnit = cam.followingUnit as Aircraft;
      this.transitionTimer += Time.deltaTime;
      cam.cameraPivot.position = Vector3.Lerp(followingUnit.transform.position, followingUnit.cockpit.transform.position, this.transitionTimer);
      a1 = Vector3.Lerp(a1, followingUnit.cockpit.rb.velocity, this.transitionTimer);
      if ((double) this.transitionTimer > 1.0)
      {
        cam.followingRB = followingUnit.cockpit.rb;
        cam.cameraPivot.SetParent(followingUnit.cockpit.transform);
        cam.cameraPivot.localPosition = Vector3.zero;
        this.transitionTimer = 0.0f;
      }
    }
    else
      cam.cameraPivot.localPosition = Vector3.zero;
    cam.cameraVelocity = a1 * Time.timeScale;
    this.FOVAdjustment -= cam.fovChangeSpeed * Mathf.Min(cam.mainCamera.fieldOfView / 20f, 1f) * GameManager.playerInput.GetAxis("FOV");
    this.FOVAdjustment = Mathf.Clamp(this.FOVAdjustment, this.minFOV - cam.desiredFOV, this.maxFOV - cam.desiredFOV);
    float b1 = Mathf.Clamp(cam.desiredFOV + this.FOVAdjustment, this.minFOV, this.maxFOV);
    cam.mainCamera.fieldOfView = Mathf.Lerp(cam.mainCamera.fieldOfView, b1, (float) (1.0 / (1.0 + 100.0 * (double) cam.fovChangeInertia)));
    float num1 = 1f + this.viewDistAdjust;
    Vector3 vector3_1 = (this.posVector.x * cam.followingUnit.transform.right + this.posVector.y * cam.followingUnit.transform.up + this.posVector.z * cam.followingUnit.transform.forward) * num1;
    Quaternion b2 = Quaternion.LookRotation(this.targetVector.x * cam.followingUnit.transform.right + this.targetVector.y * cam.followingUnit.transform.up + this.targetVector.z * cam.followingUnit.transform.forward, cam.followingUnit.transform.up);
    cam.transform.position = Vector3.Lerp(cam.transform.position, cam.cameraPivot.position + vector3_1, Mathf.Min(3f * Time.unscaledDeltaTime / PlayerSettings.viewSmoothing, 1f));
    Vector3 vector3_2 = cam.cameraPivot.position - cam.transform.position;
    if (Physics.Linecast(cam.cameraPivot.position, cam.cameraPivot.position - vector3_2 * 2f, out this.hit, this.layerMask))
    {
      float num2 = Mathf.Max(Vector3.Dot(vector3_2.normalized, this.hit.normal), 0.1f);
      Vector3 a2 = this.hit.point + vector3_2.normalized / num2;
      if ((double) FastMath.SquareDistance(a2, cam.cameraPivot.position) < (double) vector3_2.sqrMagnitude)
        cam.transform.position = a2;
    }
    cam.cameraPivot.rotation = Quaternion.Lerp(this.cameraRotationPrev, b2, Mathf.Min(5f * Time.unscaledDeltaTime / PlayerSettings.viewSmoothing, 1f));
    this.cameraRotationPrev = cam.cameraPivot.rotation;
    cam.transform.localRotation = Quaternion.Lerp(cam.transform.localRotation, this.cameraCustomRotation, Mathf.Min(5f * Time.unscaledDeltaTime / PlayerSettings.viewSmoothing, 1f));
    if (!GameManager.flightControlsEnabled)
      return;
    if (!Cursor.visible)
      this.zoomSpeed -= GameManager.playerInput.GetAxis("Zoom View") * 40f * Time.unscaledDeltaTime;
    this.zoomSpeed = Mathf.Lerp(this.zoomSpeed, 0.0f, 4f * Time.unscaledDeltaTime);
    this.zoomSpeed = Mathf.Clamp(this.zoomSpeed, -10f, 10f);
    this.viewDistAdjust += this.zoomSpeed * Time.unscaledDeltaTime;
    this.viewDistAdjust = Mathf.Clamp(this.viewDistAdjust, 0.0f, 10f);
    if (GameManager.gameState != GameState.Editor && GameManager.playerInput.GetButtonDown("Center"))
      cam.SwitchState((CameraBaseState) cam.orbitState);
    if (GameManager.gameState != GameState.Editor && GameManager.playerInput.GetButtonTimedPressUp("Switch View", 0.0f, PlayerSettings.clickDelay))
      cam.SwitchState((CameraBaseState) cam.TVState);
    if (GameManager.IsLocalAircraft(cam.followingUnit) || !CameraChaseState.AnyMoveInput())
      return;
    cam.SetFollowingUnit((Unit) null);
    cam.SwitchState((CameraBaseState) cam.freeState);
  }

  private static bool AnyMoveInput()
  {
    return (double) Mathf.Abs(GameManager.playerInput.GetAxis("Move Longitudinal")) > 0.10000000149011612 || (double) Mathf.Abs(GameManager.playerInput.GetAxis("Move Lateral")) > 0.10000000149011612;
  }

  public override void FixedUpdateState(CameraStateManager cam)
  {
  }

  public void CheckInput(CameraStateManager cam)
  {
    CameraChaseState.CameraPos cameraPos;
    if (!this.AircraftCameraPos.TryGetValue(cam.followingUnit.definition as AircraftDefinition, out cameraPos))
    {
      Debug.Log((object) $"Chase State - no camera position found for {cam.followingUnit.definition}");
    }
    else
    {
      if (SceneSingleton<CameraControlUI>.i.isOpen)
        return;
      if (Input.GetKeyDown(KeyCode.Keypad0))
      {
        this.currentPos = CameraChaseState.ChasePos.Back;
        this.posVector = cameraPos.back.position;
        this.targetVector = cameraPos.back.orientation;
      }
      else if (Input.GetKeyDown(KeyCode.Keypad1))
      {
        this.currentPos = CameraChaseState.ChasePos.WingRootL;
        this.posVector = cameraPos.wingRootL.position;
        this.targetVector = cameraPos.wingRootL.orientation;
      }
      else if (Input.GetKeyDown(KeyCode.Keypad2))
      {
        this.currentPos = CameraChaseState.ChasePos.Belly;
        this.posVector = cameraPos.belly.position;
        this.targetVector = cameraPos.belly.orientation;
      }
      else if (Input.GetKeyDown(KeyCode.Keypad3))
      {
        this.currentPos = CameraChaseState.ChasePos.WingRootR;
        this.posVector = cameraPos.wingRootR.position;
        this.targetVector = cameraPos.wingRootR.orientation;
      }
      else if (Input.GetKeyDown(KeyCode.Keypad4))
      {
        this.currentPos = CameraChaseState.ChasePos.WingL;
        this.posVector = cameraPos.wingL.position;
        this.targetVector = cameraPos.wingL.orientation;
      }
      else if (Input.GetKeyDown(KeyCode.Keypad5))
      {
        this.currentPos = CameraChaseState.ChasePos.Tail;
        this.posVector = cameraPos.tail.position;
        this.targetVector = cameraPos.tail.orientation;
      }
      else if (Input.GetKeyDown(KeyCode.Keypad6))
      {
        this.currentPos = CameraChaseState.ChasePos.WingR;
        this.posVector = cameraPos.wingR.position;
        this.targetVector = cameraPos.wingR.orientation;
      }
      else if (Input.GetKeyDown(KeyCode.Keypad7))
      {
        this.currentPos = CameraChaseState.ChasePos.Bottom;
        this.posVector = cameraPos.bottom.position;
        this.targetVector = cameraPos.bottom.orientation;
      }
      else if (Input.GetKeyDown(KeyCode.Keypad8))
      {
        this.currentPos = CameraChaseState.ChasePos.Front;
        this.posVector = cameraPos.front.position;
        this.targetVector = cameraPos.front.orientation;
      }
      else if (Input.GetKeyDown(KeyCode.Keypad9))
      {
        this.currentPos = CameraChaseState.ChasePos.Top;
        this.posVector = cameraPos.top.position;
        this.targetVector = cameraPos.top.orientation;
      }
      if (GameManager.gameState != GameState.Editor || !Input.GetKeyDown(KeyCode.H))
        return;
      this.ToggleHUD();
    }
  }

  public void CheckHUD()
  {
    if (!this.showHUD)
      FlightHud.EnableCanvas(false);
    else if (this.currentPos == CameraChaseState.ChasePos.Back || this.currentPos == CameraChaseState.ChasePos.Tail || this.currentPos == CameraChaseState.ChasePos.WingRootL || this.currentPos == CameraChaseState.ChasePos.WingRootR || this.currentPos == CameraChaseState.ChasePos.Belly)
    {
      FlightHud.EnableCanvas(true);
      DynamicMap.EnableCanvas(true);
    }
    else
    {
      FlightHud.EnableCanvas(false);
      DynamicMap.EnableCanvas(false);
    }
  }

  public void ToggleHUD()
  {
    this.showHUD = !this.showHUD;
    this.CheckHUD();
  }

  public void SetCustomTransform(Vector3 position, Vector3 eulerAngles)
  {
    this.currentPos = CameraChaseState.ChasePos.Custom;
    this.posVector = position;
    this.cameraCustomRotation = Quaternion.Euler(eulerAngles);
  }

  private enum ChasePos
  {
    Back,
    Front,
    Top,
    Bottom,
    Belly,
    Tail,
    WingL,
    WingR,
    WingRootL,
    WingRootR,
    Custom,
  }

  private struct CameraPos
  {
    public CameraChaseState.CameraPos.POV back;
    public CameraChaseState.CameraPos.POV front;
    public CameraChaseState.CameraPos.POV top;
    public CameraChaseState.CameraPos.POV bottom;
    public CameraChaseState.CameraPos.POV belly;
    public CameraChaseState.CameraPos.POV tail;
    public CameraChaseState.CameraPos.POV wingL;
    public CameraChaseState.CameraPos.POV wingR;
    public CameraChaseState.CameraPos.POV wingRootL;
    public CameraChaseState.CameraPos.POV wingRootR;

    public struct POV
    {
      public Vector3 position;
      public Vector3 orientation;
    }
  }
}
