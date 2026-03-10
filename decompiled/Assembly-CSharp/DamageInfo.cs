// Decompiled with JetBrains decompiler
// Type: DamageInfo
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

#nullable disable
public struct DamageInfo(
  float pierceDamage,
  float blastDamage,
  float fireDamage,
  float impactDamage)
{
  public CompressedFloat pierceDamage = pierceDamage.Compress();
  public CompressedFloat blastDamage = blastDamage.Compress();
  public CompressedFloat fireDamage = fireDamage.Compress();
  public CompressedFloat impactDamage = impactDamage.Compress();

  public bool IsValid()
  {
    return float.IsFinite(this.pierceDamage.Decompress()) && float.IsFinite(this.blastDamage.Decompress()) && float.IsFinite(this.fireDamage.Decompress()) && float.IsFinite(this.impactDamage.Decompress());
  }
}
