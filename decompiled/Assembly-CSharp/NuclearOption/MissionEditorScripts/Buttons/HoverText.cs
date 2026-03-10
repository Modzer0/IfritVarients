// Decompiled with JetBrains decompiler
// Type: NuclearOption.MissionEditorScripts.Buttons.HoverText
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using TMPro;
using UnityEngine;

#nullable disable
namespace NuclearOption.MissionEditorScripts.Buttons;

public class HoverText : MonoBehaviour
{
  [SerializeField]
  private GameObject hover;
  [SerializeField]
  private TextMeshProUGUI text;
  [SerializeField]
  private float offset;
  private RectTransform rectTransform;
  private object key;

  private void Awake()
  {
    this.rectTransform = this.GetComponent<RectTransform>();
    this.Hide((object) null);
  }

  public void Show(object key, string showText)
  {
    this.key = key;
    if ((Object) this.text == (Object) null)
      this.text = this.hover.GetComponentInChildren<TextMeshProUGUI>();
    if (showText != null)
      this.text.text = showText;
    this.hover.SetActive(true);
    FixLayout.ForceRebuildRecursive((RectTransform) this.hover.transform);
  }

  public void Hide(object key)
  {
    if (this.key != key)
      return;
    this.hover.SetActive(false);
  }

  public void Refresh(string showText)
  {
    if (!((Object) this.text != (Object) null) || !this.hover.activeSelf)
      return;
    if (showText != null)
      this.text.text = showText;
    FixLayout.ForceRebuildRecursive((RectTransform) this.hover.transform);
  }

  public void Move(object key, Vector2 pos)
  {
    if (!this.hover.activeSelf || this.key != key)
      return;
    Vector2 vector2_1 = this.offset * new Vector2(1f, -1f);
    float width = this.rectTransform.rect.width;
    Vector2 vector2_2;
    if ((double) pos.x > (double) Screen.width - (double) width - (double) this.offset)
    {
      this.rectTransform.pivot = new Vector2(1f, 1f);
      vector2_2 = this.offset * new Vector2(-1f, -1f);
    }
    else
    {
      this.rectTransform.pivot = new Vector2(0.0f, 1f);
      vector2_2 = this.offset * new Vector2(1f, -1f);
    }
    this.hover.transform.position = (Vector3) (pos + vector2_2);
  }
}
