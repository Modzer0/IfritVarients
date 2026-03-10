// Decompiled with JetBrains decompiler
// Type: NuclearOption.SavedMission.PositionRotation
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;

#nullable disable
namespace NuclearOption.SavedMission;

[Serializable]
public struct PositionRotation : IEquatable<PositionRotation>
{
  public GlobalPosition Position;
  public Quaternion Rotation;

  public readonly bool Equals(PositionRotation other)
  {
    return other.Position.Equals(this.Position) && other.Rotation.Equals(this.Rotation);
  }
}
