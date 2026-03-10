// Decompiled with JetBrains decompiler
// Type: FiringConeChecker
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

#nullable disable
public static class FiringConeChecker
{
  public static bool VectorWithinFiringCones(
    FiringCone[] firingCones,
    Vector3 targetVector,
    out Vector3 nearestAllowableVector)
  {
    nearestAllowableVector = targetVector;
    if (firingCones.Length == 0)
      return true;
    foreach (FiringCone firingCone in firingCones)
    {
      Vector3 allowedVector;
      if (!firingCone.VectorPermitted(nearestAllowableVector, out allowedVector))
        nearestAllowableVector = allowedVector;
    }
    return targetVector == nearestAllowableVector;
  }
}
