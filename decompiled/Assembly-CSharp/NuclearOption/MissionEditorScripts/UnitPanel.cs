// Decompiled with JetBrains decompiler
// Type: NuclearOption.MissionEditorScripts.UnitPanel
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.SavedMission;
using NuclearOption.SavedMission.ObjectiveV2;
using NuclearOption.SavedMission.ObjectiveV2.Outcomes;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

#nullable disable
namespace NuclearOption.MissionEditorScripts;

public class UnitPanel : MonoBehaviour
{
  [Header("Buttons")]
  [SerializeField]
  private Button copyButton;
  [SerializeField]
  private Button pasteButton;
  [SerializeField]
  private Button removeButton;
  [SerializeField]
  private TextMeshProUGUI removeButtonText;
  [Header("Title")]
  [SerializeField]
  private TextMeshProUGUI titleText;
  [SerializeField]
  private TextMeshProUGUI builtInNotice;
  [SerializeField]
  private TextMeshProUGUI typeText;
  [Header("Name")]
  [SerializeField]
  private TMP_InputField uniqueNameInput;
  [SerializeField]
  private GameObject uniqueNamePanel;
  [Header("Faction")]
  [SerializeField]
  private GameObject factionPanel;
  [SerializeField]
  private TMP_Dropdown factionDropdown;
  [SerializeField]
  private GameObject factionDropdownArrow;
  [SerializeField]
  private TextMeshProUGUI factionDropdownLabel;
  [SerializeField]
  private Color factionDropdownLabelNormalColor;
  [SerializeField]
  private Color factionDropdownLabelInactiveColor;
  [Header("Airbase")]
  [SerializeField]
  private GameObject airbasePanel;
  [SerializeField]
  private Button airbaseButton;
  [SerializeField]
  private ReferenceDataField airbaseReference;
  [Header("Spawning")]
  [SerializeField]
  private GameObject spawnTimingPanel;
  [SerializeField]
  private TextMeshProUGUI spawnTiming;
  [Header("Position")]
  [SerializeField]
  private GameObject positionMultipleWarning;
  [SerializeField]
  private Vector3DataField positionField;
  [SerializeField]
  private Vector3DataField rotationField;
  [Header("Capture")]
  [SerializeField]
  private GameObject capturePanel;
  [SerializeField]
  private FloatDataField.FloatSlider captureSliderMinMax = new FloatDataField.FloatSlider(0.0f, 10f);
  [SerializeField]
  private float captureSliderSteps = 0.1f;
  [SerializeField]
  private string captureSliderTextFormat = "0.0";
  [SerializeField]
  private string captureSliderDifferentValueTextFormat = "-";
  [SerializeField]
  private GameObject captureStrengthDifferentWarning;
  [SerializeField]
  private OverrideDataField overrideCaptureStrengthField;
  [SerializeField]
  private FloatDataField captureStrengthField;
  [SerializeField]
  private GameObject captureDefenseDifferentWarning;
  [SerializeField]
  private OverrideDataField overrideCaptureDefenseField;
  [SerializeField]
  private FloatDataField captureDefenseField;
  [Header("Inventory")]
  [SerializeField]
  private Button inventoryButton;
  [SerializeField]
  private InventoryInspector inventoryPanel;
  [Header("Unit Options")]
  [SerializeField]
  private AircraftOptions aircraftOptions;
  [SerializeField]
  private BuildingOptions buildingOptions;
  [SerializeField]
  private MissileOptions missileOptions;
  [SerializeField]
  private SceneryOptions sceneryOptions;
  [SerializeField]
  private ShipOptions shipOptions;
  [SerializeField]
  private VehicleOptions vehicleOptions;
  private UnitPanelOptions options;
  private readonly NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<SavedUnit> targets = new NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<SavedUnit>();
  private ValueWrapperOverride<float> captureStrengthWrapper;
  private ValueWrapperOverride<float> captureDefenseWrapper;
  private IValueWrapper<GlobalPosition> multiPositionWrapper;

  private FloatDataField.FloatSettings captureSliderSettings
  {
    get
    {
      return new FloatDataField.FloatSettings()
      {
        Slider = new FloatDataField.FloatSlider?(this.captureSliderMinMax),
        Steps = new float?(this.captureSliderSteps),
        TextFormat = this.captureSliderTextFormat
      };
    }
  }

