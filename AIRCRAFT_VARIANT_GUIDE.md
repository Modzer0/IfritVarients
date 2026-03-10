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


---

## Step 4: Make It Available at Airbases (In-Game Hangar)

The in-game aircraft selection works through this chain:

```
Airbase.GetAvailableAircraft()
  → iterates each Hangar on the airbase
    → Hangar.GetAvailableAircraft()
      → returns Hangar.availableAircraft[] (serialized on the prefab)
```

`Hangar.availableAircraft` is a serialized `AircraftDefinition[]` on the hangar prefab. Your clone won't be in this array by default because it's baked into the prefab.

### Patch: Inject into Hangar.GetAvailableAircraft

```csharp
[HarmonyPatch(typeof(Hangar), "GetAvailableAircraft")]
public static class HangarPatch
{
    [HarmonyPostfix]
    public static void Postfix(Hangar __instance, ref AircraftDefinition[] __result)
    {
        // Only inject if the original aircraft is in this hangar's list
        if (__result.Any(a => a.jsonKey == "KR-67A"))
        {
            var list = __result.ToList();
            if (!list.Contains(yourClone))
                list.Add(yourClone);
            __result = list.ToArray();
        }
    }
}
```

### Patch: Allow Spawning (CanSpawnAircraft)

`Hangar.CanSpawnAircraft` checks `availableAircraft.Contains(definition)`. Patch it:

```csharp
[HarmonyPatch(typeof(Hangar), "CanSpawnAircraft")]
public static class CanSpawnPatch
{
    [HarmonyPostfix]
    public static void Postfix(
        Hangar __instance,
        AircraftDefinition definition,
        ref bool __result)
    {
        if (!__result && definition == yourClone && __instance.Available)
        {
            // Allow if the original is available here
            var origArray = (AircraftDefinition[])AccessTools
                .Field(typeof(Hangar), "availableAircraft")
                .GetValue(__instance);
            if (origArray.Any(a => a.jsonKey == "KR-67A"))
                __result = true;
        }
    }
}
```

### Patch: Handle Spawning (TrySpawnAircraft + Spawner.SpawnAircraft)

Since the clone shares the original's `unitPrefab`, the spawned `Aircraft` component will have `definition` pointing to the original. You need to reassign it post-spawn.

```csharp
// Flag when clone is being spawned
private static bool spawningClone = false;

[HarmonyPatch(typeof(Hangar), "TrySpawnAircraft")]
public static class TrySpawnPatch
{
    [HarmonyPrefix]
    public static void Prefix(AircraftDefinition definition)
    {
        spawningClone = (definition == yourClone);
    }
}

[HarmonyPatch(typeof(Spawner), "SpawnAircraft")]
public static class SpawnAircraftPatch
{
    [HarmonyPostfix]
    public static void Postfix(Aircraft __result)
    {
        if (spawningClone && __result != null)
        {
            // Reassign definition to clone
            var defField = AccessTools.Field(typeof(Unit), "definition");
            defField.SetValue(__result, yourClone);
            spawningClone = false;
        }
    }
}
```

### Patch: Fix Preview in Selection Menu

`AircraftSelectionMenu.SpawnPreview` reads `definition` from the spawned preview aircraft. Since the prefab's definition points to the original, the preview will show the wrong name/stats.

```csharp
[HarmonyPatch(typeof(AircraftSelectionMenu), "SpawnPreview")]
public static class PreviewPatch
{
    [HarmonyPostfix]
    public static void Postfix(AircraftSelectionMenu __instance)
    {
        var selectedField = AccessTools.Field(typeof(AircraftSelectionMenu), "selectedType");
        var selectionField = AccessTools.Field(typeof(AircraftSelectionMenu), "aircraftSelection");
        var indexField = AccessTools.Field(typeof(AircraftSelectionMenu), "selectionIndex");

        var selection = (List<AircraftDefinition>)selectionField.GetValue(__instance);
        int index = (int)indexField.GetValue(__instance);

        if (selection[index] == yourClone)
            selectedField.SetValue(__instance, yourClone);
    }
}
```


---

## Step 5: Set Up Factory Production

Factories produce units on a timer. The `Factory` component on a building calls `FactionHQ.AddSupplyUnit(productionUnit, 1)` each interval.

### How Factories Work

1. A building prefab has a `Factory` component with `aircraft = true` or `vehicles = true`
2. In the mission editor, the `BuildingOptions` panel shows a production dropdown populated from `Encyclopedia.i.GetAircraftAndVehicles()` (filters `disabled == true`)
3. The selected production type is saved as `SavedBuilding.FactoryOptions.productionType` (the prefab name string)
4. At runtime, `Factory.SetFactory(productionType, productionInterval)` looks up the `UnitDefinition` via `Encyclopedia.Lookup[productionType]`
5. Every `productionInterval` seconds, `ProduceUnit()` calls `FactionHQ.AddSupplyUnit(productionUnit, 1)`

### What You Need for Factory Support

Since your clone is registered in `Encyclopedia.Lookup` with a unique `jsonKey`, and `GetAircraftAndVehicles()` includes it, factories can produce your variant with **no extra patches**:

1. Your clone appears in the editor's factory production dropdown automatically
2. Mission designers select it as the production type
3. The factory looks it up via `Encyclopedia.Lookup[jsonKey]` at runtime
4. `AddSupplyUnit` adds it to `FactionHQ.AircraftSupply`

### Important: productionType Uses jsonKey

`Factory.SetFactory` does `Encyclopedia.Lookup.TryGetValue(productionType, ...)`. The `productionType` stored in the mission JSON is the `jsonKey`. So your clone's `jsonKey` must be stable across mod versions — don't change it after missions are saved with it.

### Programmatic Factory Setup (Optional)

If you want to force a factory to produce your variant via code:

```csharp
// Find a factory building and set its production
Factory factory = someBuilding.GetComponent<Factory>();
factory.SetFactory(yourClone.jsonKey, 900f); // 900 seconds = 15 minutes
```


---

## Step 6: Supply System Integration

The supply system tracks how many of each aircraft a faction has available.

### How Supply Works

```
FactionHQ.AircraftSupply  (SyncDictionary<AircraftDefinition, RuntimeSupply>)
    ├── Factory.ProduceUnit() → AddSupplyUnit(def, +1)
    ├── Aircraft returns to base → AddSupplyUnit(def, +1)
    ├── Hangar.TrySpawnAircraft() → AddSupplyUnit(def, -1)
    └── Mission start → ModifyUnitSupply() from saved supply counts
```

### Player Aircraft Ownership

Players must "own" an airframe to fly it:
1. Player gets allocation (money) based on rank and faction funds
2. Player buys an airframe: `CreditAirframe(def, 1, false)` — deducts from allocation
3. Or receives one from supply: `CreditAirframe(def, 1, true)` — reserved from faction supply
4. Player can fly if `OwnsAirframe(def, true)` returns true

### No Extra Patches Needed

Since your clone is a proper `AircraftDefinition` registered in Encyclopedia, the supply system handles it natively:
- `ModifyUnitSupply` switches on `AircraftDefinition` type — your clone matches
- `AircraftSupply` dictionary uses the definition object as key — your clone is a distinct object
- `AddSupplyUnit` / `GetUnitSupply` work automatically

### Mission Editor: Starting Supply

In the mission editor's faction settings tab (`FactionSettingsTab`), the supply unit dropdown is populated from `Encyclopedia.i.GetAircraftAndVehicles()`. Your clone appears here automatically. Mission designers can set starting supply counts for your variant per faction.
