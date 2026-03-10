// Decompiled with JetBrains decompiler
// Type: CountermeasureManager
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
[Serializable]
public class CountermeasureManager
{
  [SerializeField]
  private List<CountermeasureManager.CountermeasureStation> countermeasureStations;
  public byte activeIndex;
  [SerializeField]
  private Aircraft aircraft;

  public void Initialize()
  {
    this.aircraft.OnRearm += new Action<RearmEventArgs>(this.CountmeasureManager_OnRearm);
  }

  public void RegisterCountermeasure(Countermeasure countermeasure)
  {
    foreach (CountermeasureManager.CountermeasureStation countermeasureStation in this.countermeasureStations)
    {
      if (countermeasureStation.displayName == countermeasure.displayName)
      {
        countermeasureStation.AddCountermeasure(countermeasure);
        return;
      }
    }
    this.countermeasureStations.Add(new CountermeasureManager.CountermeasureStation(countermeasure));
    if (GameManager.IsLocalAircraft(this.aircraft))
      this.countermeasureStations[0].SetActive(this.aircraft);
    if (this.countermeasureStations.Count <= 1)
      return;
    this.countermeasureStations.Sort((Comparison<CountermeasureManager.CountermeasureStation>) ((x, y) => x.displayName.CompareTo(y.displayName)));
  }

  public void DeregisterCountermeasure(Countermeasure countermeasure)
  {
    for (int index = this.countermeasureStations.Count - 1; index >= 0; --index)
    {
      if (this.countermeasureStations[index].displayName == countermeasure.displayName)
      {
        int countermeasuresRemaining;
        this.countermeasureStations[index].RemoveCountermeasure(countermeasure, out countermeasuresRemaining);
        if (countermeasuresRemaining != 0)
          break;
        this.countermeasureStations.RemoveAt(index);
        break;
      }
    }
  }

  public void NextCountermeasure()
  {
    ++this.activeIndex;
    if ((int) this.activeIndex >= this.countermeasureStations.Count)
      this.activeIndex = (byte) 0;
    this.aircraft.Countermeasures(false, this.activeIndex);
    this.countermeasureStations[(int) this.activeIndex].SetActive(this.aircraft);
  }

  public void DeployCountermeasure(Aircraft aircraft)
  {
    if (this.activeIndex == byte.MaxValue)
      return;
    this.countermeasureStations[(int) this.activeIndex].Fire(aircraft);
  }

  public void UpdateHUD()
  {
    if (this.countermeasureStations.Count <= 0)
      return;
    this.countermeasureStations[(int) this.activeIndex].SetActive(this.aircraft);
  }

  public string ChooseCountermeasure(Missile missileThreat)
  {
    string seekerType = missileThreat.GetSeekerType();
    this.activeIndex = byte.MaxValue;
    for (byte index = 0; (int) index < this.countermeasureStations.Count; ++index)
    {
      if (this.countermeasureStations[(int) index].threatTypes.Contains(seekerType))
      {
        this.activeIndex = index;
        break;
      }
    }
    this.aircraft.Countermeasures(false, this.activeIndex);
    return this.activeIndex != byte.MaxValue ? seekerType : string.Empty;
  }

  public void CountmeasureManager_OnRearm(RearmEventArgs e)
  {
    foreach (CountermeasureManager.CountermeasureStation countermeasureStation in this.countermeasureStations)
      countermeasureStation.Rearm(this.aircraft, e.rearmer);
  }

  public Countermeasure GetActiveCountermeasure()
  {
    return this.countermeasureStations.Count > 0 ? this.countermeasureStations[(int) this.activeIndex].GetFirstCountermeasure() : (Countermeasure) null;
  }

  [Serializable]
  private class CountermeasureStation
  {
    [SerializeField]
    private List<Countermeasure> countermeasures;
    public List<string> threatTypes;
    public string displayName;
    public Sprite icon;
    public int ammo;

    public CountermeasureStation(Countermeasure countermeasure)
    {
      this.countermeasures = new List<Countermeasure>()
      {
        countermeasure
      };
      this.threatTypes = countermeasure.GetThreatTypes();
      this.displayName = countermeasure.displayName;
      this.icon = countermeasure.displayImage;
      this.ammo += countermeasure.ammo;
    }

    public void AddCountermeasure(Countermeasure countermeasure)
    {
      if (this.threatTypes == null)
        this.threatTypes = new List<string>();
      foreach (string threatType in countermeasure.GetThreatTypes())
      {
        if (!this.threatTypes.Contains(threatType))
          this.threatTypes.Add(threatType);
      }
      this.countermeasures.Add(countermeasure);
      this.displayName = countermeasure.displayName;
      this.icon = countermeasure.displayImage;
      this.ammo += countermeasure.ammo;
    }

    public void RemoveCountermeasure(
      Countermeasure countermeasure,
      out int countermeasuresRemaining)
    {
      this.countermeasures.Remove(countermeasure);
      countermeasuresRemaining = this.countermeasures.Count;
    }

    public void SetActive(Aircraft aircraft) => this.CountAmmo(aircraft);

    public void Rearm(Aircraft aircraft, Unit rearmer)
    {
      foreach (Countermeasure countermeasure in this.countermeasures)
        countermeasure.Rearm(aircraft, rearmer);
      this.CountAmmo(aircraft);
    }

    public void CountAmmo(Aircraft aircraft)
    {
      this.ammo = 0;
      foreach (Countermeasure countermeasure in this.countermeasures)
        this.ammo += countermeasure.ammo;
      if (!((UnityEngine.Object) aircraft == (UnityEngine.Object) SceneSingleton<CombatHUD>.i.aircraft))
        return;
      this.countermeasures[0].UpdateHUD();
      SceneSingleton<CombatHUD>.i.DisplayCountermeasureAmmo(this.ammo);
    }

    public Countermeasure GetFirstCountermeasure() => this.countermeasures[0];

    public void Fire(Aircraft aircraft)
    {
      foreach (Countermeasure countermeasure in this.countermeasures)
        countermeasure.Fire();
      this.CountAmmo(aircraft);
    }
  }
}