  public static bool CanCapture(SavedUnit unit)
  {
    bool flag;
    switch (unit)
    {
      case SavedPilot _:
        flag = false;
        break;
      case SavedMissile _:
        flag = false;
        break;
      case SavedScenery _:
        flag = false;
        break;
      default:
        flag = true;
        break;
    }
    return flag;
  }

  public static bool CanHaveFaction(SavedUnit unit)
  {
    bool flag;
    switch (unit)
    {
      case SavedScenery _:
        flag = false;
        break;
      case SavedMissile _:
        flag = false;
        break;
      default:
        flag = true;
        break;
    }
    return flag;
  }

  public static void SetUnitFaction(SavedUnit saved, string factionName)
  {
    if (saved is SavedBuilding savedBuilding && savedBuilding.AirbaseRef != null)
      return;
    saved.faction = factionName;
    Unit unit = saved.Unit;
    unit.NetworkHQ = FactionRegistry.HqFromName(factionName);
    Airbase component;
    if (!unit.TryGetComponent<Airbase>(out component))
      return;
    component.EditorSetFaction(factionName, true);
  }

  private void Awake()
  {
    this.copyButton.onClick.AddListener(new UnityAction(this.CopyToClipboard));
    this.pasteButton.onClick.AddListener(new UnityAction(this.PasteFromClipboard));
    this.removeButton.onClick.AddListener(new UnityAction(this.RemoveUnit));
    this.airbaseButton.onClick.AddListener(new UnityAction(this.AirbaseButtonClicked));
    this.inventoryButton.onClick.AddListener(new UnityAction(this.OpenInventoryPanel));
    this.uniqueNameInput.onEndEdit.AddListener(new UnityAction<string>(this.ChangeUniqueName));
    List<string> factionOptions = FactionHelper.GetFactionsAndNeutral();
    this.factionDropdown.options.Clear();
    this.factionDropdown.AddOptions(factionOptions);
    this.targets.AddAndInvokeChanged((object) this.factionDropdown, (Action) (() =>
    {
      foreach (SavedUnit target in (IEnumerable<SavedUnit>) this.targets.Targets)
      {
        if (target is SavedBuilding savedBuilding2 && savedBuilding2.AirbaseRef != null)
          savedBuilding2.faction = savedBuilding2.AirbaseRef.faction;
      }
      string sameValue;
      this.factionDropdown.SetValueWithoutNotify(NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<SavedUnit>.TryGetSameValue<string>(this.targets.Targets.Where<SavedUnit>(new Func<SavedUnit, bool>(UnitPanel.CanHaveFaction)), (NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<SavedUnit>.GetField<string>) (x => x.faction), out sameValue) ? (FactionHelper.EmptyOrNoFactionOrNeutral(sameValue) ? 0 : factionOptions.IndexOf(sameValue)) : -1);
    }));
    this.factionDropdown.onValueChanged.AddListener((UnityAction<int>) (index =>
    {
      foreach (SavedUnit target in (IEnumerable<SavedUnit>) this.targets.Targets)
      {
        if (UnitPanel.CanHaveFaction(target))
        {
          if (target is SavedBuilding savedBuilding4 && savedBuilding4.AirbaseRef != null)
            break;
          this.CheckOverride(target);
          string factionName = factionOptions[index];
          UnitPanel.SetUnitFaction(target, factionName);
        }
      }
    }));
    this.captureStrengthWrapper = ValueWrapper.FromCallback<ValueWrapperOverride<float>, Override<float>>((object) this, new Override<float>(), (Action<Override<float>>) (value =>
    {
      foreach (SavedUnit target in (IEnumerable<SavedUnit>) this.targets.Targets)
      {
        if (UnitPanel.CanCapture(target))
          target.CaptureStrength = value;
      }
    }));
    this.captureStrengthWrapper.RegisterOnChange((object) this, (ValueWrapper<Override<float>>.OnChangeDelegate) (_ => this.CheckAllOverrides()));
    this.captureStrengthField.Setup("Capture Power", (IValueWrapper<float>) this.captureStrengthWrapper, new FloatDataField.FloatSettings?(this.captureSliderSettings));
    this.overrideCaptureStrengthField.Setup<float>((ValueWrapper<Override<float>>) this.captureStrengthWrapper, (OverrideDataField.InnerField) (DataField) this.captureStrengthField);
    this.captureDefenseWrapper = ValueWrapper.FromCallback<ValueWrapperOverride<float>, Override<float>>((object) this, new Override<float>(), (Action<Override<float>>) (value =>
    {
      foreach (SavedUnit target in (IEnumerable<SavedUnit>) this.targets.Targets)
      {
        if (UnitPanel.CanCapture(target))
          target.CaptureDefense = value;
      }
    }));
    this.captureDefenseWrapper.RegisterOnChange((object) this, (ValueWrapper<Override<float>>.OnChangeDelegate) (_ => this.CheckAllOverrides()));
    this.captureDefenseField.Setup("Capture Defense", (IValueWrapper<float>) this.captureDefenseWrapper, new FloatDataField.FloatSettings?(this.captureSliderSettings));
    this.overrideCaptureDefenseField.Setup<float>((ValueWrapper<Override<float>>) this.captureDefenseWrapper, (OverrideDataField.InnerField) (DataField) this.captureDefenseField);
    this.targets.AddAndInvokeChanged((object) this, new Action(this.OnTargetsChanged));
    SceneSingleton<UnitSelection>.i.OnSelect += new UnitSelection.SelectionChanged<SelectionDetails>(this.UnitMenu_OnSelect);
  }

