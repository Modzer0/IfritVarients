// Decompiled with JetBrains decompiler
// Type: NuclearOption.MissionEditorScripts.NewPanelBase`1
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

#nullable disable
namespace NuclearOption.MissionEditorScripts;

public abstract class NewPanelBase<T> : MonoBehaviour, ISidePanel where T : struct, Enum
{
  [SerializeField]
  protected TMP_InputField nameField;
  [SerializeField]
  private TMP_Dropdown typeDropdown;
  [SerializeField]
  private Button createButton;
  [SerializeField]
  private Button closeButton;
  private List<string> options;

  public SidePanel Panel { get; set; }

  void ISidePanel.PanelRefresh()
  {
  }

  private void Start()
  {
    this.createButton.onClick.AddListener(new UnityAction(this.Create));
    this.closeButton.onClick.AddListener(new UnityAction(this.Close));
    this.options = EnumNames<T>.GetNames();
    this.typeDropdown.ClearOptions();
    this.typeDropdown.AddOptions(this.options);
  }

  private void Create()
  {
    this.CreateItem(EnumNames<T>.Parse(this.options[this.typeDropdown.value]));
  }

  protected abstract void CreateItem(T type);

  private void Close() => this.Panel.Destroy();
}
