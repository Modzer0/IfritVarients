// Decompiled with JetBrains decompiler
// Type: RadarWarning
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#nullable disable
public class RadarWarning : MonoBehaviour
{
  private Aircraft aircraft;
  [SerializeField]
  private AudioClip radarWarningExisting;
  [SerializeField]
  private AudioClip radarWarningNew;
  [SerializeField]
  private GameObject radarWarningIconPrefab;
  [SerializeField]
  private GameObject jammingIconPrefab;
  [SerializeField]
  private Color radarWarningIconColor;
  private readonly List<RadarWarning.RadarWarningIcon> radarWarningIcons = new List<RadarWarning.RadarWarningIcon>();
  private readonly List<RadarWarning.JammingIcon> jammingIcons = new List<RadarWarning.JammingIcon>();
  private readonly Dictionary<Unit, RadarWarning.JammingIcon> jammingIconLookup = new Dictionary<Unit, RadarWarning.JammingIcon>();

  private void Start()
  {
    this.aircraft = SceneSingleton<CombatHUD>.i.aircraft;
    this.aircraft.onDisableUnit += new Action<Unit>(this.RadarWarning_OnUnitDisable);
    this.aircraft.onRadarWarning += new Action<Aircraft.OnRadarWarning>(this.RadarWarning_OnRadarWarning);
    this.aircraft.onJam += new Action<Unit.JamEventArgs>(this.RadarWarning_OnJammed);
  }

  private void OnDestroy()
  {
    this.aircraft.onDisableUnit -= new Action<Unit>(this.RadarWarning_OnUnitDisable);
    this.aircraft.onRadarWarning -= new Action<Aircraft.OnRadarWarning>(this.RadarWarning_OnRadarWarning);
    foreach (RadarWarning.RadarWarningIcon radarWarningIcon in this.radarWarningIcons)
      radarWarningIcon.Remove();
    foreach (RadarWarning.JammingIcon jammingIcon in this.jammingIcons)
      jammingIcon.Remove();
  }

  private void RadarWarning_OnUnitDisable(Unit unit) => UnityEngine.Object.Destroy((UnityEngine.Object) this.gameObject);

  private void ShowDirectionalWarning(Vector3 direction)
  {
    this.radarWarningIcons.Add(new RadarWarning.RadarWarningIcon(direction, this.radarWarningIconPrefab, SceneSingleton<CombatHUD>.i.iconLayer, this.radarWarningIconColor, 4f));
    this.enabled = true;
  }

  private void RadarWarning_OnRadarWarning(Aircraft.OnRadarWarning radarSource)
  {
    SoundManager.PlayRadarWarningOneShot(this.aircraft.KnownRadarWarning(radarSource.emitter) ? this.radarWarningExisting : this.radarWarningNew);
    SceneSingleton<DynamicMap>.i.ShowRadarPing(radarSource);
    if (!radarSource.detected)
      return;
    if (SceneSingleton<CombatHUD>.i.MarkerExists(radarSource.emitter))
      SceneSingleton<CombatHUD>.i.HighlightMarker(radarSource.emitter);
    else
      this.ShowDirectionalWarning(radarSource.emitter.transform.position - this.aircraft.transform.position);
  }

  private void RadarWarning_OnJammed(Unit.JamEventArgs e)
  {
    this.enabled = true;
    if (!this.jammingIconLookup.ContainsKey(e.jammingUnit))
    {
      RadarWarning.JammingIcon jammingIcon = new RadarWarning.JammingIcon(e.jammingUnit, this.jammingIconPrefab, SceneSingleton<CombatHUD>.i.iconLayer, this.radarWarningIconColor);
      this.jammingIcons.Add(jammingIcon);
      this.jammingIconLookup.Add(e.jammingUnit, jammingIcon);
    }
    else
      this.jammingIconLookup[e.jammingUnit].Refresh();
  }

  private void Update()
  {
    if (this.radarWarningIcons.Count == 0 && this.jammingIcons.Count == 0)
    {
      this.enabled = false;
    }
    else
    {
      for (int index = this.radarWarningIcons.Count - 1; index >= 0; --index)
      {
        if (!this.radarWarningIcons[index].Position())
          this.radarWarningIcons.RemoveAt(index);
      }
      for (int index = this.jammingIcons.Count - 1; index >= 0; --index)
      {
        if (!this.jammingIcons[index].Position())
        {
          this.jammingIconLookup.Remove(this.jammingIcons[index].unit);
          this.jammingIcons.RemoveAt(index);
        }
      }
    }
  }

  private class RadarWarningIcon
  {
    private readonly Image image;
    private readonly Vector3 direction;
    private readonly float creationTime;
    private readonly Color color;
    private readonly float life;

    public RadarWarningIcon(
      Vector3 direction,
      GameObject iconPrefab,
      Transform iconLayer,
      Color color,
      float life)
    {
      this.direction = direction.normalized;
      this.color = color;
      this.image = NetworkSceneSingleton<Spawner>.i.SpawnLocal(iconPrefab, iconLayer).GetComponent<Image>();
      this.creationTime = Time.timeSinceLevelLoad;
      this.life = life;
    }

    public bool Position()
    {
      Transform transform = SceneSingleton<CameraStateManager>.i.transform;
      this.image.enabled = (double) Vector3.Dot(this.direction, transform.forward) > 0.0;
      this.image.transform.position = Vector3.Scale(SceneSingleton<CameraStateManager>.i.mainCamera.WorldToScreenPoint(transform.position + this.direction * 10000f), new Vector3(1f, 1f, 0.0f));
      float num = Time.timeSinceLevelLoad - this.creationTime;
      this.image.color = this.color * (float) (1.0 - (double) num / (double) this.life);
      if ((double) num < 4.0)
        return true;
      NetworkSceneSingleton<Spawner>.i.DestroyLocal(this.image.gameObject, 0.0f);
      return false;
    }

    public void Remove()
    {
      NetworkSceneSingleton<Spawner>.i.DestroyLocal(this.image.gameObject, 0.0f);
    }
  }

  private class JammingIcon
  {
    private readonly Image image;
    public readonly Unit unit;
    private float lastJam;
    private readonly Text text;

    public JammingIcon(Unit unit, GameObject iconPrefab, Transform iconLayer, Color color)
    {
      this.unit = unit;
      this.image = NetworkSceneSingleton<Spawner>.i.SpawnLocal(iconPrefab, iconLayer).GetComponent<Image>();
      this.text = this.image.gameObject.GetComponentInChildren<Text>();
      this.image.color = color;
      this.lastJam = Time.timeSinceLevelLoad;
    }

    public void Refresh() => this.lastJam = Time.timeSinceLevelLoad;

    public bool Position()
    {
      Transform transform = SceneSingleton<CameraStateManager>.i.transform;
      this.image.enabled = (double) Vector3.Dot(this.unit.transform.position - transform.position, transform.forward) > 0.0;
      this.text.enabled = this.image.enabled;
      this.image.transform.position = Vector3.Scale(SceneSingleton<CameraStateManager>.i.mainCamera.WorldToScreenPoint(this.unit.transform.position), new Vector3(1f, 1f, 0.0f));
      if ((double) Time.timeSinceLevelLoad - (double) this.lastJam <= 1.0)
        return true;
      NetworkSceneSingleton<Spawner>.i.DestroyLocal(this.image.gameObject, 0.0f);
      return false;
    }

    public void Remove()
    {
      NetworkSceneSingleton<Spawner>.i.DestroyLocal(this.image.gameObject, 0.0f);
    }
  }
}
