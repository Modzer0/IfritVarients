// Decompiled with JetBrains decompiler
// Type: SpecialFlareEjector
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Cysharp.Threading.Tasks;
using UnityEngine;

#nullable disable
public class SpecialFlareEjector : Weapon
{
  [SerializeField]
  private GameObject flarePrefab;
  [SerializeField]
  private float ejectionVelocityVariance;
  private bool triggerPulled;
  public Transform ejectionPoint;
  [Tooltip("in seconds")]
  [SerializeField]
  private float ejectionInterval;
  private float lastEjectionTime;

  public override void AttachToUnit(Unit unit)
  {
    base.AttachToUnit(unit);
    this.enabled = false;
  }

  public override void SetTarget(Unit target)
  {
  }

  public override void Fire(
    Unit firingUnit,
    Unit target,
    Vector3 inheritedVelocity,
    WeaponStation weaponStation,
    GlobalPosition aimpoint)
  {
    if (this.attachedUnit.disabled || (double) Time.timeSinceLevelLoad - (double) this.lastEjectionTime < (double) this.ejectionInterval)
      return;
    this.lastEjectionTime = Time.timeSinceLevelLoad;
    weaponStation.UpdateLastFired(1);
    if (this.hardpoint != null)
    {
      if (this.hardpoint.part.IsDetached())
        return;
      if (this.info.useWeaponDoors)
        this.hardpoint.SpringOpenBayDoors();
    }
    this.triggerPulled = true;
    this.weaponStation = weaponStation;
    this.enabled = true;
    this.EjectFlare();
  }

  private async UniTask EjectFlare()
  {
    SpecialFlareEjector specialFlareEjector = this;
    await UniTask.WaitForFixedUpdate();
    specialFlareEjector.enabled = true;
    GameObject gameObject = NetworkSceneSingleton<Spawner>.i.SpawnLocal(specialFlareEjector.flarePrefab, Datum.origin);
    gameObject.transform.position = specialFlareEjector.ejectionPoint.position;
    Vector3 velocity = specialFlareEjector.attachedUnit.rb.velocity;
    gameObject.GetComponent<SpecialFlare>().LaunchFlare(specialFlareEjector.ejectionPoint, velocity + specialFlareEjector.ejectionPoint.forward * ((float) Random.Range(-1, 1) * specialFlareEjector.ejectionVelocityVariance));
  }
}
