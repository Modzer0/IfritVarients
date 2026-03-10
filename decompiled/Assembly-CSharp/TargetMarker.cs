// Decompiled with JetBrains decompiler
// Type: TargetMarker
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;
using UnityEngine.UI;

#nullable disable
public class TargetMarker : UnitMapMarker
{
  [SerializeField]
  private Text infoPlayer;
  [SerializeField]
  private Text infoName;
  [SerializeField]
  private Text infoRange;
  [SerializeField]
  private Text infoSpeed;
  [SerializeField]
  private Text infoAlt;
  [SerializeField]
  private Text infoHeading;

  protected override void ExtraSetup()
  {
    Unit unit = this.GetUnit();
    this.markerImg.sprite = !((UnityEngine.Object) unit.NetworkHQ != (UnityEngine.Object) null) || !((UnityEngine.Object) unit.NetworkHQ == (UnityEngine.Object) SceneSingleton<DynamicMap>.i.HQ) || !((UnityEngine.Object) SceneSingleton<DynamicMap>.i.HQ != (UnityEngine.Object) null) ? GameAssets.i.targetUnitSprite : GameAssets.i.targetUnitSpriteFriendly;
    this.infoPlayer.enabled = false;
    if (unit is Aircraft aircraft && (UnityEngine.Object) aircraft.pilots[0].player != (UnityEngine.Object) null)
    {
      this.infoPlayer.enabled = true;
      this.infoPlayer.text = aircraft.pilots[0].player.PlayerName;
    }
    this.infoName.text = unit.definition.code;
    this.infoSpeed.text = "SPD " + UnitConverter.SpeedReading(unit.speed);
    this.infoAlt.text = "ALT " + UnitConverter.AltitudeReading(unit.radarAlt);
    this.infoHeading.text = $"HDG {unit.transform.eulerAngles.y}°";
    this.infoRange.text = "RNG -";
    this.transform.localPosition = this.Icon.transform.localPosition;
    this.transform.eulerAngles = Vector3.zero;
    this.transform.localScale = Vector3.one * (1f / SceneSingleton<DynamicMap>.i.mapImage.transform.localScale.x) * 1f;
    DynamicMap.onShowTypesChanged += new Action(this.Marker_Refresh);
  }

  private void Update()
  {
    this.transform.localScale = Vector3.one * ((DynamicMap.mapMaximized ? 1f : 0.5f) / SceneSingleton<DynamicMap>.i.mapImage.transform.localScale.x);
    this.transform.eulerAngles = Vector3.zero;
    this.transform.localPosition = this.Icon.transform.localPosition;
    if ((double) Time.timeSinceLevelLoad <= (double) this.lastRefresh + (double) this.refreshDelay)
      return;
    Unit unit = this.GetUnit();
    if ((UnityEngine.Object) unit != (UnityEngine.Object) null)
    {
      if ((UnityEngine.Object) unit.NetworkHQ == (UnityEngine.Object) null || (UnityEngine.Object) unit.NetworkHQ == (UnityEngine.Object) SceneSingleton<DynamicMap>.i.HQ || (UnityEngine.Object) SceneSingleton<DynamicMap>.i.HQ == (UnityEngine.Object) null || SceneSingleton<DynamicMap>.i.HQ.IsTargetPositionAccurate(unit, 20f))
      {
        this.infoSpeed.text = "SPD " + UnitConverter.SpeedReading(unit.speed);
        switch (unit)
        {
          case Aircraft _:
            this.infoAlt.text = "ALT " + UnitConverter.AltitudeReading(unit.radarAlt);
            break;
          case Missile _:
            this.infoAlt.text = "ALT " + UnitConverter.AltitudeReading(unit.GlobalPosition().y);
            break;
          default:
            this.infoAlt.text = "ALT -";
            break;
        }
        this.infoHeading.text = $"HDG {Mathf.RoundToInt(unit.transform.eulerAngles.y)}°";
        this.infoRange.text = !((UnityEngine.Object) SceneSingleton<CombatHUD>.i.aircraft != (UnityEngine.Object) null) ? "" : "RNG " + UnitConverter.DistanceReading(FastMath.Distance(SceneSingleton<CombatHUD>.i.aircraft.GlobalPosition(), unit.GlobalPosition()));
      }
      else
      {
        this.infoSpeed.text = "SPD -";
        this.infoAlt.text = "ALT -";
        this.infoHeading.text = "HDG -";
        this.infoRange.text = "RNG -";
      }
    }
    else
      this.Remove();
    this.DynamicHide();
    this.lastRefresh = Time.timeSinceLevelLoad;
  }

  public override void DynamicHide()
  {
    if (!SceneSingleton<MapOptions>.i.showTargetInfo || !DynamicMap.mapMaximized)
    {
      this.Show(false);
    }
    else
    {
      if (DynamicMap.mapMaximized && !this.infoName.enabled)
        this.Show(true);
      if (SceneSingleton<DynamicMap>.i.mapMarkers.Count <= 1)
        return;
      int index1 = SceneSingleton<DynamicMap>.i.mapMarkers.FindIndex((Predicate<MapMarker>) (x => (UnityEngine.Object) x == (UnityEngine.Object) this));
      bool flag = false;
      for (int index2 = 0; index2 < SceneSingleton<DynamicMap>.i.mapMarkers.Count; ++index2)
      {
        MapMarker mapMarker = SceneSingleton<DynamicMap>.i.mapMarkers[index2];
        if ((UnityEngine.Object) mapMarker != (UnityEngine.Object) null && (UnityEngine.Object) mapMarker != (UnityEngine.Object) this && mapMarker is TargetMarker && (double) Vector3.Distance(this.transform.position, mapMarker.transform.position) < 80.0 && index1 < index2)
          flag = true;
      }
      if (flag)
        this.Mask();
      else
        this.Show(true);
    }
  }

  public override void Mask()
  {
    if (!this.infoRange.enabled)
      return;
    Color grey = Color.grey with { a = 0.5f };
    this.markerImg.color = grey;
    this.infoPlayer.color = grey;
    this.infoName.color = grey;
    this.infoSpeed.color = grey;
    this.infoAlt.color = grey;
    this.infoHeading.color = grey;
    this.infoRange.enabled = false;
  }

  public override void Show(bool value)
  {
    if (value)
    {
      Color white = Color.white with { a = 0.75f };
      this.markerImg.enabled = true;
      this.markerImg.color = white;
      this.infoPlayer.color = white;
      this.infoName.enabled = true;
      this.infoName.color = white;
      this.infoSpeed.enabled = true;
      this.infoSpeed.color = white;
      this.infoAlt.enabled = true;
      this.infoAlt.color = white;
      this.infoHeading.enabled = true;
      this.infoHeading.color = white;
      this.infoRange.enabled = true;
      this.infoRange.color = white;
    }
    else
    {
      Color white = Color.white with { a = 0.75f };
      this.markerImg.enabled = true;
      this.markerImg.color = white;
      this.infoName.enabled = false;
      this.infoSpeed.enabled = false;
      this.infoAlt.enabled = false;
      this.infoHeading.enabled = false;
      this.infoRange.enabled = false;
    }
  }

  public void Marker_Refresh() => this.Show(SceneSingleton<MapOptions>.i.showTargetInfo);

  private void OnDestroy() => DynamicMap.onShowTypesChanged -= new Action(this.Marker_Refresh);
}
