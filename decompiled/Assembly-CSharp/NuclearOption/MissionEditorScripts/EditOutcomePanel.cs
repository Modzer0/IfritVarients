// Decompiled with JetBrains decompiler
// Type: NuclearOption.MissionEditorScripts.EditOutcomePanel
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.SavedMission.ObjectiveV2;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

#nullable disable
namespace NuclearOption.MissionEditorScripts;

public class EditOutcomePanel : MonoBehaviour, ISidePanel
{
  [Header("Main")]
  [SerializeField]
  private TextMeshProUGUI typeText;
  [SerializeField]
  private TMP_InputField uniqueNameText;
  [Header("Controls")]
  [SerializeField]
  private Button deleteButton;
  [SerializeField]
  private TextMeshProUGUI deleteText;
  [SerializeField]
  private Button closeButton;
  [Header("Data")]
  [SerializeField]
  private RectTransform dataContent;
  [SerializeField]
  private GameObject dataPlaceholder;
  [SerializeField]
  private UIPrefabs prefabs;
  [SerializeField]
  private float dataWidth;
  private Outcome outcome;
  private Objective parentObjective;

  public SidePanel Panel { get; set; }

  private void Awake()
  {
    this.uniqueNameText.onEndEdit.AddListener(new UnityAction<string>(this.OnEdit));
    this.deleteButton.onClick.AddListener(new UnityAction(this.DeleteClicked));
    this.closeButton.onClick.AddListener(new UnityAction(this.CloseClicked));
  }

  private void OnEdit(string _) => this.OnEdit();

  private void OnEdit()
  {
    this.outcome.SavedOutcome.UniqueName = this.uniqueNameText.text;
    MissionManager.Objectives.MakeUnique(this.outcome);
    this.uniqueNameText.SetTextWithoutNotify(this.outcome.SavedOutcome.UniqueName);
    SceneSingleton<MissionEditor>.i.CheckAutoSave();
    this.Panel.Parent.Refresh();
  }

  public void SetOutcome(Outcome outcome, Objective parentObjective)
  {
    this.outcome = outcome;
    this.SetFields(outcome);
    MissionObjectives objectives = MissionManager.Objectives;
    this.deleteText.text = parentObjective != null ? "Remove from " + parentObjective.SavedObjective.UniqueName : $"Delete (References:{SaveHelper.CountUsedBy(objectives, outcome)})";
    this.DrawData();
  }

  private void DrawData()
  {
    DataDrawer drawer = new DataDrawer(this.dataContent, this.prefabs);
    drawer.Width = new float?(this.dataWidth);
    this.outcome.DrawData(drawer);
    this.dataPlaceholder.SetActive(!drawer.Success);
  }

  private void SetFields(Outcome outcome)
  {
    SavedOutcome savedOutcome = outcome.SavedOutcome;
    this.typeText.text = outcome.SavedOutcome.Type.ToNicifyString<OutcomeType>();
    this.uniqueNameText.SetTextWithoutNotify(savedOutcome.UniqueName);
  }

  private void CloseClicked() => this.Panel.Destroy();

  private void DeleteClicked()
  {
    if (this.parentObjective != null)
    {
      int index = this.parentObjective.Outcomes.IndexOf(this.outcome);
      if (index != -1)
      {
        this.parentObjective.Outcomes.RemoveAt(index);
        SceneSingleton<MissionEditor>.i.CheckAutoSave();
      }
    }
    else
    {
      MissionManager.Objectives.RemoveOutcome(this.outcome);
      SceneSingleton<MissionEditor>.i.CheckAutoSave();
    }
    this.Panel.Destroy();
    this.Panel.Parent?.Refresh();
  }

  void ISidePanel.PanelRefresh()
  {
  }
}
