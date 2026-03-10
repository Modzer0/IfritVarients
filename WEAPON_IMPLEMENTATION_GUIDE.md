# Nuclear Option — Weapon Implementation Guide

Reference guide for implementing new weapons in Nuclear Option BepInEx mods, based on analysis of the QoL mod (v1.1.6.2b2) and the base game's Assembly-CSharp.

---

## Architecture Overview

Nuclear Option's weapon system has four key objects:

1. **WeaponMount** (ScriptableObject) — The "definition" of a mountable weapon. Contains cost, mass, RCS, prefab reference, jsonKey, and a `WeaponInfo` reference. Implements `INetworkDefinition` (has `LookupIndex` for network serialization).
2. **WeaponInfo** (ScriptableObject) — Metadata about the weapon: name, short name, description, icon, damage values, targeting requirements, and a reference to the weapon's GameObject prefab.
3. **Weapon prefab** (GameObject) — The actual Unity GameObject with components like `Gun`, `MissileLauncher`, `Laser`, `MountedMissile`, `MountedCargo`, etc.
4. **Encyclopedia** — Singleton ScriptableObject that holds master lists of all game definitions. Its `AfterLoad()` method builds `IndexLookup` (for network serialization) and `WeaponLookup` (for JSON save/load).

### How They Connect

```
Encyclopedia.weaponMounts  →  List<WeaponMount>
    WeaponMount.info       →  WeaponInfo
    WeaponMount.prefab     →  GameObject (the physical weapon)
    WeaponMount.jsonKey    →  string (used for save/load)
    WeaponMount.LookupIndex → int (set by Encyclopedia.AfterLoad, used for networking)

WeaponInfo.weaponPrefab    →  GameObject (same or null)
WeaponInfo.weaponName      →  display name
WeaponInfo.weaponIcon      →  Sprite
```

### How Weapons Get Onto Aircraft

Aircraft have a `WeaponManager` component with an array of `HardpointSet[]`:

```
WeaponManager.hardpointSets[0]  = "Inner Bay"     (index 0)
WeaponManager.hardpointSets[1]  = "Forward Bay"   (index 1)
WeaponManager.hardpointSets[2]  = "Rear Bay"      (index 2)
WeaponManager.hardpointSets[3]  = "Outer Bay"     (index 3)
WeaponManager.hardpointSets[4]  = "Module Bay"    (index 4)  ← QoL adds this to Darkreach
```

Each `HardpointSet` has:
- `name` — Display name
- `weaponOptions` — `List<WeaponMount>` of selectable weapons (null = "Empty")
- `hardpoints` — `List<Hardpoint>` physical mount points
- `precludingHardpointSets` — `List<byte>` indices of other sets that disable this one when loaded
- `weaponMount` — Runtime: currently selected mount

---

## The QoL Approach: Duplicate and Modify

QoL's core pattern is: **clone an existing game asset, rename it, tweak properties, register it**. This avoids creating weapon components from scratch (which is extremely fragile in Unity modding).

### Step 1: Clone the Weapon Prefab (GameObject)

```csharp
public static GameObject DuplicatePrefab(string originalName, string newName)
{
    // Find the original by searching ALL loaded GameObjects (including inactive/hidden)
    GameObject original = FindGameObjectByExactPath(originalName);
    
    // Clone it
    GameObject clone = Object.Instantiate<GameObject>(original);
    clone.name = newName;
    clone.transform.SetParent(null);
    clone.hideFlags = HideFlags.HideAndDontSave;  // Prevent garbage collection
    clone.SetActive(false);                         // Keep inactive until spawned
    
    // Fix NetworkIdentity to avoid hash collisions
    NetworkIdentity netId = clone.GetComponentInChildren<NetworkIdentity>();
    if (netId != null)
    {
        Traverse traverse = Traverse.Create(netId);
        netId.PrefabHash = int.Parse(netId.PrefabHash.ToString().Substring(1));
        traverse.Field("_hasSpawned").SetValue(false);
        traverse.Method("NetworkReset").GetValue();
    }
    
    return clone;
}
```

