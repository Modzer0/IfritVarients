// Decompiled with JetBrains decompiler
// Type: NuclearOption.Chat.ChatManager
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Cysharp.Threading.Tasks;
using Mirage;
using Mirage.RemoteCalls;
using Mirage.Serialization;
using NuclearOption.Networking;
using System;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
namespace NuclearOption.Chat;

public class ChatManager : NetworkSceneSingleton<ChatManager>
{
  [Header("Message validation")]
  [SerializeField]
  private int lengthLimit = 128 /*0x80*/;
  [Header("Rate Limit")]
  [SerializeField]
  private int messageLimit = 5;
  [SerializeField]
  private float resetTime = 30f;
  [Header("Char Colors")]
  [SerializeField]
  private Color noFaction;
  [SerializeField]
  private Color alliedChat;
  [Tooltip("Changes faction saturation by this amount")]
  [SerializeField]
  private float allChatSaturation;
  private Dictionary<Player, RateLimiter> serverRateLimit = new Dictionary<Player, RateLimiter>();
  private RateLimiter localRateLimit;
  private List<Player> mutedPlayers = new List<Player>();
  [NonSerialized]
  private const int SYNC_VAR_COUNT = 0;
  [NonSerialized]
  private const int RPC_COUNT = 4;

  public static void SendChatMessage(string message, bool allChat)
  {
    if (!ChatManager.CanSend(message, false, true))
      return;
    NetworkSceneSingleton<ChatManager>.i.CmdSendChatMessage(message, allChat);
  }

  public static bool CanSend(string message, bool checkAsServer, bool realSend)
  {
    if (!NetworkSceneSingleton<ChatManager>.i.ValidateChatMessageSize(message))
    {
      if (realSend)
        throw new ArgumentException("Message size incorrect");
      return false;
    }
    Player localPlayer;
    if (!GameManager.GetLocalPlayer<Player>(out localPlayer))
    {
      Debug.LogError((object) "Can't send because no local player");
      return false;
    }
    if (NetworkSceneSingleton<ChatManager>.i.CheckRateLimit(localPlayer, checkAsServer, realSend))
      return true;
    if (realSend)
      throw new ArgumentException("Message rate limit");
    return false;
  }

  private bool ValidateChatMessageSize(string message)
  {
    return !string.IsNullOrEmpty(message) && message.Length <= this.lengthLimit;
  }

  private bool CheckRateLimit(Player player, bool checkAsServer, bool setSendTime)
  {
    RateLimiter rateLimiter;
    if (checkAsServer)
    {
      if (!this.serverRateLimit.TryGetValue(player, out rateLimiter))
      {
        rateLimiter = new RateLimiter(this.messageLimit, this.resetTime);
        this.serverRateLimit.Add(player, rateLimiter);
      }
    }
    else
    {
      if (this.localRateLimit == null)
        this.localRateLimit = new RateLimiter(this.messageLimit, this.resetTime);
      rateLimiter = this.localRateLimit;
    }
    float timeSinceLevelLoad = Time.timeSinceLevelLoad;
    if (rateLimiter.ShouldLimit(timeSinceLevelLoad))
      return false;
    if (setSendTime)
      rateLimiter.OnSend(timeSinceLevelLoad);
    return true;
  }

  [ServerRpc(requireAuthority = false)]
  private void CmdSendChatMessage(string message, bool allChat, INetworkPlayer sender = null)
  {
    if (ServerRpcSender.ShouldInvokeLocally((NetworkBehaviour) this, false, false))
    {
      this.UserCode_CmdSendChatMessage_\u002D456754112(message, allChat, this.Server.LocalPlayer);
    }
    else
    {
      PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
      writer.WriteString(message);
      writer.WriteBooleanExtension(allChat);
      ServerRpcSender.Send((NetworkBehaviour) this, 0, (NetworkWriter) writer, Mirage.Channel.Reliable, false);
      writer.Release();
    }
  }

  [ClientRpc(target = RpcTarget.Player)]
  public void TargetReceiveMessage(INetworkPlayer _, string message, Player player, bool allChat)
  {
    if (ClientRpcSender.ShouldInvokeLocally((NetworkBehaviour) this, RpcTarget.Player, _, false))
    {
      this.UserCode_TargetReceiveMessage_1307761090(this.Client.Player, message, player, allChat);
    }
    else
    {
      PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
      writer.WriteString(message);
      GeneratedNetworkCode._Write_NuclearOption\u002ENetworking\u002EPlayer((NetworkWriter) writer, player);
      writer.WriteBooleanExtension(allChat);
      ClientRpcSender.SendTarget((NetworkBehaviour) this, 1, (NetworkWriter) writer, Mirage.Channel.Reliable, _);
      writer.Release();
    }
  }

