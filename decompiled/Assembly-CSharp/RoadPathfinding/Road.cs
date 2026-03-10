// Decompiled with JetBrains decompiler
// Type: RoadPathfinding.Road
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
namespace RoadPathfinding;

[Serializable]
public class Road
{
  [SerializeField]
  private Bounds bounds;
  [SerializeField]
  private bool bridge;
  public List<GlobalPosition> points = new List<GlobalPosition>();
  public float length;
  [NonSerialized]
  public Node startNode;
  [NonSerialized]
  public Node endNode;
  private bool editable;

  public Road()
  {
    this.bounds = new Bounds();
    this.points = new List<GlobalPosition>();
    double num = (double) this.CalcLength();
    this.UpdateBB();
  }

  public void SetEditable(bool canEdit) => this.editable = canEdit;

  public void SetBridge(bool isBridge) => this.bridge = isBridge;

  public bool IsEditable() => this.editable;

  public bool IsBridge() => this.bridge;

  public void AddPoint(GlobalPosition point)
  {
    this.points.Add(point);
    this.UpdateBB();
  }

  public bool InBounds(GlobalPosition point) => this.bounds.Contains(point.AsVector3());

  public void UpdateBB()
  {
    if (this.points.Count == 0)
      return;
    this.bounds.center = this.points[0].AsVector3();
    this.bounds.size = Vector3.up * 10000f;
    for (int index = 0; index < this.points.Count; ++index)
      this.bounds.Encapsulate(this.points[index].AsVector3());
  }

  public void TrySplit(RoadNetwork roadNetwork, GlobalPosition splitPosition)
  {
    if (!this.bounds.Contains(splitPosition.AsVector3()))
      return;
    int index1 = -1;
    for (int index2 = 1; index2 < this.points.Count - 1; ++index2)
    {
      if (FastMath.InRange(splitPosition, this.points[index2], 10f))
      {
        index1 = index2;
        break;
      }
    }
    if (index1 == -1)
      return;
    Road road = new Road();
    road.AddPoint(this.points[index1]);
    for (int index3 = index1 - 1; index3 >= 0; --index3)
    {
      road.AddPoint(this.points[index3]);
      this.points.RemoveAt(index3);
    }
    double num1 = (double) road.CalcLength();
    roadNetwork.roads.Add(road);
    double num2 = (double) this.CalcLength();
    this.UpdateBB();
  }

  public float CalcLength()
  {
    this.length = 0.0f;
    for (int index = 0; index < this.points.Count - 1; ++index)
      this.length += FastMath.Distance(this.points[index], this.points[index + 1]);
    return this.length;
  }

  public void GenerateNodes(RoadNetwork roadNetwork)
  {
    this.startNode = (Node) null;
    this.endNode = (Node) null;
    for (int index = 0; index < roadNetwork.nodes.Count; ++index)
    {
      if (FastMath.InRange(this.points[0], roadNetwork.nodes[index].position, 10f))
        this.startNode = roadNetwork.nodes[index];
      List<GlobalPosition> points = this.points;
      if (FastMath.InRange(points[points.Count - 1], roadNetwork.nodes[index].position, 10f))
        this.endNode = roadNetwork.nodes[index];
    }
    if (this.startNode == null)
      this.startNode = new Node(roadNetwork, this.points[0]);
    if (this.endNode != null)
      return;
    RoadNetwork roadNetwork1 = roadNetwork;
    List<GlobalPosition> points1 = this.points;
    GlobalPosition position = points1[points1.Count - 1];
    this.endNode = new Node(roadNetwork1, position);
  }

  public void CheckIntersection(RoadNetwork roadNetwork)
  {
    for (int index = roadNetwork.roads.Count - 1; index >= 0; --index)
    {
      if (roadNetwork.roads[index] != this)
      {
        roadNetwork.roads[index].TrySplit(roadNetwork, this.points[0]);
        Road road = roadNetwork.roads[index];
        RoadNetwork roadNetwork1 = roadNetwork;
        List<GlobalPosition> points = this.points;
        GlobalPosition splitPosition = points[points.Count - 1];
        road.TrySplit(roadNetwork1, splitPosition);
      }
    }
  }
}
