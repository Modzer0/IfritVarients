// Decompiled with JetBrains decompiler
// Type: RadialMenuAction
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using UnityEngine;
using UnityEngine.UI;

#nullable disable
[CreateAssetMenu(fileName = "New Radial Menu Action", menuName = "ScriptableObjects/RadialMenuAction", order = 8)]
public class RadialMenuAction : ScriptableObject
{
  [SerializeField]
  public string DisplayName;
  [SerializeField]
  private RadialMenuAction.ActionType actionType;
  [SerializeField]
  private Sprite iconSprite;
  [SerializeField]
  private Sprite backgroundSprite;
  [SerializeField]
  private Text ammoText;
  [SerializeField]
  private Color backgroundColorInactive;
  [SerializeField]
  private Color backgroundColorActive;
  [SerializeField]
  public int weapon_number = -1;
  public bool Caution;
  private Image iconImage;
  private Image backgroundImage;
  private float flashAmount;
  private Color selectedColor = Color.green;
  private Color defaultColor = Color.gray;

  public bool AllowedOnAircraft(Aircraft aircraft)
  {
    switch (this.actionType)
    {
      case RadialMenuAction.ActionType.Eject:
        return true;
      case RadialMenuAction.ActionType.Gear:
        return true;
      case RadialMenuAction.ActionType.Radar:
        return (Object) aircraft.radar != (Object) null;
      case RadialMenuAction.ActionType.FlightAssist:
        return aircraft.GetControlsFilter().HasFlightAssist();
      case RadialMenuAction.ActionType.AutoHover:
        return aircraft.GetControlsFilter().HasAutoHover();
      case RadialMenuAction.ActionType.Engine:
        return true;
      case RadialMenuAction.ActionType.Nightvis:
        return true;
      case RadialMenuAction.ActionType.TurretAuto:
        return aircraft.weaponManager.StationsWithTurrets() > 0;
      case RadialMenuAction.ActionType.SelectWeapon:
        return aircraft.weaponStations.Count > this.weapon_number;
      case RadialMenuAction.ActionType.LinkGuns:
        return aircraft.weaponManager.HasMultipleGuns();
      default:
        return false;
    }
  }

  public void Setup(Image backgroundImage, Image iconImage, Text ammoText)
  {
    backgroundImage.gameObject.SetActive(true);
    this.backgroundImage = backgroundImage;
    backgroundImage.sprite = this.backgroundSprite;
    this.selectedColor = this.Caution ? Color.green * 0.9f + Color.red : Color.green;
    this.defaultColor = this.actionType == RadialMenuAction.ActionType.SelectWeapon ? Color.green : Color.gray;
    this.iconImage = iconImage;
    this.iconImage.sprite = this.iconSprite;
    iconImage.rectTransform.sizeDelta = this.actionType == RadialMenuAction.ActionType.SelectWeapon ? new Vector2(80f, 40f) : new Vector2(50f, 50f);
    iconImage.sprite = this.iconSprite;
    this.ammoText = ammoText;
    this.UnHover();
  }

  public void Hover()
  {
    this.backgroundImage.color = this.backgroundColorActive;
    this.iconImage.color = this.selectedColor;
    this.ammoText.color = this.selectedColor;
  }

  public void UnHover()
  {
    this.backgroundImage.color = this.backgroundColorInactive;
    this.iconImage.color = this.defaultColor;
    this.ammoText.color = this.defaultColor;
  }

  public void Flash()
  {
    this.iconImage.color = Color.Lerp(Color.gray, Color.yellow, this.flashAmount);
    this.flashAmount -= Time.deltaTime;
  }

  public void TriggerAction(Aircraft aircraft)
  {
    this.flashAmount = 1f;
    switch (this.actionType)
    {
      case RadialMenuAction.ActionType.Eject:
        aircraft.StartEjectionSequence();
        break;
      case RadialMenuAction.ActionType.Gear:
        if ((double) aircraft.radarAlt < 0.20000000298023224)
          break;
        if (aircraft.gearState == LandingGear.GearState.LockedExtended)
          aircraft.SetGear(false);
        if (aircraft.gearState != LandingGear.GearState.LockedRetracted)
          break;
        aircraft.SetGear(true);
        break;
      case RadialMenuAction.ActionType.Radar:
        aircraft.CmdToggleRadar();
        break;
      case RadialMenuAction.ActionType.FlightAssist:
        aircraft.TogglePitchLimiter();
        break;
      case RadialMenuAction.ActionType.AutoHover:
        aircraft.GetControlsFilter().ToggleAutoHover();
        break;
      case RadialMenuAction.ActionType.Engine:
        aircraft.CmdToggleIgnition();
        break;
      case RadialMenuAction.ActionType.Nightvis:
        NightVision.Toggle();
        break;
      case RadialMenuAction.ActionType.TurretAuto:
        SceneSingleton<CombatHUD>.i.ToggleAutoControl();
        break;
      case RadialMenuAction.ActionType.SelectWeapon:
        aircraft.SetActiveStation((byte) this.weapon_number);
        SceneSingleton<CombatHUD>.i.ShowWeaponStation(aircraft.weaponStations[this.weapon_number]);
        break;
      case RadialMenuAction.ActionType.LinkGuns:
        aircraft.weaponManager.ToggleGunsLinked();
        break;
    }
  }

  public void SetWeapon(WeaponInfo weaponInfo, int number)
  {
    this.weapon_number = number;
    this.iconSprite = weaponInfo.weaponIcon;
    this.DisplayName = weaponInfo.weaponName;
    this.selectedColor = Color.green;
    this.defaultColor = Color.green;
  }

  public void RefreshWeapon(string ammoReadout, float ammoLevel)
  {
    this.ammoText.enabled = true;
    this.ammoText.text = ammoReadout;
    if ((double) ammoLevel == 0.0)
    {
      this.selectedColor = Color.red;
      this.defaultColor = Color.grey;
    }
    else
    {
      this.selectedColor = Color.green;
      this.defaultColor = Color.green;
    }
  }

  public RadialMenuAction.ActionType GetActionType() => this.actionType;

  public enum ActionType
  {
    Eject,
    Gear,
    Radar,
    NavLights,
    FlightAssist,
    AutoHover,
    Engine,
    Nightvis,
    TurretAuto,
    SelectWeapon,
    LinkGuns,
  }
}
