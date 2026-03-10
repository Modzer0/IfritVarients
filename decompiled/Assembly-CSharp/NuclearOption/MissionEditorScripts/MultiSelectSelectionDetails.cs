// Decompiled with JetBrains decompiler
// Type: NuclearOption.MissionEditorScripts.MultiSelectSelectionDetails
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.SavedMission.ObjectiveV2;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#nullable disable
namespace NuclearOption.MissionEditorScripts;

public class MultiSelectSelectionDetails : SelectionDetails, IDisposable
{
  public readonly List<SingleSelectionDetails> Items = new List<SingleSelectionDetails>();
  private readonly List<Vector3> positionOffsets = new List<Vector3>();
  private readonly List<Transform> rotationProxy = new List<Transform>();
  private Transform proxyParent;
  public static GroupRotationMode RotationMode;
  private bool lockList;

  public override bool PositionHandleAllowed
  {
    get
    {
      return this.Items.Any<SingleSelectionDetails>((Func<SingleSelectionDetails, bool>) (x => x.PositionHandleAllowed));
    }
  }

  public override bool RotationHandleAllowed
  {
    get
    {
      return this.Items.Any<SingleSelectionDetails>((Func<SingleSelectionDetails, bool>) (x => x.RotationHandleAllowed));
    }
  }

  public System.Type SelectionType => this.Items[0].GetType();

  public override string DisplayName => $"{this.Items.Count} Objects";

  public override bool TryGetFaction(out Faction faction)
  {
    bool faction1 = false;
    faction = (Faction) null;
    foreach (SelectionDetails selectionDetails in this.Items)
    {
      Faction faction2;
      if (selectionDetails.TryGetFaction(out faction2))
      {
        if (!faction1)
        {
          faction = faction2;
          faction1 = true;
        }
        else if ((UnityEngine.Object) faction != (UnityEngine.Object) faction2)
        {
          faction = (Faction) null;
          return false;
        }
      }
    }
    return faction1;
  }

  public override bool IsDestroyed => false;

  public MultiSelectSelectionDetails()
  {
    this.PositionWrapper = (IValueWrapper<GlobalPosition>) new ValueWrapperGlobalPosition();
    this.PositionWrapper.RegisterOnChange((object) this, new Action(this.OnPositionChanged));
    this.RotationWrapper = (IValueWrapper<Quaternion>) new ValueWrapperQuaternion();
    this.RotationWrapper.RegisterOnChange((object) this, new Action(this.OnRotationChanged));
  }

  public override bool Delete()
  {
    this.lockList = true;
    try
    {
      for (int index = this.Items.Count - 1; index >= 0; --index)
      {
        if (this.Items[index].Delete())
          this.Items.RemoveAt(index);
      }
    }
    finally
    {
      this.lockList = false;
    }
    this.AfterRemove();
    return false;
  }

  public override void Focus()
  {
    Vector3 v = new Vector3();
    foreach (SingleSelectionDetails selectionDetails in this.Items)
      v += selectionDetails.PositionWrapper.Value.AsVector3() / (float) this.Items.Count;
    GlobalPosition globalPosition = new GlobalPosition(v);
    float num1 = 0.0f;
    foreach (SelectionDetails selectionDetails in this.Items)
    {
      float num2 = FastMath.Distance(selectionDetails.PositionWrapper.Value, globalPosition);
      if ((double) num1 < (double) num2)
        num1 = num2;
    }
    float distance = Mathf.Clamp(num1, 50f, 2000f);
    SceneSingleton<CameraStateManager>.i.FocusPosition(globalPosition.ToLocalPosition(), new Quaternion?(), distance);
  }

  public void Add(SingleSelectionDetails single)
  {
    foreach (SingleSelectionDetails selectionDetails in this.Items)
    {
      if (selectionDetails.Source == single.Source)
      {
        Debug.LogError((object) $"{single.Source} is already in multiselect");
        return;
      }
    }
    this.Items.Add(single);
    this.AfterAdd();
  }

  public void ClearIfSelected(IEditorSelectable value) => this.Remove(value, false);

