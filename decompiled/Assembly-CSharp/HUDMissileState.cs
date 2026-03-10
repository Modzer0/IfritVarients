// Decompiled with JetBrains decompiler
// Type: HUDMissileState
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#nullable disable
public class HUDMissileState : HUDWeaponState
{
  private bool hidden;
  private bool allRequirementsMet;
  private float minTargetDist;
  private float maxTargetDist;
  private float minRange;
  private float maxRange;
  private float noEscapeRange;
  private float maxTargetAngle;
  private float minAlignment;
  private float minPlatformSpeed;
  private float maxTargetSpeed;
  private float lastWeaponRangeCalc;
  private float lastTextDisplay;
  private GlobalPosition knownPos;
  [SerializeField]
  private Image noShoot;
  [SerializeField]
  private Image noEscapeMark;
  [SerializeField]
  private Image maxDistImage;
  [SerializeField]
  private Image minDistImage;
  [SerializeField]
  private Image distSpanImage;
  [SerializeField]
  private Image[] ladderImages;
  [SerializeField]
  private Text maxRangeText;
  [SerializeField]
  private Text minRangeText;
  [SerializeField]
  private Text noEscapeRangeText;
  [SerializeField]
  private Text targetText;
  [SerializeField]
  private Text hint;
  [SerializeField]
  private Transform rMaxTransform;
  [SerializeField]
  private Transform rMinTransform;
  [SerializeField]
  private Transform rNETransform;
  [SerializeField]
  private Transform maxTargetDistTransform;
  [SerializeField]
  private Transform minTargetDistTransform;
  [SerializeField]
  private Transform targetDistSpan;
  [SerializeField]
  private Transform avgDistTransform;
  [SerializeField]
  private Transform outRangeTransform;
  private Aircraft aircraft;
  private Unit farTarget;
  private Unit closeTarget;
  private WeaponStation weaponStation;
  private Missile prefabMissile;

  public override void UpdateWeaponDisplay(Aircraft aircraft, List<Unit> targetList)
  {
    SceneSingleton<FlightHud>.i.velocityVector.color = new Color(0.0f, 1f, 0.0f, Mathf.Clamp01((float) ((double) Vector3.Distance(SceneSingleton<FlightHud>.i.velocityVector.transform.position, SceneSingleton<CombatHUD>.i.targetDesignator.transform.position) * 0.014999999664723873 - 0.15000000596046448)));
    SceneSingleton<CombatHUD>.i.targetDesignator.color = Color.Lerp(Color.black, Color.green, Mathf.Clamp01((float) ((double) Vector3.Distance(SceneSingleton<FlightHud>.i.GetHUDCenter().position, SceneSingleton<CombatHUD>.i.targetDesignator.transform.position) * 0.014999999664723873 - 0.15000000596046448)));
    SceneSingleton<CombatHUD>.i.targetDesignator.enabled = !aircraft.gearDeployed;
    GlobalPosition b = aircraft.GlobalPosition();
    if (targetList.Count > 0 && (Object) this.farTarget != (Object) null)
    {
      GlobalPosition knownPosition1;
      if (aircraft.NetworkHQ.TryGetKnownPosition(this.farTarget, out knownPosition1))
        this.maxTargetDist = FastMath.Distance(knownPosition1, b);
      if (targetList.Count > 1)
      {
        GlobalPosition knownPosition2;
        if (aircraft.NetworkHQ.TryGetKnownPosition(this.closeTarget, out knownPosition2))
          this.minTargetDist = FastMath.Distance(knownPosition2, b);
      }
      else
        this.minTargetDist = this.maxTargetDist;
    }
    this.rNETransform.position = Vector3.Lerp(this.rMinTransform.position, this.rMaxTransform.position, Mathf.Max((float) (((double) this.noEscapeRange - (double) this.minRange) / ((double) this.maxRange - (double) this.minRange)), 0.1f));
    this.maxTargetDistTransform.position = (double) this.maxTargetDist >= (double) this.maxRange ? Vector3.Lerp(this.rMaxTransform.position, this.outRangeTransform.position, (this.maxTargetDist - this.maxRange) / this.maxRange) : Vector3.Lerp(this.rMinTransform.position, this.rMaxTransform.position, (float) (((double) this.maxTargetDist - (double) this.minRange) / ((double) this.maxRange - (double) this.minRange)));
    this.minTargetDistTransform.position = (double) this.minTargetDist >= (double) this.maxRange ? Vector3.Lerp(this.rMaxTransform.position, this.outRangeTransform.position, (this.minTargetDist - this.maxRange) / this.maxRange) : Vector3.Lerp(this.rMinTransform.position, this.rMaxTransform.position, (float) (((double) this.minTargetDist - (double) this.minRange) / ((double) this.maxRange - (double) this.minRange)));
    this.targetDistSpan.localScale = new Vector3(this.targetDistSpan.localScale.x, 1080f / (float) Screen.height * (this.maxTargetDistTransform.position.y - this.minTargetDistTransform.position.y), 1f);
    this.avgDistTransform.position = Vector3.Lerp(this.minTargetDistTransform.position, this.maxTargetDistTransform.position, 0.5f);
    this.CalcWeaponRange();
    this.DisplayText();
  }

