// Decompiled with JetBrains decompiler
// Type: MushroomCloud
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;
using UnityEngine.Rendering;

#nullable disable
public class MushroomCloud : MonoBehaviour
{
  public float yield;
  [SerializeField]
  private float effectReferenceYield;
  [SerializeField]
  private AnimationCurve lensFlareIntensity;
  [SerializeField]
  private LensFlareComponentSRP lensFlare;
  [SerializeField]
  private Optional<MushroomCloud.Fireball> fireball;
  [SerializeField]
  private MushroomCloud.Cloud cloud;
  [SerializeField]
  private Optional<MushroomCloud.BaseCloud> baseCloud;
  [SerializeField]
  private Optional<MushroomCloud.Stem> stem;
  [SerializeField]
  private Optional<MushroomCloud.Updraft> updraft;
  [SerializeField]
  private Optional<MushroomCloud.WaterEffect> waterEffect;
  [SerializeField]
  private MushroomCloud.CloudRing[] cloudRings;
  private Vector3 groundZero;
  private Vector3 groundNormal;
  private float time;
  private float blastScale;
  private float scaleAdjust;

  private void Start()
  {
    this.transform.rotation = Quaternion.identity;
    this.scaleAdjust = Mathf.Pow(this.yield / this.effectReferenceYield, 0.3333333f);
    this.blastScale = Mathf.Pow(this.yield / 1000000f, 0.3333333f);
    float burstAltitude = 0.0f;
    float groundBurstFactor = 0.0f;
    float waterBurstFactor = 0.0f;
    RaycastHit hitInfo;
    if (Physics.Raycast(this.transform.position, Vector3.down, out hitInfo, this.blastScale * 300f, 64 /*0x40*/) && (double) hitInfo.point.y > (double) Datum.origin.position.y)
    {
      burstAltitude = hitInfo.distance;
      groundBurstFactor = this.SampleGroundPosition(hitInfo.point);
    }
    if ((double) groundBurstFactor == 0.0)
    {
      burstAltitude = this.transform.position.y - Datum.origin.position.y;
      waterBurstFactor = Mathf.Clamp01((float) (((double) this.blastScale * 250.0 - (double) burstAltitude) / ((double) this.blastScale * 250.0)));
    }
    this.cloud.Initialize(groundBurstFactor, this.blastScale);
    if (this.baseCloud.Enabled)
      this.baseCloud.SetEnabled(this.baseCloud.Value.Initialize(groundBurstFactor, this.blastScale, this.groundZero, this.groundNormal));
    if (this.updraft.Enabled)
      this.updraft.SetEnabled(this.updraft.Value.Initialize(groundBurstFactor, this.groundZero));
    if (this.stem.Enabled)
    {
      GameObject gameObject = new GameObject();
      gameObject.transform.position = this.groundZero;
      gameObject.transform.SetParent(Datum.origin);
      this.stem.SetEnabled(this.stem.Value.Initialize(groundBurstFactor, this.blastScale, gameObject.transform, this.groundZero, this.groundNormal));
    }
    if (this.waterEffect.Enabled)
      this.waterEffect.Value.Initialize(waterBurstFactor);
    foreach (MushroomCloud.CloudRing cloudRing in this.cloudRings)
      cloudRing.Initialize(this.blastScale, burstAltitude);
  }

  private float SampleGroundPosition(Vector3 groundZero)
  {
    Vector3[] vector3Array = new Vector3[3];
    for (int index = 0; index < 3; ++index)
    {
      Vector3 vector3 = groundZero + new Vector3(this.blastScale * 200f * Mathf.Sin((float) (index * 120) * ((float) Math.PI / 180f)), 0.0f, this.blastScale * 200f * Mathf.Cos((float) (index * 120) * ((float) Math.PI / 180f)));
      RaycastHit hitInfo;
      if (!Physics.Linecast(vector3 + Vector3.up * this.blastScale * 200f, vector3 - Vector3.up * this.blastScale * 200f, out hitInfo, 64 /*0x40*/))
        return 0.0f;
      vector3Array[index] = hitInfo.point;
    }
    Plane plane = new Plane(vector3Array[0], vector3Array[1], vector3Array[2]);
    this.groundZero = plane.ClosestPointOnPlane(groundZero);
    this.groundNormal = plane.normal;
    return Mathf.Clamp01((float) (((double) this.blastScale * 250.0 - (double) Vector3.Distance(groundZero, this.transform.position)) / ((double) this.blastScale * 150.0)));
  }

