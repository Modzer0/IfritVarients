// Decompiled with JetBrains decompiler
// Type: PID2D
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

#nullable disable
public class PID2D
{
  private float pFactor;
  private float iFactor;
  private float dFactor;
  private float pLimit;
  private float iLimit;
  private Vector2 p;
  private Vector2 i;
  private Vector2 d;
  private Vector2 errorPrev;

  public PID2D(PIDFactors factors, float pLimit, float iLimit)
  {
    this.pFactor = factors.P;
    this.iFactor = factors.I;
    this.dFactor = factors.D;
    this.pLimit = pLimit;
    this.iLimit = iLimit;
  }

  public void SetPLimit(float pLimit) => this.pLimit = pLimit;

  public Vector2 GetOutput(Vector2 currentError, float deltaTime)
  {
    this.p = currentError;
    this.i = new Vector2((double) Mathf.Abs(this.p.x) < (double) this.iLimit ? this.i.x + this.p.x : 0.0f, (double) Mathf.Abs(this.p.y) < (double) this.iLimit ? this.i.y + this.p.y : 0.0f);
    this.d = (currentError - this.errorPrev) / deltaTime;
    this.errorPrev = this.p;
    this.p.x = Mathf.Clamp(this.p.x, -this.pLimit, this.pLimit);
    this.p.y = Mathf.Clamp(this.p.y, -this.pLimit, this.pLimit);
    return this.p * this.pFactor + this.i * this.iFactor + this.d * this.dFactor;
  }

  public Vector2 GetOutput(Vector2 currentError, Vector2 measuredD)
  {
    this.p = currentError;
    this.i = new Vector2((double) Mathf.Abs(this.p.x) < (double) this.iLimit ? this.i.x + this.p.x : 0.0f, (double) Mathf.Abs(this.p.y) < (double) this.iLimit ? this.i.y + this.p.y : 0.0f);
    this.d = measuredD;
    this.p.x = Mathf.Clamp(this.p.x, -this.pLimit, this.pLimit);
    this.p.y = Mathf.Clamp(this.p.y, -this.pLimit, this.pLimit);
    return this.p * this.pFactor + this.i * this.iFactor + this.d * this.dFactor;
  }
}
