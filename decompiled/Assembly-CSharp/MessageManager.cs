// Decompiled with JetBrains decompiler
// Type: MessageManager
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Mirage;
using Mirage.RemoteCalls;
using Mirage.Serialization;
using NuclearOption.Networking;
using System;
using UnityEngine;

#nullable disable
public class MessageManager : NetworkSceneSingleton<MessageManager>
{
  [NonSerialized]
  private const int SYNC_VAR_COUNT = 0;
  [NonSerialized]
  private const int RPC_COUNT = 9;

  private static Color GetSpawnMessageColor(
    bool localAircraft,
    FactionHQ aircraftHQ,
    FactionHQ localHq)
  {
    return !localAircraft ? (!((UnityEngine.Object) localHq == (UnityEngine.Object) null) ? (!((UnityEngine.Object) localHq == (UnityEngine.Object) aircraftHQ) ? GameAssets.i.HUDHostile : GameAssets.i.HUDFriendly) : aircraftHQ.faction.color) : GameAssets.i.HUDFriendly;
  }

  public void JoinMessage(Player joinedPlayer)
  {
    string message = $"<color=#00ff00ff>{joinedPlayer.GetNameOrCensored()} joined the game </color>";
    SceneSingleton<GameplayUI>.i.GameMessage(message);
  }

  public void DisconnectedMessage(Player player)
  {
    SceneSingleton<GameplayUI>.i.GameMessage(player.GetNameOrCensored() + " Disconnected");
  }

  [ClientRpc]
  public void RpcPlayerJoinFactionMessage(Player player, FactionHQ hq)
  {
    if (ClientRpcSender.ShouldInvokeLocally((NetworkBehaviour) this, RpcTarget.Observers, (INetworkPlayer) null, false))
      this.UserCode_RpcPlayerJoinFactionMessage_156835807(player, hq);
    PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
    GeneratedNetworkCode._Write_NuclearOption\u002ENetworking\u002EPlayer((NetworkWriter) writer, player);
    GeneratedNetworkCode._Write_FactionHQ((NetworkWriter) writer, hq);
    ClientRpcSender.Send((NetworkBehaviour) this, 0, (NetworkWriter) writer, Channel.Reliable, false);
    writer.Release();
  }

  [ClientRpc(target = RpcTarget.Player)]
  public void TargetCreditMessage(
    INetworkPlayer _,
    PersistentID killedID,
    float creditAwarded,
    FactionHQ.RewardType actionType)
  {
    if (ClientRpcSender.ShouldInvokeLocally((NetworkBehaviour) this, RpcTarget.Player, _, false))
    {
      this.UserCode_TargetCreditMessage_106951341(this.Client.Player, killedID, creditAwarded, actionType);
    }
    else
    {
      PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
      GeneratedNetworkCode._Write_PersistentID((NetworkWriter) writer, killedID);
      writer.WriteSingleConverter(creditAwarded);
      GeneratedNetworkCode._Write_FactionHQ\u002FRewardType((NetworkWriter) writer, actionType);
      ClientRpcSender.SendTarget((NetworkBehaviour) this, 1, (NetworkWriter) writer, Channel.Reliable, _);
      writer.Release();
    }
  }

  [ClientRpc]
  public void RpcBombFailMessage(PersistentID bombID, float gForce)
  {
    if (ClientRpcSender.ShouldInvokeLocally((NetworkBehaviour) this, RpcTarget.Observers, (INetworkPlayer) null, false))
      this.UserCode_RpcBombFailMessage_\u002D1758898816(bombID, gForce);
    PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
    GeneratedNetworkCode._Write_PersistentID((NetworkWriter) writer, bombID);
    writer.WriteSingleConverter(gForce);
    ClientRpcSender.Send((NetworkBehaviour) this, 2, (NetworkWriter) writer, Channel.Reliable, false);
    writer.Release();
  }

  [ClientRpc]
  public void RpcKillMessage(PersistentID killerID, PersistentID killedID, KillType killedType)
  {
    if (ClientRpcSender.ShouldInvokeLocally((NetworkBehaviour) this, RpcTarget.Observers, (INetworkPlayer) null, false))
      this.UserCode_RpcKillMessage_635947223(killerID, killedID, killedType);
    PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
    GeneratedNetworkCode._Write_PersistentID((NetworkWriter) writer, killerID);
    GeneratedNetworkCode._Write_PersistentID((NetworkWriter) writer, killedID);
    GeneratedNetworkCode._Write_KillType((NetworkWriter) writer, killedType);
    ClientRpcSender.Send((NetworkBehaviour) this, 3, (NetworkWriter) writer, Channel.Reliable, false);
    writer.Release();
  }

