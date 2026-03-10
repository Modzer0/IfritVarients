// Decompiled with JetBrains decompiler
// Type: MapToolTip
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#nullable disable
public class MapToolTip : MonoBehaviour
{
  [SerializeField]
  private Image line1;
  [SerializeField]
  private Image line2;
  [SerializeField]
  private Image circle1;
  [SerializeField]
  private Image circle2;
  [SerializeField]
  private Text infoText;
  [SerializeField]
  private GameObject iconContainer;
  [SerializeField]
  private Sprite circleThin;
  [SerializeField]
  private Sprite circleThick;
  [SerializeField]
  private List<TooltipItem> listToolTips = new List<TooltipItem>();
  private MapIcon icon;
  private float lastRefresh;
  private float refreshRate = 0.1f;
  public List<WeaponInfo> listMGTypes = new List<WeaponInfo>();
  public List<WeaponInfo> listGunTypes = new List<WeaponInfo>();
  public List<WeaponInfo> listBombTypes = new List<WeaponInfo>();
  public List<WeaponInfo> listSSMTypes = new List<WeaponInfo>();
  public List<WeaponInfo> listSAMTypes = new List<WeaponInfo>();

  private void OnDestroy() => DynamicMap.onMapChanged -= new Action(this.DynamicMap_onMapChange);

  private void Start()
  {
    DynamicMap.onMapChanged += new Action(this.DynamicMap_onMapChange);
    this.circle1.enabled = false;
    this.circle2.enabled = false;
    if ((UnityEngine.Object) this.icon == (UnityEngine.Object) null)
      return;
    this.Refresh(this.icon);
  }

  private void Update()
  {
    if ((double) Time.timeSinceLevelLoad < (double) this.lastRefresh + (double) this.refreshRate || (UnityEngine.Object) this.icon == (UnityEngine.Object) null)
      return;
    this.Refresh(this.icon);
  }

  private void DynamicMap_onMapChange()
  {
    if ((UnityEngine.Object) this.icon == (UnityEngine.Object) null)
      return;
    this.Refresh(this.icon);
  }

  public void RefreshAirbaseTooltip(Airbase airbase)
  {
    TooltipItem listToolTip1 = this.listToolTips[0];
    TooltipItem listToolTip2 = this.listToolTips[1];
    TooltipItem listToolTip3 = this.listToolTips[2];
    TooltipItem listToolTip4 = this.listToolTips[3];
    TooltipItem listToolTip5 = this.listToolTips[4];
    listToolTip1.gameObject.SetActive(true);
    listToolTip2.gameObject.SetActive(true);
    listToolTip3.gameObject.SetActive(true);
    listToolTip4.gameObject.SetActive(true);
    listToolTip5.gameObject.SetActive(true);
    int num1 = 0;
    int num2 = 0;
    int num3 = 0;
    int num4 = 0;
    int warheads = airbase.GetWarheads();
    if (airbase.AnyHangarsAvailable())
    {
      for (int index = 0; index < airbase.hangars.Count; ++index)
      {
        if (!airbase.hangars[index].Disabled)
        {
          if (airbase.hangars[index].attachedUnit.definition.code == "HPAD")
            ++num1;
          else if (airbase.hangars[index].attachedUnit.definition.code == "REV")
            ++num2;
          else if (airbase.hangars[index].attachedUnit.definition.code == "HGR-M")
            ++num3;
          else if (airbase.hangars[index].attachedUnit.definition.code == "HGR-H")
            ++num4;
        }
      }
      this.circle1.transform.localScale = Vector3.one;
      this.circle1.rectTransform.sizeDelta = 70f * Vector2.one;
      this.circle1.enabled = true;
      this.circle1.color = GameAssets.i.HUDFriendly;
      this.circle1.fillAmount = airbase.capture.controlBalance;
      this.circle2.transform.localScale = Vector3.one;
      this.circle2.rectTransform.sizeDelta = 70f * Vector2.one;
      this.circle2.enabled = true;
      this.circle2.color = GameAssets.i.HUDNeutral;
      this.circle2.fillClockwise = false;
      this.circle2.fillAmount = 1f - this.circle1.fillAmount;
    }
    listToolTip1.Setup(GameAssets.i.hangarSprite_helipad, new Color?(num1 > 0 ? Color.white : Color.grey), (string) null, new int?(num1));
    listToolTip2.Setup(GameAssets.i.hangarSprite_revetment, new Color?(num2 > 0 ? Color.white : Color.grey), (string) null, new int?(num2));
    listToolTip3.Setup(GameAssets.i.hangarSprite_medium, new Color?(num3 > 0 ? Color.white : Color.grey), (string) null, new int?(num3));
    listToolTip4.Setup(GameAssets.i.hangarSprite_shelter, new Color?(num4 > 0 ? Color.white : Color.grey), (string) null, new int?(num4));
    listToolTip5.Setup(GameAssets.i.warheadSprite, new Color?(warheads > 0 ? Color.white : Color.grey), (string) null, new int?(warheads));
  }

