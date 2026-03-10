// Decompiled with JetBrains decompiler
// Type: NuclearOption.NetworkTransforms.SendTransformBatcherDebugger
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.DebugScripts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

#nullable disable
namespace NuclearOption.NetworkTransforms;

public class SendTransformBatcherDebugger : MonoBehaviour
{
  private SendTransformBatcherDebugger.DebugUnit followingUnit;
  private readonly List<SendTransformBatcherDebugger.DebugUnit> drawList = new List<SendTransformBatcherDebugger.DebugUnit>();
  private readonly List<SendTransformBatcherDebugger.DebugUnit> tempList = new List<SendTransformBatcherDebugger.DebugUnit>();
  private readonly List<Unit> tmpUnits = new List<Unit>();

  private SendTransformBatcherDebugger.DebugUnit FindOrCreate(NetworkTransformBase networkTransform)
  {
    foreach (SendTransformBatcherDebugger.DebugUnit draw in this.drawList)
    {
      if ((UnityEngine.Object) draw.NetTransform == (UnityEngine.Object) networkTransform)
        return draw;
    }
    SendTransformBatcherDebugger.DebugUnit orCreate = new SendTransformBatcherDebugger.DebugUnit(networkTransform);
    this.drawList.Add(orCreate);
    return orCreate;
  }

  public void UpdateDebugFollow(ref VisualUpdateTime visualTime)
  {
    if (!this.enabled)
      this.enabled = true;
    NetworkTransformBase networkTransform = (NetworkTransformBase) null;
    NetworkTransformBase component;
    if ((UnityEngine.Object) SceneSingleton<CameraStateManager>.i.followingUnit != (UnityEngine.Object) null && !SceneSingleton<CameraStateManager>.i.followingUnit.HasAuthority && SceneSingleton<CameraStateManager>.i.followingUnit.TryGetComponent<NetworkTransformBase>(out component))
      networkTransform = component;
    if ((UnityEngine.Object) this.followingUnit?.NetTransform != (UnityEngine.Object) networkTransform)
    {
      if (this.followingUnit != null && (UnityEngine.Object) this.followingUnit.NetTransform != (UnityEngine.Object) null && !this.followingUnit.displaySnapshot.AnyShowing())
        this.followingUnit.NetTransform.debugActive = false;
      if ((UnityEngine.Object) networkTransform != (UnityEngine.Object) null)
      {
        SendTransformBatcherDebugger.DebugUnit orCreate = this.FindOrCreate(networkTransform);
        networkTransform.debugActive = true;
        this.followingUnit = orCreate;
      }
      else
        this.followingUnit = (SendTransformBatcherDebugger.DebugUnit) null;
    }
    this.CheckInput();
    this.UpdateDebug(ref visualTime);
  }

  private void CheckInput()
  {
    if (Input.GetKeyDown(KeyCode.Alpha5))
      ToggleGroup((SendTransformBatcherDebugger.GetField<bool>) (u => ref u.Boxes));
    if (Input.GetKeyDown(KeyCode.Alpha6))
      ToggleGroup((SendTransformBatcherDebugger.GetField<bool>) (u => ref u.Extrapolate));
    if (Input.GetKeyDown(KeyCode.Alpha7))
      ToggleGroup((SendTransformBatcherDebugger.GetField<bool>) (u => ref u.InterpolationMarker));
    if (!Input.GetKeyDown(KeyCode.Alpha8))
      return;
    ToggleGroup((SendTransformBatcherDebugger.GetField<bool>) (u => ref u.LinePath));

    void UpdateTempList()
    {
      this.tempList.Clear();
      if (Input.GetKey(KeyCode.LeftShift))
      {
        BattlefieldGrid.GetUnitsInRangeNonAlloc(SceneSingleton<CameraStateManager>.i.transform.GlobalPosition(), 500f, this.tmpUnits);
        foreach (Unit tmpUnit in this.tmpUnits)
        {
          NetworkTransformBase component;
          if (!tmpUnit.HasAuthority && tmpUnit.TryGetComponent<NetworkTransformBase>(out component))
            this.tempList.Add(this.FindOrCreate(component));
        }
      }
      else
      {
        if (this.followingUnit == null)
          return;
        this.tempList.Add(this.followingUnit);
      }
    }

    void ToggleGroup(
      SendTransformBatcherDebugger.GetField<bool> getField)
    {
      UpdateTempList();
      if (this.tempList.Count == 0)
        return;
      // ISSUE: explicit reference operation
      bool? nullable = new bool?(^getField(this.tempList[0].displaySnapshot));
      for (int index = 1; index < this.tempList.Count; ++index)
      {
        // ISSUE: explicit reference operation
        if (^getField(this.tempList[index].displaySnapshot) != nullable.Value)
        {
          nullable = new bool?();
          break;
        }
      }
      bool flag = !nullable.HasValue || !nullable.Value;
      foreach (SendTransformBatcherDebugger.DebugUnit temp in this.tempList)
      {
        // ISSUE: explicit reference operation
        ^getField(temp.displaySnapshot) = flag;
        if (flag && !temp.NetTransform.debugActive)
          temp.NetTransform.debugActive = true;
      }
    }
  }

