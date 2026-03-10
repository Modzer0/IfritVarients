// Decompiled with JetBrains decompiler
// Type: MountedCargo
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Cysharp.Threading.Tasks;
using NuclearOption.Networking;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

#nullable disable
public class MountedCargo : Weapon, IDamageable
{
  [SerializeField]
  private MountedCargo.RailDirection railDirection;
  [SerializeField]
  public UnitDefinition cargo;
  [SerializeField]
  private float railSpeed;
  [SerializeField]
  private float railDelay;
  [SerializeField]
  private AudioClip deploySound;
  [SerializeField]
  private float deployVolume;
  [SerializeField]
  private float pushDistance;
  [SerializeField]
  private float pushSpeed = 2f;
  [SerializeField]
  private ArmorProperties armorProperties;
  [SerializeField]
  private List<DamageEffect> damageEffects = new List<DamageEffect>();
  private float hitPoints = 100f;
  private UnitPart attachedPart;
  private BayDoor cargoDoor;
  private byte? id;
  private bool fired;
  private bool detached;
  private Vector3 railVector;
  private Vector3 mountedPosition;

  private void Awake()
  {
    this.enabled = false;
    if (this.railDirection == MountedCargo.RailDirection.Forward)
      this.railVector = new Vector3(0.0f, 0.0f, 1f);
    if (this.railDirection == MountedCargo.RailDirection.Backward)
      this.railVector = new Vector3(0.0f, 0.0f, -1f);
    if (this.railDirection != MountedCargo.RailDirection.Down)
      return;
    this.railVector = new Vector3(0.0f, -1f, 0.0f);
  }

  public override void Fire(
    Unit owner,
    Unit target,
    Vector3 inheritedVelocity,
    WeaponStation weaponStation,
    GlobalPosition aimpoint)
  {
    if (this.fired)
      return;
    if (this.hardpoint != null)
      this.hardpoint.SpringOpenBayDoors();
    this.fired = true;
    this.attachedUnit = owner;
    Aircraft aircraft = owner as Aircraft;
    aircraft.RequestRearm();
    if (aircraft.IsServer)
      aircraft.RpcLaunchMissile(weaponStation.Number, target, aimpoint);
    else if (aircraft.HasAuthority)
      aircraft.CmdLaunchMissile(weaponStation.Number, target, aimpoint);
    if (this.hardpoint.part.IsDetached())
      return;
    this.RailLaunch(owner, target).Forget();
  }

  public override void AttachToHardpoint(Aircraft aircraft, Hardpoint hardpoint, WeaponMount mount)
  {
    base.AttachToHardpoint(aircraft, hardpoint, mount);
    this.attachedPart = hardpoint.part;
    this.attachedPart.onPartDetached += new Action<UnitPart>(this.MountedCargo_OnPartDetached);
    this.attachedPart.onApplyDamage += new Action<UnitPart.OnApplyDamage>(this.MountedCargo_OnPartDamage);
    this.attachedUnit = hardpoint.part.parentUnit;
    this.mountedPosition = this.transform.localPosition;
    this.cargoDoor = hardpoint.GetCargoDoor();
    float prefabMass = this.cargo.unitPrefab.GetComponent<Unit>().GetPrefabMass();
    hardpoint.ModifyMass(prefabMass);
    hardpoint.ModifyDrag(mount.GetDragPerRound());
    hardpoint.ModifyRCS(mount.GetRCSPerRound());
    if (this.damageEffects.Count <= 0)
      return;
    this.id = new byte?(hardpoint.part.parentUnit.RegisterDamageable((IDamageable) this));
  }

  public void OnDestroy()
  {
    if (!this.fired && !this.detached)
      this.RemoveFromHardpoint();
    if (!this.id.HasValue)
      return;
    this.attachedUnit.DeregisterDamageable((int) this.id.Value);
  }

  private void RemoveFromHardpoint()
  {
    if (this.hardpoint == null)
      return;
    this.hardpoint.ModifyMass(-this.cargo.unitPrefab.GetComponent<Unit>().GetPrefabMass());
    this.hardpoint.ModifyDrag(-this.mount.GetDragPerRound());
    this.hardpoint.ModifyRCS(-this.mount.GetRCSPerRound());
  }

