// Decompiled with JetBrains decompiler
// Type: NuclearOption.MissionEditorScripts.DataDrawerHelper
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.SavedMission;
using NuclearOption.SavedMission.ObjectiveV2;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#nullable disable
namespace NuclearOption.MissionEditorScripts;

public static class DataDrawerHelper
{
  public static ReferenceList DrawList<T>(
    this DataDrawer drawer,
    int height,
    List<T> list,
    string typeName,
    Func<IEnumerable<T>> getAllOptions)
    where T : ISaveableReference
  {
    return drawer.DrawList<T>(height, $"Target {typeName}s", "Select " + typeName, list, (Func<T, string>) (x => x.ToUIString(true)), (Func<T, string>) (x => x.ToUIString(false)), getAllOptions);
  }

  public static ReferenceList DrawList(this DataDrawer drawer, int height, List<Objective> list)
  {
    return drawer.DrawList<Objective>(height, list, "Objective", (Func<IEnumerable<Objective>>) (() => (IEnumerable<Objective>) MissionManager.Objectives.AllObjectives));
  }

  public static ReferenceList DrawList(this DataDrawer drawer, int height, List<Outcome> list)
  {
    return drawer.DrawList<Outcome>(height, list, "Outcome", (Func<IEnumerable<Outcome>>) (() => (IEnumerable<Outcome>) MissionManager.Objectives.AllOutcomes));
  }

  public static ReferenceList DrawList(
    this DataDrawer drawer,
    int height,
    List<SavedUnit> list,
    bool includeBuiltIn)
  {
    return drawer.DrawList<SavedUnit>(height, list, "Unit", new Func<IEnumerable<SavedUnit>>(GetAllOptions));

    List<SavedUnit> GetAllOptions()
    {
      return MissionManager.GetAllSavedUnits(includeBuiltIn).Where<SavedUnit>((Func<SavedUnit, bool>) (x => !string.IsNullOrEmpty(x.UniqueName))).ToList<SavedUnit>();
    }
  }

  public static ReferenceList DrawList(this DataDrawer drawer, int height, List<SavedAirbase> list)
  {
    return drawer.DrawList<SavedAirbase>(height, list, "Airbase", (Func<IEnumerable<SavedAirbase>>) (() => (IEnumerable<SavedAirbase>) MissionManager.GetAllSavedAirbase()));
  }

  public static DropdownDataField DrawFactionDropdown(
    this DataDrawer drawer,
    string label,
    string value,
    Action<string> setValue)
  {
    List<string> factionsAndNeutral = FactionHelper.GetFactionsAndNeutral();
    return drawer.DrawDropdown(label, factionsAndNeutral, value, setValue);
  }

  public static void SetRectWidth(this Component comp, float width)
  {
    DataDrawerHelper.SetRectWidth((RectTransform) comp.transform, width);
  }

  public static void SetRectWidth(this RectTransform transform, float width)
  {
    Vector2 sizeDelta = transform.sizeDelta with
    {
      x = width
    };
    transform.sizeDelta = sizeDelta;
  }

  public static void SetRectHeight(this Component comp, float height)
  {
    DataDrawerHelper.SetRectHeight((RectTransform) comp.transform, height);
  }

  public static void SetRectHeight(this RectTransform transform, float height)
  {
    Vector2 sizeDelta = transform.sizeDelta with
    {
      y = height
    };
    transform.sizeDelta = sizeDelta;
  }

  public static void SetRectSize(this Component comp, Vector2 size)
  {
    ((RectTransform) comp.transform).sizeDelta = size;
  }

  public static RectTransform AsRectTransform(this Transform transform)
  {
    return (RectTransform) transform;
  }
}
