// Decompiled with JetBrains decompiler
// Type: NuclearOption.MissionEditorScripts.AirbasePanel
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.SavedMission;
using NuclearOption.SavedMission.ObjectiveV2;
using NuclearOption.SavedMission.ObjectiveV2.Objectives;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

#nullable disable
namespace NuclearOption.MissionEditorScripts;

public class AirbasePanel : MonoBehaviour
{
  private static readonly string builtInAirbaseMessage = "default airbase, some value can not be changed";
  private static readonly string attachedAirbaseMessage = "attached airbase, some value can not be changed";
  [SerializeField]
  private Button removeButton;
  [SerializeField]
  private TextMeshProUGUI removeText;
  [SerializeField]
  private TextMeshProUGUI builtInNotice;
  [SerializeField]
  private TMP_InputField airbaseUniqueName;
  [SerializeField]
  private TMP_InputField airbaseDisplayName;
  [SerializeField]
  private TMP_Dropdown factionDropdown;
  [SerializeField]
  private Toggle disableToggle;
  [SerializeField]
  private Toggle capturableToggle;
  [SerializeField]
  private Slider captureDefenseSlider;
  [SerializeField]
  private TextMeshProUGUI captureDefenseSliderLabel;
  [SerializeField]
  private Slider captureRangeSlider;
  [SerializeField]
  private TextMeshProUGUI captureRangeSliderLabel;
  [SerializeField]
  private Color captureRangeSliderLabelNormalColor;
  [SerializeField]
  private Color captureRangeSliderLabelInactiveColor;
  [SerializeField]
  private Image captureRangeSliderFill;
  [SerializeField]
  private Color captureRangeSliderFillNormalColor;
  [SerializeField]
  private Color captureRangeSliderFillInactiveColor;
  [SerializeField]
  private Button roadButton;
  [SerializeField]
  private Button unitButton;
  [Header("custom airbase fields")]
  [SerializeField]
  private Vector3DataField centerPositionField;
  [SerializeField]
  private Vector3DataField selectionPositionField;
  [SerializeField]
  private ReferenceDataField towerReferenceField;
  [SerializeField]
  private ReferenceList buildingList;
  [SerializeField]
  private RectTransform dataDrawerParent;
  [SerializeField]
  private ReferenceList runwayList;
  [Header("References")]
  [SerializeField]
  private UIPrefabs uiPrefabs;
  [SerializeField]
  private PositionHandle positionHandlePrefab;
  [SerializeField]
  private RoadEditor roadEditorPrefab;
  [SerializeField]
  private RunwayTab runwayTabPrefab;
  private PositionHandle centerHandle;
  private PositionHandle selectionHandle;
  private EditorTabs editorTabs;
  private DataDrawer dataDrawer;
  private List<string> factionOptions;
  private Dictionary<ValueWrapperGlobalPosition, PositionHandle> verticalLandingHandles = new Dictionary<ValueWrapperGlobalPosition, PositionHandle>();
  private Dictionary<ValueWrapperGlobalPosition, PositionHandle> servicePointsHandles = new Dictionary<ValueWrapperGlobalPosition, PositionHandle>();
  private Airbase airbase;
  private SavedAirbase savedAirbase;

  private void Awake()
  {
    this.editorTabs = this.GetComponentInParent<EditorTabs>();
    this.dataDrawer = new DataDrawer(this.dataDrawerParent, this.uiPrefabs);
    this.dataDrawer.Width = new float?(this.dataDrawerParent.sizeDelta.x);
    this.removeButton.onClick.AddListener(new UnityAction(this.RemoveAirbase));
    this.airbaseUniqueName.onEndEdit.AddListener(new UnityAction<string>(this.AirbaseUniqueNameChanged));
    this.airbaseDisplayName.onEndEdit.AddListener(new UnityAction<string>(this.AirbaseDisplayNameChanged));
    this.factionDropdown.onValueChanged.AddListener(new UnityAction<int>(this.FactionChanged));
    this.disableToggle.onValueChanged.AddListener(new UnityAction<bool>(this.DisabledChanged));
    this.capturableToggle.onValueChanged.AddListener(new UnityAction<bool>(this.CapturableChanged));
    this.captureDefenseSlider.minValue = 1f;
    this.captureDefenseSlider.maxValue = 1000f;
    this.captureDefenseSlider.onValueChanged.AddListener(new UnityAction<float>(this.CaptureDefenseChanged));
    this.captureRangeSlider.minValue = 10f;
    this.captureRangeSlider.maxValue = 10000f;
    this.captureRangeSlider.onValueChanged.AddListener(new UnityAction<float>(this.CaptureRangeChanged));
    this.roadButton.onClick.AddListener(new UnityAction(this.RoadButtonClicked));
    this.unitButton.onClick.AddListener(new UnityAction(this.UnitButtonClicked));
  }

