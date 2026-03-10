// Decompiled with JetBrains decompiler
// Type: Airbase
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Cysharp.Threading.Tasks;
using Mirage;
using Mirage.RemoteCalls;
using Mirage.Serialization;
using NuclearOption.MissionEditorScripts;
using NuclearOption.Networking;
using NuclearOption.SavedMission;
using NuclearOption.SavedMission.ObjectiveV2;
using NuclearOption.SceneLoading;
using RoadPathfinding;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#nullable disable
[RequireComponent(typeof (Capture))]
public sealed class Airbase : NetworkBehaviour, IEditorSelectable, ICapturable
{
  public const string ATTACHED_AIRBASE_PREFIX = "<UNIT_AIRBASE>++";
  [SerializeField]
  private SavedAirbase airbaseSettings = new SavedAirbase();
  public Transform aircraftSelectionTransform;
  public Transform fixedCameraTransform;
  public Transform center;
  [SerializeField]
  private Transform[] servicePoints;
  public Airbase.Runway[] runways;
  public Airbase.VerticalLandingPoint[] verticalLandingPoints;
  [SerializeField]
  public Building MapTower;
  [NonSerialized]
  private Building tower;
  [SerializeField]
  private Renderer[] runwayLights;
  [SerializeField]
  private RoadNetwork taxiNetwork;
  [SerializeField]
  private Unit attachedUnit;
  public bool AllowTaxiDuringLandings;
  public bool AllowTaxiDuringTakeoffs;
  private float initializeTimer;
  private List<GridSquare> gridSquares;
  private List<AircraftDefinition> availableAircraft = new List<AircraftDefinition>();
  private readonly List<Aircraft> cache = new List<Aircraft>();
  private bool airbaseRegistered;
  [SyncVar(initialOnly = true)]
  private string networkUniqueName;
  private ObjectType objectType;
  [NonSerialized]
  private const int SYNC_VAR_COUNT = 2;
  [NonSerialized]
  private const int RPC_COUNT = 2;

  public Capture capture { get; private set; }

  public List<Aircraft> ControlledAircraft { get; private set; }

  public FactionHQ CurrentHQ { get; private set; }

  public List<Building> buildings { get; private set; } = new List<Building>();

  public List<Hangar> hangars { get; private set; } = new List<Hangar>();

  public List<WarheadStorage> stores { get; private set; } = new List<WarheadStorage>();

  public bool disabled
  {
    get => this.\u003Cdisabled\u003Ek__BackingField;
    private set => this.Network\u003Cdisabled\u003Ek__BackingField = value;
  }

  public SavedAirbase SavedAirbase { get; private set; }

  public bool BuiltIn
  {
    get => this.ObjectType == ObjectType.MapObject || this.objectType == ObjectType.SceneObject;
  }

  public bool AttachedAirbase => this.attachedUnit != null;

  public bool IsCustom => !this.BuiltIn && !this.AttachedAirbase;

  public bool TryGetAttachedUnit(out Unit attachedUnit)
  {
    attachedUnit = this.attachedUnit;
    return this.AttachedAirbase;
  }

  public bool SavedAirbaseOverride => this.SavedAirbase != this.airbaseSettings;

  public event Action onLostControl;

  public event Action onTakeControl;

  public ObjectType ObjectType
  {
    get
    {
      if (this.objectType == ObjectType.NotSet)
        this.objectType = MapLoader.GetObjectType(this.Identity);
      return this.objectType;
    }
  }

  private void OnValidate()
  {
    if (this.airbaseSettings == null)
      this.airbaseSettings = new SavedAirbase();
    if ((UnityEngine.Object) this.attachedUnit != (UnityEngine.Object) null)
      this.airbaseSettings.Capturable = false;
    this.airbaseSettings.Tower = (UnityEngine.Object) this.MapTower != (UnityEngine.Object) null ? this.MapTower.MapUniqueName : "";
    if ((UnityEngine.Object) this.MapTower != (UnityEngine.Object) null)
      this.MapTower.MapAirbase = this;
    this.OnValidateSetCurrentHQ();
  }

  private void OnValidateSetCurrentHQ()
  {
    if (!FactionHelper.EmptyOrNoFactionOrNeutral(this.airbaseSettings.faction))
    {
      if ((UnityEngine.Object) this.CurrentHQ != (UnityEngine.Object) null && this.CurrentHQ.faction.factionName == this.airbaseSettings.faction)
        return;
      FactionHQ factionHq = ((IEnumerable<FactionHQ>) Resources.FindObjectsOfTypeAll<FactionHQ>()).Where<FactionHQ>((Func<FactionHQ, bool>) (x => x.gameObject.scene == this.gameObject.scene)).FirstOrDefault<FactionHQ>((Func<FactionHQ, bool>) (x => x.faction.factionName == this.airbaseSettings.faction));
      if ((UnityEngine.Object) factionHq == (UnityEngine.Object) null)
        Debug.LogError((object) ("Failed to find Hq in scene with name " + this.airbaseSettings.faction));
      this.CurrentHQ = factionHq;
    }
    else
      this.CurrentHQ = (FactionHQ) null;
  }

  public void LinkSavedAirbase(SavedAirbase savedAirbase, bool customAirbase)
  {
    int num = customAirbase ? 1 : 0;
    if (this.BuiltIn)
      savedAirbase.CaptureRange = this.airbaseSettings.CaptureRange;
    this.SavedAirbase = savedAirbase;
    savedAirbase.Airbase = this;
    savedAirbase.CenterWrapper.RegisterOnChange((object) this, (ValueWrapper<GlobalPosition>.OnChangeDelegate) (newPos => this.center.position = newPos.ToLocalPosition()));
    this.AfterLink(savedAirbase);
  }

  private void AfterLink(SavedAirbase savedAirbase)
  {
    if (this.IsClientOnly)
      return;
    if (!this.AttachedAirbase)
    {
      if (GameManager.gameState == GameState.Editor)
        this.EditorSetFaction(savedAirbase.faction, false);
      else
        this.CaptureFaction(savedAirbase.FindHQ());
    }
    this.disabled = savedAirbase.Disabled;
    this.capture.SetCapturable(savedAirbase.Capturable);
  }

  public void UnlinkSavedAirbase()
  {
    if (this.IsCustom || this.AttachedAirbase)
      this.airbaseSettings.UniqueName = this.SavedAirbase.UniqueName;
    this.SavedAirbase.CenterWrapper.UnregisterOnChange((object) this);
    this.SavedAirbase.Airbase = (Airbase) null;
    this.SavedAirbase = this.airbaseSettings;
    this.AfterLink(this.airbaseSettings);
  }

  public bool UnitDestroyed()
  {
    return (UnityEngine.Object) this.attachedUnit != (UnityEngine.Object) null && this.attachedUnit.disabled;
  }

  public void SetDisabled(bool disabled) => this.disabled = disabled;

  private void OnDestroy()
  {
    if (this.airbaseRegistered)
    {
      FactionRegistry.UnregisterAirbase(this);
      this.airbaseRegistered = false;
    }
    if (this.SavedAirbase == this.airbaseSettings)
      return;
    this.SavedAirbase.CenterWrapper.UnregisterOnChange((object) this);
  }

  public RoadNetwork GetTaxiNetwork() => this.taxiNetwork;

  public float GetRadius() => this.SavedAirbase != null ? this.SavedAirbase.CaptureRange : 0.0f;

