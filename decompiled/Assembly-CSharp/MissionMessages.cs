// Decompiled with JetBrains decompiler
// Type: MissionMessages
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Mirage;
using Mirage.RemoteCalls;
using Mirage.Serialization;
using System;
using UnityEngine;

#nullable disable
public class MissionMessages : NetworkSceneSingleton<MissionMessages>
{
  private int lastPlayedFrame;
  private DialogueBox _dialogueBox;
  [NonSerialized]
  private const int SYNC_VAR_COUNT = 0;
  [NonSerialized]
  private const int RPC_COUNT = 4;

  private DialogueBox DialogueBox
  {
    get
    {
      if ((UnityEngine.Object) this._dialogueBox == (UnityEngine.Object) null)
      {
        this._dialogueBox = SceneSingleton<GameplayUI>.i.DialogueBox;
        this._dialogueBox.ButtonPressed += new Action<string>(this.DialogueBox_ButtonPressed);
      }
      return this._dialogueBox;
    }
  }

  public static void ShowMessage(
    string message,
    bool playsound,
    FactionHQ faction,
    bool sendToClients)
  {
    if (string.IsNullOrEmpty(message))
      return;
    NetworkSceneSingleton<MissionMessages>.i.ShowMessgeLocal(message, playsound, faction);
    if (!sendToClients)
      return;
    NetworkSceneSingleton<MissionMessages>.i.RpcShowMessage(message, playsound, faction);
  }

  private void ShowMessgeLocal(string message, bool playsound, FactionHQ filterFaction)
  {
    if (!MissionMessages.IsLocalFaction(filterFaction))
      return;
    SceneSingleton<GameplayUI>.i.GameMessage(message);
    if (!playsound)
      return;
    NetworkSceneSingleton<MissionMessages>.i.PlaySound();
  }

  [ClientRpc]
  private void RpcShowMessage(string message, bool playsound, FactionHQ faction)
  {
    if (ClientRpcSender.ShouldInvokeLocally((NetworkBehaviour) this, RpcTarget.Observers, (INetworkPlayer) null, false))
      this.UserCode_RpcShowMessage_\u002D186615428(message, playsound, faction);
    PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
    writer.WriteString(message);
    writer.WriteBooleanExtension(playsound);
    GeneratedNetworkCode._Write_FactionHQ((NetworkWriter) writer, faction);
    ClientRpcSender.Send((NetworkBehaviour) this, 0, (NetworkWriter) writer, Channel.Reliable, false);
    writer.Release();
  }

  private void PlaySound()
  {
    int frameCount = Time.frameCount;
    if (frameCount == this.lastPlayedFrame)
      return;
    this.lastPlayedFrame = frameCount;
    SoundManager.PlayInterfaceOneShot(GameAssets.i.radioStatic);
  }

  private static bool IsLocalFaction(FactionHQ filterFaction)
  {
    if ((UnityEngine.Object) filterFaction == (UnityEngine.Object) null)
      return true;
    FactionHQ localHq;
    return GameManager.GetLocalHQ(out localHq) && (UnityEngine.Object) filterFaction == (UnityEngine.Object) localHq;
  }

  public static DialogueBox ShowDialogue(
    string title,
    string body,
    string button,
    FactionHQ filterFaction)
  {
    if (MissionMessages.IsLocalFaction(filterFaction))
      NetworkSceneSingleton<MissionMessages>.i.DialogueBox.Show(title, body, button);
    else
      NetworkSceneSingleton<MissionMessages>.i.DialogueBox.SetServerTitle(title);
    NetworkSceneSingleton<MissionMessages>.i.RpcShowDialogue(title, body, button, filterFaction);
    return NetworkSceneSingleton<MissionMessages>.i.DialogueBox;
  }

  public static void HideDialogue()
  {
    NetworkSceneSingleton<MissionMessages>.i.DialogueBox.Hide();
    NetworkSceneSingleton<MissionMessages>.i.RpcHideDialogue();
  }

  [ClientRpc]
  private void RpcShowDialogue(string title, string body, string button, FactionHQ filterFaction)
  {
    if (ClientRpcSender.ShouldInvokeLocally((NetworkBehaviour) this, RpcTarget.Observers, (INetworkPlayer) null, false))
      this.UserCode_RpcShowDialogue_1792200634(title, body, button, filterFaction);
    PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
    writer.WriteString(title);
    writer.WriteString(body);
    writer.WriteString(button);
    GeneratedNetworkCode._Write_FactionHQ((NetworkWriter) writer, filterFaction);
    ClientRpcSender.Send((NetworkBehaviour) this, 1, (NetworkWriter) writer, Channel.Reliable, false);
    writer.Release();
  }

