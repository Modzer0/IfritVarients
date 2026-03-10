// Decompiled with JetBrains decompiler
// Type: FuelGauge
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using UnityEngine;
using UnityEngine.UI;

#nullable disable
public class FuelGauge : HUDApp
{
  private Aircraft aircraft;
  [SerializeField]
  private Transform fuelReadingPivot;
  [SerializeField]
  private Text fuelReading;
  [SerializeField]
  private Text fuelLabel;
  [SerializeField]
  private Image fuelBar;
  [SerializeField]
  private Image fuelPointer;
  [SerializeField]
  private Image fuelArc;
  [SerializeField]
  private Gradient fuelGradient;
  private float lastReading;

  public override void Initialize(Aircraft aircraft)
  {
    this.aircraft = aircraft;
    this.Show(PlayerSettings.gauges);
    this.Refresh();
  }

  public override void RefreshSettings()
  {
    base.RefreshSettings();
    this.fuelReading.fontSize = this.fontSize;
    this.fuelLabel.fontSize = this.fontSize - 4;
    this.Show(PlayerSettings.gauges);
  }

  public override void Refresh()
  {
    if ((Object) this.aircraft == (Object) null || (double) Time.timeSinceLevelLoad - (double) this.lastReading < 1.0)
      return;
    this.lastReading = Time.timeSinceLevelLoad;
    float fuelLevel = this.aircraft.GetFuelLevel();
    this.fuelReadingPivot.localEulerAngles = new Vector3(0.0f, 0.0f, (float) -((double) fuelLevel * 28.0 - 14.0));
    this.fuelReading.transform.eulerAngles = new Vector3(0.0f, 0.0f, (float) -((double) SceneSingleton<CameraStateManager>.i.mainCamera.transform.eulerAngles.z - (double) this.aircraft.cockpit.transform.eulerAngles.z));
    this.fuelReading.text = (fuelLevel * 100f).ToString("F0") + "%";
    this.fuelBar.fillAmount = fuelLevel;
    this.fuelBar.color = this.fuelGradient.Evaluate(fuelLevel);
    this.fuelPointer.color = this.fuelBar.color;
    this.fuelLabel.color = this.fuelBar.color;
    this.fuelReading.color = this.fuelBar.color;
  }

  public void Show(bool arg)
  {
    this.fuelArc.enabled = arg;
    this.fuelPointer.enabled = arg;
    this.fuelBar.enabled = arg;
    this.fuelReading.enabled = arg;
    this.fuelLabel.enabled = arg;
  }
}
