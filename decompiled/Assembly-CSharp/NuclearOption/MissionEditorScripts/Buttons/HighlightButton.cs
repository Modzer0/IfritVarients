// Decompiled with JetBrains decompiler
// Type: NuclearOption.MissionEditorScripts.Buttons.HighlightButton
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using JamesFrowen.ScriptableVariables.UI;
using UnityEngine;
using UnityEngine.UI;

#nullable disable
namespace NuclearOption.MissionEditorScripts.Buttons;

public abstract class HighlightButton : ButtonController
{
  [Header("Highlight")]
  [SerializeField]
  private Image _buttonImage;
  [SerializeField]
  private Color _highlight;
  [SerializeField]
  private Color _normal;

  public void Highlight(bool highlight)
  {
    this._buttonImage.color = highlight ? this._highlight : this._normal;
  }
}
