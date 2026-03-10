// Decompiled with JetBrains decompiler
// Type: AircraftSelectionMenu
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Cysharp.Threading.Tasks;
using NuclearOption.AddressableScripts;
using NuclearOption.MissionEditorScripts.Buttons;
using NuclearOption.Networking;
using NuclearOption.SavedMission;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#nullable disable
public class AircraftSelectionMenu : MonoBehaviour
{
  [SerializeField]
  private TMP_Text aircraftName;
  [SerializeField]
  private TMP_Text info;
  [SerializeField]
  private TMP_Text airbaseName;
  [SerializeField]
  private TMP_Text playerName;
  [SerializeField]
  private TMP_Text playerRank;
  [SerializeField]
  private TMP_Text playerAllocation;
  [SerializeField]
  private TMP_Text loadoutValue;
  [SerializeField]
  private TMP_Text loadoutWeight;
  [SerializeField]
  private TMP_Text fuelPercentage;
  [SerializeField]
  private TMP_Text fuelWeight;
  [SerializeField]
  private TMP_Text grossWeight;
  [SerializeField]
  private TMP_Text RCS;
  [SerializeField]
  private TMP_Text TWR;
  [SerializeField]
  private TMP_Text TWRLabel;
  [SerializeField]
  private TMP_Text warheads;
  [SerializeField]
  private TMP_Text warheadsAtAirbase;
  [SerializeField]
  private TMP_Text factionName;
  [SerializeField]
  private TMP_Text factionFunds;
  [SerializeField]
  private TMP_Text factionScore;
  [SerializeField]
  private TMP_Text escalationText;
  [SerializeField]
  private TMP_Text weaponSeeker;
  [SerializeField]
  private TMP_Text weaponRange;
  [SerializeField]
  private TMP_Text weaponAP;
  [SerializeField]
  private TMP_Text weaponHE;
  [SerializeField]
  private TMP_Text weaponRCS;
  [SerializeField]
  private TMP_Text weaponCost;
  [SerializeField]
  private TMP_Text donateLabel;
  [SerializeField]
  private Button flyButton;
  [SerializeField]
  private Button contributeButton;
  [SerializeField]
  private ShowHoverText warheadsHoverText;
  [SerializeField]
  private ShowHoverText donateHoverText;
  [SerializeField]
  private Slider fuelLevel;
  [SerializeField]
  private Transform weaponSelectionPanel;
  [SerializeField]
  private Transform reserveNoticePosition;
  [SerializeField]
  private Transform infoPanel;
  [SerializeField]
  private Transform conventionalStart;
  [SerializeField]
  private Transform tacticalStart;
  [SerializeField]
  private Transform strategicStart;
  [SerializeField]
  private Transform strategicEnd;
  [SerializeField]
  private GameObject weaponSelectionPrefab;
  [SerializeField]
  private GameObject groundSupportEquipmentPrefab;
  [SerializeField]
  private GameObject insufficientFunds;
  [SerializeField]
  private GameObject weaponImageArea;
  [SerializeField]
  private GameObject weaponInfoArea;
  [SerializeField]
  private GameObject reserveNotice;
  [SerializeField]
  private GameObject contributePanel;
  [SerializeField]
  private Image weaponImage;
  [SerializeField]
  private Image loadoutUnaffordable;
  [SerializeField]
  private Image overWeight;
  [SerializeField]
  private Image insufficientRank;
  [SerializeField]
  private Image insufficientWarheads;
  [SerializeField]
  private Image factionLogo;
  [SerializeField]
  private Image escalationPointer;
  [SerializeField]
  private Image escalationUnderline;
  [SerializeField]
  private GameObject liveryPanel;
  [SerializeField]
  private TMP_Dropdown liveryDropdown;
  [SerializeField]
  private Light selectionLight;
  [SerializeField]
  private GameObject noHangarsPanel;
  [SerializeField]
  private GameObject warheadsAtAirbasePanel;
  [SerializeField]
  private AircraftInventoryMenu aircraftInventoryMenu;
  private GameObject groundSupportEquipment;
  private WeaponManager weaponManager;
  private Airbase airbase;
  [SerializeField]
  private Transform[] aircraftPreview;
  private List<AircraftDefinition> aircraftSelection = new List<AircraftDefinition>();
  private AircraftDefinition selectedType;
  [NonSerialized]
  public List<WeaponSelector> weaponSelectors = new List<WeaponSelector>();
  private int selectionIndex;
  private Aircraft previewAircraft;
  private AircraftInfo aircraftInfo;
  private float loadoutCost;
  private readonly List<Rigidbody> previewRBs = new List<Rigidbody>();
  private readonly List<(LiveryKey key, string label)> liveryOptions = new List<(LiveryKey, string)>();
  private Color escalationBaseColor = Color.grey;
  private Color conventionalColor = Color.green;
  private Color tacticalColor = Color.yellow;
  private Color strategicColor = Color.red;
  private Player localPlayer;
  private bool canContribute;

