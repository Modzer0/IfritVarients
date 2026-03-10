// Decompiled with JetBrains decompiler
// Type: NuclearOption.SavedMission.MissionEnvironment
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;

#nullable disable
namespace NuclearOption.SavedMission;

[Serializable]
public class MissionEnvironment
{
  public float timeOfDay = 10f;
  public float timeFactor;
  public float weatherIntensity;
  public float cloudAltitude = 1800f;
  public float windSpeed;
  public float windTurbulence;
  public float windHeading;
  public float windRandomHeading;
  public float moonPhase = 14f;
}
