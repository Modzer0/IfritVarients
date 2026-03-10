// Decompiled with JetBrains decompiler
// Type: NuclearOption.Jobs.NullableIndex
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using System.Runtime.CompilerServices;

#nullable disable
namespace NuclearOption.Jobs;

public readonly struct NullableIndex
{
  private readonly uint raw;

  public NullableIndex(int index)
  {
    if (index < 0)
      throw new ArgumentException("Index can not be negative");
    this.raw = (uint) (index << 1 | 1);
  }

  public bool HasValue
  {
    [MethodImpl(MethodImplOptions.AggressiveInlining)] get => (this.raw & 1U) > 0U;
  }

  public int Index
  {
    [MethodImpl(MethodImplOptions.AggressiveInlining)] get => (int) (this.raw >> 1);
  }

  public override string ToString() => !this.HasValue ? "NoIndex" : $"{this.Index}";
}
