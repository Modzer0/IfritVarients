// Decompiled with JetBrains decompiler
// Type: Mirage.GeneratedNetworkCode
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Mirage.Authentication;
using Mirage.RemoteCalls;
using Mirage.Serialization;
using NuclearOption;
using NuclearOption.Networking;
using NuclearOption.Networking.Authentication;
using NuclearOption.NetworkTransforms;
using NuclearOption.SavedMission;
using NuclearOption.SavedMission.ObjectiveV2;
using NuclearOption.SavedMission.ObjectiveV2.Outcomes;
using NuclearOption.SceneLoading;
using RoadPathfinding;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

#nullable disable
namespace Mirage;

[StructLayout(LayoutKind.Auto, CharSet = CharSet.Auto)]
public static class GeneratedNetworkCode
{
  public static void _Write_Mirage\u002ESceneNotReadyMessage(
    NetworkWriter writer,
    SceneNotReadyMessage value)
  {
  }

  public static SceneNotReadyMessage _Read_Mirage\u002ESceneNotReadyMessage(NetworkReader reader)
  {
    return new SceneNotReadyMessage();
  }

  public static void _Write_Mirage\u002EAddCharacterMessage(
    NetworkWriter writer,
    AddCharacterMessage value)
  {
  }

  public static AddCharacterMessage _Read_Mirage\u002EAddCharacterMessage(NetworkReader reader)
  {
    return new AddCharacterMessage();
  }

  public static void _Write_Mirage\u002ESceneMessage(NetworkWriter writer, SceneMessage value)
  {
    writer.WriteString(value.MainActivateScene);
    GeneratedNetworkCode._Write_Mirage\u002ESceneOperation(writer, value.SceneOperation);
    GeneratedNetworkCode._Write_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CSystem\u002EString\u003E(writer, value.AdditiveScenes);
  }

  public static void _Write_Mirage\u002ESceneOperation(NetworkWriter writer, SceneOperation value)
  {
    writer.WriteByteExtension((byte) value);
  }

  public static void _Write_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CSystem\u002EString\u003E(
    NetworkWriter writer,
    List<string> value)
  {
    writer.WriteList<string>(value);
  }

  public static SceneMessage _Read_Mirage\u002ESceneMessage(NetworkReader reader)
  {
    return new SceneMessage()
    {
      MainActivateScene = reader.ReadString(),
      SceneOperation = GeneratedNetworkCode._Read_Mirage\u002ESceneOperation(reader),
      AdditiveScenes = GeneratedNetworkCode._Read_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CSystem\u002EString\u003E(reader)
    };
  }

  public static SceneOperation _Read_Mirage\u002ESceneOperation(NetworkReader reader)
  {
    return (SceneOperation) reader.ReadByteExtension();
  }

  public static List<string> _Read_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CSystem\u002EString\u003E(
    NetworkReader reader)
  {
    return reader.ReadList<string>();
  }

  public static void _Write_Mirage\u002ESceneReadyMessage(
    NetworkWriter writer,
    SceneReadyMessage value)
  {
  }

  public static SceneReadyMessage _Read_Mirage\u002ESceneReadyMessage(NetworkReader reader)
  {
    return new SceneReadyMessage();
  }

  public static void _Write_Mirage\u002ESpawnMessage(NetworkWriter writer, SpawnMessage value)
  {
    writer.WritePackedUInt32(value.NetId);
    writer.WriteBooleanExtension(value.IsLocalPlayer);
    writer.WriteBooleanExtension(value.IsOwner);
    GeneratedNetworkCode._Write_System\u002ENullable\u00601\u003CSystem\u002EUInt64\u003E(writer, value.SceneId);
    GeneratedNetworkCode._Write_System\u002ENullable\u00601\u003CSystem\u002EInt32\u003E(writer, value.PrefabHash);
    GeneratedNetworkCode._Write_Mirage\u002ESpawnValues(writer, value.SpawnValues);
    writer.WriteBytesAndSizeSegment(value.Payload);
  }

  public static void _Write_System\u002ENullable\u00601\u003CSystem\u002EUInt64\u003E(
    NetworkWriter writer,
    ulong? value)
  {
    writer.WriteNullable<ulong>(value);
  }

  public static void _Write_System\u002ENullable\u00601\u003CSystem\u002EInt32\u003E(
    NetworkWriter writer,
    int? value)
  {
    writer.WriteNullable<int>(value);
  }

  public static void _Write_Mirage\u002ESpawnValues(NetworkWriter writer, SpawnValues value)
  {
    GeneratedNetworkCode._Write_System\u002ENullable\u00601\u003CUnityEngine\u002EVector3\u003E(writer, value.Position);
    GeneratedNetworkCode._Write_System\u002ENullable\u00601\u003CUnityEngine\u002EQuaternion\u003E(writer, value.Rotation);
    GeneratedNetworkCode._Write_System\u002ENullable\u00601\u003CUnityEngine\u002EVector3\u003E(writer, value.Scale);
    writer.WriteString(value.Name);
    GeneratedNetworkCode._Write_System\u002ENullable\u00601\u003CSystem\u002EBoolean\u003E(writer, value.SelfActive);
  }

  public static void _Write_System\u002ENullable\u00601\u003CUnityEngine\u002EVector3\u003E(
    NetworkWriter writer,
    Vector3? value)
  {
    writer.WriteNullable<Vector3>(value);
  }

  public static void _Write_System\u002ENullable\u00601\u003CUnityEngine\u002EQuaternion\u003E(
    NetworkWriter writer,
    Quaternion? value)
  {
    writer.WriteNullable<Quaternion>(value);
  }

  public static void _Write_System\u002ENullable\u00601\u003CSystem\u002EBoolean\u003E(
    NetworkWriter writer,
    bool? value)
  {
    writer.WriteNullable<bool>(value);
  }

  public static SpawnMessage _Read_Mirage\u002ESpawnMessage(NetworkReader reader)
  {
    return new SpawnMessage()
    {
      NetId = reader.ReadPackedUInt32(),
      IsLocalPlayer = reader.ReadBooleanExtension(),
      IsOwner = reader.ReadBooleanExtension(),
      SceneId = GeneratedNetworkCode._Read_System\u002ENullable\u00601\u003CSystem\u002EUInt64\u003E(reader),
      PrefabHash = GeneratedNetworkCode._Read_System\u002ENullable\u00601\u003CSystem\u002EInt32\u003E(reader),
      SpawnValues = GeneratedNetworkCode._Read_Mirage\u002ESpawnValues(reader),
      Payload = reader.ReadBytesAndSizeSegment()
    };
  }

  public static ulong? _Read_System\u002ENullable\u00601\u003CSystem\u002EUInt64\u003E(
    NetworkReader reader)
  {
    return reader.ReadNullable<ulong>();
  }

  public static int? _Read_System\u002ENullable\u00601\u003CSystem\u002EInt32\u003E(
    NetworkReader reader)
  {
    return reader.ReadNullable<int>();
  }

  public static SpawnValues _Read_Mirage\u002ESpawnValues(NetworkReader reader)
  {
    return new SpawnValues()
    {
      Position = GeneratedNetworkCode._Read_System\u002ENullable\u00601\u003CUnityEngine\u002EVector3\u003E(reader),
      Rotation = GeneratedNetworkCode._Read_System\u002ENullable\u00601\u003CUnityEngine\u002EQuaternion\u003E(reader),
      Scale = GeneratedNetworkCode._Read_System\u002ENullable\u00601\u003CUnityEngine\u002EVector3\u003E(reader),
      Name = reader.ReadString(),
      SelfActive = GeneratedNetworkCode._Read_System\u002ENullable\u00601\u003CSystem\u002EBoolean\u003E(reader)
    };
  }

  public static Vector3? _Read_System\u002ENullable\u00601\u003CUnityEngine\u002EVector3\u003E(
    NetworkReader reader)
  {
    return reader.ReadNullable<Vector3>();
  }

  public static Quaternion? _Read_System\u002ENullable\u00601\u003CUnityEngine\u002EQuaternion\u003E(
    NetworkReader reader)
  {
    return reader.ReadNullable<Quaternion>();
  }

  public static bool? _Read_System\u002ENullable\u00601\u003CSystem\u002EBoolean\u003E(
    NetworkReader reader)
  {
    return reader.ReadNullable<bool>();
  }

  public static void _Write_Mirage\u002ERemoveAuthorityMessage(
    NetworkWriter writer,
    RemoveAuthorityMessage value)
  {
    writer.WritePackedUInt32(value.NetId);
  }

  public static RemoveAuthorityMessage _Read_Mirage\u002ERemoveAuthorityMessage(NetworkReader reader)
  {
    return new RemoveAuthorityMessage()
    {
      NetId = reader.ReadPackedUInt32()
    };
  }

  public static void _Write_Mirage\u002ERemoveCharacterMessage(
    NetworkWriter writer,
    RemoveCharacterMessage value)
  {
    writer.WriteBooleanExtension(value.KeepAuthority);
  }

  public static RemoveCharacterMessage _Read_Mirage\u002ERemoveCharacterMessage(NetworkReader reader)
  {
    return new RemoveCharacterMessage()
    {
      KeepAuthority = reader.ReadBooleanExtension()
    };
  }

  public static void _Write_Mirage\u002EObjectDestroyMessage(
    NetworkWriter writer,
    ObjectDestroyMessage value)
  {
    writer.WritePackedUInt32(value.NetId);
  }

  public static ObjectDestroyMessage _Read_Mirage\u002EObjectDestroyMessage(NetworkReader reader)
  {
    return new ObjectDestroyMessage()
    {
      NetId = reader.ReadPackedUInt32()
    };
  }

  public static void _Write_Mirage\u002EObjectHideMessage(
    NetworkWriter writer,
    ObjectHideMessage value)
  {
    writer.WritePackedUInt32(value.NetId);
  }

  public static ObjectHideMessage _Read_Mirage\u002EObjectHideMessage(NetworkReader reader)
  {
    return new ObjectHideMessage()
    {
      NetId = reader.ReadPackedUInt32()
    };
  }

  public static void _Write_Mirage\u002EUpdateVarsMessage(
    NetworkWriter writer,
    UpdateVarsMessage value)
  {
    writer.WritePackedUInt32(value.NetId);
    writer.WriteBytesAndSizeSegment(value.Payload);
  }

  public static UpdateVarsMessage _Read_Mirage\u002EUpdateVarsMessage(NetworkReader reader)
  {
    return new UpdateVarsMessage()
    {
      NetId = reader.ReadPackedUInt32(),
      Payload = reader.ReadBytesAndSizeSegment()
    };
  }

  public static void _Write_Mirage\u002ENetworkPingMessage(
    NetworkWriter writer,
    NetworkPingMessage value)
  {
    writer.WriteDoubleConverter(value.ClientTime);
  }

  public static NetworkPingMessage _Read_Mirage\u002ENetworkPingMessage(NetworkReader reader)
  {
    return new NetworkPingMessage()
    {
      ClientTime = reader.ReadDoubleConverter()
    };
  }

  public static void _Write_Mirage\u002ENetworkPongMessage(
    NetworkWriter writer,
    NetworkPongMessage value)
  {
    writer.WriteDoubleConverter(value.ClientTime);
    writer.WriteDoubleConverter(value.ServerTime);
  }

  public static NetworkPongMessage _Read_Mirage\u002ENetworkPongMessage(NetworkReader reader)
  {
    return new NetworkPongMessage()
    {
      ClientTime = reader.ReadDoubleConverter(),
      ServerTime = reader.ReadDoubleConverter()
    };
  }

  public static void _Write_Mirage\u002ERemoteCalls\u002ERpcMessage(
    NetworkWriter writer,
    RpcMessage value)
  {
    writer.WritePackedUInt32(value.NetId);
    writer.WritePackedInt32(value.FunctionIndex);
    writer.WriteBytesAndSizeSegment(value.Payload);
  }

  public static RpcMessage _Read_Mirage\u002ERemoteCalls\u002ERpcMessage(NetworkReader reader)
  {
    return new RpcMessage()
    {
      NetId = reader.ReadPackedUInt32(),
      FunctionIndex = reader.ReadPackedInt32(),
      Payload = reader.ReadBytesAndSizeSegment()
    };
  }

  public static void _Write_Mirage\u002ERemoteCalls\u002ERpcWithReplyMessage(
    NetworkWriter writer,
    RpcWithReplyMessage value)
  {
    writer.WritePackedUInt32(value.NetId);
    writer.WritePackedInt32(value.FunctionIndex);
    writer.WritePackedInt32(value.ReplyId);
    writer.WriteBytesAndSizeSegment(value.Payload);
  }

  public static RpcWithReplyMessage _Read_Mirage\u002ERemoteCalls\u002ERpcWithReplyMessage(
    NetworkReader reader)
  {
    return new RpcWithReplyMessage()
    {
      NetId = reader.ReadPackedUInt32(),
      FunctionIndex = reader.ReadPackedInt32(),
      ReplyId = reader.ReadPackedInt32(),
      Payload = reader.ReadBytesAndSizeSegment()
    };
  }

  public static void _Write_Mirage\u002ERemoteCalls\u002ERpcReply(
    NetworkWriter writer,
    RpcReply value)
  {
    writer.WritePackedInt32(value.ReplyId);
    writer.WriteBooleanExtension(value.Success);
    writer.WriteBytesAndSizeSegment(value.Payload);
  }

  public static RpcReply _Read_Mirage\u002ERemoteCalls\u002ERpcReply(NetworkReader reader)
  {
    return new RpcReply()
    {
      ReplyId = reader.ReadPackedInt32(),
      Success = reader.ReadBooleanExtension(),
      Payload = reader.ReadBytesAndSizeSegment()
    };
  }

  public static void _Write_Mirage\u002EAuthentication\u002EAuthMessage(
    NetworkWriter writer,
    Mirage.Authentication.AuthMessage value)
  {
    writer.WriteBytesAndSizeSegment(value.Payload);
  }

  public static Mirage.Authentication.AuthMessage _Read_Mirage\u002EAuthentication\u002EAuthMessage(
    NetworkReader reader)
  {
    return new Mirage.Authentication.AuthMessage()
    {
      Payload = reader.ReadBytesAndSizeSegment()
    };
  }

  public static void _Write_Mirage\u002EAuthentication\u002EAuthSuccessMessage(
    NetworkWriter writer,
    AuthSuccessMessage value)
  {
    writer.WriteString(value.AuthenticatorName);
  }

  public static AuthSuccessMessage _Read_Mirage\u002EAuthentication\u002EAuthSuccessMessage(
    NetworkReader reader)
  {
    return new AuthSuccessMessage()
    {
      AuthenticatorName = reader.ReadString()
    };
  }

  public static NetworkMission.SyncMissionPart _Read_NetworkMission\u002FSyncMissionPart(
    NetworkReader reader)
  {
    return new NetworkMission.SyncMissionPart()
    {
      Bytes = reader.ReadBytesAndSizeSegment()
    };
  }

  public static void _Write_NetworkMission\u002FSyncMissionPart(
    NetworkWriter writer,
    NetworkMission.SyncMissionPart value)
  {
    writer.WriteBytesAndSizeSegment(value.Bytes);
  }

  public static NetworkMission.SyncMissionHeader _Read_NetworkMission\u002FSyncMissionHeader(
    NetworkReader reader)
  {
    return new NetworkMission.SyncMissionHeader()
    {
      Name = reader.ReadString(),
      State = GeneratedNetworkCode._Read_NetworkMission\u002FState(reader),
      JsonVersion = reader.ReadPackedInt32(),
      missionSettings = GeneratedNetworkCode._Read_NuclearOption\u002ESavedMission\u002EMissionSettings(reader),
      environment = GeneratedNetworkCode._Read_NuclearOption\u002ESavedMission\u002EMissionEnvironment(reader)
    };
  }

  public static NetworkMission.State _Read_NetworkMission\u002FState(NetworkReader reader)
  {
    return (NetworkMission.State) reader.ReadPackedInt32();
  }

  public static MissionSettings _Read_NuclearOption\u002ESavedMission\u002EMissionSettings(
    NetworkReader reader)
  {
    if (!reader.ReadBooleanExtension())
      return (MissionSettings) null;
    return new MissionSettings()
    {
      description = reader.ReadString(),
      Tags = GeneratedNetworkCode._Read_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002EMissionTag\u003E(reader),
      playerMode = GeneratedNetworkCode._Read_NuclearOption\u002ESavedMission\u002EPlayerMode(reader),
      allowRespawn = reader.ReadBooleanExtension(),
      playerStartingRank = reader.ReadPackedInt32(),
      rankMultiplier = reader.ReadSingleConverter(),
      successfulSortieBonus = reader.ReadSingleConverter(),
      nuclearEscalationThreshold = reader.ReadSingleConverter(),
      strategicEscalationThreshold = reader.ReadSingleConverter(),
      minRankTacticalWarhead = reader.ReadPackedInt32(),
      minRankStrategicWarhead = reader.ReadPackedInt32(),
      cameraStartPosition = GeneratedNetworkCode._Read_NuclearOption\u002ESavedMission\u002EObjectiveV2\u002EOverride\u00601\u003CNuclearOption\u002ESavedMission\u002EPositionRotation\u003E(reader),
      missionRoads = GeneratedNetworkCode._Read_RoadPathfinding\u002ERoadNetwork(reader),
      wrecksMaxNumber = reader.ReadPackedInt32(),
      wrecksDecayTime = reader.ReadSingleConverter()
    };
  }

  public static MissionTag _Read_NuclearOption\u002ESavedMission\u002EMissionTag(
    NetworkReader reader)
  {
    return new MissionTag()
    {
      Tag = reader.ReadString(),
      Color = reader.ReadColor(),
      SortOrder = reader.ReadPackedInt32()
    };
  }

  public static List<MissionTag> _Read_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002EMissionTag\u003E(
    NetworkReader reader)
  {
    return reader.ReadList<MissionTag>();
  }

  public static PlayerMode _Read_NuclearOption\u002ESavedMission\u002EPlayerMode(
    NetworkReader reader)
  {
    return (PlayerMode) reader.ReadPackedInt32();
  }

  public static Override<PositionRotation> _Read_NuclearOption\u002ESavedMission\u002EObjectiveV2\u002EOverride\u00601\u003CNuclearOption\u002ESavedMission\u002EPositionRotation\u003E(
    NetworkReader reader)
  {
    return new Override<PositionRotation>()
    {
      IsOverride = reader.ReadBooleanExtension(),
      Value = GeneratedNetworkCode._Read_NuclearOption\u002ESavedMission\u002EPositionRotation(reader)
    };
  }

  public static PositionRotation _Read_NuclearOption\u002ESavedMission\u002EPositionRotation(
    NetworkReader reader)
  {
    return new PositionRotation()
    {
      Position = reader.ReadGlobalPosition(),
      Rotation = reader.ReadQuaternion()
    };
  }

  public static RoadNetwork _Read_RoadPathfinding\u002ERoadNetwork(NetworkReader reader)
  {
    if (!reader.ReadBooleanExtension())
      return (RoadNetwork) null;
    return new RoadNetwork()
    {
      roads = GeneratedNetworkCode._Read_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CRoadPathfinding\u002ERoad\u003E(reader)
    };
  }

  public static Road _Read_RoadPathfinding\u002ERoad(NetworkReader reader)
  {
    if (!reader.ReadBooleanExtension())
      return (Road) null;
    return new Road()
    {
      points = GeneratedNetworkCode._Read_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CGlobalPosition\u003E(reader),
      length = reader.ReadSingleConverter()
    };
  }

  public static List<GlobalPosition> _Read_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CGlobalPosition\u003E(
    NetworkReader reader)
  {
    return reader.ReadList<GlobalPosition>();
  }

  public static List<Road> _Read_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CRoadPathfinding\u002ERoad\u003E(
    NetworkReader reader)
  {
    return reader.ReadList<Road>();
  }

  public static MissionEnvironment _Read_NuclearOption\u002ESavedMission\u002EMissionEnvironment(
    NetworkReader reader)
  {
    if (!reader.ReadBooleanExtension())
      return (MissionEnvironment) null;
    return new MissionEnvironment()
    {
      timeOfDay = reader.ReadSingleConverter(),
      timeFactor = reader.ReadSingleConverter(),
      weatherIntensity = reader.ReadSingleConverter(),
      cloudAltitude = reader.ReadSingleConverter(),
      windSpeed = reader.ReadSingleConverter(),
      windTurbulence = reader.ReadSingleConverter(),
      windHeading = reader.ReadSingleConverter(),
      windRandomHeading = reader.ReadSingleConverter(),
      moonPhase = reader.ReadSingleConverter()
    };
  }

  public static void _Write_NetworkMission\u002FSyncMissionHeader(
    NetworkWriter writer,
    NetworkMission.SyncMissionHeader value)
  {
    writer.WriteString(value.Name);
    GeneratedNetworkCode._Write_NetworkMission\u002FState(writer, value.State);
    writer.WritePackedInt32(value.JsonVersion);
    GeneratedNetworkCode._Write_NuclearOption\u002ESavedMission\u002EMissionSettings(writer, value.missionSettings);
    GeneratedNetworkCode._Write_NuclearOption\u002ESavedMission\u002EMissionEnvironment(writer, value.environment);
  }

  public static void _Write_NetworkMission\u002FState(
    NetworkWriter writer,
    NetworkMission.State value)
  {
    writer.WritePackedInt32((int) value);
  }

  public static void _Write_NuclearOption\u002ESavedMission\u002EMissionSettings(
    NetworkWriter writer,
    MissionSettings value)
  {
    if (value == null)
    {
      writer.WriteBooleanExtension(false);
    }
    else
    {
      writer.WriteBooleanExtension(true);
      writer.WriteString(value.description);
      GeneratedNetworkCode._Write_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002EMissionTag\u003E(writer, value.Tags);
      GeneratedNetworkCode._Write_NuclearOption\u002ESavedMission\u002EPlayerMode(writer, value.playerMode);
      writer.WriteBooleanExtension(value.allowRespawn);
      writer.WritePackedInt32(value.playerStartingRank);
      writer.WriteSingleConverter(value.rankMultiplier);
      writer.WriteSingleConverter(value.successfulSortieBonus);
      writer.WriteSingleConverter(value.nuclearEscalationThreshold);
      writer.WriteSingleConverter(value.strategicEscalationThreshold);
      writer.WritePackedInt32(value.minRankTacticalWarhead);
      writer.WritePackedInt32(value.minRankStrategicWarhead);
      GeneratedNetworkCode._Write_NuclearOption\u002ESavedMission\u002EObjectiveV2\u002EOverride\u00601\u003CNuclearOption\u002ESavedMission\u002EPositionRotation\u003E(writer, value.cameraStartPosition);
      GeneratedNetworkCode._Write_RoadPathfinding\u002ERoadNetwork(writer, value.missionRoads);
      writer.WritePackedInt32(value.wrecksMaxNumber);
      writer.WriteSingleConverter(value.wrecksDecayTime);
    }
  }

  public static void _Write_NuclearOption\u002ESavedMission\u002EMissionTag(
    NetworkWriter writer,
    MissionTag value)
  {
    writer.WriteString(value.Tag);
    writer.WriteColor(value.Color);
    writer.WritePackedInt32(value.SortOrder);
  }

  public static void _Write_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002EMissionTag\u003E(
    NetworkWriter writer,
    List<MissionTag> value)
  {
    writer.WriteList<MissionTag>(value);
  }