  private static bool KillFeedFilter(
    KillType killedType,
    PersistentUnit attacker,
    PersistentUnit killed)
  {
    if ((double) killed.unit.definition.value < (double) PlayerSettings.killFeedMinValue)
      return false;
    switch (killedType.GetFilterLevel())
    {
      case PlayerSettings.KillFeedFilter.None:
        return false;
      case PlayerSettings.KillFeedFilter.Player:
        return IsLocalPlayer(attacker) || IsLocalPlayer(killed);
      case PlayerSettings.KillFeedFilter.Friendly:
        return IsFriendlyFaction(attacker) || IsFriendlyFaction(killed);
      case PlayerSettings.KillFeedFilter.Enemy:
        return IsEnemyFaction(attacker) || IsEnemyFaction(killed);
      default:
        return true;
    }

    static bool IsLocalPlayer(PersistentUnit unit)
    {
      return unit != null && GameManager.IsLocalPlayer<Player>(unit.player);
    }

    static bool IsFriendlyFaction(PersistentUnit unit)
    {
      return unit != null && GameManager.IsLocalHQ(unit.GetHQ());
    }

    static bool IsEnemyFaction(PersistentUnit unit)
    {
      FactionHQ hq;
      FactionHQ localHq;
      return unit != null && unit.HasHQ(out hq) && GameManager.GetLocalHQ(out localHq) && (UnityEngine.Object) hq != (UnityEngine.Object) localHq;
    }
  }

  [ClientRpc]
  public void RpcAllHQMessage(string message)
  {
    if (ClientRpcSender.ShouldInvokeLocally((NetworkBehaviour) this, RpcTarget.Observers, (INetworkPlayer) null, false))
      this.UserCode_RpcAllHQMessage_913698537(message);
    PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
    writer.WriteString(message);
    ClientRpcSender.Send((NetworkBehaviour) this, 4, (NetworkWriter) writer, Channel.Reliable, false);
    writer.Release();
  }

  [ClientRpc]
  public void RpcHQMessage(FactionHQ HQ, string message)
  {
    if (ClientRpcSender.ShouldInvokeLocally((NetworkBehaviour) this, RpcTarget.Observers, (INetworkPlayer) null, false))
      this.UserCode_RpcHQMessage_324162117(HQ, message);
    PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
    GeneratedNetworkCode._Write_FactionHQ((NetworkWriter) writer, HQ);
    writer.WriteString(message);
    ClientRpcSender.Send((NetworkBehaviour) this, 5, (NetworkWriter) writer, Channel.Reliable, false);
    writer.Release();
  }

  private void HQMessageInternal(FactionHQ HQ, string message)
  {
    SoundManager.PlayInterfaceOneShot(GameAssets.i.radioStatic);
    SceneSingleton<GameplayUI>.i.GameMessage(message, 0.5f);
    if (message.Contains("Tactical"))
      MusicManager.i.QueueMusicClip(NetworkSceneSingleton<LevelInfo>.i.LoadedMapSettings.GetTacticalMusic(HQ.faction), 2f);
    if (!message.Contains("Strategic"))
      return;
    MusicManager.i.QueueMusicClip(NetworkSceneSingleton<LevelInfo>.i.LoadedMapSettings.GetStrategicMusic(HQ.faction), 2f);
  }

  [ClientRpc]
  public void RpcPilotCaptureMessage(PersistentID id, bool rescued)
  {
    if (ClientRpcSender.ShouldInvokeLocally((NetworkBehaviour) this, RpcTarget.Observers, (INetworkPlayer) null, false))
      this.UserCode_RpcPilotCaptureMessage_1175243856(id, rescued);
    PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
    GeneratedNetworkCode._Write_PersistentID((NetworkWriter) writer, id);
    writer.WriteBooleanExtension(rescued);
    ClientRpcSender.Send((NetworkBehaviour) this, 6, (NetworkWriter) writer, Channel.Reliable, false);
    writer.Release();
  }

