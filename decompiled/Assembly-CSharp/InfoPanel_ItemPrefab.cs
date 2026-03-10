// Decompiled with JetBrains decompiler
// Type: InfoPanel_ItemPrefab
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.Networking;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#nullable disable
public class InfoPanel_ItemPrefab : MonoBehaviour
{
  private Player player;
  [SerializeField]
  private Text text;
  [SerializeField]
  private Text value;
  [SerializeField]
  private Image icon;
  [SerializeField]
  private List<UnitDefinition> listUnitDefinitions = new List<UnitDefinition>();
  private float numberValue;
  private InfoPanel_Faction factionInfoPanel;
  private Color positiveColor = Color.green;
  private Color zeroColor = Color.grey;
  private Color negativeColor = Color.red;

  public void Set(InfoPanel_Faction panel) => this.factionInfoPanel = panel;

  public void Refresh(float? number, bool isLoss, bool isValue)
  {
    if (number.HasValue)
    {
      this.numberValue = number.Value;
      this.value.text = isValue ? UnitConverter.ValueReading(number.Value) : number.ToString();
      this.SetColor(number.Value, isLoss);
    }
    else
    {
      this.numberValue = 0.0f;
      this.value.text = isValue ? "0" : "-";
    }
  }

  public void RefreshDefinition(InfoPanel_Faction.DisplayType type)
  {
    if (this.listUnitDefinitions.Count == 0)
      return;
    int num = 0;
    bool isLoss = false;
    for (int index = 0; index < this.listUnitDefinitions.Count; ++index)
    {
      UnitDefinition listUnitDefinition = this.listUnitDefinitions[index];
      switch (type)
      {
        case InfoPanel_Faction.DisplayType.Forces:
          num += this.factionInfoPanel.factionHQ.missionStatsTracker.GetCurrentUnits(listUnitDefinition);
          break;
        case InfoPanel_Faction.DisplayType.Reserves:
          num += this.factionInfoPanel.factionHQ.GetUnitSupply(listUnitDefinition);
          break;
        case InfoPanel_Faction.DisplayType.Losses:
          num += this.factionInfoPanel.factionHQ.missionStatsTracker.GetLostUnits(listUnitDefinition);
          isLoss = true;
          break;
      }
    }
    this.Refresh(new float?((float) num), isLoss, false);
  }

  public void SetColor(float number, bool isLoss)
  {
    Color color = isLoss ? ((double) number > 0.0 ? this.negativeColor : this.zeroColor) : ((double) number > 0.0 ? this.positiveColor : this.zeroColor);
    this.icon.color = color;
    this.text.color = color;
    this.value.color = color;
  }

  public float GetValue() => this.numberValue;
}
