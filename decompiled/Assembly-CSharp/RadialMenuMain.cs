// Decompiled with JetBrains decompiler
// Type: RadialMenuMain
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Cysharp.Threading.Tasks;
using Rewired;
using System;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#nullable disable
public class RadialMenuMain : SceneSingleton<RadialMenuMain>
{
  [SerializeField]
  private RadialMenuAction[] actionsMain;
  [SerializeField]
  private RadialMenuAction[] actionsWeapons;
  private List<RadialMenuAction> allowedActionsMain = new List<RadialMenuAction>();
  private List<RadialMenuAction> allowedActionsWeapons = new List<RadialMenuAction>();
  [SerializeField]
  private RadialMenuAction actionWeaponPrefab;
  [SerializeField]
  private GameObject actionPrefab;
  private RadialMenuAction selectedAction;
  private Player playerInput;
  [SerializeField]
  private Aircraft aircraft;
  [SerializeField]
  private GameObject menuObject;
  [SerializeField]
  private GameObject actionsMainContainer;
  [SerializeField]
  private GameObject actionsWeaponsContainer;
  [SerializeField]
  private GameObject sectorObject;
  [SerializeField]
  private TMP_Text selectedActionText;
  [SerializeField]
  private CanvasGroup canvasGroup;
  private float lastOpen;
  private float lastSelection;
  private float degreesPerActionMain;
  private float degreesPerActionWeapons;
  private bool showWeaponWheel;
  [SerializeField]
  private List<GameObject> actionObjectsMain = new List<GameObject>();
  [SerializeField]
  private List<GameObject> actionObjectsWeapons = new List<GameObject>();
  private RadialMenuMain.RadialMenuType currentState;
  private int currentIndex = -1;
  private Vector3 mousePos;
  private Vector3 mouseDelta;

  public void CloseMenu()
  {
    this.selectedAction = (RadialMenuAction) null;
    this.currentState = RadialMenuMain.RadialMenuType.Closed;
    this.currentIndex = -1;
    this.selectedActionText.text = "";
  }

  public static bool IsInUse() => SceneSingleton<RadialMenuMain>.i.menuObject.activeSelf;

  public void OpenMenu()
  {
    Aircraft localAircraft;
    GameManager.GetLocalAircraft(out localAircraft);
    if ((UnityEngine.Object) localAircraft == (UnityEngine.Object) null)
      return;
    if ((UnityEngine.Object) localAircraft != (UnityEngine.Object) this.aircraft)
    {
      this.aircraft = localAircraft;
      this.SetupMain();
      this.SetupWeapons();
    }
    this.RefreshWeapons();
    this.lastOpen = Time.realtimeSinceStartup;
    this.mousePos = Input.mousePosition;
  }

  private void OnEnable()
  {
    this.playerInput = ReInput.players.GetPlayer(0);
    this.CloseMenu();
  }

  private void SetupMain()
  {
    this.allowedActionsMain.Clear();
    foreach (UnityEngine.Object @object in this.actionObjectsMain)
      UnityEngine.Object.Destroy(@object);
    this.actionObjectsMain.Clear();
    foreach (RadialMenuAction radialMenuAction in this.actionsMain)
    {
      if (radialMenuAction.AllowedOnAircraft(this.aircraft))
        this.allowedActionsMain.Add(radialMenuAction);
    }
    this.degreesPerActionMain = 360f / (float) this.allowedActionsMain.Count;
    for (int index = 0; index < this.allowedActionsMain.Count; ++index)
    {
      GameObject gameObject1 = UnityEngine.Object.Instantiate<GameObject>(this.sectorObject, this.actionsMainContainer.transform);
      Image component1 = gameObject1.GetComponent<Image>();
      component1.fillAmount = this.degreesPerActionMain / 360f;
      gameObject1.transform.localEulerAngles = new Vector3(0.0f, 0.0f, (float) -((double) index - 0.5) * this.degreesPerActionMain);
      RadialMenuAction radialMenuAction = this.allowedActionsMain[index];
      GameObject gameObject2 = UnityEngine.Object.Instantiate<GameObject>(this.actionPrefab, this.actionsMainContainer.transform);
      Image component2 = gameObject2.GetComponent<Image>();
      Text component3 = gameObject2.transform.Find("Text").GetComponent<Text>();
      this.allowedActionsMain[index].Setup(component1, component2, component3);
      gameObject2.transform.localPosition = 90f * new Vector3(Mathf.Sin((float) ((double) index * (double) this.degreesPerActionMain * (Math.PI / 180.0))), Mathf.Cos((float) ((double) index * (double) this.degreesPerActionMain * (Math.PI / 180.0))), 0.0f);
      this.actionObjectsMain.Add(gameObject2);
      this.actionObjectsMain.Add(gameObject1);
    }
  }

