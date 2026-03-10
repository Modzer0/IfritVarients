// Decompiled with JetBrains decompiler
// Type: NuclearOption.MissionEditorScripts.Buttons.ShowTextOnUnitHover
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

#nullable disable
namespace NuclearOption.MissionEditorScripts.Buttons;

public class ShowTextOnUnitHover : MonoBehaviour
{
  [SerializeField]
  private UnitSelection unitSelection;
  [SerializeField]
  private HoverText hoverText;
  private SingleSelectionDetails details;
  private Camera _camera;

  private void Awake()
  {
    this.unitSelection.OnHover += new UnitSelection.SelectionChanged<SingleSelectionDetails>(this.OnHover);
    this._camera = Camera.main;
  }

  private void OnDestroy()
  {
    this.unitSelection.OnHover -= new UnitSelection.SelectionChanged<SingleSelectionDetails>(this.OnHover);
  }

  private void OnHover(SingleSelectionDetails details)
  {
    this.details = details;
    if (details != null)
      this.hoverText.Show((object) this, details.DisplayName);
    else
      this.hoverText.Hide((object) this);
  }

  private void Update()
  {
    if (this.details == null || this.details.PositionWrapper == null)
      return;
    this.hoverText.Move((object) this, (Vector2) this._camera.WorldToScreenPoint(this.details.PositionWrapper.Value.ToLocalPosition()));
  }
}
