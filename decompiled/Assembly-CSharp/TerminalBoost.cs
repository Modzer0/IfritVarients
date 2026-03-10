// Decompiled with JetBrains decompiler
// Type: TerminalBoost
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;

#nullable disable
[Serializable]
public class TerminalBoost
{
  public float Amount;
  [NonSerialized]
  public bool Active;
  [SerializeField]
  private float minRange;
  [SerializeField]
  private float maxRange;

  public void ApplyTerminalBoost(
    Missile missile,
    GlobalPosition missilePos,
    GlobalPosition targetPos)
  {
    if (!FastMath.InRange(missilePos, targetPos, this.maxRange) || this.Active)
      return;
    this.Active = true;
    missile.ApplyTerminalBoost(this.Amount);
  }
}