  public Airbase.Runway.RunwayUsage? GetTakeoffRunway(Aircraft aircraft, float takeoffDist)
  {
    Airbase.Runway runway1 = (Airbase.Runway) null;
    bool flag = false;
    float num = float.MaxValue;
    foreach (Airbase.Runway runway2 in this.runways)
    {
      if (runway2.Takeoff && (double) runway2.Length >= (double) takeoffDist)
      {
        Airbase.Runway.RunwayDistanceResult distance = runway2.GetDistance(aircraft.transform);
        if ((double) distance.Distance < (double) num)
        {
          num = distance.Distance;
          runway1 = runway2;
          flag = distance.Reverse;
        }
      }
    }
    if (runway1 == null)
      return new Airbase.Runway.RunwayUsage?();
    runway1.SetUsageDirection(flag);
    return new Airbase.Runway.RunwayUsage?(new Airbase.Runway.RunwayUsage(runway1, flag));
  }

  public Airbase.Runway.RunwayUsage? RequestLanding(Aircraft aircraft, RunwayQuery runwayQuery)
  {
    Airbase.Runway runway1 = (Airbase.Runway) null;
    bool reverse = false;
    float num = 180f;
    foreach (Airbase.Runway runway2 in this.runways)
    {
      Airbase.Runway.RunwayAngleResult angle = runway2.GetAngle(aircraft.transform);
      if (runway2.Landing && runway2.IsSuitable(runwayQuery) && (double) angle.Angle < (double) num)
      {
        num = angle.Angle;
        runway1 = runway2;
        reverse = angle.Reverse;
      }
    }
    return runway1 != null ? new Airbase.Runway.RunwayUsage?(new Airbase.Runway.RunwayUsage(runway1, reverse)) : new Airbase.Runway.RunwayUsage?();
  }

  public bool RequestTaxi(Aircraft querier)
  {
    bool flag = this.AircraftApproachingRunwayInUse(querier);
    return this.AircraftOnRunwayInUse(querier) || !flag;
  }

  public bool TryRequestVerticalLanding(
    Aircraft aircraft,
    RunwayQuery runwayQuery,
    out Airbase.VerticalLandingPoint landingPoint)
  {
    landingPoint = (Airbase.VerticalLandingPoint) null;
    if (this.verticalLandingPoints.Length == 0)
      return false;
    float num1 = float.MaxValue;
    foreach (Airbase.VerticalLandingPoint verticalLandingPoint in this.verticalLandingPoints)
    {
      if (verticalLandingPoint.IsSuitable(runwayQuery))
      {
        float num2 = verticalLandingPoint.GetAngle(aircraft.transform) * (verticalLandingPoint.IsOccupied(aircraft) ? 100f : 1f);
        if ((double) num2 < (double) num1)
        {
          num1 = num2;
          landingPoint = verticalLandingPoint;
        }
      }
    }
    return landingPoint != null;
  }

  [ServerRpc(requireAuthority = false)]
  public void CmdRegisterUsage(Aircraft aircraft, bool isUsing, byte? landingRunway)
  {
    if (ServerRpcSender.ShouldInvokeLocally((NetworkBehaviour) this, false, false))
    {
      this.UserCode_CmdRegisterUsage_1828364621(aircraft, isUsing, landingRunway);
    }
    else
    {
      PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
      GeneratedNetworkCode._Write_Aircraft((NetworkWriter) writer, aircraft);
      writer.WriteBooleanExtension(isUsing);
      GeneratedNetworkCode._Write_System\u002ENullable\u00601\u003CSystem\u002EByte\u003E((NetworkWriter) writer, landingRunway);
      ServerRpcSender.Send((NetworkBehaviour) this, 0, (NetworkWriter) writer, Mirage.Channel.Reliable, false);
      writer.Release();
    }
  }

  [ClientRpc]
  public void RpcRegisterUsage(Aircraft aircraft, bool isUsing, byte? landingRunway)
  {
    if (ClientRpcSender.ShouldInvokeLocally((NetworkBehaviour) this, RpcTarget.Observers, (INetworkPlayer) null, false))
      this.UserCode_RpcRegisterUsage_2147349378(aircraft, isUsing, landingRunway);
    PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
    GeneratedNetworkCode._Write_Aircraft((NetworkWriter) writer, aircraft);
    writer.WriteBooleanExtension(isUsing);
    GeneratedNetworkCode._Write_System\u002ENullable\u00601\u003CSystem\u002EByte\u003E((NetworkWriter) writer, landingRunway);
    ClientRpcSender.Send((NetworkBehaviour) this, 1, (NetworkWriter) writer, Mirage.Channel.Reliable, false);
    writer.Release();
  }

  public void AddControlledAircraft(Aircraft aircraft)
  {
    if (this.ControlledAircraft.Contains(aircraft))
      return;
    this.ControlledAircraft.Add(aircraft);
  }

  public void RemoveControlledAircraft(Aircraft aircraft)
  {
    this.ControlledAircraft.Remove(aircraft);
  }

  public Airbase.Runway GetLandingRunway()
  {
    foreach (Airbase.Runway runway in this.runways)
    {
      if (runway.Landing)
        return runway;
    }
    return (Airbase.Runway) null;
  }

  public bool TryGetNearestServicePoint(Vector3 fromPosition, out Transform nearestServicePoint)
  {
    if (this.servicePoints.Length == 0)
    {
      nearestServicePoint = (Transform) null;
      return false;
    }
    nearestServicePoint = this.servicePoints[0];
    if (this.servicePoints.Length == 1)
      return true;
    float num1 = float.MaxValue;
    foreach (Transform servicePoint in this.servicePoints)
    {
      float num2 = FastMath.SquareDistance(servicePoint.position, fromPosition);
      if ((double) num2 < (double) num1)
      {
        num1 = num2;
        nearestServicePoint = servicePoint;
      }
    }
    return true;
  }

  public bool AircraftIsOnRunway(
    Aircraft aircraft,
    bool landingRunwaysOnly,
    out Airbase.Runway outRunway)
  {
    Airbase.Runway runway1 = (Airbase.Runway) null;
    float num1 = float.MaxValue;
    foreach (Airbase.Runway runway2 in this.runways)
    {
      if (!landingRunwaysOnly || runway2.Landing)
      {
        float num2 = runway2.AircraftDistanceFromRunway(aircraft);
        if ((double) num2 < (double) runway2.GetWidth() * 0.5 && (double) num2 < (double) num1 && (double) Vector3.Dot(aircraft.transform.position - runway2.Start.position, runway2.GetDirection(false)) >= 0.0 && (double) Vector3.Dot(aircraft.transform.position - runway2.End.position, runway2.GetDirection(true)) >= 0.0)
        {
          runway1 = runway2;
          num1 = num2;
        }
      }
    }
    outRunway = runway1;
    return outRunway != null;
  }

  public bool AircraftApproachingRunwayInUse(Aircraft aircraft)
  {
    foreach (Airbase.Runway runway in this.runways)
    {
      if (runway.OtherAircraftUsingRunway(aircraft) && runway.AircraftApproachingRunway(aircraft, 40f + aircraft.maxRadius))
        return true;
    }
    return false;
  }

  public bool AircraftOnRunwayInUse(Aircraft aircraft)
  {
    foreach (Airbase.Runway runway in this.runways)
    {
      if (runway.OtherAircraftUsingRunway(aircraft) && runway.AircraftOnRunway(aircraft))
        return true;
    }
    return false;
  }

  public bool IsSuitable(RunwayQuery query)
  {
    if (this.disabled)
      return false;
    if (query.RunwayType.QueryFor(RunwayType.LandingOrTakeoff))
    {
      foreach (Airbase.Runway runway in this.runways)
      {
        if (runway.IsSuitable(query))
          return true;
      }
    }
    if (query.RunwayType.QueryFor(RunwayType.Vertical))
    {
      foreach (Airbase.VerticalLandingPoint verticalLandingPoint in this.verticalLandingPoints)
      {
        if (verticalLandingPoint.IsSuitable(query))
          return true;
      }
    }
    return false;
  }

