// Decompiled with JetBrains decompiler
// Type: HUDUnitMarker
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;
using UnityEngine.UI;

#nullable disable
public class HUDUnitMarker
{
  public Image image;
  private Transform _transform;
  private Sprite icon;
  public Unit unit;
  private TrackingInfo trackingInfo;
  public bool selected;
  private float threat;
  private float timeCreated;
  private bool maximized;
  public bool alwaysMaximized;
  private bool flashing;
  private bool hidden;
  public bool outdated;
  private Color color;
  private float opacity = 1f;
  private float customOpacity;
  private float scale = 1f;
  private float customScale;
  private float distanceScale = 1f;

  public HUDUnitMarker(Unit unit, Image image)
  {
    this.unit = unit;
    this._transform = image.transform;
    this.image = image;
    this.image.gameObject.name = $"{unit.unitName}[{unit.persistentID}]";
    this.image.enabled = false;
    unit.onDisableUnit += new Action<Unit>(this.HUDIcon_OnDisableUnit);
    unit.onChangeFaction += new Action<Unit>(this.HUDIcon_OnChangeFaction);
    DynamicMap.onShowTypesChanged += new Action(this.HUDUnitMarker_OnMapShowTypesChanged);
    SceneSingleton<HUDOptions>.i.OnApplyOptions += new Action(this.HUDUnitMarker_OnApplyOptions);
    PlayerSettings.OnApplyOptions += new Action(this.HUDUnitMarker_OnApplyOptions);
    SceneSingleton<CombatHUD>.i.aircraft.onSetGear += new Action<Aircraft.OnSetGear>(this.HUDUnitMarker_OnSetGear);
    this.alwaysMaximized = unit is Aircraft;
    this.timeCreated = Time.timeSinceLevelLoad;
    this.trackingInfo = SceneSingleton<DynamicMap>.i.HQ.GetTrackingData(unit.persistentID);
    if (SceneSingleton<CombatHUD>.i.landingMode)
      return;
    this.scale = unit.definition.iconSize;
    this.UpdateHidden(SceneSingleton<CombatHUD>.i.aircraft.gearDeployed);
    this.CustomizeIcon();
    this.UpdatePosition(SceneSingleton<DynamicMap>.i.HQ, SceneSingleton<CameraStateManager>.i.transform.GlobalPosition(), SceneSingleton<CameraStateManager>.i.transform.forward);
    this.SetNew();
    this.UpdateColor();
  }

  private void HUDUnitMarker_OnApplyOptions() => this.CustomizeIcon();

  private void HUDUnitMarker_OnSetGear(Aircraft.OnSetGear g)
  {
    this.UpdateHidden(g.gearState == LandingGear.GearState.Extending || g.gearState == LandingGear.GearState.LockedExtended);
  }

  private void CustomizeIcon()
  {
    this.customOpacity = 1f;
    this.customScale = 1f;
    for (int index = 0; index < SceneSingleton<HUDOptions>.i.listOptionItems.Count; ++index)
    {
      if (index < 2 && SceneSingleton<HUDOptions>.i.listOptionItems[index].CheckFaction(this.unit.NetworkHQ))
      {
        this.customOpacity *= SceneSingleton<HUDOptions>.i.listOptionItems[index].transparency;
        this.customScale *= SceneSingleton<HUDOptions>.i.listOptionItems[index].size;
      }
      if (index >= 2 && SceneSingleton<HUDOptions>.i.listOptionItems[index].CheckType(this.unit.definition))
      {
        this.customOpacity *= SceneSingleton<HUDOptions>.i.listOptionItems[index].transparency;
        this.customScale *= SceneSingleton<HUDOptions>.i.listOptionItems[index].size;
      }
    }
    if (this.unit is GroundVehicle unit2)
    {
      for (int index = 0; index < SceneSingleton<HUDOptions>.i.listVehicleTypes.Count; ++index)
      {
        if (SceneSingleton<HUDOptions>.i.listVehicleTypes[index].CheckDefinition(unit2.definition) && !SceneSingleton<HUDOptions>.i.listVehicleTypes[index].status)
          this.customScale *= 0.5f;
      }
    }
    else if (this.unit is Building unit1)
    {
      for (int index = 0; index < SceneSingleton<HUDOptions>.i.listBuildingTypes.Count; ++index)
      {
        if (!SceneSingleton<HUDOptions>.i.listBuildingTypes[index].CheckDefinition(unit1.definition) && !SceneSingleton<HUDOptions>.i.listBuildingTypes[index].status)
          this.customScale *= 0.5f;
      }
    }
    this.customScale *= PlayerSettings.hmdIconSize;
    if (this.selected)
      return;
    this._transform.localScale = this.maximized ? this.scale * this.customScale * this.distanceScale * Vector3.one : 4f * Vector3.one;
  }

