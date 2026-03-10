// Decompiled with JetBrains decompiler
// Type: NuclearOption.UI.BetterBorder
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using UnityEngine;
using UnityEngine.UI;

#nullable disable
namespace NuclearOption.UI;

[AddComponentMenu("UI/Better Border", 532)]
[RequireComponent(typeof (RectTransform))]
public class BetterBorder : MaskableGraphic
{
  [Tooltip("Thickness of the border in Unity units.")]
  [SerializeField]
  private float borderThickness = 1f;
  [Tooltip("Color of the central fill area.")]
  [SerializeField]
  private Color fillColor = Color.clear;

  public float BorderThickness
  {
    get => this.borderThickness;
    set
    {
      if (Mathf.Approximately(this.borderThickness, value))
        return;
      this.borderThickness = value;
      this.SetVerticesDirty();
    }
  }

  public override Color color
  {
    get => base.color;
    set
    {
      if (base.color == value)
        return;
      base.color = value;
      this.SetVerticesDirty();
    }
  }

  public Color FillColor
  {
    get => this.fillColor;
    set
    {
      if (this.fillColor == value)
        return;
      this.fillColor = value;
      this.SetVerticesDirty();
    }
  }

  protected override void OnPopulateMesh(VertexHelper vh)
  {
    vh.Clear();
    Rect pixelAdjustedRect = this.GetPixelAdjustedRect();
    float xMin = pixelAdjustedRect.xMin;
    float yMin = pixelAdjustedRect.yMin;
    float xMax = pixelAdjustedRect.xMax;
    float yMax = pixelAdjustedRect.yMax;
    float num = Mathf.Min(this.borderThickness, pixelAdjustedRect.width / 2f, pixelAdjustedRect.height / 2f);
    float x1 = xMin + num;
    float x2 = xMax - num;
    float y1 = yMin + num;
    float y2 = yMax - num;
    this.AddQuad(vh, new Vector2(x1, y1), new Vector2(x2, y2), this.fillColor);
    this.AddQuad(vh, new Vector2(xMin, y2), new Vector2(xMax, yMax), this.color);
    this.AddQuad(vh, new Vector2(xMin, yMin), new Vector2(xMax, y1), this.color);
    this.AddQuad(vh, new Vector2(xMin, y1), new Vector2(x1, y2), this.color);
    this.AddQuad(vh, new Vector2(x2, y1), new Vector2(xMax, y2), this.color);
  }

  private void AddQuad(VertexHelper vh, Vector2 bottomLeft, Vector2 topRight, Color quadColor)
  {
    int currentVertCount = vh.currentVertCount;
    UIVertex simpleVert = UIVertex.simpleVert with
    {
      color = (Color32) quadColor,
      uv0 = (Vector4) Vector2.zero,
      position = new Vector3(bottomLeft.x, bottomLeft.y, 0.0f)
    };
    vh.AddVert(simpleVert);
    simpleVert.position = new Vector3(bottomLeft.x, topRight.y, 0.0f);
    vh.AddVert(simpleVert);
    simpleVert.position = new Vector3(topRight.x, topRight.y, 0.0f);
    vh.AddVert(simpleVert);
    simpleVert.position = new Vector3(topRight.x, bottomLeft.y, 0.0f);
    vh.AddVert(simpleVert);
    vh.AddTriangle(currentVertCount, currentVertCount + 1, currentVertCount + 2);
    vh.AddTriangle(currentVertCount + 2, currentVertCount + 3, currentVertCount);
  }
}
