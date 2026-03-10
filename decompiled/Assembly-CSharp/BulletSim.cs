// Decompiled with JetBrains decompiler
// Type: BulletSim
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.Networking;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
public class BulletSim : MonoBehaviour
{
  private BulletSim.ImpactEffect[] impactEffects;
  private Unit owner;
  private bool visualOnly;
  private Gun gun;
  private WeaponInfo weaponInfo;
  public Turret turret;
  private float noBulletsTimer;
  private float tracerSize;
  private GameObject debugVis;
  public Material tracerMeshMaterial;
  public Material tracerParticleMaterial;
  private List<BulletSim.Bullet> bullets = new List<BulletSim.Bullet>();

  public static BulletSim Create(Unit owner, Gun gun, Turret turret)
  {
    BulletSim bulletSim = new GameObject().AddComponent<BulletSim>();
    bulletSim.gameObject.name = "bulletSim";
    bulletSim.gameObject.transform.SetParent(Datum.origin);
    bulletSim.owner = owner;
    bulletSim.gun = gun;
    bulletSim.weaponInfo = gun.info;
    bulletSim.turret = turret;
    bulletSim.visualOnly = gun.ForceServerAuthority ? !owner.IsServer : owner.remoteSim;
    bulletSim.impactEffects = new BulletSim.ImpactEffect[4];
    for (int index = 0; index < 4; ++index)
      bulletSim.impactEffects[index] = new BulletSim.ImpactEffect(gun.ImpactEffectsPrefabs[index]);
    return bulletSim;
  }

  public void AddBullet(
    Transform muzzle,
    Vector3 inheritedVelocity,
    float spread,
    float destructTimer,
    bool tracer,
    float tracerSize,
    Color tracerColor,
    float timeOffset,
    Unit target)
  {
    this.tracerSize = tracerSize;
    GameObject tracer1 = (GameObject) null;
    if (tracer)
    {
      tracer1 = Object.Instantiate<GameObject>(GameAssets.i.tracer, muzzle.position, muzzle.rotation);
      tracer1.transform.SetParent(this.transform);
      Renderer component = tracer1.GetComponent<Renderer>();
      if ((Object) this.tracerMeshMaterial != (Object) null)
      {
        component.material = this.tracerMeshMaterial;
      }
      else
      {
        component.material.SetColor("_EmissionColor", tracerColor);
        this.tracerMeshMaterial = component.material;
      }
      tracer1.transform.localScale = new Vector3(1f, 1f, inheritedVelocity.magnitude + this.gun.info.muzzleVelocity * 0.0083f);
    }
    if (NetworkManagerNuclearOption.i.Server.Active)
      HitValidator.LogFiring(this.owner.persistentID, muzzle.transform.position - Datum.origin.position, inheritedVelocity);
    bool proximityFuse = (Object) target != (Object) null && (double) target.definition.armorTier < 2.0;
    Vector3 vector3 = Random.insideUnitSphere.normalized * Random.Range(0.0f, spread * 1.2f);
    BulletSim.Bullet bullet = new BulletSim.Bullet(inheritedVelocity + vector3, tracer1, destructTimer, proximityFuse, (double) this.weaponInfo.blastDamage > 0.0, target);
    this.bullets.Add(bullet);
    bullet.TrajectoryTrace(muzzle, this.weaponInfo, this.owner, this.impactEffects, tracerSize, this.visualOnly, Time.deltaTime + timeOffset);
    if (!((Object) this.turret != (Object) null))
      return;
    this.turret.SetObservedBullet(bullet);
  }

  private void FixedUpdate()
  {
    if (this.bullets.Count > 0)
    {
      this.noBulletsTimer = 0.0f;
      for (int index = this.bullets.Count - 1; index >= 0; --index)
      {
        BulletSim.Bullet bullet = this.bullets[index];
        if (bullet.active)
        {
          bullet.TrajectoryTrace((Transform) null, this.weaponInfo, this.owner, this.impactEffects, this.tracerSize, this.visualOnly, Time.deltaTime);
        }
        else
        {
          int num = (Object) bullet.tracer != (Object) null ? 1 : 0;
          Object.Destroy((Object) bullet.tracer);
          this.bullets.RemoveAt(index);
        }
      }
    }
    else
    {
      this.noBulletsTimer += Time.deltaTime;
      if ((double) this.noBulletsTimer <= 25.0)
        return;
      foreach (BulletSim.ImpactEffect impactEffect in this.impactEffects)
        impactEffect.Remove();
      Object.Destroy((Object) this.gameObject);
    }
  }

