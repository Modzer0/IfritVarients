// Decompiled with JetBrains decompiler
// Type: NodeGrid
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System.Collections.Generic;
using UnityEngine;

#nullable disable
public class NodeGrid : MonoBehaviour
{
  public Transform tester;
  public Texture2D traversabilityMap;
  public Vector2 gridWorldSize;
  private float nodeRadius;
  private AStar.Node[,] grid;
  private float nodeDiameter;
  public int maxSize;
  private int gridSizeX;
  private int gridSizeY;
  public bool initialized;
  public List<AStar.Node> path;

  private void Awake()
  {
    this.gridSizeX = this.traversabilityMap.width;
    this.gridSizeY = this.traversabilityMap.height;
    this.maxSize = this.gridSizeX * this.gridSizeY;
    this.nodeDiameter = this.gridWorldSize.x / (float) this.gridSizeX;
    this.nodeRadius = this.nodeDiameter * 0.5f;
    this.CreateGrid();
    this.initialized = true;
  }

  private void CreateGrid()
  {
    this.grid = new AStar.Node[this.gridSizeX, this.gridSizeY];
    Vector3 vector3 = this.transform.position - Vector3.right * this.gridWorldSize.x / 2f - Vector3.forward * this.gridWorldSize.y / 2f;
    for (int index1 = 0; index1 < this.gridSizeX; ++index1)
    {
      for (int index2 = 0; index2 < this.gridSizeY; ++index2)
      {
        Vector3 worldPosition = vector3 + Vector3.right * ((float) index1 * this.nodeDiameter + this.nodeRadius) + Vector3.forward * ((float) index2 * this.nodeDiameter + this.nodeRadius);
        float r = this.traversabilityMap.GetPixel(index1, index2).r;
        this.grid[index1, index2] = new AStar.Node(r, worldPosition, index1, index2);
      }
    }
  }

  public List<AStar.Node> GetNeighbors(AStar.Node node)
  {
    List<AStar.Node> neighbors = new List<AStar.Node>();
    for (int index1 = -1; index1 <= 1; ++index1)
    {
      for (int index2 = -1; index2 <= 1; ++index2)
      {
        if (index1 != 0 || index2 != 0)
        {
          int index3 = node.gridX + index1;
          int index4 = node.gridY + index2;
          if (index3 >= 0 && index3 < this.gridSizeX && index4 >= 0 && index4 < this.gridSizeY)
            neighbors.Add(this.grid[index3, index4]);
        }
      }
    }
    return neighbors;
  }

  public AStar.Node NodeFromWorldPoint(GlobalPosition worldPosition)
  {
    float num1 = (worldPosition.x + this.gridWorldSize.x / 2f) / this.gridWorldSize.x;
    float num2 = (worldPosition.z + this.gridWorldSize.y / 2f) / this.gridWorldSize.y;
    float num3 = Mathf.Clamp01(num1);
    float num4 = Mathf.Clamp01(num2);
    return this.grid[Mathf.RoundToInt((float) (this.gridSizeX - 1) * num3), Mathf.RoundToInt((float) (this.gridSizeY - 1) * num4)];
  }

  private void OnDrawGizmos()
  {
    Gizmos.DrawWireCube(this.transform.position, new Vector3(this.gridWorldSize.x, 1000f, this.gridWorldSize.y));
    if (this.grid == null)
      return;
    this.NodeFromWorldPoint(this.tester.GlobalPosition());
    if (this.path == null)
      return;
    foreach (AStar.Node node in this.path)
    {
      Gizmos.color = Color.white;
      Gizmos.DrawCube(new GlobalPosition(node.worldPosition).ToLocalPosition() + Vector3.up * 300f, Vector3.one * this.nodeDiameter * 0.9f);
    }
  }
}