  public void Remove(IEditorSelectable obj, bool errorIfNotSelected = true)
  {
    if (this.lockList)
      return;
    for (int index = 0; index < this.Items.Count; ++index)
    {
      if (this.Items[index].Source == obj)
      {
        this.Items.RemoveAt(index);
        this.AfterRemove();
        return;
      }
    }
    if (!errorIfNotSelected)
      return;
    Debug.LogError((object) $"Count not find {obj} in multiselect");
  }

  public void RemoveAll<T>(List<T> toRemove, bool errorIfNotSelected) where T : IEditorSelectable
  {
    if (this.lockList)
      return;
    foreach (T obj in toRemove)
    {
      bool flag = false;
      for (int index = 0; index < this.Items.Count; ++index)
      {
        if (this.Items[index].Source == (object) obj)
        {
          this.Items.RemoveAt(index);
          flag = true;
          break;
        }
      }
      if (!flag & errorIfNotSelected)
        Debug.LogError((object) $"Count not find {obj} in multiselect");
    }
    this.AfterRemove();
  }

  private void AfterAdd() => this.RecalculatePositionsAndRotation();

  private void AfterRemove()
  {
    if (this.Items.Count == 0)
      SceneSingleton<UnitSelection>.i.ClearMultiSelection(this);
    else if (this.Items.Count == 1)
      SceneSingleton<UnitSelection>.i.ReplaceMultiSelection(this, this.Items[0]);
    else
      this.RecalculatePositionsAndRotation();
  }

  public void Dispose()
  {
    if ((UnityEngine.Object) this.proxyParent != (UnityEngine.Object) null)
      UnityEngine.Object.Destroy((UnityEngine.Object) this.proxyParent.gameObject);
    foreach (Transform transform in this.rotationProxy)
    {
      if ((UnityEngine.Object) transform != (UnityEngine.Object) null)
        UnityEngine.Object.Destroy((UnityEngine.Object) transform.gameObject);
    }
  }

  public void RecalculatePositionsAndRotation()
  {
    Debug.Log((object) nameof (RecalculatePositionsAndRotation));
    this.RecalculatePositions();
    this.RecalculateRotations();
  }

  private void RecalculatePositions()
  {
    while (this.positionOffsets.Count < this.Items.Count)
      this.positionOffsets.Add(new Vector3());
    while (this.positionOffsets.Count > this.Items.Count)
      this.positionOffsets.RemoveAt(this.positionOffsets.Count - 1);
    int num = 0;
    foreach (SelectionDetails selectionDetails in this.Items)
    {
      if (selectionDetails.PositionHandleAllowed)
        ++num;
    }
    if (num == 0)
      return;
    GlobalPosition globalPosition = new GlobalPosition();
    foreach (SingleSelectionDetails selectionDetails in this.Items)
    {
      if (selectionDetails.PositionHandleAllowed)
        globalPosition += selectionDetails.PositionWrapper.Value.AsVector3() / (float) num;
    }
    for (int index = 0; index < this.Items.Count; ++index)
    {
      SingleSelectionDetails selectionDetails = this.Items[index];
      if (selectionDetails.PositionHandleAllowed)
        this.positionOffsets[index] = selectionDetails.PositionWrapper.Value - globalPosition;
    }
    this.PositionWrapper.SetValue(globalPosition, (object) this);
  }

