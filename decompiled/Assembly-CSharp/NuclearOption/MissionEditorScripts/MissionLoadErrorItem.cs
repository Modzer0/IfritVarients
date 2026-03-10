// Decompiled with JetBrains decompiler
// Type: NuclearOption.MissionEditorScripts.MissionLoadErrorItem
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

#nullable disable
namespace NuclearOption.MissionEditorScripts;

public class MissionLoadErrorItem : MonoBehaviour
{
  [SerializeField]
  private TextMeshProUGUI header;
  [SerializeField]
  private Image headerBackground;
  [SerializeField]
  private Button expand;
  [SerializeField]
  private GameObject bodyHolder;
  [SerializeField]
  private TextMeshProUGUI body;
  [SerializeField]
  private Image bodyBackground;
  [SerializeField]
  private Color headerWarningColor;
  [SerializeField]
  private Color bodyWarningColor;
  [SerializeField]
  private Color headerErrorColor;
  [SerializeField]
  private Color bodyErrorColor;
  private bool isExpanded;
  [SerializeField]
  private bool _debugColorError;

  private void OnValidate()
  {
    this.headerBackground.color = this._debugColorError ? this.headerErrorColor : this.headerWarningColor;
    this.bodyBackground.color = this._debugColorError ? this.bodyErrorColor : this.bodyWarningColor;
  }

  private void Awake()
  {
    this.expand.onClick.AddListener(new UnityAction(this.ToggleExpand));
    this.isExpanded = false;
    this.bodyHolder.SetActive(this.isExpanded);
  }

  private void ToggleExpand()
  {
    this.isExpanded = !this.isExpanded;
    this.bodyHolder.SetActive(this.isExpanded);
    LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform) this.transform);
  }

  public void SetWarning(string msg)
  {
    this.SetHeadText(msg);
    this.body.text = msg;
  }

  public void SetException(Exception exception)
  {
    this.SetHeadText(exception.Message);
    this.body.text = exception.ToString();
  }

  private void SetHeadText(string msg)
  {
    string str = msg;
    int length = msg.IndexOf("\n");
    if (length != -1)
      str = msg.Substring(0, length);
    this.header.text = str;
  }
}
