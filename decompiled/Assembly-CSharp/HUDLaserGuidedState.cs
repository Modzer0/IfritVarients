// Decompiled with JetBrains decompiler
// Type: HUDLaserGuidedState
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#nullable disable
public class HUDLaserGuidedState : HUDWeaponState
{
  private bool hidden;
  private bool allRequirementsMet;
  private float targetDist;
  private float minRange;
  private float maxRange;
  private float rangeRatio;
  private float maxRangeSmoothed;
  private float maxRangeSmoothingVel;
  private float minAlignment;
  private float currentMaxArc;
  private float lastWeaponRangeCalc;
  private float lastTextDisplay;
  private float fovPrev;
  private float currentMaxArcPrev;
  private GlobalPosition knownPos;
  [SerializeField]
  private Image outerCircle;
  [SerializeField]
  private Image innerCircle;
  [SerializeField]
  private Image noShoot;
  [SerializeField]
  private Text maxRangeText;
  [SerializeField]
  private Text hint;
  [SerializeField]
  private Transform textAnchor;
  private Camera cam;
  private Aircraft aircraft;
  private WeaponStation weaponStation;
  private Missile prefabMissile;

  public override void UpdateWeaponDisplay(Aircraft aircraft, List<Unit> targetList)
  {
    SceneSingleton<FlightHud>.i.velocityVector.color = new Color(0.0f, 1f, 0.0f, Mathf.Clamp01((float) ((double) Vector3.Distance(SceneSingleton<FlightHud>.i.velocityVector.transform.position, SceneSingleton<CombatHUD>.i.targetDesignator.transform.position) * 0.014999999664723873 - 0.15000000596046448)));
    SceneSingleton<CombatHUD>.i.targetDesignator.color = Color.Lerp(Color.black, Color.green, Mathf.Clamp01((float) ((double) Vector3.Distance(SceneSingleton<FlightHud>.i.GetHUDCenter().position, SceneSingleton<CombatHUD>.i.targetDesignator.transform.position) * 0.014999999664723873 - 0.15000000596046448)));
    SceneSingleton<CombatHUD>.i.targetDesignator.enabled = !aircraft.gearDeployed;
    this.maxRangeSmoothed = Mathf.SmoothDamp(this.maxRangeSmoothed, this.maxRange, ref this.maxRangeSmoothingVel, 1f, 1000f);
    this.rangeRatio = this.maxRangeSmoothed / this.targetDist;
    this.targetDist = FastMath.Distance(this.knownPos, aircraft.GlobalPosition());
    this.innerCircle.fillAmount = this.rangeRatio;
    this.currentMaxArc = Mathf.Min(this.minAlignment, Mathf.Max(this.targetDist, this.minRange) * (1f / 500f));
    if ((double) this.cam.fieldOfView != (double) this.fovPrev || (double) this.currentMaxArcPrev != (double) this.currentMaxArc)
    {
      this.fovPrev = this.cam.fieldOfView;
      this.currentMaxArcPrev = this.currentMaxArc;
      this.outerCircle.transform.localScale = (float) (50.0 / (double) this.fovPrev * ((double) this.currentMaxArc / 8.0)) * Vector3.one;
      this.hint.transform.position = this.textAnchor.position;
      this.maxRangeText.transform.position = this.textAnchor.position;
    }
    this.CalcWeaponRange();
    this.DisplayText();
  }

  private void CalcWeaponRange()
  {
    if ((double) Time.timeSinceLevelLoad - (double) this.lastWeaponRangeCalc < 1.0 || this.targetList.Count == 0)
      return;
    this.lastWeaponRangeCalc = Time.timeSinceLevelLoad;
    this.aircraft.NetworkHQ.TryGetKnownPosition(this.targetList[0], out this.knownPos);
    this.maxRange = this.prefabMissile.CalcRange(this.aircraft.speed, this.aircraft.GlobalPosition().y, this.knownPos.y, this.targetDist, 0.0f, out float _);
  }