  private void RecalculateRotations()
  {
    int num = 0;
    foreach (SelectionDetails selectionDetails in this.Items)
    {
      if (selectionDetails.RotationHandleAllowed)
        ++num;
    }
    if (num == 0)
      return;
    if ((UnityEngine.Object) this.proxyParent == (UnityEngine.Object) null)
    {
      GameObject primitive = GameObject.CreatePrimitive(PrimitiveType.Cube);
      primitive.name = $"Multiselect_Group_proxy {this.positionOffsets.Count}";
      this.proxyParent = primitive.transform;
    }
    while (this.rotationProxy.Count < this.Items.Count)
    {
      GameObject primitive = GameObject.CreatePrimitive(PrimitiveType.Cube);
      primitive.name = $"Multiselect_proxy {this.positionOffsets.Count}";
      primitive.transform.SetParent(this.proxyParent);
      this.rotationProxy.Add(primitive.transform);
    }
    while (this.rotationProxy.Count > this.Items.Count)
    {
      int index = this.rotationProxy.Count - 1;
      UnityEngine.Object.Destroy((UnityEngine.Object) this.rotationProxy[index]);
      this.rotationProxy.RemoveAt(index);
    }
    this.proxyParent.position = this.PositionWrapper.Value.ToLocalPosition();
    Quaternion? nullable = new Quaternion?();
    for (int index = 0; index < this.Items.Count; ++index)
    {
      SingleSelectionDetails selectionDetails = this.Items[index];
      if (selectionDetails.RotationHandleAllowed && !nullable.HasValue)
        nullable = new Quaternion?(selectionDetails.RotationWrapper.Value);
      if (selectionDetails.PositionHandleAllowed || selectionDetails.RotationHandleAllowed)
        this.rotationProxy[index].SetPositionAndRotation(selectionDetails.PositionWrapper.Value.ToLocalPosition(), selectionDetails.RotationWrapper.Value);
    }
    Quaternion quaternion = nullable ?? Quaternion.identity;
    this.proxyParent.rotation = quaternion;
    this.RotationWrapper.SetValue(quaternion, (object) this);
  }

  private void OnPositionChanged()
  {
    GlobalPosition globalPosition = this.PositionWrapper.Value;
    for (int index = 0; index < this.Items.Count; ++index)
    {
      SingleSelectionDetails selectionDetails1 = this.Items[index];
      if (selectionDetails1.PositionHandleAllowed)
      {
        GlobalPosition position = globalPosition + this.positionOffsets[index];
        if (selectionDetails1 is UnitSelectionDetails selectionDetails2)
          position = selectionDetails2.ClampPosition(position);
        selectionDetails1.PositionWrapper.SetValue(position, (object) this);
      }
    }
    if (!((UnityEngine.Object) this.proxyParent != (UnityEngine.Object) null))
      return;
    this.proxyParent.position = this.PositionWrapper.Value.ToLocalPosition();
  }

  private void OnRotationChanged()
  {
    Quaternion quaternion1 = this.RotationWrapper.Value;
    if (MultiSelectSelectionDetails.RotationMode == GroupRotationMode.Local)
    {
      Quaternion quaternion2 = quaternion1 * Quaternion.Inverse(this.proxyParent.rotation);
      this.proxyParent.rotation = quaternion1;
      for (int index = 0; index < this.Items.Count; ++index)
      {
        SingleSelectionDetails selectionDetails = this.Items[index];
        if (selectionDetails.RotationHandleAllowed)
          selectionDetails.RotationWrapper.SetValue(quaternion2 * selectionDetails.RotationWrapper.Value, (object) this);
        if (selectionDetails.PositionHandleAllowed || selectionDetails.RotationHandleAllowed)
          this.rotationProxy[index].SetPositionAndRotation(selectionDetails.PositionWrapper.Value.ToLocalPosition(), selectionDetails.RotationWrapper.Value);
      }
    }
    else
    {
      this.proxyParent.rotation = quaternion1;
      for (int index = 0; index < this.Items.Count; ++index)
      {
        SingleSelectionDetails selectionDetails1 = this.Items[index];
        if (selectionDetails1.PositionHandleAllowed || selectionDetails1.RotationHandleAllowed)
        {
          Vector3 position1;
          Quaternion rotation;
          this.rotationProxy[index].GetPositionAndRotation(out position1, out rotation);
          GlobalPosition position2 = position1.ToGlobalPosition();
          if (selectionDetails1.RotationHandleAllowed)
            selectionDetails1.RotationWrapper.SetValue(rotation, (object) this);
          if (selectionDetails1.PositionHandleAllowed)
          {
            if (selectionDetails1 is UnitSelectionDetails selectionDetails2)
              position2 = selectionDetails2.ClampPosition(position2);
            selectionDetails1.PositionWrapper.SetValue(position2, (object) this);
          }
        }
      }
    }
  }
}
