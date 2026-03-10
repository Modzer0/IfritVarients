// Decompiled with JetBrains decompiler
// Type: PersistentID
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;

#nullable disable
public struct PersistentID : IEquatable<PersistentID>
{
  public uint Id;

  public static PersistentID None => new PersistentID();

  public bool IsValid => this.Id > 0U;

  public bool NotValid => this.Id == 0U;

  public readonly bool TryGetUnit(out Unit unit)
  {
    return UnitRegistry.TryGetUnit(new PersistentID?(this), out unit);
  }

  public override string ToString() => this.Id.ToString();

  public bool Equals(PersistentID other) => (int) this.Id == (int) other.Id;

  public override bool Equals(object obj) => obj is PersistentID other && this.Equals(other);

  public override int GetHashCode() => this.Id.GetHashCode();

  public static bool operator ==(PersistentID left, PersistentID right) => left.Equals(right);

  public static bool operator !=(PersistentID left, PersistentID right) => !left.Equals(right);
}
