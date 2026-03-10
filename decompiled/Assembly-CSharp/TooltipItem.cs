// Decompiled with JetBrains decompiler
// Type: TooltipItem
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using UnityEngine;
using UnityEngine.UI;

#nullable enable
public class TooltipItem : MonoBehaviour
{
  public 
  #nullable disable
  Image icon;
  public Text label;
  public Text value;

  public void Setup(Sprite image, Color? color, 
  #nullable enable
  string? desc, int? number)
  {
    if ((Object) image != (Object) null)
    {
      this.icon.sprite = image;
      this.icon.color = color.HasValue ? color.Value : Color.white;
      this.icon.enabled = true;
    }
    else
      this.icon.enabled = false;
    if (desc != null)
    {
      this.label.text = desc;
      this.label.color = color.HasValue ? color.Value : Color.white;
      this.label.enabled = true;
    }
    else
      this.label.enabled = false;
    if (number.HasValue)
    {
      this.value.text = number.ToString();
      this.value.color = color.HasValue ? color.Value : Color.white;
      this.value.enabled = true;
    }
    else
      this.value.enabled = false;
  }

  public void SetValue(int number)
  {
    this.value.enabled = true;
    this.value.text = number.ToString();
  }

  public void SetLabel(
  #nullable disable
  string text)
  {
    this.label.enabled = true;
    this.label.text = text;
  }

  public int GetValue()
  {
    int result;
    return !int.TryParse(this.value.text, out result) ? 0 : result;
  }

  public void AddValue(int number)
  {
    this.value.text = (int.Parse(this.value.text) + number).ToString();
  }

  public void SetColor(Color color)
  {
    this.icon.color = color;
    this.label.color = color;
    this.value.color = color;
  }
}