  public void RefreshFactoryTooltip(Building building, Factory factory)
  {
    if (!((UnityEngine.Object) building.NetworkHQ == (UnityEngine.Object) SceneSingleton<DynamicMap>.i.HQ) && !((UnityEngine.Object) SceneSingleton<DynamicMap>.i.HQ == (UnityEngine.Object) null))
      return;
    TooltipItem listToolTip = this.listToolTips[0];
    listToolTip.gameObject.SetActive(true);
    if (factory.GetProduction() != "")
      listToolTip.Setup((Sprite) null, new Color?(Color.white), $"{factory.GetProduction()}\n{factory.GetNextProduction(true)}", new int?());
    else
      listToolTip.gameObject.SetActive(false);
    this.circle1.transform.localScale = Vector3.one;
    this.circle1.rectTransform.sizeDelta = 50f * Vector2.one;
    this.circle1.enabled = listToolTip.gameObject.activeSelf;
    this.circle1.fillAmount = 1f - factory.GetNextProduction(false);
    this.circle1.color = Color.green;
    this.circle2.transform.localScale = Vector3.one;
    this.circle2.rectTransform.sizeDelta = 50f * Vector2.one;
    this.circle2.enabled = listToolTip.gameObject.activeSelf;
    this.circle2.fillClockwise = false;
    this.circle2.fillAmount = factory.GetNextProduction(false);
    this.circle2.color = Color.grey;
  }

  public void RefreshCarrierTooltip(Airbase airbase)
  {
    TooltipItem listToolTip1 = this.listToolTips[0];
    TooltipItem listToolTip2 = this.listToolTips[4];
    listToolTip1.gameObject.SetActive(true);
    listToolTip2.gameObject.SetActive(true);
    int num = 0;
    int warheads = airbase.GetWarheads();
    if (airbase.AnyHangarsAvailable())
    {
      for (int index = 0; index < airbase.hangars.Count; ++index)
      {
        if (!airbase.hangars[index].Disabled && airbase.hangars[index].attachedUnit.definition.code == "SHP")
          ++num;
      }
    }
    listToolTip1.Setup(GameAssets.i.hangarSprite_carrier, new Color?(num > 0 ? Color.white : Color.grey), (string) null, new int?(num));
    listToolTip2.Setup(GameAssets.i.warheadSprite, new Color?(warheads > 0 ? Color.white : Color.grey), (string) null, new int?(warheads));
  }

