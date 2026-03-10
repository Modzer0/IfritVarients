# Adding a Unit to the Nuclear Option Mission Editor

## How the Mission Editor Lists Units

The mission editor's unit placement panel (`NewUnitPanel`) pulls its aircraft list from `Encyclopedia.i.aircraft`. On first open, `NewUnitPanel.Awake()` populates a static `Dictionary<string, UnitOptionProvider>` called `unitProviders`:

```csharp
// Only runs once — static cache
if (unitProviders.Count == 0)
{
    unitProviders.Add("aircraft", UnitOptionProvider.Create<AircraftDefinition>(Encyclopedia.i.aircraft));
    unitProviders.Add("vehicles", UnitOptionProvider.Create<VehicleDefinition>(Encyclopedia.i.vehicles));
    // ... ships, buildings, scenery, missiles, otherUnits
}
```

`UnitOptionProvider.Create<T>()` filters out definitions with `disabled = true`, sorts by `unitName`, and stores the result as an array.

## Key Classes

| Class | Namespace | Role |
|---|---|---|
| `NewUnitPanel` | `NuclearOption.MissionEditorScripts.Buttons` | UI panel for placing units in editor |
| `UnitOptionProvider` | Nested in `NewUnitPanel` | Wraps a filtered/sorted `UnitDefinition[]` |
| `Encyclopedia` | Global | Singleton holding all unit definition lists |
| `AircraftDefinition` | Global | ScriptableObject defining an aircraft |
| `UnitDefinition` | Global | Base class for all unit definitions |

## Steps to Inject a Custom Unit

### 1. Create the Definition

Clone an existing `AircraftDefinition` via `UnityEngine.Object.Instantiate()`. Must also clone `aircraftParameters` (separate ScriptableObject).

```csharp
clonedDef = Object.Instantiate(original);
clonedDef.name = "MyCloneKey";
clonedDef.jsonKey = "MyCloneKey";        // unique key for save/load
clonedDef.unitName = "My Custom Aircraft";
clonedDef.unitPrefab = original.unitPrefab;  // shares the same prefab
clonedDef.disabled = false;               // must be false to appear in editor

clonedDef.aircraftParameters = Object.Instantiate(original.aircraftParameters);
clonedDef.aircraftParameters.aircraftName = "My Custom Aircraft";
```

### 2. Register in Encyclopedia

Add to `Encyclopedia.i.aircraft`, the `Lookup` dictionary, and `IndexLookup` (for network sync):

```csharp
Encyclopedia enc = Encyclopedia.i;
enc.aircraft.Add(clonedDef);

if (!Encyclopedia.Lookup.ContainsKey(clonedDef.jsonKey))
    Encyclopedia.Lookup.Add(clonedDef.jsonKey, clonedDef);

((INetworkDefinition)clonedDef).LookupIndex = enc.IndexLookup.Count;
enc.IndexLookup.Add(clonedDef);

clonedDef.CacheMass();  // required — caches prefab mass
```

Best place: Harmony postfix on `Encyclopedia.AfterLoad`.

### 3. Force Mission Editor to Rebuild Its Cache

`NewUnitPanel.unitProviders` is static and only populated once. If your Encyclopedia patch runs after the editor already cached the list, your unit won't appear.

Fix: Harmony prefix on `NewUnitPanel.Awake` that clears the cache:

```csharp
[HarmonyPatch(typeof(NewUnitPanel), "Awake")]
public static class EditorInjectPatch
{
    private static readonly FieldInfo unitProvidersField =
        AccessTools.Field(typeof(NewUnitPanel), "unitProviders");

    public static void Prefix()
    {
        var dict = unitProvidersField?.GetValue(null) as IDictionary;
        if (dict != null && dict.Count > 0)
            dict.Clear();  // forces Awake() to rebuild from Encyclopedia
    }
}
```

Requires: `using NuclearOption.MissionEditorScripts.Buttons;`

### 4. Handle Spawning

The editor spawns units via `Spawner.SpawnAircraft`. Since the clone shares the original's `unitPrefab`, the spawned aircraft will have the original's definition. You need patches to reassign:

- `Hangar.TrySpawnAircraft` — flag when clone is being spawned
- `Spawner.SpawnAircraft` — postfix to reassign `Unit.definition` to clone

### 5. Handle Saving/Loading

Missions save unit definitions by `jsonKey`. As long as your clone has a unique `jsonKey` registered in `Encyclopedia.Lookup`, it will save and load correctly. The `INetworkDefinition.LookupIndex` is needed for multiplayer sync.

## Gotchas

- `unitProviders` is static — survives scene reloads. Clearing it in `Awake` prefix ensures fresh data.
- `UnitOptionProvider.Create` filters `disabled == true` definitions. Make sure `disabled = false`.
- `CacheMass()` must be called or the editor will show 0 mass.
- The clone shares the original's prefab, so runtime patches (Harmony) are needed to differentiate behavior.
- `aircraftParameters` must be a separate clone, not a reference to the original's — otherwise changes affect both.
- `code` field on `UnitDefinition` is used for some UI displays (short name).
