// Decompiled with JetBrains decompiler
// Type: IRFlare
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

#nullable disable
public class IRFlare : MonoBehaviour
{
  private Vector3 velocity;
  [SerializeField]
  private float burnTime;
  [SerializeField]
  private float drag;
  [SerializeField]
  private ParticleSystem flareParticles;
  [SerializeField]
  private ParticleSystem smokeParticles;
  [SerializeField]
  private float emitFrequency;
  [SerializeField]
  private float minSpeed = 20f;
  [SerializeField]
  private Light flareLight;
  private ParticleSystem.MainModule main;
  private float emitCounter;
  private ParticleSystem.EmitParams emitParams;
  private Vector3 gravityVector = new Vector3(0.0f, 9.81f, 0.0f);
  private RaycastHit hit;
  private int layermask = 64 /*0x40*/;
  private IRSource IR;
  private Aircraft aircraft;
  private float checkTime;
  private bool nearAircraft = true;
  public ParticleSystemRenderer flareRenderer;
  public ParticleSystemRenderer smokeRenderer;

  private void OnEnable()
  {
    this.main = this.smokeParticles.main;
    this.main.simulationSpace = ParticleSystemSimulationSpace.Custom;
    this.main.customSimulationSpace = Datum.origin;
    this.checkTime = Time.timeSinceLevelLoad;
  }

  public void LaunchFlare(Aircraft aircraft, Transform launchPoint, Vector3 launchVelocity)
  {
    this.IR = new IRSource(this.transform, 1f, true);
    this.transform.position = launchPoint.position - launchVelocity * Time.deltaTime;
    this.velocity = launchVelocity;
    this.aircraft = aircraft;
    aircraft.AddIRSource(this.IR);
    this.emitParams.position = this.transform.GlobalPosition().AsVector3();
    this.emitParams.velocity = this.velocity + (float) ((double) this.main.startSpeed.constant * (double) this.velocity.magnitude * 0.0099999997764825821) * new Vector3((float) Random.Range(-1, 1), (float) Random.Range(-1, 1), (float) Random.Range(-1, 1));
    this.smokeParticles.Emit(this.emitParams, 1);
  }

  public void Emit(float airspeed, Vector3 velocity)
  {
    this.emitCounter += Time.deltaTime * Mathf.Min(this.emitFrequency, airspeed / this.minSpeed * this.emitFrequency);
    if ((double) this.emitCounter <= 1.0)
      return;
    this.emitCounter = 0.0f;
    this.emitParams.position = this.transform.GlobalPosition().AsVector3();
    this.emitParams.velocity = velocity + (float) ((double) this.main.startSpeed.constant * (double) airspeed * 0.0099999997764825821) * new Vector3((float) Random.Range(-1, 1), (float) Random.Range(-1, 1), (float) Random.Range(-1, 1));
    this.smokeParticles.Emit(this.emitParams, 1);
  }

  private void CheckFlareDistance()
  {
    if ((Object) this.aircraft == (Object) null || !this.nearAircraft || (double) Time.timeSinceLevelLoad - (double) this.checkTime < 0.5)
      return;
    this.checkTime = Time.timeSinceLevelLoad;
    if (!FastMath.OutOfRange(this.transform.position, this.aircraft.transform.position, 100f))
      return;
    this.aircraft.RemoveIRSource(this.IR);
    this.nearAircraft = false;
  }

  private void Update()
  {
    this.transform.position += this.velocity * Time.deltaTime;
    Vector3 vector3 = this.velocity - NetworkSceneSingleton<LevelInfo>.i.GetWind(this.transform.GlobalPosition());
    this.velocity -= (vector3.normalized * vector3.sqrMagnitude * this.drag + this.gravityVector) * Time.deltaTime;
    if (Physics.Linecast(this.transform.position, this.transform.position + this.velocity * Time.deltaTime * 1.05f, out this.hit, this.layermask))
    {
      this.transform.position = this.hit.point + this.hit.normal * 0.1f;
      this.velocity = Vector3.zero;
      this.gravityVector = Vector3.zero;
    }
    if (this.velocity != Vector3.zero)
      this.transform.rotation = Quaternion.LookRotation(this.velocity);
    this.burnTime -= Time.deltaTime;
    if ((double) this.transform.position.GlobalY() <= 0.0)
    {
      this.burnTime = 0.0f;
      this.velocity = Vector3.zero;
    }
    if ((double) this.burnTime > 0.0)
    {
      this.CheckFlareDistance();
      this.Emit(this.velocity.magnitude, this.velocity);
    }
    if ((double) this.burnTime > 0.0 || !this.flareParticles.isPlaying)
      return;
    this.flareLight.enabled = false;
    this.IR.intensity = 0.0f;
    if ((Object) this.aircraft != (Object) null && this.nearAircraft)
      this.aircraft.RemoveIRSource(this.IR);
    this.flareParticles.Stop();
    this.flareLight.enabled = false;
    this.smokeParticles.gameObject.transform.SetParent(Datum.origin);
    Object.Destroy((Object) this.smokeParticles.gameObject, 15f);
    Object.Destroy((Object) this.gameObject, 1f);
  }
}
