// Decompiled with JetBrains decompiler
// Type: CameraEncyclopediaState
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;

#nullable disable
public class CameraEncyclopediaState : CameraBaseState
{
  private bool manualViewMode;
  private float cameraDistance = 10f;
  private float cameraSmoothingVel;
  private float cameraHeight;
  private float cameraAngle;
  private float cameraHeightSmoothed;
  private float cameraDistSmoothed;
  private float cameraHeightSmoothingVel;
  private float cameraDistSmoothingVel;
  private float distPrev;
  private bool allowMoveToDropFocus;
  private float viewDistance;
  private bool fixedTarget;
  private Transform target;
  private float targetSize;

  public override void EnterState(CameraStateManager cam)
  {
    cam.mainCamera.nearClipPlane = 1f;
    cam.mainCamera.fieldOfView = 50f;
    this.viewDistance = 20f;
    cam.cameraVelocity = Vector3.zero;
    this.cameraHeightSmoothed = 1.8f;
  }

  public override void LeaveState(CameraStateManager cam)
  {
  }

  public override void UpdateState(CameraStateManager cam)
  {
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
  }

  private void MoveCamera(CameraStateManager cam, float deltaTime)
  {
    if ((UnityEngine.Object) SceneSingleton<EncyclopediaBrowser>.i.spawnedUnitObject == (UnityEngine.Object) null)
      return;
    float num1 = GameManager.playerInput.GetAxis("Pan View") * PlayerSettings.viewSensitivity;
    float num2 = GameManager.playerInput.GetAxis("Tilt View") * PlayerSettings.viewSensitivity;
    float f1 = num1 * -0.2f;
    float f2 = num2 * -0.2f;
    if (!GameManager.playerInput.GetButton("Free Look"))
    {
      f1 = 0.0f;
      f2 = 0.0f;
    }
    Unit spawnedUnit = SceneSingleton<EncyclopediaBrowser>.i.GetSpawnedUnit();
    float num3 = spawnedUnit.maxRadius * 2.6f;
    if ((double) num3 != (double) this.distPrev)
      this.cameraHeight = num3 * 0.25f;
    this.distPrev = num3;
    this.cameraDistSmoothed = Mathf.Max(this.cameraDistSmoothed, num3 * 0.5f);
    this.cameraDistSmoothed = Mathf.SmoothDamp(this.cameraDistSmoothed, num3, ref this.cameraDistSmoothingVel, 1f);
    this.cameraAngle += f1 * 100f * Time.deltaTime;
    this.cameraHeight += f2 * -3f * Time.deltaTime * this.cameraDistSmoothed;
    this.cameraHeight = Mathf.Clamp(this.cameraHeight, 1.7f, num3);
    this.cameraHeightSmoothed = Mathf.Max(this.cameraHeightSmoothed, 1.7f);
    this.cameraHeightSmoothed = Mathf.SmoothDamp(this.cameraHeightSmoothed, this.cameraHeight, ref this.cameraHeightSmoothingVel, 0.25f);
    if ((double) Mathf.Abs(f1) > 0.75 || (double) Mathf.Abs(f2) > 0.75)
      this.manualViewMode = true;
    if (!this.manualViewMode)
      this.cameraAngle += 15f * Time.deltaTime;
    Vector3 vector3 = new Vector3(Mathf.Cos(this.cameraAngle * ((float) Math.PI / 180f)), 0.0f, Mathf.Sin(this.cameraAngle * ((float) Math.PI / 180f)));
    cam.transform.position = SceneSingleton<EncyclopediaBrowser>.i.spawnedUnitObject.transform.position + vector3.normalized * this.cameraDistSmoothed + Vector3.up * this.cameraHeightSmoothed;
    cam.transform.LookAt(SceneSingleton<EncyclopediaBrowser>.i.spawnedUnitObject.transform.position - spawnedUnit.definition.spawnOffset * 0.5f, Vector3.up);
  }

  public override void FixedUpdateState(CameraStateManager cam)
  {
  }
}
