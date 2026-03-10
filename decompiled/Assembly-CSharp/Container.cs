// Decompiled with JetBrains decompiler
// Type: Container
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Cysharp.Threading.Tasks;
using Mirage;
using Mirage.Serialization;
using NuclearOption.Networking;
using System;
using System.Threading;
using UnityEngine;

#nullable disable
public class Container : Unit
{
  [SyncVar]
  public PersistentID ownerID;
  [SyncVar(hook = "KinematicChanged")]
  private bool kinematic;
  [SerializeField]
  private float stability;
  [SerializeField]
  private float stabilityDamping;
  [SerializeField]
  private float waterDragCoef;
  [SerializeField]
  private float airDragCoef;
  private float volume;
  private float height;
  private float distanceSubmerged;
  private float timeStationary;
  [SerializeField]
  private CollisionTriggerZone collisionTriggerZone;
  [SerializeField]
  private bool buoyant;
  [SerializeField]
  private Container.FlotationDevice[] flotationDevices;
  [SerializeField]
  private GameObject parachuteSystem;
  private BoxCollider mainCollider;
  private float lastRadarAltCheck;
  private bool slung;
  [NonSerialized]
  private const int SYNC_VAR_COUNT = 11;
  [NonSerialized]
  private const int RPC_COUNT = 19;

  public override void Awake()
  {
    this.transform.gameObject.layer = LayerMask.NameToLayer("Ignore Collisions");
    base.Awake();
    this.Identity.OnStartClient.AddListener(new Action(this.OnStartClient));
    if (!((UnityEngine.Object) this.collisionTriggerZone != (UnityEngine.Object) null))
      return;
    this.collisionTriggerZone.OnTriggerEntered += new Action(this.Container_OnTriggerZoneEntered);
  }

  private void OnStartClient()
  {
    if (NetworkManagerNuclearOption.i.Server.Active)
      this.SetLocalSim(true);
    this.mainCollider = this.gameObject.GetComponent<BoxCollider>();
    this.volume = this.mainCollider.size.x * this.mainCollider.size.y * this.mainCollider.size.z;
    this.height = this.mainCollider.size.y;
    Aircraft localAircraft;
    if (this.remoteSim & (GameManager.GetLocalAircraft(out localAircraft) && localAircraft.persistentID == this.ownerID))
    {
      this.transform.gameObject.layer = 15;
      this.ClientCollisionDelay().Forget();
    }
    else
      this.transform.gameObject.layer = 0;
    this.transform.SetPositionAndRotation(this.startPosition.ToLocalPosition(), this.startRotation);
    this.RegisterUnit(new float?(4f));
    this.InitializeUnit();
  }

  private async UniTask ClientCollisionDelay()
  {
    Container container = this;
    CancellationToken cancel = container.destroyCancellationToken;
    await UniTask.Delay(5000);
    if (cancel.IsCancellationRequested)
    {
      cancel = new CancellationToken();
    }
    else
    {
      container.transform.gameObject.layer = 0;
      cancel = new CancellationToken();
    }
  }

  private void Container_OnTriggerZoneEntered()
  {
    if (!NetworkManagerNuclearOption.i.Server.Active || !this.kinematic)
      return;
    this.Networkkinematic = false;
  }

  public override void AttachOrDetachSlingHook(Aircraft aircraft, bool attached)
  {
    base.AttachOrDetachSlingHook(aircraft, attached);
    if (this.IsServer)
    {
      this.NetworkownerID = aircraft.persistentID;
      this.Networkkinematic = false;
      if (!attached && (UnityEngine.Object) this.gameObject.GetComponent<ImpactDetector>() == (UnityEngine.Object) null)
        this.gameObject.AddComponent<ImpactDetector>().SetGLimit(100f);
    }
    this.slung = attached;
  }

