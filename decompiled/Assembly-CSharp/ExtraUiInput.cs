// Decompiled with JetBrains decompiler
// Type: ExtraUiInput
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.MissionEditorScripts;
using Rewired;
using UnityEngine;

#nullable disable
public class ExtraUiInput : MonoBehaviour
{
  private Player player;

  private void Awake() => this.player = ReInput.players.GetPlayer(0);

  private void Update()
  {
    if (this.player.GetButtonDown("Map"))
    {
      if ((Object) SceneSingleton<Leaderboard>.i != (Object) null || InputFieldChecker.InsideInputField)
        return;
      if (!DynamicMap.mapMaximized)
        SceneSingleton<DynamicMap>.i.Maximize();
      else
        SceneSingleton<DynamicMap>.i.Minimize();
    }
    CombatHUD i = SceneSingleton<CombatHUD>.i;
    if ((!((Object) i != (Object) null) || !i.gameObject.activeSelf) && !DynamicMap.mapMaximized || !i.HasTargets)
      return;
    if (GameManager.playerInput.GetButtonTimedPressUp("Cancel", 0.0f, PlayerSettings.clickDelay))
    {
      i.DeselectLast();
      if ((Object) i.aircraft.targetCam != (Object) null)
        i.aircraft.targetCam.CancelTarget();
    }
    if (!GameManager.playerInput.GetButtonTimedPressDown("Cancel", PlayerSettings.pressDelay))
      return;
    i.DeselectAll(true);
    SceneSingleton<DynamicMap>.i.DeselectAllIcons();
    if (!((Object) i.aircraft.targetCam != (Object) null))
      return;
    i.aircraft.targetCam.CancelTarget();
  }
}
