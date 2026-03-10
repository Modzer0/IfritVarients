// Decompiled with JetBrains decompiler
// Type: NuclearOption.DedicatedServer.Commands.ServerRemoteCommands
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

#nullable disable
namespace NuclearOption.DedicatedServer.Commands;

public class ServerRemoteCommands : IDisposable
{
  public readonly Dictionary<string, ServerCommand> Commands = new Dictionary<string, ServerCommand>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
  private readonly byte[] messageBuffer;
  private readonly byte[] sendBuffer;
  private readonly ConcurrentQueue<ServerRemoteCommands.MainThreadAction> mainThreadQueue = new ConcurrentQueue<ServerRemoteCommands.MainThreadAction>();
  private readonly Thread mainThread;
  private TcpListener tcpListener;
  private Thread runThread;
  private CancellationTokenSource cancellation;
  private string runningCommand;

  public static ServerRemoteCommands Instance { get; private set; }

  public static ServerRemoteCommands GetOrCreate(int receiveBufferSize = 2048 /*0x0800*/, int sendBufferSize = 131027)
  {
    if (ServerRemoteCommands.Instance == null)
      ServerRemoteCommands.Instance = new ServerRemoteCommands(receiveBufferSize, sendBufferSize);
    return ServerRemoteCommands.Instance;
  }

  public ServerRemoteCommands(int receiveBufferSize = 2048 /*0x0800*/, int sendBufferSize = 131027)
  {
    this.messageBuffer = new byte[receiveBufferSize];
    this.sendBuffer = new byte[sendBufferSize];
    int frameCount = Time.frameCount;
    this.mainThread = Thread.CurrentThread;
  }

  public void AddCommands(List<ServerCommand> commands)
  {
    foreach (ServerCommand command in commands)
    {
      if (this.Commands.TryAdd(command.Name, command))
        ColorLog<ServerRemoteCommands>.Info("Adding command " + command.Name);
      else
        ColorLog<ServerRemoteCommands>.Info($"Failed to add command {command.Name} because one with the same name already exists");
    }
  }

  public void RunOnMainThread(Action action)
  {
    this.mainThreadQueue.Enqueue(new ServerRemoteCommands.MainThreadAction(this.runningCommand, action));
  }

  public (bool ok, T result) RunOnMainThreadBlocking<T>(Func<(bool ok, T result)> action)
  {
    if (Thread.CurrentThread == this.mainThread)
    {
      ColorLog<ServerRemoteCommands>.Info("Already running on main thread, returning result right away");
      return action();
    }
    TaskCompletionSource<(bool, T)> source = new TaskCompletionSource<(bool, T)>();
    this.RunOnMainThread((Action) (() =>
    {
      try
      {
        source.SetResult(action());
      }
      catch (Exception ex)
      {
        source.SetResult((false, default (T)));
        UnityEngine.Debug.LogException(ex);
      }
    }));
    Task<(bool, T)> task = source.Task;
    ColorLog<ServerRemoteCommands>.Info("Blocking side thread until MainThread Action is done");
    task.Wait();
    ColorLog<ServerRemoteCommands>.Info("MainThread Action is done");
    return task.Result;
  }

  public void Dispose()
  {
    this.cancellation?.Cancel();
    this.cancellation?.Dispose();
    this.tcpListener?.Stop();
    this.tcpListener = (TcpListener) null;
    this.runThread = (Thread) null;
    this.cancellation = (CancellationTokenSource) null;
  }

  public void Start(ushort port)
  {
    try
    {
      this.tcpListener = new TcpListener(IPAddress.Loopback, (int) port);
      this.tcpListener.Start();
      ColorLog<ServerRemoteCommands>.Info($"ServerRemoteCommands started on localhost:{port}, LittleEndian={BitConverter.IsLittleEndian}");
    }
    catch (SocketException ex)
    {
      ColorLog<ServerRemoteCommands>.InfoWarn("Error starting TCP listener: " + ex.Message);
    }
    try
    {
      this.cancellation = new CancellationTokenSource();
      this.runThread = new Thread(new ThreadStart(this.SideThread_Loop));
      this.runThread.Start();
    }
    catch (Exception ex)
    {
      ColorLog<ServerRemoteCommands>.InfoWarn($"Failed to start side thread {ex}");
    }
  }

