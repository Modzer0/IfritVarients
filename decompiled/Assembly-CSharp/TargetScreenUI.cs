// Decompiled with JetBrains decompiler
// Type: TargetScreenUI
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#nullable disable
public class TargetScreenUI : MonoBehaviour
{
  [SerializeField]
  private Canvas displayCanvas;
  [SerializeField]
  private Text typeText;
  [SerializeField]
  private Text pilotText;
  [SerializeField]
  private Text distance;
  [SerializeField]
  private Text heading;
  [SerializeField]
  private Text altitude;
  [SerializeField]
  private Text rel_altitude;
  [SerializeField]
  private Text speed;
  [SerializeField]
  private Text rel_speed;
  [SerializeField]
  private Text noLock;
  [SerializeField]
  private Text magText;
  [SerializeField]
  private Text modeText;
  [SerializeField]
  private Text bearingText;
  [SerializeField]
  private Text gridText;
  [SerializeField]
  private Image bearingImg;
  [SerializeField]
  private GameObject targetLockBox;
  [SerializeField]
  private GameObject lasingIcon;
  [SerializeField]
  private Sprite targetBoxSprite;
  [SerializeField]
  private Sprite friendlyBoxSprite;
  [SerializeField]
  private Sprite jammedSprite;
  [SerializeField]
  private Sprite lasedSprite;
  [SerializeField]
  private Sprite outdatedSprite;
  [SerializeField]
  private Transform bottomLeft;
  [SerializeField]
  private Image aimingBoxBgd;
  [SerializeField]
  private Image aimingBoxImg;
  [SerializeField]
  private Image aimingDotImg;
  private FactionHQ hq;
  private Vector3 scaleVector = new Vector3(1f, 1f, 0.0f);
  private bool displayingInfo;
  private Camera cam;
  private List<Image> targetBoxes = new List<Image>();
  private List<Image> lasingIcons = new List<Image>();
  private List<Unit> targetList;
  private TargetCam targetCam;
  private LaserDesignator laserDesignator;
  [SerializeField]
  private TargetScreenUI.GunCCIPDisplay gunCCIPDisplay;

  public void SetupCamera(Camera cam, Camera UICam, Aircraft aircraft)
  {
    this.displayCanvas.worldCamera = UICam;
    this.targetCam = aircraft.targetCam;
    this.hq = aircraft.NetworkHQ;
    this.cam = cam;
    this.laserDesignator = aircraft.GetLaserDesignator();
    this.targetList = aircraft.weaponManager.GetTargetList();
    this.StartSlowUpdate(0.1f, new Action(this.UpdateTargetInfo));
  }

  private void OnDestroy()
  {
    if (!((UnityEngine.Object) this.displayCanvas != (UnityEngine.Object) null))
      return;
    UnityEngine.Object.Destroy((UnityEngine.Object) this.displayCanvas.gameObject);
  }