Key points:
- `Resources.FindObjectsOfTypeAll<GameObject>()` finds objects even if they're inactive or in prefab state
- `HideFlags.HideAndDontSave` prevents Unity from destroying the clone during scene transitions
- `SetActive(false)` is critical — the prefab should only be activated when spawned by the game
- NetworkIdentity must be reset to avoid multiplayer hash collisions

### Step 2: Clone the WeaponInfo (ScriptableObject)

```csharp
public static WeaponInfo DuplicateWeaponInfo(string originalName, string newName, GameObject weapon)
{
    // Find original WeaponInfo by name
    WeaponInfo original = Resources.FindObjectsOfTypeAll<WeaponInfo>()
        .FirstOrDefault(info => info.name.Equals(originalName, StringComparison.InvariantCultureIgnoreCase));
    
    // Clone it (Object.Instantiate works on ScriptableObjects)
    WeaponInfo clone = Object.Instantiate<WeaponInfo>(original);
    clone.name = newName;
    clone.weaponPrefab = weapon;
    
    // Update back-references on the weapon's components
    if (weapon != null)
    {
        // For missiles: update the private 'info' field
        Missile missile = weapon.GetComponent<Missile>();
        if (missile != null)
        {
            typeof(Missile).GetField("info", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(missile, clone);
        }
        
        // For jamming pods: update via Weapon base class
        JammingPod jammer = weapon.GetComponentInChildren<JammingPod>();
        if (jammer != null)
        {
            typeof(Weapon).GetField("info", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .SetValue(jammer, clone);
            clone.weaponPrefab = null;  // JammingPods don't use weaponPrefab
        }
    }
    
    return clone;
}
```

Key points:
- `Object.Instantiate<WeaponInfo>(original)` deep-clones the ScriptableObject
- Back-references on weapon components (Missile, JammingPod, etc.) must be updated via reflection because the `info` field is typically private
- For guns, the `info` field on the `Gun` component is set separately (see `ProcessWeaponCommand` / `modify3` in QoL)

### Step 3: Clone the WeaponMount (ScriptableObject)

```csharp
public static WeaponMount DuplicateWeaponMount(
    string originalName, string newName,
    GameObject duplicateMount, WeaponInfo info,
    VehicleDefinition vehicleDefinition = null)
{
    WeaponMount original = Resources.FindObjectsOfTypeAll<WeaponMount>()
        .FirstOrDefault(m => m.name.Equals(originalName, StringComparison.InvariantCultureIgnoreCase));
    
    WeaponMount clone = Object.Instantiate<WeaponMount>(original);
    clone.name = newName;
    clone.jsonKey = newName;  // CRITICAL: must be unique for save/load
    clone.dontAutomaticallyAddToEncyclopedia = false;
    
    // Update MountedMissile references to point to new WeaponInfo
    foreach (MountedMissile mm in duplicateMount.GetComponentsInChildren<MountedMissile>(true))
    {
        typeof(MountedMissile).GetField("info", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .SetValue(mm, info);
    }
    
    // Update MountedCargo references (for cargo/vehicle drops)
    foreach (MountedCargo mc in duplicateMount.GetComponentsInChildren<MountedCargo>(true))
    {
        typeof(MountedCargo).GetField("cargo", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .SetValue(mc, vehicleDefinition);
        typeof(MountedCargo).GetField("info", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .SetValue(mc, info);
    }
    
    clone.info = info;
    clone.prefab = duplicateMount;
    
    return clone;
}
```

Key points:
- `jsonKey` MUST be unique — this is how the game serializes loadouts to JSON
- All `MountedMissile` and `MountedCargo` child components need their `info` fields updated via reflection
- The `prefab` field points to the cloned GameObject from Step 1

### Step 4: Register in Encyclopedia

