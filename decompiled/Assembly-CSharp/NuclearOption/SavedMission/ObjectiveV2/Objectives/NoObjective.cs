// Decompiled with JetBrains decompiler
// Type: NuclearOption.SavedMission.ObjectiveV2.Objectives.NoObjective
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.MissionEditorScripts;
using Unity.Profiling;

#nullable disable
namespace NuclearOption.SavedMission.ObjectiveV2.Objectives;

public class NoObjective : Objective
{
  private static readonly ProfilerMarker updateAndCheckMarker = new ProfilerMarker("NoObjectiveUpdateAndCheck");

  protected override void WriteObjective(ReadWriteObjective writer)
  {
  }

  protected override void DataReferenceDestroyed(ISaveableReference reference)
  {
  }

  public override void OnStart()
  {
  }

  public override void ClientOnlyUpdate()
  {
  }

  public override bool UpdateAndCheck()
  {
    using (NoObjective.updateAndCheckMarker.Auto())
      return true;
  }

  public override void DrawData(DataDrawer drawer) => drawer.Nothing();
}
