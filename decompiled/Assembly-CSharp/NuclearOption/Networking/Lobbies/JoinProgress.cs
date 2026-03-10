// Decompiled with JetBrains decompiler
// Type: NuclearOption.Networking.Lobbies.JoinProgress
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Mirage;

#nullable disable
namespace NuclearOption.Networking.Lobbies;

public struct JoinProgress
{
  public string Body;
  public bool Join;

  public static JoinProgress GetFailReason(ClientStoppedReason? disconnectReason)
  {
    if (disconnectReason.HasValue)
    {
      switch (disconnectReason.GetValueOrDefault())
      {
        case ClientStoppedReason.Timeout:
          return JoinProgress.Fail("Connection timeout with server");
        case ClientStoppedReason.LocalConnectionClosed:
        case ClientStoppedReason.ConnectingCancel:
          return JoinProgress.Fail("Local client stopped");
        case ClientStoppedReason.RemoteConnectionClosed:
        case ClientStoppedReason.InvalidPacket:
        case ClientStoppedReason.KeyInvalid:
        case ClientStoppedReason.SendBufferFull:
          return JoinProgress.Fail("Disconnected by server");
        case ClientStoppedReason.ServerFull:
          return JoinProgress.Fail("Server full");
        case ClientStoppedReason.ConnectingTimeout:
          return JoinProgress.Fail("Failed to connect to server");
      }
    }
    return JoinProgress.Fail("Unknown Reason");
  }

  public static JoinProgress Joining(string step)
  {
    return new JoinProgress() { Join = true, Body = step };
  }

  public static JoinProgress Fail(string reason)
  {
    return new JoinProgress()
    {
      Body = reason,
      Join = false
    };
  }
}
