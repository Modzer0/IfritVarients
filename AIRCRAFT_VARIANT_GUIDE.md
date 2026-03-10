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


---

## Step 2: Register in Encyclopedia

The clone must be added to three data structures:

1. `Encyclopedia.i.aircraft` — the master list
2. `Encyclopedia.Lookup` — dictionary keyed by `jsonKey` (used for save/load)
3. `Encyclopedia.i.IndexLookup` — list with index used for network serialization

### Approach A: PREFIX on Encyclopedia.AfterLoad (Recommended)

If you add your clone to `Encyclopedia.i.aircraft` in a **prefix** patch, the original `AfterLoad` method will automatically:
- Call `CacheMass()` on your clone
- Add it to `Lookup` via `jsonKey`
- Assign `LookupIndex` via `AddWithIndex`
- Sort by value

```csharp
[HarmonyPatch(typeof(Encyclopedia), "AfterLoad")]
public static class EncyclopediaPatch
{
    [HarmonyPrefix]
    public static void Prefix(Encyclopedia __instance)
    {
        // Create clone here (see Step 1)...
        __instance.aircraft.Add(clone);
        // AfterLoad will handle Lookup, IndexLookup, CacheMass, sorting
    }
}
```

### Approach B: POSTFIX on Encyclopedia.AfterLoad

If you use a postfix, you must manually register everything:

```csharp
[HarmonyPostfix]
public static void Postfix(Encyclopedia __instance)
{
    // Create clone...
    __instance.aircraft.Add(clone);

    // Manual Lookup registration
    if (!Encyclopedia.Lookup.ContainsKey(clone.jsonKey))
        Encyclopedia.Lookup.Add(clone.jsonKey, clone);

    // Manual IndexLookup registration
    ((INetworkDefinition)clone).LookupIndex = __instance.IndexLookup.Count;
    __instance.IndexLookup.Add(clone);

    // Must call CacheMass (reads prefab rigidbody mass)
    clone.CacheMass();
}
```

**Recommendation**: Use PREFIX. It's simpler and less error-prone.


---

## Step 3: Make It Appear in the Mission Editor

The mission editor's `NewUnitPanel.Awake()` populates a static dictionary of `UnitOptionProvider` objects from `Encyclopedia.i.aircraft`. This dictionary is built once and cached statically — it survives scene reloads.

```csharp
// Game code (simplified):
if (unitProviders.Count == 0)
{
    unitProviders.Add("aircraft", UnitOptionProvider.Create<AircraftDefinition>(
        Encyclopedia.i.aircraft));
    // ... vehicles, buildings, ships, etc.
}
```

`UnitOptionProvider.Create<T>()` filters out definitions where `disabled == true`, sorts by `unitName`, and stores the result.

### The Problem

If `NewUnitPanel.Awake()` runs before your Encyclopedia patch, the static cache won't include your variant. Even if it runs after, the cache persists across scene loads.

### The Fix: Clear the Static Cache

Patch `NewUnitPanel.Awake` with a prefix that clears the static dictionary:

```csharp
[HarmonyPatch(typeof(NuclearOption.MissionEditorScripts.Buttons.NewUnitPanel), "Awake")]
public static class EditorInjectPatch
{
    private static readonly FieldInfo unitProvidersField =
        AccessTools.Field(typeof(NewUnitPanel), "unitProviders");

    public static void Prefix()
    {
        var dict = unitProvidersField?.GetValue(null) as IDictionary;
        if (dict != null && dict.Count > 0)
            dict.Clear();  // Forces Awake() to rebuild from current Encyclopedia
    }
}
```

This ensures `Awake()` re-reads `Encyclopedia.i.aircraft` (which now includes your clone) every time the editor opens.

### Requirements for Editor Visibility

- `clone.disabled = false`
- Clone is in `Encyclopedia.i.aircraft`
- `NewUnitPanel.unitProviders` cache is cleared before `Awake` runs

### Factory Production Dropdown

The mission editor's `BuildingOptions` class generates the factory production dropdown from `Encyclopedia.i.GetAircraftAndVehicles()`, which returns `aircraft.Concat(vehicles)`. It filters out `disabled == true` entries. So if your clone is registered in Encyclopedia with `disabled = false`, it will automatically appear in the factory production type dropdown — no extra patches needed.
