// Decompiled with JetBrains decompiler
// Type: IRearmable
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;

#nullable disable
public interface IRearmable
{
  event Action<RearmEventArgs> OnRearm;

  void RequestRearm();

  void Rearm(RearmEventArgs args);

  bool CanRearm(bool aircraftRearm, bool vehicleRearm, bool shipRearm);
}