  private void KinematicChanged(bool oldValue, bool newValue)
  {
    this.rb.isKinematic = newValue;
    this.rb.interpolation = this.rb.isKinematic ? RigidbodyInterpolation.None : RigidbodyInterpolation.Interpolate;
    this.enabled = !newValue;
  }

  public override void InitializeUnit()
  {
    base.InitializeUnit();
    this.rb.isKinematic = this.kinematic;
    if (!((UnityEngine.Object) this.parachuteSystem != (UnityEngine.Object) null) || Physics.Linecast(this.startPosition.ToLocalPosition(), this.startPosition.ToLocalPosition() - Vector3.up * 10f, 64 /*0x40*/))
      return;
    UnityEngine.Object.Instantiate<GameObject>(this.parachuteSystem, this.transform).GetComponent<CargoDeploymentSystem>().Initialize((Unit) this);
  }

  public override void SetLocalSim(bool localSim)
  {
    base.SetLocalSim(localSim);
    this.rb.useGravity = localSim;
  }

  public override void UnitDisabled(bool oldState, bool newState)
  {
    base.UnitDisabled(oldState, newState);
    this.GetComponent<Renderer>().enabled = false;
    this.mainCollider.enabled = false;
    foreach (Container.FlotationDevice flotationDevice in this.flotationDevices)
      flotationDevice.Remove();
    if (!NetworkManagerNuclearOption.i.Server.Active)
      return;
    UnityEngine.Object.Destroy((UnityEngine.Object) this.gameObject, 2f);
  }

  public override bool IsSlung() => this.slung;

  private void FixedUpdate()
  {
    this.speed = this.rb.velocity.magnitude;
    if (!NetworkManagerNuclearOption.i.Server.Active)
      return;
    this.timeStationary = (double) this.speed > 1.0 ? 0.0f : this.timeStationary + Time.fixedDeltaTime;
    this.CheckRadarAlt();
    if ((double) this.timeStationary > 5.0)
    {
      this.timeStationary = 0.0f;
      if (this.slung)
        return;
      RaycastHit hitInfo;
      if (!Physics.Linecast(this.transform.position, this.transform.position - Vector3.up * 5f, out hitInfo, 2112) || (UnityEngine.Object) hitInfo.collider.attachedRigidbody == (UnityEngine.Object) null)
      {
        this.NetworkstartPosition = this.transform.GlobalPosition();
        this.Networkkinematic = true;
        return;
      }
    }
    float num1 = Datum.LocalSeaY - (this.transform.position.y - this.height * 0.5f);
    if ((double) num1 <= 0.0)
    {
      Vector3 vector3 = this.rb.velocity - NetworkSceneSingleton<LevelInfo>.i.GetWind(this.transform.GlobalPosition());
      float sqrMagnitude = vector3.sqrMagnitude;
      this.rb.AddForce(-vector3.normalized * sqrMagnitude * this.airDragCoef * (this.height * this.height));
    }
    else
    {
      foreach (Container.FlotationDevice flotationDevice in this.flotationDevices)
        flotationDevice.Inflate();
      if ((double) this.distanceSubmerged <= 0.0)
        UnityEngine.Object.Destroy((UnityEngine.Object) UnityEngine.Object.Instantiate<GameObject>(GameAssets.i.splash_large, new Vector3(this.transform.position.x, Datum.LocalSeaY, this.transform.position.z), Quaternion.LookRotation(Vector3.up + new Vector3(this.rb.velocity.x, 0.0f, this.rb.velocity.z) * 0.1f)), 20f);
      this.distanceSubmerged = num1;
      float num2 = Mathf.Clamp01(this.distanceSubmerged / this.height);
      float num3 = 9810f * (num2 * this.volume);
      Vector3 vector3_1 = -this.rb.velocity.normalized * this.speed * this.speed * num2 * this.waterDragCoef * (this.height * this.height);
      Vector3 vector3_2 = Vector3.Cross(this.transform.up, Vector3.up) * this.rb.mass * this.stability;
      Vector3 vector3_3 = -this.rb.angularVelocity * this.rb.mass * this.stabilityDamping;
      this.rb.AddForce(Vector3.up * num3 + vector3_1);
      this.rb.AddTorque((vector3_2 + vector3_3) * num2);
    }
  }