  private void Update()
  {
    this.time += Time.deltaTime;
    if (this.fireball.Enabled)
      this.fireball.SetEnabled(this.fireball.Value.Update(this.time));
    this.cloud.Update(this.time);
    if (this.baseCloud.Enabled)
      this.baseCloud.SetEnabled(this.baseCloud.Value.Update(this.time));
    if (this.updraft.Enabled)
      this.updraft.SetEnabled(this.updraft.Value.Update(this.time, this.cloud.GetPosition()));
    if (this.stem.Enabled)
      this.stem.SetEnabled(this.stem.Value.Update(this.time));
    foreach (MushroomCloud.CloudRing cloudRing in this.cloudRings)
      cloudRing.Update(this.time);
    if (!((UnityEngine.Object) this.lensFlare != (UnityEngine.Object) null))
      return;
    this.lensFlare.intensity = this.lensFlareIntensity.Evaluate(this.time);
    if ((double) this.lensFlare.intensity > 0.0)
      return;
    UnityEngine.Object.Destroy((UnityEngine.Object) this.lensFlare);
  }

  [Serializable]
  private class CloudRing
  {
    [SerializeField]
    private Renderer renderer;
    [SerializeField]
    private float altitudeMin;
    [SerializeField]
    private float altitudeMax;
    [SerializeField]
    private float maxRadius;
    [SerializeField]
    private float scrollSpeed;
    [SerializeField]
    private AnimationCurve emissionOverTime;
    [SerializeField]
    private Gradient colorOverRadius;
    private float heightOffset;
    private float blastScale;
    private int id_CloudColor = Shader.PropertyToID("_CloudColor");
    private int id_ScrollSpeed = Shader.PropertyToID("_ScrollSpeed");
    private int id_EmissiveStrength = Shader.PropertyToID("_EmissiveStrength");

    public void Initialize(float blastScale, float burstAltitude)
    {
      this.altitudeMin = Mathf.Max(this.altitudeMin, (float) (-(double) burstAltitude + 200.0));
      this.renderer.transform.rotation = Quaternion.identity;
      this.renderer.transform.localScale = Vector3.one;
      this.renderer.material = new Material(this.renderer.material);
      this.renderer.material.SetColor(this.id_CloudColor, Color.clear);
      this.renderer.material.SetFloat(this.id_ScrollSpeed, this.scrollSpeed);
      this.heightOffset = UnityEngine.Random.Range(-this.altitudeMin, this.altitudeMax);
      this.renderer.transform.localPosition = Vector3.up * this.heightOffset;
      this.blastScale = blastScale;
    }

    public void Update(float time)
    {
      float num1 = (float) ((double) this.blastScale * 50.0 + 340.0 * (double) time);
      if ((double) num1 < (double) Mathf.Abs(this.heightOffset))
        return;
      float num2 = Mathf.Sqrt((float) ((double) num1 * (double) num1 - (double) this.heightOffset * (double) this.heightOffset));
      this.renderer.transform.localScale = Vector3.one * num2;
      double num3 = (double) num2 / (double) this.maxRadius;
      float num4 = (float) (1.0 - (double) num1 / (double) this.maxRadius);
      this.renderer.material.SetFloat(this.id_EmissiveStrength, this.emissionOverTime.Evaluate(time));
      this.renderer.material.SetColor(this.id_CloudColor, this.colorOverRadius.Evaluate(num4 * num2 / this.maxRadius));
    }
  }

  [Serializable]
  private class Fireball
  {
    [SerializeField]
    private MeshRenderer renderer;
    [SerializeField]
    private AnimationCurve sizeOverTime;
    [SerializeField]
    private AnimationCurve emitOverTime;
    [SerializeField]
    private Gradient colorOverTime;
    private int id_FireballColor = Shader.PropertyToID("_FireballColor");
    private int id_EmissiveStrength = Shader.PropertyToID("_EmissiveStrength");

