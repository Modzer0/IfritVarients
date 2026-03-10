// Decompiled with JetBrains decompiler
// Type: qol.CommandProcessing.Core.CommandDispatcher
// Assembly: qol, Version=1.1.6.3, Culture=neutral, PublicKeyToken=null
// MVID: 3B6539EA-E38C-45EE-8FFF-FC58CC42BADC
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\BepInEx\plugins\qol_1.1.6.3b1.dll

using BepInEx.Logging;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

#nullable disable
namespace qol.CommandProcessing.Core;

public class CommandDispatcher
{
  private readonly List<ICommandHandler> _handlers;
  private readonly ManualLogSource _logger;

  public CommandDispatcher(ManualLogSource logger)
  {
    this._logger = logger;
    this._handlers = new List<ICommandHandler>();
  }

  public void RegisterHandler(ICommandHandler handler)
  {
    this._handlers.Add(handler);
    this._handlers.Sort((Comparison<ICommandHandler>) ((a, b) => b.Priority.CompareTo(a.Priority)));
  }

  public void RegisterHandlers(params ICommandHandler[] handlers)
  {
    foreach (ICommandHandler handler in handlers)
      this._handlers.Add(handler);
    this._handlers.Sort((Comparison<ICommandHandler>) ((a, b) => b.Priority.CompareTo(a.Priority)));
  }

  public bool ProcessLine(string line, CommandContext context)
  {
    context.RawLine = line;
    foreach (ICommandHandler handler in this._handlers)
    {
      Match match = handler.Pattern.Match(line);
      if (match.Success)
      {
        handler.Handle(match, context);
        return true;
      }
    }
    this._logger.LogWarning((object) ("No valid command pattern matched: " + line));
    return false;
  }

  public int HandlerCount => this._handlers.Count;
}
