// Decompiled with JetBrains decompiler
// Type: qol.CommandProcessing.Core.CommandContext
// Assembly: qol, Version=1.1.6.3, Culture=neutral, PublicKeyToken=null
// MVID: 3B6539EA-E38C-45EE-8FFF-FC58CC42BADC
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\BepInEx\plugins\qol_1.1.6.3b1.dll

using BepInEx.Logging;

#nullable disable
namespace qol.CommandProcessing.Core;

public class CommandContext
{
  public ManualLogSource Logger { get; }

  public QOLPlugin Plugin { get; }

  public int LineNumber { get; set; }

  public string RawLine { get; set; }

  public CommandContext(QOLPlugin plugin, ManualLogSource logger)
  {
    this.Plugin = plugin;
    this.Logger = logger;
  }
}