  public void AddBuilding(Building building, bool errorIfAlreadyMember)
  {
    if (this.buildings.Contains(building))
    {
      if (!errorIfAlreadyMember)
        return;
      Debug.LogError((object) $"{building} already part of {this}");
    }
    else
    {
      Hangar component1;
      bool component2 = building.TryGetComponent<Hangar>(out component1);
      WarheadStorage component3;
      bool component4 = building.TryGetComponent<WarheadStorage>(out component3);
      ColorLog<Airbase>.Info($"Adding Building{(!(component2 & component4) ? (!component2 ? (!component4 ? "" : " [storage]") : " [hangar]") : " [hangar, storage]")} ({building.name}, {building.persistentID}) to {this.name}");
      this.buildings.Add(building);
      if (NetworkManagerNuclearOption.i.Server.Active)
        building.NetworkHQ = this.CurrentHQ;
      if (component2)
        this.AddHangar(component1);
      if (component4)
        this.AddStorage(component3);
      if (!(building.UniqueName == this.SavedAirbase.Tower))
        return;
      this.tower = building;
    }
  }

  public void RemoveBuilding(Building building) => this.buildings.Remove(building);

  public void AddHangar(Hangar hangar)
  {
    if (this.hangars.Contains(hangar))
    {
      Debug.LogError((object) $"{hangar} already part of {this}");
    }
    else
    {
      this.hangars.Add(hangar);
      this.SortHangarsByPriority();
    }
  }

  internal void RemoveHangar(Hangar hangar)
  {
    this.hangars.Remove(hangar);
    if (!this.BuiltIn || !(hangar.attachedUnit is Building attachedUnit))
      return;
    this.RemoveBuilding(attachedUnit);
  }

  public void AddStorage(WarheadStorage storage)
  {
    if (this.stores.Contains(storage))
      Debug.LogError((object) $"{storage} already part of {this}");
    else
      this.stores.Add(storage);
  }

  internal void RemoveStorage(WarheadStorage storage)
  {
    this.stores.Remove(storage);
    if (!this.BuiltIn || !(storage.attachedUnit is Building attachedUnit))
      return;
    this.RemoveBuilding(attachedUnit);
  }

  public bool HasStorage() => this.stores.Count > 0;

  public int GetStorage() => this.stores.Count;

  public int GetWarheads()
  {
    if (!this.HasStorage())
      return 0;
    int warheads = 0;
    for (int index = 0; index < this.stores.Count; ++index)
    {
      if (!this.stores[index].Disabled)
        warheads += this.stores[index].number;
    }
    return warheads;
  }

  public void AddWarheads(int number)
  {
    if (!this.HasStorage())
    {
      this.CurrentHQ.AddWarheadStockpile(number);
    }
    else
    {
      while (number > 0)
      {
        int num = int.MaxValue;
        int index1 = 0;
        for (int index2 = 0; index2 < this.stores.Count; ++index2)
        {
          if (!this.stores[index2].Disabled && this.stores[index2].number < num)
          {
            num = this.stores[index2].number;
            index1 = index2;
          }
        }
        this.stores[index1].AddWarhead(1);
        --number;
        if (number == 0)
          break;
      }
    }
  }

  public void RemoveWarheads(int number)
  {
    if (!this.HasStorage())
      return;
    while (number > 0)
    {
      int num = 0;
      int index1 = 0;
      for (int index2 = 0; index2 < this.stores.Count; ++index2)
      {
        if (!this.stores[index2].Disabled && this.stores[index2].number > num)
        {
          num = this.stores[index2].number;
          index1 = index2;
        }
      }
      this.stores[index1].RemoveWarhead(1);
      --number;
      if (number == 0)
        break;
    }
  }

  public bool HasVehicleDepot()
  {
    for (int index = 0; index < this.buildings.Count; ++index)
    {
      if (this.buildings[index] is VehicleDepot)
        return true;
    }
    return false;
  }

  public void EditorSetFaction(string factionName, bool setAttachedAirbaseFaction)
  {
    FactionHQ hq = FactionRegistry.HqFromName(factionName);
    int num = setAttachedAirbaseFaction ? 1 : 0;
    this.CurrentHQ = hq;
    foreach (Building building in this.buildings)
    {
      if ((UnityEngine.Object) building != (UnityEngine.Object) null)
        building.NetworkHQ = hq;
    }
    Color colorOrGray = hq.GetColorOrGray();
    AirbaseEditorFlag airbaseEditorFlag = AirbaseEditorFlag.Find(this.center);
    if ((UnityEngine.Object) airbaseEditorFlag != (UnityEngine.Object) null)
      airbaseEditorFlag.SetColor(colorOrGray);
    AirbaseEditorRadius airbaseEditorRadius = AirbaseEditorRadius.Find(this.center);
    if (!((UnityEngine.Object) airbaseEditorRadius != (UnityEngine.Object) null))
      return;
    airbaseEditorRadius.Setup(colorOrGray, this.GetRadius());
  }

  public void SetFactionWithoutEvent(FactionHQ capturingHQ, bool runAssets = true)
  {
    if (runAssets)
    {
      int num1 = this.IsServer ? 1 : 0;
      int num2 = this.AttachedAirbase ? 1 : 0;
    }
    this.CurrentHQ = capturingHQ;
  }

  private void CaptureFaction(FactionHQ newHQ)
  {
    if ((UnityEngine.Object) this.CurrentHQ == (UnityEngine.Object) newHQ)
    {
      $"{this} already part of '{this.CurrentHQ}'";
      int gameState = (int) GameManager.gameState;
    }
    FactionHQ currentHq = this.CurrentHQ;
    this.CurrentHQ = newHQ;
    if ((UnityEngine.Object) currentHq != (UnityEngine.Object) newHQ)
    {
      if ((UnityEngine.Object) currentHq != (UnityEngine.Object) null)
      {
        currentHq.RemoveAirbase(this);
        Action onLostControl = this.onLostControl;
        if (onLostControl != null)
          onLostControl();
      }
      if ((UnityEngine.Object) newHQ != (UnityEngine.Object) null)
      {
        newHQ.AddAirbase(this);
        Action onTakeControl = this.onTakeControl;
        if (onTakeControl != null)
          onTakeControl();
      }
    }
    if ((UnityEngine.Object) newHQ == (UnityEngine.Object) null)
      return;
    foreach (Building building in this.buildings)
    {
      if ((UnityEngine.Object) building != (UnityEngine.Object) null)
        building.NetworkHQ = newHQ;
    }
    this.WaitRepair().Forget();
  }

  private async UniTask WaitRepair()
  {
    await UniTask.Delay(5000);
    foreach (Building building in this.buildings)
    {
      if ((UnityEngine.Object) building != (UnityEngine.Object) null && building.disabled && building.IsRepairable())
        building.Networkdisabled = false;
    }
    if (!((UnityEngine.Object) this.tower != (UnityEngine.Object) null) || !this.tower.IsRepairable())
      return;
    foreach (Renderer runwayLight in this.runwayLights)
      runwayLight.enabled = true;
  }

  private void Update()
  {
    if (!NetworkManagerNuclearOption.i.Server.Active)
      return;
    this.initializeTimer += Time.unscaledDeltaTime;
    if (this.gridSquares != null || (double) this.initializeTimer <= 1.0)
      return;
    this.gridSquares = BattlefieldGrid.GetGridSquaresInRange(this.center.transform.GlobalPosition(), 1000f);
  }

  private void ControlAircraft()
  {
    this.cache.Clear();
    this.cache.AddRange((IEnumerable<Aircraft>) this.ControlledAircraft);
    for (int index = this.cache.Count - 1; index >= 0; --index)
    {
      Aircraft aircraft = this.cache[index];
      if ((UnityEngine.Object) aircraft == (UnityEngine.Object) null || !FastMath.InRange(aircraft.transform.position, this.center.position, 5000f))
        this.RpcRegisterUsage(aircraft, false, new byte?());
    }
    foreach (Airbase.Runway runway in this.runways)
      runway.MonitorLandings(this);
  }

