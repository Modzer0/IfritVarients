// Decompiled with JetBrains decompiler
// Type: SlowUpdateExtensions
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Cysharp.Threading.Tasks;
using Mirage;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using Unity.Profiling;
using UnityEngine;

#nullable disable
public static class SlowUpdateExtensions
{
  private static readonly ProfilerMarker slowUpdateMarker = new ProfilerMarker("SlowUpdate");
  private static readonly ProfilerMarker UpdateMarker = new ProfilerMarker("SlowUpdateRunner.Update");
  private static readonly ProfilerMarker CheckMarker = new ProfilerMarker("SlowUpdateRunner.Check");
  private static readonly ProfilerMarker InvokeMarker = new ProfilerMarker("SlowUpdateRunner.Invoke");
  private static readonly ProfilerMarker RemoveMarker = new ProfilerMarker("SlowUpdateRunner.Remove");
  private static SlowUpdateExtensions.Runner runner;

  private static ProfilerMarker CreateMarkerFromAction(Action action)
  {
    MethodInfo methodInfo = action.GetMethodInfo();
    if (methodInfo == (MethodInfo) null)
      return new ProfilerMarker("NoMethodName");
    System.Type declaringType = methodInfo.DeclaringType;
    return declaringType == (System.Type) null ? new ProfilerMarker(methodInfo.Name) : new ProfilerMarker($"{declaringType}.{methodInfo.Name}");
  }

  public static void StartSlowUpdateDelayed(
    this MonoBehaviour behaviour,
    float startDelayAndInterval,
    Action update,
    CancellationToken cancellationToken = default (CancellationToken))
  {
    if (cancellationToken == CancellationToken.None)
      cancellationToken = behaviour.destroyCancellationToken;
    SlowUpdateExtensions.AddCall(update, startDelayAndInterval, startDelayAndInterval, cancellationToken);
  }

  public static void StartSlowUpdateDelayed(
    this MonoBehaviour behaviour,
    float startDelay,
    float interval,
    Action update,
    CancellationToken cancellationToken = default (CancellationToken))
  {
    if (cancellationToken == CancellationToken.None)
      cancellationToken = behaviour.destroyCancellationToken;
    SlowUpdateExtensions.AddCall(update, interval, startDelay, cancellationToken);
  }

  public static void StartSlowUpdate(
    this MonoBehaviour behaviour,
    float interval,
    Action update,
    CancellationToken cancellationToken = default (CancellationToken))
  {
    if (cancellationToken == CancellationToken.None)
      cancellationToken = behaviour.destroyCancellationToken;
    SlowUpdateExtensions.AddCall(update, interval, 0.0f, cancellationToken);
  }

  public static async UniTask Loop(
    Action update,
    int delayMs,
    bool ignoreTimescale,
    CancellationToken cancellationToken,
    PlayerLoopTiming timing = PlayerLoopTiming.Update)
  {
    while (!cancellationToken.IsCancellationRequested)
    {
      try
      {
        update();
      }
      catch (Exception ex)
      {
        Debug.LogError((object) $"{ex.GetType().Name} in SlowUpdate: {ex}");
      }
      if (delayMs == 0)
        await UniTask.Yield(timing);
      else
        await UniTask.Delay(delayMs, ignoreTimescale, timing);
    }
  }

  public static CancellationToken GetCancellationTokenOnNetworkStop(this NetworkBehaviour behaviour)
  {
    NetworkIdentity identity = behaviour.Identity;
    CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(behaviour.destroyCancellationToken);
    identity.OnStopClient.AddListener(new Action(OnStop));
    identity.OnStopServer.AddListener(new Action(OnStop));
    return cts.Token;

    void OnStop()
    {
      cts.Cancel();
      identity.OnStopClient.RemoveListener(new Action(OnStop));
      identity.OnStopServer.RemoveListener(new Action(OnStop));
    }
  }

  public static void AddCall(
    Action action,
    float interval,
    float startDelay,
    CancellationToken cancellationToken)
  {
    if (SlowUpdateExtensions.runner == null)
      SlowUpdateExtensions.runner = new GameObject("SlowUpdateRunner").AddComponent<SlowUpdateExtensions.Runner>();
    SlowUpdateExtensions.runner.AddCall(action, interval, startDelay, cancellationToken);
  }

