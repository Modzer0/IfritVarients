// Decompiled with JetBrains decompiler
// Type: Vector3Compressed
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

#nullable disable
public struct Vector3Compressed
{
  public CompressedFloat x;
  public CompressedFloat y;
  public CompressedFloat z;
  public static readonly Vector3Compressed zero = Vector3.zero.Compress();
  public static readonly Vector3Compressed forward = Vector3.forward.Compress();

  public override string ToString() => $"Compressed({this.Decompress()})";
}
