// Decompiled with JetBrains decompiler
// Type: NuclearOption.MissionEditorScripts.ObjectiveEditorV2
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.SavedMission;
using NuclearOption.SavedMission.ObjectiveV2;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

#nullable disable
namespace NuclearOption.MissionEditorScripts;

public class ObjectiveEditorV2 : MonoBehaviour, ISidePanel
{
  [SerializeField]
  private UIPrefabs prefabs;
  [Header("Panels")]
  [SerializeField]
  private RectTransform listParent;
  [SerializeField]
  private NewObjectivePanel newObjectivePanelPrefab;
  [SerializeField]
  private EditObjectivePanel editObjectivePanelPrefab;
  [SerializeField]
  private NewOutcomePanel newOutcomePanelPrefab;
  [SerializeField]
  private EditOutcomePanel editOutcomePanelPrefab;
  [SerializeField]
  private int listPanelHeight = 600;
  [Header("Buttons")]
  [SerializeField]
  private Button showObjectivesButton;
  [SerializeField]
  private Button showOutcomesButton;
  [SerializeField]
  private Button showUnitsButton;
  [SerializeField]
  private Button showAirbaseButton;
  [SerializeField]
  private Button showUnitTypesButton;
  private readonly List<SavedUnit> unitList = new List<SavedUnit>();
  private readonly List<SavedAirbase> airbaseList = new List<SavedAirbase>();
  private EditorTabs editorTabs;
  private ReferenceList activeList;
  private SidePanel selectedPanel;

  public SidePanel Panel { get; set; }

  private void Awake()
  {
    this.showObjectivesButton.onClick.AddListener(new UnityAction(this.ShowObjectiveList));
    this.showOutcomesButton.onClick.AddListener(new UnityAction(this.ShowOutcomeList));
    this.showUnitsButton.onClick.AddListener(new UnityAction(this.ShowUnitList));
    this.showAirbaseButton.onClick.AddListener(new UnityAction(this.ShowAirbaseList));
    this.showUnitTypesButton.onClick.AddListener(new UnityAction(this.ShowUnitTypesList));
    this.editorTabs = this.GetComponentInParent<EditorTabs>();
    this.Panel = new SidePanel(this.editorTabs, (ISidePanel) this);
    this.selectedPanel = new SidePanel(this.editorTabs);
    SidePanel sidePanel = new SidePanel(this.editorTabs);
    this.Panel.Child = this.selectedPanel;
    this.selectedPanel.Child = sidePanel;
    this.selectedPanel.Parent = this.Panel;
    sidePanel.Parent = this.selectedPanel;
  }

  private void OnEnable() => this.ShowObjectiveList();

  public void ShowObjectiveList()
  {
    this.CreateReferenceList();
    this.activeList.TitleText.text = "Objectives";
    this.activeList.SetHeight(this.listPanelHeight);
    this.activeList.Setup(new Action(this.ShowNewObjectivePanel), (Action) null, new Action<int>(this.ShowEditObjective), new Action<int>(this.RemoveObjective));
    this.activeList.SetupList<Objective>(MissionManager.CurrentMission.Objectives.AllObjectives, (Func<Objective, string>) (o => o.ToUIString(false)), ReferenceList.ButtonsEvents.DontAdd);
    FilterSet.AddFilterFaction(this.activeList.transform, this.activeList.FilterSet, this.prefabs.Dropdown);
  }

  public void ShowOutcomeList()
  {
    this.CreateReferenceList();
    this.activeList.TitleText.text = "Outcomes";
    this.activeList.SetHeight(this.listPanelHeight);
    this.activeList.Setup(new Action(this.ShowNewOutcomePanel), (Action) null, new Action<int>(this.ShowEditOutcome), new Action<int>(this.RemoveOutcome));
    this.activeList.SetupList<Outcome>(MissionManager.Objectives.AllOutcomes, (Func<Outcome, string>) (o => o.ToUIString(false)), ReferenceList.ButtonsEvents.DontAdd);
  }

  public void ShowUnitList()
  {
    this.CreateReferenceList();
    this.activeList.TitleText.text = "Units";
    this.activeList.EditButtonText = "Focus";
    this.activeList.SetHeight(this.listPanelHeight);
    this.activeList.Setup((Action) null, (Action) null, new Action<int>(this.FocusUnit), new Action<int>(this.RemoveUnit));
    Mission currentMission = MissionManager.CurrentMission;
    this.unitList.Clear();
    this.unitList.AddRange((IEnumerable<SavedUnit>) MissionManager.GetAllSavedUnits(true));
    this.activeList.SetupList<SavedUnit>(this.unitList, (Func<SavedUnit, string>) (o => o.ToUIString(false)));
    FilterSet.AddFilterUnitType(this.activeList.transform, this.activeList.FilterSet, this.prefabs.Dropdown);
    FilterSet.AddFilterFaction(this.activeList.transform, this.activeList.FilterSet, this.prefabs.Dropdown);
    FilterSet.AddFilterPlacement(this.activeList.transform, this.activeList.FilterSet, this.prefabs.Dropdown, false);
  }

