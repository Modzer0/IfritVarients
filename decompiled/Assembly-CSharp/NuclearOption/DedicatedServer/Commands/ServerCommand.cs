// Decompiled with JetBrains decompiler
// Type: NuclearOption.DedicatedServer.Commands.ServerCommand
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

#nullable disable
namespace NuclearOption.DedicatedServer.Commands;

public class ServerCommand
{
  public readonly string Name;
  private readonly ServerCommand.CommandDelegate action;

  public ServerCommand(string name, ServerCommand.CommandDelegate action)
  {
    this.Name = name;
    this.action = action;
  }

  public CommandResponse Run(ServerRemoteCommands server, string[] arguments)
  {
    return this.action(server, arguments);
  }

  public delegate CommandResponse CommandDelegate(ServerRemoteCommands server, string[] arguments);
}
