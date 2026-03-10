// Decompiled with JetBrains decompiler
// Type: IDamageable
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

#nullable disable
public interface IDamageable
{
  void TakeDamage(
    float pierceDamage,
    float blastDamage,
    float amountAffected,
    float fireDamage,
    float ImpactDamage,
    PersistentID dealerID);

  void ApplyDamage(float pierceDamage, float blastDamage, float fireDamage, float impactDamage);

  void TakeShockwave(Vector3 origin, float overpressure, float blastPower);

  void Detach(Vector3 velocity, Vector3 relativePos);

  ArmorProperties GetArmorProperties();

  Unit GetUnit();

  float GetMass();

  Transform GetTransform();
}
