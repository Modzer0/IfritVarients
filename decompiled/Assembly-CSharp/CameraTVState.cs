// Decompiled with JetBrains decompiler
// Type: CameraTVState
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;

#nullable disable
public class CameraTVState : CameraBaseState
{
  private RaycastHit hit;
  private Vector2 panTiltView;
  private Vector2 desiredPanTiltView;
  private CameraStateManager cam;
  private float shotTime;
  private float camTimer = 3f;
  private float targetSize;
  private float zoomSpeed;
  private float FOVAdjustment;
  private float transitionTimer;
  private Vector3 minimumOffset = Vector3.zero;

  public override void EnterState(CameraStateManager cam)
  {
    if (PlayerSettings.cinematicMode)
      CursorManager.Refresh();
    if (cam.followingUnit is Aircraft followingUnit)
    {
      followingUnit.SetDoppler(true);
      if (followingUnit.cockpit.IsDetached())
      {
        cam.followingRB = followingUnit.cockpit.rb;
        this.transitionTimer = 0.0f;
      }
      else
        followingUnit.cockpit.onParentDetached += new Action<UnitPart>(this.CameraTVState_OnCockpitDetach);
    }
    cam.cameraPivot.SetParent(Datum.origin);
    cam.transform.SetParent(cam.cameraPivot);
    cam.transform.localPosition = Vector3.zero;
    cam.cameraVelocity = Vector3.zero;
    cam.mainCamera.nearClipPlane = 1f;
    FlightHud.EnableCanvas(false);
    this.cam = cam;
    this.camTimer = 3f;
    this.shotTime = 0.0f;
    CameraStateManager.cameraMode = CameraMode.tv;
    this.FOVAdjustment = 0.0f;
    this.targetSize = 20f;
    if ((UnityEngine.Object) cam.followingUnit != (UnityEngine.Object) null)
    {
      this.targetSize = Mathf.Max(cam.followingUnit.definition.length, cam.followingUnit.definition.width * 0.7f) + Mathf.Max(cam.followingUnit.definition.width, cam.followingUnit.definition.length * 0.5f);
      this.targetSize /= Mathf.Clamp(Mathf.Pow(this.targetSize * 0.1f, 0.2f), 0.6f, 1.5f);
    }
    if (!((UnityEngine.Object) cam.followingUnit != (UnityEngine.Object) null))
      return;
    this.GetTVCamPoint(cam.followingUnit.transform.position, cam.followingUnit.rb.velocity);
  }

  private void CameraTVState_OnCockpitDetach(UnitPart cockpitPart)
  {
    this.transitionTimer = 0.0001f;
  }

  public override void LeaveState(CameraStateManager cam)
  {
    cam.mainCamera.fieldOfView = cam.desiredFOV;
    if (!(cam.followingUnit is Aircraft followingUnit))
      return;
    followingUnit.cockpit.onParentDetached -= new Action<UnitPart>(this.CameraTVState_OnCockpitDetach);
  }

