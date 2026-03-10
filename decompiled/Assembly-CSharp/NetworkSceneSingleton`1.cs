// Decompiled with JetBrains decompiler
// Type: NetworkSceneSingleton`1
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Mirage;
using System;

#nullable disable
public abstract class NetworkSceneSingleton<T> : NetworkBehaviour, ISceneSingleton where T : NetworkSceneSingleton<T>
{
  public static T i;
  [NonSerialized]
  private const int SYNC_VAR_COUNT = 0;
  [NonSerialized]
  private const int RPC_COUNT = 0;

  protected virtual void Awake()
  {
    NetworkSceneSingleton<T>.i = (T) this;
    GameManager.SceneSingletons.Add((ISceneSingleton) NetworkSceneSingleton<T>.i);
  }

  public static void ThrowIfInstanceNull()
  {
    if ((UnityEngine.Object) NetworkSceneSingleton<T>.i == (UnityEngine.Object) null)
      throw new InvalidOperationException(typeof (T).FullName + " was not in scene");
  }

  bool ISceneSingleton.ClearInstance()
  {
    int num = (UnityEngine.Object) this == (UnityEngine.Object) null ? 1 : 0;
    if (num == 0)
      return num != 0;
    if (!((UnityEngine.Object) this == (UnityEngine.Object) NetworkSceneSingleton<T>.i))
      return num != 0;
    NetworkSceneSingleton<T>.i = default (T);
    return num != 0;
  }

  private void MirageProcessed()
  {
  }

  protected override int GetRpcCount() => 0;
}
