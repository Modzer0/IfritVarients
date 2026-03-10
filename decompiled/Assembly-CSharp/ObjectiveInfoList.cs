// Decompiled with JetBrains decompiler
// Type: ObjectiveInfoList
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.Networking.Lobbies;
using NuclearOption.SavedMission.ObjectiveV2;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#nullable disable
public class ObjectiveInfoList : SceneSingleton<ObjectiveInfoList>
{
  public MFDScreen screen;
  private List<Objective> activeObjectives = new List<Objective>();
  [SerializeField]
  private List<ObjectiveInfoList_ObjEntry> listObjectives = new List<ObjectiveInfoList_ObjEntry>();
  [SerializeField]
  private GameObject objectivePrefab;
  [SerializeField]
  private GameObject missionInfo;
  [SerializeField]
  private GameObject objectiveInfo;
  [SerializeField]
  private Transform container;
  [SerializeField]
  private Text missionName;
  [SerializeField]
  private Text missionTime;
  [SerializeField]
  private Text missionEscalation;
  [SerializeField]
  private Text missionDescription;
  [SerializeField]
  private Button missionButton;
  [SerializeField]
  private Button objectiveButton;
  [SerializeField]
  private Text missionButtonText;
  [SerializeField]
  private Text objectiveButtonText;
  private float lastRefresh;
  private float refreshDelay = 1f;
  private bool objectiveInitialized;

  private void Start()
  {
    MissionManager.onObjectiveStarted += new Action<Objective>(this.AddObjectiveEntry);
    this.ShowMissionInfo();
  }

  private void Update()
  {
    if ((double) Time.timeSinceLevelLoad < (double) this.lastRefresh + (double) this.refreshDelay)
      return;
    if (!this.objectiveInitialized)
    {
      this.InitializeObjectiveList();
      this.InitializeMission();
    }
    this.UpdateMissionInfo();
    this.UpdateObjectiveInfo();
    this.lastRefresh = Time.timeSinceLevelLoad;
  }

  public void UpdateMissionInfo()
  {
    if (MissionManager.Runner == null)
      return;
    float currentEscalation = NetworkSceneSingleton<MissionManager>.i.currentEscalation;
    string str = "Conventional";
    if ((double) currentEscalation > (double) NetworkSceneSingleton<MissionManager>.i.strategicThreshold)
      str = "Strategic";
    else if ((double) currentEscalation > (double) NetworkSceneSingleton<MissionManager>.i.tacticalThreshold)
      str = "Tactical";
    this.missionEscalation.text = $"Score {currentEscalation:F1}  --  {str} level";
    this.missionTime.text = $"Time {UnitConverter.TimeOfDay(NetworkSceneSingleton<LevelInfo>.i.timeOfDay, true)}  --  Duration {UnitConverter.TimeOfDay(NetworkSceneSingleton<MissionManager>.i.MissionTime / 3600f, true)}";
  }

  public void UpdateObjectiveInfo()
  {
    if ((UnityEngine.Object) SceneSingleton<DynamicMap>.i.HQ == (UnityEngine.Object) null || MissionManager.Runner == null)
    {
      if (!this.objectiveButton.gameObject.activeSelf)
        return;
      this.objectiveButton.gameObject.SetActive(false);
    }
    else
    {
      if (this.listObjectives.Count == 0)
        return;
      MissionPosition.TryGetActiveObjectives(SceneSingleton<DynamicMap>.i.HQ, out this.activeObjectives);
      if (this.activeObjectives == null)
        return;
      if (!this.objectiveButton.gameObject.activeSelf)
        this.objectiveButton.gameObject.SetActive(true);
      foreach (ObjectiveInfoList_ObjEntry listObjective in this.listObjectives)
      {
        for (int index = 0; index < this.activeObjectives.Count; ++index)
        {
          if (listObjective.objective == this.activeObjectives[index])
            listObjective.Refresh(this.activeObjectives[index]);
        }
      }
    }
  }

  public void OnEnable()
  {
  }

  private void InitializeMission()
  {
    if (MissionManager.Runner == null)
      return;
    switch (GameManager.gameState)
    {
      case GameState.SinglePlayer:
        this.missionName.text = MissionManager.CurrentMission.Name;
        break;
      case GameState.Multiplayer:
        if ((UnityEngine.Object) SteamLobby.instance != (UnityEngine.Object) null)
        {
          this.missionName.text = SteamLobby.instance.CurrentLobbyName;
          break;
        }
        break;
    }
    this.missionDescription.text = MissionManager.CurrentMission.missionSettings.description;
  }

  private void InitializeObjectiveList()
  {
    if ((UnityEngine.Object) SceneSingleton<DynamicMap>.i.HQ == (UnityEngine.Object) null)
      return;
    MissionPosition.TryGetActiveObjectives(SceneSingleton<DynamicMap>.i.HQ, out this.activeObjectives);
    if (this.activeObjectives == null)
      return;
    if (this.activeObjectives.Count > 0)
    {
      for (int index = 0; index < this.activeObjectives.Count; ++index)
      {
        if (this.activeObjectives[index] is IObjectiveWithPosition && !this.activeObjectives[index].SavedObjective.Hidden)
          this.AddObjectiveEntry(this.activeObjectives[index]);
      }
    }
    this.objectiveInitialized = true;
  }

  private void AddObjectiveEntry(Objective obj)
  {
    if (!(obj is IObjectiveWithPosition) || obj.SavedObjective.Hidden || !((UnityEngine.Object) obj.FactionHQ != (UnityEngine.Object) null) || !((UnityEngine.Object) SceneSingleton<DynamicMap>.i.HQ != (UnityEngine.Object) null) || !((UnityEngine.Object) obj.FactionHQ == (UnityEngine.Object) SceneSingleton<DynamicMap>.i.HQ))
      return;
    ObjectiveInfoList_ObjEntry component = UnityEngine.Object.Instantiate<GameObject>(this.objectivePrefab, this.container).GetComponent<ObjectiveInfoList_ObjEntry>();
    component.SetObjective(obj);
    this.listObjectives.Add(component);
  }

  private void OnDestroy()
  {
    if (this.listObjectives != null && this.listObjectives.Count > 0 && this.listObjectives.Count != this.activeObjectives.Count)
    {
      foreach (Component listObjective in this.listObjectives)
        UnityEngine.Object.Destroy((UnityEngine.Object) listObjective.gameObject);
      this.listObjectives.Clear();
    }
    MissionManager.onObjectiveStarted -= new Action<Objective>(this.AddObjectiveEntry);
  }

  public void ShowMissionInfo()
  {
    this.missionInfo.gameObject.SetActive(true);
    this.objectiveInfo.gameObject.SetActive(false);
  }

  public void ShowObjectiveList()
  {
    if ((UnityEngine.Object) SceneSingleton<DynamicMap>.i.HQ == (UnityEngine.Object) null)
      return;
    this.missionInfo.gameObject.SetActive(false);
    this.objectiveInfo.gameObject.SetActive(true);
  }
}
