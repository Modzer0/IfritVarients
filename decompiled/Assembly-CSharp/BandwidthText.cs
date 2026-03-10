// Decompiled with JetBrains decompiler
// Type: BandwidthText
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using JamesFrowen.ScriptableVariables;
using Mirage.SocketLayer;
using System;
using UnityEngine;

#nullable disable
public class BandwidthText : MonoBehaviour
{
  [SerializeField]
  protected float _updateInterval = 0.2f;
  public Metrics Metrics;
  private float[] deltaTime = new float[0];
  [Header("References")]
  [SerializeField]
  private NonAllocGui.Wrapper _connectionText;
  [SerializeField]
  private NonAllocGui.Wrapper _sendCountText;
  [SerializeField]
  private NonAllocGui.Wrapper _sendBytesText;
  [SerializeField]
  private NonAllocGui.Wrapper _receiveCountText;
  [SerializeField]
  private NonAllocGui.Wrapper _receiveBytesText;
  private float _updateTimer;

  public void Update()
  {
    if (this.Metrics == null)
      return;
    this._updateTimer += UnityEngine.Time.unscaledDeltaTime;
    Metrics.Frame[] buffer = this.Metrics.buffer;
    if (this.deltaTime.Length != buffer.Length)
      Array.Resize<float>(ref this.deltaTime, buffer.Length);
    this.deltaTime[(int) this.Metrics.tick] = UnityEngine.Time.unscaledDeltaTime;
    if ((double) this._updateTimer <= (double) this._updateInterval)
      return;
    this._updateTimer = 0.0f;
    int num1 = 0;
    float num2 = 0.0f;
    int num3 = 0;
    float num4 = 0.0f;
    float num5 = 0.0f;
    for (int index = 0; index < buffer.Length; ++index)
    {
      if (buffer[index].init)
      {
        num5 += this.deltaTime[index];
        num1 += buffer[index].sendCount;
        num2 += (float) buffer[index].sendBytes;
        num3 += buffer[index].receiveCount;
        num4 += (float) buffer[index].receiveBytes;
      }
    }
    this._connectionText.SetValue((float) buffer[(int) this.Metrics.tick].connectionCount);
    this._sendCountText.SetValue((float) (int) ((double) num1 / (double) num5));
    this._sendBytesText.SetValue((float) ((double) num2 / (double) num5 / 1024.0));
    this._receiveCountText.SetValue((float) (int) ((double) num3 / (double) num5));
    this._receiveBytesText.SetValue((float) ((double) num4 / (double) num5 / 1024.0));
  }
}
