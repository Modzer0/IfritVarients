// Decompiled with JetBrains decompiler
// Type: InfoPanel_Faction
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

#nullable disable
public class InfoPanel_Faction : MonoBehaviour
{
  public MFDScreen screen;
  public InfoPanel_Faction.SelectFaction selectFaction;
  public InfoPanel_Faction.DisplayType currentDisplay;
  public bool Initialized;
  [Header("Faction")]
  [SerializeField]
  private Text factionName;
  [SerializeField]
  private Image factionImage;
  [SerializeField]
  private Text funds;
  [SerializeField]
  private Text score;
  [SerializeField]
  private Text warheads;
  [Header("Button")]
  [SerializeField]
  private Dropdown currentDisplayDropdown;
  [Header("Airbases")]
  [SerializeField]
  private GameObject airbase_container;
  [SerializeField]
  private InfoPanel_ItemPrefab airbases;
  [SerializeField]
  private Transform airbases_list;
  [Header("Value items")]
  [SerializeField]
  private GameObject total_value_container;
  [SerializeField]
  private List<InfoPanel_ItemPrefab> items_total_value = new List<InfoPanel_ItemPrefab>();
  [SerializeField]
  private List<GameObject> listSpacers = new List<GameObject>();
  [Header("Buildings")]
  [SerializeField]
  private GameObject buildings_container;
  [SerializeField]
  private List<InfoPanel_ItemPrefab> items_buildings = new List<InfoPanel_ItemPrefab>();
  [SerializeField]
  private List<InfoPanel_ItemPrefab> items_buildings_value = new List<InfoPanel_ItemPrefab>();
  [Header("Vehicles")]
  [SerializeField]
  private GameObject vehicles_container;
  [SerializeField]
  private List<InfoPanel_ItemPrefab> items_vehicles = new List<InfoPanel_ItemPrefab>();
  [SerializeField]
  private List<InfoPanel_ItemPrefab> items_vehicles_value = new List<InfoPanel_ItemPrefab>();
  [Header("Ships")]
  [SerializeField]
  private GameObject ships_container;
  [SerializeField]
  private List<InfoPanel_ItemPrefab> items_ships = new List<InfoPanel_ItemPrefab>();
  [SerializeField]
  private List<InfoPanel_ItemPrefab> items_ships_value = new List<InfoPanel_ItemPrefab>();
  [Header("Aircraft")]
  [SerializeField]
  private GameObject aircraft_container;
  [SerializeField]
  private Transform aircraft_list;
  [SerializeField]
  private List<InfoPanel_ItemPrefab> items_aircraft = new List<InfoPanel_ItemPrefab>();
  [SerializeField]
  private List<InfoPanel_ItemPrefab> items_aircraft_value = new List<InfoPanel_ItemPrefab>();
  [Header("Players")]
  [SerializeField]
  private GameObject players_container;
  [SerializeField]
  private InfoPanel_ItemPrefab players;
  [SerializeField]
  private Transform players_list;
  public List<Player> listPlayers;
  public List<Airbase> listAirbases;
  private float refreshRate = 1f;
  private float lastRefresh;
  [Header("Prefabs")]
  [SerializeField]
  private GameObject playerEntry_prefab;
  [SerializeField]
  private GameObject airbaseEntry_prefab;
  [SerializeField]
  private GameObject airbaseHeader_prefab;
  [SerializeField]
  private GameObject carrierEntry_prefab;

  public FactionHQ factionHQ { get; private set; }

  private void Start()
  {
    SceneSingleton<DynamicMap>.i.onMapGenerated += new Action(this.SetFaction);
    this.Initialized = false;
  }

  private void Update()
  {
    if (!this.Initialized || !this.screen.isActive || (double) Time.timeSinceLevelLoad <= (double) this.lastRefresh + (double) this.refreshRate)
      return;
    this.funds.text = UnitConverter.ValueReading(this.factionHQ.factionFunds);
    this.funds.color = GameAssets.i.redGreenGradient.Evaluate(this.factionHQ.factionFunds / 100f);
    this.score.text = this.factionHQ.factionScore.ToString("N1") ?? "";
    this.warheads.text = this.factionHQ.GetWarheadStockpile().ToString() ?? "";
    this.airbases.Refresh(new float?((float) this.factionHQ.GetAirbases().Count<Airbase>()), false, false);
    switch (this.currentDisplay)
    {
      case InfoPanel_Faction.DisplayType.Forces:
        this.UpdateDisplayForces();
        break;
      case InfoPanel_Faction.DisplayType.Players:
        this.UpdateDisplayPlayers();
        break;
      case InfoPanel_Faction.DisplayType.Reserves:
        this.UpdateDisplayReserves();
        break;
      case InfoPanel_Faction.DisplayType.Losses:
        this.UpdateDisplayLosses();
        break;
      case InfoPanel_Faction.DisplayType.Value:
        this.UpdateDisplayValue();
        break;
      case InfoPanel_Faction.DisplayType.Manpower:
        this.UpdateDisplayManpower();
        break;
    }
    this.lastRefresh = Time.timeSinceLevelLoad;
  }

