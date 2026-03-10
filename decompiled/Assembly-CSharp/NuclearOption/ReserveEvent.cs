// Decompiled with JetBrains decompiler
// Type: NuclearOption.ReserveEvent
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

#nullable disable
namespace NuclearOption;

public enum ReserveEvent : byte
{
  Invalid,
  accepted,
  acceptedInQueue,
  rejectedDuplicate,
  rejectedRank,
  rejectedAfford,
  rejectedOwned,
  rejectedPossessesReserved,
  cancelledAfford,
  cancelledOwned,
  cancelledRank,
  granted,
}
