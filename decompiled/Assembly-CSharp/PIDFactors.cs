// Decompiled with JetBrains decompiler
// Type: PIDFactors
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;

#nullable disable
[Serializable]
public class PIDFactors
{
  [SerializeField]
  private Vector3 PID;

  public float P => this.PID.x;

  public float I => this.PID.y;

  public float D => this.PID.z;

  public PIDFactors(float p, float i, float d)
  {
    this.PID.x = p;
    this.PID.y = i;
    this.PID.z = d;
  }
}
