// Decompiled with JetBrains decompiler
// Type: Datum
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

#nullable disable
public static class Datum
{
  public static readonly GlobalPosition SeaLevel = new GlobalPosition(0.0f, 0.0f, 0.0f);

  public static Transform origin { get; private set; }

  public static Vector3 originPosition { get; private set; }

  public static void AfterOriginShift() => Datum.originPosition = Datum.origin.position;

  public static float LocalSeaY
  {
    [MethodImpl(MethodImplOptions.AggressiveInlining)] get => Datum.originPosition.y;
  }

  public static Datum.DatumDestroyTracker SetOrigin(Transform newOrigin)
  {
    Datum.origin = !((UnityEngine.Object) newOrigin == (UnityEngine.Object) null) ? newOrigin : throw new ArgumentNullException(nameof (newOrigin));
    Datum.DatumDestroyTracker datumDestroyTracker = Datum.origin.gameObject.AddComponent<Datum.DatumDestroyTracker>();
    Datum.AfterOriginShift();
    return datumDestroyTracker;
  }

  public static void OnDatumDestroyed(Transform oldDatum)
  {
    Datum.origin = (Transform) null;
    Datum.originPosition = Vector3.zero;
  }

  public static Plane WaterPlane() => new Plane(Vector3.up, Datum.origin.position);

  public static BurstDatum GetBurstDatum()
  {
    return new BurstDatum()
    {
      datumPosition = Datum.originPosition
    };
  }

  [StructLayout(LayoutKind.Sequential, Size = 1)]
  private struct DatumLog
  {
  }

  public class DatumDestroyTracker : MonoBehaviour
  {
    private void OnDestroy() => Datum.OnDatumDestroyed(this.transform);
  }
}
