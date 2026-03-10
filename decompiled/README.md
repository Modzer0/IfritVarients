# Decompiled Game Code

This directory contains decompiled code from Nuclear Option's `Assembly-CSharp.dll` for reference during mod development.

## Important Notes

⚠️ **This directory is excluded from version control** - The decompiled code is proprietary and should not be committed to the repository.

## Setup Instructions

1. **Locate the game assembly:**
   ```
   D:\SteamLibrary\steamapps\common\Nuclear Option\Nuclear Option_Data\Managed\Assembly-CSharp.dll
   ```

2. **Decompile using ILSpy:**
   - Open ILSpy
   - File → Open → Select `Assembly-CSharp.dll`
   - File → Save Code → Select this `decompiled/` folder
   - This will create a folder structure with all decompiled C# files

3. **Alternative: Use dnSpy for debugging:**
   - Open dnSpy
   - File → Open → Select `Assembly-CSharp.dll`
   - Browse classes in the tree view
   - No need to save - use for interactive debugging

## What to Look For

Based on the requirements, document these key classes:

### AI Systems
- [ ] `AircraftAI` or `UnitAI` - Main AI controller
- [ ] `CombatManager` - Combat decision-making
- [ ] `TargetingSystem` - Target selection logic
- [ ] `WeaponController` - Weapon management

### Detection & Information
- [ ] `DatalinkSystem` - Information sharing between units
- [ ] `RadarSystem` - Radar detection
- [ ] `SensorSystem` - Optical/IR detection
- [ ] `RWRSystem` - Radar warning receiver

### Movement & Control
- [ ] `FlightController` - Aircraft movement
- [ ] `PathfindingSystem` - Route planning
- [ ] `FormationController` - Formation flying
- [ ] `MovementController` - General movement

### Weapons
- [ ] `MissileSystem` - Missile guidance
- [ ] `GunSystem` - Gun control
- [ ] `WeaponData` - Weapon characteristics

### Airfield Operations
- [ ] `AirfieldManager` - Airfield operations
- [ ] `LandingSystem` - Landing procedures
- [ ] `RefuelSystem` - Refuel/rearm logic
- [ ] `TrafficController` - Air traffic management

### Naval & Ground
- [ ] `ShipAI` - Naval unit AI
- [ ] `TankAI` - Ground unit AI
- [ ] `NavalWeaponSystem` - Ship weapons

## Documentation Template

Create a `game-code-analysis.md` file documenting your findings:

```markdown
# Game Code Analysis

## AI Controller

**Class Name:** `AircraftAI`
**Namespace:** (blank or specific namespace)

### Key Methods
- `UpdateBehavior()` - Main AI update loop
- `SelectTarget()` - Target selection logic
- `FireWeapon()` - Weapon firing logic

### Private Fields
- `currentSpeed` (float)
- `currentTarget` (Target)
- `ammunition` (int)

### Methods to Patch
1. `SelectTarget()` - Improve target prioritization (Req 11, 35)
2. `UpdateBehavior()` - Add skill level delays (Req 41)
3. `FireWeapon()` - Add weapon selection logic (Req 14, 38)

## [Continue for each system...]
```

## Usage During Development

Reference the decompiled code when:
- Writing Harmony patches (need exact method signatures)
- Accessing private fields (need exact field names)
- Understanding game logic flow
- Debugging patch issues

## Legal Note

The decompiled code is for reference only during mod development. Do not redistribute or commit to version control.
