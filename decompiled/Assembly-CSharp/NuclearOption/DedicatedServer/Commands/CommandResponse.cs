// Decompiled with JetBrains decompiler
// Type: NuclearOption.DedicatedServer.Commands.CommandResponse
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using System.Text;
using UnityEngine;

#nullable disable
namespace NuclearOption.DedicatedServer.Commands;

public struct CommandResponse
{
  public StatusCode StatusCode;
  public byte[] Body;

  public void SetBody<T>(T body)
  {
    this.Body = Encoding.UTF8.GetBytes(JsonUtility.ToJson((object) body));
  }

  public static CommandResponse Create(StatusCode statusCode)
  {
    return new CommandResponse() { StatusCode = statusCode };
  }

  public static CommandResponse Create(StatusCode statusCode, string message)
  {
    return CommandResponse.Create<CommandResponse.MessageResponse>(statusCode, new CommandResponse.MessageResponse()
    {
      message = message
    });
  }

  public static CommandResponse Create<T>(StatusCode statusCode, T body) where T : struct
  {
    CommandResponse commandResponse = new CommandResponse();
    commandResponse.StatusCode = statusCode;
    commandResponse.SetBody<T>(body);
    return commandResponse;
  }

  [Serializable]
  public struct MessageResponse
  {
    public string message;
  }
}