  private void SideThread_Loop()
  {
    CancellationToken token = this.cancellation.Token;
    TcpListener tcpListener = this.tcpListener;
    while (!token.IsCancellationRequested)
    {
      try
      {
        Stopwatch stopwatch;
        using (TcpClient tcpClient = tcpListener.AcceptTcpClient())
        {
          stopwatch = Stopwatch.StartNew();
          tcpClient.ReceiveTimeout = 1000;
          tcpClient.NoDelay = true;
          tcpClient.LingerState = new LingerOption(true, 10);
          using (NetworkStream stream = tcpClient.GetStream())
          {
            this.SideThread_ReadNext(stream);
            tcpClient.Client.Shutdown(SocketShutdown.Send);
          }
        }
        ColorLog<ServerRemoteCommands>.Info($"TcpClient closed, process time: {stopwatch.Elapsed}");
        stopwatch.Stop();
        stopwatch = (Stopwatch) null;
      }
      catch (SocketException ex) when (ex.SocketErrorCode == SocketError.Interrupted)
      {
        ColorLog<ServerRemoteCommands>.InfoWarn("TcpListener Interrupted");
        break;
      }
      catch (ThreadAbortException ex)
      {
        ColorLog<ServerRemoteCommands>.Info("Thread Aborted");
      }
      catch (Exception ex)
      {
        ColorLog<ServerRemoteCommands>.InfoWarn($"Error from SideThread_ReadNext: {ex}");
      }
    }
  }

  private void SideThread_ReadNext(NetworkStream stream)
  {
    ColorLog<ServerRemoteCommands>.Info("New command connection received.");
    try
    {
      CommandResponse commandResponse = this.SideThread_ReadInner(stream);
      ColorLog<ServerRemoteCommands>.Info($"response: {commandResponse.StatusCode}");
      int num1 = 0;
      Span<byte> span = MemoryExtensions.AsSpan<byte>(this.sendBuffer);
      BitConverter.TryWriteBytes(span, (int) commandResponse.StatusCode);
      int num2 = num1 + 4;
      byte[] body = commandResponse.Body;
      int length = body != null ? body.Length : 0;
      BitConverter.TryWriteBytes(span.Slice(num2), length);
      int num3 = num2 + 4;
      if (commandResponse.Body != null)
      {
        MemoryExtensions.CopyTo<byte>(commandResponse.Body, span.Slice(num3));
        num3 += commandResponse.Body.Length;
      }
      stream.Write(Span<byte>.op_Implicit(span.Slice(0, num3)));
    }
    catch (SocketException ex) when (ex.SocketErrorCode == SocketError.TimedOut)
    {
      ColorLog<ServerRemoteCommands>.InfoWarn($"Read timeout: {ex}");
    }
    catch (Exception ex)
    {
      ColorLog<ServerRemoteCommands>.InfoWarn($"Error processing command: {ex}");
    }
  }

  private CommandResponse SideThread_ReadInner(NetworkStream stream)
  {
    Span<byte> span = stackalloc byte[4];
    if (stream.Read(span) < 4)
    {
      ColorLog<ServerRemoteCommands>.InfoWarn("Incomplete message length received. Discarding.");
      return CommandResponse.Create(StatusCode.BadHeader);
    }
    int int32 = BitConverter.ToInt32(Span<byte>.op_Implicit(span));
    if (int32 <= 0)
    {
      ColorLog<ServerRemoteCommands>.InfoWarn($"Message Length {int32} is 0 or negative. Discarding.");
      return CommandResponse.Create(StatusCode.BadLength);
    }
    if (int32 > this.messageBuffer.Length)
    {
      ColorLog<ServerRemoteCommands>.InfoWarn($"Message Length {int32} but max size is set to {this.messageBuffer.Length}. Discarding.");
      return CommandResponse.Create(StatusCode.BadLength);
    }
    int offset = 0;
    while (offset < int32)
      offset += stream.Read(this.messageBuffer, offset, int32 - offset);
    string json = Encoding.UTF8.GetString(this.messageBuffer, 0, int32);
    CommandMessage message;
    try
    {
      message = JsonUtility.FromJson<CommandMessage>(json);
    }
    catch (Exception ex)
    {
      ColorLog<ServerRemoteCommands>.InfoWarn($"Failed to deserialize JSON: {ex}");
      return CommandResponse.Create(StatusCode.JsonError);
    }
    return this.FindAndRunCommand(message);
  }