  private void Start()
  {
    this.PopulateFactionOptions();
    this.buildingList.TitleText.text = "Buildings";
    this.buildingList.SelectExistingDropdown.HideUnitTypeFilter();
    this.AddNoAirbaseFilterToggle();
    this.buildingList.ItemAdded += new Action<ISaveableReference>(this.BuildListItemAdded);
    this.buildingList.ItemRemoved += new Action<ISaveableReference>(this.BuildListItemRemoved);
    this.runwayList.TitleText.text = "Runways";
    this.runwayList.SetHeight(300);
    SceneSingleton<UnitSelection>.i.OnSelect += new UnitSelection.SelectionChanged<SelectionDetails>(this.AirbaseMenu_OnSelect);
    this.Setup(SceneSingleton<UnitSelection>.i.SelectionDetails is AirbaseSelectionDetails selectionDetails ? selectionDetails.Airbase : (Airbase) null);
  }

  private void AddNoAirbaseFilterToggle()
  {
    ReferencePopup existingDropdown = this.buildingList.SelectExistingDropdown;
    FilterSet.SetupToggleFilter(UnityEngine.Object.Instantiate<BoolDataField>(this.dataDrawer.Prefabs.BoolFieldPrefab, existingDropdown.filterHolder.transform), "noAirbase", "No Airbase", true, existingDropdown.FilterSet, new FilterSet.Filter(NoAirbase));

    static bool NoAirbase(object savedRef)
    {
      return string.IsNullOrEmpty(((SavedBuilding) savedRef).Airbase);
    }
  }

  private void OnDestroy()
  {
    this.Cleanup();
    SceneSingleton<UnitSelection>.i.OnSelect -= new UnitSelection.SelectionChanged<SelectionDetails>(this.AirbaseMenu_OnSelect);
  }

  private void Cleanup()
  {
    if ((UnityEngine.Object) this.centerHandle != (UnityEngine.Object) null)
      UnityEngine.Object.Destroy((UnityEngine.Object) this.centerHandle.gameObject);
    if ((UnityEngine.Object) this.selectionHandle != (UnityEngine.Object) null)
      UnityEngine.Object.Destroy((UnityEngine.Object) this.selectionHandle.gameObject);
    foreach (PositionHandle positionHandle in this.verticalLandingHandles.Values)
    {
      if ((UnityEngine.Object) positionHandle != (UnityEngine.Object) null)
        UnityEngine.Object.Destroy((UnityEngine.Object) positionHandle.gameObject);
    }
    this.verticalLandingHandles.Clear();
    foreach (PositionHandle positionHandle in this.servicePointsHandles.Values)
    {
      if ((UnityEngine.Object) positionHandle != (UnityEngine.Object) null)
        UnityEngine.Object.Destroy((UnityEngine.Object) positionHandle.gameObject);
    }
    this.servicePointsHandles.Clear();
  }