  private void Awake()
  {
    this.capture = this.GetComponent<Capture>();
    this.ControlledAircraft = new List<Aircraft>();
    for (int index = 0; index < this.runways.Length; ++index)
      this.runways[index].Setup(this, index);
    this.airbaseSettings.Center = this.center.GlobalPosition();
    if ((UnityEngine.Object) this.aircraftSelectionTransform != (UnityEngine.Object) null)
      this.airbaseSettings.SelectionPosition = this.aircraftSelectionTransform.GlobalPosition();
    this.SavedAirbase = this.airbaseSettings;
    this.Identity.OnStartServer.AddListener(new Action(this.OnStartServer));
    this.Identity.OnStartClient.AddListener(new Action(this.OnStartClientOnly));
    this.Identity.OnStopServer.AddListener(new Action(this.OnStopServer));
  }

  public void SetupCustomAirbase(SavedAirbase saved)
  {
    this.LinkSavedAirbase(saved, true);
    this.transform.position = saved.Center.ToLocalPosition();
    this.center.localPosition = Vector3.zero;
    if ((UnityEngine.Object) this.aircraftSelectionTransform != (UnityEngine.Object) null)
      this.aircraftSelectionTransform.position = saved.SelectionPosition.ToLocalPosition();
    this.verticalLandingPoints = new Airbase.VerticalLandingPoint[saved.VerticalLandingPoints.Count];
    for (int index = 0; index < saved.VerticalLandingPoints.Count; ++index)
      this.verticalLandingPoints[index] = Airbase.VerticalLandingPoint.FromSaved(this, saved.VerticalLandingPoints[index]);
    this.servicePoints = new Transform[saved.ServicePoints.Count];
    for (int index = 0; index < saved.ServicePoints.Count; ++index)
      this.servicePoints[index] = new GameObject($"ServicePoints {index}")
      {
        transform = {
          parent = this.transform,
          position = saved.ServicePoints[index].ToLocalPosition()
        }
      }.transform;
    List<SavedRunway> runways = saved.runways;
    // ISSUE: explicit non-virtual call
    int count = runways != null ? __nonvirtual (runways.Count) : 0;
    this.runways = new Airbase.Runway[count];
    for (int index = 0; index < count; ++index)
    {
      this.runways[index] = Airbase.Runway.FromSaved(this, saved.runways[index]);
      this.runways[index].Setup(this, index);
    }
    this.taxiNetwork = saved.roads;
  }

  public void SetupAttachedAirbase(Unit unit)
  {
    string uniqueName = "<UNIT_AIRBASE>++" + unit.UniqueName;
    this.SavedAirbase.UniqueName = uniqueName;
    this.CurrentHQ = unit.NetworkHQ;
    this.LinkSavedAirbase(MissionManager.CurrentMission.airbases.FirstOrDefault<SavedAirbase>((Func<SavedAirbase, bool>) (x => x.UniqueName == uniqueName)) ?? this.SavedAirbase, false);
  }

  private void FindAttachedHangars()
  {
    foreach (Hangar componentsInChild in this.attachedUnit.GetComponentsInChildren<Hangar>())
      this.AddHangar(componentsInChild);
  }

  private void OnStartClientOnly()
  {
    if (this.IsServer)
      return;
    ColorLog<Airbase>.Info($"Airbase spawned on client NetId={this.NetId} UniqueName={this.networkUniqueName}");
    FactionRegistry.RegisterAirbase(this.networkUniqueName, this);
    this.airbaseRegistered = true;
    FactionHQ capturingHQ = (FactionHQ) null;
    foreach (FactionHQ factionHq in FactionRegistry.HQLookup.Values)
    {
      if (factionHq.ContainsAirbase(this))
        capturingHQ = factionHq;
    }
    if ((UnityEngine.Object) capturingHQ != (UnityEngine.Object) null)
      this.SetFactionWithoutEvent(capturingHQ, false);
    if (this.IsCustom)
    {
      SavedAirbase saved = MissionManager.CurrentMission.airbases.Find((Predicate<SavedAirbase>) (x => x.UniqueName == this.networkUniqueName));
      if (saved != null)
        this.SetupCustomAirbase(saved);
      else
        Debug.LogError((object) ("Could not find SavedAirbase in mission for CustomAirbase, name=" + this.networkUniqueName));
    }
    if (!this.AttachedAirbase)
      return;
    this.FindAttachedHangars();
    this.SetupAttachedAirbase(this.attachedUnit);
  }

  private void OnStartServer()
  {
    FactionRegistry.RegisterAirbase(this.SavedAirbase.UniqueName, this);
    this.airbaseRegistered = true;
    this.NetworknetworkUniqueName = this.SavedAirbase.UniqueName;
    if ((UnityEngine.Object) this.tower != (UnityEngine.Object) null)
      this.tower.onDisableUnit += new Action<Unit>(this.Airbase_OnTowerDisabled);
    if (this.taxiNetwork == null)
      this.taxiNetwork = new RoadNetwork();
    this.taxiNetwork.RegenerateNetwork();
    this.StartSlowUpdate(5f, new Action(this.ControlAircraft));
    if ((UnityEngine.Object) this.attachedUnit != (UnityEngine.Object) null)
    {
      this.attachedUnit.onDisableUnit += new Action<Unit>(this.Airbase_OnUnitDisabled);
      this.FindAttachedHangars();
      if ((UnityEngine.Object) this.attachedUnit.NetworkHQ != (UnityEngine.Object) null)
        this.attachedUnit.NetworkHQ.AddAirbase(this);
      this.CaptureFaction(this.attachedUnit.NetworkHQ);
    }
    this.disabled = this.SavedAirbase.Disabled;
    this.capture.SetCapturable(this.SavedAirbase.Capturable);
    foreach (Airbase.Runway runway in this.runways)
      runway.FindCrossingRunways();
    foreach (Airbase.VerticalLandingPoint verticalLandingPoint in this.verticalLandingPoints)
      verticalLandingPoint.FindCrossingRunways(this);
  }

  private void OnStopServer()
  {
    if (!this.airbaseRegistered)
      return;
    FactionRegistry.UnregisterAirbase(this);
    this.airbaseRegistered = false;
  }

  private void Airbase_OnUnitDisabled(Unit unit)
  {
    this.hangars.Clear();
    this.CaptureFaction((FactionHQ) null);
  }

  private void Airbase_OnTowerDisabled(Unit unit)
  {
    foreach (Renderer runwayLight in this.runwayLights)
      runwayLight.enabled = false;
  }

  public bool AnyHangarsAvailable()
  {
    if (this.disabled)
      return false;
    foreach (Hangar hangar in this.hangars)
    {
      if (!hangar.Disabled)
        return true;
    }
    return false;
  }

  public List<AircraftDefinition> GetAvailableAircraft()
  {
    this.availableAircraft.Clear();
    if (this.disabled)
      return this.availableAircraft;
    foreach (Hangar hangar in this.hangars)
    {
      if (!hangar.Disabled)
      {
        foreach (AircraftDefinition aircraftDefinition in hangar.GetAvailableAircraft())
        {
          if (!this.availableAircraft.Contains(aircraftDefinition))
            this.availableAircraft.Add(aircraftDefinition);
        }
      }
    }
    return this.availableAircraft;
  }

  public bool CanSpawnAircraft(AircraftDefinition definition)
  {
    if (this.disabled)
      return false;
    foreach (Hangar hangar in this.hangars)
    {
      if (!hangar.Disabled && hangar.CanSpawnAircraft(definition))
        return true;
    }
    return false;
  }

