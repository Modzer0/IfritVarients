// Decompiled with JetBrains decompiler
// Type: NuclearOption.SavedMission.SavedAirbase
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using JamesFrowen.ScriptableVariables;
using NuclearOption.SavedMission.ObjectiveV2;
using RoadPathfinding;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#nullable disable
namespace NuclearOption.SavedMission;

[Serializable]
public class SavedAirbase : ISaveableReference, IHasFaction, IHasPlacementType
{
  private static readonly string noNameStr = "<no name>".AddColor(new Color(0.5f, 0.5f, 0.5f));
  public static readonly string builtInStr = "(built in) ".AddColor(new Color(0.8f, 0.8f, 1f)).AddSize(0.7f);
  public static readonly string overrideStr = "(override) ".AddColor(new Color(0.8f, 1f, 0.8f)).AddSize(0.7f);
  private static readonly string attachedStr = "(attached) ".AddColor(new Color(1f, 0.8f, 0.8f)).AddSize(0.7f);
  [HideInInspector]
  public bool IsOverride;
  [FactionField]
  public string faction;
  public string UniqueName;
  public string DisplayName;
  public bool Disabled;
  public bool Capturable = true;
  public float CaptureDefense = 10f;
  public float CaptureRange = 1000f;
  [HideInInspector]
  public GlobalPosition Center;
  [HideInInspector]
  public GlobalPosition SelectionPosition;
  [SavedBuildingField]
  [ReadOnly]
  [Tooltip("Edit MapTower instead")]
  public string Tower;
  [HideInInspector]
  public List<GlobalPosition> VerticalLandingPoints = new List<GlobalPosition>();
  [HideInInspector]
  public List<GlobalPosition> ServicePoints = new List<GlobalPosition>();
  [HideInInspector]
  public RoadNetwork roads = new RoadNetwork();
  [HideInInspector]
  public List<SavedRunway> runways = new List<SavedRunway>();
  [NonSerialized]
  public SavedBuilding TowerRef;
  [NonSerialized]
  public readonly List<SavedBuilding> BuildingsRef = new List<SavedBuilding>();
  [NonSerialized]
  public Airbase Airbase;
  [NonSerialized]
  public bool SavedInMission;
  [NonSerialized]
  public ValueWrapperGlobalPosition CenterWrapper = new ValueWrapperGlobalPosition();
  [NonSerialized]
  public ValueWrapperGlobalPosition SelectionPositionWrapper = new ValueWrapperGlobalPosition();
  [NonSerialized]
  public List<ValueWrapperGlobalPosition> VerticalLandingPointsWrappers = new List<ValueWrapperGlobalPosition>();
  [NonSerialized]
  public List<ValueWrapperGlobalPosition> ServicePointsWrappers = new List<ValueWrapperGlobalPosition>();

  public SavedAirbase()
  {
    this.AfterLoadEditor((Airbase) null, (IReadOnlyList<SavedBuilding>) null);
  }

  public static SavedAirbase CreateOverride(SavedAirbase other)
  {
    return new SavedAirbase(other) { IsOverride = true };
  }

  private SavedAirbase(SavedAirbase other)
  {
    this.AfterLoadEditor((Airbase) null, (IReadOnlyList<SavedBuilding>) null);
    this.faction = other.faction;
    this.UniqueName = other.UniqueName;
    this.DisplayName = other.DisplayName;
    this.Disabled = other.Disabled;
    this.Capturable = other.Capturable;
    this.CaptureDefense = other.CaptureDefense;
    this.CaptureRange = other.CaptureRange;
    this.Center = other.Center;
    this.SelectionPosition = other.SelectionPosition;
    this.Tower = other.Tower;
    this.TowerRef = other.TowerRef;
    this.BuildingsRef = new List<SavedBuilding>((IEnumerable<SavedBuilding>) other.BuildingsRef);
    this.CenterWrapper = other.CenterWrapper;
    this.SelectionPositionWrapper = other.SelectionPositionWrapper;
  }

  string IHasFaction.FactionName => this.faction;

  string ISaveableReference.UniqueName => this.UniqueName;

  bool ISaveableReference.Destroyed { get; set; }

  bool ISaveableReference.CanBeReference => true;

  bool ISaveableReference.CanBeSorted => false;

  PlacementType IHasPlacementType.PlacementType => this.CalculatePlacementType();

  bool IHasPlacementType.CanBeAttached => true;