  private void OnTargetsChanged()
  {
    this.inventoryPanel.gameObject.SetActive(false);
    int count = this.targets.Targets.Count;
    if (count == 0)
      return;
    bool flag1 = count == 1;
    SavedUnit firstSaved = this.targets.Targets[0];
    Unit unit1 = firstSaved.Unit;
    if (flag1)
    {
      if (unit1.BuiltIn)
        this.titleText.text = unit1.unitName + " [Map Object]";
      else
        this.titleText.text = unit1.unitName;
    }
    else
      this.titleText.text = $"{count} units selected";
    this.factionPanel.SetActive(this.targets.Any(new NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<SavedUnit>.GetField<bool>(UnitPanel.CanHaveFaction)));
    this.UpdateFactionInteractable();
    this.builtInNotice.gameObject.SetActive(this.targets.Any((NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<SavedUnit>.GetField<bool>) (unit => unit.Unit.BuiltIn)));
    this.typeText.text = this.targets.GetSameLabel((NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<SavedUnit>.GetField<string>) (unit => unit.type));
    this.uniqueNamePanel.gameObject.SetActive(flag1);
    this.uniqueNameInput.interactable = flag1 && firstSaved.PlacementType != PlacementType.BuiltIn;
    this.uniqueNameInput.SetTextWithoutNotify(flag1 ? firstSaved.UniqueName : "-");
    this.copyButton.interactable = flag1;
    this.pasteButton.interactable = SceneSingleton<UnitCopyPaste>.i.Clipboard != null;
    this.removeButton.interactable = this.targets.Any((NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<SavedUnit>.GetField<bool>) (x => x.PlacementType != PlacementType.BuiltIn));
    if (flag1)
      this.removeButtonText.text = firstSaved.PlacementType == PlacementType.Override ? "Remove Override" : "Remove Unit";
    else if (this.targets.All((NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<SavedUnit>.GetField<bool>) (saved => saved.PlacementType == PlacementType.Override)))
      this.removeButtonText.text = $"Remove {count} Overrides";
    else if (this.targets.All((NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<SavedUnit>.GetField<bool>) (saved => saved.PlacementType == PlacementType.Custom)))
      this.removeButtonText.text = $"Remove {count} Units";
    else if (this.targets.All((NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<SavedUnit>.GetField<bool>) (saved => saved.PlacementType == PlacementType.BuiltIn)))
      this.removeButtonText.text = "Cant Remove";
    else
      this.removeButtonText.text = $"Remove {count} Units and Overrides";
    bool flag2 = flag1 && unit1.TryGetComponent<Airbase>(out Airbase _);
    bool flag3 = this.targets.All((NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<SavedUnit>.GetField<bool>) (x => x is SavedBuilding));
    this.airbasePanel.SetActive(flag2 | flag3);
    this.airbaseButton.gameObject.SetActive(flag2);
    this.airbaseReference.gameObject.SetActive(flag3);
    if (flag3)
    {
      IEnumerable<SavedAirbase> source = MissionManager.GetAllSavedAirbase().Where<SavedAirbase>((Func<SavedAirbase, bool>) (x => !((UnityEngine.Object) x.Airbase != (UnityEngine.Object) null) || !x.Airbase.AttachedAirbase));
      SavedAirbase differentValuePlaceholder = (SavedAirbase) null;
      SavedAirbase sameValue;
      SavedAirbase current;
      if (this.targets.TryGetSameValue<SavedAirbase>(new NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<SavedUnit>.GetField<SavedAirbase>(GetAirbaseRef), out sameValue))
      {
        current = sameValue;
      }
      else
      {
        differentValuePlaceholder = new SavedAirbase()
        {
          UniqueName = "-",
          DisplayName = "-"
        };
        current = differentValuePlaceholder;
        source.Prepend<SavedAirbase>(differentValuePlaceholder);
      }
      this.airbaseReference.Setup<SavedAirbase>("Airbase", source.ToList<SavedAirbase>(), current, (Action<SavedAirbase>) (value =>
      {
        if (differentValuePlaceholder != null && value.UniqueName == differentValuePlaceholder.UniqueName)
          return;
        this.CheckAllOverrides();
        foreach (SavedBuilding target in (IEnumerable<SavedUnit>) this.targets.Targets)
          target.SetOrRemoveAirbase(value);
        this.UpdateFactionInteractable();
      }));
    }
    this.spawnTimingPanel.SetActive(flag1);
    if (flag1)
    {
      string str = string.Join("\n", MissionManager.CurrentMission.Objectives.AllOutcomes.OfType<SpawnUnitOutcome>().Where<SpawnUnitOutcome>((Func<SpawnUnitOutcome, bool>) (outcome => outcome.UnitsToSpawn.Contains(firstSaved))).Select<SpawnUnitOutcome, string>((Func<SpawnUnitOutcome, string>) (outcome => "- " + outcome.SavedOutcome.UniqueName)));
      if (str.Length == 0)
        str = "<i>Mission Start</i>";
      this.spawnTiming.text = str;
    }
    bool flag4 = this.targets.Any(new NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<SavedUnit>.GetField<bool>(UnitPanel.CanCapture));
    this.capturePanel.SetActive(flag4);
    if (flag4)
    {
      Override<float> sameValue1;
      bool sameValue2 = NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<SavedUnit>.TryGetSameValue<Override<float>>(this.targets.Targets.Where<SavedUnit>(new Func<SavedUnit, bool>(UnitPanel.CanCapture)), (NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<SavedUnit>.GetField<Override<float>>) (x => x.CaptureStrength), out sameValue1);
      this.captureStrengthDifferentWarning.SetActive(!sameValue2);
      this.captureStrengthField.TextFormat = sameValue2 ? this.captureSliderSettings.TextFormat : this.captureSliderDifferentValueTextFormat;
      this.captureStrengthWrapper.SetValue(sameValue2 ? sameValue1 : new Override<float>(), (object) this, true);
      Override<float> sameValue3;
      bool sameValue4 = NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<SavedUnit>.TryGetSameValue<Override<float>>(this.targets.Targets.Where<SavedUnit>(new Func<SavedUnit, bool>(UnitPanel.CanCapture)), (NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<SavedUnit>.GetField<Override<float>>) (x => x.CaptureDefense), out sameValue3);
      this.captureDefenseDifferentWarning.SetActive(!sameValue4);
      this.captureDefenseField.TextFormat = sameValue4 ? this.captureSliderSettings.TextFormat : this.captureSliderDifferentValueTextFormat;
      this.captureDefenseWrapper.SetValue(sameValue4 ? sameValue3 : new Override<float>(), (object) this, true);
    }
    this.positionMultipleWarning.gameObject.SetActive(!flag1);
    if (flag1)
    {
      if (firstSaved.PlacementType == PlacementType.Custom)
      {
        this.positionField.Setup("Position", (IValueWrapper<Vector3>) firstSaved.PositionWrapper);
        this.rotationField.Setup("Rotation", (IValueWrapper<Vector3>) firstSaved.RotationWrapper);
      }
      else
      {
        Vector3 position;
        Quaternion rotation;
        unit1.transform.GetPositionAndRotation(out position, out rotation);
        this.positionField.SetupReadOnly("Position", position.ToGlobalPosition());
        this.rotationField.SetupReadOnly("Rotation", rotation.eulerAngles);
      }
    }
    else
      this.RefreshMultiTargetPosition();
    this.inventoryButton.gameObject.SetActive(this.targets.Any((NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<SavedUnit>.GetField<bool>) (saved => saved.Unit.TryGetComponent<UnitStorage>(out UnitStorage _))));
    UnitPanelOptions optionsPanel = this.GetOptionsPanel();
    if (this.options?.GetType() != optionsPanel?.GetType())
    {
      if ((UnityEngine.Object) this.options != (UnityEngine.Object) null)
      {
        this.options.gameObject.SetActive(false);
        this.options.Cleanup();
        UnityEngine.Object.Destroy((UnityEngine.Object) this.options.gameObject);
        this.options = (UnitPanelOptions) null;
      }
      if ((UnityEngine.Object) optionsPanel != (UnityEngine.Object) null)
      {
        this.options = UnityEngine.Object.Instantiate<UnitPanelOptions>(optionsPanel, this.transform);
        this.options.Setup(this, this.targets);
      }
    }
    if ((UnityEngine.Object) this.options != (UnityEngine.Object) null)
      this.options.OnTargetsChanged();
    this.GetComponentInParent<ILayerRebuildRoot>()?.Rebuild();

    static SavedAirbase GetAirbaseRef(SavedUnit savedUnit)
    {
      SavedBuilding savedBuilding = (SavedBuilding) savedUnit;
      if (savedUnit.PlacementType != PlacementType.BuiltIn)
        return savedBuilding.AirbaseRef;
      Airbase mapAirbase = savedUnit.Unit.MapAirbase;
      return !((UnityEngine.Object) mapAirbase != (UnityEngine.Object) null) ? (SavedAirbase) null : mapAirbase.SavedAirbase;
    }
  }

