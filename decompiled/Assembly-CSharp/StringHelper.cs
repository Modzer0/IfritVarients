// Decompiled with JetBrains decompiler
// Type: StringHelper
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Steamworks;
using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

#nullable disable
public static class StringHelper
{
  public const int STEAM_NAME_MAX_LENGTH = 32 /*0x20*/;
  public const char MISSING_CHARACTER = '□';
  private static ProfanityFilter.ProfanityFilter profanityFilter;

  public static string ProfanityFilter(this string message)
  {
    return PlayerSettings.chatFilter ? StringHelper.RunProfanityFilter(message) : message;
  }

  public static string RunProfanityFilter(string message)
  {
    if (StringHelper.profanityFilter == null)
      StringHelper.profanityFilter = new ProfanityFilter.ProfanityFilter();
    return StringHelper.profanityFilter.CensorString(message);
  }

  public static string SanitizeRichText(this string input, int maxLength)
  {
    if (input == null)
      return string.Empty;
    if (input.Length > maxLength)
      input = input.Substring(0, maxLength);
    input = input.Replace("<", "<<i></i>");
    return input;
  }

  public static void GetSanitizeSteamName(
    out string rawName,
    out string safeName,
    TMP_FontAsset font)
  {
    rawName = SteamFriends.GetPersonaName();
    safeName = rawName.SanitizeRichText(32 /*0x20*/);
    if (!((UnityEngine.Object) font != (UnityEngine.Object) null))
      return;
    safeName = safeName.ReplaceCharactersNotInFont(font);
  }

  public static string ReplaceCharactersNotInFont(this string input, TMP_FontAsset font)
  {
    if (string.IsNullOrEmpty(input))
      return input;
    List<char> missingCharacters;
    bool flag = font.HasCharacters(input, out missingCharacters);
    if (!flag && missingCharacters == null)
    {
      font.ReadFontAssetDefinition();
      flag = font.HasCharacters(input, out missingCharacters);
      if (!flag && missingCharacters == null)
      {
        Debug.LogError((object) "Failed to load font asset for ReplaceCharactersNotInFont");
        return input;
      }
    }
    if (flag)
      return input;
    Span<char> span = stackalloc char[input.Length];
    MemoryExtensions.AsSpan(input).CopyTo(span);
    for (int index = 0; index < input.Length; ++index)
    {
      if (missingCharacters.Contains(span[index]))
        span[index] = '□';
    }
    return new string(Span<char>.op_Implicit(span));
  }

  public static string AddColor(this string message, Color color)
  {
    return $"<color=#{ColorUtility.ToHtmlStringRGBA(color)}>{message}</color>";
  }

  public static string AddColor(this string message, string colorHex)
  {
    return $"<color={colorHex}>{message}</color>";
  }

  public static string AddSize(this string message, float sizePercent)
  {
    return $"<size={(ValueType) (float) ((double) sizePercent * 100.0)}%>{message}</size>";
  }

  public static string AddSize(this string message, string size)
  {
    return $"<size={size}>{message}</size>";
  }

  public static Color ChangeHue(this Color color, float hue)
  {
    float S;
    float V;
    Color.RGBToHSV(color, out float _, out S, out V);
    return Color.HSVToRGB(hue, S, V);
  }

  public static int GetByteLength(string str) => Encoding.UTF8.GetByteCount(str);

  public static List<string> SplitStringByByteCount(
    string rawInput,
    bool includeEmptyLast,
    int chunkSize,
    int maxChunks)
  {
    ReadOnlySpan<char> readOnlySpan1 = MemoryExtensions.AsSpan(rawInput);
    List<string> stringList = new List<string>();
    Encoding utF8 = Encoding.UTF8;
    int num1 = 0;
    int num2 = 0;
    do
    {
      int val1 = readOnlySpan1.Length - num1;
      if (val1 != 0)
      {
        int num3 = chunkSize / 4;
        int num4 = Math.Min(val1, chunkSize);
        if (num3 > num4)
          num3 = num4;
        int num5 = num4;
        ReadOnlySpan<char> readOnlySpan2 = readOnlySpan1.Slice(num1, num4);
        if (utF8.GetByteCount(readOnlySpan2) <= chunkSize)
        {
          num5 = num4;
        }
        else
        {
          while (num3 <= num4)
          {
            int num6 = num3 + (num4 - num3) / 2;
            if (num6 == 0)
            {
              num3 = num6 + 1;
            }
            else
            {
              ReadOnlySpan<char> readOnlySpan3 = readOnlySpan1.Slice(num1, num6);
              if (utF8.GetByteCount(readOnlySpan3) <= chunkSize)
              {
                num5 = num6;
                num3 = num6 + 1;
              }
              else
                num4 = num6 - 1;
            }
          }
        }
        stringList.Add(new string(readOnlySpan1.Slice(num1, num5)));
        num1 += num5;
        ++num2;
      }
      else
        break;
    }
    while (num2 < maxChunks);
    if (includeEmptyLast && num2 < maxChunks)
      stringList.Add("");
    return stringList;
  }

  public static int CountLines(this string str)
  {
    if (str == null)
      return 0;
    int num = 1;
    foreach (char ch in str)
    {
      if (ch == '\n')
        ++num;
    }
    return num;
  }
}