  [ClientRpc]
  private void RpcHideDialogue()
  {
    if (ClientRpcSender.ShouldInvokeLocally((NetworkBehaviour) this, RpcTarget.Observers, (INetworkPlayer) null, false))
      this.UserCode_RpcHideDialogue_1784621292();
    PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
    ClientRpcSender.Send((NetworkBehaviour) this, 2, (NetworkWriter) writer, Channel.Reliable, false);
    writer.Release();
  }

  private void DialogueBox_ButtonPressed(string title)
  {
    if (this.IsServer)
      return;
    this.CmdDialogueButton(title);
  }

  [ServerRpc(requireAuthority = false)]
  private void CmdDialogueButton(string title)
  {
    if (ServerRpcSender.ShouldInvokeLocally((NetworkBehaviour) this, false, false))
    {
      this.UserCode_CmdDialogueButton_\u002D1450673487(title);
    }
    else
    {
      PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
      writer.WriteString(title);
      ServerRpcSender.Send((NetworkBehaviour) this, 3, (NetworkWriter) writer, Channel.Reliable, false);
      writer.Release();
    }
  }

  private void MirageProcessed()
  {
  }

  private void UserCode_RpcShowMessage_\u002D186615428(
    string message,
    bool playsound,
    FactionHQ faction)
  {
    if (this.IsServer)
      return;
    this.ShowMessgeLocal(message, playsound, faction);
  }

  protected static void Skeleton_RpcShowMessage_\u002D186615428(
    NetworkBehaviour behaviour,
    NetworkReader reader,
    INetworkPlayer senderConnection,
    int replyId)
  {
    ((MissionMessages) behaviour).UserCode_RpcShowMessage_\u002D186615428(reader.ReadString(), reader.ReadBooleanExtension(), GeneratedNetworkCode._Read_FactionHQ(reader));
  }

  private void UserCode_RpcShowDialogue_1792200634(
    string title,
    string body,
    string button,
    FactionHQ filterFaction)
  {
    if (this.IsServer || !MissionMessages.IsLocalFaction(filterFaction))
      return;
    this.DialogueBox.Show(title, body, button);
  }

  protected static void Skeleton_RpcShowDialogue_1792200634(
    NetworkBehaviour behaviour,
    NetworkReader reader,
    INetworkPlayer senderConnection,
    int replyId)
  {
    ((MissionMessages) behaviour).UserCode_RpcShowDialogue_1792200634(reader.ReadString(), reader.ReadString(), reader.ReadString(), GeneratedNetworkCode._Read_FactionHQ(reader));
  }

  private void UserCode_RpcHideDialogue_1784621292()
  {
    if (this.IsServer)
      return;
    this.DialogueBox.Hide();
  }

  protected static void Skeleton_RpcHideDialogue_1784621292(
    NetworkBehaviour behaviour,
    NetworkReader reader,
    INetworkPlayer senderConnection,
    int replyId)
  {
    ((MissionMessages) behaviour).UserCode_RpcHideDialogue_1784621292();
  }

  private void UserCode_CmdDialogueButton_\u002D1450673487(string title)
  {
    if (!(this.DialogueBox.Title == title))
      return;
    this.DialogueBox.InvokeButtonPress();
  }

  protected static void Skeleton_CmdDialogueButton_\u002D1450673487(
    NetworkBehaviour behaviour,
    NetworkReader reader,
    INetworkPlayer senderConnection,
    int replyId)
  {
    ((MissionMessages) behaviour).UserCode_CmdDialogueButton_\u002D1450673487(reader.ReadString());
  }

  protected override int GetRpcCount() => 4;

  public override void RegisterRpc(RemoteCallCollection collection)
  {
    base.RegisterRpc(collection);
    collection.Register(0, "MissionMessages.RpcShowMessage", false, RpcInvokeType.ClientRpc, (NetworkBehaviour) this, new RpcDelegate(MissionMessages.Skeleton_RpcShowMessage_\u002D186615428));
    collection.Register(1, "MissionMessages.RpcShowDialogue", false, RpcInvokeType.ClientRpc, (NetworkBehaviour) this, new RpcDelegate(MissionMessages.Skeleton_RpcShowDialogue_1792200634));
    collection.Register(2, "MissionMessages.RpcHideDialogue", false, RpcInvokeType.ClientRpc, (NetworkBehaviour) this, new RpcDelegate(MissionMessages.Skeleton_RpcHideDialogue_1784621292));
    collection.Register(3, "MissionMessages.CmdDialogueButton", false, RpcInvokeType.ServerRpc, (NetworkBehaviour) this, new RpcDelegate(MissionMessages.Skeleton_CmdDialogueButton_\u002D1450673487));
  }
}
