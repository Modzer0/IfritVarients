// Decompiled with JetBrains decompiler
// Type: NuclearOption.MissionEditorScripts.EditorCameraNavigator
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

#nullable disable
namespace NuclearOption.MissionEditorScripts;

public class EditorCameraNavigator : SceneSingleton<EditorCameraNavigator>
{
  [SerializeField]
  private float cameraAcceleration;
  [SerializeField]
  private float cameraDamping;
  [SerializeField]
  private float shiftMultiplier;
  private Transform camTransform;
  private RaycastHit hit;
  private int layerMask = 64 /*0x40*/;
  private Vector3 cameraSpeed = Vector3.zero;

  private void Start() => this.camTransform = SceneSingleton<CameraStateManager>.i.transform;

  private void Update()
  {
    Vector3 vector3 = new Vector3(this.transform.eulerAngles.x, this.transform.eulerAngles.y, 0.0f);
    if (Input.GetMouseButton(1))
    {
      vector3.x += GameManager.playerInput.GetAxis("Tilt View") * 0.1f;
      vector3.y += GameManager.playerInput.GetAxis("Pan View") * 0.1f;
    }
    float num1 = 1f;
    if (Input.GetKey("left shift"))
      num1 = this.shiftMultiplier;
    this.cameraSpeed += this.cameraAcceleration * num1 * this.transform.forward * -GameManager.playerInput.GetAxis("Pitch") * Time.unscaledDeltaTime;
    this.cameraSpeed += this.cameraAcceleration * num1 * this.transform.right * GameManager.playerInput.GetAxis("Roll") * Time.unscaledDeltaTime;
    this.transform.localEulerAngles = vector3;
    this.transform.position += this.cameraSpeed * 0.02f;
    if (Physics.Linecast(this.transform.position + Vector3.up * 5000f, this.transform.position - Vector3.up * 5000f, out this.hit, this.layerMask))
      this.transform.position = new Vector3(this.transform.position.x, Mathf.Max(this.transform.position.y, this.hit.point.y + 1.7f), this.transform.position.z);
    Vector3 position = this.transform.position;
    float num2 = Datum.LocalSeaY + 1.7f;
    if ((double) position.y < (double) num2)
    {
      position.y = num2;
      this.transform.position = position;
    }
    this.cameraSpeed -= this.cameraSpeed * this.cameraDamping * 0.01f;
    this.camTransform.position = this.transform.position;
    this.camTransform.rotation = this.transform.rotation;
  }
}
