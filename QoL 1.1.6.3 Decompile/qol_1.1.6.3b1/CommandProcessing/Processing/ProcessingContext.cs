// Decompiled with JetBrains decompiler
// Type: qol.CommandProcessing.Processing.ProcessingContext
// Assembly: qol, Version=1.1.6.3, Culture=neutral, PublicKeyToken=null
// MVID: 3B6539EA-E38C-45EE-8FFF-FC58CC42BADC
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\BepInEx\plugins\qol_1.1.6.3b1.dll

using BepInEx.Logging;
using System;
using UnityEngine;

#nullable disable
namespace qol.CommandProcessing.Processing;

public class ProcessingContext
{
  public ManualLogSource Logger { get; }

  public bool NotDedicatedServer { get; }

  public bool MultiThreadingEnabled { get; }

  public string PluginGuid { get; }

  public Action<string> ProcessLine { get; }

  public Func<Font> GetBestFont { get; }

  public string PrimaryWhite { get; }

  public string PrimaryGreen { get; }

  public string SecondaryWhite { get; }

  public ProcessingContext(
    ManualLogSource logger,
    bool notDedicatedServer,
    bool multiThreadingEnabled,
    string pluginGuid,
    Action<string> processLine,
    Func<Font> getBestFont,
    string primaryWhite,
    string primaryGreen,
    string secondaryWhite)
  {
    this.Logger = logger;
    this.NotDedicatedServer = notDedicatedServer;
    this.MultiThreadingEnabled = multiThreadingEnabled;
    this.PluginGuid = pluginGuid;
    this.ProcessLine = processLine;
    this.GetBestFont = getBestFont;
    this.PrimaryWhite = primaryWhite;
    this.PrimaryGreen = primaryGreen;
    this.SecondaryWhite = secondaryWhite;
  }
}
