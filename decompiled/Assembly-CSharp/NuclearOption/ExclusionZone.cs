// Decompiled with JetBrains decompiler
// Type: NuclearOption.ExclusionZone
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

#nullable disable
namespace NuclearOption;

public readonly struct ExclusionZone
{
  public readonly PersistentID sourceId;
  public readonly GlobalPosition position;
  public readonly float radius;

  public ExclusionZone(Unit unit, GlobalPosition position, float radius)
  {
    this.sourceId = unit.persistentID;
    this.position = position;
    this.radius = radius;
  }
}
