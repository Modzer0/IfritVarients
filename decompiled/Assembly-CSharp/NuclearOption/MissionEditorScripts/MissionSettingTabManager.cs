// Decompiled with JetBrains decompiler
// Type: NuclearOption.MissionEditorScripts.MissionSettingTabManager
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.SavedMission;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

#nullable disable
namespace NuclearOption.MissionEditorScripts;

public class MissionSettingTabManager : MonoBehaviour
{
  [SerializeField]
  private Button openParametersTab;
  [SerializeField]
  private Button openEnvironmentTab;
  [SerializeField]
  private Button openFactionTab;
  [SerializeField]
  private Button openRestrictionsTab;
  [SerializeField]
  private PanelScrollView panel;
  [SerializeField]
  private Color normalColor;
  [SerializeField]
  private Color openColor;
  [SerializeField]
  private MissionSettingsTab parametersTab;
  [SerializeField]
  private EnvironmentTab environmentTab;
  [SerializeField]
  private FactionSettingsTab factionTab;
  [SerializeField]
  private RestrictionsTab restrictionsTab;
  private Button activeButton;
  private IMissionTab activeTab;
  private Mission mission;

  private void Awake()
  {
    this.openParametersTab.onClick.AddListener(new UnityAction(this.OpenParameters));
    this.openEnvironmentTab.onClick.AddListener(new UnityAction(this.OpenEnvironment));
    this.openFactionTab.onClick.AddListener(new UnityAction(this.OpenFaction));
    this.openRestrictionsTab.onClick.AddListener(new UnityAction(this.OpenRestrictions));
    MissionManager.OnEditorOrPickerMissionChanged += new Action<Mission>(this.MissionManager_OnEditorOrPickerMissionChanged);
    this.SetTabActive((MonoBehaviour) this.parametersTab, false);
    this.SetTabActive((MonoBehaviour) this.environmentTab, false);
    this.SetTabActive((MonoBehaviour) this.restrictionsTab, false);
    this.SetTabActive((MonoBehaviour) this.factionTab, false);
  }

  private void Start() => this.OpenParameters();

  private void OnDestroy()
  {
    MissionManager.OnEditorOrPickerMissionChanged -= new Action<Mission>(this.MissionManager_OnEditorOrPickerMissionChanged);
  }

  private void MissionManager_OnEditorOrPickerMissionChanged(Mission mission)
  {
    this.mission = mission;
    this.activeTab?.SetMission(mission);
  }

  private void SetActive<T>(Button button, T tab) where T : MonoBehaviour, IMissionTab
  {
    if ((UnityEngine.Object) this.activeButton != (UnityEngine.Object) null)
      this.activeButton.image.color = this.normalColor;
    this.activeButton = button;
    this.activeButton.image.color = this.openColor;
    if (this.activeTab != null)
      this.SetTabActive((MonoBehaviour) this.activeTab, false);
    this.activeTab = (IMissionTab) tab;
    this.activeTab.SetMission(this.mission);
    this.SetTabActive((MonoBehaviour) tab, true);
    this.panel.SetChild(tab.transform.AsRectTransform());
  }

  private void SetTabActive(MonoBehaviour tab, bool active) => tab.gameObject.SetActive(active);

  private void OpenParameters()
  {
    this.SetActive<MissionSettingsTab>(this.openParametersTab, this.parametersTab);
  }

  private void OpenEnvironment()
  {
    this.SetActive<EnvironmentTab>(this.openEnvironmentTab, this.environmentTab);
  }

  private void OpenFaction()
  {
    this.SetActive<FactionSettingsTab>(this.openFactionTab, this.factionTab);
  }

  private void OpenRestrictions()
  {
    this.SetActive<RestrictionsTab>(this.openRestrictionsTab, this.restrictionsTab);
  }
}
