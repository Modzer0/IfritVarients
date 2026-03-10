// Decompiled with JetBrains decompiler
// Type: NuclearOption.MissionEditorScripts.AirbaseSelectionDetails
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.SavedMission.ObjectiveV2;
using System;
using UnityEngine;

#nullable disable
namespace NuclearOption.MissionEditorScripts;

public class AirbaseSelectionDetails : SingleSelectionDetails
{
  public readonly Airbase Airbase;

  public override string DisplayName
  {
    get
    {
      Unit attachedUnit;
      if (this.Airbase.TryGetAttachedUnit(out attachedUnit))
        return attachedUnit.UniqueName + " Airbase";
      return string.IsNullOrEmpty(this.Airbase.SavedAirbase.UniqueName) ? this.Airbase.name : this.Airbase.SavedAirbase.UniqueName;
    }
  }

  public override bool PositionHandleAllowed => false;

  public AirbaseSelectionDetails(Airbase airbase)
    : base((IEditorSelectable) airbase, (IValueWrapper<GlobalPosition>) AirbaseSelectionDetails.GetAirbasePosition(airbase), (IValueWrapper<Quaternion>) null)
  {
    this.Airbase = !((UnityEngine.Object) airbase == (UnityEngine.Object) null) ? airbase : throw new ArgumentNullException(nameof (Airbase));
  }

  private static ValueWrapperGlobalPosition GetAirbasePosition(Airbase airbase)
  {
    Unit attachedUnit;
    return airbase.TryGetAttachedUnit(out attachedUnit) ? attachedUnit.SavedUnit.PositionWrapper : airbase.SavedAirbase.CenterWrapper;
  }

  public override void Focus()
  {
    SceneSingleton<CameraStateManager>.i.FocusAirbase(this.Airbase, true);
  }

  public override bool Delete() => MissionEditor.RemoveAirbase(this.Airbase);

  public override bool IsDestroyed => false;

  public override bool TryGetFaction(out Faction faction)
  {
    faction = (UnityEngine.Object) this.Airbase.CurrentHQ != (UnityEngine.Object) null ? this.Airbase.CurrentHQ.faction : (Faction) null;
    return true;
  }
}
