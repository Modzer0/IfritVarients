// Decompiled with JetBrains decompiler
// Type: SpecialFlare
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;

#nullable disable
[Serializable]
public class SpecialFlare : MonoBehaviour
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
  private float checkTime;
  public ParticleSystemRenderer flareRenderer;
  public ParticleSystemRenderer smokeRenderer;
  public Color[] listColor = new Color[6]
  {
    Color.blue,
    Color.green,
    Color.magenta,
    Color.magenta,
    Color.red,
    Color.yellow
  };
  public Material[] listSmoke = new Material[0];
  public Material[] listSparks = new Material[0];

  private void OnEnable()
  {
    this.main = this.smokeParticles.main;
    this.main.simulationSpace = ParticleSystemSimulationSpace.Custom;
    this.main.customSimulationSpace = Datum.origin;
    this.checkTime = Time.timeSinceLevelLoad;
    int index = UnityEngine.Random.Range(0, this.listSmoke.Length);
    this.flareParticles.startColor = this.listColor[index];
    this.smokeParticles.startColor = this.listColor[index];
    ParticleSystem.ColorOverLifetimeModule colorOverLifetime = this.smokeParticles.colorOverLifetime;
    Gradient gradient = new Gradient();
    gradient.SetKeys(new GradientColorKey[2]
    {
      new GradientColorKey(this.listColor[index], 0.0f),
      new GradientColorKey(this.listColor[index], 1f)
    }, new GradientAlphaKey[2]
    {
      new GradientAlphaKey(1f, 0.0f),
      new GradientAlphaKey(0.0f, 1f)
    });
    colorOverLifetime.color = (ParticleSystem.MinMaxGradient) gradient;
    this.flareLight.color = this.listColor[index];
    this.flareRenderer.material = this.listSparks[index];
    this.smokeRenderer.material = this.listSmoke[index];
  }

  public void LaunchFlare(Transform launchPoint, Vector3 launchVelocity)
  {
    this.transform.position = launchPoint.position - launchVelocity * Time.deltaTime;
    this.velocity = launchVelocity;
    this.emitParams.position = this.transform.GlobalPosition().AsVector3();
    this.emitParams.velocity = this.velocity + (float) ((double) this.main.startSpeed.constant * (double) this.velocity.magnitude * 0.0099999997764825821) * new Vector3((float) UnityEngine.Random.Range(-1, 1), (float) UnityEngine.Random.Range(-1, 1), (float) UnityEngine.Random.Range(-1, 1));
    this.smokeParticles.Emit(this.emitParams, 1);
  }

  public void Emit(float airspeed, Vector3 velocity)
  {
    this.emitCounter += Time.deltaTime * Mathf.Min(this.emitFrequency, airspeed / this.minSpeed * this.emitFrequency);
    if ((double) this.emitCounter <= 1.0)
      return;
    this.emitCounter = 0.0f;
    this.emitParams.position = this.transform.GlobalPosition().AsVector3();
    this.emitParams.velocity = velocity + (float) ((double) this.main.startSpeed.constant * (double) airspeed * 0.0099999997764825821) * new Vector3((float) UnityEngine.Random.Range(-1, 1), (float) UnityEngine.Random.Range(-1, 1), (float) UnityEngine.Random.Range(-1, 1));
    this.smokeParticles.Emit(this.emitParams, 1);
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
      this.Emit(this.velocity.magnitude, this.velocity);
    if ((double) this.burnTime > 0.0 || !this.flareParticles.isPlaying)
      return;
    this.flareLight.enabled = false;
    this.flareParticles.Stop();
    this.flareLight.enabled = false;
    this.smokeParticles.gameObject.transform.SetParent(Datum.origin);
    UnityEngine.Object.Destroy((UnityEngine.Object) this.smokeParticles.gameObject, 15f);
    UnityEngine.Object.Destroy((UnityEngine.Object) this.gameObject, 1f);
  }
}
