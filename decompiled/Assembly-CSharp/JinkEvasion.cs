// Decompiled with JetBrains decompiler
// Type: JinkEvasion
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;

#nullable disable
[Serializable]
public class JinkEvasion
{
  public float amount;
  [SerializeField]
  private float period;
  [SerializeField]
  private float minRange;
  [SerializeField]
  private float maxRange;
  private Vector3 jinkOffset;
  private float lastJink;

  public Vector3 ApplyJink(GlobalPosition missilePos, GlobalPosition targetPos)
  {
    if ((double) Time.timeSinceLevelLoad - (double) this.lastJink > (double) this.period)
    {
      this.lastJink = Time.timeSinceLevelLoad;
      this.jinkOffset = Vector3.zero;
      Vector3 planeNormal = targetPos - missilePos;
      float magnitude = planeNormal.magnitude;
      if ((double) magnitude > (double) this.minRange && (double) magnitude < (double) this.maxRange)
      {
        this.jinkOffset = Vector3.ProjectOnPlane(UnityEngine.Random.insideUnitSphere, planeNormal).normalized * (this.amount * magnitude);
        this.jinkOffset.y = Mathf.Max(this.jinkOffset.y, 0.0f);
      }
    }
    return this.jinkOffset;
  }
}
