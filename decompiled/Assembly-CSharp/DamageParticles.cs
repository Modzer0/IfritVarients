// Decompiled with JetBrains decompiler
// Type: DamageParticles
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Cysharp.Threading.Tasks;
using NuclearOption.Networking;
using System;
using System.Threading;
using UnityEngine;

#nullable disable
public class DamageParticles : MonoBehaviour
{
  [SerializeField]
  private float fireDamage;
  [SerializeField]
  private float fireRange;
  [SerializeField]
  private float fireLifetime;
  [SerializeField]
  private DamageParticles.SystemBehaviour[] systemBehaviours;
  [SerializeField]
  private Light fireLight;
  [SerializeField]
  private bool orientUpward;
  [SerializeField]
  private bool stopWhenParentCulled;
  private static readonly Collider[] fireColliders = new Collider[100];
  private UnitPart unitPart;
  private float time;
  private float speed;
  private float lightBaseIntensity;
  private float fireAnimationSeed;
  private float windSpeed;
  private bool detached;
  private Rigidbody rb;

  private void Awake()
  {
    this.enabled = (UnityEngine.Object) this.fireLight != (UnityEngine.Object) null;
    this.unitPart = this.transform.GetComponentInParent<UnitPart>();
    if (this.orientUpward)
      this.transform.rotation = Quaternion.identity;
    if ((UnityEngine.Object) this.fireLight != (UnityEngine.Object) null)
    {
      this.lightBaseIntensity = this.fireLight.intensity;
      this.fireAnimationSeed = (float) UnityEngine.Random.Range(0, 5);
    }
    if ((UnityEngine.Object) this.unitPart != (UnityEngine.Object) null && (UnityEngine.Object) this.unitPart.rb != (UnityEngine.Object) null && !this.unitPart.rb.isKinematic)
    {
      this.speed = (this.unitPart.rb.velocity - NetworkSceneSingleton<LevelInfo>.i.windVelocity).magnitude;
    }
    else
    {
      this.rb = this.transform.GetComponentInParent<Rigidbody>();
      if ((UnityEngine.Object) this.rb != (UnityEngine.Object) null && !this.rb.isKinematic)
      {
        this.speed = (this.rb.velocity - NetworkSceneSingleton<LevelInfo>.i.windVelocity).magnitude;
      }
      else
      {
        this.speed = this.windSpeed = NetworkSceneSingleton<LevelInfo>.i.windVelocity.magnitude;
        this.detached = true;
      }
      this.DetachTimer().Forget();
    }
    foreach (DamageParticles.SystemBehaviour systemBehaviour in this.systemBehaviours)
      systemBehaviour.Initialize(this.speed);
    this.StartSlowUpdateDelayed(1f, new Action(this.SlowUpdate));
  }

  public void ParentObjectCulled()
  {
    this.transform.SetParent(Datum.origin, true);
    this.unitPart = (UnitPart) null;
    this.enabled = false;
    this.detached = true;
    if ((UnityEngine.Object) this.fireLight != (UnityEngine.Object) null)
      UnityEngine.Object.Destroy((UnityEngine.Object) this.fireLight.gameObject);
    if (this.stopWhenParentCulled)
    {
      foreach (DamageParticles.SystemBehaviour systemBehaviour in this.systemBehaviours)
        systemBehaviour.Stop();
    }
    else
    {
      RaycastHit hitInfo;
      if (Physics.Linecast(this.transform.position + Vector3.up * 10f, this.transform.position - Vector3.up * 30f, out hitInfo, 64 /*0x40*/))
      {
        this.transform.position = hitInfo.point;
        foreach (DamageParticles.SystemBehaviour systemBehaviour in this.systemBehaviours)
          systemBehaviour.Update(this.time, 0.0f);
        this.speed = this.windSpeed;
      }
      else
      {
        foreach (DamageParticles.SystemBehaviour systemBehaviour in this.systemBehaviours)
          systemBehaviour.Stop();
      }
    }
  }

  private async UniTask DetachTimer()
  {
    DamageParticles damageParticles = this;
    CancellationToken cancel = damageParticles.destroyCancellationToken;
    await UniTask.Delay(1500);
    if (cancel.IsCancellationRequested)
    {
      cancel = new CancellationToken();
    }
    else
    {
      damageParticles.ParentObjectCulled();
      cancel = new CancellationToken();
    }
  }

