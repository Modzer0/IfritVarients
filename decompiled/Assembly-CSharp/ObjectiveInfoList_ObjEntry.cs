// Decompiled with JetBrains decompiler
// Type: ObjectiveInfoList_ObjEntry
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.SavedMission.ObjectiveV2;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#nullable disable
public class ObjectiveInfoList_ObjEntry : MonoBehaviour
{
  public Objective objective;
  public Text OBJ_Name;
  public Text OBJ_State;
  public Text OBJ_Complete;
  public Button toggleButton;
  [SerializeField]
  private Transform container;
  [SerializeField]
  private List<ObjectiveInfoList_Item> listPositions;
  [SerializeField]
  private ObjectiveInfoList_Item itemPrefab;
  private List<MissionPosition.PositionResult> resultCache = new List<MissionPosition.PositionResult>();

  public void SetObjective(Objective obj)
  {
    this.objective = obj;
    this.OBJ_Name.color = Color.white;
    this.OBJ_Name.text = this.objective.SavedObjective.DisplayName;
    this.OBJ_State.text = "Active";
    this.OBJ_State.color = Color.white;
    this.OBJ_Complete.text = "0%";
    this.OBJ_Complete.color = Color.white;
    MissionManager.onObjectiveComplete += new Action<Objective>(this.OnComplete);
    MissionManager.onObjectiveStarted += new Action<Objective>(this.OnStarted);
  }

  public void Refresh(Objective obj)
  {
    if (obj.Status == ObjectiveStatus.Complete)
    {
      this.OnComplete(obj);
    }
    else
    {
      this.OBJ_Complete.text = $"{(ValueType) (float) (100.0 * (double) obj.CompletePercent):F0}%";
      MissionPosition.GetAllPositionsResults(SceneSingleton<DynamicMap>.i.HQ, Datum.originPosition.ToGlobalPosition(), false, this.resultCache);
      List<MissionPosition.PositionResult> positionResultList = new List<MissionPosition.PositionResult>();
      foreach (MissionPosition.PositionResult positionResult in this.resultCache)
      {
        if (positionResult.Objective == obj && !positionResultList.Contains(positionResult))
          positionResultList.Add(positionResult);
      }
      while (this.listPositions.Count < positionResultList.Count)
        this.listPositions.Add(this.CreateItem());
      for (int index = 0; index < this.listPositions.Count; ++index)
      {
        ObjectiveInfoList_Item listPosition = this.listPositions[index];
        if (index < positionResultList.Count && SceneSingleton<MapOptions>.i.showObjectives)
        {
          listPosition.gameObject.SetActive(true);
          listPosition.Refresh(positionResultList[index]);
        }
        else
          listPosition.gameObject.SetActive(false);
      }
    }
  }

  private ObjectiveInfoList_Item CreateItem()
  {
    return UnityEngine.Object.Instantiate<ObjectiveInfoList_Item>(this.itemPrefab, this.container);
  }

  public void ToggleButton()
  {
    this.container.gameObject.SetActive(!this.container.gameObject.activeSelf);
    LayoutRebuilder.ForceRebuildLayoutImmediate(this.GetComponent<RectTransform>());
  }

  public void OnComplete(Objective obj)
  {
    if (this.objective != obj)
      return;
    this.OBJ_Name.color = Color.green;
    this.OBJ_State.text = "Complete";
    this.OBJ_State.color = Color.green;
    this.OBJ_Complete.text = "100 %";
    this.OBJ_Complete.color = Color.green;
    this.toggleButton.gameObject.SetActive(false);
    if (this.listPositions.Count > 0)
    {
      foreach (ObjectiveInfoList_Item listPosition in this.listPositions)
      {
        listPosition.OnObjectiveComplete();
        UnityEngine.Object.Destroy((UnityEngine.Object) listPosition.gameObject);
      }
      this.listPositions.Clear();
    }
    this.enabled = false;
  }

  public void OnStarted(Objective obj)
  {
    if (this.objective != obj)
      return;
    this.OBJ_State.text = this.objective.Status.ToString();
    this.OBJ_Complete.text = $"{(ValueType) (float) (100.0 * (double) this.objective.CompletePercent):F0}%";
  }

  public void OnDestroy()
  {
    MissionManager.onObjectiveComplete -= new Action<Objective>(this.OnComplete);
    MissionManager.onObjectiveStarted -= new Action<Objective>(this.OnStarted);
  }
}
