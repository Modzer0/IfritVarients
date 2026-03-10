// Decompiled with JetBrains decompiler
// Type: NuclearOption.MissionEditorScripts.MissionEditorInput
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Rewired;
using UnityEngine;

#nullable disable
namespace NuclearOption.MissionEditorScripts;

public class MissionEditorInput : MonoBehaviour
{
  private Player player;

  private void Awake() => this.player = ReInput.players.GetPlayer(0);

  private void Update()
  {
    if (InputFieldChecker.InsideInputField)
      return;
    if (this.player.GetButtonDown("FocusUnit"))
      SceneSingleton<UnitSelection>.i.SelectionDetails?.Focus();
    if (!this.player.GetButtonDown("DeleteUnit"))
      return;
    SelectionDetails selectionDetails = SceneSingleton<UnitSelection>.i.SelectionDetails;
    if (selectionDetails == null || !selectionDetails.Delete())
      return;
    SceneSingleton<UnitSelection>.i.ClearSelection();
  }
}
