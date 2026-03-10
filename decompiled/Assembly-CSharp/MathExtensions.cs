// Decompiled with JetBrains decompiler
// Type: MathExtensions
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

#nullable disable
public static class MathExtensions
{
  public static void ClampPos(this Rect rect, ref Vector2 pos, float factor = 1f)
  {
    if ((double) pos.x > (double) factor * (double) rect.xMax)
      pos.x = factor * rect.xMax;
    if ((double) pos.x < (double) factor * (double) rect.xMin)
      pos.x = factor * rect.xMin;
    if ((double) pos.y > (double) factor * (double) rect.yMax)
      pos.y = factor * rect.yMax;
    if ((double) pos.y >= (double) factor * (double) rect.yMin)
      return;
    pos.y = factor * rect.yMin;
  }
}