  [Mirage.Server]
  public Airbase.TrySpawnResult TrySpawnAircraft(
    Player player,
    AircraftDefinition definition,
    LiveryKey livery,
    Loadout loadout,
    float fuelLevel)
  {
    if (!this.IsServer)
      throw new MethodInvocationException("[Server] function 'TrySpawnAircraft' called when server not active");
    if (!NetworkManagerNuclearOption.i.Server.Active)
      throw new MethodInvocationException("TrySpawnAircraft called when server is not active");
    for (int index = 0; index < this.hangars.Count; ++index)
    {
      if (!((UnityEngine.Object) this.hangars[index] == (UnityEngine.Object) null))
      {
        Airbase.TrySpawnResult trySpawnResult = this.hangars[index].TrySpawnAircraft(player, definition, livery, loadout, fuelLevel);
        if (trySpawnResult.Allowed)
          return trySpawnResult;
      }
    }
    return new Airbase.TrySpawnResult();
  }

  private void SortHangarsByPriority()
  {
    this.hangars.Sort((Comparison<Hangar>) ((a, b) => b.GetPriority().CompareTo(a.GetPriority())));
  }

  private void OnDrawGizmos()
  {
    if (this.verticalLandingPoints == null)
      return;
    Airbase.DrawSphere(Color.green, this.center, 30f);
    Airbase.DrawSphere(Color.blue, this.aircraftSelectionTransform, 30f);
    foreach (Airbase.VerticalLandingPoint verticalLandingPoint in this.verticalLandingPoints)
      Airbase.DrawSphere(Color.magenta, verticalLandingPoint.point);
    foreach (Airbase.Runway runway in this.runways)
    {
      Airbase.DrawSphere(new Color(0.0f, 0.6f, 1f), runway.Start, 15f);
      Airbase.DrawSphere(new Color(0.0f, 0.2f, 1f), runway.End, 15f);
      foreach (Transform point in runway.entryPoints ?? Array.Empty<Transform>())
        Airbase.DrawSphere(new Color(0.0f, 1f, 0.6f), point, 10f);
      foreach (Transform point in runway.exitPoints ?? Array.Empty<Transform>())
        Airbase.DrawSphere(new Color(0.0f, 1f, 0.2f), point, 10f);
    }
    foreach (Road road in this.taxiNetwork.roads)
    {
      Color green = Color.green;
      Color red = Color.red;
      for (int index = 0; index < road.points.Count; ++index)
      {
        Airbase.DrawSphere(Color.Lerp(green, red, (float) index / (float) Math.Max(road.points.Count - 1, 1)), road.points[index]);
        if (index - 1 >= 0)
        {
          Vector3 localPosition1 = road.points[index - 1].ToLocalPosition();
          Vector3 localPosition2 = road.points[index].ToLocalPosition();
          Vector3 b = localPosition2;
          Gizmos.DrawLine(Vector3.Lerp(localPosition1, b, 0.5f), localPosition2);
        }
        if (index + 1 < road.points.Count)
        {
          Vector3 localPosition = road.points[index].ToLocalPosition();
          Gizmos.DrawLine(localPosition, Vector3.Lerp(localPosition, road.points[index + 1].ToLocalPosition(), 0.5f));
        }
      }
    }
  }

  private static void DrawSphere(Color color, GlobalPosition point)
  {
    Gizmos.color = color;
    Gizmos.DrawSphere(point.ToLocalPosition(), 5f);
  }

  private static void DrawSphere(Color color, Transform point, float size = 5f)
  {
    if ((UnityEngine.Object) point == (UnityEngine.Object) null)
      return;
    Gizmos.color = color;
    Gizmos.DrawSphere(point.position, size);
  }

  SingleSelectionDetails IEditorSelectable.CreateSelectionDetails()
  {
    return (SingleSelectionDetails) new AirbaseSelectionDetails(this);
  }

  Capture ICapturable.Capture => this.capture;

  bool ICapturable.disabled => this.disabled;

  Transform ICapturable.center => this.center;

  float ICapturable.CaptureRange => this.SavedAirbase.CaptureRange;

  float ICapturable.CaptureDefense => this.SavedAirbase.CaptureDefense;

  FactionHQ ICapturable.CurrentHQ => this.CurrentHQ;

  List<GridSquare> ICapturable.gridSquares => this.gridSquares;

  void ICapturable.OnCapture(FactionHQ value) => this.CaptureFaction(value);

  IEnumerable<Unit> ICapturable.GetDefenseUnits() => (IEnumerable<Unit>) this.buildings;

  private void MirageProcessed()
  {
  }

  public string NetworknetworkUniqueName
  {
    get => this.networkUniqueName;
    set => this.networkUniqueName = value;
  }

  public bool Network\u003Cdisabled\u003Ek__BackingField
  {
    get => this.\u003Cdisabled\u003Ek__BackingField;
    set
    {
      if (this.SyncVarEqual<bool>(value, this.\u003Cdisabled\u003Ek__BackingField))
        return;
      bool disabledKBackingField = this.\u003Cdisabled\u003Ek__BackingField;
      this.\u003Cdisabled\u003Ek__BackingField = value;
      this.SetDirtyBit(2UL);
    }
  }

  public override bool SerializeSyncVars(NetworkWriter writer, bool initialize)
  {
    ulong syncVarDirtyBits = this.SyncVarDirtyBits;
    bool flag = base.SerializeSyncVars(writer, initialize);
    if (initialize)
    {
      writer.WriteString(this.networkUniqueName);
      // ISSUE: reference to a compiler-generated field
      writer.WriteBooleanExtension(this.\u003Cdisabled\u003Ek__BackingField);
      return true;
    }
    writer.Write(syncVarDirtyBits, 2);
    if (((long) syncVarDirtyBits & 2L) != 0L)
    {
      // ISSUE: reference to a compiler-generated field
      writer.WriteBooleanExtension(this.\u003Cdisabled\u003Ek__BackingField);
      flag = true;
    }
    return flag;
  }

  public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
  {
    base.DeserializeSyncVars(reader, initialState);
    if (initialState)
    {
      this.networkUniqueName = reader.ReadString();
      // ISSUE: reference to a compiler-generated field
      this.\u003Cdisabled\u003Ek__BackingField = reader.ReadBooleanExtension();
    }
    else
    {
      ulong dirtyBit = reader.Read(2);
      this.SetDeserializeMask(dirtyBit, 0);
      if (((long) dirtyBit & 2L) == 0L)
        return;
      // ISSUE: reference to a compiler-generated field
      this.\u003Cdisabled\u003Ek__BackingField = reader.ReadBooleanExtension();
    }
  }

  public void UserCode_CmdRegisterUsage_1828364621(
    Aircraft aircraft,
    bool isUsing,
    byte? landingRunway)
  {
    this.RpcRegisterUsage(aircraft, isUsing, landingRunway);
  }

  protected static void Skeleton_CmdRegisterUsage_1828364621(
    NetworkBehaviour behaviour,
    NetworkReader reader,
    INetworkPlayer senderConnection,
    int replyId)
  {
    ((Airbase) behaviour).UserCode_CmdRegisterUsage_1828364621(GeneratedNetworkCode._Read_Aircraft(reader), reader.ReadBooleanExtension(), GeneratedNetworkCode._Read_System\u002ENullable\u00601\u003CSystem\u002EByte\u003E(reader));
  }