  private void UpdateTargetInfo()
  {
    if ((UnityEngine.Object) SceneSingleton<CombatHUD>.i.aircraft == (UnityEngine.Object) null)
      return;
    while (this.targetBoxes.Count != this.targetList.Count)
    {
      if (this.targetBoxes.Count < this.targetList.Count)
      {
        this.targetBoxes.Add(UnityEngine.Object.Instantiate<GameObject>(this.targetLockBox, this.bottomLeft).GetComponent<Image>());
      }
      else
      {
        List<Image> targetBoxes = this.targetBoxes;
        UnityEngine.Object.Destroy((UnityEngine.Object) targetBoxes[targetBoxes.Count - 1].gameObject);
        this.targetBoxes.RemoveAt(this.targetBoxes.Count - 1);
      }
    }
    if (this.targetList.Count == 0)
    {
      if (!this.displayingInfo)
        return;
      this.ToggleInfoDisplay(false);
    }
    else
    {
      if (!this.displayingInfo)
        this.ToggleInfoDisplay(true);
      for (int index = 0; index < this.targetBoxes.Count; ++index)
      {
        Image targetBox = this.targetBoxes[index];
        Unit target = this.targetList[index];
        Sprite sprite = this.targetBoxSprite;
        if ((UnityEngine.Object) target.NetworkHQ == (UnityEngine.Object) SceneSingleton<CombatHUD>.i.aircraft.NetworkHQ)
        {
          sprite = this.friendlyBoxSprite;
        }
        else
        {
          if (target.HasRadarEmission() && (this.targetList[index].radar as Radar).IsJammed())
            sprite = this.jammedSprite;
          if (this.hq.IsTargetLased(target))
            sprite = this.lasedSprite;
          if (!this.hq.IsTargetPositionAccurate(target, 20f))
            sprite = this.outdatedSprite;
        }
        if ((UnityEngine.Object) targetBox.sprite != (UnityEngine.Object) sprite)
          targetBox.sprite = sprite;
      }
      this.magText.text = $"Mag x{this.targetCam.GetMag():F1}";
      this.distance.text = "RNG " + UnitConverter.DistanceReading(this.targetCam.GetDist());
      this.gridText.text = "GRID: " + this.targetCam.GetGrid();
      this.modeText.text = this.targetCam.UsingIR() ? "MODE: IR" : "MODE: COLOR";
      Transform camMount = this.targetCam.GetCamMount();
      this.bearingText.text = $"{camMount.transform.localEulerAngles.y:F0}°";
      this.bearingImg.rectTransform.localEulerAngles = new Vector3(0.0f, 0.0f, -camMount.transform.localEulerAngles.y);
      Unit target1 = this.targetList[0];
      bool flag = target1 is Aircraft || target1 is Missile;
      if ((UnityEngine.Object) target1.NetworkHQ == (UnityEngine.Object) null)
        this.typeText.color = Color.white;
      else
        this.typeText.color = (UnityEngine.Object) target1.NetworkHQ == (UnityEngine.Object) this.hq ? GameAssets.i.HUDFriendly : GameAssets.i.HUDHostile;
      if (flag && target1 is Aircraft aircraft && (UnityEngine.Object) aircraft.pilots[0].player != (UnityEngine.Object) null)
      {
        this.pilotText.gameObject.SetActive(true);
        this.pilotText.text = "Pilot : " + aircraft.pilots[0].player.PlayerName;
        this.pilotText.color = this.typeText.color;
      }
      else
        this.pilotText.gameObject.SetActive(false);
      if (this.hq.IsTargetPositionAccurate(this.targetList[0], 20f) & flag)
      {
        GlobalPosition globalPosition = this.targetList[0].GlobalPosition();
        Vector3 vector3 = globalPosition - SceneSingleton<CombatHUD>.i.aircraft.GlobalPosition();
        this.heading.text = $"HDG {this.targetList[0].transform.eulerAngles.y:F0}°";
        this.altitude.text = "ALT " + UnitConverter.AltitudeReading(globalPosition.y);
        this.rel_altitude.text = "REL " + UnitConverter.AltitudeReading(vector3.y);
        this.speed.text = "SPD " + UnitConverter.SpeedReading(this.targetList[0].speed);
        this.rel_speed.text = "REL " + UnitConverter.SpeedReading(Vector3.Dot(SceneSingleton<CombatHUD>.i.aircraft.rb.velocity, vector3.normalized) - Vector3.Dot(this.targetList[0].rb.velocity, vector3.normalized));
      }
      else
      {
        this.heading.text = "HDG -";
        this.altitude.text = "ALT -";
        this.rel_altitude.text = "REL -";
        this.speed.text = "SPD -";
        this.rel_speed.text = "REL -";
      }
      if (this.targetList.Count > 1)
      {
        this.typeText.text = $"{this.targetList.Count} targets";
        this.heading.text = "HDG -";
        this.altitude.text = "ALT -";
        this.rel_altitude.text = "REL -";
        this.speed.text = "SPD -";
        this.rel_speed.text = "REL -";
      }
      else
        this.typeText.text = target1 is Aircraft ? target1.definition.unitName : target1.unitName;
      if (SceneSingleton<CombatHUD>.i.aircraft.weaponManager.currentWeaponStation != null && SceneSingleton<CombatHUD>.i.aircraft.weaponManager.currentWeaponStation.HasTurret())
      {
        if (!this.aimingBoxBgd.enabled)
          this.aimingBoxBgd.enabled = true;
        if (!this.aimingBoxImg.enabled)
          this.aimingBoxImg.enabled = true;
        if (!this.aimingDotImg.enabled)
          this.aimingDotImg.enabled = true;
        Vector2 turretRelativeAim = SceneSingleton<CombatHUD>.i.aircraft.weaponManager.currentWeaponStation.GetTurretRelativeAim();
        this.aimingDotImg.transform.localPosition = new Vector3(Mathf.Clamp(-turretRelativeAim.x, -30f, 30f), Mathf.Clamp(turretRelativeAim.y, -30f, 30f), 0.0f);
      }
      else
      {
        if (this.aimingBoxBgd.enabled)
          this.aimingBoxBgd.enabled = false;
        if (this.aimingBoxImg.enabled)
          this.aimingBoxImg.enabled = false;
        if (!this.aimingDotImg.enabled)
          return;
        this.aimingDotImg.enabled = false;
      }
    }
  }

