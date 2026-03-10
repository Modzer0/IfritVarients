// Decompiled with JetBrains decompiler
// Type: CameraControlledState
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

#nullable disable
public class CameraControlledState : CameraBaseState
{
  private Vector3 cameraPosition;
  private Vector3 cameraAngle;
  private Vector3 pivotPosition;
  private Vector3 pivotAngle;
  private Vector3 pivotRotation;
  private Vector3 cameraTranslation;
  private Vector3 translationVector;
  private Vector3 rotationVector;
  private Vector3 moveVector;
  private float FOVAdjustment;
  private float minFOV = 1f;
  private float maxFOV = 120f;
  private Quaternion pivotRotationPrev;
  private Transform lookAt;
  private CameraControlledState.VectorUp vectorUp;
  private RaycastHit hit;
  private int layerMask = 64 /*0x40*/;

  public override void EnterState(CameraStateManager cam)
  {
    if ((Object) cam.followingUnit != (Object) null)
    {
      cam.cameraPivot.SetParent(cam.followingUnit.transform);
      cam.cameraPivot.localPosition = Vector3.zero;
      cam.cameraPivot.localEulerAngles = Vector3.zero;
      cam.transform.SetParent(cam.cameraPivot, true);
      this.vectorUp = CameraControlledState.VectorUp.Unit;
    }
    else
    {
      cam.cameraPivot.position = cam.transform.position;
      cam.cameraPivot.eulerAngles = Vector3.zero;
      cam.cameraPivot.SetParent(Datum.origin, true);
      cam.transform.SetParent(cam.cameraPivot);
      cam.transform.localPosition = Vector3.zero;
      this.vectorUp = CameraControlledState.VectorUp.World;
    }
    CameraStateManager.cameraMode = CameraMode.free;
    FlightHud.EnableCanvas(false);
    this.FOVAdjustment = 0.0f;
    this.pivotPosition = cam.cameraPivot.localPosition;
    this.pivotAngle = cam.cameraPivot.localEulerAngles;
    this.cameraPosition = cam.transform.localPosition;
    this.cameraAngle = cam.transform.localEulerAngles;
    this.moveVector = Vector3.zero;
    this.cameraTranslation = Vector3.zero;
    this.pivotRotation = Vector3.zero;
    this.translationVector = Vector3.zero;
    this.rotationVector = Vector3.zero;
    this.lookAt = (Transform) null;
    if ((Object) cam.followingUnit != (Object) null)
      this.pivotRotationPrev = cam.cameraPivot.localRotation;
    else
      this.pivotRotationPrev = cam.transform.localRotation;
  }

  public override void LeaveState(CameraStateManager cam)
  {
  }

  public override void UpdateState(CameraStateManager cam)
  {
    if (GameManager.playerInput.GetButton("Free Look"))
    {
      float f1 = (float) ((double) Mathf.Min(cam.mainCamera.fieldOfView / 20f, 1f) * (double) GameManager.playerInput.GetAxis("Pan View") * 0.30000001192092896 * (double) PlayerSettings.viewSensitivity * (PlayerSettings.viewInvertPitch ? -1.0 : 1.0));
      float f2 = (float) ((double) Mathf.Min(cam.mainCamera.fieldOfView / 20f, 1f) * (double) GameManager.playerInput.GetAxis("Tilt View") * 0.30000001192092896) * PlayerSettings.viewSensitivity;
      if ((double) Mathf.Abs(f1) > 0.0 || (double) Mathf.Abs(f2) > 0.0)
        this.cameraAngle += cam.desiredRotSpeed * (f1 * Vector3.up + f2 * Vector3.right);
    }
    this.moveVector = Vector3.zero;
    float axis1 = GameManager.playerInput.GetAxis("Move Longitudinal");
    float axis2 = GameManager.playerInput.GetAxis("Move Lateral");
    float axis3 = GameManager.playerInput.GetAxis("Move Vertical");
    if (cam.allowInputs && ((double) axis1 != 0.0 || (double) axis2 != 0.0 || (double) axis3 != 0.0))
    {
      float num = 50f;
      if (Input.GetKey(KeyCode.LeftShift))
        num = 500f;
      this.moveVector = cam.desiredTransSpeed * num * Time.unscaledDeltaTime * (cam.transform.forward * axis1 + cam.transform.right * axis2 + Vector3.up * axis3);
    }
    if ((Object) cam.followingUnit != (Object) null)
      this.UpdateFollowing(cam);
    else
      this.UpdateFree(cam);
    this.FOVAdjustment -= cam.fovChangeSpeed * Mathf.Min(cam.mainCamera.fieldOfView / 20f, 1f) * GameManager.playerInput.GetAxis("FOV");
    this.FOVAdjustment = Mathf.Clamp(this.FOVAdjustment, this.minFOV - cam.desiredFOV, this.maxFOV - cam.desiredFOV);
    float b = Mathf.Clamp(cam.desiredFOV + this.FOVAdjustment, this.minFOV, this.maxFOV);
    cam.mainCamera.fieldOfView = Mathf.Lerp(cam.mainCamera.fieldOfView, b, (float) (1.0 / (1.0 + 100.0 * (double) cam.fovChangeInertia)));
    if (!Physics.Linecast(cam.transform.position + Vector3.up * 5000f, cam.transform.position - Vector3.up * 5000f, out this.hit, 64 /*0x40*/))
      return;
    cam.transform.position = new Vector3(cam.transform.position.x, Mathf.Max(cam.transform.position.y, this.hit.point.y + 1.7f), cam.transform.position.z);
  }

