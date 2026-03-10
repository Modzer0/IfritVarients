// Decompiled with JetBrains decompiler
// Type: WeaponManager
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Cysharp.Threading.Tasks;
using JamesFrowen.ScriptableVariables;
using NuclearOption.Networking;
using NuclearOption.SavedMission;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

#nullable disable
public class WeaponManager : MonoBehaviour
{
  private static readonly List<WeaponMount> legalWeaponsCache = new List<WeaponMount>();
  public HardpointSet[] hardpointSets;
  [SerializeField]
  private Aircraft aircraft;
  [NonSerialized]
  public WeaponStation currentWeaponStation;
  private List<Renderer> colorables = new List<Renderer>();
  private List<Renderer> skinnables = new List<Renderer>();
  private bool canRearm;
  private List<Unit> targetList = new List<Unit>();
  private LiveryData liveryData;
  private MaterialCleanup materialCleanup;
  private bool gunsLinked;

  public event Action OnWeaponsLoaded;

  private void Awake()
  {
    if (GameManager.gameState == GameState.Encyclopedia)
      return;
    this.aircraft.OnRearm += new Action<RearmEventArgs>(this.WeaponManager_OnRearm);
    this.aircraft.onInitialize += new Action(this.WeaponManager_OnInitialize);
  }

  private void OnDestroy() => this.materialCleanup?.CleanupAll();

  public void UpdateColorables(LiveryData liveryData)
  {
    if ((UnityEngine.Object) liveryData == (UnityEngine.Object) null)
      return;
    this.liveryData = liveryData;
    if (this.materialCleanup == null)
      this.materialCleanup = new MaterialCleanup();
    for (int index = this.colorables.Count - 1; index >= 0; --index)
    {
      if (!((UnityEngine.Object) this.colorables[index] == (UnityEngine.Object) null))
      {
        Material material = this.colorables[index].material;
        this.materialCleanup.Add(material);
        LiveryData.TextureColor[] colors = liveryData.Colors;
        if ((colors != null ? (colors.Length != 0 ? 1 : 0) : 0) != 0)
          material.SetColor("_Color", (Color) liveryData.Colors[0].Color);
      }
    }
    foreach (Renderer skinnable in this.skinnables)
    {
      Material material = skinnable.material;
      this.materialCleanup.Add(material);
      material.SetTexture("_Livery", (Texture) liveryData.Texture);
      material.SetFloat("_Glossiness", liveryData.Glossiness);
    }
  }

  public Loadout SelectWeapons(bool preferNukes)
  {
    Loadout loadout = new Loadout();
    int warheadsAvailable = 0;
    Airbase nearestAirbase = this.aircraft.NetworkHQ.GetNearestAirbase(this.aircraft.transform.position);
    if ((UnityEngine.Object) nearestAirbase != (UnityEngine.Object) null)
      warheadsAvailable = Mathf.Min(nearestAirbase.GetWarheads(), this.aircraft.NetworkHQ.GetWarheadStockpile() - this.aircraft.NetworkHQ.warheadsReserve);
    if (!MissionManager.AllowTactical())
      warheadsAvailable = 0;
    foreach (HardpointSet hardpointSet in this.hardpointSets)
    {
      WeaponChecker.GetAvailableWeaponsNonAlloc(nearestAirbase, new int?(), hardpointSet, this.aircraft.NetworkHQ, new int?(warheadsAvailable), true, WeaponManager.legalWeaponsCache);
      if (preferNukes)
        WeaponChecker.PreferNukesFilter(warheadsAvailable, hardpointSet, WeaponManager.legalWeaponsCache);
      WeaponMount weaponMount = WeaponManager.legalWeaponsCache.RandomItem<WeaponMount>();
      if ((UnityEngine.Object) weaponMount != (UnityEngine.Object) null && (UnityEngine.Object) weaponMount.info != (UnityEngine.Object) null && weaponMount.info.nuclear)
        warheadsAvailable -= weaponMount.ammo * hardpointSet.hardpoints.Count;
      loadout.weapons.Add(weaponMount);
    }
    return loadout;
  }

  public Loadout GetCurrentLoadout() => this.aircraft.loadout;

