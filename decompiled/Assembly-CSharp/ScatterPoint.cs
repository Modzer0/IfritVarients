// Decompiled with JetBrains decompiler
// Type: ScatterPoint
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;

#nullable disable
[Serializable]
public struct ScatterPoint(Vector3 globalPos, Quaternion rotation, float scale)
{
  public readonly Vector3 globalPos = globalPos;
  public readonly Quaternion rotation = rotation;
  public readonly float scale = scale;

  public readonly Vector3 ScaleVector => new Vector3(this.scale, this.scale, this.scale);
}
