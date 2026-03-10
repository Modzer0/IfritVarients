// Decompiled with JetBrains decompiler
// Type: NuclearOption.MissionEditorScripts.EditObjectivePanel
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.SavedMission;
using NuclearOption.SavedMission.ObjectiveV2;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

#nullable disable
namespace NuclearOption.MissionEditorScripts;

public class EditObjectivePanel : MonoBehaviour, ISidePanel
{
  [Header("Main")]
  [SerializeField]
  private TextMeshProUGUI typeText;
  [SerializeField]
  private TMP_InputField uniqueNameText;
  [SerializeField]
  private TMP_InputField displayNameText;
  [SerializeField]
  private TextMeshProUGUI noFactionError;
  [SerializeField]
  private FactionDataField factionField;
  [SerializeField]
  private Toggle hiddenToggle;
  [Header("Controls")]
  [SerializeField]
  private Button deleteButton;
  [SerializeField]
  private Button closeButton;
  [Header("Data")]
  [SerializeField]
  private UIPrefabs prefabs;
  [SerializeField]
  private RectTransform dataContent;
  [SerializeField]
  private GameObject dataPlaceholder;
  [SerializeField]
  private float dataWidth;
  [Header("Outcomes")]
  [SerializeField]
  private RectTransform outcomeParent;
  [SerializeField]
  private int outcomeListHeight;
  [Header("Outcomes Panel")]
  [SerializeField]
  private NewOutcomePanel newOutcomePanelPrefab;
  [SerializeField]
  private EditOutcomePanel editOutcomePanelPrefab;
  private ReferenceList outcomeList;
  private DataDrawer dataDrawer;
  private Objective objective;
  private readonly ValueWrapperString factionName = new ValueWrapperString();
  private IObjectiveEditorUpdate objectiveEditorUpdates;

  public SidePanel Panel { get; set; }

  private void Awake()
  {
    this.noFactionError.text = GoogleIconFont.FontString("\uE002") + " Objective needs a faction";
    this.noFactionError.gameObject.SetActive(false);
    this.uniqueNameText.onEndEdit.AddListener(new UnityAction<string>(this.OnEdit));
    this.displayNameText.onEndEdit.AddListener(new UnityAction<string>(this.OnEdit));
    this.hiddenToggle.onValueChanged.AddListener(new UnityAction<bool>(this.OnEdit));
    this.deleteButton.onClick.AddListener(new UnityAction(this.DeleteClicked));
    this.closeButton.onClick.AddListener(new UnityAction(this.CloseClicked));
    this.factionName.RegisterOnChange((object) this, (ValueWrapper<string>.OnChangeDelegate) (name =>
    {
      this.objective.FactionHQ = FactionRegistry.HqFromName(name);
      this.objective.SavedObjective.Faction = name;
      this.CheckFactionError();
      this.AfterEdit();
    }));
  }

  private void OnEnable()
  {
    if (this.objective == null)
      return;
    this.SetObjective(this.objective);
  }

  private void OnDisable()
  {
    this.objectiveEditorUpdates?.Destroy();
    this.objectiveEditorUpdates = (IObjectiveEditorUpdate) null;
    this.dataDrawer?.Cleanup();
  }

  private void OnEdit(string _) => this.OnEdit();

  private void OnEdit(int _) => this.OnEdit();

  private void OnEdit(bool _) => this.OnEdit();

  private void OnEdit()
  {
    this.objective.SavedObjective.UniqueName = this.uniqueNameText.text;
    MissionManager.Objectives.MakeUnique(this.objective);
    this.uniqueNameText.SetTextWithoutNotify(this.objective.SavedObjective.UniqueName);
    this.objective.SavedObjective.DisplayName = this.displayNameText.text;
    this.objective.SavedObjective.Hidden = this.hiddenToggle.isOn;
    this.AfterEdit();
  }

  private void AfterEdit()
  {
    SceneSingleton<MissionEditor>.i.CheckAutoSave();
    this.Panel.Parent.Refresh();
    this.CheckFactionError();
    this.dataDrawer?.InvokeAfterEdit();
  }

  private FactionHQ GetFactionHq(string name)
  {
    if (name == "None")
      return (FactionHQ) null;
    foreach (MissionFaction faction in MissionManager.CurrentMission.factions)
    {
      if (faction.factionName == name)
        return faction.FactionHQ;
    }
    throw new KeyNotFoundException($"Could not find a faction with name:'{name}'");
  }

