// Decompiled with JetBrains decompiler
// Type: NuclearOption.SavedMission.ObjectiveV2.MissionLoadException
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;

#nullable disable
namespace NuclearOption.SavedMission.ObjectiveV2;

public class MissionLoadException : Exception
{
  public readonly LoadErrors LoadErrors;

  public MissionLoadException(LoadErrors errors)
    : base($"Failed to load mission because of {errors.Exceptions.Count} errors")
  {
    this.LoadErrors = errors;
  }
}