  public float GetCurrentValue(bool includeCargo)
  {
    float currentValue = 0.0f;
    foreach (HardpointSet hardpointSet in this.hardpointSets)
    {
      foreach (Hardpoint hardpoint in hardpointSet.hardpoints)
      {
        if (!((UnityEngine.Object) hardpointSet.weaponMount == (UnityEngine.Object) null) && (!((UnityEngine.Object) hardpointSet.weaponMount.info != (UnityEngine.Object) null) || !hardpointSet.weaponMount.info.cargo || includeCargo))
          currentValue += hardpointSet.weaponMount.emptyCost;
      }
    }
    foreach (WeaponStation weaponStation in this.aircraft.weaponStations)
    {
      if (includeCargo || !weaponStation.WeaponInfo.cargo)
        currentValue += (float) weaponStation.Ammo * weaponStation.WeaponInfo.costPerRound;
    }
    return currentValue;
  }

  public int GetCurrentWarheads()
  {
    int currentWarheads = 0;
    foreach (WeaponStation weaponStation in this.aircraft.weaponStations)
    {
      if (weaponStation.WeaponInfo.nuclear)
        currentWarheads += weaponStation.Ammo;
    }
    return currentWarheads;
  }

  public float GetCurrentMass()
  {
    float currentMass = 0.0f;
    foreach (HardpointSet hardpointSet in this.hardpointSets)
    {
      foreach (Hardpoint hardpoint in hardpointSet.hardpoints)
        currentMass += (UnityEngine.Object) hardpointSet.weaponMount == (UnityEngine.Object) null ? 0.0f : hardpointSet.weaponMount.emptyMass;
    }
    foreach (WeaponStation weaponStation in this.aircraft.weaponStations)
    {
      if (weaponStation.Cargo)
      {
        foreach (Weapon weapon in weaponStation.Weapons)
          currentMass += weapon.GetMass();
      }
      else
        currentMass += (float) weaponStation.Ammo * weaponStation.WeaponInfo.massPerRound;
    }
    return currentMass;
  }

  public void ClearTargetList()
  {
    this.targetList.Clear();
    this.TargetListChanged();
  }

  public void AddTargetList(Unit target)
  {
    if ((UnityEngine.Object) target != (UnityEngine.Object) null)
      this.targetList.Insert(0, target);
    this.TargetListChanged();
  }

  public void RemoveTargetList(Unit target)
  {
    this.targetList.Remove(target);
    this.TargetListChanged();
  }

  public List<Unit> GetTargetList() => this.targetList;

  public bool CheckIsTarget(Unit candidate)
  {
    return this.targetList.Count > 0 && this.targetList.Contains(candidate);
  }

  public void TargetListChanged()
  {
    if (!this.aircraft.networked)
      return;
    Span<PersistentID> span = stackalloc PersistentID[this.targetList.Count];
    for (int index = 0; index < this.targetList.Count; ++index)
      span[index] = this.targetList[index].persistentID;
    if (this.aircraft.weaponStations.Count <= 0)
      return;
    this.aircraft.SetStationTargets(this.currentWeaponStation.Number, Span<PersistentID>.op_Implicit(span));
  }

  public void SetTargetList(ReadOnlySpan<PersistentID> targetIDs)
  {
    this.targetList.Clear();
    ReadOnlySpan<PersistentID> readOnlySpan = targetIDs;
    for (int index = 0; index < readOnlySpan.Length; ++index)
    {
      Unit unit;
      if (UnitRegistry.TryGetUnit(new PersistentID?(readOnlySpan[index]), out unit))
        this.targetList.Add(unit);
    }
  }

  public void SetActiveStation(byte stationIndex)
  {
    foreach (WeaponStation weaponStation in this.aircraft.weaponStations)
      weaponStation.SetStationActive(this.aircraft, (int) weaponStation.Number == (int) stationIndex);
    this.currentWeaponStation = this.aircraft.weaponStations[(int) stationIndex];
    Span<PersistentID> span = stackalloc PersistentID[this.targetList.Count];
    for (int index = 0; index < this.targetList.Count; ++index)
      span[index] = this.targetList[index].persistentID;
    this.currentWeaponStation.SetStationTargets(Span<PersistentID>.op_Implicit(span));
  }

  private void WeaponManager_OnInitialize()
  {
    this.aircraft.onInitialize -= new Action(this.WeaponManager_OnInitialize);
    if (GameManager.gameState == GameState.Encyclopedia)
      return;
    this.InitializeWeaponManager();
  }

  public void WeaponManager_OnLoadoutChanged() => this.SpawnWeapons();

