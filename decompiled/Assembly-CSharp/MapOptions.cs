// Decompiled with JetBrains decompiler
// Type: MapOptions
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

#nullable disable
public class MapOptions : SceneSingleton<MapOptions>
{
  public MFDScreen screen;
  public MapOptions.TooltipType tooltipType = MapOptions.TooltipType.Info;
  public bool showObjectives = true;
  public bool showTargetInfo = true;
  public bool showJamming = true;
  public bool showPilotIcons = true;
  public bool showGridLabels = true;
  public bool showAirbaseIcon = true;
  public float iconSize = 1f;

  public void ToggleShowObjectives()
  {
    this.showObjectives = !this.showObjectives;
    SceneSingleton<DynamicMap>.i.ShowTypeChanged();
  }

  public void ToggleShowTargetInfo()
  {
    this.showTargetInfo = !this.showTargetInfo;
    SceneSingleton<DynamicMap>.i.ShowTypeChanged();
  }

  public void ToggleShowJamming()
  {
    this.showJamming = !this.showJamming;
    SceneSingleton<DynamicMap>.i.ShowTypeChanged();
  }

  public void ToggleShowGridLabels() => this.showGridLabels = !this.showGridLabels;

  public void SetToolTipType(int value) => this.tooltipType = (MapOptions.TooltipType) value;

  public void SetIconSize(int value)
  {
    this.iconSize = (float) (0.60000002384185791 + 0.20000000298023224 * (double) value);
  }

  public void ToggleShowPilotIcons()
  {
    this.showPilotIcons = !this.showPilotIcons;
    foreach (MapIcon mapIcon in SceneSingleton<DynamicMap>.i.mapIcons)
    {
      if (mapIcon is UnitMapIcon unitMapIcon && unitMapIcon.unit is PilotDismounted)
        mapIcon.gameObject.SetActive(this.showPilotIcons);
    }
    SceneSingleton<DynamicMap>.i.ShowTypeChanged();
  }

  public void ToggleShowAirbaseIcons() => this.showAirbaseIcon = !this.showAirbaseIcon;

  public enum TooltipType
  {
    None,
    Info,
    Ammo,
    Order,
  }
}
