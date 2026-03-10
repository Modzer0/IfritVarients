// Decompiled with JetBrains decompiler
// Type: HUDSlingState
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#nullable disable
public class HUDSlingState : HUDWeaponState
{
  private Image targetDesignator;
  private WeaponStation weaponStation;
  private SlingloadHook hook;
  [SerializeField]
  private Sprite defaultImg;
  [SerializeField]
  private Image reticleImg;
  [SerializeField]
  private Image centerImg;
  [SerializeField]
  private Image unitImg;
  [SerializeField]
  private Image unitFwdImg;
  [SerializeField]
  private Text hookState;
  [SerializeField]
  private Text vehicleInfo;
  [SerializeField]
  private Text ropeInfo;
  [SerializeField]
  private GameObject rangeObject;
  private Vector3 relPos;
  private float relAngle;
  private float UI_scale = 5f;

  public override void SetHUDWeaponState(
    Image targetDesignator,
    Aircraft aircraft,
    WeaponStation weaponStation)
  {
    this.weaponStation = weaponStation;
    this.weaponInfo = weaponStation.WeaponInfo;
    this.targetDesignator = targetDesignator;
    targetDesignator.transform.localScale = Vector3.one;
    SceneSingleton<CombatHUD>.i.targetDesignator.enabled = true;
    SceneSingleton<FlightHud>.i.waterline.enabled = false;
    SceneSingleton<FlightHud>.i.velocityVector.transform.localScale = Vector3.one;
    this.hook = weaponStation.Weapons[0] as SlingloadHook;
    this.reticleImg.enabled = false;
    this.centerImg.enabled = false;
    this.centerImg.rectTransform.sizeDelta = Vector2.zero;
    this.unitImg.enabled = false;
    this.unitFwdImg.enabled = false;
    this.vehicleInfo.enabled = false;
    this.ropeInfo.enabled = false;
    this.rangeObject.SetActive(false);
  }

  public override void HUDFixedUpdate(Aircraft aircraft, List<Unit> targetList)
  {
  }

