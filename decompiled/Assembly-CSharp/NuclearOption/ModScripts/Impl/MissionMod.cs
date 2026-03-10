// Decompiled with JetBrains decompiler
// Type: NuclearOption.ModScripts.Impl.MissionMod
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.SavedMission;
using NuclearOption.Workshop;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#nullable disable
namespace NuclearOption.ModScripts.Impl;

internal sealed class MissionMod : ModType
{
  public MissionMod()
    : base("Mission")
  {
  }

  protected override string GetRootFolder() => MissionGroup.UserGroup.UserMissionDirectory;

  public override List<string> ListItems()
  {
    return MissionGroup.User.GetMissions().Select<MissionKey, string>((Func<MissionKey, string>) (x => x.Name)).ToList<string>();
  }

  public override WorkshopUploadItem GetItem(string itemName)
  {
    if (string.IsNullOrEmpty(itemName))
      throw new InvalidOperationException("Page 3 should not be active when mission name is null");
    string error;
    if (MissionSaveLoad.TryLoad(new MissionKey(itemName, (MissionGroup) MissionGroup.User), out Mission _, out error))
      return new WorkshopUploadItem(this.Tag, SubscribedItemType.Mission, MissionGroup.UserGroup.GetFolder(itemName), itemName);
    Debug.LogError((object) error);
    return (WorkshopUploadItem) null;
  }
}