  [ClientRpc]
  public void RpcServerMessage(string message, bool runTtsIfEnabled)
  {
    if (ClientRpcSender.ShouldInvokeLocally((NetworkBehaviour) this, RpcTarget.Observers, (INetworkPlayer) null, false))
      this.UserCode_RpcServerMessage_1244201393(message, runTtsIfEnabled);
    PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
    writer.WriteString(message);
    writer.WriteBooleanExtension(runTtsIfEnabled);
    ClientRpcSender.Send((NetworkBehaviour) this, 2, (NetworkWriter) writer, Mirage.Channel.Reliable, false);
    writer.Release();
  }

  [ClientRpc(target = RpcTarget.Player)]
  public void RpcTargetServerMessage(INetworkPlayer _, string message, bool runTtsIfEnabled)
  {
    if (ClientRpcSender.ShouldInvokeLocally((NetworkBehaviour) this, RpcTarget.Player, _, false))
    {
      this.UserCode_RpcTargetServerMessage_\u002D537879009(this.Client.Player, message, runTtsIfEnabled);
    }
    else
    {
      PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
      writer.WriteString(message);
      writer.WriteBooleanExtension(runTtsIfEnabled);
      ClientRpcSender.SendTarget((NetworkBehaviour) this, 3, (NetworkWriter) writer, Mirage.Channel.Reliable, _);
      writer.Release();
    }
  }

  private async UniTask RunTTS(string playerName, string message)
  {
    await WindowsTTS.SpeakAsync(PlayerSettings.chatTtsSpeed, PlayerSettings.chatTtsVolume, $"{playerName} said: {message}", PlayerSettings.chatFilter);
  }

  private Color GetTextColor(FactionHQ senderHQ, bool allChat)
  {
    if ((UnityEngine.Object) senderHQ == (UnityEngine.Object) null)
      return this.noFaction;
    if (!allChat)
      return this.alliedChat;
    float H;
    float S;
    float V;
    Color.RGBToHSV(senderHQ.faction.color, out H, out S, out V);
    return Color.HSVToRGB(H, S * this.allChatSaturation, V);
  }

  public static bool IsMuted(Player player)
  {
    return NetworkSceneSingleton<ChatManager>.i.mutedPlayers.Contains(player);
  }

  public static bool ToggleMute(Player player)
  {
    if (ChatManager.IsMuted(player))
    {
      NetworkSceneSingleton<ChatManager>.i.mutedPlayers.Remove(player);
      return false;
    }
    NetworkSceneSingleton<ChatManager>.i.mutedPlayers.Add(player);
    return true;
  }

  private void MirageProcessed()
  {
  }

  private void UserCode_CmdSendChatMessage_\u002D456754112(
    string message,
    bool allChat,
    INetworkPlayer sender)
  {
    if (!this.ValidateChatMessageSize(message))
      return;
    Player player1;
    if (!sender.TryGetPlayer<Player>(out player1))
    {
      Debug.LogWarning((object) "CmdSendChatMessage called without a player, disconnecting player");
      sender.Disconnect();
    }
    else
    {
      if (!this.CheckRateLimit(player1, true, true))
        return;
      ColorLog<ChatManager>.Info($"CmdSendChatMessage allChat:{allChat} {sender} {player1.name} {message}");
      foreach (Player player2 in UnitRegistry.playerLookup.Values)
      {
        if (allChat || (UnityEngine.Object) player2.HQ == (UnityEngine.Object) player1.HQ)
          this.TargetReceiveMessage(player2.Owner, message, player1, allChat);
      }
    }
  }

  protected static void Skeleton_CmdSendChatMessage_\u002D456754112(
    NetworkBehaviour behaviour,
    NetworkReader reader,
    INetworkPlayer senderConnection,
    int replyId)
  {
    ((ChatManager) behaviour).UserCode_CmdSendChatMessage_\u002D456754112(reader.ReadString(), reader.ReadBooleanExtension(), senderConnection);
  }