  private void SlowUpdate()
  {
    if ((double) this.transform.position.y < (double) Datum.LocalSeaY)
    {
      foreach (DamageParticles.SystemBehaviour systemBehaviour in this.systemBehaviours)
        systemBehaviour.Stop();
      this.ParentObjectCulled();
      UnityEngine.Object.Destroy((UnityEngine.Object) this.gameObject, 45f);
      if (!((UnityEngine.Object) this.fireLight != (UnityEngine.Object) null))
        return;
      this.fireLifetime = 0.0f;
      this.fireDamage = 0.0f;
      this.enabled = false;
      UnityEngine.Object.Destroy((UnityEngine.Object) this.fireLight.gameObject);
    }
    else
    {
      ++this.time;
      if ((UnityEngine.Object) this.rb != (UnityEngine.Object) null)
        this.speed = (this.rb.velocity - NetworkSceneSingleton<LevelInfo>.i.windVelocity).magnitude;
      else if (!this.detached)
        this.speed = (this.unitPart.rb.velocity - NetworkSceneSingleton<LevelInfo>.i.windVelocity).magnitude;
      bool flag = false;
      foreach (DamageParticles.SystemBehaviour systemBehaviour in this.systemBehaviours)
      {
        flag = flag || systemBehaviour.IsActive();
        systemBehaviour.Update(this.time, this.speed);
      }
      if (!flag)
        UnityEngine.Object.Destroy((UnityEngine.Object) this.gameObject);
      if ((double) this.fireLifetime <= 0.0)
        return;
      if ((double) this.time > (double) this.fireLifetime)
      {
        this.fireLifetime = 0.0f;
        this.fireDamage = 0.0f;
        this.enabled = false;
        if ((UnityEngine.Object) this.fireLight != (UnityEngine.Object) null)
          UnityEngine.Object.Destroy((UnityEngine.Object) this.fireLight.gameObject);
      }
      if ((double) this.fireDamage <= 0.0 || !NetworkManagerNuclearOption.i.Server.Active)
        return;
      int num = Physics.OverlapSphereNonAlloc(this.transform.position, this.fireRange, DamageParticles.fireColliders);
      for (int index = 0; index < num; ++index)
      {
        IDamageable component;
        if (DamageParticles.fireColliders[index].TryGetComponent<IDamageable>(out component))
          component.TakeDamage(0.0f, 0.0f, 1f, this.fireDamage, 0.0f, PersistentID.None);
      }
    }
  }

  private void Update()
  {
    this.fireLight.intensity = this.lightBaseIntensity * Mathf.PerlinNoise1D(this.fireAnimationSeed + Time.timeSinceLevelLoad * 3f);
  }

  [Serializable]
  private class SystemBehaviour
  {
    [SerializeField]
    private ParticleSystem system;
    [SerializeField]
    [Tooltip("Seconds of particle main duration spent fading out")]
    private float fadeDuration;
    [SerializeField]
    private float increaseRateWithSpeed;
    [SerializeField]
    private float reduceOpacityWithSpeed;
    [SerializeField]
    private float reduceLifeWithSpeed;
    [SerializeField]
    private bool globalSim = true;
    private float fadeOpacity;
    private float baseRate;
    private float duration;
    private ParticleSystem.MainModule main;
    private ParticleSystem.EmissionModule emission;
    private float lowerLifetime;
    private float upperLifetime;
    private Color upperColor;
    private Color lowerColor;
    private bool active;
    private bool updating;

    public void Initialize(float speed)
    {
      this.active = true;
      this.main = this.system.main;
      this.fadeOpacity = 1f;
      this.duration = this.main.duration - this.fadeDuration;
      if (this.main.startColor.mode == ParticleSystemGradientMode.TwoColors)
      {
        this.upperColor = this.main.startColor.colorMax;
        this.lowerColor = this.main.startColor.colorMin;
      }
      else
        this.upperColor = this.lowerColor = this.main.startColor.color;
      this.lowerLifetime = this.main.startLifetime.constantMin;
      this.upperLifetime = this.main.startLifetime.constantMax;
      this.emission = this.system.emission;
      this.baseRate = this.emission.rateOverTime.constant;
      if (this.globalSim)
      {
        this.main.simulationSpace = ParticleSystemSimulationSpace.Custom;
        this.main.customSimulationSpace = Datum.origin;
      }
      this.updating = (double) this.fadeDuration > 0.0 || (double) this.increaseRateWithSpeed > 0.0 || (double) this.reduceOpacityWithSpeed > 0.0 || (double) this.reduceLifeWithSpeed > 0.0;
      this.Update(0.0f, speed);
    }

    public void Stop() => this.system.Stop();

    public void Update(float time, float speed)
    {
      if (!this.updating)
        return;
      if ((double) time > (double) this.duration)
      {
        this.fadeOpacity -= 1f / Mathf.Max(this.fadeDuration, 0.5f);
        if ((double) this.fadeOpacity <= 0.0)
          this.updating = false;
      }
      this.emission.rateOverTime = (ParticleSystem.MinMaxCurve) (this.baseRate + speed * this.increaseRateWithSpeed);
      float num1 = 1f / Mathf.Max(speed * this.reduceLifeWithSpeed, 1f);
      this.main.startLifetime = new ParticleSystem.MinMaxCurve(this.lowerLifetime * num1, this.upperLifetime * num1);
      float num2 = (float) (1.0 / (1.0 + (double) this.reduceOpacityWithSpeed * (double) speed * 0.0099999997764825821));
      this.main.startColor = new ParticleSystem.MinMaxGradient(this.lowerColor * new Color(1f, 1f, 1f, this.fadeOpacity * num2), this.upperColor * new Color(1f, 1f, 1f, this.fadeOpacity * num2));
    }

    public bool IsActive()
    {
      if (!this.active)
        return false;
      this.active = this.system.particleCount > 0;
      return this.active;
    }
  }
}