  public void UserCode_RpcRegisterUsage_2147349378(
    Aircraft aircraft,
    bool isUsing,
    byte? landingRunway)
  {
    if ((UnityEngine.Object) aircraft == (UnityEngine.Object) null)
      this.ControlledAircraft.RemoveAll((Predicate<Aircraft>) (s => (UnityEngine.Object) s == (UnityEngine.Object) null));
    else if (isUsing)
    {
      this.AddControlledAircraft(aircraft);
      if (landingRunway.HasValue)
      {
        byte index = landingRunway.Value;
        if ((int) index < this.runways.Length)
          this.runways[(int) index].RegisterLanding(aircraft);
        else
          ColorLog<Airbase>.LogError("runwayIndex out of bounds");
      }
      if (!((UnityEngine.Object) this.attachedUnit != (UnityEngine.Object) null) || !(this.attachedUnit is Ship attachedUnit))
        return;
      attachedUnit.ReduceSteeringRate();
    }
    else
      this.RemoveControlledAircraft(aircraft);
  }

  protected static void Skeleton_RpcRegisterUsage_2147349378(
    NetworkBehaviour behaviour,
    NetworkReader reader,
    INetworkPlayer senderConnection,
    int replyId)
  {
    ((Airbase) behaviour).UserCode_RpcRegisterUsage_2147349378(GeneratedNetworkCode._Read_Aircraft(reader), reader.ReadBooleanExtension(), GeneratedNetworkCode._Read_System\u002ENullable\u00601\u003CSystem\u002EByte\u003E(reader));
  }

  protected override int GetRpcCount() => 2;

  public override void RegisterRpc(RemoteCallCollection collection)
  {
    base.RegisterRpc(collection);
    collection.Register(0, "Airbase.CmdRegisterUsage", false, RpcInvokeType.ServerRpc, (NetworkBehaviour) this, new RpcDelegate(Airbase.Skeleton_CmdRegisterUsage_1828364621));
    collection.Register(1, "Airbase.RpcRegisterUsage", false, RpcInvokeType.ClientRpc, (NetworkBehaviour) this, new RpcDelegate(Airbase.Skeleton_RpcRegisterUsage_2147349378));
  }

  [Serializable]
  public class VerticalLandingPoint
  {
    public Transform point;
    public float approachAngleRange;
    public float size = 40f;
    [Tooltip("Parent Unit part (optional)")]
    public UnitPart unitPart;
    private readonly Queue<Aircraft> landingQueue = new Queue<Aircraft>();
    [NonSerialized]
    private List<Airbase.Runway> crossingRunways = new List<Airbase.Runway>();

    public void FindCrossingRunways(Airbase airbase)
    {
      this.crossingRunways = new List<Airbase.Runway>();
      foreach (Airbase.Runway runway in airbase.runways)
      {
        if (FastMath.InRange(runway.GetNearestPoint(this.point, false), this.point.position, this.size))
          this.crossingRunways.Add(runway);
      }
    }

    public bool IsOccupied(Aircraft querier)
    {
      foreach (Airbase.Runway crossingRunway in this.crossingRunways)
      {
        if (crossingRunway.OtherAircraftUsingRunway(querier))
          return true;
      }
      Aircraft aircraft;
      while (this.landingQueue.TryPeek(ref aircraft))
      {
        if (!((UnityEngine.Object) aircraft == (UnityEngine.Object) null))
          return (UnityEngine.Object) aircraft != (UnityEngine.Object) querier;
        this.landingQueue.Dequeue();
      }
      return false;
    }

    public Queue<Aircraft> GetLandingQueue() => this.landingQueue;

    public void RegisterLanding(Aircraft aircraft) => this.landingQueue.Enqueue(aircraft);

    public bool IsAvailable()
    {
      if ((UnityEngine.Object) this.unitPart == (UnityEngine.Object) null)
        return true;
      return (double) Vector3.Dot(this.point.transform.up, Vector3.up) >= 0.89999997615814209 && this.unitPart.parentUnit.enabled;
    }

    public float GetAngle(Transform fromTransform)
    {
      return Vector3.Angle(fromTransform.position - this.point.position, this.point.forward);
    }

    public GlobalPosition GetApproachPoint(Aircraft landingAircraft)
    {
      return (this.point.position + Vector3.RotateTowards(this.point.transform.forward with
      {
        y = 0.0f
      }, (landingAircraft.transform.position - this.point.position) with
      {
        y = 0.0f
      }, this.approachAngleRange * ((float) Math.PI / 180f), 0.0f) * 500f).ToGlobalPosition();
    }

    public Vector3 GetVelocity()
    {
      return (UnityEngine.Object) this.unitPart == (UnityEngine.Object) null || (UnityEngine.Object) this.unitPart.rb == (UnityEngine.Object) null ? Vector3.zero : this.unitPart.rb.GetPointVelocity(this.point.position);
    }

    public bool IsSuitable(RunwayQuery query) => (double) this.size > (double) query.MinSize;

    public static Airbase.VerticalLandingPoint FromSaved(
      Airbase airbase,
      GlobalPosition globalPosition)
    {
      Transform transform = new GameObject("verticalLandingPoints").transform;
      transform.parent = airbase.transform;
      transform.transform.position = globalPosition.ToLocalPosition();
      return new Airbase.VerticalLandingPoint()
      {
        point = transform,
        approachAngleRange = 180f,
        size = 40f,
        unitPart = (UnitPart) null
      };
    }
  }

  [Serializable]
  public class Runway
  {
    [SerializeField]
    private string name;
    public Transform Start;
    public Transform End;
    public Transform[] entryPoints;
    public Transform[] exitPoints;
    public bool Reversable;
    public bool Takeoff;
    public bool Landing;
    public bool Arrestor;
    public bool SkiJump;
    public bool AllowSimultaneousTakeoff = true;
    [SerializeField]
    private float width;
    [SerializeField]
    private Rigidbody rb;
    [SerializeField]
    private Renderer entryLightsAvailable;
    [SerializeField]
    private Renderer entryLightsOccupied;
    private List<Aircraft> landingList = new List<Aircraft>();
    private Vector3 direction;
    private Queue<Aircraft> takeoffQueue = new Queue<Aircraft>();
    private bool CurrentlyOperatingReversed;
    [NonSerialized]
    private List<Airbase.Runway> crossingRunways = new List<Airbase.Runway>();
    [NonSerialized]
    public float LastUsed;
    [NonSerialized]
    public byte index;
    [NonSerialized]
    public Airbase airbase;
    [NonSerialized]
    public bool occupied;

    public float Length { get; private set; }

    public event Action<Aircraft> OnRegisterLanding;

    private RunwayType RunwayType => (RunwayType) ((this.Landing ? 1 : 0) | (this.Takeoff ? 2 : 0));

    public static Airbase.Runway FromSaved(Airbase airbase, SavedRunway saved)
    {
      Airbase.Runway runway = new Airbase.Runway();
      runway.name = saved.Name;
      runway.Reversable = saved.Reversable;
      runway.Takeoff = saved.Takeoff;
      runway.Landing = saved.Landing;
      runway.Arrestor = saved.Arrestor;
      runway.SkiJump = saved.SkiJump;
      runway.width = saved.Width;
      runway.Start = CreateTransform(airbase.transform, saved.Start);
      runway.End = CreateTransform(airbase.transform, saved.End);
      runway.exitPoints = CreateTransforms(airbase.transform, saved.exitPoints);
      Transform[] exitPoints = runway.exitPoints;
      if ((exitPoints != null ? (exitPoints.Length != 0 ? 1 : 0) : 0) == 0)
      {
        runway.exitPoints = new Transform[2];
        runway.exitPoints[0] = runway.Start;
        runway.exitPoints[1] = runway.End;
      }
      return runway;

      static Transform CreateTransform(Transform parent, GlobalPosition position)
      {
        Transform transform = new GameObject("runway_transform").transform;
        transform.parent = parent;
        transform.position = position.ToLocalPosition();
        return transform;
      }

      static Transform[] CreateTransforms(Transform parent, GlobalPosition[] position)
      {
        Transform[] transforms = new Transform[position.Length];
        for (int index = 0; index < position.Length; ++index)
          transforms[index] = CreateTransform(parent, position[index]);
        return transforms;
      }
    }

