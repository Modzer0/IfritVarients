// Decompiled with JetBrains decompiler
// Type: NuclearOption.ReserveNotice
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

#nullable disable
namespace NuclearOption;

public struct ReserveNotice(
  ReserveEvent outcome,
  AircraftDefinition aircraftDefinition,
  bool isReserving,
  int queuePosition)
{
  public ReserveEvent outcome = outcome;
  public AircraftDefinition aircraftDefinition = aircraftDefinition;
  public bool isReserving = isReserving;
  public int queuePosition = queuePosition;
  public static readonly ReserveNotice Invalid = new ReserveNotice(ReserveEvent.Invalid, (AircraftDefinition) null, false, 0);
}
