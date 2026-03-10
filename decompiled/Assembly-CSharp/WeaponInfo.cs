// Decompiled with JetBrains decompiler
// Type: WeaponInfo
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

#nullable disable
[CreateAssetMenu(fileName = "New Weapon Info", menuName = "ScriptableObjects/WeaponInfo", order = 3)]
public class WeaponInfo : ScriptableObject
{
  public GameObject weaponPrefab;
  public RoleIdentity effectiveness;
  public TargetRequirements targetRequirements;
  public float pK;
  public string weaponName;
  public string shortName;
  public string description;
  public float fireInterval;
  public float muzzleVelocity;
  public float maxSpeed = -1f;
  public float dragCoef;
  public float pierceDamage;
  public float blastDamage;
  public Sprite weaponIcon;
  public float armorTierEffectiveness;
  public float airburstHeight;
  public float visibilityWhenFired;
  public float costPerRound;
  public float massPerRound;
  public bool useWeaponDoors = true;
  public bool boresight;
  public bool laserGuided;
  public bool bomb;
  public bool gun;
  public bool overHorizon;
  public bool nuclear;
  public bool strategic;
  public bool energy;
  public bool jammer;
  public bool troops;
  public bool hideInDisplay;
  public bool cargo;
  public bool rearmGround;
  public bool rearmShip;
  public bool sling;

  public void SetMassPerRound(float massPerRound) => this.massPerRound = massPerRound;

  public void SetCostPerRound(float costPerRound) => this.costPerRound = costPerRound;

  public float GetMaxSpeed()
  {
    if ((double) this.maxSpeed == -1.0)
    {
      if ((Object) this.weaponPrefab != (Object) null)
      {
        Missile component = this.weaponPrefab.GetComponent<Missile>();
        if ((Object) component != (Object) null)
          this.maxSpeed = component.GetTopSpeed(0.0f, 0.0f);
      }
      else
        this.maxSpeed = this.muzzleVelocity;
    }
    return this.maxSpeed;
  }

  public float CalcAttacksNeeded(Unit target)
  {
    return Mathf.Max(target.definition.damageTolerance, 0.1f) / Mathf.Max(this.pK, 0.01f);
  }
}
