// Decompiled with JetBrains decompiler
// Type: Downwash
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;

#nullable disable
public class Downwash : MonoBehaviour
{
  [SerializeField]
  private Unit aircraft;
  [SerializeField]
  private ParticleSystemForceField forceField;
  [SerializeField]
  private Downwash.DownwashSource[] downwashSources;
  [SerializeField]
  private Transform dustTransform;
  [SerializeField]
  private Transform waterSprayTransform;
  private ParticleSystem[] dustSystems;
  private ParticleSystem[] waterSystems;
  [SerializeField]
  private float downwashSpeed = 40f;
  [SerializeField]
  private float speedReducesRange = 1f;
  [SerializeField]
  private float downwashRange = 70f;
  private GlobalPosition positionTarget;
  private Vector3 normalTarget;
  private float forceFieldMaxStrength;
  private bool dustEffectPlaying;
  private bool waterEffectPlaying;

  private void Awake()
  {
    this.enabled = false;
    this.dustTransform.SetParent(Datum.origin);
    this.waterSprayTransform.SetParent(Datum.origin);
    this.waterSprayTransform.rotation = Quaternion.identity;
    if ((UnityEngine.Object) this.forceField != (UnityEngine.Object) null)
      this.forceFieldMaxStrength = this.forceField.directionY.constant;
    this.dustSystems = this.dustTransform.GetComponents<ParticleSystem>();
    this.waterSystems = this.waterSprayTransform.GetComponentsInChildren<ParticleSystem>();
    foreach (Downwash.DownwashSource downwashSource in this.downwashSources)
      downwashSource.Initialize();
    this.StartSlowUpdateDelayed(0.5f, new Action(this.CheckSurface));
  }

  private void UpdateSystems(bool hitWater, bool hitGround)
  {
    this.enabled = hitGround | hitWater;
    if (hitGround)
    {
      if (!this.dustEffectPlaying)
      {
        this.dustEffectPlaying = true;
        this.dustTransform.position = this.positionTarget.ToLocalPosition();
        foreach (ParticleSystem dustSystem in this.dustSystems)
          dustSystem.Play();
      }
    }
    else if (this.dustEffectPlaying)
    {
      this.dustEffectPlaying = false;
      foreach (ParticleSystem dustSystem in this.dustSystems)
        dustSystem.Stop();
    }
    if (hitWater)
    {
      if (this.waterEffectPlaying)
        return;
      this.waterEffectPlaying = true;
      this.dustTransform.position = this.positionTarget.ToLocalPosition();
      foreach (ParticleSystem waterSystem in this.waterSystems)
        waterSystem.Play();
    }
    else
    {
      if (!this.waterEffectPlaying)
        return;
      this.waterEffectPlaying = false;
      foreach (ParticleSystem waterSystem in this.waterSystems)
        waterSystem.Stop();
    }
  }

  private void CheckSurface()
  {
    if ((UnityEngine.Object) this.aircraft == (UnityEngine.Object) null)
    {
      UnityEngine.Object.Destroy((UnityEngine.Object) this.dustTransform.gameObject);
      UnityEngine.Object.Destroy((UnityEngine.Object) this.waterSprayTransform.gameObject);
    }
    else
    {
      int num1 = 0;
      float num2 = 0.0f;
      Vector3 zero = Vector3.zero;
      bool hitWater1 = false;
      bool hitGround1 = false;
      float range = this.downwashRange - this.speedReducesRange * this.aircraft.speed;
      this.normalTarget = Vector3.zero;
      if ((double) this.aircraft.radarAlt < (double) range)
      {
        foreach (Downwash.DownwashSource downwashSource in this.downwashSources)
        {
          Vector3 contactPoint;
          Vector3 contactNormal;
          bool hitWater2;
          bool hitGround2;
          float thrustRatio;
          downwashSource.GetSurfaceContact(range, this.aircraft.radarAlt, out contactPoint, out contactNormal, out hitWater2, out hitGround2, out thrustRatio);
          num2 += thrustRatio;
          if (hitWater2 | hitGround2)
          {
            ++num1;
            zero += contactPoint;
            this.normalTarget += contactNormal;
            hitWater1 = hitWater2 | hitWater1;
            hitGround1 = hitGround2 | hitGround1;
          }
        }
      }
      float num3 = num2 * (float) (1 / this.downwashSources.Length);
      if ((UnityEngine.Object) this.forceField != (UnityEngine.Object) null)
        this.forceField.directionY = new ParticleSystem.MinMaxCurve(this.forceFieldMaxStrength * num3);
      if (num1 > 0)
      {
        float num4 = 1f / (float) num1;
        Vector3 position = zero * num4;
        this.normalTarget *= num4;
        this.positionTarget = position.ToGlobalPosition();
      }
      this.UpdateSystems(hitWater1, hitGround1);
    }
  }

  private void Update()
  {
    if ((UnityEngine.Object) this.aircraft == (UnityEngine.Object) null)
      return;
    float num = this.downwashSpeed / Mathf.Max(this.aircraft.radarAlt, 1f);
    this.positionTarget += new Vector3(this.aircraft.rb.velocity.x, 0.0f, this.aircraft.rb.velocity.z) * Time.deltaTime;
    this.dustTransform.SetPositionAndRotation(Vector3.Lerp(this.dustTransform.position, this.positionTarget.ToLocalPosition(), num * Time.deltaTime), Quaternion.LookRotation(Vector3.Lerp(this.dustTransform.forward, this.normalTarget, Time.deltaTime)));
    this.waterSprayTransform.position = new Vector3(this.dustTransform.position.x, Datum.LocalSeaY, this.dustTransform.position.z);
  }

  [Serializable]
  private class DownwashSource
  {
    [SerializeField]
    private GameObject thrustSourceObject;
    [SerializeField]
    private Transform castTransform;
    private IThrustSource thrustSource;
    [SerializeField]
    private float maxThrust;

    public void Initialize()
    {
      this.thrustSource = this.thrustSourceObject.GetComponent<IThrustSource>();
    }

    public void GetSurfaceContact(
      float range,
      float radarAlt,
      out Vector3 contactPoint,
      out Vector3 contactNormal,
      out bool hitWater,
      out bool hitGround,
      out float thrustRatio)
    {
      hitWater = false;
      hitGround = false;
      thrustRatio = this.thrustSource.GetThrust() / this.maxThrust;
      float num = range * thrustRatio;
      contactPoint = this.castTransform.position - this.castTransform.forward * num;
      contactNormal = Vector3.up;
      if ((double) thrustRatio <= 0.10000000149011612 || (double) radarAlt >= (double) num || (double) this.castTransform.position.y <= (double) Datum.LocalSeaY)
        return;
      float enter;
      if (Datum.WaterPlane().Raycast(new Ray(this.castTransform.position, -this.castTransform.forward), out enter) && (double) enter < (double) num && (double) enter > 0.0)
      {
        contactPoint = this.castTransform.position - enter * this.castTransform.forward;
        hitWater = true;
      }
      RaycastHit hitInfo;
      if (!Physics.Linecast(this.castTransform.position, contactPoint, out hitInfo, 2112) || (double) hitInfo.point.y <= (double) Datum.LocalSeaY - 0.10000000149011612)
        return;
      contactPoint = hitInfo.point;
      contactNormal = hitInfo.normal;
      hitGround = (UnityEngine.Object) hitInfo.collider.sharedMaterial == (UnityEngine.Object) GameAssets.i.terrainMaterial;
      hitWater = false;
    }
  }
}