  private void ToggleInfoDisplay(bool enabled)
  {
    this.displayingInfo = enabled;
    this.noLock.gameObject.SetActive(!enabled);
    this.typeText.gameObject.SetActive(enabled);
    this.pilotText.gameObject.SetActive(enabled);
    this.distance.gameObject.SetActive(enabled);
    this.heading.gameObject.SetActive(enabled);
    this.altitude.gameObject.SetActive(enabled);
    this.rel_altitude.gameObject.SetActive(enabled);
    this.speed.gameObject.SetActive(enabled);
    this.rel_speed.gameObject.SetActive(enabled);
    this.magText.gameObject.SetActive(enabled);
    this.modeText.gameObject.SetActive(enabled);
    this.gridText.gameObject.SetActive(enabled);
    this.bearingText.gameObject.SetActive(enabled);
    this.bearingImg.gameObject.SetActive(enabled);
    this.aimingBoxBgd.gameObject.SetActive(enabled);
    this.aimingDotImg.gameObject.SetActive(enabled);
  }

  public void LateUpdate()
  {
    if ((UnityEngine.Object) SceneSingleton<CombatHUD>.i.aircraft == (UnityEngine.Object) null)
      return;
    for (int index = 0; index < this.targetBoxes.Count && index < this.targetList.Count; ++index)
    {
      GlobalPosition knownPosition;
      SceneSingleton<CombatHUD>.i.aircraft.NetworkHQ.TryGetKnownPosition(this.targetList[index], out knownPosition);
      this.targetBoxes[index].gameObject.transform.localPosition = Vector3.Scale(this.cam.WorldToScreenPoint(knownPosition.ToLocalPosition()), this.scaleVector);
    }
    this.gunCCIPDisplay.Update(this.cam, SceneSingleton<CombatHUD>.i.aircraft, this.targetList);
  }

  [Serializable]
  private class GunCCIPDisplay
  {
    [SerializeField]
    private Image crosshair;
    [NonSerialized]
    public bool Active;

    public void Update(Camera cam, Aircraft aircraft, List<Unit> targetList)
    {
      WeaponStation currentWeaponStation = aircraft.weaponManager.currentWeaponStation;
      if (targetList.Count == 0 || currentWeaponStation == null || (double) currentWeaponStation.WeaponInfo.muzzleVelocity == 0.0 || currentWeaponStation.HasTurret())
      {
        this.crosshair.enabled = false;
      }
      else
      {
        Unit target = targetList[0];
        GlobalPosition? impactPoint;
        aircraft.GetControlsFilter().GetAim(target, out GlobalPosition? _, out impactPoint);
        this.crosshair.enabled = impactPoint.HasValue;
        if (!this.crosshair.enabled)
          return;
        this.crosshair.transform.localPosition = Vector3.Scale(cam.WorldToScreenPoint(impactPoint.Value.ToLocalPosition()), new Vector3(1f, 1f, 0.0f));
        this.crosshair.color = FastMath.InRange(impactPoint.Value, target.GlobalPosition(), target.maxRadius) ? Color.green : Color.gray;
      }
    }
  }
}
