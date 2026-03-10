// Decompiled with JetBrains decompiler
// Type: TerrainScatter
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Cysharp.Threading.Tasks;
using Mirage.Logging;
using Mirage.SocketLayer;
using System;
using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;

#nullable disable
public class TerrainScatter : MonoBehaviour
{
  private static readonly ILogger matrixPoolLogger = LogFactory.GetLogger("TerrainScatter.MatrixPoolLogger");
  private static readonly ProfilerMarker GenerateScattersDensity = new ProfilerMarker(nameof (GenerateScattersDensity));
  private static readonly ProfilerMarker GenerateScattersLineCast = new ProfilerMarker(nameof (GenerateScattersLineCast));
  private static readonly ProfilerMarker GenerateScattersCreatePoint = new ProfilerMarker(nameof (GenerateScattersCreatePoint));
  private static readonly ProfilerMarker scattersRegenMarker = new ProfilerMarker("TerrainScatter.ScattersRegen");
  public static TerrainScatter i;
  [SerializeField]
  private MapSettings mapSettings;
  [SerializeField]
  private float globalDensity;
  private Vector2Int sectorCount;
  private Vector2 terrainSize;
  [SerializeField]
  private float sectorSize;
  [SerializeField]
  private GameObject sectorVis;
  [SerializeField]
  public ScatterType[] scatterTypes;
  [SerializeField]
  private Transform datum;
  [SerializeField]
  public TextAsset SavedData;
  private List<Matrix4x4> buildBatch = new List<Matrix4x4>(2000);
  private int totalScatters;
  private const int MAX_BATCH = 1023 /*0x03FF*/;
  private Pool<Matrix4x4[]> arrayPool = new Pool<Matrix4x4[]>((Pool<Matrix4x4[]>.CreateNewItemNoCount) (_ => new Matrix4x4[1023 /*0x03FF*/]), 5, 100, TerrainScatter.matrixPoolLogger);
  public Vector3 previousCameraPosition;
  public Vector3 previousCameraForward;

  private void Awake()
  {
    this.Setup();
    if (!GameManager.ShowEffects)
      return;
    this.LoadScatter();
    SlowUpdateExtensions.Loop(new Action(this.ScattersRegen), 500, true, this.destroyCancellationToken).Forget();
  }

  private void OnEnable() => TerrainScatter.i = this;

  private void OnDestroy()
  {
    if (!((UnityEngine.Object) TerrainScatter.i == (UnityEngine.Object) this))
      return;
    TerrainScatter.i = (TerrainScatter) null;
  }

  public void Setup()
  {
    this.terrainSize = this.mapSettings.MapSize;
    this.sectorCount.x = (int) ((double) this.mapSettings.MapSize.x / (double) this.sectorSize);
    this.sectorCount.y = (int) ((double) this.mapSettings.MapSize.y / (double) this.sectorSize);
  }

  private void LoadScatter()
  {
    if ((UnityEngine.Object) this.SavedData != (UnityEngine.Object) null)
    {
      using (BenchmarkScope.Create("GenerateScatters LoadFromBinary"))
        ScatterSaveLoad.Load(this.SavedData, this.scatterTypes);
    }
    else
      this.GenerateScatters();
  }

  public void GenerateScatters()
  {
    using (BenchmarkScope.Create(nameof (GenerateScatters)))
    {
      this.Setup();
      UnityEngine.Random.InitState(42);
      this.totalScatters = 0;
      foreach (ScatterType scatterType in this.scatterTypes)
        this.GenerateScatter(scatterType);
    }
  }

