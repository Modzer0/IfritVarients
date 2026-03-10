// Decompiled with JetBrains decompiler
// Type: MountedMissile
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

#nullable disable
public class MountedMissile : Weapon
{
  [SerializeField]
  private MountedMissile.RailDirection railDirection;
  [SerializeField]
  private float railLength;
  [SerializeField]
  private float railSpeed;
  [SerializeField]
  private float railDelay;
  [SerializeField]
  private AudioClip deploySound;
  [SerializeField]
  private float deployVolume;
  [SerializeField]
  private float doorOpenDuration = 0.5f;
  [SerializeField]
  private BayDoor[] bayDoors;
  private float railPosition;
  private Vector3 mountedPosition;
  private bool fired;
  private Vector3 railVector;

  public override void AttachToHardpoint(
    Aircraft aircraft,
    Hardpoint hardpoint,
    WeaponMount weaponMount)
  {
    base.AttachToHardpoint(aircraft, hardpoint, weaponMount);
    hardpoint.ModifyMass(this.info.massPerRound);
    hardpoint.ModifyDrag(this.mount.GetDragPerRound());
    hardpoint.ModifyRCS(this.mount.GetRCSPerRound());
    this.mountedPosition = this.transform.localPosition;
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
    Aircraft aircraft = owner as Aircraft;
    aircraft.RequestRearm();
    this.RemoveFromHardpoint();
    this.fired = true;
    switch (this.railDirection)
    {
      case MountedMissile.RailDirection.Forward:
        this.railVector = new Vector3(0.0f, 0.0f, 1f);
        break;
      case MountedMissile.RailDirection.Down:
        this.railVector = new Vector3(0.0f, -1f, 0.0f);
        break;
      case MountedMissile.RailDirection.Right:
        this.railVector = new Vector3(1f, 0.0f, 0.0f);
        break;
      case MountedMissile.RailDirection.Left:
        this.railVector = new Vector3(-1f, 0.0f, 0.0f);
        break;
      case MountedMissile.RailDirection.Backward:
        this.railVector = new Vector3(0.0f, 0.0f, -1f);
        break;
      case MountedMissile.RailDirection.Up:
        this.railVector = new Vector3(0.0f, 1f, 0.0f);
        break;
    }
    if (aircraft.IsServer)
      aircraft.RpcLaunchMissile(weaponStation.Number, target, aimpoint);
    else if (aircraft.HasAuthority)
      aircraft.CmdLaunchMissile(weaponStation.Number, target, aimpoint);
    if (this.bayDoors.Length != 0)
    {
      foreach (BayDoor bayDoor in this.bayDoors)
        bayDoor.OpenDoor(this.doorOpenDuration);
    }
    else if (this.hardpoint != null)
      this.hardpoint.SpringOpenBayDoors();
    if (this.hardpoint.part.IsDetached())
      return;
    this.RailLaunch(owner, target).Forget();
    this.TrackFiringVisibility().Forget();
  }

  private async UniTask RailLaunch(Unit owner, Unit target)
  {
    MountedMissile mountedMissile = this;
    CancellationToken cancel = mountedMissile.destroyCancellationToken;
    await UniTask.Delay((int) ((double) mountedMissile.railDelay * 1000.0));
    if ((Object) mountedMissile.deploySound != (Object) null)
      mountedMissile.PlayLaunchSound();
    Vector3 vector3 = new Vector3();
    for (; (double) mountedMissile.railPosition < (double) mountedMissile.railLength; mountedMissile.railPosition += mountedMissile.railSpeed * Time.deltaTime)
    {
      await UniTask.Yield();
      if (cancel.IsCancellationRequested)
      {
        cancel = new CancellationToken();
        return;
      }
      if ((Object) owner == (Object) null)
      {
        cancel = new CancellationToken();
        return;
      }
      vector3 = (mountedMissile.railVector.z * mountedMissile.transform.forward + mountedMissile.railVector.y * mountedMissile.transform.up + mountedMissile.railVector.x * mountedMissile.transform.right) * mountedMissile.railSpeed;
      mountedMissile.transform.position += vector3 * Time.deltaTime;
    }
    if (owner.IsServer)
      NetworkSceneSingleton<Spawner>.i.SpawnMissile(mountedMissile.info.weaponPrefab, mountedMissile.transform.position, mountedMissile.transform.rotation, owner.rb.GetPointVelocity(mountedMissile.transform.position) + vector3, target, owner);
    mountedMissile.gameObject.SetActive(false);
    if (!((Object) owner != (Object) null))
      cancel = new CancellationToken();
    else if (!((Object) owner.NetworkHQ != (Object) null))
      cancel = new CancellationToken();
    else if (!owner.IsServer)
    {
      cancel = new CancellationToken();
    }
    else
    {
      owner.NetworkHQ.missionStatsTracker.MunitionCost(owner, mountedMissile.info.costPerRound);
      cancel = new CancellationToken();
    }
  }

  public override int GetAmmoLoaded() => !this.fired ? 1 : 0;

  public override int GetAmmoTotal() => !this.fired ? 1 : 0;

  private void OnDestroy()
  {
    if (this.fired || this.hardpoint == null)
      return;
    this.RemoveFromHardpoint();
  }

  private void RemoveFromHardpoint()
  {
    if (this.hardpoint == null)
      return;
    this.hardpoint.ModifyMass(-this.info.massPerRound);
    this.hardpoint.ModifyDrag(-this.mount.GetDragPerRound());
    this.hardpoint.ModifyRCS(-this.mount.GetRCSPerRound());
  }

  public override void Rearm()
  {
    if (!this.fired)
      return;
    this.transform.localPosition = this.mountedPosition;
    this.gameObject.SetActive(true);
    this.railPosition = 0.0f;
    this.fired = false;
    if (this.hardpoint != null)
    {
      this.hardpoint.ModifyMass(this.info.massPerRound);
      this.hardpoint.ModifyDrag(this.mount.GetDragPerRound());
      this.hardpoint.ModifyRCS(this.mount.GetRCSPerRound());
    }
    this.ReportReloading(false);
  }

  private void PlayLaunchSound()
  {
    AudioSource audioSource = this.transform.parent.gameObject.AddComponent<AudioSource>();
    audioSource.outputAudioMixerGroup = SoundManager.i.EffectsMixer;
    audioSource.bypassListenerEffects = true;
    audioSource.clip = this.deploySound;
    audioSource.volume = this.deployVolume;
    audioSource.pitch = Random.Range(0.8f, 1.2f);
    audioSource.spatialBlend = 1f;
    audioSource.dopplerLevel = 0.0f;
    audioSource.spread = 5f;
    audioSource.maxDistance = 40f;
    audioSource.minDistance = 5f;
    audioSource.Play();
    Object.Destroy((Object) audioSource, 5f);
  }

  public enum RailDirection
  {
    Forward,
    Down,
    Right,
    Left,
    Backward,
    Up,
  }
}
