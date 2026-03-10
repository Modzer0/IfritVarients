// Decompiled with JetBrains decompiler
// Type: StatusDisplay
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#nullable disable
public class StatusDisplay : MonoBehaviour
{
  [SerializeField]
  private List<PartStatusDisplay> statusDisplays = new List<PartStatusDisplay>();
  [SerializeField]
  private Image aircraftBackground;
  [SerializeField]
  private List<GameObject> failureIndicators = new List<GameObject>();
  private Dictionary<string, GameObject> failureIndicatorsLookup = new Dictionary<string, GameObject>();
  private AudioSource damageAlertSource;
  private List<IReportDamage> damageReporters = new List<IReportDamage>();
  private bool initialized;
  private float displayTimer = 10f;
  private List<AudioClip> messageQueue = new List<AudioClip>();
  private Aircraft aircraft;

  public void Initialize(Aircraft aircraft)
  {
    for (int index = 0; index < this.statusDisplays.Count; ++index)
      this.statusDisplays[index].DamageSubscribe(aircraft, this);
    for (int index = 0; index < this.failureIndicators.Count; ++index)
    {
      this.failureIndicatorsLookup.Add(this.failureIndicators[index].name, this.failureIndicators[index]);
      this.failureIndicators[index].SetActive(false);
    }
    foreach (Component component1 in aircraft.partLookup)
    {
      IReportDamage component2;
      if (component1.TryGetComponent<IReportDamage>(out component2))
      {
        component2.onReportDamage += new Action<OnReportDamage>(this.StatusDisplay_OnReportDamage);
        this.damageReporters.Add(component2);
      }
    }
    this.aircraft = aircraft;
    aircraft.onDisableUnit += new Action<Unit>(this.StatusDisplay_OnDisable);
    this.damageAlertSource = aircraft.CockpitRB().gameObject.AddComponent<AudioSource>();
    this.damageAlertSource.outputAudioMixerGroup = SoundManager.i.InterfaceMixer;
    this.damageAlertSource.bypassEffects = true;
    this.damageAlertSource.bypassListenerEffects = true;
    this.damageAlertSource.spatialize = false;
    this.damageAlertSource.volume = 1f;
    this.initialized = true;
    this.aircraftBackground.color = new Color(1f, 1f, 1f, 1f);
    this.transform.SetParent(SceneSingleton<FlightHud>.i.statusAnchor);
    this.transform.localPosition = Vector3.zero;
  }

  private void StatusDisplay_OnReportDamage(OnReportDamage e)
  {
    if ((UnityEngine.Object) e.audioReport != (UnityEngine.Object) null)
      this.AddMessage(e.audioReport);
    if (this.failureIndicatorsLookup.ContainsKey(e.failureMessage))
      this.failureIndicatorsLookup[e.failureMessage].SetActive(true);
    SceneSingleton<AircraftActionsReport>.i.ReportText($"<color=red>{e.failureMessage}</color>", 5f);
    this.aircraftBackground.color = Color.white;
  }

  private void StatusDisplay_OnDisable(Unit unit) => this.damageAlertSource.Stop();

  public void AddMessage(AudioClip message) => this.messageQueue.Add(message);

  public void DisplayDamage()
  {
    this.aircraftBackground.color = Color.white;
    this.displayTimer = float.MaxValue;
    this.enabled = true;
  }

  private void OnDestroy()
  {
    if (!this.initialized)
      return;
    for (int index = 0; index < this.statusDisplays.Count; ++index)
      this.statusDisplays[index].DamageUnsubscribe();
    foreach (IReportDamage damageReporter in this.damageReporters)
    {
      if ((bool) (damageReporter as UnityEngine.Object))
        damageReporter.onReportDamage -= new Action<OnReportDamage>(this.StatusDisplay_OnReportDamage);
    }
  }

  private void Update()
  {
    this.displayTimer -= Time.deltaTime;
    this.displayTimer = Mathf.Max(this.displayTimer, 0.0f);
    this.aircraftBackground.color = new Color(1f, 1f, 1f, this.displayTimer * 0.1f);
    if ((double) this.displayTimer < 10.0)
    {
      for (int index = 0; index < this.statusDisplays.Count; ++index)
      {
        Color color = this.statusDisplays[index].partImage.color with
        {
          a = (float) ((1.0 - (double) this.statusDisplays[index].displayCondition) * (double) this.displayTimer * 0.10000000149011612)
        };
        this.statusDisplays[index].partImage.color = color;
      }
    }
    if ((double) this.displayTimer <= 0.0)
      this.enabled = false;
    if (this.messageQueue.Count <= 0 || this.aircraft.disabled || this.damageAlertSource.isPlaying)
      return;
    this.damageAlertSource.clip = this.messageQueue[0];
    this.damageAlertSource.Play();
    this.messageQueue.RemoveAt(0);
  }
}