  [ClientRpc]
  public void RpcRepairMessage(PersistentID id)
  {
    if (ClientRpcSender.ShouldInvokeLocally((NetworkBehaviour) this, RpcTarget.Observers, (INetworkPlayer) null, false))
      this.UserCode_RpcRepairMessage_1444052622(id);
    PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
    GeneratedNetworkCode._Write_PersistentID((NetworkWriter) writer, id);
    ClientRpcSender.Send((NetworkBehaviour) this, 7, (NetworkWriter) writer, Channel.Reliable, false);
    writer.Release();
  }

  [ClientRpc]
  public void RpcWarheadDestroyedMessage(Airbase airbase, FactionHQ hq, int number)
  {
    if (ClientRpcSender.ShouldInvokeLocally((NetworkBehaviour) this, RpcTarget.Observers, (INetworkPlayer) null, false))
      this.UserCode_RpcWarheadDestroyedMessage_\u002D662431755(airbase, hq, number);
    PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
    GeneratedNetworkCode._Write_Airbase((NetworkWriter) writer, airbase);
    GeneratedNetworkCode._Write_FactionHQ((NetworkWriter) writer, hq);
    writer.WritePackedInt32(number);
    ClientRpcSender.Send((NetworkBehaviour) this, 8, (NetworkWriter) writer, Channel.Reliable, false);
    writer.Release();
  }

  private static Color ColorFromFaction(FactionHQ hq)
  {
    if ((UnityEngine.Object) hq == (UnityEngine.Object) null)
      return GameAssets.i.HUDNeutral;
    FactionHQ localHq;
    if (!GameManager.GetLocalHQ(out localHq))
      return hq.faction.color;
    return !((UnityEngine.Object) localHq == (UnityEngine.Object) hq) ? GameAssets.i.HUDHostile : GameAssets.i.HUDFriendly;
  }

  private void MirageProcessed()
  {
  }

  public void UserCode_RpcPlayerJoinFactionMessage_156835807(Player player, FactionHQ hq)
  {
    if (GameManager.IsLocalPlayer<Player>(player))
      return;
    Color color = MessageManager.ColorFromFaction(hq);
    string message = $"{((UnityEngine.Object) player != (UnityEngine.Object) null ? player.GetNameOrCensored() : "").AddColor(color)} joined {hq.faction.factionExtendedName.AddColor(color)}";
    SceneSingleton<GameplayUI>.i.GameMessage(message);
  }

  protected static void Skeleton_RpcPlayerJoinFactionMessage_156835807(
    NetworkBehaviour behaviour,
    NetworkReader reader,
    INetworkPlayer senderConnection,
    int replyId)
  {
    ((MessageManager) behaviour).UserCode_RpcPlayerJoinFactionMessage_156835807(GeneratedNetworkCode._Read_NuclearOption\u002ENetworking\u002EPlayer(reader), GeneratedNetworkCode._Read_FactionHQ(reader));
  }

  public void UserCode_TargetCreditMessage_106951341(
    INetworkPlayer _,
    PersistentID killedID,
    float creditAwarded,
    FactionHQ.RewardType actionType)
  {
    if (PlayerSettings.cinematicMode)
      return;
    PersistentUnit persistentUnit = (PersistentUnit) null;
    if (killedID.IsValid)
      UnitRegistry.TryGetPersistentUnit(killedID, out persistentUnit);
    if ((UnityEngine.Object) SceneSingleton<KillDisplay>.i == (UnityEngine.Object) null)
      UnityEngine.Object.Instantiate<GameObject>(GameAssets.i.killDisplay, SceneSingleton<GameplayUI>.i.gameplayCanvas.transform);
    SceneSingleton<KillDisplay>.i.DisplayKill(persistentUnit, creditAwarded, actionType);
  }

  protected static void Skeleton_TargetCreditMessage_106951341(
    NetworkBehaviour behaviour,
    NetworkReader reader,
    INetworkPlayer senderConnection,
    int replyId)
  {
    ((MessageManager) behaviour).UserCode_TargetCreditMessage_106951341(behaviour.Client.Player, GeneratedNetworkCode._Read_PersistentID(reader), reader.ReadSingleConverter(), GeneratedNetworkCode._Read_FactionHQ\u002FRewardType(reader));
  }

  public void UserCode_RpcBombFailMessage_\u002D1758898816(PersistentID bombID, float gForce)
  {
    PersistentUnit persistentUnit;
    if (!UnitRegistry.TryGetPersistentUnit(bombID, out persistentUnit))
      return;
    Color color = MessageManager.ColorFromFaction(persistentUnit.GetHQ());
    SceneSingleton<GameplayUI>.i.GameMessage($"{persistentUnit.unitName.AddColor(color)} failed on {gForce:F0}g impact");
  }

