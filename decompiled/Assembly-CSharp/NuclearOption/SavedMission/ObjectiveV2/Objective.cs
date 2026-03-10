// Decompiled with JetBrains decompiler
// Type: NuclearOption.SavedMission.ObjectiveV2.Objective
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.MissionEditorScripts;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#nullable disable
namespace NuclearOption.SavedMission.ObjectiveV2;

public abstract class Objective : ISaveableReference, IHasFaction
{
  public SavedObjective SavedObjective;
  public ObjectiveStatus Status;
  private FactionHQ hq;
  public readonly List<Outcome> Outcomes = new List<Outcome>();
  public MissionManager MissionManager;

  public virtual float CompletePercent => 0.0f;

  public virtual IObjectiveEditorUpdate CreateEditorUpdate(Canvas canvas, UIPrefabs prefabs)
  {
    return (IObjectiveEditorUpdate) null;
  }

  public virtual string FactionLabelOverride => (string) null;

  bool ISaveableReference.CanBeSorted => true;

  public FactionHQ FactionHQ
  {
    get
    {
      FactionHQ hq = this.hq;
      return this.hq;
    }
    set => this.hq = value;
  }

  string IHasFaction.FactionName => this.SavedObjective.Faction;

  string ISaveableReference.UniqueName => this.SavedObjective.UniqueName;

  bool ISaveableReference.Destroyed { get; set; }

  bool ISaveableReference.CanBeReference
  {
    get => this.SavedObjective.UniqueName != MissionObjectivesFactory.MissionStartName;
  }

  public virtual bool NeedsFaction => false;

  public abstract void OnStart();

  public virtual void Cleanup()
  {
  }

  public abstract bool UpdateAndCheck();

  public abstract void ClientOnlyUpdate();

  public void LoadNew(SavedObjective savedObjective)
  {
    this.SavedObjective = savedObjective;
    this.WriteObjective(new ReadWriteObjective(ReadWriteObjective.Mode.Read, ref this.SavedObjective.Data, new MissionLookups((LoadErrors) null)));
  }

  public void Load(MissionLookups lookups)
  {
    this.WriteObjective(new ReadWriteObjective(ReadWriteObjective.Mode.Read, ref this.SavedObjective.Data, lookups));
    this.LoadOutcomes(lookups.Outcomes);
  }

  public void Save()
  {
    this.WriteObjective(new ReadWriteObjective(ReadWriteObjective.Mode.Write, ref this.SavedObjective.Data, (MissionLookups) null));
    this.SaveOutcomes();
    this.SavedObjective.TypeName = this.SavedObjective.Type.ToString();
  }

  protected abstract void WriteObjective(ReadWriteObjective writer);

  private void LoadOutcomes(Dictionary<string, Outcome> outcomeLookup)
  {
    if (this.SavedObjective.Outcomes == null)
      return;
    this.Outcomes.AddRange(this.SavedObjective.Outcomes.Select<string, Outcome>((Func<string, Outcome>) (x => outcomeLookup[x])));
  }

  private void SaveOutcomes()
  {
    this.SavedObjective.Outcomes = this.Outcomes.Select<Outcome, string>((Func<Outcome, string>) (x => x.SavedOutcome.UniqueName)).ToList<string>();
  }

  public void ReferenceDestroyed(ISaveableReference reference)
  {
    if (reference is Outcome outcome)
      this.Outcomes.Remove(outcome);
    this.DataReferenceDestroyed(reference);
  }

  protected abstract void DataReferenceDestroyed(ISaveableReference reference);

  public void Complete()
  {
    foreach (Outcome outcome in this.Outcomes)
      outcome.Complete(this);
  }

  public override string ToString()
  {
    return $"[{this.SavedObjective.UniqueName} Type={this.SavedObjective.Type} Faction={((UnityEngine.Object) this.FactionHQ != (UnityEngine.Object) null ? this.FactionHQ.faction.factionName.AddColor(this.FactionHQ.faction.color) : "None")} Hidden={this.SavedObjective.Hidden}]";
  }

  public abstract void DrawData(DataDrawer drawer);

  public string ToUIString(bool oneLine = false)
  {
    SavedObjective savedObjective = this.SavedObjective;
    string str1 = string.IsNullOrEmpty(savedObjective.DisplayName) ? "<no name>".AddColor(new Color(0.5f, 0.5f, 0.5f)) : savedObjective.DisplayName;
    string str2 = savedObjective.UniqueName.AddColor(new Color(0.7f, 0.7f, 0.7f));
    string str3 = savedObjective.Type.ToString();
    string str4 = str3.AddColor(ColorLog.ColorFromName(str3, 0.2f, 1f));
    string uiString = $"{str1} - [{str2}] - Type:{str4}";
    if (oneLine)
      return uiString;
    FactionHQ factionHq = this.FactionHQ;
    string str5 = (UnityEngine.Object) factionHq != (UnityEngine.Object) null ? factionHq.faction.factionName.AddColor(factionHq.faction.color) : "None";
    string str6 = savedObjective.Hidden ? "true".AddColor(new Color(0.4f, 1f, 0.4f)) : "false".AddColor(new Color(1f, 0.4f, 0.4f));
    int num = SaveHelper.CountStartedBy(MissionManager.CurrentMission.Objectives, this);
    return $"{uiString}\nRef:{num} - Type:{str4} - Faction:{str5} - Hidden:{str6}";
  }

  public virtual void ReceiveNetworkData(List<int> data)
  {
  }
}
