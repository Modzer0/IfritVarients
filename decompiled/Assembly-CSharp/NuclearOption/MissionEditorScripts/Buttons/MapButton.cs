// Decompiled with JetBrains decompiler
// Type: NuclearOption.MissionEditorScripts.Buttons.MapButton
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using UnityEngine;
using UnityEngine.UI;

#nullable disable
namespace NuclearOption.MissionEditorScripts.Buttons;

[RequireComponent(typeof (Button))]
public class MapButton : HighlightButton
{
  [Header("Reference")]
  [SerializeField]
  private EditorTabs tabs;

  protected override void onClick() => this.tabs.ChangeTabMap((HighlightButton) this);
}
