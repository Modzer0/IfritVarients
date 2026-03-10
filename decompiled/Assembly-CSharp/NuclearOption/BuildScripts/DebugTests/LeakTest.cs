// Decompiled with JetBrains decompiler
// Type: NuclearOption.BuildScripts.DebugTests.LeakTest
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Cysharp.Threading.Tasks;
using NuclearOption.Networking;
using NuclearOption.SavedMission;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
namespace NuclearOption.BuildScripts.DebugTests;

public class LeakTest
{
  public bool running;
  private static List<string> logs;
  private static object logLock = new object();

  public static void SafeLog(string msg)
  {
    lock (LeakTest.logLock)
      LeakTest.logs?.Add(msg);
  }

  private void FlushLogs()
  {
    int count = LeakTest.logs.Count;
    for (int index = 0; index < count; ++index)
      Debug.Log((object) ("[SafeLog] " + LeakTest.logs[index]));
    lock (LeakTest.logLock)
      LeakTest.logs.RemoveRange(0, count);
  }

  private LeakTest()
  {
  }

  public static void StartNew(ref LeakTest field)
  {
    field?.Stop();
    field = new LeakTest();
    field.RunLeakTest().Forget();
  }

  public void Stop() => this.running = false;

  private async UniTask LogMemory()
  {
    while (this.running)
    {
      NetworkManagerNuclearOption.LogMonoHeap("MonoHeap: {0}");
      await UniTask.Delay(1000);
    }
  }

  private async UniTask LogLoop()
  {
    LeakTest.logs = new List<string>();
    while (this.running)
    {
      await UniTask.Yield();
      this.FlushLogs();
    }
    LeakTest.logs = (List<string>) null;
  }

  public async UniTask RunLeakTest()
  {
    this.running = true;
    this.LogMemory().Forget();
    this.LogLoop().Forget();
    YieldAwaitable yieldAwaitable;
    while ((Object) NetworkManagerNuclearOption.i == (Object) null)
    {
      yieldAwaitable = UniTask.Yield();
      await yieldAwaitable;
    }
    yieldAwaitable = UniTask.Yield();
    await yieldAwaitable;
    if (!this.running)
      return;
    await this.RunLoop();
  }

  private async UniTask RunLoop()
  {
    do
    {
      Mission mission;
      MissionSaveLoad.TryLoad(new MissionKey("Escalation", (MissionGroup) MissionGroup.BuiltIn), out mission, out string _);
      MissionManager.SetMission(mission, false);
      NetworkManagerNuclearOption.i.StartHost(new HostOptions(SocketType.Offline, GameState.SinglePlayer, mission.MapKey));
      await UniTask.Delay(20000);
      if (this.running)
      {
        NetworkManagerNuclearOption.i.Stop(false);
        await UniTask.Delay(5000);
      }
      else
        goto label_2;
    }
    while (this.running);
    goto label_5;
label_2:
    return;
label_5:;
  }
}