  public void ShowAirbaseList()
  {
    this.CreateReferenceList();
    this.activeList.TitleText.text = "Airbase";
    this.activeList.EditButtonText = "Focus";
    this.activeList.SetHeight(this.listPanelHeight);
    this.activeList.Setup((Action) null, (Action) null, new Action<int>(this.FocusAirbase), (Action<int>) null);
    Mission currentMission = MissionManager.CurrentMission;
    this.airbaseList.Clear();
    this.airbaseList.AddRange((IEnumerable<SavedAirbase>) MissionManager.GetAllSavedAirbase());
    this.activeList.SetupList<SavedAirbase>(this.airbaseList, (Func<SavedAirbase, string>) (o => o.ToUIString(false)));
    FilterSet.AddFilterFaction(this.activeList.transform, this.activeList.FilterSet, this.prefabs.Dropdown);
    FilterSet.AddFilterPlacement(this.activeList.transform, this.activeList.FilterSet, this.prefabs.Dropdown, true);
  }

  public void ShowUnitTypesList() => throw new NotImplementedException();

  private void CreateReferenceList()
  {
    if ((UnityEngine.Object) this.activeList != (UnityEngine.Object) null)
      UnityEngine.Object.Destroy((UnityEngine.Object) this.activeList.gameObject);
    this.activeList = UnityEngine.Object.Instantiate<ReferenceList>(this.prefabs.ReferenceListPrefab, (Transform) this.listParent);
    this.selectedPanel.Destroy();
    this.editorTabs.RequestRebuild();
  }

  void ISidePanel.PanelRefresh()
  {
    if (!((UnityEngine.Object) this.activeList != (UnityEngine.Object) null))
      return;
    this.activeList.RefreshList();
  }

  private void ShowNewObjectivePanel()
  {
    this.selectedPanel.Create<NewObjectivePanel>(this.newObjectivePanelPrefab);
  }

  private void ShowEditObjective(int index)
  {
    this.ShowEditObjective(MissionManager.Objectives.AllObjectives[index]);
  }

  public void ShowEditObjective(Objective objective)
  {
    this.selectedPanel.Create<EditObjectivePanel>(this.editObjectivePanelPrefab).SetObjective(objective);
  }

  private void RemoveObjective(int index)
  {
    MissionManager.Objectives.RemoveObjectiveAt(index);
    SceneSingleton<MissionEditor>.i.CheckAutoSave();
    this.ShowObjectiveList();
  }

  private void ShowNewOutcomePanel()
  {
    this.selectedPanel.Create<NewOutcomePanel>(this.newOutcomePanelPrefab).Setup((Objective) null);
  }

  private void ShowEditOutcome(int index)
  {
    this.selectedPanel.Create<EditOutcomePanel>(this.editOutcomePanelPrefab).SetOutcome(MissionManager.Objectives.AllOutcomes[index], (Objective) null);
  }

  private void RemoveOutcome(int index)
  {
    MissionManager.Objectives.RemoveOutcomeAt(index);
    SceneSingleton<MissionEditor>.i.CheckAutoSave();
    this.ShowOutcomeList();
  }

  private void FocusUnit(int index) => ObjectiveEditorV2.FocusUnit(this.unitList[index]);

  private static void FocusUnit(SavedUnit savedUnit)
  {
    Unit unit = savedUnit.Unit;
    SceneSingleton<CameraStateManager>.i.SetFollowingUnit(unit);
    SceneSingleton<UnitSelection>.i.SetSelection((IEditorSelectable) unit);
  }

  private void RemoveUnit(int index)
  {
    SavedUnit unit = this.unitList[index];
    SceneSingleton<MissionEditor>.i.RemoveUnit(unit);
    this.unitList.RemoveAt(index);
    this.activeList.RefreshList();
  }

  private void FocusAirbase(int index) => ObjectiveEditorV2.FocusAirbase(this.airbaseList[index]);

  private static void FocusAirbase(SavedAirbase savedAirbase)
  {
    Airbase airbase = savedAirbase.Airbase;
    SceneSingleton<CameraStateManager>.i.FocusAirbase(airbase, true);
    SceneSingleton<UnitSelection>.i.SetSelection((IEditorSelectable) airbase);
  }
}
