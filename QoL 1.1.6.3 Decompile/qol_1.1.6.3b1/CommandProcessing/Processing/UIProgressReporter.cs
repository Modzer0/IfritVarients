// Decompiled with JetBrains decompiler
// Type: qol.CommandProcessing.Processing.UIProgressReporter
// Assembly: qol, Version=1.1.6.3, Culture=neutral, PublicKeyToken=null
// MVID: 3B6539EA-E38C-45EE-8FFF-FC58CC42BADC
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\BepInEx\plugins\qol_1.1.6.3b1.dll

using System;
using UnityEngine;
using UnityEngine.UI;

#nullable disable
namespace qol.CommandProcessing.Processing;

public class UIProgressReporter
{
  private readonly ProcessingContext _context;
  private readonly string _barName;
  private GameObject _barObj;
  private Text _textComponent;
  private int _oldVSyncCount;
  private int _oldTargetFramerate;
  private const string OverlayName = "2082LoadOverlay";
  private const string ContainerName = "LoadingBarsContainer";

  public UIProgressReporter(ProcessingContext context, string barName)
  {
    this._context = context;
    this._barName = barName;
  }

  public void Initialize()
  {
    if (!this._context.NotDedicatedServer)
      return;
    GameObject gameObject1 = GameObject.Find("MainCanvas") ?? GameObject.Find("GameplayUICanvas");
    GameObject gameObject2 = GameObject.Find("2082LoadOverlay");
    if ((UnityEngine.Object) gameObject2 == (UnityEngine.Object) null)
    {
      gameObject2 = new GameObject("2082LoadOverlay");
      gameObject2.transform.SetParent(gameObject1.transform, false);
      gameObject2.AddComponent<Image>().color = new Color(0.0f, 0.0f, 0.0f, 0.9f);
      RectTransform component = gameObject2.GetComponent<RectTransform>();
      component.anchorMin = Vector2.zero;
      component.anchorMax = Vector2.one;
      component.offsetMin = Vector2.zero;
      component.offsetMax = Vector2.one;
      GameObject gameObject3 = new GameObject("LoadingBarsContainer");
      gameObject3.transform.SetParent(gameObject2.transform, false);
      RectTransform rectTransform = gameObject3.AddComponent<RectTransform>();
      rectTransform.anchorMin = new Vector2(0.0f, 1f);
      rectTransform.anchorMax = new Vector2(0.0f, 1f);
      rectTransform.pivot = new Vector2(0.0f, 1f);
      rectTransform.anchoredPosition = new Vector2(300f, -50f);
      rectTransform.sizeDelta = new Vector2(800f, 0.0f);
      VerticalLayoutGroup verticalLayoutGroup = gameObject3.AddComponent<VerticalLayoutGroup>();
      verticalLayoutGroup.padding = new RectOffset(0, 0, 0, 0);
      verticalLayoutGroup.spacing = 100f;
      verticalLayoutGroup.childAlignment = TextAnchor.UpperLeft;
      verticalLayoutGroup.childControlHeight = true;
      verticalLayoutGroup.childControlWidth = true;
      verticalLayoutGroup.childForceExpandHeight = false;
      verticalLayoutGroup.childForceExpandWidth = true;
    }
    Transform parent = gameObject2.transform.Find("LoadingBarsContainer");
    this._barObj = new GameObject(this._barName);
    this._barObj.transform.SetParent(parent, false);
    LayoutElement layoutElement = this._barObj.AddComponent<LayoutElement>();
    layoutElement.preferredHeight = 200f;
    layoutElement.flexibleHeight = 0.0f;
    this._textComponent = this._barObj.AddComponent<Text>();
    this._textComponent.font = this._context.GetBestFont();
    this._textComponent.color = Color.white;
    this._textComponent.alignment = TextAnchor.UpperLeft;
    this._textComponent.fontSize = 25;
    this._textComponent.horizontalOverflow = HorizontalWrapMode.Overflow;
    this._textComponent.verticalOverflow = VerticalWrapMode.Overflow;
    this._textComponent.lineSpacing = 1.33f;
    this._oldVSyncCount = QualitySettings.vSyncCount;
    this._oldTargetFramerate = Application.targetFrameRate;
    QualitySettings.vSyncCount = 0;
    Application.targetFrameRate = 120;
  }

  public void UpdateProgress(int processedLines, int totalLines, string line, string version)
  {
    if ((UnityEngine.Object) this._textComponent == (UnityEngine.Object) null)
      return;
    string str = "";
    int length = line.IndexOf(" ");
    if (length > 0)
      str = " | Modifying " + line.Substring(0, length);
    int count = (int) Mathf.Round((float) (20 * (processedLines + 1)) / (float) (totalLines + 1));
    this._textComponent.text = $"<color={this._context.PrimaryWhite}>Processing {this._context.PluginGuid} v{version}{str}</color>\n";
    this._textComponent.text += $"<color={this._context.PrimaryWhite}>[</color><color={this._context.PrimaryGreen}>{new string('=', count)}</color><color={this._context.SecondaryWhite}>{new string('=', 20 - count)}</color><color={this._context.PrimaryWhite}>] {(ValueType) (float) ((double) (1000 * processedLines / totalLines) / 10.0)}%</color>\n";
  }