  public void InitializeWeaponManager()
  {
    if (this.aircraft.loadout == null || this.aircraft.loadout.weapons.Count != this.hardpointSets.Length)
      this.aircraft.Networkloadout = this.aircraft.definition.aircraftParameters.loadouts[1];
    this.aircraft.onLoadoutChanged += new Action(this.WeaponManager_OnLoadoutChanged);
    this.SpawnWeapons();
  }

  public void RemoveWeapons()
  {
    foreach (HardpointSet hardpointSet in this.hardpointSets)
      hardpointSet.RemoveMounts();
    if (NetworkManagerNuclearOption.i.Server.Active)
      this.aircraft.ClearWeaponStates();
    this.aircraft.ClearWeaponStations();
  }

  public void ReturnWeapons()
  {
    if (this.aircraft.IsServer && this.aircraft.networked)
    {
      int currentWarheads = this.GetCurrentWarheads();
      float currentValue = this.GetCurrentValue(true);
      if ((UnityEngine.Object) this.aircraft.Player != (UnityEngine.Object) null)
        this.aircraft.Player.AddAllocation(currentValue);
      if ((UnityEngine.Object) this.aircraft.NetworkHQ != (UnityEngine.Object) null)
      {
        Airbase nearestAirbase = this.aircraft.NetworkHQ.GetNearestAirbase(this.aircraft.transform.position);
        if (currentWarheads != 0 && (UnityEngine.Object) nearestAirbase != (UnityEngine.Object) null)
          nearestAirbase.AddWarheads(currentWarheads);
      }
    }
    this.RemoveWeapons();
  }

  public void SpawnWeapons()
  {
    float num = 0.0f;
    int currentWarheads = this.GetCurrentWarheads();
    if (this.aircraft.IsServer && this.aircraft.networked && (UnityEngine.Object) this.aircraft.Player != (UnityEngine.Object) null)
      num = this.GetCurrentValue(true);
    if (this.currentWeaponStation != null)
    {
      int number1 = (int) this.currentWeaponStation.Number;
    }
    this.RemoveWeapons();
    this.colorables.Clear();
    this.skinnables.Clear();
    for (int index = 0; index < this.hardpointSets.Length; ++index)
    {
      if (index < this.aircraft.loadout.weapons.Count)
        this.LoadHardpointSet(this.hardpointSets[index], this.aircraft.loadout.weapons[index]);
    }
    this.OrganizeWeaponStations();
    if (this.aircraft.IsServer && this.aircraft.networked)
    {
      if ((UnityEngine.Object) this.aircraft.Player != (UnityEngine.Object) null)
        this.aircraft.Player.AddAllocation(-(this.GetCurrentValue(true) - num));
      if ((UnityEngine.Object) this.aircraft.NetworkHQ != (UnityEngine.Object) null)
      {
        int number2 = this.GetCurrentWarheads() - currentWarheads;
        Airbase nearestAirbase = this.aircraft.NetworkHQ.GetNearestAirbase(this.aircraft.transform.position);
        if (number2 != 0 && (UnityEngine.Object) nearestAirbase != (UnityEngine.Object) null)
        {
          if (number2 > 0)
            nearestAirbase.RemoveWarheads(number2);
          else
            nearestAirbase.AddWarheads(-number2);
        }
      }
    }
    Action onWeaponsLoaded = this.OnWeaponsLoaded;
    if (onWeaponsLoaded != null)
      onWeaponsLoaded();
    if (!PlayerSettings.debugVis)
      return;
    this.aircraft.DebugCoM();
  }

  public void RegisterWeapon(Weapon weapon, WeaponMount weaponMount, Hardpoint hardpoint)
  {
    WeaponStation existingStation;
    if (!this.StationExistsForWeaponInfo(weapon.info, weaponMount.Cargo, out existingStation))
    {
      existingStation = new WeaponStation((Unit) this.aircraft, weaponMount.Cargo, weaponMount.GearSafety, weaponMount.GroundSafety, weaponMount.sortWeapons);
      this.aircraft.RegisterWeaponStation(existingStation);
    }
    existingStation.RegisterWeapon(weapon, this.aircraft, weaponMount, hardpoint);
  }

  private bool StationExistsForWeaponInfo(
    WeaponInfo weaponInfo,
    bool cargo,
    out WeaponStation existingStation)
  {
    existingStation = (WeaponStation) null;
    foreach (WeaponStation weaponStation in this.aircraft.weaponStations)
    {
      if (cargo && weaponStation.Cargo)
      {
        existingStation = weaponStation;
        return true;
      }
      if ((UnityEngine.Object) weaponInfo != (UnityEngine.Object) null && (UnityEngine.Object) weaponStation.WeaponInfo == (UnityEngine.Object) weaponInfo)
      {
        existingStation = weaponStation;
        return true;
      }
    }
    return false;
  }

