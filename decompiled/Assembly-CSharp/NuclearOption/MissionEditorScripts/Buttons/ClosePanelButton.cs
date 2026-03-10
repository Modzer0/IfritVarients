// Decompiled with JetBrains decompiler
// Type: NuclearOption.MissionEditorScripts.Buttons.ClosePanelButton
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using JamesFrowen.ScriptableVariables.UI;
using UnityEngine;

#nullable disable
namespace NuclearOption.MissionEditorScripts.Buttons;

public class ClosePanelButton : ButtonController
{
  [SerializeField]
  private GameObject panel;

  protected override void onClick() => this.panel.SetActive(false);
}