  private void DisplayText()
  {
    if ((double) Time.timeSinceLevelLoad - (double) this.lastTextDisplay < 0.10000000149011612 || this.hidden && this.targetList.Count == 0)
      return;
    this.lastTextDisplay = Time.timeSinceLevelLoad;
    if (this.targetList.Count == 0 || this.weaponStation.Ammo == 0)
    {
      this.HideAll();
    }
    else
    {
      this.ShowAll();
      this.maxRangeText.text = "MAX " + UnitConverter.DistanceReading(this.maxRangeSmoothed);
      this.maxRangeText.enabled = (double) this.rangeRatio < 2.0;
      float num1 = Vector3.Angle(this.aircraft.transform.forward, this.knownPos - this.aircraft.GlobalPosition());
      bool flag = this.aircraft.NetworkHQ.IsTargetLased(this.targetList[0]);
      int num2 = this.allRequirementsMet ? 1 : 0;
      this.allRequirementsMet = false;
      if ((double) this.targetDist > (double) this.maxRangeSmoothed)
        this.hint.text = "OUT OF RANGE";
      else if ((double) num1 > (double) this.currentMaxArc)
        this.hint.text = "OUT OF ARC";
      else if (!flag)
        this.hint.text = "NOT LASED";
      else if ((double) this.targetDist < (double) this.minRange)
      {
        this.hint.text = "TOO CLOSE";
      }
      else
      {
        this.allRequirementsMet = true;
        this.hint.text = "SHOOT";
      }
      int num3 = this.allRequirementsMet ? 1 : 0;
      if (num2 == num3)
        return;
      this.noShoot.enabled = !this.allRequirementsMet;
      this.outerCircle.color = this.allRequirementsMet ? Color.black : Color.white * 0.5f;
      this.innerCircle.color = this.allRequirementsMet ? Color.green : Color.green * 0.5f + Color.red * 0.5f;
    }
  }

  private void HideAll()
  {
    if (this.hidden)
      return;
    this.hidden = true;
    this.outerCircle.enabled = false;
    this.innerCircle.enabled = false;
    this.hint.enabled = false;
    this.maxRangeText.enabled = false;
    this.noShoot.enabled = false;
  }

  private void ShowAll()
  {
    if (!this.hidden)
      return;
    this.hidden = false;
    this.outerCircle.enabled = true;
    this.innerCircle.enabled = true;
    this.hint.enabled = true;
    this.maxRangeText.enabled = true;
    this.ResetRange();
  }

  private void ResetRange()
  {
    if (this.targetList.Count > 0)
    {
      this.aircraft.NetworkHQ.TryGetKnownPosition(this.targetList[0], out this.knownPos);
      this.CalcWeaponRange();
      this.maxRangeSmoothed = this.maxRange;
    }
    else
      this.maxRangeSmoothed = this.weaponInfo.targetRequirements.maxRange;
  }

  public override void SetHUDWeaponState(
    Image targetDesignator,
    Aircraft aircraft,
    WeaponStation weaponStation)
  {
    this.weaponStation = weaponStation;
    this.weaponInfo = weaponStation.WeaponInfo;
    this.aircraft = SceneSingleton<CombatHUD>.i.aircraft;
    targetDesignator.transform.localScale = Vector3.one;
    this.targetList = aircraft.weaponManager.GetTargetList();
    this.prefabMissile = weaponStation.WeaponInfo.weaponPrefab.GetComponent<Missile>();
    this.minRange = this.weaponInfo.targetRequirements.minRange;
    this.minAlignment = this.weaponInfo.targetRequirements.minAlignment;
    this.cam = SceneSingleton<CameraStateManager>.i.mainCamera;
    SceneSingleton<FlightHud>.i.waterline.enabled = false;
    SceneSingleton<FlightHud>.i.velocityVector.transform.localScale = Vector3.one;
    this.ResetRange();
    this.HideAll();
  }
}