  private LiveryKey CurrentLivery => this.liveryOptions[this.liveryDropdown.value].key;

  public event Action<AircraftDefinition> OnSelectedAircraftChange;

  public event Action<AircraftDefinition> OnAircraftOwnedChange;

  private void OnEnable()
  {
    SceneSingleton<DynamicMap>.i.DeselectAllIcons();
    DynamicMap.AllowedToOpen = false;
    GameplayUI.AllowPauseKeybind = false;
    SceneSingleton<CameraStateManager>.i.SetFollowingUnit((Unit) null);
    SceneSingleton<CameraStateManager>.i.Illuminator.enabled = true;
    MessageUI.SetFixedBoxSize();
    SceneSingleton<AllocationDisplay>.i.SetVisible(false);
  }

  private void SelectionMenu_OnAirbaseCapture() => this.ReturnToMap();

  public void Initialize(Player localPlayer, Airbase airbase)
  {
    this.localPlayer = localPlayer;
    this.aircraftInventoryMenu.Initialize(localPlayer);
    this.Refresh(airbase);
  }

  public void Refresh(Airbase airbase)
  {
    this.ClearEvents();
    SceneSingleton<ReserveReport>.i.SetPosition(this.reserveNoticePosition.position);
    this.fuelLevel.SetValueWithoutNotify(1f);
    airbase.onLostControl += new Action(this.SelectionMenu_OnAirbaseCapture);
    SceneSingleton<CameraStateManager>.i.FocusAirbase(airbase, false);
    this.airbaseName.text = airbase.SavedAirbase.DisplayName;
    this.playerName.text = this.localPlayer.PlayerName;
    this.playerRank.text = $"Rank {this.localPlayer.PlayerRank}";
    SceneSingleton<GameplayUI>.i.HideSelectAirbase();
    this.factionLogo.sprite = airbase.CurrentHQ.faction.factionColorLogo;
    this.factionName.text = airbase.CurrentHQ.faction.factionExtendedName;
    this.selectionIndex = 0;
    this.airbase = airbase;
    if (this.aircraftSelection == null)
      this.aircraftSelection = new List<AircraftDefinition>();
    if (this.aircraftSelection.Count > 0)
      this.aircraftSelection.Clear();
    this.DestroyPreviewAircraft();
    foreach (AircraftDefinition aircraftDefinition in airbase.GetAvailableAircraft())
    {
      if (!airbase.CurrentHQ.restrictedAircraft.Contains(aircraftDefinition.unitPrefab.name))
        this.aircraftSelection.Add(aircraftDefinition);
    }
    if (this.aircraftSelection.Count > 0)
    {
      this.aircraftSelection.Sort((Comparison<AircraftDefinition>) ((a, b) => a.aircraftParameters.rankRequired.CompareTo(b.aircraftParameters.rankRequired)));
      this.aircraftName.text = this.aircraftSelection[this.selectionIndex].unitName ?? "";
      this.info.text = this.aircraftSelection[this.selectionIndex].aircraftParameters.aircraftDescription;
      this.RefreshLiveryOptions(true);
      this.SpawnPreview();
    }
    else
    {
      this.liveryPanel.SetActive(false);
      this.aircraftName.text = "No aircraft available";
      this.info.text = "There are no operational hangars at this airbase";
      this.flyButton.interactable = false;
      this.contributePanel.SetActive(false);
    }
    SceneSingleton<GameplayUI>.i.menuCanvas.enabled = true;
    SceneSingleton<GameplayUI>.i.gameplayCanvas.enabled = true;
    CursorManager.SetFlag(CursorFlags.SelectionMenu, true);
    this.aircraftInventoryMenu.SetSelectedType(this.selectedType);
    this.DisplayLoadoutInfo();
  }

  public void GoToNextAirbase()
  {
    if ((UnityEngine.Object) this.airbase.CurrentHQ == (UnityEngine.Object) null)
      return;
    List<Airbase> list = this.airbase.CurrentHQ.GetAirbases().ToList<Airbase>();
    if (list.Count == 1)
      return;
    int index = list.IndexOf(this.airbase) + 1;
    if (index > list.Count - 1)
      index = 0;
    this.Refresh(list[index]);
  }

