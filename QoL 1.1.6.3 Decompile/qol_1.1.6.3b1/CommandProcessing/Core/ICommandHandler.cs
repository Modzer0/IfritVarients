// Decompiled with JetBrains decompiler
// Type: qol.CommandProcessing.Core.ICommandHandler
// Assembly: qol, Version=1.1.6.3, Culture=neutral, PublicKeyToken=null
// MVID: 3B6539EA-E38C-45EE-8FFF-FC58CC42BADC
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\BepInEx\plugins\qol_1.1.6.3b1.dll

using System.Text.RegularExpressions;

#nullable disable
namespace qol.CommandProcessing.Core;

public interface ICommandHandler
{
  Regex Pattern { get; }

  int Priority { get; }

  void Handle(Match match, CommandContext context);
}
