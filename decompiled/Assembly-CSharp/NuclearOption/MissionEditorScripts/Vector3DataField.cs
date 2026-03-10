// Decompiled with JetBrains decompiler
// Type: NuclearOption.MissionEditorScripts.Vector3DataField
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.SavedMission.ObjectiveV2;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

#nullable disable
namespace NuclearOption.MissionEditorScripts;

public class Vector3DataField : DataField, IDataField<Vector3>
{
  [SerializeField]
  private TMP_InputField xText;
  [SerializeField]
  private TMP_InputField yText;
  [SerializeField]
  private TMP_InputField zText;
  public string FormatString;
  private IValueWrapper<Vector3> wrapper;

  protected override void SetFieldInteractable(bool value)
  {
    this.xText.interactable = value;
    this.yText.interactable = value;
    this.zText.interactable = value;
  }

  protected override void AwakeSetup()
  {
    this.xText.contentType = TMP_InputField.ContentType.DecimalNumber;
    this.yText.contentType = TMP_InputField.ContentType.DecimalNumber;
    this.zText.contentType = TMP_InputField.ContentType.DecimalNumber;
    this.xText.onEndEdit.AddListener(new UnityAction<string>(this.Text_OnValueChanged));
    this.yText.onEndEdit.AddListener(new UnityAction<string>(this.Text_OnValueChanged));
    this.zText.onEndEdit.AddListener(new UnityAction<string>(this.Text_OnValueChanged));
  }

  public void Setup(string label, IValueWrapper<Vector3> wrapper)
  {
    this.wrapper?.UnregisterOnChange((object) this);
    this.wrapper = wrapper;
    this.wrapper.RegisterOnChange((object) this, new ValueWrapper<Vector3>.OnChangeDelegate(this.Wrapper_OnChange));
    this.label.text = label;
    this.UpdateFields(wrapper.Value);
    this.Interactable = true;
  }

  private void OnDestroy() => this.wrapper?.UnregisterOnChange((object) this);

  public void SetupReadOnly(string label, GlobalPosition value)
  {
    this.SetupReadOnly(label, value.AsVector3());
  }

  public void SetupReadOnly(string label, Vector3 value)
  {
    this.wrapper?.UnregisterOnChange((object) this);
    this.wrapper = (IValueWrapper<Vector3>) null;
    this.label.text = label;
    this.UpdateFields(value);
    this.Interactable = false;
  }

  public void SetupNonValue(string label)
  {
    this.wrapper?.UnregisterOnChange((object) this);
    this.wrapper = (IValueWrapper<Vector3>) null;
    this.label.text = label;
    this.xText.SetIfNotFocus("-");
    this.yText.SetIfNotFocus("-");
    this.zText.SetIfNotFocus("-");
    this.Interactable = false;
  }

  private void Wrapper_OnChange(Vector3 newValue) => this.UpdateFields(newValue);

  private void UpdateFields(Vector3 current)
  {
    if (string.IsNullOrEmpty(this.FormatString))
    {
      this.xText.SetIfNotFocus(current.x.ToString());
      this.yText.SetIfNotFocus(current.y.ToString());
      this.zText.SetIfNotFocus(current.z.ToString());
    }
    else
    {
      this.xText.SetIfNotFocus(current.x.ToString(this.FormatString));
      this.yText.SetIfNotFocus(current.y.ToString(this.FormatString));
      this.zText.SetIfNotFocus(current.z.ToString(this.FormatString));
    }
  }

  private void Text_OnValueChanged(string arg0)
  {
    float result1;
    float result2;
    float result3;
    if ((0 | (float.TryParse(this.xText.text, out result1) ? 1 : 0) | (float.TryParse(this.yText.text, out result2) ? 1 : 0) | (float.TryParse(this.zText.text, out result3) ? 1 : 0)) == 0)
      return;
    this.wrapper?.SetValue(new Vector3(result1, result2, result3), (object) this);
  }
}
