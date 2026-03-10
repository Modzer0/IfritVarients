// Decompiled with JetBrains decompiler
// Type: qol.WeaponLoading.Configs.StorageTankExplosionConfigs
// Assembly: qol, Version=1.1.6.3, Culture=neutral, PublicKeyToken=null
// MVID: 3B6539EA-E38C-45EE-8FFF-FC58CC42BADC
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\BepInEx\plugins\qol_1.1.6.3b1.dll

using System.Collections.Generic;

#nullable disable
namespace qol.WeaponLoading.Configs;

public static class StorageTankExplosionConfigs
{
  public const string ShockwavePath = "StorageTankExplosion/Shockwave";
  public const string EmbersPath = "StorageTankExplosion/fireball_large/Embers";
  public const float DamageParticlesDamage = 50f;
  public const float DamageParticlesDuration = 20f;
  public const float DamageParticlesRange = 250f;
  public const string FireballsPath = "StorageTankExplosion/fireball_large/fireballs";
  public const float FireballsEmissionRate = 1f;
  public const float FireballsStartSpeedMultiplier = 0.0f;
  public static readonly IReadOnlyList<string> FireballChildPaths = (IReadOnlyList<string>) new List<string>()
  {
    "StorageTankExplosion/fireball_large/fireballs/firesplat",
    "StorageTankExplosion/fireball_large/fireballs/smokesplat",
    "StorageTankExplosion/fireball_large/fireballs/smoketrail",
    "StorageTankExplosion/fireball_large/fireballs/firetrail"
  };
  public const float ChildStartSpeedMultiplier = 0.5f;
  public const float ChildGravityModifier = -0.1f;
}
