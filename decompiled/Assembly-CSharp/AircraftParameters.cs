// Decompiled with JetBrains decompiler
// Type: AircraftParameters
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.SavedMission;
using System;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
[CreateAssetMenu(fileName = "New Aircraft Parameters", menuName = "ScriptableObjects/AircraftParameters", order = 1)]
public class AircraftParameters : ScriptableObject
{
  public string aircraftName;
  public string aircraftDescription;
  public int rankRequired;
  public Airfoil[] airfoils;
  private Airfoil defaultAirfoil;
  public List<Loadout> loadouts;
  public StandardLoadout[] StandardLoadouts;
  public float DefaultFuelLevel = 1f;
  public List<AircraftParameters.Livery> liveries;
  public GameObject StatusDisplay;
  public GameObject HUDExtras;
  public AudioClip takeoffMusic;
  public AircraftParameters.OnboardAoAEffects AoAEffects;
  public float aircraftGLimit = 9f;
  public float PIDReferenceAirspeed;
  public float maxSpeed;
  public float takeoffSpeed;
  public float takeoffDistance;
  public bool verticalLanding;
  public float turningRadius;
  public float cornerSpeed;
  public float approachSpeed = 60f;
  public float landingSpeed = 30f;
  public float shortLandingSpeed = 30f;
  public float cruiseThrottle = 0.9f;
  public float minimumRadarAlt;
  public float levelBias;
  public float hoverTiltFactor = 1f;
  public Vector3 collectivePID;
  public Vector3 hoverPID;
  public Vector3 tiltPID;
  public float groundTurningRadius = 10f;

  public int GetAirfoilID(int index) => index < 0 ? -1 : this.airfoils[index].id;

  public void AddAirfoils(ref List<Airfoil> airfoilsList)
  {
    foreach (Airfoil airfoil in this.airfoils)
      airfoilsList.Add(airfoil);
  }

  public StandardLoadout GetRandomStandardLoadout(AircraftDefinition definition, FactionHQ hq)
  {
    if (this.StandardLoadouts == null)
      return (StandardLoadout) null;
    if (this.StandardLoadouts.Length == 0)
      return (StandardLoadout) null;
    WeaponManager weaponManager = definition.unitPrefab.GetComponent<Aircraft>().weaponManager;
    List<StandardLoadout> standardLoadoutList = new List<StandardLoadout>();
    foreach (StandardLoadout standardLoadout in this.StandardLoadouts)
    {
      if (!standardLoadout.disabled && standardLoadout.AllowedByHQ(weaponManager, hq))
        standardLoadoutList.Add(standardLoadout);
    }
    if (standardLoadoutList.Count == 0)
      return (StandardLoadout) null;
    int index = UnityEngine.Random.Range(0, standardLoadoutList.Count);
    return standardLoadoutList[index];
  }

  public int GetRandomLiveryForFaction(Faction faction)
  {
    int maxExclusive = 0;
    foreach (AircraftParameters.Livery livery in this.liveries)
    {
      if ((UnityEngine.Object) livery.faction == (UnityEngine.Object) faction)
        ++maxExclusive;
    }
    if (maxExclusive == 0)
      return 0;
    int num = UnityEngine.Random.Range(0, maxExclusive);
    for (int index = 0; index < this.liveries.Count; ++index)
    {
      if ((UnityEngine.Object) this.liveries[index].faction == (UnityEngine.Object) faction)
      {
        if (num == 0)
          return index;
        --num;
      }
    }
    Debug.LogError((object) "Failed random index failed");
    return 0;
  }

  public int GetFirstLiveryForFaction(Faction faction)
  {
    for (int index = 0; index < this.liveries.Count; ++index)
    {
      if ((UnityEngine.Object) this.liveries[index].faction == (UnityEngine.Object) faction)
        return index;
    }
    if ((UnityEngine.Object) faction != (UnityEngine.Object) null)
      Debug.LogWarning((object) ("No skips found for faction " + faction.factionName));
    return 0;
  }

  [Serializable]
  public class OnboardAoAEffects
  {
    public AudioClip AudioClip;
    public float OnsetSpeed = 60f;
    public float FullVolumeSpeed = 200f;
    public float OnsetAlpha = 5f;
    public float FullVolumeAlpha = 45f;
    public float ShakeFactor = 0.1f;
  }

  [Serializable]
  public class Livery
  {
    public string name;
    public Faction faction;
    public AssetReferenceLiveryData assetReference;
  }
}
