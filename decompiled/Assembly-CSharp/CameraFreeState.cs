// Decompiled with JetBrains decompiler
// Type: CameraFreeState
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.MissionEditorScripts;
using UnityEngine;

#nullable disable
public class CameraFreeState : CameraBaseState
{
  private float FOVAdjustment;
  private float minFOV = 1f;
  private float maxFOV = 120f;
  private float stateTime;
  private RaycastHit hit;
  private bool trackTarget;
  private float panView;
  private float tiltView;
  public bool DontResetRotationFlag;

  public override void EnterState(CameraStateManager cam)
  {
    cam.cameraPivot.SetParent((Transform) null);
    cam.transform.SetParent((Transform) null, true);
    CameraStateManager.cameraMode = CameraMode.free;
    cam.cockpitCamRender.enabled = false;
    FlightHud.EnableCanvas(false);
    this.FOVAdjustment = 0.0f;
    this.stateTime = 0.0f;
    this.trackTarget = true;
    this.panView = cam.transform.eulerAngles.y;
    this.tiltView = !this.DontResetRotationFlag ? 0.0f : cam.transform.eulerAngles.x;
    this.DontResetRotationFlag = false;
    if (!((Object) cam.previousFollowingUnit != (Object) null))
      return;
    cam.transform.LookAt(cam.previousFollowingUnit.transform.position);
  }

  public override void LeaveState(CameraStateManager cam)
  {
  }

  public override void UpdateState(CameraStateManager cam)
  {
    this.stateTime += Time.deltaTime;
    cam.windNoiseExternal.volume = 0.0f;
    this.FOVAdjustment -= cam.fovChangeSpeed * Mathf.Min(cam.mainCamera.fieldOfView / 20f, 1f) * GameManager.playerInput.GetAxis("FOV");
    this.FOVAdjustment = Mathf.Clamp(this.FOVAdjustment, this.minFOV - cam.desiredFOV, this.maxFOV - cam.desiredFOV);
    float b = Mathf.Clamp(cam.desiredFOV + this.FOVAdjustment, this.minFOV, this.maxFOV);
    cam.mainCamera.fieldOfView = Mathf.Lerp(cam.mainCamera.fieldOfView, b, (float) (1.0 / (1.0 + 100.0 * (double) cam.fovChangeInertia)));
    if (Input.GetMouseButton(1))
      CursorManager.Refresh();
    bool flag = false;
    if (!InputFieldChecker.InsideInputField)
    {
      float f1 = (float) ((double) Mathf.Min(cam.mainCamera.fieldOfView / 20f, 1f) * (double) GameManager.playerInput.GetAxis("Pan View") * 0.30000001192092896 * (double) PlayerSettings.viewSensitivity * (PlayerSettings.viewInvertPitch ? -1.0 : 1.0));
      float f2 = (float) ((double) Mathf.Min(cam.mainCamera.fieldOfView / 20f, 1f) * (double) GameManager.playerInput.GetAxis("Tilt View") * 0.30000001192092896) * PlayerSettings.viewSensitivity;
      if (!GameManager.playerInput.GetButton("Free Look"))
      {
        f1 = 0.0f;
        f2 = 0.0f;
      }
      if ((double) Mathf.Abs(f1) > 0.0 || (double) Mathf.Abs(f2) > 0.0)
      {
        this.tiltView += f2;
        this.panView += f1;
        this.trackTarget = false;
      }
      float axis1 = GameManager.playerInput.GetAxis("Move Longitudinal");
      float axis2 = GameManager.playerInput.GetAxis("Move Lateral");
      float axis3 = GameManager.playerInput.GetAxis("Move Vertical");
      if (cam.allowInputs && ((double) axis1 != 0.0 || (double) axis2 != 0.0 || (double) axis3 != 0.0))
      {
        float num = 500f;
        if (Input.GetKey(KeyCode.LeftShift))
          num = 5000f;
        Vector3 vector3 = cam.desiredTransSpeed * num * Time.unscaledDeltaTime * (cam.transform.forward * axis1 + cam.transform.right * axis2 + Vector3.up * axis3);
        cam.cameraVelocity += vector3;
        flag = true;
      }
    }
    Vector3 position1;
    Quaternion rotation1;
    cam.transform.GetPositionAndRotation(out position1, out rotation1);
    Vector3 position2 = position1 + cam.cameraVelocity * Time.unscaledDeltaTime;
    Quaternion rotation2 = Quaternion.Lerp(rotation1, Quaternion.Euler(this.tiltView, this.panView, 0.0f), Mathf.Min(2f * Time.unscaledDeltaTime / PlayerSettings.viewSmoothing, 1f));
    if (((Object) SceneSingleton<MissionEditor>.i == (Object) null || !SceneSingleton<MissionEditor>.i.allowCameraClip) && Physics.Linecast(position2 + Vector3.up * 5000f, position2 - Vector3.up * 5000f, out this.hit, 64 /*0x40*/))
      position2 = new Vector3(position2.x, Mathf.Max(position2.y, this.hit.point.y + 1.7f), position2.z);
    float num1 = Datum.LocalSeaY + 1.7f;
    if ((double) position2.y < (double) num1)
    {
      position2.y = num1;
      cam.transform.position = position2;
    }
    cam.transform.SetPositionAndRotation(position2, rotation2);
    float sqrMagnitude = cam.cameraVelocity.sqrMagnitude;
    float num2 = !flag ? ((double) sqrMagnitude >= 1.0 ? Mathf.Lerp(0.7f, 0.96f, sqrMagnitude / 250000f) : 0.0f) : 0.96f;
    cam.cameraVelocity *= num2;
    if (!GameManager.playerInput.GetButtonTimedPressUp("Switch View", 0.0f, PlayerSettings.clickDelay))
      return;
    cam.SwitchState((CameraBaseState) cam.controlledState);
  }

  public override void FixedUpdateState(CameraStateManager cam)
  {
  }
}