  private void RefreshMultiTargetPosition()
  {
    int count = this.targets.Targets.Count;
    Vector3 vector3 = new Vector3();
    foreach (SavedUnit target in (IEnumerable<SavedUnit>) this.targets.Targets)
    {
      GlobalPosition globalPosition = target.PlacementType == PlacementType.Custom ? target.globalPosition : target.Unit.GlobalPosition();
      vector3 += globalPosition.AsVector3() / (float) count;
    }
    this.positionField.SetupReadOnly("Position", vector3);
    this.rotationField.SetupNonValue("Rotation");
  }

  private void UpdateFactionInteractable()
  {
    bool flag = this.targets.All((NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<SavedUnit>.GetField<bool>) (saved => !(saved is SavedBuilding savedBuilding) || savedBuilding.AirbaseRef == null));
    this.factionDropdown.interactable = flag;
    this.factionDropdownLabel.color = flag ? this.factionDropdownLabelNormalColor : this.factionDropdownLabelInactiveColor;
    this.factionDropdownArrow.gameObject.SetActive(flag);
  }

  private UnitPanelOptions GetOptionsPanel()
  {
    System.Type sameValue;
    if (!this.targets.TryGetSameValue<System.Type>((NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<SavedUnit>.GetField<System.Type>) (x => x.GetType()), out sameValue))
      return (UnitPanelOptions) null;
    if (sameValue == typeof (SavedAircraft))
      return (UnitPanelOptions) this.aircraftOptions;
    if (sameValue == typeof (SavedBuilding))
      return (UnitPanelOptions) this.buildingOptions;
    if (sameValue == typeof (SavedMissile))
      return (UnitPanelOptions) this.missileOptions;
    if (sameValue == typeof (SavedShip))
      return (UnitPanelOptions) this.shipOptions;
    if (sameValue == typeof (SavedVehicle))
      return (UnitPanelOptions) this.vehicleOptions;
    if (sameValue == typeof (SavedScenery))
      return (UnitPanelOptions) this.sceneryOptions;
    if (sameValue == typeof (SavedPilot))
      return (UnitPanelOptions) null;
    if (sameValue == typeof (SavedContainer))
      return (UnitPanelOptions) null;
    Debug.LogError((object) $"No unit Options for type {sameValue}");
    return (UnitPanelOptions) null;
  }