  public void BeforeSave()
  {
    this.Tower = this.TowerRef != null ? this.TowerRef.UniqueName : "";
    this.VerticalLandingPoints.Clear();
    foreach (ValueWrapper<GlobalPosition> landingPointsWrapper in this.VerticalLandingPointsWrappers)
      this.VerticalLandingPoints.Add(landingPointsWrapper.Value);
    this.ServicePoints.Clear();
    foreach (ValueWrapper<GlobalPosition> servicePointsWrapper in this.ServicePointsWrappers)
      this.ServicePoints.Add(servicePointsWrapper.Value);
  }

  public void AfterLoadEditor(Airbase buildInAirbase, IReadOnlyList<SavedBuilding> allBuildings)
  {
    this.CenterWrapper.SetValue(this.Center, (object) this, true);
    this.SelectionPositionWrapper.SetValue(this.SelectionPosition, (object) this, true);
    this.CenterWrapper.RegisterOnChange((object) this, (ValueWrapper<GlobalPosition>.OnChangeDelegate) (v => this.Center = v));
    this.SelectionPositionWrapper.RegisterOnChange((object) this, (ValueWrapper<GlobalPosition>.OnChangeDelegate) (v => this.SelectionPosition = v));
    if ((UnityEngine.Object) buildInAirbase != (UnityEngine.Object) null)
    {
      this.TowerRef = (UnityEngine.Object) buildInAirbase.MapTower != (UnityEngine.Object) null ? (SavedBuilding) buildInAirbase.MapTower.SavedUnit : (SavedBuilding) null;
      this.TowerRef?.SetAirbase(this);
    }
    else if (allBuildings != null)
    {
      if (!string.IsNullOrEmpty(this.Tower))
      {
        SavedBuilding savedBuilding = allBuildings.FirstOrDefault<SavedBuilding>((Func<SavedBuilding, bool>) (b => b.UniqueName == this.Tower));
        if (savedBuilding != null)
        {
          this.TowerRef = savedBuilding;
          this.TowerRef.SetAirbase(this);
        }
        else
          Debug.LogWarning((object) ("Failed to find tower with named " + this.Tower));
      }
      else
        this.TowerRef = (SavedBuilding) null;
    }
    this.VerticalLandingPointsWrappers.Clear();
    foreach (GlobalPosition verticalLandingPoint in this.VerticalLandingPoints)
      this.VerticalLandingPointsWrappers.Add(new ValueWrapperGlobalPosition(verticalLandingPoint));
    this.ServicePointsWrappers.Clear();
    foreach (GlobalPosition servicePoint in this.ServicePoints)
      this.ServicePointsWrappers.Add(new ValueWrapperGlobalPosition(servicePoint));
  }

  public string ToUIString(bool oneLine = false)
  {
    string uiString = this.UIStringFirstLine();
    if (oneLine)
      return uiString;
    string str = !((UnityEngine.Object) this.Airbase != (UnityEngine.Object) null) ? FactionHelper.ToUIString(this.faction) : FactionHelper.ToUIString(this.Airbase.CurrentHQ);
    return $"{uiString}\nFaction:{str}";
  }

  private string UIStringFirstLine()
  {
    string str1 = string.IsNullOrEmpty(this.DisplayName) ? SavedAirbase.noNameStr : this.DisplayName;
    string str2 = this.UniqueName.AddColor(new Color(0.7f, 0.7f, 0.7f));
    if ((UnityEngine.Object) this.Airbase != (UnityEngine.Object) null)
    {
      if (this.Airbase.AttachedAirbase && this.IsOverride)
        return SavedAirbase.attachedStr + SavedAirbase.overrideStr + str1;
      if (this.Airbase.AttachedAirbase)
        return SavedAirbase.attachedStr + str1;
      if (this.IsOverride)
        return $"{SavedAirbase.overrideStr}{str1} - [{str2}]";
      if (this.Airbase.BuiltIn)
        return $"{SavedAirbase.builtInStr}{str1} - [{str2}]";
    }
    return $"{str1} - [{str2}]";
  }

  private PlacementType CalculatePlacementType()
  {
    if ((UnityEngine.Object) this.Airbase != (UnityEngine.Object) null)
    {
      if (this.Airbase.AttachedAirbase)
        return PlacementType.Attached;
      if (this.IsOverride)
        return PlacementType.Override;
      if (this.Airbase.BuiltIn)
        return PlacementType.BuiltIn;
    }
    return PlacementType.Custom;
  }
}
