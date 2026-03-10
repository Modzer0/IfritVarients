// Decompiled with JetBrains decompiler
// Type: DamageEffects
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.Networking;
using UnityEngine;

#nullable disable
public static class DamageEffects
{
  public static Collider[] hitColliders;

  public static void ArmorPenetrate(
    Vector3 position,
    Vector3 velocity,
    float muzzleVelocity,
    float pierceDamage,
    float blastDamage,
    PersistentID dealerID)
  {
    int num = 0;
    Vector3 start = position;
    Vector3 vector3 = velocity;
    if ((double) blastDamage > 0.0)
      DamageEffects.BlastFrag(blastDamage, position, dealerID, PersistentID.None);
    RaycastHit hitInfo;
    for (; num < 10 && Physics.Linecast(start, start + vector3 * 0.1f, out hitInfo, -8193) && (double) vector3.sqrMagnitude > (double) muzzleVelocity * (double) muzzleVelocity * 0.10000000149011612; start = hitInfo.point + 0.1f * vector3.normalized)
    {
      ++num;
      IDamageable component = hitInfo.collider.gameObject.GetComponent<IDamageable>();
      if (component == null)
        break;
      float pierceDamage1 = (float) ((double) Mathf.Max(Vector3.Dot(vector3.normalized, -hitInfo.normal), 0.5f) * (double) pierceDamage * ((double) vector3.magnitude / (double) muzzleVelocity));
      component.TakeDamage(pierceDamage1, 1f, 0.0f, 0.0f, 0.0f, dealerID);
      ArmorProperties armorProperties = component.GetArmorProperties();
      if ((double) pierceDamage1 <= (double) armorProperties.pierceArmor)
        break;
      vector3 *= (pierceDamage1 - armorProperties.pierceArmor * 2f) / pierceDamage1;
    }
  }

  public static void BlastFrag(
    float blastYield,
    Vector3 blastPosition,
    PersistentID dealerID,
    PersistentID missileID)
  {
    float blastPower = Mathf.Pow(blastYield, 0.3333f);
    float radius = blastPower * 20f;
    Transform transform = Datum.origin;
    if (PlayerSettings.debugVis)
    {
      Unit nearestUnit;
      transform = UnitRegistry.TryGetNearestUnit(blastPosition.ToGlobalPosition(), out nearestUnit, 100f) ? nearestUnit.transform : Datum.origin;
      GameObject gameObject = NetworkSceneSingleton<Spawner>.i.SpawnLocal(GameAssets.i.blastRadiusDebug, transform);
      gameObject.GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", new Color(1f, 0.5f, 0.0f, 1f));
      gameObject.transform.position = blastPosition;
      gameObject.transform.localScale = Vector3.one * blastPower;
      NetworkSceneSingleton<Spawner>.i.DestroyLocal(gameObject, 10f);
    }
    if (DamageEffects.hitColliders == null)
      DamageEffects.hitColliders = new Collider[512 /*0x0200*/];
    int num = Physics.OverlapSphereNonAlloc(blastPosition, radius, DamageEffects.hitColliders);
    for (int index = 0; index < num; ++index)
    {
      Collider hitCollider = DamageEffects.hitColliders[index];
      if (hitCollider.gameObject.TryGetComponent<IDamageable>(out IDamageable _))
      {
        Vector3 vector3 = hitCollider.transform.position - blastPosition;
        DamageEffects.FragTrace(blastPosition, vector3.normalized * radius, blastYield, blastPower, hitCollider, dealerID, transform);
      }
    }
  }

  public static void FragTrace(
    Vector3 origin,
    Vector3 fragVector,
    float blastYield,
    float blastPower,
    Collider fragTarget,
    PersistentID dealerID,
    Transform debugTransform)
  {
    float z1 = 0.0f;
    Vector3 start = origin;
    float num1 = 0.0f;
    int num2 = 0;
    RaycastHit hitInfo;
    while (num2 < 10 && Physics.Linecast(start, start + fragVector, out hitInfo, -40961))
    {
      ++num2;
      z1 += hitInfo.distance;
      IDamageable component;
      if (!hitInfo.collider.gameObject.TryGetComponent<IDamageable>(out component))
      {
        if (!((Object) hitInfo.collider.sharedMaterial != (Object) GameAssets.i.terrainMaterial) || !((Object) hitInfo.collider.sharedMaterial != (Object) null))
          break;
        start = hitInfo.point + fragVector.normalized * 0.1f;
      }
      else
      {
        ArmorProperties armorProperties = component.GetArmorProperties();
        if (armorProperties == null)
          break;
        float num3 = Mathf.Max((z1 + num1) / blastPower, 1f);
        float overpressure = (float) (25000.0 / ((double) num3 * (double) num3 * (double) num3));
        if ((double) overpressure <= 0.0)
          break;
        if ((Object) hitInfo.collider == (Object) fragTarget)
        {
          double x = (double) hitInfo.collider.bounds.extents.x;
          Bounds bounds = hitInfo.collider.bounds;
          double y = (double) bounds.extents.y;
          double num4 = x + y;
          bounds = hitInfo.collider.bounds;
          double z2 = (double) bounds.extents.z;
          double num5 = (num4 + z2) * 0.33329999446868896;
          float num6 = (float) (num5 * num5);
          float num7 = Mathf.Clamp(Mathf.Max(z1 * z1, blastPower * blastPower) / num6, 0.0f, 10f) / (float) (1.0 + (double) armorProperties.blastArmor * 0.05000000074505806);
          if ((double) blastPower >= 0.5)
            component.TakeShockwave(origin, overpressure, blastPower);
          float num8 = Mathf.Clamp01((float) (((double) blastPower * 500.0 - (double) armorProperties.blastArmor) / ((double) armorProperties.blastArmor * 2.0)));
          float blastDamage = overpressure * num8 - armorProperties.blastArmor;
          if ((double) blastDamage <= 0.0)
            break;
          if (NetworkManagerNuclearOption.i.Server.Active)
            component.TakeDamage(0.0f, blastDamage, Mathf.Clamp01(num7), 0.0f, 0.0f, dealerID);
          if (!PlayerSettings.debugVis)
            break;
          GameObject gameObject = NetworkSceneSingleton<Spawner>.i.SpawnLocal(GameAssets.i.debugArrow, debugTransform);
          gameObject.GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", new Color(1f, 0.5f, 0.0f, 1f));
          gameObject.transform.position = origin;
          gameObject.transform.rotation = Quaternion.LookRotation(hitInfo.point - origin);
          gameObject.transform.localScale = new Vector3(0.5f, 0.5f, z1);
          NetworkSceneSingleton<Spawner>.i.DestroyLocal(gameObject, 10f);
          break;
        }
        num1 += armorProperties.blastArmor * 0.1f;
        start = hitInfo.point + fragVector.normalized * 0.1f;
      }
    }
  }
}