  public void Setup(Airbase airbase)
  {
    this.Cleanup();
    this.airbase = airbase;
    this.savedAirbase = (UnityEngine.Object) airbase != (UnityEngine.Object) null ? airbase.SavedAirbase : (SavedAirbase) null;
    bool flag1;
    bool flag2;
    bool flag3;
    bool flag4;
    bool flag5;
    if ((UnityEngine.Object) airbase == (UnityEngine.Object) null)
    {
      flag1 = false;
      flag2 = false;
      flag3 = false;
      flag4 = false;
      flag5 = false;
    }
    else
    {
      flag1 = true;
      flag2 = airbase.BuiltIn;
      flag3 = airbase.AttachedAirbase;
      flag4 = !flag2 && !flag3;
      flag5 = airbase.SavedAirbaseOverride;
      int num = flag4 ? 1 : 0;
    }
    if (flag2)
    {
      this.builtInNotice.text = AirbasePanel.builtInAirbaseMessage;
      this.builtInNotice.gameObject.SetActive(true);
    }
    else if (flag3)
    {
      this.builtInNotice.text = AirbasePanel.attachedAirbaseMessage;
      this.builtInNotice.gameObject.SetActive(true);
    }
    else
      this.builtInNotice.gameObject.SetActive(false);
    this.airbaseUniqueName.interactable = flag4;
    this.airbaseDisplayName.interactable = flag1;
    this.factionDropdown.interactable = flag4 | flag2;
    this.disableToggle.interactable = flag1;
    this.capturableToggle.interactable = flag1;
    this.captureDefenseSlider.interactable = flag1;
    this.captureRangeSlider.interactable = flag4;
    this.captureRangeSliderLabel.color = flag4 ? this.captureRangeSliderLabelNormalColor : this.captureRangeSliderLabelInactiveColor;
    this.captureRangeSliderFill.color = flag4 ? this.captureRangeSliderFillNormalColor : this.captureRangeSliderFillInactiveColor;
    this.removeButton.interactable = flag5;
    this.removeText.text = flag4 ? "Delete airbase" : "Remove overrides";
    this.airbaseUniqueName.SetTextWithoutNotify(this.savedAirbase?.UniqueName);
    this.airbaseDisplayName.SetTextWithoutNotify(this.savedAirbase?.DisplayName);
    this.factionDropdown.SetValueWithoutNotify(!((UnityEngine.Object) airbase != (UnityEngine.Object) null) || !((UnityEngine.Object) airbase.CurrentHQ != (UnityEngine.Object) null) ? 0 : this.factionOptions.IndexOf(airbase.CurrentHQ.faction.factionName));
    Toggle disableToggle = this.disableToggle;
    SavedAirbase savedAirbase1 = this.savedAirbase;
    int num1 = savedAirbase1 != null ? (savedAirbase1.Disabled ? 1 : 0) : 0;
    disableToggle.SetIsOnWithoutNotify(num1 != 0);
    Toggle capturableToggle = this.capturableToggle;
    SavedAirbase savedAirbase2 = this.savedAirbase;
    int num2 = savedAirbase2 != null ? (savedAirbase2.Capturable ? 1 : 0) : 0;
    capturableToggle.SetIsOnWithoutNotify(num2 != 0);
    Slider captureDefenseSlider = this.captureDefenseSlider;
    SavedAirbase savedAirbase3 = this.savedAirbase;
    double input1 = savedAirbase3 != null ? (double) savedAirbase3.CaptureDefense : 1.0;
    captureDefenseSlider.SetValueWithoutNotify((float) input1);
    TextMeshProUGUI defenseSliderLabel = this.captureDefenseSliderLabel;
    float num3 = this.captureDefenseSlider.value;
    string str1 = num3.ToString("0.0");
    defenseSliderLabel.text = str1;
    Slider captureRangeSlider = this.captureRangeSlider;
    SavedAirbase savedAirbase4 = this.savedAirbase;
    double input2 = savedAirbase4 != null ? (double) savedAirbase4.CaptureRange : 1000.0;
    captureRangeSlider.SetValueWithoutNotify((float) input2);
    TextMeshProUGUI rangeSliderLabel = this.captureRangeSliderLabel;
    num3 = this.captureRangeSlider.value;
    string str2 = num3.ToString("0.0");
    rangeSliderLabel.text = str2;
    this.roadButton.gameObject.SetActive(flag2 | flag4);
    this.unitButton.gameObject.SetActive(flag3);
    if (flag4)
    {
      this.centerHandle = UnityEngine.Object.Instantiate<PositionHandle>(this.positionHandlePrefab);
      this.centerHandle.SetHue(Color.green);
      this.centerHandle.Setup((IValueWrapper<GlobalPosition>) this.savedAirbase.CenterWrapper, (Func<string>) (() => "Center " + airbase.SavedAirbase.DisplayName), (Action) null);
      this.centerPositionField.Setup("Center", (IValueWrapper<Vector3>) this.savedAirbase.CenterWrapper);
      this.selectionHandle = UnityEngine.Object.Instantiate<PositionHandle>(this.positionHandlePrefab);
      this.selectionHandle.SetHue(Color.blue);
      this.selectionHandle.Setup((IValueWrapper<GlobalPosition>) this.savedAirbase.SelectionPositionWrapper, (Func<string>) (() => "Selection " + airbase.SavedAirbase.DisplayName), (Action) null);
      this.selectionPositionField.Setup("Selection", (IValueWrapper<Vector3>) this.savedAirbase.SelectionPositionWrapper);
    }
    else
    {
      this.centerPositionField.SetupReadOnly("Center", (UnityEngine.Object) airbase != (UnityEngine.Object) null ? airbase.center.GlobalPosition() : new GlobalPosition());
      this.selectionPositionField.SetupReadOnly("Selection", (UnityEngine.Object) airbase != (UnityEngine.Object) null ? airbase.aircraftSelectionTransform.GlobalPosition() : new GlobalPosition());
    }
    if (flag1 && !flag3)
    {
      this.towerReferenceField.Popup.HideUnitTypeFilter();
      this.towerReferenceField.Setup<SavedBuilding>("Tower", (IList) MissionManager.GetAllSavedBuildings(true), this.savedAirbase.TowerRef, (Action<SavedBuilding>) (value =>
      {
        this.CheckOverride();
        this.savedAirbase.TowerRef = value;
        if (value == null || value.AirbaseRef == this.savedAirbase)
          return;
        value.SetAirbase(this.savedAirbase);
      }));
      this.buildingList.SetHeight(300);
      this.buildingList.SetupList<SavedBuilding>(this.savedAirbase.BuildingsRef, new Func<SavedBuilding, string>(PickerName), (Func<SavedBuilding, string>) (x => x.ToUIString(false)), (Func<IEnumerable<SavedBuilding>>) (() => (IEnumerable<SavedBuilding>) MissionManager.GetAllSavedBuildings(true)), ReferenceList.ButtonsEvents.OverrideNullOnly);
    }
    else
    {
      this.towerReferenceField.gameObject.SetActive(false);
      this.buildingList.gameObject.SetActive(false);
    }
    if (flag4)
    {
      this.SetupVerticalLandingList();
      this.SetupServicePointsList();
      this.runwayList.Setup(new Action(this.CreateNewRunway), (Action) null, new Action<int>(this.ShowRunwayPanel), new Action<int>(this.RemoveRunway));
      this.runwayList.SetupList<SavedRunway>(this.savedAirbase.runways, (Func<SavedRunway, string>) (o => o.ToUIString(false)), ReferenceList.ButtonsEvents.DontAdd);
    }
    else
      this.runwayList.gameObject.SetActive(false);
    FixLayout.ForceRebuildRecursive((RectTransform) this.transform);

    static string PickerName(SavedBuilding building)
    {
      return string.IsNullOrEmpty(building.Airbase) ? building.UniqueName : $"{building.UniqueName} - {building.Airbase.AddColor(new Color(0.4f, 0.4f, 0.4f)).AddSize(0.75f)}";
    }
  }

