// Decompiled with JetBrains decompiler
// Type: PowerSupply
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;

#nullable disable
public class PowerSupply : MonoBehaviour
{
  [SerializeField]
  private GameObject[] powerSources;
  private IEngine[] engineInterfaces;
  private float charge;
  private float powerRequested;
  private float powerDrawn;
  [SerializeField]
  private float maxCharge;
  [SerializeField]
  private float maxPower;
  [SerializeField]
  private float chargePerRPM;
  [SerializeField]
  private float pitchMin;
  [SerializeField]
  private float pitchMax;
  [SerializeField]
  [Range(0.0f, 2f)]
  private float volumeMultiplier;
  [SerializeField]
  private AnimationCurve supplyAtCharge;
  [SerializeField]
  private AudioSource source;
  [SerializeField]
  private Aircraft aircraft;

  public int Users { get; private set; }

  public event Action<PowerSupply> onChargeChanged;

  private void Awake()
  {
    this.engineInterfaces = new IEngine[this.powerSources.Length];
    this.aircraft.onInitialize += new Action(this.PowerSupply_OnSpawnedInPosition);
    for (int index = 0; index < this.powerSources.Length; ++index)
      this.engineInterfaces[index] = this.powerSources[index].GetComponent<IEngine>();
  }

  public void AddUser() => ++this.Users;

  public void RemoveUser() => --this.Users;

  public void ModifyCapacitance(float capacitance) => this.maxCharge += capacitance;

  private void FixedUpdate()
  {
    if ((double) this.charge >= (double) this.maxCharge && (double) this.powerDrawn == 0.0)
    {
      this.enabled = false;
      this.source.Stop();
    }
    else
    {
      if (!this.source.isPlaying)
        this.source.Play();
      foreach (IEngine engineInterface in this.engineInterfaces)
        this.charge += this.chargePerRPM * engineInterface.GetRPM() * Time.deltaTime;
      this.charge = Mathf.Clamp(this.charge, 0.0f, this.maxCharge);
      Action<PowerSupply> onChargeChanged = this.onChargeChanged;
      if (onChargeChanged != null)
        onChargeChanged(this);
      this.source.pitch = Mathf.Lerp(this.pitchMin, this.pitchMax, this.powerDrawn / Mathf.Max(this.powerRequested, 0.01f));
      this.source.volume = Mathf.Sqrt(this.powerDrawn / this.maxPower) * this.volumeMultiplier;
      this.powerRequested = 0.0f;
      this.powerDrawn = 0.0f;
    }
  }

  private void PowerSupply_OnSpawnedInPosition()
  {
    this.charge = (double) this.aircraft.radarAlt > (double) this.aircraft.definition.spawnOffset.y + 1.0 ? this.maxCharge : 0.0f;
  }

  public float GetCharge() => this.charge / this.maxCharge;

  public float GetPowerSupplied() => this.powerDrawn / this.maxPower;

  public float GetPowerAvailable() => this.supplyAtCharge.Evaluate(this.charge / this.maxCharge);

  public float GetChargeKJ() => this.charge;

  public float DrawPower(float powerRequested)
  {
    this.enabled = true;
    if (!this.source.isPlaying)
      this.source.Play();
    this.powerRequested += powerRequested;
    float num = (double) this.charge > 0.0 ? powerRequested * this.supplyAtCharge.Evaluate(this.charge / this.maxCharge) : 0.0f;
    this.charge -= num * Time.deltaTime;
    this.charge = Mathf.Max(this.charge, 0.0f);
    this.powerDrawn += num;
    return num;
  }
}