  private void AddAirbase(Airbase airbase)
  {
    if (this.listAirbases.Contains(airbase))
      return;
    this.AddAirbaseEntry(airbase);
  }

  private void UpdateAirbaseList()
  {
    List<Airbase> list = this.factionHQ.GetAirbases().ToList<Airbase>();
    if (list.Count == 0 || list.Count == this.listAirbases.Count)
      return;
    for (int index = 0; index < list.Count; ++index)
    {
      if (!this.listAirbases.Contains(list[index]))
        this.AddAirbase(list[index]);
    }
  }

  public void SetFaction()
  {
    this.ResetLists();
    this.listPlayers = new List<Player>();
    this.listAirbases = new List<Airbase>();
    string factionName1 = "Boscali";
    string factionName2 = "Primeva";
    if ((UnityEngine.Object) SceneSingleton<DynamicMap>.i.HQ != (UnityEngine.Object) null)
    {
      factionName1 = SceneSingleton<DynamicMap>.i.HQ.faction.name;
      factionName2 = !(factionName1 == "Boscali") ? "Boscali" : "Primeva";
    }
    this.factionHQ = this.selectFaction != InfoPanel_Faction.SelectFaction.Current ? FactionRegistry.HqFromName(factionName2) : FactionRegistry.HqFromName(factionName1);
    this.screen.shortName = this.factionHQ.faction.factionTag;
    this.screen.virtualMFD.SetupButtons();
    this.factionHQ.onAirbaseAdded += new Action<Airbase>(this.AddAirbase);
    this.factionImage.sprite = this.factionHQ.faction.factionColorLogo;
    this.factionName.text = this.factionHQ.faction.factionName.ToUpper();
    this.lastRefresh = Time.timeSinceLevelLoad;
    foreach (InfoPanel_ItemPrefab infoPanelItemPrefab in this.items_total_value)
      infoPanelItemPrefab.Set(this);
    foreach (InfoPanel_ItemPrefab itemsBuilding in this.items_buildings)
      itemsBuilding.Set(this);
    foreach (InfoPanel_ItemPrefab infoPanelItemPrefab in this.items_buildings_value)
      infoPanelItemPrefab.Set(this);
    foreach (InfoPanel_ItemPrefab itemsVehicle in this.items_vehicles)
      itemsVehicle.Set(this);
    foreach (InfoPanel_ItemPrefab infoPanelItemPrefab in this.items_vehicles_value)
      infoPanelItemPrefab.Set(this);
    foreach (InfoPanel_ItemPrefab itemsShip in this.items_ships)
      itemsShip.Set(this);
    foreach (InfoPanel_ItemPrefab infoPanelItemPrefab in this.items_ships_value)
      infoPanelItemPrefab.Set(this);
    foreach (InfoPanel_ItemPrefab infoPanelItemPrefab in this.items_aircraft)
      infoPanelItemPrefab.Set(this);
    foreach (InfoPanel_ItemPrefab infoPanelItemPrefab in this.items_aircraft_value)
      infoPanelItemPrefab.Set(this);
    this.UpdateAirbaseList();
    this.Initialized = true;
  }

  public void DeselectPlayers()
  {
    foreach (Component players in this.players_list)
      players.GetComponent<InfoPanel_PlayerEntry>().Deselect();
  }

  public bool CheckIfPlayerSelected()
  {
    bool flag = false;
    foreach (Component players in this.players_list)
    {
      if (players.GetComponent<InfoPanel_PlayerEntry>().IsSelected())
        flag = true;
    }
    return flag;
  }

  public void AddPlayerEntry(Player p)
  {
    UnityEngine.Object.Instantiate<GameObject>(this.playerEntry_prefab, this.players_list).GetComponent<InfoPanel_PlayerEntry>().SetPlayer(p, this);
  }

