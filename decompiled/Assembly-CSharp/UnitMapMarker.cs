// Decompiled with JetBrains decompiler
// Type: UnitMapMarker
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

#nullable disable
public abstract class UnitMapMarker : MapMarker
{
  public UnitMapIcon Icon { get; private set; }

  public Unit GetUnit() => !((Object) this.Icon != (Object) null) ? (Unit) null : this.Icon.unit;

  protected abstract void ExtraSetup();

  public void Setup(UnitMapIcon icon)
  {
    this.Icon = icon;
    this.ExtraSetup();
  }
}
