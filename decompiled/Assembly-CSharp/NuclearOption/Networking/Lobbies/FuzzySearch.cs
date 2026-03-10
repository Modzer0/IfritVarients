// Decompiled with JetBrains decompiler
// Type: NuclearOption.Networking.Lobbies.FuzzySearch
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

#nullable disable
namespace NuclearOption.Networking.Lobbies;

public class FuzzySearch
{
  private readonly int maxLength;
  private readonly float[,] distanceCache;

  public FuzzySearch(int maxLength)
  {
    this.maxLength = maxLength;
    this.distanceCache = new float[maxLength + 1, maxLength + 1];
  }

  public static bool SubstringSearch<T>(
    IEnumerable<T> items,
    Func<T, string> selector,
    string searchTerm,
    List<T> results)
  {
    results.Clear();
    if (string.IsNullOrWhiteSpace(searchTerm))
    {
      results.AddRange(items);
      return false;
    }
    searchTerm = searchTerm.ToLower();
    foreach (T obj in items)
    {
      if (selector(obj).ToLower().Contains(searchTerm))
        results.Add(obj);
    }
    return true;
  }

  public bool Search<T>(
    IEnumerable<T> items,
    Func<T, string> selector,
    string searchTerm,
    int maxDistance,
    List<T> results,
    List<int> optionalScores)
  {
    results.Clear();
    optionalScores?.Clear();
    if (string.IsNullOrWhiteSpace(searchTerm))
    {
      results.AddRange(items);
      return false;
    }
    searchTerm = searchTerm.ToLower();
    List<(string, float, T)> source = new List<(string, float, T)>();
    foreach (T obj in items)
    {
      string str = selector(obj);
      float num = this.Distance(str.ToLower(), searchTerm);
      if ((double) num <= (double) maxDistance)
        source.Add((str, num, obj));
    }
    foreach ((string, float, T) tuple in (IEnumerable<(string, float, T)>) source.OrderBy<(string, float, T), float>((Func<(string, float, T), float>) (x => x.Score)))
    {
      results.Add(tuple.Item3);
      optionalScores.Add((int) tuple.Item2);
    }
    return true;
  }

  public float Distance(string s, string t)
  {
    int length1 = s.Length;
    int length2 = t.Length;
    if (length1 > this.maxLength || length2 > this.maxLength)
      return (float) int.MaxValue;
    if (length1 == 0)
      return (float) length2;
    if (length2 == 0)
      return (float) length1;
    float[,] distanceCache = this.distanceCache;
    for (int index = 0; index <= length1; ++index)
      this.distanceCache[index, 0] = (float) index;
    for (int index = 0; index <= length2; ++index)
      this.distanceCache[0, index] = (float) index;
    for (int index1 = 1; index1 <= length1; ++index1)
    {
      for (int index2 = 1; index2 <= length2; ++index2)
      {
        float val1 = distanceCache[index1 - 1, index2] + 0.2f;
        float val2_1 = distanceCache[index1, index2 - 1] + 3f;
        float num = (int) t[index2 - 1] == (int) s[index1 - 1] ? 0.0f : 1.2f;
        float val2_2 = distanceCache[index1 - 1, index2 - 1] + num;
        distanceCache[index1, index2] = Math.Min(Math.Min(val1, val2_1), val2_2);
      }
    }
    return distanceCache[length1, length2];
  }

  public void LogDistanceCache(string s, string t)
  {
    if (s.Length > this.maxLength || t.Length > this.maxLength)
    {
      Debug.LogWarning((object) $"Cannot log distance cache: One or both string lengths ({s.Length}, {t.Length}) exceed the cache's maximum length ({this.maxLength}).");
    }
    else
    {
      int length1 = s.Length;
      int length2 = t.Length;
      StringBuilder stringBuilder = new StringBuilder();
      stringBuilder.Append($"{"",6}");
      stringBuilder.Append($"{"\"\"",6}");
      for (int index = 0; index < length2; ++index)
        stringBuilder.Append($"{t[index],6}");
      stringBuilder.AppendLine();
      for (int index1 = 0; index1 <= length1; ++index1)
      {
        if (index1 == 0)
          stringBuilder.Append($"{"\"\"",6}");
        else
          stringBuilder.Append($"{s[index1 - 1],6}");
        for (int index2 = 0; index2 <= length2; ++index2)
          stringBuilder.Append($"{this.distanceCache[index1, index2],6:F1}");
        stringBuilder.AppendLine();
      }
      Debug.Log((object) stringBuilder.ToString());
    }
  }
}
