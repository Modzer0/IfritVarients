// Decompiled with JetBrains decompiler
// Type: ObjectiveOverlayManager
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
public class ObjectiveOverlayManager : MonoBehaviour
{
  [SerializeField]
  private ObjectiveOverlay overlayPrefab;
  [Header("Limit")]
  [SerializeField]
  private ObjectiveOverlayManager.LimitMode limitMode = ObjectiveOverlayManager.LimitMode.Multiple;
  [SerializeField]
  private int multipleLimit = 3;
  [Header("Stop text overlap")]
  [SerializeField]
  private float textDistance = 30f;
  [SerializeField]
  private Vector2 textPush = new Vector2(2000f, 500f);
  [SerializeField]
  private float textLerp = 0.8f;
  [SerializeField]
  private float textNudgeDecrease = 0.5f;
  private Aircraft aircraft;
  private Transform iconLayer;
  private readonly List<ObjectiveOverlay> overlays = new List<ObjectiveOverlay>();
  private List<MissionPosition.PositionResult> resultCache = new List<MissionPosition.PositionResult>();

  public void Initialize(Aircraft aircraft, Transform iconLayer)
  {
    this.aircraft = aircraft;
    this.iconLayer = iconLayer;
  }

  private ObjectiveOverlay CreateItem()
  {
    ObjectiveOverlay objectiveOverlay = UnityEngine.Object.Instantiate<ObjectiveOverlay>(this.overlayPrefab, this.transform);
    objectiveOverlay.Initialize(this.iconLayer);
    return objectiveOverlay;
  }

  private void Update()
  {
    if ((UnityEngine.Object) this.aircraft == (UnityEngine.Object) null || (UnityEngine.Object) this.aircraft.NetworkHQ == (UnityEngine.Object) null || MissionManager.Runner == null)
      return;
    this.UpdateOverlays();
    this.StopTextOverlap();
  }

  private void UpdateOverlays()
  {
    List<MissionPosition.PositionResult> allPositions = this.GetAllPositions();
    while (this.overlays.Count < allPositions.Count)
      this.overlays.Add(this.CreateItem());
    for (int index = 0; index < this.overlays.Count; ++index)
    {
      ObjectiveOverlay overlay = this.overlays[index];
      if (index < allPositions.Count)
        overlay.UpdateOverlay(allPositions[index]);
      else
        overlay.HideOverlay();
    }
  }

  private List<MissionPosition.PositionResult> GetAllPositions()
  {
    switch (this.limitMode)
    {
      case ObjectiveOverlayManager.LimitMode.One:
        this.resultCache.Clear();
        MissionPosition.PositionResult result;
        if (MissionPosition.TryGetClosestObjectivePosition((Unit) this.aircraft, out result))
        {
          this.resultCache.Add(result);
          break;
        }
        break;
      case ObjectiveOverlayManager.LimitMode.Multiple:
        MissionPosition.GetAllPositionsResults((Unit) this.aircraft, false, this.resultCache);
        if (this.resultCache.Count > this.multipleLimit)
        {
          this.resultCache.Sort((Comparison<MissionPosition.PositionResult>) ((a, b) => a.Distance.CompareTo(b.Distance)));
          this.resultCache.RemoveRange(this.multipleLimit, this.resultCache.Count - this.multipleLimit);
          break;
        }
        break;
      case ObjectiveOverlayManager.LimitMode.NoLimit:
        MissionPosition.GetAllPositionsResults((Unit) this.aircraft, false, this.resultCache);
        break;
    }
    return this.resultCache;
  }

  private void StopTextOverlap()
  {
    for (int index1 = 0; index1 < this.overlays.Count; ++index1)
    {
      ObjectiveOverlay overlay1 = this.overlays[index1];
      for (int index2 = index1 + 1; index2 < this.overlays.Count; ++index2)
      {
        ObjectiveOverlay overlay2 = this.overlays[index2];
        TextNoOverlap textNoOverlap1 = overlay1.TextNoOverlap;
        TextNoOverlap textNoOverlap2 = overlay2.TextNoOverlap;
        Vector2 targetPosition1 = textNoOverlap1.TargetPosition;
        Vector2 targetPosition2 = textNoOverlap2.TargetPosition;
        if ((double) Vector2.Distance(targetPosition1, targetPosition2) < (double) this.textDistance)
        {
          Vector2 vector2_1 = targetPosition2 - targetPosition1;
          if ((double) vector2_1.y < 0.10000000149011612)
            vector2_1.y += 5f;
          vector2_1.Normalize();
          float deltaTime = Time.deltaTime;
          Vector2 vector2_2 = new Vector2(vector2_1.x * this.textPush.x * deltaTime, vector2_1.y * this.textPush.y * deltaTime);
          textNoOverlap1.NudgeOffset += vector2_2;
          textNoOverlap2.NudgeOffset -= vector2_2;
        }
      }
    }
    foreach (ObjectiveOverlay overlay in this.overlays)
    {
      TextNoOverlap textNoOverlap = overlay.TextNoOverlap;
      Vector2 vector2 = Vector2.Lerp(textNoOverlap.PreviousPosition, textNoOverlap.TargetPosition + textNoOverlap.NudgeOffset, this.textLerp);
      textNoOverlap.Text.transform.position = (Vector3) vector2;
      textNoOverlap.PreviousPosition = vector2;
      textNoOverlap.NudgeOffset *= this.textNudgeDecrease;
      textNoOverlap.AutomaticlalySetPosition = false;
    }
  }

  [Serializable]
  private enum LimitMode
  {
    One,
    Multiple,
    NoLimit,
  }
}
