// Decompiled with JetBrains decompiler
// Type: VirtualMFD
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#nullable disable
public class VirtualMFD : MonoBehaviour
{
  [SerializeField]
  private Text speed;
  [SerializeField]
  private Text altitude;
  [SerializeField]
  private TMP_Text missionTime;
  [SerializeField]
  private Image attitude;
  [SerializeField]
  private Image horizon;
  [SerializeField]
  private List<Button> leftButtons;
  [SerializeField]
  private List<Button> rightButtons;
  [SerializeField]
  private List<MFDScreen> leftScreens = new List<MFDScreen>();
  [SerializeField]
  private List<MFDScreen> rightScreens = new List<MFDScreen>();
  [SerializeField]
  private Vector3 showPos;
  [SerializeField]
  private Vector3 hidePos;
  private MFDScreen activeLeft;
  private MFDScreen activeRight;

  private void Start()
  {
    if (GameManager.gameState == GameState.Editor)
    {
      this.gameObject.SetActive(false);
    }
    else
    {
      this.showPos = Vector3.zero;
      this.hidePos = (float) Screen.width * Vector3.right;
      SceneSingleton<DynamicMap>.i.onMapMaximized += new Action(this.VirtualMFD_onMapMaximized);
      SceneSingleton<DynamicMap>.i.onMapMinimized += new Action(this.VirtualMFD_onMapMinimized);
      this.SetupButtons();
      this.ToggleAllButtons(false);
      this.HideAllLeftScreens();
      this.HideAllRightScreens();
    }
  }

  private void Update()
  {
    if (DynamicMap.mapMaximized)
    {
      if ((UnityEngine.Object) SceneSingleton<CombatHUD>.i.aircraft != (UnityEngine.Object) null)
      {
        if (this.missionTime.isActiveAndEnabled)
        {
          this.missionTime.gameObject.SetActive(false);
          this.missionTime.transform.parent.gameObject.SetActive(false);
        }
        if (!this.speed.isActiveAndEnabled)
          this.speed.gameObject.SetActive(true);
        this.speed.text = UnitConverter.SpeedReading(SceneSingleton<CombatHUD>.i.aircraft.speed);
        if (!this.altitude.isActiveAndEnabled)
          this.altitude.gameObject.SetActive(true);
        this.altitude.text = UnitConverter.AltitudeReading(SceneSingleton<CombatHUD>.i.aircraft.radarAlt);
        if (!this.attitude.isActiveAndEnabled)
          this.attitude.gameObject.SetActive(true);
        this.horizon.transform.eulerAngles = new Vector3(0.0f, 0.0f, -SceneSingleton<CombatHUD>.i.aircraft.transform.eulerAngles.z);
        float x = SceneSingleton<CombatHUD>.i.aircraft.transform.eulerAngles.x;
        if ((double) x > 180.0)
          x -= 360f;
        this.horizon.fillAmount = Mathf.Clamp((float) (0.5 + (double) x / 180.0), 0.0f, 1f);
        this.horizon.transform.localPosition = Mathf.Clamp(x, -15f, 15f) * this.horizon.transform.up;
      }
      else
      {
        if (!this.missionTime.isActiveAndEnabled)
        {
          this.missionTime.gameObject.SetActive(true);
          this.missionTime.transform.parent.gameObject.SetActive(true);
        }
        if (this.speed.isActiveAndEnabled)
          this.speed.gameObject.SetActive(false);
        if (this.altitude.isActiveAndEnabled)
          this.altitude.gameObject.SetActive(false);
        if (this.attitude.isActiveAndEnabled)
          this.attitude.gameObject.SetActive(false);
        this.missionTime.text = UnitConverter.TimeOfDay(NetworkSceneSingleton<MissionManager>.i.MissionTime / 3600f, true);
      }
    }
    else
    {
      if (this.missionTime.isActiveAndEnabled)
      {
        this.missionTime.gameObject.SetActive(false);
        this.missionTime.transform.parent.gameObject.SetActive(false);
      }
      if (this.speed.isActiveAndEnabled)
        this.speed.gameObject.SetActive(false);
      if (this.altitude.isActiveAndEnabled)
        this.altitude.gameObject.SetActive(false);
      if (!this.attitude.isActiveAndEnabled)
        return;
      this.attitude.gameObject.SetActive(false);
    }
  }

