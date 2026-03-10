// Decompiled with JetBrains decompiler
// Type: qol.CommandProcessing.Processing.ThreadedProcessor
// Assembly: qol, Version=1.1.6.3, Culture=neutral, PublicKeyToken=null
// MVID: 3B6539EA-E38C-45EE-8FFF-FC58CC42BADC
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\BepInEx\plugins\qol_1.1.6.3b1.dll

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

#nullable disable
namespace qol.CommandProcessing.Processing;

public class ThreadedProcessor
{
  private readonly ProcessingContext _context;

  public int MaxThreadCount { get; set; } = 4;

  public int ThreadAllocationDelayMs { get; set; } = 2;

  public ThreadedProcessor(ProcessingContext context) => this._context = context;

  public IEnumerator Process(string configContent)
  {
    UIProgressReporter uiReporter = new UIProgressReporter(this._context, "2082LoadingBar_" + this._context.PluginGuid);
    string[] lines = configContent.Split(new string[2]
    {
      "\r\n",
      "\n"
    }, StringSplitOptions.None);
    int totalLines = lines.Length;
    int processedLines = 0;
    float time = 0.0f;
    int threadCount = Math.Min(this.MaxThreadCount, Math.Max(1, Environment.ProcessorCount - 1));
    this._context.Logger.LogInfo((object) $"Using {threadCount} threads for processing");
    CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
    ConcurrentQueue<Exception> exceptions = new ConcurrentQueue<Exception>();
    BlockingCollection<ThreadedProcessor.CommandItem> commandQueue = new BlockingCollection<ThreadedProcessor.CommandItem>((IProducerConsumerCollection<ThreadedProcessor.CommandItem>) new ConcurrentQueue<ThreadedProcessor.CommandItem>());
    (bool, int)[] threadStatus = new (bool, int)[threadCount];
    for (int index = 0; index < threadCount; ++index)
      threadStatus[index] = (false, -1);
    Task[] processingTasks = new Task[threadCount];
    for (int index = 0; index < threadCount; ++index)
    {
      int threadId = index;
      processingTasks[index] = Task.Run((Action) (() =>
      {
        try
        {
          foreach (ThreadedProcessor.CommandItem consuming in commandQueue.GetConsumingEnumerable(cancellationTokenSource.Token))
          {
            threadStatus[threadId] = (true, consuming.LineNumber);
            if (!string.IsNullOrEmpty(consuming.Line) && !consuming.Line.StartsWith("#"))
              this._context.ProcessLine(consuming.Line);
            Interlocked.Increment(ref processedLines);
            threadStatus[threadId] = (false, -1);
            if (this.ThreadAllocationDelayMs > 0)
              Thread.Sleep(this.ThreadAllocationDelayMs);
          }
        }
        catch (Exception ex)
        {
          exceptions.Enqueue(ex);
          cancellationTokenSource.Cancel();
        }
      }), cancellationTokenSource.Token);
    }
    Task.Run((Action) (() =>
    {
      try
      {
        for (int index = 0; index < lines.Length && !cancellationTokenSource.Token.IsCancellationRequested; ++index)
          commandQueue.Add(new ThreadedProcessor.CommandItem()
          {
            Line = lines[index],
            LineNumber = index + 1
          });
        commandQueue.CompleteAdding();
      }
      catch (Exception ex)
      {
        exceptions.Enqueue(ex);
        cancellationTokenSource.Cancel();
      }
    }), cancellationTokenSource.Token);
    while (!Task.WhenAll(processingTasks).IsCompleted)
    {
      time += Time.deltaTime;
      if (this._context.NotDedicatedServer)
        uiReporter.UpdateProgressThreaded(processedLines, totalLines, threadStatus, threadCount, this.MaxThreadCount, this.ThreadAllocationDelayMs);
      if (this._context.NotDedicatedServer)
        yield return (object) null;
    }
    Exception result;
    if (exceptions.TryDequeue(out result))
      this._context.Logger.LogError((object) ("Error processing commands: " + result.Message));
    this._context.Logger.LogInfo((object) $"Processed {processedLines} lines in {(ValueType) (float) ((double) Mathf.Round(time * 10f) * 0.10000000149011612)} seconds using {threadCount} threads");
    if (this._context.NotDedicatedServer)
    {
      uiReporter.ShowCompletionThreaded(totalLines, time, threadCount);
      uiReporter.DestroyAfterDelay(1f);
    }
  }

  private class CommandItem
  {
    public string Line { get; set; }

    public int LineNumber { get; set; }
  }
}
