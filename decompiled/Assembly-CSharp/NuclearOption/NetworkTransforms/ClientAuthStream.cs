// Decompiled with JetBrains decompiler
// Type: NuclearOption.NetworkTransforms.ClientAuthStream
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Mirage.Serialization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using UnityEngine;

#nullable disable
namespace NuclearOption.NetworkTransforms;

public class ClientAuthStream : IDisposable
{
  public static string OpenPath;
  private BinaryWriter Writer;
  private Thread thread;
  private CancellationTokenSource cancellation;
  private readonly Queue<ClientAuthStream.LogEntry> logEntries = new Queue<ClientAuthStream.LogEntry>(1000);
  private readonly AutoResetEvent updateFinished = new AutoResetEvent(false);
  private readonly NetworkWriter netWriter = new NetworkWriter(1200, true);

  public void Open(string path)
  {
    this.Writer = this.Writer == null ? new BinaryWriter((Stream) new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read)) : throw new InvalidOperationException("Already open");
    this.cancellation = new CancellationTokenSource();
    this.thread = new Thread(new ThreadStart(this.ThreadLoop));
    this.thread.Priority = System.Threading.ThreadPriority.BelowNormal;
    this.thread.Start();
  }

  public void Dispose()
  {
    this.cancellation?.Cancel();
    this.updateFinished.Set();
    if (this.thread != null && !this.thread.Join(TimeSpan.FromMilliseconds(100.0)))
      Debug.Log((object) "ClientAuthStream did not finish within 100ms");
    this.thread = (Thread) null;
    this.Writer?.Close();
    this.Writer?.Dispose();
    this.Writer = (BinaryWriter) null;
    this.cancellation?.Dispose();
    this.cancellation = (CancellationTokenSource) null;
    this.updateFinished.Dispose();
  }

  public void ThreadLoop()
  {
    CancellationToken token = this.cancellation.Token;
    List<ClientAuthStream.LogEntry> logEntryList = new List<ClientAuthStream.LogEntry>(100);
    while (!token.IsCancellationRequested)
    {
      try
      {
        this.updateFinished.WaitOne();
        logEntryList.Clear();
        lock (this.logEntries)
        {
          logEntryList.AddRange((IEnumerable<ClientAuthStream.LogEntry>) this.logEntries);
          this.logEntries.Clear();
        }
        foreach (ClientAuthStream.LogEntry entry in logEntryList)
          this.Write(entry);
      }
      catch (Exception ex)
      {
        Debug.LogError((object) $"ClientAuthStream {ex}");
      }
    }
  }

  public void MarkUpdatedFinished()
  {
    bool flag;
    lock (this.logEntries)
      flag = this.logEntries.Count > 0;
    if (!flag)
      return;
    this.updateFinished.Set();
  }

  public void LogBlank()
  {
    lock (this.logEntries)
      this.logEntries.Enqueue(new ClientAuthStream.LogEntry());
  }

  public void Log(
    AircraftNetworkTransform nt,
    ClientAuthChecks.RejectMask acceptedMask,
    NetworkTransformBase.NetworkSnapshot snapshot,
    double clientLocalTime,
    double clientServerTime)
  {
    ulong valueOrDefault = nt?.Aircraft?.Player?.GetAuthData()?.SteamID.m_SteamID.GetValueOrDefault();
    uint netId = nt.NetId;
    ClientAuthStream.LogEntry logEntry = new ClientAuthStream.LogEntry()
    {
      steamId = valueOrDefault,
      netId = netId,
      acceptedMask = acceptedMask,
      clientLocalTime = clientLocalTime,
      clientServerTime = clientServerTime,
      snapshot = snapshot
    };
    lock (this.logEntries)
      this.logEntries.Enqueue(logEntry);
  }

  private void Write(ClientAuthStream.LogEntry entry)
  {
    this.netWriter.Reset();
    this.netWriter.WriteUInt64(entry.steamId);
    this.netWriter.WriteUInt32(entry.netId);
    this.netWriter.WriteDouble(entry.clientLocalTime);
    this.netWriter.WriteDouble(entry.clientServerTime);
    this.netWriter.WriteUInt32((uint) entry.acceptedMask);
    this.netWriter.Write<NetworkTransformBase.NetworkSnapshot>(entry.snapshot);
    ArraySegment<byte> arraySegment = this.netWriter.ToArraySegment();
    this.Writer.Write(arraySegment.Count);
    this.Writer.Write(ReadOnlySpan<byte>.op_Implicit(arraySegment));
  }

  public static void ToCSV(string input, string output)
  {
    using (BinaryReader binaryReader = new BinaryReader((Stream) new FileStream(input, FileMode.Open, FileAccess.Read, FileShare.Read)))
    {
      using (StreamWriter streamWriter1 = new StreamWriter(output))
      {
        using (NetworkReader reader = new NetworkReader())
        {
          streamWriter1.WriteLine("SteamID,NetId,AcceptedMask,ClientLocalTime,ClientServerTime,Timestamp,ExtraExtrapolation,HasInputs,GlobalPos_X,GlobalPos_Y,GlobalPos_Z,Velocity_X,Velocity_Y,Velocity_Z,Rotation_X,Rotation_Y,Rotation_Z,Rotation_W");
          while (true)
          {
            try
            {
              int count = binaryReader.ReadInt32();
              byte[] array = binaryReader.ReadBytes(count);
              reader.Reset(array);
            }
            catch (EndOfStreamException ex)
            {
              break;
            }
            catch (Exception ex)
            {
              Console.WriteLine($"Error reading log file: {ex.Message}. Stopping conversion.");
              break;
            }
            try
            {
              ulong num1 = reader.ReadUInt64();
              uint num2 = reader.ReadUInt32();
              double num3 = reader.ReadDouble();
              double num4 = reader.ReadDouble();
              ClientAuthChecks.RejectMask rejectMask = (ClientAuthChecks.RejectMask) reader.ReadUInt32();
              NetworkTransformBase.NetworkSnapshot networkSnapshot = reader.Read<NetworkTransformBase.NetworkSnapshot>();
              Vector3 vector3 = networkSnapshot.velocity.Decompress();
              Quaternion rotation = networkSnapshot.rotation;
              StreamWriter streamWriter2 = streamWriter1;
              string[] strArray = new string[31 /*0x1F*/];
              strArray[0] = $"{num1},";
              strArray[1] = $"{num2},";
              strArray[2] = $"{rejectMask},";
              strArray[3] = num3.ToString("F6", (IFormatProvider) CultureInfo.InvariantCulture);
              strArray[4] = ",";
              strArray[5] = num4.ToString("F6", (IFormatProvider) CultureInfo.InvariantCulture);
              strArray[6] = ",";
              strArray[7] = NullableValue(networkSnapshot.timestamp);
              strArray[8] = ",";
              float? extraExtrapolation = networkSnapshot.extraExtrapolation;
              strArray[9] = NullableValue(extraExtrapolation.HasValue ? new double?((double) extraExtrapolation.GetValueOrDefault()) : new double?());
              strArray[10] = ",";
              strArray[11] = $"{networkSnapshot.ClientInputs.HasValue},";
              strArray[12] = networkSnapshot.globalPos.x.ToString("F6", (IFormatProvider) CultureInfo.InvariantCulture);
              strArray[13] = ",";
              strArray[14] = networkSnapshot.globalPos.y.ToString("F6", (IFormatProvider) CultureInfo.InvariantCulture);
              strArray[15] = ",";
              strArray[16 /*0x10*/] = networkSnapshot.globalPos.z.ToString("F6", (IFormatProvider) CultureInfo.InvariantCulture);
              strArray[17] = ",";
              strArray[18] = vector3.x.ToString("F4", (IFormatProvider) CultureInfo.InvariantCulture);
              strArray[19] = ",";
              strArray[20] = vector3.y.ToString("F4", (IFormatProvider) CultureInfo.InvariantCulture);
              strArray[21] = ",";
              strArray[22] = vector3.z.ToString("F4", (IFormatProvider) CultureInfo.InvariantCulture);
              strArray[23] = ",";
              strArray[24] = rotation.x.ToString("F4", (IFormatProvider) CultureInfo.InvariantCulture);
              strArray[25] = ",";
              strArray[26] = rotation.y.ToString("F4", (IFormatProvider) CultureInfo.InvariantCulture);
              strArray[27] = ",";
              strArray[28] = rotation.z.ToString("F4", (IFormatProvider) CultureInfo.InvariantCulture);
              strArray[29] = ",";
              strArray[30] = rotation.w.ToString("F4", (IFormatProvider) CultureInfo.InvariantCulture);
              string str = string.Concat(strArray);
              streamWriter2.WriteLine(str);
            }
            catch (Exception ex)
            {
              Console.WriteLine($"Error parsing line: {ex.Message}.");
            }
          }
        }
      }
    }

    static string NullableValue(double? val)
    {
      return !val.HasValue ? string.Empty : val.Value.ToString((IFormatProvider) CultureInfo.InvariantCulture);
    }
  }

  private struct LogEntry
  {
    public ulong steamId;
    public uint netId;
    public ClientAuthChecks.RejectMask acceptedMask;
    public double clientLocalTime;
    public double clientServerTime;
    public NetworkTransformBase.NetworkSnapshot snapshot;
  }
}
