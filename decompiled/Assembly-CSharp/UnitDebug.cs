// Decompiled with JetBrains decompiler
// Type: UnitDebug
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

#nullable disable
public class UnitDebug : MonoBehaviour
{
  private Unit followingUnit;
  private TrackingInfo trackingInfo;
  [SerializeField]
  private TMP_Text unitName;
  [SerializeField]
  private TMP_Text position;
  [SerializeField]
  private TMP_Text speed;
  [SerializeField]
  private TMP_Text radarAlt;
  [SerializeField]
  private TMP_Text target;
  [SerializeField]
  private TMP_Text missileAttacks;
  [SerializeField]
  private TMP_Text state;
  [SerializeField]
  private TMP_Text weapon;
  [SerializeField]
  private TMP_Text gForce;
  [SerializeField]
  private TMP_Text lookAt;
  [SerializeField]
  private GameObject mainPanel;
  [SerializeField]
  private GameObject speedPanel;
  [SerializeField]
  private GameObject targetPanel;
  [SerializeField]
  private GameObject statePanel;
  [SerializeField]
  private GameObject weaponPanel;
  [SerializeField]
  private GameObject gForcePanel;
  [SerializeField]
  private GameObject weaponStationsPanel;
  [SerializeField]
  private GameObject lookAtPanel;
  private Vector3 velPrev;
  private Turret turret;
  private List<Unit> targetList;
  private WeaponManager weaponManager;
  private Pilot pilot;
  private ShipAI shipAI;
  private List<WeaponInfo> weaponInfoToShow = new List<WeaponInfo>();
  [SerializeField]
  private UnitDebug.WeaponStationDebug[] weaponStationDisplays;

  private void Awake()
  {
    this.enabled = false;
    this.gameObject.SetActive(false);
    if (GameManager.gameState == GameState.Encyclopedia)
      return;
    CameraStateManager.onFollowingUnitSet += new Action<Unit>(this.UnitDebug_OnFollowingUnitSet);
  }

  private void OnDestroy()
  {
    CameraStateManager.onFollowingUnitSet -= new Action<Unit>(this.UnitDebug_OnFollowingUnitSet);
  }

  private void UnitDebug_OnFollowingUnitSet(Unit unit)
  {
    this.velPrev = Vector3.zero;
    if ((UnityEngine.Object) unit == (UnityEngine.Object) null || (UnityEngine.Object) unit == (UnityEngine.Object) SceneSingleton<CombatHUD>.i.aircraft)
    {
      this.followingUnit = (Unit) null;
      this.enabled = false;
      this.gameObject.SetActive(false);
    }
    else
    {
      this.UpdateWeaponDisplay(unit);
      this.followingUnit = unit;
      this.gameObject.SetActive(true);
      this.enabled = true;
      this.unitName.text = this.followingUnit.unitName;
      this.trackingInfo = (TrackingInfo) null;
      this.targetList = (List<Unit>) null;
      this.turret = (Turret) null;
      this.weaponManager = (WeaponManager) null;
      this.pilot = (Pilot) null;
      if ((UnityEngine.Object) unit.NetworkHQ != (UnityEngine.Object) null)
      {
        foreach (FactionHQ allHq in FactionRegistry.GetAllHQs())
        {
          if ((UnityEngine.Object) allHq != (UnityEngine.Object) unit.NetworkHQ)
            this.trackingInfo = allHq.GetTrackingData(unit.persistentID);
        }
        this.turret = unit.gameObject.GetComponentInChildren<Turret>();
      }
      if (this.followingUnit is Aircraft followingUnit1)
      {
        this.turret = (Turret) null;
        this.speedPanel.SetActive(true);
        this.targetPanel.SetActive(true);
        this.statePanel.SetActive(true);
        this.gForcePanel.SetActive(true);
        this.lookAtPanel.SetActive(true);
        this.lookAt.text = SceneSingleton<CameraStateManager>.i.orbitState.GetCurrentFocus();
        this.targetList = followingUnit1.weaponManager.GetTargetList();
        this.weaponManager = followingUnit1.weaponManager;
        if (followingUnit1.pilots.Length != 0 && (UnityEngine.Object) followingUnit1.pilots[0] != (UnityEngine.Object) null)
          this.pilot = followingUnit1.pilots[0];
      }
      if (this.followingUnit is Building)
      {
        this.speedPanel.SetActive(false);
        this.targetPanel.SetActive(false);
        this.statePanel.SetActive(false);
        this.gForcePanel.SetActive(false);
        this.lookAtPanel.SetActive(false);
      }
      if (this.followingUnit is GroundVehicle)
      {
        this.speedPanel.SetActive(true);
        this.targetPanel.SetActive(true);
        this.statePanel.SetActive(false);
        this.gForcePanel.SetActive(false);
        this.lookAtPanel.SetActive(true);
        this.lookAt.text = SceneSingleton<CameraStateManager>.i.orbitState.GetCurrentFocus();
      }
      if (this.followingUnit is Missile)
      {
        this.speedPanel.SetActive(true);
        this.targetPanel.SetActive(true);
        this.statePanel.SetActive(false);
        this.gForcePanel.SetActive(true);
        this.lookAtPanel.SetActive(true);
        this.lookAt.text = SceneSingleton<CameraStateManager>.i.orbitState.GetCurrentFocus();
      }
      if (this.followingUnit is Ship followingUnit2)
      {
        this.speedPanel.SetActive(true);
        this.targetPanel.SetActive(true);
        this.statePanel.SetActive(true);
        this.gForcePanel.SetActive(false);
        this.lookAtPanel.SetActive(true);
        this.lookAt.text = SceneSingleton<CameraStateManager>.i.orbitState.GetCurrentFocus();
        this.shipAI = followingUnit2.GetComponent<ShipAI>();
      }
      if (!(this.followingUnit is PilotDismounted))
        return;
      this.speedPanel.SetActive(true);
      this.gForcePanel.SetActive(false);
      this.lookAtPanel.SetActive(false);
    }
  }

