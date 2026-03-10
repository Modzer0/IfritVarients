// Decompiled with JetBrains decompiler
// Type: NuclearOption.Networking.Authentication.TimeoutConfig
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Mirage;
using System;

#nullable disable
namespace NuclearOption.Networking.Authentication;

[Serializable]
public class TimeoutConfig
{
  public int FreeDisconnectAllowed = 3;
  public float RapidDisconnectWindowSeconds = 60f;
  public float BaseTimeoutSeconds = 30f;
  public float TimeoutMultiplier = 2f;
  public float MaxTimeoutSeconds = 21600f;
  public float SpamConnectPenaltySeconds = 10f;
  public float PenaltyDecayRateSeconds = 600f;
  public float ErrorKickBaseTimeout = 300f;
  public bool BanOnRepeatedErrorKicks = true;
  public int MaxErrorKicksBeforeBan = 5;
  public float ErrorKickDecayRateSeconds = 7200f;
  public PlayerErrorFlags InstantBanErrorFlags = PlayerErrorFlags.Critical;

  public bool CheckInstantBanErrorFlags(PlayerErrorFlags flags)
  {
    return (this.InstantBanErrorFlags & flags) != 0;
  }
}
