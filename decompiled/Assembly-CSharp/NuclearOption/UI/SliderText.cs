// Decompiled with JetBrains decompiler
// Type: NuclearOption.UI.SliderText
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

#nullable disable
namespace NuclearOption.UI;

public class SliderText : MonoBehaviour
{
  [SerializeField]
  private Slider slider;
  [SerializeField]
  private TextMeshProUGUI text;
  [Tooltip("use {0} to insert value")]
  [SerializeField]
  private string format;

  private void Awake()
  {
    this.slider.onValueChanged.AddListener(new UnityAction<float>(this.SetText));
    this.SetText(this.slider.value);
  }

  private void OnValidate()
  {
    if (!((Object) this.slider != (Object) null) || !((Object) this.text != (Object) null))
      return;
    this.SetText(this.slider.value);
  }

  private void SetText(float value)
  {
    this.text.text = !string.IsNullOrEmpty(this.format) ? string.Format(this.format, (object) value) : value.ToString("0.0");
  }
}
