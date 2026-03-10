// Decompiled with JetBrains decompiler
// Type: ArtificialHorizon
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using UnityEngine;
using UnityEngine.UI;

#nullable disable
public class ArtificialHorizon : HUDApp
{
  [SerializeField]
  private Image attitude_img;
  [SerializeField]
  private Image horizon_img;
  [SerializeField]
  private Image sky_img;
  [SerializeField]
  private Aircraft aircraft;

  public override void Initialize(Aircraft aircraft) => this.aircraft = aircraft;

  public override void RefreshSettings()
  {
  }

  public override void Refresh()
  {
    if ((Object) SceneSingleton<CombatHUD>.i.aircraft == (Object) null || (Object) this.aircraft == (Object) null)
      return;
    this.horizon_img.transform.localEulerAngles = new Vector3(0.0f, 0.0f, -SceneSingleton<CombatHUD>.i.aircraft.transform.eulerAngles.z);
    float x = SceneSingleton<CombatHUD>.i.aircraft.transform.eulerAngles.x;
    if ((double) x > 180.0)
      x -= 360f;
    this.horizon_img.fillAmount = Mathf.Clamp((float) (0.5 + (double) x / 180.0), 0.0f, 1f);
    this.sky_img.fillAmount = Mathf.Clamp((float) (0.5 - (double) x / 180.0), 0.0f, 1f);
  }
}