  private void CalcWeaponRange()
  {
    if ((double) Time.timeSinceLevelLoad - (double) this.lastWeaponRangeCalc < 1.0 || this.targetList.Count == 0 || this.weaponStation.Ammo == 0)
      return;
    this.lastWeaponRangeCalc = Time.timeSinceLevelLoad;
    GlobalPosition b = this.aircraft.GlobalPosition();
    float num1 = 0.0f;
    float num2 = float.MaxValue;
    this.maxTargetAngle = 0.0f;
    this.maxTargetSpeed = 0.0f;
    foreach (Unit target in this.targetList)
    {
      GlobalPosition knownPosition;
      if (this.aircraft.NetworkHQ.TryGetKnownPosition(target, out knownPosition))
      {
        float num3 = FastMath.SquareDistance(knownPosition, b);
        if ((double) num3 > (double) num1)
        {
          num1 = num3;
          this.farTarget = target;
        }
        if ((double) num3 < (double) num2)
        {
          num2 = num3;
          this.closeTarget = target;
        }
        this.maxTargetAngle = Mathf.Max(this.maxTargetAngle, Vector3.Angle(knownPosition - b, this.aircraft.transform.forward));
        this.maxTargetSpeed = Mathf.Max(target.speed, this.maxTargetSpeed);
      }
    }
    if (!((Object) this.prefabMissile != (Object) null))
      return;
    this.maxRange = this.prefabMissile.CalcRange(this.aircraft.speed, this.aircraft.GlobalPosition().y, this.knownPos.y, this.maxTargetDist, this.maxTargetSpeed, out this.noEscapeRange);
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
      this.maxRangeText.text = "MAX \n" + UnitConverter.DistanceReading(this.maxRange);
      this.minRangeText.text = "MIN \n" + UnitConverter.DistanceReading(this.minRange);
      if ((double) this.noEscapeRange < (double) this.maxRange * 0.89999997615814209)
      {
        this.noEscapeMark.enabled = true;
        this.noEscapeRangeText.enabled = true;
        this.noEscapeRangeText.text = "NEZ \n" + UnitConverter.DistanceReading(this.noEscapeRange);
      }
      else
      {
        this.noEscapeMark.enabled = false;
        this.noEscapeRangeText.enabled = false;
      }
      this.allRequirementsMet = false;
      if ((double) this.maxTargetDist > (double) this.maxRange)
      {
        this.hint.enabled = true;
        this.hint.text = "OUT OF RANGE";
      }
      else if ((double) this.minTargetDist < (double) this.minRange)
      {
        this.hint.enabled = true;
        this.hint.text = "TOO CLOSE";
      }
      else if ((double) this.maxTargetAngle > (double) this.minAlignment)
        this.hint.text = "OUT OF ARC";
      else if ((double) this.aircraft.speed < (double) this.minPlatformSpeed)
      {
        this.hint.text = "TOO SLOW";
      }
      else
      {
        this.allRequirementsMet = true;
        this.hint.text = "SHOOT";
        this.hint.enabled = (double) this.maxTargetDist < (double) this.noEscapeRange;
      }
      this.noShoot.enabled = !this.allRequirementsMet;
    }
  }

  private void HideAll()
  {
    if (this.hidden)
      return;
    this.hidden = true;
    this.hint.enabled = false;
    this.maxRangeText.enabled = false;
    this.minRangeText.enabled = false;
    this.targetText.enabled = false;
    this.noEscapeMark.enabled = false;
    this.noEscapeRangeText.enabled = false;
    this.noShoot.enabled = false;
    this.maxDistImage.enabled = false;
    this.minDistImage.enabled = false;
    this.distSpanImage.enabled = false;
    foreach (Behaviour ladderImage in this.ladderImages)
      ladderImage.enabled = false;
  }

  private void ShowAll()
  {
    if (!this.hidden)
      return;
    this.hidden = false;
    this.maxRangeText.enabled = true;
    this.minRangeText.enabled = true;
    this.targetText.enabled = true;
    this.maxDistImage.enabled = true;
    this.minDistImage.enabled = true;
    this.distSpanImage.enabled = true;
    foreach (Behaviour ladderImage in this.ladderImages)
      ladderImage.enabled = true;
    this.ResetRange();
  }

  private void ResetRange()
  {
    if (this.targetList.Count > 0)
      this.CalcWeaponRange();
    else
      this.maxRange = this.weaponInfo.targetRequirements.maxRange;
  }

  public override void SetHUDWeaponState(
    Image targetDesignator,
    Aircraft aircraft,
    WeaponStation weaponStation)
  {
    this.weaponStation = weaponStation;
    targetDesignator.transform.localScale = Vector3.one;
    this.weaponInfo = weaponStation.WeaponInfo;
    this.aircraft = SceneSingleton<CombatHUD>.i.aircraft;
    this.targetList = aircraft.weaponManager.GetTargetList();
    if ((Object) weaponStation.WeaponInfo.weaponPrefab != (Object) null)
      this.prefabMissile = weaponStation.WeaponInfo.weaponPrefab.GetComponent<Missile>();
    this.minRange = this.weaponInfo.targetRequirements.minRange;
    this.maxRange = this.weaponInfo.targetRequirements.maxRange;
    this.noEscapeRange = this.maxRange;
    this.minAlignment = this.weaponInfo.targetRequirements.minAlignment;
    this.minPlatformSpeed = this.weaponInfo.targetRequirements.minOwnerSpeed;
    SceneSingleton<FlightHud>.i.waterline.enabled = true;
    SceneSingleton<FlightHud>.i.velocityVector.transform.localScale = Vector3.one;
    this.ResetRange();
    this.HideAll();
  }
}
