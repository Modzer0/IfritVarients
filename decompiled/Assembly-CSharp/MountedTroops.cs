// Decompiled with JetBrains decompiler
// Type: MountedTroops
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;

#nullable disable
public class MountedTroops : Weapon
{
  [SerializeField]
  private float mass;
  [SerializeField]
  private float captureStrength;
  private Aircraft attachedAircraft;
  private UnitPart attachedPart;
  private bool disabled;
  private bool captureActive;

  private void Awake() => this.ammo = (int) this.captureStrength;

  public override void AttachToHardpoint(Aircraft aircraft, Hardpoint hardpoint, WeaponMount mount)
  {
    base.AttachToHardpoint(aircraft, hardpoint, mount);
    this.attachedPart = hardpoint.part;
    this.attachedPart.onPartDetached += new Action<UnitPart>(this.MountedTroops_OnPartDetached);
    this.attachedAircraft = aircraft;
    this.attachedAircraft = aircraft;
    this.hardpoint = hardpoint;
    hardpoint.ModifyMass(this.mass);
    this.StartSlowUpdateDelayed(2f, new Action(this.CheckCaptureConditions));
  }

  public override int GetAmmoLoaded() => (int) this.captureStrength;

  public override int GetAmmoTotal() => (int) this.captureStrength;

  public override void Fire(
    Unit owner,
    Unit target,
    Vector3 inheritedVelocity,
    WeaponStation weaponStation,
    GlobalPosition aimpoint)
  {
    if (this.hardpoint != null)
      this.hardpoint.SpringOpenBayDoors();
    if (this.attachedAircraft.IsServer)
    {
      this.attachedAircraft.RpcLaunchMissile(weaponStation.Number, target, aimpoint);
    }
    else
    {
      if (!this.attachedAircraft.HasAuthority)
        return;
      this.attachedAircraft.CmdLaunchMissile(weaponStation.Number, target, aimpoint);
    }
  }

  private void MountedTroops_OnPartDetached(UnitPart part)
  {
    this.disabled = true;
    UnityEngine.Object.Destroy((UnityEngine.Object) this);
  }

  private void OnDestroy()
  {
    if ((UnityEngine.Object) this.attachedPart != (UnityEngine.Object) null)
    {
      this.attachedPart.onPartDetached -= new Action<UnitPart>(this.MountedTroops_OnPartDetached);
      this.hardpoint.ModifyMass(-this.mass);
    }
    if (!((UnityEngine.Object) this.attachedAircraft != (UnityEngine.Object) null) || !this.captureActive)
      return;
    this.attachedAircraft.ModifyCaptureStrength(-this.captureStrength);
  }

  private void CheckCaptureConditions()
  {
    bool flag = (double) this.attachedAircraft.speed < 2.0 && (double) this.attachedAircraft.radarAlt < 1.0;
    if (flag == this.captureActive)
      return;
    this.captureActive = flag;
    this.attachedAircraft.ModifyCaptureStrength(this.captureActive ? this.captureStrength : -this.captureStrength);
  }
}
