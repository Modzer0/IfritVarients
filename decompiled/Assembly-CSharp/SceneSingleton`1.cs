// Decompiled with JetBrains decompiler
// Type: SceneSingleton`1
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;

#nullable disable
public abstract class SceneSingleton<T> : MonoBehaviour, ISceneSingleton where T : SceneSingleton<T>
{
  public static T i;

  protected virtual void Awake() => this.SetupSingleton();

  protected void SetupSingleton()
  {
    if (!((UnityEngine.Object) SceneSingleton<T>.i != (UnityEngine.Object) this))
      return;
    SceneSingleton<T>.i = (T) this;
    GameManager.SceneSingletons.Add((ISceneSingleton) SceneSingleton<T>.i);
  }

  public static void ThrowIfInstanceNull()
  {
    if ((UnityEngine.Object) SceneSingleton<T>.i == (UnityEngine.Object) null)
      throw new InvalidOperationException(typeof (T).FullName + " was not in scene");
  }

  bool ISceneSingleton.ClearInstance()
  {
    int num = (UnityEngine.Object) this == (UnityEngine.Object) null ? 1 : 0;
    if (num == 0)
      return num != 0;
    if (!((UnityEngine.Object) this == (UnityEngine.Object) SceneSingleton<T>.i))
      return num != 0;
    SceneSingleton<T>.i = default (T);
    return num != 0;
  }
}
