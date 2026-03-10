// Decompiled with JetBrains decompiler
// Type: TypeIdentity
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;

#nullable disable
[Serializable]
public struct TypeIdentity(float surface, float air, float missile, float radar, float strategic)
{
  [Range(0.0f, 1f)]
  public float surface = surface;
  [Range(0.0f, 1f)]
  public float air = air;
  [Range(0.0f, 1f)]
  public float missile = missile;
  [Range(0.0f, 1f)]
  public float radar = radar;
  [Range(0.0f, 1f)]
  public float strategic = strategic;

  public float ThreatPosedBy(RoleIdentity threat)
  {
    return (float) ((double) this.surface * (double) threat.antiSurface + (double) this.air * (double) threat.antiAir + (double) this.missile * (double) threat.antiMissile + (double) this.radar * (double) threat.antiRadar);
  }
}