  public override float GetMass() => this.fired ? 0.0f : this.cargo.mass;

  public override void Rearm()
  {
    if (!this.fired)
      return;
    this.transform.localPosition = this.mountedPosition;
    this.gameObject.SetActive(true);
    this.fired = false;
    if (this.hardpoint != null)
    {
      this.hardpoint.ModifyMass(this.info.massPerRound);
      this.hardpoint.ModifyDrag(this.mount.GetDragPerRound());
      this.hardpoint.ModifyRCS(this.mount.GetRCSPerRound());
    }
    this.ReportReloading(false);
  }

  public void TakeDamage(
    float pierceDamage,
    float blastDamage,
    float amountAffected,
    float fireDamage,
    float impactDamage,
    PersistentID dealerID)
  {
    float pierceDamage1 = Mathf.Max(pierceDamage - this.armorProperties.pierceArmor, 0.0f) / Mathf.Max(this.armorProperties.pierceTolerance, 0.01f);
    float blastDamage1 = Mathf.Max(blastDamage - this.armorProperties.blastArmor, 0.0f) * amountAffected / Mathf.Max(this.armorProperties.blastTolerance, 0.01f);
    float fireDamage1 = Mathf.Max(fireDamage - this.armorProperties.fireArmor, 0.0f) / Mathf.Max(this.armorProperties.fireTolerance, 0.01f);
    float damageAmount = pierceDamage1 + blastDamage1 + fireDamage1 + impactDamage;
    if ((UnityEngine.Object) this.attachedUnit == (UnityEngine.Object) null || (double) damageAmount <= 0.0)
      return;
    if (dealerID.IsValid && dealerID != this.attachedUnit.persistentID)
      this.attachedUnit.RecordDamage(dealerID, damageAmount);
    if (!this.id.HasValue)
      return;
    this.attachedUnit.RpcDamage(this.id.Value, new DamageInfo(pierceDamage1, blastDamage1, fireDamage1, impactDamage));
  }

  public void ApplyDamage(
    float netPierceDamage,
    float netBlastDamage,
    float netFireDamage,
    float netImpactDamage)
  {
    this.hitPoints -= netPierceDamage + netBlastDamage + netFireDamage + netImpactDamage;
    if (this.detached || (UnityEngine.Object) this == (UnityEngine.Object) null)
      return;
    for (int index = this.damageEffects.Count - 1; index >= 0; --index)
    {
      DamageEffect damageEffect = this.damageEffects[index];
      if ((double) this.hitPoints < (double) damageEffect.threshold)
      {
        GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(damageEffect.prefab, Datum.origin);
        gameObject.transform.position = this.transform.position;
        if (gameObject.TryGetComponent<DamageParticles>(out DamageParticles _))
          gameObject.transform.SetParent(this.transform);
        this.damageEffects.RemoveAt(index);
        this.attachedPart.onPartDetached -= new Action<UnitPart>(this.MountedCargo_OnPartDetached);
        foreach (Renderer componentsInChild in this.gameObject.GetComponentsInChildren<MeshRenderer>())
          componentsInChild.enabled = false;
        foreach (Collider componentsInChild in this.gameObject.GetComponentsInChildren<Collider>())
          componentsInChild.enabled = false;
        this.detached = true;
      }
    }
  }

  public void TakeShockwave(Vector3 origin, float overpressure, float blastPower)
  {
  }

  public void Detach(Vector3 velocity, Vector3 relativePos)
  {
  }

  public ArmorProperties GetArmorProperties() => this.armorProperties;

  public Unit GetUnit() => this.attachedUnit;

  public override int GetAmmoLoaded() => !this.fired ? 1 : 0;

  public override int GetAmmoTotal() => !this.fired ? 1 : 0;

  public Transform GetTransform() => this.transform;

  private void MountedCargo_OnPartDamage(UnitPart.OnApplyDamage e)
  {
    if ((double) e.impactDamage <= 0.0)
      return;
    this.ApplyDamage(0.0f, 0.0f, 0.0f, e.impactDamage);
  }