  private void SetupVerticalLandingList()
  {
    // ISSUE: method pointer
    // ISSUE: method pointer
    EmptyDataList emptyDataList = this.dataDrawer.DrawList<ValueWrapperGlobalPosition>(300, this.savedAirbase.VerticalLandingPointsWrappers, new DrawInnerData<ValueWrapperGlobalPosition>(AirbasePanel.DrawVector3ListContent), new Func<ValueWrapperGlobalPosition>((object) this, __methodptr(\u003CSetupVerticalLandingList\u003Eg__CreateWrapper\u007C46_0)), new Action<ValueWrapperGlobalPosition>((object) this, __methodptr(\u003CSetupVerticalLandingList\u003Eg__OnDelete\u007C46_2)));
    emptyDataList.AllowSwapItems = false;
    emptyDataList.TitleText.text = "Vertical Landing Points";
    emptyDataList.RefreshList();
    for (int index = 0; index < this.savedAirbase.VerticalLandingPointsWrappers.Count; ++index)
    {
      ValueWrapperGlobalPosition landingPointsWrapper = this.savedAirbase.VerticalLandingPointsWrappers[index];
      OnCreate(index, landingPointsWrapper);
    }

    void OnCreate(int index, ValueWrapperGlobalPosition wrapper)
    {
      PositionHandle positionHandle = UnityEngine.Object.Instantiate<PositionHandle>(this.positionHandlePrefab);
      positionHandle.SetHue(Color.yellow);
      positionHandle.Setup((IValueWrapper<GlobalPosition>) wrapper, (Func<string>) (() => $"Vertical Landing {index + 1} " + this.airbase.SavedAirbase.DisplayName), (Action) null);
      this.verticalLandingHandles[wrapper] = positionHandle;
    }
  }

