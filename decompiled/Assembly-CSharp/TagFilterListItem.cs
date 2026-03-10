// Decompiled with JetBrains decompiler
// Type: TagFilterListItem
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using JamesFrowen.ScriptableVariables.UI;
using NuclearOption.SavedMission;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

#nullable disable
public class TagFilterListItem : 
  ListItem<TagFilterListItem.Item>,
  IPointerClickHandler,
  IEventSystemHandler
{
  [SerializeField]
  private LayoutElement layout;
  [SerializeField]
  private TextMeshProUGUI text;
  [SerializeField]
  private Image image;
  [SerializeField]
  private Image boarder;
  [SerializeField]
  private float padding = 6f;
  [Header("Colors")]
  [SerializeField]
  private float enabledColor = 1.2f;
  [SerializeField]
  private float normalColor = 1f;
  [SerializeField]
  private float disableColor = 0.4f;
  [Space]
  [SerializeField]
  private float boarderColor = 1.3f;

  protected override void SetValue(TagFilterListItem.Item value)
  {
    MissionTag tag = value.Tag;
    if (!value.Enabled && value.RefCount > 0)
      this.text.text = $"{tag.Tag} ({value.RefCount})";
    else
      this.text.text = tag.Tag;
    if (value.Enabled)
      this.SetColor(tag.Color * this.enabledColor);
    else if (value.RefCount > 0)
      this.SetColor(tag.Color * this.normalColor);
    else
      this.SetColor(tag.Color * this.disableColor);
    this.SetSize();
  }

  private void SetColor(Color color)
  {
    this.image.color = color;
    this.boarder.color = color * this.boarderColor;
  }

  private void SetSize()
  {
    float num = this.text.preferredWidth + this.padding * 2f;
    if ((double) this.layout.preferredWidth == (double) num)
      return;
    this.layout.preferredWidth = num;
    this.layout.minWidth = num;
  }

  private void OnValidate()
  {
    if (!((UnityEngine.Object) this.layout != (UnityEngine.Object) null) || !((UnityEngine.Object) this.text != (UnityEngine.Object) null))
      return;
    this.SetSize();
  }

  public void OnPointerClick(PointerEventData eventData)
  {
    if ((this.Value.Enabled ? 1 : (this.Value.RefCount > 0 ? 1 : 0)) == 0)
      return;
    Action<MissionTag> onClick = this.Value.OnClick;
    if (onClick == null)
      return;
    onClick(this.Value.Tag);
  }

  public readonly struct Item(
    MissionTag tag,
    Action<MissionTag> onClick,
    bool enabled,
    int refCount)
  {
    public readonly MissionTag Tag = tag;
    public readonly Action<MissionTag> OnClick = onClick;
    public readonly bool Enabled = enabled;
    public readonly int RefCount = refCount;
  }
}
