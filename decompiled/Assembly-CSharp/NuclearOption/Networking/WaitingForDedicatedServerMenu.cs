// Decompiled with JetBrains decompiler
// Type: NuclearOption.Networking.WaitingForDedicatedServerMenu
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Mirage;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

#nullable disable
namespace NuclearOption.Networking;

public class WaitingForDedicatedServerMenu : MonoBehaviour
{
  [SerializeField]
  private Button DisconnectButton;
  [SerializeField]
  private TextMeshProUGUI loadingText;
  [SerializeField]
  private TextMeshProUGUI pingText;
  private float updateTime;

  private void Awake()
  {
    this.DisconnectButton.onClick.AddListener(new UnityAction(this.DisconnectClicked));
  }

  private void Update()
  {
    float timeSinceLevelLoad = Time.timeSinceLevelLoad;
    if ((double) timeSinceLevelLoad <= (double) this.updateTime)
      return;
    this.updateTime = timeSinceLevelLoad + 1f;
    NetworkClient client = NetworkManagerNuclearOption.i.Client;
    if (!client.Active)
      return;
    this.pingText.text = $"{(int) (client.World.Time.Rtt * 1000.0)}ms";
  }

  public void SetLoadingMessage(string message) => this.loadingText.text = message ?? "";

  private void DisconnectClicked() => NetworkManagerNuclearOption.i.Stop(true);
}
