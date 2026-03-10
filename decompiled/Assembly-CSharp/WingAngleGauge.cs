// Decompiled with JetBrains decompiler
// Type: WingAngleGauge
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using UnityEngine;
using UnityEngine.UI;

#nullable disable
public class WingAngleGauge : HUDApp
{
  [SerializeField]
  private Transform wingIcon;
  [SerializeField]
  private Transform inputPivot;
  [SerializeField]
  private Text label;
  private TiltWingController tiltWingController;
  [SerializeField]
  private Transform HUDAnchor;
  [SerializeField]
  private bool fade;
  [SerializeField]
  private Image[] fadeImages;
  private float angleMin;
  private float angleMax;
  private ControlInputs inputs;
  private float lastAngle;
  private float lastChange;
  private float opacity;
  private float[] imageAlphas;

  public override void Initialize(Aircraft aircraft)
  {
    this.inputs = aircraft.GetInputs();
    this.tiltWingController = aircraft.gameObject.GetComponent<TiltWingController>();
    this.angleMin = this.tiltWingController.GetAngleLimits().Item1;
    this.angleMax = this.tiltWingController.GetAngleLimits().Item2;
    this.opacity = 1f;
    this.imageAlphas = new float[this.fadeImages.Length];
    for (int index = 0; index < this.fadeImages.Length; ++index)
      this.imageAlphas[index] = this.fadeImages[index].color.a;
  }

  private void FadeOut()
  {
    this.opacity -= Time.deltaTime;
    if ((double) this.opacity <= 0.0)
    {
      foreach (Behaviour fadeImage in this.fadeImages)
        fadeImage.enabled = false;
      this.label.enabled = false;
    }
    else
    {
      for (int index = 0; index < this.fadeImages.Length; ++index)
        this.fadeImages[index].color = new Color(0.0f, 1f, 0.0f, this.imageAlphas[index] * this.opacity);
      this.label.color = new Color(0.0f, 1f, 0.0f, this.opacity);
    }
  }

  private void FadeIn()
  {
    if ((double) this.opacity <= 0.0)
    {
      foreach (Behaviour fadeImage in this.fadeImages)
        fadeImage.enabled = true;
      this.label.enabled = true;
    }
    this.opacity += 3f * Time.deltaTime;
    this.opacity = Mathf.Clamp01(this.opacity);
    for (int index = 0; index < this.fadeImages.Length; ++index)
      this.fadeImages[index].color = new Color(0.0f, 1f, 0.0f, this.imageAlphas[index] * this.opacity);
    this.label.color = new Color(0.0f, 1f, 0.0f, this.opacity);
  }

  public override void Refresh()
  {
    float z = Mathf.Lerp(this.angleMin, this.angleMax, this.inputs.customAxis1);
    float averageAngle = this.tiltWingController.GetAverageAngle();
    if ((double) averageAngle != (double) this.lastAngle)
      this.lastChange = Time.timeSinceLevelLoad;
    this.lastAngle = averageAngle;
    if (this.fade)
    {
      if ((double) Time.timeSinceLevelLoad - (double) this.lastChange > 5.0)
      {
        if ((double) this.opacity <= 0.0)
          return;
        this.FadeOut();
      }
      else if ((double) this.opacity < 1.0)
        this.FadeIn();
    }
    this.inputPivot.localEulerAngles = new Vector3(0.0f, 0.0f, z);
    this.wingIcon.localEulerAngles = new Vector3(0.0f, 0.0f, averageAngle);
    this.label.text = $"WING {z:0}°";
  }
}
