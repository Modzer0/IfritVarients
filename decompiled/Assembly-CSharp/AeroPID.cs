// Decompiled with JetBrains decompiler
// Type: AeroPID
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;

#nullable disable
[Serializable]
public class AeroPID
{
  private PIDFactors factors;
  private float referenceAirspeed;
  private float p;
  private float i;
  private float d;
  private float errorPrev;

  public AeroPID(PIDFactors factors, float referenceAirspeed)
  {
    this.factors = factors;
    this.referenceAirspeed = referenceAirspeed;
  }

  public void Reseti() => this.i = 0.0f;

  public float GetOutput(
    float currentError,
    float measuredD,
    float clampI,
    float airspeed,
    float deltaTime)
  {
    float num = this.referenceAirspeed * this.referenceAirspeed / Mathf.Max(airspeed * airspeed, 1000f);
    this.p = currentError;
    this.i += this.p * deltaTime;
    this.i = Mathf.Clamp(this.i, -clampI, clampI);
    return (float) ((double) this.p * (double) this.factors.P * (double) num + (double) this.i * (double) this.factors.I * (double) num + (double) measuredD * (double) this.factors.D * (double) num);
  }

  public float GetOutput(float currentError, float clampI, float airspeed)
  {
    float num = Mathf.Clamp(this.referenceAirspeed * this.referenceAirspeed / Mathf.Max(airspeed * airspeed, 10f), 0.0f, 4f);
    this.p = currentError;
    this.i = (double) Mathf.Abs(this.p) < (double) clampI ? this.i + this.p * Time.fixedDeltaTime : 0.0f;
    this.d = (currentError - this.errorPrev) / Time.fixedDeltaTime;
    this.errorPrev = this.p;
    return (float) ((double) this.p * (double) this.factors.P + (double) this.i * (double) this.factors.I + (double) this.d * (double) this.factors.D * (double) num);
  }

  public float GetOutputClampP(float currentError, float clampP, float clampI, float airspeed)
  {
    float num = Mathf.Clamp(this.referenceAirspeed * this.referenceAirspeed / Mathf.Max(airspeed * airspeed, 10f), 0.0f, 4f);
    this.p = currentError;
    this.i = (double) Mathf.Abs(this.p) < (double) clampI ? this.i + this.p * Time.fixedDeltaTime : 0.0f;
    this.d = (currentError - this.errorPrev) / Time.fixedDeltaTime;
    this.errorPrev = this.p;
    return (float) ((double) Mathf.Clamp(this.p, -clampP, clampP) * (double) this.factors.P + (double) this.i * (double) this.factors.I + (double) this.d * (double) this.factors.D * (double) num);
  }
}
