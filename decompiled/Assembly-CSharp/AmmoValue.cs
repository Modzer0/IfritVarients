// Decompiled with JetBrains decompiler
// Type: AmmoValue
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

#nullable disable
public readonly struct AmmoValue(float currentValue, float totalValue)
{
  public readonly float current = currentValue;
  public readonly float total = totalValue;

  public float Missing => this.total - this.current;
}