  public void Start() => this.Setup(SceneSingleton<UnitSelection>.i.SelectionDetails);

  public void SelectedRefreshed() => this.Setup(SceneSingleton<UnitSelection>.i.SelectionDetails);

  public void Setup(SelectionDetails details)
  {
    this.multiPositionWrapper?.UnregisterOnChange((object) this);
    this.multiPositionWrapper = (IValueWrapper<GlobalPosition>) null;
    switch (details)
    {
      case UnitSelectionDetails selectionDetails3:
        if (selectionDetails3.Unit.SavedUnit == null)
        {
          Debug.LogError((object) "UnitPanel unit did not have a SavedUnit");
          break;
        }
        this.targets.ReplaceTargets(selectionDetails3.Unit.SavedUnit);
        break;
      case MultiSelectSelectionDetails selectionDetails4:
        this.multiPositionWrapper = selectionDetails4.PositionWrapper;
        this.multiPositionWrapper.RegisterOnChange((object) this, new Action(this.RefreshMultiTargetPosition));
        List<SavedUnit> targets = new List<SavedUnit>();
        foreach (SingleSelectionDetails selectionDetails1 in selectionDetails4.Items)
        {
          if (!(selectionDetails1 is UnitSelectionDetails selectionDetails2))
          {
            Debug.LogError((object) "Unit Panel should not be open while MultiSelectSelectionDetails is not Unit");
            return;
          }
          if (selectionDetails2.Unit.SavedUnit == null)
            Debug.LogError((object) "UnitPanel unit did not have a SavedUnit");
          else
            targets.Add(selectionDetails2.Unit.SavedUnit);
        }
        this.targets.ReplaceTargets((IReadOnlyList<SavedUnit>) targets);
        break;
      default:
        Debug.LogError((object) "UnitPanel was given a null details");
        break;
    }
  }