    public bool Update(float time)
    {
      if (!this.renderer.enabled)
        return false;
      this.renderer.transform.localScale = Vector3.one * this.sizeOverTime.Evaluate(time);
      Material material = this.renderer.material;
      int idFireballColor = this.id_FireballColor;
      Gradient colorOverTime = this.colorOverTime;
      double num1 = (double) time;
      Keyframe[] keys1 = this.emitOverTime.keys;
      double time1 = (double) keys1[keys1.Length - 1].time;
      double time2 = num1 / time1;
      Color color = colorOverTime.Evaluate((float) time2);
      material.SetColor(idFireballColor, color);
      this.renderer.material.SetFloat(this.id_EmissiveStrength, this.emitOverTime.Evaluate(time));
      double num2 = (double) time;
      Keyframe[] keys2 = this.emitOverTime.keys;
      double time3 = (double) keys2[keys2.Length - 1].time;
      if (num2 <= time3)
        return true;
      this.renderer.enabled = false;
      return false;
    }
  }

  [Serializable]
  private class Cloud
  {
    [SerializeField]
    private Optional<AnimationCurve> emitOverTime;
    [SerializeField]
    private Optional<AnimationCurve> emitRoughnessOverTime;
    [SerializeField]
    private AnimationCurve scrollOverTime;
    [SerializeField]
    private AnimationCurve horizontalSizeOverTime;
    [SerializeField]
    private AnimationCurve relativeHeightOverTime;
    [SerializeField]
    private AnimationCurve particleRateOverTime;
    [SerializeField]
    private AnimationCurve riseRateOverTime;
    [SerializeField]
    private AnimationCurve windInfluenceOverTime;
    [SerializeField]
    private Gradient colorOverTime;
    [SerializeField]
    private MeshRenderer meshRenderer;
    [SerializeField]
    private ParticleSystem cloudParticles;
    private float lifetime;
    private float groundBurstFactor;
    private ParticleSystem.MainModule main;
    private ParticleSystem.EmissionModule emission;
    private ParticleSystem.ShapeModule shape;
    private ParticleSystem.ForceOverLifetimeModule force;
    private Vector3 baseShape;
    private Vector3 appliedWind;
    private float lastSlowUpdate;
    private float riseRate;
    private bool underwater;
    private int id_EmissiveStrength = Shader.PropertyToID("_EmissiveStrength");
    private int id_EmissiveRoughness = Shader.PropertyToID("_EmissiveRoughness");
    private int id_CloudColor = Shader.PropertyToID("_CloudColor");
    private int id_ScrollPosition = Shader.PropertyToID("_ScrollPosition");

    public void Initialize(float groundBurstFactor, float blastScale)
    {
      this.meshRenderer.material = new Material(this.meshRenderer.material);
      this.meshRenderer.transform.position += Vector3.up * 25f * blastScale * (groundBurstFactor * groundBurstFactor);
      this.meshRenderer.transform.rotation = Quaternion.identity;
      this.main = this.cloudParticles.main;
      this.lifetime = this.main.duration;
      this.emission = this.cloudParticles.emission;
      this.shape = this.cloudParticles.shape;
      this.baseShape = new Vector3(this.shape.scale.x / this.meshRenderer.transform.localScale.x, this.shape.scale.y / this.meshRenderer.transform.localScale.y, this.shape.scale.z / this.meshRenderer.transform.localScale.z);
      this.riseRate = this.riseRateOverTime.Evaluate(0.0f);
      float num = this.horizontalSizeOverTime.Evaluate(0.0f);
      this.meshRenderer.transform.localScale = new Vector3(num, num * this.relativeHeightOverTime.Evaluate(0.0f), num);
    }

    public Vector3 GetPosition() => this.meshRenderer.transform.position;

