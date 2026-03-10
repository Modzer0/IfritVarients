// Decompiled with JetBrains decompiler
// Type: RoadPathfinding.Node
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System.Collections.Generic;

#nullable disable
namespace RoadPathfinding;

public class Node
{
  public int id;
  public GlobalPosition position;
  public Dictionary<Road, Node> connectionsLookup = new Dictionary<Road, Node>();
  public Node parent;
  public float dist;

  public Node(RoadNetwork roadNetwork, GlobalPosition position)
  {
    this.position = position;
    this.id = roadNetwork.nodes.Count;
    roadNetwork.nodes.Add(this);
  }

  public void GenerateConnections(RoadNetwork roadNetwork)
  {
    this.connectionsLookup.Clear();
    foreach (Road road in roadNetwork.roads)
    {
      if (road.startNode == this || road.endNode == this)
      {
        Node node = road.startNode != this ? road.startNode : road.endNode;
        this.connectionsLookup.Add(road, node);
      }
    }
  }

  public void ClearPathfindingData()
  {
    this.parent = (Node) null;
    this.dist = float.MaxValue;
  }
}
