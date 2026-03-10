// Decompiled with JetBrains decompiler
// Type: NuclearOption.SavedMission.ObjectiveV2.Objectives.DialogueBoxObjective
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.MissionEditorScripts;
using System;
using Unity.Profiling;

#nullable disable
namespace NuclearOption.SavedMission.ObjectiveV2.Objectives;

public class DialogueBoxObjective : Objective
{
  private static readonly ProfilerMarker updateAndCheckMarker = new ProfilerMarker("DialogueBoxObjectiveUpdateAndCheck");
  private string title;
  private string body;
  private string button;
  private readonly ValueWrapperBool factionOnly = new ValueWrapperBool();
  private bool buttonPressed;
  private DialogueBox box;

  public override bool NeedsFaction => this.factionOnly.Value;

  public override float CompletePercent => 0.0f;

  protected override void WriteObjective(ReadWriteObjective writer)
  {
    writer.String(ref this.title);
    writer.String(ref this.body);
    writer.String(ref this.button, "ok");
    writer.Bool(ref this.factionOnly.GetValueRef());
  }

  protected override void DataReferenceDestroyed(ISaveableReference reference)
  {
  }

  public override void OnStart()
  {
    if (!this.MissionManager.IsServer)
      return;
    this.box = MissionMessages.ShowDialogue(this.title, this.body, this.button, !(bool) (ValueWrapper<bool>) this.factionOnly ? (FactionHQ) null : this.FactionHQ);
    this.box.ButtonPressed += new Action<string>(this.OnButtonPressed);
  }

  public override void ClientOnlyUpdate()
  {
  }

  public override bool UpdateAndCheck()
  {
    using (DialogueBoxObjective.updateAndCheckMarker.Auto())
      return this.buttonPressed;
  }

  private void OnButtonPressed(string title)
  {
    if (title != this.title)
      return;
    this.buttonPressed = true;
    this.box.ButtonPressed -= new Action<string>(this.OnButtonPressed);
    MissionMessages.HideDialogue();
  }

  public override void DrawData(DataDrawer drawer)
  {
    drawer.InstantiateWithParent<StringDataField>(drawer.Prefabs.StringFieldPrefab).Setup("Title", this.title, (Action<string>) (v => this.title = v));
    drawer.InstantiateWithParent<StringDataField>(drawer.Prefabs.StringFieldPrefab).Setup("Body", this.body, (Action<string>) (v => this.body = v), new int?(5));
    drawer.InstantiateWithParent<StringDataField>(drawer.Prefabs.StringFieldPrefab).Setup("Ok Button", this.button, (Action<string>) (v => this.button = v));
    drawer.InstantiateWithParent<BoolDataField>(drawer.Prefabs.BoolFieldPrefab).Setup("Faction Only", (IValueWrapper<bool>) this.factionOnly);
    drawer.TrackChanges((IValueWrapper) this.factionOnly);
  }
}
