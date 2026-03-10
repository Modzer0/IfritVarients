// Decompiled with JetBrains decompiler
// Type: NuclearOption.MissionEditorScripts.InventoryInspectorItem
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.SavedMission;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

#nullable disable
namespace NuclearOption.MissionEditorScripts;

public class InventoryInspectorItem : MonoBehaviour
{
  [SerializeField]
  private Button addButton;
  [SerializeField]
  private Button removeButton;
  [SerializeField]
  private TextMeshProUGUI unitName;
  private InventoryInspector inventoryInspector;

  public UnitDefinition UnitDefinition { get; private set; }

  private void Awake()
  {
    this.addButton.onClick.AddListener(new UnityAction(this.AddOne));
    this.removeButton.onClick.AddListener(new UnityAction(this.RemoveOne));
  }

  public void SetEntry(InventoryInspector inventoryInspector, StoredUnitCount supply)
  {
    this.inventoryInspector = inventoryInspector;
    this.UnitDefinition = Encyclopedia.Lookup[supply.UnitType];
    this.unitName.text = $"{this.UnitDefinition.unitName}[{supply.Count}]";
  }

  public void AddOne() => this.inventoryInspector.AddOne(this);

  public void RemoveOne() => this.inventoryInspector.RemoveOne(this);
}