  private void UpdateFree(CameraStateManager cam)
  {
    Vector3 vector3_1 = this.pivotPosition.x * Vector3.right + this.pivotPosition.y * Vector3.up + this.pivotPosition.z * Vector3.forward;
    if ((double) this.translationVector.magnitude > 0.0)
      this.cameraTranslation += (this.translationVector.x * cam.cameraPivot.right + this.translationVector.y * cam.cameraPivot.up + this.translationVector.z * cam.cameraPivot.forward) * Time.unscaledDeltaTime;
    cam.cameraPivot.localPosition = Vector3.Lerp(cam.cameraPivot.localPosition, vector3_1 + this.cameraTranslation, Mathf.Min(3f * Time.unscaledDeltaTime / PlayerSettings.viewSmoothing, 1f));
    cam.cameraPivot.localRotation = Quaternion.Lerp(cam.cameraPivot.localRotation, Quaternion.Euler(this.pivotAngle), Mathf.Min(5f * Time.unscaledDeltaTime / PlayerSettings.viewSmoothing, 1f));
    if ((double) this.moveVector.magnitude > 0.0)
      this.pivotPosition += this.moveVector;
    Vector3 vector3_2 = this.cameraPosition.x * cam.cameraPivot.right + this.cameraPosition.y * cam.cameraPivot.up + this.cameraPosition.z * cam.cameraPivot.forward;
    cam.transform.position = Vector3.Lerp(cam.transform.position, cam.cameraPivot.position + vector3_2, Mathf.Min(3f * Time.unscaledDeltaTime / PlayerSettings.viewSmoothing, 1f));
    if ((Object) this.lookAt != (Object) null)
    {
      cam.transform.LookAt(this.lookAt);
    }
    else
    {
      if ((double) this.rotationVector.magnitude > 0.0)
        this.pivotRotation += this.rotationVector * Time.unscaledDeltaTime;
      cam.transform.localRotation = Quaternion.Lerp(this.pivotRotationPrev, Quaternion.Euler(this.cameraAngle + this.pivotRotation), Mathf.Min(5f * Time.unscaledDeltaTime / PlayerSettings.viewSmoothing, 1f));
    }
    this.pivotRotationPrev = cam.transform.localRotation;
    if (!GameManager.playerInput.GetButtonTimedPressUp("Switch View", 0.0f, PlayerSettings.clickDelay))
      return;
    cam.SwitchState((CameraBaseState) cam.freeState);
  }