  private void MountedCargo_OnPartDetached(UnitPart part)
  {
    if ((UnityEngine.Object) this == (UnityEngine.Object) null || !this.gameObject.activeSelf)
      return;
    part.onPartDetached -= new Action<UnitPart>(this.MountedCargo_OnPartDetached);
    this.gameObject.SetActive(false);
    if (!this.attachedUnit.IsServer)
      return;
    Player player = (Player) null;
    if (this.attachedUnit is Aircraft attachedUnit)
      player = attachedUnit.Player;
    if (!this.detached)
      this.RemoveFromHardpoint();
    NetworkSceneSingleton<Spawner>.i.SpawnUnit(this.cargo, this.transform.position, this.transform.rotation, part.rb.GetPointVelocity(this.transform.position), this.attachedUnit, player).DisableUnit();
    this.detached = true;
  }

  private async UniTask RailLaunch(Unit owner, Unit target)
  {
    MountedCargo mountedCargo = this;
    CancellationToken cancel = mountedCargo.destroyCancellationToken;
    if ((double) mountedCargo.railDelay > 0.0)
      await UniTask.Delay((int) ((double) mountedCargo.railDelay * 1000.0));
    if (mountedCargo.cargoDoor is CargoRamp cargoRamp)
    {
      while (!cargoRamp.IsOpen())
      {
        if (mountedCargo.hardpoint != null)
          mountedCargo.hardpoint.SpringOpenBayDoors();
        await UniTask.Delay(100);
        if (cancel.IsCancellationRequested)
        {
          cancel = new CancellationToken();
          cargoRamp = (CargoRamp) null;
          return;
        }
      }
    }
    if ((UnityEngine.Object) mountedCargo.deploySound != (UnityEngine.Object) null)
      mountedCargo.PlayLaunchSound();
    YieldAwaitable yieldAwaitable;
    if ((UnityEngine.Object) mountedCargo.cargoDoor != (UnityEngine.Object) null)
    {
      while ((double) Vector3.Dot(mountedCargo.transform.position - mountedCargo.cargoDoor.transform.position, mountedCargo.transform.forward) > 0.0)
      {
        if ((UnityEngine.Object) owner == (UnityEngine.Object) null)
        {
          cancel = new CancellationToken();
          cargoRamp = (CargoRamp) null;
          return;
        }
        if (mountedCargo.hardpoint != null)
          mountedCargo.hardpoint.SpringOpenBayDoors();
        mountedCargo.transform.localPosition += mountedCargo.railSpeed * Time.deltaTime * mountedCargo.railVector;
        yieldAwaitable = UniTask.Yield();
        await yieldAwaitable;
        if (cancel.IsCancellationRequested)
        {
          cancel = new CancellationToken();
          cargoRamp = (CargoRamp) null;
          return;
        }
      }
    }
    if (mountedCargo.detached)
    {
      cancel = new CancellationToken();
      cargoRamp = (CargoRamp) null;
    }
    else
    {
      yieldAwaitable = UniTask.WaitForFixedUpdate();
      await yieldAwaitable;
      mountedCargo.gameObject.SetActive(false);
      mountedCargo.RemoveFromHardpoint();
      Vector3 vector3 = mountedCargo.transform.TransformVector(mountedCargo.railVector) * mountedCargo.railSpeed;
      if (cancel.IsCancellationRequested)
      {
        cancel = new CancellationToken();
        cargoRamp = (CargoRamp) null;
      }
      else if (!owner.IsServer)
      {
        cancel = new CancellationToken();
        cargoRamp = (CargoRamp) null;
      }
      else
      {
        Player player = mountedCargo.attachedUnit.GetPlayer();
        Unit unit = NetworkSceneSingleton<Spawner>.i.SpawnUnit(mountedCargo.cargo, mountedCargo.transform.position, mountedCargo.transform.rotation, owner.rb.GetPointVelocity(mountedCargo.transform.position) + vector3, owner, player);
        mountedCargo.attachedPart.onApplyDamage -= new Action<UnitPart.OnApplyDamage>(mountedCargo.MountedCargo_OnPartDamage);
        mountedCargo.attachedPart.onPartDetached -= new Action<UnitPart>(mountedCargo.MountedCargo_OnPartDetached);
        Rigidbody component = unit.GetComponent<Rigidbody>();
        mountedCargo.PushCargoOut(component).Forget();
        cancel = new CancellationToken();
        cargoRamp = (CargoRamp) null;
      }
    }
  }

