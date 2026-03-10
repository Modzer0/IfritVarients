// Decompiled with JetBrains decompiler
// Type: UnitMapIcon
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.MissionEditorScripts;
using System;
using UnityEngine;
using UnityEngine.UI;

#nullable disable
public class UnitMapIcon : MapIcon
{
  private bool orient;
  private bool flashing;
  private bool scale;
  private Vector3 iconProportions = Vector3.one;
  private Vector3 unitTrueSize;
  private float unitMaxDimension;
  private TrackingInfo trackingInfo;
  private float unitSizeFactor = 1f;
  private TargetMarker targetMarker;
  private JammedMarker jammedMarker;

  public Unit unit { get; private set; }

  public Factory Factory { get; private set; }

  protected override FactionHQ GetHQ() => this.unit.NetworkHQ;

  protected override bool IsLocalPlayerAircraft()
  {
    return (UnityEngine.Object) SceneSingleton<CombatHUD>.i.aircraft == (UnityEngine.Object) this.unit;
  }

  public void SetIcon(Unit unit)
  {
    this.transform.localScale = Vector3.one * SceneSingleton<MapOptions>.i.iconSize;
    this.transform.eulerAngles = Vector3.zero;
    if ((UnityEngine.Object) SceneSingleton<DynamicMap>.i.HQ != (UnityEngine.Object) null)
      this.trackingInfo = SceneSingleton<DynamicMap>.i.HQ.GetTrackingData(unit.persistentID);
    this.gameObject.name = $"{unit.unitName}[{unit.persistentID}]";
    this.unit = unit;
    this.Factory = unit is Building ? unit.GetComponent<Factory>() : (Factory) null;
    this.iconImage.sprite = unit.definition.mapIcon;
    this.orient = unit.definition.mapOrient;
    unit.onChangeFaction += new Action<Unit>(this.UnitMapIcon_OnFactionChanged);
    unit.onDisableUnit += new Action<Unit>(this.UnitMapIcon_OnUnitDisabled);
    unit.onJam += new Action<Unit.JamEventArgs>(this.UnitMapIcon_OnUnitJammed);
    float num1 = 1f / SceneSingleton<DynamicMap>.i.mapImage.transform.localScale.x;
    if (unit is Building)
    {
      this.scale = (double) unit.maxRadius > 10.0;
      this.unitTrueSize = new Vector3(unit.definition.width, unit.definition.length, 1f) * SceneSingleton<DynamicMap>.i.MetersToPixels();
      this.unitMaxDimension = Mathf.Max(this.unitTrueSize.x, this.unitTrueSize.y);
      this.iconProportions = this.unitTrueSize.normalized;
      float num2 = num1 * 10f;
      if ((double) this.unitMaxDimension < (double) num2)
        this.iconImage.transform.localScale = this.iconProportions * num2;
      else
        this.iconImage.transform.localScale = this.unitTrueSize;
    }
    else
      this.iconImage.transform.localScale = num1 * 15f * unit.definition.mapIconSize * this.unitSizeFactor * Vector3.one;
    this.UnitMapIcon_UpdateColor();
    if (!(unit is PilotDismounted) || SceneSingleton<MapOptions>.i.showPilotIcons || !this.gameObject.activeSelf)
      return;
    this.gameObject.SetActive(false);
  }

  public void UnitMapIcon_UpdateColor()
  {
    this.UpdateColor();
    if ((UnityEngine.Object) this.unit == (UnityEngine.Object) null)
      return;
    int num = this.IsLocalPlayerAircraft() ? 1 : 0;
    if (num == 0 && SceneSingleton<TargetListSelector>.i.CheckExclusions(this.unit))
    {
      Image iconImage = this.iconImage;
      iconImage.color = iconImage.color * 0.67f;
    }
    if (num == 0 || !this.iconImage.raycastTarget)
      return;
    this.HighlightIcon();
  }

  private void UnitMapIcon_OnUnitDisabled(Unit unit)
  {
    SceneSingleton<DynamicMap>.i.RemoveIcon((MapIcon) this);
    this.RemoveIcon();
  }

  private void UnitMapIcon_OnFactionChanged(Unit unit) => this.UnitMapIcon_UpdateColor();

