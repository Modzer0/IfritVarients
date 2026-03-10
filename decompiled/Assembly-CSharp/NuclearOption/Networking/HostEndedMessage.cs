// Decompiled with JetBrains decompiler
// Type: NuclearOption.Networking.HostEndedMessage
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Mirage;

#nullable disable
namespace NuclearOption.Networking;

[NetworkMessage]
public struct HostEndedMessage
{
  public string HostName;
}