```csharp
public static void AddMountToEncyclopedia(Encyclopedia instance, string name, WeaponMount mount)
{
    instance.weaponMounts.Add(mount);
}
```

That's it for the simple version. QoL calls this from a **PREFIX** patch on `Encyclopedia.AfterLoad`, which means the mount is added to `weaponMounts` BEFORE `AfterLoad` iterates the list to build `IndexLookup`. This is the cleanest approach because `AfterLoad` will:
1. Call `mount.Initialize()` on your new mount
2. Add it to `WeaponLookup` (keyed by `jsonKey`)
3. Assign `LookupIndex` via `AddWithIndex`

If you use a POSTFIX instead, you must manually handle `LookupIndex` and `WeaponLookup`:

```csharp
// POSTFIX approach — must manually register
private void AddWeaponMountToEncyclopedia(WeaponMount weaponMount)
{
    Encyclopedia enc = Encyclopedia.i;
    enc.weaponMounts.Add(weaponMount);
    
    // Manually add to WeaponLookup (static Dictionary<string, WeaponMount>)
    var field = typeof(Encyclopedia).GetField("WeaponLookup", BindingFlags.Static | BindingFlags.NonPublic);
    var lookup = (Dictionary<string, WeaponMount>)field.GetValue(null);
    lookup[weaponMount.jsonKey] = weaponMount;
    
    // LookupIndex must also be set for networking
    // (cast to INetworkDefinition to access the explicit interface property)
}
```

**Recommendation: Use PREFIX.** It's simpler and less error-prone.

### Step 5: Add to Aircraft Hardpoints

Two approaches:

#### A) Direct code (in your Harmony patch)

```csharp
// Find the aircraft prefab's WeaponManager
WeaponManager wm = FindGameObjectByExactPath("Darkreach/fuselage_F/cockpit")
    .GetComponent<WeaponManager>();

// Add your mount to a specific hardpoint set's weapon options
// Index is 0-based: 0=Inner Bay, 1=Forward Bay, etc.
if (!wm.hardpointSets[0].weaponOptions.Contains(yourMount))
    wm.hardpointSets[0].weaponOptions.Add(yourMount);
```

#### B) QoL's config command approach

QoL uses a text-based command system in `.qol` files:
```
# Add weapon to hardpoint set (1-indexed in commands)
Darkreach addweapon1 YourWeaponMountName
Darkreach addweapon5 JammingPod1

# Remove weapon from hardpoint set
Darkreach removeweapon2 bomb_500_internalx4_stack

# Add null (empty option) to a hardpoint
Darkreach addweapon5 null
```

The `addweaponN` command maps to `ProcessWeaponCommand`, which:
1. Finds the aircraft's `WeaponManager` (searches all children)
2. Gets `hardpointSets[N-1]` (commands are 1-indexed)
3. Calls `hardpointSet.weaponOptions.Add(mount)` if not already present

---

## Adding New Hardpoint Sets

QoL adds entirely new hardpoint sets to aircraft. Example from `commands.qol`:

```
# Add a new hardpoint set (becomes index 4 on Darkreach, which already has 0-3)
Darkreach/fuselage_F/cockpit hardpointset add 4

# Add a physical hardpoint to the new set
Darkreach/fuselage_F/cockpit hardpointset addhardpoint 4

# Configure the hardpoint's transform and part reference
Darkreach/fuselage_F/cockpit hardpointset modifyhardpoint 4 Blank 0 Darkreach/contactSparks Darkreach
```

In code (`ProcessAddHardpointSet`):
```csharp
HardpointSet newSet = new HardpointSet()
{
    name = "NewSet_N",
    hardpoints = new List<Hardpoint>(),
    weaponOptions = new List<WeaponMount>()
};

// Resize the array
HardpointSet[] newArray = new HardpointSet[wm.hardpointSets.Length + 1];
Array.Copy(wm.hardpointSets, newArray, wm.hardpointSets.Length);
newArray[newArray.Length - 1] = newSet;
wm.hardpointSets = newArray;
```

