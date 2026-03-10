// Decompiled with JetBrains decompiler
// Type: PropFan
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;

#nullable disable
public class PropFan : MonoBehaviour, IEngine, IPowerOutput, IReportDamage
{
  [SerializeField]
  private Transmission transmission;
  [SerializeField]
  private AnimationCurve efficiencyCurve;
  [SerializeField]
  private UnitPart part;
  [SerializeField]
  private AudioSource source;
  [SerializeField]
  private AudioClip interiorSound;
  [SerializeField]
  private AudioClip exteriorSound;
  [SerializeField]
  private float volumeBase = 0.2f;
  [SerializeField]
  private float rpmVolumePortion = 0.5f;
  [SerializeField]
  private float aoaVolumePortion = 0.3f;
  [SerializeField]
  private float pitchMultiplier = 1f;
  [SerializeField]
  private float rpmPitchPortion = 0.9f;
  [SerializeField]
  private float aoaPitchPortion = 0.1f;
  [SerializeField]
  private PropFan.Prop[] props;
  [SerializeField]
  private PropStrikeDetector propStrike;
  private ControlInputs inputs;
  [SerializeField]
  private Transform thrustTransform;
  [SerializeField]
  private float area;
  [SerializeField]
  private float nominalRPM;
  [SerializeField]
  private float startTime;
  [SerializeField]
  private string failureMessage;
  [SerializeField]
  private AudioClip failureMessageAudio;
  private Aircraft aircraft;
  private float availablePower;
  private float nominalPower;
  private float rpmRatio;
  private float powerRatio;
  private float currentRPM;
  private float condition;
  private float thrust;
  private bool damageReported;

  public event Action OnEngineDisable;

  public event Action OnEngineDamage;

  public event Action<OnReportDamage> onReportDamage;

  private void Awake()
  {
    this.aircraft = this.part.parentUnit as Aircraft;
    this.inputs = this.aircraft.GetInputs();
    this.aircraft.onInitialize += new Action(this.PropFan_OnInitialize);
    this.condition = 1f;
    if ((UnityEngine.Object) this.propStrike != (UnityEngine.Object) null)
      this.propStrike.OnStrike += new Action<float>(this.PropFan_OnPropStrike);
    this.aircraft.engines.Add((IEngine) this);
  }

  private void PropFan_OnPropStrike(float distance)
  {
    this.currentRPM *= 0.5f;
    this.condition = 0.0f;
    if (this.damageReported || this.part.IsDetached())
      return;
    this.damageReported = true;
    foreach (PropFan.Prop prop in this.props)
      prop.Damage();
    Action<OnReportDamage> onReportDamage = this.onReportDamage;
    if (onReportDamage == null)
      return;
    onReportDamage(new OnReportDamage()
    {
      failureMessage = this.failureMessage,
      audioReport = this.failureMessageAudio
    });
  }

  private void PropFan_OnInitialize()
  {
    if ((double) this.aircraft.radarAlt <= 1.0)
      return;
    this.currentRPM = this.nominalRPM;
  }

  private void Start() => this.nominalPower = this.transmission.GetMaxPower();

  public void SendPower(float power) => this.availablePower += power;

  public float GetMaxThrust() => 0.0f;

  public float GetThrust() => this.thrust;

  public float GetRPM() => this.currentRPM;

  public float GetRPMRatio() => this.rpmRatio;

  public void SetInteriorSounds(bool useInteriorSound)
  {
    if (this.source.isPlaying)
    {
      if (useInteriorSound)
      {
        this.source.Stop();
        this.source.clip = this.interiorSound;
        this.source.time = UnityEngine.Random.Range(0.0f, this.source.clip.length);
        this.source.Play();
      }
      else
      {
        this.source.Stop();
        this.source.clip = this.exteriorSound;
        this.source.time = UnityEngine.Random.Range(0.0f, this.source.clip.length);
        this.source.Play();
      }
    }
    else if (useInteriorSound)
      this.source.clip = this.interiorSound;
    else
      this.source.clip = this.exteriorSound;
  }

  public void ReportDamage()
  {
    if (this.damageReported)
      return;
    this.damageReported = true;
    Action<OnReportDamage> onReportDamage = this.onReportDamage;
    if (onReportDamage == null)
      return;
    onReportDamage(new OnReportDamage()
    {
      failureMessage = this.failureMessage,
      audioReport = this.failureMessageAudio
    });
  }

