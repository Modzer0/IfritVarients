// Decompiled with JetBrains decompiler
// Type: NuclearOption.MissionEditorScripts.MissionLoadErrorPanel
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.SavedMission.ObjectiveV2;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

#nullable disable
namespace NuclearOption.MissionEditorScripts;

public class MissionLoadErrorPanel : MonoBehaviour
{
  [Header("Main panel")]
  [SerializeField]
  private GameObject mainHolder;
  [SerializeField]
  private Button closeButton;
  [SerializeField]
  private Button toggleListButton;
  [SerializeField]
  private TextMeshProUGUI errorCount;
  [SerializeField]
  private TextMeshProUGUI warnCount;
  [Header("List panel")]
  [SerializeField]
  private GameObject listHolder;
  [SerializeField]
  private Transform listParent;
  [SerializeField]
  private MissionLoadErrorItem prefab;
  private readonly List<MissionLoadErrorItem> items = new List<MissionLoadErrorItem>();
  private bool listExpanded;

  private void Awake()
  {
    this.closeButton.onClick.AddListener(new UnityAction(this.Close));
    this.toggleListButton.onClick.AddListener(new UnityAction(this.ToggleList));
    this.listHolder.SetActive(false);
  }

  private void ToggleList()
  {
    this.listExpanded = !this.listExpanded;
    this.listHolder.SetActive(this.listExpanded);
  }

  private void Close() => this.mainHolder.SetActive(false);

  public void SetErrors(LoadErrors loadError)
  {
    int count1 = loadError.Warnings.Count;
    int count2 = loadError.Exceptions.Count;
    if (count1 + count2 == 0)
    {
      this.mainHolder.SetActive(false);
    }
    else
    {
      this.mainHolder.SetActive(true);
      this.listHolder.SetActive(false);
      this.errorCount.text = count2.ToString();
      this.warnCount.text = count1.ToString();
      foreach (Component component in this.items)
        UnityEngine.Object.Destroy((UnityEngine.Object) component.gameObject);
      this.items.Clear();
      foreach (string warning in loadError.Warnings)
      {
        MissionLoadErrorItem missionLoadErrorItem = UnityEngine.Object.Instantiate<MissionLoadErrorItem>(this.prefab, this.listParent);
        missionLoadErrorItem.SetWarning(warning);
        this.items.Add(missionLoadErrorItem);
      }
      foreach (Exception exception in loadError.Exceptions)
      {
        MissionLoadErrorItem missionLoadErrorItem = UnityEngine.Object.Instantiate<MissionLoadErrorItem>(this.prefab, this.listParent);
        missionLoadErrorItem.SetException(exception);
        this.items.Add(missionLoadErrorItem);
      }
    }
  }
}
