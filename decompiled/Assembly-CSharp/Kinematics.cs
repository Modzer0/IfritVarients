// Decompiled with JetBrains decompiler
// Type: Kinematics
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System.Collections.Generic;
using UnityEngine;

#nullable disable
public static class Kinematics
{
  private static List<GameObject> visualizations = new List<GameObject>();

  public static float FallTime(float initialHeight, float initialVerticalVelocity)
  {
    float num1 = -4.905f;
    float num2 = initialVerticalVelocity;
    float num3 = initialHeight;
    float f = (float) ((double) num2 * (double) num2 - 4.0 * (double) num1 * (double) num3);
    if ((double) f < 0.0)
      return -1f;
    float a = (float) ((-(double) num2 + (double) Mathf.Sqrt(f)) / (2.0 * (double) num1));
    float b = (float) ((-(double) num2 - (double) Mathf.Sqrt(f)) / (2.0 * (double) num1));
    if ((double) a < 0.0)
      a = b;
    if ((double) b < 0.0)
      b = a;
    return Mathf.Min(a, b);
  }

  public static Vector3 TrajectorySim(
    WeaponInfo weaponInfo,
    Vector3 initialVelocity,
    GlobalPosition initialPosition,
    GlobalPosition targetPos,
    Vector3 targetVel,
    Vector3 targetAccel,
    float timeStep,
    out float timeToTarget)
  {
    foreach (GameObject visualization in Kinematics.visualizations)
      NetworkSceneSingleton<Spawner>.i.DestroyLocal(visualization, 1f);
    Kinematics.visualizations.Clear();
    timeToTarget = 0.0f;
    GlobalPosition globalPosition1 = targetPos;
    Vector3 vector3_1 = targetVel;
    GlobalPosition position = initialPosition;
    Vector3 vector3_2 = initialVelocity;
    bool flag = false;
    while (!flag)
    {
      timeStep += 0.02f;
      if (PlayerSettings.debugVis)
      {
        GameObject gameObject = NetworkSceneSingleton<Spawner>.i.SpawnLocal(GameAssets.i.debugArrowGreen, Datum.origin);
        gameObject.transform.position = position.ToLocalPosition();
        gameObject.transform.rotation = Quaternion.LookRotation(vector3_2);
        gameObject.transform.localScale = new Vector3(2f, 2f, vector3_2.magnitude * timeStep);
        Kinematics.visualizations.Add(gameObject);
      }
      Vector3 vector3_3 = 9.81f * timeStep * Vector3.up + vector3_2.sqrMagnitude * weaponInfo.dragCoef * timeStep * vector3_2.normalized / weaponInfo.muzzleVelocity;
      vector3_1 += targetAccel * timeStep;
      globalPosition1 += vector3_1 * timeStep;
      vector3_2 -= vector3_3 * 0.3f;
      position += vector3_2 * timeStep;
      vector3_2 -= vector3_3 * 0.7f;
      flag = (double) Vector3.Dot(vector3_2, globalPosition1 - position) <= 0.0;
      timeToTarget += timeStep;
    }
    GlobalPosition globalPosition2 = position - 0.5f * timeStep * vector3_2;
    GlobalPosition a = globalPosition1 - 0.5f * timeStep * vector3_1;
    if (PlayerSettings.debugVis)
    {
      GameObject gameObject = NetworkSceneSingleton<Spawner>.i.SpawnLocal(GameAssets.i.debugArrowGreen, Datum.origin);
      gameObject.transform.position = targetPos.ToLocalPosition();
      gameObject.transform.rotation = Quaternion.LookRotation(a - targetPos);
      gameObject.transform.localScale = new Vector3(1f, 1f, FastMath.Distance(a, targetPos));
      Kinematics.visualizations.Add(gameObject);
    }
    return Vector3.ProjectOnPlane(globalPosition2 - a, vector3_2);
  }
}