  protected override Color GetColor()
  {
    FactionHQ hq = this.GetHQ();
    switch (DynamicMap.GetFactionMode(hq, true))
    {
      case FactionMode.Spectator:
        return !this.isSelected ? hq.faction.color : hq.faction.selectedColor;
      case FactionMode.Friendly:
        return !this.isSelected ? GameAssets.i.HUDFriendly : GameAssets.i.HUDFriendlySelected;
      case FactionMode.Enemy:
        return !this.isSelected ? GameAssets.i.HUDHostile : GameAssets.i.HUDHostileSelected;
      default:
        return !this.isSelected ? GameAssets.i.HUDNeutral : GameAssets.i.HUDNeutralSelected;
    }
  }

  protected override void OnSelectIcon()
  {
    this.targetMarker = UnityEngine.Object.Instantiate<GameObject>(SceneSingleton<DynamicMap>.i.targetMarker, SceneSingleton<DynamicMap>.i.infoLayer.transform).GetComponent<TargetMarker>();
    this.targetMarker.Setup(this);
    SceneSingleton<DynamicMap>.i.mapMarkers.Add((MapMarker) this.targetMarker);
  }

  protected override void OnDeselectIcon()
  {
    if ((UnityEngine.Object) this.targetMarker != (UnityEngine.Object) null)
      this.targetMarker.Remove();
    this.targetMarker = (TargetMarker) null;
  }

  protected override void OnRemoveIcon()
  {
    if ((UnityEngine.Object) this.unit != (UnityEngine.Object) null)
    {
      this.unit.onChangeFaction -= new Action<Unit>(this.UnitMapIcon_OnFactionChanged);
      this.unit.onDisableUnit -= new Action<Unit>(this.UnitMapIcon_OnUnitDisabled);
      this.unit.onJam -= new Action<Unit.JamEventArgs>(this.UnitMapIcon_OnUnitJammed);
    }
    if ((UnityEngine.Object) this.targetMarker != (UnityEngine.Object) null)
      this.targetMarker.Remove();
    if (!((UnityEngine.Object) this != (UnityEngine.Object) null))
      return;
    UnityEngine.Object.Destroy((UnityEngine.Object) this.gameObject);
  }

  public override void ClickIcon(MapIcon.ClickSource clickSource)
  {
    if ((UnityEngine.Object) this.unit == (UnityEngine.Object) SceneSingleton<CombatHUD>.i.aircraft)
      return;
    bool shiftPressed = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
    if (this.isSelected & shiftPressed)
      Deselect();
    else
      Select(shiftPressed);

    void Deselect()
    {
      if ((UnityEngine.Object) SceneSingleton<CombatHUD>.i.aircraft == (UnityEngine.Object) null || SceneSingleton<CombatHUD>.i.aircraft.disabled)
        SceneSingleton<CombatHUD>.i.DeSelectUnit(this.unit);
      else
        SceneSingleton<DynamicMap>.i.DeselectIcon(this.unit);
    }

    void Select(bool shiftPressed)
    {
      bool flag1 = GameManager.gameState == GameState.Editor;
      bool flag2 = DynamicMap.GetFactionMode() == FactionMode.Spectator;
      int num = !((UnityEngine.Object) SceneSingleton<CombatHUD>.i.aircraft != (UnityEngine.Object) null) ? 0 : (!SceneSingleton<CombatHUD>.i.aircraft.disabled ? 1 : 0);
      if (num == 0 && !shiftPressed | flag1 | flag2)
        SceneSingleton<DynamicMap>.i.UnselectAll();
      if (num != 0)
      {
        if (SceneSingleton<DynamicMap>.i.selectedIcons.Contains((MapIcon) this) || SceneSingleton<TargetListSelector>.i.CheckExclusions(this.unit))
          return;
        SceneSingleton<CombatHUD>.i.SelectUnit(this.unit);
      }
      else
      {
        SceneSingleton<DynamicMap>.i.SelectIcon(this.unit);
        FactionHQ localHq;
        if (!flag1 && GameManager.GetLocalHQ(out localHq) && (UnityEngine.Object) this.unit.NetworkHQ != (UnityEngine.Object) null && (UnityEngine.Object) localHq != (UnityEngine.Object) this.unit.NetworkHQ && !localHq.IsTargetPositionAccurate(this.unit, 100f))
          return;
        SceneSingleton<CameraStateManager>.i.transform.position = this.unit.transform.position + Vector3.up * this.unit.definition.height * 0.8f - SceneSingleton<CameraStateManager>.i.transform.forward * this.unit.definition.length * 2f;
        SceneSingleton<CameraStateManager>.i.transform.LookAt(this.unit.transform);
        if (flag1)
        {
          if (shiftPressed)
          {
            SceneSingleton<UnitSelection>.i.ToggleInMultiSelection((IEditorSelectable) this.unit);
          }
          else
          {
            SceneSingleton<UnitSelection>.i.SetSelection((IEditorSelectable) this.unit);
            SceneSingleton<DynamicMap>.i.Minimize();
          }
        }
        else
          SceneSingleton<CameraStateManager>.i.SetFollowingUnit(this.unit);
      }
    }
  }

