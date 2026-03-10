// Decompiled with JetBrains decompiler
// Type: NuclearOption.MissionEditorScripts.SingleSelectionDetails
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.SavedMission.ObjectiveV2;
using UnityEngine;

#nullable disable
namespace NuclearOption.MissionEditorScripts;

public abstract class SingleSelectionDetails : SelectionDetails
{
  public readonly IEditorSelectable Source;

  protected SingleSelectionDetails(
    IEditorSelectable source,
    IValueWrapper<GlobalPosition> positionWrapper,
    IValueWrapper<Quaternion> rotationWrapper)
  {
    this.Source = source;
    this.PositionWrapper = positionWrapper;
    this.RotationWrapper = rotationWrapper;
  }
}
