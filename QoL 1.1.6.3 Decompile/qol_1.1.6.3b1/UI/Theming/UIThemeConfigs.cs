// Decompiled with JetBrains decompiler
// Type: qol.UI.Theming.UIThemeConfigs
// Assembly: qol, Version=1.1.6.3, Culture=neutral, PublicKeyToken=null
// MVID: 3B6539EA-E38C-45EE-8FFF-FC58CC42BADC
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\BepInEx\plugins\qol_1.1.6.3b1.dll

using UnityEngine;
using UnityEngine.UI;

#nullable disable
namespace qol.UI.Theming;

public static class UIThemeConfigs
{
  public static readonly Color BackgroundMenu = new Color(0.0f, 0.0f, 0.0f, 0.5f);
  public static readonly Color BackgroundMenuDark = new Color(0.0f, 0.0f, 0.0f, 0.75f);
  public static readonly Color BackgroundMenuAlt = new Color(0.0f, 0.0f, 0.0f, 0.25f);
  public static readonly Color Transparent = new Color(0.0f, 0.0f, 0.0f, 0.0f);
  public static readonly Color ButtonMain = new Color(0.2f, 0.21f, 0.22f, 0.5f);
  public static readonly Color ButtonHigh = new Color(0.39f, 0.41f, 0.43f, 0.5f);
  public static readonly Color ButtonHigher = new Color(0.39f, 0.41f, 0.43f, 0.5f);
  public static readonly Color ButtonPress = new Color(0.6f, 0.84f, 0.58f, 0.5f);
  public static readonly Color ButtonPressHigh = new Color(0.6f, 0.84f, 0.58f, 0.75f);
  public static readonly Color ExitRed = new Color(1f, 0.2f, 0.2f, 0.25f);
  public static readonly Color ExitRedHigh = new Color(1f, 0.2f, 0.2f, 0.5f);
  public static readonly Color BackgroundDarkened = new Color(0.7f, 0.7f, 0.7f, 1f);
  public static readonly Color BackgroundDarkenedMore = new Color(0.5f, 0.5f, 0.5f, 1f);
  public static readonly Color MissionPickerOverlay = new Color(0.0f, 0.0f, 0.0f, 0.5f);
  public static readonly Color WorkshopBackground = new Color(0.0f, 0.0f, 0.0f, 0.9f);

  public static ColorBlock ButtonColor
  {
    get
    {
      return new ColorBlock()
      {
        colorMultiplier = 1f,
        fadeDuration = 0.2f,
        normalColor = UIThemeConfigs.ButtonHigh,
        selectedColor = UIThemeConfigs.ButtonHigher,
        pressedColor = UIThemeConfigs.ButtonPressHigh,
        highlightedColor = UIThemeConfigs.ButtonHigher,
        disabledColor = UIThemeConfigs.ButtonMain
      };
    }
  }

  public static ColorBlock EnterButtonColorBlock
  {
    get
    {
      return new ColorBlock()
      {
        colorMultiplier = 1f,
        fadeDuration = 0.2f,
        normalColor = UIThemeConfigs.ButtonPress,
        selectedColor = UIThemeConfigs.ButtonPressHigh,
        pressedColor = UIThemeConfigs.ButtonPressHigh,
        highlightedColor = UIThemeConfigs.ButtonPressHigh,
        disabledColor = UIThemeConfigs.ButtonMain
      };
    }
  }

  public static ColorBlock ExitButtonColorBlock
  {
    get
    {
      return new ColorBlock()
      {
        colorMultiplier = 1f,
        fadeDuration = 0.2f,
        normalColor = UIThemeConfigs.ExitRed,
        selectedColor = UIThemeConfigs.ExitRedHigh,
        pressedColor = UIThemeConfigs.ExitRedHigh,
        highlightedColor = UIThemeConfigs.ExitRedHigh,
        disabledColor = UIThemeConfigs.ExitRed
      };
    }
  }
}
