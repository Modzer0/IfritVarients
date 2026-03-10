// Decompiled with JetBrains decompiler
// Type: NuclearOption.DebugScripts.DebugText
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using TMPro;
using UnityEngine;

#nullable disable
namespace NuclearOption.DebugScripts;

[DefaultExecutionOrder(100)]
public class DebugText : MonoBehaviour
{
  public TextMeshProUGUI Text;
  private Camera camera;

  private void OnEnable() => this.camera = Camera.main;

  private void LateUpdate()
  {
    if (!((Object) this.camera != (Object) null))
      return;
    this.transform.rotation = Quaternion.LookRotation(this.transform.position - this.camera.transform.position, Vector3.up);
  }
}