  public CommandResponse FindAndRunCommand(CommandMessage message)
  {
    ColorLog<ServerRemoteCommands>.Info($"Received Command {message.name ?? "NULL"} [{(message.arguments != null ? string.Join(",", message.arguments) : "")}]");
    if (!string.IsNullOrEmpty(message.name))
    {
      ServerCommand serverCommand;
      if (this.Commands.TryGetValue(message.name, out serverCommand))
      {
        try
        {
          this.runningCommand = message.name;
          return serverCommand.Run(this, message.arguments);
        }
        catch (Exception ex)
        {
          ColorLog<ServerRemoteCommands>.InfoWarn($"Error running command: {ex}");
          return CommandResponse.Create(StatusCode.CommandError);
        }
      }
    }
    ColorLog<ServerRemoteCommands>.InfoWarn("Unknown command " + message.name);
    return CommandResponse.Create(StatusCode.UnknownCommand);
  }

  public void PollAll(int maxMessage)
  {
    int num = 0;
    while (this.PollOne())
    {
      ++num;
      if (num >= maxMessage)
        break;
    }
  }

  public bool PollOne()
  {
    ServerRemoteCommands.MainThreadAction result;
    if (!this.mainThreadQueue.TryDequeue(out result))
      return false;
    ColorLog<ServerRemoteCommands>.InfoWarn($"Running {result.Name} on main thread");
    try
    {
      result.Action();
    }
    catch (Exception ex)
    {
      ColorLog<ServerRemoteCommands>.InfoWarn($"Error running {result.Name} on main thread: {ex}");
    }
    return true;
  }

  public static void DEBUG_Send()
  {
    using (TcpClient tcpClient = new TcpClient())
    {
      tcpClient.SendTimeout = 1000;
      tcpClient.ReceiveTimeout = 1000;
      tcpClient.Connect("localhost", 7779);
      using (NetworkStream stream = tcpClient.GetStream())
      {
        Span<byte> span1 = stackalloc byte[200];
        int bytes = Encoding.UTF8.GetBytes(string.op_Implicit(JsonUtility.ToJson((object) new CommandMessage()
        {
          name = "update-ready"
        })), span1.Slice(4));
        BitConverter.TryWriteBytes(span1.Slice(0, 4), bytes);
        stream.Write(Span<byte>.op_Implicit(span1));
        UnityEngine.Debug.Log((object) "Sent");
        Span<byte> span2 = stackalloc byte[8];
        int num1 = 0;
        do
        {
          num1 += stream.Read(span2);
        }
        while (num1 != 8);
        StatusCode int32_1 = (StatusCode) BitConverter.ToInt32(Span<byte>.op_Implicit(span2.Slice(0, 4)));
        int int32_2 = BitConverter.ToInt32(Span<byte>.op_Implicit(span2.Slice(4, 4)));
        UnityEngine.Debug.Log((object) $"Status:{int32_1}");
        UnityEngine.Debug.Log((object) $"JsonLength:{int32_2}");
        if (int32_2 <= 0)
          return;
        Span<byte> span3 = stackalloc byte[int32_2];
        int num2 = 0;
        do
        {
          num2 += stream.Read(span3);
        }
        while (num2 != int32_2);
        UnityEngine.Debug.Log((object) ("Json:" + Encoding.UTF8.GetString(Span<byte>.op_Implicit(span3))));
      }
    }
  }

  public readonly struct MainThreadAction(string name, Action action)
  {
    public readonly string Name = name;
    public readonly Action Action = action;
  }
}
