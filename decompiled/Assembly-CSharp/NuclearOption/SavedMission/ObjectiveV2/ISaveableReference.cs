// Decompiled with JetBrains decompiler
// Type: NuclearOption.SavedMission.ObjectiveV2.ISaveableReference
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

#nullable disable
namespace NuclearOption.SavedMission.ObjectiveV2;

public interface ISaveableReference
{
  bool CanBeSorted { get; }

  bool Destroyed { get; set; }

  string UniqueName { get; }

  bool CanBeReference { get; }

  string ToUIString(bool oneLine = false);
}
