// Decompiled with JetBrains decompiler
// Type: DamageablePart
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

#nullable disable
public readonly struct DamageablePart(IDamageable part, bool removed = false)
{
  public readonly IDamageable Damageable = part;
  public readonly bool Removed = removed;

  public DamageablePart CreateRemoved() => new DamageablePart(this.Damageable, true);
}
