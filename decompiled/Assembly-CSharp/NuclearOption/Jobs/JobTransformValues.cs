// Decompiled with JetBrains decompiler
// Type: NuclearOption.Jobs.JobTransformValues
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

#nullable disable
namespace NuclearOption.Jobs;

[StructLayout(LayoutKind.Explicit, Size = 28)]
public struct JobTransformValues
{
  [FieldOffset(0)]
  public Quaternion Rotation;
  [FieldOffset(16 /*0x10*/)]
  public Vector3 Position;

  [StructLayout(LayoutKind.Explicit, Size = 28)]
  public readonly struct ReadOnly
  {
    [FieldOffset(0)]
    public readonly Quaternion Rotation;
    [FieldOffset(16 /*0x10*/)]
    public readonly Vector3 Position;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector3 Forward() => this.Rotation * Vector3.forward;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector3 Back() => this.Rotation * Vector3.back;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector3 Up() => this.Rotation * Vector3.up;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector3 Down() => this.Rotation * Vector3.down;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector3 Right() => this.Rotation * Vector3.right;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector3 Left() => this.Rotation * Vector3.left;
  }
}
