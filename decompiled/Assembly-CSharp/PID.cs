// Decompiled with JetBrains decompiler
// Type: PID
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;

#nullable disable
[Serializable]
public class PID
{
  public float pFactor;
  public float iFactor;
  public float dFactor;
  private float p;
  private float i;
  private float d;
  private float previousError;

  public PID(float p, float i, float d)
  {
    this.pFactor = p;
    this.iFactor = i;
    this.dFactor = d;
  }

  public PID(Vector3 pid)
  {
    this.pFactor = pid.x;
    this.iFactor = pid.y;
    this.dFactor = pid.z;
  }

  public PID(PIDFactors factors)
  {
    this.pFactor = factors.P;
    this.iFactor = factors.I;
    this.dFactor = factors.D;
  }

  public void SetValues(float p, float i, float d)
  {
    this.pFactor = p;
    this.iFactor = i;
    this.dFactor = d;
  }

  public void Reseti() => this.i = 0.0f;

  public float GetOutput(float currentError, float deltaTime)
  {
    this.p = currentError;
    this.i += this.p * deltaTime;
    this.d = (currentError - this.previousError) / deltaTime;
    this.previousError = this.p;
    return (float) ((double) this.p * (double) this.pFactor + (double) this.i * (double) this.iFactor + (double) this.d * (double) this.dFactor);
  }

  public float GetOutput(float currentError, float iThreshold, float deltaTime)
  {
    this.p = currentError;
    this.i = (double) Mathf.Abs(this.p) < (double) iThreshold ? this.i + this.p * deltaTime : 0.0f;
    this.d = (currentError - this.previousError) / deltaTime;
    this.previousError = this.p;
    return (float) ((double) this.p * (double) this.pFactor + (double) this.i * (double) this.iFactor + (double) this.d * (double) this.dFactor);
  }

  public float GetOutput(float currentError, float iThreshold, float deltaTime, Vector3 gain)
  {
    this.p = currentError;
    this.i = (double) Mathf.Abs(this.p) < (double) iThreshold ? this.i + this.p * deltaTime : 0.0f;
    this.d = (currentError - this.previousError) / deltaTime;
    this.previousError = this.p;
    return (float) ((double) this.p * (double) gain.x + (double) this.i * (double) gain.y + (double) this.d * (double) gain.z);
  }

  public float GetOutputSqrt(float currentError, float iThreshold, float deltaTime, Vector3 gain)
  {
    this.p = (double) Mathf.Abs(currentError) > 1.0 ? Mathf.Sign(currentError) * Mathf.Sqrt(Mathf.Abs(currentError)) : currentError;
    this.i = (double) Mathf.Abs(this.p) < (double) iThreshold ? this.i + this.p * deltaTime : 0.0f;
    this.d = (currentError - this.previousError) / deltaTime;
    this.previousError = this.p;
    return (float) ((double) this.p * (double) gain.x + (double) this.i * (double) gain.y + (double) this.d * (double) gain.z);
  }

  public float GetOutput(float currentError, float measuredD, float iThreshold, float deltaTime)
  {
    this.p = currentError;
    this.i = (double) Mathf.Abs(this.p) < (double) iThreshold ? this.i + this.p * deltaTime : 0.0f;
    return (float) ((double) this.p * (double) this.pFactor + (double) this.i * (double) this.iFactor + (double) measuredD * (double) this.dFactor);
  }

  public float GetOutput(
    float currentError,
    float measuredD,
    float iThreshold,
    float iClamp,
    float deltaTime)
  {
    this.p = currentError;
    this.i = (double) Mathf.Abs(this.p) < (double) iThreshold ? this.i + this.p * deltaTime : this.i;
    this.i = Mathf.Clamp(this.i, -iClamp, iClamp);
    return (float) ((double) this.p * (double) this.pFactor + (double) this.i * (double) this.iFactor + (double) measuredD * (double) this.dFactor);
  }

  public float GetOutputDirect(
    float currentError,
    float measuredD,
    float iThreshold,
    float deltaTime,
    Vector3 gain)
  {
    this.p = currentError;
    this.i = (double) Mathf.Abs(this.p) < (double) iThreshold ? this.i + this.p * deltaTime : this.i;
    return (float) ((double) this.p * (double) gain.x + (double) this.i * (double) gain.y + (double) measuredD * (double) gain.z);
  }
}