### Hardpoint Preclusion (Mutual Exclusion)

To make hardpoint sets disable each other:
```
Darkreach/fuselage_F/cockpit hardpointset precludehardpoint 0 1
```

This adds index `1` to set `0`'s `precludingHardpointSets`, meaning if set 1 (Forward Bay) has a weapon loaded, set 0 (Inner Bay) is blocked.

In code:
```csharp
hardpointSet.precludingHardpointSets.Add((byte)otherSetIndex);
```

The game checks this in `HardpointSet.BlockedByOtherHardpoint(Loadout)`:
```csharp
public bool BlockedByOtherHardpoint(Loadout loadout)
{
    for (int i = 0; i < loadout.weapons.Count; i++)
    {
        if (loadout.weapons[i] != null && precludingHardpointSets.Contains((byte)i))
            return true;
    }
    return false;
}
```

---

## Modifying Existing Weapon Properties

QoL's `modifymount` command modifies fields on existing WeaponMount objects:

```
gun_155mm_pod_P modifymount mountName "40mm Railcannon"
gun_155mm_pod_P modifymount info Railgun1_P
gun_155mm_pod_P modifymount ammo 55
gun_155mm_pod_P modifymount emptyCost 2.5
gun_155mm_pod_P modifymount emptyMass 800
```

In code (`ProcessMountMod`):
```csharp
WeaponMount mount = Resources.FindObjectsOfTypeAll<WeaponMount>()
    .FirstOrDefault(m => m.name == mountName);

FieldInfo field = mount.GetType().GetField(fieldName);

if (fieldName == "info")
{
    // Special case: look up WeaponInfo by name
    mount.info = Resources.FindObjectsOfTypeAll<WeaponInfo>()
        .FirstOrDefault(i => i.name.Equals(value, StringComparison.InvariantCultureIgnoreCase));
}
else
{
    field.SetValue(mount, Convert.ChangeType(value, field.FieldType));
}
```

---

## Harmony Patches Required

### Essential Patches

| Patch Target | Type | Purpose |
|---|---|---|
| `Encyclopedia.AfterLoad` | PREFIX | Inject custom weapons into `weaponMounts` before IndexLookup is built |
| `Hardpoint.SpawnMount` | PREFIX | Null-check validation before spawning (prevents NREs from bad mounts) |
| `WeaponChecker.VetLoadout` | PREFIX | Ensure custom weapon is in `weaponOptions` on the PREFAB before spawn validation strips it |

### Useful Patches

| Patch Target | Type | Purpose |
|---|---|---|
| `WeaponSelector.PopulateOptions` | POSTFIX | Resize dropdown to fit longer weapon names |
| `WeaponMount.Initialize` | PREFIX | Diagnostic logging for mount initialization |
| `MountedCargo.AttachToHardpoint` | PREFIX | Diagnostic logging for cargo attachment |
| `HardpointSet.BlockedByOtherHardpoint` | POSTFIX | Override preclusion logic for custom weapons |
| `WeaponManager.OrganizeWeaponStations` | POSTFIX | Ensure custom weapons appear in weapon cycling |

### Critical: VetLoadout

`WeaponChecker.VetLoadout` is called during `Spawner.TrySpawnAircraft` and validates the loadout against the PREFAB's `WeaponManager`. If your weapon isn't in `hardpointSet.weaponOptions` on the prefab, it gets stripped:

```csharp
// Game code (simplified):
if (!hardpointSet.weaponOptions.Contains(weapon))
    weapon = null;  // Stripped!
```

You MUST ensure your weapon is in `weaponOptions` before this runs. Patch it as a PREFIX:

```csharp
[HarmonyPrefix]
[HarmonyPatch(typeof(WeaponChecker), "VetLoadout")]
static void EnsureWeaponInOptions(/* params */)
{
    // Find the prefab's WeaponManager and add your mount to weaponOptions
}
```