  public void RefreshAirUnitTooltip(Unit unit)
  {
    TooltipItem listToolTip1 = this.listToolTips[0];
    TooltipItem listToolTip2 = this.listToolTips[1];
    TooltipItem listToolTip3 = this.listToolTips[2];
    TooltipItem listToolTip4 = this.listToolTips[0];
    TooltipItem listToolTip5 = this.listToolTips[1];
    TooltipItem listToolTip6 = this.listToolTips[2];
    TooltipItem listToolTip7 = this.listToolTips[3];
    if (!(bool) (UnityEngine.Object) SceneSingleton<DynamicMap>.i.mapMarkers.Find((Predicate<MapMarker>) (x => x is TargetMarker targetMarker2 && (UnityEngine.Object) targetMarker2.Icon != (UnityEngine.Object) null && (UnityEngine.Object) targetMarker2.Icon.unit == (UnityEngine.Object) unit)) && !listToolTip1.gameObject.activeSelf)
    {
      if (SceneSingleton<MapOptions>.i.tooltipType == MapOptions.TooltipType.Ammo)
      {
        listToolTip4.gameObject.SetActive(true);
        listToolTip5.gameObject.SetActive(true);
        listToolTip6.gameObject.SetActive(true);
        listToolTip7.gameObject.SetActive(true);
      }
      else if (SceneSingleton<MapOptions>.i.tooltipType == MapOptions.TooltipType.Info)
      {
        listToolTip1.gameObject.SetActive(true);
        listToolTip2.gameObject.SetActive(true);
        listToolTip3.gameObject.SetActive(true);
      }
    }
    else if ((bool) (UnityEngine.Object) SceneSingleton<DynamicMap>.i.mapMarkers.Find((Predicate<MapMarker>) (x => x is TargetMarker targetMarker1 && (UnityEngine.Object) targetMarker1.Icon != (UnityEngine.Object) null && (UnityEngine.Object) targetMarker1.Icon.unit == (UnityEngine.Object) unit)) && listToolTip1.gameObject.activeSelf)
    {
      if (SceneSingleton<MapOptions>.i.tooltipType == MapOptions.TooltipType.Ammo)
      {
        listToolTip4.gameObject.SetActive(false);
        listToolTip5.gameObject.SetActive(false);
        listToolTip6.gameObject.SetActive(false);
        listToolTip7.gameObject.SetActive(false);
      }
      else if (SceneSingleton<MapOptions>.i.tooltipType == MapOptions.TooltipType.Info)
      {
        listToolTip1.gameObject.SetActive(false);
        listToolTip2.gameObject.SetActive(false);
        listToolTip3.gameObject.SetActive(false);
      }
    }
    if (SceneSingleton<MapOptions>.i.tooltipType == MapOptions.TooltipType.Info)
    {
      if ((UnityEngine.Object) unit.NetworkHQ == (UnityEngine.Object) null || (UnityEngine.Object) unit.NetworkHQ == (UnityEngine.Object) SceneSingleton<DynamicMap>.i.HQ || (UnityEngine.Object) SceneSingleton<DynamicMap>.i.HQ == (UnityEngine.Object) null || SceneSingleton<DynamicMap>.i.HQ.IsTargetPositionAccurate(unit, 20f))
      {
        listToolTip1.Setup((Sprite) null, new Color?(Color.white), "SPD " + UnitConverter.SpeedReading(unit.speed), new int?());
        listToolTip2.Setup((Sprite) null, new Color?(Color.white), "ALT " + UnitConverter.AltitudeReading(unit.radarAlt), new int?());
        listToolTip3.Setup((Sprite) null, new Color?(Color.white), $"HDG {Mathf.RoundToInt(unit.transform.eulerAngles.y)}°", new int?());
      }
      else
      {
        listToolTip1.Setup((Sprite) null, new Color?(Color.white), "SPD -", new int?());
        listToolTip2.Setup((Sprite) null, new Color?(Color.white), "ALT -", new int?());
        listToolTip3.Setup((Sprite) null, new Color?(Color.white), "HDG -", new int?());
      }
    }
    else if (SceneSingleton<MapOptions>.i.tooltipType == MapOptions.TooltipType.Ammo)
    {
      if ((UnityEngine.Object) unit.NetworkHQ == (UnityEngine.Object) null || (UnityEngine.Object) unit.NetworkHQ == (UnityEngine.Object) SceneSingleton<DynamicMap>.i.HQ || (UnityEngine.Object) SceneSingleton<DynamicMap>.i.HQ == (UnityEngine.Object) null)
      {
        int[] ammoByType1 = unit.GetAmmoByType(this.listMGTypes);
        int[] ammoByType2 = unit.GetAmmoByType(this.listBombTypes);
        int[] ammoByType3 = unit.GetAmmoByType(this.listSSMTypes);
        int[] ammoByType4 = unit.GetAmmoByType(this.listSAMTypes);
        listToolTip4.Setup((Sprite) null, new Color?(Color.white), ammoByType1[1] > 0 ? $"GUN {ammoByType1[0]}/{ammoByType1[1]}" : "GUN -", new int?());
        listToolTip5.Setup((Sprite) null, new Color?(Color.white), ammoByType2[1] > 0 ? $"BMB {ammoByType2[0]}/{ammoByType2[1]}" : "BMB -", new int?());
        listToolTip6.Setup((Sprite) null, new Color?(Color.white), ammoByType3[1] > 0 ? $"AGM {ammoByType3[0]}/{ammoByType3[1]}" : "AGM -", new int?());
        listToolTip7.Setup((Sprite) null, new Color?(Color.white), ammoByType4[1] > 0 ? $"AAM {ammoByType4[0]}/{ammoByType4[1]}" : "AAM -", new int?());
      }
      else
      {
        listToolTip4.Setup((Sprite) null, new Color?(Color.white), "GUN ?", new int?());
        listToolTip5.Setup((Sprite) null, new Color?(Color.white), "BMB ?", new int?());
        listToolTip6.Setup((Sprite) null, new Color?(Color.white), "AGM ?", new int?());
        listToolTip7.Setup((Sprite) null, new Color?(Color.white), "AAM ?", new int?());
      }
    }
    float num1 = 0.0f;
    float num2 = 0.0f;
    Aircraft aircraft = unit as Aircraft;
    if ((UnityEngine.Object) aircraft.weaponManager != (UnityEngine.Object) null && (UnityEngine.Object) aircraft.weaponManager.currentWeaponStation?.WeaponInfo != (UnityEngine.Object) null)
      num1 = aircraft.weaponManager.currentWeaponStation.WeaponInfo.targetRequirements.maxRange;
    TargetDetector[] componentsInChildren = unit.transform.GetComponentsInChildren<TargetDetector>();
    if (componentsInChildren.Length != 0)
    {
      for (int index = 0; index < componentsInChildren.Length; ++index)
      {
        if (componentsInChildren[index] is Radar && (double) componentsInChildren[index].GetRadarRange() > (double) num2)
          num2 = componentsInChildren[index].GetRadarRange();
        else if ((double) componentsInChildren[index].GetVisualRange() > (double) num2)
          num2 = componentsInChildren[index].GetVisualRange();
      }
    }
    Vector3 vector3_1 = 2f * num1 * SceneSingleton<DynamicMap>.i.MetersToPixels() * Vector3.one;
    Vector3 vector3_2 = 2f * num2 * SceneSingleton<DynamicMap>.i.MetersToPixels() * Vector3.one;
    this.circle1.rectTransform.sizeDelta = Vector2.one;
    this.circle1.sprite = this.circleThin;
    this.circle1.color = Color.red;
    this.circle1.transform.localScale = vector3_1 * SceneSingleton<DynamicMap>.i.mapImage.transform.localScale.x;
    this.circle2.rectTransform.sizeDelta = Vector2.one;
    this.circle2.sprite = this.circleThin;
    this.circle2.color = Color.cyan;
    this.circle2.transform.localScale = vector3_2 * SceneSingleton<DynamicMap>.i.mapImage.transform.localScale.x;
    List<Unit> targetList = aircraft.weaponManager.GetTargetList();
    if (targetList != null && targetList.Count > 0 && (UnityEngine.Object) targetList[0] != (UnityEngine.Object) null)
    {
      GlobalPosition knownPosition;
      unit.NetworkHQ.TryGetKnownPosition(targetList[0], out knownPosition);
      Vector3 vector3_3 = FastMath.Direction(unit.GlobalPosition(), knownPosition);
      Vector3 to = new Vector3(vector3_3.x, 0.0f, vector3_3.z);
      float num3 = 0.0f;
      float num4 = Vector3.SignedAngle(Vector3.forward, to, Vector3.up);
      UnitMapIcon mapIcon;
      if (DynamicMap.TryGetMapIcon(targetList[0], out mapIcon))
        num3 = Vector3.Distance(mapIcon.transform.localPosition, this.icon.transform.localPosition);
      this.line1.rectTransform.sizeDelta = new Vector2(1f, num3 * SceneSingleton<DynamicMap>.i.mapImage.transform.localScale.x);
      this.line1.rectTransform.localEulerAngles = new Vector3(0.0f, 0.0f, -num4);
      Color color = Color.grey;
      if (aircraft.weaponManager.currentWeaponStation.WeaponInfo.jammer)
        color = Color.yellow;
      else if ((double) aircraft.weaponManager.currentWeaponStation.WeaponInfo.effectiveness.antiSurface > 0.0)
        color = Color.red;
      else if ((double) aircraft.weaponManager.currentWeaponStation.WeaponInfo.effectiveness.antiAir > 0.0)
        color = Color.cyan;
      color.a = 0.6f;
      this.line1.color = color;
    }
    this.line1.enabled = (UnityEngine.Object) SceneSingleton<DynamicMap>.i.HQ == (UnityEngine.Object) null || (UnityEngine.Object) unit.NetworkHQ == (UnityEngine.Object) SceneSingleton<DynamicMap>.i.HQ;
    this.circle1.enabled = (UnityEngine.Object) SceneSingleton<DynamicMap>.i.HQ == (UnityEngine.Object) null || (UnityEngine.Object) unit.NetworkHQ == (UnityEngine.Object) SceneSingleton<DynamicMap>.i.HQ;
    this.circle2.enabled = (UnityEngine.Object) SceneSingleton<DynamicMap>.i.HQ == (UnityEngine.Object) null || (UnityEngine.Object) unit.NetworkHQ == (UnityEngine.Object) SceneSingleton<DynamicMap>.i.HQ;
  }