  public void GoToPreviousAirbase()
  {
    if ((UnityEngine.Object) this.airbase.CurrentHQ == (UnityEngine.Object) null)
      return;
    List<Airbase> list = this.airbase.CurrentHQ.GetAirbases().ToList<Airbase>();
    if (list.Count == 1)
      return;
    int index = list.IndexOf(this.airbase) - 1;
    if (index < 0)
      index = list.Count - 1;
    this.Refresh(list[index]);
  }

  private void ShowHardpoints()
  {
    foreach (Component weaponSelector in this.weaponSelectors)
      UnityEngine.Object.Destroy((UnityEngine.Object) weaponSelector.gameObject);
    this.weaponSelectors.Clear();
    if (!((UnityEngine.Object) this.weaponManager != (UnityEngine.Object) null))
      return;
    for (int index = 0; index < this.weaponManager.hardpointSets.Length; ++index)
    {
      HardpointSet hardpointSet = this.weaponManager.hardpointSets[index];
      WeaponSelector component = UnityEngine.Object.Instantiate<GameObject>(this.weaponSelectionPrefab, this.weaponSelectionPanel).GetComponent<WeaponSelector>();
      component.Initialize(hardpointSet, this.airbase.CurrentHQ, this.airbase);
      component.OnWeaponSelected += (Action<WeaponMount>) (mount =>
      {
        this.UpdateWeapons(true);
        this.SaveDefaults();
        if (!((UnityEngine.Object) mount != (UnityEngine.Object) null))
          return;
        this.DisplayInfo(mount.info);
      });
      this.weaponSelectors.Add(component);
    }
    this.infoPanel.SetAsLastSibling();
  }

  private void RefreshLiveryOptions(bool selectRandom)
  {
    AircraftSelectionMenu.RefreshLiveryOptions(this.liveryDropdown, this.liveryOptions, this.aircraftSelection[this.selectionIndex], this.airbase.CurrentHQ.faction.factionName, true, selectRandom);
  }

  public static void RefreshLiveryOptions(
    TMP_Dropdown liveryDropdown,
    List<(LiveryKey key, string label)> liveryOptions,
    AircraftDefinition aircraft,
    string aircraftFaction,
    bool allowFactionLivery,
    bool selectRandom)
  {
    AircraftSelectionMenu.GetLiveryOptions(liveryOptions, aircraft, aircraftFaction, allowFactionLivery);
    liveryDropdown.ClearOptions();
    foreach ((LiveryKey key, string label) liveryOption in liveryOptions)
      liveryDropdown.options.Add(new TMP_Dropdown.OptionData(liveryOption.label));
    if (!(liveryOptions.Count > 0 & selectRandom))
      return;
    liveryDropdown.SetValueWithoutNotify(0);
    liveryDropdown.RefreshShownValue();
  }

  public static void GetLiveryOptions(
    List<(LiveryKey key, string label)> resultsList,
    AircraftDefinition aircraft,
    string aircraftFaction,
    bool allowFactionLivery)
  {
    resultsList.Clear();
    AircraftParameters aircraftParameters = aircraft.aircraftParameters;
    for (int index = 0; index < aircraftParameters.liveries.Count; ++index)
    {
      AircraftParameters.Livery livery = aircraftParameters.liveries[index];
      if (ValidFaction(livery.faction?.factionName))
        resultsList.Add((new LiveryKey(index), livery.name));
    }
    if (!ModLoadCache.HasSkinMetaData || GameManager.gameState == GameState.Editor)
    {
      ModLoadCache.SkinMetaData.Clear();
      ModLoadCache.SkinMetaData.AddRange(ModFolders.AppDataSkins.ListMetaData());
      ModLoadCache.SkinMetaData.AddRange(ModFolders.WorkshopSkins.ListMetaData());
      ModLoadCache.HasSkinMetaData = true;
    }
    foreach (LiveryMetaData metaData in ModLoadCache.SkinMetaData)
    {
      if (metaData.Aircraft == aircraft.unitName && ValidFaction(metaData.Faction))
      {
        bool workshop = metaData.Id != PublishedFileId_t.Invalid;
        string str = workshop ? " (workshop)" : " (app data)";
        resultsList.Add((new LiveryKey(metaData, workshop), metaData.DisplayName + str));
      }
    }

    bool ValidFaction(string liveryFaction)
    {
      if (FactionHelper.EmptyOrNoFactionOrNeutral(liveryFaction))
        return true;
      if (!allowFactionLivery)
        return false;
      return FactionHelper.EmptyOrNoFactionOrNeutral(aircraftFaction) || liveryFaction == aircraftFaction;
    }
  }

  public void SelectLivery() => this.previewAircraft.SetLiveryKey(this.CurrentLivery, true);

