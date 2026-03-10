// Decompiled with JetBrains decompiler
// Type: Wreckage
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

#nullable disable
public class Wreckage : MonoBehaviour
{
  private Rigidbody debrisRB;
  [SerializeField]
  private float mass = 7000f;

  private void Start()
  {
    NetworkSceneSingleton<MissionManager>.i.listWrecks.Add(this);
    if ((double) NetworkSceneSingleton<MissionManager>.i.wrecksDecayTime <= 0.0)
      return;
    UniTask.Delay((int) (1.0 + (double) NetworkSceneSingleton<MissionManager>.i.wrecksDecayTime * 60.0) * 1000).ContinueWith((Action) (() =>
    {
      if (!((UnityEngine.Object) this != (UnityEngine.Object) null))
        return;
      this.Deactivate();
    })).Forget();
  }

  private void OnCollisionEnter(Collision collision)
  {
    if (!((UnityEngine.Object) this.debrisRB == (UnityEngine.Object) null))
      return;
    if ((double) this.mass > 500.0)
    {
      this.mass -= 500f;
      this.debrisRB = this.gameObject.AddComponent<Rigidbody>();
      this.debrisRB.mass = this.mass;
      UnityEngine.Object.Destroy((UnityEngine.Object) this.debrisRB, 20f);
    }
    else
      this.Deactivate();
  }

  public void Deactivate()
  {
    foreach (UnityEngine.Object component in this.GetComponents<Collider>())
      UnityEngine.Object.Destroy(component);
    UnityEngine.Object.Destroy((UnityEngine.Object) UnityEngine.Object.Instantiate<GameObject>(GameAssets.i.vehicleWreckDestroyed, this.transform.position + Vector3.up, this.transform.rotation), 10f);
    this.transform.position += new Vector3(0.0f, -1f, 0.0f);
    this.transform.eulerAngles += new Vector3(UnityEngine.Random.Range(15f, -15f), 0.0f, UnityEngine.Random.Range(30f, -30f));
    if (NetworkSceneSingleton<MissionManager>.i.wrecksMaxNumber > 0)
    {
      GridSquare gridSquare;
      if (!BattlefieldGrid.TryGetGridSquare(this.transform.GlobalPosition(), out gridSquare))
        return;
      for (int index = 0; index < gridSquare.obstacles.Count; ++index)
      {
        if ((UnityEngine.Object) gridSquare.obstacles[index].Transform == (UnityEngine.Object) this.transform)
        {
          gridSquare.obstacles.RemoveAt(index);
          break;
        }
      }
      NetworkSceneSingleton<MissionManager>.i.listWrecks.Remove(this);
      UnityEngine.Object.Destroy((UnityEngine.Object) this.gameObject, 1f);
    }
    else
      UnityEngine.Object.Destroy((UnityEngine.Object) this);
  }
}
