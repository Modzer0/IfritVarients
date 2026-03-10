// Decompiled with JetBrains decompiler
// Type: NuclearOption.Networking.SpectatorPlayer
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;

#nullable disable
namespace NuclearOption.Networking;

public class SpectatorPlayer : BasePlayer
{
  [NonSerialized]
  private const int SYNC_VAR_COUNT = 1;
  [NonSerialized]
  private const int RPC_COUNT = 0;

  private void MirageProcessed()
  {
  }

  protected override int GetRpcCount() => 0;
}
