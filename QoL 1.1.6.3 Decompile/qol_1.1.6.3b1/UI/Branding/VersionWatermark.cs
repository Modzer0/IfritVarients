// Decompiled with JetBrains decompiler
// Type: qol.UI.Branding.VersionWatermark
// Assembly: qol, Version=1.1.6.3, Culture=neutral, PublicKeyToken=null
// MVID: 3B6539EA-E38C-45EE-8FFF-FC58CC42BADC
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\BepInEx\plugins\qol_1.1.6.3b1.dll

using BepInEx.Logging;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

#nullable disable
namespace qol.UI.Branding;

public static class VersionWatermark
{
  private const string TEXT_OBJECT_NAME = "P2082ModTag";
  private static ManualLogSource _logger;
  private static bool _isBeta;
  private static string _guid;

  public static void Initialize(ManualLogSource logger, bool isBeta, string guid)
  {
    VersionWatermark._logger = logger;
    VersionWatermark._isBeta = isBeta;
    VersionWatermark._guid = guid;
  }

  public static IEnumerator AddWhenReady(bool notDedicatedServer, Func<Font> fontProvider)
  {
    if (notDedicatedServer)
      yield return (object) null;
    GameObject parent = GameObject.Find("MainCanvas");
    if ((UnityEngine.Object) parent == (UnityEngine.Object) null)
    {
      parent = GameObject.Find("GameplayUICanvas");
      if ((UnityEngine.Object) parent == (UnityEngine.Object) null)
      {
        ManualLogSource logger = VersionWatermark._logger;
        if (logger == null)
          yield break;
        logger.LogInfo((object) "MainCanvas not found in this scene, ignoring tag");
        yield break;
      }
    }
    GameObject textObj = GameObject.Find("P2082ModTag");
    if (!((UnityEngine.Object) textObj != (UnityEngine.Object) null))
      VersionWatermark.CreateWatermarkObject(parent, fontProvider);
    else
      VersionWatermark.UpdateExistingWatermark(textObj);
  }

  private static GameObject CreateWatermarkObject(GameObject parent, Func<Font> fontProvider)
  {
    GameObject watermarkObject = new GameObject("P2082ModTag");
    watermarkObject.transform.SetParent(parent.transform, false);
    Text text = watermarkObject.AddComponent<Text>();
    text.text = !VersionWatermark._isBeta ? $"Primeva 2082<color=#ffffff80>  |  {VersionWatermark._guid} <size=10>v1.1.6.3</size></color>" : $"<color=#00ffffff>BETA</color>  Primeva 2082<color=#ffffff80>  |  {VersionWatermark._guid} <size=10>v1.1.6.3</size></color>";
    text.font = fontProvider != null ? fontProvider() : (Font) null;
    text.color = Color.white;
    text.alignment = TextAnchor.LowerRight;
    text.fontSize = 13;
    RectTransform component = watermarkObject.GetComponent<RectTransform>();
    component.anchorMin = new Vector2(1f, 0.0f);
    component.anchorMax = new Vector2(1f, 0.0f);
    component.pivot = new Vector2(1f, 0.0f);
    component.anchoredPosition = new Vector2(-10f, 10f);
    component.sizeDelta = new Vector2(400f, 50f);
    return watermarkObject;
  }

  private static void UpdateExistingWatermark(GameObject textObj)
  {
    Text component = textObj.GetComponent<Text>();
    if (!((UnityEngine.Object) component != (UnityEngine.Object) null) || component.text.Contains("qol".ToUpper()) || component.text.Contains(VersionWatermark._guid))
      return;
    Text text = component;
    text.text = $"{text.text}<color=#ffffff80>  |  {VersionWatermark._guid} <size=10>v1.1.6.3</size></color>";
  }
}
