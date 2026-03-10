// Decompiled with JetBrains decompiler
// Type: ThrottleGauge
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;
using UnityEngine.UI;

#nullable disable
public class ThrottleGauge : HUDApp
{
  [SerializeField]
  private Image throttleBar;
  [SerializeField]
  private Image throttleArc;
  [SerializeField]
  private Image throttlePointer;
  [SerializeField]
  private Text throttleReading;
  [SerializeField]
  private Text throttleLabel;
  [SerializeField]
  private Gradient throttleGradient;
  [SerializeField]
  private Color afterburnerColor;
  [SerializeField]
  private Transform throttleReadingPivot;
  [SerializeField]
  private Transform throttleBoundaryPivot;
  private ControlInputs inputs;
  [SerializeField]
  private bool airbrake;
  [SerializeField]
  private bool afterburner;
  [SerializeField]
  private ThrottleGauge.ThrottleRegion[] throttleRegions;
  private ThrottleGauge.ThrottleRegion currentRegion;
  private Aircraft aircraft;
  private float throttlePrev = -1f;

  public override void Initialize(Aircraft aircraft)
  {
    this.inputs = aircraft.GetInputs();
    this.aircraft = aircraft;
    this.throttlePrev = -1f;
    if ((UnityEngine.Object) this.throttleBoundaryPivot != (UnityEngine.Object) null && this.throttleRegions.Length != 0 && this.afterburner)
    {
      Transform throttleBoundaryPivot = this.throttleBoundaryPivot;
      ThrottleGauge.ThrottleRegion[] throttleRegions = this.throttleRegions;
      Vector3 vector3 = new Vector3(0.0f, 0.0f, (float) (((double) throttleRegions[throttleRegions.Length - 1].GetStart() + 0.0099999997764825821) * 26.0 - 13.0));
      throttleBoundaryPivot.localEulerAngles = vector3;
    }
    this.Show(PlayerSettings.gauges);
  }

  public override void RefreshSettings()
  {
    base.RefreshSettings();
    this.throttleReading.fontSize = this.fontSize;
    this.throttleLabel.fontSize = this.fontSize - 4;
    this.Show(PlayerSettings.gauges);
  }

  public override void Refresh()
  {
    if ((UnityEngine.Object) this.aircraft == (UnityEngine.Object) null || (double) this.throttlePrev == (double) this.inputs.throttle)
      return;
    this.throttlePrev = this.inputs.throttle;
    this.throttleReadingPivot.localEulerAngles = new Vector3(0.0f, 0.0f, (float) ((double) this.inputs.throttle * 26.0 - 13.0));
    this.throttleReading.transform.eulerAngles = new Vector3(0.0f, 0.0f, (float) -((double) SceneSingleton<CameraStateManager>.i.mainCamera.transform.eulerAngles.z - (double) this.aircraft.cockpit.transform.eulerAngles.z));
    this.throttleBar.fillAmount = this.inputs.throttle;
    if (this.throttleRegions.Length != 0)
    {
      foreach (ThrottleGauge.ThrottleRegion throttleRegion in this.throttleRegions)
      {
        if (throttleRegion.IsActive(this.inputs.throttle))
          this.currentRegion = throttleRegion;
      }
    }
    if (this.currentRegion != null)
    {
      double percent = (double) this.currentRegion.GetPercent(this.inputs.throttle);
      Color color = this.currentRegion.GetColor();
      this.throttleBar.color = color;
      this.throttleReading.color = color;
      this.throttleReading.text = this.currentRegion.GetText();
    }
    else
    {
      this.throttleReading.text = $"{(ValueType) (float) ((double) this.inputs.throttle * 100.0):F0}%";
      this.throttleBar.color = this.throttleGradient.Evaluate(this.inputs.throttle);
      this.throttleReading.color = this.throttleBar.color;
      if (this.afterburner && (double) this.inputs.throttle == 1.0)
      {
        this.throttleReading.text = "AFTERBURNER";
        this.throttleReading.color = this.afterburnerColor;
        this.throttleBar.color = this.afterburnerColor;
      }
      if (!this.airbrake || (double) this.inputs.throttle != 0.0)
        return;
      this.throttleReading.text = "AIRBRAKE";
    }
  }

  public void Show(bool arg)
  {
    this.throttleArc.enabled = arg;
    this.throttleBar.enabled = arg;
    this.throttlePointer.enabled = arg;
    this.throttleLabel.enabled = arg;
    this.throttleReading.enabled = arg;
  }

  [Serializable]
  private class ThrottleRegion
  {
    [SerializeField]
    private string name;
    [SerializeField]
    private bool showName;
    [SerializeField]
    private bool showPercent;
    [SerializeField]
    private float start;
    [SerializeField]
    private float end;
    [SerializeField]
    private Gradient gradient;
    private float percent;

    public bool IsActive(float input)
    {
      return (double) input >= (double) this.start && (double) input <= (double) this.end;
    }

    public float GetPercent(float input)
    {
      this.percent = (double) this.end - (double) this.start > 0.0 ? (float) (((double) input - (double) this.start) / ((double) this.end - (double) this.start)) : 1f;
      return this.percent;
    }

    public Color GetColor() => this.gradient.Evaluate(this.percent);

    public string GetText()
    {
      string text = string.Empty;
      if (this.showName)
        text = this.name;
      if (this.showPercent)
        text += $"{(ValueType) (float) ((double) this.percent * 100.0):0}%";
      return text;
    }

    public float GetStart() => this.start;

    public float GetEnd() => this.end;
  }
}
