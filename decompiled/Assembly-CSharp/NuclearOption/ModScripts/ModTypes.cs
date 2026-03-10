// Decompiled with JetBrains decompiler
// Type: NuclearOption.ModScripts.ModTypes
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.ModScripts.Impl;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace NuclearOption.ModScripts;

public static class ModTypes
{
  public static readonly ModType Missions = (ModType) new MissionMod();
  public static readonly ModType AircraftLivery = (ModType) new AircraftLiveryMod();
  public static readonly ModType[] ModTypesArray = new ModType[2]
  {
    ModTypes.Missions,
    ModTypes.AircraftLivery
  };

  public static ModType FromTag(string tag)
  {
    return ((IEnumerable<ModType>) ModTypes.ModTypesArray).First<ModType>((Func<ModType, bool>) (x => x.Tag == tag));
  }
}