  public void RemoveNullPlayers()
  {
    for (int index = 0; index < this.listPlayers.Count; ++index)
    {
      if ((UnityEngine.Object) this.listPlayers[index] == (UnityEngine.Object) null)
        this.listPlayers.RemoveAt(index);
    }
  }

  public void AddAirbaseEntry(Airbase a)
  {
    if (this.airbases_list.childCount == 0)
      UnityEngine.Object.Instantiate<GameObject>(this.airbaseHeader_prefab, this.airbases_list);
    Unit attachedUnit;
    a.TryGetAttachedUnit(out attachedUnit);
    (!((UnityEngine.Object) attachedUnit != (UnityEngine.Object) null) ? UnityEngine.Object.Instantiate<GameObject>(this.airbaseEntry_prefab, this.airbases_list) : UnityEngine.Object.Instantiate<GameObject>(this.carrierEntry_prefab, this.airbases_list)).GetComponent<InfoPanel_AirbaseEntry>().SetAirbase(a, this);
    this.listAirbases.Add(a);
  }

  public void SetDisplayForces()
  {
    this.currentDisplay = InfoPanel_Faction.DisplayType.Forces;
    this.players_container.SetActive(false);
    this.airbase_container.SetActive(true);
    this.total_value_container.SetActive(false);
    this.vehicles_container.SetActive(true);
    this.ships_container.SetActive(true);
    this.buildings_container.SetActive(true);
    this.aircraft_container.SetActive(true);
    this.aircraft_list.GetComponent<GridLayoutGroup>().constraintCount = 2;
    foreach (GameObject listSpacer in this.listSpacers)
      listSpacer.SetActive(false);
    foreach (Component component in this.items_buildings_value)
      component.gameObject.SetActive(false);
    foreach (Component itemsBuilding in this.items_buildings)
      itemsBuilding.gameObject.SetActive(true);
    foreach (Component component in this.items_vehicles_value)
      component.gameObject.SetActive(false);
    foreach (Component itemsVehicle in this.items_vehicles)
      itemsVehicle.gameObject.SetActive(true);
    foreach (Component component in this.items_ships_value)
      component.gameObject.SetActive(false);
    foreach (Component itemsShip in this.items_ships)
      itemsShip.gameObject.SetActive(true);
    foreach (Component component in this.items_aircraft_value)
      component.gameObject.SetActive(false);
    foreach (Component component in this.items_aircraft)
      component.gameObject.SetActive(true);
  }

  public void UpdateDisplayForces()
  {
    for (int index = 0; index < this.items_buildings.Count; ++index)
    {
      if (index == 0)
        this.items_buildings[index].Refresh(new float?((float) (int) this.factionHQ.missionStatsTracker.units.buildings.current), false, false);
      else
        this.items_buildings[index].RefreshDefinition(this.currentDisplay);
    }
    for (int index = 0; index < this.items_vehicles.Count; ++index)
    {
      if (index == 0)
        this.items_vehicles[index].Refresh(new float?((float) (int) this.factionHQ.missionStatsTracker.units.vehicles.current), false, false);
      else
        this.items_vehicles[index].RefreshDefinition(this.currentDisplay);
    }
    for (int index = 0; index < this.items_ships.Count; ++index)
    {
      if (index == 0)
        this.items_ships[index].Refresh(new float?((float) (int) this.factionHQ.missionStatsTracker.units.ships.current), false, false);
      else
        this.items_ships[index].RefreshDefinition(this.currentDisplay);
    }
    for (int index = 0; index < this.items_aircraft.Count; ++index)
    {
      if (index == 0)
        this.items_aircraft[index].Refresh(new float?((float) (int) this.factionHQ.missionStatsTracker.units.aircraft.current), false, false);
      else
        this.items_aircraft[index].RefreshDefinition(this.currentDisplay);
    }
  }

