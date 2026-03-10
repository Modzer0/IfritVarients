// Decompiled with JetBrains decompiler
// Type: NuclearOption.SavedMission.ObjectiveV2.Objectives.Waypoint
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

#nullable disable
namespace NuclearOption.SavedMission.ObjectiveV2.Objectives;

public class Waypoint : ISaveableData
{
  public readonly ValueWrapperGlobalPosition GlobalPosition = new ValueWrapperGlobalPosition();
  public readonly ValueWrapperFloat Range = new ValueWrapperFloat(50f);

  public Waypoint()
  {
  }

  public Waypoint(global::GlobalPosition globalPosition, float range = 50f)
  {
    this.GlobalPosition.SetValue(globalPosition, (object) this, true);
    this.Range.SetValue(range, (object) this, true);
  }

  public void FromObjectiveData(ObjectiveData data, MissionLookups lookups)
  {
    this.GlobalPosition.SetValue(new global::GlobalPosition(data.VectorValue), (object) this, true);
    this.Range.SetValue(data.FloatValue, (object) this, true);
  }

  public ObjectiveData ToObjectiveData()
  {
    return new ObjectiveData()
    {
      VectorValue = this.GlobalPosition.Value.AsVector3(),
      FloatValue = this.Range.Value
    };
  }

  public ObjectivePosition ToObjectivePosition()
  {
    return new ObjectivePosition((global::GlobalPosition) (ValueWrapper<global::GlobalPosition>) this.GlobalPosition, new float?((float) (ValueWrapper<float>) this.Range));
  }

  public override string ToString() => $"[Pos {this.GlobalPosition}, Range {this.Range}]";
}
