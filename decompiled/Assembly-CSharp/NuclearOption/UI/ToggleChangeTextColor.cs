// Decompiled with JetBrains decompiler
// Type: NuclearOption.UI.ToggleChangeTextColor
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

#nullable disable
namespace NuclearOption.UI;

public class ToggleChangeTextColor : MonoBehaviour
{
  [SerializeField]
  private TextMeshProUGUI text;
  [SerializeField]
  private Toggle toggle;
  [SerializeField]
  private Color onColor = Color.white;
  [SerializeField]
  private Color offColor = Color.grey;

  private void Awake()
  {
    this.toggle.onValueChanged.AddListener(new UnityAction<bool>(this.OnChanged));
    this.OnChanged(this.toggle.isOn);
  }

  private void OnValidate()
  {
    if (!((Object) this.text != (Object) null) || !((Object) this.toggle != (Object) null))
      return;
    this.OnChanged(this.toggle.isOn);
  }

  private void OnChanged(bool isOn) => this.text.color = isOn ? this.onColor : this.offColor;
}
