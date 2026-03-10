// Decompiled with JetBrains decompiler
// Type: RelaxedStabilityController
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;

#nullable disable
public class RelaxedStabilityController : MonoBehaviour
{
  [SerializeField]
  private float canardRange;
  [SerializeField]
  private GameObject engine;
  private IEngine engineInterface;
  private float effectiveness = 1f;

  private void Awake()
  {
    this.engineInterface = this.engine.GetComponent<IEngine>();
    this.engineInterface.OnEngineDisable += new Action(this.RelaxedStabilityController_OnEngineDisable);
  }

  public void FilterInput(ControlInputs inputs, Rigidbody rb, float gForce, float rawPitch)
  {
    if ((double) this.effectiveness == 0.0)
      return;
    float a = TargetCalc.GetAngleOnAxis(rb.transform.forward, rb.velocity, rb.transform.right) / this.canardRange;
    if ((double) rb.velocity.sqrMagnitude <= 900.0)
      return;
    inputs.pitch = Mathf.Lerp(a, rawPitch, Mathf.Abs(rawPitch));
  }

  private void RelaxedStabilityController_OnEngineDisable() => this.effectiveness = 0.0f;
}
