// Decompiled with JetBrains decompiler
// Type: NuclearOption.MissionEditorScripts.AircraftOptions
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.SavedMission;
using NuclearOption.SavedMission.ObjectiveV2;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

#nullable disable
namespace NuclearOption.MissionEditorScripts;

public class AircraftOptions : UnitPanelOptions
{
  [Header("Player Controlled")]
  [SerializeField]
  private Toggle playerControlledToggle;
  [SerializeField]
  private GameObject playerControlledToggleDifferentValue;
  [SerializeField]
  private GameObject priorityHolder;
  [SerializeField]
  private TMP_InputField priorityInput;
  [SerializeField]
  private GameObject playerControlledWarning;
  [Header("Speed")]
  [SerializeField]
  private TextMeshProUGUI altitudeValue;
  [Space]
  [SerializeField]
  private Slider airspeedSlider;
  [SerializeField]
  private TextMeshProUGUI airspeedHandleLabel;
  [SerializeField]
  private GameObject airspeedDifferentValue;
  [Header("Aircraft")]
  [SerializeField]
  private Slider fuelSlider;
  [SerializeField]
  private TextMeshProUGUI fuelLabel;
  [Header("Livery")]
  [SerializeField]
  private GameObject typeDifferentValueLivery;
  [SerializeField]
  private GameObject liveryDifferentFactionWarning;
  [SerializeField]
  private TMP_Dropdown liverySelection;
  [Header("Weapon")]
  [SerializeField]
  private GameObject typeDifferentValueWeapons;
  [SerializeField]
  private RectTransform loadoutPanel;
  [SerializeField]
  private WeaponSelector weaponSelectorPrefab;
  private readonly List<WeaponSelector> weaponSelectors = new List<WeaponSelector>();
  private readonly List<(LiveryKey key, string label)> liveryOptions = new List<(LiveryKey, string)>();
  private float maxAirSpeed = 300f;
  private readonly List<ValueWrapperGlobalPosition> positionWrappers = new List<ValueWrapperGlobalPosition>();

  public override void Cleanup()
  {
    this.targets.RemoveChanged((object) this.playerControlledToggle);
    this.targets.RemoveChanged((object) this.priorityInput);
    this.targets.RemoveChanged((object) this.airspeedSlider);
    this.targets.RemoveChanged((object) this.fuelSlider);
    foreach (ValueWrapper<GlobalPosition> positionWrapper in this.positionWrappers)
      positionWrapper.UnregisterOnChange((object) this);
    this.positionWrappers.Clear();
    this.DestroyWeaponSelectors();
  }

  private void DestroyWeaponSelectors()
  {
    foreach (WeaponSelector weaponSelector in this.weaponSelectors)
    {
      if ((UnityEngine.Object) weaponSelector != (UnityEngine.Object) null)
      {
        weaponSelector.gameObject.SetActive(false);
        UnityEngine.Object.Destroy((UnityEngine.Object) weaponSelector.gameObject);
      }
    }
    this.weaponSelectors.Clear();
  }

  protected override void SetupInner()
  {
    this.liverySelection.onValueChanged.AddListener(new UnityAction<int>(this.LiveryChanged));
    this.targets.SetupToggle(this.playerControlledToggle, this.playerControlledToggleDifferentValue, (NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<SavedUnit>.GetFieldRef<bool>) (x => ref ((SavedAircraft) x).playerControlled));
    this.playerControlledToggle.onValueChanged.AddListener(new UnityAction<bool>(this.SetPlayerControlled));
    this.priorityInput.contentType = TMP_InputField.ContentType.IntegerNumber;
    this.targets.SetupInputField<int>(this.priorityInput, (NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<SavedUnit>.GetFieldRef<int>) (x => ref ((SavedAircraft) x).playerControlledPriority), (NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<SavedUnit>.ModifyValue<int, string>) (v => v.ToString()), (NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<SavedUnit>.ModifyValue<string, int>) (v => int.Parse(v)));
    this.targets.SetupSlider<float>(this.airspeedSlider, this.airspeedHandleLabel, (NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<SavedUnit>.GetFieldRef<float>) (x => ref ((SavedAircraft) x).startingSpeed), (NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<SavedUnit>.ModifyValue<float, float>) (v => Mathf.Sqrt(v / this.maxAirSpeed)), (NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<SavedUnit>.ModifyValue<float, float>) (v => v * v * this.maxAirSpeed), (NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<SavedUnit>.GetLabelString<float>) (v => UnitConverter.SpeedReading(v)));
    this.targets.SetupSlider(this.fuelSlider, this.fuelLabel, (NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<SavedUnit>.GetFieldRef<float>) (x => ref ((SavedAircraft) x).fuel), (NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<SavedUnit>.GetLabelString<float>) (v => $"Fuel {(ValueType) (v * 100f):F0}%"));
  }