  private void UpdateFollowing(CameraStateManager cam)
  {
    Vector3 vector3_1 = this.pivotPosition.x * cam.followingUnit.transform.right + this.pivotPosition.y * cam.followingUnit.transform.up + this.pivotPosition.z * cam.followingUnit.transform.forward;
    cam.cameraPivot.position = Vector3.Lerp(cam.cameraPivot.position, cam.followingUnit.transform.position + vector3_1, Mathf.Min(3f * Time.unscaledDeltaTime / PlayerSettings.viewSmoothing, 1f));
    if ((double) this.rotationVector.magnitude > 0.0)
      this.pivotRotation += this.rotationVector * Time.unscaledDeltaTime;
    if (this.vectorUp == CameraControlledState.VectorUp.World)
    {
      cam.cameraPivot.rotation = Quaternion.Lerp(this.pivotRotationPrev, Quaternion.Euler(cam.followingUnit.transform.eulerAngles + this.pivotAngle + this.pivotRotation), Mathf.Min(5f * Time.unscaledDeltaTime / PlayerSettings.viewSmoothing, 1f));
      this.pivotRotationPrev = cam.cameraPivot.rotation;
      cam.cameraPivot.rotation = Quaternion.LookRotation(cam.cameraPivot.forward, Vector3.up);
    }
    else
    {
      cam.cameraPivot.localRotation = Quaternion.Lerp(this.pivotRotationPrev, Quaternion.Euler(this.pivotAngle + this.pivotRotation), Mathf.Min(5f * Time.unscaledDeltaTime / PlayerSettings.viewSmoothing, 1f));
      this.pivotRotationPrev = cam.cameraPivot.localRotation;
    }
    if ((double) this.translationVector.magnitude > 0.0)
      this.cameraTranslation += this.translationVector * Time.unscaledDeltaTime;
    Vector3 vector3_2 = this.cameraPosition.x * cam.cameraPivot.right + this.cameraPosition.y * cam.cameraPivot.up + this.cameraPosition.z * cam.cameraPivot.forward;
    cam.transform.localPosition = Vector3.Lerp(cam.transform.localPosition, this.cameraPosition + this.cameraTranslation, Mathf.Min(3f * Time.unscaledDeltaTime / PlayerSettings.viewSmoothing, 1f));
    if ((double) this.moveVector.magnitude > 0.0)
      this.cameraPosition += new Vector3(Vector3.Dot(this.moveVector, cam.cameraPivot.right), Vector3.Dot(this.moveVector, cam.cameraPivot.up), Vector3.Dot(this.moveVector, cam.cameraPivot.forward));
    if ((Object) this.lookAt != (Object) null)
      cam.transform.LookAt(this.lookAt);
    else
      cam.transform.localRotation = Quaternion.Lerp(cam.transform.localRotation, Quaternion.Euler(this.cameraAngle), Mathf.Min(5f * Time.unscaledDeltaTime / PlayerSettings.viewSmoothing, 1f));
    if (!GameManager.playerInput.GetButtonTimedPressUp("Switch View", 0.0f, PlayerSettings.clickDelay))
      return;
    cam.SwitchState((CameraBaseState) cam.orbitState);
  }

  public override void FixedUpdateState(CameraStateManager cam)
  {
  }

  public void SetCustomTransform(Vector3 position, Vector3 eulerAngles)
  {
    this.cameraPosition = position;
    this.cameraAngle = eulerAngles;
  }

  public void SetCustomPivot(Vector3 position, Vector3 eulerAngles)
  {
    this.pivotPosition = !((Object) SceneSingleton<CameraStateManager>.i.followingUnit != (Object) null) ? position.ToGlobalPosition().AsVector3() : position;
    this.pivotAngle = eulerAngles;
  }

  public void SwitchPivotUp()
  {
    if ((Object) SceneSingleton<CameraStateManager>.i.followingUnit == (Object) null)
      return;
    if (this.vectorUp == CameraControlledState.VectorUp.World)
      this.vectorUp = CameraControlledState.VectorUp.Unit;
    else
      this.vectorUp = CameraControlledState.VectorUp.World;
  }

  public string GetPivotUp() => this.vectorUp != CameraControlledState.VectorUp.World ? "U" : "W";

  public void SetCustomMovement(Vector3 translation, Vector3 rotation, bool cancel)
  {
    this.translationVector = translation;
    this.rotationVector = rotation;
    if (!cancel)
      return;
    this.cameraTranslation = Vector3.zero;
    this.pivotRotation = Vector3.zero;
  }

  public void SetLookAt(Transform target) => this.lookAt = target;

  public bool IsLookingAt() => (Object) this.lookAt != (Object) null;

  private enum VectorUp
  {
    World,
    Unit,
  }
}