  public static void _Write_NuclearOption\u002ESavedMission\u002EPlayerMode(
    NetworkWriter writer,
    PlayerMode value)
  {
    writer.WritePackedInt32((int) value);
  }

  public static void _Write_NuclearOption\u002ESavedMission\u002EObjectiveV2\u002EOverride\u00601\u003CNuclearOption\u002ESavedMission\u002EPositionRotation\u003E(
    NetworkWriter writer,
    Override<PositionRotation> value)
  {
    writer.WriteBooleanExtension(value.IsOverride);
    GeneratedNetworkCode._Write_NuclearOption\u002ESavedMission\u002EPositionRotation(writer, value.Value);
  }

  public static void _Write_NuclearOption\u002ESavedMission\u002EPositionRotation(
    NetworkWriter writer,
    PositionRotation value)
  {
    writer.WriteGlobalPosition(value.Position);
    writer.WriteQuaternion(value.Rotation);
  }

  public static void _Write_RoadPathfinding\u002ERoadNetwork(
    NetworkWriter writer,
    RoadNetwork value)
  {
    if (value == null)
    {
      writer.WriteBooleanExtension(false);
    }
    else
    {
      writer.WriteBooleanExtension(true);
      GeneratedNetworkCode._Write_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CRoadPathfinding\u002ERoad\u003E(writer, value.roads);
    }
  }

  public static void _Write_RoadPathfinding\u002ERoad(NetworkWriter writer, Road value)
  {
    if (value == null)
    {
      writer.WriteBooleanExtension(false);
    }
    else
    {
      writer.WriteBooleanExtension(true);
      GeneratedNetworkCode._Write_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CGlobalPosition\u003E(writer, value.points);
      writer.WriteSingleConverter(value.length);
    }
  }

  public static void _Write_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CGlobalPosition\u003E(
    NetworkWriter writer,
    List<GlobalPosition> value)
  {
    writer.WriteList<GlobalPosition>(value);
  }

  public static void _Write_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CRoadPathfinding\u002ERoad\u003E(
    NetworkWriter writer,
    List<Road> value)
  {
    writer.WriteList<Road>(value);
  }

  public static void _Write_NuclearOption\u002ESavedMission\u002EMissionEnvironment(
    NetworkWriter writer,
    MissionEnvironment value)
  {
    if (value == null)
    {
      writer.WriteBooleanExtension(false);
    }
    else
    {
      writer.WriteBooleanExtension(true);
      writer.WriteSingleConverter(value.timeOfDay);
      writer.WriteSingleConverter(value.timeFactor);
      writer.WriteSingleConverter(value.weatherIntensity);
      writer.WriteSingleConverter(value.cloudAltitude);
      writer.WriteSingleConverter(value.windSpeed);
      writer.WriteSingleConverter(value.windTurbulence);
      writer.WriteSingleConverter(value.windHeading);
      writer.WriteSingleConverter(value.windRandomHeading);
      writer.WriteSingleConverter(value.moonPhase);
    }
  }

  public static NetworkMission.SyncMissionFooter _Read_NetworkMission\u002FSyncMissionFooter(
    NetworkReader reader)
  {
    return new NetworkMission.SyncMissionFooter()
    {
      Name = reader.ReadString()
    };
  }

  public static void _Write_NetworkMission\u002FSyncMissionFooter(
    NetworkWriter writer,
    NetworkMission.SyncMissionFooter value)
  {
    writer.WriteString(value.Name);
  }

  public static NetworkMission.SyncMission _Read_NetworkMission\u002FSyncMission(
    NetworkReader reader)
  {
    return new NetworkMission.SyncMission()
    {
      Name = reader.ReadString(),
      Mission = GeneratedNetworkCode._Read_NuclearOption\u002ESavedMission\u002EMission(reader)
    };
  }

  public static Mission _Read_NuclearOption\u002ESavedMission\u002EMission(NetworkReader reader)
  {
    if (!reader.ReadBooleanExtension())
      return (Mission) null;
    return new Mission()
    {
      JsonVersion = reader.ReadPackedInt32(),
      WorkshopId = reader.ReadPackedUInt64(),
      MapKey = GeneratedNetworkCode._Read_NuclearOption\u002ESceneLoading\u002EMapKey(reader),
      missionSettings = GeneratedNetworkCode._Read_NuclearOption\u002ESavedMission\u002EMissionSettings(reader),
      environment = GeneratedNetworkCode._Read_NuclearOption\u002ESavedMission\u002EMissionEnvironment(reader),
      aircraft = GeneratedNetworkCode._Read_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002ESavedAircraft\u003E(reader),
      vehicles = GeneratedNetworkCode._Read_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002ESavedVehicle\u003E(reader),
      ships = GeneratedNetworkCode._Read_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002ESavedShip\u003E(reader),
      buildings = GeneratedNetworkCode._Read_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002ESavedBuilding\u003E(reader),
      scenery = GeneratedNetworkCode._Read_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002ESavedScenery\u003E(reader),
      containers = GeneratedNetworkCode._Read_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002ESavedContainer\u003E(reader),
      missiles = GeneratedNetworkCode._Read_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002ESavedMissile\u003E(reader),
      pilots = GeneratedNetworkCode._Read_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002ESavedPilot\u003E(reader),
      factions = GeneratedNetworkCode._Read_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002EMissionFaction\u003E(reader),
      airbases = GeneratedNetworkCode._Read_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002ESavedAirbase\u003E(reader),
      unitInventories = GeneratedNetworkCode._Read_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002EUnitInventory\u003E(reader),
      objectives = GeneratedNetworkCode._Read_NuclearOption\u002ESavedMission\u002EObjectiveV2\u002ESavedMissionObjectives(reader)
    };
  }

  public static MapKey _Read_NuclearOption\u002ESceneLoading\u002EMapKey(NetworkReader reader)
  {
    return new MapKey()
    {
      Type = GeneratedNetworkCode._Read_NuclearOption\u002ESceneLoading\u002EMapKey\u002FKeyType(reader),
      TypeName = reader.ReadString(),
      Path = reader.ReadString()
    };
  }

  public static MapKey.KeyType _Read_NuclearOption\u002ESceneLoading\u002EMapKey\u002FKeyType(
    NetworkReader reader)
  {
    return (MapKey.KeyType) reader.ReadByteExtension();
  }

  public static SavedAircraft _Read_NuclearOption\u002ESavedMission\u002ESavedAircraft(
    NetworkReader reader)
  {
    if (!reader.ReadBooleanExtension())
      return (SavedAircraft) null;
    return new SavedAircraft()
    {
      playerControlled = reader.ReadBooleanExtension(),
      playerControlledPriority = reader.ReadPackedInt32(),
      loadout = GeneratedNetworkCode._Read_NuclearOption\u002ESavedMission\u002ELoadoutOld(reader),
      savedLoadout = GeneratedNetworkCode._Read_NuclearOption\u002ESavedMission\u002ESavedLoadout(reader),
      livery = reader.ReadPackedInt32(),
      liveryType = GeneratedNetworkCode._Read_LiveryKey\u002FKeyType(reader),
      liveryName = reader.ReadString(),
      fuel = reader.ReadSingleConverter(),
      skill = reader.ReadSingleConverter(),
      bravery = reader.ReadSingleConverter(),
      startingSpeed = reader.ReadSingleConverter(),
      type = reader.ReadString(),
      faction = reader.ReadString(),
      UniqueName = reader.ReadString(),
      globalPosition = reader.ReadGlobalPosition(),
      rotation = reader.ReadQuaternion(),
      CaptureStrength = GeneratedNetworkCode._Read_NuclearOption\u002ESavedMission\u002EObjectiveV2\u002EOverride\u00601\u003CSystem\u002ESingle\u003E(reader),
      CaptureDefense = GeneratedNetworkCode._Read_NuclearOption\u002ESavedMission\u002EObjectiveV2\u002EOverride\u00601\u003CSystem\u002ESingle\u003E(reader),
      unitCustomID = reader.ReadString(),
      spawnTiming = reader.ReadString()
    };
  }

  public static LoadoutOld _Read_NuclearOption\u002ESavedMission\u002ELoadoutOld(
    NetworkReader reader)
  {
    return new LoadoutOld()
    {
      weaponSelections = GeneratedNetworkCode._Read_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CSystem\u002EByte\u003E(reader)
    };
  }

  public static List<byte> _Read_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CSystem\u002EByte\u003E(
    NetworkReader reader)
  {
    return reader.ReadList<byte>();
  }

  public static SavedLoadout _Read_NuclearOption\u002ESavedMission\u002ESavedLoadout(
    NetworkReader reader)
  {
    if (!reader.ReadBooleanExtension())
      return (SavedLoadout) null;
    return new SavedLoadout()
    {
      Selected = GeneratedNetworkCode._Read_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002ESavedLoadout\u002FSelectedMount\u003E(reader)
    };
  }

  public static SavedLoadout.SelectedMount _Read_NuclearOption\u002ESavedMission\u002ESavedLoadout\u002FSelectedMount(
    NetworkReader reader)
  {
    return new SavedLoadout.SelectedMount()
    {
      Key = reader.ReadString()
    };
  }

  public static List<SavedLoadout.SelectedMount> _Read_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002ESavedLoadout\u002FSelectedMount\u003E(
    NetworkReader reader)
  {
    return reader.ReadList<SavedLoadout.SelectedMount>();
  }

  public static LiveryKey.KeyType _Read_LiveryKey\u002FKeyType(NetworkReader reader)
  {
    return (LiveryKey.KeyType) reader.ReadByteExtension();
  }

  public static Override<float> _Read_NuclearOption\u002ESavedMission\u002EObjectiveV2\u002EOverride\u00601\u003CSystem\u002ESingle\u003E(
    NetworkReader reader)
  {
    return new Override<float>()
    {
      IsOverride = reader.ReadBooleanExtension(),
      Value = reader.ReadSingleConverter()
    };
  }

  public static List<SavedAircraft> _Read_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002ESavedAircraft\u003E(
    NetworkReader reader)
  {
    return reader.ReadList<SavedAircraft>();
  }

  public static SavedVehicle _Read_NuclearOption\u002ESavedMission\u002ESavedVehicle(
    NetworkReader reader)
  {
    if (!reader.ReadBooleanExtension())
      return (SavedVehicle) null;
    return new SavedVehicle()
    {
      holdPosition = reader.ReadBooleanExtension(),
      skill = reader.ReadSingleConverter(),
      waypoints = GeneratedNetworkCode._Read_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002EVehicleWaypoint\u003E(reader),
      type = reader.ReadString(),
      faction = reader.ReadString(),
      UniqueName = reader.ReadString(),
      globalPosition = reader.ReadGlobalPosition(),
      rotation = reader.ReadQuaternion(),
      CaptureStrength = GeneratedNetworkCode._Read_NuclearOption\u002ESavedMission\u002EObjectiveV2\u002EOverride\u00601\u003CSystem\u002ESingle\u003E(reader),
      CaptureDefense = GeneratedNetworkCode._Read_NuclearOption\u002ESavedMission\u002EObjectiveV2\u002EOverride\u00601\u003CSystem\u002ESingle\u003E(reader),
      unitCustomID = reader.ReadString(),
      spawnTiming = reader.ReadString()
    };
  }

  public static VehicleWaypoint _Read_NuclearOption\u002ESavedMission\u002EVehicleWaypoint(
    NetworkReader reader)
  {
    if (!reader.ReadBooleanExtension())
      return (VehicleWaypoint) null;
    return new VehicleWaypoint()
    {
      position = reader.ReadGlobalPosition(),
      objective = reader.ReadString()
    };
  }

  public static List<VehicleWaypoint> _Read_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002EVehicleWaypoint\u003E(
    NetworkReader reader)
  {
    return reader.ReadList<VehicleWaypoint>();
  }

  public static List<SavedVehicle> _Read_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002ESavedVehicle\u003E(
    NetworkReader reader)
  {
    return reader.ReadList<SavedVehicle>();
  }

  public static SavedShip _Read_NuclearOption\u002ESavedMission\u002ESavedShip(NetworkReader reader)
  {
    if (!reader.ReadBooleanExtension())
      return (SavedShip) null;
    return new SavedShip()
    {
      holdPosition = reader.ReadBooleanExtension(),
      skill = reader.ReadSingleConverter(),
      waypoints = GeneratedNetworkCode._Read_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002EVehicleWaypoint\u003E(reader),
      type = reader.ReadString(),
      faction = reader.ReadString(),
      UniqueName = reader.ReadString(),
      globalPosition = reader.ReadGlobalPosition(),
      rotation = reader.ReadQuaternion(),
      CaptureStrength = GeneratedNetworkCode._Read_NuclearOption\u002ESavedMission\u002EObjectiveV2\u002EOverride\u00601\u003CSystem\u002ESingle\u003E(reader),
      CaptureDefense = GeneratedNetworkCode._Read_NuclearOption\u002ESavedMission\u002EObjectiveV2\u002EOverride\u00601\u003CSystem\u002ESingle\u003E(reader),
      unitCustomID = reader.ReadString(),
      spawnTiming = reader.ReadString()
    };
  }

  public static List<SavedShip> _Read_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002ESavedShip\u003E(
    NetworkReader reader)
  {
    return reader.ReadList<SavedShip>();
  }

  public static SavedBuilding _Read_NuclearOption\u002ESavedMission\u002ESavedBuilding(
    NetworkReader reader)
  {
    if (!reader.ReadBooleanExtension())
      return (SavedBuilding) null;
    return new SavedBuilding()
    {
      capturable = reader.ReadBooleanExtension(),
      Airbase = reader.ReadString(),
      factoryOptions = GeneratedNetworkCode._Read_NuclearOption\u002ESavedMission\u002ESavedBuilding\u002FFactoryOptions(reader),
      placementOffset = reader.ReadSingleConverter(),
      type = reader.ReadString(),
      faction = reader.ReadString(),
      UniqueName = reader.ReadString(),
      globalPosition = reader.ReadGlobalPosition(),
      rotation = reader.ReadQuaternion(),
      CaptureStrength = GeneratedNetworkCode._Read_NuclearOption\u002ESavedMission\u002EObjectiveV2\u002EOverride\u00601\u003CSystem\u002ESingle\u003E(reader),
      CaptureDefense = GeneratedNetworkCode._Read_NuclearOption\u002ESavedMission\u002EObjectiveV2\u002EOverride\u00601\u003CSystem\u002ESingle\u003E(reader),
      unitCustomID = reader.ReadString(),
      spawnTiming = reader.ReadString()
    };
  }

  public static SavedBuilding.FactoryOptions _Read_NuclearOption\u002ESavedMission\u002ESavedBuilding\u002FFactoryOptions(
    NetworkReader reader)
  {
    if (!reader.ReadBooleanExtension())
      return (SavedBuilding.FactoryOptions) null;
    return new SavedBuilding.FactoryOptions()
    {
      productionType = reader.ReadString(),
      productionTime = reader.ReadSingleConverter()
    };
  }

  public static List<SavedBuilding> _Read_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002ESavedBuilding\u003E(
    NetworkReader reader)
  {
    return reader.ReadList<SavedBuilding>();
  }

  public static SavedScenery _Read_NuclearOption\u002ESavedMission\u002ESavedScenery(
    NetworkReader reader)
  {
    if (!reader.ReadBooleanExtension())
      return (SavedScenery) null;
    return new SavedScenery()
    {
      indestructible = reader.ReadBooleanExtension(),
      type = reader.ReadString(),
      faction = reader.ReadString(),
      UniqueName = reader.ReadString(),
      globalPosition = reader.ReadGlobalPosition(),
      rotation = reader.ReadQuaternion(),
      CaptureStrength = GeneratedNetworkCode._Read_NuclearOption\u002ESavedMission\u002EObjectiveV2\u002EOverride\u00601\u003CSystem\u002ESingle\u003E(reader),
      CaptureDefense = GeneratedNetworkCode._Read_NuclearOption\u002ESavedMission\u002EObjectiveV2\u002EOverride\u00601\u003CSystem\u002ESingle\u003E(reader),
      unitCustomID = reader.ReadString(),
      spawnTiming = reader.ReadString()
    };
  }

  public static List<SavedScenery> _Read_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002ESavedScenery\u003E(
    NetworkReader reader)
  {
    return reader.ReadList<SavedScenery>();
  }

  public static SavedContainer _Read_NuclearOption\u002ESavedMission\u002ESavedContainer(
    NetworkReader reader)
  {
    if (!reader.ReadBooleanExtension())
      return (SavedContainer) null;
    return new SavedContainer()
    {
      type = reader.ReadString(),
      faction = reader.ReadString(),
      UniqueName = reader.ReadString(),
      globalPosition = reader.ReadGlobalPosition(),
      rotation = reader.ReadQuaternion(),
      CaptureStrength = GeneratedNetworkCode._Read_NuclearOption\u002ESavedMission\u002EObjectiveV2\u002EOverride\u00601\u003CSystem\u002ESingle\u003E(reader),
      CaptureDefense = GeneratedNetworkCode._Read_NuclearOption\u002ESavedMission\u002EObjectiveV2\u002EOverride\u00601\u003CSystem\u002ESingle\u003E(reader),
      unitCustomID = reader.ReadString(),
      spawnTiming = reader.ReadString()
    };
  }

  public static List<SavedContainer> _Read_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002ESavedContainer\u003E(
    NetworkReader reader)
  {
    return reader.ReadList<SavedContainer>();
  }

  public static SavedMissile _Read_NuclearOption\u002ESavedMission\u002ESavedMissile(
    NetworkReader reader)
  {
    if (!reader.ReadBooleanExtension())
      return (SavedMissile) null;
    return new SavedMissile()
    {
      startingSpeed = reader.ReadSingleConverter(),
      type = reader.ReadString(),
      faction = reader.ReadString(),
      UniqueName = reader.ReadString(),
      globalPosition = reader.ReadGlobalPosition(),
      rotation = reader.ReadQuaternion(),
      CaptureStrength = GeneratedNetworkCode._Read_NuclearOption\u002ESavedMission\u002EObjectiveV2\u002EOverride\u00601\u003CSystem\u002ESingle\u003E(reader),
      CaptureDefense = GeneratedNetworkCode._Read_NuclearOption\u002ESavedMission\u002EObjectiveV2\u002EOverride\u00601\u003CSystem\u002ESingle\u003E(reader),
      unitCustomID = reader.ReadString(),
      spawnTiming = reader.ReadString()
    };
  }

  public static List<SavedMissile> _Read_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002ESavedMissile\u003E(
    NetworkReader reader)
  {
    return reader.ReadList<SavedMissile>();
  }

  public static SavedPilot _Read_NuclearOption\u002ESavedMission\u002ESavedPilot(
    NetworkReader reader)
  {
    if (!reader.ReadBooleanExtension())
      return (SavedPilot) null;
    return new SavedPilot()
    {
      type = reader.ReadString(),
      faction = reader.ReadString(),
      UniqueName = reader.ReadString(),
      globalPosition = reader.ReadGlobalPosition(),
      rotation = reader.ReadQuaternion(),
      CaptureStrength = GeneratedNetworkCode._Read_NuclearOption\u002ESavedMission\u002EObjectiveV2\u002EOverride\u00601\u003CSystem\u002ESingle\u003E(reader),
      CaptureDefense = GeneratedNetworkCode._Read_NuclearOption\u002ESavedMission\u002EObjectiveV2\u002EOverride\u00601\u003CSystem\u002ESingle\u003E(reader),
      unitCustomID = reader.ReadString(),
      spawnTiming = reader.ReadString()
    };
  }

  public static List<SavedPilot> _Read_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002ESavedPilot\u003E(
    NetworkReader reader)
  {
    return reader.ReadList<SavedPilot>();
  }

  public static MissionFaction _Read_NuclearOption\u002ESavedMission\u002EMissionFaction(
    NetworkReader reader)
  {
    if (!reader.ReadBooleanExtension())
      return (MissionFaction) null;
    return new MissionFaction()
    {
      factionName = reader.ReadString(),
      preventJoin = reader.ReadBooleanExtension(),
      preventDonation = reader.ReadBooleanExtension(),
      supplies = GeneratedNetworkCode._Read_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002EFactionSupply\u003E(reader),
      startingBalance = reader.ReadSingleConverter(),
      playerJoinAllowance = reader.ReadSingleConverter(),
      playerTaxRate = reader.ReadSingleConverter(),
      regularIncome = reader.ReadSingleConverter(),
      excessFundsDistributePercent = reader.ReadSingleConverter(),
      killReward = reader.ReadSingleConverter(),
      startingWarheads = reader.ReadPackedInt32(),
      reserveWarheads = reader.ReadPackedInt32(),
      reserveAirframes = reader.ReadPackedInt32(),
      extraReservesPerPlayer = reader.ReadPackedInt32(),
      AIAircraftLimit = reader.ReadPackedInt32(),
      reduceAIPerFriendlyPlayer = reader.ReadSingleConverter(),
      addAIPerEnemyPlayer = reader.ReadSingleConverter(),
      objectives = GeneratedNetworkCode._Read_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002EMissionObjective\u003E(reader),
      restrictions = GeneratedNetworkCode._Read_NuclearOption\u002ESavedMission\u002ERestrictions(reader),
      cameraStartPosition = GeneratedNetworkCode._Read_NuclearOption\u002ESavedMission\u002EObjectiveV2\u002EOverride\u00601\u003CNuclearOption\u002ESavedMission\u002EPositionRotation\u003E(reader)
    };
  }

  public static FactionSupply _Read_NuclearOption\u002ESavedMission\u002EFactionSupply(
    NetworkReader reader)
  {
    if (!reader.ReadBooleanExtension())
      return (FactionSupply) null;
    return new FactionSupply()
    {
      unitType = reader.ReadString(),
      count = reader.ReadPackedInt32()
    };
  }

  public static List<FactionSupply> _Read_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002EFactionSupply\u003E(
    NetworkReader reader)
  {
    return reader.ReadList<FactionSupply>();
  }

  public static MissionObjective _Read_NuclearOption\u002ESavedMission\u002EMissionObjective(
    NetworkReader reader)
  {
    if (!reader.ReadBooleanExtension())
      return (MissionObjective) null;
    return new MissionObjective()
    {
      objectiveName = reader.ReadString(),
      message = reader.ReadString(),
      positionTrigger = reader.ReadBooleanExtension(),
      victoryObjective = reader.ReadBooleanExtension(),
      nonSequentialObjective = reader.ReadBooleanExtension(),
      triggerRange = reader.ReadSingleConverter(),
      position = reader.ReadVector3(),
      targetUnits = GeneratedNetworkCode._Read_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CSystem\u002EString\u003E(reader)
    };
  }

  public static List<MissionObjective> _Read_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002EMissionObjective\u003E(
    NetworkReader reader)
  {
    return reader.ReadList<MissionObjective>();
  }

  public static Restrictions _Read_NuclearOption\u002ESavedMission\u002ERestrictions(
    NetworkReader reader)
  {
    if (!reader.ReadBooleanExtension())
      return (Restrictions) null;
    return new Restrictions()
    {
      aircraft = GeneratedNetworkCode._Read_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CSystem\u002EString\u003E(reader),
      weapons = GeneratedNetworkCode._Read_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CSystem\u002EString\u003E(reader)
    };
  }

  public static List<MissionFaction> _Read_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002EMissionFaction\u003E(
    NetworkReader reader)
  {
    return reader.ReadList<MissionFaction>();
  }

