// Decompiled with JetBrains decompiler
// Type: qol.Utilities.UnityExtensions
// Assembly: qol, Version=1.1.6.3, Culture=neutral, PublicKeyToken=null
// MVID: 3B6539EA-E38C-45EE-8FFF-FC58CC42BADC
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\BepInEx\plugins\qol_1.1.6.3b1.dll

using UnityEngine;

#nullable disable
namespace qol.Utilities;

public static class UnityExtensions
{
  public static Color WithAlpha(this Color color, float alpha)
  {
    return new Color(color.r, color.g, color.b, alpha);
  }

  public static Color WithBrightness(this Color color, float factor)
  {
    return new Color(color.r * factor, color.g * factor, color.b * factor, color.a);
  }

  public static Sprite ToSprite(this Texture2D texture)
  {
    return Sprite.Create(texture, new Rect(0.0f, 0.0f, (float) texture.width, (float) texture.height), new Vector2(0.5f, 0.5f));
  }

  public static Sprite ToSprite(this Texture2D texture, Vector2 pivot)
  {
    return Sprite.Create(texture, new Rect(0.0f, 0.0f, (float) texture.width, (float) texture.height), pivot);
  }
}
