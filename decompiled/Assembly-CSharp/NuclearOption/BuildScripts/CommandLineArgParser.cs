// Decompiled with JetBrains decompiler
// Type: NuclearOption.BuildScripts.CommandLineArgParser
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Cysharp.Threading.Tasks;
using Mirage;
using NuclearOption.BuildScripts.DebugTests;
using NuclearOption.DedicatedServer;
using NuclearOption.DedicatedServer.Commands;
using NuclearOption.MissionEditorScripts;
using NuclearOption.Networking;
using NuclearOption.NetworkTransforms;
using NuclearOption.SavedMission;
using NuclearOption.SavedMission.ObjectiveV2;
using NuclearOption.SceneLoading;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;

#nullable disable
namespace NuclearOption.BuildScripts;

public class CommandLineArgParser : MonoBehaviour
{
  [SerializeField]
  private bool runInEditor;
  public static NuclearOption.Networking.SocketType? SocketType;
  private int? port;
  private string address;
  private string missionName;
  private GameState? state;
  private static CommandLineArgParser i;
  private LeakTest leakTest;
  private CancellationToken destroyCancellationToken;
  public static bool IsAutoStart;
  public static bool UseSteamNetworkingVerboseLogging;

  private void Awake()
  {
    if ((UnityEngine.Object) CommandLineArgParser.i == (UnityEngine.Object) null)
    {
      CommandLineArgParser.i = this;
      UnityEngine.Object.DontDestroyOnLoad((UnityEngine.Object) this.gameObject);
      this.Parse();
    }
    else
      UnityEngine.Object.Destroy((UnityEngine.Object) this.gameObject);
  }

  public void Parse()
  {
    this.destroyCancellationToken = this.destroyCancellationToken;
    List<string> stringList = new List<string>((IEnumerable<string>) Environment.GetCommandLineArgs());
    CommandLineArgParser.addEditorArgs(stringList);
    CommandParser.Parse(stringList, this.getArgCommands());
  }

  private static void addEditorArgs(List<string> args)
  {
  }

  private List<CommandParser.ArgCommand> getArgCommands()
  {
    return new List<CommandParser.ArgCommand>()
    {
      new CommandParser.ArgCommand("-autoHost", new CommandParser.HandleArgDelegateCoroutine(this.parseAutoHost)),
      new CommandParser.ArgCommand("-autoConnect", new CommandParser.HandleArgDelegateCoroutine(this.parseAutoConnect)),
      new CommandParser.ArgCommand("-socket", new CommandParser.HandleArgDelegate(this.parseSetSocket)),
      new CommandParser.ArgCommand("-port", new CommandParser.HandleArgDelegate(this.parseSetPort)),
      new CommandParser.ArgCommand("-address", new CommandParser.HandleArgDelegate(this.parseSetAddress)),
      new CommandParser.ArgCommand("-mission", new CommandParser.HandleArgDelegate(this.parseSetMission)),
      new CommandParser.ArgCommand("-state", new CommandParser.HandleArgDelegate(this.parseSetGameSate)),
      new CommandParser.ArgCommand("-recordcpu", new CommandParser.HandleArgDelegate(this.parseRecordCpu)),
      new CommandParser.ArgCommand("-closeafter", new CommandParser.HandleArgDelegate(this.parseCloseAfter)),
      new CommandParser.ArgCommand("-limitframerate", new CommandParser.HandleArgDelegate(this.parseLimitFrameRate)),
      new CommandParser.ArgCommand("-BenchmarkScope", new CommandParser.HandleArgDelegate(this.parseBenchmarkScope)),
      new CommandParser.ArgCommand("-SceneLoadTest", new CommandParser.HandleArgDelegateCoroutine(this.parseSceneLoadTest)),
      new CommandParser.ArgCommand("-leakTest", new CommandParser.HandleArgDelegate(this.parseLeakTest)),
      new CommandParser.ArgCommand("-pwTest", new CommandParser.HandleArgDelegate(this.parseLobbyPasswordTest)),
      new CommandParser.ArgCommand("-descTest", new CommandParser.HandleArgDelegate(this.parseDescriptionSplitTest)),
      new CommandParser.ArgCommand("-createtestmission", new CommandParser.HandleArgDelegate(this.parseCreateTestMission)),
      new CommandParser.ArgCommand("-DedicatedServer", new CommandParser.HandleArgDelegateCoroutine(this.parseDedicatedServer)),
      new CommandParser.ArgCommand("-ServerRemoteCommands", new CommandParser.HandleArgDelegate(this.parseServerRemoteCommands)),
      new CommandParser.ArgCommand("-ClientAuthDebugStream", new CommandParser.HandleArgDelegate(this.parseClientAuthDebugStream)),
      new CommandParser.ArgCommand("-ClientAuthDebugStreamToCsv", new CommandParser.HandleArgDelegate(this.parseClientAuthDebugStreamToCsv)),
      new CommandParser.ArgCommand("-ValidateMode", new CommandParser.HandleArgDelegate(this.parseValidateMode)),
      new CommandParser.ArgCommand("-SteamNetworkingVerboseLogging", (CommandParser.HandleArgDelegate) (_ => CommandLineArgParser.UseSteamNetworkingVerboseLogging = true))
    };
  }

