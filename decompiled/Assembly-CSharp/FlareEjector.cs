// Decompiled with JetBrains decompiler
// Type: FlareEjector
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
public class FlareEjector : Countermeasure
{
  [SerializeField]
  private GameObject flarePrefab;
  [SerializeField]
  private float ejectionVelocity;
  [SerializeField]
  private float ejectionVelocityVariance;
  [SerializeField]
  private FlareEjector.EjectionPoint[] ejectionPoints;
  [SerializeField]
  private FlareEjector.FlareDoor[] flareDoors;
  [Tooltip("in seconds")]
  [SerializeField]
  private float ejectionInterval;
  [SerializeField]
  private int ejectionGrouping = 2;
  [SerializeField]
  private AudioClip ejectionSound;
  [SerializeField]
  private float ejectionVolume;
  private int ejectionIndex;
  private int maxAmmo;
  private float lastEjectionTime;

  protected override void Awake()
  {
    this.maxAmmo = this.ammo;
    this.enabled = false;
    base.Awake();
  }

  public override List<string> GetThreatTypes()
  {
    if (this.threatTypes == null)
      this.threatTypes = new List<string>() { "IR" };
    return this.threatTypes;
  }

  public override void Fire()
  {
    if (this.maxAmmo == -1)
      this.maxAmmo = this.ammo;
    if (this.aircraft.disabled || (double) Time.timeSinceLevelLoad - (double) this.lastEjectionTime < (double) this.ejectionInterval)
      return;
    this.lastEjectionTime = Time.timeSinceLevelLoad;
    this.aircraft.RequestRearm();
    for (int index = 0; index < this.ejectionGrouping; ++index)
    {
      if (this.ejectionIndex >= this.ejectionPoints.Length)
        this.ejectionIndex = 0;
      if (this.ammo > 0 && (UnityEngine.Object) this.ejectionPoints[this.ejectionIndex].transform != (UnityEngine.Object) null)
        this.EjectFlare(this.aircraft, this.ejectionPoints[this.ejectionIndex]).Forget();
      ++this.ejectionIndex;
    }
    if (!GameManager.IsLocalAircraft(this.aircraft))
      return;
    this.UpdateHUD();
  }

  private async UniTask EjectFlare(Aircraft aircraft, FlareEjector.EjectionPoint ejectionPoint)
  {
    FlareEjector flareEjector = this;
    await UniTask.WaitForFixedUpdate();
    flareEjector.enabled = true;
    GameObject gameObject = NetworkSceneSingleton<Spawner>.i.SpawnLocal(flareEjector.flarePrefab, Datum.origin);
    gameObject.transform.position = ejectionPoint.transform.position;
    IRFlare component = gameObject.GetComponent<IRFlare>();
    Vector3 velocity = ejectionPoint.part.rb.velocity;
    Aircraft aircraft1 = aircraft;
    Transform transform = ejectionPoint.transform;
    Vector3 launchVelocity = velocity + ejectionPoint.transform.forward * (flareEjector.ejectionVelocity + (float) UnityEngine.Random.Range(-1, 1) * flareEjector.ejectionVelocityVariance * flareEjector.ejectionVelocity);
    component.LaunchFlare(aircraft1, transform, launchVelocity);
    --flareEjector.ammo;
    AudioSource audioSource = ejectionPoint.sound;
    if ((UnityEngine.Object) audioSource == (UnityEngine.Object) null)
    {
      audioSource = ejectionPoint.transform.gameObject.AddComponent<AudioSource>();
      ejectionPoint.sound = audioSource;
      audioSource.outputAudioMixerGroup = SoundManager.i.EffectsMixer;
      audioSource.clip = flareEjector.ejectionSound;
      audioSource.volume = flareEjector.ejectionVolume;
      audioSource.spatialBlend = 1f;
      audioSource.dopplerLevel = 0.0f;
      audioSource.spread = 5f;
      audioSource.maxDistance = 40f;
      audioSource.minDistance = 5f;
    }
    audioSource.pitch = UnityEngine.Random.Range(0.8f, 1.2f);
    audioSource.PlayOneShot(flareEjector.ejectionSound);
  }

  public override void Rearm(Aircraft aircraft, Unit rearmer)
  {
    if (this.ammo == this.maxAmmo)
      return;
    this.ammo = this.maxAmmo;
    if (!GameManager.IsLocalAircraft(aircraft))
      return;
    this.UpdateHUD();
    SceneSingleton<AircraftActionsReport>.i.ReportText("Flares rearmed by " + rearmer.unitName, 5f);
  }

  public override void UpdateHUD()
  {
    SceneSingleton<CombatHUD>.i.DisplayCountermeasures(this.displayName, this.displayImage, this.ammo);
  }

  private void Update()
  {
    foreach (FlareEjector.FlareDoor flareDoor in this.flareDoors)
    {
      if (!flareDoor.Animate(this.lastEjectionTime))
        this.enabled = false;
    }
  }

  public int GetAmmo() => this.ammo;

  public int GetMaxAmmo() => this.maxAmmo;

  [Serializable]
  private class FlareDoor
  {
    [SerializeField]
    private Transform rotator;
    [SerializeField]
    private float openAngle;
    [SerializeField]
    private float openSpeed;
    [SerializeField]
    private float openTime;
    [SerializeField]
    private float closeSpeed;
    private float openAmount;

    public bool Animate(float lastEjectionTime)
    {
      bool flag = true;
      if ((double) Time.timeSinceLevelLoad - (double) lastEjectionTime < (double) this.openTime)
      {
        this.openAmount += this.openSpeed * Time.deltaTime;
      }
      else
      {
        this.openAmount -= this.closeSpeed * Time.deltaTime;
        if ((double) this.openAmount < 0.0)
          flag = false;
      }
      this.openAmount = Mathf.Clamp01(this.openAmount);
      this.rotator.transform.localEulerAngles = new Vector3(0.0f, 0.0f, this.openAmount * this.openAngle);
      return flag;
    }
  }

  [Serializable]
  public class EjectionPoint
  {
    public UnitPart part;
    public Transform transform;
    [NonSerialized]
    public AudioSource sound;
  }
}
