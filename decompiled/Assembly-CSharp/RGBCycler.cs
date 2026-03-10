// Decompiled with JetBrains decompiler
// Type: RGBCycler
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#nullable disable
public class RGBCycler : MonoBehaviour
{
  private List<Image> images = new List<Image>();
  private Text[] texts;
  [SerializeField]
  private float duration = 200f;
  [SerializeField]
  private float cycleSpeed = 1f;
  [SerializeField]
  private float flashSpeed = 2f;
  [SerializeField]
  private float cameraShakeAmount = 0.5f;
  private Color[] imageColors;
  private Color[] textColors;
  private Color currentColor;
  private float startTime;
  private float lastBeat;
  private float beatTimer;
  private Light light;
  private Aircraft aircraft;
  private bool started;

  private void Start()
  {
  }

  private void StartPlaying()
  {
    this.aircraft = SceneSingleton<CombatHUD>.i.aircraft;
    this.started = true;
    this.startTime = Time.timeSinceLevelLoad;
    this.StoreColors();
    this.light = this.aircraft.cockpit.gameObject.AddComponent<Light>();
    this.CycleColors();
  }

  private void StoreColors()
  {
    foreach (Image componentsInChild in this.gameObject.GetComponentsInChildren<Image>())
    {
      if ((double) componentsInChild.color.g > 0.25)
        this.images.Add(componentsInChild);
    }
    this.texts = this.gameObject.GetComponentsInChildren<Text>();
    this.imageColors = new Color[this.images.Count];
    this.textColors = new Color[this.texts.Length];
    for (int index = 0; index < this.images.Count; ++index)
      this.imageColors[index] = this.images[index].color;
    for (int index = 0; index < this.texts.Length; ++index)
      this.textColors[index] = this.texts[index].color;
  }

  private void Beat()
  {
    if (!this.started)
      return;
    this.beatTimer += Time.deltaTime;
    if ((double) this.beatTimer > 1.0 / (double) this.flashSpeed)
    {
      this.beatTimer -= 1f / this.flashSpeed;
      this.CycleColors();
    }
    if (!((Object) this.aircraft != (Object) null) || !this.aircraft.disabled && (double) Time.timeSinceLevelLoad - (double) this.startTime <= (double) this.duration)
      return;
    Object.Destroy((Object) this);
  }

  private void CycleColors()
  {
    this.currentColor = Color.HSVToRGB((float) (0.5 * ((double) Mathf.Sin(Time.timeSinceLevelLoad * this.cycleSpeed) + 1.0)), 1f, 1f);
    this.light.color = this.currentColor;
    this.light.intensity = (double) NetworkSceneSingleton<LevelInfo>.i.timeOfDay < 6.0 || (double) NetworkSceneSingleton<LevelInfo>.i.timeOfDay > 18.0 ? 0.1f : 1f;
    this.lastBeat = Time.timeSinceLevelLoad;
    SceneSingleton<CameraStateManager>.i.ShakeCamera(0.0f, 1f);
  }

  private void UpdateColors()
  {
    Color color = Color.white * Mathf.Max((float) (1.0 - ((double) Time.timeSinceLevelLoad - (double) this.lastBeat) * 4.0), 0.0f);
    for (int index = 0; index < this.images.Count; ++index)
      this.images[index].color = new Color(this.currentColor.r, this.currentColor.g, this.currentColor.b, this.imageColors[index].a) + color * 0.5f;
    for (int index = 0; index < this.texts.Length; ++index)
      this.texts[index].color = new Color(this.currentColor.r, this.currentColor.g, this.currentColor.b, this.textColors[index].a) + color * 0.5f;
  }

  private void RestoreColors()
  {
    for (int index = 0; index < this.images.Count; ++index)
      this.images[index].color = this.imageColors[index];
    for (int index = 0; index < this.texts.Length; ++index)
      this.texts[index].color = this.textColors[index];
  }

  private void OnDestroy()
  {
    if (!this.started)
      return;
    this.RestoreColors();
    this.light.intensity = 0.0f;
  }

  private void SetAircraft()
  {
    this.aircraft = SceneSingleton<CombatHUD>.i.aircraft;
    if (!((Object) this.aircraft != (Object) null) || !MusicManager.i.HasPlayedMusic(this.aircraft.definition.aircraftParameters.takeoffMusic))
      return;
    Object.Destroy((Object) this);
  }

  private void Update()
  {
    if (!this.started)
    {
      if ((Object) this.aircraft == (Object) null)
      {
        this.SetAircraft();
        return;
      }
      if ((double) this.aircraft.radarAlt <= 1.0)
        return;
      this.StartPlaying();
    }
    this.Beat();
    this.UpdateColors();
  }
}
