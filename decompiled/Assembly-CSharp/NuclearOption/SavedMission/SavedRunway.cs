// Decompiled with JetBrains decompiler
// Type: NuclearOption.SavedMission.SavedRunway
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.SavedMission.ObjectiveV2;
using System;

#nullable disable
namespace NuclearOption.SavedMission;

[Serializable]
public class SavedRunway : ISaveableReference
{
  public string Name;
  public bool Reversable;
  public bool Takeoff;
  public bool Landing;
  public bool Arrestor;
  public bool SkiJump;
  public float Width;
  public GlobalPosition Start;
  public GlobalPosition End;
  public GlobalPosition[] exitPoints = new GlobalPosition[0];

  bool ISaveableReference.Destroyed { get; set; }

  string ISaveableReference.UniqueName => this.Name;

  bool ISaveableReference.CanBeReference => false;

  bool ISaveableReference.CanBeSorted => true;

  public string ToUIString(bool oneLine = false)
  {
    string str = "Runway " + this.Name;
    return oneLine ? str : $"{str}\n{this.Start.AsVector3()} -> {this.End.AsVector3()}";
  }
}