  private void FixedUpdate()
  {
    if (!((UnityEngine.Object) this.followingUnit != (UnityEngine.Object) null) || !this.gForcePanel.activeSelf || (UnityEngine.Object) this.followingUnit.rb == (UnityEngine.Object) null)
      return;
    if (this.velPrev != Vector3.zero)
      this.gForce.text = $"{(ValueType) (float) ((double) ((this.followingUnit.rb.velocity - this.velPrev) / Time.fixedDeltaTime + Vector3.up * 9.81f).magnitude / 9.8100004196167):F1}g";
    this.velPrev = this.followingUnit.rb.velocity;
  }

  private void UpdateWeaponDisplay(Unit unit)
  {
    this.weaponStationsPanel.SetActive(false);
    foreach (UnitDebug.WeaponStationDebug weaponStationDisplay in this.weaponStationDisplays)
      weaponStationDisplay.Hide();
    this.weaponInfoToShow.Clear();
    foreach (WeaponStation weaponStation in unit.weaponStations)
    {
      if (!this.weaponInfoToShow.Contains(weaponStation.WeaponInfo))
        this.weaponInfoToShow.Add(weaponStation.WeaponInfo);
    }
    Faction localFaction;
    GameManager.GetLocalFaction(out localFaction);
    this.weaponStationsPanel.SetActive((this.weaponInfoToShow.Count > 0 && (UnityEngine.Object) localFaction == (UnityEngine.Object) null || (UnityEngine.Object) unit.NetworkHQ != (UnityEngine.Object) null && (UnityEngine.Object) localFaction == (UnityEngine.Object) unit.NetworkHQ.faction) && !PlayerSettings.cinematicMode);
    this.weaponInfoToShow.Sort((Comparison<WeaponInfo>) ((a, b) => a.costPerRound.CompareTo(b.costPerRound)));
    for (int index = 0; index < this.weaponInfoToShow.Count; ++index)
      this.weaponStationDisplays[index].Show(unit, this.weaponInfoToShow[index]);
  }

