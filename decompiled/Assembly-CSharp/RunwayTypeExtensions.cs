// Decompiled with JetBrains decompiler
// Type: RunwayTypeExtensions
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System.Runtime.CompilerServices;

#nullable disable
public static class RunwayTypeExtensions
{
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool Allowed(this RunwayType runway, RunwayQueryType query)
  {
    if (runway == RunwayType.None)
      return false;
    return query == RunwayQueryType.Any || (runway & (RunwayType) query) > RunwayType.None;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool QueryFor(this RunwayQueryType query, RunwayType runway)
  {
    return query == RunwayQueryType.Any || (runway & (RunwayType) query) > RunwayType.None;
  }
}