  private void UpdateDebug(ref VisualUpdateTime visualTime)
  {
    for (int index = this.drawList.Count - 1; index >= 0; --index)
    {
      if ((UnityEngine.Object) this.drawList[index].NetTransform == (UnityEngine.Object) null)
        this.drawList.RemoveAt(index);
    }
    foreach (SendTransformBatcherDebugger.DebugUnit draw in this.drawList)
    {
      if (draw.NetTransform is ShipNetworkTransform)
        break;
      if (draw.Active)
      {
        float num = draw.NetTransform.SyncSettings.Interval * 2.5f;
        double snapshotTime = visualTime.interpolationTime - (double) num;
        double extrapolationTime = visualTime.interpolationTime + visualTime.extrapolationOffset * (double) draw.NetTransform.extrapolationFactor;
        draw.displaySnapshot.VisualUpdate(snapshotTime, extrapolationTime, visualTime.maxExtrapolateAge);
      }
    }
  }

  private void OnGUI()
  {
    if (!DebugVis.Enabled)
    {
      this.drawList.Clear();
      this.enabled = false;
    }
    else
    {
      if (this.drawList.Count == 0 || this.drawList.All<SendTransformBatcherDebugger.DebugUnit>((Func<SendTransformBatcherDebugger.DebugUnit, bool>) (x => !x.Active)))
        return;
      SendTransformBatcherDebugger.DisplaySnapshot displaySnapshot = this.followingUnit?.displaySnapshot;
      GUILayout.Space(30f);
      GUILayout.Label($"(5) Boxes: {(ValueType) (bool) (displaySnapshot != null ? (displaySnapshot.Boxes ? 1 : 0) : 0)}");
      GUILayout.Label($"(6) Extrapolation: {(ValueType) (bool) (displaySnapshot != null ? (displaySnapshot.Extrapolate ? 1 : 0) : 0)}");
      GUILayout.Label($"(7) Interpolation Marker: {(ValueType) (bool) (displaySnapshot != null ? (displaySnapshot.InterpolationMarker ? 1 : 0) : 0)}");
      GUILayout.Label($"(8) Extrapolate Line: {(ValueType) (bool) (displaySnapshot != null ? (displaySnapshot.LinePath ? 1 : 0) : 0)}");
      GUILayout.Label("(shift) Enable All nearby");
      SendTransformBatcherDebugger.Gui? debugGui = this.followingUnit?.NetTransform.debugGui;
      if (this.followingUnit != null)
      {
        GUILayout.Space(30f);
        GUILayout.Label("Type: " + debugGui?.snapType);
        GUILayout.Label("Pos: " + ToBars(debugGui.HasValue ? debugGui.GetValueOrDefault().influence_position : 0.0f));
        GUILayout.Label("Vel: " + ToBars(debugGui.HasValue ? debugGui.GetValueOrDefault().influence_velocity : 0.0f));
        GUILayout.Label("Acc: " + ToBars(debugGui.HasValue ? debugGui.GetValueOrDefault().influence_acceleration : 0.0f));
      }
      GUILayout.Space(30f);
      foreach (SendTransformBatcherDebugger.DebugUnit draw in this.drawList)
      {
        if (!((UnityEngine.Object) draw.NetTransform == (UnityEngine.Object) null))
        {
          bool flag = GUILayout.Toggle(draw.Active, draw.NetTransform.name);
          if (flag != draw.Active)
          {
            if (flag)
            {
              draw.NetTransform.debugActive = true;
              if (this.followingUnit != null)
              {
                draw.displaySnapshot.Boxes = this.followingUnit.displaySnapshot.Boxes;
                draw.displaySnapshot.Extrapolate = this.followingUnit.displaySnapshot.Extrapolate;
                draw.displaySnapshot.InterpolationMarker = this.followingUnit.displaySnapshot.InterpolationMarker;
                draw.displaySnapshot.LinePath = this.followingUnit.displaySnapshot.LinePath;
              }
            }
            else
            {
              draw.NetTransform.debugActive = false;
              draw.displaySnapshot.Boxes = false;
              draw.displaySnapshot.Extrapolate = false;
              draw.displaySnapshot.InterpolationMarker = false;
              draw.displaySnapshot.LinePath = false;
            }
          }
        }
      }
    }

    static string ToBars(float percent)
    {
      int count = (int) ((double) Mathf.Clamp01(percent) * 20.0);
      return $"[{new string('#', count)}{new string('_', 20 - count)}]";
    }
  }

