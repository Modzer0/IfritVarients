// Decompiled with JetBrains decompiler
// Type: qol.WeaponLoading.Configs.WeaponVariantDef
// Assembly: qol, Version=1.1.6.3, Culture=neutral, PublicKeyToken=null
// MVID: 3B6539EA-E38C-45EE-8FFF-FC58CC42BADC
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\BepInEx\plugins\qol_1.1.6.3b1.dll

#nullable disable
namespace qol.WeaponLoading.Configs;

public struct WeaponVariantDef
{
  public string VariantId;
  public VariantType Type;
  public bool Enabled;
  public string SourcePrefab;
  public string SourceInfo;
  public string SourceDefinition;
  public string NewPrefabName;
  public string NewInfoName;
  public MountDef[] Mounts;
  public string PostProcessorId;
}
