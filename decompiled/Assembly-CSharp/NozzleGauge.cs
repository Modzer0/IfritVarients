// Decompiled with JetBrains decompiler
// Type: NozzleGauge
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using UnityEngine;
using UnityEngine.UI;

#nullable disable
public class NozzleGauge : HUDApp
{
  [SerializeField]
  private Transform nozzleIcon;
  [SerializeField]
  private Transform inputPivot;
  [SerializeField]
  private Text label;
  [SerializeField]
  private float inputRange = -120f;
  [SerializeField]
  private bool fade;
  [SerializeField]
  private Image[] fadeImages;
  private INozzleGauge system;
  [SerializeField]
  private string sourceName;
  [SerializeField]
  private Transform HUDAnchor;
  private ControlInputs inputs;
  private float lastAngle;
  private float lastChange;
  private float opacity;
  private float[] imageAlphas;

  public override void Initialize(Aircraft aircraft)
  {
    this.inputs = aircraft.GetInputs();
    foreach (UnitPart unitPart in aircraft.partLookup)
    {
      if ((Object) unitPart != (Object) null && unitPart.gameObject.name == this.sourceName)
      {
        this.system = unitPart.gameObject.GetComponent<INozzleGauge>();
        break;
      }
    }
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
    float nozzleAngle = this.system.GetNozzleAngle();
    if ((double) nozzleAngle != (double) this.lastAngle)
      this.lastChange = Time.timeSinceLevelLoad;
    this.lastAngle = nozzleAngle;
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
    this.inputPivot.localEulerAngles = new Vector3(0.0f, 0.0f, Mathf.Clamp01(1f - this.inputs.customAxis1) * this.inputRange);
    this.nozzleIcon.localEulerAngles = new Vector3(0.0f, 0.0f, -nozzleAngle);
    this.label.text = $"NOZZLE {nozzleAngle:0}°";
  }
}