  public void RefreshMissileUnitTooltip(Unit unit)
  {
    TooltipItem listToolTip1 = this.listToolTips[0];
    TooltipItem listToolTip2 = this.listToolTips[1];
    TooltipItem listToolTip3 = this.listToolTips[2];
    if (!(bool) (UnityEngine.Object) SceneSingleton<DynamicMap>.i.mapMarkers.Find((Predicate<MapMarker>) (x => x is TargetMarker targetMarker2 && (UnityEngine.Object) targetMarker2.Icon != (UnityEngine.Object) null && (UnityEngine.Object) targetMarker2.Icon.unit == (UnityEngine.Object) unit)) && !listToolTip1.gameObject.activeSelf)
    {
      listToolTip1.gameObject.SetActive(true);
      listToolTip2.gameObject.SetActive(true);
      listToolTip3.gameObject.SetActive(true);
    }
    else if ((bool) (UnityEngine.Object) SceneSingleton<DynamicMap>.i.mapMarkers.Find((Predicate<MapMarker>) (x => x is TargetMarker targetMarker1 && (UnityEngine.Object) targetMarker1.Icon != (UnityEngine.Object) null && (UnityEngine.Object) targetMarker1.Icon.unit == (UnityEngine.Object) unit)) && listToolTip1.gameObject.activeSelf)
    {
      listToolTip1.gameObject.SetActive(false);
      listToolTip2.gameObject.SetActive(false);
      listToolTip3.gameObject.SetActive(false);
    }
    if ((UnityEngine.Object) unit.NetworkHQ == (UnityEngine.Object) null || (UnityEngine.Object) unit.NetworkHQ == (UnityEngine.Object) SceneSingleton<DynamicMap>.i.HQ || (UnityEngine.Object) SceneSingleton<DynamicMap>.i.HQ == (UnityEngine.Object) null || SceneSingleton<DynamicMap>.i.HQ.IsTargetPositionAccurate(unit, 20f))
    {
      listToolTip1.Setup((Sprite) null, new Color?(Color.white), "SPD " + UnitConverter.SpeedReading(unit.speed), new int?());
      listToolTip2.Setup((Sprite) null, new Color?(Color.white), "ALT " + UnitConverter.AltitudeReading(unit.GlobalPosition().y), new int?());
      listToolTip3.Setup((Sprite) null, new Color?(Color.white), $"HDG {Mathf.RoundToInt(unit.transform.eulerAngles.y)}°", new int?());
    }
    else
    {
      listToolTip1.Setup((Sprite) null, new Color?(Color.white), "SPD -", new int?());
      listToolTip2.Setup((Sprite) null, new Color?(Color.white), "ALT -", new int?());
      listToolTip3.Setup((Sprite) null, new Color?(Color.white), "HDG -", new int?());
    }
    Unit unit1;
    UnitRegistry.TryGetUnit(new PersistentID?((unit as Missile).targetID), out unit1);
    if ((UnityEngine.Object) unit1 != (UnityEngine.Object) null)
    {
      GlobalPosition knownPosition;
      unit.NetworkHQ.TryGetKnownPosition(unit1, out knownPosition);
      Vector3 vector3 = FastMath.Direction(unit.GlobalPosition(), knownPosition);
      Vector3 to = new Vector3(vector3.x, 0.0f, vector3.z);
      float num1 = 0.0f;
      float num2 = Vector3.SignedAngle(Vector3.forward, to, Vector3.up);
      UnitMapIcon mapIcon;
      if (DynamicMap.TryGetMapIcon(unit1, out mapIcon))
        num1 = Vector3.Distance(mapIcon.transform.localPosition, this.icon.transform.localPosition);
      this.line1.rectTransform.sizeDelta = new Vector2(1f, num1 * SceneSingleton<DynamicMap>.i.mapImage.transform.localScale.x);
      this.line1.rectTransform.localEulerAngles = new Vector3(0.0f, 0.0f, -num2);
      this.line1.color = Color.red with { a = 0.6f };
    }
    this.line1.enabled = (UnityEngine.Object) SceneSingleton<DynamicMap>.i.HQ == (UnityEngine.Object) null || (UnityEngine.Object) unit.NetworkHQ == (UnityEngine.Object) SceneSingleton<DynamicMap>.i.HQ;
  }

