// Decompiled with JetBrains decompiler
// Type: LiveryMetaData
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.AddressableScripts;
using NuclearOption.ModScripts;
using Steamworks;
using System;

#nullable disable
[CopyToModProject]
[Serializable]
public struct LiveryMetaData : IMetaData
{
  public string DisplayName;
  public string Faction;
  public string Aircraft;

  public PublishedFileId_t Id { get; set; }

  public string FolderFullPath { get; set; }
}