  public void AssessThreat(Unit assessor)
  {
    if ((UnityEngine.Object) assessor == (UnityEngine.Object) null)
      return;
    this.threat = assessor.definition.ThreatPosedBy(this.unit.definition.roleIdentity);
  }

  public float AssessPriority(
    Aircraft aircraft,
    GameObject targetDesignator,
    WeaponStation weaponStation)
  {
    float num1 = this.trackingInfo != null ? FastMath.Distance(aircraft.GlobalPosition(), this.trackingInfo.GetPosition()) : Vector3.Distance(aircraft.transform.position, this.unit.transform.position);
    float num2 = FastMath.SquareDistance(targetDesignator.transform.position, this._transform.position);
    if ((UnityEngine.Object) this.unit.NetworkHQ == (UnityEngine.Object) aircraft.NetworkHQ || (UnityEngine.Object) this.unit.NetworkHQ == (UnityEngine.Object) null || weaponStation == null)
      return (float) (0.10000000149011612 / ((double) num1 * (double) num2));
    OpportunityThreat opportunityThreat = weaponStation.CalcOpportunityThreat(this.unit.definition, (Unit) aircraft);
    return Mathf.Max(opportunityThreat.opportunity + opportunityThreat.threat, 0.2f) / (num1 * num2);
  }

  public void SetNew()
  {
    this.timeCreated = Time.timeSinceLevelLoad;
    switch (DynamicMap.GetFactionMode(this.unit.NetworkHQ))
    {
      case FactionMode.NoFaction:
        this.icon = this.unit.definition.friendlyIcon;
        this.color = GameAssets.i.HUDNeutral;
        break;
      case FactionMode.Friendly:
        this.icon = this.unit.definition.friendlyIcon;
        this.color = GameAssets.i.HUDFriendly;
        break;
      case FactionMode.Enemy:
        this.icon = this.unit.definition.hostileIcon;
        this.color = GameAssets.i.HUDHostile;
        break;
    }
    this.UpdateVisibility(SceneSingleton<DynamicMap>.i.HQ, SceneSingleton<CameraStateManager>.i.transform.GlobalPosition());
  }

  public void SetFlashing(bool flashing)
  {
    this.flashing = flashing;
    if (this.selected || flashing)
      return;
    this.UpdateVisibility(SceneSingleton<DynamicMap>.i.HQ, SceneSingleton<CameraStateManager>.i.transform.GlobalPosition());
    this.UpdateColor();
  }

  private void HUDIcon_OnDisableUnit(Unit unit)
  {
    this.selected = false;
    SceneSingleton<CombatHUD>.i.RemoveMarker(this);
    this.RemoveIcon();
  }

  public void RemoveIcon()
  {
    this.unit.onDisableUnit -= new Action<Unit>(this.HUDIcon_OnDisableUnit);
    this.unit.onChangeFaction -= new Action<Unit>(this.HUDIcon_OnChangeFaction);
    DynamicMap.onShowTypesChanged -= new Action(this.HUDUnitMarker_OnMapShowTypesChanged);
    SceneSingleton<HUDOptions>.i.OnApplyOptions -= new Action(this.HUDUnitMarker_OnApplyOptions);
    PlayerSettings.OnApplyOptions -= new Action(this.HUDUnitMarker_OnApplyOptions);
    if ((UnityEngine.Object) SceneSingleton<CombatHUD>.i.aircraft != (UnityEngine.Object) null)
      SceneSingleton<CombatHUD>.i.aircraft.onSetGear -= new Action<Aircraft.OnSetGear>(this.HUDUnitMarker_OnSetGear);
    if (this.trackingInfo != null && this.outdated)
      this.trackingInfo.OnSpotted -= new Action(this.HUDUnitMarker_OnSpotted);
    NetworkSceneSingleton<Spawner>.i.DestroyLocal(this.image.gameObject, 0.0f);
  }