  public void RefreshGroundUnitTooltip(Unit unit)
  {
    TooltipItem listToolTip1 = this.listToolTips[0];
    TooltipItem listToolTip2 = this.listToolTips[1];
    TooltipItem listToolTip3 = this.listToolTips[2];
    TooltipItem listToolTip4 = this.listToolTips[0];
    TooltipItem listToolTip5 = this.listToolTips[1];
    TooltipItem listToolTip6 = this.listToolTips[2];
    TooltipItem listToolTip7 = this.listToolTips[3];
    TooltipItem orderPlayer = this.listToolTips[0];
    TooltipItem orderTime = this.listToolTips[1];
    if (!(bool) (UnityEngine.Object) SceneSingleton<DynamicMap>.i.mapMarkers.Find((Predicate<MapMarker>) (x => x is TargetMarker targetMarker2 && (UnityEngine.Object) targetMarker2.Icon != (UnityEngine.Object) null && (UnityEngine.Object) targetMarker2.Icon.unit == (UnityEngine.Object) unit)) && !this.listToolTips[0].gameObject.activeSelf)
    {
      if (SceneSingleton<MapOptions>.i.tooltipType == MapOptions.TooltipType.Ammo)
      {
        listToolTip4.gameObject.SetActive(true);
        listToolTip5.gameObject.SetActive(true);
        listToolTip6.gameObject.SetActive(true);
        listToolTip7.gameObject.SetActive(true);
      }
      else if (SceneSingleton<MapOptions>.i.tooltipType == MapOptions.TooltipType.Info)
      {
        listToolTip1.gameObject.SetActive(true);
        listToolTip3.gameObject.SetActive(true);
      }
      else if (SceneSingleton<MapOptions>.i.tooltipType == MapOptions.TooltipType.Order)
      {
        orderPlayer.gameObject.SetActive(true);
        orderTime.gameObject.SetActive(true);
      }
    }
    else if ((bool) (UnityEngine.Object) SceneSingleton<DynamicMap>.i.mapMarkers.Find((Predicate<MapMarker>) (x => x is TargetMarker targetMarker1 && (UnityEngine.Object) targetMarker1.Icon != (UnityEngine.Object) null && (UnityEngine.Object) targetMarker1.Icon.unit == (UnityEngine.Object) unit)) && this.listToolTips[0].gameObject.activeSelf)
    {
      if (SceneSingleton<MapOptions>.i.tooltipType == MapOptions.TooltipType.Ammo)
      {
        listToolTip4.gameObject.SetActive(false);
        listToolTip5.gameObject.SetActive(false);
        listToolTip6.gameObject.SetActive(false);
        listToolTip7.gameObject.SetActive(false);
      }
      else if (SceneSingleton<MapOptions>.i.tooltipType == MapOptions.TooltipType.Info)
      {
        listToolTip1.gameObject.SetActive(false);
        listToolTip3.gameObject.SetActive(false);
      }
      else if (SceneSingleton<MapOptions>.i.tooltipType == MapOptions.TooltipType.Order)
      {
        orderPlayer.gameObject.SetActive(false);
        orderTime.gameObject.SetActive(false);
      }
    }
    if (SceneSingleton<MapOptions>.i.tooltipType == MapOptions.TooltipType.Info)
    {
      if ((UnityEngine.Object) unit.NetworkHQ == (UnityEngine.Object) null || (UnityEngine.Object) unit.NetworkHQ == (UnityEngine.Object) SceneSingleton<DynamicMap>.i.HQ || (UnityEngine.Object) SceneSingleton<DynamicMap>.i.HQ == (UnityEngine.Object) null || SceneSingleton<DynamicMap>.i.HQ.IsTargetPositionAccurate(unit, 20f))
      {
        listToolTip1.Setup((Sprite) null, new Color?(Color.white), "SPD " + UnitConverter.SpeedReading(unit.speed), new int?());
        listToolTip3.Setup((Sprite) null, new Color?(Color.white), $"HDG {Mathf.RoundToInt(unit.transform.eulerAngles.y)}°", new int?());
      }
      else
      {
        listToolTip1.Setup((Sprite) null, new Color?(Color.white), "SPD -", new int?());
        listToolTip3.Setup((Sprite) null, new Color?(Color.white), "HDG -", new int?());
      }
    }
    else if (SceneSingleton<MapOptions>.i.tooltipType == MapOptions.TooltipType.Ammo)
    {
      if ((UnityEngine.Object) unit.NetworkHQ == (UnityEngine.Object) null || (UnityEngine.Object) unit.NetworkHQ == (UnityEngine.Object) SceneSingleton<DynamicMap>.i.HQ || (UnityEngine.Object) SceneSingleton<DynamicMap>.i.HQ == (UnityEngine.Object) null)
      {
        int[] ammoByType1 = unit.GetAmmoByType(this.listMGTypes);
        int[] ammoByType2 = unit.GetAmmoByType(this.listGunTypes);
        int[] ammoByType3 = unit.GetAmmoByType(this.listSSMTypes);
        int[] ammoByType4 = unit.GetAmmoByType(this.listSAMTypes);
        listToolTip4.Setup((Sprite) null, new Color?(Color.white), ammoByType1[1] > 0 ? $"MG {ammoByType1[0]}/{ammoByType1[1]}" : "MG -", new int?());
        listToolTip5.Setup((Sprite) null, new Color?(Color.white), ammoByType2[1] > 0 ? $"GUN {ammoByType2[0]}/{ammoByType2[1]}" : "GUN -", new int?());
        listToolTip6.Setup((Sprite) null, new Color?(Color.white), ammoByType3[1] > 0 ? $"SSM {ammoByType3[0]}/{ammoByType3[1]}" : "SSM -", new int?());
        listToolTip7.Setup((Sprite) null, new Color?(Color.white), ammoByType4[1] > 0 ? $"SAM {ammoByType4[0]}/{ammoByType4[1]}" : "SAM -", new int?());
      }
      else
      {
        listToolTip4.Setup((Sprite) null, new Color?(Color.white), "MG ?", new int?());
        listToolTip5.Setup((Sprite) null, new Color?(Color.white), "GUN ?", new int?());
        listToolTip6.Setup((Sprite) null, new Color?(Color.white), "SSM ?", new int?());
        listToolTip7.Setup((Sprite) null, new Color?(Color.white), "SAM ?", new int?());
      }
    }
    else if (SceneSingleton<MapOptions>.i.tooltipType == MapOptions.TooltipType.Order)
    {
      if ((UnityEngine.Object) unit.NetworkHQ == (UnityEngine.Object) null || (UnityEngine.Object) unit.NetworkHQ == (UnityEngine.Object) SceneSingleton<DynamicMap>.i.HQ || (UnityEngine.Object) SceneSingleton<DynamicMap>.i.HQ == (UnityEngine.Object) null)
      {
        this.RefreshOrderFromServer(unit, (Action<UnitCommand.Command>) (lastOrder =>
        {
          string desc = UnitConverter.TimeOfDay(lastOrder.time / 3600f, true);
          if ((UnityEngine.Object) lastOrder.player != (UnityEngine.Object) null)
            orderPlayer.Setup((Sprite) null, new Color?(Color.white), lastOrder.player.PlayerName ?? "", new int?());
          else
            orderPlayer.Setup((Sprite) null, new Color?(Color.white), "AI", new int?());
          orderTime.Setup((Sprite) null, new Color?(Color.white), desc, new int?());
          if ((double) lastOrder.position.AsVector3().magnitude <= 1.0)
            return;
          Vector3 vector3 = FastMath.Direction(unit.GlobalPosition(), lastOrder.position);
          float num = Vector3.SignedAngle(Vector3.forward, new Vector3(vector3.x, 0.0f, vector3.z), Vector3.up);
          this.line2.rectTransform.sizeDelta = new Vector2(1f, Vector3.Distance(unit.GlobalPosition().AsVector3(), lastOrder.position.AsVector3()) * SceneSingleton<DynamicMap>.i.MetersToPixels() * SceneSingleton<DynamicMap>.i.mapImage.transform.localScale.x);
          this.line2.rectTransform.localEulerAngles = new Vector3(0.0f, 0.0f, -num);
          this.line2.color = Color.yellow with { a = 0.6f };
          this.line2.enabled = true;
        }));
      }
      else
      {
        orderPlayer.Setup((Sprite) null, new Color?(Color.white), "?", new int?());
        orderTime.Setup((Sprite) null, new Color?(Color.white), "?", new int?());
      }
    }
    float num1 = 0.0f;
    if (unit.weaponStations.Count > 0)
    {
      for (int index = 0; index < unit.weaponStations.Count; ++index)
      {
        if (unit.weaponStations[index] != null && ((double) unit.weaponStations[index].WeaponInfo.effectiveness.antiAir > 0.0 || (double) unit.weaponStations[index].WeaponInfo.effectiveness.antiMissile > 0.0) && (double) unit.weaponStations[index].WeaponInfo.targetRequirements.maxRange > (double) num1)
          num1 = unit.weaponStations[index].WeaponInfo.targetRequirements.maxRange;
      }
    }
    TargetDetector[] componentsInChildren = unit.transform.GetComponentsInChildren<TargetDetector>();
    float num2 = 0.0f;
    if (componentsInChildren.Length != 0)
    {
      for (int index = 0; index < componentsInChildren.Length; ++index)
      {
        if (componentsInChildren[index] is Radar && (double) componentsInChildren[index].GetRadarRange() > (double) num2)
          num2 = componentsInChildren[index].GetRadarRange();
        else if ((double) componentsInChildren[index].GetVisualRange() > (double) num2)
          num2 = componentsInChildren[index].GetVisualRange();
      }
    }
    Vector3 vector3_1 = 2f * num1 * SceneSingleton<DynamicMap>.i.MetersToPixels() * Vector3.one;
    Vector3 vector3_2 = 2f * num2 * SceneSingleton<DynamicMap>.i.MetersToPixels() * Vector3.one;
    this.circle1.rectTransform.sizeDelta = Vector2.one;
    this.circle1.sprite = this.circleThin;
    this.circle1.color = Color.red;
    this.circle1.transform.localScale = vector3_1 * SceneSingleton<DynamicMap>.i.mapImage.transform.localScale.x;
    this.circle2.rectTransform.sizeDelta = Vector2.one;
    this.circle2.sprite = this.circleThin;
    this.circle2.color = Color.cyan;
    this.circle2.transform.localScale = vector3_2 * SceneSingleton<DynamicMap>.i.mapImage.transform.localScale.x;
    this.circle1.enabled = true;
    this.circle2.enabled = true;
  }