  protected static void Skeleton_RpcBombFailMessage_\u002D1758898816(
    NetworkBehaviour behaviour,
    NetworkReader reader,
    INetworkPlayer senderConnection,
    int replyId)
  {
    ((MessageManager) behaviour).UserCode_RpcBombFailMessage_\u002D1758898816(GeneratedNetworkCode._Read_PersistentID(reader), reader.ReadSingleConverter());
  }

  public void UserCode_RpcKillMessage_635947223(
    PersistentID killerID,
    PersistentID killedID,
    KillType killedType)
  {
    PersistentUnit persistentUnit1;
    if (!UnitRegistry.TryGetPersistentUnit(killedID, out persistentUnit1))
      return;
    PersistentUnit persistentUnit2;
    bool persistentUnit3 = UnitRegistry.TryGetPersistentUnit(killerID, out persistentUnit2);
    if (!MessageManager.KillFeedFilter(killedType, persistentUnit2, persistentUnit1))
      return;
    Color color1 = MessageManager.ColorFromFaction(persistentUnit1.GetHQ());
    string verb = killedType.GetVerb(persistentUnit3);
    if (!persistentUnit3)
    {
      string message = $"{persistentUnit1.unitName.AddColor(color1)} {verb}";
      SceneSingleton<GameplayUI>.i.KillFeed(message);
    }
    else
    {
      Color color2 = MessageManager.ColorFromFaction(persistentUnit2.GetHQ());
      string message = $"{persistentUnit2.unitName.AddColor(color2)} {verb} {persistentUnit1.unitName.AddColor(color1)}";
      SceneSingleton<GameplayUI>.i.KillFeed(message);
    }
  }

  protected static void Skeleton_RpcKillMessage_635947223(
    NetworkBehaviour behaviour,
    NetworkReader reader,
    INetworkPlayer senderConnection,
    int replyId)
  {
    ((MessageManager) behaviour).UserCode_RpcKillMessage_635947223(GeneratedNetworkCode._Read_PersistentID(reader), GeneratedNetworkCode._Read_PersistentID(reader), GeneratedNetworkCode._Read_KillType(reader));
  }

  public void UserCode_RpcAllHQMessage_913698537(string message)
  {
    FactionHQ localHq;
    if (!GameManager.GetLocalHQ(out localHq))
      return;
    this.HQMessageInternal(localHq, message);
  }

  protected static void Skeleton_RpcAllHQMessage_913698537(
    NetworkBehaviour behaviour,
    NetworkReader reader,
    INetworkPlayer senderConnection,
    int replyId)
  {
    ((MessageManager) behaviour).UserCode_RpcAllHQMessage_913698537(reader.ReadString());
  }

  public void UserCode_RpcHQMessage_324162117(FactionHQ HQ, string message)
  {
    if (!GameManager.IsLocalHQ(HQ))
      return;
    this.HQMessageInternal(HQ, message);
  }

  protected static void Skeleton_RpcHQMessage_324162117(
    NetworkBehaviour behaviour,
    NetworkReader reader,
    INetworkPlayer senderConnection,
    int replyId)
  {
    ((MessageManager) behaviour).UserCode_RpcHQMessage_324162117(GeneratedNetworkCode._Read_FactionHQ(reader), reader.ReadString());
  }

  public void UserCode_RpcPilotCaptureMessage_1175243856(PersistentID id, bool rescued)
  {
    PersistentUnit persistentUnit;
    if (!UnitRegistry.TryGetPersistentUnit(id, out persistentUnit))
      return;
    Color color = MessageManager.ColorFromFaction(persistentUnit.GetHQ());
    string str = rescued ? nameof (rescued) : "captured";
    string message = $"{persistentUnit.unitName.AddColor(color)} was {str}";
    SceneSingleton<GameplayUI>.i.GameMessage(message);
  }

  protected static void Skeleton_RpcPilotCaptureMessage_1175243856(
    NetworkBehaviour behaviour,
    NetworkReader reader,
    INetworkPlayer senderConnection,
    int replyId)
  {
    ((MessageManager) behaviour).UserCode_RpcPilotCaptureMessage_1175243856(GeneratedNetworkCode._Read_PersistentID(reader), reader.ReadBooleanExtension());
  }

