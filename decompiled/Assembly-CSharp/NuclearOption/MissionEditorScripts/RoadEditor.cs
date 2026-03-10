// Decompiled with JetBrains decompiler
// Type: NuclearOption.MissionEditorScripts.RoadEditor
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using RoadPathfinding;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

#nullable disable
namespace NuclearOption.MissionEditorScripts;

public class RoadEditor : MonoBehaviour
{
  public const string ROADS = "Roads";
  public const string SEA_LANES = "Sea Lanes";
  private RoadEditor.RoadType roadType;
  [SerializeField]
  private TMP_Dropdown networkSelector;
  [SerializeField]
  private Button placeRoadButton;
  [SerializeField]
  private Button deleteRoadButton;
  [SerializeField]
  private GameObject pointPrefab;
  [SerializeField]
  private GameObject segmentPrefab;
  [SerializeField]
  private GameObject nodeInspectorPrefab;
  [SerializeField]
  private Toggle bridgeToggle;
  private RoadNetwork roadNetwork;
  private GameObject submenu;
  private GameObject placingSegment;
  private Road selectedRoad;
  private Road constructingRoad;
  [SerializeField]
  private Material visMat;
  [SerializeField]
  private Material visMatBridge;
  private Material visMatSelected;
  private Material visMatNonEditable;
  private Material nodeVisMat;
  private float lastPointsVisTime;
  private float visWidth;
  private readonly List<string> networkSelectorKeys = new List<string>();
  private readonly Dictionary<Road, List<GameObject>> pointVis = new Dictionary<Road, List<GameObject>>();
  private bool hasSelectedNetwork;
  private RoadEditor.Pool pointPrefabPool;
  private RoadEditor.Pool segmentPrefabPool;

  private void Awake()
  {
    this.pointPrefabPool = new RoadEditor.Pool(this.pointPrefab);
    this.segmentPrefabPool = new RoadEditor.Pool(this.segmentPrefab);
    this.networkSelector.onValueChanged.AddListener(new UnityAction<int>(this.SelectNetwork));
    this.placeRoadButton.onClick.AddListener(new UnityAction(this.EnterPlacingMode));
    this.deleteRoadButton.onClick.AddListener(new UnityAction(this.DeleteSelected));
    this.CreateSelectorList();
    this.visMat.color = new Color(0.0f, 1f, 0.0f, 0.5f);
    this.visMatSelected = new Material(this.visMat);
    this.visMatSelected.color = new Color(1f, 1f, 1f, 0.5f);
    this.visMatNonEditable = new Material(this.visMat);
    this.visMatNonEditable.color = new Color(1f, 1f, 1f, 0.2f);
    this.nodeVisMat = new Material(this.visMat);
    this.nodeVisMat.color = new Color(0.25f, 0.25f, 1f, 0.7f);
    this.lastPointsVisTime = Time.realtimeSinceStartup;
    UnitSelection.DisallowSelection((object) this, true);
  }

  private void OnDestroy()
  {
    UnitSelection.DisallowSelection((object) this, false);
    this.pointPrefabPool?.Dispose();
    this.segmentPrefabPool?.Dispose();
    this.ClearPointVis();
  }

  private bool CanEditRoad(Road road) => road.IsEditable() || Application.isEditor;

  private void CreateSelectorList()
  {
    this.networkSelector.options.Clear();
    AddOption("Roads");
    AddOption("Sea Lanes");
    foreach (Airbase airbase in (IEnumerable<Airbase>) FactionRegistry.airbaseLookup.Values.OrderByDescending<Airbase, bool>((Func<Airbase, bool>) (x => x.IsCustom)))
    {
      if (!airbase.AttachedAirbase)
        AddOption(airbase.SavedAirbase.UniqueName, airbase.SavedAirbase.ToUIString(true));
    }

    void AddOption(string key, string display = null)
    {
      this.networkSelectorKeys.Add(key);
      this.networkSelector.options.Add(new TMP_Dropdown.OptionData(display ?? key));
    }
  }

  private void Start()
  {
    if (this.hasSelectedNetwork)
      return;
    this.SelectNetwork("Roads");
  }

  public void EnterPlacingMode() => this.placeRoadButton.interactable = false;

  public void DeleteSelected()
  {
    if (this.selectedRoad == null || !this.CanEditRoad(this.selectedRoad))
      return;
    this.roadNetwork.roads.Remove(this.selectedRoad);
    if (Application.isEditor)
      NetworkSceneSingleton<LevelInfo>.i.roadNetwork.roads.Remove(this.selectedRoad);
    if (this.pointVis.ContainsKey(this.selectedRoad))
    {
      foreach (UnityEngine.Object @object in this.pointVis[this.selectedRoad])
        UnityEngine.Object.Destroy(@object);
      this.pointVis.Remove(this.selectedRoad);
    }
    this.selectedRoad = (Road) null;
    this.SelectNetwork();
  }

