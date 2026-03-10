// Decompiled with JetBrains decompiler
// Type: NuclearOption.MissionEditorScripts.EditorCursor
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
namespace NuclearOption.MissionEditorScripts;

public class EditorCursor : MonoBehaviour
{
  [SerializeField]
  private UnitSelection unitSelection;
  [SerializeField]
  private Renderer _renderer;
  [SerializeField]
  private EditorCursor.ColorMode colorMode;
  [SerializeField]
  private bool multiSelect;
  [SerializeField]
  private bool disableIfNoneSelected;
  [SerializeField]
  private bool followUnit;
  [Header("Hover Colors")]
  [SerializeField]
  private Color noUnit;
  [SerializeField]
  private Color noFaction;
  [SerializeField]
  private float emissionMultiplier = 1f;
  private Unit unit;
  private Material material;
  [NonSerialized]
  private readonly List<EditorCursor> multiSelectClones = new List<EditorCursor>(0);

  public bool IsClone { get; private set; }

  private void Awake() => this.material = this._renderer.material;

  private void Start()
  {
    if (this.IsClone)
      return;
    switch (this.colorMode)
    {
      case EditorCursor.ColorMode.OnSelect:
        this.unitSelection.OnSelect += new UnitSelection.SelectionChanged<SelectionDetails>(this.UnitSelection_OnSelect);
        break;
      case EditorCursor.ColorMode.OnHover:
        this.unitSelection.OnHover += new UnitSelection.SelectionChanged<SingleSelectionDetails>(this.UnitSelection_OnSelect);
        break;
    }
    this.UnitSelection_OnSelect(this.unitSelection.SelectionDetails);
  }

  private void OnDestroy()
  {
    switch (this.colorMode)
    {
      case EditorCursor.ColorMode.OnSelect:
        this.unitSelection.OnSelect -= new UnitSelection.SelectionChanged<SelectionDetails>(this.UnitSelection_OnSelect);
        break;
      case EditorCursor.ColorMode.OnHover:
        this.unitSelection.OnHover -= new UnitSelection.SelectionChanged<SingleSelectionDetails>(this.UnitSelection_OnSelect);
        break;
    }
    this.DestroyClones();
  }

  private void DestroyClones()
  {
    foreach (Component multiSelectClone in this.multiSelectClones)
      UnityEngine.Object.Destroy((UnityEngine.Object) multiSelectClone.gameObject);
    this.multiSelectClones.Clear();
  }

  private void UnitSelection_OnSelect(SelectionDetails selectionDetails)
  {
    this.DestroyClones();
    if (this.multiSelect && selectionDetails is MultiSelectSelectionDetails multi && multi.SelectionType == typeof (UnitSelectionDetails))
    {
      if (this.disableIfNoneSelected)
        this.gameObject.SetActive(true);
      this.MultiSelect(multi);
    }
    else if (selectionDetails is UnitSelectionDetails selectionDetails1)
    {
      if (this.disableIfNoneSelected)
        this.gameObject.SetActive(true);
      this.SetUnitColor(selectionDetails1.Unit, selectionDetails1.Faction);
    }
    else
    {
      if (this.disableIfNoneSelected)
        this.gameObject.SetActive(false);
      this.SetUnitColor((Unit) null, (Faction) null);
    }
  }

  private void MultiSelect(MultiSelectSelectionDetails multi)
  {
    UnitSelectionDetails selectionDetails1 = (UnitSelectionDetails) multi.Items[0];
    this.SetUnitColor(selectionDetails1.Unit, selectionDetails1.Faction);
    int num = multi.Items.Count - 1;
    for (int index = 0; index < num; ++index)
    {
      EditorCursor editorCursor = UnityEngine.Object.Instantiate<EditorCursor>(this, this.transform.parent);
      editorCursor.gameObject.SetActive(true);
      this.multiSelectClones.Add(editorCursor);
      editorCursor.IsClone = true;
      UnitSelectionDetails selectionDetails2 = (UnitSelectionDetails) multi.Items[index + 1];
      editorCursor.SetUnitColor(selectionDetails2.Unit, selectionDetails2.Faction);
    }
  }

  public void SetUnitColor(Unit unit, Faction faction)
  {
    this.material.SetColor("_EmissionColor", (!((UnityEngine.Object) unit == (UnityEngine.Object) null) ? (!((UnityEngine.Object) faction == (UnityEngine.Object) null) ? faction.color : this.noFaction) : this.noUnit) * this.emissionMultiplier);
    if (!this.followUnit)
      return;
    if ((UnityEngine.Object) unit != (UnityEngine.Object) null)
    {
      this.unit = unit;
      this.transform.localScale = Vector3.one * (unit.definition.length / 7f);
    }
    else
      this.unit = (Unit) null;
  }

  private void LateUpdate()
  {
    if (!this.followUnit)
      return;
    this.MoveSelectionSquare();
  }

  private void MoveSelectionSquare()
  {
    if ((UnityEngine.Object) this.unit == (UnityEngine.Object) null)
      return;
    Vector3 position;
    Quaternion rotation1;
    this.unit.transform.GetPositionAndRotation(out position, out rotation1);
    Quaternion rotation2 = Quaternion.Euler(0.0f, rotation1.eulerAngles.y, 0.0f);
    this.transform.SetPositionAndRotation(position, rotation2);
  }

  [Serializable]
  public enum ColorMode
  {
    OnSelect,
    OnHover,
  }
}