  public void OpenInventoryPanel()
  {
    if (this.inventoryPanel.gameObject.activeSelf)
      return;
    List<(SavedUnit, UnitStorage)> storageTargets = new List<(SavedUnit, UnitStorage)>();
    foreach (SavedUnit target in (IEnumerable<SavedUnit>) this.targets.Targets)
    {
      UnitStorage component;
      if (target.Unit.TryGetComponent<UnitStorage>(out component))
        storageTargets.Add((target, component));
    }
    if (storageTargets.Count == 0)
    {
      Debug.LogError((object) "OpenInventoryPanel called but no units had UnitStorage");
    }
    else
    {
      this.inventoryPanel.gameObject.SetActive(true);
      this.inventoryPanel.transform.SetParent(SceneSingleton<MissionEditor>.i.transform);
      this.inventoryPanel.transform.localPosition = Vector3.zero;
      this.inventoryPanel.UnitPanelTargetsChanged(storageTargets);
    }
  }

  private void OnDestroy()
  {
    if ((UnityEngine.Object) this.inventoryPanel != (UnityEngine.Object) null)
      UnityEngine.Object.Destroy((UnityEngine.Object) this.inventoryPanel.gameObject);
    SceneSingleton<UnitSelection>.i.OnSelect -= new UnitSelection.SelectionChanged<SelectionDetails>(this.UnitMenu_OnSelect);
  }

  private void UnitMenu_OnSelect(SelectionDetails selectionDetails)
  {
    if (selectionDetails == null)
      UnityEngine.Object.Destroy((UnityEngine.Object) this.gameObject);
    if (selectionDetails is UnitSelectionDetails || !(selectionDetails is MultiSelectSelectionDetails selectionDetails1))
      return;
    int num = selectionDetails1.SelectionType == typeof (UnitSelectionDetails) ? 1 : 0;
  }

  public void CopyToClipboard()
  {
    if (this.targets.Targets.Count == 1)
    {
      SceneSingleton<UnitCopyPaste>.i.Clipboard = this.targets.Targets.First<SavedUnit>();
      this.pasteButton.interactable = true;
    }
    else
      Debug.LogError((object) "Can't copy when more than 1 unit is selected");
  }

