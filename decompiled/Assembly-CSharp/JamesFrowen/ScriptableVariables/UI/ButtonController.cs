// Decompiled with JetBrains decompiler
// Type: JamesFrowen.ScriptableVariables.UI.ButtonController
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

#nullable disable
namespace JamesFrowen.ScriptableVariables.UI;

[RequireComponent(typeof (Button))]
public abstract class ButtonController : MonoBehaviour
{
  protected Button _button;

  protected virtual void Awake()
  {
    this._button = this.GetComponent<Button>();
    this._button.onClick.AddListener(new UnityAction(this.onClick));
  }

  protected abstract void onClick();
}
