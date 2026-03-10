// Decompiled with JetBrains decompiler
// Type: TimedDeletion
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Cysharp.Threading.Tasks;
using UnityEngine;

#nullable disable
public class TimedDeletion : MonoBehaviour
{
  [SerializeField]
  private float deleteDelay;

  private void Start() => this.DeleteTime().Forget();

  private async UniTask DeleteTime()
  {
    TimedDeletion timedDeletion = this;
    await UniTask.Delay((int) ((double) timedDeletion.deleteDelay * 1000.0));
    if ((Object) timedDeletion == (Object) null)
      return;
    Object.Destroy((Object) timedDeletion.gameObject);
  }
}
