// Decompiled with JetBrains decompiler
// Type: NuclearOption.MissionEditorScripts.FilterSet
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.SavedMission;
using NuclearOption.SavedMission.ObjectiveV2;
using System;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
namespace NuclearOption.MissionEditorScripts;

public class FilterSet
{
  private readonly Dictionary<object, FilterSet.Filter> filters = new Dictionary<object, FilterSet.Filter>();
  public static readonly string noFilter = "<no filter>".AddColor(new Color(0.5f, 0.5f, 0.5f));

  public int Count => this.filters.Count;

  public event Action OnFilterChanged;

  public void Apply(object key, FilterSet.Filter filter)
  {
    this.filters[key] = filter;
    Action onFilterChanged = this.OnFilterChanged;
    if (onFilterChanged == null)
      return;
    onFilterChanged();
  }

  public void Clear(object key)
  {
    this.filters.Remove(key);
    Action onFilterChanged = this.OnFilterChanged;
    if (onFilterChanged == null)
      return;
    onFilterChanged();
  }

  public bool FilterItem(object item)
  {
    foreach (FilterSet.Filter filter in this.filters.Values)
    {
      if (!filter(item))
        return false;
    }
    return true;
  }

  public static void AddFilterFaction(
    Transform parent,
    FilterSet filterSet,
    DropdownDataField dropdownPrefab)
  {
    DropdownDataField filter = UnityEngine.Object.Instantiate<DropdownDataField>(dropdownPrefab, parent);
    filter.transform.SetAsFirstSibling();
    FilterSet.SetupFilterFaction(filter, "Faction Filter:", filterSet);
  }

  public static void AddFilterPlacement(
    Transform parent,
    FilterSet filterSet,
    DropdownDataField dropdownPrefab,
    bool includeAttached)
  {
    DropdownDataField filter = UnityEngine.Object.Instantiate<DropdownDataField>(dropdownPrefab, parent);
    filter.transform.SetAsFirstSibling();
    FilterSet.SetupFilterPlacement(filter, "Placement:", filterSet, includeAttached);
  }

  public static void AddFilterUnitType(
    Transform parent,
    FilterSet filterSet,
    DropdownDataField dropdownPrefab)
  {
    DropdownDataField filter = UnityEngine.Object.Instantiate<DropdownDataField>(dropdownPrefab, parent);
    filter.transform.SetAsFirstSibling();
    FilterSet.SetupFilterUnitType(filter, "Type Filter:", filterSet);
  }

  public static void AddFilterMapBuildings(
    Transform parent,
    FilterSet filterSet,
    BoolDataField togglePrefab)
  {
    BoolDataField filter = UnityEngine.Object.Instantiate<BoolDataField>(togglePrefab, parent);
    filter.transform.SetAsFirstSibling();
    FilterSet.SetupFilterMapObjects(filter, "Include Map Buildings:", false, filterSet);
  }

  public static void SetupFilterMapObjects(
    BoolDataField filter,
    string label,
    bool startingValue,
    FilterSet filterSet)
  {
    filter.Setup<ValueWrapperBool, bool>(label, startingValue, new Action<bool>(GetFilter));
    GetFilter(startingValue);

    static bool HideMapBuildings(object obj)
    {
      return (!(obj is SavedBuilding savedBuilding) ? 0 : (savedBuilding.PlacementType == PlacementType.BuiltIn ? 1 : 0)) == 0;
    }

    void GetFilter(bool isOn)
    {
      if (isOn)
        filterSet.Clear((object) "ShowMapBuildings");
      else
        filterSet.Apply((object) "ShowMapBuildings", new FilterSet.Filter(HideMapBuildings));
    }
  }

