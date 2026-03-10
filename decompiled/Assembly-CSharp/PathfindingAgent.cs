// Decompiled with JetBrains decompiler
// Type: PathfindingAgent
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using RoadPathfinding;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
public class PathfindingAgent
{
  private readonly List<RoadPathfinding.Node> nodes = new List<RoadPathfinding.Node>();
  private readonly List<GlobalPosition> waypoints = new List<GlobalPosition>();
  private readonly List<GlobalPosition> nextWaypoints = new List<GlobalPosition>();
  private GlobalPosition targetPos;
  private Transform movingTarget;
  private RoadPoint startingRoadPoint;
  private RoadPoint endingRoadPoint;
  private Unit unit;
  private Road currentRoad;
  private float lastWaterCheck;
  private float lastSkipCheck;

  public PathfindingAgent(Unit unit)
  {
    this.nodes = new List<RoadPathfinding.Node>();
    this.waypoints = new List<GlobalPosition>();
    this.nextWaypoints = new List<GlobalPosition>();
    this.unit = unit;
  }

  public void Shortcut(GlobalPosition destination)
  {
    this.movingTarget = (Transform) null;
    this.nodes.Clear();
    this.waypoints.Clear();
    this.waypoints.Add(destination);
    this.currentRoad = (Road) null;
    if (!PlayerSettings.debugVis || !((Object) this.unit == (Object) SceneSingleton<CameraStateManager>.i.followingUnit))
      return;
    NetworkSceneSingleton<LevelInfo>.i.VisualizeWaypoints(new List<GlobalPosition>()
    {
      this.unit.GlobalPosition(),
      this.targetPos
    }, this.unit);
  }

  public bool IsOnBridge() => this.currentRoad != null && this.currentRoad.IsBridge();

  public void Pathfind(
    RoadNetwork network,
    GlobalPosition targetPos,
    bool stayOnRoad,
    Transform lineOfSightChecker)
  {
    this.movingTarget = (Transform) null;
    this.nodes.Clear();
    this.waypoints.Clear();
    GlobalPosition globalPosition = this.unit.GlobalPosition();
    if (network == null || network.roads.Count == 0)
    {
      this.waypoints.Add(targetPos);
    }
    else
    {
      this.targetPos = targetPos;
      RaycastHit hit;
      if (!(this.unit is Ship) && PathfindingAgent.RaycastTerrain(targetPos, out hit))
        this.targetPos = hit.point.ToGlobalPosition();
      if (!RoadPathfinder.TryPathfind(network, globalPosition, this.targetPos, this.nodes))
        this.waypoints.Add(globalPosition);
      else
        this.waypoints.AddRange((IEnumerable<GlobalPosition>) this.GetStartingWaypoints(globalPosition, lineOfSightChecker));
      this.lastSkipCheck = 0.0f;
      this.SkipWaypointIfBehind(globalPosition);
      this.RouteDebug();
    }
  }

  public GlobalPosition GetDryPosition(GlobalPosition startPos, GlobalPosition targetPos)
  {
    RaycastHit hit1;
    if ((double) targetPos.y == 0.0 && PathfindingAgent.RaycastTerrain(targetPos, out hit1))
      targetPos = hit1.point.ToGlobalPosition();
    Vector3 vector3 = targetPos.ToLocalPosition();
    Vector3 localPosition = startPos.ToLocalPosition();
    for (int index = 0; index < 5; ++index)
    {
      RaycastHit hit2;
      if (PathfindingAgent.RaycastTerrain(vector3, out hit2) && (double) hit2.point.y >= (double) Datum.LocalSeaY)
      {
        vector3.y = hit2.point.y;
        break;
      }
      vector3 = FastMath.LerpXZ(vector3, localPosition, 0.5f);
    }
    GlobalPosition globalPosition = vector3.ToGlobalPosition();
    globalPosition.NotEqual(targetPos);
    return globalPosition;
  }

