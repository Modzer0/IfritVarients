// Decompiled with JetBrains decompiler
// Type: FixedTimer
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Cysharp.Threading.Tasks;
using UnityEngine;

#nullable disable
public class FixedTimer
{
  private float timer;

  public async UniTask Wait(float addTime)
  {
    this.timer += addTime;
    for (this.timer -= Time.deltaTime; (double) this.timer > 0.0; this.timer -= Time.deltaTime)
      await UniTask.Yield();
  }
}