  private void GenerateScatter(ScatterType scatter)
  {
    Vector3 position = this.datum.position;
    Vector3 zero = Vector3.zero;
    (byte[] redPixel, int width, int height) = TerrainScatter.GetRedPixels(scatter.densityMap);
    for (int key = 0; key < this.sectorCount.x * this.sectorCount.y; ++key)
    {
      int num1 = key % this.sectorCount.x;
      int num2 = Mathf.FloorToInt((float) (key / this.sectorCount.x));
      List<ScatterPoint> scatterPointList = new List<ScatterPoint>();
      scatter.sectors.Add(key, scatterPointList);
      for (int index1 = 0; (double) index1 < (double) scatter.maxDensity * (double) this.globalDensity; ++index1)
      {
        Vector3 vector3_1 = new Vector3((float) num1 * this.sectorSize, 400f, (float) num2 * this.sectorSize) + new Vector3(this.sectorSize, 0.0f, this.sectorSize) * 0.5f - new Vector3(this.terrainSize.x, 0.0f, this.terrainSize.y) * 0.5f + new Vector3(UnityEngine.Random.Range((float) (-(double) this.sectorSize * 0.5), this.sectorSize * 0.5f), 0.0f, UnityEngine.Random.Range((float) (-(double) this.sectorSize * 0.5), this.sectorSize * 0.5f));
        zero.x = (float) width * (vector3_1.x + this.terrainSize.x * 0.5f) / this.terrainSize.x;
        zero.z = (float) height * (vector3_1.z + this.terrainSize.y * 0.5f) / this.terrainSize.y;
        int index2 = ((int) zero.x + width) % width + ((int) zero.z + height) % height * width;
        if ((double) redPixel[index2] > (double) UnityEngine.Random.value * (double) byte.MaxValue)
        {
          Vector3 vector3_2 = vector3_1 + Vector3.up * 3000f;
          int num3 = 0;
          for (int index3 = 0; index3 < scatter.samplePoints; ++index3)
          {
            Vector3 vector3_3 = vector3_1;
            if (scatter.samplePoints > 0)
            {
              float f = 6.28318548f * (float) index3 / (float) scatter.samplePoints;
              vector3_3 += (Vector3.right * Mathf.Sin(f) + Vector3.forward * Mathf.Cos(f)) * scatter.sampleRadius;
            }
            RaycastHit hitInfo;
            if (Physics.Linecast(vector3_3 + position, vector3_3 + position - Vector3.up * 5000f, out hitInfo, 64 /*0x40*/) && (double) hitInfo.point.y > (double) position.y && (UnityEngine.Object) hitInfo.collider.sharedMaterial == (UnityEngine.Object) GameAssets.i.terrainMaterial)
            {
              vector3_2.y = Mathf.Min(vector3_2.y, hitInfo.point.y - position.y);
              ++num3;
            }
          }
          if (num3 == scatter.samplePoints)
          {
            float scale = UnityEngine.Random.Range(scatter.minScale, scatter.maxScale);
            Vector3 globalPos = vector3_2 - UnityEngine.Random.value * scatter.heightVariation * Vector3.up;
            scatterPointList.Add(new ScatterPoint(globalPos, Quaternion.identity, scale));
            ++this.totalScatters;
          }
        }
      }
    }
  }

  private static (byte[] redPixel, int width, int height) GetRedPixels(Texture2D map)
  {
    Color32[] pixels32 = map.GetPixels32();
    byte[] numArray = new byte[pixels32.Length];
    for (int index = 0; index < pixels32.Length; ++index)
      numArray[index] = pixels32[index].r;
    return (numArray, map.width, map.height);
  }

  public void ClearScatters(GlobalPosition worldGlobalPosition, float radius)
  {
    if (GameManager.IsHeadless)
      return;
    Vector3 a = worldGlobalPosition.AsVector3();
    int num1 = (int) (((double) a.x + (double) this.sectorCount.x * (double) this.sectorSize * 0.5) * (1.0 / 1000.0));
    int num2 = (int) (((double) a.z + (double) this.sectorCount.y * (double) this.sectorSize * 0.5) * (1.0 / 1000.0));
    for (int index1 = num1 - 1; index1 <= num1 + 1; ++index1)
    {
      for (int index2 = num2 - 1; index2 <= num2 + 1; ++index2)
      {
        if (index1 >= 0 && index1 < this.sectorCount.x && index2 >= 0 && index2 < this.sectorCount.y)
        {
          for (int index3 = 0; index3 < this.scatterTypes.Length; ++index3)
          {
            List<ScatterPoint> sector = this.scatterTypes[index3].sectors[index2 * this.sectorCount.y + index1];
            for (int index4 = sector.Count - 1; index4 >= 0; --index4)
            {
              if (FastMath.InRange(a, sector[index4].globalPos, radius))
                sector.RemoveAt(index4);
            }
          }
        }
      }
    }
  }

  private void ScattersRegen() => this.ScattersRegen(false);

