// Decompiled with JetBrains decompiler
// Type: qol.UI.Branding.BackgroundManager
// Assembly: qol, Version=1.1.6.3, Culture=neutral, PublicKeyToken=null
// MVID: 3B6539EA-E38C-45EE-8FFF-FC58CC42BADC
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\BepInEx\plugins\qol_1.1.6.3b1.dll

using BepInEx.Logging;
using qol.Utilities;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

#nullable disable
namespace qol.UI.Branding;

public static class BackgroundManager
{
  private static ManualLogSource _logger;
  private static string[] _backgroundNames;

  public static void Initialize(ManualLogSource logger, string[] backgroundNames)
  {
    BackgroundManager._logger = logger;
    BackgroundManager._backgroundNames = backgroundNames;
  }

  public static void ReplaceBackgroundImage()
  {
    bool darken1 = false;
    bool darken2 = false;
    GameObject backgroundObject = GameObject.Find("MainCanvas/Prejoin menu/background");
    if ((Object) backgroundObject == (Object) null)
    {
      backgroundObject = GameObject.Find("MainCanvas/background");
      darken1 = true;
    }
    if ((Object) backgroundObject == (Object) null)
    {
      backgroundObject = GameObject.Find("MainCanvas/BackgroundImage");
      darken2 = true;
    }
    GameObject logoObject = GameObject.Find("MainCanvas/Prejoin menu/NewLogo");
    if ((Object) backgroundObject != (Object) null)
      BackgroundManager.ApplyBackgroundImage(backgroundObject, darken1, darken2);
    if (!((Object) logoObject != (Object) null))
      return;
    BackgroundManager.ApplyLogoImage(logoObject);
  }

  private static void ApplyBackgroundImage(GameObject backgroundObject, bool darken1, bool darken2)
  {
    Image component = backgroundObject.GetComponent<Image>();
    component.GetComponent<AspectRatioFitter>().aspectRatio = 1.77777779f;
    Texture2D randomHomescreen = BackgroundManager.GetRandomHomescreen();
    if ((Object) randomHomescreen != (Object) null)
    {
      Sprite sprite = Sprite.Create(randomHomescreen, new Rect(0.0f, 0.0f, (float) randomHomescreen.width, (float) randomHomescreen.height), new Vector2(0.5f, 0.5f));
      component.sprite = sprite;
      if (darken1)
      {
        component.color = new Color(0.8f, 0.8f, 0.8f, 1f);
      }
      else
      {
        if (!darken2)
          return;
        component.color = new Color(0.5f, 0.5f, 0.5f, 1f);
      }
    }
    else
      BackgroundManager._logger?.LogError((object) "Failed to load embedded background texture!");
  }

  private static void ApplyLogoImage(GameObject logoObject)
  {
    Image component = logoObject.GetComponent<Image>();
    Texture2D texture = BackgroundManager.LoadTextureFromResources("NewGameLogo2c.png");
    if ((Object) texture != (Object) null)
    {
      Sprite sprite = Sprite.Create(texture, new Rect(0.0f, 0.0f, (float) texture.width, (float) texture.height), new Vector2(0.5f, 0.5f));
      component.sprite = sprite;
      component.material = PathLookup.Find("MainCanvas/HintPanel/ButtonLeft").GetComponent<CanvasRenderer>().GetMaterial();
    }
    else
      BackgroundManager._logger?.LogError((object) "Failed to load embedded logo texture!");
  }

  public static Texture2D GetRandomHomescreen()
  {
    if (BackgroundManager._backgroundNames != null && BackgroundManager._backgroundNames.Length != 0)
      return BackgroundManager.LoadTextureFromResources(BackgroundManager._backgroundNames[Random.Range(0, BackgroundManager._backgroundNames.Length)]);
    BackgroundManager._logger?.LogError((object) "No background names configured!");
    return (Texture2D) null;
  }

  public static Texture2D LoadTextureFromResources(string resourceName)
  {
    Assembly executingAssembly = Assembly.GetExecutingAssembly();
    string name = $"{executingAssembly.GetName().Name}.Resources.Backgrounds.{resourceName}";
    using (Stream manifestResourceStream = executingAssembly.GetManifestResourceStream(name))
    {
      if (manifestResourceStream == null)
      {
        BackgroundManager._logger?.LogError((object) ("Resource not found: " + name));
        return (Texture2D) null;
      }
      byte[] numArray = new byte[manifestResourceStream.Length];
      manifestResourceStream.Read(numArray, 0, numArray.Length);
      Texture2D tex = new Texture2D(2, 2);
      tex.LoadImage(numArray);
      return tex;
    }
  }
}
