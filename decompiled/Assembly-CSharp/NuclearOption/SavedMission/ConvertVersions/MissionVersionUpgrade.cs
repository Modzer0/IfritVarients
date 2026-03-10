// Decompiled with JetBrains decompiler
// Type: NuclearOption.SavedMission.ConvertVersions.MissionVersionUpgrade
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#nullable disable
namespace NuclearOption.SavedMission.ConvertVersions;

public static class MissionVersionUpgrade
{
  public static int LatestVersion = 4;

  public static void Upgrade(Mission mission)
  {
    if (mission.JsonVersion == 0)
    {
      Debug.LogWarning((object) "Mission had no version, setting to V1");
      mission.JsonVersion = 1;
    }
    if (mission.JsonVersion == 1)
    {
      Debug.LogWarning((object) "Converting V1 Mission to V2");
      MissionVersionUpgrade.Convert_v1_to_v2(mission);
      mission.JsonVersion = 2;
    }
    if (mission.JsonVersion == 2)
    {
      Debug.LogWarning((object) "Converting V2 Mission to V3");
      MissionVersionUpgrade.Convert_v2_to_v3(mission);
      mission.JsonVersion = 3;
    }
    if (mission.JsonVersion != 3)
      return;
    Debug.LogWarning((object) "Converting V3 Mission to V4");
    Debug.LogWarning((object) "Converting Buildings to Scenery");
    MissionVersionUpgrade.Convert_v3_to_v4(mission);
    mission.JsonVersion = 4;
  }

  private static void Convert_v1_to_v2(Mission mission)
  {
    mission.objectives = ConvertOldMissions.ConvertObjective(mission.factions, MissionManager.GetAllSavedUnits(mission, false));
  }

  [Obsolete("")]
  private static void Convert_v2_to_v3(Mission mission)
  {
    V2LoadoutMap v2LoadoutMap = V2LoadoutMap.Load();
    foreach (SavedAircraft savedAircraft1 in mission.aircraft)
    {
      SavedAircraft savedAircraft = savedAircraft1;
      V2LoadoutMap.UnitLoadout unitLoadout = v2LoadoutMap.Units.FirstOrDefault<V2LoadoutMap.UnitLoadout>((Func<V2LoadoutMap.UnitLoadout, bool>) (x => x.UnitKey == savedAircraft.type));
      if (unitLoadout == null)
      {
        Debug.LogWarning((object) ("Could not find unit with name " + savedAircraft.type));
      }
      else
      {
        SavedLoadout savedLoadout = new SavedLoadout();
        savedAircraft.savedLoadout = savedLoadout;
        LoadoutOld loadout = savedAircraft.loadout;
        List<byte> weaponSelections = loadout.weaponSelections;
        // ISSUE: explicit non-virtual call
        int count = weaponSelections != null ? __nonvirtual (weaponSelections.Count) : 0;
        for (int index = 0; index < count; ++index)
        {
          byte weaponSelection = loadout.weaponSelections[index];
          V2LoadoutMap.HardPoint hardPoint = unitLoadout.HardPoints[index];
          if ((int) weaponSelection < hardPoint.Options.Count)
          {
            string option = hardPoint.Options[(int) weaponSelection];
            savedLoadout.Selected.Add(new SavedLoadout.SelectedMount()
            {
              Key = option
            });
          }
          else
          {
            Debug.LogWarning((object) $"Old weapon index was out of range for {savedAircraft.type}, hardpoint index {index}. Old index:{weaponSelection} V2 option count: {hardPoint.Options.Count}");
            savedLoadout.Selected.Add(new SavedLoadout.SelectedMount()
            {
              Key = (string) null
            });
          }
        }
      }
    }
  }

  private static void Convert_v3_to_v4(Mission mission)
  {
    HashSet<string> stringSet1 = new HashSet<string>(Encyclopedia.i.buildings.Select<BuildingDefinition, string>((Func<BuildingDefinition, string>) (x => x.jsonKey)));
    HashSet<string> stringSet2 = new HashSet<string>(Encyclopedia.i.scenery.Select<SceneryDefinition, string>((Func<SceneryDefinition, string>) (x => x.jsonKey)));
    List<SavedBuilding> savedBuildingList = new List<SavedBuilding>();
    foreach (SavedBuilding building in mission.buildings)
    {
      if (!stringSet1.Contains(building.type))
      {
        if (stringSet2.Contains(building.type))
          savedBuildingList.Add(building);
        else
          Debug.LogError((object) ("building now found in building or scenery list: " + building.type));
      }
    }
    foreach (SavedBuilding savedBuilding in savedBuildingList)
    {
      ColorLog<SavedScenery>.Info($"Converting {savedBuilding.UniqueName} ({savedBuilding.type}) to Scenery");
      SavedScenery savedScenery1 = new SavedScenery();
      savedScenery1.type = savedBuilding.type;
      savedScenery1.UniqueName = savedBuilding.UniqueName;
      savedScenery1.globalPosition = savedBuilding.globalPosition;
      savedScenery1.rotation = savedBuilding.rotation;
      SavedScenery savedScenery2 = savedScenery1;
      mission.scenery.Add(savedScenery2);
      mission.buildings.Remove(savedBuilding);
    }
  }
}
