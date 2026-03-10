// Decompiled with JetBrains decompiler
// Type: FrameTimeLogger
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System.IO;
using UnityEngine;

#nullable disable
public class FrameTimeLogger : MonoBehaviour
{
  public string OutFile = "./frames.csv";
  private const int FRAME_TIMING_COUNT = 100;
  private FrameTiming[] frameTimings = new FrameTiming[100];
  private StreamWriter writer;

  private void Start()
  {
    this.writer = new StreamWriter(this.OutFile)
    {
      AutoFlush = true
    };
    this.writer.WriteLine("count,frame,average");
  }

  private void OnDestroy()
  {
    this.writer.Close();
    this.writer.Dispose();
    this.writer = (StreamWriter) null;
  }

  private void Update()
  {
    uint latestTimings = FrameTimingManager.GetLatestTimings((uint) this.frameTimings.Length, this.frameTimings);
    double num1 = 0.0;
    for (int index = 0; (long) index < (long) latestTimings; ++index)
      num1 += this.frameTimings[index].cpuMainThreadFrameTime;
    double num2 = num1 / (double) latestTimings;
    this.writer.WriteLine($"{latestTimings},{this.frameTimings[0].cpuMainThreadFrameTime},{num2}");
  }
}