---

## Finding Game Objects

QoL uses a path-based system to find GameObjects:

```csharp
// By exact hierarchy path (most reliable)
GameObject obj = FindGameObjectByExactPath("Darkreach/fuselage_F/cockpit");

// By type search (for ScriptableObjects)
WeaponMount mount = Resources.FindObjectsOfTypeAll<WeaponMount>()
    .FirstOrDefault(m => m.name.Equals("AAM2_double_internal", StringComparison.InvariantCultureIgnoreCase));

// By type search (for components)
WeaponInfo info = Resources.FindObjectsOfTypeAll<WeaponInfo>()
    .FirstOrDefault(i => i.name.Equals("AAM2_info", StringComparison.InvariantCultureIgnoreCase));
```

`FindGameObjectByExactPath` splits the path by `/`, finds root objects matching the first segment, then walks `transform.Find()` down the hierarchy. QoL caches results in a `Dictionary<string, GameObject>`.

---

## Complete Workflow: Adding a New Weapon

### Minimal Example: Duplicate an existing missile mount for a new aircraft slot

```csharp
[HarmonyPatch(typeof(Encyclopedia), "AfterLoad")]
class EncyclopediaPatch
{
    [HarmonyPrefix]
    static bool Prefix(Encyclopedia __instance)
    {
        // 1. Clone the prefab
        GameObject prefab = DuplicatePrefab("AGM_heavy_internal", "MyCustomAGM_internal");
        
        // 2. Clone the WeaponInfo
        WeaponInfo info = DuplicateWeaponInfo("info_AGM_heavy", "MyCustomAGM_info", prefab);
        info.weaponName = "My Custom AGM";
        info.shortName = "CAGM";
        
        // 3. Clone the WeaponMount
        WeaponMount mount = DuplicateWeaponMount(
            "AGM_heavy_internal", "MyCustomAGM_internal", prefab, info);
        mount.emptyCost = 5f;    // $5 million
        mount.emptyMass = 500f;  // 500 kg
        
        // 4. Register in encyclopedia (PREFIX means AfterLoad handles IndexLookup)
        __instance.weaponMounts.Add(mount);
        
        // 5. Add to aircraft hardpoint
        WeaponManager wm = FindGameObjectByExactPath("Darkreach/fuselage_F/cockpit")
            .GetComponent<WeaponManager>();
        wm.hardpointSets[0].weaponOptions.Add(mount);  // Inner Bay
        
        return true;  // Let original AfterLoad run
    }
}
```

### Modifying Weapon Visuals

To swap meshes on a cloned weapon (e.g., replace missile model):
```csharp
Material originalMat = originalGO.GetComponent<MeshRenderer>().sharedMaterial;
Mesh originalMesh = originalGO.GetComponent<MeshFilter>().sharedMesh;

foreach (Transform child in clonedMount.GetComponentsInChildren<Transform>(true))
{
    if (child.gameObject.name.Contains("targetMeshName"))
    {
        child.gameObject.GetComponent<MeshRenderer>().sharedMaterial = originalMat;
        child.gameObject.GetComponent<MeshFilter>().sharedMesh = originalMesh;
    }
}
```

### Loading Custom Assets from Embedded Resources

QoL embeds AssetBundles as assembly resources:
```csharp
Assembly asm = Assembly.GetExecutingAssembly();
using (Stream stream = asm.GetManifestResourceStream(asm.GetName().Name + ".Resources.MyBundle.bundle"))
using (MemoryStream ms = new MemoryStream())
{
    stream.CopyTo(ms);
    AssetBundle bundle = AssetBundle.LoadFromMemory(ms.ToArray());
    GameObject[] assets = bundle.LoadAllAssets<GameObject>();
    // Use assets[0].GetComponent<MeshFilter>().mesh, etc.
    bundle.Unload(false);  // false = keep loaded assets in memory
}
```

