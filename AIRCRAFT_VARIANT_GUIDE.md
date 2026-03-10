# Nuclear Option — Aircraft Variant Creation Guide

Complete reference for creating aircraft variants via BepInEx/Harmony mods, making them available in the mission editor, setting up factories to produce them, and flying them in-game. Based on analysis of the NuclearOptionIfritMod (KR-67X SuperIfrit) and the QoL mod.

---

## Overview: The Aircraft Lifecycle

An aircraft variant must be wired into four systems to be fully functional:

1. **Encyclopedia** — The master registry. If it's not here, it doesn't exist.
2. **Mission Editor** — `NewUnitPanel` reads from Encyclopedia to list placeable units.
3. **Hangar / Airbase** — Determines which aircraft can spawn at which airbases in-game.
4. **Factory / Supply** — Factories produce units over time; the supply system tracks inventory per faction.

```
Encyclopedia.aircraft
    ├── Mission Editor (NewUnitPanel reads Encyclopedia.i.aircraft)
    ├── Airbase → Hangar.availableAircraft[] (what can spawn here)
    ├── Factory.productionUnit (what this factory produces)
    └── FactionHQ.AircraftSupply (runtime inventory per faction)
```

---

## Step 1: Clone the AircraftDefinition

`AircraftDefinition` extends `UnitDefinition` and adds `aircraftParameters` (a separate ScriptableObject) and `aircraftInfo`. Both must be cloned independently.

### Key Fields on UnitDefinition (base class)

| Field | Type | Purpose |
|---|---|---|
| `jsonKey` | string | Unique key for save/load and network lookup. **Must be unique.** |
| `unitName` | string | Display name shown in UI |
| `code` | string | Short code (e.g. "KR-67A") used in factory labels |
| `value` | float | Cost in millions (affects allocation/purchasing) |
| `disabled` | bool | If true, hidden from editor and hangar lists |
| `unitPrefab` | GameObject | The actual aircraft prefab. Variants share the original's prefab. |
| `spawnOffset` | Vector3 | Offset when spawning on ground |

### Key Fields on AircraftDefinition

| Field | Type | Purpose |
|---|---|---|
| `aircraftParameters` | AircraftParameters | Flight model, loadouts, liveries, rank requirement, etc. |
| `aircraftInfo` | AircraftInfo | UI metadata (max weight display, etc.) |

### Key Fields on AircraftParameters

| Field | Type | Purpose |
|---|---|---|
| `aircraftName` | string | Name shown in aircraft info panels |
| `aircraftDescription` | string | Description text in selection menu |
| `rankRequired` | int | Minimum player rank to fly |
| `maxSpeed` | float | Reference max speed |
| `loadouts` | List\<Loadout\> | Default loadout options |
| `StandardLoadouts` | StandardLoadout[] | AI loadout presets |
| `liveries` | List\<Livery\> | Available paint schemes per faction |
| `DefaultFuelLevel` | float | Default fuel (0-1) |

### Clone Code

```csharp
// In your Encyclopedia.AfterLoad postfix:
AircraftDefinition original = Encyclopedia.i.aircraft
    .FirstOrDefault(a => a.jsonKey == "KR-67A");

AircraftDefinition clone = UnityEngine.Object.Instantiate(original);
clone.name = "KR-67X_SuperIfrit";           // internal asset name
clone.jsonKey = "KR-67X_SuperIfrit";         // MUST be unique
clone.unitName = "KR-67X SuperIfrit";        // display name
clone.code = "KR-67X";                       // short code
clone.value = 1000f;                         // cost in $M
clone.disabled = false;                      // must be false to appear
clone.unitPrefab = original.unitPrefab;      // shares the same prefab

// Clone aircraftParameters separately (it's a ScriptableObject)
clone.aircraftParameters = UnityEngine.Object.Instantiate(original.aircraftParameters);
clone.aircraftParameters.aircraftName = "KR-67X SuperIfrit";
clone.aircraftParameters.aircraftDescription = "Hypersonic variant...";
clone.aircraftParameters.rankRequired = 0;   // or whatever rank

// Clone aircraftInfo if you need to change max weight etc.
clone.aircraftInfo = UnityEngine.Object.Instantiate(original.aircraftInfo);
```

**Critical**: `aircraftParameters` MUST be a separate clone. If you just reference the original's, any changes affect both aircraft.