  public override void UpdateState(CameraStateManager cam)
  {
    if (Physics.Linecast(cam.cameraPivot.position + Vector3.up * 2000f, cam.cameraPivot.position - Vector3.up * 1.7f, out this.hit, -8193))
      cam.cameraPivot.position = this.hit.point + Vector3.up * 1.7f;
    cam.cameraVelocity = Vector3.zero;
    float num1 = Vector3.Distance(cam.followingRB.position, cam.transform.position);
    cam.transform.LookAt(cam.followingRB.transform.position);
    if ((double) this.transitionTimer > 0.0)
    {
      Aircraft followingUnit = cam.followingUnit as Aircraft;
      this.transitionTimer += Time.deltaTime;
      Vector3 worldPosition = Vector3.Lerp(cam.followingRB.position, followingUnit.cockpit.transform.position, this.transitionTimer);
      cam.transform.LookAt(worldPosition);
      if ((double) this.transitionTimer > 1.0)
      {
        this.transitionTimer = 0.0f;
        cam.followingRB = followingUnit.cockpit.rb;
      }
    }
    this.desiredPanTiltView.x += GameManager.playerInput.GetAxis("Pan View") * 3f * Time.unscaledDeltaTime;
    this.desiredPanTiltView.y += GameManager.playerInput.GetAxis("Tilt View") * 3f * Time.unscaledDeltaTime;
    this.panTiltView = Vector2.Lerp(this.panTiltView, this.desiredPanTiltView, Mathf.Min(0.5f * Time.unscaledDeltaTime / PlayerSettings.viewSmoothing, 1f));
    cam.transform.rotation *= Quaternion.AngleAxis(this.panTiltView.x, Vector3.up);
    cam.transform.rotation *= Quaternion.AngleAxis(this.panTiltView.y, Vector3.right);
    float num2 = (float) (1.5 * (double) this.targetSize - (double) num1 * 0.20000000298023224);
    this.FOVAdjustment -= 1f * GameManager.playerInput.GetAxis("Zoom View");
    this.FOVAdjustment = Mathf.Clamp(this.FOVAdjustment, 5f - num2, 80f - num2);
    float target = Mathf.Clamp(num2 + this.FOVAdjustment, 5f, 80f);
    cam.mainCamera.fieldOfView = Mathf.SmoothDamp(cam.mainCamera.fieldOfView, target, ref this.zoomSpeed, 0.7f, float.MaxValue, Time.unscaledDeltaTime);
    if ((double) Vector3.Dot(cam.followingRB.velocity, cam.transform.position - cam.followingRB.position) < 0.0)
      this.shotTime += Time.deltaTime;
    else
      this.shotTime = 0.0f;
    if ((double) this.shotTime > (double) this.camTimer)
    {
      this.GetTVCamPoint(cam.followingRB.position, cam.followingRB.velocity);
      this.shotTime = 0.0f;
    }
    if (!GameManager.flightControlsEnabled)
      return;
    if (GameManager.playerInput.GetButtonDown("Center"))
      this.panTiltView = Vector2.zero;
    if (!GameManager.playerInput.GetButtonTimedPressUp("Switch View", 0.0f, PlayerSettings.clickDelay))
      return;
    cam.SwitchState((UnityEngine.Object) cam.followingUnit.cockpitViewPoint != (UnityEngine.Object) null ? (CameraBaseState) cam.cockpitState : (CameraBaseState) cam.orbitState);
  }

  public override void FixedUpdateState(CameraStateManager cam)
  {
  }

  private void GetTVCamPoint(Vector3 targetPos, Vector3 targetVel)
  {
    this.panTiltView = Vector2.zero;
    this.desiredPanTiltView = Vector2.zero;
    Vector3 position = targetPos + targetVel * 2f + this.minimumOffset + Vector3.Cross(targetVel, Vector3.up).normalized * Mathf.Sign(UnityEngine.Random.value - 0.5f) * (float) UnityEngine.Random.Range(1, 4) * this.targetSize + (float) UnityEngine.Random.Range(-20, 20) * Vector3.up;
    if ((double) targetVel.sqrMagnitude < 10.0)
      position = targetPos + new Vector3((float) UnityEngine.Random.Range(-1, 1), 0.0f, (float) UnityEngine.Random.Range(-1, 1)).normalized * (float) UnityEngine.Random.Range(20, 50);
    if (Physics.Linecast(position + Vector3.up * 2000f, position - Vector3.up * 1.7f, out this.hit, -8193))
      position.y = this.hit.point.y + 1.7f;
    GlobalPosition globalPosition = position.ToGlobalPosition();
    globalPosition.y = Mathf.Max(globalPosition.y, 1.7f);
    this.cam.cameraPivot.localPosition = globalPosition.AsVector3();
  }

  public void SetCustomTransform(Vector3 position, Vector3 eulerAngles)
  {
    this.minimumOffset = position;
  }
}
