// Decompiled with JetBrains decompiler
// Type: AStar.Node
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;

#nullable disable
namespace AStar;

public class Node : IHeapItem<Node>, IComparable<Node>
{
  public float traversability;
  public bool traversable;
  public Vector3 worldPosition;
  private int heapIndex;
  public int gCost;
  public int hCost;
  public Node parent;
  public int gridX;
  public int gridY;

  public int fCost => this.gCost + this.hCost;

  public Node(float traversability, Vector3 worldPosition, int gridX, int gridY)
  {
    this.traversability = traversability;
    this.traversable = (double) traversability > 0.05000000074505806;
    this.worldPosition = worldPosition;
    this.gridX = gridX;
    this.gridY = gridY;
  }

  public int HeapIndex
  {
    get => this.heapIndex;
    set => this.heapIndex = value;
  }

  public int CompareTo(Node nodeToCompare)
  {
    int num = this.fCost.CompareTo(nodeToCompare.fCost);
    if (num == 0)
      num = this.hCost.CompareTo(nodeToCompare.hCost);
    return -num;
  }
}