  private void RefreshOrderFromServer(Unit unit, Action<UnitCommand.Command> setUI)
  {
    if (!(unit is ICommandable commandable))
    {
      setUI(new UnitCommand.Command());
    }
    else
    {
      UnitCommand unitCommand = commandable.UnitCommand;
      setUI(unitCommand.GetCommandCached());
      if (unitCommand.IsServer)
        return;
      UniTask.Void((Func<UniTaskVoid>) (async () =>
      {
        MapIcon currentIcon = this.icon;
        UnitCommand.Command command = await unitCommand.CmdGetCommand();
        if (!((UnityEngine.Object) currentIcon == (UnityEngine.Object) this.icon))
        {
          currentIcon = (MapIcon) null;
        }
        else
        {
          setUI(command);
          currentIcon = (MapIcon) null;
        }
      }));
    }
  }

  public void ShowTooltip(MapIcon mapIcon)
  {
    this.icon = mapIcon;
    if (!((UnityEngine.Object) this.icon != (UnityEngine.Object) null))
      return;
    this.ResetAll();
    this.Refresh(this.icon);
  }

  public void Refresh(MapIcon icon)
  {
    this.lastRefresh = Time.timeSinceLevelLoad;
    this.transform.localScale = Vector3.one * (1f / SceneSingleton<DynamicMap>.i.mapImage.transform.localScale.x);
    this.transform.position = icon.transform.position;
    this.infoText.text = icon.GetInfoText();
    this.infoText.color = icon.iconImage.color;
    if (SceneSingleton<MapOptions>.i.tooltipType == MapOptions.TooltipType.None || GameManager.gameState == GameState.Editor)
      return;
    switch (icon)
    {
      case AirbaseMapIcon airbaseMapIcon:
        Unit attachedUnit;
        airbaseMapIcon.airbase.TryGetAttachedUnit(out attachedUnit);
        if ((UnityEngine.Object) attachedUnit != (UnityEngine.Object) null)
        {
          this.RefreshCarrierTooltip(airbaseMapIcon.airbase);
          break;
        }
        this.RefreshAirbaseTooltip(airbaseMapIcon.airbase);
        break;
      case UnitMapIcon unitMapIcon:
        if (unitMapIcon.unit is Aircraft)
        {
          this.RefreshAirUnitTooltip(unitMapIcon.unit);
          break;
        }
        if (unitMapIcon.unit is Missile)
        {
          this.RefreshMissileUnitTooltip(unitMapIcon.unit);
          break;
        }
        if (unitMapIcon.unit is Ship || unitMapIcon.unit is GroundVehicle || unitMapIcon.unit is Building)
        {
          this.RefreshGroundUnitTooltip(unitMapIcon.unit);
          break;
        }
        if (!((UnityEngine.Object) unitMapIcon.Factory != (UnityEngine.Object) null))
          break;
        this.RefreshFactoryTooltip((Building) unitMapIcon.unit, unitMapIcon.Factory);
        break;
    }
  }

  public void ResetAll()
  {
    foreach (Component listToolTip in this.listToolTips)
      listToolTip.gameObject.SetActive(false);
    this.infoText.text = "";
    this.circle1.transform.localScale = Vector3.zero;
    this.circle1.rectTransform.sizeDelta = Vector2.zero;
    this.circle1.fillClockwise = true;
    this.circle1.fillAmount = 1f;
    this.circle1.color = Color.white;
    this.circle1.sprite = this.circleThick;
    this.circle1.enabled = false;
    this.circle2.transform.localScale = Vector3.zero;
    this.circle2.rectTransform.sizeDelta = Vector2.zero;
    this.circle2.sprite = this.circleThick;
    this.circle2.fillAmount = 1f;
    this.circle2.color = Color.white;
    this.circle2.fillClockwise = true;
    this.circle2.enabled = false;
    this.line1.enabled = false;
    this.line1.rectTransform.sizeDelta = Vector2.zero;
    this.line2.enabled = false;
    this.line2.rectTransform.sizeDelta = Vector2.zero;
  }
}