  public void UserCode_TargetReceiveMessage_1307761090(
    INetworkPlayer _,
    string message,
    Player player,
    bool allChat)
  {
    if (!PlayerSettings.chatEnabled)
      return;
    if (ChatManager.IsMuted(player) || BlockList.IsBlocked(player))
    {
      Debug.Log((object) $"Ignoring message from muted player '{player}'");
    }
    else
    {
      string nameOrCensored = player.GetNameOrCensored();
      message = message.ProfanityFilter();
      string playerName = nameOrCensored.ProfanityFilter();
      Color textColor = this.GetTextColor(player.HQ, allChat);
      string str1 = $"{(allChat ? "" : "(ally)")}{playerName}:".AddColor(textColor);
      string str2 = message.SanitizeRichText(this.lengthLimit);
      SceneSingleton<GameplayUI>.i.GameMessage($"{str1} {str2}");
      if (!PlayerSettings.chatTts)
        return;
      this.RunTTS(playerName, message).Forget();
    }
  }

  protected static void Skeleton_TargetReceiveMessage_1307761090(
    NetworkBehaviour behaviour,
    NetworkReader reader,
    INetworkPlayer senderConnection,
    int replyId)
  {
    ((ChatManager) behaviour).UserCode_TargetReceiveMessage_1307761090(behaviour.Client.Player, reader.ReadString(), GeneratedNetworkCode._Read_NuclearOption\u002ENetworking\u002EPlayer(reader), reader.ReadBooleanExtension());
  }

  public void UserCode_RpcServerMessage_1244201393(string message, bool runTtsIfEnabled)
  {
    if (!PlayerSettings.chatEnabled)
      return;
    message = message.ProfanityFilter();
    ColorLog<ChatManager>.Info("RpcServerMessage " + message);
    SceneSingleton<GameplayUI>.i.GameMessage(message);
    if (!(PlayerSettings.chatTts & runTtsIfEnabled))
      return;
    this.RunTTS("server", message).Forget();
  }

  protected static void Skeleton_RpcServerMessage_1244201393(
    NetworkBehaviour behaviour,
    NetworkReader reader,
    INetworkPlayer senderConnection,
    int replyId)
  {
    ((ChatManager) behaviour).UserCode_RpcServerMessage_1244201393(reader.ReadString(), reader.ReadBooleanExtension());
  }

  public void UserCode_RpcTargetServerMessage_\u002D537879009(
    INetworkPlayer _,
    string message,
    bool runTtsIfEnabled)
  {
    if (!PlayerSettings.chatEnabled)
      return;
    message = message.ProfanityFilter();
    ColorLog<ChatManager>.Info("RpcTargetServerMessage " + message);
    SceneSingleton<GameplayUI>.i.GameMessage(message);
    if (!(PlayerSettings.chatTts & runTtsIfEnabled))
      return;
    this.RunTTS("server", message).Forget();
  }

  protected static void Skeleton_RpcTargetServerMessage_\u002D537879009(
    NetworkBehaviour behaviour,
    NetworkReader reader,
    INetworkPlayer senderConnection,
    int replyId)
  {
    ((ChatManager) behaviour).UserCode_RpcTargetServerMessage_\u002D537879009(behaviour.Client.Player, reader.ReadString(), reader.ReadBooleanExtension());
  }

  protected override int GetRpcCount() => 4;

  public override void RegisterRpc(RemoteCallCollection collection)
  {
    base.RegisterRpc(collection);
    collection.Register(0, "NuclearOption.Chat.ChatManager.CmdSendChatMessage", false, RpcInvokeType.ServerRpc, (NetworkBehaviour) this, new RpcDelegate(ChatManager.Skeleton_CmdSendChatMessage_\u002D456754112));
    collection.Register(1, "NuclearOption.Chat.ChatManager.TargetReceiveMessage", false, RpcInvokeType.ClientRpc, (NetworkBehaviour) this, new RpcDelegate(ChatManager.Skeleton_TargetReceiveMessage_1307761090));
    collection.Register(2, "NuclearOption.Chat.ChatManager.RpcServerMessage", false, RpcInvokeType.ClientRpc, (NetworkBehaviour) this, new RpcDelegate(ChatManager.Skeleton_RpcServerMessage_1244201393));
    collection.Register(3, "NuclearOption.Chat.ChatManager.RpcTargetServerMessage", false, RpcInvokeType.ClientRpc, (NetworkBehaviour) this, new RpcDelegate(ChatManager.Skeleton_RpcTargetServerMessage_\u002D537879009));
  }
}
