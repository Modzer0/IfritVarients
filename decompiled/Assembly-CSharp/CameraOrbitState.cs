// Decompiled with JetBrains decompiler
// Type: CameraOrbitState
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

#nullable disable
public class CameraOrbitState : CameraBaseState
{
  private RaycastHit hit;
  private float panView;
  private float tiltView;
  private Vector3 followVector;
  private Vector3 flatVelSmoothed;
  private float viewDistAdjust;
  private int layerMask = 64 /*0x40*/;
  private float zoomSpeed;
  private Quaternion pivotRotationPrev;
  private Quaternion cameraRotationPrev;
  private float FOVAdjustment;
  private float minFOV = 20f;
  private float maxFOV = 120f;
  private float followingMaxRadius;
  private float transitionTimer;
  private CameraOrbitState.CameraFocus cameraFocus = CameraOrbitState.CameraFocus.current;
  private Transform lookAtTransform;

  public override void EnterState(CameraStateManager cam)
  {
    this.viewDistAdjust = 0.0f;
    this.zoomSpeed = 0.0f;
    this.transitionTimer = 0.0f;
    if ((UnityEngine.Object) cam.followingUnit != (UnityEngine.Object) null && !cam.followingUnit.disabled)
    {
      this.lookAtTransform = cam.followingUnit.transform;
      this.followVector = cam.followingUnit.transform.forward;
      this.followVector.y = 0.0f;
      cam.cameraPivot.SetParent(cam.followingUnit.transform);
      cam.transform.SetParent(cam.cameraPivot);
    }
    cam.cameraPivot.localPosition = Vector3.zero;
    this.FOVAdjustment = 0.0f;
    if ((UnityEngine.Object) cam.followingUnit != (UnityEngine.Object) null && !cam.followingUnit.disabled && cam.followingUnit is Aircraft followingUnit)
    {
      followingUnit.SetDoppler(false);
      if (followingUnit.cockpit.IsDetached())
      {
        cam.followingRB = followingUnit.cockpit.rb;
        cam.cameraPivot.SetParent(followingUnit.cockpit.transform);
        cam.cameraPivot.localPosition = Vector3.zero;
        this.lookAtTransform = followingUnit.cockpit.transform;
      }
      else
        followingUnit.cockpit.onParentDetached += new Action<UnitPart>(this.CameraOrbitState_OnCockpitDetach);
    }
    else
    {
      cam.previousFollowingUnit = (Unit) null;
      if ((UnityEngine.Object) cam.followingRB != (UnityEngine.Object) null)
      {
        cam.cameraPivot.SetParent(cam.followingRB.transform);
        cam.cameraPivot.localPosition = Vector3.zero;
        this.lookAtTransform = cam.followingRB.transform;
        this.followVector = cam.followingRB.transform.forward;
        this.followVector.y = 0.0f;
      }
    }
    cam.mainCamera.nearClipPlane = 1f;
    if (!GameManager.IsLocalAircraft(cam.followingUnit) && (UnityEngine.Object) cam.previousFollowingUnit != (UnityEngine.Object) null && (UnityEngine.Object) cam.previousFollowingUnit != (UnityEngine.Object) cam.followingUnit && (UnityEngine.Object) cam.previousFollowingUnit.NetworkHQ != (UnityEngine.Object) null && (UnityEngine.Object) cam.followingUnit.NetworkHQ != (UnityEngine.Object) null && (UnityEngine.Object) cam.previousFollowingUnit.NetworkHQ != (UnityEngine.Object) cam.followingUnit.NetworkHQ)
    {
      this.cameraFocus = CameraOrbitState.CameraFocus.previous;
      this.lookAtTransform = cam.previousFollowingUnit.transform;
      this.followVector = this.lookAtTransform.transform.position - cam.followingUnit.transform.position;
    }
    else
    {
      this.cameraFocus = CameraOrbitState.CameraFocus.current;
      this.pivotRotationPrev = Quaternion.identity;
      this.cameraRotationPrev = Quaternion.identity;
    }
    if ((UnityEngine.Object) cam.followingRB != (UnityEngine.Object) null)
    {
      this.flatVelSmoothed = cam.followingRB.velocity + this.lookAtTransform.forward * 10f;
      this.flatVelSmoothed.y = 0.0f;
    }
    cam.transform.position = cam.cameraPivot.position - this.followVector.normalized * cam.followingUnit.maxRadius * 2f + Vector3.up * cam.followingUnit.maxRadius * 0.8f;
    this.panView = 0.0f;
    this.tiltView = 0.0f;
    FlightHud.EnableCanvas(false);
    CameraStateManager.cameraMode = CameraMode.orbit;
    ((UniversalRenderPipelineAsset) GraphicsSettings.currentRenderPipeline).shadowDistance = Mathf.Max(2000f, (float) (2000.0 * (double) cam.followingUnit.maxRadius * 2.0 / 30.0));
  }