  private void parseSetSocket(CommandParser.CommandArguments arguments)
  {
    CommandLineArgParser.SocketType = new NuclearOption.Networking.SocketType?(arguments.GetNextEnum<NuclearOption.Networking.SocketType>(1));
  }

  private void parseSetPort(CommandParser.CommandArguments arguments)
  {
    this.port = new int?(arguments.GetNextInt(1));
  }

  private void parseSetAddress(CommandParser.CommandArguments arguments)
  {
    this.address = arguments.GetNext(1);
  }

  private void parseSetMission(CommandParser.CommandArguments arguments)
  {
    this.missionName = arguments.GetNext(1);
  }

  private void parseSetGameSate(CommandParser.CommandArguments arguments)
  {
    this.state = new GameState?(arguments.GetNextEnum<GameState>(1));
  }

  private void parseRecordCpu(CommandParser.CommandArguments arguments)
  {
    GameObject target = new GameObject("FrameTimeLogger");
    FrameTimeLogger frameTimeLogger = target.AddComponent<FrameTimeLogger>();
    UnityEngine.Object.DontDestroyOnLoad((UnityEngine.Object) target);
    string str;
    if (!arguments.TryGetNext(1, out str))
      return;
    frameTimeLogger.OutFile = str;
  }

  private void parseCloseAfter(CommandParser.CommandArguments arguments)
  {
    int seconds = arguments.GetNextInt(1);
    UniTask.Void((Func<UniTaskVoid>) (async () =>
    {
      await UniTask.Delay(seconds * 1000, true);
      if (this.destroyCancellationToken.IsCancellationRequested)
        return;
      CommandLineArgParser.Quit();
    }));
  }

  private void parseLimitFrameRate(CommandParser.CommandArguments arguments)
  {
    GameManager.OverrideTargetFrameRate = new int?(arguments.GetNextInt(1));
  }

  private void parseBenchmarkScope(CommandParser.CommandArguments arguments)
  {
    BenchmarkScope.ShowInRelease = true;
  }

