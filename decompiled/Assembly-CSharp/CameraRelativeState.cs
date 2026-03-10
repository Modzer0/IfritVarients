// Decompiled with JetBrains decompiler
// Type: CameraRelativeState
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.MissionEditorScripts;
using UnityEngine;

#nullable disable
public class CameraRelativeState : CameraBaseState
{
  private float desiredFOV = 50f;

  public override void EnterState(CameraStateManager cam)
  {
    cam.cameraPivot.SetParent((Transform) null);
    cam.transform.SetParent(cam.followingUnit.transform, true);
    CameraStateManager.cameraMode = CameraMode.free;
    FlightHud.EnableCanvas(false);
  }

  public override void LeaveState(CameraStateManager cam)
  {
  }

  public override void UpdateState(CameraStateManager cam)
  {
    cam.windNoiseExternal.volume = 0.0f;
    if (Input.GetKey(KeyCode.PageUp))
      this.desiredFOV -= 25f * Time.unscaledDeltaTime;
    if (Input.GetKey(KeyCode.PageDown))
      this.desiredFOV += 25f * Time.unscaledDeltaTime;
    this.desiredFOV = Mathf.Clamp(this.desiredFOV, 5f, 90f);
    cam.mainCamera.fieldOfView = Mathf.Lerp(cam.mainCamera.fieldOfView, this.desiredFOV, Time.unscaledDeltaTime);
    if (Input.GetMouseButton(1))
      CursorManager.Refresh();
    if (InputFieldChecker.InsideInputField)
      return;
    double axis1 = (double) GameManager.playerInput.GetAxis("Move Longitudinal");
    double axis2 = (double) GameManager.playerInput.GetAxis("Move Lateral");
    if (!((Object) cam.followingUnit == (Object) null) && !cam.followingUnit.disabled)
      return;
    cam.SetFollowingUnit((Unit) null);
    cam.SwitchState((CameraBaseState) cam.freeState);
  }

  public override void FixedUpdateState(CameraStateManager cam)
  {
  }
}
