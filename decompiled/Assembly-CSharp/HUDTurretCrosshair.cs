// Decompiled with JetBrains decompiler
// Type: HUDTurretCrosshair
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using UnityEngine;
using UnityEngine.UI;

#nullable disable
public class HUDTurretCrosshair : MonoBehaviour
{
  [SerializeField]
  private Image circle;
  [SerializeField]
  private Image readinessCircle;
  [SerializeField]
  private Image crosshair;
  private Turret turret;
  private Gun gun;

  public void Initialize(Turret turret)
  {
    this.turret = turret;
    if (!(turret.GetWeapon() is Gun weapon))
      return;
    this.gun = weapon;
  }

  public void Refresh(Camera mainCamera, out Vector3 crosshairPosition)
  {
    Vector3 direction = this.turret.GetDirection();
    bool flag = this.turret.IsOnTarget();
    crosshairPosition = Vector3.one * 10000f;
    if ((double) Vector3.Dot(mainCamera.transform.forward, direction - mainCamera.transform.position) > 0.0)
    {
      crosshairPosition = SceneSingleton<CameraStateManager>.i.mainCamera.WorldToScreenPoint(direction);
      crosshairPosition.z = 0.0f;
      this.transform.position = crosshairPosition;
      this.crosshair.enabled = true;
      float reloadProgress = this.gun.GetReloadProgress();
      if ((double) reloadProgress > 0.0)
      {
        if (!this.readinessCircle.enabled)
        {
          this.readinessCircle.enabled = true;
          this.crosshair.color = Color.red + Color.green * 0.5f;
        }
        this.readinessCircle.fillAmount = reloadProgress;
      }
      else if (this.readinessCircle.enabled)
      {
        this.readinessCircle.enabled = false;
        this.crosshair.color = Color.green;
      }
      this.circle.enabled = flag && (double) reloadProgress <= 0.0;
    }
    else
    {
      this.circle.enabled = false;
      this.readinessCircle.enabled = false;
      this.crosshair.enabled = false;
    }
  }
}
