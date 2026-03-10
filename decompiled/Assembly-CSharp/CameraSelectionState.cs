// Decompiled with JetBrains decompiler
// Type: CameraSelectionState
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

#nullable disable
public class CameraSelectionState : CameraBaseState
{
  private bool manualViewMode;
  private float cameraDistance = 10f;
  private float cameraSmoothingVel;
  private float cameraHeight;
  private bool allowMoveToDropFocus;
  private float viewDistance;
  private bool fixedTarget;
  private Transform target;
  private float targetSize;
  private Transform followProxy;
  private float orbitalAngle;

  public void FocusAirbase(
    CameraStateManager cam,
    Airbase airbase,
    bool allowMoveToDropFocus,
    float viewDistance = 20f,
    float upDistance = 1.75f)
  {
    if ((Object) airbase.fixedCameraTransform != (Object) null)
    {
      this.fixedTarget = true;
      this.target = airbase.fixedCameraTransform;
    }
    else
    {
      this.fixedTarget = false;
      this.target = airbase.aircraftSelectionTransform;
    }
    this.allowMoveToDropFocus = allowMoveToDropFocus;
    cam.SwitchState((CameraBaseState) this);
    if ((Object) this.followProxy != (Object) null)
      Object.Destroy((Object) this.followProxy.gameObject);
    this.followProxy = new GameObject("CameraFollowProxy").transform;
    this.followProxy.SetParent(airbase.center.transform);
    this.cameraDistance = viewDistance;
    this.viewDistance = viewDistance;
    Vector3 vector3 = Vector3.up * upDistance;
    Vector3 position1 = airbase.aircraftSelectionTransform.position + vector3;
    cam.cameraPivot.SetPositionAndRotation(position1, Quaternion.identity);
    this.followProxy.SetPositionAndRotation(position1, Quaternion.identity);
    Vector3 position2 = position1 + Vector3.forward * (viewDistance + 8f);
    cam.transform.SetPositionAndRotation(position2, Quaternion.LookRotation(Vector3.back, Vector3.up));
    this.orbitalAngle = 30f;
    this.UpdateOrbit(cam);
  }

  private void UpdateOrbit(CameraStateManager cam)
  {
    Quaternion quaternion1 = !((Object) this.followProxy != (Object) null) ? Quaternion.identity : this.followProxy.rotation;
    this.orbitalAngle %= 360f;
    Quaternion quaternion2 = quaternion1 * Quaternion.Euler(0.0f, this.orbitalAngle, 0.0f);
    cam.cameraPivot.rotation = quaternion2;
  }

  public void SetPreviewAircraft(Aircraft previewAircraft)
  {
    if (!this.fixedTarget)
      this.target = previewAircraft.transform;
    this.viewDistance = (float) (((double) Mathf.Max(previewAircraft.definition.length, previewAircraft.definition.width * 0.7f) + (double) Mathf.Max(previewAircraft.definition.width, previewAircraft.definition.length * 0.7f)) * 0.5);
    this.viewDistance /= Mathf.Clamp(Mathf.Pow(this.viewDistance * 0.1f, 0.2f), 0.6f, 1.5f);
  }

  public override void EnterState(CameraStateManager cam)
  {
    cam.mainCamera.nearClipPlane = 1f;
    cam.mainCamera.fieldOfView = 50f;
    FlightHud.EnableCanvas(false);
    cam.transform.SetParent(cam.cameraPivot);
    CameraStateManager.cameraMode = CameraMode.selection;
    cam.cameraVelocity = Vector3.zero;
    SceneSingleton<DynamicMap>.i.Minimize();
  }

  public override void LeaveState(CameraStateManager cam)
  {
    cam.mainCamera.fieldOfView = cam.desiredFOV;
    if (!((Object) this.followProxy != (Object) null))
      return;
    Object.Destroy((Object) this.followProxy.gameObject);
  }

  public override void UpdateState(CameraStateManager cam)
  {
    if ((Object) this.target == (Object) null)
    {
      ColorLog<CameraSelectionState>.Info("Target destroyed changing to free cam");
      cam.SetFollowingUnit((Unit) null);
      cam.SwitchState((CameraBaseState) cam.freeState);
    }
    else
    {
      if ((Object) this.followProxy != (Object) null)
        cam.cameraPivot.position = this.followProxy.position;
      if (this.fixedTarget)
      {
        Vector3 position;
        Quaternion rotation;
        this.target.GetPositionAndRotation(out position, out rotation);
        cam.transform.SetPositionAndRotation(position, rotation);
      }
      else
      {
        float unscaledDeltaTime = Time.unscaledDeltaTime;
        this.MoveCamera(cam, unscaledDeltaTime);
      }
      if (!this.allowMoveToDropFocus)
        return;
      this.CheckChangeState(cam);
    }
  }

  private void MoveCamera(CameraStateManager cam, float deltaTime)
  {
    float num1 = GameManager.playerInput.GetAxis("Pan View") * PlayerSettings.viewSensitivity;
    float num2 = GameManager.playerInput.GetAxis("Tilt View") * PlayerSettings.viewSensitivity;
    float f1 = num1 * -0.2f;
    float f2 = num2 * -0.2f;
    if (!GameManager.playerInput.GetButton("Free Look"))
    {
      f1 = 0.0f;
      f2 = 0.0f;
    }
    if ((double) Mathf.Abs(f1) > 0.75 || (double) Mathf.Abs(f2) > 0.75)
      this.manualViewMode = true;
    if (this.manualViewMode)
    {
      this.orbitalAngle += f1 * -150f * deltaTime;
      this.cameraHeight += f2 * -0.5f * deltaTime;
      this.cameraHeight = Mathf.Clamp01(this.cameraHeight);
    }
    else
      this.orbitalAngle -= 12f * deltaTime;
    this.UpdateOrbit(cam);
    Quaternion b = Quaternion.LookRotation(this.target.position - cam.transform.position);
    Vector3 position = cam.cameraPivot.position + (cam.cameraPivot.forward + Vector3.up * this.cameraHeight) * this.cameraDistance * 1.4f;
    Quaternion rotation = Quaternion.Slerp(cam.transform.rotation, b, 5f * deltaTime);
    cam.transform.SetPositionAndRotation(position, rotation);
    this.cameraDistance = Mathf.SmoothDamp(this.cameraDistance, this.viewDistance, ref this.cameraSmoothingVel, 0.5f, float.MaxValue, deltaTime);
  }

  private void CheckChangeState(CameraStateManager cam)
  {
    if (!AnyMoveInput())
      return;
    cam.SetFollowingUnit((Unit) null);
    cam.SwitchState((CameraBaseState) cam.freeState);

    static bool AnyMoveInput()
    {
      return (double) Mathf.Abs(GameManager.playerInput.GetAxis("Move Longitudinal")) > 0.10000000149011612 || (double) Mathf.Abs(GameManager.playerInput.GetAxis("Move Lateral")) > 0.10000000149011612;
    }
  }

  public override void FixedUpdateState(CameraStateManager cam)
  {
  }
}
