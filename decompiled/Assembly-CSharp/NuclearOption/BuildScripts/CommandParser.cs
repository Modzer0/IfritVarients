// Decompiled with JetBrains decompiler
// Type: NuclearOption.BuildScripts.CommandParser
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

#nullable disable
namespace NuclearOption.BuildScripts;

public static class CommandParser
{
  public static void Parse(List<string> stringArgs, List<CommandParser.ArgCommand> argCommands)
  {
    Debug.Log((object) $"Command Line Args: {stringArgs.Count}");
    foreach (string stringArg in stringArgs)
      Debug.Log((object) ("  " + stringArg));
    for (int index = 0; index < stringArgs.Count; ++index)
    {
      string stringArg = stringArgs[index];
      if (stringArg.StartsWith("-"))
      {
        string lower = stringArg.ToLower();
        foreach (CommandParser.ArgCommand argCommand in argCommands)
        {
          if (lower == argCommand.Start)
          {
            try
            {
              Debug.Log((object) ("Found command:" + argCommand.Start));
              argCommand.Handler(new CommandParser.CommandArguments(index, stringArgs));
            }
            catch (Exception ex)
            {
              Debug.LogError((object) $"Command failed with error: {ex}");
            }
          }
        }
      }
    }
  }

  public delegate void HandleArgDelegate(CommandParser.CommandArguments arguments);

  public delegate UniTask HandleArgDelegateCoroutine(CommandParser.CommandArguments arguments);

  public struct ArgCommand
  {
    public string Start;
    public CommandParser.HandleArgDelegate Handler;

    public ArgCommand(string start, CommandParser.HandleArgDelegateCoroutine handler)
    {
      this.Start = (start ?? throw new ArgumentNullException(nameof (start))).ToLower();
      this.Handler = (CommandParser.HandleArgDelegate) (a => handler(a).Forget());
    }

    public ArgCommand(string start, CommandParser.HandleArgDelegate handler)
    {
      this.Start = (start ?? throw new ArgumentNullException(nameof (start))).ToLower();
      this.Handler = handler ?? throw new ArgumentNullException(nameof (handler));
    }
  }

  public struct CommandArguments(int start, List<string> allArgs)
  {
    public readonly int start = start;
    public readonly List<string> AllArgs = allArgs;

    public string GetNext(int offset)
    {
      int index = this.start + offset;
      if (index < 0 || index >= this.AllArgs.Count)
        throw new ArgumentOutOfRangeException(nameof (offset), "Argument offset is out of bounds.");
      string allArg = this.AllArgs[index];
      return !allArg.StartsWith("-") ? allArg : throw new InvalidOperationException($"Expected a value but next argument was a command. Value = '{allArg}'");
    }

    public bool TryGetNext(int offset, out string value)
    {
      int index = this.start + offset;
      if (index < 0 || index >= this.AllArgs.Count)
      {
        value = (string) null;
        return false;
      }
      string allArg = this.AllArgs[index];
      if (allArg.StartsWith("-"))
      {
        value = (string) null;
        return false;
      }
      value = allArg;
      return true;
    }

    public int GetNextInt(int offset)
    {
      return int.Parse(this.GetNext(offset), (IFormatProvider) CultureInfo.InvariantCulture);
    }

    public bool TryGetNextInt(int offset, out int value)
    {
      string s;
      if (this.TryGetNext(offset, out s))
        return int.TryParse(s, NumberStyles.Integer, (IFormatProvider) CultureInfo.InvariantCulture, out value);
      value = 0;
      return false;
    }

    public ulong GetNextULong(int offset)
    {
      return ulong.Parse(this.GetNext(offset), (IFormatProvider) CultureInfo.InvariantCulture);
    }

    public bool TryGetNextULong(int offset, out ulong value)
    {
      string s;
      if (this.TryGetNext(offset, out s))
        return ulong.TryParse(s, NumberStyles.Any, (IFormatProvider) CultureInfo.InvariantCulture, out value);
      value = 0UL;
      return false;
    }

    public TEnum GetNextEnum<TEnum>(int offset) where TEnum : struct, Enum
    {
      return Enum.Parse<TEnum>(this.GetNext(offset), true);
    }

    public bool TryGetNextEnum<TEnum>(int offset, out TEnum value) where TEnum : struct, Enum
    {
      string str;
      if (this.TryGetNext(offset, out str))
        return Enum.TryParse<TEnum>(str, true, out value);
      value = default (TEnum);
      return false;
    }
  }
}