  private async UniTask parseAutoHost(CommandParser.CommandArguments arguments)
  {
    CommandLineArgParser.IsAutoStart = true;
    await MainMenu.WaitForLoaded(this.destroyCancellationToken);
    if (this.destroyCancellationToken.IsCancellationRequested)
      return;
    if (string.IsNullOrEmpty(this.missionName))
      this.missionName = "Free Flight";
    Mission mission = CommandLineArgParser.LoadMission(this.missionName);
    MissionManager.SetMission(mission, false);
    GameState? state = this.state;
    GameState gameState = GameState.Editor;
    if (state.GetValueOrDefault() == gameState & state.HasValue)
    {
      await MissionEditor.LoadEditor(mission);
    }
    else
    {
      int num1 = (int) CommandLineArgParser.SocketType ?? 1;
      state = this.state;
      int num2 = (int) state ?? 2;
      MapKey mapKey = mission.MapKey;
      await NetworkManagerNuclearOption.i.StartHostAsync(new HostOptions((NuclearOption.Networking.SocketType) num1, (GameState) num2, mapKey)
      {
        MaxConnections = new int?(),
        UdpPort = this.port
      });
    }
  }

  public static Mission LoadMission(string missionName)
  {
    Mission mission;
    string error;
    if (MissionSaveLoad.TryLoad(new MissionKey(missionName, (MissionGroup) MissionGroup.User), out mission, out error) || MissionSaveLoad.TryLoad(new MissionKey(missionName, (MissionGroup) MissionGroup.Tutorial), out mission, out error) || MissionSaveLoad.TryLoad(new MissionKey(missionName, (MissionGroup) MissionGroup.BuiltIn), out mission, out error))
      return mission;
    Debug.LogError((object) $"Failed to load mission with name: {mission}");
    return MissionSaveLoad.LoadDefault();
  }

  private async UniTask parseAutoConnect(CommandParser.CommandArguments arguments)
  {
    await MainMenu.WaitForLoaded(this.destroyCancellationToken);
    if (this.destroyCancellationToken.IsCancellationRequested)
      return;
    string s;
    if (arguments.TryGetNext(1, out s))
    {
      if (string.IsNullOrEmpty(this.address))
      {
        if (ulong.TryParse(s, out ulong _))
        {
          ColorLog<CommandLineArgParser>.Info("using Steam with id: " + s);
          CommandLineArgParser.SocketType = new NuclearOption.Networking.SocketType?(NuclearOption.Networking.SocketType.Steam);
        }
        else
          ColorLog<CommandLineArgParser>.Info("Setting url address to " + s);
        this.address = s;
      }
      else
        ColorLog<CommandLineArgParser>.InfoWarn("cant set address from autoConnect because it was already set");
    }
    NuclearOption.Networking.SocketType socketType = (NuclearOption.Networking.SocketType) ((int) CommandLineArgParser.SocketType ?? 1);
    ConnectOptions options;
    switch (socketType)
    {
      case NuclearOption.Networking.SocketType.Steam:
      case NuclearOption.Networking.SocketType.LagSteam:
        options = new ConnectOptions(socketType, this.address);
        break;
      default:
        options = new ConnectOptions(socketType, this.address, this.port);
        break;
    }
    NetworkManagerNuclearOption.i.Client.Disconnected.AddListener(new UnityAction<ClientStoppedReason>(Disconnected));
    NetworkManagerNuclearOption.i.StartClient(options);

    static void Disconnected(ClientStoppedReason reason)
    {
      Debug.LogError((object) "Disconnected from AutoConnect, closing automatically");
      CommandLineArgParser.Quit();
    }
  }

  private async UniTask parseSceneLoadTest(CommandParser.CommandArguments arguments)
  {
    await new SceneLoadTest().Run();
  }

  private void parseLeakTest(CommandParser.CommandArguments arguments)
  {
    LeakTest.StartNew(ref this.leakTest);
  }

  private void parseLobbyPasswordTest(CommandParser.CommandArguments arguments)
  {
    LobbyPasswordTest.Run();
  }

  private void parseDescriptionSplitTest(CommandParser.CommandArguments arguments)
  {
    DescriptionTests.RunAllTests();
  }