  public void SetDisplayReserves()
  {
    this.currentDisplay = InfoPanel_Faction.DisplayType.Reserves;
    this.players_container.SetActive(false);
    this.airbase_container.SetActive(false);
    this.total_value_container.SetActive(false);
    this.vehicles_container.SetActive(true);
    this.ships_container.SetActive(true);
    this.buildings_container.SetActive(true);
    this.aircraft_container.SetActive(true);
    this.aircraft_list.GetComponent<GridLayoutGroup>().constraintCount = 2;
    foreach (GameObject listSpacer in this.listSpacers)
      listSpacer.SetActive(false);
    foreach (Component component in this.items_buildings_value)
      component.gameObject.SetActive(false);
    foreach (Component itemsBuilding in this.items_buildings)
      itemsBuilding.gameObject.SetActive(true);
    foreach (Component component in this.items_vehicles_value)
      component.gameObject.SetActive(false);
    foreach (Component itemsVehicle in this.items_vehicles)
      itemsVehicle.gameObject.SetActive(true);
    foreach (Component component in this.items_ships_value)
      component.gameObject.SetActive(false);
    foreach (Component itemsShip in this.items_ships)
      itemsShip.gameObject.SetActive(true);
    foreach (Component component in this.items_aircraft_value)
      component.gameObject.SetActive(false);
    foreach (Component component in this.items_aircraft)
      component.gameObject.SetActive(true);
  }

  public void UpdateDisplayReserves()
  {
    float num1 = 0.0f;
    float num2 = 0.0f;
    for (int index = 0; index < this.items_ships.Count; ++index)
      this.items_ships[index].Refresh(new float?(0.0f), false, false);
    for (int index = 0; index < this.items_buildings.Count; ++index)
      this.items_buildings[index].Refresh(new float?(0.0f), false, false);
    for (int index = 0; index < this.items_vehicles.Count; ++index)
    {
      if (index > 0)
      {
        this.items_vehicles[index].RefreshDefinition(this.currentDisplay);
        num1 += this.items_vehicles[index].GetValue();
      }
    }
    this.items_vehicles[0].Refresh(new float?(num1), false, false);
    for (int index = 0; index < this.items_aircraft.Count; ++index)
    {
      if (index > 0)
      {
        this.items_aircraft[index].RefreshDefinition(this.currentDisplay);
        num2 += this.items_aircraft[index].GetValue();
      }
    }
    this.items_aircraft[0].Refresh(new float?(num2), false, false);
  }

  public void SetDisplayLosses()
  {
    this.currentDisplay = InfoPanel_Faction.DisplayType.Losses;
    this.players_container.SetActive(false);
    this.airbase_container.SetActive(false);
    this.total_value_container.SetActive(false);
    this.vehicles_container.SetActive(true);
    this.ships_container.SetActive(true);
    this.buildings_container.SetActive(true);
    this.aircraft_container.SetActive(true);
    this.aircraft_list.GetComponent<GridLayoutGroup>().constraintCount = 2;
    foreach (GameObject listSpacer in this.listSpacers)
      listSpacer.SetActive(false);
    foreach (Component component in this.items_buildings_value)
      component.gameObject.SetActive(false);
    foreach (Component itemsBuilding in this.items_buildings)
      itemsBuilding.gameObject.SetActive(true);
    foreach (Component component in this.items_vehicles_value)
      component.gameObject.SetActive(false);
    foreach (Component itemsVehicle in this.items_vehicles)
      itemsVehicle.gameObject.SetActive(true);
    foreach (Component component in this.items_ships_value)
      component.gameObject.SetActive(false);
    foreach (Component itemsShip in this.items_ships)
      itemsShip.gameObject.SetActive(true);
    foreach (Component component in this.items_aircraft_value)
      component.gameObject.SetActive(false);
    foreach (Component component in this.items_aircraft)
      component.gameObject.SetActive(true);
  }

  public void UpdateDisplayLosses()
  {
    for (int index = 0; index < this.items_buildings.Count; ++index)
    {
      if (index == 0)
        this.items_buildings[index].Refresh(new float?((float) (int) this.factionHQ.missionStatsTracker.units.buildings.lost), true, false);
      else
        this.items_buildings[index].RefreshDefinition(this.currentDisplay);
    }
    for (int index = 0; index < this.items_vehicles.Count; ++index)
    {
      if (index == 0)
        this.items_vehicles[index].Refresh(new float?((float) (int) this.factionHQ.missionStatsTracker.units.vehicles.lost), true, false);
      else
        this.items_vehicles[index].RefreshDefinition(this.currentDisplay);
    }
    for (int index = 0; index < this.items_ships.Count; ++index)
    {
      if (index == 0)
        this.items_ships[index].Refresh(new float?((float) (int) this.factionHQ.missionStatsTracker.units.ships.lost), true, false);
      else
        this.items_ships[index].RefreshDefinition(this.currentDisplay);
    }
    for (int index = 0; index < this.items_aircraft.Count; ++index)
    {
      if (index == 0)
        this.items_aircraft[index].Refresh(new float?((float) (int) this.factionHQ.missionStatsTracker.units.aircraft.lost), true, false);
      else
        this.items_aircraft[index].RefreshDefinition(this.currentDisplay);
    }
  }