  private void LoadHardpointSet(HardpointSet hardpointSet, WeaponMount weaponMount)
  {
    if ((UnityEngine.Object) weaponMount == (UnityEngine.Object) null || (UnityEngine.Object) this.aircraft.NetworkHQ != (UnityEngine.Object) null && this.aircraft.NetworkHQ.restrictedWeapons.Contains(weaponMount.name))
      hardpointSet.RemoveMounts();
    else
      hardpointSet.SpawnMounts(this.aircraft, weaponMount);
  }

  private void OrganizeWeaponStations()
  {
    this.UpdateColorables(this.liveryData);
    for (int index = this.aircraft.weaponStations.Count - 1; index >= 0; --index)
    {
      this.aircraft.weaponStations[index].TypeLookup = new Dictionary<UnitDefinition, OpportunityThreat>();
      this.aircraft.weaponStations[index].Number = (byte) index;
      this.aircraft.weaponStations[index].SortWeapons(this.aircraft);
      this.aircraft.weaponStations[index].AccountAmmo();
    }
    if (this.aircraft.weaponStations.Count > 0)
    {
      this.currentWeaponStation = this.aircraft.weaponStations[0];
      this.currentWeaponStation.SetStationActive(this.aircraft, true);
      this.TargetListChanged();
    }
    else
      this.currentWeaponStation = (WeaponStation) null;
    if (!((UnityEngine.Object) SceneSingleton<CombatHUD>.i.aircraft == (UnityEngine.Object) this.aircraft))
      return;
    SceneSingleton<CombatHUD>.i.ShowWeaponStation(this.currentWeaponStation);
  }

  public void RegisterColorable(Renderer renderer) => this.colorables.Add(renderer);

  public void RegisterSkinnable(Renderer renderer) => this.skinnables.Add(renderer);

  public int StationsWithTurrets()
  {
    int num = 0;
    foreach (WeaponStation weaponStation in this.aircraft.weaponStations)
    {
      if (weaponStation.HasTurret())
        ++num;
    }
    return num;
  }

  public bool HasTailHook()
  {
    foreach (HardpointSet hardpointSet in this.hardpointSets)
    {
      if ((UnityEngine.Object) hardpointSet.weaponMount != (UnityEngine.Object) null && hardpointSet.weaponMount.tailHook)
        return true;
    }
    return false;
  }

  public void SetCanRearm() => this.canRearm = true;

  private void WeaponManager_OnRearm(RearmEventArgs e)
  {
    if (!this.canRearm || this.aircraft.weaponStations.Count == 0)
      return;
    if ((UnityEngine.Object) this.aircraft.Player != (UnityEngine.Object) null && this.aircraft.networked)
    {
      float num = WeaponChecker.GetLoadoutFullValue(this.aircraft.weaponManager, false) - this.GetCurrentValue(false);
      if ((double) num > (double) this.aircraft.Player.Allocation)
      {
        if (!((UnityEngine.Object) SceneSingleton<CombatHUD>.i.aircraft == (UnityEngine.Object) this.aircraft))
          return;
        SceneSingleton<AircraftActionsReport>.i.ReportText("Insufficient Funds to Rearm", 4f);
        return;
      }
      if (this.aircraft.IsServer)
        this.aircraft.Player.AddAllocation(-num);
    }
    foreach (WeaponStation weaponStation in this.aircraft.weaponStations)
    {
      if (!weaponStation.Cargo)
        weaponStation.Rearm();
    }
    this.canRearm = false;
    if (!((UnityEngine.Object) SceneSingleton<CombatHUD>.i.aircraft == (UnityEngine.Object) this.aircraft))
      return;
    SceneSingleton<AircraftActionsReport>.i.ReportText("Weapons Rearmed by " + e.rearmer.unitName, 5f);
  }

  public void NextWeaponStation()
  {
    if (this.aircraft.weaponStations.Count == 0 || this.aircraft.weaponStations.Count == 1)
      return;
    int num = (int) this.currentWeaponStation.Number < this.aircraft.weaponStations.Count - 1 ? (int) this.currentWeaponStation.Number + 1 : 0;
    Span<PersistentID> span = stackalloc PersistentID[this.targetList.Count];
    for (int index = 0; index < this.targetList.Count; ++index)
      span[index] = this.targetList[index].persistentID;
    this.currentWeaponStation = this.aircraft.weaponStations[num];
    this.aircraft.SetActiveStation((byte) num);
    SceneSingleton<CombatHUD>.i.ShowWeaponStation(this.currentWeaponStation);
  }