  private void SetupWeapons()
  {
    this.showWeaponWheel = false;
    this.allowedActionsWeapons.Clear();
    foreach (UnityEngine.Object actionObjectsWeapon in this.actionObjectsWeapons)
      UnityEngine.Object.Destroy(actionObjectsWeapon);
    this.actionObjectsWeapons.Clear();
    foreach (RadialMenuAction actionsWeapon in this.actionsWeapons)
    {
      if (actionsWeapon.AllowedOnAircraft(this.aircraft))
      {
        this.allowedActionsWeapons.Add(actionsWeapon);
        this.showWeaponWheel = true;
      }
    }
    foreach (WeaponStation weaponStation in this.aircraft.weaponStations)
    {
      RadialMenuAction radialMenuAction = UnityEngine.Object.Instantiate<RadialMenuAction>(this.actionWeaponPrefab);
      radialMenuAction.SetWeapon(weaponStation.WeaponInfo, (int) weaponStation.Number);
      this.allowedActionsWeapons.Add(radialMenuAction);
    }
    if (this.allowedActionsWeapons.Count > 2)
      this.showWeaponWheel = true;
    this.degreesPerActionWeapons = 360f / (float) this.allowedActionsWeapons.Count;
    for (int index = 0; index < this.allowedActionsWeapons.Count; ++index)
    {
      GameObject gameObject1 = UnityEngine.Object.Instantiate<GameObject>(this.sectorObject, this.actionsWeaponsContainer.transform);
      Image component1 = gameObject1.GetComponent<Image>();
      component1.fillAmount = this.degreesPerActionWeapons / 360f;
      gameObject1.transform.localEulerAngles = new Vector3(0.0f, 0.0f, (float) -((double) index - 0.5) * this.degreesPerActionWeapons);
      RadialMenuAction allowedActionsWeapon = this.allowedActionsWeapons[index];
      GameObject gameObject2 = UnityEngine.Object.Instantiate<GameObject>(this.actionPrefab, this.actionsWeaponsContainer.transform);
      Image component2 = gameObject2.GetComponent<Image>();
      Text component3 = gameObject2.transform.Find("Text").GetComponent<Text>();
      this.allowedActionsWeapons[index].Setup(component1, component2, component3);
      gameObject2.transform.localPosition = 90f * new Vector3(Mathf.Sin((float) ((double) index * (double) this.degreesPerActionWeapons * (Math.PI / 180.0))), Mathf.Cos((float) ((double) index * (double) this.degreesPerActionWeapons * (Math.PI / 180.0))), 0.0f);
      this.actionObjectsWeapons.Add(gameObject2);
      this.actionObjectsWeapons.Add(gameObject1);
    }
  }

  public void RefreshWeapons()
  {
    for (int index = 0; index < this.allowedActionsWeapons.Count; ++index)
    {
      RadialMenuAction allowedActionsWeapon = this.allowedActionsWeapons[index];
      if (allowedActionsWeapon.GetActionType() == RadialMenuAction.ActionType.SelectWeapon && !this.aircraft.weaponStations[allowedActionsWeapon.weapon_number].WeaponInfo.sling && !this.aircraft.weaponStations[allowedActionsWeapon.weapon_number].WeaponInfo.energy)
        allowedActionsWeapon.RefreshWeapon(this.aircraft.weaponStations[allowedActionsWeapon.weapon_number].GetAmmoReadout(), this.aircraft.weaponStations[allowedActionsWeapon.weapon_number].GetAmmoLevel());
    }
  }

