// Decompiled with JetBrains decompiler
// Type: NuclearOption.SavedMission.ObjectiveV2.Objectives.PositionHandle
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.MissionEditorScripts;
using System;
using System.Linq;
using UnityEngine;

#nullable disable
namespace NuclearOption.SavedMission.ObjectiveV2.Objectives;

public class PositionHandle : MonoBehaviour, IEditorSelectable
{
  [SerializeField]
  private MeshRenderer renderer;
  [SerializeField]
  private bool scaleWithDistance;
  [SerializeField]
  private AnimationCurve scaleCurve;
  [SerializeField]
  private Color normalColor;
  [SerializeField]
  private Color hoverColor;
  [SerializeField]
  private Color selectColor;
  private Func<string> getParentName;
  private float oldScale;
  private Action destroyCallback;

  public IValueWrapper<GlobalPosition> PositionWrapper { get; private set; }

  public bool Selected { get; private set; }

  public bool Hover { get; private set; }

  public void SetHue(Color hueColor)
  {
    float H;
    Color.RGBToHSV(hueColor, out H, out float _, out float _);
    this.normalColor = this.normalColor.ChangeHue(H);
    this.hoverColor = this.hoverColor.ChangeHue(H);
    this.selectColor = this.selectColor.ChangeHue(H);
    this.SetColor(this.normalColor);
  }

  public void Setup(
    IValueWrapper<GlobalPosition> wrapper,
    Func<string> getParentName,
    Action destroyCallback)
  {
    if (this.PositionWrapper != wrapper && this.Selected)
      SceneSingleton<UnitSelection>.i.ClearSelection((IEditorSelectable) this);
    this.destroyCallback = destroyCallback;
    this.PositionWrapper = wrapper;
    wrapper.RegisterOnChange((object) this, new ValueWrapper<GlobalPosition>.OnChangeDelegate(this.PositionChanged));
    this.PositionChanged(wrapper.Value);
    this.getParentName = getParentName;
    this.gameObject.SetActive(true);
    this.SetColor(this.normalColor);
  }

  private void Awake()
  {
    SceneSingleton<UnitSelection>.i.OnSelect += new UnitSelection.SelectionChanged<SelectionDetails>(this.SelectionChanged);
    SceneSingleton<UnitSelection>.i.OnHover += new UnitSelection.SelectionChanged<SingleSelectionDetails>(this.HoverChanged);
  }

  private void OnDestroy()
  {
    SceneSingleton<UnitSelection>.i.OnSelect -= new UnitSelection.SelectionChanged<SelectionDetails>(this.SelectionChanged);
    SceneSingleton<UnitSelection>.i.OnHover -= new UnitSelection.SelectionChanged<SingleSelectionDetails>(this.HoverChanged);
    if (this.PositionWrapper != null)
      this.PositionWrapper.UnregisterOnChange((object) this);
    if (!this.Selected)
      return;
    SceneSingleton<UnitSelection>.i.ClearSelection((IEditorSelectable) this);
  }

  private void PositionChanged(GlobalPosition newValue)
  {
    this.transform.position = newValue.ToLocalPosition();
    Physics.SyncTransforms();
  }

  public void Hide()
  {
    if (this.PositionWrapper != null)
      this.PositionWrapper.UnregisterOnChange((object) this);
    this.PositionWrapper = (IValueWrapper<GlobalPosition>) null;
    this.gameObject.SetActive(false);
    if (!this.Selected)
      return;
    SceneSingleton<UnitSelection>.i.ClearSelection((IEditorSelectable) this);
  }

  private void Update()
  {
    if (!this.scaleWithDistance)
      return;
    float num = this.scaleCurve.Evaluate(FastMath.Distance(this.PositionWrapper.Value, SceneSingleton<CameraStateManager>.i.mainCamera.transform.position.ToGlobalPosition()));
    if ((double) num == (double) this.oldScale)
      return;
    this.transform.localScale = Vector3.one * num;
    Physics.SyncTransforms();
    this.oldScale = num;
  }

  private void SelectionChanged(SelectionDetails details)
  {
    bool flag;
    switch (details)
    {
      case SingleSelectionDetails selectionDetails1:
        flag = selectionDetails1?.Source == this;
        break;
      case MultiSelectSelectionDetails selectionDetails2:
        flag = selectionDetails2.Items.Any<SingleSelectionDetails>((Func<SingleSelectionDetails, bool>) (x => x.Source == this));
        break;
      default:
        flag = false;
        break;
    }
    if (flag)
    {
      this.Selected = true;
      this.SetColor(this.selectColor);
    }
    else
    {
      this.Selected = false;
      this.SetColor(this.normalColor);
    }
  }

  private void HoverChanged(SingleSelectionDetails details)
  {
    if (this.Selected)
      return;
    if (details?.Source == this)
      this.SetColor(this.hoverColor);
    else
      this.SetColor(this.normalColor);
  }

  private void SetColor(Color color) => this.renderer.material.color = color;

  public string GetDisplayName()
  {
    return $"{this.getParentName()} - {this.PositionWrapper.Value.AsVector3()}";
  }

  SingleSelectionDetails IEditorSelectable.CreateSelectionDetails()
  {
    return (SingleSelectionDetails) new PositionSelectionDetails(this, this.destroyCallback);
  }
}
