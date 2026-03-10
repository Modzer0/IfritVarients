// Decompiled with JetBrains decompiler
// Type: NuclearOption.UI.OverrideField
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

#nullable disable
namespace NuclearOption.UI;

public class OverrideField : MonoBehaviour
{
  [SerializeField]
  private BaseToggle overrideToggle;
  [SerializeField]
  private Selectable[] fields;

  private void Awake()
  {
    this.overrideToggle.onValueChanged.AddListener(new UnityAction<bool>(this.OverrideChanged));
    this.OverrideChanged(this.overrideToggle.isOn);
  }

  private void OnValidate()
  {
    if (!((Object) this.overrideToggle != (Object) null))
      return;
    this.OverrideChanged(this.overrideToggle.isOn);
  }

  private void OverrideChanged(bool isOn)
  {
    foreach (Selectable field in this.fields)
      field.interactable = isOn;
  }
}