  private void Update()
  {
    foreach (PropFan.Prop prop in this.props)
      prop.Animate(this.currentRPM, this.rpmRatio);
  }

  private void FixedUpdate()
  {
    Vector3 vector3 = this.aircraft.rb.velocity - NetworkSceneSingleton<LevelInfo>.i.GetWind(this.transform.GlobalPosition());
    float powerRequested = this.nominalPower * Mathf.Max(this.inputs.throttle, 0.1f) * this.condition;
    float num1 = this.efficiencyCurve.Evaluate(vector3.magnitude);
    this.rpmRatio = this.currentRPM / this.nominalRPM;
    this.availablePower *= this.condition;
    this.powerRatio = this.availablePower / this.nominalPower;
    if ((double) this.currentRPM < (double) this.nominalRPM && (double) this.availablePower > 0.0)
    {
      powerRequested = this.nominalPower;
      this.currentRPM += this.nominalRPM / this.startTime * Mathf.Clamp((float) (1.0 - (double) Mathf.Abs(this.rpmRatio - 0.5f) * 2.0), 0.2f, 2f) * this.powerRatio * Time.fixedDeltaTime;
      this.currentRPM = Mathf.Min(this.currentRPM, this.nominalRPM);
    }
    if ((double) this.availablePower == 0.0)
    {
      float num2 = Mathf.Lerp(1f, 0.1f, this.condition);
      this.currentRPM -= (float) ((double) Mathf.Max(Mathf.Sqrt(this.rpmRatio), 0.005f) * 3.0 * (double) num2 * ((double) this.nominalRPM / (double) this.startTime)) * Time.fixedDeltaTime;
      this.currentRPM = Mathf.Max(this.currentRPM, 0.0f);
    }
    float num3 = Mathf.Min(this.powerRatio, this.inputs.throttle);
    this.thrust = Mathf.Pow(2f * this.aircraft.GetAirDensity() * this.area * this.availablePower * this.availablePower, 0.33333f) * num1 * num3;
    if (this.aircraft.LocalSim)
      this.part.rb.AddForceAtPosition(this.thrust * this.rpmRatio * this.thrustTransform.forward, this.thrustTransform.position);
    this.source.volume = (float) ((double) this.volumeBase + (double) this.rpmVolumePortion * (double) this.rpmRatio + (double) this.aoaVolumePortion * (double) num3 * (double) this.rpmRatio);
    this.source.pitch = this.pitchMultiplier * (float) ((double) this.rpmPitchPortion * (double) this.rpmRatio + (double) this.aoaPitchPortion * (double) num3 * (double) this.rpmRatio);
    this.transmission.RequestPower((IPowerOutput) this, powerRequested);
    this.availablePower = 0.0f;
  }

  Transform IEngine.get_transform() => this.transform;

  [Serializable]
  private class Prop
  {
    [SerializeField]
    private Transform transform;
    [SerializeField]
    private bool antiClockwise;
    [SerializeField]
    private Renderer propRenderer;
    [SerializeField]
    private Renderer diskRenderer;
    [SerializeField]
    private MeshFilter propMeshFilter;
    [SerializeField]
    private MeshFilter diskMeshFilter;
    [SerializeField]
    private Mesh[] diskMeshes;
    [SerializeField]
    private Mesh damageMesh;
    private bool damaged;

    public void Animate(float rpm, float rpmRatio)
    {
      if ((double) Time.timeScale == 0.0)
        return;
      this.transform.Rotate((float) ((double) rpm * 0.016666669398546219 * 360.0 * (this.antiClockwise ? 1.0 : -1.0)) * Time.deltaTime * Vector3.forward);
      if (this.damaged)
        return;
      bool flag = (double) rpmRatio * (double) Time.timeScale > 0.10000000149011612;
      this.propRenderer.enabled = !flag;
      this.diskRenderer.enabled = flag;
      if (!flag)
        return;
      this.diskMeshFilter.mesh = (double) rpmRatio > 0.550000011920929 ? this.diskMeshes[1] : this.diskMeshes[0];
    }

    public void Damage()
    {
      this.propRenderer.enabled = true;
      this.diskRenderer.enabled = false;
      this.propMeshFilter.mesh = this.damageMesh;
      this.damaged = true;
    }
  }
}