  private void HUDIcon_OnChangeFaction(Unit unit)
  {
    this.SetNew();
    this.UpdateColor();
  }

  public void SetLandingMode()
  {
    if (this.unit is Aircraft)
      return;
    this.image.enabled = false;
  }

  public void UpdatePosition(FactionHQ hq, GlobalPosition viewPosition, Vector3 cameraForward)
  {
    if (this.hidden)
      return;
    GlobalPosition knownPosition = this.unit.GlobalPosition();
    if (this.outdated && !hq.TryGetKnownPosition(this.unit, out knownPosition))
      return;
    if (this.selected)
    {
      Vector3 rayToScreen;
      float arrowAngle;
      if (HUDFunctions.PinToScreenEdge(knownPosition.ToLocalPosition(), out rayToScreen, out arrowAngle))
      {
        this.image.enabled = false;
        SceneSingleton<CombatHUD>.i.SetTargetArrow(true, rayToScreen, new Vector3(0.0f, 0.0f, (float) ((double) arrowAngle * 57.295780181884766 - 90.0)));
      }
      else
      {
        this.image.enabled = true;
        this._transform.position = rayToScreen;
        SceneSingleton<CombatHUD>.i.SetTargetArrow(false, Vector3.zero, Vector3.zero);
      }
      if (!this.unit.HasRadarEmission())
        return;
      if ((this.unit.radar as Radar).IsJammed())
      {
        if (!((UnityEngine.Object) this.image.sprite != (UnityEngine.Object) GameAssets.i.targetUnitSpriteJammed))
          return;
        this.image.sprite = GameAssets.i.targetUnitSpriteJammed;
      }
      else
      {
        if (!((UnityEngine.Object) this.image.sprite == (UnityEngine.Object) GameAssets.i.targetUnitSpriteJammed))
          return;
        this.image.sprite = DynamicMap.GetFactionMode(this.unit.NetworkHQ) == FactionMode.Friendly ? GameAssets.i.targetUnitSpriteFriendly : this.icon;
      }
    }
    else if ((double) Vector3.Dot(knownPosition - viewPosition, cameraForward) < 0.0)
    {
      if (!this.image.enabled)
        return;
      this.image.enabled = false;
    }
    else
    {
      if (!this.image.enabled)
        this.image.enabled = true;
      this._transform.position = Vector3.Scale(SceneSingleton<CameraStateManager>.i.mainCamera.WorldToScreenPoint(knownPosition.ToLocalPosition()), new Vector3(1f, 1f, 0.0f));
      if ((double) Time.timeSinceLevelLoad - (double) this.timeCreated < 1.0)
        this.image.color = Color.Lerp(this.color + Color.yellow, this.color, Time.timeSinceLevelLoad - this.timeCreated);
      if (!this.flashing)
        return;
      this.image.color = Color.Lerp(this.color + Color.yellow, this.color, Mathf.Sin(Time.timeSinceLevelLoad * 20f) + 0.5f);
    }
  }

  public void JammingDistortion(float jammingStrength)
  {
    if (this.hidden)
      return;
    jammingStrength = Mathf.Min(jammingStrength, 4f);
    Vector3 vector3 = UnityEngine.Random.insideUnitSphere * jammingStrength * 3f;
    this._transform.position += new Vector3(vector3.x, vector3.y, 0.0f);
    this.image.color = new Color(this.image.color.r, this.image.color.g, this.image.color.b, (float) (1.0 - (double) jammingStrength * (double) UnityEngine.Random.value));
  }

  private void SetOutdated(bool newState)
  {
    if ((UnityEngine.Object) this.image == (UnityEngine.Object) null || this.trackingInfo == null)
      return;
    this.outdated = newState;
    if (this.selected)
      this.image.sprite = this.outdated ? GameAssets.i.targetUnitSpriteOld : this.icon;
    else
      this.UpdateColor();
    if (this.outdated)
      this.trackingInfo.OnSpotted += new Action(this.HUDUnitMarker_OnSpotted);
    else
      this.trackingInfo.OnSpotted -= new Action(this.HUDUnitMarker_OnSpotted);
  }

