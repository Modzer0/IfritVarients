// Decompiled with JetBrains decompiler
// Type: AoALimiter
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;

#nullable disable
[Serializable]
public class AoALimiter
{
  private Aircraft aircraft;
  [SerializeField]
  private float threshold = 7f;
  [SerializeField]
  private float limit = 20f;

  public void Initialize(Aircraft aircraft) => this.aircraft = aircraft;

  public float GetExcessAlpha()
  {
    Vector3 vector3 = this.aircraft.cockpit.transform.InverseTransformDirection(this.aircraft.rb.velocity);
    return Mathf.Clamp01((float) ((-(double) Mathf.Atan2(vector3.y, vector3.z) * 57.295780181884766 - (double) this.threshold) / ((double) this.limit - (double) this.threshold))) * this.limit;
  }
}