  private void MirageProcessed()
  {
  }

  public PersistentID NetworkownerID
  {
    get => this.ownerID;
    set
    {
      if (this.SyncVarEqual<PersistentID>(value, this.ownerID))
        return;
      PersistentID ownerId = this.ownerID;
      this.ownerID = value;
      this.SetDirtyBit(512UL /*0x0200*/);
    }
  }

  public bool Networkkinematic
  {
    get => this.kinematic;
    set
    {
      if (this.SyncVarEqual<bool>(value, this.kinematic))
        return;
      bool kinematic = this.kinematic;
      this.kinematic = value;
      this.SetDirtyBit(1024UL /*0x0400*/);
      if (!this.GetSyncVarHookGuard(1024UL /*0x0400*/) && this.IsHost)
      {
        this.SetSyncVarHookGuard(1024UL /*0x0400*/, true);
        this.KinematicChanged(kinematic, value);
        this.SetSyncVarHookGuard(1024UL /*0x0400*/, false);
      }
    }
  }

  public override bool SerializeSyncVars(NetworkWriter writer, bool initialize)
  {
    ulong syncVarDirtyBits = this.SyncVarDirtyBits;
    bool flag = base.SerializeSyncVars(writer, initialize);
    if (initialize)
    {
      GeneratedNetworkCode._Write_PersistentID(writer, this.ownerID);
      writer.WriteBooleanExtension(this.kinematic);
      return true;
    }
    writer.Write(syncVarDirtyBits >> 9, 2);
    if (((long) syncVarDirtyBits & 512L /*0x0200*/) != 0L)
    {
      GeneratedNetworkCode._Write_PersistentID(writer, this.ownerID);
      flag = true;
    }
    if (((long) syncVarDirtyBits & 1024L /*0x0400*/) != 0L)
    {
      writer.WriteBooleanExtension(this.kinematic);
      flag = true;
    }
    return flag;
  }

  public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
  {
    base.DeserializeSyncVars(reader, initialState);
    if (initialState)
    {
      this.ownerID = GeneratedNetworkCode._Read_PersistentID(reader);
      bool kinematic = this.kinematic;
      this.kinematic = reader.ReadBooleanExtension();
      if (this.IsServer || this.SyncVarEqual<bool>(kinematic, this.kinematic))
        return;
      this.KinematicChanged(kinematic, this.kinematic);
    }
    else
    {
      ulong dirtyBit = reader.Read(2);
      this.SetDeserializeMask(dirtyBit, 9);
      if (((long) dirtyBit & 1L) != 0L)
        this.ownerID = GeneratedNetworkCode._Read_PersistentID(reader);
      if (((long) dirtyBit & 2L) == 0L)
        return;
      bool kinematic = this.kinematic;
      this.kinematic = reader.ReadBooleanExtension();
      if (!this.IsServer && !this.SyncVarEqual<bool>(kinematic, this.kinematic))
        this.KinematicChanged(kinematic, this.kinematic);
    }
  }

  protected override int GetRpcCount() => 19;

  [Serializable]
  private class FlotationDevice
  {
    [SerializeField]
    private Transform transform;
    [SerializeField]
    private Vector3 stowedScale;
    [SerializeField]
    private Vector3 inflatedScale;
    [SerializeField]
    private float inflationSpeed;
    private float inflatedAmount;

    public void Inflate()
    {
      this.inflatedAmount += this.inflationSpeed * Time.deltaTime;
      this.transform.localScale = Vector3.Lerp(this.stowedScale, this.inflatedScale, this.inflatedAmount);
    }

    public void Remove() => this.transform.gameObject.SetActive(false);
  }
}
