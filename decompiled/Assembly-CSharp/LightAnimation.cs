// Decompiled with JetBrains decompiler
// Type: LightAnimation
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

#nullable disable
public class LightAnimation : MonoBehaviour
{
  [SerializeField]
  private float lightIntensity;
  [SerializeField]
  private Light animatedLight;
  [SerializeField]
  private AnimationCurve intensityCurve;
  [SerializeField]
  private Gradient colorAnimation;
  [SerializeField]
  private Transform lightAnchor;
  [SerializeField]
  private float verticalOffset;
  private float timeSinceSpawn;

  private void OnEnable()
  {
    if ((double) this.lightIntensity <= 10000.0)
      return;
    ExposureController.RegisterBrightLight(this.animatedLight);
  }

  private void Update()
  {
    this.animatedLight.intensity = this.lightIntensity * this.intensityCurve.Evaluate(this.timeSinceSpawn);
    this.animatedLight.color = this.colorAnimation.Evaluate(this.timeSinceSpawn);
    if ((Object) this.lightAnchor != (Object) null)
      this.animatedLight.gameObject.transform.position = this.lightAnchor.position + Vector3.up * this.verticalOffset;
    this.timeSinceSpawn += Time.deltaTime;
    if ((double) this.animatedLight.intensity > 0.0)
      return;
    Object.Destroy((Object) this.animatedLight.gameObject);
    Object.Destroy((Object) this);
  }
}
