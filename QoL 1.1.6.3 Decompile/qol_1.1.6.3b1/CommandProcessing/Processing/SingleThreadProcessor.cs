// Decompiled with JetBrains decompiler
// Type: qol.CommandProcessing.Processing.SingleThreadProcessor
// Assembly: qol, Version=1.1.6.3, Culture=neutral, PublicKeyToken=null
// MVID: 3B6539EA-E38C-45EE-8FFF-FC58CC42BADC
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\BepInEx\plugins\qol_1.1.6.3b1.dll

using System;
using System.Collections;
using System.Diagnostics;
using UnityEngine;

#nullable disable
namespace qol.CommandProcessing.Processing;

public class SingleThreadProcessor
{
  private readonly ProcessingContext _context;
  private readonly string _version;

  public SingleThreadProcessor(ProcessingContext context, string version)
  {
    this._context = context;
    this._version = version;
  }

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
    if (this._context.NotDedicatedServer)
      uiReporter.Initialize();
    Stopwatch stopwatch = new Stopwatch();
    stopwatch.Start();
    int startFrame = Time.frameCount;
    for (int i = 0; i < lines.Length; ++i)
    {
      string line = lines[i];
      ++processedLines;
      if (string.IsNullOrEmpty(line) || line.StartsWith("#"))
      {
        if (this._context.NotDedicatedServer)
          yield return (object) null;
      }
      else
      {
        if (this._context.NotDedicatedServer)
          uiReporter.UpdateProgress(processedLines, totalLines, line, this._version);
        try
        {
          this._context.ProcessLine(line);
        }
        catch (Exception ex)
        {
          this._context.Logger.LogError((object) $"Error processing line {i + 1}: {ex.Message}");
        }
        if (this._context.NotDedicatedServer)
          yield return (object) null;
      }
    }
    stopwatch.Stop();
    if (this._context.NotDedicatedServer)
      yield return (object) null;
    float num = (float) (Time.frameCount - startFrame) * Time.deltaTime;
    this._context.Logger.LogInfo((object) $"Processed {processedLines} lines");
    this._context.Logger.LogInfo((object) $"Elapsed {(ValueType) (float) ((double) Mathf.Round(num * 100f) * 0.0099999997764825821)}s in-game ({Time.frameCount - startFrame} frames)");
    this._context.Logger.LogInfo((object) $"Elapsed {stopwatch.ElapsedMilliseconds / 1000L}s realtime ({stopwatch.ElapsedTicks} ticks)");
    if (this._context.NotDedicatedServer)
    {
      uiReporter.ShowCompletion(totalLines, stopwatch.ElapsedMilliseconds, stopwatch.ElapsedTicks);
      yield return (object) new WaitForSeconds(1f);
      uiReporter.Cleanup();
    }
  }
}