  public static void DestroyRunner()
  {
    if ((UnityEngine.Object) SlowUpdateExtensions.runner != (UnityEngine.Object) null)
      UnityEngine.Object.Destroy((UnityEngine.Object) SlowUpdateExtensions.runner.gameObject);
    SlowUpdateExtensions.runner = (SlowUpdateExtensions.Runner) null;
  }

  private class Runner : MonoBehaviour
  {
    private float now;
    private int count;
    private SlowUpdateExtensions.Call[] calls = new SlowUpdateExtensions.Call[128 /*0x80*/];
    private float[] nextInvokeTime = new float[128 /*0x80*/];
    private List<int> toCall = new List<int>();
    private List<int> toRemove = new List<int>();

    private void OnDestroy() => SlowUpdateExtensions.runner = (SlowUpdateExtensions.Runner) null;

    public void AddCall(
      Action action,
      float interval,
      float startDelay,
      CancellationToken cancellationToken)
    {
      if ((double) startDelay > 0.0)
      {
        float num = UnityEngine.Random.value * Mathf.Min(5f, interval);
        startDelay += num;
      }
      if (this.calls.Length < this.count + 1)
      {
        int newSize = this.calls.Length * 2;
        Array.Resize<SlowUpdateExtensions.Call>(ref this.calls, newSize);
        Array.Resize<float>(ref this.nextInvokeTime, newSize);
      }
      this.calls[this.count] = new SlowUpdateExtensions.Call(action, interval, cancellationToken);
      this.nextInvokeTime[this.count] = this.now + startDelay;
      ++this.count;
    }

    private void Update()
    {
      using (SlowUpdateExtensions.UpdateMarker.Auto())
      {
        this.now = Time.timeSinceLevelLoad;
        using (SlowUpdateExtensions.CheckMarker.Auto())
        {
          this.toCall.Clear();
          for (int index = 0; index < this.count; ++index)
          {
            if ((double) this.now > (double) this.nextInvokeTime[index])
              this.toCall.Add(index);
          }
        }
        if (this.toCall.Count > 0)
        {
          using (SlowUpdateExtensions.InvokeMarker.Auto())
          {
            foreach (int index in this.toCall)
            {
              ref SlowUpdateExtensions.Call local = ref this.calls[index];
              if (local.Cancellation.IsCancellationRequested)
                this.toRemove.Add(index);
              else
                this.Invoke(ref this.nextInvokeTime[index], ref local);
            }
          }
        }
        if (this.toRemove.Count <= 0)
          return;
        this.RemoveTimers();
      }
    }

    private void RemoveTimers()
    {
      using (SlowUpdateExtensions.RemoveMarker.Auto())
      {
        foreach (int index in this.toRemove)
          this.calls[index] = new SlowUpdateExtensions.Call();
        int index1 = this.toRemove[0];
        for (int index2 = index1 + 1; index2 < this.count; ++index2)
        {
          if (this.calls[index2].Action != null)
          {
            this.calls[index1] = this.calls[index2];
            this.nextInvokeTime[index1] = this.nextInvokeTime[index2];
            ++index1;
          }
        }
        this.count -= this.toRemove.Count;
        this.toRemove.Clear();
      }
    }

    private void Invoke(ref float nextInvoke, ref SlowUpdateExtensions.Call call)
    {
      try
      {
        call.Action();
      }
      catch (Exception ex)
      {
        Debug.LogError((object) $"{ex.GetType().Name} in SlowUpdate: {ex}");
      }
      nextInvoke = this.now + call.Interval;
    }
  }

  private readonly struct Call
  {
    public readonly Action Action;
    public readonly float Interval;
    public readonly CancellationToken Cancellation;

    public Call(Action action, float delay, CancellationToken cancellationToken)
      : this()
    {
      this.Action = action;
      this.Interval = delay;
      this.Cancellation = cancellationToken;
    }
  }
}
