// Decompiled with JetBrains decompiler
// Type: ColorLog
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Cysharp.Threading.Tasks;
using Mirage;
using NuclearOption.Networking;
using System;
using System.Diagnostics;
using UnityEngine;

#nullable disable
public class ColorLog
{
  private const int ColorSeed = 403;
  private const float ColorSaturation = 0.6f;
  private const float ColorValue = 0.8f;

  protected ColorLog()
  {
  }

  public static Color ColorFromName(string fullName)
  {
    return ColorLog.ColorFromName(fullName, 0.6f, 0.8f);
  }

  public static Color ColorFromName(string fullName, float saturation, float value)
  {
    return Color.HSVToRGB(Mathf.Abs((float) (403 * fullName.GetStableHashCode()) / (float) int.MaxValue), saturation, value);
  }

  public static string CreatePrefixForType<T>(bool addColor)
  {
    return ColorLog.CreatePrefixForType(typeof (T), addColor);
  }

  public static string CreatePrefixForType(System.Type type, bool addColor)
  {
    string name = type.Name;
    if (!addColor)
      return $"[{name}]";
    return $"<color=#{ColorUtility.ToHtmlStringRGB(ColorLog.ColorFromName(type.FullName))}>[{name}]</color>";
  }

  [Conditional("UNITY_ASSERTIONS")]
  public static void AssertDestroyedAfter(MonoBehaviour self, float delaySeconds = 1f)
  {
    UniTask.Void((Func<UniTaskVoid>) (async () =>
    {
      await UniTask.Delay((int) ((double) delaySeconds * 1000.0), true);
      if (!((UnityEngine.Object) self != (UnityEngine.Object) null))
        return;
      string prefixForType = ColorLog.CreatePrefixForType(self.GetType(), true);
      UnityEngine.Debug.LogError((object) $"{LogTimeUpdater.unscaledTime:0.000}: {prefixForType} [Assert] was not destroyed");
    }));
  }

  [Conditional("UNITY_ASSERTIONS")]
  public static void AssertIsServer(string message = null)
  {
    if (NetworkManagerNuclearOption.i.Server.Active)
      return;
    if (message == null)
      message = "";
    UnityEngine.Debug.LogError((object) $"{LogTimeUpdater.unscaledTime:0.000}: [Assert] Not server. {message}");
  }

  [Conditional("UNITY_ASSERTIONS")]
  public static void AssertAreClose(float a, float b, string message = null)
  {
    if ((double) Math.Abs(a - b) <= 0.0099999997764825821)
      return;
    if (message == null)
      message = "";
    UnityEngine.Debug.LogError((object) $"{LogTimeUpdater.unscaledTime:0.000}: [Assert] {a} and {b} are not close {message}");
  }
}