  private delegate ref T GetField<T>(SendTransformBatcherDebugger.DisplaySnapshot unit);

  private class DebugUnit
  {
    public readonly NetworkTransformBase NetTransform;
    public readonly SendTransformBatcherDebugger.DisplaySnapshot displaySnapshot;

    public bool Active => this.NetTransform.debugActive;

    public DebugUnit(NetworkTransformBase unit)
    {
      this.NetTransform = unit;
      if (this.displaySnapshot != null)
        return;
      this.displaySnapshot = new SendTransformBatcherDebugger.DisplaySnapshot(unit.SnapshotBuffer, unit.name, unit.SendBatcher.LineRendererPrefab);
    }
  }

  public struct Gui
  {
    public string snapType;
    public Vector3 rb_MovePosition;
    public Quaternion rb_MoveRotation;
    public Vector3 rb_velocity;
    public float influence_position;
    public float influence_velocity;
    public float influence_acceleration;
    public Vector3 snapshot_velocity;
    public Vector3 snapshot_acceleration;

    public void CalculateInfluence(NetworkTransformBase.ViewSnapshot snapshot, Vector3 oldPos)
    {
      this.influence_position = (snapshot.Position - oldPos - this.snapshot_velocity - this.snapshot_acceleration).magnitude;
      this.influence_velocity = this.snapshot_velocity.magnitude;
      this.influence_acceleration = this.snapshot_acceleration.magnitude;
      float num = this.influence_position + this.influence_velocity + this.influence_acceleration;
      this.influence_position /= num;
      this.influence_velocity /= num;
      this.influence_acceleration /= num;
      this.rb_MovePosition = snapshot.Position;
      this.rb_MoveRotation = snapshot.Rotation;
      this.rb_velocity = snapshot.Velocity;
    }
  }

  public static class DebugSnapshotWriter
  {
    private static StreamWriter writer;
    private static StringBuilder builder = new StringBuilder();

    public static void Debug_WriteSnapshots(
      Aircraft aircraft,
      double smoothTime,
      Action<Action<object>> writeValues)
    {
      if (SendTransformBatcherDebugger.DebugSnapshotWriter.writer == null)
      {
        SendTransformBatcherDebugger.DebugSnapshotWriter.writer = new StreamWriter($"./SmoothTransform_{DateTime.Now:HH-mm--ss}.csv")
        {
          AutoFlush = true
        };
        SendTransformBatcherDebugger.DebugSnapshotWriter.writer.WriteLine("{NetworkTime},{smoothTime},{previous.timestamp},{previous.snapshot.globalPos},{previous.snapshot.velocity},{current.timestamp},{current.snapshot.globalPos},{current.snapshot.velocity}");
      }
      SendTransformBatcherDebugger.DebugSnapshotWriter.writer.Write($"{aircraft.NetworkTime.Time},{smoothTime},");
      writeValues((Action<object>) (value => SendTransformBatcherDebugger.DebugSnapshotWriter.writer.Write($"{value},")));
      SendTransformBatcherDebugger.DebugSnapshotWriter.writer.Write("\n");
    }
  }

