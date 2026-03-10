// Decompiled with JetBrains decompiler
// Type: NuclearOption.SavedMission.MissionObjective
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
namespace NuclearOption.SavedMission;

[Obsolete("Use V2 instead", true)]
[Serializable]
public class MissionObjective
{
  public string objectiveName;
  public string message;
  public bool positionTrigger;
  public bool victoryObjective;
  public bool nonSequentialObjective;
  public float triggerRange;
  public Vector3 position;
  public List<string> targetUnits = new List<string>();
}
