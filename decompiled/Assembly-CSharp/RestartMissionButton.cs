// Decompiled with JetBrains decompiler
// Type: RestartMissionButton
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Cysharp.Threading.Tasks;
using JamesFrowen.ScriptableVariables.UI;
using NuclearOption.BuildScripts;
using NuclearOption.Networking;
using UnityEngine;

#nullable disable
public class RestartMissionButton : ButtonController
{
  protected override void Awake()
  {
    base.Awake();
    if (RestartMissionButton.RestartAllowed())
      return;
    this.gameObject.SetActive(false);
  }

  protected override void onClick()
  {
    if (RestartMissionButton.RestartAllowed())
      MissionManager.RestartMission().Forget();
    else
      Debug.LogError((object) "Restart Mission pressed, but was not single player");
  }

  private static bool RestartAllowed()
  {
    if (GameManager.gameState == GameState.SinglePlayer)
      return true;
    return NetworkManagerNuclearOption.i.Server.Active && CommandLineArgParser.IsAutoStart;
  }
}
