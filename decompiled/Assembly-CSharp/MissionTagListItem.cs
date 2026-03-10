// Decompiled with JetBrains decompiler
// Type: MissionTagListItem
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using JamesFrowen.ScriptableVariables.UI;
using NuclearOption.SavedMission;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#nullable disable
public class MissionTagListItem : ListItem<MissionTag>
{
  [SerializeField]
  private LayoutElement layout;
  [SerializeField]
  private TextMeshProUGUI text;
  [SerializeField]
  private Image image;
  [SerializeField]
  private float padding = 6f;

  protected override void SetValue(MissionTag tag)
  {
    this.text.text = tag.Tag;
    this.image.color = tag.Color;
    this.SetSize();
  }

  private void SetSize()
  {
    float num = this.text.preferredWidth + this.padding * 2f;
    this.layout.preferredWidth = num;
    this.layout.minWidth = num;
  }

  private void OnValidate()
  {
    if (!((Object) this.layout != (Object) null) || !((Object) this.text != (Object) null))
      return;
    this.SetSize();
  }
}
