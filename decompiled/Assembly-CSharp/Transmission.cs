// Decompiled with JetBrains decompiler
// Type: Transmission
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System.Collections.Generic;
using UnityEngine;

#nullable disable
public class Transmission : MonoBehaviour
{
  [SerializeField]
  private GameObject[] powerSources;
  [SerializeField]
  private float governorSmoothing = 0.5f;
  private IPowerSource[] sourceInterfaces;
  private List<IPowerOutput> outputInterfaces = new List<IPowerOutput>();
  private List<float> powerRequests = new List<float>();
  private float totalPowerRequested;
  private float maxPowerOutput;
  private float throttlePosition;
  private float throttleSmoothingVel;

  private void Awake()
  {
    this.sourceInterfaces = new IPowerSource[this.powerSources.Length];
    for (int index = 0; index < this.powerSources.Length; ++index)
    {
      this.sourceInterfaces[index] = this.powerSources[index].GetComponent<IPowerSource>();
      this.maxPowerOutput += this.sourceInterfaces[index].GetMaxPower();
    }
  }

  public float GetMaxPower() => this.maxPowerOutput;

  public void RequestPower(IPowerOutput output, float powerRequested)
  {
    this.outputInterfaces.Add(output);
    this.powerRequests.Add(powerRequested);
    this.totalPowerRequested += powerRequested;
  }

  private void FixedUpdate()
  {
    double num1 = ((double) this.maxPowerOutput - (double) this.totalPowerRequested) / (double) this.maxPowerOutput;
    float num2 = 0.0f;
    foreach (IPowerSource sourceInterface in this.sourceInterfaces)
    {
      sourceInterface.Throttle(this.throttlePosition);
      num2 += sourceInterface.GetPower();
    }
    float num3 = num2 / Mathf.Max(this.totalPowerRequested, 1f);
    for (int index = 0; index < this.outputInterfaces.Count; ++index)
      this.outputInterfaces[index].SendPower(this.powerRequests[index] * num3);
    this.throttlePosition = Mathf.SmoothDamp(this.throttlePosition, this.totalPowerRequested / this.maxPowerOutput, ref this.throttleSmoothingVel, this.governorSmoothing);
    this.outputInterfaces.Clear();
    this.powerRequests.Clear();
    this.totalPowerRequested = 0.0f;
  }
}
