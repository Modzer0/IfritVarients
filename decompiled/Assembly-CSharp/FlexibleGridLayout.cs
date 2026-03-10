// Decompiled with JetBrains decompiler
// Type: FlexibleGridLayout
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using UnityEngine;
using UnityEngine.UI;

#nullable disable
public class FlexibleGridLayout : LayoutGroup
{
  [Header("Flexible Grid")]
  public FlexibleGridLayout.FitType fitType;
  public int rows;
  public int columns;
  public Vector2 cellSize;
  public Vector2 spacing;
  public bool fitX;
  public bool fitY;

  public override void CalculateLayoutInputHorizontal()
  {
    base.CalculateLayoutInputHorizontal();
    if (this.fitType == FlexibleGridLayout.FitType.WIDTH || this.fitType == FlexibleGridLayout.FitType.HEIGHT || this.fitType == FlexibleGridLayout.FitType.UNIFORM)
    {
      this.rows = this.columns = Mathf.CeilToInt(Mathf.Sqrt((float) this.transform.childCount));
      switch (this.fitType)
      {
        case FlexibleGridLayout.FitType.UNIFORM:
          this.fitX = this.fitY = true;
          break;
        case FlexibleGridLayout.FitType.WIDTH:
          this.fitX = true;
          this.fitY = false;
          break;
        case FlexibleGridLayout.FitType.HEIGHT:
          this.fitX = false;
          this.fitY = true;
          break;
      }
    }
    if (this.fitType == FlexibleGridLayout.FitType.WIDTH || this.fitType == FlexibleGridLayout.FitType.FIXEDCOLUMNS)
      this.rows = Mathf.CeilToInt((float) this.transform.childCount / (float) this.columns);
    if (this.fitType == FlexibleGridLayout.FitType.HEIGHT || this.fitType == FlexibleGridLayout.FitType.FIXEDROWS)
      this.columns = Mathf.CeilToInt((float) this.transform.childCount / (float) this.rows);
    double width = (double) this.rectTransform.rect.width;
    float height = this.rectTransform.rect.height;
    double columns = (double) this.columns;
    float num1 = (float) (width / columns - (double) this.spacing.x / (double) this.columns * (double) (this.columns - 1) - (double) this.padding.left / (double) this.columns - (double) this.padding.right / (double) this.columns);
    float num2 = (float) ((double) height / (double) this.rows - (double) this.spacing.y / (double) this.rows * (double) (this.rows - 1) - (double) this.padding.top / (double) this.rows - (double) this.padding.bottom / (double) this.rows);
    this.cellSize.x = this.fitX ? num1 : this.cellSize.x;
    this.cellSize.y = this.fitY ? num2 : this.cellSize.y;
    for (int index = 0; index < this.rectChildren.Count; ++index)
    {
      int num3 = index / this.columns;
      int num4 = index % this.columns;
      RectTransform rectChild = this.rectChildren[index];
      float pos1 = (float) ((double) this.cellSize.x * (double) num4 + (double) this.spacing.x * (double) num4) + (float) this.padding.left;
      float pos2 = (float) ((double) this.cellSize.y * (double) num3 + (double) this.spacing.y * (double) num3) + (float) this.padding.top;
      this.SetChildAlongAxis(rectChild, 0, pos1, this.cellSize.x);
      this.SetChildAlongAxis(rectChild, 1, pos2, this.cellSize.y);
    }
  }

  public override void CalculateLayoutInputVertical()
  {
  }

  public override void SetLayoutHorizontal()
  {
  }

  public override void SetLayoutVertical()
  {
  }

  public enum FitType
  {
    UNIFORM,
    WIDTH,
    HEIGHT,
    FIXEDROWS,
    FIXEDCOLUMNS,
  }
}
