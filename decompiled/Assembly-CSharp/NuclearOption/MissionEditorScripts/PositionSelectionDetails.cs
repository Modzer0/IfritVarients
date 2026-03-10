// Decompiled with JetBrains decompiler
// Type: NuclearOption.MissionEditorScripts.PositionSelectionDetails
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.SavedMission.ObjectiveV2;
using NuclearOption.SavedMission.ObjectiveV2.Objectives;
using System;
using UnityEngine;

#nullable disable
namespace NuclearOption.MissionEditorScripts;

public class PositionSelectionDetails : SingleSelectionDetails
{
  public PositionHandle Handle;
  private readonly Action destroyCallback;

  public PositionSelectionDetails(PositionHandle handle, Action destroyCallback)
    : base((IEditorSelectable) handle, handle.PositionWrapper, (IValueWrapper<Quaternion>) null)
  {
    this.Handle = handle;
    this.destroyCallback = destroyCallback;
  }

  public override string DisplayName => this.Handle.GetDisplayName();

  public override bool IsDestroyed => (UnityEngine.Object) this.Handle == (UnityEngine.Object) null;

  public override void Focus()
  {
    Vector3 localPosition = this.PositionWrapper.Value.ToLocalPosition();
    SceneSingleton<CameraStateManager>.i.FocusPosition(localPosition, new Quaternion?(), 50f);
  }

  public override bool Delete()
  {
    if (this.destroyCallback == null)
      return false;
    this.destroyCallback();
    return true;
  }
}