  private void AnyPositionChanged()
  {
    bool flag = true;
    float num = 0.0f;
    float altitude1 = GetAltitude(this.targets.Targets[0]);
    float altitude2 = num + altitude1 / (float) this.targets.Targets.Count;
    for (int index = 1; index < this.targets.Targets.Count; ++index)
    {
      float altitude3 = GetAltitude(this.targets.Targets[index]);
      altitude2 += altitude3 / (float) this.targets.Targets.Count;
      if ((double) altitude1 != (double) altitude3)
        flag = false;
    }
    if (flag)
      this.altitudeValue.text = UnitConverter.AltitudeReading(altitude2);
    else
      this.altitudeValue.text = "(Avg) " + UnitConverter.AltitudeReading(altitude2);

    static float GetAltitude(SavedUnit target)
    {
      GlobalPosition globalPosition = target.globalPosition;
      RaycastHit hit;
      return globalPosition.y - (PathfindingAgent.RaycastTerrain(globalPosition, out hit) ? hit.point.GlobalY() : 0.0f) - target.Unit.definition.spawnOffset.y;
    }
  }

  public override void OnTargetsChanged()
  {
    foreach (ValueWrapper<GlobalPosition> positionWrapper in this.positionWrappers)
      positionWrapper.UnregisterOnChange((object) this);
    this.positionWrappers.Clear();
    foreach (SavedUnit target in (IEnumerable<SavedUnit>) this.targets.Targets)
    {
      ValueWrapperGlobalPosition positionWrapper = target.PositionWrapper;
      this.positionWrappers.Add(positionWrapper);
      positionWrapper.RegisterOnChange((object) this, new Action(this.AnyPositionChanged));
      this.AnyPositionChanged();
    }
    float sameValue1;
    bool sameValue2 = this.targets.TryGetSameValue<float>((NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<SavedUnit>.GetField<float>) (x => ((Aircraft) x.Unit).GetAircraftParameters().maxSpeed), out sameValue1);
    this.airspeedDifferentValue.SetActive(!sameValue2);
    if (sameValue2)
      this.maxAirSpeed = sameValue1;
    this.SetPlayerControlled(this.targets.GetSameValueOrDefault<bool>((NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<SavedUnit>.GetField<bool>) (x => ((SavedAircraft) x).playerControlled), false));
    bool flag = this.targets.AllTheSame<string>((NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<SavedUnit>.GetField<string>) (x => x.type));
    this.typeDifferentValueLivery.SetActive(!flag);
    this.typeDifferentValueWeapons.SetActive(!flag);
    if (flag)
    {
      string type = this.targets.Targets[0].type;
      this.SetupLiveryOptions((SavedAircraft) this.targets.Targets[0]);
      this.SetupWeaponOptions((SavedAircraft) this.targets.Targets[0]);
    }
    this.GetComponentInParent<ILayerRebuildRoot>()?.RebuildEndOfFrame();
  }

  private void SetPlayerControlled(bool isOn)
  {
    this.priorityHolder.SetActive(isOn);
    this.playerControlledWarning.SetActive(isOn && this.targets.Any((NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<SavedUnit>.GetField<bool>) (x => FactionHelper.EmptyOrNoFactionOrNeutral(x.faction))));
    this.GetComponentInParent<ILayerRebuildRoot>()?.Rebuild();
  }

  private void SetupLiveryOptions(SavedAircraft first)
  {
    bool allowFactionLivery = this.targets.AllTheSame<string>((NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<SavedUnit>.GetField<string>) (x => x.faction));
    this.liveryDifferentFactionWarning.SetActive(!allowFactionLivery);
    AircraftSelectionMenu.GetLiveryOptions(this.liveryOptions, (AircraftDefinition) first.Unit.definition, first.faction ?? (string) null, allowFactionLivery);
    this.liverySelection.ClearOptions();
    foreach ((LiveryKey key, string label) liveryOption in this.liveryOptions)
      this.liverySelection.options.Add(new TMP_Dropdown.OptionData(liveryOption.label));
    LiveryKey sameLivery;
    this.liverySelection.SetValueWithoutNotify(this.targets.TryGetSameValue<LiveryKey>((NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<SavedUnit>.GetField<LiveryKey>) (x => ((SavedAircraft) x).liveryKey), out sameLivery) ? this.liveryOptions.FindIndex((Predicate<(LiveryKey, string)>) (v => v.key.Equals(sameLivery))) : -1);
    this.liverySelection.RefreshShownValue();
  }

  private void LiveryChanged(int arg0)
  {
    (LiveryKey key, string label) liveryOption = this.liveryOptions[this.liverySelection.value];
    foreach (SavedUnit target in (IEnumerable<SavedUnit>) this.targets.Targets)
    {
      ((Aircraft) target.Unit).SetLiveryKey(liveryOption.key, true);
      ((SavedAircraft) target).liveryKey = liveryOption.key;
    }
  }