  public void SetDisplayValue()
  {
    this.currentDisplay = InfoPanel_Faction.DisplayType.Value;
    this.players_container.SetActive(false);
    this.airbase_container.SetActive(false);
    this.total_value_container.SetActive(true);
    this.vehicles_container.SetActive(true);
    this.ships_container.SetActive(true);
    this.buildings_container.SetActive(true);
    this.aircraft_container.SetActive(true);
    this.aircraft_list.GetComponent<GridLayoutGroup>().constraintCount = 1;
    this.items_total_value[2].gameObject.SetActive(true);
    foreach (GameObject listSpacer in this.listSpacers)
      listSpacer.SetActive(true);
    foreach (Component itemsBuilding in this.items_buildings)
      itemsBuilding.gameObject.SetActive(false);
    foreach (Component component in this.items_buildings_value)
      component.gameObject.SetActive(true);
    foreach (Component itemsVehicle in this.items_vehicles)
      itemsVehicle.gameObject.SetActive(false);
    foreach (Component component in this.items_vehicles_value)
      component.gameObject.SetActive(true);
    foreach (Component itemsShip in this.items_ships)
      itemsShip.gameObject.SetActive(false);
    foreach (Component component in this.items_ships_value)
      component.gameObject.SetActive(true);
    foreach (Component component in this.items_aircraft)
      component.gameObject.SetActive(false);
    foreach (Component component in this.items_aircraft_value)
      component.gameObject.SetActive(true);
  }

  public void UpdateDisplayValue()
  {
    List<InfoPanel_ItemPrefab>[] infoPanelItemPrefabListArray = new List<InfoPanel_ItemPrefab>[5]
    {
      this.items_total_value,
      this.items_buildings_value,
      this.items_vehicles_value,
      this.items_ships_value,
      this.items_aircraft_value
    };
    for (int index = 0; index < 5; ++index)
    {
      MissionStatsTracker.Stat stat = this.factionHQ.missionStatsTracker.value.total;
      if (index == 1)
        stat = this.factionHQ.missionStatsTracker.value.buildings;
      else if (index == 2)
        stat = this.factionHQ.missionStatsTracker.value.vehicles;
      else if (index == 3)
        stat = this.factionHQ.missionStatsTracker.value.ships;
      else if (index == 4)
        stat = this.factionHQ.missionStatsTracker.value.aircraft;
      infoPanelItemPrefabListArray[index][0].Refresh(new float?(stat.total), false, true);
      infoPanelItemPrefabListArray[index][1].Refresh(new float?(stat.current), false, true);
      infoPanelItemPrefabListArray[index][2].Refresh(new float?(stat.spent), false, true);
      infoPanelItemPrefabListArray[index][3].Refresh(new float?(stat.lost), true, true);
    }
  }

  public void SetDisplayManpower()
  {
    this.currentDisplay = InfoPanel_Faction.DisplayType.Manpower;
    this.players_container.SetActive(false);
    this.airbase_container.SetActive(false);
    this.total_value_container.SetActive(true);
    this.vehicles_container.SetActive(true);
    this.ships_container.SetActive(true);
    this.buildings_container.SetActive(true);
    this.aircraft_container.SetActive(true);
    this.aircraft_list.GetComponent<GridLayoutGroup>().constraintCount = 1;
    this.items_total_value[2].gameObject.SetActive(false);
    foreach (GameObject listSpacer in this.listSpacers)
      listSpacer.SetActive(true);
    foreach (Component itemsBuilding in this.items_buildings)
      itemsBuilding.gameObject.SetActive(false);
    foreach (Component component in this.items_buildings_value)
      component.gameObject.SetActive(true);
    this.items_buildings_value[2].gameObject.SetActive(false);
    foreach (Component itemsVehicle in this.items_vehicles)
      itemsVehicle.gameObject.SetActive(false);
    foreach (Component component in this.items_vehicles_value)
      component.gameObject.SetActive(true);
    this.items_vehicles_value[2].gameObject.SetActive(false);
    foreach (Component itemsShip in this.items_ships)
      itemsShip.gameObject.SetActive(false);
    foreach (Component component in this.items_ships_value)
      component.gameObject.SetActive(true);
    this.items_ships_value[2].gameObject.SetActive(false);
    foreach (Component component in this.items_aircraft)
      component.gameObject.SetActive(false);
    foreach (Component component in this.items_aircraft_value)
      component.gameObject.SetActive(true);
    this.items_aircraft_value[2].gameObject.SetActive(false);
  }

