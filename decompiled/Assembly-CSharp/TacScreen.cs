// Decompiled with JetBrains decompiler
// Type: TacScreen
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;
using UnityEngine.UI;

#nullable disable
public class TacScreen : MonoBehaviour
{
  [SerializeField]
  private Text timeDisplay;
  [SerializeField]
  private GameObject timeObject;
  [SerializeField]
  private bool targetCamHidesTime = true;
  [SerializeField]
  private Canvas canvas;
  [SerializeField]
  private RenderTexture renderTexture;
  [SerializeField]
  private Camera cam;
  [SerializeField]
  private GameObject targetCamDisplay;
  [SerializeField]
  private GameObject landingCamDisplay;
  [SerializeField]
  private Material screenMaterial;
  private bool radarOn;
  private Aircraft aircraft;
  private TargetDetector opticalScanner;
  [SerializeField]
  private float displaySize;
  [SerializeField]
  private float metersPerPixel;
  [SerializeField]
  private float lastUpdate;
  [SerializeField]
  private Transform radarCenter;
  [SerializeField]
  private Transform iconLayer;
  [SerializeField]
  private GameObject iconPrefab;
  [SerializeField]
  private GameObject pingPrefab;
  [SerializeField]
  private GameObject missilePrefab;
  [SerializeField]
  private GameObject radarUnitPrefab;
  private Vector3 headingAtScan;
  [SerializeField]
  private Image radarCone;
  [SerializeField]
  private Image scanLine;

  public void Initialize(Aircraft aircraft, Cockpit cockpit)
  {
    this.aircraft = aircraft;
    this.headingAtScan = aircraft.transform.forward;
    aircraft.onDisableUnit += new Action<Unit>(this.TacScreen_OnAircraftDisabled);
    aircraft.targetCam.onCamToggle += new Action<TargetCam.OnCamToggle>(this.TacScreen_OnCamToggle);
    this.targetCamDisplay.SetActive(false);
    this.landingCamDisplay.SetActive(false);
    this.StartSlowUpdateDelayed(1.5f, 1f, new Action(this.ShowTime));
    this.StartSlowUpdateDelayed(1.5f, 1f, new Action(this.UpdateMissileWarning));
    if ((UnityEngine.Object) aircraft.radar != (UnityEngine.Object) null)
    {
      this.scanLine.enabled = true;
      aircraft.radar.onScan += new Action(this.TacScreen_OnRadarScan);
    }
    else
    {
      this.scanLine.enabled = false;
      this.opticalScanner = aircraft.gameObject.GetComponentInChildren<TargetDetector>();
      this.opticalScanner.onScan += new Action(this.TacScreen_OnOpticalScan);
    }
    this.metersPerPixel = this.iconLayer.GetComponent<RectTransform>().rect.height / this.displaySize;
    aircraft.onRadarWarning += new Action<Aircraft.OnRadarWarning>(this.TacScreen_OnRadarWarning);
  }

  private void OnDestroy()
  {
    if ((UnityEngine.Object) this.aircraft != (UnityEngine.Object) null)
    {
      this.aircraft.onDisableUnit -= new Action<Unit>(this.TacScreen_OnAircraftDisabled);
      this.aircraft.onRadarWarning -= new Action<Aircraft.OnRadarWarning>(this.TacScreen_OnRadarWarning);
      if ((UnityEngine.Object) this.aircraft.targetCam != (UnityEngine.Object) null)
        this.aircraft.targetCam.onCamToggle -= new Action<TargetCam.OnCamToggle>(this.TacScreen_OnCamToggle);
      if ((UnityEngine.Object) this.aircraft.radar != (UnityEngine.Object) null)
        this.aircraft.radar.onScan -= new Action(this.TacScreen_OnRadarScan);
    }
    if (!((UnityEngine.Object) this.opticalScanner != (UnityEngine.Object) null))
      return;
    this.opticalScanner.onScan -= new Action(this.TacScreen_OnOpticalScan);
  }

