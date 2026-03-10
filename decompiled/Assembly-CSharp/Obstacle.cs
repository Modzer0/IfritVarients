// Decompiled with JetBrains decompiler
// Type: Obstacle
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

#nullable disable
public readonly struct Obstacle(Transform transform, float radius, float top)
{
  public readonly Transform Transform = transform;
  public readonly float Radius = radius;
  public readonly float Top = top;
}