  private async UniTask PushCargoOut(Rigidbody cargoRB)
  {
    MountedCargo mountedCargo = this;
    CancellationToken cancel = mountedCargo.destroyCancellationToken;
    BoxCollider cargoCollider = cargoRB.gameObject.GetComponent<BoxCollider>();
    cargoCollider.enabled = true;
    PhysicMaterial cargoColliderMaterial = cargoCollider.sharedMaterial;
    Unit cargoUnit = cargoRB.gameObject.GetComponent<Unit>();
    cargoCollider.sharedMaterial = GameAssets.i.frictionlessMaterial;
    if ((UnityEngine.Object) cargoUnit != (UnityEngine.Object) null)
    {
      cargoUnit.enabled = false;
      cargoUnit.displayDetail = 2f;
    }
    while (!mountedCargo.attachedUnit.disabled && FastMath.InRange(cargoRB.position.ToGlobalPosition(), mountedCargo.transform.GlobalPosition(), mountedCargo.pushDistance))
    {
      if (cargoUnit is GroundVehicle groundVehicle)
        groundVehicle.AnimateWheels(Vector3.Dot(cargoRB.transform.forward, cargoRB.velocity));
      await UniTask.WaitForFixedUpdate();
      if (cancel.IsCancellationRequested)
      {
        mountedCargo.ActivateCargoVehicle(cargoUnit, (Collider) cargoCollider, cargoColliderMaterial);
        cancel = new CancellationToken();
        cargoCollider = (BoxCollider) null;
        cargoColliderMaterial = (PhysicMaterial) null;
        cargoUnit = (Unit) null;
        return;
      }
      float num = Vector3.Dot(cargoRB.velocity - mountedCargo.attachedPart.rb.velocity, -mountedCargo.transform.forward);
      Vector3 force = -mountedCargo.transform.forward * (cargoRB.mass * 10f * Mathf.Clamp(mountedCargo.pushSpeed - num, 0.0f, 1.2f));
      cargoRB.AddForce(force);
      mountedCargo.attachedPart.rb.AddForce(-force);
    }
    mountedCargo.ActivateCargoVehicle(cargoUnit, (Collider) cargoCollider, cargoColliderMaterial);
    cancel = new CancellationToken();
    cargoCollider = (BoxCollider) null;
    cargoColliderMaterial = (PhysicMaterial) null;
    cargoUnit = (Unit) null;
  }

  public void ActivateCargoVehicle(
    Unit cargoUnit,
    Collider cargoCollider,
    PhysicMaterial cargoColliderMaterial)
  {
    switch (cargoUnit)
    {
      case GroundVehicle groundVehicle:
        cargoCollider.enabled = false;
        cargoCollider.sharedMaterial = cargoColliderMaterial;
        groundVehicle.enabled = true;
        groundVehicle.SetHoldPosition(this.attachedUnit is Aircraft attachedUnit && (UnityEngine.Object) attachedUnit.Player != (UnityEngine.Object) null);
        break;
      case Container container:
        cargoCollider.sharedMaterial = cargoColliderMaterial;
        container.enabled = true;
        break;
    }
  }

  private void PlayLaunchSound()
  {
    AudioSource audioSource = this.transform.parent.gameObject.AddComponent<AudioSource>();
    audioSource.outputAudioMixerGroup = SoundManager.i.EffectsMixer;
    audioSource.bypassListenerEffects = true;
    audioSource.clip = this.deploySound;
    audioSource.volume = this.deployVolume;
    audioSource.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
    audioSource.spatialBlend = 1f;
    audioSource.dopplerLevel = 0.0f;
    audioSource.spread = 5f;
    audioSource.maxDistance = 100f;
    audioSource.minDistance = 15f;
    audioSource.Play();
    UnityEngine.Object.Destroy((UnityEngine.Object) audioSource, 5f);
  }

  public enum RailDirection
  {
    Forward,
    Backward,
    Down,
    Right,
    Left,
  }
}