  public static SavedAirbase _Read_NuclearOption\u002ESavedMission\u002ESavedAirbase(
    NetworkReader reader)
  {
    if (!reader.ReadBooleanExtension())
      return (SavedAirbase) null;
    return new SavedAirbase()
    {
      IsOverride = reader.ReadBooleanExtension(),
      faction = reader.ReadString(),
      UniqueName = reader.ReadString(),
      DisplayName = reader.ReadString(),
      Disabled = reader.ReadBooleanExtension(),
      Capturable = reader.ReadBooleanExtension(),
      CaptureDefense = reader.ReadSingleConverter(),
      CaptureRange = reader.ReadSingleConverter(),
      Center = reader.ReadGlobalPosition(),
      SelectionPosition = reader.ReadGlobalPosition(),
      Tower = reader.ReadString(),
      VerticalLandingPoints = GeneratedNetworkCode._Read_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CGlobalPosition\u003E(reader),
      ServicePoints = GeneratedNetworkCode._Read_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CGlobalPosition\u003E(reader),
      roads = GeneratedNetworkCode._Read_RoadPathfinding\u002ERoadNetwork(reader),
      runways = GeneratedNetworkCode._Read_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002ESavedRunway\u003E(reader)
    };
  }

  public static SavedRunway _Read_NuclearOption\u002ESavedMission\u002ESavedRunway(
    NetworkReader reader)
  {
    if (!reader.ReadBooleanExtension())
      return (SavedRunway) null;
    return new SavedRunway()
    {
      Name = reader.ReadString(),
      Reversable = reader.ReadBooleanExtension(),
      Takeoff = reader.ReadBooleanExtension(),
      Landing = reader.ReadBooleanExtension(),
      Arrestor = reader.ReadBooleanExtension(),
      SkiJump = reader.ReadBooleanExtension(),
      Width = reader.ReadSingleConverter(),
      Start = reader.ReadGlobalPosition(),
      End = reader.ReadGlobalPosition(),
      exitPoints = GeneratedNetworkCode._Read_GlobalPosition\u005B\u005D(reader)
    };
  }

  public static GlobalPosition[] _Read_GlobalPosition\u005B\u005D(NetworkReader reader)
  {
    return reader.ReadArray<GlobalPosition>();
  }

  public static List<SavedRunway> _Read_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002ESavedRunway\u003E(
    NetworkReader reader)
  {
    return reader.ReadList<SavedRunway>();
  }

  public static List<SavedAirbase> _Read_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002ESavedAirbase\u003E(
    NetworkReader reader)
  {
    return reader.ReadList<SavedAirbase>();
  }

  public static UnitInventory _Read_NuclearOption\u002ESavedMission\u002EUnitInventory(
    NetworkReader reader)
  {
    if (!reader.ReadBooleanExtension())
      return (UnitInventory) null;
    return new UnitInventory()
    {
      AttachedUnitUniqueName = reader.ReadString(),
      StoredList = GeneratedNetworkCode._Read_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002EStoredUnitCount\u003E(reader)
    };
  }

  public static StoredUnitCount _Read_NuclearOption\u002ESavedMission\u002EStoredUnitCount(
    NetworkReader reader)
  {
    if (!reader.ReadBooleanExtension())
      return (StoredUnitCount) null;
    return new StoredUnitCount()
    {
      UnitType = reader.ReadString(),
      Count = reader.ReadPackedInt32()
    };
  }

  public static List<StoredUnitCount> _Read_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002EStoredUnitCount\u003E(
    NetworkReader reader)
  {
    return reader.ReadList<StoredUnitCount>();
  }

  public static List<UnitInventory> _Read_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002EUnitInventory\u003E(
    NetworkReader reader)
  {
    return reader.ReadList<UnitInventory>();
  }

  public static SavedMissionObjectives _Read_NuclearOption\u002ESavedMission\u002EObjectiveV2\u002ESavedMissionObjectives(
    NetworkReader reader)
  {
    return new SavedMissionObjectives()
    {
      Objectives = GeneratedNetworkCode._Read_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002EObjectiveV2\u002ESavedObjective\u003E(reader),
      Outcomes = GeneratedNetworkCode._Read_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002EObjectiveV2\u002ESavedOutcome\u003E(reader)
    };
  }

  public static SavedObjective _Read_NuclearOption\u002ESavedMission\u002EObjectiveV2\u002ESavedObjective(
    NetworkReader reader)
  {
    return new SavedObjective()
    {
      UniqueName = reader.ReadString(),
      Faction = reader.ReadString(),
      DisplayName = reader.ReadString(),
      Hidden = reader.ReadBooleanExtension(),
      Type = GeneratedNetworkCode._Read_NuclearOption\u002ESavedMission\u002EObjectiveV2\u002EObjectiveType(reader),
      TypeName = reader.ReadString(),
      Data = GeneratedNetworkCode._Read_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002EObjectiveV2\u002EObjectiveData\u003E(reader),
      Outcomes = GeneratedNetworkCode._Read_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CSystem\u002EString\u003E(reader)
    };
  }

  public static ObjectiveType _Read_NuclearOption\u002ESavedMission\u002EObjectiveV2\u002EObjectiveType(
    NetworkReader reader)
  {
    return (ObjectiveType) reader.ReadPackedInt32();
  }

  public static ObjectiveData _Read_NuclearOption\u002ESavedMission\u002EObjectiveV2\u002EObjectiveData(
    NetworkReader reader)
  {
    return new ObjectiveData()
    {
      StringValue = reader.ReadString(),
      FloatValue = reader.ReadSingleConverter(),
      VectorValue = reader.ReadVector3()
    };
  }

  public static List<ObjectiveData> _Read_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002EObjectiveV2\u002EObjectiveData\u003E(
    NetworkReader reader)
  {
    return reader.ReadList<ObjectiveData>();
  }

  public static List<SavedObjective> _Read_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002EObjectiveV2\u002ESavedObjective\u003E(
    NetworkReader reader)
  {
    return reader.ReadList<SavedObjective>();
  }

  public static SavedOutcome _Read_NuclearOption\u002ESavedMission\u002EObjectiveV2\u002ESavedOutcome(
    NetworkReader reader)
  {
    return new SavedOutcome()
    {
      UniqueName = reader.ReadString(),
      Type = GeneratedNetworkCode._Read_NuclearOption\u002ESavedMission\u002EObjectiveV2\u002EOutcomeType(reader),
      TypeName = reader.ReadString(),
      Data = GeneratedNetworkCode._Read_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002EObjectiveV2\u002EObjectiveData\u003E(reader)
    };
  }

  public static OutcomeType _Read_NuclearOption\u002ESavedMission\u002EObjectiveV2\u002EOutcomeType(
    NetworkReader reader)
  {
    return (OutcomeType) reader.ReadPackedInt32();
  }

  public static List<SavedOutcome> _Read_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002EObjectiveV2\u002ESavedOutcome\u003E(
    NetworkReader reader)
  {
    return reader.ReadList<SavedOutcome>();
  }

  public static void _Write_NetworkMission\u002FSyncMission(
    NetworkWriter writer,
    NetworkMission.SyncMission value)
  {
    writer.WriteString(value.Name);
    GeneratedNetworkCode._Write_NuclearOption\u002ESavedMission\u002EMission(writer, value.Mission);
  }

  public static void _Write_NuclearOption\u002ESavedMission\u002EMission(
    NetworkWriter writer,
    Mission value)
  {
    if (value == null)
    {
      writer.WriteBooleanExtension(false);
    }
    else
    {
      writer.WriteBooleanExtension(true);
      writer.WritePackedInt32(value.JsonVersion);
      writer.WritePackedUInt64(value.WorkshopId);
      GeneratedNetworkCode._Write_NuclearOption\u002ESceneLoading\u002EMapKey(writer, value.MapKey);
      GeneratedNetworkCode._Write_NuclearOption\u002ESavedMission\u002EMissionSettings(writer, value.missionSettings);
      GeneratedNetworkCode._Write_NuclearOption\u002ESavedMission\u002EMissionEnvironment(writer, value.environment);
      GeneratedNetworkCode._Write_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002ESavedAircraft\u003E(writer, value.aircraft);
      GeneratedNetworkCode._Write_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002ESavedVehicle\u003E(writer, value.vehicles);
      GeneratedNetworkCode._Write_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002ESavedShip\u003E(writer, value.ships);
      GeneratedNetworkCode._Write_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002ESavedBuilding\u003E(writer, value.buildings);
      GeneratedNetworkCode._Write_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002ESavedScenery\u003E(writer, value.scenery);
      GeneratedNetworkCode._Write_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002ESavedContainer\u003E(writer, value.containers);
      GeneratedNetworkCode._Write_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002ESavedMissile\u003E(writer, value.missiles);
      GeneratedNetworkCode._Write_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002ESavedPilot\u003E(writer, value.pilots);
      GeneratedNetworkCode._Write_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002EMissionFaction\u003E(writer, value.factions);
      GeneratedNetworkCode._Write_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002ESavedAirbase\u003E(writer, value.airbases);
      GeneratedNetworkCode._Write_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002EUnitInventory\u003E(writer, value.unitInventories);
      GeneratedNetworkCode._Write_NuclearOption\u002ESavedMission\u002EObjectiveV2\u002ESavedMissionObjectives(writer, value.objectives);
    }
  }

  public static void _Write_NuclearOption\u002ESceneLoading\u002EMapKey(
    NetworkWriter writer,
    MapKey value)
  {
    GeneratedNetworkCode._Write_NuclearOption\u002ESceneLoading\u002EMapKey\u002FKeyType(writer, value.Type);
    writer.WriteString(value.TypeName);
    writer.WriteString(value.Path);
  }

  public static void _Write_NuclearOption\u002ESceneLoading\u002EMapKey\u002FKeyType(
    NetworkWriter writer,
    MapKey.KeyType value)
  {
    writer.WriteByteExtension((byte) value);
  }

  public static void _Write_NuclearOption\u002ESavedMission\u002ESavedAircraft(
    NetworkWriter writer,
    SavedAircraft value)
  {
    if (value == null)
    {
      writer.WriteBooleanExtension(false);
    }
    else
    {
      writer.WriteBooleanExtension(true);
      writer.WriteBooleanExtension(value.playerControlled);
      writer.WritePackedInt32(value.playerControlledPriority);
      GeneratedNetworkCode._Write_NuclearOption\u002ESavedMission\u002ELoadoutOld(writer, value.loadout);
      GeneratedNetworkCode._Write_NuclearOption\u002ESavedMission\u002ESavedLoadout(writer, value.savedLoadout);
      writer.WritePackedInt32(value.livery);
      GeneratedNetworkCode._Write_LiveryKey\u002FKeyType(writer, value.liveryType);
      writer.WriteString(value.liveryName);
      writer.WriteSingleConverter(value.fuel);
      writer.WriteSingleConverter(value.skill);
      writer.WriteSingleConverter(value.bravery);
      writer.WriteSingleConverter(value.startingSpeed);
      writer.WriteString(value.type);
      writer.WriteString(value.faction);
      writer.WriteString(value.UniqueName);
      writer.WriteGlobalPosition(value.globalPosition);
      writer.WriteQuaternion(value.rotation);
      GeneratedNetworkCode._Write_NuclearOption\u002ESavedMission\u002EObjectiveV2\u002EOverride\u00601\u003CSystem\u002ESingle\u003E(writer, value.CaptureStrength);
      GeneratedNetworkCode._Write_NuclearOption\u002ESavedMission\u002EObjectiveV2\u002EOverride\u00601\u003CSystem\u002ESingle\u003E(writer, value.CaptureDefense);
      writer.WriteString(value.unitCustomID);
      writer.WriteString(value.spawnTiming);
    }
  }

  public static void _Write_NuclearOption\u002ESavedMission\u002ELoadoutOld(
    NetworkWriter writer,
    LoadoutOld value)
  {
    GeneratedNetworkCode._Write_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CSystem\u002EByte\u003E(writer, value.weaponSelections);
  }

  public static void _Write_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CSystem\u002EByte\u003E(
    NetworkWriter writer,
    List<byte> value)
  {
    writer.WriteList<byte>(value);
  }

  public static void _Write_NuclearOption\u002ESavedMission\u002ESavedLoadout(
    NetworkWriter writer,
    SavedLoadout value)
  {
    if (value == null)
    {
      writer.WriteBooleanExtension(false);
    }
    else
    {
      writer.WriteBooleanExtension(true);
      GeneratedNetworkCode._Write_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002ESavedLoadout\u002FSelectedMount\u003E(writer, value.Selected);
    }
  }

  public static void _Write_NuclearOption\u002ESavedMission\u002ESavedLoadout\u002FSelectedMount(
    NetworkWriter writer,
    SavedLoadout.SelectedMount value)
  {
    writer.WriteString(value.Key);
  }

  public static void _Write_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002ESavedLoadout\u002FSelectedMount\u003E(
    NetworkWriter writer,
    List<SavedLoadout.SelectedMount> value)
  {
    writer.WriteList<SavedLoadout.SelectedMount>(value);
  }

  public static void _Write_LiveryKey\u002FKeyType(NetworkWriter writer, LiveryKey.KeyType value)
  {
    writer.WriteByteExtension((byte) value);
  }

  public static void _Write_NuclearOption\u002ESavedMission\u002EObjectiveV2\u002EOverride\u00601\u003CSystem\u002ESingle\u003E(
    NetworkWriter writer,
    Override<float> value)
  {
    writer.WriteBooleanExtension(value.IsOverride);
    writer.WriteSingleConverter(value.Value);
  }

  public static void _Write_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002ESavedAircraft\u003E(
    NetworkWriter writer,
    List<SavedAircraft> value)
  {
    writer.WriteList<SavedAircraft>(value);
  }

  public static void _Write_NuclearOption\u002ESavedMission\u002ESavedVehicle(
    NetworkWriter writer,
    SavedVehicle value)
  {
    if (value == null)
    {
      writer.WriteBooleanExtension(false);
    }
    else
    {
      writer.WriteBooleanExtension(true);
      writer.WriteBooleanExtension(value.holdPosition);
      writer.WriteSingleConverter(value.skill);
      GeneratedNetworkCode._Write_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002EVehicleWaypoint\u003E(writer, value.waypoints);
      writer.WriteString(value.type);
      writer.WriteString(value.faction);
      writer.WriteString(value.UniqueName);
      writer.WriteGlobalPosition(value.globalPosition);
      writer.WriteQuaternion(value.rotation);
      GeneratedNetworkCode._Write_NuclearOption\u002ESavedMission\u002EObjectiveV2\u002EOverride\u00601\u003CSystem\u002ESingle\u003E(writer, value.CaptureStrength);
      GeneratedNetworkCode._Write_NuclearOption\u002ESavedMission\u002EObjectiveV2\u002EOverride\u00601\u003CSystem\u002ESingle\u003E(writer, value.CaptureDefense);
      writer.WriteString(value.unitCustomID);
      writer.WriteString(value.spawnTiming);
    }
  }

  public static void _Write_NuclearOption\u002ESavedMission\u002EVehicleWaypoint(
    NetworkWriter writer,
    VehicleWaypoint value)
  {
    if (value == null)
    {
      writer.WriteBooleanExtension(false);
    }
    else
    {
      writer.WriteBooleanExtension(true);
      writer.WriteGlobalPosition(value.position);
      writer.WriteString(value.objective);
    }
  }

  public static void _Write_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002EVehicleWaypoint\u003E(
    NetworkWriter writer,
    List<VehicleWaypoint> value)
  {
    writer.WriteList<VehicleWaypoint>(value);
  }

  public static void _Write_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002ESavedVehicle\u003E(
    NetworkWriter writer,
    List<SavedVehicle> value)
  {
    writer.WriteList<SavedVehicle>(value);
  }

  public static void _Write_NuclearOption\u002ESavedMission\u002ESavedShip(
    NetworkWriter writer,
    SavedShip value)
  {
    if (value == null)
    {
      writer.WriteBooleanExtension(false);
    }
    else
    {
      writer.WriteBooleanExtension(true);
      writer.WriteBooleanExtension(value.holdPosition);
      writer.WriteSingleConverter(value.skill);
      GeneratedNetworkCode._Write_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002EVehicleWaypoint\u003E(writer, value.waypoints);
      writer.WriteString(value.type);
      writer.WriteString(value.faction);
      writer.WriteString(value.UniqueName);
      writer.WriteGlobalPosition(value.globalPosition);
      writer.WriteQuaternion(value.rotation);
      GeneratedNetworkCode._Write_NuclearOption\u002ESavedMission\u002EObjectiveV2\u002EOverride\u00601\u003CSystem\u002ESingle\u003E(writer, value.CaptureStrength);
      GeneratedNetworkCode._Write_NuclearOption\u002ESavedMission\u002EObjectiveV2\u002EOverride\u00601\u003CSystem\u002ESingle\u003E(writer, value.CaptureDefense);
      writer.WriteString(value.unitCustomID);
      writer.WriteString(value.spawnTiming);
    }
  }

  public static void _Write_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002ESavedShip\u003E(
    NetworkWriter writer,
    List<SavedShip> value)
  {
    writer.WriteList<SavedShip>(value);
  }

  public static void _Write_NuclearOption\u002ESavedMission\u002ESavedBuilding(
    NetworkWriter writer,
    SavedBuilding value)
  {
    if (value == null)
    {
      writer.WriteBooleanExtension(false);
    }
    else
    {
      writer.WriteBooleanExtension(true);
      writer.WriteBooleanExtension(value.capturable);
      writer.WriteString(value.Airbase);
      GeneratedNetworkCode._Write_NuclearOption\u002ESavedMission\u002ESavedBuilding\u002FFactoryOptions(writer, value.factoryOptions);
      writer.WriteSingleConverter(value.placementOffset);
      writer.WriteString(value.type);
      writer.WriteString(value.faction);
      writer.WriteString(value.UniqueName);
      writer.WriteGlobalPosition(value.globalPosition);
      writer.WriteQuaternion(value.rotation);
      GeneratedNetworkCode._Write_NuclearOption\u002ESavedMission\u002EObjectiveV2\u002EOverride\u00601\u003CSystem\u002ESingle\u003E(writer, value.CaptureStrength);
      GeneratedNetworkCode._Write_NuclearOption\u002ESavedMission\u002EObjectiveV2\u002EOverride\u00601\u003CSystem\u002ESingle\u003E(writer, value.CaptureDefense);
      writer.WriteString(value.unitCustomID);
      writer.WriteString(value.spawnTiming);
    }
  }

  public static void _Write_NuclearOption\u002ESavedMission\u002ESavedBuilding\u002FFactoryOptions(
    NetworkWriter writer,
    SavedBuilding.FactoryOptions value)
  {
    if (value == null)
    {
      writer.WriteBooleanExtension(false);
    }
    else
    {
      writer.WriteBooleanExtension(true);
      writer.WriteString(value.productionType);
      writer.WriteSingleConverter(value.productionTime);
    }
  }

  public static void _Write_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002ESavedBuilding\u003E(
    NetworkWriter writer,
    List<SavedBuilding> value)
  {
    writer.WriteList<SavedBuilding>(value);
  }

  public static void _Write_NuclearOption\u002ESavedMission\u002ESavedScenery(
    NetworkWriter writer,
    SavedScenery value)
  {
    if (value == null)
    {
      writer.WriteBooleanExtension(false);
    }
    else
    {
      writer.WriteBooleanExtension(true);
      writer.WriteBooleanExtension(value.indestructible);
      writer.WriteString(value.type);
      writer.WriteString(value.faction);
      writer.WriteString(value.UniqueName);
      writer.WriteGlobalPosition(value.globalPosition);
      writer.WriteQuaternion(value.rotation);
      GeneratedNetworkCode._Write_NuclearOption\u002ESavedMission\u002EObjectiveV2\u002EOverride\u00601\u003CSystem\u002ESingle\u003E(writer, value.CaptureStrength);
      GeneratedNetworkCode._Write_NuclearOption\u002ESavedMission\u002EObjectiveV2\u002EOverride\u00601\u003CSystem\u002ESingle\u003E(writer, value.CaptureDefense);
      writer.WriteString(value.unitCustomID);
      writer.WriteString(value.spawnTiming);
    }
  }

  public static void _Write_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002ESavedScenery\u003E(
    NetworkWriter writer,
    List<SavedScenery> value)
  {
    writer.WriteList<SavedScenery>(value);
  }

  public static void _Write_NuclearOption\u002ESavedMission\u002ESavedContainer(
    NetworkWriter writer,
    SavedContainer value)
  {
    if (value == null)
    {
      writer.WriteBooleanExtension(false);
    }
    else
    {
      writer.WriteBooleanExtension(true);
      writer.WriteString(value.type);
      writer.WriteString(value.faction);
      writer.WriteString(value.UniqueName);
      writer.WriteGlobalPosition(value.globalPosition);
      writer.WriteQuaternion(value.rotation);
      GeneratedNetworkCode._Write_NuclearOption\u002ESavedMission\u002EObjectiveV2\u002EOverride\u00601\u003CSystem\u002ESingle\u003E(writer, value.CaptureStrength);
      GeneratedNetworkCode._Write_NuclearOption\u002ESavedMission\u002EObjectiveV2\u002EOverride\u00601\u003CSystem\u002ESingle\u003E(writer, value.CaptureDefense);
      writer.WriteString(value.unitCustomID);
      writer.WriteString(value.spawnTiming);
    }
  }

  public static void _Write_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002ESavedContainer\u003E(
    NetworkWriter writer,
    List<SavedContainer> value)
  {
    writer.WriteList<SavedContainer>(value);
  }

  public static void _Write_NuclearOption\u002ESavedMission\u002ESavedMissile(
    NetworkWriter writer,
    SavedMissile value)
  {
    if (value == null)
    {
      writer.WriteBooleanExtension(false);
    }
    else
    {
      writer.WriteBooleanExtension(true);
      writer.WriteSingleConverter(value.startingSpeed);
      writer.WriteString(value.type);
      writer.WriteString(value.faction);
      writer.WriteString(value.UniqueName);
      writer.WriteGlobalPosition(value.globalPosition);
      writer.WriteQuaternion(value.rotation);
      GeneratedNetworkCode._Write_NuclearOption\u002ESavedMission\u002EObjectiveV2\u002EOverride\u00601\u003CSystem\u002ESingle\u003E(writer, value.CaptureStrength);
      GeneratedNetworkCode._Write_NuclearOption\u002ESavedMission\u002EObjectiveV2\u002EOverride\u00601\u003CSystem\u002ESingle\u003E(writer, value.CaptureDefense);
      writer.WriteString(value.unitCustomID);
      writer.WriteString(value.spawnTiming);
    }
  }

  public static void _Write_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002ESavedMissile\u003E(
    NetworkWriter writer,
    List<SavedMissile> value)
  {
    writer.WriteList<SavedMissile>(value);
  }

  public static void _Write_NuclearOption\u002ESavedMission\u002ESavedPilot(
    NetworkWriter writer,
    SavedPilot value)
  {
    if (value == null)
    {
      writer.WriteBooleanExtension(false);
    }
    else
    {
      writer.WriteBooleanExtension(true);
      writer.WriteString(value.type);
      writer.WriteString(value.faction);
      writer.WriteString(value.UniqueName);
      writer.WriteGlobalPosition(value.globalPosition);
      writer.WriteQuaternion(value.rotation);
      GeneratedNetworkCode._Write_NuclearOption\u002ESavedMission\u002EObjectiveV2\u002EOverride\u00601\u003CSystem\u002ESingle\u003E(writer, value.CaptureStrength);
      GeneratedNetworkCode._Write_NuclearOption\u002ESavedMission\u002EObjectiveV2\u002EOverride\u00601\u003CSystem\u002ESingle\u003E(writer, value.CaptureDefense);
      writer.WriteString(value.unitCustomID);
      writer.WriteString(value.spawnTiming);
    }
  }

