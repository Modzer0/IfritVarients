// Decompiled with JetBrains decompiler
// Type: qol.Utilities.UIPathHelpers
// Assembly: qol, Version=1.1.6.3, Culture=neutral, PublicKeyToken=null
// MVID: 3B6539EA-E38C-45EE-8FFF-FC58CC42BADC
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\BepInEx\plugins\qol_1.1.6.3b1.dll

using TMPro;
using UnityEngine;
using UnityEngine.UI;

#nullable disable
namespace qol.Utilities;

public static class UIPathHelpers
{
  public static T GetComponentAtPath<T>(string path) where T : Component
  {
    GameObject gameObject = PathLookup.Find(path);
    return !((Object) gameObject != (Object) null) ? default (T) : gameObject.GetComponent<T>();
  }

  public static bool SetImageColor(string path, Color color)
  {
    Image componentAtPath = UIPathHelpers.GetComponentAtPath<Image>(path);
    if (!((Object) componentAtPath != (Object) null))
      return false;
    componentAtPath.color = color;
    return true;
  }

  public static bool SetImageFillCenter(string path, bool fillCenter)
  {
    Image componentAtPath = UIPathHelpers.GetComponentAtPath<Image>(path);
    if (!((Object) componentAtPath != (Object) null))
      return false;
    componentAtPath.fillCenter = fillCenter;
    return true;
  }

  public static bool SetButtonColors(string path, ColorBlock colors)
  {
    Button componentAtPath = UIPathHelpers.GetComponentAtPath<Button>(path);
    if (!((Object) componentAtPath != (Object) null))
      return false;
    componentAtPath.colors = colors;
    return true;
  }

  public static bool SetTextColor(string path, Color color)
  {
    TextMeshProUGUI componentAtPath = UIPathHelpers.GetComponentAtPath<TextMeshProUGUI>(path);
    if (!((Object) componentAtPath != (Object) null))
      return false;
    componentAtPath.color = color;
    return true;
  }

  public static void ApplyImageColors(params (string path, Color color)[] modifications)
  {
    foreach ((string path, Color color) modification in modifications)
      UIPathHelpers.SetImageColor(modification.path, modification.color);
  }

  public static void ApplyButtonColors(ColorBlock colors, params string[] paths)
  {
    foreach (string path in paths)
      UIPathHelpers.SetButtonColors(path, colors);
  }

  public static void ApplyTextColors(Color color, params string[] paths)
  {
    foreach (string path in paths)
      UIPathHelpers.SetTextColor(path, color);
  }
}