  private class DisplaySnapshot
  {
    private Dictionary<double, (GameObject go, Material mat)> proxies = new Dictionary<double, (GameObject, Material)>();
    private readonly string name;
    private readonly LineRenderer lineRendererPrefab;
    private readonly NetworkTransformBase.SnapshotBufferLocalSnapshot snapshotBuffer;
    public bool Boxes;
    public bool Extrapolate;
    public bool LinePath;
    public bool InterpolationMarker;
    private GameObject interpolationSphereMarker;
    private int colorIndex;
    private Stack<(GameObject go, Material mat)> pool = new Stack<(GameObject, Material)>();
    private LineRenderer lineRenderer;
    private Vector3[] positionList = new Vector3[1000];
    private LineRenderer pathRenderer;

    public bool AnyShowing() => this.Boxes || this.LinePath || this.InterpolationMarker;

    public (GameObject go, Material mat) CreateProxy(Vector3 scale, PrimitiveType primitiveType = PrimitiveType.Cube)
    {
      GameObject primitive = GameObject.CreatePrimitive(primitiveType);
      primitive.name = this.name;
      primitive.transform.localScale = scale;
      Renderer component1 = primitive.GetComponent<Renderer>();
      Material material = component1.material;
      material.color = Color.red;
      component1.material = material;
      Collider component2;
      if (primitive.TryGetComponent<Collider>(out component2))
        component2.enabled = false;
      return (primitive, material);
    }

    public DisplaySnapshot(
      NetworkTransformBase.SnapshotBufferLocalSnapshot snapshotBuffer,
      string name,
      LineRenderer lineRendererPrefab)
    {
      this.name = name;
      this.lineRendererPrefab = lineRendererPrefab;
      this.snapshotBuffer = snapshotBuffer;
    }

    public void VisualUpdate(double snapshotTime, double extrapolationTime, float maxAge)
    {
      if (this.snapshotBuffer.Count > 0)
      {
        if (this.Boxes)
          this.CreateNew(extrapolationTime, maxAge);
        if (this.LinePath && SendTransformBatcherDebugger.DisplaySnapshot.CloseToCamera(this.snapshotBuffer))
          this.DrawPathLine(snapshotTime, extrapolationTime, maxAge);
        if (this.InterpolationMarker)
        {
          if ((UnityEngine.Object) this.interpolationSphereMarker == (UnityEngine.Object) null)
          {
            (GameObject go, Material mat) proxy = this.CreateProxy(Vector3.one, PrimitiveType.Sphere);
            this.interpolationSphereMarker = proxy.go;
            proxy.mat.color = Color.green;
          }
          this.interpolationSphereMarker.transform.position = this.snapshotBuffer.GetSnapshotForTime(snapshotTime).Position;
        }
      }
      this.RemoveOld(snapshotTime);
    }

    private static bool CloseToCamera(
      NetworkTransformBase.SnapshotBufferLocalSnapshot snapshotBuffer)
    {
      Camera mainCamera = SceneSingleton<CameraStateManager>.i.mainCamera;
      Vector3 localPosition = snapshotBuffer.Get(snapshotBuffer.Count - 1).Snapshot.globalPos.ToLocalPosition();
      if (!FastMath.InRange(mainCamera.transform.position, localPosition, 1000f))
        return false;
      Vector3 viewportPoint = mainCamera.WorldToViewportPoint(localPosition);
      return (double) viewportPoint.z > -100.0 && -0.10000000149011612 <= (double) viewportPoint.x && (double) viewportPoint.x <= 1.1000000238418579 && -0.10000000149011612 <= (double) viewportPoint.y && (double) viewportPoint.y <= 1.1000000238418579;
    }

