// Decompiled with JetBrains decompiler
// Type: NuclearOption.MissionEditorScripts.OpenUnitTabOnSelect
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.MissionEditorScripts.Buttons;
using UnityEngine;

#nullable disable
namespace NuclearOption.MissionEditorScripts;

public class OpenUnitTabOnSelect : MonoBehaviour
{
  [SerializeField]
  private UnitSelection unitSelection;
  [SerializeField]
  private EditorTabs editorTabs;
  [Header("Prefabs")]
  [SerializeField]
  private GameObject unitPanelPrefab;
  [SerializeField]
  private GameObject airbasePrefab;

  private void Awake()
  {
    this.unitSelection.OnSelect += new UnitSelection.SelectionChanged<SelectionDetails>(this.UnitSelection_onSelect);
  }

  private void UnitSelection_onSelect(SelectionDetails selectionDetails)
  {
    switch (selectionDetails)
    {
      case UnitSelectionDetails _:
label_3:
        this.OnSelectUnit();
        break;
      case MultiSelectSelectionDetails selectionDetails1:
        if (!(selectionDetails1.SelectionType == typeof (UnitSelectionDetails)))
          break;
        goto label_3;
      case AirbaseSelectionDetails _:
        this.OnSelectAirbase();
        break;
    }
  }

  private void OnSelectUnit()
  {
    UnitPanel foundPanel;
    if (this.editorTabs.TryGetOpenTab<UnitPanel>(out foundPanel))
    {
      foundPanel.SelectedRefreshed();
    }
    else
    {
      this.editorTabs.HideTab(false);
      this.editorTabs.ToggleTabPrefab((HighlightButton) null, this.unitPanelPrefab, false);
    }
  }

  private void OnSelectAirbase()
  {
    this.editorTabs.HideTab(false);
    this.editorTabs.ToggleTabPrefab((HighlightButton) null, this.airbasePrefab, false);
  }
}
