// Decompiled with JetBrains decompiler
// Type: Bearing
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using UnityEngine;
using UnityEngine.UI;

#nullable disable
public class Bearing : HUDApp
{
  [SerializeField]
  private Text bearing;
  [SerializeField]
  private Aircraft aircraft;
  [SerializeField]
  private Image border;

  public override void Initialize(Aircraft aircraft) => this.aircraft = aircraft;

  public override void RefreshSettings()
  {
    base.RefreshSettings();
    this.bearing.fontSize = this.fontSize;
  }

  public override void Refresh()
  {
    if ((Object) this.aircraft == (Object) null)
      return;
    this.bearing.text = this.type == HUDApp.AppType.HMD ? $"{SceneSingleton<CameraStateManager>.i.transform.eulerAngles.y:F0}°" : $"{this.aircraft.transform.eulerAngles.y:F0}°";
  }
}
