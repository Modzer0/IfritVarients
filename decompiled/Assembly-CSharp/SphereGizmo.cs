// Decompiled with JetBrains decompiler
// Type: SphereGizmo
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#nullable disable
public class SphereGizmo : MonoBehaviour
{
  private readonly Queue<GlobalPosition> positions = new Queue<GlobalPosition>();
  public int MaxPositions;
  public Color[] Colors;
  public float Radius;

  public int Count => this.positions.Count;

  public void Add(Vector3 position) => this.positions.Enqueue(position.ToGlobalPosition());

  public static SphereGizmo Create(int max, Color color, float radius)
  {
    SphereGizmo sphereGizmo = new GameObject(nameof (SphereGizmo)).AddComponent<SphereGizmo>();
    sphereGizmo.MaxPositions = max;
    sphereGizmo.Radius = radius;
    sphereGizmo.Colors = new Color[5];
    float H;
    Color.RGBToHSV(color, out H, out float _, out float _);
    sphereGizmo.Colors[0] = Color.HSVToRGB(H, 1f, 0.3f);
    sphereGizmo.Colors[1] = Color.HSVToRGB(H, 1f, 0.7f);
    sphereGizmo.Colors[2] = Color.HSVToRGB(H, 1f, 1f);
    sphereGizmo.Colors[3] = Color.HSVToRGB(H, 0.8f, 1f);
    sphereGizmo.Colors[4] = Color.HSVToRGB(H, 0.6f, 1f);
    return sphereGizmo;
  }

  private void OnDrawGizmos()
  {
    while (this.positions.Count > this.MaxPositions)
      this.positions.Dequeue();
    if (this.positions.Count <= 0)
      return;
    this.transform.position = this.positions.First<GlobalPosition>().ToLocalPosition();
    int index = 0;
    Vector3 start = Vector3.zero;
    bool flag = false;
    foreach (GlobalPosition position in this.positions)
    {
      Vector3 localPosition = position.ToLocalPosition();
      Color color = this.Colors[index];
      index = (index + 1) % this.Colors.Length;
      Gizmos.color = color;
      Gizmos.DrawSphere(localPosition, this.Radius);
      if (flag)
        Debug.DrawLine(start, localPosition, color, 0.1f);
      start = localPosition;
      flag = true;
    }
  }
}