---

## Common Pitfalls

1. **Forgetting `HideFlags.HideAndDontSave`** — Your cloned prefab gets garbage collected and you get NullReferenceExceptions at spawn time.

2. **Not resetting NetworkIdentity** — Multiplayer hash collisions cause desync or spawn failures.

3. **Using POSTFIX without setting LookupIndex** — Network serialization breaks. Use PREFIX on `Encyclopedia.AfterLoad` instead.

4. **Not adding to `weaponOptions` on the PREFAB** — `WeaponChecker.VetLoadout` strips your weapon before spawn. Must be on the prefab's `WeaponManager`, not just the spawned instance.

5. **jsonKey collision** — If two WeaponMounts share a `jsonKey`, `Encyclopedia.AfterLoad` throws on `Dictionary.Add`. Always use unique keys.

6. **Unity `Instantiate` doesn't run C# constructors** — MonoBehaviour field initializers and constructors don't execute on `Instantiate`. If your custom component has fields that need initialization, use a `ForceInitialize()` method called after instantiation.

7. **Reflection field names are case-sensitive** — `"info"` ≠ `"Info"`. Use the exact field name from the decompiled source.

8. **`WeaponChecker.GetAvailableWeaponsNonAlloc` starts at index 1** — Index 0 in `weaponOptions` is always "Empty"/null. Your weapon should be at index ≥ 1.

9. **Modifying prefabs affects ALL instances** — If you change a field on a shared prefab, every aircraft using that prefab is affected. Clone first if you only want to modify one variant.

10. **Gun prefabs must be active for SpawnMount** — QoL's `HardpointSpawnPatch` temporarily activates gun prefabs before `SpawnMount` and deactivates them after, because the game's spawn logic requires the prefab to be active for guns specifically.

---

## QoL Config Command Reference

For mods that want to replicate QoL's text-based config system:

| Command | Syntax | Example |
|---|---|---|
| Add weapon | `<aircraft> addweapon<N> <mountName>` | `Darkreach addweapon1 MyMount` |
| Remove weapon | `<aircraft> removeweapon<N> <mountName>` | `Darkreach removeweapon2 bomb_500` |
| Add empty option | `<aircraft> addweapon<N> null` | `Darkreach addweapon5 null` |
| Add hardpoint set | `<path> hardpointset add <index>` | `Darkreach/fuselage_F/cockpit hardpointset add 4` |
| Add hardpoint | `<path> hardpointset addhardpoint <setIndex>` | — |
| Modify hardpoint | `<path> hardpointset modifyhardpoint <set> <name> <hp> <transform> <part>` | — |
| Preclude set | `<path> hardpointset precludehardpoint <set> <otherSet>` | — |
| Modify mount field | `<mount> modifymount <field> <value>` | `MyMount modifymount emptyMass 500` |

Note: `addweapon` commands are **1-indexed** (addweapon1 = hardpointSets[0]).

---

## Key Type References

| Type | Location | Notes |
|---|---|---|
| `Encyclopedia` | Assembly-CSharp | Singleton, loaded via `ResourcesAsyncLoader` |
| `WeaponMount` | Assembly-CSharp | ScriptableObject, implements `INetworkDefinition`, `IHasJsonKey` |
| `WeaponInfo` | Assembly-CSharp | ScriptableObject |
| `WeaponManager` | Assembly-CSharp | MonoBehaviour on aircraft cockpit |
| `HardpointSet` | Assembly-CSharp | Serializable class (not MonoBehaviour) |
| `Hardpoint` | Assembly-CSharp | Serializable class with `transform` and `part` |
| `WeaponChecker` | Assembly-CSharp | Static methods for loadout validation |
| `WeaponSelector` | Assembly-CSharp | UI component for weapon dropdown |
| `Spawner` | Assembly-CSharp | Calls `VetLoadout` before spawning aircraft |
