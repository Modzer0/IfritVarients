// Decompiled with JetBrains decompiler
// Type: NuclearOption.DedicatedServer.Commands.CommandMessage
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;

#nullable disable
namespace NuclearOption.DedicatedServer.Commands;

[Serializable]
public struct CommandMessage
{
  public string name;
  public string[] arguments;
}