  public void UpdateDisplayManpower()
  {
    List<InfoPanel_ItemPrefab>[] infoPanelItemPrefabListArray = new List<InfoPanel_ItemPrefab>[5]
    {
      this.items_total_value,
      this.items_buildings_value,
      this.items_vehicles_value,
      this.items_ships_value,
      this.items_aircraft_value
    };
    for (int index = 0; index < 5; ++index)
    {
      MissionStatsTracker.Stat stat = this.factionHQ.missionStatsTracker.manpower.total;
      if (index == 1)
        stat = this.factionHQ.missionStatsTracker.manpower.buildings;
      else if (index == 2)
        stat = this.factionHQ.missionStatsTracker.manpower.vehicles;
      else if (index == 3)
        stat = this.factionHQ.missionStatsTracker.manpower.ships;
      else if (index == 4)
        stat = this.factionHQ.missionStatsTracker.manpower.aircraft;
      infoPanelItemPrefabListArray[index][0].Refresh(new float?((float) (int) stat.total), false, false);
      infoPanelItemPrefabListArray[index][1].Refresh(new float?((float) (int) stat.current), false, false);
      infoPanelItemPrefabListArray[index][2].Refresh(new float?((float) (int) stat.spent), false, false);
      infoPanelItemPrefabListArray[index][3].Refresh(new float?((float) (int) stat.lost), true, false);
    }
  }

  public void SetDisplayPlayers()
  {
    this.currentDisplay = InfoPanel_Faction.DisplayType.Players;
    this.players_container.SetActive(true);
    this.airbase_container.SetActive(false);
    this.total_value_container.SetActive(false);
    this.vehicles_container.SetActive(false);
    this.ships_container.SetActive(false);
    this.buildings_container.SetActive(false);
    this.aircraft_container.SetActive(false);
  }

  public void UpdateDisplayPlayers()
  {
    this.players.Refresh(new float?((float) this.factionHQ.factionPlayers.Count), false, false);
    if (this.factionHQ.factionPlayers.Count == 0)
      return;
    for (int i = 0; i < this.factionHQ.factionPlayers.Count; ++i)
    {
      List<Player> listPlayers1 = this.listPlayers;
      PlayerRef factionPlayer = this.factionHQ.factionPlayers[i];
      Player player1 = factionPlayer.Player;
      if (!listPlayers1.Contains(player1))
      {
        List<Player> listPlayers2 = this.listPlayers;
        factionPlayer = this.factionHQ.factionPlayers[i];
        Player player2 = factionPlayer.Player;
        listPlayers2.Add(player2);
        factionPlayer = this.factionHQ.factionPlayers[i];
        this.AddPlayerEntry(factionPlayer.Player);
      }
    }
  }

  public void ToggleDisplayType()
  {
    this.currentDisplay = (InfoPanel_Faction.DisplayType) this.currentDisplayDropdown.value;
    switch (this.currentDisplay)
    {
      case InfoPanel_Faction.DisplayType.Forces:
        this.SetDisplayForces();
        break;
      case InfoPanel_Faction.DisplayType.Players:
        this.SetDisplayPlayers();
        break;
      case InfoPanel_Faction.DisplayType.Reserves:
        this.SetDisplayReserves();
        break;
      case InfoPanel_Faction.DisplayType.Losses:
        this.SetDisplayLosses();
        break;
      case InfoPanel_Faction.DisplayType.Value:
        this.SetDisplayValue();
        break;
      case InfoPanel_Faction.DisplayType.Manpower:
        this.SetDisplayManpower();
        break;
    }
  }

  public void ResetLists()
  {
    if (this.airbases_list.childCount > 0)
    {
      for (int index = 1; index < this.airbases_list.childCount; ++index)
        UnityEngine.Object.Destroy((UnityEngine.Object) this.airbases_list.GetChild(index).gameObject);
    }
    if (this.players_list.childCount > 0)
    {
      foreach (Component players in this.players_list)
        UnityEngine.Object.Destroy((UnityEngine.Object) players.gameObject);
    }
    this.listAirbases.Clear();
    this.listPlayers.Clear();
  }

  public enum SelectFaction
  {
    Current,
    Other,
  }

  public enum DisplayType
  {
    Forces,
    Players,
    Reserves,
    Losses,
    Value,
    Manpower,
  }
}
