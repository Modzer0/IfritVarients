// Decompiled with JetBrains decompiler
// Type: BattlefieldGrid
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

#nullable disable
public static class BattlefieldGrid
{
  public static GridSquare[] gridLookup;
  private static float mapSize;
  private static float gridSize;
  private static int divisions;
  private static readonly List<Unit> inRangeUnitsCache = new List<Unit>(100);
  private static readonly List<Wreckage> inRangeWrecksCache = new List<Wreckage>(100);
  private static readonly List<GridSquare> gridCache = new List<GridSquare>(100);

  public static void GenerateGrid(float setMapSize, float setGridSize)
  {
    if ((double) setMapSize == 0.0)
      Debug.LogError((object) "Can set mapSize to 0");
    else if ((double) setGridSize == 0.0)
    {
      Debug.LogError((object) "Can set gridSize to 0");
    }
    else
    {
      BattlefieldGrid.mapSize = setMapSize;
      BattlefieldGrid.gridSize = setGridSize;
      BattlefieldGrid.divisions = (int) ((double) BattlefieldGrid.mapSize / (double) BattlefieldGrid.gridSize);
      BattlefieldGrid.gridLookup = new GridSquare[BattlefieldGrid.divisions * BattlefieldGrid.divisions];
      for (int index = 0; index < BattlefieldGrid.divisions * BattlefieldGrid.divisions; ++index)
      {
        int x = index / BattlefieldGrid.divisions;
        int y = index % BattlefieldGrid.divisions;
        BattlefieldGrid.gridLookup[index] = new GridSquare(x, y);
      }
    }
  }

  public static void Clear()
  {
    BattlefieldGrid.gridLookup = (GridSquare[]) null;
    BattlefieldGrid.inRangeUnitsCache.Clear();
    BattlefieldGrid.inRangeWrecksCache.Clear();
    BattlefieldGrid.gridCache.Clear();
    BattlefieldGrid.divisions = 0;
  }

  public static bool TryGetGridSquare(GlobalPosition coord, out GridSquare gridSquare)
  {
    int gridCoordX;
    int gridCoordY;
    if (BattlefieldGrid.TryGetGridXY(coord, out gridCoordX, out gridCoordY))
    {
      int index = gridCoordY * BattlefieldGrid.divisions + gridCoordX;
      gridSquare = BattlefieldGrid.gridLookup[index];
      return true;
    }
    gridSquare = (GridSquare) null;
    return false;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool TryGetGridXY(GlobalPosition coord, out int gridCoordX, out int gridCoordY)
  {
    if (!float.IsFinite(coord.x) || !float.IsFinite(coord.z))
    {
      gridCoordX = 0;
      gridCoordY = 0;
      return false;
    }
    gridCoordX = (int) Mathf.Clamp((coord.x + BattlefieldGrid.mapSize * 0.5f) / BattlefieldGrid.gridSize, 0.0f, (float) (BattlefieldGrid.divisions - 1));
    gridCoordY = (int) Mathf.Clamp((coord.z + BattlefieldGrid.mapSize * 0.5f) / BattlefieldGrid.gridSize, 0.0f, (float) (BattlefieldGrid.divisions - 1));
    return true;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static GridSquare GetSquare(int gridCoordX, int gridCoordY)
  {
    return BattlefieldGrid.gridLookup[gridCoordY * BattlefieldGrid.divisions + gridCoordX];
  }

  public static void UpdateUnit(Unit unit, ref GridSquare gridSquare)
  {
    GridSquare gridSquare1;
    if (!BattlefieldGrid.TryGetGridSquare(unit.transform.GlobalPosition(), out gridSquare1) || gridSquare1 == gridSquare)
      return;
    gridSquare?.units.Remove(unit);
    gridSquare = gridSquare1;
    gridSquare.units.Add(unit);
  }

  public static List<GridSquare> GetGridSquaresInRange(GlobalPosition coord, float range)
  {
    List<GridSquare> gridSquares = new List<GridSquare>();
    BattlefieldGrid.GetGridSquaresInRangeNonAlloc(coord, range, gridSquares);
    return gridSquares;
  }

  public static void GetGridSquaresInRangeNonAlloc(
    GlobalPosition coord,
    float range,
    List<GridSquare> gridSquares)
  {
    gridSquares.Clear();
    int gridCoordX1;
    int gridCoordY1;
    if (!BattlefieldGrid.TryGetGridXY(coord, out gridCoordX1, out gridCoordY1))
      return;
    int num1 = Mathf.CeilToInt(range * (float) BattlefieldGrid.divisions / BattlefieldGrid.mapSize);
    int num2 = Mathf.Max(gridCoordX1 - num1, 0);
    int num3 = Mathf.Min(gridCoordX1 + num1, BattlefieldGrid.divisions);
    int num4 = Mathf.Max(gridCoordY1 - num1, 0);
    int num5 = Mathf.Min(gridCoordY1 + num1, BattlefieldGrid.divisions);
    for (int gridCoordX2 = num2; gridCoordX2 < num3; ++gridCoordX2)
    {
      for (int gridCoordY2 = num4; gridCoordY2 < num5; ++gridCoordY2)
        gridSquares.Add(BattlefieldGrid.GetSquare(gridCoordX2, gridCoordY2));
    }
  }

  public static IEnumerable<Unit> GetUnitsInRangeEnumerable(GlobalPosition coord, float range)
  {
    BattlefieldGrid.GetUnitsInRangeNonAlloc(coord, range, BattlefieldGrid.inRangeUnitsCache);
    return (IEnumerable<Unit>) BattlefieldGrid.inRangeUnitsCache;
  }

  public static void GetUnitsInRangeNonAlloc(GlobalPosition coord, float range, List<Unit> units)
  {
    units.Clear();
    BattlefieldGrid.GetGridSquaresInRangeNonAlloc(coord, range, BattlefieldGrid.gridCache);
    foreach (GridSquare gridSquare in BattlefieldGrid.gridCache)
    {
      List<Unit> units1 = gridSquare.units;
      if (units1.Count > 0)
      {
        for (int index = units1.Count - 1; index >= 0; --index)
        {
          Unit unit = units1[index];
          units.Add(unit);
        }
      }
    }
  }

  public static IEnumerable<Wreckage> GetWrecksInRangeEnumerable(GlobalPosition coord, float range)
  {
    BattlefieldGrid.GetWrecksInRangeNonAlloc(coord, range, BattlefieldGrid.inRangeWrecksCache);
    return (IEnumerable<Wreckage>) BattlefieldGrid.inRangeWrecksCache;
  }

  public static void GetWrecksInRangeNonAlloc(
    GlobalPosition coord,
    float range,
    List<Wreckage> wrecks)
  {
    wrecks.Clear();
    BattlefieldGrid.GetGridSquaresInRangeNonAlloc(coord, range, BattlefieldGrid.gridCache);
    foreach (GridSquare gridSquare in BattlefieldGrid.gridCache)
    {
      List<Obstacle> obstacles = gridSquare.obstacles;
      if (obstacles.Count > 0)
      {
        for (int index = obstacles.Count - 1; index >= 0; --index)
        {
          Wreckage component;
          if (obstacles[index].Transform.TryGetComponent<Wreckage>(out component))
            wrecks.Add(component);
        }
      }
    }
  }
}
