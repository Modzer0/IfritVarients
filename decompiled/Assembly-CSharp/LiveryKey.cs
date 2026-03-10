// Decompiled with JetBrains decompiler
// Type: LiveryKey
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Cysharp.Threading.Tasks;
using NuclearOption.AddressableScripts;
using NuclearOption.AddressableScripts.ModFoldersImpl;
using Steamworks;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

#nullable disable
[Serializable]
public readonly struct LiveryKey : IEquatable<LiveryKey>
{
  public readonly LiveryKey.KeyType Type;
  public readonly int Index;
  public readonly string AppDataName;
  public readonly ulong Id;

  public PublishedFileId_t WorkshopId => new PublishedFileId_t(this.Id);

  public LiveryKey(int index)
    : this()
  {
    this.Type = LiveryKey.KeyType.Builtin;
    this.Index = index;
  }

  public LiveryKey(LiveryMetaData metaData, bool workshop)
    : this()
  {
    this.Type = workshop ? LiveryKey.KeyType.Workshop : LiveryKey.KeyType.AppData;
    if (workshop)
      this.Id = metaData.Id.m_PublishedFileId;
    else
      this.AppDataName = Path.GetFileName(metaData.FolderFullPath);
  }

  public LiveryKey(LiveryKey.KeyType type, int index, string nameOrId)
    : this()
  {
    this.Type = type;
    this.Index = index;
    if (type == LiveryKey.KeyType.AppData)
    {
      this.AppDataName = nameOrId;
    }
    else
    {
      if (type != LiveryKey.KeyType.Workshop)
        return;
      this.Id = ulong.Parse(nameOrId);
    }
  }

  public void Save(out LiveryKey.KeyType type, out int index, out string nameOrId)
  {
    type = this.Type;
    index = this.Index;
    if (this.Type == LiveryKey.KeyType.AppData)
      nameOrId = this.AppDataName;
    else if (this.Type == LiveryKey.KeyType.Workshop)
      nameOrId = this.Id.ToString();
    else
      nameOrId = "";
  }

  public override string ToString()
  {
    switch (this.Type)
    {
      case LiveryKey.KeyType.AppData:
        return $"({this.Type},{this.AppDataName})";
      case LiveryKey.KeyType.Workshop:
        return $"({this.Type},{this.Id})";
      default:
        return $"({this.Type},{this.Index})";
    }
  }

  public bool CanLoad(Aircraft aircraft, out string folder)
  {
    switch (this.Type)
    {
      case LiveryKey.KeyType.Builtin:
        folder = (string) null;
        return true;
      case LiveryKey.KeyType.AppData:
        return Skins.CanLoad(this, aircraft, out folder);
      case LiveryKey.KeyType.Workshop:
        return Skins.CanLoad(this, aircraft, out folder);
      default:
        throw new InvalidEnumArgumentException("Type", (int) this.Type, typeof (LiveryKey.KeyType));
    }
  }

  public async UniTask<(bool success, AsyncOperationHandle<LiveryData> handle)> Load(
    Aircraft aircraft)
  {
    string folder;
    if (!this.CanLoad(aircraft, out folder))
      return (false, new AsyncOperationHandle<LiveryData>());
    AsyncOperationHandle<LiveryData> asyncOperationHandle = await this.LoadImpl(aircraft, folder);
    return (asyncOperationHandle.IsValid(), asyncOperationHandle);
  }

  private UniTask<AsyncOperationHandle<LiveryData>> LoadImpl(Aircraft aircraft, string folder)
  {
    switch (this.Type)
    {
      case LiveryKey.KeyType.Builtin:
        return this.GetBuiltin(aircraft);
      case LiveryKey.KeyType.AppData:
        return ModFolders.AppDataSkins.LoadAsset<LiveryData>(folder);
      case LiveryKey.KeyType.Workshop:
        return ModFolders.WorkshopSkins.LoadAsset<LiveryData>(folder);
      default:
        throw new InvalidEnumArgumentException("Type", (int) this.Type, typeof (LiveryKey.KeyType));
    }
  }

  private UniTask<AsyncOperationHandle<LiveryData>> GetBuiltin(Aircraft aircraft)
  {
    AircraftParameters aircraftParameters = aircraft.GetAircraftParameters();
    List<AircraftParameters.Livery> liveries = aircraftParameters.liveries;
    if (this.Index < liveries.Count)
      return ModLoader.LoadAsset<LiveryData>((AssetReferenceT<LiveryData>) aircraft.GetAircraftParameters().liveries[this.Index].assetReference);
    Debug.LogError((object) $"Livery index for {aircraftParameters.aircraftName} out of range. index:{this.Index} Max:{liveries.Count}");
    return new UniTask<AsyncOperationHandle<LiveryData>>();
  }

  public bool Equals(LiveryKey other)
  {
    return this.Type == other.Type && this.Index == other.Index && this.AppDataName == other.AppDataName && (long) this.Id == (long) other.Id;
  }

  public enum KeyType : byte
  {
    Builtin,
    AppData,
    Workshop,
  }
}
