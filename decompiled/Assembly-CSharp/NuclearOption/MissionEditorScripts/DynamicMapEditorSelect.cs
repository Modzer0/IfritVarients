// Decompiled with JetBrains decompiler
// Type: NuclearOption.MissionEditorScripts.DynamicMapEditorSelect
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System.Collections.Generic;
using UnityEngine;

#nullable disable
namespace NuclearOption.MissionEditorScripts;

public class DynamicMapEditorSelect : MonoBehaviour
{
  [SerializeField]
  private UnitSelection unitSelection;

  private void Awake()
  {
    this.unitSelection.OnSelect += new UnitSelection.SelectionChanged<SelectionDetails>(this.UnitSelection_OnSelect);
  }

  private void OnDestroy()
  {
    this.unitSelection.OnSelect -= new UnitSelection.SelectionChanged<SelectionDetails>(this.UnitSelection_OnSelect);
  }

  private void UnitSelection_OnSelect(SelectionDetails details)
  {
    if (!((Object) SceneSingleton<DynamicMap>.i != (Object) null))
      return;
    SceneSingleton<DynamicMap>.i.DeselectAllIcons();
    if (details == null)
      return;
    SelectOne(details);

    static void SelectOne(SelectionDetails details)
    {
      switch (details)
      {
        case MultiSelectSelectionDetails selectionDetails1:
          using (List<SingleSelectionDetails>.Enumerator enumerator = selectionDetails1.Items.GetEnumerator())
          {
            while (enumerator.MoveNext())
              SelectOne((SelectionDetails) enumerator.Current);
            break;
          }
        case AirbaseSelectionDetails selectionDetails2:
          SceneSingleton<DynamicMap>.i.SelectIcon(selectionDetails2.Airbase);
          break;
        case UnitSelectionDetails selectionDetails3:
          SceneSingleton<DynamicMap>.i.SelectIcon(selectionDetails3.Unit);
          break;
      }
    }
  }
}
