// Decompiled with JetBrains decompiler
// Type: NuclearOption.OwnedAirframe
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

#nullable disable
namespace NuclearOption;

public struct OwnedAirframe(AircraftDefinition definition, bool reserved)
{
  public readonly AircraftDefinition Definition = definition;
  public readonly bool Reserved = reserved;
}
