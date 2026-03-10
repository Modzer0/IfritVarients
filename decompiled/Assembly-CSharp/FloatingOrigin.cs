// Decompiled with JetBrains decompiler
// Type: FloatingOrigin
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.SceneManagement;

#nullable disable
[RequireComponent(typeof (Camera))]
public class FloatingOrigin : MonoBehaviour
{
  private static readonly ProfilerMarker originShiftMarker = new ProfilerMarker("FloatingOrigin.OriginShift");
  private static readonly ProfilerMarker moveRootsMarker = new ProfilerMarker("FloatingOrigin.OriginShift_MoveRoots");
  [SerializeField]
  private Transform globalDatum;
  [SerializeField]
  private AudioClip shiftSound;
  public float threshold = 1000f;
  public static FloatingOrigin Instance;
  private Bounds worldSimulate;
  private readonly List<GameObject> roots = new List<GameObject>();

  private void Awake()
  {
    Datum.SetOrigin(this.globalDatum);
    FloatingOrigin.Instance = this;
    this.worldSimulate.center = Datum.origin.position;
    this.worldSimulate.SetMinMax(new Vector3(this.worldSimulate.center.x - 90000f, this.worldSimulate.center.y - 90000f, this.worldSimulate.center.z - 90000f), new Vector3(this.worldSimulate.center.x + 90000f, this.worldSimulate.center.y + 90000f, this.worldSimulate.center.z + 90000f));
  }

  public void OriginShift(Vector3 cameraPosition)
  {
    if ((double) cameraPosition.sqrMagnitude < (double) this.threshold * (double) this.threshold)
      return;
    ProfilerMarker profilerMarker = FloatingOrigin.originShiftMarker;
    using (profilerMarker.Auto())
    {
      profilerMarker = FloatingOrigin.moveRootsMarker;
      using (profilerMarker.Auto())
      {
        this.roots.Clear();
        SceneManager.GetActiveScene().GetRootGameObjects(this.roots);
        foreach (GameObject root in this.roots)
          root.transform.position -= cameraPosition;
      }
      Datum.AfterOriginShift();
      TerrainScatter.i.ScattersRegen(true);
      Physics.SyncTransforms();
    }
  }

  private void RBKillBounds()
  {
    foreach (Rigidbody rigidbody in Object.FindObjectsOfType(typeof (Rigidbody)))
    {
      if (!this.worldSimulate.Contains(rigidbody.transform.position))
      {
        Debug.Log((object) (rigidbody.gameObject.name + " was outside world bounds and has been destroyed"));
        Object.Destroy((Object) rigidbody.gameObject);
      }
    }
  }
}
