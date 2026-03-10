// Decompiled with JetBrains decompiler
// Type: BurstDatum
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System.Runtime.CompilerServices;
using UnityEngine;

#nullable disable
public struct BurstDatum
{
  public Vector3 datumPosition;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Vector3 ToGlobalPosition(Vector3 position) => position - this.datumPosition;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Vector3 ToLocalPosition(Vector3 position) => position + this.datumPosition;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public float GlobalX(Vector3 position) => position.x - this.datumPosition.x;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public float GlobalY(Vector3 position) => position.y - this.datumPosition.y;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public float GlobalZ(Vector3 position) => position.z - this.datumPosition.z;
}