  public void PreviousWeaponStation()
  {
    if (this.aircraft.weaponStations.Count == 0 || this.aircraft.weaponStations.Count == 1)
      return;
    int num = this.currentWeaponStation.Number > (byte) 0 ? (int) this.currentWeaponStation.Number - 1 : this.aircraft.weaponStations.Count - 1;
    Span<PersistentID> span = stackalloc PersistentID[this.targetList.Count];
    for (int index = 0; index < this.targetList.Count; ++index)
      span[index] = this.targetList[index].persistentID;
    this.currentWeaponStation = this.aircraft.weaponStations[num];
    this.aircraft.SetActiveStation((byte) num);
    SceneSingleton<CombatHUD>.i.ShowWeaponStation(this.currentWeaponStation);
  }

  public void FireGuns()
  {
    Unit target = this.targetList.Count == 0 ? (Unit) null : this.targetList[0];
    foreach (WeaponStation weaponStation in this.aircraft.weaponStations)
    {
      if (weaponStation.WeaponInfo.gun && weaponStation.TurretCount() == 0 && weaponStation.Ready())
        weaponStation.Fire((Unit) this.aircraft, target);
    }
  }

  public void Fire()
  {
    if (this.currentWeaponStation == null || this.currentWeaponStation.SafetyIsOn(this.aircraft) || this.aircraft.weaponStations.Count == 0)
      return;
    if (this.currentWeaponStation.WeaponInfo.gun && this.gunsLinked)
    {
      this.FireGuns();
    }
    else
    {
      Unit target = this.targetList.Count == 0 ? (Unit) null : this.targetList[0];
      if (this.aircraft.remoteSim || !this.currentWeaponStation.Ready() || this.currentWeaponStation.SalvoInProgress)
        return;
      if (this.currentWeaponStation.WeaponInfo.gun || (double) this.currentWeaponStation.WeaponInfo.fireInterval == 0.0 || this.currentWeaponStation.WeaponInfo.sling)
        this.currentWeaponStation.Fire((Unit) this.aircraft, target);
      else if (this.targetList.Count > 1)
      {
        this.currentWeaponStation.SalvoInProgress = true;
        this.SalvoFire(this.targetList, this.currentWeaponStation.WeaponInfo.fireInterval * 1.1f).Forget();
      }
      else
        this.currentWeaponStation.LaunchMount((Unit) this.aircraft, target, this.aircraft.GlobalPosition() + this.aircraft.transform.forward * 50000f);
    }
  }

  private async UniTask SalvoFire(List<Unit> targets, float salvoInterval)
  {
    WeaponManager weaponManager = this;
    int i = 0;
    WeaponStation salvoStation = weaponManager.currentWeaponStation;
    CancellationToken cancel = weaponManager.destroyCancellationToken;
    while (i < targets.Count)
    {
      Unit target = targets[i];
      if ((UnityEngine.Object) target != (UnityEngine.Object) null && !target.disabled)
        salvoStation.LaunchMount((Unit) weaponManager.aircraft, targets[i], new GlobalPosition());
      ++i;
      await UniTask.Delay((int) ((double) salvoInterval * 1000.0));
      if (cancel.IsCancellationRequested)
      {
        salvoStation = (WeaponStation) null;
        cancel = new CancellationToken();
        return;
      }
    }
    salvoStation.SalvoInProgress = false;
    salvoStation = (WeaponStation) null;
    cancel = new CancellationToken();
  }

  public void ToggleGunsLinked()
  {
    this.gunsLinked = !this.gunsLinked;
    string report = this.gunsLinked ? "All Guns <b>Linked</b>" : "Guns <b>Split</b>";
    SceneSingleton<AircraftActionsReport>.i.ReportText(report, 5f);
  }

  public void SetGunsLinked(bool gunsLinked) => this.gunsLinked = gunsLinked;

  public bool HasMultipleGuns()
  {
    int num = 0;
    foreach (WeaponStation weaponStation in this.aircraft.weaponStations)
    {
      if (weaponStation.WeaponInfo.gun && weaponStation.TurretCount() == 0)
        ++num;
    }
    return num > 1;
  }
}