  public void SetBridge()
  {
    if (this.selectedRoad == null || !this.CanEditRoad(this.selectedRoad))
      return;
    this.selectedRoad.SetBridge(this.bridgeToggle.isOn);
    this.SelectNetwork();
  }

  public void SelectNetwork(Airbase airbase, bool focus)
  {
    this.SelectNetwork(airbase.SavedAirbase.UniqueName, focus);
  }

  private void SelectNetwork(int index) => this.SelectNetwork(this.networkSelectorKeys[index]);

  public void SelectNetwork()
  {
    this.SelectNetwork(this.networkSelectorKeys[this.networkSelector.value], false);
  }

  public void SelectNetwork(string networkName, bool focusAirbase = true)
  {
    this.hasSelectedNetwork = true;
    int input = this.networkSelectorKeys.IndexOf(networkName);
    if (input != -1)
      this.networkSelector.SetValueWithoutNotify(input);
    Airbase airbase;
    bool flag;
    if (FactionRegistry.airbaseLookup.TryGetValue(networkName, out airbase))
    {
      if (focusAirbase)
        SceneSingleton<CameraStateManager>.i.FocusAirbase(airbase, true, 200f, 40f);
      this.visWidth = 10f;
      this.visMat.color = new Color(0.0f, 1f, 0.0f, 0.5f);
      this.roadNetwork = airbase.GetTaxiNetwork();
      this.roadType = RoadEditor.RoadType.airbase;
      flag = airbase.IsCustom;
    }
    else
    {
      switch (networkName)
      {
        case "Roads":
          flag = true;
          this.visWidth = 10f;
          this.visMat.color = new Color(0.0f, 1f, 0.0f, 0.5f);
          this.roadNetwork = Application.isEditor ? NetworkSceneSingleton<LevelInfo>.i.roadNetwork : MissionManager.CurrentMission.missionSettings.missionRoads;
          this.roadType = RoadEditor.RoadType.road;
          break;
        case "Sea Lanes":
          flag = false;
          this.visWidth = 300f;
          this.visMat.color = new Color(0.0f, 1f, 1f, 0.5f);
          this.roadNetwork = NetworkSceneSingleton<LevelInfo>.i.seaLanes;
          this.roadType = RoadEditor.RoadType.seaLane;
          break;
        default:
          flag = false;
          this.visWidth = 10f;
          this.visMat.color = new Color(0.0f, 1f, 0.0f, 0.5f);
          this.visMatSelected.color = new Color(1f, 1f, 1f, 0.5f);
          this.roadNetwork = NetworkSceneSingleton<LevelInfo>.i.roadNetwork;
          this.roadType = RoadEditor.RoadType.road;
          break;
      }
    }
    this.deleteRoadButton.interactable = false;
    this.bridgeToggle.gameObject.SetActive(false);
    this.placeRoadButton.interactable = flag;
    this.VisualizeNetworks();
    this.ClearPointVis();
  }

  private void PlacePoint(GlobalPosition position)
  {
    if (this.constructingRoad == null)
      this.constructingRoad = new Road();
    this.placingSegment = this.segmentPrefabPool.Get(Datum.origin);
    this.placingSegment.GetComponent<Renderer>().material = this.visMat;
    this.constructingRoad.AddPoint(position);
  }

  private RoadPathfinding.Node ClickSelect(RaycastHit click)
  {
    UnityEngine.Object.Destroy((UnityEngine.Object) this.submenu);
    RoadNodeMarker component = click.transform.GetComponent<RoadNodeMarker>();
    if (!((UnityEngine.Object) component != (UnityEngine.Object) null) || component.node == null)
      return (RoadPathfinding.Node) null;
    this.submenu = UnityEngine.Object.Instantiate<GameObject>(this.nodeInspectorPrefab, this.transform);
    this.submenu.GetComponent<RoadNodeInspector>().DisplayNodeInfo(component.node);
    return component.node;
  }

