// Decompiled with JetBrains decompiler
// Type: RoleIdentity
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;

#nullable disable
[Serializable]
public struct RoleIdentity
{
  [Range(0.0f, 1f)]
  public float antiSurface;
  [Range(0.0f, 1f)]
  public float antiAir;
  [Range(0.0f, 1f)]
  public float antiMissile;
  [Range(0.0f, 1f)]
  public float antiRadar;

  public float OpportunityAgainst(TypeIdentity target)
  {
    return (float) ((double) target.surface * (double) this.antiSurface + (double) target.air * (double) this.antiAir + (double) target.missile * (double) this.antiMissile + (double) target.radar * (double) this.antiRadar);
  }
}
