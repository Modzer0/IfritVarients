# Nuclear Option Modding Notes

## Game Internals

### Units
- Game uses **meters** internally, altimeter displays **feet**. Conversion: `feet * 0.3048 = meters`.
- Speed of sound: `LevelInfo.GetSpeedOfSound(altitude) = Max(-0.005 * altitude + 340, 290)` (m/s). Floors at 290 m/s at high altitude.
- Air density: `LevelInfo.GetAirDensity(altitude)` uses a 64-entry lookup chart from `GameAssets.airDensityAltitude` AnimationCurve, sampled at `altitude * 0.0021`.

### Turbojet Engine (`Turbojet.cs`)
- Key fields: `maxThrust`, `maxSpeed` (default 522 m/s), `minDensity`, `altitudeThrust` (AnimationCurve), `thrust`, `rpm`, `spoolRate`.
- **Speed limiter**: In `FixedUpdate`, if `speed > maxSpeed`: `thrust *= Max(1 - 5*(speed - maxSpeed)/maxSpeed, 0)`. Thrust reaches zero at `1.2 * maxSpeed`.
- **Altitude thrust**: `thrust *= altitudeThrust.Evaluate(altitude)` — AnimationCurve that normally reduces thrust at altitude.
- **Density gate**: Engine won't run if `airDensity < minDensity`. Setting `minDensity = -1` bypasses this.
- **Fuel**: Checked every 1 second via `UseFuel()`.
- RPM spools up/down via `spoolRate`. `thrustRatio = (rpm - minRPM*0.85) / (maxRPM - minRPM)`.

### JetNozzle / Afterburner
- `JetNozzle.Thrust(thrust, rpmRatio, thrustRatio, throttle, allowAfterburner)` called per nozzle per engine.
- Afterburner nested type has its own `thrust` field and `GetThrust()` method.
- `totalThrust` field on JetNozzle accumulates engine + afterburner thrust.

### Airbrake (`Airbrake.cs`)
- **Activation**: Deploys when `throttle == 0`, retracts otherwise. Rate controlled by `openSpeed` field.
- **openAmount**: 0-1 float, clamped. Ramps at `openSpeed * deltaTime`.
- **Drag force** (in `FixedUpdate`):
  ```
  force = openAmount * -velocity.normalized * (dragAmount * airDensity * velocity.sqrMagnitude)
  ```
  This is a standard drag equation: `F = dragAmount * ρ * v² * openAmount`, applied opposite to velocity.
- **Key fields**:
  - `dragAmount`: drag coefficient/area product (serialized, set per aircraft prefab)
  - `maxAngle`: visual rotation angle when fully open
  - `openSpeed`: rate of deployment (units/sec)
  - `maxVolumeSpeed`: speed at which audio reaches full volume (default 340 m/s, i.e. Mach 1)
- **Note**: Drag scales with `airDensity * v²` (dynamic pressure). At high altitude where density is very low, airbrakes become nearly useless even at hypersonic speeds. At Mach 10 / 50km altitude, density might be ~0.001 vs 1.225 at sea level — airbrake effectiveness is ~0.08% of sea level value despite 10x the speed.
- Airbrake force is applied via `part.rb.AddForce()` on the specific UnitPart's rigidbody, not the whole aircraft — so it can create torque if offset from CoM.

### Aerodynamics (`AeroJob_Math.cs`, Burst-compiled)
- Runs per `AeroPart` in parallel via Unity Jobs.
- **Lift**: `liftCoef * airDensity * v² * 0.5 * wingArea * wingEffectiveness`
- **Drag**: `dragCoef * airDensity * v² * 0.5 * wingArea * wingEffectiveness` + parasitic drag from `dragArea`
- **Transonic drag rise**: Between Mach 0.8-1.2, drag gets a cubic bump (wave drag): `drag *= 1 + ((0.2 - |1-mach|)/0.2)³ * 0.15`
- Lift/drag coefficients come from airfoil lookup charts (128-entry tables indexed by AoA).
- `wingEffectiveness` field on each AeroPart controls how much lift/drag the surface produces. Setting to 0 makes it a pure drag body.
- Water submersion reduces `wingEffectiveness` toward 0.01 and increases density toward 1000.

### Control Systems

#### FlyByWire (`ControlsFilter.FlyByWire`)
- PD controller for pitch rate. Uses `speed * airDensity / 1.225` as effective speed (`a`).
- At low effective speed (high altitude), uses `pFactorSlow`/`dFactorSlow` gains. At high effective speed, uses `pFactorFast`/`dFactorFast`.
- `pitchAdjuster` is an integrator clamped to `pitchAdjusterLimitSlow/Fast`.
- G-limit: `gLimitPositive * 9.81 / max(effectiveSpeed, cornerSpeed*0.75)` — target pitch rate from stick input.
- AoA limiter kicks in below `cornerSpeed * 1.3` when AoA exceeds `alphaLimiter` degrees.
- Roll uses angular velocity damping with `rollTightness` and `maxRollAngularVel`.
- **Hypersonic issue**: At high altitude, `effectiveSpeed` is low (density is tiny), so FBW thinks it's slow and uses aggressive gains. But actual aero forces (∝ density * v²) are still significant at hypersonic speeds, causing oscillations.

