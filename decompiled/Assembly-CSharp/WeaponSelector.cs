// Decompiled with JetBrains decompiler
// Type: WeaponSelector
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.Networking;
using NuclearOption.SavedMission;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

#nullable disable
public class WeaponSelector : MonoBehaviour
{
  [SerializeField]
  private TMP_Text hardpointName;
  [FormerlySerializedAs("weaponOptions")]
  [SerializeField]
  private TMP_Dropdown dropdown;
  [SerializeField]
  private TextMeshProUGUI dropdownText;
  [SerializeField]
  private Color dropdownTextNormalColor;
  [FormerlySerializedAs("dropdownTextNormalInactive")]
  [SerializeField]
  private Color dropdownTextInactiveColor;
  private readonly List<WeaponMount> getCache = new List<WeaponMount>();
  private readonly List<(WeaponSelector.OptionType type, WeaponMount mount)> dropdownOptions = new List<(WeaponSelector.OptionType, WeaponMount)>();
  private HardpointSet hardpointSet;

  public event Action<WeaponMount> OnWeaponSelected;

  private void Awake()
  {
    this.dropdown.onValueChanged.AddListener(new UnityAction<int>(this.DropdownChanged));
  }

  private void DropdownChanged(int index)
  {
    WeaponMount weaponMount = this.GetValue();
    if (this.dropdownOptions[0].type == WeaponSelector.OptionType.DifferentValues)
    {
      if (index == 0)
        return;
      this.dropdownOptions.RemoveAt(0);
      this.dropdown.options.RemoveAt(0);
      this.dropdown.SetValueWithoutNotify(index - 1);
      this.dropdown.RefreshShownValue();
    }
    Action<WeaponMount> onWeaponSelected = this.OnWeaponSelected;
    if (onWeaponSelected == null)
      return;
    onWeaponSelected(weaponMount);
  }

  public void Initialize(HardpointSet hardpointSet, FactionHQ HQ, Airbase airbase)
  {
    this.hardpointSet = hardpointSet;
    this.hardpointName.text = hardpointSet.name;
    this.PopulateOptions(airbase, HQ, hardpointSet);
  }

  public void Initialize(HardpointSet hardpointSet, SavedLoadout.SelectedMount? selectedMount)
  {
    this.hardpointSet = hardpointSet;
    this.hardpointName.text = hardpointSet.name;
    this.PopulateOptions((Airbase) null, (FactionHQ) null, hardpointSet, !selectedMount.HasValue);
    if (selectedMount.HasValue)
      this.SetValue(selectedMount.Value.GetWeaponMount(hardpointSet));
    else
      this.dropdown.SetValueWithoutNotify(0);
  }

  public void SetInteractable(Loadout loadout)
  {
    bool flag = this.hardpointSet.BlockedByOtherHardpoint(loadout);
    this.dropdown.interactable = !flag;
    this.dropdownText.color = flag ? this.dropdownTextInactiveColor : this.dropdownTextNormalColor;
    if (!flag)
      return;
    this.dropdown.value = 0;
  }

  private void PopulateOptions(
    Airbase airbase,
    FactionHQ hq,
    HardpointSet hardpointSet,
    bool includeDifferentMultiselectValue = false)
  {
    this.dropdown.ClearOptions();
    this.hardpointSet = hardpointSet;
    FactionHQ HQ;
    int? playerRank;
    int? warheadsAvailable;
    if (GameManager.gameState != GameState.Editor)
    {
      HQ = hq;
      Player localPlayer;
      playerRank = GameManager.GetLocalPlayer<Player>(out localPlayer) ? new int?(localPlayer.PlayerRank) : new int?();
      warheadsAvailable = (UnityEngine.Object) airbase != (UnityEngine.Object) null ? new int?(airbase.GetWarheads()) : new int?();
    }
    else
    {
      HQ = (FactionHQ) null;
      playerRank = new int?();
      warheadsAvailable = new int?();
    }
    WeaponChecker.GetAvailableWeaponsNonAlloc(airbase, playerRank, hardpointSet, HQ, warheadsAvailable, false, this.getCache);
    this.getCache.Sort((Comparison<WeaponMount>) ((x, y) => x.mountName.CompareTo(y.mountName)));
    if (includeDifferentMultiselectValue)
      this.dropdownOptions.Add((WeaponSelector.OptionType.DifferentValues, (WeaponMount) null));
    this.dropdownOptions.Add((WeaponSelector.OptionType.Empty, (WeaponMount) null));
    foreach (WeaponMount weaponMount in this.getCache)
      this.dropdownOptions.Add((WeaponSelector.OptionType.Weapon, weaponMount));
    for (int index = 0; index < this.dropdownOptions.Count; ++index)
    {
      (WeaponSelector.OptionType type, WeaponMount mount) = this.dropdownOptions[index];
      string text;
      switch (type)
      {
        case WeaponSelector.OptionType.DifferentValues:
          text = "-";
          break;
        case WeaponSelector.OptionType.Empty:
          text = "Empty";
          break;
        case WeaponSelector.OptionType.Weapon:
          text = mount.mountName;
          break;
        default:
          throw new InvalidEnumArgumentException();
      }
      this.dropdown.options.Add(new TMP_Dropdown.OptionData(text));
    }
  }

  public void SetValue(WeaponMount weaponMount)
  {
    int index = this.dropdownOptions.FindIndex((Predicate<(WeaponSelector.OptionType, WeaponMount)>) (x => x.type == WeaponSelector.OptionType.Empty && (UnityEngine.Object) weaponMount == (UnityEngine.Object) null || x.type == WeaponSelector.OptionType.Weapon && (UnityEngine.Object) x.mount == (UnityEngine.Object) weaponMount));
    if (index == -1)
      Debug.LogWarning((object) $"Count not find {weaponMount} in dropdown options");
    this.dropdown.SetValueWithoutNotify(index);
  }

  public WeaponMount GetValue() => this.dropdownOptions[this.dropdown.value].mount;

  private enum OptionType
  {
    DifferentValues,
    Empty,
    Weapon,
  }
}
