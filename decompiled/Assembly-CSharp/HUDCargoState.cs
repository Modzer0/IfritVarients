// Decompiled with JetBrains decompiler
// Type: HUDCargoState
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#nullable disable
public class HUDCargoState : HUDWeaponState
{
  [SerializeField]
  private GameObject capturePanel;
  [SerializeField]
  private RectTransform captureBar;
  [SerializeField]
  private Image capturePanelImage;
  [SerializeField]
  private Image captureBarImage;
  [SerializeField]
  private TMP_Text currentAirbaseTitle;
  [SerializeField]
  private TMP_Text captureText;
  [SerializeField]
  private CanvasGroup captureCanvasGroup;
  private float fadeTarget;
  private float fadeSmoothed;
  private Image targetDesignator;
  private WeaponStation weaponStation;
  private float lastAirbaseCheck;
  private float progressSmoothed;
  private float progressSmoothingVel;
  private Airbase currentAirbase;
  private Vector2 captureBarSize;

  public override void SetHUDWeaponState(
    Image targetDesignator,
    Aircraft aircraft,
    WeaponStation weaponStation)
  {
    this.weaponStation = weaponStation;
    this.weaponInfo = weaponStation.WeaponInfo;
    this.targetDesignator = targetDesignator;
    targetDesignator.transform.localScale = Vector3.one;
    SceneSingleton<FlightHud>.i.waterline.enabled = true;
    SceneSingleton<FlightHud>.i.velocityVector.transform.localScale = Vector3.one;
    this.captureBarSize = this.captureBar.sizeDelta;
    this.capturePanel.SetActive(false);
    this.fadeTarget = 0.0f;
    this.progressSmoothed = 0.5f;
  }

  public void RefreshNearestAirbase(Aircraft aircraft)
  {
    if ((double) Time.timeSinceLevelLoad - (double) this.lastAirbaseCheck < 1.0)
      return;
    this.lastAirbaseCheck = Time.timeSinceLevelLoad;
    this.currentAirbase = (Airbase) null;
    if ((double) aircraft.speed < 20.0)
    {
      float b1 = float.MaxValue;
      GlobalPosition b2 = aircraft.GlobalPosition();
      foreach (KeyValuePair<string, Airbase> keyValuePair in FactionRegistry.airbaseLookup)
      {
        Airbase airbase = keyValuePair.Value;
        if (FastMath.InRange(airbase.center.GlobalPosition(), b2, Mathf.Min(airbase.GetRadius(), b1)))
        {
          b1 = FastMath.Distance(airbase.center.GlobalPosition(), b2);
          this.currentAirbase = airbase;
        }
      }
    }
    this.fadeTarget = (UnityEngine.Object) this.currentAirbase != (UnityEngine.Object) null ? 1f : 0.0f;
    if ((double) this.fadeTarget == 0.0)
      return;
    string factionTag = aircraft.NetworkHQ.faction.factionTag;
    string str1 = (UnityEngine.Object) this.currentAirbase.CurrentHQ != (UnityEngine.Object) null ? " Defending" : " Capturing";
    if ((double) this.currentAirbase.capture.controlBalance == 1.0)
      str1 = "";
    if ((UnityEngine.Object) this.currentAirbase.CurrentHQ != (UnityEngine.Object) null)
      factionTag = this.currentAirbase.CurrentHQ.faction.factionTag;
    this.currentAirbaseTitle.text = this.currentAirbase.SavedAirbase.DisplayName;
    string str2 = "";
    if ((double) this.currentAirbase.capture.controlBalance < 1.0)
      str2 = $" {(ValueType) (float) ((double) this.currentAirbase.capture.controlBalance * 100.0):F0}%";
    this.captureText.text = $"({factionTag}{str1}{str2})";
    if ((UnityEngine.Object) this.currentAirbase.CurrentHQ == (UnityEngine.Object) null)
      this.captureBarImage.color = Color.green;
    else
      this.captureBarImage.color = (UnityEngine.Object) this.currentAirbase.CurrentHQ == (UnityEngine.Object) aircraft.NetworkHQ ? Color.green : Color.red + Color.green * 0.25f;
    this.capturePanelImage.color = new Color(this.captureBarImage.color.r * 0.1f, this.captureBarImage.color.g * 0.1f, this.captureBarImage.color.b * 0.1f, 0.7f);
    this.currentAirbaseTitle.color = this.captureBarImage.color;
    this.captureText.color = this.captureBarImage.color;
  }

  public override void HUDFixedUpdate(Aircraft aircraft, List<Unit> targetList)
  {
    this.RefreshNearestAirbase(aircraft);
    if ((double) this.fadeTarget == 0.0 && (double) this.fadeSmoothed <= 0.0)
    {
      if (!this.capturePanel.activeSelf)
        return;
      this.capturePanel.SetActive(false);
    }
    else
    {
      if (!this.capturePanel.activeSelf)
        this.capturePanel.SetActive(true);
      this.fadeSmoothed += Mathf.Clamp(this.fadeTarget - this.fadeSmoothed, -2f * Time.fixedDeltaTime, 2f * Time.fixedDeltaTime);
      this.captureCanvasGroup.alpha = this.fadeSmoothed;
      if ((double) this.fadeTarget <= 0.0)
        return;
      this.progressSmoothed = (double) this.currentAirbase.capture.controlBalance >= 1.0 ? 1f : Mathf.SmoothDamp(this.progressSmoothed, this.currentAirbase.capture.controlBalance, ref this.progressSmoothingVel, 1f);
      this.captureBar.sizeDelta = this.captureBarSize * new Vector2(this.progressSmoothed, 1f);
    }
  }

  public override void UpdateWeaponDisplay(Aircraft aircraft, List<Unit> targetList)
  {
  }
}