  private Loadout GenerateLoadoutFromDropdowns()
  {
    Loadout loadoutFromDropdowns = new Loadout();
    for (int index = 0; index < this.weaponSelectors.Count; ++index)
      loadoutFromDropdowns.weapons.Add(this.weaponSelectors[index].GetValue());
    return loadoutFromDropdowns;
  }

  public void SaveDefaults()
  {
    if (!GameManager.aircraftCustomization.ContainsKey(this.aircraftSelection[this.selectionIndex]))
    {
      AircraftCustomization aircraftCustomization = new AircraftCustomization(this.GenerateLoadoutFromDropdowns(), this.fuelLevel.value, this.liveryDropdown.value);
      GameManager.aircraftCustomization.Add(this.aircraftSelection[this.selectionIndex], aircraftCustomization);
    }
    else
      GameManager.aircraftCustomization[this.aircraftSelection[this.selectionIndex]].Update(this.GenerateLoadoutFromDropdowns(), this.fuelLevel.value, this.liveryDropdown.value);
  }

  private void LoadDefaults()
  {
    AircraftCustomization aircraftCustomization;
    if (!GameManager.aircraftCustomization.TryGetValue(this.aircraftSelection[this.selectionIndex], out aircraftCustomization))
      aircraftCustomization = new AircraftCustomization(this.aircraftSelection[this.selectionIndex].aircraftParameters.loadouts[1], this.aircraftSelection[this.selectionIndex].aircraftParameters.DefaultFuelLevel, Mathf.FloorToInt((float) UnityEngine.Random.Range(0, this.liveryOptions.Count)));
    for (int index = 0; index < aircraftCustomization.loadout.weapons.Count; ++index)
    {
      WeaponMount weapon = aircraftCustomization.loadout.weapons[index];
      this.weaponSelectors[index].SetValue(weapon);
    }
    this.UpdateWeapons(false);
    this.liveryDropdown.SetValueWithoutNotify(aircraftCustomization.livery);
    this.previewAircraft.SetLiveryKey(this.CurrentLivery, true);
    this.fuelLevel.SetValueWithoutNotify(aircraftCustomization.fuelLevel);
    this.ChangeFuelLevel();
  }

  public void ChangeFuelLevel()
  {
    if ((UnityEngine.Object) this.previewAircraft != (UnityEngine.Object) null)
    {
      this.previewAircraft.NetworkfuelLevel = this.fuelLevel.value;
      this.previewAircraft.Refuel((Unit) null);
    }
    this.fuelPercentage.text = $"{(ValueType) (float) ((double) this.fuelLevel.value * 100.0):F0}%";
    this.DisplayLoadoutInfo();
  }

  public void UpdateWeapons(bool respawnWeapons)
  {
    this.previewAircraft.Networkloadout = this.GenerateLoadoutFromDropdowns();
    if (respawnWeapons)
      this.weaponManager.SpawnWeapons();
    foreach (WeaponSelector weaponSelector in this.weaponSelectors)
      weaponSelector.SetInteractable(this.previewAircraft.loadout);
    this.DisplayLoadoutInfo();
  }

  private void DisplayLoadoutInfo()
  {
    if ((UnityEngine.Object) this.previewAircraft == (UnityEngine.Object) null)
      return;
    this.loadoutCost = this.weaponManager.GetCurrentValue(true);
    this.loadoutValue.text = UnitConverter.ValueReading(this.loadoutCost);
    this.loadoutWeight.text = UnitConverter.WeightReading(this.weaponManager.GetCurrentMass());
    float weight = 0.0f;
    foreach (UnitPart allPart in this.previewAircraft.GetAllParts())
      weight += allPart.mass;
    this.TWR.text = "0";
    float maxPower;
    if (this.previewAircraft.GetMaxPower(out maxPower))
    {
      this.ShowPWR(maxPower, weight);
    }
    else
    {
      float maxThrust;
      if (this.previewAircraft.GetMaxThrust(out maxThrust))
        this.ShowTWR(maxThrust, weight);
    }
    this.fuelWeight.text = UnitConverter.WeightReading(this.previewAircraft.GetFuelQuantity()) ?? "";
  }

  private void ShowTWR(float thrust, float weight)
  {
    this.TWR.text = $"{(ValueType) (float) ((double) thrust / ((double) weight * 9.8100004196167)):F2}";
    this.TWRLabel.text = "T/W";
  }

  private void ShowPWR(float power, float weight)
  {
    this.TWR.text = UnitConverter.PowerToWeightReading(power * (1f / 1000f) / weight);
    this.TWRLabel.text = "P/W";
  }

