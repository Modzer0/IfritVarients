// Decompiled with JetBrains decompiler
// Type: SpecialSmokeEjector
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

#nullable disable
public class SpecialSmokeEjector : Weapon
{
  [SerializeField]
  private SmokeEmitter[] emitters;
  private bool triggerPulled;
  [SerializeField]
  private float ejectionInterval;
  private float lastEjectionTime;
  private float emitCounter;
  private ParticleSystem.EmitParams emitParams;
  private Aircraft aircraft;
  private Vector3 velocity;
  private float airspeed;

  public override void AttachToUnit(Unit unit)
  {
    base.AttachToUnit(unit);
    this.enabled = true;
    this.aircraft = this.attachedUnit as Aircraft;
    for (int index = 0; index < this.emitters.Length; ++index)
      this.emitters[index].Initialize();
  }

  public override void SetTarget(Unit target)
  {
  }

  private void Update()
  {
    this.airspeed = this.aircraft.speed;
    this.velocity = this.aircraft.rb.velocity;
    this.Emit(this.airspeed, this.velocity);
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
    if (this.hardpoint != null)
    {
      if (this.hardpoint.part.IsDetached())
        return;
      if (this.info.useWeaponDoors)
        this.hardpoint.SpringOpenBayDoors();
    }
    this.triggerPulled = !this.triggerPulled;
    Debug.Log((object) $"FIRE PRESSED {this.triggerPulled}");
    this.weaponStation = weaponStation;
    this.enabled = true;
  }

  public void Emit(float airspeed, Vector3 velocity)
  {
    for (int index = 0; index < this.emitters.Length; ++index)
      this.emitters[index].Emit(this.triggerPulled, airspeed, velocity);
  }
}
