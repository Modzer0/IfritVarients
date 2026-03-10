// Decompiled with JetBrains decompiler
// Type: Shockwave
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.Networking;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

#nullable disable
public class Shockwave : MonoBehaviour
{
  [SerializeField]
  private float yieldKilotons;
  [SerializeField]
  private float maxOverpressure;
  private float blastPower;
  private float blastRadius;
  private float blastPropagation;
  [SerializeField]
  private GameObject groundDecal;
  [SerializeField]
  private GameObject waterDecal;
  private DecalProjector decalProjector;
  private float dustOpacity = 1f;
  private float cloudAlpha = 1f;
  [SerializeField]
  private GameObject vaporCloud;
  private Material vaporCloudMat;
  [SerializeField]
  private Light vaporCloudEmissiveLight;
  [SerializeField]
  private float vaporCloudEmissiveFactor;
  [SerializeField]
  private AnimationCurve vaporCloudAlpha;
  [SerializeField]
  private float vaporCloudDetailScale;
  private float blastTime;
  private float overpressure;
  private PersistentID ownerID;
  private List<Shockwave.InfluencedObject> influencedObjects = new List<Shockwave.InfluencedObject>();
  private static int id_decalSize = Shader.PropertyToID("_decalSize");
  private static int id_opacity = Shader.PropertyToID("_opacity");
  private static int id_shockwaveExpansion = Shader.PropertyToID("_shockwaveExpansion");
  private static int id_ShockwaveAlpha = Shader.PropertyToID("_ShockwaveAlpha");
  private static int id_Emission = Shader.PropertyToID("_Emission");
  private static int id_Size = Shader.PropertyToID("_Size");
  private static int id_ShockwaveSoftness = Shader.PropertyToID("_ShockwaveSoftness");

  public void SetOwner(PersistentID ownerID, float yield)
  {
    this.ownerID = ownerID;
    this.yieldKilotons = yield;
  }

  private void Start()
  {
    this.dustOpacity = 1f;
    this.blastPower = Mathf.Pow(this.yieldKilotons * 1000000f, 0.3333f);
    this.blastRadius = this.blastPower * 13f;
    this.blastPropagation = this.blastPower * 0.5f;
    if ((Object) this.vaporCloud != (Object) null)
      this.vaporCloudMat = this.vaporCloud.GetComponent<UnityEngine.Renderer>().material;
    if ((Object) this.groundDecal != (Object) null)
    {
      RaycastHit hitInfo;
      if (Physics.Linecast(this.transform.position + this.blastRadius * 0.5f * Vector3.up, this.transform.position - this.blastRadius * 0.5f * Vector3.up, out hitInfo, 64 /*0x40*/))
      {
        this.groundDecal.transform.SetParent(Datum.origin);
        this.groundDecal.transform.rotation = Quaternion.LookRotation(Vector3.down);
        this.groundDecal.transform.position = hitInfo.point;
        this.decalProjector = this.groundDecal.GetComponent<DecalProjector>();
        this.decalProjector.size = new Vector3(this.blastRadius * 2f, this.blastRadius * 2f, this.blastRadius * 0.3f);
        this.decalProjector.material = new Material(this.decalProjector.material);
        this.decalProjector.material.SetFloat(Shockwave.id_decalSize, this.blastRadius * 2f);
        this.decalProjector.material.SetFloat(Shockwave.id_opacity, 1f);
      }
      else
        Object.Destroy((Object) this.groundDecal);
    }
    if ((Object) this.waterDecal != (Object) null && (double) this.transform.position.y < (double) Datum.LocalSeaY + (double) this.blastRadius * 0.5)
      this.waterDecal.SetActive(true);
    if ((double) this.yieldKilotons < 0.00019999999494757503)
      return;
    foreach (Collider collider in Physics.OverlapSphere(this.transform.position, this.blastRadius * 2f))
    {
      Shockwave.InfluencedObject influencedObject = new Shockwave.InfluencedObject(collider);
      if (influencedObject.IsInteractable())
        this.influencedObjects.Add(influencedObject);
    }
    if ((double) this.yieldKilotons <= 0.01)
      return;
    TerrainScatter.i.ClearScatters(this.transform.GlobalPosition(), this.blastRadius * 0.5f);
  }

