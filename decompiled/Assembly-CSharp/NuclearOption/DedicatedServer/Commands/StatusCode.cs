// Decompiled with JetBrains decompiler
// Type: NuclearOption.DedicatedServer.Commands.StatusCode
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

#nullable disable
namespace NuclearOption.DedicatedServer.Commands;

public enum StatusCode
{
  Success = 2000, // 0x000007D0
  BadRequest = 4000, // 0x00000FA0
  BadHeader = 4001, // 0x00000FA1
  BadLength = 4002, // 0x00000FA2
  JsonError = 4003, // 0x00000FA3
  UnknownCommand = 4004, // 0x00000FA4
  BadArguments = 4005, // 0x00000FA5
  InternalServerError = 5000, // 0x00001388
  CommandError = 5001, // 0x00001389
  ConfigError = 5002, // 0x0000138A
}
