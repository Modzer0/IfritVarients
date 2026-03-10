// Decompiled with JetBrains decompiler
// Type: Airbrake
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System.Collections.Generic;
using UnityEngine;

#nullable disable
public class Airbrake : MonoBehaviour
{
  [SerializeField]
  private Transform[] transforms;
  [SerializeField]
  private float dragAmount;
  [SerializeField]
  private float maxAngle;
  [SerializeField]
  private float openSpeed;
  [SerializeField]
  private UnitPart part;
  [SerializeField]
  private AudioSource airbrakeSound;
  [SerializeField]
  [Range(0.0f, 2f)]
  private float volumeMultiplier;
  [SerializeField]
  private Aircraft aircraft;
  [SerializeField]
  private float maxVolumeSpeed = 340f;
  [SerializeField]
  private float pitchMin = 0.5f;
  [SerializeField]
  private float pitchMax = 2f;
  private float openAmount;
  private List<Vector3> baseAngles = new List<Vector3>();
  private ControlInputs controlInputs;
  private Aircraft attachedAircraft;

  private void Start()
  {
    this.attachedAircraft = this.part.parentUnit as Aircraft;
    this.controlInputs = this.attachedAircraft.GetInputs();
    for (int index = 0; index < this.transforms.Length; ++index)
      this.baseAngles.Add(this.transforms[index].localEulerAngles);
  }

  private void Update()
  {
    this.openAmount += (double) this.controlInputs.throttle == 0.0 ? this.openSpeed * Time.deltaTime : -this.openSpeed * Time.deltaTime;
    this.openAmount = Mathf.Clamp01(this.openAmount);
    if ((Object) this.airbrakeSound != (Object) null)
    {
      if ((double) this.openAmount > 0.0)
      {
        NetworkFloatHelper.LogNaN(this.aircraft.speed, "aircraft.speed");
        float t = Mathf.Clamp01((this.aircraft.speed - 5f) / this.maxVolumeSpeed);
        this.airbrakeSound.volume = Mathf.Sqrt(this.openAmount * t) * this.volumeMultiplier;
        this.airbrakeSound.pitch = Mathf.Lerp(this.pitchMin, this.pitchMax, t);
        if (!this.airbrakeSound.isPlaying)
          this.airbrakeSound.Play();
      }
      else if (this.airbrakeSound.isPlaying)
        this.airbrakeSound.Stop();
    }
    for (int index = 0; index < this.transforms.Length; ++index)
    {
      this.transforms[index].localEulerAngles = this.baseAngles[index];
      this.transforms[index].Rotate(new Vector3(this.openAmount * this.maxAngle, 0.0f, 0.0f), Space.Self);
    }
  }

  private void FixedUpdate()
  {
    if ((double) this.openAmount <= 0.0 || this.attachedAircraft.remoteSim)
      return;
    this.part.rb.AddForce(this.openAmount * -this.part.rb.velocity.normalized * (this.dragAmount * this.part.parentUnit.airDensity * this.part.rb.velocity.sqrMagnitude));
  }
}
