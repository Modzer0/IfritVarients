// Decompiled with JetBrains decompiler
// Type: NuclearOption.MissionEditorScripts.FloatDataField
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.SavedMission.ObjectiveV2;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

#nullable disable
namespace NuclearOption.MissionEditorScripts;

public class FloatDataField : DataField, IDataField<float>
{
  [SerializeField]
  private TMP_InputField text;
  [SerializeField]
  private Slider slider;
  [SerializeField]
  private float textWidthNoSlider = 240f;
  [SerializeField]
  private float textWidthWithSlider = 80f;
  private IValueWrapper<float> wrapper;
  private bool useSlider;
  private float? valueSteps;
  public string TextFormat;

  protected override void SetFieldInteractable(bool value)
  {
    this.text.interactable = value;
    this.slider.interactable = value;
  }

  protected override void AwakeSetup()
  {
    this.text.onValueChanged.AddListener(new UnityAction<string>(this.Text_OnValueChanged));
    this.slider.onValueChanged.AddListener(new UnityAction<float>(this.Slider_OnValueChanged));
  }

  public void Setup(string label, ValueWrapperInt wrapper, FloatDataField.IntSettings? settings = null)
  {
    this.Setup(label, (IValueWrapper<float>) wrapper, (FloatDataField.DataSettings?) settings);
  }

  void IDataField<float>.Setup(string label, IValueWrapper<float> wrapper)
  {
    this.Setup(label, wrapper);
  }

  public void Setup(
    string label,
    IValueWrapper<float> wrapper,
    FloatDataField.FloatSettings? settings = null)
  {
    this.Setup(label, wrapper, (FloatDataField.DataSettings?) settings);
  }

  public void Setup(
    string label,
    IValueWrapper<float> wrapper,
    FloatDataField.DataSettings? settings)
  {
    this.wrapper?.UnregisterOnChange((object) this);
    this.wrapper = wrapper;
    wrapper.RegisterOnChange((object) this, new ValueWrapper<float>.OnChangeDelegate(this.Wrapper_OnChange));
    this.SetupFields(label, settings);
    this.UpdateFields(wrapper.Value);
    this.Interactable = true;
  }

  private void OnDestroy() => this.wrapper?.UnregisterOnChange((object) this);

  public void SetupReadOnly(string label, int value, FloatDataField.IntSettings? settings = null)
  {
    this.SetupReadOnly(label, (float) value, (FloatDataField.DataSettings?) settings);
  }

  public void SetupReadOnly(string label, float value, FloatDataField.FloatSettings? settings = null)
  {
    this.SetupReadOnly(label, value, (FloatDataField.DataSettings?) settings);
  }

  public void SetupReadOnly(string label, float value, FloatDataField.DataSettings? settings)
  {
    this.wrapper?.UnregisterOnChange((object) this);
    this.wrapper = (IValueWrapper<float>) null;
    this.SetupFields(label, settings);
    this.UpdateFields(value);
    this.slider.interactable = false;
  }

  private void SetupFields(string label, FloatDataField.DataSettings? settingsNullable)
  {
    this.label.text = label;
    FloatDataField.DataSettings valueOrDefault = settingsNullable.GetValueOrDefault();
    this.SetContentType(valueOrDefault.WholeNumber);
    this.valueSteps = valueOrDefault.Steps;
    this.TextFormat = valueOrDefault.TextFormat;
    if (valueOrDefault.Slider.HasValue)
    {
      this.SetSliderSettingsInternal(valueOrDefault.WholeNumber, valueOrDefault.Slider.Value);
    }
    else
    {
      this.useSlider = false;
      RectTransform transform = (RectTransform) this.text.transform;
      Vector2 sizeDelta = transform.sizeDelta;
      this.slider.gameObject.SetActive(false);
      sizeDelta.x = this.textWidthNoSlider;
      transform.sizeDelta = sizeDelta;
    }
  }

  public void SetContentType(bool wholeNumbers)
  {
    this.text.contentType = wholeNumbers ? TMP_InputField.ContentType.IntegerNumber : TMP_InputField.ContentType.DecimalNumber;
  }

  public void SetSteps(float? steps) => this.valueSteps = steps;

  public void SetSliderSettings(FloatDataField.IntSlider sliderSettings)
  {
    this.SetSliderSettingsInternal(true, (FloatDataField.FloatSlider) sliderSettings);
  }

