// Decompiled with JetBrains decompiler
// Type: Parabola
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

#nullable disable
public class Parabola
{
  public static Vector3 Coefficients(Vector2 p1, Vector2 p2, Vector2 p3)
  {
    float x = (float) ((((double) p1.y - (double) p2.y) * ((double) p1.x - (double) p3.x) - ((double) p1.y - (double) p3.y) * ((double) p1.x - (double) p2.x)) / (((double) p1.x * (double) p1.x - (double) p2.x * (double) p2.x) * ((double) p1.x - (double) p3.x) - ((double) p1.x * (double) p1.x - (double) p3.x * (double) p3.x) * ((double) p1.x - (double) p2.x)));
    float y = (float) (((double) p1.y - (double) p2.y - (double) x * ((double) p1.x * (double) p1.x - (double) p2.x * (double) p2.x)) / ((double) p1.x - (double) p2.x));
    float z = (float) ((double) p1.y - (double) x * (double) p1.x * (double) p1.x - (double) y * (double) p1.x);
    return new Vector3(x, y, z);
  }
}