  private void UpdateMissileWarning()
  {
    if (this.aircraft.GetMissileWarningSystem().knownMissiles.Count <= 0)
      return;
    foreach (Missile knownMissile in this.aircraft.GetMissileWarningSystem().knownMissiles)
    {
      GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.missilePrefab, this.iconLayer);
      Vector3 vector3 = this.aircraft.transform.InverseTransformPoint(knownMissile.transform.position - this.aircraft.transform.position);
      Color color = Color.yellow;
      color = (double) vector3.magnitude >= (double) this.displaySize / 4.0 ? ((double) vector3.magnitude >= (double) this.displaySize / 2.0 ? Color.yellow : new Color(1f, 0.5f, 0.0f)) : Color.red;
      vector3 *= this.metersPerPixel;
      gameObject.transform.localPosition = new Vector3(vector3.x, vector3.z, 0.0f);
      gameObject.GetComponentInChildren<Image>().color = color;
      UnityEngine.Object.Destroy((UnityEngine.Object) gameObject, 1f);
    }
  }

  private void TacScreen_OnRadarWarning(Aircraft.OnRadarWarning e)
  {
    float num1 = Vector3.Distance(e.emitter.transform.position, this.aircraft.transform.position);
    Color color1 = new Color(1f, 1f, 0.0f, 0.5f);
    Color color2 = new Color(1f, 0.0f, 0.0f, 0.5f);
    Color color3 = e.isTarget ? color2 : color1;
    if (!this.aircraft.NetworkHQ.trackingDatabase.ContainsKey(e.emitter.persistentID) || (double) num1 > (double) this.displaySize / 2.0)
    {
      float t = 1f;
      GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.pingPrefab, this.iconLayer);
      Vector3 vector3_1 = e.emitter.transform.position;
      vector3_1 = new Vector3(vector3_1.x, 0.0f, vector3_1.z);
      Vector3 vector3_2 = this.aircraft.transform.position;
      vector3_2 = new Vector3(vector3_2.x, 0.0f, vector3_2.z);
      Vector3 from = this.aircraft.transform.forward;
      from = new Vector3(from.x, 0.0f, from.z);
      Vector3 normalized = (vector3_1 - vector3_2).normalized;
      float num2 = Vector3.SignedAngle(from, normalized, this.aircraft.transform.up);
      gameObject.transform.localEulerAngles = new Vector3(0.0f, 0.0f, -num2);
      if (e.detected)
      {
        gameObject.GetComponent<Image>().color = color3;
        ++t;
      }
      UnityEngine.Object.Destroy((UnityEngine.Object) gameObject, t);
    }
    else
    {
      float t = 2f;
      GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.radarUnitPrefab, this.iconLayer);
      Vector3 vector3 = this.aircraft.transform.InverseTransformPoint(e.emitter.transform.position - this.aircraft.transform.position) * this.metersPerPixel;
      gameObject.transform.localPosition = new Vector3(vector3.x, vector3.z, 0.0f);
      Image component = gameObject.GetComponent<Image>();
      component.sprite = e.emitter.definition.hostileIcon;
      if (e.detected)
      {
        component.color = color3;
        ++t;
      }
      UnityEngine.Object.Destroy((UnityEngine.Object) gameObject, t);
    }
  }

  private void TacScreen_OnRadarScan()
  {
    this.headingAtScan = this.aircraft.transform.forward;
    foreach (Unit detectedTarget in this.aircraft.radar.detectedTargets)
    {
      GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.iconPrefab, this.iconLayer);
      Vector3 vector3 = this.aircraft.transform.InverseTransformPoint(detectedTarget.transform.position - this.aircraft.transform.position) * this.metersPerPixel;
      gameObject.transform.localPosition = new Vector3(vector3.x, vector3.z, 0.0f);
      UnityEngine.Object.Destroy((UnityEngine.Object) gameObject, 4f);
    }
  }

  private void TacScreen_OnOpticalScan()
  {
    this.headingAtScan = this.aircraft.transform.forward;
    Vector3 normalized = new Vector3(this.aircraft.transform.forward.x, 0.0f, this.aircraft.transform.forward.z).normalized;
    foreach (Unit detectedTarget in this.opticalScanner.detectedTargets)
    {
      GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.iconPrefab, this.iconLayer);
      Vector3 vector3 = (detectedTarget.transform.position - this.aircraft.transform.position) with
      {
        y = 0.0f
      } * this.metersPerPixel;
      gameObject.transform.localPosition = new Vector3(Vector3.Dot(vector3, Vector3.Cross(normalized, -Vector3.up)), Vector3.Dot(normalized, vector3), 0.0f);
      UnityEngine.Object.Destroy((UnityEngine.Object) gameObject, 3f);
    }
  }

  private void ScanRadar()
  {
    this.scanLine.transform.localEulerAngles = Vector3.forward * Mathf.Sin((float) ((double) Time.timeSinceLevelLoad * 0.5 * 3.1415927410125732)) * 26f;
    this.iconLayer.transform.localEulerAngles = Vector3.forward * TargetCalc.GetAngleOnAxis(this.aircraft.transform.forward, this.headingAtScan, -Vector3.up);
  }

  private void ScanOptical()
  {
    this.scanLine.transform.localEulerAngles = Vector3.forward * Time.timeSinceLevelLoad * -180f;
    this.iconLayer.transform.localEulerAngles = Vector3.forward * TargetCalc.GetAngleOnAxis(this.aircraft.transform.forward, this.headingAtScan, -Vector3.up);
  }

  private void TacScreen_OnCamToggle(TargetCam.OnCamToggle e)
  {
    if (e.camMode != TargetCam.CamMode.landingMode)
      this.targetCamDisplay.SetActive(e.enabled);
    else
      this.landingCamDisplay.SetActive(e.enabled);
    if (!this.targetCamHidesTime)
      return;
    this.timeObject.SetActive(!e.enabled);
  }

  private void TacScreen_OnAircraftDisabled(Unit unit) => UnityEngine.Object.Destroy((UnityEngine.Object) this.gameObject);

  private void ShowTime()
  {
    this.timeDisplay.text = UnitConverter.TimeOfDay(NetworkSceneSingleton<LevelInfo>.i.timeOfDay, true);
  }

  private void Update()
  {
    if ((double) Time.timeSinceLevelLoad - (double) this.lastUpdate < 0.05000000074505806)
      return;
    this.lastUpdate = Time.timeSinceLevelLoad;
    float num = 2f;
    if ((double) NetworkSceneSingleton<LevelInfo>.i.timeOfDay < 6.0 || (double) NetworkSceneSingleton<LevelInfo>.i.timeOfDay > 18.0)
      num = 0.5f;
    if (!NetworkSceneSingleton<LevelInfo>.i.PostProcessing.enabled)
      num /= 5f;
    this.screenMaterial.SetColor("_EmissionColor", Color.white * num);
    if ((UnityEngine.Object) this.radarCone == (UnityEngine.Object) null || (UnityEngine.Object) this.aircraft.radar == (UnityEngine.Object) null)
    {
      this.ScanOptical();
    }
    else
    {
      if (this.aircraft.radar.activated && !this.radarOn)
      {
        this.radarCone.enabled = true;
        this.scanLine.enabled = true;
        this.radarOn = true;
      }
      if (!this.aircraft.radar.activated && this.radarOn)
      {
        this.radarCone.enabled = false;
        this.scanLine.enabled = false;
        this.radarOn = false;
      }
      if (!this.radarOn)
        return;
      this.ScanRadar();
    }
  }

  [Serializable]
  private class HardpointSetDisplay
  {
    [SerializeField]
    private Image[] hardpoints;
  }
}
