// Decompiled with JetBrains decompiler
// Type: RoadPathfinding.RoadNetwork
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;

#nullable disable
namespace RoadPathfinding;

[Serializable]
public class RoadNetwork
{
  public List<Road> roads = new List<Road>();
  [NonSerialized]
  public List<Node> nodes = new List<Node>();

  public bool Exists() => this.roads.Count > 0;

  public void RegenerateNetwork()
  {
    this.nodes.Clear();
    foreach (Road road in this.roads)
      road.GenerateNodes(this);
    foreach (Node node in this.nodes)
      node.GenerateConnections(this);
  }

  public void ClearPathfindingData()
  {
    foreach (Node node in this.nodes)
      node.ClearPathfindingData();
  }

  public bool TryGetNearestNode(GlobalPosition fromPosition, out Node nearestNode)
  {
    nearestNode = (Node) null;
    if (this.nodes.Count == 0)
      return false;
    float num = float.MaxValue;
    foreach (Node node in this.nodes)
    {
      if ((double) (fromPosition - node.position).sqrMagnitude < (double) num)
      {
        num = FastMath.SquareDistance(fromPosition, node.position);
        nearestNode = node;
      }
    }
    return true;
  }

  public bool TryGetNearestPoint(GlobalPosition fromPosition, out GlobalPosition nearestPoint)
  {
    nearestPoint = fromPosition;
    Node nearestNode;
    if (!this.TryGetNearestNode(fromPosition, out nearestNode))
      return false;
    float num1 = float.MaxValue;
    foreach (KeyValuePair<Road, Node> keyValuePair in nearestNode.connectionsLookup)
    {
      foreach (GlobalPosition point in keyValuePair.Key.points)
      {
        float num2 = FastMath.SquareDistance(point, fromPosition);
        if ((double) num2 < (double) num1)
        {
          nearestPoint = point;
          num1 = num2;
        }
      }
    }
    return true;
  }
}
