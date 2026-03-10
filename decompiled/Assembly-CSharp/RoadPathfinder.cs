// Decompiled with JetBrains decompiler
// Type: RoadPathfinder
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using RoadPathfinding;
using System;
using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;

#nullable disable
public static class RoadPathfinder
{
  private static readonly ProfilerMarker tryPathfindMarker = new ProfilerMarker("RoadPathfinder.TryPathfind");
  private static readonly List<RoadPathfinding.Node> unvisitedSet = new List<RoadPathfinding.Node>();
  private static List<GlobalPosition> generatedWaypoints = new List<GlobalPosition>();

  public static bool TryPathfind(
    RoadNetwork network,
    GlobalPosition startPos,
    GlobalPosition targetPos,
    List<RoadPathfinding.Node> results)
  {
    using (RoadPathfinder.tryPathfindMarker.Auto())
    {
      network.ClearPathfindingData();
      RoadPathfinding.Node closestNode1;
      if (!RoadPathfinder.TryFindNearestNode(network, startPos, out closestNode1))
      {
        Debug.LogWarning((object) "Failed to find node");
        return false;
      }
      closestNode1.dist = 0.0f;
      RoadPathfinding.Node closestNode2;
      if (!RoadPathfinder.TryFindNearestNode(network, targetPos, out closestNode2))
      {
        Debug.LogWarning((object) "Failed to find node");
        return false;
      }
      RoadPathfinder.unvisitedSet.Clear();
      RoadPathfinder.unvisitedSet.AddRange((IEnumerable<RoadPathfinding.Node>) network.nodes);
      while (RoadPathfinder.unvisitedSet.Count > 0)
      {
        RoadPathfinder.unvisitedSet.Sort((Comparison<RoadPathfinding.Node>) ((a, b) => a.dist.CompareTo(b.dist)));
        closestNode1 = RoadPathfinder.unvisitedSet[0];
        RoadPathfinder.unvisitedSet.Remove(closestNode1);
        foreach (KeyValuePair<Road, RoadPathfinding.Node> keyValuePair in closestNode1.connectionsLookup)
        {
          RoadPathfinding.Node node = keyValuePair.Value;
          Road key = keyValuePair.Key;
          if (RoadPathfinder.unvisitedSet.Contains(node) && (double) closestNode1.dist + (double) key.length < (double) node.dist)
          {
            node.dist = closestNode1.dist + key.length;
            node.parent = closestNode1;
          }
        }
        if ((double) closestNode1.dist == 3.4028234663852886E+38)
        {
          results.Clear();
          return false;
        }
        if (closestNode1 == closestNode2)
          break;
      }
      results.Clear();
      for (; closestNode1.parent != null; closestNode1 = closestNode1.parent)
        results.Add(closestNode1);
      results.Add(closestNode1);
      results.Reverse();
      return true;
    }
  }

  public static bool TryFindNearestNode(
    RoadNetwork network,
    GlobalPosition worldPos,
    out RoadPathfinding.Node closestNode)
  {
    closestNode = (RoadPathfinding.Node) null;
    float num1 = float.MaxValue;
    for (int index = 0; index < network.nodes.Count; ++index)
    {
      float num2 = FastMath.SquareDistance(worldPos, network.nodes[index].position);
      if ((double) num2 < (double) num1)
      {
        closestNode = network.nodes[index];
        num1 = num2;
      }
    }
    return closestNode != null;
  }

  public static List<GlobalPosition> GetWaypointsToStartNode(
    RoadPoint startInfo,
    List<RoadPathfinding.Node> nodelist,
    bool debug,
    out Road startRoad)
  {
    startRoad = startInfo.road;
    RoadPathfinder.generatedWaypoints.Clear();
    if (nodelist.Count > 1 && (startRoad.startNode == nodelist[1] || startRoad.endNode == nodelist[1]))
    {
      int index2 = startRoad.startNode == nodelist[1] ? 1 : startRoad.points.Count - 2;
      RoadPathfinder.GetRoadFragment(RoadPathfinder.generatedWaypoints, startRoad, startInfo.index, index2, out Road _);
      nodelist.RemoveAt(0);
    }
    else if (!FastMath.InRange(startRoad.points[startInfo.index], nodelist[0].position, 10f))
    {
      int index2 = startRoad.startNode == nodelist[0] ? 1 : startRoad.points.Count - 2;
      RoadPathfinder.GetRoadFragment(RoadPathfinder.generatedWaypoints, startRoad, startInfo.index, index2, out Road _);
      if (debug)
        Debug.Log((object) $"generating starting road fragment from {startInfo.index} to {index2}");
    }
    return RoadPathfinder.generatedWaypoints;
  }