  public void SetupButtons()
  {
    foreach (Button leftButton in this.leftButtons)
    {
      int index = this.leftButtons.IndexOf(leftButton);
      if (index < this.leftScreens.Count && (UnityEngine.Object) this.leftScreens[index] != (UnityEngine.Object) null)
        this.leftScreens[index].Setup(this, this.leftScreens[index].shortName);
      else
        leftButton.enabled = false;
    }
    foreach (Button rightButton in this.rightButtons)
    {
      int index = this.rightButtons.IndexOf(rightButton);
      if (index < this.rightScreens.Count && (UnityEngine.Object) this.rightScreens[index] != (UnityEngine.Object) null)
        this.rightScreens[index].Setup(this, this.rightScreens[index].shortName);
      else
        rightButton.enabled = false;
    }
  }

  public void VirtualMFD_onMapMaximized()
  {
    this.ToggleAllButtons(true);
    if ((UnityEngine.Object) SceneSingleton<CombatHUD>.i.aircraft != (UnityEngine.Object) null)
    {
      this.speed.gameObject.SetActive(true);
      this.altitude.gameObject.SetActive(true);
      this.attitude.gameObject.SetActive(true);
    }
    if ((UnityEngine.Object) this.activeLeft != (UnityEngine.Object) null)
      this.activeLeft.ShowScreen(-this.showPos);
    if (!((UnityEngine.Object) this.activeRight != (UnityEngine.Object) null))
      return;
    this.activeRight.ShowScreen(this.showPos);
  }

  public void VirtualMFD_onMapMinimized()
  {
    this.ToggleAllButtons(false);
    this.HideAllLeftScreens();
    this.HideAllRightScreens();
    this.speed.gameObject.SetActive(false);
    this.altitude.gameObject.SetActive(false);
    this.attitude.gameObject.SetActive(false);
  }

  public void PressLeftButton(Button button)
  {
    int index = this.leftButtons.IndexOf(button);
    MFDScreen leftScreen = this.leftScreens[index];
    if (!((UnityEngine.Object) leftScreen != (UnityEngine.Object) null) || (UnityEngine.Object) SceneSingleton<CombatHUD>.i.aircraft == (UnityEngine.Object) null && leftScreen.aircraftOnly)
      return;
    if (leftScreen.isActive)
    {
      leftScreen.CloseScreen(-this.hidePos);
      this.activeLeft = (MFDScreen) null;
    }
    else
    {
      if (leftScreen.isActive)
        return;
      this.HideAllLeftScreens();
      leftScreen.ShowScreen(-this.showPos);
      this.activeLeft = this.leftScreens[index];
    }
  }

  public void PressRightButton(Button button)
  {
    int index = this.rightButtons.IndexOf(button);
    MFDScreen rightScreen = this.rightScreens[index];
    if (!((UnityEngine.Object) rightScreen != (UnityEngine.Object) null) || (UnityEngine.Object) SceneSingleton<CombatHUD>.i.aircraft == (UnityEngine.Object) null && rightScreen.aircraftOnly)
      return;
    if (rightScreen.isActive)
    {
      rightScreen.CloseScreen(this.hidePos);
      this.activeRight = (MFDScreen) null;
    }
    else
    {
      if (rightScreen.isActive)
        return;
      this.HideAllRightScreens();
      rightScreen.ShowScreen(this.showPos);
      this.activeRight = this.rightScreens[index];
    }
  }

  public void HideAllLeftScreens()
  {
    foreach (MFDScreen leftScreen in this.leftScreens)
    {
      if ((UnityEngine.Object) leftScreen != (UnityEngine.Object) null)
        leftScreen.CloseScreen(-this.hidePos);
    }
  }

  public void HideAllRightScreens()
  {
    foreach (MFDScreen rightScreen in this.rightScreens)
    {
      if ((UnityEngine.Object) rightScreen != (UnityEngine.Object) null)
        rightScreen.CloseScreen(this.hidePos);
    }
  }

  public void ToggleAllButtons(bool show)
  {
    foreach (Component leftButton in this.leftButtons)
      leftButton.gameObject.SetActive(show);
    foreach (Component rightButton in this.rightButtons)
      rightButton.gameObject.SetActive(show);
  }
}