  public override void UpdateWeaponDisplay(Aircraft aircraft, List<Unit> targetList)
  {
    Unit unit = (Unit) null;
    Unit suspendedUnit = this.hook.GetSuspendedUnit();
    if (aircraft.weaponManager.GetTargetList().Count > 0)
      unit = aircraft.weaponManager.GetTargetList()[0];
    bool flag = (UnityEngine.Object) unit != (UnityEngine.Object) null && !unit.disabled && unit.definition.CanSlingLoad;
    this.targetDesignator.enabled = !flag;
    this.targetDesignator.color = Color.green;
    switch (this.hook.deployState)
    {
      case SlingloadHook.DeployState.Retracted:
        this.hookState.text = flag ? "RETRACTED" : "NO TARGET";
        if (this.reticleImg.enabled)
          this.reticleImg.enabled = false;
        if (this.centerImg.enabled)
        {
          this.centerImg.enabled = false;
          this.centerImg.rectTransform.sizeDelta = Vector2.zero;
        }
        if (this.unitImg.enabled)
          this.unitImg.enabled = false;
        if (this.unitFwdImg.enabled)
          this.unitFwdImg.enabled = false;
        if (this.vehicleInfo.enabled)
          this.vehicleInfo.enabled = false;
        if (this.ropeInfo.enabled)
          this.ropeInfo.enabled = false;
        if (!this.rangeObject.activeSelf)
          break;
        this.rangeObject.SetActive(false);
        break;
      case SlingloadHook.DeployState.Deployed:
        float lineLength = this.hook.GetLineLength();
        float lineMaxLength = this.hook.GetLineMaxLength();
        this.hookState.text = (double) lineLength >= (double) lineMaxLength ? (flag ? ((double) aircraft.radarAlt <= (double) lineLength ? "READY" : "TOO HIGH") : "NO TARGET") : $"EXTENDING v {lineLength:F1}m";
        if (!this.reticleImg.enabled)
          this.reticleImg.enabled = true;
        if (!this.centerImg.enabled)
          this.centerImg.enabled = true;
        this.unitImg.enabled = (UnityEngine.Object) unit != (UnityEngine.Object) null;
        this.unitFwdImg.enabled = (UnityEngine.Object) unit != (UnityEngine.Object) null;
        if (!((UnityEngine.Object) unit != (UnityEngine.Object) null))
          break;
        if (!this.rangeObject.activeSelf)
          this.rangeObject.SetActive(true);
        float num1 = Mathf.Clamp(lineLength - aircraft.radarAlt + unit.definition.height, 0.0f, lineLength) * this.UI_scale;
        this.centerImg.rectTransform.sizeDelta = num1 * Vector2.one;
        this.relPos = this.hook.winch.InverseTransformPoint(unit.transform.position);
        this.relAngle = Vector3.SignedAngle(unit.transform.forward, aircraft.transform.forward, Vector3.up);
        this.relPos.x = Mathf.Clamp(this.relPos.x, -40f, 40f);
        this.relPos.y = 0.0f;
        this.relPos.z = Mathf.Clamp(this.relPos.z, -40f, 40f);
        this.unitImg.rectTransform.sizeDelta = this.UI_scale * new Vector2(unit.definition.width, unit.definition.length);
        this.unitImg.transform.localPosition = this.UI_scale * new Vector3(this.relPos.x, this.relPos.z, 0.0f);
        this.unitImg.transform.localEulerAngles = new Vector3(0.0f, 0.0f, this.relAngle);
        if ((double) this.unitImg.transform.localPosition.magnitude < 1.0 * (double) num1)
        {
          this.centerImg.color = Color.green;
          this.unitImg.color = Color.green;
          this.unitFwdImg.color = Color.green;
          this.unitImg.pixelsPerUnitMultiplier = 2f;
          break;
        }
        if ((double) this.unitImg.transform.localPosition.magnitude < 2.0 * (double) num1)
        {
          this.centerImg.color = Color.yellow;
          this.unitImg.color = Color.yellow;
          this.unitFwdImg.color = Color.yellow;
          this.unitImg.pixelsPerUnitMultiplier = 3f;
          break;
        }
        this.centerImg.color = Color.grey;
        this.unitImg.color = Color.white;
        this.unitFwdImg.color = Color.white;
        this.unitImg.pixelsPerUnitMultiplier = 4f;
        break;
      case SlingloadHook.DeployState.Connected:
        if ((UnityEngine.Object) suspendedUnit == (UnityEngine.Object) null)
          break;
        double num2 = (double) this.hook.loadForce * 0.000101971621;
        if (num2 < 1.0)
          num2 = (double) suspendedUnit.definition.mass * (1.0 / 1000.0);
        this.hookState.text = suspendedUnit.definition.code + " CONNECTED";
        suspendedUnit.CheckRadarAlt();
        if (!this.vehicleInfo.enabled)
          this.vehicleInfo.enabled = true;
        this.vehicleInfo.text = $"{num2:F1}T - {UnitConverter.AltitudeReading(suspendedUnit.radarAlt)}";
        if (!this.ropeInfo.enabled)
          this.ropeInfo.enabled = true;
        this.ropeInfo.text = $"{this.hook.GetRopeAngle():F1}°\n{(ValueType) (float) (1.0 / 1000.0 * (double) this.hook.loadForce):F1}kN";
        if ((double) this.hook.GetRopeAngle() > 45.0 || (double) this.hook.GetRopeFactor() > 0.5)
          this.ropeInfo.color = Color.yellow;
        else if ((double) this.hook.GetRopeAngle() > 80.0 || (double) this.hook.GetRopeFactor() > 0.800000011920929)
          this.ropeInfo.color = Color.red;
        else
          this.ropeInfo.color = Color.green;
        if (!this.reticleImg.enabled)
          this.reticleImg.enabled = true;
        if (this.centerImg.enabled)
          this.centerImg.enabled = false;
        if (!this.unitImg.enabled)
        {
          this.unitImg.enabled = true;
          this.unitImg.pixelsPerUnitMultiplier = 2f;
          this.unitImg.color = Color.green;
        }
        if (!this.unitFwdImg.enabled)
        {
          this.unitFwdImg.enabled = true;
          this.unitFwdImg.color = Color.green;
        }
        this.relPos = this.hook.transform.InverseTransformPoint(suspendedUnit.transform.position);
        this.relAngle = Vector3.SignedAngle(aircraft.transform.forward, suspendedUnit.transform.forward, Vector3.up);
        this.relPos.x = Mathf.Clamp(this.relPos.x, -40f, 40f);
        this.relPos.z = Mathf.Clamp(this.relPos.z, -40f, 40f);
        this.unitImg.rectTransform.sizeDelta = this.UI_scale * new Vector2(suspendedUnit.definition.width, suspendedUnit.definition.length);
        this.unitImg.transform.localPosition = this.UI_scale * new Vector3(this.relPos.x, this.relPos.z, 0.0f);
        this.unitImg.transform.localEulerAngles = new Vector3(0.0f, 0.0f, -this.relAngle);
        this.ropeInfo.transform.eulerAngles = Vector3.zero;
        break;
      case SlingloadHook.DeployState.Retracting:
        this.hookState.text = $"RETRACTING ^ {this.hook.GetLineLength():F1}m";
        if (this.reticleImg.enabled)
          this.reticleImg.enabled = false;
        if (this.centerImg.enabled)
        {
          this.centerImg.enabled = false;
          this.centerImg.rectTransform.sizeDelta = Vector2.zero;
        }
        if (this.unitImg.enabled)
          this.unitImg.enabled = false;
        if (this.unitFwdImg.enabled)
          this.unitFwdImg.enabled = false;
        if (this.vehicleInfo.enabled)
          this.vehicleInfo.enabled = false;
        if (this.ropeInfo.enabled)
          this.ropeInfo.enabled = false;
        if (!this.rangeObject.activeSelf)
          break;
        this.rangeObject.SetActive(false);
        break;
      case SlingloadHook.DeployState.RescuePilot:
        if ((UnityEngine.Object) suspendedUnit == (UnityEngine.Object) null || !(suspendedUnit is PilotDismounted))
          break;
        this.hookState.text = $"RECOVERING PILOT ^ {this.hook.GetLineLength():F1}m";
        double num3 = (double) this.hook.loadForce * 0.000101971621;
        if (num3 < 1.0)
          num3 = (double) suspendedUnit.definition.mass * (1.0 / 1000.0);
        suspendedUnit.CheckRadarAlt();
        if (!this.vehicleInfo.enabled)
          this.vehicleInfo.enabled = true;
        this.vehicleInfo.text = $"{num3:F1}T - {UnitConverter.AltitudeReading(suspendedUnit.radarAlt)}";
        if (!this.ropeInfo.enabled)
          this.ropeInfo.enabled = true;
        this.ropeInfo.text = $"{this.hook.GetRopeAngle():F1}°\n{(ValueType) (float) (1.0 / 1000.0 * (double) this.hook.loadForce):F1}kN";
        if ((double) this.hook.GetRopeAngle() > 45.0 || (double) this.hook.GetRopeFactor() > 0.5)
          this.ropeInfo.color = Color.yellow;
        else if ((double) this.hook.GetRopeAngle() > 80.0 || (double) this.hook.GetRopeFactor() > 0.800000011920929)
          this.ropeInfo.color = Color.red;
        else
          this.ropeInfo.color = Color.green;
        if (!this.reticleImg.enabled)
          this.reticleImg.enabled = true;
        if (this.centerImg.enabled)
        {
          this.centerImg.enabled = false;
          this.centerImg.rectTransform.sizeDelta = Vector2.zero;
        }
        if (!this.unitImg.enabled)
        {
          this.unitImg.enabled = true;
          this.unitImg.pixelsPerUnitMultiplier = 2f;
          this.unitImg.color = Color.green;
        }
        if (!this.unitFwdImg.enabled)
        {
          this.unitFwdImg.enabled = true;
          this.unitFwdImg.color = Color.green;
        }
        this.relPos = this.hook.transform.InverseTransformPoint(suspendedUnit.transform.position);
        this.relAngle = Vector3.SignedAngle(aircraft.transform.forward, suspendedUnit.transform.forward, Vector3.up);
        this.relPos.x = Mathf.Clamp(this.relPos.x, -40f, 40f);
        this.relPos.z = Mathf.Clamp(this.relPos.z, -40f, 40f);
        this.unitImg.rectTransform.sizeDelta = this.UI_scale * new Vector2(suspendedUnit.definition.width, suspendedUnit.definition.length);
        this.unitImg.transform.localPosition = this.UI_scale * new Vector3(this.relPos.x, this.relPos.z, 0.0f);
        this.unitImg.transform.localEulerAngles = new Vector3(0.0f, 0.0f, -this.relAngle);
        this.ropeInfo.transform.eulerAngles = Vector3.zero;
        break;
    }
  }
}
