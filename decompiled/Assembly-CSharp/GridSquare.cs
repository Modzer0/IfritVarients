// Decompiled with JetBrains decompiler
// Type: GridSquare
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System.Collections.Generic;

#nullable disable
public class GridSquare
{
  public readonly int X;
  public readonly int Y;
  public readonly List<Unit> units = new List<Unit>();
  public readonly List<Obstacle> obstacles = new List<Obstacle>();

  public GridSquare(int x, int y)
  {
    this.X = x;
    this.Y = y;
  }

  public override string ToString() => $"GridSquare({this.X},{this.Y})";
}