  private void Update()
  {
    if ((double) Time.timeScale >= 1.0)
      return;
    for (int index = this.bullets.Count - 1; index >= 0; --index)
    {
      if ((Object) this.bullets[index].tracer != (Object) null)
        this.bullets[index].tracer.transform.position += this.bullets[index].velocity * Time.deltaTime;
    }
  }

  public class Bullet
  {
    public bool active;
    public bool impacted;
    public Vector3 velocity;
    public GlobalPosition position;
    public GameObject tracer;
    public float destructTimer;
    public bool explosive;
    public bool proximityFuse;
    public float reliability;
    public Unit target;

    public Bullet(
      Vector3 velocity,
      GameObject tracer,
      float destructTimer,
      bool proximityFuse,
      bool explosive,
      Unit target)
    {
      this.active = true;
      this.velocity = velocity;
      this.tracer = tracer;
      this.destructTimer = destructTimer;
      this.explosive = explosive;
      this.target = target;
      this.proximityFuse = proximityFuse;
      this.reliability = Random.value;
      if (!((Object) target != (Object) null) || (double) target.definition.armorTier <= 2.0)
        return;
      this.proximityFuse = false;
    }

    public void TrajectoryTrace(
      Transform muzzle,
      WeaponInfo info,
      Unit owner,
      BulletSim.ImpactEffect[] impactEffects,
      float tracerSize,
      bool visualOnly,
      float deltaTime)
    {
      Vector3 start = (Object) muzzle != (Object) null ? muzzle.position : this.position.ToLocalPosition();
      if ((Object) muzzle != (Object) null)
        this.position = muzzle.position.ToGlobalPosition();
      this.velocity.y -= 9.81f * deltaTime;
      this.velocity -= this.velocity.sqrMagnitude * info.dragCoef * deltaTime * this.velocity.normalized / info.muzzleVelocity;
      this.destructTimer -= deltaTime;
      GlobalPosition globalPosition1 = this.position + this.velocity * deltaTime;
      RaycastHit hitInfo;
      if (Physics.Linecast(start, start + this.velocity * 1.5f * deltaTime, out hitInfo, -8193))
      {
        this.impacted = true;
        this.position = hitInfo.point.ToGlobalPosition() - this.velocity.normalized * 0.1f;
        if ((double) hitInfo.point.y < (double) Datum.LocalSeaY || (Object) hitInfo.collider.sharedMaterial == (Object) GameAssets.i.WaterMaterial)
        {
          globalPosition1.y = 0.0f;
          impactEffects[2].Play(this.position.ToLocalPosition(), Quaternion.identity);
          this.active = false;
        }
        else
        {
          IDamageable component = hitInfo.collider.gameObject.GetComponent<IDamageable>();
          Vector3 vector3 = this.velocity;
          ImpactType index = (Object) hitInfo.collider.sharedMaterial == (Object) GameAssets.i.terrainMaterial ? ImpactType.GroundHits : ImpactType.ArmorHits;
          impactEffects[(int) index].Play(hitInfo.point, Quaternion.LookRotation(Vector3.Reflect(this.velocity, hitInfo.normal), Vector3.up));
          if (component != null)
          {
            Unit unit = component.GetUnit();
            float pierceDamage = (float) ((double) info.pierceDamage * (double) this.velocity.sqrMagnitude / ((double) info.muzzleVelocity * (double) info.muzzleVelocity));
            if (!visualOnly)
            {
              if ((Object) unit != (Object) null)
              {
                Vector3 relativePos = unit.transform.InverseTransformPoint(hitInfo.point - this.velocity.normalized * 0.05f);
                owner.RegisterHit(unit, relativePos, this.velocity, info);
                if (GameManager.IsLocalAircraft(owner))
                  SceneSingleton<CombatHUD>.i.DisplayHit(this.position, unit);
              }
              else
                component.TakeDamage(pierceDamage, info.blastDamage, 1f, 0.0f, 0.0f, PersistentID.None);
            }
            this.active = (double) pierceDamage < (double) component.GetArmorProperties().pierceArmor;
          }
          if ((bool) (Object) this.tracer)
          {
            if ((double) Mathf.Abs(Vector3.Dot(this.velocity.normalized, hitInfo.normal)) < (double) Random.Range(0.0f, 0.5f))
              vector3 = Vector3.Reflect(this.velocity, hitInfo.normal) * 0.3f + this.velocity.magnitude * 0.05f * Random.insideUnitSphere;
            else
              this.active = false;
          }
          this.active = this.active && (bool) (Object) this.tracer && (double) vector3.sqrMagnitude > 625.0;
          this.explosive = false;
          this.velocity = vector3;
        }
      }
      else if ((double) globalPosition1.y < 0.0)
      {
        this.impacted = true;
        this.position = new GlobalPosition(globalPosition1.x, 0.0f, globalPosition1.z);
        impactEffects[2].Play(this.position.ToLocalPosition(), Quaternion.identity);
        this.active = false;
      }
      else
      {
        if ((Object) this.tracer != (Object) null && (Object) SceneSingleton<CameraStateManager>.i != (Object) null)
        {
          float num1 = Mathf.Sqrt(FastMath.Distance(this.position, SceneSingleton<CameraStateManager>.i.transform.position.ToGlobalPosition()));
          float num2 = (float) ((double) tracerSize * 0.44999998807907104 + (double) num1 * (1.0 / 500.0) * (double) Camera.main.fieldOfView);
          this.tracer.transform.position = this.position.ToLocalPosition();
          float magnitude = (this.velocity - SceneSingleton<CameraStateManager>.i.cameraVelocity).magnitude;
          this.tracer.transform.localScale = new Vector3(num2, num2, magnitude * deltaTime);
          if ((double) magnitude > 0.10000000149011612)
            this.tracer.transform.rotation = Quaternion.LookRotation(this.velocity - SceneSingleton<CameraStateManager>.i.cameraVelocity);
        }
        if (this.proximityFuse && (Object) this.target != (Object) null && FastMath.InRange(this.target.GlobalPosition(), this.position, 100f))
        {
          if ((double) this.reliability < 0.10000000149011612 && FastMath.OutOfRange(this.position, owner.GlobalPosition(), 500f))
          {
            if (NetworkManagerNuclearOption.i.Server.Active)
              DamageEffects.BlastFrag(info.blastDamage, this.position.ToLocalPosition(), owner.persistentID, PersistentID.None);
            impactEffects[3].Play(this.position.ToLocalPosition(), Quaternion.LookRotation(this.velocity));
            this.active = false;
            return;
          }
          GlobalPosition globalPosition2 = this.position + this.velocity * deltaTime;
          if ((double) Vector3.Dot(this.velocity, this.target.GlobalPosition() - globalPosition2) < 0.0)
          {
            GlobalPosition position = this.position + Vector3.Project(this.target.GlobalPosition() - this.position, this.velocity) + Random.Range(-1f, 1f) * this.target.maxRadius * this.velocity.normalized;
            this.position = Physics.Linecast(this.position.ToLocalPosition(), position.ToLocalPosition(), out RaycastHit _, -8193) ? hitInfo.point.ToGlobalPosition() : position;
            if (NetworkManagerNuclearOption.i.Server.Active)
              DamageEffects.BlastFrag(info.blastDamage, this.position.ToLocalPosition(), owner.persistentID, PersistentID.None);
            impactEffects[3].Play(this.position.ToLocalPosition(), Quaternion.LookRotation(this.velocity));
            this.active = false;
            return;
          }
        }
        if ((double) this.destructTimer <= 0.0 || (double) this.velocity.sqrMagnitude < (double) info.muzzleVelocity * (double) info.muzzleVelocity * 0.019999999552965164)
        {
          if (this.explosive)
            impactEffects[3].Play(this.position.ToLocalPosition(), Quaternion.LookRotation(this.velocity));
          this.active = false;
        }
        else
          this.position += this.velocity * deltaTime;
      }
    }

    public void Remove()
    {
      if (!((Object) this.tracer != (Object) null))
        return;
      Object.Destroy((Object) this.tracer);
    }
  }

  public class ImpactEffect
  {
    private GameObject gameObject;
    private ParticleSystem[] systems;
    private AudioSource source;

    public ImpactEffect(GameObject prefab)
    {
      if ((Object) prefab == (Object) null)
        return;
      this.gameObject = Object.Instantiate<GameObject>(prefab, Datum.origin);
      this.systems = this.gameObject.GetComponentsInChildren<ParticleSystem>();
      this.source = this.gameObject.GetComponent<AudioSource>();
    }

    public void Play(Vector3 position, Quaternion rotation)
    {
      if ((Object) this.gameObject == (Object) null)
        return;
      this.gameObject.transform.rotation = rotation;
      this.gameObject.SetActive(true);
      this.gameObject.transform.position = position;
      if ((Object) this.source != (Object) null)
      {
        this.source.pitch = Random.Range(0.9f, 1.1f);
        this.source.Play();
      }
      foreach (ParticleSystem system in this.systems)
        system.Play();
    }

    public void Remove()
    {
      if ((Object) this.gameObject == (Object) null)
        return;
      Object.Destroy((Object) this.gameObject);
    }
  }
}