  public override void LeaveState(CameraStateManager cam)
  {
    cam.cameraPivot.SetParent((Transform) null);
    ((UniversalRenderPipelineAsset) GraphicsSettings.currentRenderPipeline).shadowDistance = 2000f;
    if (!((UnityEngine.Object) cam.followingUnit != (UnityEngine.Object) null) || !(cam.followingUnit is Aircraft followingUnit))
      return;
    followingUnit.SetDoppler(true);
    followingUnit.cockpit.onParentDetached -= new Action<UnitPart>(this.CameraOrbitState_OnCockpitDetach);
  }

  private void CameraOrbitState_OnCockpitDetach(UnitPart cockpitPart)
  {
    this.transitionTimer = 1f / 1000f;
    SceneSingleton<CameraStateManager>.i.cameraPivot.transform.SetParent((Transform) null);
  }

  public override void UpdateState(CameraStateManager cam)
  {
    if ((UnityEngine.Object) cam.followingRB == (UnityEngine.Object) null)
      return;
    Vector3 vector3 = cam.followingRB.velocity;
    if ((double) this.transitionTimer > 0.0)
    {
      cam.cameraPivot.SetParent((Transform) null);
      Aircraft followingUnit = cam.followingUnit as Aircraft;
      this.lookAtTransform = followingUnit.cockpit.transform;
      this.transitionTimer += Time.deltaTime;
      cam.cameraPivot.position = Vector3.Lerp(followingUnit.transform.position, followingUnit.cockpit.transform.position, this.transitionTimer);
      vector3 = Vector3.Lerp(vector3, followingUnit.cockpit.rb.velocity, this.transitionTimer);
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
    this.UpdateCameraPosition(cam, vector3);
    if (!GameManager.flightControlsEnabled)
      return;
    this.UpdateInputs(cam);
    if (GameManager.playerInput.GetButtonDown("Center"))
    {
      this.panView = 0.0f;
      this.tiltView = 0.0f;
      if (GameManager.gameState != GameState.Editor)
        cam.SwitchState((CameraBaseState) cam.chaseState);
    }
    if (GameManager.playerInput.GetButtonDown("Cycle Look At"))
      this.CycleLookAt(cam);
    if (GameManager.gameState != GameState.Editor && GameManager.playerInput.GetButtonTimedPressUp("Switch View", 0.0f, PlayerSettings.clickDelay))
      cam.SwitchState((CameraBaseState) cam.TVState);
    if (Input.GetMouseButton(1))
      CursorManager.Refresh();
    if (!((UnityEngine.Object) cam.followingUnit == (UnityEngine.Object) null) && GameManager.IsLocalAircraft(cam.followingUnit) && !cam.followingUnit.disabled || !CameraOrbitState.AnyMoveInput())
      return;
    cam.SetFollowingUnit((Unit) null);
    cam.SwitchState((CameraBaseState) cam.freeState);
  }

  private static bool AnyMoveInput()
  {
    return (double) Mathf.Abs(GameManager.playerInput.GetAxis("Move Longitudinal")) > 0.10000000149011612 || (double) Mathf.Abs(GameManager.playerInput.GetAxis("Move Lateral")) > 0.10000000149011612;
  }

  private static bool AnyPanTilt()
  {
    return (double) Mathf.Abs(GameManager.playerInput.GetAxis("Pan View")) > 0.10000000149011612 || (double) Mathf.Abs(GameManager.playerInput.GetAxis("Tilt View")) > 0.10000000149011612;
  }

  public override void FixedUpdateState(CameraStateManager cam)
  {
  }

  private void UpdateCameraPosition(CameraStateManager cam, Vector3 targetVel)
  {
    cam.cameraVelocity = targetVel * Time.timeScale;
    float t = Mathf.Min(3f * Time.unscaledDeltaTime / PlayerSettings.viewSmoothing, 1f);
    if ((UnityEngine.Object) this.lookAtTransform == (UnityEngine.Object) null)
    {
      this.SetLookAtFollowed(cam);
    }
    else
    {
      if ((UnityEngine.Object) cam.followingRB != (UnityEngine.Object) null)
        this.flatVelSmoothed = Vector3.RotateTowards(this.flatVelSmoothed, (cam.followingRB.velocity + this.lookAtTransform.forward * 10f) with
        {
          y = 0.0f
        }, 1.04719758f * Time.fixedDeltaTime, 1f);
      if ((UnityEngine.Object) cam.followingUnit != (UnityEngine.Object) null)
        this.followingMaxRadius = cam.followingUnit.maxRadius;
      float num1 = (float) (1.0 + (double) this.followingMaxRadius * (1.0 + (double) this.viewDistAdjust));
      Vector3 vector3_1 = -this.followVector.normalized * num1 * 2f + Vector3.up * num1 * 0.8f;
      if (this.cameraFocus == CameraOrbitState.CameraFocus.current)
      {
        this.followVector = Vector3.SmoothDamp(this.followVector, this.flatVelSmoothed, ref cam.cameraVelocity, 2f * t);
        cam.cameraPivot.rotation = Quaternion.LookRotation(this.followVector, Vector3.up);
        cam.transform.position = Vector3.Lerp(cam.transform.position, cam.cameraPivot.position + vector3_1, t);
        cam.cameraPivot.Rotate(0.0f, this.panView, 0.0f, Space.World);
        cam.cameraPivot.Rotate(this.tiltView, 0.0f, 0.0f, Space.Self);
        cam.cameraPivot.rotation = Quaternion.Lerp(this.pivotRotationPrev, cam.cameraPivot.rotation, t);
      }
      else
      {
        if ((UnityEngine.Object) cam.followingUnit == (UnityEngine.Object) null || (UnityEngine.Object) this.lookAtTransform == (UnityEngine.Object) null)
        {
          if (this.cameraFocus == CameraOrbitState.CameraFocus.target)
            this.SetLookAtTarget(cam);
          else
            this.SetLookAtFollowed(cam);
        }
        else
          this.followVector = Vector3.SmoothDamp(this.followVector, this.lookAtTransform.transform.position - cam.followingUnit.transform.position, ref cam.cameraVelocity, 1f * t);
        cam.cameraPivot.rotation = Quaternion.LookRotation(this.followVector, Vector3.up);
        cam.transform.position = Vector3.Lerp(cam.transform.position, cam.cameraPivot.position + vector3_1, t);
        cam.cameraPivot.rotation = Quaternion.Lerp(this.pivotRotationPrev, cam.cameraPivot.rotation, t);
      }
      this.pivotRotationPrev = cam.cameraPivot.rotation;
      Vector3 vector3_2 = cam.cameraPivot.position - cam.transform.position;
      if (Physics.Linecast(cam.cameraPivot.position, cam.cameraPivot.position - vector3_2 * 2f, out this.hit, this.layerMask))
      {
        float num2 = Mathf.Max(Vector3.Dot(vector3_2.normalized, this.hit.normal), 0.1f);
        Vector3 a = this.hit.point + vector3_2.normalized / num2;
        if ((double) FastMath.SquareDistance(a, cam.cameraPivot.position) < (double) vector3_2.sqrMagnitude)
          cam.transform.position = a;
      }
      cam.transform.LookAt(cam.followingUnit.transform, Vector3.up);
      cam.transform.localRotation = Quaternion.Lerp(this.cameraRotationPrev, cam.transform.localRotation, t);
      this.cameraRotationPrev = cam.transform.localRotation;
    }
  }

  private void UpdateInputs(CameraStateManager cam)
  {
    if (!Cursor.visible)
    {
      float axis1 = GameManager.playerInput.GetAxis("Pan View");
      float axis2 = GameManager.playerInput.GetAxis("Tilt View");
      if (this.cameraFocus == CameraOrbitState.CameraFocus.current)
      {
        this.panView += (float) ((double) Mathf.Min(cam.mainCamera.fieldOfView / 20f, 1f) * (double) axis1 * 90.0) * PlayerSettings.viewSensitivity * Time.unscaledDeltaTime;
        this.tiltView += (float) ((double) Mathf.Min(cam.mainCamera.fieldOfView / 20f, 1f) * (double) axis2 * 90.0 * (double) PlayerSettings.viewSensitivity * (double) Time.unscaledDeltaTime * (PlayerSettings.viewInvertPitch ? -1.0 : 1.0));
      }
      else if (CameraOrbitState.AnyPanTilt())
        this.SetLookAtFollowed(cam);
      this.zoomSpeed -= GameManager.playerInput.GetAxis("Zoom View") * 60f * Time.unscaledDeltaTime;
    }
    this.FOVAdjustment -= cam.fovChangeSpeed * Mathf.Min(cam.mainCamera.fieldOfView / 20f, 1f) * GameManager.playerInput.GetAxis("FOV");
    this.FOVAdjustment = Mathf.Clamp(this.FOVAdjustment, this.minFOV - cam.desiredFOV, this.maxFOV - cam.desiredFOV);
    float b = Mathf.Clamp(cam.desiredFOV + this.FOVAdjustment, this.minFOV, this.maxFOV);
    cam.mainCamera.fieldOfView = Mathf.Lerp(cam.mainCamera.fieldOfView, b, (float) (1.0 / (1.0 + 100.0 * (double) cam.fovChangeInertia)));
    this.zoomSpeed = Mathf.Lerp(this.zoomSpeed, 0.0f, 4f * Time.unscaledDeltaTime);
    this.zoomSpeed = Mathf.Clamp(this.zoomSpeed, -10f, 10f);
    this.viewDistAdjust += this.zoomSpeed * Time.unscaledDeltaTime;
    this.viewDistAdjust = Mathf.Clamp(this.viewDistAdjust, 0.0f, 10f);
  }

  private void CycleLookAt(CameraStateManager cam)
  {
    if (this.cameraFocus == CameraOrbitState.CameraFocus.previous)
    {
      if (Input.GetKey(KeyCode.LeftShift))
        this.SetLookAtTarget(cam);
      else
        this.SetLookAtFollowed(cam);
    }
    else if (this.cameraFocus == CameraOrbitState.CameraFocus.current)
    {
      if (Input.GetKey(KeyCode.LeftShift))
        this.SetLookAtPrevious(cam);
      else
        this.SetLookAtTarget(cam);
    }
    else if (Input.GetKey(KeyCode.LeftShift))
      this.SetLookAtTarget(cam);
    else
      this.SetLookAtFollowed(cam);
  }

  public void SetLookAtFollowed(CameraStateManager cam)
  {
    this.cameraFocus = CameraOrbitState.CameraFocus.current;
    if ((UnityEngine.Object) cam.followingUnit != (UnityEngine.Object) null)
    {
      this.lookAtTransform = cam.followingUnit.transform;
      if (cam.followingUnit is Aircraft followingUnit && followingUnit.cockpit.IsDetached())
        this.lookAtTransform = followingUnit.cockpit.transform;
    }
    this.panView = 0.0f;
    this.tiltView = 0.0f;
  }

  public void SetLookAtTarget(CameraStateManager cam)
  {
    if ((UnityEngine.Object) cam.followingUnit == (UnityEngine.Object) null)
    {
      this.SetLookAtFollowed(cam);
    }
    else
    {
      Unit unit = (Unit) null;
      if (cam.followingUnit is Missile followingUnit2)
        UnitRegistry.TryGetUnit(new PersistentID?(followingUnit2.targetID), out unit);
      else if (cam.followingUnit is Aircraft followingUnit1)
      {
        List<Unit> targetList = followingUnit1.weaponManager.GetTargetList();
        if (targetList.Count > 0)
          unit = targetList[0];
      }
      else
      {
        Turret componentInChildren = cam.followingUnit.gameObject.GetComponentInChildren<Turret>();
        if ((UnityEngine.Object) componentInChildren != (UnityEngine.Object) null)
          unit = (UnityEngine.Object) componentInChildren.GetTarget() != (UnityEngine.Object) null ? componentInChildren.GetTarget() : (Unit) null;
      }
      if ((UnityEngine.Object) unit == (UnityEngine.Object) null)
      {
        this.SetLookAtFollowed(cam);
      }
      else
      {
        this.cameraFocus = CameraOrbitState.CameraFocus.target;
        this.lookAtTransform = unit.transform;
      }
    }
  }

  public void SetLookAtPrevious(CameraStateManager cam)
  {
    if ((UnityEngine.Object) cam.previousFollowingUnit != (UnityEngine.Object) null && (UnityEngine.Object) cam.previousFollowingUnit != (UnityEngine.Object) cam.followingUnit)
    {
      this.cameraFocus = CameraOrbitState.CameraFocus.previous;
      this.lookAtTransform = cam.previousFollowingUnit.transform;
    }
    else
      this.SetLookAtFollowed(cam);
  }

  public string GetCurrentFocus() => $"{this.cameraFocus}";

  public string GetLookAtUnit()
  {
    return (UnityEngine.Object) this.lookAtTransform == (UnityEngine.Object) null ? string.Empty : this.lookAtTransform.name ?? "";
  }

  private enum CameraFocus
  {
    previous,
    current,
    target,
  }
}