  public static void _Write_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002ESavedPilot\u003E(
    NetworkWriter writer,
    List<SavedPilot> value)
  {
    writer.WriteList<SavedPilot>(value);
  }

  public static void _Write_NuclearOption\u002ESavedMission\u002EMissionFaction(
    NetworkWriter writer,
    MissionFaction value)
  {
    if (value == null)
    {
      writer.WriteBooleanExtension(false);
    }
    else
    {
      writer.WriteBooleanExtension(true);
      writer.WriteString(value.factionName);
      writer.WriteBooleanExtension(value.preventJoin);
      writer.WriteBooleanExtension(value.preventDonation);
      GeneratedNetworkCode._Write_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002EFactionSupply\u003E(writer, value.supplies);
      writer.WriteSingleConverter(value.startingBalance);
      writer.WriteSingleConverter(value.playerJoinAllowance);
      writer.WriteSingleConverter(value.playerTaxRate);
      writer.WriteSingleConverter(value.regularIncome);
      writer.WriteSingleConverter(value.excessFundsDistributePercent);
      writer.WriteSingleConverter(value.killReward);
      writer.WritePackedInt32(value.startingWarheads);
      writer.WritePackedInt32(value.reserveWarheads);
      writer.WritePackedInt32(value.reserveAirframes);
      writer.WritePackedInt32(value.extraReservesPerPlayer);
      writer.WritePackedInt32(value.AIAircraftLimit);
      writer.WriteSingleConverter(value.reduceAIPerFriendlyPlayer);
      writer.WriteSingleConverter(value.addAIPerEnemyPlayer);
      GeneratedNetworkCode._Write_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002EMissionObjective\u003E(writer, value.objectives);
      GeneratedNetworkCode._Write_NuclearOption\u002ESavedMission\u002ERestrictions(writer, value.restrictions);
      GeneratedNetworkCode._Write_NuclearOption\u002ESavedMission\u002EObjectiveV2\u002EOverride\u00601\u003CNuclearOption\u002ESavedMission\u002EPositionRotation\u003E(writer, value.cameraStartPosition);
    }
  }

  public static void _Write_NuclearOption\u002ESavedMission\u002EFactionSupply(
    NetworkWriter writer,
    FactionSupply value)
  {
    if (value == null)
    {
      writer.WriteBooleanExtension(false);
    }
    else
    {
      writer.WriteBooleanExtension(true);
      writer.WriteString(value.unitType);
      writer.WritePackedInt32(value.count);
    }
  }

  public static void _Write_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002EFactionSupply\u003E(
    NetworkWriter writer,
    List<FactionSupply> value)
  {
    writer.WriteList<FactionSupply>(value);
  }

  public static void _Write_NuclearOption\u002ESavedMission\u002EMissionObjective(
    NetworkWriter writer,
    MissionObjective value)
  {
    if (value == null)
    {
      writer.WriteBooleanExtension(false);
    }
    else
    {
      writer.WriteBooleanExtension(true);
      writer.WriteString(value.objectiveName);
      writer.WriteString(value.message);
      writer.WriteBooleanExtension(value.positionTrigger);
      writer.WriteBooleanExtension(value.victoryObjective);
      writer.WriteBooleanExtension(value.nonSequentialObjective);
      writer.WriteSingleConverter(value.triggerRange);
      writer.WriteVector3(value.position);
      GeneratedNetworkCode._Write_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CSystem\u002EString\u003E(writer, value.targetUnits);
    }
  }

  public static void _Write_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002EMissionObjective\u003E(
    NetworkWriter writer,
    List<MissionObjective> value)
  {
    writer.WriteList<MissionObjective>(value);
  }

  public static void _Write_NuclearOption\u002ESavedMission\u002ERestrictions(
    NetworkWriter writer,
    Restrictions value)
  {
    if (value == null)
    {
      writer.WriteBooleanExtension(false);
    }
    else
    {
      writer.WriteBooleanExtension(true);
      GeneratedNetworkCode._Write_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CSystem\u002EString\u003E(writer, value.aircraft);
      GeneratedNetworkCode._Write_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CSystem\u002EString\u003E(writer, value.weapons);
    }
  }

  public static void _Write_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002EMissionFaction\u003E(
    NetworkWriter writer,
    List<MissionFaction> value)
  {
    writer.WriteList<MissionFaction>(value);
  }

  public static void _Write_NuclearOption\u002ESavedMission\u002ESavedAirbase(
    NetworkWriter writer,
    SavedAirbase value)
  {
    if (value == null)
    {
      writer.WriteBooleanExtension(false);
    }
    else
    {
      writer.WriteBooleanExtension(true);
      writer.WriteBooleanExtension(value.IsOverride);
      writer.WriteString(value.faction);
      writer.WriteString(value.UniqueName);
      writer.WriteString(value.DisplayName);
      writer.WriteBooleanExtension(value.Disabled);
      writer.WriteBooleanExtension(value.Capturable);
      writer.WriteSingleConverter(value.CaptureDefense);
      writer.WriteSingleConverter(value.CaptureRange);
      writer.WriteGlobalPosition(value.Center);
      writer.WriteGlobalPosition(value.SelectionPosition);
      writer.WriteString(value.Tower);
      GeneratedNetworkCode._Write_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CGlobalPosition\u003E(writer, value.VerticalLandingPoints);
      GeneratedNetworkCode._Write_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CGlobalPosition\u003E(writer, value.ServicePoints);
      GeneratedNetworkCode._Write_RoadPathfinding\u002ERoadNetwork(writer, value.roads);
      GeneratedNetworkCode._Write_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002ESavedRunway\u003E(writer, value.runways);
    }
  }

  public static void _Write_NuclearOption\u002ESavedMission\u002ESavedRunway(
    NetworkWriter writer,
    SavedRunway value)
  {
    if (value == null)
    {
      writer.WriteBooleanExtension(false);
    }
    else
    {
      writer.WriteBooleanExtension(true);
      writer.WriteString(value.Name);
      writer.WriteBooleanExtension(value.Reversable);
      writer.WriteBooleanExtension(value.Takeoff);
      writer.WriteBooleanExtension(value.Landing);
      writer.WriteBooleanExtension(value.Arrestor);
      writer.WriteBooleanExtension(value.SkiJump);
      writer.WriteSingleConverter(value.Width);
      writer.WriteGlobalPosition(value.Start);
      writer.WriteGlobalPosition(value.End);
      GeneratedNetworkCode._Write_GlobalPosition\u005B\u005D(writer, value.exitPoints);
    }
  }

  public static void _Write_GlobalPosition\u005B\u005D(NetworkWriter writer, GlobalPosition[] value)
  {
    writer.WriteArray<GlobalPosition>(value);
  }

  public static void _Write_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002ESavedRunway\u003E(
    NetworkWriter writer,
    List<SavedRunway> value)
  {
    writer.WriteList<SavedRunway>(value);
  }

  public static void _Write_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002ESavedAirbase\u003E(
    NetworkWriter writer,
    List<SavedAirbase> value)
  {
    writer.WriteList<SavedAirbase>(value);
  }

  public static void _Write_NuclearOption\u002ESavedMission\u002EUnitInventory(
    NetworkWriter writer,
    UnitInventory value)
  {
    if (value == null)
    {
      writer.WriteBooleanExtension(false);
    }
    else
    {
      writer.WriteBooleanExtension(true);
      writer.WriteString(value.AttachedUnitUniqueName);
      GeneratedNetworkCode._Write_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002EStoredUnitCount\u003E(writer, value.StoredList);
    }
  }

  public static void _Write_NuclearOption\u002ESavedMission\u002EStoredUnitCount(
    NetworkWriter writer,
    StoredUnitCount value)
  {
    if (value == null)
    {
      writer.WriteBooleanExtension(false);
    }
    else
    {
      writer.WriteBooleanExtension(true);
      writer.WriteString(value.UnitType);
      writer.WritePackedInt32(value.Count);
    }
  }

  public static void _Write_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002EStoredUnitCount\u003E(
    NetworkWriter writer,
    List<StoredUnitCount> value)
  {
    writer.WriteList<StoredUnitCount>(value);
  }

  public static void _Write_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002EUnitInventory\u003E(
    NetworkWriter writer,
    List<UnitInventory> value)
  {
    writer.WriteList<UnitInventory>(value);
  }

  public static void _Write_NuclearOption\u002ESavedMission\u002EObjectiveV2\u002ESavedMissionObjectives(
    NetworkWriter writer,
    SavedMissionObjectives value)
  {
    GeneratedNetworkCode._Write_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002EObjectiveV2\u002ESavedObjective\u003E(writer, value.Objectives);
    GeneratedNetworkCode._Write_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002EObjectiveV2\u002ESavedOutcome\u003E(writer, value.Outcomes);
  }

  public static void _Write_NuclearOption\u002ESavedMission\u002EObjectiveV2\u002ESavedObjective(
    NetworkWriter writer,
    SavedObjective value)
  {
    writer.WriteString(value.UniqueName);
    writer.WriteString(value.Faction);
    writer.WriteString(value.DisplayName);
    writer.WriteBooleanExtension(value.Hidden);
    GeneratedNetworkCode._Write_NuclearOption\u002ESavedMission\u002EObjectiveV2\u002EObjectiveType(writer, value.Type);
    writer.WriteString(value.TypeName);
    GeneratedNetworkCode._Write_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002EObjectiveV2\u002EObjectiveData\u003E(writer, value.Data);
    GeneratedNetworkCode._Write_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CSystem\u002EString\u003E(writer, value.Outcomes);
  }

  public static void _Write_NuclearOption\u002ESavedMission\u002EObjectiveV2\u002EObjectiveType(
    NetworkWriter writer,
    ObjectiveType value)
  {
    writer.WritePackedInt32((int) value);
  }

  public static void _Write_NuclearOption\u002ESavedMission\u002EObjectiveV2\u002EObjectiveData(
    NetworkWriter writer,
    ObjectiveData value)
  {
    writer.WriteString(value.StringValue);
    writer.WriteSingleConverter(value.FloatValue);
    writer.WriteVector3(value.VectorValue);
  }

  public static void _Write_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002EObjectiveV2\u002EObjectiveData\u003E(
    NetworkWriter writer,
    List<ObjectiveData> value)
  {
    writer.WriteList<ObjectiveData>(value);
  }

  public static void _Write_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002EObjectiveV2\u002ESavedObjective\u003E(
    NetworkWriter writer,
    List<SavedObjective> value)
  {
    writer.WriteList<SavedObjective>(value);
  }

  public static void _Write_NuclearOption\u002ESavedMission\u002EObjectiveV2\u002ESavedOutcome(
    NetworkWriter writer,
    SavedOutcome value)
  {
    writer.WriteString(value.UniqueName);
    GeneratedNetworkCode._Write_NuclearOption\u002ESavedMission\u002EObjectiveV2\u002EOutcomeType(writer, value.Type);
    writer.WriteString(value.TypeName);
    GeneratedNetworkCode._Write_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002EObjectiveV2\u002EObjectiveData\u003E(writer, value.Data);
  }

  public static void _Write_NuclearOption\u002ESavedMission\u002EObjectiveV2\u002EOutcomeType(
    NetworkWriter writer,
    OutcomeType value)
  {
    writer.WritePackedInt32((int) value);
  }

  public static void _Write_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002EObjectiveV2\u002ESavedOutcome\u003E(
    NetworkWriter writer,
    List<SavedOutcome> value)
  {
    writer.WriteList<SavedOutcome>(value);
  }

  public static NetworkMission.SyncMissionStart _Read_NetworkMission\u002FSyncMissionStart(
    NetworkReader reader)
  {
    return new NetworkMission.SyncMissionStart();
  }

  public static void _Write_NetworkMission\u002FSyncMissionStart(
    NetworkWriter writer,
    NetworkMission.SyncMissionStart value)
  {
  }

  public static LoadMapMessage _Read_NuclearOption\u002ESceneLoading\u002ELoadMapMessage(
    NetworkReader reader)
  {
    return new LoadMapMessage()
    {
      Key = GeneratedNetworkCode._Read_NuclearOption\u002ESceneLoading\u002EMapKey(reader)
    };
  }

  public static void _Write_NuclearOption\u002ESceneLoading\u002ELoadMapMessage(
    NetworkWriter writer,
    LoadMapMessage value)
  {
    GeneratedNetworkCode._Write_NuclearOption\u002ESceneLoading\u002EMapKey(writer, value.Key);
  }

  public static NetworkTransformBase.NetworkSnapshot _Read_NuclearOption\u002ENetworkTransforms\u002ENetworkTransformBase\u002FNetworkSnapshot(
    NetworkReader reader)
  {
    return new NetworkTransformBase.NetworkSnapshot()
    {
      ClientInputs = GeneratedNetworkCode._Read_System\u002ENullable\u00601\u003CCompressedInputs\u003E(reader),
      timestamp = GeneratedNetworkCode._Read_System\u002ENullable\u00601\u003CSystem\u002EDouble\u003E(reader),
      extraExtrapolation = GeneratedNetworkCode._Read_System\u002ENullable\u00601\u003CSystem\u002ESingle\u003E(reader),
      globalPos = reader.ReadGlobalPosition(),
      velocity = GeneratedNetworkCode._Read_Vector3Compressed(reader),
      rotation = NetworkTransformBase.NetworkSnapshot.rotation__Packer.Unpack(reader)
    };
  }

  public static CompressedInputs _Read_CompressedInputs(NetworkReader reader)
  {
    return new CompressedInputs()
    {
      pitch = GeneratedNetworkCode._Read_CompressedFloat(reader),
      roll = GeneratedNetworkCode._Read_CompressedFloat(reader),
      yaw = GeneratedNetworkCode._Read_CompressedFloat(reader),
      throttle = GeneratedNetworkCode._Read_CompressedFloat(reader),
      brake = GeneratedNetworkCode._Read_CompressedFloat(reader),
      customAxis1 = GeneratedNetworkCode._Read_CompressedFloat(reader)
    };
  }

  public static CompressedFloat _Read_CompressedFloat(NetworkReader reader)
  {
    return new CompressedFloat()
    {
      Value = reader.ReadUInt16Extension()
    };
  }

  public static CompressedInputs? _Read_System\u002ENullable\u00601\u003CCompressedInputs\u003E(
    NetworkReader reader)
  {
    return reader.ReadNullable<CompressedInputs>();
  }

  public static double? _Read_System\u002ENullable\u00601\u003CSystem\u002EDouble\u003E(
    NetworkReader reader)
  {
    return reader.ReadNullable<double>();
  }

  public static float? _Read_System\u002ENullable\u00601\u003CSystem\u002ESingle\u003E(
    NetworkReader reader)
  {
    return reader.ReadNullable<float>();
  }

  public static Vector3Compressed _Read_Vector3Compressed(NetworkReader reader)
  {
    return new Vector3Compressed()
    {
      x = GeneratedNetworkCode._Read_CompressedFloat(reader),
      y = GeneratedNetworkCode._Read_CompressedFloat(reader),
      z = GeneratedNetworkCode._Read_CompressedFloat(reader)
    };
  }

  public static void _Write_NuclearOption\u002ENetworkTransforms\u002ENetworkTransformBase\u002FNetworkSnapshot(
    NetworkWriter writer,
    NetworkTransformBase.NetworkSnapshot value)
  {
    GeneratedNetworkCode._Write_System\u002ENullable\u00601\u003CCompressedInputs\u003E(writer, value.ClientInputs);
    GeneratedNetworkCode._Write_System\u002ENullable\u00601\u003CSystem\u002EDouble\u003E(writer, value.timestamp);
    GeneratedNetworkCode._Write_System\u002ENullable\u00601\u003CSystem\u002ESingle\u003E(writer, value.extraExtrapolation);
    writer.WriteGlobalPosition(value.globalPos);
    GeneratedNetworkCode._Write_Vector3Compressed(writer, value.velocity);
    NetworkTransformBase.NetworkSnapshot.rotation__Packer.Pack(writer, value.rotation);
  }

  public static void _Write_CompressedInputs(NetworkWriter writer, CompressedInputs value)
  {
    GeneratedNetworkCode._Write_CompressedFloat(writer, value.pitch);
    GeneratedNetworkCode._Write_CompressedFloat(writer, value.roll);
    GeneratedNetworkCode._Write_CompressedFloat(writer, value.yaw);
    GeneratedNetworkCode._Write_CompressedFloat(writer, value.throttle);
    GeneratedNetworkCode._Write_CompressedFloat(writer, value.brake);
    GeneratedNetworkCode._Write_CompressedFloat(writer, value.customAxis1);
  }

  public static void _Write_CompressedFloat(NetworkWriter writer, CompressedFloat value)
  {
    writer.WriteUInt16Extension(value.Value);
  }

  public static void _Write_System\u002ENullable\u00601\u003CCompressedInputs\u003E(
    NetworkWriter writer,
    CompressedInputs? value)
  {
    writer.WriteNullable<CompressedInputs>(value);
  }

  public static void _Write_System\u002ENullable\u00601\u003CSystem\u002EDouble\u003E(
    NetworkWriter writer,
    double? value)
  {
    writer.WriteNullable<double>(value);
  }

  public static void _Write_System\u002ENullable\u00601\u003CSystem\u002ESingle\u003E(
    NetworkWriter writer,
    float? value)
  {
    writer.WriteNullable<float>(value);
  }

  public static void _Write_Vector3Compressed(NetworkWriter writer, Vector3Compressed value)
  {
    GeneratedNetworkCode._Write_CompressedFloat(writer, value.x);
    GeneratedNetworkCode._Write_CompressedFloat(writer, value.y);
    GeneratedNetworkCode._Write_CompressedFloat(writer, value.z);
  }

  public static SendTransformBatcher.TransformMessage _Read_NuclearOption\u002ENetworkTransforms\u002ESendTransformBatcher\u002FTransformMessage(
    NetworkReader reader)
  {
    return new SendTransformBatcher.TransformMessage()
    {
      timestamp = reader.ReadDoubleConverter(),
      data = reader.ReadBytesAndSizeSegment()
    };
  }

  public static void _Write_NuclearOption\u002ENetworkTransforms\u002ESendTransformBatcher\u002FTransformMessage(
    NetworkWriter writer,
    SendTransformBatcher.TransformMessage value)
  {
    writer.WriteDoubleConverter(value.timestamp);
    writer.WriteBytesAndSizeSegment(value.data);
  }

  public static HostEndedMessage _Read_NuclearOption\u002ENetworking\u002EHostEndedMessage(
    NetworkReader reader)
  {
    return new HostEndedMessage()
    {
      HostName = reader.ReadString()
    };
  }

  public static void _Write_NuclearOption\u002ENetworking\u002EHostEndedMessage(
    NetworkWriter writer,
    HostEndedMessage value)
  {
    writer.WriteString(value.HostName);
  }

  public static LoadWaitingSceneMessage _Read_NuclearOption\u002ENetworking\u002ELoadWaitingSceneMessage(
    NetworkReader reader)
  {
    return new LoadWaitingSceneMessage();
  }

  public static void _Write_NuclearOption\u002ENetworking\u002ELoadWaitingSceneMessage(
    NetworkWriter writer,
    LoadWaitingSceneMessage value)
  {
  }

  public static ServerLoadingProgressMessage _Read_NuclearOption\u002ENetworking\u002EServerLoadingProgressMessage(
    NetworkReader reader)
  {
    return new ServerLoadingProgressMessage()
    {
      LoadingMessage = reader.ReadString()
    };
  }

  public static void _Write_NuclearOption\u002ENetworking\u002EServerLoadingProgressMessage(
    NetworkWriter writer,
    ServerLoadingProgressMessage value)
  {
    writer.WriteString(value.LoadingMessage);
  }

  public static NetworkAuthenticatorNuclearOption.AuthMessage _Read_NuclearOption\u002ENetworking\u002EAuthentication\u002ENetworkAuthenticatorNuclearOption\u002FAuthMessage(
    NetworkReader reader)
  {
    return new NetworkAuthenticatorNuclearOption.AuthMessage()
    {
      BuildHash = reader.ReadPackedUInt32(),
      JoinAs = GeneratedNetworkCode._Read_NuclearOption\u002ENetworking\u002EPlayerType(reader),
      SteamAuthToken = reader.ReadBytesAndSizeSegment()
    };
  }

  public static PlayerType _Read_NuclearOption\u002ENetworking\u002EPlayerType(NetworkReader reader)
  {
    return (PlayerType) reader.ReadPackedInt32();
  }

  public static void _Write_NuclearOption\u002ENetworking\u002EAuthentication\u002ENetworkAuthenticatorNuclearOption\u002FAuthMessage(
    NetworkWriter writer,
    NetworkAuthenticatorNuclearOption.AuthMessage value)
  {
    writer.WritePackedUInt32(value.BuildHash);
    GeneratedNetworkCode._Write_NuclearOption\u002ENetworking\u002EPlayerType(writer, value.JoinAs);
    writer.WriteBytesAndSizeSegment(value.SteamAuthToken);
  }

  public static void _Write_NuclearOption\u002ENetworking\u002EPlayerType(
    NetworkWriter writer,
    PlayerType value)
  {
    writer.WritePackedInt32((int) value);
  }

  public static NetworkAuthenticatorNuclearOption.PasswordChallenge _Read_NuclearOption\u002ENetworking\u002EAuthentication\u002ENetworkAuthenticatorNuclearOption\u002FPasswordChallenge(
    NetworkReader reader)
  {
    return new NetworkAuthenticatorNuclearOption.PasswordChallenge()
    {
      Nonce = reader.ReadBytesAndSizeSegment()
    };
  }

  public static void _Write_NuclearOption\u002ENetworking\u002EAuthentication\u002ENetworkAuthenticatorNuclearOption\u002FPasswordChallenge(
    NetworkWriter writer,
    NetworkAuthenticatorNuclearOption.PasswordChallenge value)
  {
    writer.WriteBytesAndSizeSegment(value.Nonce);
  }

  public static NetworkAuthenticatorNuclearOption.PasswordResponse _Read_NuclearOption\u002ENetworking\u002EAuthentication\u002ENetworkAuthenticatorNuclearOption\u002FPasswordResponse(
    NetworkReader reader)
  {
    return new NetworkAuthenticatorNuclearOption.PasswordResponse()
    {
      Response = reader.ReadBytesAndSizeSegment()
    };
  }

  public static void _Write_NuclearOption\u002ENetworking\u002EAuthentication\u002ENetworkAuthenticatorNuclearOption\u002FPasswordResponse(
    NetworkWriter writer,
    NetworkAuthenticatorNuclearOption.PasswordResponse value)
  {
    writer.WriteBytesAndSizeSegment(value.Response);
  }

  public static NetworkAuthenticatorNuclearOption.AuthFailReason _Read_NuclearOption\u002ENetworking\u002EAuthentication\u002ENetworkAuthenticatorNuclearOption\u002FAuthFailReason(
    NetworkReader reader)
  {
    return new NetworkAuthenticatorNuclearOption.AuthFailReason()
    {
      Reason = reader.ReadString()
    };
  }

