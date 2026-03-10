// Decompiled with JetBrains decompiler
// Type: ThreatList
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
public class ThreatList : MonoBehaviour
{
  [SerializeField]
  private GameObject threatItemPrefab;
  [SerializeField]
  private ThreatList.MissileAlarm[] alarmTypes;
  private MissileWarning missileWarning;
  private Dictionary<PersistentID, ThreatItem> itemLookup = new Dictionary<PersistentID, ThreatItem>();
  private Dictionary<string, ThreatList.MissileAlarm> alarmLookup = new Dictionary<string, ThreatList.MissileAlarm>();

  public void SetAircraft(Aircraft aircraft)
  {
    this.alarmLookup.Clear();
    foreach (ThreatList.MissileAlarm alarmType in this.alarmTypes)
    {
      this.alarmLookup.Add(alarmType.seekerType, alarmType);
      alarmType.Setup(aircraft);
      alarmType.ClearMissiles();
    }
    foreach (KeyValuePair<PersistentID, ThreatItem> keyValuePair in this.itemLookup)
      UnityEngine.Object.Destroy((UnityEngine.Object) keyValuePair.Value.gameObject);
    this.itemLookup.Clear();
    this.missileWarning = aircraft.GetMissileWarningSystem();
    this.missileWarning.onMissileWarning += new Action<MissileWarning.OnMissileWarning>(this.ThreatList_OnMissileWarning);
    this.missileWarning.offMissileWarning += new Action<MissileWarning.OffMissileWarning>(this.ThreatList_OffMissileWarning);
    aircraft.onDisableUnit += new Action<Unit>(this.ThreatList_OnAircraftDisable);
  }

  private void ThreatList_OnMissileWarning(MissileWarning.OnMissileWarning e)
  {
    foreach (KeyValuePair<string, ThreatList.MissileAlarm> keyValuePair in this.alarmLookup)
      keyValuePair.Value.SyncTime();
    if (!this.itemLookup.ContainsKey(e.missile.persistentID))
    {
      ThreatItem component = UnityEngine.Object.Instantiate<GameObject>(this.threatItemPrefab, this.transform).GetComponent<ThreatItem>();
      component.SetItem(e.missile.persistentID);
      ThreatList.MissileAlarm missileAlarm;
      if (!this.alarmLookup.TryGetValue(e.missile.GetSeekerType(), out missileAlarm))
        return;
      missileAlarm.AddMissile(e.missile);
      this.itemLookup.Add(e.missile.persistentID, component);
    }
    this.enabled = true;
    SceneSingleton<CombatHUD>.i.FlashMarker((Unit) e.missile, true);
    SceneSingleton<DynamicMap>.i.FlagIncomingMissile((Unit) e.missile);
  }

  private void ThreatList_OffMissileWarning(MissileWarning.OffMissileWarning e)
  {
    SceneSingleton<CombatHUD>.i.FlashMarker((Unit) e.missile, false);
    SceneSingleton<DynamicMap>.i.ClearIncomingMissile((Unit) e.missile);
    ThreatItem threatItem;
    if (!this.itemLookup.TryGetValue(e.missile.persistentID, out threatItem))
      return;
    UnityEngine.Object.Destroy((UnityEngine.Object) threatItem.gameObject);
    this.alarmLookup[e.missile.GetSeekerType()].RemoveMissile(e.missile);
    this.itemLookup.Remove(e.missile.persistentID);
  }

  private void ThreatList_OnAircraftDisable(Unit unit)
  {
    foreach (ThreatList.MissileAlarm alarmType in this.alarmTypes)
      alarmType.Remove();
    this.missileWarning.onMissileWarning -= new Action<MissileWarning.OnMissileWarning>(this.ThreatList_OnMissileWarning);
    this.missileWarning.offMissileWarning -= new Action<MissileWarning.OffMissileWarning>(this.ThreatList_OffMissileWarning);
  }

  private void Update()
  {
    if (this.itemLookup.Count == 0)
      this.enabled = false;
    foreach (KeyValuePair<PersistentID, ThreatItem> keyValuePair in this.itemLookup)
      keyValuePair.Value.AnimateItem();
    foreach (KeyValuePair<string, ThreatList.MissileAlarm> keyValuePair in this.alarmLookup)
      keyValuePair.Value.ManageAlarmSound();
  }

  [Serializable]
  private class MissileAlarm
  {
    public string seekerType;
    public AudioClip[] alarmClips;
    public AudioClip alertClip;
    private AudioSource alarmSource;
    private List<Missile> missiles;

    public void Setup(Aircraft aircraft)
    {
      this.missiles = new List<Missile>();
      this.alarmSource = aircraft.gameObject.AddComponent<AudioSource>();
      this.alarmSource.playOnAwake = false;
      this.alarmSource.loop = true;
      this.alarmSource.clip = this.alarmClips[0];
      this.alarmSource.volume = 0.8f;
      this.alarmSource.spatialBlend = 0.0f;
      this.alarmSource.dopplerLevel = 0.0f;
      this.alarmSource.pitch = 1f;
      this.alarmSource.outputAudioMixerGroup = SoundManager.i.MissileAlertMixer;
    }

    public void ManageAlarmSound()
    {
      if (this.alarmClips.Length < 2 || this.missiles.Count == 0)
        return;
      int a = this.alarmClips.Length - 1;
      foreach (Missile missile in this.missiles)
        a = Mathf.Min(a, (int) (missile.seekerMode - (byte) 1));
      AudioClip alarmClip = this.alarmClips[a];
      if (!((UnityEngine.Object) alarmClip != (UnityEngine.Object) this.alarmSource.clip))
        return;
      this.alarmSource.clip = alarmClip;
      this.SyncTime();
      this.alarmSource.Play();
    }

    public void AddMissile(Missile missile)
    {
      this.missiles.Add(missile);
      this.ManageAlarmSound();
      this.alarmSource.Play();
      SoundManager.PlayInterfaceOneShot(this.alertClip);
    }

    public void ClearMissiles() => this.missiles.Clear();

    public void SyncTime()
    {
      if (!((UnityEngine.Object) this.alarmSource != (UnityEngine.Object) null))
        return;
      this.alarmSource.time = 0.0f;
    }

    public void RemoveMissile(Missile missile)
    {
      int count = this.missiles.Count;
      this.missiles.Remove(missile);
      if (this.missiles.Count == count || this.missiles.Count != 0)
        return;
      this.alarmSource.Stop();
    }

    public void Remove() => UnityEngine.Object.Destroy((UnityEngine.Object) this.alarmSource);
  }
}
