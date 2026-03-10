// Decompiled with JetBrains decompiler
// Type: NuclearOption.AddressableScripts.IMetaData
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Steamworks;

#nullable disable
namespace NuclearOption.AddressableScripts;

public interface IMetaData
{
  PublishedFileId_t Id { get; set; }

  string FolderFullPath { get; set; }
}