  public static List<GlobalPosition> GetWaypointsFromEndNode(RoadPoint endInfo, List<RoadPathfinding.Node> nodelist)
  {
    RoadPathfinder.generatedWaypoints.Clear();
    Road road = endInfo.road;
    if (nodelist.Count == 0)
      return RoadPathfinder.generatedWaypoints;
    if (road.startNode == nodelist[0] || road.endNode == nodelist[0])
    {
      int index1 = road.startNode == nodelist[0] ? 0 : road.points.Count - 1;
      RoadPathfinder.GetRoadFragment(RoadPathfinder.generatedWaypoints, road, index1, endInfo.index, out Road _);
      nodelist.Clear();
      return RoadPathfinder.generatedWaypoints;
    }
    RoadPathfinder.generatedWaypoints = RoadPathfinder.GetWaypointsBetweenNodes(nodelist[0], nodelist[1], out Road _);
    int index1_1 = road.startNode == nodelist[1] ? 0 : road.points.Count - 1;
    RoadPathfinder.GetRoadFragment(RoadPathfinder.generatedWaypoints, road, index1_1, endInfo.index, out Road _);
    nodelist.Clear();
    return RoadPathfinder.generatedWaypoints;
  }

  public static RoadPoint GetNearestRoad(GlobalPosition position, RoadPathfinding.Node fromNode)
  {
    float num1 = float.MaxValue;
    int index1 = -1;
    Road road = (Road) null;
    foreach (KeyValuePair<Road, RoadPathfinding.Node> keyValuePair in fromNode.connectionsLookup)
    {
      Road key = keyValuePair.Key;
      for (int index2 = 0; index2 < key.points.Count; ++index2)
      {
        float num2 = FastMath.SquareDistance(position, key.points[index2]);
        if ((double) num2 < (double) num1)
        {
          road = key;
          num1 = num2;
          index1 = index2;
        }
      }
    }
    return new RoadPoint(road, index1);
  }

  public static RoadPoint GetStartingRoadPoint(GlobalPosition startPosition, List<RoadPathfinding.Node> nodelist)
  {
    Road road = (Road) null;
    float num1 = 0.0f;
    foreach (KeyValuePair<Road, RoadPathfinding.Node> keyValuePair in nodelist[0].connectionsLookup)
    {
      Road key = keyValuePair.Key;
      float num2 = 0.0f;
      foreach (GlobalPosition point in key.points)
        num2 += 1f / FastMath.SquareDistance(startPosition, point);
      if ((double) num2 > (double) num1)
      {
        road = key;
        num1 = num2;
      }
    }
    float num3 = float.MaxValue;
    int index1 = 0;
    for (int index2 = 0; index2 < road.points.Count; ++index2)
    {
      float num4 = FastMath.Distance(road.points[index2], startPosition);
      if ((double) num4 < (double) num3)
      {
        index1 = index2;
        num3 = num4;
      }
    }
    return new RoadPoint(road, index1);
  }

  public static void GetRoadFragment(
    List<GlobalPosition> result,
    Road road,
    int index1,
    int index2,
    out Road currentRoad)
  {
    currentRoad = road;
    if (index1 < index2)
    {
      for (int index = index1; index <= index2; ++index)
        result.Add(road.points[index]);
    }
    else
    {
      for (int index = index1; index >= index2; --index)
        result.Add(road.points[index]);
    }
  }

  public static List<GlobalPosition> GetWaypointsBetweenNodes(
    RoadPathfinding.Node startNode,
    RoadPathfinding.Node endNode,
    out Road currentRoad)
  {
    RoadPathfinder.generatedWaypoints.Clear();
    currentRoad = (Road) null;
    foreach (KeyValuePair<Road, RoadPathfinding.Node> keyValuePair in startNode.connectionsLookup)
    {
      if (keyValuePair.Key.endNode == endNode)
      {
        RoadPathfinder.GetRoadFragment(RoadPathfinder.generatedWaypoints, keyValuePair.Key, 0, keyValuePair.Key.points.Count - 2, out currentRoad);
        break;
      }
      if (keyValuePair.Key.startNode == endNode)
      {
        RoadPathfinder.GetRoadFragment(RoadPathfinder.generatedWaypoints, keyValuePair.Key, keyValuePair.Key.points.Count - 1, 1, out currentRoad);
        break;
      }
    }
    return RoadPathfinder.generatedWaypoints;
  }

  public static bool VisibleUnderwater(GlobalPosition fromPos, GlobalPosition toPos)
  {
    toPos.y = fromPos.y;
    return !Physics.Linecast(fromPos.ToLocalPosition(), toPos.ToLocalPosition(), 8256);
  }
}