  public static void _Write_NuclearOption\u002ENetworking\u002EAuthentication\u002ENetworkAuthenticatorNuclearOption\u002FAuthFailReason(
    NetworkWriter writer,
    NetworkAuthenticatorNuclearOption.AuthFailReason value)
  {
    writer.WriteString(value.Reason);
  }

  public static NetworkAuthenticatorNuclearOption.BuildHashMismatch _Read_NuclearOption\u002ENetworking\u002EAuthentication\u002ENetworkAuthenticatorNuclearOption\u002FBuildHashMismatch(
    NetworkReader reader)
  {
    return new NetworkAuthenticatorNuclearOption.BuildHashMismatch()
    {
      BuildHash = reader.ReadPackedUInt32()
    };
  }

  public static void _Write_NuclearOption\u002ENetworking\u002EAuthentication\u002ENetworkAuthenticatorNuclearOption\u002FBuildHashMismatch(
    NetworkWriter writer,
    NetworkAuthenticatorNuclearOption.BuildHashMismatch value)
  {
    writer.WritePackedUInt32(value.BuildHash);
  }

  public static void _Write_PersistentID(NetworkWriter writer, PersistentID value)
  {
    writer.WritePackedUInt32(value.Id);
  }

  public static PersistentID _Read_PersistentID(NetworkReader reader)
  {
    return new PersistentID()
    {
      Id = reader.ReadPackedUInt32()
    };
  }

  public static PlayerRef _Read_NuclearOption\u002ENetworking\u002EPlayerRef(NetworkReader reader)
  {
    return new PlayerRef()
    {
      PlayerId = reader.ReadPackedUInt32()
    };
  }

  public static void _Write_NuclearOption\u002ENetworking\u002EPlayerRef(
    NetworkWriter writer,
    PlayerRef value)
  {
    writer.WritePackedUInt32(value.PlayerId);
  }

  public static Airbase _Read_Airbase(NetworkReader reader)
  {
    return (Airbase) reader.ReadNetworkBehaviour();
  }

  public static NetworkBehaviorSyncvar<Airbase> _Read_Mirage\u002ENetworkBehaviorSyncvar\u00601\u003CAirbase\u003E(
    NetworkReader reader)
  {
    return reader.ReadGenericNetworkBehaviourSyncVar<Airbase>();
  }

  public static void _Write_Airbase(NetworkWriter writer, Airbase value)
  {
    writer.WriteNetworkBehaviour((NetworkBehaviour) value);
  }

  public static void _Write_Mirage\u002ENetworkBehaviorSyncvar\u00601\u003CAirbase\u003E(
    NetworkWriter writer,
    NetworkBehaviorSyncvar<Airbase> value)
  {
    writer.WriteGenericNetworkBehaviorSyncVar<Airbase>(value);
  }

  public static FactionHQ.RuntimeSupply _Read_FactionHQ\u002FRuntimeSupply(NetworkReader reader)
  {
    return new FactionHQ.RuntimeSupply()
    {
      Count = reader.ReadPackedInt32()
    };
  }

  public static void _Write_FactionHQ\u002FRuntimeSupply(
    NetworkWriter writer,
    FactionHQ.RuntimeSupply value)
  {
    writer.WritePackedInt32(value.Count);
  }

  public static void _Write_NuclearOption\u002EExclusionZone(
    NetworkWriter writer,
    ExclusionZone value)
  {
    GeneratedNetworkCode._Write_PersistentID(writer, value.sourceId);
    writer.WriteGlobalPosition(value.position);
    writer.WriteSingleConverter(value.radius);
  }

  public static ExclusionZone _Read_NuclearOption\u002EExclusionZone(NetworkReader reader)
  {
    return new ExclusionZone()
    {
      sourceId = GeneratedNetworkCode._Read_PersistentID(reader),
      position = reader.ReadGlobalPosition(),
      radius = reader.ReadSingleConverter()
    };
  }

  public static void _Write_NuclearOption\u002ESavedMission\u002EObjectiveV2\u002EOutcomes\u002EEndType(
    NetworkWriter writer,
    EndType value)
  {
    writer.WritePackedInt32((int) value);
  }

  public static EndType _Read_NuclearOption\u002ESavedMission\u002EObjectiveV2\u002EOutcomes\u002EEndType(
    NetworkReader reader)
  {
    return (EndType) reader.ReadPackedInt32();
  }

  public static List<int> _Read_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CSystem\u002EInt32\u003E(
    NetworkReader reader)
  {
    return reader.ReadList<int>();
  }

  public static void _Write_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CSystem\u002EInt32\u003E(
    NetworkWriter writer,
    List<int> value)
  {
    writer.WriteList<int>(value);
  }

  public static void _Write_FactionHQ(NetworkWriter writer, FactionHQ value)
  {
    writer.WriteNetworkBehaviour((NetworkBehaviour) value);
  }

  public static FactionHQ _Read_FactionHQ(NetworkReader reader)
  {
    return (FactionHQ) reader.ReadNetworkBehaviour();
  }

  public static void _Write_MissionStatsTracker\u002FTypeStat(
    NetworkWriter writer,
    MissionStatsTracker.TypeStat value)
  {
    GeneratedNetworkCode._Write_MissionStatsTracker\u002FStat(writer, value.total);
    GeneratedNetworkCode._Write_MissionStatsTracker\u002FStat(writer, value.buildings);
    GeneratedNetworkCode._Write_MissionStatsTracker\u002FStat(writer, value.ships);
    GeneratedNetworkCode._Write_MissionStatsTracker\u002FStat(writer, value.vehicles);
    GeneratedNetworkCode._Write_MissionStatsTracker\u002FStat(writer, value.aircraft);
  }

  public static void _Write_MissionStatsTracker\u002FStat(
    NetworkWriter writer,
    MissionStatsTracker.Stat value)
  {
    writer.WriteSingleConverter(value.total);
    writer.WriteSingleConverter(value.current);
    writer.WriteSingleConverter(value.spent);
    writer.WriteSingleConverter(value.lost);
  }

  public static MissionStatsTracker.TypeStat _Read_MissionStatsTracker\u002FTypeStat(
    NetworkReader reader)
  {
    return new MissionStatsTracker.TypeStat()
    {
      total = GeneratedNetworkCode._Read_MissionStatsTracker\u002FStat(reader),
      buildings = GeneratedNetworkCode._Read_MissionStatsTracker\u002FStat(reader),
      ships = GeneratedNetworkCode._Read_MissionStatsTracker\u002FStat(reader),
      vehicles = GeneratedNetworkCode._Read_MissionStatsTracker\u002FStat(reader),
      aircraft = GeneratedNetworkCode._Read_MissionStatsTracker\u002FStat(reader)
    };
  }

  public static MissionStatsTracker.Stat _Read_MissionStatsTracker\u002FStat(NetworkReader reader)
  {
    return new MissionStatsTracker.Stat()
    {
      total = reader.ReadSingleConverter(),
      current = reader.ReadSingleConverter(),
      spent = reader.ReadSingleConverter(),
      lost = reader.ReadSingleConverter()
    };
  }

  public static void _Write_Airbase\u002FTrySpawnResult(
    NetworkWriter writer,
    Airbase.TrySpawnResult value)
  {
    writer.WriteBooleanExtension(value.Allowed);
    GeneratedNetworkCode._Write_Hangar(writer, value.Hangar);
    writer.WriteBooleanExtension(value.DelayedSpawn);
  }

  public static void _Write_Hangar(NetworkWriter writer, Hangar value)
  {
    writer.WriteNetworkBehaviour((NetworkBehaviour) value);
  }

  public static Airbase.TrySpawnResult _Read_Airbase\u002FTrySpawnResult(NetworkReader reader)
  {
    return new Airbase.TrySpawnResult()
    {
      Allowed = reader.ReadBooleanExtension(),
      Hangar = GeneratedNetworkCode._Read_Hangar(reader),
      DelayedSpawn = reader.ReadBooleanExtension()
    };
  }

  public static Hangar _Read_Hangar(NetworkReader reader) => (Hangar) reader.ReadNetworkBehaviour();

  public static void _Write_LiveryKey(NetworkWriter writer, LiveryKey value)
  {
    GeneratedNetworkCode._Write_LiveryKey\u002FKeyType(writer, value.Type);
    writer.WritePackedInt32(value.Index);
    writer.WriteString(value.AppDataName);
    writer.WritePackedUInt64(value.Id);
  }

  public static LiveryKey _Read_LiveryKey(NetworkReader reader)
  {
    return new LiveryKey()
    {
      Type = GeneratedNetworkCode._Read_LiveryKey\u002FKeyType(reader),
      Index = reader.ReadPackedInt32(),
      AppDataName = reader.ReadString(),
      Id = reader.ReadPackedUInt64()
    };
  }

  public static void _Write_NuclearOption\u002ESavedMission\u002ELoadout(
    NetworkWriter writer,
    Loadout value)
  {
    if (value == null)
    {
      writer.WriteBooleanExtension(false);
    }
    else
    {
      writer.WriteBooleanExtension(true);
      GeneratedNetworkCode._Write_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CWeaponMount\u003E(writer, value.weapons);
    }
  }

  public static void _Write_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CWeaponMount\u003E(
    NetworkWriter writer,
    List<WeaponMount> value)
  {
    writer.WriteList<WeaponMount>(value);
  }

  public static Loadout _Read_NuclearOption\u002ESavedMission\u002ELoadout(NetworkReader reader)
  {
    if (!reader.ReadBooleanExtension())
      return (Loadout) null;
    return new Loadout()
    {
      weapons = GeneratedNetworkCode._Read_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CWeaponMount\u003E(reader)
    };
  }

  public static List<WeaponMount> _Read_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CWeaponMount\u003E(
    NetworkReader reader)
  {
    return reader.ReadList<WeaponMount>();
  }

  public static void _Write_NuclearOption\u002ENetworking\u002EPlayer(
    NetworkWriter writer,
    Player value)
  {
    writer.WriteNetworkBehaviour((NetworkBehaviour) value);
  }

  public static Player _Read_NuclearOption\u002ENetworking\u002EPlayer(NetworkReader reader)
  {
    return (Player) reader.ReadNetworkBehaviour();
  }

  public static void _Write_FactionHQ\u002FRewardType(
    NetworkWriter writer,
    FactionHQ.RewardType value)
  {
    writer.WriteByteExtension((byte) value);
  }

  public static FactionHQ.RewardType _Read_FactionHQ\u002FRewardType(NetworkReader reader)
  {
    return (FactionHQ.RewardType) reader.ReadByteExtension();
  }

  public static void _Write_KillType(NetworkWriter writer, KillType value)
  {
    writer.WriteByteExtension((byte) value);
  }

  public static KillType _Read_KillType(NetworkReader reader)
  {
    return (KillType) reader.ReadByteExtension();
  }

  public static void _Write_Unit\u002FUnitState(NetworkWriter writer, Unit.UnitState value)
  {
    writer.WriteByteExtension((byte) value);
  }

  public static Unit.UnitState _Read_Unit\u002FUnitState(NetworkReader reader)
  {
    return (Unit.UnitState) reader.ReadByteExtension();
  }

  public static void _Write_WeaponMask(NetworkWriter writer, WeaponMask value)
  {
    writer.WritePackedInt32(value.Mask);
  }

  public static WeaponMask _Read_WeaponMask(NetworkReader reader)
  {
    return new WeaponMask()
    {
      Mask = reader.ReadPackedInt32()
    };
  }

  public static void _Write_Unit\u002FJamEventArgs(NetworkWriter writer, Unit.JamEventArgs value)
  {
    GeneratedNetworkCode._Write_Unit(writer, value.jammingUnit);
    writer.WriteSingleConverter(value.jamAmount);
  }

  public static void _Write_Unit(NetworkWriter writer, Unit value)
  {
    writer.WriteNetworkBehaviour((NetworkBehaviour) value);
  }

  public static Unit.JamEventArgs _Read_Unit\u002FJamEventArgs(NetworkReader reader)
  {
    return new Unit.JamEventArgs()
    {
      jammingUnit = GeneratedNetworkCode._Read_Unit(reader),
      jamAmount = reader.ReadSingleConverter()
    };
  }

  public static Unit _Read_Unit(NetworkReader reader) => (Unit) reader.ReadNetworkBehaviour();

  public static void _Write_System\u002EReadOnlySpan\u00601\u003CPersistentID\u003E(
    NetworkWriter writer,
    ReadOnlySpan<PersistentID> value)
  {
    writer.WriteReadOnlySpan<PersistentID>(value);
  }

  public static ReadOnlySpan<PersistentID> _Read_System\u002EReadOnlySpan\u00601\u003CPersistentID\u003E(
    NetworkReader reader)
  {
    return reader.ReadReadOnlySpan<PersistentID>();
  }

  public static void _Write_DamageInfo(NetworkWriter writer, DamageInfo value)
  {
    GeneratedNetworkCode._Write_CompressedFloat(writer, value.pierceDamage);
    GeneratedNetworkCode._Write_CompressedFloat(writer, value.blastDamage);
    GeneratedNetworkCode._Write_CompressedFloat(writer, value.fireDamage);
    GeneratedNetworkCode._Write_CompressedFloat(writer, value.impactDamage);
  }

  public static DamageInfo _Read_DamageInfo(NetworkReader reader)
  {
    return new DamageInfo()
    {
      pierceDamage = GeneratedNetworkCode._Read_CompressedFloat(reader),
      blastDamage = GeneratedNetworkCode._Read_CompressedFloat(reader),
      fireDamage = GeneratedNetworkCode._Read_CompressedFloat(reader),
      impactDamage = GeneratedNetworkCode._Read_CompressedFloat(reader)
    };
  }

  public static void _Write_System\u002EInt32\u005B\u005D(NetworkWriter writer, int[] value)
  {
    writer.WriteArray<int>(value);
  }

  public static int[] _Read_System\u002EInt32\u005B\u005D(NetworkReader reader)
  {
    return reader.ReadArray<int>();
  }

  public static void _Write_SlingloadHook\u002FDeployState(
    NetworkWriter writer,
    SlingloadHook.DeployState value)
  {
    writer.WriteByteExtension((byte) value);
  }

  public static SlingloadHook.DeployState _Read_SlingloadHook\u002FDeployState(NetworkReader reader)
  {
    return (SlingloadHook.DeployState) reader.ReadByteExtension();
  }

  public static void _Write_RearmEventArgs(NetworkWriter writer, RearmEventArgs value)
  {
    GeneratedNetworkCode._Write_Unit(writer, value.rearmer);
  }

  public static RearmEventArgs _Read_RearmEventArgs(NetworkReader reader)
  {
    return new RearmEventArgs()
    {
      rearmer = GeneratedNetworkCode._Read_Unit(reader)
    };
  }

  public static void _Write_Aircraft(NetworkWriter writer, Aircraft value)
  {
    writer.WriteNetworkBehaviour((NetworkBehaviour) value);
  }

  public static Aircraft _Read_Aircraft(NetworkReader reader)
  {
    return (Aircraft) reader.ReadNetworkBehaviour();
  }

  public static void _Write_System\u002ENullable\u00601\u003CSystem\u002EByte\u003E(
    NetworkWriter writer,
    byte? value)
  {
    writer.WriteNullable<byte>(value);
  }

  public static byte? _Read_System\u002ENullable\u00601\u003CSystem\u002EByte\u003E(
    NetworkReader reader)
  {
    return reader.ReadNullable<byte>();
  }

  public static void _Write_Hangar\u002FDoorState(NetworkWriter writer, Hangar.DoorState value)
  {
    writer.WriteBooleanExtension(value.opening);
    writer.WriteSingleConverter(value.openAmount);
  }

  public static Hangar.DoorState _Read_Hangar\u002FDoorState(NetworkReader reader)
  {
    return new Hangar.DoorState()
    {
      opening = reader.ReadBooleanExtension(),
      openAmount = reader.ReadSingleConverter()
    };
  }

  public static void _Write_UnitCommand\u002FCommand(
    NetworkWriter writer,
    UnitCommand.Command value)
  {
    writer.WriteSingleConverter(value.time);
    GeneratedNetworkCode._Write_NuclearOption\u002ENetworking\u002EPlayer(writer, value.player);
    writer.WriteGlobalPosition(value.position);
  }

  public static UnitCommand.Command _Read_UnitCommand\u002FCommand(NetworkReader reader)
  {
    return new UnitCommand.Command()
    {
      time = reader.ReadSingleConverter(),
      player = GeneratedNetworkCode._Read_NuclearOption\u002ENetworking\u002EPlayer(reader),
      position = reader.ReadGlobalPosition()
    };
  }

  public static void _Write_PilotDismounted\u002FPilotState(
    NetworkWriter writer,
    PilotDismounted.PilotState value)
  {
    writer.WriteByteExtension((byte) value);
  }

  public static PilotDismounted.PilotState _Read_PilotDismounted\u002FPilotState(
    NetworkReader reader)
  {
    return (PilotDismounted.PilotState) reader.ReadByteExtension();
  }

  public static void _Write_Missile\u002FSeekerMode(NetworkWriter writer, Missile.SeekerMode value)
  {
    writer.WriteByteExtension((byte) value);
  }

  public static Missile.SeekerMode _Read_Missile\u002FSeekerMode(NetworkReader reader)
  {
    return (Missile.SeekerMode) reader.ReadByteExtension();
  }

  public static void _Write_NuclearOption\u002EOwnedAirframe(
    NetworkWriter writer,
    OwnedAirframe value)
  {
    writer.WriteAircraftDefinition(value.Definition);
    writer.WriteBooleanExtension(value.Reserved);
  }

  public static void _Write_System\u002ENullable\u00601\u003CNuclearOption\u002EOwnedAirframe\u003E(
    NetworkWriter writer,
    OwnedAirframe? value)
  {
    writer.WriteNullable<OwnedAirframe>(value);
  }

  public static OwnedAirframe _Read_NuclearOption\u002EOwnedAirframe(NetworkReader reader)
  {
    return new OwnedAirframe()
    {
      Definition = reader.ReadAircraftDefinition(),
      Reserved = reader.ReadBooleanExtension()
    };
  }

  public static OwnedAirframe? _Read_System\u002ENullable\u00601\u003CNuclearOption\u002EOwnedAirframe\u003E(
    NetworkReader reader)
  {
    return reader.ReadNullable<OwnedAirframe>();
  }

  public static void _Write_NuclearOption\u002EReserveNotice(
    NetworkWriter writer,
    ReserveNotice value)
  {
    GeneratedNetworkCode._Write_NuclearOption\u002EReserveEvent(writer, value.outcome);
    writer.WriteAircraftDefinition(value.aircraftDefinition);
    writer.WriteBooleanExtension(value.isReserving);
    writer.WritePackedInt32(value.queuePosition);
  }

  public static void _Write_NuclearOption\u002EReserveEvent(
    NetworkWriter writer,
    ReserveEvent value)
  {
    writer.WriteByteExtension((byte) value);
  }

  public static ReserveNotice _Read_NuclearOption\u002EReserveNotice(NetworkReader reader)
  {
    return new ReserveNotice()
    {
      outcome = GeneratedNetworkCode._Read_NuclearOption\u002EReserveEvent(reader),
      aircraftDefinition = reader.ReadAircraftDefinition(),
      isReserving = reader.ReadBooleanExtension(),
      queuePosition = reader.ReadPackedInt32()
    };
  }

