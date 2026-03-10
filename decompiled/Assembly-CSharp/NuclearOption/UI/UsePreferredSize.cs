// Decompiled with JetBrains decompiler
// Type: NuclearOption.UI.UsePreferredSize
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using UnityEngine;
using UnityEngine.UI;

#nullable disable
namespace NuclearOption.UI;

[RequireComponent(typeof (ILayoutElement))]
public class UsePreferredSize : MonoBehaviour, ILayoutSelfController, ILayoutController
{
  [SerializeField]
  private Vector2 padding;
  [SerializeField]
  private bool setWidth;
  [SerializeField]
  private bool setHeight;
  private ILayoutElement element;

  public void SetLayoutHorizontal() => this.Resize(this.setWidth, false);

  public void SetLayoutVertical() => this.Resize(false, this.setHeight);

  private void Resize(bool setWidth, bool setHeight)
  {
    if (!setHeight && !setWidth || (Object) this == (Object) null)
      return;
    if (this.element as Object == (Object) null)
      this.element = this.GetComponent<ILayoutElement>();
    RectTransform transform = (RectTransform) this.transform;
    Vector2 sizeDelta = transform.sizeDelta;
    if (setWidth)
      sizeDelta.x = this.element.preferredWidth + this.padding.x;
    if (setHeight)
      sizeDelta.y = this.element.preferredHeight + this.padding.y;
    transform.sizeDelta = sizeDelta;
  }
}
