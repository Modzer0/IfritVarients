// Decompiled with JetBrains decompiler
// Type: TeamKillPanel
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.Networking;
using TMPro;
using UnityEngine;

#nullable disable
public class TeamKillPanel : SceneSingleton<TeamKillPanel>
{
  [SerializeField]
  private TMP_Text teamkillText;
  [SerializeField]
  private Transform panel;
  private Player suspectPlayer;

  protected override void Awake()
  {
    base.Awake();
    this.panel.gameObject.SetActive(false);
  }

  public void ShowTeamKillPanel(Player teamKiller)
  {
    this.teamkillText.text = "Teamkilled by " + teamKiller.PlayerName;
    this.suspectPlayer = teamKiller;
  }

  public void KickPlayer()
  {
    if (!((Object) this.suspectPlayer != (Object) null) || !GameManager.GetLocalPlayer<Player>(out Player _))
      return;
    Debug.LogWarning((object) "KickPlayer Not Implemented");
  }
}