  public static ReserveEvent _Read_NuclearOption\u002EReserveEvent(NetworkReader reader)
  {
    return (ReserveEvent) reader.ReadByteExtension();
  }

  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
  public static void InitReadWriters()
  {
    Writer<INetworkDefinition>.Write = new Action<NetworkWriter, INetworkDefinition>(DefinitionWriters.WriteNetworkDefinition);
    Writer<UnitDefinition>.Write = new Action<NetworkWriter, UnitDefinition>(DefinitionWriters.WriteUnitDefinition);
    Writer<AircraftDefinition>.Write = new Action<NetworkWriter, AircraftDefinition>(DefinitionWriters.WriteAircraftDefinition);
    Writer<VehicleDefinition>.Write = new Action<NetworkWriter, VehicleDefinition>(DefinitionWriters.WriteVehicleDefinition);
    Writer<MissileDefinition>.Write = new Action<NetworkWriter, MissileDefinition>(DefinitionWriters.WriteMissileDefinition);
    Writer<BuildingDefinition>.Write = new Action<NetworkWriter, BuildingDefinition>(DefinitionWriters.WriteBuildingDefinition);
    Writer<ShipDefinition>.Write = new Action<NetworkWriter, ShipDefinition>(DefinitionWriters.WriteShipDefinition);
    Writer<WeaponMount>.Write = new Action<NetworkWriter, WeaponMount>(DefinitionWriters.WriteWeaponMount);
    Writer<GlobalPosition>.Write = new Action<NetworkWriter, GlobalPosition>(GlobalPositionExtensions.WriteGlobalPosition);
    Writer<GameObjectSyncvar>.Write = new Action<NetworkWriter, GameObjectSyncvar>(GameObjectSerializers.WriteGameObjectSyncVar);
    Writer<NetworkBehaviorSyncvar>.Write = new Action<NetworkWriter, NetworkBehaviorSyncvar>(NetworkBehaviorSerializers.WriteNetworkBehaviorSyncVar);
    Writer<NetworkIdentitySyncvar>.Write = new Action<NetworkWriter, NetworkIdentitySyncvar>(NetworkIdentitySerializers.WriteNetworkIdentitySyncVar);
    Writer<SyncPrefab>.Write = new Action<NetworkWriter, SyncPrefab>(SyncPrefabSerialize.WriteSyncPrefab);
    Writer<byte[]>.Write = new Action<NetworkWriter, byte[]>(CollectionExtensions.WriteBytesAndSize);
    Writer<ArraySegment<byte>>.Write = new Action<NetworkWriter, ArraySegment<byte>>(CollectionExtensions.WriteBytesAndSizeSegment);
    Writer<Quaternion>.Write = new Action<NetworkWriter, Quaternion>(CompressedExtensions.WriteQuaternion);
    Writer<NetworkIdentity>.Write = new Action<NetworkWriter, NetworkIdentity>(MirageTypesExtensions.WriteNetworkIdentity);
    Writer<NetworkBehaviour>.Write = new Action<NetworkWriter, NetworkBehaviour>(MirageTypesExtensions.WriteNetworkBehaviour);
    Writer<GameObject>.Write = new Action<NetworkWriter, GameObject>(MirageTypesExtensions.WriteGameObject);
    Writer<int>.Write = new Action<NetworkWriter, int>(PackedExtensions.WritePackedInt32);
    Writer<uint>.Write = new Action<NetworkWriter, uint>(PackedExtensions.WritePackedUInt32);
    Writer<long>.Write = new Action<NetworkWriter, long>(PackedExtensions.WritePackedInt64);
    Writer<ulong>.Write = new Action<NetworkWriter, ulong>(PackedExtensions.WritePackedUInt64);
    Writer<string>.Write = new Action<NetworkWriter, string>(StringExtensions.WriteString);
    Writer<byte>.Write = new Action<NetworkWriter, byte>(SystemTypesExtensions.WriteByteExtension);
    Writer<sbyte>.Write = new Action<NetworkWriter, sbyte>(SystemTypesExtensions.WriteSByteExtension);
    Writer<char>.Write = new Action<NetworkWriter, char>(SystemTypesExtensions.WriteChar);
    Writer<bool>.Write = new Action<NetworkWriter, bool>(SystemTypesExtensions.WriteBooleanExtension);
    Writer<ushort>.Write = new Action<NetworkWriter, ushort>(SystemTypesExtensions.WriteUInt16Extension);
    Writer<short>.Write = new Action<NetworkWriter, short>(SystemTypesExtensions.WriteInt16Extension);
    Writer<float>.Write = new Action<NetworkWriter, float>(SystemTypesExtensions.WriteSingleConverter);
    Writer<double>.Write = new Action<NetworkWriter, double>(SystemTypesExtensions.WriteDoubleConverter);
    Writer<Decimal>.Write = new Action<NetworkWriter, Decimal>(SystemTypesExtensions.WriteDecimalConverter);
    Writer<Guid>.Write = new Action<NetworkWriter, Guid>(SystemTypesExtensions.WriteGuid);
    Writer<Vector2>.Write = new Action<NetworkWriter, Vector2>(UnityTypesExtensions.WriteVector2);
    Writer<Vector3>.Write = new Action<NetworkWriter, Vector3>(UnityTypesExtensions.WriteVector3);
    Writer<Vector4>.Write = new Action<NetworkWriter, Vector4>(UnityTypesExtensions.WriteVector4);
    Writer<Vector2Int>.Write = new Action<NetworkWriter, Vector2Int>(UnityTypesExtensions.WriteVector2Int);
    Writer<Vector3Int>.Write = new Action<NetworkWriter, Vector3Int>(UnityTypesExtensions.WriteVector3Int);
    Writer<Color>.Write = new Action<NetworkWriter, Color>(UnityTypesExtensions.WriteColor);
    Writer<Color32>.Write = new Action<NetworkWriter, Color32>(UnityTypesExtensions.WriteColor32);
    Writer<Rect>.Write = new Action<NetworkWriter, Rect>(UnityTypesExtensions.WriteRect);
    Writer<Plane>.Write = new Action<NetworkWriter, Plane>(UnityTypesExtensions.WritePlane);
    Writer<Ray>.Write = new Action<NetworkWriter, Ray>(UnityTypesExtensions.WriteRay);
    Writer<Matrix4x4>.Write = new Action<NetworkWriter, Matrix4x4>(UnityTypesExtensions.WriteMatrix4X4);
    Writer<SceneNotReadyMessage>.Write = new Action<NetworkWriter, SceneNotReadyMessage>(GeneratedNetworkCode._Write_Mirage\u002ESceneNotReadyMessage);
    Writer<AddCharacterMessage>.Write = new Action<NetworkWriter, AddCharacterMessage>(GeneratedNetworkCode._Write_Mirage\u002EAddCharacterMessage);
    Writer<SceneMessage>.Write = new Action<NetworkWriter, SceneMessage>(GeneratedNetworkCode._Write_Mirage\u002ESceneMessage);
    Writer<SceneOperation>.Write = new Action<NetworkWriter, SceneOperation>(GeneratedNetworkCode._Write_Mirage\u002ESceneOperation);
    Writer<List<string>>.Write = new Action<NetworkWriter, List<string>>(GeneratedNetworkCode._Write_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CSystem\u002EString\u003E);
    Writer<SceneReadyMessage>.Write = new Action<NetworkWriter, SceneReadyMessage>(GeneratedNetworkCode._Write_Mirage\u002ESceneReadyMessage);
    Writer<SpawnMessage>.Write = new Action<NetworkWriter, SpawnMessage>(GeneratedNetworkCode._Write_Mirage\u002ESpawnMessage);
    Writer<ulong?>.Write = new Action<NetworkWriter, ulong?>(GeneratedNetworkCode._Write_System\u002ENullable\u00601\u003CSystem\u002EUInt64\u003E);
    Writer<int?>.Write = new Action<NetworkWriter, int?>(GeneratedNetworkCode._Write_System\u002ENullable\u00601\u003CSystem\u002EInt32\u003E);
    Writer<SpawnValues>.Write = new Action<NetworkWriter, SpawnValues>(GeneratedNetworkCode._Write_Mirage\u002ESpawnValues);
    Writer<Vector3?>.Write = new Action<NetworkWriter, Vector3?>(GeneratedNetworkCode._Write_System\u002ENullable\u00601\u003CUnityEngine\u002EVector3\u003E);
    Writer<Quaternion?>.Write = new Action<NetworkWriter, Quaternion?>(GeneratedNetworkCode._Write_System\u002ENullable\u00601\u003CUnityEngine\u002EQuaternion\u003E);
    Writer<bool?>.Write = new Action<NetworkWriter, bool?>(GeneratedNetworkCode._Write_System\u002ENullable\u00601\u003CSystem\u002EBoolean\u003E);
    Writer<RemoveAuthorityMessage>.Write = new Action<NetworkWriter, RemoveAuthorityMessage>(GeneratedNetworkCode._Write_Mirage\u002ERemoveAuthorityMessage);
    Writer<RemoveCharacterMessage>.Write = new Action<NetworkWriter, RemoveCharacterMessage>(GeneratedNetworkCode._Write_Mirage\u002ERemoveCharacterMessage);
    Writer<ObjectDestroyMessage>.Write = new Action<NetworkWriter, ObjectDestroyMessage>(GeneratedNetworkCode._Write_Mirage\u002EObjectDestroyMessage);
    Writer<ObjectHideMessage>.Write = new Action<NetworkWriter, ObjectHideMessage>(GeneratedNetworkCode._Write_Mirage\u002EObjectHideMessage);
    Writer<UpdateVarsMessage>.Write = new Action<NetworkWriter, UpdateVarsMessage>(GeneratedNetworkCode._Write_Mirage\u002EUpdateVarsMessage);
    Writer<NetworkPingMessage>.Write = new Action<NetworkWriter, NetworkPingMessage>(GeneratedNetworkCode._Write_Mirage\u002ENetworkPingMessage);
    Writer<NetworkPongMessage>.Write = new Action<NetworkWriter, NetworkPongMessage>(GeneratedNetworkCode._Write_Mirage\u002ENetworkPongMessage);
    Writer<RpcMessage>.Write = new Action<NetworkWriter, RpcMessage>(GeneratedNetworkCode._Write_Mirage\u002ERemoteCalls\u002ERpcMessage);
    Writer<RpcWithReplyMessage>.Write = new Action<NetworkWriter, RpcWithReplyMessage>(GeneratedNetworkCode._Write_Mirage\u002ERemoteCalls\u002ERpcWithReplyMessage);
    Writer<RpcReply>.Write = new Action<NetworkWriter, RpcReply>(GeneratedNetworkCode._Write_Mirage\u002ERemoteCalls\u002ERpcReply);
    Writer<Mirage.Authentication.AuthMessage>.Write = new Action<NetworkWriter, Mirage.Authentication.AuthMessage>(GeneratedNetworkCode._Write_Mirage\u002EAuthentication\u002EAuthMessage);
    Writer<AuthSuccessMessage>.Write = new Action<NetworkWriter, AuthSuccessMessage>(GeneratedNetworkCode._Write_Mirage\u002EAuthentication\u002EAuthSuccessMessage);
    Writer<NetworkMission.SyncMissionPart>.Write = new Action<NetworkWriter, NetworkMission.SyncMissionPart>(GeneratedNetworkCode._Write_NetworkMission\u002FSyncMissionPart);
    Writer<NetworkMission.SyncMissionHeader>.Write = new Action<NetworkWriter, NetworkMission.SyncMissionHeader>(GeneratedNetworkCode._Write_NetworkMission\u002FSyncMissionHeader);
    Writer<NetworkMission.State>.Write = new Action<NetworkWriter, NetworkMission.State>(GeneratedNetworkCode._Write_NetworkMission\u002FState);
    Writer<MissionSettings>.Write = new Action<NetworkWriter, MissionSettings>(GeneratedNetworkCode._Write_NuclearOption\u002ESavedMission\u002EMissionSettings);
    Writer<MissionTag>.Write = new Action<NetworkWriter, MissionTag>(GeneratedNetworkCode._Write_NuclearOption\u002ESavedMission\u002EMissionTag);
    Writer<List<MissionTag>>.Write = new Action<NetworkWriter, List<MissionTag>>(GeneratedNetworkCode._Write_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002EMissionTag\u003E);
    Writer<PlayerMode>.Write = new Action<NetworkWriter, PlayerMode>(GeneratedNetworkCode._Write_NuclearOption\u002ESavedMission\u002EPlayerMode);
    Writer<Override<PositionRotation>>.Write = new Action<NetworkWriter, Override<PositionRotation>>(GeneratedNetworkCode._Write_NuclearOption\u002ESavedMission\u002EObjectiveV2\u002EOverride\u00601\u003CNuclearOption\u002ESavedMission\u002EPositionRotation\u003E);
    Writer<PositionRotation>.Write = new Action<NetworkWriter, PositionRotation>(GeneratedNetworkCode._Write_NuclearOption\u002ESavedMission\u002EPositionRotation);
    Writer<RoadNetwork>.Write = new Action<NetworkWriter, RoadNetwork>(GeneratedNetworkCode._Write_RoadPathfinding\u002ERoadNetwork);
    Writer<Road>.Write = new Action<NetworkWriter, Road>(GeneratedNetworkCode._Write_RoadPathfinding\u002ERoad);
    Writer<List<GlobalPosition>>.Write = new Action<NetworkWriter, List<GlobalPosition>>(GeneratedNetworkCode._Write_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CGlobalPosition\u003E);
    Writer<List<Road>>.Write = new Action<NetworkWriter, List<Road>>(GeneratedNetworkCode._Write_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CRoadPathfinding\u002ERoad\u003E);
    Writer<MissionEnvironment>.Write = new Action<NetworkWriter, MissionEnvironment>(GeneratedNetworkCode._Write_NuclearOption\u002ESavedMission\u002EMissionEnvironment);
    Writer<NetworkMission.SyncMissionFooter>.Write = new Action<NetworkWriter, NetworkMission.SyncMissionFooter>(GeneratedNetworkCode._Write_NetworkMission\u002FSyncMissionFooter);
    Writer<NetworkMission.SyncMission>.Write = new Action<NetworkWriter, NetworkMission.SyncMission>(GeneratedNetworkCode._Write_NetworkMission\u002FSyncMission);
    Writer<Mission>.Write = new Action<NetworkWriter, Mission>(GeneratedNetworkCode._Write_NuclearOption\u002ESavedMission\u002EMission);
    Writer<MapKey>.Write = new Action<NetworkWriter, MapKey>(GeneratedNetworkCode._Write_NuclearOption\u002ESceneLoading\u002EMapKey);
    Writer<MapKey.KeyType>.Write = new Action<NetworkWriter, MapKey.KeyType>(GeneratedNetworkCode._Write_NuclearOption\u002ESceneLoading\u002EMapKey\u002FKeyType);
    Writer<SavedAircraft>.Write = new Action<NetworkWriter, SavedAircraft>(GeneratedNetworkCode._Write_NuclearOption\u002ESavedMission\u002ESavedAircraft);
    Writer<LoadoutOld>.Write = new Action<NetworkWriter, LoadoutOld>(GeneratedNetworkCode._Write_NuclearOption\u002ESavedMission\u002ELoadoutOld);
    Writer<List<byte>>.Write = new Action<NetworkWriter, List<byte>>(GeneratedNetworkCode._Write_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CSystem\u002EByte\u003E);
    Writer<SavedLoadout>.Write = new Action<NetworkWriter, SavedLoadout>(GeneratedNetworkCode._Write_NuclearOption\u002ESavedMission\u002ESavedLoadout);
    Writer<SavedLoadout.SelectedMount>.Write = new Action<NetworkWriter, SavedLoadout.SelectedMount>(GeneratedNetworkCode._Write_NuclearOption\u002ESavedMission\u002ESavedLoadout\u002FSelectedMount);
    Writer<List<SavedLoadout.SelectedMount>>.Write = new Action<NetworkWriter, List<SavedLoadout.SelectedMount>>(GeneratedNetworkCode._Write_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002ESavedLoadout\u002FSelectedMount\u003E);
    Writer<LiveryKey.KeyType>.Write = new Action<NetworkWriter, LiveryKey.KeyType>(GeneratedNetworkCode._Write_LiveryKey\u002FKeyType);
    Writer<Override<float>>.Write = new Action<NetworkWriter, Override<float>>(GeneratedNetworkCode._Write_NuclearOption\u002ESavedMission\u002EObjectiveV2\u002EOverride\u00601\u003CSystem\u002ESingle\u003E);
    Writer<List<SavedAircraft>>.Write = new Action<NetworkWriter, List<SavedAircraft>>(GeneratedNetworkCode._Write_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002ESavedAircraft\u003E);
    Writer<SavedVehicle>.Write = new Action<NetworkWriter, SavedVehicle>(GeneratedNetworkCode._Write_NuclearOption\u002ESavedMission\u002ESavedVehicle);
    Writer<VehicleWaypoint>.Write = new Action<NetworkWriter, VehicleWaypoint>(GeneratedNetworkCode._Write_NuclearOption\u002ESavedMission\u002EVehicleWaypoint);
    Writer<List<VehicleWaypoint>>.Write = new Action<NetworkWriter, List<VehicleWaypoint>>(GeneratedNetworkCode._Write_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002EVehicleWaypoint\u003E);
    Writer<List<SavedVehicle>>.Write = new Action<NetworkWriter, List<SavedVehicle>>(GeneratedNetworkCode._Write_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002ESavedVehicle\u003E);
    Writer<SavedShip>.Write = new Action<NetworkWriter, SavedShip>(GeneratedNetworkCode._Write_NuclearOption\u002ESavedMission\u002ESavedShip);
    Writer<List<SavedShip>>.Write = new Action<NetworkWriter, List<SavedShip>>(GeneratedNetworkCode._Write_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002ESavedShip\u003E);
    Writer<SavedBuilding>.Write = new Action<NetworkWriter, SavedBuilding>(GeneratedNetworkCode._Write_NuclearOption\u002ESavedMission\u002ESavedBuilding);
    Writer<SavedBuilding.FactoryOptions>.Write = new Action<NetworkWriter, SavedBuilding.FactoryOptions>(GeneratedNetworkCode._Write_NuclearOption\u002ESavedMission\u002ESavedBuilding\u002FFactoryOptions);
    Writer<List<SavedBuilding>>.Write = new Action<NetworkWriter, List<SavedBuilding>>(GeneratedNetworkCode._Write_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002ESavedBuilding\u003E);
    Writer<SavedScenery>.Write = new Action<NetworkWriter, SavedScenery>(GeneratedNetworkCode._Write_NuclearOption\u002ESavedMission\u002ESavedScenery);
    Writer<List<SavedScenery>>.Write = new Action<NetworkWriter, List<SavedScenery>>(GeneratedNetworkCode._Write_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002ESavedScenery\u003E);
    Writer<SavedContainer>.Write = new Action<NetworkWriter, SavedContainer>(GeneratedNetworkCode._Write_NuclearOption\u002ESavedMission\u002ESavedContainer);
    Writer<List<SavedContainer>>.Write = new Action<NetworkWriter, List<SavedContainer>>(GeneratedNetworkCode._Write_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002ESavedContainer\u003E);
    Writer<SavedMissile>.Write = new Action<NetworkWriter, SavedMissile>(GeneratedNetworkCode._Write_NuclearOption\u002ESavedMission\u002ESavedMissile);
    Writer<List<SavedMissile>>.Write = new Action<NetworkWriter, List<SavedMissile>>(GeneratedNetworkCode._Write_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002ESavedMissile\u003E);
    Writer<SavedPilot>.Write = new Action<NetworkWriter, SavedPilot>(GeneratedNetworkCode._Write_NuclearOption\u002ESavedMission\u002ESavedPilot);
    Writer<List<SavedPilot>>.Write = new Action<NetworkWriter, List<SavedPilot>>(GeneratedNetworkCode._Write_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002ESavedPilot\u003E);
    Writer<MissionFaction>.Write = new Action<NetworkWriter, MissionFaction>(GeneratedNetworkCode._Write_NuclearOption\u002ESavedMission\u002EMissionFaction);
    Writer<FactionSupply>.Write = new Action<NetworkWriter, FactionSupply>(GeneratedNetworkCode._Write_NuclearOption\u002ESavedMission\u002EFactionSupply);
    Writer<List<FactionSupply>>.Write = new Action<NetworkWriter, List<FactionSupply>>(GeneratedNetworkCode._Write_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002EFactionSupply\u003E);
    Writer<MissionObjective>.Write = new Action<NetworkWriter, MissionObjective>(GeneratedNetworkCode._Write_NuclearOption\u002ESavedMission\u002EMissionObjective);
    Writer<List<MissionObjective>>.Write = new Action<NetworkWriter, List<MissionObjective>>(GeneratedNetworkCode._Write_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002EMissionObjective\u003E);
    Writer<Restrictions>.Write = new Action<NetworkWriter, Restrictions>(GeneratedNetworkCode._Write_NuclearOption\u002ESavedMission\u002ERestrictions);
    Writer<List<MissionFaction>>.Write = new Action<NetworkWriter, List<MissionFaction>>(GeneratedNetworkCode._Write_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002EMissionFaction\u003E);
    Writer<SavedAirbase>.Write = new Action<NetworkWriter, SavedAirbase>(GeneratedNetworkCode._Write_NuclearOption\u002ESavedMission\u002ESavedAirbase);
    Writer<SavedRunway>.Write = new Action<NetworkWriter, SavedRunway>(GeneratedNetworkCode._Write_NuclearOption\u002ESavedMission\u002ESavedRunway);
    Writer<GlobalPosition[]>.Write = new Action<NetworkWriter, GlobalPosition[]>(GeneratedNetworkCode._Write_GlobalPosition\u005B\u005D);
    Writer<List<SavedRunway>>.Write = new Action<NetworkWriter, List<SavedRunway>>(GeneratedNetworkCode._Write_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002ESavedRunway\u003E);
    Writer<List<SavedAirbase>>.Write = new Action<NetworkWriter, List<SavedAirbase>>(GeneratedNetworkCode._Write_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002ESavedAirbase\u003E);
    Writer<UnitInventory>.Write = new Action<NetworkWriter, UnitInventory>(GeneratedNetworkCode._Write_NuclearOption\u002ESavedMission\u002EUnitInventory);
    Writer<StoredUnitCount>.Write = new Action<NetworkWriter, StoredUnitCount>(GeneratedNetworkCode._Write_NuclearOption\u002ESavedMission\u002EStoredUnitCount);
    Writer<List<StoredUnitCount>>.Write = new Action<NetworkWriter, List<StoredUnitCount>>(GeneratedNetworkCode._Write_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002EStoredUnitCount\u003E);
    Writer<List<UnitInventory>>.Write = new Action<NetworkWriter, List<UnitInventory>>(GeneratedNetworkCode._Write_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002EUnitInventory\u003E);
    Writer<SavedMissionObjectives>.Write = new Action<NetworkWriter, SavedMissionObjectives>(GeneratedNetworkCode._Write_NuclearOption\u002ESavedMission\u002EObjectiveV2\u002ESavedMissionObjectives);
    Writer<SavedObjective>.Write = new Action<NetworkWriter, SavedObjective>(GeneratedNetworkCode._Write_NuclearOption\u002ESavedMission\u002EObjectiveV2\u002ESavedObjective);
    Writer<ObjectiveType>.Write = new Action<NetworkWriter, ObjectiveType>(GeneratedNetworkCode._Write_NuclearOption\u002ESavedMission\u002EObjectiveV2\u002EObjectiveType);
    Writer<ObjectiveData>.Write = new Action<NetworkWriter, ObjectiveData>(GeneratedNetworkCode._Write_NuclearOption\u002ESavedMission\u002EObjectiveV2\u002EObjectiveData);
    Writer<List<ObjectiveData>>.Write = new Action<NetworkWriter, List<ObjectiveData>>(GeneratedNetworkCode._Write_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002EObjectiveV2\u002EObjectiveData\u003E);
    Writer<List<SavedObjective>>.Write = new Action<NetworkWriter, List<SavedObjective>>(GeneratedNetworkCode._Write_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002EObjectiveV2\u002ESavedObjective\u003E);
    Writer<SavedOutcome>.Write = new Action<NetworkWriter, SavedOutcome>(GeneratedNetworkCode._Write_NuclearOption\u002ESavedMission\u002EObjectiveV2\u002ESavedOutcome);
    Writer<OutcomeType>.Write = new Action<NetworkWriter, OutcomeType>(GeneratedNetworkCode._Write_NuclearOption\u002ESavedMission\u002EObjectiveV2\u002EOutcomeType);
    Writer<List<SavedOutcome>>.Write = new Action<NetworkWriter, List<SavedOutcome>>(GeneratedNetworkCode._Write_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002EObjectiveV2\u002ESavedOutcome\u003E);
    Writer<NetworkMission.SyncMissionStart>.Write = new Action<NetworkWriter, NetworkMission.SyncMissionStart>(GeneratedNetworkCode._Write_NetworkMission\u002FSyncMissionStart);
    Writer<LoadMapMessage>.Write = new Action<NetworkWriter, LoadMapMessage>(GeneratedNetworkCode._Write_NuclearOption\u002ESceneLoading\u002ELoadMapMessage);
    Writer<NetworkTransformBase.NetworkSnapshot>.Write = new Action<NetworkWriter, NetworkTransformBase.NetworkSnapshot>(GeneratedNetworkCode._Write_NuclearOption\u002ENetworkTransforms\u002ENetworkTransformBase\u002FNetworkSnapshot);
    Writer<CompressedInputs>.Write = new Action<NetworkWriter, CompressedInputs>(GeneratedNetworkCode._Write_CompressedInputs);
    Writer<CompressedFloat>.Write = new Action<NetworkWriter, CompressedFloat>(GeneratedNetworkCode._Write_CompressedFloat);
    Writer<CompressedInputs?>.Write = new Action<NetworkWriter, CompressedInputs?>(GeneratedNetworkCode._Write_System\u002ENullable\u00601\u003CCompressedInputs\u003E);
    Writer<double?>.Write = new Action<NetworkWriter, double?>(GeneratedNetworkCode._Write_System\u002ENullable\u00601\u003CSystem\u002EDouble\u003E);
    Writer<float?>.Write = new Action<NetworkWriter, float?>(GeneratedNetworkCode._Write_System\u002ENullable\u00601\u003CSystem\u002ESingle\u003E);
    Writer<Vector3Compressed>.Write = new Action<NetworkWriter, Vector3Compressed>(GeneratedNetworkCode._Write_Vector3Compressed);
    Writer<SendTransformBatcher.TransformMessage>.Write = new Action<NetworkWriter, SendTransformBatcher.TransformMessage>(GeneratedNetworkCode._Write_NuclearOption\u002ENetworkTransforms\u002ESendTransformBatcher\u002FTransformMessage);
    Writer<HostEndedMessage>.Write = new Action<NetworkWriter, HostEndedMessage>(GeneratedNetworkCode._Write_NuclearOption\u002ENetworking\u002EHostEndedMessage);
    Writer<LoadWaitingSceneMessage>.Write = new Action<NetworkWriter, LoadWaitingSceneMessage>(GeneratedNetworkCode._Write_NuclearOption\u002ENetworking\u002ELoadWaitingSceneMessage);
    Writer<ServerLoadingProgressMessage>.Write = new Action<NetworkWriter, ServerLoadingProgressMessage>(GeneratedNetworkCode._Write_NuclearOption\u002ENetworking\u002EServerLoadingProgressMessage);
    Writer<NetworkAuthenticatorNuclearOption.AuthMessage>.Write = new Action<NetworkWriter, NetworkAuthenticatorNuclearOption.AuthMessage>(GeneratedNetworkCode._Write_NuclearOption\u002ENetworking\u002EAuthentication\u002ENetworkAuthenticatorNuclearOption\u002FAuthMessage);
    Writer<PlayerType>.Write = new Action<NetworkWriter, PlayerType>(GeneratedNetworkCode._Write_NuclearOption\u002ENetworking\u002EPlayerType);
    Writer<NetworkAuthenticatorNuclearOption.PasswordChallenge>.Write = new Action<NetworkWriter, NetworkAuthenticatorNuclearOption.PasswordChallenge>(GeneratedNetworkCode._Write_NuclearOption\u002ENetworking\u002EAuthentication\u002ENetworkAuthenticatorNuclearOption\u002FPasswordChallenge);
    Writer<NetworkAuthenticatorNuclearOption.PasswordResponse>.Write = new Action<NetworkWriter, NetworkAuthenticatorNuclearOption.PasswordResponse>(GeneratedNetworkCode._Write_NuclearOption\u002ENetworking\u002EAuthentication\u002ENetworkAuthenticatorNuclearOption\u002FPasswordResponse);
    Writer<NetworkAuthenticatorNuclearOption.AuthFailReason>.Write = new Action<NetworkWriter, NetworkAuthenticatorNuclearOption.AuthFailReason>(GeneratedNetworkCode._Write_NuclearOption\u002ENetworking\u002EAuthentication\u002ENetworkAuthenticatorNuclearOption\u002FAuthFailReason);
    Writer<NetworkAuthenticatorNuclearOption.BuildHashMismatch>.Write = new Action<NetworkWriter, NetworkAuthenticatorNuclearOption.BuildHashMismatch>(GeneratedNetworkCode._Write_NuclearOption\u002ENetworking\u002EAuthentication\u002ENetworkAuthenticatorNuclearOption\u002FBuildHashMismatch);
    Writer<PersistentID>.Write = new Action<NetworkWriter, PersistentID>(GeneratedNetworkCode._Write_PersistentID);
    Writer<PlayerRef>.Write = new Action<NetworkWriter, PlayerRef>(GeneratedNetworkCode._Write_NuclearOption\u002ENetworking\u002EPlayerRef);
    Writer<Airbase>.Write = new Action<NetworkWriter, Airbase>(GeneratedNetworkCode._Write_Airbase);
    Writer<NetworkBehaviorSyncvar<Airbase>>.Write = new Action<NetworkWriter, NetworkBehaviorSyncvar<Airbase>>(GeneratedNetworkCode._Write_Mirage\u002ENetworkBehaviorSyncvar\u00601\u003CAirbase\u003E);
    Writer<FactionHQ.RuntimeSupply>.Write = new Action<NetworkWriter, FactionHQ.RuntimeSupply>(GeneratedNetworkCode._Write_FactionHQ\u002FRuntimeSupply);
    Writer<ExclusionZone>.Write = new Action<NetworkWriter, ExclusionZone>(GeneratedNetworkCode._Write_NuclearOption\u002EExclusionZone);
    Writer<EndType>.Write = new Action<NetworkWriter, EndType>(GeneratedNetworkCode._Write_NuclearOption\u002ESavedMission\u002EObjectiveV2\u002EOutcomes\u002EEndType);
    Writer<List<int>>.Write = new Action<NetworkWriter, List<int>>(GeneratedNetworkCode._Write_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CSystem\u002EInt32\u003E);
    Writer<FactionHQ>.Write = new Action<NetworkWriter, FactionHQ>(GeneratedNetworkCode._Write_FactionHQ);
    Writer<MissionStatsTracker.TypeStat>.Write = new Action<NetworkWriter, MissionStatsTracker.TypeStat>(GeneratedNetworkCode._Write_MissionStatsTracker\u002FTypeStat);
    Writer<MissionStatsTracker.Stat>.Write = new Action<NetworkWriter, MissionStatsTracker.Stat>(GeneratedNetworkCode._Write_MissionStatsTracker\u002FStat);
    Writer<Airbase.TrySpawnResult>.Write = new Action<NetworkWriter, Airbase.TrySpawnResult>(GeneratedNetworkCode._Write_Airbase\u002FTrySpawnResult);
    Writer<Hangar>.Write = new Action<NetworkWriter, Hangar>(GeneratedNetworkCode._Write_Hangar);
    Writer<LiveryKey>.Write = new Action<NetworkWriter, LiveryKey>(GeneratedNetworkCode._Write_LiveryKey);
    Writer<Loadout>.Write = new Action<NetworkWriter, Loadout>(GeneratedNetworkCode._Write_NuclearOption\u002ESavedMission\u002ELoadout);
    Writer<List<WeaponMount>>.Write = new Action<NetworkWriter, List<WeaponMount>>(GeneratedNetworkCode._Write_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CWeaponMount\u003E);
    Writer<Player>.Write = new Action<NetworkWriter, Player>(GeneratedNetworkCode._Write_NuclearOption\u002ENetworking\u002EPlayer);
    Writer<FactionHQ.RewardType>.Write = new Action<NetworkWriter, FactionHQ.RewardType>(GeneratedNetworkCode._Write_FactionHQ\u002FRewardType);
    Writer<KillType>.Write = new Action<NetworkWriter, KillType>(GeneratedNetworkCode._Write_KillType);
    Writer<Unit.UnitState>.Write = new Action<NetworkWriter, Unit.UnitState>(GeneratedNetworkCode._Write_Unit\u002FUnitState);
    Writer<WeaponMask>.Write = new Action<NetworkWriter, WeaponMask>(GeneratedNetworkCode._Write_WeaponMask);
    Writer<Unit.JamEventArgs>.Write = new Action<NetworkWriter, Unit.JamEventArgs>(GeneratedNetworkCode._Write_Unit\u002FJamEventArgs);
    Writer<Unit>.Write = new Action<NetworkWriter, Unit>(GeneratedNetworkCode._Write_Unit);
    Writer<ReadOnlySpan<PersistentID>>.Write = new Action<NetworkWriter, ReadOnlySpan<PersistentID>>(GeneratedNetworkCode._Write_System\u002EReadOnlySpan\u00601\u003CPersistentID\u003E);
    Writer<DamageInfo>.Write = new Action<NetworkWriter, DamageInfo>(GeneratedNetworkCode._Write_DamageInfo);
    Writer<int[]>.Write = new Action<NetworkWriter, int[]>(GeneratedNetworkCode._Write_System\u002EInt32\u005B\u005D);
    Writer<SlingloadHook.DeployState>.Write = new Action<NetworkWriter, SlingloadHook.DeployState>(GeneratedNetworkCode._Write_SlingloadHook\u002FDeployState);
    Writer<RearmEventArgs>.Write = new Action<NetworkWriter, RearmEventArgs>(GeneratedNetworkCode._Write_RearmEventArgs);
    Writer<Aircraft>.Write = new Action<NetworkWriter, Aircraft>(GeneratedNetworkCode._Write_Aircraft);
    Writer<byte?>.Write = new Action<NetworkWriter, byte?>(GeneratedNetworkCode._Write_System\u002ENullable\u00601\u003CSystem\u002EByte\u003E);
    Writer<Hangar.DoorState>.Write = new Action<NetworkWriter, Hangar.DoorState>(GeneratedNetworkCode._Write_Hangar\u002FDoorState);
    Writer<UnitCommand.Command>.Write = new Action<NetworkWriter, UnitCommand.Command>(GeneratedNetworkCode._Write_UnitCommand\u002FCommand);
    Writer<PilotDismounted.PilotState>.Write = new Action<NetworkWriter, PilotDismounted.PilotState>(GeneratedNetworkCode._Write_PilotDismounted\u002FPilotState);
    Writer<Missile.SeekerMode>.Write = new Action<NetworkWriter, Missile.SeekerMode>(GeneratedNetworkCode._Write_Missile\u002FSeekerMode);
    Writer<OwnedAirframe>.Write = new Action<NetworkWriter, OwnedAirframe>(GeneratedNetworkCode._Write_NuclearOption\u002EOwnedAirframe);
    Writer<OwnedAirframe?>.Write = new Action<NetworkWriter, OwnedAirframe?>(GeneratedNetworkCode._Write_System\u002ENullable\u00601\u003CNuclearOption\u002EOwnedAirframe\u003E);
    Writer<ReserveNotice>.Write = new Action<NetworkWriter, ReserveNotice>(GeneratedNetworkCode._Write_NuclearOption\u002EReserveNotice);
    Writer<ReserveEvent>.Write = new Action<NetworkWriter, ReserveEvent>(GeneratedNetworkCode._Write_NuclearOption\u002EReserveEvent);
    Reader<UnitDefinition>.Read = new Func<NetworkReader, UnitDefinition>(DefinitionWriters.ReadUnitDefinition);
    Reader<AircraftDefinition>.Read = new Func<NetworkReader, AircraftDefinition>(DefinitionWriters.ReadAircraftDefinition);
    Reader<VehicleDefinition>.Read = new Func<NetworkReader, VehicleDefinition>(DefinitionWriters.ReadVehicleDefinition);
    Reader<MissileDefinition>.Read = new Func<NetworkReader, MissileDefinition>(DefinitionWriters.ReadMissileDefinition);
    Reader<BuildingDefinition>.Read = new Func<NetworkReader, BuildingDefinition>(DefinitionWriters.ReadBuildingDefinition);
    Reader<ShipDefinition>.Read = new Func<NetworkReader, ShipDefinition>(DefinitionWriters.ReadShipDefinition);
    Reader<WeaponMount>.Read = new Func<NetworkReader, WeaponMount>(DefinitionWriters.ReadWeaponMount);
    Reader<GlobalPosition>.Read = new Func<NetworkReader, GlobalPosition>(GlobalPositionExtensions.ReadGlobalPosition);
    Reader<GameObjectSyncvar>.Read = new Func<NetworkReader, GameObjectSyncvar>(GameObjectSerializers.ReadGameObjectSyncVar);
    Reader<NetworkBehaviorSyncvar>.Read = new Func<NetworkReader, NetworkBehaviorSyncvar>(NetworkBehaviorSerializers.ReadNetworkBehaviourSyncVar);
    Reader<NetworkIdentitySyncvar>.Read = new Func<NetworkReader, NetworkIdentitySyncvar>(NetworkIdentitySerializers.ReadNetworkIdentitySyncVar);
    Reader<SyncPrefab>.Read = new Func<NetworkReader, SyncPrefab>(SyncPrefabSerialize.ReadSyncPrefab);
    Reader<byte[]>.Read = new Func<NetworkReader, byte[]>(CollectionExtensions.ReadBytesAndSize);
    Reader<ArraySegment<byte>>.Read = new Func<NetworkReader, ArraySegment<byte>>(CollectionExtensions.ReadBytesAndSizeSegment);
    Reader<Quaternion>.Read = new Func<NetworkReader, Quaternion>(CompressedExtensions.ReadQuaternion);
    Reader<MirageNetworkReader>.Read = new Func<NetworkReader, MirageNetworkReader>(MirageTypesExtensions.ToMirageReader);
    Reader<NetworkIdentity>.Read = new Func<NetworkReader, NetworkIdentity>(MirageTypesExtensions.ReadNetworkIdentity);
    Reader<NetworkBehaviour>.Read = new Func<NetworkReader, NetworkBehaviour>(MirageTypesExtensions.ReadNetworkBehaviour);
    Reader<GameObject>.Read = new Func<NetworkReader, GameObject>(MirageTypesExtensions.ReadGameObject);
    Reader<int>.Read = new Func<NetworkReader, int>(PackedExtensions.ReadPackedInt32);
    Reader<uint>.Read = new Func<NetworkReader, uint>(PackedExtensions.ReadPackedUInt32);
    Reader<long>.Read = new Func<NetworkReader, long>(PackedExtensions.ReadPackedInt64);
    Reader<ulong>.Read = new Func<NetworkReader, ulong>(PackedExtensions.ReadPackedUInt64);
    Reader<string>.Read = new Func<NetworkReader, string>(StringExtensions.ReadString);
    Reader<byte>.Read = new Func<NetworkReader, byte>(SystemTypesExtensions.ReadByteExtension);
    Reader<sbyte>.Read = new Func<NetworkReader, sbyte>(SystemTypesExtensions.ReadSByteExtension);
    Reader<char>.Read = new Func<NetworkReader, char>(SystemTypesExtensions.ReadChar);
    Reader<bool>.Read = new Func<NetworkReader, bool>(SystemTypesExtensions.ReadBooleanExtension);
    Reader<short>.Read = new Func<NetworkReader, short>(SystemTypesExtensions.ReadInt16Extension);
    Reader<ushort>.Read = new Func<NetworkReader, ushort>(SystemTypesExtensions.ReadUInt16Extension);
    Reader<float>.Read = new Func<NetworkReader, float>(SystemTypesExtensions.ReadSingleConverter);
    Reader<double>.Read = new Func<NetworkReader, double>(SystemTypesExtensions.ReadDoubleConverter);
    Reader<Decimal>.Read = new Func<NetworkReader, Decimal>(SystemTypesExtensions.ReadDecimalConverter);
    Reader<Guid>.Read = new Func<NetworkReader, Guid>(SystemTypesExtensions.ReadGuid);
    Reader<Vector2>.Read = new Func<NetworkReader, Vector2>(UnityTypesExtensions.ReadVector2);
    Reader<Vector3>.Read = new Func<NetworkReader, Vector3>(UnityTypesExtensions.ReadVector3);
    Reader<Vector4>.Read = new Func<NetworkReader, Vector4>(UnityTypesExtensions.ReadVector4);
    Reader<Vector2Int>.Read = new Func<NetworkReader, Vector2Int>(UnityTypesExtensions.ReadVector2Int);
    Reader<Vector3Int>.Read = new Func<NetworkReader, Vector3Int>(UnityTypesExtensions.ReadVector3Int);
    Reader<Color>.Read = new Func<NetworkReader, Color>(UnityTypesExtensions.ReadColor);
    Reader<Color32>.Read = new Func<NetworkReader, Color32>(UnityTypesExtensions.ReadColor32);
    Reader<Rect>.Read = new Func<NetworkReader, Rect>(UnityTypesExtensions.ReadRect);
    Reader<Plane>.Read = new Func<NetworkReader, Plane>(UnityTypesExtensions.ReadPlane);
    Reader<Ray>.Read = new Func<NetworkReader, Ray>(UnityTypesExtensions.ReadRay);
    Reader<Matrix4x4>.Read = new Func<NetworkReader, Matrix4x4>(UnityTypesExtensions.ReadMatrix4x4);
    Reader<SceneNotReadyMessage>.Read = new Func<NetworkReader, SceneNotReadyMessage>(GeneratedNetworkCode._Read_Mirage\u002ESceneNotReadyMessage);
    Reader<AddCharacterMessage>.Read = new Func<NetworkReader, AddCharacterMessage>(GeneratedNetworkCode._Read_Mirage\u002EAddCharacterMessage);
    Reader<SceneMessage>.Read = new Func<NetworkReader, SceneMessage>(GeneratedNetworkCode._Read_Mirage\u002ESceneMessage);
    Reader<SceneOperation>.Read = new Func<NetworkReader, SceneOperation>(GeneratedNetworkCode._Read_Mirage\u002ESceneOperation);
    Reader<List<string>>.Read = new Func<NetworkReader, List<string>>(GeneratedNetworkCode._Read_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CSystem\u002EString\u003E);
    Reader<SceneReadyMessage>.Read = new Func<NetworkReader, SceneReadyMessage>(GeneratedNetworkCode._Read_Mirage\u002ESceneReadyMessage);
    Reader<SpawnMessage>.Read = new Func<NetworkReader, SpawnMessage>(GeneratedNetworkCode._Read_Mirage\u002ESpawnMessage);
    Reader<ulong?>.Read = new Func<NetworkReader, ulong?>(GeneratedNetworkCode._Read_System\u002ENullable\u00601\u003CSystem\u002EUInt64\u003E);
    Reader<int?>.Read = new Func<NetworkReader, int?>(GeneratedNetworkCode._Read_System\u002ENullable\u00601\u003CSystem\u002EInt32\u003E);
    Reader<SpawnValues>.Read = new Func<NetworkReader, SpawnValues>(GeneratedNetworkCode._Read_Mirage\u002ESpawnValues);
    Reader<Vector3?>.Read = new Func<NetworkReader, Vector3?>(GeneratedNetworkCode._Read_System\u002ENullable\u00601\u003CUnityEngine\u002EVector3\u003E);
    Reader<Quaternion?>.Read = new Func<NetworkReader, Quaternion?>(GeneratedNetworkCode._Read_System\u002ENullable\u00601\u003CUnityEngine\u002EQuaternion\u003E);
    Reader<bool?>.Read = new Func<NetworkReader, bool?>(GeneratedNetworkCode._Read_System\u002ENullable\u00601\u003CSystem\u002EBoolean\u003E);
    Reader<RemoveAuthorityMessage>.Read = new Func<NetworkReader, RemoveAuthorityMessage>(GeneratedNetworkCode._Read_Mirage\u002ERemoveAuthorityMessage);
    Reader<RemoveCharacterMessage>.Read = new Func<NetworkReader, RemoveCharacterMessage>(GeneratedNetworkCode._Read_Mirage\u002ERemoveCharacterMessage);
    Reader<ObjectDestroyMessage>.Read = new Func<NetworkReader, ObjectDestroyMessage>(GeneratedNetworkCode._Read_Mirage\u002EObjectDestroyMessage);
    Reader<ObjectHideMessage>.Read = new Func<NetworkReader, ObjectHideMessage>(GeneratedNetworkCode._Read_Mirage\u002EObjectHideMessage);
    Reader<UpdateVarsMessage>.Read = new Func<NetworkReader, UpdateVarsMessage>(GeneratedNetworkCode._Read_Mirage\u002EUpdateVarsMessage);
    Reader<NetworkPingMessage>.Read = new Func<NetworkReader, NetworkPingMessage>(GeneratedNetworkCode._Read_Mirage\u002ENetworkPingMessage);
    Reader<NetworkPongMessage>.Read = new Func<NetworkReader, NetworkPongMessage>(GeneratedNetworkCode._Read_Mirage\u002ENetworkPongMessage);
    Reader<RpcMessage>.Read = new Func<NetworkReader, RpcMessage>(GeneratedNetworkCode._Read_Mirage\u002ERemoteCalls\u002ERpcMessage);
    Reader<RpcWithReplyMessage>.Read = new Func<NetworkReader, RpcWithReplyMessage>(GeneratedNetworkCode._Read_Mirage\u002ERemoteCalls\u002ERpcWithReplyMessage);
    Reader<RpcReply>.Read = new Func<NetworkReader, RpcReply>(GeneratedNetworkCode._Read_Mirage\u002ERemoteCalls\u002ERpcReply);
    Reader<Mirage.Authentication.AuthMessage>.Read = new Func<NetworkReader, Mirage.Authentication.AuthMessage>(GeneratedNetworkCode._Read_Mirage\u002EAuthentication\u002EAuthMessage);
    Reader<AuthSuccessMessage>.Read = new Func<NetworkReader, AuthSuccessMessage>(GeneratedNetworkCode._Read_Mirage\u002EAuthentication\u002EAuthSuccessMessage);
    Reader<NetworkMission.SyncMissionPart>.Read = new Func<NetworkReader, NetworkMission.SyncMissionPart>(GeneratedNetworkCode._Read_NetworkMission\u002FSyncMissionPart);
    Reader<NetworkMission.SyncMissionHeader>.Read = new Func<NetworkReader, NetworkMission.SyncMissionHeader>(GeneratedNetworkCode._Read_NetworkMission\u002FSyncMissionHeader);
    Reader<NetworkMission.State>.Read = new Func<NetworkReader, NetworkMission.State>(GeneratedNetworkCode._Read_NetworkMission\u002FState);
    Reader<MissionSettings>.Read = new Func<NetworkReader, MissionSettings>(GeneratedNetworkCode._Read_NuclearOption\u002ESavedMission\u002EMissionSettings);
    Reader<MissionTag>.Read = new Func<NetworkReader, MissionTag>(GeneratedNetworkCode._Read_NuclearOption\u002ESavedMission\u002EMissionTag);
    Reader<List<MissionTag>>.Read = new Func<NetworkReader, List<MissionTag>>(GeneratedNetworkCode._Read_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002EMissionTag\u003E);
    Reader<PlayerMode>.Read = new Func<NetworkReader, PlayerMode>(GeneratedNetworkCode._Read_NuclearOption\u002ESavedMission\u002EPlayerMode);
    Reader<Override<PositionRotation>>.Read = new Func<NetworkReader, Override<PositionRotation>>(GeneratedNetworkCode._Read_NuclearOption\u002ESavedMission\u002EObjectiveV2\u002EOverride\u00601\u003CNuclearOption\u002ESavedMission\u002EPositionRotation\u003E);
    Reader<PositionRotation>.Read = new Func<NetworkReader, PositionRotation>(GeneratedNetworkCode._Read_NuclearOption\u002ESavedMission\u002EPositionRotation);
    Reader<RoadNetwork>.Read = new Func<NetworkReader, RoadNetwork>(GeneratedNetworkCode._Read_RoadPathfinding\u002ERoadNetwork);
    Reader<Road>.Read = new Func<NetworkReader, Road>(GeneratedNetworkCode._Read_RoadPathfinding\u002ERoad);
    Reader<List<GlobalPosition>>.Read = new Func<NetworkReader, List<GlobalPosition>>(GeneratedNetworkCode._Read_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CGlobalPosition\u003E);
    Reader<List<Road>>.Read = new Func<NetworkReader, List<Road>>(GeneratedNetworkCode._Read_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CRoadPathfinding\u002ERoad\u003E);
    Reader<MissionEnvironment>.Read = new Func<NetworkReader, MissionEnvironment>(GeneratedNetworkCode._Read_NuclearOption\u002ESavedMission\u002EMissionEnvironment);
    Reader<NetworkMission.SyncMissionFooter>.Read = new Func<NetworkReader, NetworkMission.SyncMissionFooter>(GeneratedNetworkCode._Read_NetworkMission\u002FSyncMissionFooter);
    Reader<NetworkMission.SyncMission>.Read = new Func<NetworkReader, NetworkMission.SyncMission>(GeneratedNetworkCode._Read_NetworkMission\u002FSyncMission);
    Reader<Mission>.Read = new Func<NetworkReader, Mission>(GeneratedNetworkCode._Read_NuclearOption\u002ESavedMission\u002EMission);
    Reader<MapKey>.Read = new Func<NetworkReader, MapKey>(GeneratedNetworkCode._Read_NuclearOption\u002ESceneLoading\u002EMapKey);
    Reader<MapKey.KeyType>.Read = new Func<NetworkReader, MapKey.KeyType>(GeneratedNetworkCode._Read_NuclearOption\u002ESceneLoading\u002EMapKey\u002FKeyType);
    Reader<SavedAircraft>.Read = new Func<NetworkReader, SavedAircraft>(GeneratedNetworkCode._Read_NuclearOption\u002ESavedMission\u002ESavedAircraft);
    Reader<LoadoutOld>.Read = new Func<NetworkReader, LoadoutOld>(GeneratedNetworkCode._Read_NuclearOption\u002ESavedMission\u002ELoadoutOld);
    Reader<List<byte>>.Read = new Func<NetworkReader, List<byte>>(GeneratedNetworkCode._Read_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CSystem\u002EByte\u003E);
    Reader<SavedLoadout>.Read = new Func<NetworkReader, SavedLoadout>(GeneratedNetworkCode._Read_NuclearOption\u002ESavedMission\u002ESavedLoadout);
    Reader<SavedLoadout.SelectedMount>.Read = new Func<NetworkReader, SavedLoadout.SelectedMount>(GeneratedNetworkCode._Read_NuclearOption\u002ESavedMission\u002ESavedLoadout\u002FSelectedMount);
    Reader<List<SavedLoadout.SelectedMount>>.Read = new Func<NetworkReader, List<SavedLoadout.SelectedMount>>(GeneratedNetworkCode._Read_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002ESavedLoadout\u002FSelectedMount\u003E);
    Reader<LiveryKey.KeyType>.Read = new Func<NetworkReader, LiveryKey.KeyType>(GeneratedNetworkCode._Read_LiveryKey\u002FKeyType);
    Reader<Override<float>>.Read = new Func<NetworkReader, Override<float>>(GeneratedNetworkCode._Read_NuclearOption\u002ESavedMission\u002EObjectiveV2\u002EOverride\u00601\u003CSystem\u002ESingle\u003E);
    Reader<List<SavedAircraft>>.Read = new Func<NetworkReader, List<SavedAircraft>>(GeneratedNetworkCode._Read_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002ESavedAircraft\u003E);
    Reader<SavedVehicle>.Read = new Func<NetworkReader, SavedVehicle>(GeneratedNetworkCode._Read_NuclearOption\u002ESavedMission\u002ESavedVehicle);
    Reader<VehicleWaypoint>.Read = new Func<NetworkReader, VehicleWaypoint>(GeneratedNetworkCode._Read_NuclearOption\u002ESavedMission\u002EVehicleWaypoint);
    Reader<List<VehicleWaypoint>>.Read = new Func<NetworkReader, List<VehicleWaypoint>>(GeneratedNetworkCode._Read_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002EVehicleWaypoint\u003E);
    Reader<List<SavedVehicle>>.Read = new Func<NetworkReader, List<SavedVehicle>>(GeneratedNetworkCode._Read_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002ESavedVehicle\u003E);
    Reader<SavedShip>.Read = new Func<NetworkReader, SavedShip>(GeneratedNetworkCode._Read_NuclearOption\u002ESavedMission\u002ESavedShip);
    Reader<List<SavedShip>>.Read = new Func<NetworkReader, List<SavedShip>>(GeneratedNetworkCode._Read_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002ESavedShip\u003E);
    Reader<SavedBuilding>.Read = new Func<NetworkReader, SavedBuilding>(GeneratedNetworkCode._Read_NuclearOption\u002ESavedMission\u002ESavedBuilding);
    Reader<SavedBuilding.FactoryOptions>.Read = new Func<NetworkReader, SavedBuilding.FactoryOptions>(GeneratedNetworkCode._Read_NuclearOption\u002ESavedMission\u002ESavedBuilding\u002FFactoryOptions);
    Reader<List<SavedBuilding>>.Read = new Func<NetworkReader, List<SavedBuilding>>(GeneratedNetworkCode._Read_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002ESavedBuilding\u003E);
    Reader<SavedScenery>.Read = new Func<NetworkReader, SavedScenery>(GeneratedNetworkCode._Read_NuclearOption\u002ESavedMission\u002ESavedScenery);
    Reader<List<SavedScenery>>.Read = new Func<NetworkReader, List<SavedScenery>>(GeneratedNetworkCode._Read_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002ESavedScenery\u003E);
    Reader<SavedContainer>.Read = new Func<NetworkReader, SavedContainer>(GeneratedNetworkCode._Read_NuclearOption\u002ESavedMission\u002ESavedContainer);
    Reader<List<SavedContainer>>.Read = new Func<NetworkReader, List<SavedContainer>>(GeneratedNetworkCode._Read_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002ESavedContainer\u003E);
    Reader<SavedMissile>.Read = new Func<NetworkReader, SavedMissile>(GeneratedNetworkCode._Read_NuclearOption\u002ESavedMission\u002ESavedMissile);
    Reader<List<SavedMissile>>.Read = new Func<NetworkReader, List<SavedMissile>>(GeneratedNetworkCode._Read_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002ESavedMissile\u003E);
    Reader<SavedPilot>.Read = new Func<NetworkReader, SavedPilot>(GeneratedNetworkCode._Read_NuclearOption\u002ESavedMission\u002ESavedPilot);
    Reader<List<SavedPilot>>.Read = new Func<NetworkReader, List<SavedPilot>>(GeneratedNetworkCode._Read_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002ESavedPilot\u003E);
    Reader<MissionFaction>.Read = new Func<NetworkReader, MissionFaction>(GeneratedNetworkCode._Read_NuclearOption\u002ESavedMission\u002EMissionFaction);
    Reader<FactionSupply>.Read = new Func<NetworkReader, FactionSupply>(GeneratedNetworkCode._Read_NuclearOption\u002ESavedMission\u002EFactionSupply);
    Reader<List<FactionSupply>>.Read = new Func<NetworkReader, List<FactionSupply>>(GeneratedNetworkCode._Read_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002EFactionSupply\u003E);
    Reader<MissionObjective>.Read = new Func<NetworkReader, MissionObjective>(GeneratedNetworkCode._Read_NuclearOption\u002ESavedMission\u002EMissionObjective);
    Reader<List<MissionObjective>>.Read = new Func<NetworkReader, List<MissionObjective>>(GeneratedNetworkCode._Read_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002EMissionObjective\u003E);
    Reader<Restrictions>.Read = new Func<NetworkReader, Restrictions>(GeneratedNetworkCode._Read_NuclearOption\u002ESavedMission\u002ERestrictions);
    Reader<List<MissionFaction>>.Read = new Func<NetworkReader, List<MissionFaction>>(GeneratedNetworkCode._Read_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002EMissionFaction\u003E);
    Reader<SavedAirbase>.Read = new Func<NetworkReader, SavedAirbase>(GeneratedNetworkCode._Read_NuclearOption\u002ESavedMission\u002ESavedAirbase);
    Reader<SavedRunway>.Read = new Func<NetworkReader, SavedRunway>(GeneratedNetworkCode._Read_NuclearOption\u002ESavedMission\u002ESavedRunway);
    Reader<GlobalPosition[]>.Read = new Func<NetworkReader, GlobalPosition[]>(GeneratedNetworkCode._Read_GlobalPosition\u005B\u005D);
    Reader<List<SavedRunway>>.Read = new Func<NetworkReader, List<SavedRunway>>(GeneratedNetworkCode._Read_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002ESavedRunway\u003E);
    Reader<List<SavedAirbase>>.Read = new Func<NetworkReader, List<SavedAirbase>>(GeneratedNetworkCode._Read_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002ESavedAirbase\u003E);
    Reader<UnitInventory>.Read = new Func<NetworkReader, UnitInventory>(GeneratedNetworkCode._Read_NuclearOption\u002ESavedMission\u002EUnitInventory);
    Reader<StoredUnitCount>.Read = new Func<NetworkReader, StoredUnitCount>(GeneratedNetworkCode._Read_NuclearOption\u002ESavedMission\u002EStoredUnitCount);
    Reader<List<StoredUnitCount>>.Read = new Func<NetworkReader, List<StoredUnitCount>>(GeneratedNetworkCode._Read_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002EStoredUnitCount\u003E);
    Reader<List<UnitInventory>>.Read = new Func<NetworkReader, List<UnitInventory>>(GeneratedNetworkCode._Read_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002EUnitInventory\u003E);
    Reader<SavedMissionObjectives>.Read = new Func<NetworkReader, SavedMissionObjectives>(GeneratedNetworkCode._Read_NuclearOption\u002ESavedMission\u002EObjectiveV2\u002ESavedMissionObjectives);
    Reader<SavedObjective>.Read = new Func<NetworkReader, SavedObjective>(GeneratedNetworkCode._Read_NuclearOption\u002ESavedMission\u002EObjectiveV2\u002ESavedObjective);
    Reader<ObjectiveType>.Read = new Func<NetworkReader, ObjectiveType>(GeneratedNetworkCode._Read_NuclearOption\u002ESavedMission\u002EObjectiveV2\u002EObjectiveType);
    Reader<ObjectiveData>.Read = new Func<NetworkReader, ObjectiveData>(GeneratedNetworkCode._Read_NuclearOption\u002ESavedMission\u002EObjectiveV2\u002EObjectiveData);
    Reader<List<ObjectiveData>>.Read = new Func<NetworkReader, List<ObjectiveData>>(GeneratedNetworkCode._Read_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002EObjectiveV2\u002EObjectiveData\u003E);
    Reader<List<SavedObjective>>.Read = new Func<NetworkReader, List<SavedObjective>>(GeneratedNetworkCode._Read_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002EObjectiveV2\u002ESavedObjective\u003E);
    Reader<SavedOutcome>.Read = new Func<NetworkReader, SavedOutcome>(GeneratedNetworkCode._Read_NuclearOption\u002ESavedMission\u002EObjectiveV2\u002ESavedOutcome);
    Reader<OutcomeType>.Read = new Func<NetworkReader, OutcomeType>(GeneratedNetworkCode._Read_NuclearOption\u002ESavedMission\u002EObjectiveV2\u002EOutcomeType);
    Reader<List<SavedOutcome>>.Read = new Func<NetworkReader, List<SavedOutcome>>(GeneratedNetworkCode._Read_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CNuclearOption\u002ESavedMission\u002EObjectiveV2\u002ESavedOutcome\u003E);
    Reader<NetworkMission.SyncMissionStart>.Read = new Func<NetworkReader, NetworkMission.SyncMissionStart>(GeneratedNetworkCode._Read_NetworkMission\u002FSyncMissionStart);
    Reader<LoadMapMessage>.Read = new Func<NetworkReader, LoadMapMessage>(GeneratedNetworkCode._Read_NuclearOption\u002ESceneLoading\u002ELoadMapMessage);
    Reader<NetworkTransformBase.NetworkSnapshot>.Read = new Func<NetworkReader, NetworkTransformBase.NetworkSnapshot>(GeneratedNetworkCode._Read_NuclearOption\u002ENetworkTransforms\u002ENetworkTransformBase\u002FNetworkSnapshot);
    Reader<CompressedInputs>.Read = new Func<NetworkReader, CompressedInputs>(GeneratedNetworkCode._Read_CompressedInputs);
    Reader<CompressedFloat>.Read = new Func<NetworkReader, CompressedFloat>(GeneratedNetworkCode._Read_CompressedFloat);
    Reader<CompressedInputs?>.Read = new Func<NetworkReader, CompressedInputs?>(GeneratedNetworkCode._Read_System\u002ENullable\u00601\u003CCompressedInputs\u003E);
    Reader<double?>.Read = new Func<NetworkReader, double?>(GeneratedNetworkCode._Read_System\u002ENullable\u00601\u003CSystem\u002EDouble\u003E);
    Reader<float?>.Read = new Func<NetworkReader, float?>(GeneratedNetworkCode._Read_System\u002ENullable\u00601\u003CSystem\u002ESingle\u003E);
    Reader<Vector3Compressed>.Read = new Func<NetworkReader, Vector3Compressed>(GeneratedNetworkCode._Read_Vector3Compressed);
    Reader<SendTransformBatcher.TransformMessage>.Read = new Func<NetworkReader, SendTransformBatcher.TransformMessage>(GeneratedNetworkCode._Read_NuclearOption\u002ENetworkTransforms\u002ESendTransformBatcher\u002FTransformMessage);
    Reader<HostEndedMessage>.Read = new Func<NetworkReader, HostEndedMessage>(GeneratedNetworkCode._Read_NuclearOption\u002ENetworking\u002EHostEndedMessage);
    Reader<LoadWaitingSceneMessage>.Read = new Func<NetworkReader, LoadWaitingSceneMessage>(GeneratedNetworkCode._Read_NuclearOption\u002ENetworking\u002ELoadWaitingSceneMessage);
    Reader<ServerLoadingProgressMessage>.Read = new Func<NetworkReader, ServerLoadingProgressMessage>(GeneratedNetworkCode._Read_NuclearOption\u002ENetworking\u002EServerLoadingProgressMessage);
    Reader<NetworkAuthenticatorNuclearOption.AuthMessage>.Read = new Func<NetworkReader, NetworkAuthenticatorNuclearOption.AuthMessage>(GeneratedNetworkCode._Read_NuclearOption\u002ENetworking\u002EAuthentication\u002ENetworkAuthenticatorNuclearOption\u002FAuthMessage);
    Reader<PlayerType>.Read = new Func<NetworkReader, PlayerType>(GeneratedNetworkCode._Read_NuclearOption\u002ENetworking\u002EPlayerType);
    Reader<NetworkAuthenticatorNuclearOption.PasswordChallenge>.Read = new Func<NetworkReader, NetworkAuthenticatorNuclearOption.PasswordChallenge>(GeneratedNetworkCode._Read_NuclearOption\u002ENetworking\u002EAuthentication\u002ENetworkAuthenticatorNuclearOption\u002FPasswordChallenge);
    Reader<NetworkAuthenticatorNuclearOption.PasswordResponse>.Read = new Func<NetworkReader, NetworkAuthenticatorNuclearOption.PasswordResponse>(GeneratedNetworkCode._Read_NuclearOption\u002ENetworking\u002EAuthentication\u002ENetworkAuthenticatorNuclearOption\u002FPasswordResponse);
    Reader<NetworkAuthenticatorNuclearOption.AuthFailReason>.Read = new Func<NetworkReader, NetworkAuthenticatorNuclearOption.AuthFailReason>(GeneratedNetworkCode._Read_NuclearOption\u002ENetworking\u002EAuthentication\u002ENetworkAuthenticatorNuclearOption\u002FAuthFailReason);
    Reader<NetworkAuthenticatorNuclearOption.BuildHashMismatch>.Read = new Func<NetworkReader, NetworkAuthenticatorNuclearOption.BuildHashMismatch>(GeneratedNetworkCode._Read_NuclearOption\u002ENetworking\u002EAuthentication\u002ENetworkAuthenticatorNuclearOption\u002FBuildHashMismatch);
    Reader<PersistentID>.Read = new Func<NetworkReader, PersistentID>(GeneratedNetworkCode._Read_PersistentID);
    Reader<PlayerRef>.Read = new Func<NetworkReader, PlayerRef>(GeneratedNetworkCode._Read_NuclearOption\u002ENetworking\u002EPlayerRef);
    Reader<Airbase>.Read = new Func<NetworkReader, Airbase>(GeneratedNetworkCode._Read_Airbase);
    Reader<NetworkBehaviorSyncvar<Airbase>>.Read = new Func<NetworkReader, NetworkBehaviorSyncvar<Airbase>>(GeneratedNetworkCode._Read_Mirage\u002ENetworkBehaviorSyncvar\u00601\u003CAirbase\u003E);
    Reader<FactionHQ.RuntimeSupply>.Read = new Func<NetworkReader, FactionHQ.RuntimeSupply>(GeneratedNetworkCode._Read_FactionHQ\u002FRuntimeSupply);
    Reader<ExclusionZone>.Read = new Func<NetworkReader, ExclusionZone>(GeneratedNetworkCode._Read_NuclearOption\u002EExclusionZone);
    Reader<EndType>.Read = new Func<NetworkReader, EndType>(GeneratedNetworkCode._Read_NuclearOption\u002ESavedMission\u002EObjectiveV2\u002EOutcomes\u002EEndType);
    Reader<List<int>>.Read = new Func<NetworkReader, List<int>>(GeneratedNetworkCode._Read_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CSystem\u002EInt32\u003E);
    Reader<FactionHQ>.Read = new Func<NetworkReader, FactionHQ>(GeneratedNetworkCode._Read_FactionHQ);
    Reader<MissionStatsTracker.TypeStat>.Read = new Func<NetworkReader, MissionStatsTracker.TypeStat>(GeneratedNetworkCode._Read_MissionStatsTracker\u002FTypeStat);
    Reader<MissionStatsTracker.Stat>.Read = new Func<NetworkReader, MissionStatsTracker.Stat>(GeneratedNetworkCode._Read_MissionStatsTracker\u002FStat);
    Reader<Airbase.TrySpawnResult>.Read = new Func<NetworkReader, Airbase.TrySpawnResult>(GeneratedNetworkCode._Read_Airbase\u002FTrySpawnResult);
    Reader<Hangar>.Read = new Func<NetworkReader, Hangar>(GeneratedNetworkCode._Read_Hangar);
    Reader<LiveryKey>.Read = new Func<NetworkReader, LiveryKey>(GeneratedNetworkCode._Read_LiveryKey);
    Reader<Loadout>.Read = new Func<NetworkReader, Loadout>(GeneratedNetworkCode._Read_NuclearOption\u002ESavedMission\u002ELoadout);
    Reader<List<WeaponMount>>.Read = new Func<NetworkReader, List<WeaponMount>>(GeneratedNetworkCode._Read_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CWeaponMount\u003E);
    Reader<Player>.Read = new Func<NetworkReader, Player>(GeneratedNetworkCode._Read_NuclearOption\u002ENetworking\u002EPlayer);
    Reader<FactionHQ.RewardType>.Read = new Func<NetworkReader, FactionHQ.RewardType>(GeneratedNetworkCode._Read_FactionHQ\u002FRewardType);
    Reader<KillType>.Read = new Func<NetworkReader, KillType>(GeneratedNetworkCode._Read_KillType);
    Reader<Unit.UnitState>.Read = new Func<NetworkReader, Unit.UnitState>(GeneratedNetworkCode._Read_Unit\u002FUnitState);
    Reader<WeaponMask>.Read = new Func<NetworkReader, WeaponMask>(GeneratedNetworkCode._Read_WeaponMask);
    Reader<Unit.JamEventArgs>.Read = new Func<NetworkReader, Unit.JamEventArgs>(GeneratedNetworkCode._Read_Unit\u002FJamEventArgs);
    Reader<Unit>.Read = new Func<NetworkReader, Unit>(GeneratedNetworkCode._Read_Unit);
    Reader<ReadOnlySpan<PersistentID>>.Read = new Func<NetworkReader, ReadOnlySpan<PersistentID>>(GeneratedNetworkCode._Read_System\u002EReadOnlySpan\u00601\u003CPersistentID\u003E);
    Reader<DamageInfo>.Read = new Func<NetworkReader, DamageInfo>(GeneratedNetworkCode._Read_DamageInfo);
    Reader<int[]>.Read = new Func<NetworkReader, int[]>(GeneratedNetworkCode._Read_System\u002EInt32\u005B\u005D);
    Reader<SlingloadHook.DeployState>.Read = new Func<NetworkReader, SlingloadHook.DeployState>(GeneratedNetworkCode._Read_SlingloadHook\u002FDeployState);
    Reader<RearmEventArgs>.Read = new Func<NetworkReader, RearmEventArgs>(GeneratedNetworkCode._Read_RearmEventArgs);
    Reader<Aircraft>.Read = new Func<NetworkReader, Aircraft>(GeneratedNetworkCode._Read_Aircraft);
    Reader<byte?>.Read = new Func<NetworkReader, byte?>(GeneratedNetworkCode._Read_System\u002ENullable\u00601\u003CSystem\u002EByte\u003E);
    Reader<Hangar.DoorState>.Read = new Func<NetworkReader, Hangar.DoorState>(GeneratedNetworkCode._Read_Hangar\u002FDoorState);
    Reader<UnitCommand.Command>.Read = new Func<NetworkReader, UnitCommand.Command>(GeneratedNetworkCode._Read_UnitCommand\u002FCommand);
    Reader<PilotDismounted.PilotState>.Read = new Func<NetworkReader, PilotDismounted.PilotState>(GeneratedNetworkCode._Read_PilotDismounted\u002FPilotState);
    Reader<Missile.SeekerMode>.Read = new Func<NetworkReader, Missile.SeekerMode>(GeneratedNetworkCode._Read_Missile\u002FSeekerMode);
    Reader<OwnedAirframe>.Read = new Func<NetworkReader, OwnedAirframe>(GeneratedNetworkCode._Read_NuclearOption\u002EOwnedAirframe);
    Reader<OwnedAirframe?>.Read = new Func<NetworkReader, OwnedAirframe?>(GeneratedNetworkCode._Read_System\u002ENullable\u00601\u003CNuclearOption\u002EOwnedAirframe\u003E);
    Reader<ReserveNotice>.Read = new Func<NetworkReader, ReserveNotice>(GeneratedNetworkCode._Read_NuclearOption\u002EReserveNotice);
    Reader<ReserveEvent>.Read = new Func<NetworkReader, ReserveEvent>(GeneratedNetworkCode._Read_NuclearOption\u002EReserveEvent);
    MessagePacker.RegisterMessage<SceneNotReadyMessage>();
    MessagePacker.RegisterMessage<AddCharacterMessage>();
    MessagePacker.RegisterMessage<SceneMessage>();
    MessagePacker.RegisterMessage<SceneReadyMessage>();
    MessagePacker.RegisterMessage<SpawnMessage>();
    MessagePacker.RegisterMessage<RemoveAuthorityMessage>();
    MessagePacker.RegisterMessage<RemoveCharacterMessage>();
    MessagePacker.RegisterMessage<ObjectDestroyMessage>();
    MessagePacker.RegisterMessage<ObjectHideMessage>();
    MessagePacker.RegisterMessage<UpdateVarsMessage>();
    MessagePacker.RegisterMessage<NetworkPingMessage>();
    MessagePacker.RegisterMessage<NetworkPongMessage>();
    MessagePacker.RegisterMessage<RpcMessage>();
    MessagePacker.RegisterMessage<RpcWithReplyMessage>();
    MessagePacker.RegisterMessage<RpcReply>();
    MessagePacker.RegisterMessage<Mirage.Authentication.AuthMessage>();
    MessagePacker.RegisterMessage<AuthSuccessMessage>();
    MessagePacker.RegisterMessage<NetworkMission.SyncMissionPart>();
    MessagePacker.RegisterMessage<NetworkMission.SyncMissionHeader>();
    MessagePacker.RegisterMessage<NetworkMission.SyncMissionFooter>();
    MessagePacker.RegisterMessage<NetworkMission.SyncMission>();
    MessagePacker.RegisterMessage<NetworkMission.SyncMissionStart>();
    MessagePacker.RegisterMessage<LoadMapMessage>();
    MessagePacker.RegisterMessage<NetworkTransformBase.NetworkSnapshot>();
    MessagePacker.RegisterMessage<SendTransformBatcher.TransformMessage>();
    MessagePacker.RegisterMessage<HostEndedMessage>();
    MessagePacker.RegisterMessage<LoadWaitingSceneMessage>();
    MessagePacker.RegisterMessage<ServerLoadingProgressMessage>();
    MessagePacker.RegisterMessage<NetworkAuthenticatorNuclearOption.AuthMessage>();
    MessagePacker.RegisterMessage<NetworkAuthenticatorNuclearOption.PasswordChallenge>();
    MessagePacker.RegisterMessage<NetworkAuthenticatorNuclearOption.PasswordResponse>();
    MessagePacker.RegisterMessage<NetworkAuthenticatorNuclearOption.AuthFailReason>();
    MessagePacker.RegisterMessage<NetworkAuthenticatorNuclearOption.BuildHashMismatch>();
    MessagePacker.RegisterMessage<Mission>();
  }
}
