// Decompiled with JetBrains decompiler
// Type: LandingScreenUI
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using UnityEngine;
using UnityEngine.UI;

#nullable disable
public class LandingScreenUI : MonoBehaviour
{
  [SerializeField]
  private Canvas displayCanvas;
  [SerializeField]
  private Text typeText;
  [SerializeField]
  private Text altitude;
  [SerializeField]
  private Text vert_speed;
  [SerializeField]
  private Text speed;
  [SerializeField]
  private Text rel_speed;
  [SerializeField]
  private Text magText;
  [SerializeField]
  private Text modeText;
  [SerializeField]
  private Image velocity;
  private Camera cam;

  public void SetupCamera(Camera cam, Camera UICam)
  {
    this.displayCanvas.worldCamera = UICam;
    this.cam = cam;
  }

  private void OnDestroy()
  {
    if (!((Object) this.displayCanvas != (Object) null))
      return;
    Object.Destroy((Object) this.displayCanvas.gameObject);
  }

  public void SetInfo(float mag, bool IR)
  {
    if ((Object) this.displayCanvas == (Object) null)
      return;
    this.magText.text = $"Mag x{mag:F1}";
    this.modeText.text = IR ? "MODE: IR" : "MODE: COLOR";
  }

  private void Start()
  {
  }

  private void LateUpdate()
  {
    if ((Object) SceneSingleton<CombatHUD>.i.aircraft != (Object) null)
    {
      this.altitude.text = "ALT " + UnitConverter.DistanceReading(SceneSingleton<CombatHUD>.i.aircraft.radarAlt);
      this.speed.text = "SPD " + UnitConverter.SpeedReading(SceneSingleton<CombatHUD>.i.aircraft.speed);
      this.vert_speed.text = $"V {SceneSingleton<CombatHUD>.i.aircraft.rb.velocity.y:F1}";
      this.rel_speed.text = $"H {new Vector3(SceneSingleton<CombatHUD>.i.aircraft.rb.velocity.x, 0.0f, SceneSingleton<CombatHUD>.i.aircraft.rb.velocity.z).magnitude:F1}";
      if ((double) Vector3.Dot(SceneSingleton<CombatHUD>.i.aircraft.transform.forward, SceneSingleton<CombatHUD>.i.aircraft.rb.velocity) > 0.0)
      {
        if (!this.velocity.enabled)
          this.velocity.enabled = true;
        Vector3 vector3 = Vector3.Scale(this.cam.WorldToScreenPoint(this.cam.transform.position + SceneSingleton<CombatHUD>.i.aircraft.rb.velocity * 5f), new Vector3(1f, 1f, 0.0f)) - new Vector3(180f, 100f, 0.0f);
        vector3 = new Vector3(Mathf.Clamp(vector3.x, -180f, 180f), Mathf.Clamp(vector3.y, -100f, 100f), 0.0f);
        this.velocity.transform.localPosition = vector3;
      }
      else
      {
        if (!this.velocity.enabled)
          return;
        this.velocity.enabled = false;
      }
    }
    else
      this.gameObject.SetActive(false);
  }
}
