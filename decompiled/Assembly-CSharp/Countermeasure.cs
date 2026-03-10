// Decompiled with JetBrains decompiler
// Type: Countermeasure
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System.Collections.Generic;
using UnityEngine;

#nullable disable
public abstract class Countermeasure : MonoBehaviour
{
  public string displayName;
  public Sprite displayImage;
  public bool chargeable;
  public int ammo;
  protected List<string> threatTypes;
  public Aircraft aircraft;

  protected virtual void Awake()
  {
    if (!((Object) this.aircraft != (Object) null))
      return;
    this.aircraft.countermeasureManager.RegisterCountermeasure(this);
  }

  public virtual List<string> GetThreatTypes() => new List<string>();

  public virtual void AttachToUnit(Aircraft aircraft)
  {
    if ((Object) this.aircraft != (Object) null)
      return;
    this.aircraft = aircraft;
    aircraft.countermeasureManager.RegisterCountermeasure(this);
  }

  public virtual void Fire()
  {
  }

  public virtual void UpdateHUD()
  {
  }

  protected virtual void OnDestroy()
  {
    if (!((Object) this.aircraft != (Object) null))
      return;
    this.aircraft.countermeasureManager.DeregisterCountermeasure(this);
  }

  public virtual void Rearm(Aircraft aircraft, Unit rearmer)
  {
  }
}
