// Decompiled with JetBrains decompiler
// Type: NuclearOption.MissionEditorScripts.SelectionDetails
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.SavedMission.ObjectiveV2;
using RuntimeHandle;
using UnityEngine;

#nullable disable
namespace NuclearOption.MissionEditorScripts;

public abstract class SelectionDetails
{
  public abstract string DisplayName { get; }

  public abstract bool IsDestroyed { get; }

  public virtual bool TryGetFaction(out Faction faction)
  {
    faction = (Faction) null;
    return false;
  }

  public virtual bool AutoUnhover => true;

  public virtual HandleAxes AllowedPositionAxes => HandleAxes.XYZ;

  public virtual bool PositionHandleAllowed => this.PositionWrapper != null;

  public virtual bool RotationHandleAllowed => this.RotationWrapper != null;

  public IValueWrapper<GlobalPosition> PositionWrapper { get; protected set; }

  public IValueWrapper<Quaternion> RotationWrapper { get; protected set; }

  public abstract void Focus();

  public abstract bool Delete();

  public override string ToString() => this.GetType().Name;
}
