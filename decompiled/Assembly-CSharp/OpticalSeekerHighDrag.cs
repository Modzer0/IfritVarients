// Decompiled with JetBrains decompiler
// Type: OpticalSeekerHighDrag
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System.Collections.Generic;
using UnityEngine;

#nullable disable
public class OpticalSeekerHighDrag : MissileSeeker
{
  private GlobalPosition knownPos;
  private Vector3 knownVel;
  private Transform targetTransform;
  private bool terminalMode;
  [SerializeField]
  private List<Transform> listPetals = new List<Transform>();
  [SerializeField]
  private float openDelay;
  [SerializeField]
  private float openAltitude;
  [SerializeField]
  private float openAngle = -60f;
  [SerializeField]
  private float openSpeed = 1f;
  [SerializeField]
  private bool deployed;
  private float timeSinceSpawn;
  private float currentAngle;
  private float openRatio;

  public override void Initialize(Unit target, GlobalPosition aimpoint)
  {
    this.knownPos = aimpoint;
    if (UnitRegistry.TryGetUnit(new PersistentID?(this.missile.targetID), out target))
    {
      this.targetUnit = target;
      this.targetTransform = target.GetRandomPart();
      this.knownPos = this.missile.NetworkHQ.GetKnownPosition(target) ?? this.missile.GlobalPosition() + this.missile.transform.forward * 10000f;
      this.knownVel = (Object) target.rb != (Object) null ? target.rb.velocity : Vector3.zero;
      this.missile.NetworkseekerMode = Missile.SeekerMode.passive;
    }
    else
      this.knownPos = this.missile.GlobalPosition() + this.missile.transform.forward * 10000f;
    this.missile.SetAimpoint(this.knownPos, this.knownVel);
    if ((double) this.openAngle >= 0.0)
      return;
    this.openAngle += 360f;
  }

  public override string GetSeekerType() => "Optical";

  public override void Seek()
  {
    this.timeSinceSpawn += Time.fixedDeltaTime;
    if ((Object) this.targetUnit != (Object) null && !this.targetUnit.disabled)
    {
      GlobalPosition? knownPosition = this.missile.NetworkHQ.GetKnownPosition(this.targetUnit);
      if (knownPosition.HasValue)
      {
        this.knownPos = knownPosition.Value;
        this.knownVel = (Object) this.targetUnit.rb != (Object) null ? this.targetUnit.rb.velocity : Vector3.zero;
      }
    }
    if (!this.deployed)
    {
      if ((double) this.missile.radarAlt >= (double) this.openAltitude || (double) this.timeSinceSpawn <= (double) this.openDelay)
        return;
      this.deployed = true;
    }
    else
    {
      foreach (Transform listPetal in this.listPetals)
      {
        this.currentAngle = Mathf.LerpAngle(listPetal.localEulerAngles.x, this.openAngle, this.openSpeed * Time.fixedDeltaTime);
        this.openRatio = Mathf.Lerp(this.openRatio, 1f, this.openSpeed * Time.fixedDeltaTime);
        listPetal.localEulerAngles = new Vector3(this.currentAngle, 0.0f, 0.0f);
      }
    }
  }

  private void OnDestroy()
  {
    using (List<Transform>.Enumerator enumerator = this.listPetals.GetEnumerator())
    {
      if (!enumerator.MoveNext())
        return;
      Object.Destroy((Object) enumerator.Current);
    }
  }
}
