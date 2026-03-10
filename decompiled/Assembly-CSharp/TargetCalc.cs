// Decompiled with JetBrains decompiler
// Type: TargetCalc
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

#nullable disable
public static class TargetCalc
{
  public static Quaternion FromToRotation(Quaternion start, Quaternion end)
  {
    return Quaternion.Inverse(start) * end;
  }

  public static float GetAngleOnAxis(Vector3 self, Vector3 other, Vector3 axis)
  {
    return Vector3.SignedAngle(Vector3.Cross(axis, self), Vector3.Cross(axis, other), axis);
  }

  public static Vector3 GetLeadVector(
    GlobalPosition targetPos,
    GlobalPosition platformPos,
    Vector3 targetVel,
    Vector3 platformVel,
    float maxLead)
  {
    float a = Vector3.Dot((targetPos - platformPos).normalized, platformVel - targetVel);
    return Mathf.Clamp(FastMath.Distance(platformPos, targetPos) / Mathf.Max(a, 10f), 0.0f, maxLead) * targetVel;
  }

  public static Vector3 GetLeadVectorWithAccel(
    GlobalPosition targetPos,
    GlobalPosition platformPos,
    Vector3 targetVel,
    Vector3 platformVel,
    Vector3 targetAccel,
    float maxLead)
  {
    targetAccel = Vector3.ClampMagnitude(targetAccel, 100f) + 9.81f * Vector3.up;
    float a = Vector3.Dot((targetPos - platformPos).normalized, platformVel - targetVel);
    float num = Mathf.Clamp(FastMath.Distance(platformPos, targetPos) / Mathf.Max(a, 10f), 0.0f, maxLead);
    return num * targetVel + Mathf.Min(num * num, 1f) * 0.5f * targetAccel;
  }

  public static float TargetLeadTime(
    Unit target,
    GameObject gun,
    Rigidbody muzzleRB,
    float muzzleVelocity,
    float dragCoef,
    int iterations)
  {
    Vector3 vector3 = (Object) target.rb != (Object) null ? target.rb.velocity : Vector3.zero;
    float num1 = muzzleVelocity;
    if ((Object) muzzleRB != (Object) null)
      num1 += Vector3.Dot(muzzleRB.velocity, gun.transform.forward);
    float num2 = 0.0f;
    for (int index = 0; index < iterations; ++index)
    {
      float num3 = Vector3.Distance(target.transform.position + vector3 * num2, gun.transform.position);
      num2 = (Mathf.Pow(2.71828f, dragCoef * num3 / num1) - 1f) / dragCoef;
      if (!float.IsFinite(num2) || (double) num2 > 120.0)
        return 120f;
    }
    return num2;
  }

  public static void GetLeadFromMaxTargetSpeed(
    Unit target,
    Transform targetTransform,
    Transform fromTransform,
    GlobalPosition oldKnownPos,
    float maxTargetSpeed,
    out GlobalPosition newKnownPos,
    out Vector3 knownVel)
  {
    Vector3 localPosition = oldKnownPos.ToLocalPosition();
    Vector3 position1 = targetTransform.position;
    newKnownPos = position1.ToGlobalPosition();
    knownVel = (Object) target.rb != (Object) null ? target.rb.velocity : Vector3.zero;
    if ((double) target.speed <= (double) maxTargetSpeed || (double) Vector3.ProjectOnPlane(target.rb.velocity, (position1 - fromTransform.position).normalized).sqrMagnitude <= (double) maxTargetSpeed * (double) maxTargetSpeed)
      return;
    Vector3 position2 = Vector3.Lerp(localPosition + maxTargetSpeed * Time.fixedDeltaTime * (position1 - localPosition).normalized, targetTransform.position, 0.5f * Time.fixedDeltaTime);
    newKnownPos = position2.ToGlobalPosition();
    knownVel = (position2 - localPosition) * Time.fixedDeltaTime;
  }

  public static bool LineOfSight(Transform source, Transform destination, float radius)
  {
    RaycastHit hitInfo;
    return !Physics.Linecast(source.position, destination.position, out hitInfo, 64 /*0x40*/) || FastMath.InRange(hitInfo.point, destination.position, radius);
  }

  public static bool ClosestPointsOnTwoLines(
    out Vector3 closestPointLine1,
    out Vector3 closestPointLine2,
    Vector3 linePoint1,
    Vector3 lineVec1,
    Vector3 linePoint2,
    Vector3 lineVec2)
  {
    closestPointLine1 = Vector3.zero;
    closestPointLine2 = Vector3.zero;
    float num1 = Vector3.Dot(lineVec1, lineVec1);
    float num2 = Vector3.Dot(lineVec1, lineVec2);
    float num3 = Vector3.Dot(lineVec2, lineVec2);
    float num4 = (float) ((double) num1 * (double) num3 - (double) num2 * (double) num2);
    if ((double) num4 == 0.0)
      return false;
    Vector3 rhs = linePoint1 - linePoint2;
    float num5 = Vector3.Dot(lineVec1, rhs);
    float num6 = Vector3.Dot(lineVec2, rhs);
    float num7 = (float) ((double) num2 * (double) num6 - (double) num5 * (double) num3) / num4;
    float num8 = (float) ((double) num1 * (double) num6 - (double) num5 * (double) num2) / num4;
    closestPointLine1 = linePoint1 + lineVec1 * num7;
    closestPointLine2 = linePoint2 + lineVec2 * num8;
    return true;
  }
}
