// Decompiled with JetBrains decompiler
// Type: NuclearOption.MissionEditorScripts.IPlacingMenu
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

#nullable disable
namespace NuclearOption.MissionEditorScripts;

public interface IPlacingMenu
{
  (bool placeMore, IEditorSelectable placedObject) Place(bool shift);

  void CancelPlace();

  void MoveCursor(Transform placementTransform);
}
