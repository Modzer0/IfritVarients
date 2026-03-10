// Decompiled with JetBrains decompiler
// Type: HangarDoor
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;

#nullable disable
[Serializable]
public class HangarDoor
{
  public Transform transform;
  public AnimatedPhysicsSurface animatedPhysicsSurface;
  private Vector3 baseAngle;
  public Vector3 openAngle;
  private Vector3 basePos;
  public Vector3 openPos;
  private float openAmountPrev;

  public void Initialize(float openAmount)
  {
    this.baseAngle = this.transform.localEulerAngles;
    this.basePos = this.transform.localPosition;
    this.Move(openAmount);
  }

  public void Move(float openAmount)
  {
    this.transform.localEulerAngles = Vector3.Lerp(this.baseAngle, this.baseAngle + this.openAngle, openAmount);
    this.transform.localPosition = Vector3.Lerp(this.basePos, this.basePos + this.openPos, openAmount);
    if ((UnityEngine.Object) this.animatedPhysicsSurface != (UnityEngine.Object) null && (double) Time.deltaTime > 0.0 && (double) openAmount != (double) this.openAmountPrev)
      this.animatedPhysicsSurface.SetAnimationVelocity(this.openPos * ((openAmount - this.openAmountPrev) / Time.fixedDeltaTime));
    this.openAmountPrev = openAmount;
  }
}
