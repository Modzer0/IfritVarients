// Decompiled with JetBrains decompiler
// Type: ObjectiveMarkerManager
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
public class ObjectiveMarkerManager : MonoBehaviour
{
  [SerializeField]
  private ObjectiveMarker markerPrefab;
  private readonly List<ObjectiveMarker> objectiveMarkers = new List<ObjectiveMarker>();
  private Transform iconLayer;
  private List<MissionPosition.PositionResult> resultCache = new List<MissionPosition.PositionResult>();

  public void Initialize(Transform layer)
  {
    this.iconLayer = layer;
    this.StartSlowUpdateDelayed(1f, new Action(this.UpdateObjectiveMarkers));
  }

  private void Update()
  {
    if ((UnityEngine.Object) SceneSingleton<DynamicMap>.i.HQ == (UnityEngine.Object) null || MissionManager.Runner == null)
      return;
    this.UpdateObjectiveMarkers();
  }

  private ObjectiveMarker CreateItem(MissionPosition.PositionResult positionResult)
  {
    ObjectiveMarker objectiveMarker = UnityEngine.Object.Instantiate<ObjectiveMarker>(this.markerPrefab, this.iconLayer);
    objectiveMarker.SetObjective(this, positionResult);
    return objectiveMarker;
  }

  private void UpdateObjectiveMarkers()
  {
    List<MissionPosition.PositionResult> allPositions = this.GetAllPositions();
    while (this.objectiveMarkers.Count < allPositions.Count)
      this.objectiveMarkers.Add(this.CreateItem(allPositions[this.objectiveMarkers.Count]));
    for (int index1 = 0; index1 < this.objectiveMarkers.Count; ++index1)
    {
      ObjectiveMarker objectiveMarker = this.objectiveMarkers[index1];
      if (index1 < allPositions.Count && SceneSingleton<MapOptions>.i.showObjectives)
      {
        bool flag = false;
        for (int index2 = 0; index2 < allPositions.Count; ++index2)
        {
          if (index1 != index2 && allPositions[index1].Objective == allPositions[index2].Objective)
          {
            float num = Vector3.Distance(objectiveMarker.transform.localPosition, this.objectiveMarkers[index2].transform.localPosition) * SceneSingleton<DynamicMap>.i.mapImage.transform.localScale.x;
            if (this.objectiveMarkers[index2].shown && !this.objectiveMarkers[index2].masked && (double) num < 40.0)
              flag = true;
          }
        }
        objectiveMarker.Show(true);
        if (flag)
          objectiveMarker.Mask();
        objectiveMarker.UpdateMarker(allPositions[index1]);
      }
      else
        objectiveMarker.Show(false);
    }
  }

  private List<MissionPosition.PositionResult> GetAllPositions()
  {
    MissionPosition.GetAllPositionsResults(SceneSingleton<DynamicMap>.i.HQ, Datum.originPosition.ToGlobalPosition(), false, this.resultCache);
    return this.resultCache;
  }
}
