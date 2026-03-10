// Decompiled with JetBrains decompiler
// Type: DebugUI
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Cysharp.Threading.Tasks;
using Mirage;
using Mirage.SocketLayer;
using NuclearOption.Networking;
using Rewired;
using System;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

#nullable disable
public class DebugUI : MonoBehaviour
{
  private static readonly ResourcesAsyncLoader<DebugUI> loader = ResourcesAsyncLoader.Create<DebugUI>(nameof (DebugUI), (Func<GameObject, DebugUI>) null);
  [SerializeField]
  private GameObject mission;
  [SerializeField]
  private Canvas canvas;
  [SerializeField]
  private GameObject graphy;
  [SerializeField]
  private GameObject performanceText;
  [SerializeField]
  private GameObject bandwidthText;
  [SerializeField]
  private GameObject[] ClientGraphs;
  [SerializeField]
  private NetworkRTTGraphDataSource networkDataSource;
  [SerializeField]
  private BandwidthText bandwidthTextSource;
  [SerializeField]
  private bool toggleLayouts_editorButton;
  private bool clientOnlyActive;
  private bool serverWithConnActive;

  public static DebugUI i => DebugUI.loader.Get();

  public static async UniTask Preload(CancellationToken cancel)
  {
    await DebugUI.loader.Load(cancel);
  }

  private void Awake() => DebugUI.loader.AssetNotLoaded();

  private void OnValidate()
  {
    if (!this.toggleLayouts_editorButton)
      return;
    this.toggleLayouts_editorButton = false;
    bool enabled = this.GetComponentInChildren<LayoutGroup>().enabled;
    foreach (Behaviour componentsInChild in this.GetComponentsInChildren<LayoutGroup>())
      componentsInChild.enabled = !enabled;
    foreach (Behaviour componentsInChild in this.GetComponentsInChildren<ContentSizeFitter>())
      componentsInChild.enabled = !enabled;
  }

  private void Start()
  {
    this.mission.SetActive(DebugUI.GetBoolPlayerPrefs("MissionDebug"));
    this.graphy.SetActive(DebugUI.GetBoolPlayerPrefs("GraphDebug"));
    this.performanceText.SetActive(DebugUI.GetBoolPlayerPrefs("PerfDebug"));
    this.OnChange();
  }

  private void OnChange()
  {
    if (this.serverWithConnActive)
      this.bandwidthTextSource.Metrics = NetworkManagerNuclearOption.i.Server.Metrics;
    else if (this.clientOnlyActive)
    {
      this.networkDataSource.Client = NetworkManagerNuclearOption.i.Client;
      this.bandwidthTextSource.Metrics = NetworkManagerNuclearOption.i.Client.Metrics;
    }
    else
      this.bandwidthTextSource.Metrics = (Metrics) null;
    this.canvas.gameObject.SetActive(this.graphy.activeSelf || this.performanceText.activeSelf);
    this.bandwidthText.SetActive(this.performanceText.activeSelf && this.bandwidthTextSource.Metrics != null);
    foreach (GameObject clientGraph in this.ClientGraphs)
      clientGraph.SetActive(this.clientOnlyActive);
  }

  private void Update()
  {
    if (MainMenu.State != MainMenu.LoadingState.Loaded || !ReInput.isReady)
      return;
    Rewired.Player player = ReInput.players.GetPlayer(0);
    if ((0 | (DebugUI.CheckToggle(player, "MissionDebug", this.mission) ? 1 : 0) | (DebugUI.CheckToggle(player, "GraphDebug", this.graphy) ? 1 : 0) | (DebugUI.CheckToggle(player, "PerfDebug", this.performanceText) ? 1 : 0) | (this.CheckNetwork() ? 1 : 0)) == 0)
      return;
    this.OnChange();
  }

  private static bool CheckToggle(Rewired.Player player, string input, GameObject target)
  {
    if (!player.GetButtonDown(input))
      return false;
    target.SetActive(!target.activeSelf);
    DebugUI.SetBoolPlayerPrefs(input, target.activeSelf);
    return true;
  }

  private bool CheckNetwork()
  {
    bool flag1 = NetworkManagerNuclearOption.i.Client.Active && !NetworkManagerNuclearOption.i.Server.Active;
    bool flag2 = NetworkManagerNuclearOption.i.Server.Active && (NetworkManagerNuclearOption.i.Server.AllPlayers.Count >= 2 || NetworkManagerNuclearOption.i.Server.AllPlayers.Count == 1 && !NetworkManagerNuclearOption.i.Server.AllPlayers.First<INetworkPlayer>().IsHost);
    int num = this.clientOnlyActive != flag1 ? 1 : (this.serverWithConnActive != flag2 ? 1 : 0);
    this.clientOnlyActive = flag1;
    this.serverWithConnActive = flag2;
    return num != 0;
  }

  private static bool GetBoolPlayerPrefs(string input)
  {
    return PlayerPrefs.GetInt("DebugUI_" + input, 0) == 1;
  }

  private static void SetBoolPlayerPrefs(string input, bool value)
  {
    PlayerPrefs.SetInt("DebugUI_" + input, value ? 1 : 0);
  }
}
