// Decompiled with JetBrains decompiler
// Type: TopAttack
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;

#nullable disable
[Serializable]
public class TopAttack
{
  public float Amount;
  public float TooCloseRange = 2000f;
  public float probability = 1f;
  [NonSerialized]
  public bool Active;
  [SerializeField]
  private float minRange;
  [SerializeField]
  private float maxRange;

  public Vector3 ApplyTopAttack(GlobalPosition missilePos, GlobalPosition targetPos, float speed)
  {
    if (!FastMath.InRange(missilePos, targetPos, this.maxRange))
      return Vector3.zero;
    this.Active = true;
    return Mathf.Clamp01((float) ((double) ((targetPos - missilePos) with
    {
      y = 0.0f
    }).magnitude * 0.5 / (double) this.minRange - 0.20000000298023224)) * this.Amount * Vector3.up;
  }
}