  private void SetupServicePointsList()
  {
    // ISSUE: method pointer
    // ISSUE: method pointer
    EmptyDataList emptyDataList = this.dataDrawer.DrawList<ValueWrapperGlobalPosition>(300, this.savedAirbase.ServicePointsWrappers, new DrawInnerData<ValueWrapperGlobalPosition>(AirbasePanel.DrawVector3ListContent), new Func<ValueWrapperGlobalPosition>((object) this, __methodptr(\u003CSetupServicePointsList\u003Eg__CreateWrapper\u007C47_0)), new Action<ValueWrapperGlobalPosition>((object) this, __methodptr(\u003CSetupServicePointsList\u003Eg__OnDelete\u007C47_2)));
    emptyDataList.AllowSwapItems = false;
    emptyDataList.TitleText.text = "Service Points";
    emptyDataList.RefreshList();
    for (int index = 0; index < this.savedAirbase.ServicePointsWrappers.Count; ++index)
    {
      ValueWrapperGlobalPosition servicePointsWrapper = this.savedAirbase.ServicePointsWrappers[index];
      OnCreate(index, servicePointsWrapper);
    }

    void OnCreate(int index, ValueWrapperGlobalPosition wrapper)
    {
      PositionHandle positionHandle = UnityEngine.Object.Instantiate<PositionHandle>(this.positionHandlePrefab);
      positionHandle.SetHue(new Color(1f, 0.3f, 0.08f));
      positionHandle.Setup((IValueWrapper<GlobalPosition>) wrapper, (Func<string>) (() => $"Service points {index + 1} " + this.airbase.SavedAirbase.DisplayName), (Action) null);
      this.servicePointsHandles[wrapper] = positionHandle;
    }
  }

  private static void DrawVector3ListContent(
    int index,
    ValueWrapperGlobalPosition wrapper,
    DataDrawer dataDrawer)
  {
    Vector3DataField vector3DataField = dataDrawer.InstantiateWithParent<Vector3DataField>(dataDrawer.Prefabs.VectorFieldPrefab);
    vector3DataField.LabelLayout.minWidth = 40f;
    vector3DataField.Setup($"{index + 1}", (IValueWrapper<Vector3>) wrapper);
  }

  private void CreateNewRunway()
  {
    SavedRunway runway = new SavedRunway();
    runway.Start = this.savedAirbase.Center - new Vector3(50f, 0.0f, 0.0f);
    runway.End = this.savedAirbase.Center - new Vector3(-50f, 0.0f, 0.0f);
    this.savedAirbase.runways.Add(runway);
    this.ShowRunwayPanel(runway);
  }

  private void ShowRunwayPanel(SavedRunway runway)
  {
    Airbase airbase = this.airbase;
    this.editorTabs.ChangeTab<RunwayTab>(this.runwayTabPrefab, true).Setup(airbase, runway);
  }

  private void ShowRunwayPanel(int index) => this.ShowRunwayPanel(this.savedAirbase.runways[index]);

  private void RemoveRunway(int index)
  {
    this.savedAirbase.runways.RemoveAt(index);
    this.runwayList.RefreshList();
  }

  private void BuildListItemAdded(ISaveableReference savedRef)
  {
    this.CheckOverride();
    ((SavedBuilding) savedRef).SetAirbase(this.savedAirbase);
  }

  private void BuildListItemRemoved(ISaveableReference savedRef)
  {
    ((SavedBuilding) savedRef).RemoveAirbase();
  }

  private void AirbaseMenu_OnSelect(SelectionDetails selectionDetails)
  {
    if (selectionDetails != null)
      return;
    UnityEngine.Object.Destroy((UnityEngine.Object) this.gameObject);
  }

  private void PopulateFactionOptions()
  {
    this.factionOptions = FactionHelper.GetFactionsAndNeutral();
    this.factionDropdown.ClearOptions();
    this.factionDropdown.AddOptions(this.factionOptions);
  }

  public static string NewAirbaseUniqueName(string nameField)
  {
    bool flag = string.IsNullOrEmpty(nameField);
    string str = flag ? "CustomAirbase" : nameField;
    string name = str;
    Dictionary<string, Airbase> allAirbase = MissionManager.GetAllAirbase();
    SaveHelper.MakeUnique<Airbase>(ref name, allAirbase, !flag);
    ColorLog<Airbase>.Info($"Creating Unique airbase name, want={str} unique={name}");
    return name;
  }

  private void CheckOverride()
  {
    if (this.airbase.SavedAirbaseOverride)
      return;
    this.CreateOverride();
  }

