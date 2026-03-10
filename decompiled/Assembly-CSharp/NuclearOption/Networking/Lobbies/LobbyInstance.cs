// Decompiled with JetBrains decompiler
// Type: NuclearOption.Networking.Lobbies.LobbyInstance
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Steamworks;
using System;
using System.Globalization;

#nullable disable
namespace NuclearOption.Networking.Lobbies;

public abstract class LobbyInstance
{
  public const string TAG_YES = "1";
  public const string TAG_NO = "0";
  public const string GAME_ENDED_NAME = "Game Ended";
  public const string HOST_ADDRESS_KEY = "HostAddress";
  public const string HOST_LOCATION = "HostPing";
  public const string HOST_VERSION_KEY = "version";
  public const string MODDED_KEY = "modded_server";
  public const string LOBBY_NAME = "name";
  public const string UDP_ADDRESS = "UDP_Address";
  public const string UDP_PORT = "UDP_Port";
  public const string HAS_PASSWORD = "has_password";
  public const string SHORT_PASSWORD = "short_password";
  public const string MISSION_NAME_KEY = "mission_name";
  public const string MAP_NAME_KEY = "map_name";
  public const string MISSION_DESCRIPTION_KEY = "mission_description";
  public const string MISSION_PVP_TYPE = "mission_pvp_type";
  public const string START_TIME_KEY = "start_time";
  public const string START_TIME_NO_GAME = "no-game";
  public const string OPEN_MEMBER_SPOTS_KEY = "open_member_spots";
  public const string MAX_MEMBERS_KEY = "max_members";
  public const string MISSION_WORKSHOP_ID_KEY = "mission_workshop_id";
  public const int MAX_LOBBY_NAME_LENGTH = 128 /*0x80*/;
  public const int MAX_MISSION_NAME_LENGTH = 128 /*0x80*/;
  public const int MAX_MAP_NAME_LENGTH = 64 /*0x40*/;
  public const int MAX_MISSION_DESCRIPTION_LENGTH = 1000;
  public bool InList;

  public static string BoolToTag(bool value) => !value ? "0" : "1";

  public static bool TagToBool(string value) => value == "1";

  protected abstract string GetData(string key);

  public abstract string HostAddress { get; }

  public abstract string UdpAddress { get; }

  public abstract string UdpPort { get; }

  public abstract bool DedicatedServer { get; }

  public string HostVersion => this.GetData("version");

  public bool ModdedServer => this.GetData("modded_server") == "1";

  public abstract int? CalculatePing();

  public abstract bool GetPlayerCounts(out int current, out int max);

  public DateTime? StartTime
  {
    get
    {
      string data = this.GetData("start_time");
      if (data == "no-game")
        return new DateTime?();
      DateTime result;
      return !DateTime.TryParse(data, (IFormatProvider) null, DateTimeStyles.AdjustToUniversal, out result) ? new DateTime?() : new DateTime?(result);
    }
  }

  protected virtual string LobbyName => this.GetData("name");

  public string LobbyNameSanitized
  {
    get => this.LobbyName.ProfanityFilter().SanitizeRichText(128 /*0x80*/);
  }

  public abstract bool IsPasswordProtected(out string shortPassword);

  public string MissionNameRaw => this.GetData("mission_name");

  public string MissionNameSanitized
  {
    get => this.GetData("mission_name").ProfanityFilter().SanitizeRichText(128 /*0x80*/);
  }

  public string MapNameSanitized
  {
    get => this.GetData("map_name").ProfanityFilter().SanitizeRichText(64 /*0x40*/);
  }

  public virtual string MissionDescriptionSanitized
  {
    get => this.GetData("mission_description").ProfanityFilter().SanitizeRichText(1000);
  }

  public MissionPvpType MissionPvpType
  {
    get
    {
      int result;
      return !int.TryParse(this.GetData("mission_pvp_type"), out result) ? MissionPvpType.All : (MissionPvpType) result;
    }
  }

  public PublishedFileId_t MissionWorkshopId
  {
    get
    {
      ulong result;
      return !ulong.TryParse(this.GetData("mission_workshop_id"), NumberStyles.HexNumber, (IFormatProvider) CultureInfo.InvariantCulture, out result) ? PublishedFileId_t.Invalid : new PublishedFileId_t(result);
    }
  }

  public static bool ValidName(string name)
  {
    return !string.IsNullOrEmpty(name) && !(name == "Game Ended");
  }

  public static string CreateStartTime() => DateTime.UtcNow.ToString("o");

  public static string TimeSpanString(DateTime? startTime, bool includeSeconds)
  {
    TimeSpan? timespan = new TimeSpan?();
    if (startTime.HasValue)
      timespan = new TimeSpan?(DateTime.UtcNow - startTime.Value);
    return LobbyInstance.TimeSpanString(timespan, includeSeconds);
  }

  public static string TimeSpanString(TimeSpan? timespan, bool includeSeconds)
  {
    if (!timespan.HasValue)
      return "";
    TimeSpan timeSpan = timespan.Value;
    return includeSeconds ? $"{(int) timeSpan.TotalHours}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}" : $"{(int) timeSpan.TotalHours}:{timeSpan.Minutes:D2}";
  }
}
