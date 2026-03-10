// Decompiled with JetBrains decompiler
// Type: NuclearOption.SavedMission.SavedUnit
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.SavedMission.ObjectiveV2;
using System;
using UnityEngine;

#nullable disable
namespace NuclearOption.SavedMission;

[Serializable]
public abstract class SavedUnit : ISaveableReference, IHasFaction, IHasPlacementType
{
  public string type;
  public string faction;
  public string UniqueName;
  public GlobalPosition globalPosition;
  public Quaternion rotation;
  public Override<float> CaptureStrength;
  public Override<float> CaptureDefense;
  [Obsolete("Use UniqueName instead", true)]
  public string unitCustomID;
  [Obsolete("Use SpawnUnit Outcome instead", true)]
  public string spawnTiming;
  [NonSerialized]
  public PlacementType PlacementType;
  [NonSerialized]
  public Unit Unit;
  [NonSerialized]
  public readonly ValueWrapperGlobalPosition PositionWrapper = new ValueWrapperGlobalPosition();
  [NonSerialized]
  public readonly ValueWrapperQuaternion RotationWrapper = new ValueWrapperQuaternion();
  [NonSerialized]
  public bool HasSpawned;

  public event Action<string> OnUniqueNameChanged;

  public void AfterCreate(Unit unit, string uniqueName)
  {
    this.PlacementType = PlacementType.Custom;
    this.type = unit.definition.jsonKey;
    this.CaptureStrength = new Override<float>();
    this.UniqueName = uniqueName;
    unit.NetworkUniqueName = uniqueName;
    if ((UnityEngine.Object) unit.NetworkHQ != (UnityEngine.Object) null)
      this.faction = unit.NetworkHQ.faction.factionName;
    this.SetPosition(unit.transform, (object) this);
  }

  public void SetUniqueName(string newUniqueName)
  {
    this.UniqueName = newUniqueName;
    Action<string> uniqueNameChanged = this.OnUniqueNameChanged;
    if (uniqueNameChanged == null)
      return;
    uniqueNameChanged(newUniqueName);
  }

  public virtual void AfterLoadEditor()
  {
    this.PositionWrapper.SetValue(this.globalPosition, (object) this, true);
    this.RotationWrapper.SetValue(this.rotation, (object) this, true);
    this.PositionWrapper.RegisterOnChange((object) this, (ValueWrapper<GlobalPosition>.OnChangeDelegate) (v =>
    {
      this.globalPosition = v;
      if (!((UnityEngine.Object) this.Unit != (UnityEngine.Object) null))
        return;
      this.Unit.transform.position = v.ToLocalPosition();
    }));
    this.RotationWrapper.RegisterOnChange((object) this, (ValueWrapper<Quaternion>.OnChangeDelegate) (v =>
    {
      this.rotation = v;
      if (!((UnityEngine.Object) this.Unit != (UnityEngine.Object) null))
        return;
      this.Unit.transform.rotation = v;
    }));
  }

  public virtual void AfterAddOverride(Unit unit) => this.SetOverrideDefaultValues(unit);

  protected virtual void SetOverrideDefaultValues(Unit unit)
  {
    this.faction = (UnityEngine.Object) unit.MapHQ != (UnityEngine.Object) null ? unit.MapHQ.faction.factionName : "";
    this.CaptureStrength = new Override<float>();
    this.CaptureDefense = new Override<float>();
  }

  string IHasFaction.FactionName => this.faction;

  string ISaveableReference.UniqueName => this.UniqueName;

  bool ISaveableReference.Destroyed { get; set; }

  bool ISaveableReference.CanBeReference => true;

  bool ISaveableReference.CanBeSorted => false;

  PlacementType IHasPlacementType.PlacementType => this.PlacementType;

  bool IHasPlacementType.CanBeAttached => false;

  public void SetPosition(Transform target, object source)
  {
    Vector3 position;
    Quaternion rotation;
    target.GetPositionAndRotation(out position, out rotation);
    this.SetPosition(position.ToGlobalPosition(), rotation, source);
  }

  public void SetPosition(GlobalPosition globalPosition, Quaternion rotation, object source)
  {
    this.globalPosition = globalPosition;
    this.rotation = rotation;
    this.PositionWrapper.SetValue(globalPosition, source, true);
    this.RotationWrapper.SetValue(rotation, source, true);
  }

  public string ToUIString(bool oneLine = false)
  {
    string message = this.UniqueName;
    if (message.StartsWith("<MAP_UNIT>++"))
      message = message.Substring("<MAP_UNIT>++".Length);
    string str1 = message.AddColor(new Color(0.7f, 0.7f, 0.7f));
    string str2 = this.type.AddColor(ColorLog.ColorFromName(this.type, 0.2f, 1f));
    string str3;
    switch (this.PlacementType)
    {
      case PlacementType.BuiltIn:
        str3 = SavedAirbase.builtInStr;
        break;
      case PlacementType.Override:
        str3 = SavedAirbase.overrideStr;
        break;
      default:
        str3 = "";
        break;
    }
    string uiString1 = $"{str3}[{str1}] - Type:{str2}";
    if (oneLine)
      return uiString1;
    int num = SaveHelper.CountSpawnedBy(MissionManager.CurrentMission.Objectives, this);
    string uiString2 = FactionHelper.ToUIString(this.faction);
    return $"{uiString1}\nRef:{num} - Faction:{uiString2}";
  }
}