    public void Setup(Airbase airbase, int index)
    {
      this.airbase = airbase;
      this.index = (byte) index;
      this.LastUsed = -100f;
      this.Length = Vector3.Distance(this.Start.position, this.End.position);
    }

    public List<Aircraft> GetLandingList() => this.landingList;

    public void SetUsageDirection(bool reversed)
    {
      this.CurrentlyOperatingReversed = reversed;
      this.LastUsed = Time.timeSinceLevelLoad;
    }

    private bool CrossesRunway(Airbase.Runway otherRunway)
    {
      Vector3 closestPointLine1;
      Vector3 closestPointLine2;
      return TargetCalc.ClosestPointsOnTwoLines(out closestPointLine1, out closestPointLine2, this.Start.position, this.GetDirection(false), otherRunway.Start.position, otherRunway.GetDirection(false)) && (double) Vector3.Distance(closestPointLine1, closestPointLine2) < (double) this.GetWidth();
    }

    public void FindCrossingRunways()
    {
      this.crossingRunways = new List<Airbase.Runway>();
      foreach (Airbase.Runway runway in this.airbase.runways)
      {
        if (runway.CrossesRunway(this))
          this.crossingRunways.Add(runway);
      }
    }

    public bool IsAvailableForTakeoff(Aircraft querier)
    {
      if (this.landingList.Count > 0)
        return false;
      Aircraft aircraft;
      if (this.takeoffQueue.TryPeek(ref aircraft))
      {
        if ((UnityEngine.Object) aircraft == (UnityEngine.Object) null || aircraft.disabled)
          this.takeoffQueue.Dequeue();
        return (UnityEngine.Object) aircraft == (UnityEngine.Object) querier;
      }
      if (this.OtherAircraftUsingRunway(querier))
        return false;
      foreach (Airbase.Runway crossingRunway in this.crossingRunways)
      {
        if (crossingRunway.OtherAircraftUsingRunway(querier))
          return false;
      }
      return true;
    }

    public void QueueTakeoff(Aircraft plane)
    {
      if (this.takeoffQueue == null)
        this.takeoffQueue = new Queue<Aircraft>();
      this.takeoffQueue.Enqueue(plane);
    }

    public void DequeueTakeoff(Aircraft plane)
    {
      if (this.takeoffQueue == null)
        this.takeoffQueue = new Queue<Aircraft>();
      Aircraft aircraft;
      if (!this.takeoffQueue.TryPeek(ref aircraft) || !((UnityEngine.Object) aircraft == (UnityEngine.Object) plane) && !((UnityEngine.Object) aircraft == (UnityEngine.Object) null) && !aircraft.disabled)
        return;
      this.takeoffQueue.Dequeue();
    }

    public void RegisterStartTakeoff(Aircraft plane)
    {
      Aircraft aircraft;
      if (!this.AllowSimultaneousTakeoff || !this.takeoffQueue.TryPeek(ref aircraft) || !((UnityEngine.Object) aircraft == (UnityEngine.Object) plane) && !((UnityEngine.Object) aircraft == (UnityEngine.Object) null) && !aircraft.disabled)
        return;
      this.takeoffQueue.Dequeue();
    }

    public void RegisterTakeoffLeftRunway(Aircraft plane)
    {
      Aircraft aircraft;
      if (!this.takeoffQueue.TryPeek(ref aircraft) || !((UnityEngine.Object) aircraft == (UnityEngine.Object) plane) && !((UnityEngine.Object) aircraft == (UnityEngine.Object) null) && !aircraft.disabled)
        return;
      this.takeoffQueue.Dequeue();
    }

    public void RegisterLanding(Aircraft plane)
    {
      if (this.landingList.Contains(plane))
        return;
      this.landingList.Add(plane);
      Action<Aircraft> onRegisterLanding = this.OnRegisterLanding;
      if (onRegisterLanding == null)
        return;
      onRegisterLanding(plane);
    }

    public void DeregisterLanding(Aircraft plane) => this.landingList.Remove(plane);

    public void MonitorLandings(Airbase airbase)
    {
      for (int index = this.landingList.Count - 1; index >= 0; --index)
      {
        if ((UnityEngine.Object) this.landingList[index] == (UnityEngine.Object) null)
          this.landingList.RemoveAt(index);
        this.LastUsed = Time.timeSinceLevelLoad;
      }
      if (this.landingList.Count != 0 || !this.Landing || !(airbase.attachedUnit is Ship attachedUnit))
        return;
      attachedUnit.RestoreSteeringRate();
    }

    public bool LandingsInProgress() => this.landingList.Count > 0;

    public bool IsSuitable(RunwayQuery query)
    {
      if (!this.RunwayType.Allowed(query.RunwayType) || !this.IsLevel())
        return false;
      float num1 = query.MinSize;
      if (this.Arrestor)
      {
        if (query.TailHook)
          return true;
        float num2 = (UnityEngine.Object) this.airbase.attachedUnit != (UnityEngine.Object) null ? this.airbase.attachedUnit.speed : 0.0f;
        num1 = (float) (((double) query.LandingSpeed - (double) num2) * ((double) query.LandingSpeed - (double) num2) / 12.0);
      }
      return (double) num1 < (double) this.Length;
    }

    public bool OtherAircraftUsingRunway(Aircraft checker)
    {
      foreach (UnityEngine.Object landing in this.landingList)
      {
        if (landing != (UnityEngine.Object) checker)
          return true;
      }
      Aircraft aircraft;
      return this.takeoffQueue.TryPeek(ref aircraft) && (UnityEngine.Object) aircraft != (UnityEngine.Object) null && (UnityEngine.Object) aircraft != (UnityEngine.Object) checker;
    }

    public bool CrossingRunwaysInUse()
    {
      foreach (Airbase.Runway crossingRunway in this.crossingRunways)
      {
        if (crossingRunway.LandingsInProgress())
          return true;
      }
      return false;
    }

    public bool ClearForTakeoff(Aircraft aircraft, bool checkCrossing)
    {
      foreach (Airbase.Runway crossingRunway in this.crossingRunways)
      {
        if (!crossingRunway.ClearForTakeoff(aircraft, false))
          return false;
      }
      Aircraft aircraft1;
      return this.takeoffQueue.Count == 0 || !this.takeoffQueue.TryPeek(ref aircraft1) || (UnityEngine.Object) aircraft1 == (UnityEngine.Object) aircraft;
    }

    public bool AircraftOnApproach(Aircraft aircraft, float range, bool excludeBetweenEndpoints)
    {
      return (!excludeBetweenEndpoints || (double) Vector3.Dot(aircraft.transform.position - this.Start.position, this.GetDirection(false)) <= 0.0 || (double) Vector3.Dot(aircraft.transform.position - this.End.position, this.GetDirection(true)) <= 0.0) && (FastMath.InRange(this.Start.position, aircraft.transform.position, range) || FastMath.InRange(this.End.position, aircraft.transform.position, range)) && (double) Vector3.Dot(aircraft.transform.forward, ((this.Start.position + this.End.position) * 0.5f - aircraft.transform.position).normalized) > 0.800000011920929;
    }

    public bool AircraftOnRunway(Aircraft aircraft)
    {
      return (double) Vector3.Distance(aircraft.transform.position, this.GetNearestPoint(aircraft.transform, false)) < (double) this.GetWidth() * 0.5 + (double) aircraft.maxRadius;
    }