  public void DisplayInfo(WeaponInfo weaponInfo)
  {
    if ((UnityEngine.Object) weaponInfo == (UnityEngine.Object) null)
    {
      this.weaponImageArea.SetActive(false);
      this.weaponInfoArea.SetActive(false);
      this.info.text = (UnityEngine.Object) this.previewAircraft != (UnityEngine.Object) null ? this.aircraftSelection[this.selectionIndex].aircraftParameters.aircraftDescription : "Nothing Selected";
    }
    else
    {
      if ((UnityEngine.Object) weaponInfo.weaponIcon != (UnityEngine.Object) null)
      {
        this.weaponImageArea.SetActive(true);
        this.weaponImage.sprite = weaponInfo.weaponIcon;
        this.weaponInfoArea.SetActive(true);
        if (!weaponInfo.cargo && !weaponInfo.troops)
        {
          this.weaponSeeker.text = (UnityEngine.Object) weaponInfo.weaponPrefab != (UnityEngine.Object) null ? weaponInfo.weaponPrefab.GetComponent<MissileSeeker>().GetSeekerType() : (weaponInfo.energy ? "Laser" : "Gun");
          this.weaponRange.text = "R: " + UnitConverter.DistanceReading(weaponInfo.targetRequirements.maxRange);
          this.weaponAP.text = (UnityEngine.Object) weaponInfo.weaponPrefab != (UnityEngine.Object) null ? $"AP: {weaponInfo.weaponPrefab.GetComponent<Missile>().GetPierce()}" : $"AP: {weaponInfo.pierceDamage}";
          this.weaponHE.text = (UnityEngine.Object) weaponInfo.weaponPrefab != (UnityEngine.Object) null ? "HE: " + UnitConverter.YieldReading(weaponInfo.weaponPrefab.GetComponent<Missile>().GetYield()) : "HE: " + UnitConverter.YieldReading(weaponInfo.blastDamage);
          this.weaponRCS.text = (UnityEngine.Object) weaponInfo.weaponPrefab != (UnityEngine.Object) null ? $"RCS: {weaponInfo.weaponPrefab.GetComponent<Missile>().definition.radarSize}" : "-";
          this.weaponCost.text = (UnityEngine.Object) weaponInfo.weaponPrefab != (UnityEngine.Object) null ? "C: " + UnitConverter.ValueReading(weaponInfo.costPerRound) : "-";
        }
        else
        {
          this.weaponSeeker.text = "Cargo";
          this.weaponRange.text = "-";
          this.weaponAP.text = "-";
          this.weaponHE.text = "-";
          this.weaponRCS.text = "M: " + UnitConverter.WeightReading(weaponInfo.massPerRound);
          this.weaponCost.text = "C: " + UnitConverter.ValueReading(weaponInfo.costPerRound);
        }
      }
      else
      {
        this.weaponImageArea.SetActive(false);
        this.weaponInfoArea.SetActive(false);
      }
      this.info.text = weaponInfo.description;
    }
  }

  private void DisplayEscalationPointer()
  {
    float currentEscalation = NetworkSceneSingleton<MissionManager>.i.currentEscalation;
    float tacticalThreshold = NetworkSceneSingleton<MissionManager>.i.tacticalThreshold;
    float strategicThreshold = NetworkSceneSingleton<MissionManager>.i.strategicThreshold;
    if ((double) currentEscalation < (double) tacticalThreshold)
    {
      float t = Mathf.InverseLerp(0.0f, tacticalThreshold, currentEscalation);
      this.escalationPointer.transform.position = Vector3.Lerp(this.conventionalStart.position, this.tacticalStart.position, t);
      this.escalationPointer.color = Color.Lerp(this.escalationBaseColor, this.tacticalColor, t);
      this.escalationText.color = this.escalationPointer.color;
      this.escalationUnderline.color = this.escalationPointer.color;
      this.escalationText.text = "NON-NUCLEAR";
    }
    else if ((double) currentEscalation < (double) strategicThreshold)
    {
      float t = Mathf.InverseLerp(tacticalThreshold, strategicThreshold, currentEscalation);
      this.escalationPointer.transform.position = Vector3.Lerp(this.tacticalStart.position, this.strategicStart.position, t);
      this.escalationPointer.color = Color.Lerp(this.tacticalColor, this.strategicColor, t);
      this.escalationText.color = this.escalationPointer.color;
      this.escalationUnderline.color = this.escalationPointer.color;
      this.escalationText.text = "TACTICAL NUCLEAR";
    }
    else
    {
      this.escalationPointer.transform.position = Vector3.Lerp(this.strategicStart.position, this.strategicEnd.position, Mathf.InverseLerp(strategicThreshold, 2f * strategicThreshold, currentEscalation));
      this.escalationPointer.color = this.strategicColor;
      this.escalationText.color = this.strategicColor;
      this.escalationUnderline.color = this.strategicColor;
      this.escalationText.text = "STRATEGIC NUCLEAR";
    }
  }