#### RelaxedStabilityController (`RelaxedStabilityController.cs`)
- Simple canard-style stability augmentation.
- `FilterInput`: Blends AoA (angle between velocity and forward) divided by `canardRange` with `rawPitch` based on `|rawPitch|`.
  ```csharp
  float a = GetAngleOnAxis(forward, velocity, right) / canardRange;
  inputs.pitch = Lerp(a, rawPitch, Abs(rawPitch));
  ```
- Only active when `velocity.sqrMagnitude > 900` (speed > 30 m/s).
- Disabled when engine is killed (`effectiveness = 0`).
- **Hypersonic issue**: At high speed, even tiny AoA produces large correction. The `canardRange` divisor is fixed, so corrections become disproportionately strong.

#### Flaps (`ControlSurface.cs`)
- Surfaces with `flap = true` ignore pilot inputs; driven purely by `gearState`.
- Gear extending/extended → flaps deploy. Gear retracting/retracted → flaps retract.
- Logic runs in Burst-compiled `ControlSurfaceJob_Math`.

### Aircraft Cloning
- `Encyclopedia.AfterLoad` postfix to clone `AircraftDefinition`.
- Must clone `aircraftParameters` separately (it's a ScriptableObject).
- `INetworkDefinition.LookupIndex` must be set for multiplayer sync.
- `CacheMass()` must be called after cloning.
- Hangar patches needed: `GetAvailableAircraft` (inject), `CanSpawnAircraft` (allow), `TrySpawnAircraft` (flag), `Spawner.SpawnAircraft` (reassign definition).

---

## KR-67X SuperIfrit Mod

### Current Constants
| Constant | Value | Notes |
|---|---|---|
| TargetMaxSpeed | 4500 m/s | Allows ~Mach 15.5 at altitude before throttle-back |
| DoubledMaxThrust | 200,000 N | Per engine, subsonic/supersonic |
| DoubledAfterburnerThrust | 94,000 N | Per afterburner |
| ScramjetThrustPerEngine | 500,000 N | Per engine in scramjet mode (1,000,000 with Darkstar) |
| ScramjetMinMach | 4.5 | Scramjet activation threshold |
| ScramjetMinAltM | 18,288 m | 60,000 ft minimum altitude for scramjet |
| FlameoutAltM | 49,987 m | 164,000 ft engine ceiling |
| CloneCostMillions | 1000 | $1,000M |

### Features
1. **Aircraft clone**: KR-67X SuperIfrit appears alongside KR-67A in hangar.
2. **Doubled engine thrust**: 200kN per engine (vs stock ~100kN).
3. **Scramjet mode**: Above Mach 4.5 and 60,000 ft, engines produce 500kN each. HUD indicator pulses green.
4. **Darkstar Mode**: BepInEx config option. When enabled, scramjet thrust doubles to 1,000kN per engine.
5. **Engine flameout**: Above 164,000 ft, engines cut out. Relight on descent.
6. **Flap lockout**: Flaps retract above Mach 1 (prevents structural issues).
7. **Hypersonic stability**: RelaxedStabilityController gains reduced above Mach 2. FBW pitch/roll dampened above Mach 3 (factor = 3/mach).
8. **Speed limiter removed**: maxSpeed raised to 4500 m/s, altitude thrust curve flattened, minDensity bypassed.
9. **Overspeed warnings suppressed**: No red airspeed, no overspeed voice callout.
10. **Afterburner density compensation**: At low air density, afterburner thrust is boosted to compensate.

### Patches Applied
| Target | Method | Type | Purpose |
|---|---|---|---|
| Turbojet | FixedUpdate | Prefix+Postfix | Thrust override, maxSpeed, scramjet logic, flameout |
| JetNozzle | Thrust | Prefix+Postfix | Afterburner thrust, density compensation |
| SpeedGauge | Refresh | Prefix+Postfix | Suppress overspeed warnings |
| SpeedGauge | Initialize | Postfix | Set overspeed threshold |
| FlightHud | Update | Postfix | Scramjet HUD indicator |
| Encyclopedia | AfterLoad | Postfix | Clone aircraft definition |
| Hangar | GetAvailableAircraft | Postfix | Inject clone into hangar |
| Hangar | CanSpawnAircraft | Postfix | Allow clone spawning |
| Hangar | TrySpawnAircraft | Prefix | Flag clone spawn |
| Spawner | SpawnAircraft | Postfix | Reassign definition post-spawn |
| AircraftSelectionMenu | SpawnPreview | Postfix | Fix preview definition |
| ControlSurface | UpdateJobFields | Prefix+Postfix | Flap lockout above Mach 1 |
| RelaxedStabilityController | FilterInput | Prefix+Postfix | Dampen stability at hypersonic |
| ControlsFilter | Filter | Postfix | Dampen FBW at hypersonic |
