// Decompiled with JetBrains decompiler
// Type: NuclearOption.SavedMission.ObjectiveV2.Override`1
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;

#nullable disable
namespace NuclearOption.SavedMission.ObjectiveV2;

[Serializable]
public struct Override<T>(bool isOverride, T value) : IEquatable<Override<T>> where T : IEquatable<T>
{
  public bool IsOverride = isOverride;
  public T Value = value;

  public override bool Equals(object obj) => obj is Override<T> other && this.Equals(other);

  public override int GetHashCode() => (this.IsOverride ? 1 : 0) | this.Value.GetHashCode() << 1;

  public bool Equals(Override<T> other)
  {
    return this.IsOverride == other.IsOverride && this.Value.Equals(other.Value);
  }

  public static void SetAssert(ref Override<T> value, T newValue)
  {
    value = new Override<T>(true, newValue);
  }

  public static Override<T> NewAssert(Override<T> value, T newValue)
  {
    value = new Override<T>(true, newValue);
    return value;
  }
}
