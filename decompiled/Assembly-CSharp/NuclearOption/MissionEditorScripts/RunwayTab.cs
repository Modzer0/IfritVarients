// Decompiled with JetBrains decompiler
// Type: NuclearOption.MissionEditorScripts.RunwayTab
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.SavedMission;
using NuclearOption.SavedMission.ObjectiveV2;
using NuclearOption.SavedMission.ObjectiveV2.Objectives;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

#nullable disable
namespace NuclearOption.MissionEditorScripts;

public class RunwayTab : MonoBehaviour
{
  [SerializeField]
  private Button backButton;
  [SerializeField]
  private Button deleteButton;
  [SerializeField]
  private TMP_InputField nameField;
  [SerializeField]
  private Toggle reversableToggle;
  [SerializeField]
  private Toggle takeoffToggle;
  [SerializeField]
  private Toggle landingToggle;
  [SerializeField]
  private Toggle arrestorToggle;
  [SerializeField]
  private Toggle skiJumpToggle;
  [SerializeField]
  private Slider widthSlider;
  [SerializeField]
  private TextMeshProUGUI widthSliderLabel;
  [SerializeField]
  private Vector3DataField startField;
  [SerializeField]
  private Vector3DataField endField;
  [SerializeField]
  private PositionHandle positionHandlePrefab;
  private Airbase airbase;
  private SavedRunway runway;
  private PositionHandle startHandle;
  private PositionHandle endHandle;
  private ValueWrapperGlobalPosition startWrapper;
  private ValueWrapperGlobalPosition endWrapper;

  private void Awake()
  {
    this.nameField.onEndEdit.AddListener(new UnityAction<string>(this.NameChanged));
    this.reversableToggle.onValueChanged.AddListener(new UnityAction<bool>(this.ReversableChanged));
    this.takeoffToggle.onValueChanged.AddListener(new UnityAction<bool>(this.TakeoffChanged));
    this.landingToggle.onValueChanged.AddListener(new UnityAction<bool>(this.LandingChanged));
    this.arrestorToggle.onValueChanged.AddListener(new UnityAction<bool>(this.ArrestorChanged));
    this.skiJumpToggle.onValueChanged.AddListener(new UnityAction<bool>(this.SkiJumpChanged));
    this.widthSlider.onValueChanged.AddListener(new UnityAction<float>(this.WidthChanged));
    (this.startWrapper, this.startHandle) = this.CreateHandle(this.startField, new ValueWrapper<GlobalPosition>.OnChangeDelegate(this.StartChanged), "Start", Color.green);
    (this.endWrapper, this.endHandle) = this.CreateHandle(this.endField, new ValueWrapper<GlobalPosition>.OnChangeDelegate(this.EndChanged), "End", Color.red);
    this.backButton.onClick.AddListener(new UnityAction(this.BackClicked));
    this.deleteButton.onClick.AddListener(new UnityAction(this.DeleteClicked));
  }

  private void BackClicked()
  {
    SceneSingleton<UnitSelection>.i.ClearSelection();
    SceneSingleton<UnitSelection>.i.SetSelection((IEditorSelectable) this.airbase);
  }

  private void DeleteClicked()
  {
    this.airbase.SavedAirbase.runways.Remove(this.runway);
    this.BackClicked();
  }

  private void OnDestroy()
  {
    if ((UnityEngine.Object) this.startHandle != (UnityEngine.Object) null)
      UnityEngine.Object.Destroy((UnityEngine.Object) this.startHandle);
    if (!((UnityEngine.Object) this.endHandle != (UnityEngine.Object) null))
      return;
    UnityEngine.Object.Destroy((UnityEngine.Object) this.endHandle);
  }

  public void Setup(Airbase airbase, SavedRunway runway)
  {
    this.airbase = airbase;
    this.runway = runway;
    this.nameField.SetTextWithoutNotify(runway.Name);
    this.reversableToggle.SetIsOnWithoutNotify(runway.Reversable);
    this.takeoffToggle.SetIsOnWithoutNotify(runway.Takeoff);
    this.landingToggle.SetIsOnWithoutNotify(runway.Landing);
    this.arrestorToggle.SetIsOnWithoutNotify(runway.Arrestor);
    this.skiJumpToggle.SetIsOnWithoutNotify(runway.SkiJump);
    this.widthSlider.SetValueWithoutNotify(runway.Width);
    this.widthSliderLabel.text = runway.Width.ToString();
    this.startWrapper.SetValue(runway.Start, (object) this, false);
    this.endWrapper.SetValue(runway.End, (object) this, false);
  }

  private (ValueWrapperGlobalPosition wrapper, PositionHandle handle) CreateHandle(
    Vector3DataField field,
    ValueWrapper<GlobalPosition>.OnChangeDelegate onChange,
    string label,
    Color color)
  {
    ValueWrapperGlobalPosition wrapper = new ValueWrapperGlobalPosition();
    wrapper.RegisterOnChange((object) this, onChange);
    PositionHandle positionHandle = UnityEngine.Object.Instantiate<PositionHandle>(this.positionHandlePrefab);
    positionHandle.SetHue(color);
    positionHandle.Setup((IValueWrapper<GlobalPosition>) wrapper, (Func<string>) (() => label + "handle"), (Action) null);
    field.Setup(label, (IValueWrapper<Vector3>) wrapper);
    return (wrapper, positionHandle);
  }

  private void NameChanged(string value) => this.runway.Name = value;

  private void ReversableChanged(bool value) => this.runway.Reversable = value;

  private void TakeoffChanged(bool value) => this.runway.Takeoff = value;

  private void LandingChanged(bool value) => this.runway.Landing = value;

  private void ArrestorChanged(bool value) => this.runway.Arrestor = value;

  private void SkiJumpChanged(bool value) => this.runway.SkiJump = value;

  private void WidthChanged(float value)
  {
    this.runway.Width = value;
    this.widthSliderLabel.text = value.ToString();
  }

  private void StartChanged(GlobalPosition value) => this.runway.Start = value;

  private void EndChanged(GlobalPosition value) => this.runway.End = value;
}