    public float AircraftDistanceFromRunway(Aircraft aircraft)
    {
      return (double) Vector3.Dot(aircraft.transform.position - this.Start.position, this.GetDirection(false)) < 0.0 || (double) Vector3.Dot(aircraft.transform.position - this.End.position, this.GetDirection(true)) < 0.0 ? float.MaxValue : Vector3.Distance(aircraft.transform.position, this.GetNearestPoint(aircraft.transform, false)) - this.GetWidth() + aircraft.maxRadius;
    }

    public bool AircraftApproachingRunway(Aircraft aircraft, float distance)
    {
      Vector3 nearestPoint = this.GetNearestPoint(aircraft.transform, true);
      return (double) Vector3.Dot(nearestPoint - aircraft.transform.position, aircraft.transform.forward) >= 0.0 && (double) (nearestPoint - aircraft.transform.position).sqrMagnitude < (double) distance * (double) distance;
    }

    public Vector3 GetGlideslopeAimpoint(
      Aircraft aircraft,
      float distance,
      bool reverse,
      float timeToTouchdown)
    {
      Transform transform = reverse ? this.End : this.Start;
      Vector3 normalized = (-this.GetDirection(reverse) + Vector3.up * this.Length * 0.06f).normalized;
      Vector3 vector3 = Vector3.zero;
      if ((UnityEngine.Object) this.rb != (UnityEngine.Object) null && (double) timeToTouchdown > 0.0)
        vector3 = this.GetVelocity() * timeToTouchdown;
      return transform.position + Vector3.up * (aircraft.definition.spawnOffset.y + 0.5f) + normalized * distance + vector3;
    }

    public float GetGlideslopeError(Aircraft aircraft, float timeToTouchdown, bool reverse)
    {
      Vector3 vector3_1 = reverse ? this.End.position : this.Start.position;
      if ((UnityEngine.Object) this.rb != (UnityEngine.Object) null)
        vector3_1 += this.GetVelocity() * timeToTouchdown;
      Vector3 vector3_2 = new Vector3(aircraft.transform.position.x, 0.0f, aircraft.transform.position.z) - new Vector3(vector3_1.x, 0.0f, vector3_1.z);
      float num = (float) ((double) vector3_1.y + (double) aircraft.definition.spawnOffset.y + 0.059999998658895493 * (double) vector3_2.magnitude);
      return aircraft.transform.position.y - num;
    }

    public bool TryGetExitTaxiPoint(Transform fromTransform, float speed, out Transform exitPoint)
    {
      exitPoint = (Transform) null;
      float num = float.MaxValue;
      for (int index = 0; index < this.exitPoints.Length; ++index)
      {
        Vector3 lhs = this.exitPoints[index].position - fromTransform.position;
        if ((double) Vector3.Dot(this.exitPoints[index].forward, fromTransform.forward) > 0.0 && (double) Vector3.Dot(lhs, fromTransform.forward) > 0.0)
        {
          float magnitude = lhs.magnitude;
          if ((double) magnitude < (double) num && 225.0 > (double) speed * (double) speed - 12.0 * (double) magnitude)
          {
            num = magnitude;
            exitPoint = this.exitPoints[index];
          }
        }
      }
      return (UnityEngine.Object) exitPoint != (UnityEngine.Object) null;
    }

    public Vector3 GetVelocity()
    {
      return (UnityEngine.Object) this.rb == (UnityEngine.Object) null ? Vector3.zero : this.rb.GetPointVelocity(this.Start.position);
    }

    public bool IsLevel()
    {
      return (double) Mathf.Abs(this.Start.position.y - this.End.position.y) < (double) this.Length * 0.029999999329447746;
    }

    public float GetWidth() => this.width;

    public Vector3 GetNearestPoint(Transform fromTransform, bool extend)
    {
      return this.GetNearestPoint(fromTransform.position, extend);
    }

    public Vector3 GetNearestPoint(Vector3 fromPosition, bool extend)
    {
      if (!extend)
      {
        Vector3 lhs1 = fromPosition - this.Start.position;
        Vector3 lhs2 = fromPosition - this.End.position;
        if ((double) Vector3.Dot(lhs1, this.GetDirection(false)) < 0.0 || (double) Vector3.Dot(lhs2, this.GetDirection(true)) < 0.0)
          return (double) lhs1.sqrMagnitude >= (double) lhs2.sqrMagnitude ? this.End.position : this.Start.position;
      }
      return this.Start.position + Vector3.Project(fromPosition - this.Start.position, this.GetDirection(false));
    }

    public Vector3 GetDirection(bool reverse)
    {
      return !reverse ? this.End.position - this.Start.position : this.Start.position - this.End.position;
    }

    public string GetName(bool reverse)
    {
      if (!string.IsNullOrEmpty(this.name))
        return this.name;
      int num = Mathf.RoundToInt(Quaternion.LookRotation(reverse ? this.Start.position - this.End.position : this.End.position - this.Start.position, Vector3.up).eulerAngles.y * 0.1f);
      if (num == 0)
        num = 36;
      return num >= 10 ? $"Runway {num}" : $"Runway 0{num}";
    }

    public Airbase.Runway.RunwayDistanceResult GetDistance(Transform fromTransform)
    {
      if ((double) Time.timeSinceLevelLoad - (double) this.LastUsed < 30.0)
        return new Airbase.Runway.RunwayDistanceResult(Vector3.Distance((this.CurrentlyOperatingReversed ? this.End : this.Start).position, fromTransform.position), this.CurrentlyOperatingReversed);
      float distance1 = Vector3.Distance(this.Start.position, fromTransform.position);
      if (!this.Reversable)
        return new Airbase.Runway.RunwayDistanceResult(distance1, false);
      float distance2 = Vector3.Distance(this.End.position, fromTransform.position);
      return (double) distance2 < (double) distance1 ? new Airbase.Runway.RunwayDistanceResult(distance2, true) : new Airbase.Runway.RunwayDistanceResult(distance1, false);
    }

    public Airbase.Runway.RunwayAngleResult GetAngle(Transform fromTransform)
    {
      if (this.direction == Vector3.zero)
        this.direction = (this.End.position - this.Start.position).normalized;
      Vector3 from1 = (this.Start.position - fromTransform.position) with
      {
        y = 0.0f
      };
      float angle1 = Vector3.Angle(from1, this.direction);
      if (!this.Reversable)
        return new Airbase.Runway.RunwayAngleResult(angle1, false);
      Vector3 from2 = this.End.position - fromTransform.position;
      from1.y = 0.0f;
      Vector3 to = -this.direction;
      float angle2 = Vector3.Angle(from2, to);
      return (double) angle1 < (double) angle2 ? new Airbase.Runway.RunwayAngleResult(angle1, false) : new Airbase.Runway.RunwayAngleResult(angle2, true);
    }

    public readonly struct RunwayAngleResult(float angle, bool reverse)
    {
      public readonly float Angle = angle;
      public readonly bool Reverse = reverse;
    }

    public readonly struct RunwayDistanceResult(float distance, bool reverse)
    {
      public readonly float Distance = distance;
      public readonly bool Reverse = reverse;
    }

    public readonly struct RunwayUsage(Airbase.Runway runway, bool reverse)
    {
      public readonly Airbase.Runway Runway = runway;
      public readonly bool Reverse = reverse;

      public string GetName() => this.Runway.GetName(this.Reverse);

      public Vector3 GetDirection() => this.Runway.GetDirection(this.Reverse);

      public Transform GetEnd() => !this.Reverse ? this.Runway.Start : this.Runway.End;
    }
  }

  public readonly struct TrySpawnResult(bool allowed, Hangar hangar, bool delayedSpawn)
  {
    public readonly bool Allowed = allowed;
    public readonly Hangar Hangar = hangar;
    public readonly bool DelayedSpawn = delayedSpawn;
  }
}
