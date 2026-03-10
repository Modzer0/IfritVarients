// Decompiled with JetBrains decompiler
// Type: NuclearOption.Chat.BlockList
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Mirage.SteamworksSocket;
using NuclearOption.Networking;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

#nullable disable
namespace NuclearOption.Chat;

public class BlockList
{
  private static readonly string blockFile = Path.Join(string.op_Implicit(Application.persistentDataPath), string.op_Implicit("blocklist.txt"));
  private static HashSet<ulong> playerId;

  private BlockList()
  {
  }

  private static void CheckLoaded()
  {
    if (BlockList.playerId != null)
      return;
    BlockList.Load();
  }

  private static void Save()
  {
    File.WriteAllLines(BlockList.blockFile, BlockList.playerId.Select<ulong, string>((Func<ulong, string>) (x => x.ToString())));
  }

  private static void Load()
  {
    if (BlockList.playerId == null)
      BlockList.playerId = new HashSet<ulong>();
    BlockList.playerId.Clear();
    if (!File.Exists(BlockList.blockFile))
      return;
    foreach (string readAllLine in File.ReadAllLines(BlockList.blockFile))
      BlockList.playerId.Add(ulong.Parse(readAllLine));
  }

  public static bool IsBlocked(Player player) => BlockList.IsBlocked(player.SteamID);

  public static bool IsBlocked(ulong id)
  {
    if (id == 0UL)
      return false;
    BlockList.CheckLoaded();
    return BlockList.playerId.Contains(id);
  }

  public static bool ToggleBlock(Player player)
  {
    ulong steamId = player.SteamID;
    if (steamId == 0UL)
      return false;
    if (BlockList.IsBlocked(steamId))
    {
      BlockList.RemoveBlock(steamId);
      return false;
    }
    if (!(NetworkManagerNuclearOption.i.Server.SocketFactory is SteamworksSocketFactory))
    {
      Debug.LogWarning((object) "Can only block user when using Steam transport");
      return false;
    }
    BlockList.AddBlock(steamId);
    return true;
  }

  public static bool AddBlock(ulong id)
  {
    if (id == 0UL)
      return false;
    int num = BlockList.playerId.Add(id) ? 1 : 0;
    if (num == 0)
      return num != 0;
    BlockList.Save();
    return num != 0;
  }

  public static bool RemoveBlock(ulong id)
  {
    if (id == 0UL)
      return false;
    int num = BlockList.playerId.Remove(id) ? 1 : 0;
    if (num == 0)
      return num != 0;
    BlockList.Save();
    return num != 0;
  }
}
