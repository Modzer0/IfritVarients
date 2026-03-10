// Decompiled with JetBrains decompiler
// Type: ChargeIndicator
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;
using UnityEngine.UI;

#nullable disable
public class ChargeIndicator : MonoBehaviour
{
  [SerializeField]
  private Image chargeBar;
  [SerializeField]
  private Gradient barGradient;
  [SerializeField]
  private Text readout;
  private PowerSupply powerSupply;
  private float barLength;
  private float barHeight;

  private void Awake()
  {
    this.barLength = this.chargeBar.rectTransform.sizeDelta.x;
    this.barHeight = this.chargeBar.rectTransform.sizeDelta.y;
    CombatHUD.onSetAircraft += new Action<CombatHUD>(this.ChargeIndicator_OnSetAircraft);
  }

  private void Start()
  {
    if (!((UnityEngine.Object) SceneSingleton<CombatHUD>.i.aircraft != (UnityEngine.Object) null))
      return;
    this.UpdateDisplay();
  }

  private void ChargeIndicator_OnSetAircraft(CombatHUD sender)
  {
    if ((UnityEngine.Object) this.powerSupply != (UnityEngine.Object) null)
      this.powerSupply.onChargeChanged -= new Action<PowerSupply>(this.ChargeIndicator_OnChargeChanged);
    this.powerSupply = sender.aircraft.GetPowerSupply();
    if ((UnityEngine.Object) this.powerSupply != (UnityEngine.Object) null)
      this.powerSupply.onChargeChanged += new Action<PowerSupply>(this.ChargeIndicator_OnChargeChanged);
    this.SetVisibility(false);
  }

  private void SetVisibility(bool visible)
  {
    if (visible)
    {
      this.enabled = true;
      this.gameObject.SetActive(true);
      this.UpdateDisplay();
    }
    else
    {
      this.enabled = false;
      this.gameObject.SetActive(false);
    }
  }

  private void OnDestroy()
  {
    CombatHUD.onSetAircraft -= new Action<CombatHUD>(this.ChargeIndicator_OnSetAircraft);
  }

  private void ChargeIndicator_OnChargeChanged(object sender)
  {
    if (this.powerSupply.Users > 0 && !this.enabled)
      this.SetVisibility(true);
    this.UpdateDisplay();
  }

  private void UpdateDisplay()
  {
    float powerSupplied = this.powerSupply.GetPowerSupplied();
    float charge = this.powerSupply.GetCharge();
    Color color1 = this.barGradient.Evaluate(this.powerSupply.GetPowerAvailable());
    Color color2 = color1;
    color1.a = Mathf.Lerp(0.2f, color1.a, Mathf.Sqrt(powerSupplied));
    this.chargeBar.rectTransform.sizeDelta = new Vector2(charge * this.barLength, this.barHeight);
    this.chargeBar.color = color1;
    this.readout.text = $"CAPACITOR {this.powerSupply.GetChargeKJ():0}  kJ";
    this.readout.color = color2;
  }
}