  private void Update()
  {
    if ((double) Time.realtimeSinceStartup - (double) this.lastPointsVisTime > 1.0)
    {
      this.lastPointsVisTime = Time.realtimeSinceStartup;
      this.ShowPoints();
    }
    if (Input.GetKeyDown(KeyCode.LeftShift) && this.selectedRoad != null)
    {
      this.selectedRoad = (Road) null;
      this.VisualizeNetworks();
    }
    if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
    {
      if (Input.GetKey(KeyCode.LeftShift) || !this.placeRoadButton.interactable || this.constructingRoad != null)
      {
        if ((UnityEngine.Object) this.placingSegment == (UnityEngine.Object) null || (double) this.placingSegment.transform.localScale.z >= 10.0)
        {
          GlobalPosition position = SceneSingleton<UnitSelection>.i.placementTransform.GlobalPosition();
          RaycastHit hitInfo;
          if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo, 100000f))
          {
            RoadPathfinding.Node node = this.ClickSelect(hitInfo);
            if (node != null)
              position = node.position;
          }
          this.PlacePoint(position);
        }
      }
      else
      {
        this.selectedRoad = this.SelectRoad();
        this.VisualizeNetworks();
      }
    }
    if (Input.GetMouseButtonDown(1))
    {
      this.placeRoadButton.interactable = true;
      if (this.selectedRoad != null)
      {
        this.selectedRoad = (Road) null;
        this.deleteRoadButton.interactable = false;
        this.bridgeToggle.gameObject.SetActive(false);
        this.VisualizeNetworks();
      }
    }
    if (this.selectedRoad != null)
    {
      this.constructingRoad = (Road) null;
      if (Input.GetKeyDown(KeyCode.Delete))
        this.DeleteSelected();
    }
    if (this.constructingRoad == null)
      return;
    this.selectedRoad = (Road) null;
    if ((UnityEngine.Object) this.placingSegment != (UnityEngine.Object) null)
    {
      Transform transform = this.placingSegment.transform;
      List<GlobalPosition> points = this.constructingRoad.points;
      Vector3 vector3 = points[points.Count - 1].AsVector3();
      transform.localPosition = vector3;
      this.placingSegment.transform.LookAt(SceneSingleton<UnitSelection>.i.placementTransform);
      this.placingSegment.transform.localScale = new Vector3(this.visWidth, this.visWidth * 0.1f, Vector3.Distance(this.placingSegment.transform.position, SceneSingleton<UnitSelection>.i.placementTransform.position));
    }
    if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
    {
      this.placeRoadButton.interactable = true;
      if (this.constructingRoad.points.Count > 1)
      {
        double num = (double) this.constructingRoad.CalcLength();
        this.constructingRoad.SetEditable(true);
        this.roadNetwork.roads.Add(this.constructingRoad);
        this.constructingRoad.GenerateNodes(this.roadNetwork);
        this.constructingRoad.CheckIntersection(this.roadNetwork);
        this.selectedRoad = this.constructingRoad;
        this.constructingRoad = (Road) null;
      }
      this.placingSegment = (GameObject) null;
      this.VisualizeNetworks();
    }
    if (!Input.GetKeyDown(KeyCode.Escape))
      return;
    this.placeRoadButton.interactable = true;
    this.constructingRoad = (Road) null;
    this.placingSegment = (GameObject) null;
    this.VisualizeNetworks();
  }

  public void ClearVis()
  {
    this.pointPrefabPool.ReturnAll();
    this.segmentPrefabPool.ReturnAll();
  }

  public void VisualizeNetworks()
  {
    this.ClearVis();
    if (this.roadType == RoadEditor.RoadType.road && this.roadNetwork != NetworkSceneSingleton<LevelInfo>.i.roadNetwork)
      this.VisualizeNetwork(NetworkSceneSingleton<LevelInfo>.i.roadNetwork, Application.isEditor);
    this.VisualizeNetwork(this.roadNetwork, true);
    Physics.SyncTransforms();
  }

  private void VisualizeNetwork(RoadNetwork network, bool editable)
  {
    Debug.Log((object) $"Visualizing {network.roads.Count} roads in editable network");
    for (int index = 0; index < network.roads.Count; ++index)
    {
      network.roads[index].SetEditable(editable);
      this.VisualizeRoad(network.roads[index]);
    }
    this.VisualizeNodes(network);
  }

  public void VisualizeRoad(Road road)
  {
    for (int index = 0; index < road.points.Count - 1; ++index)
    {
      GameObject gameObject = this.segmentPrefabPool.Get(Datum.origin);
      gameObject.transform.localPosition = road.points[index].AsVector3();
      Vector3 forward = road.points[index + 1] - road.points[index];
      gameObject.transform.rotation = forward == Vector3.zero ? Quaternion.identity : Quaternion.LookRotation(forward);
      gameObject.transform.localScale = new Vector3(this.visWidth, this.visWidth * 0.1f, FastMath.Distance(road.points[index + 1], road.points[index]));
      Renderer component = gameObject.GetComponent<Renderer>();
      component.material = road.IsEditable() ? this.visMat : this.visMatNonEditable;
      if (road.IsEditable() && road.IsBridge())
        component.material = this.visMatBridge;
      if (road == this.selectedRoad)
        component.material = this.visMatSelected;
    }
  }

  public void VisualizeNodes(RoadNetwork roadNetwork)
  {
    roadNetwork.RegenerateNetwork();
    for (int index = roadNetwork.nodes.Count - 1; index >= 0; --index)
      this.VisualizeNode(roadNetwork.nodes[index]);
  }

  private void VisualizeNode(RoadPathfinding.Node node)
  {
    GameObject gameObject = this.pointPrefabPool.Get(Datum.origin);
    gameObject.GetComponent<Renderer>().material = this.nodeVisMat;
    gameObject.GetComponent<RoadNodeMarker>().node = node;
    gameObject.transform.localPosition = node.position.AsVector3();
    gameObject.transform.localScale = Vector3.one * this.visWidth * 0.25f;
  }

  private Road SelectRoad()
  {
    Road road1 = (Road) null;
    float num1 = float.MaxValue;
    GlobalPosition globalPosition = SceneSingleton<UnitSelection>.i.placementTransform.GlobalPosition();
    if (Application.isEditor && this.networkSelector.options[this.networkSelector.value].text == "Roads")
    {
      foreach (Road road2 in NetworkSceneSingleton<LevelInfo>.i.roadNetwork.roads)
      {
        if (road2.InBounds(globalPosition))
        {
          foreach (GlobalPosition point in road2.points)
          {
            float num2 = FastMath.SquareDistance(point, globalPosition);
            if ((double) num2 < (double) num1)
            {
              road1 = road2;
              num1 = num2;
            }
          }
        }
      }
    }
    foreach (Road road3 in this.roadNetwork.roads)
    {
      if (road3.InBounds(globalPosition))
      {
        foreach (GlobalPosition point in road3.points)
        {
          float num3 = FastMath.SquareDistance(point, globalPosition);
          if ((double) num3 < (double) num1)
          {
            road1 = road3;
            num1 = num3;
          }
        }
      }
    }
    if (road1 != null && road1.IsEditable())
    {
      this.deleteRoadButton.interactable = true;
      this.bridgeToggle.gameObject.SetActive(true);
      this.bridgeToggle.SetIsOnWithoutNotify(road1.IsBridge());
    }
    return road1;
  }

  private void ShowPoints()
  {
    GlobalPosition point = SceneSingleton<UnitSelection>.i.placementTransform.GlobalPosition();
    foreach (Road road in this.roadNetwork.roads)
    {
      List<GameObject> gameObjectList1;
      bool flag1 = this.pointVis.TryGetValue(road, out gameObjectList1);
      bool flag2 = road.InBounds(point);
      if (flag2 && !flag1)
      {
        List<GameObject> gameObjectList2 = new List<GameObject>();
        for (int index = 1; index < road.points.Count - 1; ++index)
        {
          GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.pointPrefab, Datum.origin);
          gameObject.GetComponent<Renderer>().material = this.nodeVisMat;
          gameObject.GetComponent<RoadNodeMarker>().node = (RoadPathfinding.Node) null;
          gameObject.transform.localPosition = road.points[index].AsVector3();
          gameObject.transform.localScale = Vector3.one * this.visWidth * 0.1f;
          gameObjectList2.Add(gameObject);
        }
        this.pointVis.Add(road, gameObjectList2);
      }
      else if (!flag2 & flag1)
      {
        foreach (UnityEngine.Object @object in gameObjectList1)
          UnityEngine.Object.Destroy(@object);
        this.pointVis.Remove(road);
      }
    }
  }

  private void ClearPointVis()
  {
    foreach (List<GameObject> gameObjectList in this.pointVis.Values)
    {
      foreach (GameObject gameObject in gameObjectList)
      {
        if ((UnityEngine.Object) gameObject != (UnityEngine.Object) null)
          UnityEngine.Object.Destroy((UnityEngine.Object) gameObject);
      }
      gameObjectList.Clear();
    }
    this.pointVis.Clear();
  }

  private enum RoadType
  {
    road,
    seaLane,
    airbase,
  }

  private class Pool
  {
    private readonly GameObject prefab;
    private Stack<GameObject> pool = new Stack<GameObject>();
    private List<GameObject> outOfPool = new List<GameObject>();

    public Pool(GameObject prefab) => this.prefab = prefab;

    public GameObject Get(Transform parent)
    {
      GameObject gameObject;
      if (this.pool.TryPop(ref gameObject))
      {
        gameObject.transform.parent = parent;
        gameObject.gameObject.SetActive(true);
      }
      else
        gameObject = UnityEngine.Object.Instantiate<GameObject>(this.prefab, parent);
      this.outOfPool.Add(gameObject);
      return gameObject;
    }

    public void ReturnAll()
    {
      foreach (GameObject gameObject in this.outOfPool)
      {
        gameObject.gameObject.SetActive(false);
        this.pool.Push(gameObject);
      }
      this.outOfPool.Clear();
    }

    public void Dispose()
    {
      foreach (UnityEngine.Object @object in this.outOfPool)
        UnityEngine.Object.Destroy(@object);
      this.outOfPool.Clear();
      foreach (UnityEngine.Object @object in this.pool)
        UnityEngine.Object.Destroy(@object);
      this.pool.Clear();
    }
  }
}
