// Decompiled with JetBrains decompiler
// Type: MissileLauncher
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

#nullable disable
public class MissileLauncher : Weapon
{
  [SerializeField]
  private float reloadTime;
  [SerializeField]
  public MissileDefinition missile;
  [SerializeField]
  private float railLength;
  [SerializeField]
  private float railSpeed;
  [SerializeField]
  private Vector3 ejectionVelocity;
  [SerializeField]
  private Transform[] launchTransforms;
  [SerializeField]
  private float cellSeperation;
  [SerializeField]
  private int cellColumns = 1;
  [SerializeField]
  private int cellRows = 1;
  [SerializeField]
  private AudioSource launchSound;
  [SerializeField]
  private ParticleSystem launchParticles;
  private int maxAmmo;
  private int currentCell;
  private Transform launchTransform;

  private void OnEnable()
  {
    this.maxAmmo = this.ammo;
    for (int index = 0; index < this.launchTransforms.Length; ++index)
      this.launchTransforms[index].gameObject.SetActive(false);
    if (!((Object) this.launchTransform == (Object) null))
      return;
    this.launchTransform = new GameObject("launchTransform").transform;
    this.launchTransform.parent = this.transform;
    this.launchTransform.localPosition = Vector3.zero;
    this.launchTransform.localRotation = Quaternion.identity;
  }

  public override void Fire(
    Unit owner,
    Unit target,
    Vector3 inheritedVelocity,
    WeaponStation weaponStation,
    GlobalPosition aimpoint)
  {
    if (this.ammo <= 0 || (double) Time.timeSinceLevelLoad - (double) this.lastFired <= (double) this.reloadTime)
      return;
    if (this.launchTransforms.Length != 0)
    {
      this.launchTransform = this.launchTransforms[this.currentCell].transform;
      ++this.currentCell;
      if (this.currentCell > this.launchTransforms.Length - 1)
        this.currentCell = 0;
    }
    else if (this.cellColumns > 0)
    {
      this.launchTransform.localPosition = new Vector3((float) (this.currentCell % this.cellColumns) * this.cellSeperation, (float) (this.currentCell / this.cellColumns % this.cellRows) * this.cellSeperation, 0.0f);
      ++this.currentCell;
      if (this.currentCell > this.cellRows * this.cellColumns)
        this.currentCell = 0;
    }
    this.TrackFiringVisibility().Forget();
    this.lastFired = Time.timeSinceLevelLoad;
    weaponStation.UpdateLastFired(1);
    if (owner.LocalSim)
    {
      Vector3 velocity = inheritedVelocity + this.ejectionVelocity.x * this.launchTransform.right + this.ejectionVelocity.y * this.launchTransform.up + this.ejectionVelocity.z * this.launchTransform.forward;
      NetworkSceneSingleton<Spawner>.i.SpawnMissile(this.missile, this.launchTransform.position, this.launchTransform.rotation, velocity, target, owner);
    }
    if ((Object) this.launchParticles != (Object) null)
    {
      this.launchParticles.transform.position = this.launchTransform.position;
      this.launchParticles.Play();
    }
    if ((Object) this.launchSound != (Object) null)
    {
      this.launchSound.pitch = Random.Range(0.95f, 1.05f);
      this.launchSound.Play();
    }
    --this.ammo;
    weaponStation.AccountAmmo();
    weaponStation.Updated();
    this.ReportReloading(true);
    if (this.attachedUnit.IsServer)
      this.attachedUnit.RpcSyncAmmoCount(weaponStation.Number, weaponStation.Ammo);
    if (this.ammo > 0)
      this.WaitForReload().Forget();
    if (!((Object) this.attachedUnit != (Object) null) || !((Object) this.attachedUnit.NetworkHQ != (Object) null) || !this.attachedUnit.IsServer)
      return;
    this.attachedUnit.NetworkHQ.missionStatsTracker.MunitionCost(this.attachedUnit, this.info.costPerRound);
  }

  public override void Rearm()
  {
    if (!this.Rearmable)
      return;
    this.ammo = this.maxAmmo;
  }

  public override int GetFullAmmo() => this.maxAmmo;

  public override int GetAmmoTotal() => this.ammo;

  public async UniTask WaitForReload()
  {
    MissileLauncher missileLauncher = this;
    CancellationToken cancel = missileLauncher.destroyCancellationToken;
    await UniTask.Delay((int) ((double) missileLauncher.reloadTime * 1000.0));
    if (cancel.IsCancellationRequested)
    {
      cancel = new CancellationToken();
    }
    else
    {
      missileLauncher.ReportReloading(false);
      cancel = new CancellationToken();
    }
  }
}
