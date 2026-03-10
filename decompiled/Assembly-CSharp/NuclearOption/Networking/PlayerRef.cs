// Decompiled with JetBrains decompiler
// Type: NuclearOption.Networking.PlayerRef
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using System.Runtime.CompilerServices;

#nullable disable
namespace NuclearOption.Networking;

public struct PlayerRef : IEquatable<PlayerRef>
{
  public readonly uint PlayerId;
  private Player player;

  public static PlayerRef Invalid => new PlayerRef();

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool Valid() => this.PlayerId > 0U;

  public PlayerRef(Player player)
  {
    this.PlayerId = player.NetId;
    this.player = player;
  }

  public Player Player
  {
    get
    {
      if (!this.Valid())
        return (Player) null;
      if ((UnityEngine.Object) this.player == (UnityEngine.Object) null)
        UnitRegistry.playerLookup.TryGetValue(this, out this.player);
      return this.player;
    }
  }

  public override int GetHashCode() => (int) this.PlayerId;

  public override bool Equals(object obj) => obj is PlayerRef other && this.Equals(other);

  public bool Equals(PlayerRef other) => (int) this.PlayerId == (int) other.PlayerId;

  public override string ToString() => $"Player({this.PlayerId})";
}
