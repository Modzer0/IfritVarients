// Decompiled with JetBrains decompiler
// Type: MissileSeeker
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

#nullable disable
public class MissileSeeker : MonoBehaviour
{
  [SerializeField]
  protected Missile missile;
  protected Unit targetUnit;
  public bool triggerMissileWarning = true;

  public virtual string GetSeekerType() => string.Empty;

  public virtual float GetSeekerThreat() => 1f;

  public virtual float GetMinSpeed() => 200f;

  public virtual GlobalPosition GetEvasionPoint() => this.missile.GlobalPosition();

  public virtual void Seek()
  {
  }

  public virtual void Initialize(Unit target, GlobalPosition aimpoint)
  {
  }
}