  public void SetObjective(Objective objective)
  {
    this.objectiveEditorUpdates?.Destroy();
    this.objectiveEditorUpdates = (IObjectiveEditorUpdate) null;
    this.objective = objective;
    this.factionName.SetValue(objective.SavedObjective.Faction, (object) this, true);
    this.factionField.Setup((IValueWrapper<string>) this.factionName);
    string factionLabelOverride = objective.FactionLabelOverride;
    if (!string.IsNullOrEmpty(factionLabelOverride))
      this.factionField.SetNoFactionString(factionLabelOverride);
    this.CheckFactionError();
    this.SetupFields(objective.SavedObjective);
    this.CheckDestroyAllowed((ISaveableReference) objective);
    this.DrawData();
    if ((UnityEngine.Object) this.outcomeList == (UnityEngine.Object) null)
      this.outcomeList = UnityEngine.Object.Instantiate<ReferenceList>(this.prefabs.ReferenceListPrefab, (Transform) this.outcomeParent);
    this.outcomeList.TitleText.text = "Outcomes";
    this.outcomeList.SetHeight(this.outcomeListHeight);
    this.outcomeList.Setup(new Action(this.OpenNewOutcomePanel), (Action) null, new Action<int>(this.EditOutcome), new Action<int>(this.RemoveOutcome));
    this.outcomeList.SetupList<Outcome>(objective.Outcomes, (Func<Outcome, string>) (o => o.ToUIString(true)), (Func<Outcome, string>) (o => o.ToUIString(false)), (Func<IEnumerable<Outcome>>) (() => (IEnumerable<Outcome>) MissionManager.Objectives.AllOutcomes), ReferenceList.ButtonsEvents.OverrideNullOnly);
    this.objectiveEditorUpdates = objective.CreateEditorUpdate(this.GetComponentInParent<Canvas>(), this.prefabs);
  }

  private void CheckDestroyAllowed(ISaveableReference objective)
  {
    this.deleteButton.gameObject.SetActive(objective.CanBeReference);
  }

  private void DrawData()
  {
    if (this.dataDrawer == null)
      this.dataDrawer = new DataDrawer(this.dataContent, this.prefabs);
    else
      this.dataDrawer.Reset();
    this.dataDrawer.TrackedValueChanged += new Action(this.DataDrawer_TrackedValueChanged);
    this.dataDrawer.Width = new float?(this.dataWidth);
    this.objective.DrawData(this.dataDrawer);
    this.dataPlaceholder.SetActive(!this.dataDrawer.Success);
  }

  private void DataDrawer_TrackedValueChanged() => this.CheckFactionError();

  private void CheckFactionError()
  {
    bool flag = this.objective.NeedsFaction && FactionHelper.EmptyOrNoFaction(this.objective.SavedObjective.Faction);
    if (flag == this.noFactionError.gameObject.activeSelf)
      return;
    this.noFactionError.gameObject.SetActive(flag);
    FixLayout.ForceRebuildAtEndOfFrame(this.transform.AsRectTransform());
  }

  private void SetupFields(SavedObjective objective)
  {
    this.typeText.text = objective.Type.ToNicifyString<ObjectiveType>();
    this.uniqueNameText.SetTextWithoutNotify(objective.UniqueName);
    this.displayNameText.SetTextWithoutNotify(objective.DisplayName);
    this.hiddenToggle.SetIsOnWithoutNotify(objective.Hidden);
  }

  private void CloseClicked() => this.Panel.Destroy();

  private void DeleteClicked()
  {
    MissionObjectives objectives = MissionManager.Objectives;
    objectives.RemoveObjectiveAt(objectives.AllObjectives.IndexOf(this.objective));
    this.Panel.Destroy();
    this.Panel.Parent?.Refresh();
  }

  private void OpenNewOutcomePanel()
  {
    this.Panel.Child.Create<NewOutcomePanel>(this.newOutcomePanelPrefab).Setup(this.objective);
  }

  private void EditOutcome(int index) => this.EditOutcome(this.objective.Outcomes[index]);

  private void EditOutcome(Outcome outcome)
  {
    this.Panel.Child.Create<EditOutcomePanel>(this.editOutcomePanelPrefab).SetOutcome(outcome, this.objective);
  }

  public void RemoveOutcome(int index)
  {
    this.objective.Outcomes.RemoveAt(index);
    SceneSingleton<MissionEditor>.i.CheckAutoSave();
    this.outcomeList.RefreshList();
  }

  void ISidePanel.PanelRefresh() => this.outcomeList.RefreshList();

  private void Update() => this.objectiveEditorUpdates?.Update();
}