  public void UserCode_RpcRepairMessage_1444052622(PersistentID id)
  {
    PersistentUnit persistentUnit;
    if (!UnitRegistry.TryGetPersistentUnit(id, out persistentUnit))
      return;
    Color color = MessageManager.ColorFromFaction(persistentUnit.GetHQ());
    string message = persistentUnit.unitName.AddColor(color) + " was repaired";
    SceneSingleton<GameplayUI>.i.GameMessage(message);
  }

  protected static void Skeleton_RpcRepairMessage_1444052622(
    NetworkBehaviour behaviour,
    NetworkReader reader,
    INetworkPlayer senderConnection,
    int replyId)
  {
    ((MessageManager) behaviour).UserCode_RpcRepairMessage_1444052622(GeneratedNetworkCode._Read_PersistentID(reader));
  }

  public void UserCode_RpcWarheadDestroyedMessage_\u002D662431755(
    Airbase airbase,
    FactionHQ hq,
    int number)
  {
    if ((UnityEngine.Object) airbase == (UnityEngine.Object) null || !((UnityEngine.Object) hq != (UnityEngine.Object) null) || !GameManager.IsLocalHQ(hq))
      return;
    Color color = MessageManager.ColorFromFaction(hq);
    string str = number > 1 ? "warheads" : "warhead";
    string message = $"Warning : {number} {str} destroyed at {airbase.SavedAirbase.DisplayName.AddColor(color)}";
    this.HQMessageInternal(hq, message);
  }

  protected static void Skeleton_RpcWarheadDestroyedMessage_\u002D662431755(
    NetworkBehaviour behaviour,
    NetworkReader reader,
    INetworkPlayer senderConnection,
    int replyId)
  {
    ((MessageManager) behaviour).UserCode_RpcWarheadDestroyedMessage_\u002D662431755(GeneratedNetworkCode._Read_Airbase(reader), GeneratedNetworkCode._Read_FactionHQ(reader), reader.ReadPackedInt32());
  }

  protected override int GetRpcCount() => 9;

  public override void RegisterRpc(RemoteCallCollection collection)
  {
    base.RegisterRpc(collection);
    collection.Register(0, "MessageManager.RpcPlayerJoinFactionMessage", false, RpcInvokeType.ClientRpc, (NetworkBehaviour) this, new RpcDelegate(MessageManager.Skeleton_RpcPlayerJoinFactionMessage_156835807));
    collection.Register(1, "MessageManager.TargetCreditMessage", false, RpcInvokeType.ClientRpc, (NetworkBehaviour) this, new RpcDelegate(MessageManager.Skeleton_TargetCreditMessage_106951341));
    collection.Register(2, "MessageManager.RpcBombFailMessage", false, RpcInvokeType.ClientRpc, (NetworkBehaviour) this, new RpcDelegate(MessageManager.Skeleton_RpcBombFailMessage_\u002D1758898816));
    collection.Register(3, "MessageManager.RpcKillMessage", false, RpcInvokeType.ClientRpc, (NetworkBehaviour) this, new RpcDelegate(MessageManager.Skeleton_RpcKillMessage_635947223));
    collection.Register(4, "MessageManager.RpcAllHQMessage", false, RpcInvokeType.ClientRpc, (NetworkBehaviour) this, new RpcDelegate(MessageManager.Skeleton_RpcAllHQMessage_913698537));
    collection.Register(5, "MessageManager.RpcHQMessage", false, RpcInvokeType.ClientRpc, (NetworkBehaviour) this, new RpcDelegate(MessageManager.Skeleton_RpcHQMessage_324162117));
    collection.Register(6, "MessageManager.RpcPilotCaptureMessage", false, RpcInvokeType.ClientRpc, (NetworkBehaviour) this, new RpcDelegate(MessageManager.Skeleton_RpcPilotCaptureMessage_1175243856));
    collection.Register(7, "MessageManager.RpcRepairMessage", false, RpcInvokeType.ClientRpc, (NetworkBehaviour) this, new RpcDelegate(MessageManager.Skeleton_RpcRepairMessage_1444052622));
    collection.Register(8, "MessageManager.RpcWarheadDestroyedMessage", false, RpcInvokeType.ClientRpc, (NetworkBehaviour) this, new RpcDelegate(MessageManager.Skeleton_RpcWarheadDestroyedMessage_\u002D662431755));
  }
}
