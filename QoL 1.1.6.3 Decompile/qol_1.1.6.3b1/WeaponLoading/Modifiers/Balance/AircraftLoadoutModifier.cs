// Decompiled with JetBrains decompiler
// Type: qol.WeaponLoading.Modifiers.Balance.AircraftLoadoutModifier
// Assembly: qol, Version=1.1.6.3, Culture=neutral, PublicKeyToken=null
// MVID: 3B6539EA-E38C-45EE-8FFF-FC58CC42BADC
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\BepInEx\plugins\qol_1.1.6.3b1.dll

using qol.WeaponLoading.Configs;
using qol.WeaponLoading.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#nullable disable
namespace qol.WeaponLoading.Modifiers.Balance;

public class AircraftLoadoutModifier : IEntityModifier
{
  public string ModifierId => "AircraftLoadout";

  public int Priority => 100;

  public IReadOnlyList<string> Dependencies => (IReadOnlyList<string>) Array.Empty<string>();

  public bool CanApply(ModificationContext context) => true;

  public void Apply(ModificationContext context)
  {
    int num1 = 0;
    int num2 = 0;
    foreach (AircraftLoadoutConfigs.AircraftLoadoutDef aircraftLoadout in (IEnumerable<AircraftLoadoutConfigs.AircraftLoadoutDef>) AircraftLoadoutConfigs.AircraftLoadouts)
    {
      AircraftLoadoutConfigs.AircraftLoadoutDef aircraftDef = aircraftLoadout;
      if (aircraftDef.Enabled && aircraftDef.SlotAssignments != null && aircraftDef.SlotAssignments.Count != 0)
      {
        AircraftParameters aircraftParameters = ((IEnumerable<AircraftParameters>) Resources.FindObjectsOfTypeAll<AircraftParameters>()).FirstOrDefault<AircraftParameters>((Func<AircraftParameters, bool>) (p => p.name.Equals(aircraftDef.ParametersName, StringComparison.InvariantCultureIgnoreCase)));
        if ((UnityEngine.Object) aircraftParameters == (UnityEngine.Object) null)
        {
          context.Logger.LogWarning((object) $"[{this.ModifierId}] AircraftParameters '{aircraftDef.ParametersName}' not found");
        }
        else
        {
          Dictionary<string, StandardLoadout> loadoutDict = this.BuildLoadouts(aircraftDef.Loadouts, context);
          bool flag = false;
          foreach (string slotAssignment in (IEnumerable<string>) aircraftDef.SlotAssignments)
          {
            if (!loadoutDict.ContainsKey(slotAssignment))
            {
              context.Logger.LogError((object) $"[{this.ModifierId}] Loadout '{slotAssignment}' not found for {aircraftDef.ParametersName}");
              flag = true;
            }
          }
          if (!flag)
          {
            aircraftParameters.StandardLoadouts = aircraftDef.SlotAssignments.Select<string, StandardLoadout>((Func<string, StandardLoadout>) (id => loadoutDict[id])).ToArray<StandardLoadout>();
            ++num1;
            num2 += loadoutDict.Count;
          }
        }
      }
    }
    context.Logger.LogInfo((object) $"[{this.ModifierId}] Modified {num1} aircraft with {num2} loadout types");
  }

  private Dictionary<string, StandardLoadout> BuildLoadouts(
    IReadOnlyList<AircraftLoadoutConfigs.LoadoutDef> loadoutDefs,
    ModificationContext context)
  {
    Dictionary<string, StandardLoadout> dictionary = new Dictionary<string, StandardLoadout>();
    foreach (AircraftLoadoutConfigs.LoadoutDef loadoutDef in (IEnumerable<AircraftLoadoutConfigs.LoadoutDef>) loadoutDefs)
    {
      StandardLoadout standardLoadout = QOLPlugin.CreateStandardLoadout(loadoutDef.FuelRatio);
      if (loadoutDef.Weapons != null)
      {
        foreach (string weapon in loadoutDef.Weapons)
        {
          if (string.IsNullOrEmpty(weapon))
          {
            standardLoadout.loadout.weapons.Add((WeaponMount) null);
          }
          else
          {
            WeaponMount mount = AircraftLoadoutModifier.GetMount(weapon);
            if ((UnityEngine.Object) mount == (UnityEngine.Object) null)
              context.Logger.LogWarning((object) $"[{this.ModifierId}] WeaponMount '{weapon}' not found");
            standardLoadout.loadout.weapons.Add(mount);
          }
        }
      }
      dictionary[loadoutDef.Id] = standardLoadout;
    }
    return dictionary;
  }

  private static WeaponMount GetMount(string name)
  {
    return ((IEnumerable<WeaponMount>) Resources.FindObjectsOfTypeAll<WeaponMount>()).FirstOrDefault<WeaponMount>((Func<WeaponMount, bool>) (wep => wep.name.Equals(name, StringComparison.InvariantCultureIgnoreCase)));
  }
}