  private void Update()
  {
    this.blastPropagation += 340f * Time.deltaTime;
    this.blastTime += Time.deltaTime;
    if ((Object) this.groundDecal != (Object) null && (Object) this.decalProjector != (Object) null)
      this.decalProjector.material.SetFloat(Shockwave.id_shockwaveExpansion, 1f * this.blastRadius / this.blastPropagation);
    if ((double) this.blastPropagation > (double) this.blastRadius)
    {
      this.dustOpacity -= Time.deltaTime * 0.1f;
      if ((Object) this.decalProjector != (Object) null)
        this.decalProjector.material.SetFloat(Shockwave.id_opacity, this.dustOpacity);
      if ((double) this.dustOpacity <= 0.0)
      {
        Object.Destroy((Object) this.groundDecal);
        Object.Destroy((Object) this);
      }
      if ((Object) this.vaporCloud != (Object) null && (double) this.cloudAlpha <= 0.0)
        Object.Destroy((Object) this.vaporCloud);
    }
    float num1 = Mathf.Max(this.blastPropagation / this.blastPower, 1f);
    float overpressure = (float) (25000.0 / ((double) num1 * (double) num1 * (double) num1));
    if ((double) overpressure > 0.5)
    {
      for (int index = this.influencedObjects.Count - 1; index >= 0; --index)
      {
        if (this.influencedObjects[index].HasShockwaveReached(this.transform.position, this.blastPropagation, overpressure, this.yieldKilotons * 1000000f, this.blastPower, this.ownerID))
          this.influencedObjects.RemoveAt(index);
      }
    }
    else
      this.influencedObjects.Clear();
    if (!((Object) this.vaporCloud != (Object) null))
      return;
    this.vaporCloud.transform.LookAt(SceneSingleton<CameraStateManager>.i.transform.position);
    this.vaporCloud.transform.localScale = Vector3.one * this.blastPropagation;
    this.cloudAlpha = this.vaporCloudAlpha.Evaluate(this.blastTime);
    float num2 = !((Object) this.vaporCloudEmissiveLight != (Object) null) || !this.vaporCloudEmissiveLight.isActiveAndEnabled ? 0.0f : this.vaporCloudEmissiveLight.intensity * this.vaporCloudEmissiveFactor;
    this.vaporCloudMat.SetFloat(Shockwave.id_ShockwaveAlpha, this.cloudAlpha);
    if ((double) num2 > 0.0)
      this.vaporCloudMat.SetFloat(Shockwave.id_Emission, num2);
    this.vaporCloudMat.SetFloat(Shockwave.id_Size, this.blastPropagation / this.vaporCloudDetailScale);
    this.vaporCloudMat.SetFloat(Shockwave.id_ShockwaveSoftness, 4f / this.vaporCloud.transform.localScale.x);
    if ((double) this.cloudAlpha > 0.0)
      return;
    Object.Destroy((Object) this.vaporCloud);
  }

  private struct InfluencedObject
  {
    private Collider collider;
    private float averageRadius;
    private Rigidbody rb;
    private IDamageable damageable;

    public InfluencedObject(Collider collider)
    {
      this.collider = collider;
      this.rb = collider.attachedRigidbody;
      this.damageable = collider.gameObject.GetComponent<IDamageable>();
      Bounds bounds = collider.bounds;
      double x = (double) bounds.extents.x;
      bounds = collider.bounds;
      double y = (double) bounds.extents.y;
      double num = x + y;
      bounds = collider.bounds;
      double z = (double) bounds.extents.z;
      this.averageRadius = (float) ((num + z) * 0.33300000429153442);
    }

    public bool IsInteractable() => (Object) this.rb != (Object) null || this.damageable != null;

    public bool HasShockwaveReached(
      Vector3 blastOrigin,
      float blastPropagation,
      float overpressure,
      float blastYield,
      float blastPower,
      PersistentID ownerID)
    {
      if ((Object) this.collider == (Object) null)
        return true;
      blastPropagation += this.averageRadius;
      if ((double) Vector3.SqrMagnitude(this.collider.bounds.center - blastOrigin) > (double) blastPropagation * (double) blastPropagation)
        return false;
      float num1 = 3.14159274f * this.averageRadius * this.averageRadius;
      float num2 = (Object) this.rb != (Object) null ? this.rb.mass : 0.0f;
      float num3 = Mathf.Clamp(Mathf.Max(blastPower * blastPower, blastPropagation * blastPropagation) / num1, 0.0f, 10f);
      if (this.damageable != null)
      {
        ArmorProperties armorProperties = this.damageable.GetArmorProperties();
        num3 /= (float) (1.0 + (double) armorProperties.blastArmor * 0.019999999552965164);
        Unit unit = this.damageable.GetUnit();
        num2 = this.damageable.GetMass();
        if (unit is Building building)
          building.RegisterRecentExplosion(blastOrigin.ToGlobalPosition(), blastYield);
        if (NetworkManagerNuclearOption.i.Server.Active)
          this.damageable.TakeDamage(0.0f, overpressure, Mathf.Clamp01(num3), 0.0f, 0.0f, ownerID);
      }
      if ((Object) this.rb != (Object) null && (double) num2 > 0.0)
      {
        float num4 = Mathf.Min((float) ((double) num1 * (double) Mathf.Clamp01(num3) * (double) overpressure * 25.0), blastYield * 200f, 60f * num2);
        this.rb.AddForceAtPosition((this.collider.bounds.center - blastOrigin).normalized * num4, this.collider.bounds.center, ForceMode.Impulse);
      }
      return true;
    }
  }
}
