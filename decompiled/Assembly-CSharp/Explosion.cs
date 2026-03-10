// Decompiled with JetBrains decompiler
// Type: Explosion
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System.Collections.Generic;
using UnityEngine;

#nullable disable
public static class Explosion
{
  public static void SimulateForce(Vector3 position, float yield)
  {
    float num = Mathf.Pow(yield, 0.3333f);
    float radius = num * 20f;
    Dictionary<Collider, IDamageable> dictionary = new Dictionary<Collider, IDamageable>();
    List<Collider> colliderList = new List<Collider>();
    foreach (Collider key in Physics.OverlapSphere(position, radius))
    {
      IDamageable component = key.gameObject.GetComponent<IDamageable>();
      if (component != null)
        dictionary.Add(key, component);
    }
    foreach (KeyValuePair<Collider, IDamageable> keyValuePair in dictionary)
    {
      RaycastHit hitInfo;
      if (Physics.Linecast(position, keyValuePair.Key.gameObject.transform.position, out hitInfo, -8193))
      {
        if (dictionary.ContainsKey(hitInfo.collider) && !colliderList.Contains(hitInfo.collider))
        {
          dictionary[hitInfo.collider].TakeShockwave(position, num, num);
          if (PlayerSettings.debugVis)
          {
            GameObject gameObject = NetworkSceneSingleton<Spawner>.i.SpawnLocal(GameAssets.i.debugArrow, hitInfo.collider.transform);
            gameObject.transform.position = position;
            gameObject.transform.rotation = Quaternion.LookRotation(hitInfo.point - position);
            gameObject.transform.localScale = new Vector3(1f, 1f, hitInfo.distance);
            NetworkSceneSingleton<Spawner>.i.DestroyLocal(gameObject, 10f);
          }
        }
        colliderList.Add(hitInfo.collider);
      }
    }
  }
}
