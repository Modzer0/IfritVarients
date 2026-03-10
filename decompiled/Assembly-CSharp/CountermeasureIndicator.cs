// Decompiled with JetBrains decompiler
// Type: CountermeasureIndicator
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;
using UnityEngine.UI;

#nullable disable
public class CountermeasureIndicator : HUDApp
{
  [SerializeField]
  private Image counterImage;
  [SerializeField]
  private Text counterName;
  [SerializeField]
  private Text counterAmmo;
  private Aircraft aircraft;

  public override void Initialize(Aircraft aircraft)
  {
    this.aircraft = aircraft;
    if (!((UnityEngine.Object) aircraft.countermeasureManager.GetActiveCountermeasure() == (UnityEngine.Object) null))
      return;
    this.gameObject.SetActive(false);
  }

  public override void RefreshSettings()
  {
    base.RefreshSettings();
    this.counterName.fontSize = this.fontSize;
    this.counterAmmo.fontSize = this.fontSize;
    this.Show(PlayerSettings.hudWeapons);
    this.Refresh();
  }

  public override void Refresh()
  {
    if ((UnityEngine.Object) this.aircraft == (UnityEngine.Object) null || !PlayerSettings.hudWeapons)
      return;
    if ((UnityEngine.Object) this.aircraft.countermeasureManager.GetActiveCountermeasure() != (UnityEngine.Object) null)
    {
      this.counterImage.color = Color.green;
      this.counterName.color = Color.green;
      this.counterAmmo.color = Color.green;
      Countermeasure activeCountermeasure = this.aircraft.countermeasureManager.GetActiveCountermeasure();
      if (activeCountermeasure is FlareEjector flareEjector)
      {
        if (this.counterName.text != "FLARE")
        {
          if (!this.counterImage.enabled)
            this.counterImage.enabled = true;
          this.counterImage.sprite = flareEjector.displayImage;
          this.counterName.text = "FLARE";
        }
        int ammo = flareEjector.GetAmmo();
        this.counterAmmo.text = $"{ammo}";
        if ((double) ammo != 0.0)
          return;
        this.counterImage.color = Color.grey;
        this.counterName.color = Color.grey;
        this.counterAmmo.color = Color.grey;
      }
      else
      {
        if (this.counterName.text != "JAMMER")
        {
          this.counterImage.sprite = activeCountermeasure.displayImage;
          this.counterName.text = "JAMMER";
        }
        float charge = this.aircraft.GetPowerSupply().GetCharge();
        this.counterAmmo.text = $"{(ValueType) (float) (100.0 * (double) charge):F0}%";
        if ((double) charge >= 0.0099999997764825821)
          return;
        this.counterAmmo.color = Color.red;
      }
    }
    else
    {
      this.counterImage.enabled = false;
      this.counterName.text = "";
      this.counterAmmo.text = "";
    }
  }

  public void Show(bool arg)
  {
    this.counterImage.enabled = arg;
    this.counterName.enabled = arg;
    this.counterAmmo.enabled = arg;
  }
}
