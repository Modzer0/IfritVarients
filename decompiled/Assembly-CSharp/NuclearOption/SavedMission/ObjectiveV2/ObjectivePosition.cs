// Decompiled with JetBrains decompiler
// Type: NuclearOption.SavedMission.ObjectiveV2.ObjectivePosition
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

#nullable disable
namespace NuclearOption.SavedMission.ObjectiveV2;

public readonly struct ObjectivePosition(GlobalPosition position, float? range)
{
  public readonly GlobalPosition Position = position;
  public readonly float? Range = range;
}