  private void CheckRank()
  {
    if (this.aircraftSelection[this.selectionIndex].aircraftParameters.rankRequired > this.localPlayer.PlayerRank)
      this.flyButton.interactable = false;
    else
      this.flyButton.interactable = true;
  }

  private void SpawnPreview()
  {
    RaycastHit hitInfo1;
    if (!Physics.Linecast(this.airbase.aircraftSelectionTransform.position + Vector3.up * 5f, this.airbase.aircraftSelectionTransform.position - Vector3.up * 100f, out hitInfo1, 2112))
      return;
    this.warheadsHoverText.SetText("Available warheads at this airbase");
    if ((UnityEngine.Object) this.groundSupportEquipment == (UnityEngine.Object) null)
    {
      this.groundSupportEquipment = UnityEngine.Object.Instantiate<GameObject>(this.groundSupportEquipmentPrefab, hitInfo1.point, Quaternion.LookRotation(this.airbase.aircraftSelectionTransform.forward, hitInfo1.normal));
      this.groundSupportEquipment.transform.SetParent(hitInfo1.collider.transform);
      foreach (Transform componentsInChild in this.groundSupportEquipment.GetComponentsInChildren<Transform>())
      {
        RaycastHit hitInfo2;
        if (Physics.Linecast(componentsInChild.position + componentsInChild.up, componentsInChild.position - componentsInChild.up, out hitInfo2))
          componentsInChild.SetPositionAndRotation(hitInfo2.point, Quaternion.LookRotation(componentsInChild.forward, hitInfo2.normal));
      }
    }
    Vector3 vector3 = (UnityEngine.Object) hitInfo1.collider.attachedRigidbody != (UnityEngine.Object) null ? hitInfo1.collider.attachedRigidbody.GetPointVelocity(hitInfo1.point) : Vector3.zero;
    GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.aircraftSelection[this.selectionIndex].unitPrefab, hitInfo1.point + Vector3.up * this.aircraftSelection[this.selectionIndex].spawnOffset.y, this.airbase.aircraftSelectionTransform.rotation);
    gameObject.GetComponent<Rigidbody>().velocity = vector3;
    this.previewAircraft = gameObject.GetComponentInChildren<Aircraft>();
    this.previewAircraft.networked = false;
    this.previewAircraft.NetworkHQ = this.localPlayer.HQ;
    this.weaponManager = this.previewAircraft.weaponManager;
    this.aircraftPreview = gameObject.GetComponentsInChildren<Transform>();
    this.previewAircraft.SetComplexPhysics();
    this.previewAircraft.SetGear(LandingGear.GearState.LockedExtended);
    this.previewAircraft.rb.interpolation = RigidbodyInterpolation.Interpolate;
    foreach (Pilot pilot in this.previewAircraft.pilots)
      pilot.TogglePilotVisibility(false);
    SceneSingleton<CameraStateManager>.i.selectionState.SetPreviewAircraft(this.previewAircraft);
    this.selectedType = this.previewAircraft.definition;
    this.aircraftInfo = this.selectedType.aircraftInfo;
    this.ShowHardpoints();
    this.LoadDefaults();
    this.DisplayInfo((WeaponInfo) null);
    if ((UnityEngine.Object) this.weaponManager != (UnityEngine.Object) null)
      this.weaponManager.InitializeWeaponManager();
    this.previewRBs.Clear();
    foreach (Transform transform in this.aircraftPreview)
    {
      transform.gameObject.layer = LayerMask.NameToLayer("Ignore Collisions");
      Rigidbody component = transform.GetComponent<Rigidbody>();
      if (!((UnityEngine.Object) component == (UnityEngine.Object) null) && !this.previewRBs.Contains(component))
        this.previewRBs.Add(component);
    }
    this.previewAircraft.OpenCanopies();
    this.previewAircraft.ShowGroundEquipment();
    this.info.text = this.aircraftSelection[this.selectionIndex].aircraftParameters.aircraftDescription;
    this.DisplayLoadoutInfo();
    Action<AircraftDefinition> selectedAircraftChange = this.OnSelectedAircraftChange;
    if (selectedAircraftChange == null)
      return;
    selectedAircraftChange(this.selectedType);
  }

  public void FlyAircraft() => this.FlyAircraftAsync().Forget();

  private async UniTask FlyAircraftAsync()
  {
    if ((UnityEngine.Object) this.airbase == (UnityEngine.Object) null)
      return;
    AircraftDefinition definition = this.aircraftSelection[this.selectionIndex];
    if (!this.airbase.CanSpawnAircraft(definition))
    {
      this.noHangarsPanel.SetActive(true);
    }
    else
    {
      this.GenerateLoadoutFromDropdowns();
      if (await NetworkSceneSingleton<Spawner>.i.RequestSpawnAtAirbase(this.airbase, definition, this.CurrentLivery, this.GenerateLoadoutFromDropdowns(), this.fuelLevel.value))
        this.HideSelection();
      else
        this.noHangarsPanel.SetActive(true);
    }
  }

  private void DestroyPreviewAircraft()
  {
    if (this.aircraftPreview.Length == 0)
      return;
    foreach (Transform transform in this.aircraftPreview)
    {
      if ((UnityEngine.Object) transform != (UnityEngine.Object) null)
        UnityEngine.Object.Destroy((UnityEngine.Object) transform.gameObject);
    }
  }

  private void HideSelection()
  {
    this.DestroyPreviewAircraft();
    DynamicMap.AllowedToOpen = true;
    if (!((UnityEngine.Object) this != (UnityEngine.Object) null))
      return;
    UnityEngine.Object.Destroy((UnityEngine.Object) this.gameObject);
  }

  private void Update()
  {
    if ((UnityEngine.Object) this.previewAircraft == (UnityEngine.Object) null || CameraStateManager.cameraMode != CameraMode.selection)
      this.flyButton.interactable = false;
    else if (this.airbase.UnitDestroyed())
    {
      this.ReturnToMap();
    }
    else
    {
      this.CheckContribute();
      this.contributeButton.interactable = this.canContribute;
      this.previewAircraft.GetInputs().brake = 1f;
      this.RCS.text = $"{this.previewAircraft.RCS:F4}";
      int warheads = this.airbase.GetWarheads();
      bool flag = (double) NetworkSceneSingleton<MissionManager>.i.currentEscalation >= (double) NetworkSceneSingleton<MissionManager>.i.tacticalThreshold;
      this.warheads.text = $"{this.airbase.CurrentHQ.GetWarheadStockpile()}";
      this.warheadsAtAirbasePanel.SetActive(flag);
      this.warheadsAtAirbase.text = $"   x {warheads}";
      this.factionFunds.text = UnitConverter.ValueReading(this.airbase.CurrentHQ.factionFunds) ?? "";
      this.factionScore.text = $"{this.airbase.CurrentHQ.factionScore:F1}";
      this.DisplayEscalationPointer();
      if ((UnityEngine.Object) this.selectedType != (UnityEngine.Object) null)
      {
        this.playerAllocation.text = $"${this.localPlayer.Allocation:F2}m";
        this.insufficientFunds.SetActive((double) this.localPlayer.Allocation < (double) this.selectedType.value);
        this.loadoutUnaffordable.enabled = (double) this.localPlayer.Allocation < (double) this.loadoutCost;
        this.insufficientRank.enabled = this.localPlayer.PlayerRank < this.selectedType.aircraftParameters.rankRequired;
        this.CheckWarheads();
        float weight = 0.0f;
        foreach (UnitPart allPart in this.previewAircraft.GetAllParts())
          weight += allPart.mass;
        float maxWeight = this.aircraftInfo.maxWeight;
        this.grossWeight.text = $"{UnitConverter.WeightReading(weight)} / {UnitConverter.WeightReading(maxWeight)}";
        this.overWeight.enabled = (double) weight > (double) maxWeight;
        this.aircraftName.text = this.aircraftSelection[this.selectionIndex].unitName ?? "";
        if (this.localPlayer.OwnsAirframe(this.selectedType, true))
          this.flyButton.interactable = !this.loadoutUnaffordable.enabled && !this.insufficientWarheads.enabled;
        else
          this.flyButton.interactable = false;
      }
      else
      {
        this.aircraftName.text = "No aircraft available";
        this.loadoutUnaffordable.enabled = false;
        this.overWeight.enabled = false;
        this.insufficientRank.enabled = false;
      }
      this.selectionLight.enabled = (double) NetworkSceneSingleton<LevelInfo>.i.timeOfDay < 6.0 || (double) NetworkSceneSingleton<LevelInfo>.i.timeOfDay > 18.0;
      this.selectionLight.transform.position = this.previewAircraft.transform.position + Vector3.up * 20f + Vector3.forward * 10f;
      this.selectionLight.transform.LookAt(this.previewAircraft.transform.position);
      if (!GameManager.playerInput.GetButton("Pause") && !Input.GetKeyDown(KeyCode.Escape) && !((UnityEngine.Object) this.localPlayer.HQ != (UnityEngine.Object) this.airbase.CurrentHQ))
        return;
      this.ReturnToMap();
    }
  }

  private void CheckWarheads()
  {
    int currentWarheads = this.previewAircraft.weaponManager.GetCurrentWarheads();
    this.warheadsHoverText.SetText("Number of warheads available to this faction");
    this.insufficientWarheads.enabled = false;
    if (!this.airbase.HasStorage())
      this.warheadsHoverText.SetText("No warheads storage at this airbase");
    else if (this.airbase.GetWarheads() == 0)
      this.warheadsHoverText.SetText("No warheads available at this airbase");
    else if (currentWarheads > 0 && this.airbase.GetWarheads() < currentWarheads)
    {
      this.warheadsHoverText.SetText("Not enough warheads available at this airbase");
      this.insufficientWarheads.enabled = true;
    }
    else if (MissionManager.AllowTactical() && this.localPlayer.PlayerRank < MissionManager.CurrentMission.missionSettings.minRankTacticalWarhead)
    {
      this.warheadsHoverText.SetText($"Minimum rank {MissionManager.CurrentMission.missionSettings.minRankTacticalWarhead} required");
    }
    else
    {
      if (!MissionManager.AllowStrategic() || this.localPlayer.PlayerRank >= MissionManager.CurrentMission.missionSettings.minRankStrategicWarhead)
        return;
      this.warheadsHoverText.SetText("Only tactical warhead allowed at your rank");
    }
  }

  public void ReturnToMap()
  {
    this.HideSelection();
    SceneSingleton<CameraStateManager>.i.transform.position = this.airbase.aircraftSelectionTransform.transform.position + Vector3.up * 2f;
    this.aircraftPreview = new Transform[0];
    SceneSingleton<CameraStateManager>.i.SwitchState((CameraBaseState) SceneSingleton<CameraStateManager>.i.freeState);
    SceneSingleton<DynamicMap>.i.mapBackground.gameObject.SetActive(true);
    SceneSingleton<DynamicMap>.i.Maximize();
  }

  private void OnDestroy()
  {
    this.ClearEvents();
    UnityEngine.Object.Destroy((UnityEngine.Object) this.groundSupportEquipment);
    SceneSingleton<ReserveReport>.i.SetPosition(SceneSingleton<GameplayUI>.i.topPanelTransform.position - Vector3.up * 50f);
    MessageUI.SetDynamicBoxSize();
    SceneSingleton<AllocationDisplay>.i.SetVisible(true);
    GameplayUI.AllowPauseKeybind = true;
    CursorManager.SetFlag(CursorFlags.SelectionMenu, false);
    if ((UnityEngine.Object) SceneSingleton<CameraStateManager>.i.Illuminator != (UnityEngine.Object) null)
      SceneSingleton<CameraStateManager>.i.Illuminator.enabled = false;
    if (!((UnityEngine.Object) SceneSingleton<CameraStateManager>.i.mainCamera != (UnityEngine.Object) null))
      return;
    SceneSingleton<CameraStateManager>.i.mainCamera.nearClipPlane = 1f;
  }

  private void ClearEvents()
  {
    if (!((UnityEngine.Object) this.airbase != (UnityEngine.Object) null))
      return;
    this.airbase.onLostControl -= new Action(this.SelectionMenu_OnAirbaseCapture);
  }

  public void SetSelectedType(AircraftDefinition definition)
  {
    this.selectionIndex = this.aircraftSelection.IndexOf(definition);
    this.RefreshLiveryOptions(true);
    this.DestroyPreviewAircraft();
    this.aircraftPreview = new Transform[0];
    this.SpawnPreview();
  }

  public void AircraftManagementToggle()
  {
    this.aircraftInventoryMenu.gameObject.SetActive(!this.aircraftInventoryMenu.gameObject.activeSelf);
  }

  public AircraftDefinition GetSelectedType() => this.selectedType;

  public bool CanFlyAircraft(AircraftDefinition definition)
  {
    return !((UnityEngine.Object) definition == (UnityEngine.Object) null) && this.aircraftSelection.Contains(definition);
  }

  public async void CheckContribute()
  {
    float delayContribute = await this.localPlayer.CmdGetDelayContribute();
    this.canContribute = (double) delayContribute <= 0.0;
    string text = "Contribute funds or vehicles to the war effort";
    string str = "DONATE";
    if (!this.canContribute)
    {
      text = $"Wait {delayContribute:F0}s until next contribution";
      str = $"Wait {delayContribute:F0}s";
    }
    this.donateHoverText.SetText(text);
    this.donateLabel.text = str;
  }
}