    private void DrawPathLine(double snapshotTime, double extrapolationTime, float maxAge)
    {
      double num = extrapolationTime - snapshotTime;
      int index = 0;
      double timestamp1 = this.snapshotBuffer.Get(0).Timestamp;
      double timestamp2 = this.snapshotBuffer.Get(this.snapshotBuffer.Count - 1).Timestamp;
      for (double snapshotTime1 = timestamp1; snapshotTime1 < timestamp2; snapshotTime1 += 1.0 / 1000.0)
      {
        NetworkTransformBase.ViewSnapshot snapshotForTime = this.snapshotBuffer.GetSnapshotForTime(snapshotTime1);
        NetworkTransformBase.ViewSnapshot viewSnapshot = snapshotForTime.Extrapolate(snapshotForTime.Timestamp + num, maxAge);
        if (this.positionList.Length <= index)
          Array.Resize<Vector3>(ref this.positionList, this.positionList.Length * 2);
        this.positionList[index] = viewSnapshot.Position.ToGlobalPosition().AsVector3();
        ++index;
      }
      if ((UnityEngine.Object) this.pathRenderer == (UnityEngine.Object) null)
      {
        this.pathRenderer = UnityEngine.Object.Instantiate<LineRenderer>(this.lineRendererPrefab, Datum.origin);
        this.pathRenderer.name = this.name + "_pathRenderer";
      }
      this.pathRenderer.positionCount = index;
      this.pathRenderer.SetPositions(this.positionList);
    }

    private void DrawLinesBetweenSnapshots()
    {
      Color color = this.Extrapolate ? Color.green : Color.blue;
      for (int i = 1; i < this.snapshotBuffer.Count; ++i)
      {
        SnapshotBuffer<NetworkTransformBase.LocalSnapshot>.TimedSnapshot timedSnapshot1 = this.snapshotBuffer.Get(i);
        SnapshotBuffer<NetworkTransformBase.LocalSnapshot>.TimedSnapshot timedSnapshot2 = this.snapshotBuffer.Get(i - 1);
        Debug.DrawLine(timedSnapshot1.Snapshot.globalPos.ToLocalPosition(), timedSnapshot2.Snapshot.globalPos.ToLocalPosition(), color);
      }
    }

    private void CreateNew(double extrapolationTime, float maxAge)
    {
      for (int index = 1; index < this.snapshotBuffer.Count; ++index)
      {
        SnapshotBuffer<NetworkTransformBase.LocalSnapshot>.TimedSnapshot timedSnapshot = this.snapshotBuffer.Get(index);
        if (!this.proxies.ContainsKey(timedSnapshot.Timestamp))
        {
          GameObject proxy = this.GetProxy(timedSnapshot.Timestamp, this.Extrapolate);
          if (this.Extrapolate)
          {
            NetworkTransformBase.ViewSnapshot viewSnapshot = this.snapshotBuffer.AfterCurrent(index).Extrapolate(extrapolationTime, maxAge);
            proxy.transform.SetPositionAndRotation(viewSnapshot.Position, viewSnapshot.Rotation);
          }
          else
            proxy.transform.SetPositionAndRotation(timedSnapshot.Snapshot.globalPos.ToLocalPosition(), timedSnapshot.Snapshot.rotation ?? Quaternion.identity);
        }
      }
    }

    private GameObject GetProxy(double timestamp, bool extrapolate)
    {
      (GameObject, Material) valueTuple1;
      if (this.pool.Count == 0)
      {
        valueTuple1 = this.CreateProxy(new Vector3(10f, 2f, 4f) * 0.5f);
        valueTuple1.Item1.transform.localScale *= 0.7f;
      }
      else
      {
        valueTuple1 = this.pool.Pop();
        valueTuple1.Item1.SetActive(true);
      }
      float num = (float) ((double) this.colorIndex / 10.0 * 0.800000011920929);
      ++this.colorIndex;
      if (this.colorIndex > 10)
        this.colorIndex = 0;
      (GameObject, Material) valueTuple2 = valueTuple1;
      GameObject proxy = valueTuple2.Item1;
      valueTuple2.Item2.color = !extrapolate ? new Color(num, num, 1f, 0.5f) : new Color(num, 1f, num, 0.5f);
      this.proxies.Add(timestamp, valueTuple1);
      return proxy;
    }

    private void RemoveOld(double removeTime)
    {
      Span<double> span = stackalloc double[this.proxies.Count];
      int num = 0;
      foreach (KeyValuePair<double, (GameObject go, Material mat)> proxy in this.proxies)
      {
        double key = proxy.Key;
        if (key < removeTime - 2.0)
        {
          span[num] = key;
          ++num;
        }
      }
      for (int index = 0; index < num; ++index)
      {
        double key = span[index];
        (GameObject go, Material mat) proxy = this.proxies[key];
        this.proxies.Remove(key);
        this.pool.Push(proxy);
        proxy.go.SetActive(false);
      }
    }
  }
}
