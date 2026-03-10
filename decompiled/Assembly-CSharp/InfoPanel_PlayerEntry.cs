// Decompiled with JetBrains decompiler
// Type: InfoPanel_PlayerEntry
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.Networking;
using UnityEngine;
using UnityEngine.UI;

#nullable disable
public class InfoPanel_PlayerEntry : MonoBehaviour
{
  private Player player;
  private InfoPanel_Faction factionInfoPanel;
  [SerializeField]
  private Text playerName;
  [SerializeField]
  private Button playerAircraft_Button;
  [SerializeField]
  private Text playerAircraft;
  [SerializeField]
  private Text playerRank;
  [SerializeField]
  private Text playerScore;
  [SerializeField]
  private Text playerFunds;
  private bool selected;
  [SerializeField]
  private Image selectedImg;
  private float lastRefresh;
  private float refreshRate = 1f;

  private void Awake() => this.lastRefresh = Time.timeSinceLevelLoad;

  public void Update()
  {
    if ((double) Time.timeSinceLevelLoad <= (double) this.lastRefresh + (double) this.refreshRate)
      return;
    if ((Object) this.player != (Object) null)
    {
      this.playerRank.text = this.player.PlayerRank.ToString("N0");
      this.playerScore.text = this.player.PlayerScore.ToString("N1");
      if ((Object) this.player.Aircraft != (Object) null)
      {
        this.playerAircraft.text = this.player.Aircraft.definition.code;
      }
      else
      {
        if (this.selected)
          this.Deselect();
        this.playerAircraft.text = "-";
      }
      this.playerFunds.text = $"$ {this.player.Allocation.ToString("N1")}M";
    }
    else
    {
      this.factionInfoPanel.RemoveNullPlayers();
      Object.Destroy((Object) this.gameObject);
    }
    this.lastRefresh = Time.timeSinceLevelLoad;
  }

  public void SetPlayer(Player p, InfoPanel_Faction panel)
  {
    this.player = p;
    this.playerName.text = p.PlayerName;
    this.factionInfoPanel = panel;
  }

  public void Select()
  {
    this.selected = true;
    this.selectedImg.enabled = true;
    SceneSingleton<DynamicMap>.i.DeselectAllIcons();
    SceneSingleton<DynamicMap>.i.SelectIcon((Unit) this.player.Aircraft);
    SceneSingleton<CameraStateManager>.i.SetFollowingUnit((Unit) this.player.Aircraft);
    SceneSingleton<GameplayUI>.i.ShowSpectatorPanel();
    SceneSingleton<DynamicMap>.i.enabled = true;
  }

  public void Deselect()
  {
    this.selected = false;
    this.selectedImg.enabled = false;
    SceneSingleton<DynamicMap>.i.DeselectIcon((Unit) this.player.Aircraft);
    SceneSingleton<CameraStateManager>.i.SetFollowingUnit((Unit) null);
    SceneSingleton<GameplayUI>.i.ShowSpectatorPanel();
    SceneSingleton<DynamicMap>.i.enabled = true;
  }

  public bool IsSelected() => this.selected;

  public void OnButtonClick()
  {
    if (!((Object) SceneSingleton<DynamicMap>.i.HQ == (Object) null) || !((Object) this.player.Aircraft != (Object) null))
      return;
    this.selected = !this.selected;
    if (this.selected)
    {
      SceneSingleton<GameplayUI>.i.SpectatorPanelDeselectAll();
      this.Select();
    }
    else
      this.Deselect();
  }
}