  public static void SetupFilterPlacement(
    DropdownDataField filter,
    string label,
    FilterSet filterSet,
    bool includeAttached)
  {
    List<string> options = new List<string>(5);
    options.Add(FilterSet.noFilter);
    options.Add("BuiltIn");
    options.Add("Override");
    if (includeAttached)
      options.Add("Attached");
    options.Add("Custom");
    FilterSet.SetupFilter(filter, label, "PlacementType", filterSet, options, new FilterSet.FilterFromOption(TypeFilter));

    static FilterSet.Filter TypeFilter(string option)
    {
      return (FilterSet.Filter) (obj =>
      {
        PlacementType placementType = ((IHasPlacementType) obj).PlacementType;
        switch (option)
        {
          case "BuiltIn":
            return placementType == PlacementType.BuiltIn;
          case "Override":
            return placementType == PlacementType.Override;
          case "Attached":
            return placementType == PlacementType.Attached;
          case "Custom":
            return placementType == PlacementType.Custom;
          default:
            return false;
        }
      });
    }
  }

  public static void SetupFilterFaction(
    DropdownDataField filter,
    string label,
    FilterSet filterSet)
  {
    List<string> options = new List<string>()
    {
      FilterSet.noFilter,
      "None",
      "Boscali",
      "Primeva"
    };
    FilterSet.SetupFilter(filter, label, "FilterFaction", filterSet, options, new FilterSet.FilterFromOption(FactionFilter));

    static FilterSet.Filter FactionFilter(string filterName)
    {
      return (FilterSet.Filter) (obj => ((IHasFaction) obj).BelongsToFaction(filterName));
    }
  }

  public static void SetupFilterUnitType(
    DropdownDataField filter,
    string label,
    FilterSet filterSet)
  {
    List<string> options = new List<string>()
    {
      FilterSet.noFilter,
      "Aircraft",
      "Buildings",
      "Ships",
      "Vehicles",
      "Scenery",
      "Containers",
      "Missiles",
      "Pilots"
    };
    FilterSet.SetupFilter(filter, label, "FilterUnitType", filterSet, options, new FilterSet.FilterFromOption(UnitFilter));

    static FilterSet.Filter UnitFilter(string filterName)
    {
      switch (filterName)
      {
        case "Aircraft":
          return (FilterSet.Filter) (obj => obj is SavedAircraft);
        case "Buildings":
          return (FilterSet.Filter) (obj => obj is SavedBuilding);
        case "Containers":
          return (FilterSet.Filter) (obj => obj is SavedContainer);
        case "Missiles":
          return (FilterSet.Filter) (obj => obj is SavedMissile);
        case "Pilots":
          return (FilterSet.Filter) (obj => obj is SavedPilot);
        case "Scenery":
          return (FilterSet.Filter) (obj => obj is SavedScenery);
        case "Ships":
          return (FilterSet.Filter) (obj => obj is SavedShip);
        case "Vehicles":
          return (FilterSet.Filter) (obj => obj is SavedVehicle);
        default:
          throw new KeyNotFoundException("No type with name " + filterName);
      }
    }
  }

  public static void SetupToggleFilter(
    BoolDataField field,
    string filterKey,
    string label,
    bool value,
    FilterSet filterSet,
    FilterSet.Filter filter)
  {
    ValueWrapperBool wrapper = ValueWrapper.FromCallback<ValueWrapperBool, bool>(value, new Action<bool>(OnSet));
    field.Setup(label, (IValueWrapper<bool>) wrapper);
    OnSet(value);

    void OnSet(bool value)
    {
      if (value)
        filterSet.Apply((object) filterKey, filter);
      else
        filterSet.Clear((object) filterKey);
    }
  }

  private static void SetupFilter(
    DropdownDataField filter,
    string label,
    string filterKey,
    FilterSet filterSet,
    List<string> options,
    FilterSet.FilterFromOption filterFunc)
  {
    filter.Setup(label, options, (string) null, (Action<string>) (filterName =>
    {
      if (filterName == FilterSet.noFilter)
        filterSet.Clear((object) filterKey);
      else
        filterSet.Apply((object) filterKey, filterFunc(filterName));
    }));
  }

  public delegate bool Filter(object item);

  public delegate void ApplyFilter(object key, FilterSet.Filter filter);

  public delegate void ClearFilter(object key);

  public delegate FilterSet.Filter FilterFromOption(string option);
}
