// Decompiled with JetBrains decompiler
// Type: CustomizeMissionMenu
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.MissionEditorScripts;
using NuclearOption.SavedMission;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

#nullable disable
public class CustomizeMissionMenu : MonoBehaviour
{
  [SerializeField]
  private MissionsPicker missionsPicker;
  [SerializeField]
  private Button openButton;
  [SerializeField]
  private Button closeButton;
  [SerializeField]
  private GameObject holder;
  [SerializeField]
  private MissionSettingsTab parametersTab;
  [SerializeField]
  private EnvironmentTab environmentTab;
  [SerializeField]
  private FactionSettingsTab factionTab;
  private Mission mission;

  private void Awake()
  {
    this.openButton.onClick.AddListener(new UnityAction(this.OpenCustomizeMenu));
    this.closeButton.onClick.AddListener(new UnityAction(this.CloseCustomizeMenu));
    this.holder.SetActive(false);
    this.missionsPicker.OnMissionSelect += new Action<Mission>(this.SetMission);
    this.SetMission((Mission) null);
  }

  private void OpenCustomizeMenu()
  {
    this.holder.SetActive(true);
    this.parametersTab.SetMission(this.mission);
    this.environmentTab.SetMission(this.mission);
    this.factionTab.SetMission(this.mission);
  }

  private void CloseCustomizeMenu()
  {
    this.holder.SetActive(false);
    Mission missionCopy;
    string loadErrors;
    if (MissionSaveLoad.SaveMissionTemp(this.missionsPicker.Mission, "CurrentMission", false, out missionCopy, out loadErrors))
      this.missionsPicker.SetMissionWithoutNotify(missionCopy);
    else
      Debug.LogError((object) ("Failed to save copy:\n" + loadErrors));
  }

  public void SetMission(Mission mission)
  {
    this.openButton.interactable = mission != null;
    this.mission = mission;
  }
}
