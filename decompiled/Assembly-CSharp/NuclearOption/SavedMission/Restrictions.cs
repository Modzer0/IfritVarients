// Decompiled with JetBrains decompiler
// Type: NuclearOption.SavedMission.Restrictions
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;

#nullable disable
namespace NuclearOption.SavedMission;

[Serializable]
public class Restrictions
{
  public List<string> aircraft = new List<string>();
  public List<string> weapons = new List<string>();
}
