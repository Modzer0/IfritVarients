// Decompiled with JetBrains decompiler
// Type: FiringCone
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;

#nullable disable
[Serializable]
public class FiringCone
{
  [SerializeField]
  private Transform transform;
  [SerializeField]
  private float coneAngle;
  [SerializeField]
  private bool exclusion;

  public Vector3 GetDirection() => this.transform.forward;

  public float GetAngle() => this.coneAngle;

  public bool VectorPermitted(Vector3 checkVector, out Vector3 allowedVector)
  {
    allowedVector = checkVector;
    float num1 = Vector3.Angle(checkVector, this.transform.forward);
    if (!this.exclusion && (double) num1 > (double) this.coneAngle)
      allowedVector = Vector3.RotateTowards(this.transform.forward, checkVector, this.coneAngle * ((float) Math.PI / 180f), 1f);
    if (this.exclusion && (double) num1 < (double) this.coneAngle)
    {
      float num2 = this.coneAngle - num1;
      allowedVector = Vector3.RotateTowards(checkVector, this.transform.forward, (float) (-(double) num2 * (Math.PI / 180.0)), 1f);
    }
    return checkVector == allowedVector;
  }
}
