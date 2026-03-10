// Decompiled with JetBrains decompiler
// Type: JamesFrowen.ScriptableVariables.ConsoleParser
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

#nullable disable
namespace JamesFrowen.ScriptableVariables;

public static class ConsoleParser
{
  private static List<string> tokenizeCahce = new List<string>();

  public static List<string> TokenizeNonAlloc(string input)
  {
    ConsoleParser.tokenizeCahce.Clear();
    ConsoleParser.Tokenize(input, ConsoleParser.tokenizeCahce);
    return ConsoleParser.tokenizeCahce;
  }

  public static List<string> Tokenize(string input)
  {
    List<string> results = new List<string>();
    ConsoleParser.Tokenize(input, results);
    return results;
  }

  public static void Tokenize(string input, List<string> results, bool clearList = false)
  {
    if (input.Length > 10000)
      throw new ArgumentException("Input string too long. Max length 10000");
    if (clearList)
      results.Clear();
    int pos = 0;
    while (pos < input.Length)
    {
      if (ConsoleParser.isWhiteSpace(input, pos))
        ++pos;
      else if (ConsoleParser.isNonEscapedQuote(input, pos))
        results.Add(ConsoleParser.parseQuoted(input, ref pos));
      else
        results.Add(ConsoleParser.parse(input, ref pos));
    }
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static bool isWhiteSpace(string input, int pos)
  {
    switch (input[pos])
    {
      case '\t':
        return true;
      case ' ':
        return true;
      default:
        return false;
    }
  }

  private static bool isNonEscapedQuote(string input, int pos)
  {
    bool flag1 = input[pos] == '"';
    if (pos == 0)
      return flag1;
    bool flag2 = input[pos - 1] == '\\';
    return flag1 && !flag2;
  }

  private static string parseQuoted(string input, ref int pos)
  {
    ++pos;
    int startIndex = pos;
    while (pos < input.Length)
    {
      if (ConsoleParser.isNonEscapedQuote(input, pos))
      {
        ++pos;
        return input.Substring(startIndex, pos - startIndex - 1);
      }
      ++pos;
    }
    return input.Substring(startIndex);
  }

  private static string parse(string input, ref int pos)
  {
    int startIndex = pos;
    while (pos < input.Length)
    {
      if (ConsoleParser.isWhiteSpace(input, pos))
        return input.Substring(startIndex, pos - startIndex);
      ++pos;
    }
    return input.Substring(startIndex);
  }
}
