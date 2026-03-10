// Decompiled with JetBrains decompiler
// Type: NuclearOption.SavedMission.ObjectiveV2.Outcome
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.MissionEditorScripts;
using UnityEngine;

#nullable disable
namespace NuclearOption.SavedMission.ObjectiveV2;

public abstract class Outcome : ISaveableReference
{
  public SavedOutcome SavedOutcome;

  string ISaveableReference.UniqueName => this.SavedOutcome.UniqueName;

  bool ISaveableReference.Destroyed { get; set; }

  bool ISaveableReference.CanBeReference => true;

  bool ISaveableReference.CanBeSorted => true;

  public abstract void Complete(Objective completedObjective);

  public void LoadNew(SavedOutcome savedOutcome)
  {
    this.SavedOutcome = savedOutcome;
    this.WriteOutcome(new ReadWriteObjective(ReadWriteObjective.Mode.Read, ref this.SavedOutcome.Data, new MissionLookups((LoadErrors) null)));
  }

  public void Load(MissionLookups lookups)
  {
    this.WriteOutcome(new ReadWriteObjective(ReadWriteObjective.Mode.Read, ref this.SavedOutcome.Data, lookups));
  }

  public void Save()
  {
    this.WriteOutcome(new ReadWriteObjective(ReadWriteObjective.Mode.Write, ref this.SavedOutcome.Data, (MissionLookups) null));
    this.SavedOutcome.TypeName = this.SavedOutcome.Type.ToString();
  }

  protected abstract void WriteOutcome(ReadWriteObjective writer);

  public abstract void ReferenceDestroyed(ISaveableReference reference);

  public override string ToString()
  {
    return $"[{this.SavedOutcome.UniqueName} Type={this.SavedOutcome.Type}]";
  }

  public abstract void DrawData(DataDrawer drawer);

  public string ToUIString(bool oneLine = false)
  {
    SavedOutcome savedOutcome = this.SavedOutcome;
    string str1 = savedOutcome.UniqueName.AddColor(new Color(0.7f, 0.7f, 0.7f));
    string str2 = savedOutcome.Type.ToString();
    string str3 = str2.AddColor(ColorLog.ColorFromName(str2, 0.8f, 1f));
    string uiString = $"[{str1}] - Type:{str3}";
    if (oneLine)
      return uiString;
    int num = SaveHelper.CountUsedBy(MissionManager.CurrentMission.Objectives, this);
    return $"{uiString}\nRef:{num}";
  }
}