  private void SetupWeaponOptions(SavedAircraft first)
  {
    this.DestroyWeaponSelectors();
    WeaponManager weaponManager = ((Aircraft) first.Unit).weaponManager;
    AircraftParameters aircraftParameters = ((AircraftDefinition) first.Unit.definition).aircraftParameters;
    foreach (SavedAircraft target in (IEnumerable<SavedUnit>) this.targets.Targets)
      AircraftOptions.CheckSavedLoadoutLength(ref target.savedLoadout, weaponManager, aircraftParameters);
    for (int index = 0; index < weaponManager.hardpointSets.Length; ++index)
    {
      HardpointSet hardpointSet = weaponManager.hardpointSets[index];
      WeaponSelector weaponSelector = UnityEngine.Object.Instantiate<WeaponSelector>(this.weaponSelectorPrefab, (Transform) this.loadoutPanel);
      int hardpointIndex = index;
      SavedLoadout.SelectedMount? sameValueOrDefault = this.targets.GetSameValueOrDefault<SavedLoadout.SelectedMount?>((NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<SavedUnit>.GetField<SavedLoadout.SelectedMount?>) (saved => new SavedLoadout.SelectedMount?(((SavedAircraft) saved).savedLoadout.Selected[hardpointIndex])), new SavedLoadout.SelectedMount?());
      weaponSelector.Initialize(hardpointSet, sameValueOrDefault);
      weaponSelector.OnWeaponSelected += (Action<WeaponMount>) (value => this.WeaponSelected(hardpointIndex, value));
      this.weaponSelectors.Add(weaponSelector);
    }
    this.RebuildAircraftLoadout();
  }

  public static void CheckSavedLoadoutLength(
    ref SavedLoadout loadout,
    WeaponManager weaponManager,
    AircraftParameters aircraftParameters)
  {
    if (loadout == null)
      loadout = new SavedLoadout();
    List<SavedLoadout.SelectedMount> selected = loadout.Selected;
    while (selected.Count > weaponManager.hardpointSets.Length)
      selected.RemoveAt(selected.Count - 1);
    List<WeaponMount> weapons = ((IEnumerable<StandardLoadout>) aircraftParameters.StandardLoadouts).FirstOrDefault<StandardLoadout>()?.loadout.weapons;
    if (weapons != null && weapons.Count != weaponManager.hardpointSets.Length)
      Debug.LogError((object) "standardWeapons did not have the same length as hardpointSets");
    while (selected.Count < weaponManager.hardpointSets.Length)
    {
      int nextIndex = selected.Count;
      WeaponMount weaponOption;
      if (weapons != null)
      {
        weaponOption = nextIndex < weapons.Count ? weapons[nextIndex] : (WeaponMount) null;
      }
      else
      {
        HardpointSet hardpointSet = weaponManager.hardpointSets[nextIndex];
        weaponOption = !hardpointSet.precludingHardpointSets.Any<byte>((Func<byte, bool>) (x => (int) x < nextIndex)) ? (hardpointSet.weaponOptions.Count >= 2 ? hardpointSet.weaponOptions[1] : (WeaponMount) null) : (WeaponMount) null;
      }
      selected.Add(new SavedLoadout.SelectedMount()
      {
        Key = (UnityEngine.Object) weaponOption != (UnityEngine.Object) null ? weaponOption.jsonKey : ""
      });
    }
  }

  public void WeaponSelected(int hardpointIndex, WeaponMount weaponMount)
  {
    SavedLoadout.SelectedMount selectedMount = new SavedLoadout.SelectedMount()
    {
      Key = (UnityEngine.Object) weaponMount != (UnityEngine.Object) null ? weaponMount.jsonKey : ""
    };
    Debug.Log((object) $"from option {weaponMount}, Adding {selectedMount} to loadout");
    foreach (SavedAircraft target in (IEnumerable<SavedUnit>) this.targets.Targets)
      target.savedLoadout.Selected[hardpointIndex] = selectedMount;
    this.RebuildAircraftLoadout();
  }

  public void RebuildAircraftLoadout()
  {
    foreach (SavedUnit target in (IEnumerable<SavedUnit>) this.targets.Targets)
    {
      SavedUnit savedUnit;
      SavedLoadout savedLoadout = ((SavedAircraft) (savedUnit = target)).savedLoadout;
      Aircraft unit = (Aircraft) savedUnit.Unit;
      unit.Networkloadout = savedLoadout.CreateLoadout(unit.weaponManager);
    }
    Loadout loadout = ((Aircraft) this.targets.Targets[0].Unit).loadout;
    foreach (WeaponSelector weaponSelector in this.weaponSelectors)
      weaponSelector.SetInteractable(loadout);
  }
}