  private List<GlobalPosition> GetStartingWaypoints(
    GlobalPosition startPos,
    Transform lineOfSightChecker)
  {
    this.nextWaypoints.Clear();
    if ((Object) lineOfSightChecker != (Object) null)
    {
      if (RoadPathfinder.VisibleUnderwater(lineOfSightChecker.GlobalPosition(), this.targetPos))
      {
        this.Shortcut(this.targetPos);
        return this.nextWaypoints;
      }
      if (this.TrySkipNodesUnderwater(lineOfSightChecker))
        startPos = this.nodes[0].position;
    }
    this.startingRoadPoint = RoadPathfinder.GetStartingRoadPoint(startPos, this.nodes);
    GlobalPosition targetPos = this.targetPos;
    List<RoadPathfinding.Node> nodes = this.nodes;
    RoadPathfinding.Node fromNode = nodes[nodes.Count - 1];
    this.endingRoadPoint = RoadPathfinder.GetNearestRoad(targetPos, fromNode);
    float num1 = FastMath.SquareDistance(startPos, this.targetPos);
    float num2 = FastMath.SquareDistance(startPos, this.startingRoadPoint.road.points[this.startingRoadPoint.index]);
    if ((Object) lineOfSightChecker == (Object) null && (double) num2 > (double) num1 * 0.25)
    {
      this.nodes.Clear();
      this.nextWaypoints.Add(this.targetPos);
      return this.nextWaypoints;
    }
    if (this.startingRoadPoint.road != this.endingRoadPoint.road)
      return RoadPathfinder.GetWaypointsToStartNode(this.startingRoadPoint, this.nodes, (Object) SceneSingleton<CameraStateManager>.i.followingUnit == (Object) this.unit, out this.currentRoad);
    this.nodes.Clear();
    this.currentRoad = this.startingRoadPoint.road;
    if (this.startingRoadPoint.index == this.endingRoadPoint.index)
    {
      this.nextWaypoints.Add(this.startingRoadPoint.road.points[this.startingRoadPoint.index]);
      this.nextWaypoints.Add(this.targetPos);
    }
    RoadPathfinder.GetRoadFragment(this.nextWaypoints, this.startingRoadPoint.road, this.startingRoadPoint.index, this.endingRoadPoint.index, out this.currentRoad);
    this.nextWaypoints.Add(this.targetPos);
    return this.nextWaypoints;
  }

  private List<GlobalPosition> GetNextWaypoints(List<RoadPathfinding.Node> nodeList)
  {
    this.nextWaypoints.Clear();
    if (nodeList.Count > 2)
    {
      this.nextWaypoints.AddRange((IEnumerable<GlobalPosition>) RoadPathfinder.GetWaypointsBetweenNodes(nodeList[0], nodeList[1], out this.currentRoad));
      nodeList.RemoveAt(0);
    }
    else
    {
      this.currentRoad = (Road) null;
      this.nextWaypoints.AddRange((IEnumerable<GlobalPosition>) RoadPathfinder.GetWaypointsFromEndNode(this.endingRoadPoint, nodeList));
      this.nextWaypoints.Add(this.targetPos);
      nodeList.Clear();
    }
    return this.nextWaypoints;
  }

  private void RouteDebug()
  {
    if (!PlayerSettings.debugVis || !((Object) this.unit == (Object) SceneSingleton<CameraStateManager>.i.followingUnit))
      return;
    List<RoadPathfinding.Node> nodeList = new List<RoadPathfinding.Node>((IEnumerable<RoadPathfinding.Node>) this.nodes);
    List<GlobalPosition> waypoints = new List<GlobalPosition>((IEnumerable<GlobalPosition>) this.waypoints);
    foreach (RoadPathfinding.Node node in this.nodes)
    {
      GameObject gameObject = Object.Instantiate<GameObject>(GameAssets.i.debugPoint, Datum.origin);
      gameObject.GetComponent<Renderer>().material.SetColor("_EmissionColor", Color.green);
      gameObject.transform.localScale = Vector3.one * 30f;
      gameObject.transform.localPosition = node.position.AsVector3();
      NetworkSceneSingleton<Spawner>.i.DestroyLocal(gameObject, 5f);
    }
    GlobalPosition globalPosition = this.unit.GlobalPosition();
    waypoints.Insert(0, globalPosition);
    while (nodeList.Count > 0)
      waypoints.AddRange((IEnumerable<GlobalPosition>) this.GetNextWaypoints(nodeList));
    NetworkSceneSingleton<LevelInfo>.i.VisualizeWaypoints(waypoints, this.unit);
  }