    public void Update(float time)
    {
      if (this.underwater)
        return;
      if (this.emitOverTime.Enabled)
      {
        float num = this.emitOverTime.Value.Evaluate(time);
        this.emitOverTime.SetEnabled((double) num > 0.0);
        this.meshRenderer.material.SetFloat(this.id_EmissiveStrength, num);
        this.meshRenderer.material.SetFloat(this.id_EmissiveRoughness, this.emitRoughnessOverTime.Value.Evaluate(time));
        this.meshRenderer.material.SetColor(this.id_CloudColor, this.colorOverTime.Evaluate(time / this.lifetime));
      }
      this.meshRenderer.material.SetFloat(this.id_ScrollPosition, this.scrollOverTime.Evaluate(time));
      this.meshRenderer.transform.position += (Vector3.up * this.riseRate + NetworkSceneSingleton<LevelInfo>.i.GetWind(this.meshRenderer.transform.position.ToGlobalPosition())) * Time.deltaTime;
      float num1 = this.horizontalSizeOverTime.Evaluate(time);
      float num2 = this.relativeHeightOverTime.Evaluate(time);
      this.meshRenderer.transform.localScale = new Vector3(num1, num1 * num2, num1);
      if ((double) Time.timeSinceLevelLoad - (double) this.lastSlowUpdate < 1.0)
        return;
      this.lastSlowUpdate = Time.timeSinceLevelLoad;
      Color color = this.colorOverTime.Evaluate(time / this.lifetime);
      this.meshRenderer.material.SetColor(this.id_CloudColor, color);
      this.riseRate = this.riseRateOverTime.Evaluate(time);
      this.appliedWind = NetworkSceneSingleton<LevelInfo>.i.GetWind(this.meshRenderer.transform.position.ToGlobalPosition()) * this.windInfluenceOverTime.Evaluate(time);
      this.main.startSize = new ParticleSystem.MinMaxCurve(num1 * 0.5f);
      this.emission.rateOverTime = new ParticleSystem.MinMaxCurve(this.particleRateOverTime.Evaluate(time));
      this.main.startColor = new ParticleSystem.MinMaxGradient(color * 0.8f, color * 1.2f);
    }
  }

  [Serializable]
  private class BaseCloud
  {
    [SerializeField]
    private ParticleSystem system;
    [SerializeField]
    private Optional<AnimationCurve> radiusOverTime;
    [SerializeField]
    private Optional<AnimationCurve> rateOverTime;
    [SerializeField]
    private Optional<AnimationCurve> startSizeOverTime;
    [SerializeField]
    private Gradient colorOverTime;
    private float lifetime;
    private float lastSlowUpdate;
    private float groundBurstFactor;
    private ParticleSystem.MainModule main;
    private ParticleSystem.EmissionModule emission;
    private ParticleSystem.ShapeModule shape;

    public bool Initialize(
      float groundBurstFactor,
      float blastScale,
      Vector3 groundZero,
      Vector3 groundNormal)
    {
      if ((double) groundBurstFactor == 0.0)
      {
        UnityEngine.Object.Destroy((UnityEngine.Object) this.system);
        return false;
      }
      this.groundBurstFactor = groundBurstFactor;
      this.system.transform.rotation = Quaternion.LookRotation(groundNormal == Vector3.zero ? Vector3.forward : Vector3.Cross(groundNormal, Vector3.up), groundNormal);
      this.system.transform.position = groundZero + this.system.transform.up * 10f * blastScale;
      this.main = this.system.main;
      this.lifetime = this.main.duration;
      this.emission = this.system.emission;
      this.shape = this.system.shape;
      return true;
    }