  public void UpdateVisibility(FactionHQ hq, GlobalPosition viewPosition)
  {
    if (!this.outdated && !hq.IsTargetPositionAccurate(this.unit, 20f))
      this.SetOutdated(true);
    GlobalPosition knownPosition;
    if (this.selected || !hq.TryGetKnownPosition(this.unit, out knownPosition))
      return;
    this.UpdateMaximized(knownPosition, viewPosition, (UnityEngine.Object) hq != (UnityEngine.Object) null && (UnityEngine.Object) hq != (UnityEngine.Object) this.unit.NetworkHQ);
  }

  private void UpdateMaximized(
    GlobalPosition knownPosition,
    GlobalPosition viewPosition,
    bool enemy)
  {
    if (this.alwaysMaximized)
    {
      this.maximized = true;
      this.distanceScale = Mathf.Lerp(1f, 0.45f, (float) ((double) FastMath.Distance(viewPosition, knownPosition) * 3.9999998989515007E-05 - 0.5));
      this._transform.localScale = this.scale * this.customScale * this.distanceScale * Vector3.one;
      this.image.sprite = this.icon;
    }
    else
    {
      this.maximized = this.flashing || (double) Time.timeSinceLevelLoad - (double) this.timeCreated <= 4.0 || FastMath.InRange(knownPosition, viewPosition, (float) ((0.10000000149011612 + (double) this.threat) * 15000.0));
      if (this.maximized)
      {
        this._transform.localScale = this.scale * this.customScale * this.distanceScale * Vector3.one;
        this.image.sprite = this.icon;
      }
      else if (enemy)
      {
        this._transform.localScale = 6f * Vector3.one;
        this.image.sprite = SceneSingleton<CombatHUD>.i.minimizedHostile;
      }
      else
      {
        this._transform.localScale = 4f * Vector3.one;
        this.image.sprite = (Sprite) null;
      }
    }
  }

  private void UpdateColor()
  {
    if (this.selected)
      this.image.color = Color.green;
    else
      this.image.color = new Color(this.color.r, this.color.g, this.color.b, (float) ((double) this.opacity * (double) this.customOpacity * (!this.outdated || !this.maximized ? 1.0 : 0.5)));
  }

  public void HUDUnitMarker_OnSpotted()
  {
    if (!this.outdated)
      return;
    this.SetOutdated(false);
  }

  private void UpdateHidden(bool gearExtended)
  {
    if (gearExtended)
    {
      this.hidden = !this.alwaysMaximized;
    }
    else
    {
      this.hidden = false;
      if (this.unit is PilotDismounted)
        this.hidden = !SceneSingleton<MapOptions>.i.showPilotIcons;
    }
    if (this.hidden)
      this.image.enabled = false;
    else
      this.image.sprite = this.icon;
  }

  public void HUDUnitMarker_OnMapShowTypesChanged()
  {
    this.UpdateHidden(SceneSingleton<CombatHUD>.i.aircraft.gearDeployed);
  }

  public void SelectMarker()
  {
    this.selected = true;
    this.image.sprite = this.outdated ? GameAssets.i.targetUnitSpriteOld : this.icon;
    if ((UnityEngine.Object) this.unit.NetworkHQ == (UnityEngine.Object) SceneSingleton<CombatHUD>.i.aircraft.NetworkHQ)
      this.image.sprite = GameAssets.i.targetUnitSpriteFriendly;
    this._transform.localScale = Vector3.one * 20f;
    this.UpdateColor();
    SceneSingleton<DynamicMap>.i.SelectIcon(this.unit);
  }

  public void DeselectMarker()
  {
    this.selected = false;
    if ((UnityEngine.Object) SceneSingleton<CombatHUD>.i.aircraft != (UnityEngine.Object) null)
      this.UpdateVisibility(SceneSingleton<CombatHUD>.i.aircraft.NetworkHQ, SceneSingleton<CameraStateManager>.i.transform.GlobalPosition());
    this.UpdateColor();
    SceneSingleton<DynamicMap>.i.DeselectIcon(this.unit);
  }
}