  private void Update()
  {
    if (this.currentState != RadialMenuMain.RadialMenuType.Closed)
    {
      if ((double) Time.realtimeSinceStartup - (double) this.lastOpen > 0.05000000074505806 && (double) this.canvasGroup.alpha < 1.0)
        this.canvasGroup.alpha += 10f * Time.deltaTime;
      if (!this.menuObject.activeSelf)
        this.menuObject.SetActive(true);
      switch (this.currentState)
      {
        case RadialMenuMain.RadialMenuType.Main:
          if (!this.actionsMainContainer.activeSelf)
            this.actionsMainContainer.SetActive(true);
          if (this.actionsWeaponsContainer.activeSelf)
            this.actionsWeaponsContainer.SetActive(false);
          this.CheckAction(this.allowedActionsMain, this.degreesPerActionMain, "Radial Menu");
          break;
        case RadialMenuMain.RadialMenuType.Weapons:
          if (this.actionsMainContainer.activeSelf)
            this.actionsMainContainer.SetActive(false);
          if (!this.actionsWeaponsContainer.activeSelf)
            this.actionsWeaponsContainer.SetActive(true);
          this.CheckAction(this.allowedActionsWeapons, this.degreesPerActionWeapons, "Weapon Wheel");
          break;
      }
    }
    else
    {
      if (this.playerInput.GetButtonTimedPressDown("Radial Menu", PlayerSettings.pressDelay))
        this.currentState = RadialMenuMain.RadialMenuType.Main;
      else if (this.playerInput.GetButtonTimedPressDown("Weapon Wheel", PlayerSettings.pressDelay))
        this.currentState = RadialMenuMain.RadialMenuType.Weapons;
      if (this.currentState != RadialMenuMain.RadialMenuType.Closed)
        this.OpenMenu();
      if (this.currentState == RadialMenuMain.RadialMenuType.Weapons && !this.showWeaponWheel)
        this.currentState = RadialMenuMain.RadialMenuType.Closed;
      if ((double) this.canvasGroup.alpha > 0.0)
      {
        this.canvasGroup.alpha -= 5f * Time.deltaTime;
      }
      else
      {
        if (!this.menuObject.activeSelf)
          return;
        this.menuObject.SetActive(false);
      }
    }
  }

  private async UniTask FlashSelection(RadialMenuAction action)
  {
    RadialMenuMain radialMenuMain = this;
    float time = 0.0f;
    CancellationToken cancel = radialMenuMain.destroyCancellationToken;
    while ((double) time < 2.0)
    {
      time += Time.deltaTime;
      action.Flash();
      await UniTask.Yield();
      if (cancel.IsCancellationRequested)
      {
        cancel = new CancellationToken();
        return;
      }
    }
    cancel = new CancellationToken();
  }

  private void CheckAction(
    List<RadialMenuAction> allowedActions,
    float degreesPerAction,
    string buttonName)
  {
    if ((UnityEngine.Object) this.aircraft == (UnityEngine.Object) null)
      this.CloseMenu();
    else if (PlayerSettings.radialControl < 2)
      this.ControlAxis(allowedActions, degreesPerAction, buttonName);
    else
      this.ControlButtons(allowedActions, degreesPerAction, buttonName);
  }

  private void ControlAxis(
    List<RadialMenuAction> allowedActions,
    float degreesPerAction,
    string buttonName)
  {
    if (!this.playerInput.GetButton(buttonName))
    {
      if ((UnityEngine.Object) this.selectedAction != (UnityEngine.Object) null)
      {
        this.selectedAction.UnHover();
        this.selectedAction.TriggerAction(this.aircraft);
        this.FlashSelection(this.selectedAction).Forget();
      }
      this.CloseMenu();
    }
    else
    {
      this.mouseDelta += (this.playerInput.GetAxis("Pan View") * Vector3.right - this.playerInput.GetAxis("Tilt View") * Vector3.up) * 0.5f;
      this.mouseDelta = Vector3.Lerp(this.mouseDelta, Vector3.zero, 0.05f);
      Vector2 vector2 = new Vector2(PlayerSettings.radialControl == 0 ? this.playerInput.GetAxis("Radial Menu Horizontal") : this.mouseDelta.x, PlayerSettings.radialControl == 0 ? -this.playerInput.GetAxis("Radial Menu Vertical") : this.mouseDelta.y);
      if ((double) vector2.sqrMagnitude > 0.10000000149011612)
      {
        this.lastSelection = Time.realtimeSinceStartup;
        float num = -Vector2.SignedAngle(Vector2.up, vector2.normalized);
        if ((double) num < 0.0)
          num += 360f;
        int index = Mathf.Clamp(Mathf.FloorToInt(Mathf.Repeat(num + degreesPerAction * 0.5f, 360f) / degreesPerAction), 0, allowedActions.Count - 1);
        RadialMenuAction allowedAction = allowedActions[index];
        if (!((UnityEngine.Object) allowedAction != (UnityEngine.Object) this.selectedAction))
          return;
        if ((UnityEngine.Object) this.selectedAction != (UnityEngine.Object) null)
          this.selectedAction.UnHover();
        this.selectedAction = allowedAction;
        this.selectedAction.Hover();
        this.selectedActionText.text = this.selectedAction.DisplayName;
      }
      else
      {
        if ((double) Time.realtimeSinceStartup - (double) this.lastSelection <= 0.05000000074505806 || !((UnityEngine.Object) this.selectedAction != (UnityEngine.Object) null))
          return;
        this.selectedAction.UnHover();
        this.selectedAction = (RadialMenuAction) null;
        this.selectedActionText.text = "";
      }
    }
  }