    public bool Update(float time)
    {
      if (this.radiusOverTime.Enabled)
      {
        float time1 = time / this.lifetime;
        this.shape.radius = this.radiusOverTime.Value.Evaluate(time1);
        ref Optional<AnimationCurve> local = ref this.radiusOverTime;
        double num1 = (double) time1;
        Keyframe[] keys = this.radiusOverTime.Value.keys;
        double time2 = (double) keys[keys.Length - 1].time;
        int num2 = num1 < time2 ? 1 : 0;
        local.SetEnabled(num2 != 0);
      }
      if ((double) Time.timeSinceLevelLoad - (double) this.lastSlowUpdate < 1.0)
        return true;
      this.lastSlowUpdate = Time.timeSinceLevelLoad;
      float time3 = time / (this.lifetime * Mathf.Clamp(this.groundBurstFactor, 0.5f, 1f));
      Color color = this.colorOverTime.Evaluate(time3);
      this.main.startColor = new ParticleSystem.MinMaxGradient(new Color(color.r * 1.2f, color.g * 1.2f, color.b * 1.2f, 1.2f * color.a * Mathf.Clamp(this.groundBurstFactor, 0.5f, 1f)), new Color(color.r * 0.8f, color.g * 0.8f, color.b * 0.8f, 0.8f * color.a * Mathf.Clamp(this.groundBurstFactor, 0.5f, 1f)));
      if (this.startSizeOverTime.Enabled)
        this.main.startSize = new ParticleSystem.MinMaxCurve(this.startSizeOverTime.Value.Evaluate(time3));
      if (this.rateOverTime.Enabled)
        this.emission.rateOverTime = new ParticleSystem.MinMaxCurve(this.groundBurstFactor * this.rateOverTime.Value.Evaluate(time3));
      return (double) color.a > 0.0;
    }
  }

  [Serializable]
  private class Updraft
  {
    [SerializeField]
    private ParticleSystemForceField forceField;
    [SerializeField]
    private Optional<AnimationCurve> strengthOverTime;
    private float startingStrength;
    private float startingUpdraft;
    private float lastSlowUpdate;

    public bool Initialize(float groundBurstFactor, Vector3 groundZero)
    {
      if ((double) groundBurstFactor == 0.0)
      {
        UnityEngine.Object.Destroy((UnityEngine.Object) this.forceField);
        return false;
      }
      ParticleSystem.MinMaxCurve minMaxCurve = this.forceField.gravity;
      this.startingStrength = minMaxCurve.constant;
      minMaxCurve = this.forceField.directionY;
      this.startingUpdraft = minMaxCurve.constant;
      this.forceField.transform.SetPositionAndRotation(groundZero, Quaternion.identity);
      return true;
    }

    public bool Update(float time, Vector3 cloudPosition)
    {
      if ((double) Time.timeSinceLevelLoad - (double) this.lastSlowUpdate < 1.0)
        return true;
      this.lastSlowUpdate = Time.timeSinceLevelLoad;
      float num = this.strengthOverTime.Value.Evaluate(time);
      this.forceField.transform.rotation = Quaternion.LookRotation(Vector3.Cross(Vector3.up, cloudPosition - this.forceField.transform.position), cloudPosition - this.forceField.transform.position);
      this.forceField.gravity = new ParticleSystem.MinMaxCurve(this.startingStrength * num);
      this.forceField.directionY = new ParticleSystem.MinMaxCurve(this.startingUpdraft * num);
      return (double) num > 0.0;
    }
  }

  [Serializable]
  private class Stem
  {
    [SerializeField]
    private ParticleSystem system;
    [SerializeField]
    private Optional<AnimationCurve> rateOverTime;
    [SerializeField]
    private Optional<AnimationCurve> startSizeOverTime;
    [SerializeField]
    private Optional<AnimationCurve> lifeOverTime;
    [SerializeField]
    private Optional<AnimationCurve> noiseOverTime;
    [SerializeField]
    private Gradient startColorOverTime;
    private float lifetime;
    private float lastSlowUpdate;
    private float groundBurstFactor;
    private ParticleSystem.MainModule main;
    private ParticleSystem.EmissionModule emission;
    private ParticleSystem.NoiseModule noise;
    private ParticleSystem.ForceOverLifetimeModule force;
    private float lastEmit;
    private float emitInterval;
    private float blastScale;

    public bool Initialize(
      float groundBurstFactor,
      float blastScale,
      Transform baseCloud,
      Vector3 groundZero,
      Vector3 groundNormal)
    {
      if ((double) groundBurstFactor == 0.0)
      {
        UnityEngine.Object.Destroy((UnityEngine.Object) this.system);
        return false;
      }
      this.blastScale = blastScale;
      this.groundBurstFactor = groundBurstFactor;
      this.system.transform.rotation = Quaternion.identity;
      this.main = this.system.main;
      this.main.simulationSpace = ParticleSystemSimulationSpace.Custom;
      this.main.customSimulationSpace = baseCloud;
      this.lifetime = this.main.duration;
      this.emission = this.system.emission;
      this.noise = this.system.noise;
      return true;
    }

