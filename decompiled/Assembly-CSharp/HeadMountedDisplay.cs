// Decompiled with JetBrains decompiler
// Type: HeadMountedDisplay
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;
using UnityEngine.UI;

#nullable disable
public class HeadMountedDisplay : SceneSingleton<HeadMountedDisplay>
{
  [SerializeField]
  private Image bearing_img;
  [SerializeField]
  private HUDApp horizon;
  [SerializeField]
  private HUDApp altitude;
  [SerializeField]
  private HUDApp speed;
  [SerializeField]
  private HUDApp bearing;
  private Aircraft aircraftPrev;
  [SerializeField]
  private float posX = 350f;
  [SerializeField]
  private float posY = 300f;
  [SerializeField]
  private float topY = 300f;
  [SerializeField]
  private float sizeX = 1920f;
  [SerializeField]
  private float sizeY = 1080f;
  private float hideDistance = 600f;

  private void Start()
  {
    this.aircraftPrev = SceneSingleton<CombatHUD>.i.aircraft;
    this.horizon.Initialize(SceneSingleton<CombatHUD>.i.aircraft);
    this.altitude.Initialize(SceneSingleton<CombatHUD>.i.aircraft);
    this.speed.Initialize(SceneSingleton<CombatHUD>.i.aircraft);
    this.bearing.Initialize(SceneSingleton<CombatHUD>.i.aircraft);
    PlayerSettings.OnApplyOptions += new Action(this.RefreshSettings);
    this.RefreshSettings();
  }

  public void RefreshSettings()
  {
    this.sizeX = PlayerSettings.hmdWidth;
    this.sizeY = PlayerSettings.hmdHeight;
    this.hideDistance = (float) ((double) PlayerSettings.hmdHideDist * 0.5 * ((double) this.sizeX + (double) this.sizeY));
    this.posX = PlayerSettings.hmdSideDist * Mathf.Cos((float) Math.PI / 180f * PlayerSettings.hmdSideAngle);
    this.posY = PlayerSettings.hmdSideDist * Mathf.Sin((float) Math.PI / 180f * PlayerSettings.hmdSideAngle);
    this.topY = PlayerSettings.hmdTopHeight;
    this.GetComponent<RectTransform>().sizeDelta = new Vector2(this.sizeX, this.sizeY);
    this.altitude.transform.localPosition = (Vector3) new Vector2(this.posX, this.posY);
    this.speed.transform.localPosition = (Vector3) new Vector2(-this.posX, this.posY);
    this.bearing.transform.localPosition = (Vector3) new Vector2(0.0f, this.topY + 50f);
    this.horizon.transform.localPosition = (Vector3) new Vector2(0.0f, this.topY);
    this.horizon.RefreshSettings();
    this.altitude.RefreshSettings();
    this.speed.RefreshSettings();
    this.bearing.RefreshSettings();
  }

  private void Update()
  {
    if ((UnityEngine.Object) SceneSingleton<CombatHUD>.i.aircraft == (UnityEngine.Object) null)
      return;
    if ((UnityEngine.Object) SceneSingleton<CombatHUD>.i.aircraft != (UnityEngine.Object) this.aircraftPrev)
    {
      this.bearing.Initialize(SceneSingleton<CombatHUD>.i.aircraft);
      this.horizon.Initialize(SceneSingleton<CombatHUD>.i.aircraft);
      this.altitude.Initialize(SceneSingleton<CombatHUD>.i.aircraft);
      this.speed.Initialize(SceneSingleton<CombatHUD>.i.aircraft);
      this.aircraftPrev = SceneSingleton<CombatHUD>.i.aircraft;
    }
    Vector3 position = SceneSingleton<FlightHud>.i.GetHUDCenter().position;
    bool flag1 = FastMath.InRange(position, this.altitude.transform.position, this.hideDistance);
    bool flag2 = FastMath.InRange(position, this.speed.transform.position, this.hideDistance);
    bool flag3 = FastMath.InRange(position, this.bearing.transform.position, this.hideDistance);
    int num = FastMath.InRange(position, this.horizon.transform.position, this.hideDistance) ? 1 : 0;
    this.horizon.Refresh();
    this.altitude.Refresh();
    this.speed.Refresh();
    this.bearing.Refresh();
    if (flag1)
    {
      if (this.altitude.gameObject.activeSelf)
        this.altitude.gameObject.SetActive(false);
    }
    else if (!this.altitude.gameObject.activeSelf)
      this.altitude.gameObject.SetActive(true);
    if (flag2)
    {
      if (this.speed.gameObject.activeSelf)
        this.speed.gameObject.SetActive(false);
    }
    else if (!this.speed.gameObject.activeSelf)
      this.speed.gameObject.SetActive(true);
    if (flag3)
    {
      if (this.bearing.gameObject.activeSelf)
        this.bearing.gameObject.SetActive(false);
    }
    else if (!this.bearing.gameObject.activeSelf)
      this.bearing.gameObject.SetActive(true);
    if (num != 0)
    {
      if (!this.horizon.gameObject.activeSelf)
        return;
      this.horizon.gameObject.SetActive(false);
    }
    else
    {
      if (this.horizon.gameObject.activeSelf)
        return;
      this.horizon.gameObject.SetActive(true);
    }
  }

  private void OnDestroy() => PlayerSettings.OnApplyOptions -= new Action(this.RefreshSettings);
}
