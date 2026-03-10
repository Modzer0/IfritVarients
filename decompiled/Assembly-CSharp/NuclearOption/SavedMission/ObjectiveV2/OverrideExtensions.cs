// Decompiled with JetBrains decompiler
// Type: NuclearOption.SavedMission.ObjectiveV2.OverrideExtensions
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;

#nullable disable
namespace NuclearOption.SavedMission.ObjectiveV2;

public static class OverrideExtensions
{
  public static T? AsNullable<T>(this Override<T> @override) where T : struct, IEquatable<T>
  {
    return !@override.IsOverride ? new T?() : new T?(@override.Value);
  }

  public static void IfOverride<T>(this ValueWrapperOverride<T> wrapper, Action<T> callback) where T : IEquatable<T>
  {
    wrapper.Value.IfOverride<T>(callback);
  }

  public static void IfOverride<T>(this Override<T> @override, Action<T> callback) where T : IEquatable<T>
  {
    if (!@override.IsOverride)
      return;
    callback(@override.Value);
  }
}
