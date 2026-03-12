# Ifrit Variants — Nuclear Option Mod

A BepInEx mod for [Nuclear Option](https://store.steampowered.com/app/1456900/Nuclear_Option/) that adds two new variants of the KR-67A Ifrit multirole fighter. Both aircraft appear in the hangar alongside the base KR-67A and are available in the mission editor.

## Aircraft

### KR-67EX

An upgraded KR-67A with more powerful engines, enhanced radar, additional missile mounts, and a reinforced airframe.

| Stat | Value |
|------|-------|
| Service Ceiling | 60,000 ft |
| Thrust | 1.2x base KR-67A |
| Overspeed | None |
| G Limit | 12G |
| Radar Jammer | Enhanced (intensity 12) |
| Weapons | Standard KR-67A loadout plus quad AAM-29 and AAM-36 internal pylon mounts |
| Cost | 2x base KR-67A |

### KR-67R

A stealth reconnaissance variant with a drastically reduced radar cross-section, long-range optical sensor, and high-altitude/high-speed performance. Designed to fly fast and high — not to fight.

| Stat | Value |
|------|-------|
| Service Ceiling | 85,000 ft |
| Cruise Altitude | 80,000 ft |
| Top Speed | Mach 5 at altitude |
| Overspeed | Mach 5 |
| Thrust | 3x base KR-67A |
| RCS | 1/100th of base KR-67A |
| Optical Sensor | 50 NM range |
| Radar | Disabled |
| Radar Jammer | Enhanced (intensity 12) |
| Weapons | Internal weapons bay only (gun, forward bay, and wing pylons disabled) |
| Cost | 4x base KR-67A |

**Note:** Opening weapon bay doors will spike the RCS, making the aircraft visible to radar and air defenses. Use ordnance sparingly.

## Requirements

- [Nuclear Option](https://store.steampowered.com/app/1456900/Nuclear_Option/)
- [BepInEx 5.x](https://github.com/BepInEx/BepInEx) installed for Nuclear Option

## Installation

1. Download `IfritVariants_v1.3.dll` from the [latest release](https://github.com/Modzer0/IfritVarients/releases/latest).
2. Drop the DLL into your `BepInEx/plugins/` folder:
   ```
   <Steam Library>/steamapps/common/Nuclear Option/BepInEx/plugins/
   ```
3. Remove any older `IfritVariants_*.dll` files from the plugins folder.
4. Clear the BepInEx cache (optional but recommended after updates):
   ```
   <Steam Library>/steamapps/common/Nuclear Option/BepInEx/cache/
   ```
5. Launch the game. Both variants will appear in the hangar and mission editor.

## Configuration

After first launch, a config file is generated at:
```
BepInEx/config/com.nuclearoption.ifritvariants.cfg
```

You can tweak thrust multipliers, speed limits, G limits, costs, RCS divisor, optical sensor range, jammer intensity, and enable/disable each variant individually.

## Building from Source

Requires .NET SDK with `net472` targeting pack.

```bash
cd src
dotnet build IfritVariants.csproj -c Release
```

The DLL is output to `src/bin/Release/net472/` and automatically copied to your BepInEx plugins folder.

Game references are resolved from `D:\SteamLibrary\steamapps\common\Nuclear Option` — update the `GameDir` property in `IfritVariants.csproj` if your install path differs.

## License

See [LICENSE](LICENSE).