  public void PasteFromClipboard()
  {
    SavedUnit clipboard = SceneSingleton<UnitCopyPaste>.i.Clipboard;
    if (clipboard == null)
    {
      Debug.LogWarning((object) "Can't paste because clipboard unit was null");
    }
    else
    {
      foreach (SavedUnit target in (IEnumerable<SavedUnit>) this.targets.Targets)
      {
        this.CheckOverride(target);
        UnitCopyPaste.CopyPaste(MissionManager.CurrentMission, clipboard, target.Unit, target);
        target.Unit.NetworkHQ = FactionRegistry.HqFromName(target.faction);
      }
      this.targets.ReplaceTargets(this.targets.Targets);
      SceneSingleton<UnitSelection>.i.RefreshSelected();
    }
  }

  public void RemoveUnit()
  {
    List<Unit> objects = new List<Unit>();
    foreach (SavedUnit savedUnit in this.targets.Targets.ToList<SavedUnit>())
    {
      if (savedUnit.PlacementType == PlacementType.Override || savedUnit.PlacementType == PlacementType.Custom)
        SceneSingleton<MissionEditor>.i.RemoveUnit(savedUnit.Unit);
      if (savedUnit.PlacementType == PlacementType.BuiltIn || savedUnit.PlacementType == PlacementType.Override)
        objects.Add(savedUnit.Unit);
    }
    SceneSingleton<UnitSelection>.i.ReplaceSelection<Unit>(objects);
  }

  public void ChangeUniqueName(string newValue)
  {
    if (this.targets.Targets.Count != 1)
    {
      Debug.LogError((object) "Can't set unique name unless only 1 unit is selected");
    }
    else
    {
      SavedUnit current = this.targets.Targets.First<SavedUnit>();
      current.UniqueName = this.uniqueNameInput.text;
      if (string.IsNullOrEmpty(current.UniqueName))
        current.UniqueName = current.type;
      SaveHelper.MakeUnique(ref current.UniqueName, (ISaveableReference) current, (IEnumerable<ISaveableReference>) MissionManager.GetAllSavedUnits(true));
      current.Unit.NetworkUniqueName = current.UniqueName;
      this.uniqueNameInput.SetTextWithoutNotify(current.UniqueName);
      Airbase component;
      if (!current.Unit.TryGetComponent<Airbase>(out component))
        return;
      string newName = "<UNIT_AIRBASE>++" + current.UniqueName;
      FactionRegistry.ChangeAirbaseName(component, newName);
      component.SavedAirbase.UniqueName = newName;
    }
  }

  private void AirbaseButtonClicked()
  {
    if (this.targets.Targets.Count != 1)
    {
      Debug.LogError((object) "Can't set unique name unless only 1 unit is selected");
    }
    else
    {
      Airbase component;
      if (!this.targets.Targets.First<SavedUnit>().Unit.TryGetComponent<Airbase>(out component))
        return;
      SceneSingleton<UnitSelection>.i.SetSelection((IEditorSelectable) component);
    }
  }

  public void CheckAllOverrides()
  {
    foreach (SavedUnit target in (IEnumerable<SavedUnit>) this.targets.Targets)
      this.CheckOverride(target);
  }

  public void CheckOverride(SavedUnit savedUnit)
  {
    if (savedUnit.PlacementType != PlacementType.BuiltIn)
      return;
    this.CreateOverride(savedUnit);
  }

  private void CreateOverride(SavedUnit savedUnit)
  {
    string uniqueName = savedUnit.UniqueName;
    Mission currentMission = MissionManager.CurrentMission;
    if (MissionManager.GetAllSavedUnits(false).FirstOrDefault<SavedUnit>((Func<SavedUnit, bool>) (x => x.UniqueName == uniqueName)) != null)
    {
      Debug.LogError((object) $"Unit with name {uniqueName} already in mission but trying to create a new override for it");
    }
    else
    {
      savedUnit.PlacementType = PlacementType.Override;
      SceneSingleton<MissionEditor>.i.AddUnitOverride(savedUnit.Unit, savedUnit);
    }
    this.removeButton.interactable = true;
  }
}