  private void parseCreateTestMission(CommandParser.CommandArguments arguments)
  {
    MissionManager.NewMission(NewMissionConfig.DefaultMission());
    Mission currentMission = MissionManager.CurrentMission;
    foreach (ObjectiveType type in Enum.GetValues(typeof (ObjectiveType)))
    {
      SavedObjective savedObjective = new SavedObjective(type.ToNicifyString<ObjectiveType>(), type);
      currentMission.objectives.Objectives.Add(savedObjective);
    }
    foreach (OutcomeType type in Enum.GetValues(typeof (OutcomeType)))
    {
      SavedOutcome savedOutcome = new SavedOutcome(type.ToNicifyString<OutcomeType>(), type);
      currentMission.objectives.Outcomes.Add(savedOutcome);
    }
    string saveName = "testmission";
    MissionSaveLoad.SaveMission(MissionManager.CurrentMission, ref saveName, false);
    Debug.LogWarning((object) "Created testmission, exiting play mode");
    CommandLineArgParser.Quit();
  }

  private async UniTask parseDedicatedServer(CommandParser.CommandArguments arguments)
  {
    string path;
    if (arguments.TryGetNext(1, out path))
    {
      ColorLog<CommandLineArgParser>.Info("Found config path argument: " + path);
      DedicatedServerConfig config;
      if (DedicatedServerConfig.TryLoad(path, out config))
      {
        ColorLog<CommandLineArgParser>.Info("Loaded config " + path);
        ColorLog<CommandLineArgParser>.Info("Setting DedicatedServerConfig.AutoRun and config");
        DedicatedServerManager.AutoRun = true;
        DedicatedServerManager.SetAutoRunConfig(config, path);
      }
      else
      {
        Debug.LogError((object) ("Failed to load DedicatedServerConfig at " + path));
        await UniTask.Yield();
        CommandLineArgParser.Quit();
      }
    }
    else
    {
      ColorLog<CommandLineArgParser>.Info("Setting DedicatedServerConfig.AutoRun");
      DedicatedServerManager.AutoRun = true;
    }
  }

  private void parseServerRemoteCommands(CommandParser.CommandArguments arguments)
  {
    int num;
    ushort port = arguments.TryGetNextInt(1, out num) ? (ushort) num : (ushort) 7779;
    ColorLog<CommandLineArgParser>.Info("Starting remote command server on port 7779");
    ServerRemoteCommands server = ServerRemoteCommands.GetOrCreate();
    server.AddCommands(DefaultServerCommands.CreateCommands());
    server.AddCommands(BanServerCommands.CreateCommands());
    server.Start(port);
    UniTask.Void((Func<UniTaskVoid>) (async () =>
    {
      CancellationToken cancel = this.destroyCancellationToken;
      while (!cancel.IsCancellationRequested)
      {
        server.PollAll(10);
        await UniTask.Yield();
      }
      server.Dispose();
      cancel = new CancellationToken();
    }));
  }

  private void parseClientAuthDebugStream(CommandParser.CommandArguments arguments)
  {
    ClientAuthStream.OpenPath = arguments.GetNext(1);
  }

  private void parseClientAuthDebugStreamToCsv(CommandParser.CommandArguments arguments)
  {
    string next = arguments.GetNext(1);
    if (Directory.Exists(next))
    {
      foreach (string file in Directory.GetFiles(next))
        ClientAuthStream.ToCSV(file, file + ".csv");
    }
    else
    {
      string input = next;
      string output;
      if (!arguments.TryGetNext(2, out output))
        output = input + ".csv";
      ClientAuthStream.ToCSV(input, output);
    }
    CommandLineArgParser.Quit();
  }

  private void parseValidateMode(CommandParser.CommandArguments arguments)
  {
    ClientAuthChecks.CheckMode checkMode;
    ClientAuthChecks.SetRunChecks(arguments.TryGetNextEnum<ClientAuthChecks.CheckMode>(1, out checkMode) ? new ClientAuthChecks.CheckMode?(checkMode) : new ClientAuthChecks.CheckMode?());
  }

  public static void Quit() => Application.Quit();
}