  public void UpdateProgressThreaded(
    int processedLines,
    int totalLines,
    (bool Active, int LineNumber)[] threadStatus,
    int threadCount,
    int maxThreadCount,
    int delayMs)
  {
    if ((UnityEngine.Object) this._textComponent == (UnityEngine.Object) null)
      return;
    string str1 = "           ";
    int count = (int) Mathf.Round((float) (20 * (processedLines + 1)) / (float) (totalLines + 1));
    string str2 = "";
    for (int index = 0; index < threadCount; ++index)
    {
      string str3 = threadStatus[index].Active ? this._context.PrimaryGreen : this._context.SecondaryWhite;
      string str4 = threadStatus[index].LineNumber > 0 ? $"[{threadStatus[index].LineNumber}]" : "[-]";
      str2 += $"{str1}<size=20><color={str3}>T{index + 1} {str4}</color></size>\n";
    }
    this._textComponent.text = $"<color={this._context.PrimaryWhite}>{str1}Patching </color><color={this._context.PrimaryGreen}>{this._context.PluginGuid}</color>\n";
    this._textComponent.text += $"{str1}<size=30><color={this._context.PrimaryWhite}>Processing line {processedLines + 1} of {totalLines}</color></size>\n";
    Text textComponent = this._textComponent;
    textComponent.text = $"{textComponent.text}{str1}<size=30><color={this._context.PrimaryWhite}>[</color><color={this._context.PrimaryGreen}>{new string('=', count)}</color><color={this._context.SecondaryWhite}>{new string('=', 20 - count)}</color><color={this._context.PrimaryWhite}>]</color></size>\n";
    this._textComponent.text += str2;
    this._textComponent.text += $"{str1}<size=20><color={this._context.SecondaryWhite}>Threading | Delay: {delayMs}ms | Threads: {threadCount}/{maxThreadCount}</color></size>";
  }

  public void ShowCompletion(int totalLines, long elapsedMs, long ticks)
  {
    if ((UnityEngine.Object) this._textComponent == (UnityEngine.Object) null)
      return;
    this._textComponent.text = "";
    Text textComponent = this._textComponent;
    textComponent.text = $"{textComponent.text}<color={this._context.PrimaryWhite}>Processed {this._context.PluginGuid}</color>\n";
    this._textComponent.text += $"<color={this._context.SecondaryWhite}>Elapsed {elapsedMs / 1000L}s ({ticks} ticks)</color>\n";
  }

  public void ShowCompletionThreaded(int totalLines, float elapsedSeconds, int threadCount)
  {
    if ((UnityEngine.Object) this._textComponent == (UnityEngine.Object) null)
      return;
    string str = "           ";
    this._textComponent.text = $"<color={this._context.PrimaryGreen}>{str}Patched {this._context.PluginGuid}</color>\n";
    this._textComponent.text += $"{str}<size=30><color={this._context.SecondaryWhite}>Processed {totalLines} lines</color></size>\n";
  }

  public void Cleanup()
  {
    if (!this._context.NotDedicatedServer)
      return;
    QualitySettings.vSyncCount = this._oldVSyncCount;
    Application.targetFrameRate = this._oldTargetFramerate;
    GameObject gameObject = GameObject.Find("2082LoadOverlay");
    if (!((UnityEngine.Object) gameObject != (UnityEngine.Object) null))
      return;
    Transform transform1 = gameObject.transform.Find("LoadingBarsContainer");
    if (!((UnityEngine.Object) transform1 != (UnityEngine.Object) null))
      return;
    Transform transform2 = transform1.Find(this._barName);
    if ((UnityEngine.Object) transform2 != (UnityEngine.Object) null)
      UnityEngine.Object.Destroy((UnityEngine.Object) transform2.gameObject);
    bool flag = false;
    foreach (Transform transform3 in transform1)
    {
      if (transform3.name.StartsWith("2082LoadingBar_") && transform3.name != this._barName)
      {
        flag = true;
        break;
      }
    }
    if (flag)
      return;
    UnityEngine.Object.Destroy((UnityEngine.Object) gameObject);
  }

  public void DestroyAfterDelay(float delay)
  {
    if (!((UnityEngine.Object) this._barObj != (UnityEngine.Object) null))
      return;
    UnityEngine.Object.Destroy((UnityEngine.Object) this._barObj, delay);
  }
}
