// Decompiled with JetBrains decompiler
// Type: Pathfinding
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

#nullable disable
public class Pathfinding : MonoBehaviour
{
  private NodeGrid grid;
  public Transform seeker;
  public Transform target;
  private bool pathFound;
  private Stopwatch sw = new Stopwatch();

  private void Awake() => this.grid = this.GetComponent<NodeGrid>();

  private void Update()
  {
    if (!this.grid.initialized || this.pathFound || !Input.GetKey(KeyCode.Space))
      return;
    this.FindPath(this.seeker.position.ToGlobalPosition(), this.target.GlobalPosition());
    this.pathFound = true;
  }

  private int GetDistance(AStar.Node nodeA, AStar.Node nodeB)
  {
    int num1 = Mathf.Abs(nodeA.gridX - nodeB.gridX);
    int num2 = Mathf.Abs(nodeA.gridY - nodeB.gridY);
    return num1 > num2 ? 14 * num2 + 10 * (num1 - num2) : 14 * num1 + 10 * (num2 - num1);
  }

  private void FindPath(GlobalPosition startPos, GlobalPosition targetPos)
  {
    this.sw.Start();
    AStar.Node startNode = this.grid.NodeFromWorldPoint(startPos);
    AStar.Node node1 = this.grid.NodeFromWorldPoint(targetPos);
    Heap<AStar.Node> heap = new Heap<AStar.Node>(this.grid.maxSize);
    HashSet<AStar.Node> nodeSet = new HashSet<AStar.Node>();
    heap.Add(startNode);
    while (heap.Count > 0)
    {
      AStar.Node node2 = heap.RemoveFirst();
      nodeSet.Add(node2);
      if (node2 == node1)
      {
        this.sw.Stop();
        this.RetracePath(startNode, node1);
        break;
      }
      foreach (AStar.Node neighbor in this.grid.GetNeighbors(node2))
      {
        if (neighbor.traversable && !nodeSet.Contains(neighbor))
        {
          int num = node2.gCost + (int) ((double) this.GetDistance(node2, neighbor) / (double) neighbor.traversability);
          if (num < neighbor.gCost || !heap.Contains(neighbor))
          {
            neighbor.gCost = num;
            neighbor.hCost = this.GetDistance(neighbor, node1);
            neighbor.parent = node2;
            if (!heap.Contains(neighbor))
              heap.Add(neighbor);
            else
              heap.UpdateItem(neighbor);
          }
        }
      }
    }
  }

  private void RetracePath(AStar.Node startNode, AStar.Node endNode)
  {
    List<AStar.Node> nodeList = new List<AStar.Node>();
    for (AStar.Node node = endNode; node != startNode; node = node.parent)
      nodeList.Add(node);
    nodeList.Reverse();
    this.grid.path = nodeList;
  }
}
