// Decompiled with JetBrains decompiler
// Type: VLSBooster
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;

#nullable disable
public class VLSBooster : MonoBehaviour
{
  [SerializeField]
  private Missile missile;
  private Rigidbody rb;
  [SerializeField]
  private float thrust;
  [SerializeField]
  private float burnTime;
  [SerializeField]
  private float fuelMass;
  [SerializeField]
  private float dryMass;
  [SerializeField]
  private float torque;
  [SerializeField]
  private float maxTurnRate;
  private float burnRate;
  private float originalTorque;
  private float originalMaxTurnRate;
  private bool activated;
  private bool separated;
  private bool splashed;
  [SerializeField]
  private ParticleSystem[] particleSystems;
  [SerializeField]
  private TrailEmitter[] trailEmitters;
  [SerializeField]
  private AudioSource[] audioSources;
  [SerializeField]
  private Light[] lights;
  [SerializeField]
  private GameObject splashPrefab;

  private void Awake()
  {
    this.missile.onInitialize += new Action(this.VLSBooster_OnInitialize);
    this.burnRate = this.fuelMass / this.burnTime;
  }

  private void VLSBooster_OnInitialize()
  {
    this.missile.onInitialize -= new Action(this.VLSBooster_OnInitialize);
    if ((UnityEngine.Object) this.missile.owner == (UnityEngine.Object) null || this.missile.owner is Aircraft || GameManager.gameState == GameState.Encyclopedia)
      UnityEngine.Object.Destroy((UnityEngine.Object) this.gameObject);
    else
      this.Activate();
  }

  private void Activate()
  {
    this.activated = true;
    foreach (ParticleSystem particleSystem in this.particleSystems)
      particleSystem.Play();
    foreach (AudioSource audioSource in this.audioSources)
      audioSource.Play();
    foreach (Behaviour light in this.lights)
      light.enabled = true;
    this.originalTorque = this.missile.GetTorque();
    this.originalMaxTurnRate = this.missile.GetMaxTurnRate();
    this.missile.SetTorque(this.torque, this.maxTurnRate);
  }

  public float Thrust()
  {
    if (!this.activated)
      return 0.0f;
    this.fuelMass -= this.burnRate * Time.fixedDeltaTime;
    this.missile.rb.mass -= this.burnRate * Time.fixedDeltaTime;
    if ((double) this.fuelMass <= 0.0 || (double) this.transform.position.y < (double) Datum.LocalSeaY)
      this.Burnout();
    if (this.missile.LocalSim)
      this.missile.rb.AddForce(this.thrust * this.missile.transform.forward);
    return this.thrust;
  }

  public void Splash()
  {
    this.splashed = true;
    GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.splashPrefab, Datum.origin);
    gameObject.transform.position = new Vector3(this.transform.position.x, Datum.LocalSeaY, this.transform.position.z);
    gameObject.transform.rotation = Quaternion.LookRotation(Vector3.up + this.rb.velocity.normalized);
    this.enabled = false;
    UnityEngine.Object.Destroy((UnityEngine.Object) gameObject, 20f);
  }

  public void Burnout()
  {
    foreach (ParticleSystem particleSystem in this.particleSystems)
      particleSystem.Stop();
    foreach (AudioSource audioSource in this.audioSources)
      audioSource.Stop();
    foreach (Behaviour light in this.lights)
      light.enabled = false;
    this.separated = true;
    this.transform.SetParent((Transform) null);
    this.rb = this.gameObject.AddComponent<Rigidbody>();
    this.rb.mass = this.dryMass;
    this.rb.drag = 0.1f;
    this.rb.angularDrag = 0.01f;
    this.rb.isKinematic = false;
    this.rb.interpolation = RigidbodyInterpolation.Interpolate;
    this.rb.velocity = this.missile.rb.isKinematic ? Vector3.zero : this.missile.rb.velocity;
    this.missile.SetTorque(this.originalTorque, this.originalMaxTurnRate);
    UnityEngine.Object.Destroy((UnityEngine.Object) this.gameObject, 10f);
  }

  private void FixedUpdate()
  {
    if ((double) this.fuelMass > 0.0)
    {
      double num = (double) this.Thrust();
    }
    if (!this.separated || this.splashed || (double) this.transform.position.y >= (double) Datum.LocalSeaY)
      return;
    this.Splash();
  }
}