  public void ScattersRegen(bool force)
  {
    if (GameManager.IsHeadless)
      return;
    using (TerrainScatter.scattersRegenMarker.Auto())
    {
      if ((UnityEngine.Object) SceneSingleton<CameraStateManager>.i == (UnityEngine.Object) null)
        return;
      Vector3 position = SceneSingleton<CameraStateManager>.i.transform.position;
      Vector3 forward = SceneSingleton<CameraStateManager>.i.transform.forward;
      if (!force && ((double) FastMath.SquareDistance(this.previousCameraPosition, position) > 50.0 ? 1 : ((double) Vector3.Angle(this.previousCameraForward, forward) > 45.0 ? 1 : 0)) == 0)
        return;
      this.previousCameraPosition = position;
      this.previousCameraForward = forward;
      Vector3 vector3 = new Vector3(position.x - this.datum.position.x, 0.0f, position.z - this.datum.position.z);
      int num1 = (int) (((double) vector3.x + (double) this.sectorCount.x * (double) this.sectorSize * 0.5) / (double) this.sectorSize);
      int num2 = (int) (((double) vector3.z + (double) this.sectorCount.y * (double) this.sectorSize * 0.5) / (double) this.sectorSize);
      Vector3 originPosition = Datum.originPosition;
      for (int index1 = 0; index1 < this.scatterTypes.Length; ++index1)
      {
        ScatterType scatterType = this.scatterTypes[index1];
        this.buildBatch.Clear();
        foreach (RenderBatch renderBatch in scatterType.renderBatches)
          this.arrayPool.Put(renderBatch.Array);
        scatterType.renderBatches.Clear();
        int num3 = (int) ((double) scatterType.drawDistance / (double) this.sectorSize);
        for (int index2 = num1 - num3; index2 <= num1 + num3; ++index2)
        {
          for (int index3 = num2 - num3; index3 <= num2 + num3; ++index3)
          {
            if (index2 >= 0 && index2 < this.sectorCount.x && index3 >= 0 && index3 < this.sectorCount.y)
            {
              List<ScatterPoint> sector = scatterType.sectors[index3 * this.sectorCount.x + index2];
              Vector3 lhs = new Vector3()
              {
                x = (float) ((double) index2 * (double) this.sectorSize + (double) this.sectorSize * 0.5 - (double) this.terrainSize.x * 0.5),
                z = (float) ((double) index3 * (double) this.sectorSize + (double) this.sectorSize * 0.5 - (double) this.terrainSize.y * 0.5)
              } - vector3;
              float sqrMagnitude = lhs.sqrMagnitude;
              if ((double) sqrMagnitude < (double) this.sectorSize * (double) this.sectorSize * 2.0 || (double) Vector3.Dot(lhs, forward) > 0.0 && (double) sqrMagnitude < (double) this.sectorSize * (double) this.sectorSize * (double) scatterType.drawDistance * (double) scatterType.drawDistance)
              {
                foreach (ScatterPoint scatterPoint in sector)
                {
                  this.buildBatch.Add(Matrix4x4.TRS(scatterPoint.globalPos + originPosition, scatterPoint.rotation, scatterPoint.ScaleVector));
                  if (this.buildBatch.Count >= 1023 /*0x03FF*/)
                  {
                    Matrix4x4[] array = this.arrayPool.Take();
                    this.buildBatch.CopyTo(array);
                    scatterType.renderBatches.Add(new RenderBatch(array, 1023 /*0x03FF*/));
                    this.buildBatch.Clear();
                  }
                }
              }
            }
          }
        }
        if (this.buildBatch.Count > 0)
        {
          Matrix4x4[] array = this.arrayPool.Take();
          this.buildBatch.CopyTo(array);
          scatterType.renderBatches.Add(new RenderBatch(array, this.buildBatch.Count));
        }
      }
    }
  }

  private void LateUpdate() => this.RenderBatches();

  private void RenderBatches()
  {
    if (GameManager.IsHeadless)
      return;
    foreach (ScatterType scatterType in this.scatterTypes)
    {
      for (int index = 0; index < scatterType.renderBatches.Count; ++index)
      {
        RenderBatch renderBatch = scatterType.renderBatches[index];
        Graphics.DrawMeshInstanced(scatterType.mesh, 0, scatterType.material, renderBatch.Array, renderBatch.Count);
      }
    }
  }
}