  public void SetSliderSettings(FloatDataField.FloatSlider sliderSettings)
  {
    this.SetSliderSettingsInternal(false, sliderSettings);
  }

  private void SetSliderSettingsInternal(
    bool wholeNumbers,
    FloatDataField.FloatSlider sliderSettings)
  {
    this.useSlider = true;
    this.slider.gameObject.SetActive(true);
    this.slider.wholeNumbers = wholeNumbers;
    this.slider.minValue = sliderSettings.Min;
    this.slider.maxValue = sliderSettings.Max;
    RectTransform transform = (RectTransform) this.text.transform;
    transform.sizeDelta = transform.sizeDelta with
    {
      x = this.textWidthWithSlider
    };
    float result;
    if (!float.TryParse(this.text.text, out result))
      return;
    this.slider.SetValueWithoutNotify(result);
  }

  private void Wrapper_OnChange(float newValue) => this.UpdateFields(newValue);

  private void UpdateFields(float value)
  {
    if (this.useSlider)
      this.slider.SetValueWithoutNotify(value);
    this.text.SetTextWithoutNotify(this.TextFormat != null ? value.ToString(this.TextFormat) : value.ToString());
  }

  private void Slider_OnValueChanged(float value)
  {
    value = this.RoundToStep(value);
    this.UpdateFields(value);
    this.wrapper.SetValue(value, (object) this);
  }

  private float RoundToStep(float value)
  {
    if (this.valueSteps.HasValue)
    {
      float num = this.valueSteps.Value;
      value /= num;
      value = Mathf.Round(value);
      value *= num;
    }
    return value;
  }

  private void Text_OnValueChanged(string stringValue)
  {
    float result;
    if (!float.TryParse(this.text.text, out result))
      return;
    float step = this.RoundToStep(result);
    this.UpdateFields(step);
    this.wrapper.SetValue(step, (object) this);
  }

  [Serializable]
  public struct IntSlider(int min, int max)
  {
    public int Min = min;
    public int Max = max;

    public static explicit operator FloatDataField.FloatSlider?(FloatDataField.IntSlider? v)
    {
      return !v.HasValue ? new FloatDataField.FloatSlider?() : new FloatDataField.FloatSlider?((FloatDataField.FloatSlider) v.Value);
    }

    public static implicit operator FloatDataField.FloatSlider(FloatDataField.IntSlider v)
    {
      return new FloatDataField.FloatSlider()
      {
        Min = (float) v.Min,
        Max = (float) v.Max
      };
    }
  }

  [Serializable]
  public struct IntSettings
  {
    public FloatDataField.IntSlider? Slider;
    public int? Steps;
    public string TextFormat;

    public static explicit operator FloatDataField.DataSettings?(FloatDataField.IntSettings? v)
    {
      return !v.HasValue ? new FloatDataField.DataSettings?() : new FloatDataField.DataSettings?((FloatDataField.DataSettings) v.Value);
    }

    public static implicit operator FloatDataField.DataSettings(FloatDataField.IntSettings v)
    {
      return new FloatDataField.DataSettings()
      {
        Slider = (FloatDataField.FloatSlider?) v.Slider,
        WholeNumber = true,
        Steps = new float?((float) (v.Steps ?? 1)),
        TextFormat = v.TextFormat
      };
    }
  }

  [Serializable]
  public struct FloatSlider(float min, float max)
  {
    public float Min = min;
    public float Max = max;
  }

  [Serializable]
  public struct FloatSettings
  {
    public FloatDataField.FloatSlider? Slider;
    public float? Steps;
    public string TextFormat;

    public static explicit operator FloatDataField.DataSettings?(FloatDataField.FloatSettings? v)
    {
      return !v.HasValue ? new FloatDataField.DataSettings?() : new FloatDataField.DataSettings?((FloatDataField.DataSettings) v.Value);
    }

    public static implicit operator FloatDataField.DataSettings(FloatDataField.FloatSettings v)
    {
      return new FloatDataField.DataSettings()
      {
        Slider = v.Slider,
        WholeNumber = false,
        Steps = v.Steps,
        TextFormat = v.TextFormat
      };
    }
  }

  [Serializable]
  public struct DataSettings
  {
    public FloatDataField.FloatSlider? Slider;
    public float? Steps;
    public bool WholeNumber;
    public string TextFormat;
  }
}
