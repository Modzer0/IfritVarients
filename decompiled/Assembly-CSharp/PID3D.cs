// Decompiled with JetBrains decompiler
// Type: PID3D
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

#nullable disable
public class PID3D
{
  private Vector3 p;
  private Vector3 i;
  private Vector3 d;
  private Vector3 errorPrev;

  public Vector3 GetOutput(Vector3 currentError, float resetILimit, float deltaTime, Vector3 gain)
  {
    this.p = currentError;
    this.i = new Vector3((double) this.p.x < (double) resetILimit ? this.i.x + this.p.x : 0.0f, (double) this.p.y < (double) resetILimit ? this.i.y + this.p.y : 0.0f, (double) this.p.z < (double) resetILimit ? this.i.z + this.p.z : 0.0f);
    this.d = (currentError - this.errorPrev) / deltaTime;
    this.errorPrev = this.p;
    return this.p * gain.x + this.i * gain.y + this.d * gain.z;
  }

  public Vector3 GetOutputSqrt(
    Vector3 currentError,
    float resetILimit,
    float deltaTime,
    PIDFactors factors)
  {
    float magnitude = currentError.magnitude;
    this.p = Vector3.ClampMagnitude(currentError, Mathf.Sqrt(magnitude));
    this.i = (double) magnitude > (double) resetILimit ? Vector3.zero : this.i + this.p * deltaTime;
    this.d = (currentError - this.errorPrev) / deltaTime;
    this.errorPrev = currentError;
    return this.p * factors.P + this.i * factors.I + this.d * factors.D;
  }
}