  public void SetMovingTarget(Transform target) => this.movingTarget = target;

  private bool TrySkipNodesUnderwater(Transform lineOfSightChecker)
  {
    if (this.nodes.Count < 2)
      return false;
    int count = 0;
    for (int index = 0; index < this.nodes.Count && RoadPathfinder.VisibleUnderwater(lineOfSightChecker.GlobalPosition(), this.nodes[index].position); ++index)
      count = index;
    if (count > 0)
    {
      if (PlayerSettings.debugVis && (Object) this.unit == (Object) SceneSingleton<CameraStateManager>.i.followingUnit)
      {
        Debug.Log((object) $"Removed first {count} nodes without underwater obstructions");
        for (int index = 0; index <= count; ++index)
        {
          GameObject gameObject = Object.Instantiate<GameObject>(GameAssets.i.debugPoint, Datum.origin);
          gameObject.GetComponent<Renderer>().material.SetColor("_EmissionColor", Color.red);
          gameObject.transform.localScale = Vector3.one * 40f;
          gameObject.transform.localPosition = this.nodes[index].position.AsVector3();
          NetworkSceneSingleton<Spawner>.i.DestroyLocal(gameObject, 5f);
        }
      }
      this.nodes.RemoveRange(0, count);
    }
    return count > 0;
  }

  private void TrySkipToWaypointVisibleUnderwater(Transform keel, float margin)
  {
    if (this.waypoints.Count < 3 || (double) Time.timeSinceLevelLoad - (double) this.lastSkipCheck < 5.0)
      return;
    this.lastSkipCheck = Time.timeSinceLevelLoad;
    if (!NetworkSceneSingleton<LevelInfo>.i.seaLanes.TryGetNearestPoint(keel.GlobalPosition(), out GlobalPosition _) || !RoadPathfinder.VisibleUnderwater(keel.GlobalPosition(), this.waypoints[2]))
      return;
    this.waypoints.RemoveAt(0);
  }

  private void SkipWaypointIfBehind(GlobalPosition position)
  {
    if (this.waypoints.Count < 2)
    {
      if (this.waypoints.Count <= 0 || this.nodes.Count <= 0 || (double) Vector3.Dot(FastMath.Direction(position, this.waypoints[0]), FastMath.Direction(position, this.nodes[0].position)) >= 0.0)
        return;
      this.waypoints[0] = Vector3.Lerp(position.ToLocalPosition(), this.nodes[0].position.ToLocalPosition(), 0.5f).ToGlobalPosition();
    }
    else
    {
      if ((double) Vector3.Dot(FastMath.Direction(this.waypoints[0], this.waypoints[1]), FastMath.Direction(position, this.waypoints[0])) >= 0.0)
        return;
      this.waypoints.RemoveAt(0);
    }
  }

