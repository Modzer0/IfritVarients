// Decompiled with JetBrains decompiler
// Type: AirbaseMapIcon
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

#nullable disable
public class AirbaseMapIcon : MapIcon
{
  public Airbase airbase { get; private set; }

  protected override FactionHQ GetHQ() => this.airbase.CurrentHQ;

  protected override bool IsLocalPlayerAircraft() => false;

  protected override void OnSelectIcon()
  {
  }

  protected override void OnDeselectIcon()
  {
  }

  protected override void OnRemoveIcon()
  {
  }

  public void SetIcon(Airbase airbase)
  {
    this.transform.localScale = Vector3.one;
    this.transform.eulerAngles = Vector3.zero;
    this.iconImage.sprite = GameAssets.i.airbaseSprite;
    this.airbase = airbase;
    this.iconImage.transform.localScale = 50f * Vector3.one;
    this.UpdateColor();
  }

  protected override Color GetColor()
  {
    if (!this.airbase.AnyHangarsAvailable())
      return GameAssets.i.HUDAirbaseNotAvailable;
    return this.isSelected ? GameAssets.i.HUDFriendlySelected : GameAssets.i.HUDFriendly;
  }

  public override void ClickIcon(MapIcon.ClickSource clickSource)
  {
    if ((Object) SceneSingleton<CombatHUD>.i.aircraft != (Object) null && !SceneSingleton<CombatHUD>.i.aircraft.disabled || GameManager.gameResolution == GameResolution.Defeat || !this.airbase.AnyHangarsAvailable())
      return;
    SceneSingleton<DynamicMap>.i.UnselectAll();
    SceneSingleton<DynamicMap>.i.DeselectAllIcons();
    SceneSingleton<DynamicMap>.i.SelectIcon(this.airbase);
    SceneSingleton<GameplayUI>.i.SelectAirbase(this.airbase);
    this.iconImage.raycastTarget = false;
  }

  public override void UpdateIcon(
    float mapDisplayFactor,
    float mapInverseScale,
    Transform mapTransform,
    bool mapMaximized)
  {
    if ((Object) this.airbase == (Object) null)
      return;
    this.UpdateColor();
    this.gameObject.SetActive(mapMaximized && SceneSingleton<MapOptions>.i.showAirbaseIcon);
    this.globalPosition = this.airbase.center.GlobalPosition().AsVector3() * mapDisplayFactor;
    this.iconImage.transform.localPosition = new Vector3(this.globalPosition.x, this.globalPosition.z, 0.0f);
    this.iconImage.transform.eulerAngles = Vector3.zero;
    this.iconImage.transform.localScale = mapInverseScale * 50f * Vector3.one;
  }

  public override string GetInfoText()
  {
    if (this.airbase.disabled)
      return this.airbase.SavedAirbase.DisplayName + "\n(Disabled)";
    return this.airbase.AnyHangarsAvailable() ? this.airbase.SavedAirbase.DisplayName : this.airbase.SavedAirbase.DisplayName + "\n(No Hangars)";
  }
}
