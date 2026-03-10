// Decompiled with JetBrains decompiler
// Type: TargetListSelector_UnitItem
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using UnityEngine;
using UnityEngine.UI;

#nullable disable
public class TargetListSelector_UnitItem : MonoBehaviour
{
  public TargetListSelector selector;
  public Unit unit;
  [SerializeField]
  private Text unitName;
  [SerializeField]
  private Text unitFaction;
  [SerializeField]
  private Text unitDist;
  private float lastUpdate;
  private float refreshRate = 1f;

  private void Start()
  {
  }

  private void Update()
  {
    if ((double) Time.timeSinceLevelLoad < (double) this.lastUpdate + (double) this.refreshRate)
      return;
    string str = "?";
    if ((Object) SceneSingleton<DynamicMap>.i.HQ == (Object) null || (Object) SceneSingleton<DynamicMap>.i.HQ == (Object) this.unit.NetworkHQ || (Object) SceneSingleton<DynamicMap>.i.HQ != (Object) this.unit.NetworkHQ && SceneSingleton<DynamicMap>.i.HQ.IsTargetPositionAccurate(this.unit, 100f))
      str = SceneSingleton<DynamicMap>.i.gridLabels.GetGridPosition(this.unit.GlobalPosition()) ?? "";
    if ((Object) SceneSingleton<DynamicMap>.i.HQ != (Object) null)
      str += (Object) SceneSingleton<CombatHUD>.i.aircraft != (Object) null ? " | " + UnitConverter.DistanceReading(FastMath.Distance(SceneSingleton<CombatHUD>.i.aircraft.GlobalPosition(), this.unit.GlobalPosition())) : "";
    this.unitDist.text = str;
    this.lastUpdate = Time.timeSinceLevelLoad;
  }

  public void ToggleSelect()
  {
    if ((Object) SceneSingleton<CombatHUD>.i.aircraft != (Object) null)
      SceneSingleton<CombatHUD>.i.DeSelectUnit(this.unit);
    else
      SceneSingleton<DynamicMap>.i.DeselectIcon(this.unit);
  }

  public void SetUnit(Unit u)
  {
    this.unit = u;
    this.unitName.text = this.unit.unitName;
    this.unitFaction.text = !((Object) this.unit.NetworkHQ != (Object) null) ? "N/A" : this.unit.NetworkHQ.faction.factionTag;
    Color green = Color.green;
    Color color = !((Object) SceneSingleton<DynamicMap>.i.HQ != (Object) null) ? ((Object) this.unit.NetworkHQ != (Object) null ? this.unit.NetworkHQ.faction.color : Color.white) : ((Object) this.unit.NetworkHQ == (Object) SceneSingleton<DynamicMap>.i.HQ ? GameAssets.i.HUDFriendly : GameAssets.i.HUDHostile);
    this.unitName.color = color;
    this.unitFaction.color = color;
    this.unitDist.color = color;
  }
}