  public override void UpdateIcon(
    float mapDisplayFactor,
    float mapInverseScale,
    Transform mapTransform,
    bool mapMaximized)
  {
    if (this.unit is PilotDismounted)
    {
      if (!SceneSingleton<MapOptions>.i.showPilotIcons && this.gameObject.activeSelf)
      {
        this.gameObject.SetActive(false);
        return;
      }
      if (SceneSingleton<MapOptions>.i.showPilotIcons && !this.gameObject.activeSelf)
        this.gameObject.SetActive(true);
    }
    this.globalPosition = (!GameManager.GetLocalFaction(out Faction _) || this.trackingInfo == null ? this.unit.GlobalPosition() : this.trackingInfo.GetPosition()).AsVector3() * mapDisplayFactor;
    this.iconImage.transform.localPosition = new Vector3(this.globalPosition.x, this.globalPosition.z, 0.0f);
    if (this.scale)
    {
      float num = mapInverseScale * 10f;
      if ((double) this.unitMaxDimension < (double) num)
        this.iconImage.transform.localScale = this.iconProportions * num;
      else
        this.iconImage.transform.localScale = this.unitTrueSize;
    }
    else
      this.iconImage.transform.localScale = mapInverseScale * 15f * this.unit.definition.mapIconSize * this.unitSizeFactor * Vector3.one * SceneSingleton<MapOptions>.i.iconSize;
    if (this.orient)
    {
      if (this.trackingInfo == null || this.trackingInfo.Observed())
        this.iconImage.transform.eulerAngles = new Vector3(0.0f, 0.0f, mapTransform.eulerAngles.z - this.unit.transform.eulerAngles.y);
    }
    else
      this.iconImage.transform.eulerAngles = Vector3.zero;
    if (!this.flashing)
      return;
    this.iconImage.color = new Color(1f, (float) ((double) Mathf.Sin(Time.realtimeSinceStartup * 20f) * 0.5 + 0.5), 0.0f, 1f);
    this.iconImage.transform.localScale *= 1.2f;
  }

  public override string GetInfoText()
  {
    string infoText = this.unit.unitName;
    if (this.unit is Aircraft)
    {
      Aircraft unit = this.unit as Aircraft;
      if ((UnityEngine.Object) unit.pilots[0] != (UnityEngine.Object) null && (UnityEngine.Object) unit.Player != (UnityEngine.Object) null)
        infoText = $"{infoText}\n{unit.Player.PlayerName}";
    }
    return infoText;
  }

  public void SetMissileWarning()
  {
    this.iconImage.sprite = GameAssets.i.missileWarningSprite;
    this.iconImage.color = Color.yellow;
    this.flashing = true;
  }

  public void ClearMissileWarning()
  {
    if ((UnityEngine.Object) this.iconImage == (UnityEngine.Object) null)
      return;
    this.iconImage.sprite = this.unit.definition.mapIcon;
    this.UnitMapIcon_UpdateColor();
    this.flashing = false;
  }

  public void JammingDistortion(float jammingStrength)
  {
    jammingStrength = Mathf.Min(jammingStrength, 4f);
    this.iconImage.transform.position += UnityEngine.Random.insideUnitSphere * jammingStrength * 5f / 2f;
    this.iconImage.color = new Color(this.iconImage.color.r, this.iconImage.color.g, this.iconImage.color.b, (float) (1.0 - (double) jammingStrength * 0.699999988079071));
  }

  public void ClearJammingDistortion()
  {
    this.iconImage.color = new Color(this.iconImage.color.r, this.iconImage.color.g, this.iconImage.color.b, 1f);
  }

  private void UnitMapIcon_OnUnitJammed(Unit.JamEventArgs jam)
  {
    if (!SceneSingleton<MapOptions>.i.showJamming || !(this.unit.radar is Radar radar) || !((UnityEngine.Object) this.jammedMarker == (UnityEngine.Object) null))
      return;
    this.jammedMarker = UnityEngine.Object.Instantiate<GameObject>(SceneSingleton<DynamicMap>.i.jammedMarker, SceneSingleton<DynamicMap>.i.infoLayer.transform).GetComponent<JammedMarker>();
    this.jammedMarker.Setup(this, jam.jammingUnit, radar);
  }
}