  private void CreateOverride()
  {
    string uniqueName = this.airbase.SavedAirbase.UniqueName;
    Mission currentMission = MissionManager.CurrentMission;
    int index = currentMission.airbases.FindIndex((Predicate<SavedAirbase>) (x => x.UniqueName == uniqueName));
    if (index != -1)
    {
      Debug.LogError((object) $"Airbase with name {uniqueName} already in mission but trying to create a new override for it");
      this.savedAirbase = currentMission.airbases[index];
    }
    else
    {
      this.savedAirbase = SavedAirbase.CreateOverride(this.airbase.SavedAirbase);
      this.savedAirbase.SavedInMission = true;
      currentMission.airbases.Add(this.savedAirbase);
    }
    this.airbase.LinkSavedAirbase(this.savedAirbase, false);
    this.removeButton.interactable = true;
  }

  public void RemoveAirbase()
  {
    MissionEditor.RemoveAirbase(this.airbase);
    if (this.airbase.BuiltIn || this.airbase.AttachedAirbase)
    {
      this.Setup(this.airbase);
    }
    else
    {
      this.airbase = (Airbase) null;
      this.savedAirbase = (SavedAirbase) null;
      UnityEngine.Object.Destroy((UnityEngine.Object) this.gameObject);
    }
  }

  private void RoadButtonClicked()
  {
    Airbase airbase = this.airbase;
    this.editorTabs.ChangeTab<RoadEditor>(this.roadEditorPrefab, true).SelectNetwork(airbase, false);
  }

  private void UnitButtonClicked()
  {
    Unit attachedUnit;
    if (!this.airbase.TryGetAttachedUnit(out attachedUnit))
      return;
    SceneSingleton<UnitSelection>.i.SetSelection((IEditorSelectable) attachedUnit);
  }

  private void AirbaseUniqueNameChanged(string value)
  {
    if (value.StartsWith("<UNIT_AIRBASE>++"))
      value = value.Substring("<UNIT_AIRBASE>++".Length);
    if (this.savedAirbase == null)
      return;
    string str = AirbasePanel.NewAirbaseUniqueName(value);
    FactionRegistry.ChangeAirbaseName(this.airbase, str);
    this.savedAirbase.UniqueName = value;
    this.airbaseUniqueName.SetTextWithoutNotify(str);
    this.airbase.name = str;
  }

  private void AirbaseDisplayNameChanged(string value)
  {
    this.CheckOverride();
    if (this.savedAirbase == null)
      return;
    this.savedAirbase.DisplayName = value;
  }

  private void FactionChanged(int index)
  {
    this.CheckOverride();
    string factionOption = this.factionOptions[index];
    this.savedAirbase.faction = factionOption;
    foreach (SavedBuilding savedBuilding in this.savedAirbase.BuildingsRef)
    {
      savedBuilding.faction = factionOption;
      if ((UnityEngine.Object) savedBuilding.Unit != (UnityEngine.Object) null)
        savedBuilding.Unit.NetworkHQ = FactionRegistry.HqFromName(factionOption);
    }
    this.buildingList.RefreshList();
    this.airbase.EditorSetFaction(factionOption, false);
  }

  private void DisabledChanged(bool value)
  {
    this.CheckOverride();
    if (this.savedAirbase == null)
      return;
    this.savedAirbase.Disabled = value;
  }

  private void CapturableChanged(bool value)
  {
    this.CheckOverride();
    if (this.savedAirbase == null)
      return;
    this.savedAirbase.Capturable = value;
  }

  private void CaptureDefenseChanged(float value)
  {
    this.CheckOverride();
    if (this.savedAirbase != null)
      this.savedAirbase.CaptureDefense = value;
    this.captureDefenseSliderLabel.text = value.ToString("0.0");
  }

  private void CaptureRangeChanged(float value)
  {
    this.CheckOverride();
    if (this.savedAirbase != null)
      this.savedAirbase.CaptureRange = value;
    if ((UnityEngine.Object) this.airbase != (UnityEngine.Object) null)
    {
      AirbaseEditorRadius airbaseEditorRadius = AirbaseEditorRadius.Find(this.airbase.center);
      if ((UnityEngine.Object) airbaseEditorRadius != (UnityEngine.Object) null)
        airbaseEditorRadius.Setup(this.airbase.CurrentHQ.GetColorOrGray(), value);
    }
    this.captureRangeSliderLabel.text = value.ToString("0.0");
  }
}