    public bool Update(float time)
    {
      if ((double) Time.timeSinceLevelLoad - (double) this.lastEmit > (double) this.emitInterval)
      {
        this.lastEmit = Time.timeSinceLevelLoad;
        ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams();
        Vector2 vector2 = UnityEngine.Random.insideUnitCircle;
        vector2 = vector2.normalized * vector2.sqrMagnitude;
        Vector3 vector3 = new Vector3(vector2.x, 0.0f, vector2.y);
        emitParams.position = vector3 * this.blastScale * 300f + Vector3.up * this.blastScale * 10f;
        emitParams.velocity = -(vector3 * this.blastScale * 20f) + Vector3.up * 5f;
        this.system.Emit(emitParams, 1);
      }
      if ((double) Time.timeSinceLevelLoad - (double) this.lastSlowUpdate < 1.0)
        return true;
      this.lastSlowUpdate = Time.timeSinceLevelLoad;
      float time1 = time / (this.lifetime * this.groundBurstFactor);
      Color color = this.startColorOverTime.Evaluate(time1);
      this.main.startColor = new ParticleSystem.MinMaxGradient(new Color(color.r * 1.2f, color.g * 1.2f, color.b * 1.2f, 1.2f * color.a * Mathf.Clamp(this.groundBurstFactor, 0.5f, 1f)), new Color(color.r * 0.8f, color.g * 0.8f, color.b * 0.8f, 0.8f * color.a * Mathf.Clamp(this.groundBurstFactor, 0.5f, 1f)));
      if (this.rateOverTime.Enabled)
      {
        this.emitInterval = this.rateOverTime.Value.Evaluate(time1);
        this.emission.rateOverTime = new ParticleSystem.MinMaxCurve(this.groundBurstFactor * this.emitInterval);
        this.emitInterval = 0.5f / this.emitInterval;
      }
      if (this.startSizeOverTime.Enabled)
      {
        float num = this.startSizeOverTime.Value.Evaluate(time1);
        this.main.startSize = new ParticleSystem.MinMaxCurve(num * 0.8f, num * 1.2f);
      }
      if (this.lifeOverTime.Enabled)
      {
        float num = Mathf.Clamp(this.groundBurstFactor, 0.5f, 1f) * this.lifeOverTime.Value.Evaluate(time);
        this.main.startLifetime = new ParticleSystem.MinMaxCurve(num * 0.6f, num * 1.2f);
      }
      return (double) color.a > 0.0;
    }
  }

  [Serializable]
  private class WaterEffect
  {
    [SerializeField]
    private GameObject[] surfaceEffects;
    [SerializeField]
    private GameObject[] underwaterEffects;
    [SerializeField]
    private GameObject[] aboveWaterEffects;
    [SerializeField]
    private Renderer[] aboveWaterRenderers;

    public bool Initialize(float waterBurstFactor)
    {
      if ((double) waterBurstFactor == 0.0)
        return false;
      foreach (GameObject surfaceEffect in this.surfaceEffects)
      {
        surfaceEffect.SetActive(true);
        surfaceEffect.transform.position = new Vector3(surfaceEffect.transform.position.x, Datum.origin.position.y, surfaceEffect.transform.position.z);
      }
      if ((double) waterBurstFactor == 1.0)
      {
        foreach (GameObject underwaterEffect in this.underwaterEffects)
        {
          underwaterEffect.SetActive(true);
          underwaterEffect.transform.position = new Vector3(underwaterEffect.transform.position.x, Datum.origin.position.y, underwaterEffect.transform.position.z);
        }
        foreach (GameObject aboveWaterEffect in this.aboveWaterEffects)
          aboveWaterEffect.SetActive(false);
        foreach (Renderer aboveWaterRenderer in this.aboveWaterRenderers)
          aboveWaterRenderer.enabled = false;
      }
      return true;
    }
  }
}