  private void Update()
  {
    if ((UnityEngine.Object) this.followingUnit == (UnityEngine.Object) null)
    {
      this.enabled = false;
    }
    else
    {
      if (this.mainPanel.activeSelf)
      {
        if (PlayerSettings.cinematicMode)
        {
          this.mainPanel.SetActive(false);
          this.weaponStationsPanel.SetActive(false);
        }
      }
      else if (!PlayerSettings.cinematicMode)
      {
        this.mainPanel.SetActive(true);
        this.weaponStationsPanel.SetActive(this.followingUnit.weaponStations.Count > 0);
      }
      GlobalPosition globalPosition = this.followingUnit.GlobalPosition();
      this.position.text = $"[{globalPosition.x:F0}, {globalPosition.y:F0}, {globalPosition.z:F0}]";
      this.radarAlt.text = UnitConverter.AltitudeReading(this.followingUnit.radarAlt);
      this.speed.text = UnitConverter.SpeedReading(this.followingUnit.speed);
      float speedOfSound = LevelInfo.GetSpeedOfSound(globalPosition.y);
      if ((double) this.followingUnit.speed > (double) speedOfSound)
        this.speed.text = $"M {(ValueType) (float) ((double) this.followingUnit.speed / (double) speedOfSound):F2}";
      if (this.followingUnit is GroundVehicle)
        this.speed.text = UnitConverter.SpeedReadingGround(this.followingUnit.speed);
      this.missileAttacks.text = this.trackingInfo != null ? $"{this.trackingInfo.missileAttacks}" : "0";
      this.state.text = (UnityEngine.Object) this.pilot != (UnityEngine.Object) null ? this.pilot.GetCurrentState() : "none";
      if ((UnityEngine.Object) this.turret != (UnityEngine.Object) null)
      {
        this.target.text = (UnityEngine.Object) this.turret.GetTarget() != (UnityEngine.Object) null ? this.turret.GetTarget().unitName : "none";
        this.turret.GetWeaponStation();
      }
      else
        this.target.text = this.targetList == null || this.targetList.Count <= 0 ? "none" : this.targetList[0].unitName;
      if (this.followingUnit is Missile followingUnit)
      {
        Unit unit;
        this.target.text = UnitRegistry.TryGetUnit(new PersistentID?(followingUnit.targetID), out unit) ? unit.unitName : "none";
      }
      if (this.followingUnit is Ship)
        this.state.text = $"{this.shipAI.state}";
      if (this.lookAtPanel.activeSelf && SceneSingleton<CameraStateManager>.i.orbitState.GetCurrentFocus() == "current")
        this.lookAtPanel.SetActive(false);
      else if (!this.lookAtPanel.activeSelf && SceneSingleton<CameraStateManager>.i.orbitState.GetCurrentFocus() != "current")
        this.lookAtPanel.SetActive(true);
      if (!this.lookAtPanel.activeSelf)
        return;
      this.lookAt.text = $"Look at {SceneSingleton<CameraStateManager>.i.orbitState.GetCurrentFocus()} : {SceneSingleton<CameraStateManager>.i.orbitState.GetLookAtUnit()}";
    }
  }

  [Serializable]
  public class WeaponStationDebug
  {
    [SerializeField]
    private GameObject panel;
    [SerializeField]
    private TMP_Text text;
    private List<WeaponStation> weaponStations;
    private WeaponInfo weaponInfo;

    public void Hide()
    {
      this.panel.SetActive(false);
      if (this.weaponStations != null)
      {
        foreach (WeaponStation weaponStation in this.weaponStations)
          weaponStation.OnUpdated -= new Action(this.WeaponStationDebug_OnFired);
        this.weaponStations.Clear();
      }
      this.weaponInfo = (WeaponInfo) null;
    }

    public void Show(Unit unit, WeaponInfo weaponInfo)
    {
      this.weaponInfo = weaponInfo;
      if (this.weaponStations != null)
      {
        foreach (WeaponStation weaponStation in this.weaponStations)
          weaponStation.OnUpdated -= new Action(this.WeaponStationDebug_OnFired);
      }
      else
        this.weaponStations = new List<WeaponStation>();
      this.weaponStations.Clear();
      foreach (WeaponStation weaponStation in unit.weaponStations)
      {
        if ((UnityEngine.Object) weaponStation.WeaponInfo == (UnityEngine.Object) weaponInfo)
        {
          this.weaponStations.Add(weaponStation);
          weaponStation.OnUpdated += new Action(this.WeaponStationDebug_OnFired);
        }
      }
      this.panel.SetActive(true);
      this.UpdateText();
    }

    private void UpdateText()
    {
      int num1 = 0;
      int num2 = 0;
      foreach (WeaponStation weaponStation in this.weaponStations)
      {
        num1 += weaponStation.Ammo;
        num2 += weaponStation.FullAmmo;
      }
      this.text.text = $"{this.weaponInfo.weaponName}: {num1} / {num2}";
    }

    private void WeaponStationDebug_OnFired() => this.UpdateText();
  }
}
