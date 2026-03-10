// Decompiled with JetBrains decompiler
// Type: ControlSurfacePhysics
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

#nullable disable
public class ControlSurfacePhysics : MonoBehaviour
{
  [SerializeField]
  private AeroPart[] connectedParts;
  [SerializeField]
  private float pitchRange;
  [SerializeField]
  private float rollRange;
  [SerializeField]
  private float yawRange;
  [SerializeField]
  private float servoSpeed;
  [SerializeField]
  private float spring;
  [SerializeField]
  private float damp;
  [SerializeField]
  private float breakStrength;
  [SerializeField]
  private AeroPart part;
  private float currentAngle;
  private ControlInputs inputs;

  private void Awake()
  {
    if (!((Object) this.part.parentUnit != (Object) null) || !(this.part.parentUnit is Aircraft parentUnit))
      return;
    this.inputs = parentUnit.GetInputs();
  }

  private void FixedUpdate()
  {
    this.currentAngle += Mathf.Clamp((float) ((double) this.inputs.pitch * (double) this.pitchRange + (double) this.inputs.yaw * (double) this.yawRange + (double) this.inputs.roll * (double) this.rollRange) - this.currentAngle, -this.servoSpeed * Time.fixedDeltaTime, this.servoSpeed * Time.fixedDeltaTime);
    for (int index = 0; index < this.connectedParts.Length; ++index)
      this.part.SetHingeJoint(index, this.connectedParts[index], this.spring, this.damp, this.currentAngle, this.breakStrength, 0.0f);
  }
}
