// Decompiled with JetBrains decompiler
// Type: qol.WeaponLoading.Configs.MountDef
// Assembly: qol, Version=1.1.6.3, Culture=neutral, PublicKeyToken=null
// MVID: 3B6539EA-E38C-45EE-8FFF-FC58CC42BADC
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\BepInEx\plugins\qol_1.1.6.3b1.dll

#nullable disable
namespace qol.WeaponLoading.Configs;

public struct MountDef
{
  public string SourceMount;
  public string NewName;
  public string MeshSwapPattern;
  public float MeshScale;
  public string[] DestroyPatterns;
  public string MeshSourcePath;

  public static MountDef Simple(string sourceMount, string newName)
  {
    return new MountDef()
    {
      SourceMount = sourceMount,
      NewName = newName,
      MeshSwapPattern = (string) null,
      MeshScale = 1f,
      DestroyPatterns = (string[]) null,
      MeshSourcePath = (string) null
    };
  }

  public static MountDef WithMeshSwap(
    string sourceMount,
    string newName,
    string pattern,
    float scale = 1f,
    string meshSource = null)
  {
    return new MountDef()
    {
      SourceMount = sourceMount,
      NewName = newName,
      MeshSwapPattern = pattern,
      MeshScale = scale,
      DestroyPatterns = (string[]) null,
      MeshSourcePath = meshSource
    };
  }
}
