// Decompiled with JetBrains decompiler
// Type: HighLiftDevice
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;

#nullable disable
public class HighLiftDevice : MonoBehaviour
{
  [SerializeField]
  private AeroPart aeroPart;
  [SerializeField]
  private float speedDeployed;
  [SerializeField]
  private float speedRetracted;
  [SerializeField]
  private float deployedArea;
  [SerializeField]
  private HighLiftDevice.MovingPart[] movingParts;
  private float partAreaDeployed;
  private float partAreaRetracted;
  private Aircraft aircraft;

  private void Start()
  {
    this.aircraft = this.aeroPart.parentUnit as Aircraft;
    this.partAreaRetracted = this.aeroPart.GetWingArea();
    this.partAreaDeployed = this.partAreaRetracted + this.deployedArea;
  }

  private void FixedUpdate()
  {
    if ((double) this.aircraft.speed > (double) this.speedRetracted * 1.1000000238418579)
      return;
    float num = (float) (1.0 - (double) Mathf.Max(this.aircraft.speed - this.speedDeployed, 0.0f) / ((double) this.speedRetracted - (double) this.speedDeployed));
    this.aeroPart.SetWingArea(Mathf.Lerp(this.partAreaRetracted, this.partAreaDeployed, num));
    foreach (HighLiftDevice.MovingPart movingPart in this.movingParts)
      movingPart.Animate(num);
  }

  [Serializable]
  private class MovingPart
  {
    [SerializeField]
    private bool move;
    [SerializeField]
    private bool rotate;
    [SerializeField]
    private Transform transform;
    [SerializeField]
    private Vector3 positionRetracted;
    [SerializeField]
    private Vector3 positionDeployed;
    [SerializeField]
    private Vector3 anglesRetracted;
    [SerializeField]
    private Vector3 anglesDeployed;

    public void Animate(float deployedAmount)
    {
      if (this.move)
        this.transform.localPosition = Vector3.Lerp(this.positionRetracted, this.positionDeployed, deployedAmount);
      if (!this.rotate)
        return;
      this.transform.localEulerAngles = Vector3.Lerp(this.anglesRetracted, this.anglesDeployed, deployedAmount);
    }
  }
}