  private void ControlButtons(
    List<RadialMenuAction> allowedActions,
    float degreesPerAction,
    string buttonName)
  {
    if (!this.playerInput.GetButton(buttonName))
    {
      if ((UnityEngine.Object) this.selectedAction != (UnityEngine.Object) null)
      {
        this.selectedAction.UnHover();
        this.selectedAction.TriggerAction(this.aircraft);
        this.FlashSelection(this.selectedAction).Forget();
      }
      this.CloseMenu();
    }
    else
    {
      bool buttonDown1 = this.playerInput.GetButtonDown("Radial Menu Horizontal");
      bool negativeButtonDown1 = this.playerInput.GetNegativeButtonDown("Radial Menu Horizontal");
      bool buttonDown2 = this.playerInput.GetButtonDown("Radial Menu Vertical");
      bool negativeButtonDown2 = this.playerInput.GetNegativeButtonDown("Radial Menu Vertical");
      if (buttonDown1 | negativeButtonDown1 | buttonDown2 | negativeButtonDown2)
      {
        this.lastSelection = Time.realtimeSinceStartup;
        if (this.currentIndex < 0)
        {
          if (buttonDown2)
            this.currentIndex = 0;
          else if (negativeButtonDown2)
            this.currentIndex = Mathf.RoundToInt(0.5f * (float) allowedActions.Count);
          if (buttonDown1)
            this.currentIndex = Mathf.RoundToInt(0.25f * (float) allowedActions.Count);
          else if (negativeButtonDown1)
            this.currentIndex = Mathf.RoundToInt(0.75f * (float) allowedActions.Count);
        }
        float num = (float) this.currentIndex * degreesPerAction;
        if ((double) num > 225.0 && (double) num < 315.0)
        {
          if (buttonDown2)
            ++this.currentIndex;
          else if (negativeButtonDown2)
            --this.currentIndex;
        }
        else if ((double) num > 135.0)
        {
          if (buttonDown1)
            --this.currentIndex;
          else if (negativeButtonDown1)
            ++this.currentIndex;
        }
        else if ((double) num > 45.0)
        {
          if (buttonDown2)
            --this.currentIndex;
          else if (negativeButtonDown2)
            ++this.currentIndex;
        }
        else if (buttonDown1)
          ++this.currentIndex;
        else if (negativeButtonDown1)
          --this.currentIndex;
        if (this.currentIndex < 0)
          this.currentIndex = allowedActions.Count - 1;
        else if (this.currentIndex > allowedActions.Count - 1)
          this.currentIndex = 0;
        RadialMenuAction allowedAction = allowedActions[this.currentIndex];
        if (!((UnityEngine.Object) allowedAction != (UnityEngine.Object) this.selectedAction))
          return;
        if ((UnityEngine.Object) this.selectedAction != (UnityEngine.Object) null)
          this.selectedAction.UnHover();
        this.selectedAction = allowedAction;
        this.selectedAction.Hover();
        this.selectedActionText.text = this.selectedAction.DisplayName;
      }
      else
      {
        if ((double) Time.realtimeSinceStartup - (double) this.lastSelection <= 0.5 || !((UnityEngine.Object) this.selectedAction != (UnityEngine.Object) null))
          return;
        this.currentIndex = -1;
        this.selectedAction.UnHover();
        this.selectedAction = (RadialMenuAction) null;
        this.selectedActionText.text = "";
      }
    }
  }

  private void OnDestroy()
  {
    this.CloseMenu();
    this.aircraft = (Aircraft) null;
    this.actionObjectsMain.Clear();
    this.actionObjectsWeapons.Clear();
  }

  public enum RadialMenuType
  {
    Closed,
    Main,
    Weapons,
  }
}