  public SteeringInfo? GetSteerpoint(
    GlobalPosition position,
    Vector3 forward,
    float speed,
    bool stayOnRoad)
  {
    if ((Object) this.movingTarget != (Object) null)
      return new SteeringInfo?(new SteeringInfo(FastMath.Direction(position, this.movingTarget.position.ToGlobalPosition()), 90f));
    if (this.waypoints.Count < 2 && this.nodes.Count > 0)
      this.waypoints.AddRange((IEnumerable<GlobalPosition>) this.GetNextWaypoints(this.nodes));
    if (this.waypoints.Count == 0)
    {
      this.currentRoad = (Road) null;
      return SteeringInfo.None;
    }
    GlobalPosition waypoint1 = this.waypoints[0];
    int b1 = 20;
    GlobalPosition to1 = waypoint1;
    bool flag;
    float nextWaypointAngle;
    if (this.waypoints.Count > 1 && this.waypoints[1].NotEqual(waypoint1))
    {
      GlobalPosition waypoint2 = this.waypoints[1];
      Vector3 to2 = FastMath.NormalizedDirection(waypoint1, waypoint2);
      GlobalPosition b2 = to1 + to2 * (float) b1;
      float num = FastMath.Distance(position, b2);
      to1 = b2 - to2 * Mathf.Min(num * 0.5f, (float) b1);
      flag = (double) num < (double) (b1 + 10) && (double) Vector3.Dot(FastMath.Direction(position, waypoint1), forward) < 0.0;
      nextWaypointAngle = (double) speed * (double) speed < (double) num ? 0.0f : Vector3.Angle(forward, to2);
    }
    else
    {
      float num = FastMath.Distance(position, waypoint1);
      flag = (double) num < (double) (b1 + 10) && (double) Vector3.Dot(FastMath.Direction(position, waypoint1), forward) < 0.0;
      nextWaypointAngle = (double) speed > (double) num * 0.30000001192092896 ? 90f : 0.0f;
    }
    if (flag)
      this.waypoints.RemoveAt(0);
    if ((double) Time.timeSinceLevelLoad - (double) this.lastWaterCheck > 4.0 && this.waypoints.Count > 0)
    {
      this.lastWaterCheck = Time.timeSinceLevelLoad;
      Vector3 position1 = this.unit.transform.position;
      Vector3 zero = Vector3.zero;
      RaycastHit hit1;
      RaycastHit hit2;
      if ((!PathfindingAgent.RaycastTerrain(!FastMath.InRange(this.unit.GlobalPosition(), this.waypoints[0], 100f) ? this.unit.transform.position + FastMath.NormalizedDirection(this.unit.GlobalPosition(), this.waypoints[0]) * 100f : this.waypoints[0].ToLocalPosition(), out hit1) || (double) hit1.point.y < (double) Datum.LocalSeaY) && PathfindingAgent.RaycastTerrain(position1, out hit2) && (double) hit2.point.y > (double) Datum.LocalSeaY)
      {
        this.waypoints.Clear();
        this.nodes.Clear();
      }
    }
    return position == to1 ? SteeringInfo.None : new SteeringInfo?(new SteeringInfo(FastMath.Direction(position, to1), nextWaypointAngle));
  }

  public SteeringInfo? GetShipSteerpoint(
    GlobalPosition position,
    Vector3 forward,
    float speed,
    bool stayOnRoad,
    Transform keel)
  {
    float num1 = (float) (((double) this.unit.maxRadius + 50.0) * 5.0);
    this.TrySkipToWaypointVisibleUnderwater(keel, this.unit.maxRadius * 3f);
    if (this.waypoints.Count < 2 && this.nodes.Count > 0)
      this.waypoints.AddRange((IEnumerable<GlobalPosition>) this.GetNextWaypoints(this.nodes));
    if (this.waypoints.Count == 0)
      return SteeringInfo.None;
    GlobalPosition waypoint1 = this.waypoints[0];
    float num2 = FastMath.Distance(waypoint1, position);
    float nextWaypointAngle = 0.0f;
    GlobalPosition to = waypoint1;
    bool flag = false;
    if (this.waypoints.Count > 1)
    {
      if ((double) num2 < (double) num1 * 2.0)
      {
        GlobalPosition waypoint2 = this.waypoints[1];
        float num3 = Mathf.Clamp01((num1 - num2) / num1);
        if ((double) num3 > 0.0)
          to = waypoint1 + num3 * num1 * FastMath.NormalizedDirection(waypoint1, waypoint2);
        flag = (double) Vector3.Dot(waypoint1 - position, forward) < 0.0;
      }
    }
    else
      flag = (double) num2 < (double) num1 && (double) Vector3.Dot(FastMath.Direction(position, waypoint1), forward) < 0.0;
    if (flag)
      this.waypoints.RemoveAt(0);
    return new SteeringInfo?(new SteeringInfo(FastMath.Direction(position, to), nextWaypointAngle));
  }

  public static bool RaycastTerrain(GlobalPosition position, out RaycastHit hit)
  {
    return PathfindingAgent.RaycastTerrain(position.ToLocalPosition(), out hit);
  }

  public static bool RaycastTerrain(Vector3 position, out RaycastHit hit)
  {
    return Physics.Raycast(position + Vector3.up * 10000f, Vector3.down * 20000f, out hit, 20000f, 64 /*0x40*/);
  }
}
