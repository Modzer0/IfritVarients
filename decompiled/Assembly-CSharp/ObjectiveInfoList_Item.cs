// Decompiled with JetBrains decompiler
// Type: ObjectiveInfoList_Item
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using UnityEngine;
using UnityEngine.UI;

#nullable disable
public class ObjectiveInfoList_Item : MonoBehaviour
{
  public Text OBJ_Name;
  public Text OBJ_Pos;
  public Text OBJ_Dist;
  private GlobalPosition currentPos;

  public void Refresh(MissionPosition.PositionResult position)
  {
    this.currentPos = position.Position;
    this.OBJ_Name.text = position.Objective.SavedObjective.TypeName;
    this.OBJ_Pos.text = SceneSingleton<DynamicMap>.i.gridLabels.GetGridPosition(this.currentPos);
    if ((Object) SceneSingleton<CombatHUD>.i.aircraft == (Object) null)
      this.OBJ_Dist.text = "-";
    else
      this.OBJ_Dist.text = UnitConverter.DistanceReading(FastMath.Distance(SceneSingleton<CombatHUD>.i.aircraft.GlobalPosition(), this.currentPos));
  }

  public void OnObjectiveComplete()
  {
    this.OBJ_Pos.text = "-";
    this.OBJ_Dist.text = "-";
    this.enabled = false;
  }

  public void OnButtonClick() => SceneSingleton<DynamicMap>.i.SetMapTarget(this.currentPos);
}
