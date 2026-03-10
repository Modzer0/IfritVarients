// Decompiled with JetBrains decompiler
// Type: CameraControlUI
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;
using UnityEngine.UI;

#nullable disable
public class CameraControlUI : SceneSingleton<CameraControlUI>
{
  public static CameraStateManager cam;
  [Header("Main Container")]
  [SerializeField]
  private GameObject container;
  [Header("Camera States")]
  [SerializeField]
  private GameObject StatesPanel;
  [SerializeField]
  private Button cockpitButton;
  [SerializeField]
  private Button orbitButton;
  [SerializeField]
  private Button chaseButton;
  [SerializeField]
  private Button controlledButton;
  [SerializeField]
  private Button freeButton;
  [SerializeField]
  private Button flybyButton;
  private Image cockpitButtonImg;
  private Image orbitButtonImg;
  private Image chaseButtonImg;
  private Image controlledButtonImg;
  private Image freeButtonImg;
  private Image flybyButtonImg;
  [Header("Field of View")]
  [SerializeField]
  private GameObject FOVPanel;
  [SerializeField]
  private Slider camFOV;
  [SerializeField]
  private Slider camFOVSpeed;
  [SerializeField]
  private Slider camFOVInertia;
  [SerializeField]
  private Text camFOVValue;
  [SerializeField]
  private Text camFOVSpeedValue;
  [SerializeField]
  private Text camFOVInertiaValue;
  [Header("Inputs")]
  [SerializeField]
  private GameObject InputsPanel;
  [SerializeField]
  private Slider camTransSpeed;
  [SerializeField]
  private Slider camRotSpeed;
  [SerializeField]
  private Toggle camInputToggle;
  [SerializeField]
  private Text camTransSpeedValue;
  [SerializeField]
  private Text camRotSpeedValue;
  [Header("Time Factor")]
  [SerializeField]
  private GameObject TimePanel;
  [SerializeField]
  private Slider timeFactor;
  [SerializeField]
  private Text timeFactorValue;
  [Header("Pivot")]
  [SerializeField]
  private GameObject PivotPanel;
  [SerializeField]
  private Text pivotParent;
  [SerializeField]
  private Text pivotUp;
  [SerializeField]
  private InputField pivotPosX;
  [SerializeField]
  private InputField pivotPosY;
  [SerializeField]
  private InputField pivotPosZ;
  [SerializeField]
  private InputField pivotAngleX;
  [SerializeField]
  private InputField pivotAngleY;
  [SerializeField]
  private InputField pivotAngleZ;
  [Header("Camera Transform")]
  [SerializeField]
  private GameObject CameraPanel;
  [SerializeField]
  private Text camTarget;
  [SerializeField]
  private InputField camPosX;
  [SerializeField]
  private InputField camPosY;
  [SerializeField]
  private InputField camPosZ;
  [SerializeField]
  private InputField camAngleX;
  [SerializeField]
  private InputField camAngleY;
  [SerializeField]
  private InputField camAngleZ;
  [Header("Movement")]
  [SerializeField]
  private GameObject MovementPanel;
  [SerializeField]
  private InputField camTranslationX;
  [SerializeField]
  private InputField camTranslationY;
  [SerializeField]
  private InputField camTranslationZ;
  [SerializeField]
  private InputField camRotationX;
  [SerializeField]
  private InputField camRotationY;
  [SerializeField]
  private InputField camRotationZ;
  [Header("Current Values")]
  private Vector3 pivotPos = Vector3.zero;
  private Vector3 pivotAngle = Vector3.zero;
  private Vector3 camPos = Vector3.zero;
  private Vector3 camAngle = Vector3.zero;
  private Vector3 camTranslation = Vector3.zero;
  private Vector3 camRotation = Vector3.zero;
  public bool isOpen;
  public bool isMoving;

  private void Start()
  {
    CameraControlUI.cam = SceneSingleton<CameraStateManager>.i;
    this.camFOV.value = CameraControlUI.cam.desiredFOV;
    this.camFOVSpeed.value = 0.5f;
    this.camFOVInertia.value = 0.5f;
    this.camTransSpeed.value = 1f;
    this.camRotSpeed.value = 1f;
    this.camInputToggle.isOn = true;
    CameraControlUI.cam.onSwitchCamera += new Action(this.SetCamState);
    this.camPosX.contentType = InputField.ContentType.DecimalNumber;
    this.camPosY.contentType = InputField.ContentType.DecimalNumber;
    this.camPosZ.contentType = InputField.ContentType.DecimalNumber;
    this.camAngleX.contentType = InputField.ContentType.DecimalNumber;
    this.camAngleY.contentType = InputField.ContentType.DecimalNumber;
    this.camAngleZ.contentType = InputField.ContentType.DecimalNumber;
    this.camTranslationX.contentType = InputField.ContentType.DecimalNumber;
    this.camTranslationY.contentType = InputField.ContentType.DecimalNumber;
    this.camTranslationZ.contentType = InputField.ContentType.DecimalNumber;
    this.container.SetActive(false);
    this.isOpen = false;
    this.isMoving = false;
    this.cockpitButtonImg = this.cockpitButton.GetComponent<Image>();
    this.orbitButtonImg = this.orbitButton.GetComponent<Image>();
    this.freeButtonImg = this.freeButton.GetComponent<Image>();
    this.flybyButtonImg = this.flybyButton.GetComponent<Image>();
    this.chaseButtonImg = this.chaseButton.GetComponent<Image>();
    this.controlledButtonImg = this.controlledButton.GetComponent<Image>();
  }

  public void Update()
  {
    if (SceneSingleton<CameraStateManager>.i.currentState != SceneSingleton<CameraStateManager>.i.selectionState && GameManager.playerInput.GetButtonTimedPressDown("Switch View", PlayerSettings.pressDelay + 0.2f))
    {
      if (!this.isOpen)
        this.OpenMenu();
      else
        this.CloseMenu();
    }
    if (this.isOpen && Input.GetKeyDown(KeyCode.Escape))
      this.CloseMenu();
    if (!Input.GetKeyDown(KeyCode.Space))
      return;
    if (this.isMoving)
      this.PauseMovement();
    else
      this.ApplyMovement();
  }

  public void OpenMenu()
  {
    this.container.SetActive(true);
    this.SetCamState();
    if (GameManager.gameState == GameState.SinglePlayer)
      this.PauseTime();
    CursorManager.SetFlag(CursorFlags.Pause, true);
    this.isOpen = true;
  }

  public void CloseMenu()
  {
    CursorManager.SetFlag(CursorFlags.Pause, false);
    this.container.SetActive(false);
    this.isOpen = false;
  }

  public void ApplyTimeFactor()
  {
    this.timeFactor.value = Mathf.Round(100f * this.timeFactor.value) / 100f;
    Time.timeScale = this.timeFactor.value;
  }

  public void PauseTime()
  {
    this.timeFactor.value = 0.0f;
    Time.timeScale = this.timeFactor.value;
  }

  public void PlayTime()
  {
    this.timeFactor.value = 1f;
    Time.timeScale = this.timeFactor.value;
  }

  public void ResetFOV()
  {
    CameraControlUI.cam.SetDesiredFoV(PlayerSettings.defaultFoV, 0.0f);
    this.camFOVSpeed.value = 0.5f;
    this.camFOVInertia.value = 0.5f;
    CameraControlUI.cam.fovChangeSpeed = this.camFOVSpeed.value;
    CameraControlUI.cam.fovChangeInertia = this.camFOVInertia.value;
  }

  public void ApplyFOV() => CameraControlUI.cam.SetDesiredFoV(this.camFOV.value, 0.0f);

  public void ApplyFOVSpeed()
  {
    CameraControlUI.cam.fovChangeSpeed = Mathf.Clamp(this.camFOVSpeed.value, 1f / 1000f, 1f);
  }

  public void ApplyFOVInertia()
  {
    CameraControlUI.cam.fovChangeInertia = Mathf.Clamp(this.camFOVInertia.value, 0.0f, 1f);
  }

  public void ApplyInputSpeed()
  {
    CameraControlUI.cam.SetDesiredSpeed(this.camTransSpeed.value, this.camRotSpeed.value);
  }

  public void ApplyInputsToggle() => CameraControlUI.cam.allowInputs = this.camInputToggle.isOn;

  public void ApplyCamPos()
  {
    float result1;
    float result2;
    float result3;
    if ((0 | (float.TryParse(this.camPosX.text, out result1) ? 1 : 0) | (float.TryParse(this.camPosY.text, out result2) ? 1 : 0) | (float.TryParse(this.camPosZ.text, out result3) ? 1 : 0)) == 0)
      return;
    this.camPos = new Vector3(result1, result2, result3);
    this.camPosX.text = $"{result1:F2}";
    this.camPosY.text = $"{result2:F2}";
    this.camPosZ.text = $"{result3:F2}";
    if (CameraControlUI.cam.currentState == CameraControlUI.cam.controlledState)
      CameraControlUI.cam.controlledState.SetCustomTransform(this.camPos, this.camAngle);
    else if (CameraControlUI.cam.currentState == CameraControlUI.cam.chaseState)
      CameraControlUI.cam.chaseState.SetCustomTransform(this.camPos, this.camAngle);
    else if (CameraControlUI.cam.currentState == CameraControlUI.cam.TVState)
      CameraControlUI.cam.TVState.SetCustomTransform(this.camPos, this.camAngle);
    this.GetValues();
  }

  public void ApplyCamAngle()
  {
    float result1;
    float result2;
    float result3;
    if ((0 | (float.TryParse(this.camAngleX.text, out result1) ? 1 : 0) | (float.TryParse(this.camAngleY.text, out result2) ? 1 : 0) | (float.TryParse(this.camAngleZ.text, out result3) ? 1 : 0)) == 0)
      return;
    this.camAngle = new Vector3(result1, result2, result3);
    this.camAngleX.text = $"{result1:F0}";
    this.camAngleY.text = $"{result2:F0}";
    this.camAngleZ.text = $"{result3:F0}";
    if (CameraControlUI.cam.currentState == CameraControlUI.cam.controlledState)
      CameraControlUI.cam.controlledState.SetCustomTransform(this.camPos, this.camAngle);
    else if (CameraControlUI.cam.currentState == CameraControlUI.cam.chaseState)
      CameraControlUI.cam.chaseState.SetCustomTransform(this.camPos, this.camAngle);
    this.GetValues();
  }

  public void ToggleCameraTarget()
  {
    if ((UnityEngine.Object) CameraControlUI.cam.followingUnit == (UnityEngine.Object) null || CameraControlUI.cam.currentState != CameraControlUI.cam.controlledState)
      return;
    Transform transform = CameraControlUI.cam.controlledState.IsLookingAt() ? (Transform) null : CameraControlUI.cam.followingUnit.transform;
    CameraControlUI.cam.controlledState.SetLookAt(transform);
    this.camTarget.text = (UnityEngine.Object) transform != (UnityEngine.Object) null ? CameraControlUI.cam.followingUnit.UniqueName : "-";
    this.GetValues();
  }

  public void ApplyPivotPos()
  {
    float result1;
    float result2;
    float result3;
    if ((0 | (float.TryParse(this.pivotPosX.text, out result1) ? 1 : 0) | (float.TryParse(this.pivotPosY.text, out result2) ? 1 : 0) | (float.TryParse(this.pivotPosZ.text, out result3) ? 1 : 0)) == 0)
      return;
    this.pivotPos = new Vector3(result1, result2, result3);
    this.pivotPosX.text = $"{result1:F2}";
    this.pivotPosY.text = $"{result2:F2}";
    this.pivotPosZ.text = $"{result3:F2}";
    if (CameraControlUI.cam.currentState == CameraControlUI.cam.controlledState)
      CameraControlUI.cam.controlledState.SetCustomPivot(this.pivotPos, this.pivotAngle);
    this.GetValues();
  }

  public void ApplyPivotAngle()
  {
    float result1;
    float result2;
    float result3;
    if ((0 | (float.TryParse(this.pivotAngleX.text, out result1) ? 1 : 0) | (float.TryParse(this.pivotAngleY.text, out result2) ? 1 : 0) | (float.TryParse(this.pivotAngleZ.text, out result3) ? 1 : 0)) == 0)
      return;
    this.pivotAngle = new Vector3(result1, result2, result3);
    this.pivotAngleX.text = $"{result1:F2}";
    this.pivotAngleY.text = $"{result2:F2}";
    this.pivotAngleZ.text = $"{result3:F2}";
    if (CameraControlUI.cam.currentState == CameraControlUI.cam.controlledState)
      CameraControlUI.cam.controlledState.SetCustomPivot(this.pivotPos, this.pivotAngle);
    this.GetValues();
  }

  public void ReturnPivotToUnit()
  {
    if ((UnityEngine.Object) CameraControlUI.cam.followingUnit == (UnityEngine.Object) null)
      return;
    this.pivotPos = Vector3.zero;
    this.pivotPosX.text = $"{0:F2}";
    this.pivotPosY.text = $"{0:F2}";
    this.pivotPosZ.text = $"{0:F2}";
    this.pivotAngle = Vector3.zero;
    this.pivotAngleX.text = $"{0:F2}";
    this.pivotAngleY.text = $"{0:F2}";
    this.pivotAngleZ.text = $"{0:F2}";
    CameraControlUI.cam.controlledState.SetCustomPivot(Vector3.zero, Vector3.zero);
  }

  public void SwitchPivotUp()
  {
    if ((UnityEngine.Object) CameraControlUI.cam.followingUnit == (UnityEngine.Object) null)
      return;
    CameraControlUI.cam.controlledState.SwitchPivotUp();
    this.pivotUp.text = CameraControlUI.cam.controlledState.GetPivotUp();
  }

  public void ApplyMovement()
  {
    float result1;
    float result2;
    float result3;
    float result4;
    float result5;
    float result6;
    if ((0 | (float.TryParse(this.camTranslationX.text, out result1) ? 1 : 0) | (float.TryParse(this.camTranslationY.text, out result2) ? 1 : 0) | (float.TryParse(this.camTranslationZ.text, out result3) ? 1 : 0) | (float.TryParse(this.camRotationX.text, out result4) ? 1 : 0) | (float.TryParse(this.camRotationY.text, out result5) ? 1 : 0) | (float.TryParse(this.camRotationZ.text, out result6) ? 1 : 0)) == 0)
      return;
    this.camTranslation = new Vector3(result1, result2, result3);
    this.camTranslationX.text = $"{result1:F0}";
    this.camTranslationY.text = $"{result2:F0}";
    this.camTranslationZ.text = $"{result3:F0}";
    this.camRotation = new Vector3(result4, result5, result6);
    this.camRotationX.text = $"{result4:F0}";
    this.camRotationY.text = $"{result5:F0}";
    this.camRotationZ.text = $"{result6:F0}";
    if ((double) this.camTranslation.magnitude == 0.0 && (double) this.camRotation.magnitude == 0.0)
      return;
    if (CameraControlUI.cam.currentState == CameraControlUI.cam.controlledState)
    {
      CameraControlUI.cam.controlledState.SetCustomMovement(this.camTranslation, this.camRotation, false);
      this.isMoving = true;
    }
    this.GetValues();
  }

  public void PauseMovement()
  {
    if (CameraControlUI.cam.currentState != CameraControlUI.cam.controlledState)
      return;
    CameraControlUI.cam.controlledState.SetCustomMovement(Vector3.zero, Vector3.zero, false);
    this.isMoving = false;
  }

  public void CancelMovement()
  {
    if (CameraControlUI.cam.currentState != CameraControlUI.cam.controlledState)
      return;
    CameraControlUI.cam.controlledState.SetCustomMovement(Vector3.zero, Vector3.zero, true);
    this.isMoving = false;
  }

  public void ReverseMovement()
  {
    float result1;
    float result2;
    float result3;
    float result4;
    float result5;
    float result6;
    if ((0 | (float.TryParse(this.camTranslationX.text, out result1) ? 1 : 0) | (float.TryParse(this.camTranslationY.text, out result2) ? 1 : 0) | (float.TryParse(this.camTranslationZ.text, out result3) ? 1 : 0) | (float.TryParse(this.camRotationX.text, out result4) ? 1 : 0) | (float.TryParse(this.camRotationY.text, out result5) ? 1 : 0) | (float.TryParse(this.camRotationZ.text, out result6) ? 1 : 0)) == 0 || (double) this.camTranslation.magnitude == 0.0 && (double) this.camRotation.magnitude == 0.0)
      return;
    this.camTranslation = new Vector3(result1, result2, result3);
    this.camTranslationX.text = $"{result1:F0}";
    this.camTranslationY.text = $"{result2:F0}";
    this.camTranslationZ.text = $"{result3:F0}";
    this.camRotation = new Vector3(result4, result5, result6);
    this.camRotationX.text = $"{result4:F0}";
    this.camRotationY.text = $"{result5:F0}";
    this.camRotationZ.text = $"{result6:F0}";
    this.camTranslation = -this.camTranslation;
    this.camRotation = -this.camRotation;
    if (CameraControlUI.cam.currentState != CameraControlUI.cam.controlledState)
      return;
    CameraControlUI.cam.controlledState.SetCustomMovement(this.camTranslation, this.camRotation, false);
    this.isMoving = true;
  }

  public void GetValues()
  {
    if ((UnityEngine.Object) CameraControlUI.cam.followingUnit != (UnityEngine.Object) null)
    {
      this.pivotParent.text = CameraControlUI.cam.followingUnit.UniqueName;
      this.pivotPos = CameraControlUI.cam.cameraPivot.localPosition;
      this.pivotAngle = CameraControlUI.cam.cameraPivot.localEulerAngles;
    }
    else
    {
      this.pivotParent.text = "-";
      this.pivotPos = CameraControlUI.cam.cameraPivot.position;
      this.pivotAngle = CameraControlUI.cam.cameraPivot.eulerAngles;
    }
    this.camPos = CameraControlUI.cam.transform.localPosition;
    this.camAngle = CameraControlUI.cam.transform.localEulerAngles;
    this.camFOV.SetValueWithoutNotify(CameraControlUI.cam.mainCamera.fieldOfView);
    this.camFOVSpeed.SetValueWithoutNotify(CameraControlUI.cam.fovChangeSpeed);
    this.camFOVInertia.SetValueWithoutNotify(CameraControlUI.cam.fovChangeInertia);
    this.timeFactor.value = Time.timeScale;
    this.UpdateValues();
  }

  public void UpdateValues()
  {
    this.timeFactorValue.text = $"{this.timeFactor.value:F1}";
    this.camFOVValue.text = $"{this.camFOV.value:F0}";
    this.camFOVSpeedValue.text = $"{(ValueType) (float) (100.0 * (double) this.camFOVSpeed.value):F0}%";
    this.camFOVInertiaValue.text = $"{(ValueType) (float) (100.0 * (double) this.camFOVInertia.value):F0}%";
    this.camTransSpeedValue.text = $"{this.camTransSpeed.value:F1}";
    this.camRotSpeedValue.text = $"{this.camRotSpeed.value:F1}";
    this.pivotParent.text = (UnityEngine.Object) CameraControlUI.cam.followingUnit != (UnityEngine.Object) null ? CameraControlUI.cam.followingUnit.UniqueName : "-";
    this.pivotPosX.SetTextWithoutNotify($"{this.pivotPos.x:F2}");
    this.pivotPosY.SetTextWithoutNotify($"{this.pivotPos.y:F2}");
    this.pivotPosZ.SetTextWithoutNotify($"{this.pivotPos.z:F2}");
    this.pivotAngleX.SetTextWithoutNotify($"{this.pivotAngle.x:F0}");
    this.pivotAngleY.SetTextWithoutNotify($"{this.pivotAngle.y:F0}");
    this.pivotAngleZ.SetTextWithoutNotify($"{this.pivotAngle.z:F0}");
    this.camPosX.SetTextWithoutNotify($"{this.camPos.x:F2}");
    this.camPosY.SetTextWithoutNotify($"{this.camPos.y:F2}");
    this.camPosZ.SetTextWithoutNotify($"{this.camPos.z:F2}");
    this.camAngleX.SetTextWithoutNotify($"{this.camAngle.x:F0}");
    this.camAngleY.SetTextWithoutNotify($"{this.camAngle.y:F0}");
    this.camAngleZ.SetTextWithoutNotify($"{this.camAngle.z:F0}");
  }

  public void SetCamFree()
  {
    CameraControlUI.cam.SwitchState((CameraBaseState) CameraControlUI.cam.freeState);
  }

  public void SetCamCockpit()
  {
    if ((UnityEngine.Object) CameraControlUI.cam.followingUnit == (UnityEngine.Object) null || (UnityEngine.Object) CameraControlUI.cam.followingUnit.cockpitViewPoint == (UnityEngine.Object) null)
      return;
    CameraControlUI.cam.SwitchState((CameraBaseState) CameraControlUI.cam.cockpitState);
  }

  public void SetCamOrbit()
  {
    if ((UnityEngine.Object) CameraControlUI.cam.followingUnit == (UnityEngine.Object) null)
      return;
    CameraControlUI.cam.SwitchState((CameraBaseState) CameraControlUI.cam.orbitState);
  }

  public void SetCamChase()
  {
    if ((UnityEngine.Object) CameraControlUI.cam.followingUnit == (UnityEngine.Object) null)
      return;
    CameraControlUI.cam.SwitchState((CameraBaseState) CameraControlUI.cam.chaseState);
  }

  public void SetCamControlled()
  {
    CameraControlUI.cam.SwitchState((CameraBaseState) CameraControlUI.cam.controlledState);
  }

  public void SetCamFlyby()
  {
    if ((UnityEngine.Object) CameraControlUI.cam.followingUnit == (UnityEngine.Object) null)
      return;
    CameraControlUI.cam.SwitchState((CameraBaseState) CameraControlUI.cam.TVState);
  }

  public void SetCamState()
  {
    if (CameraControlUI.cam.currentState == CameraControlUI.cam.cockpitState)
    {
      this.cockpitButtonImg.color = Color.green;
      this.orbitButtonImg.color = Color.white;
      this.chaseButtonImg.color = Color.white;
      this.controlledButtonImg.color = Color.white;
      this.freeButtonImg.color = Color.white;
      this.flybyButtonImg.color = Color.white;
      this.InputsPanel.gameObject.SetActive(false);
      this.FOVPanel.gameObject.SetActive(true);
      this.TimePanel.gameObject.SetActive(GameManager.gameState == GameState.SinglePlayer);
      this.PivotPanel.gameObject.SetActive(false);
      this.CameraPanel.gameObject.SetActive(false);
      this.MovementPanel.gameObject.SetActive(false);
    }
    else if (CameraControlUI.cam.currentState == CameraControlUI.cam.orbitState)
    {
      this.cockpitButtonImg.color = Color.white;
      this.orbitButtonImg.color = Color.green;
      this.chaseButtonImg.color = Color.white;
      this.controlledButtonImg.color = Color.white;
      this.freeButtonImg.color = Color.white;
      this.flybyButtonImg.color = Color.white;
      this.InputsPanel.gameObject.SetActive(false);
      this.FOVPanel.gameObject.SetActive(true);
      this.TimePanel.gameObject.SetActive(GameManager.gameState == GameState.SinglePlayer);
      this.PivotPanel.gameObject.SetActive(false);
      this.CameraPanel.gameObject.SetActive(false);
      this.MovementPanel.gameObject.SetActive(false);
      this.camTarget.text = CameraControlUI.cam.followingUnit.UniqueName;
    }
    else if (CameraControlUI.cam.currentState == CameraControlUI.cam.chaseState)
    {
      this.cockpitButtonImg.color = Color.white;
      this.orbitButtonImg.color = Color.white;
      this.chaseButtonImg.color = Color.green;
      this.controlledButtonImg.color = Color.white;
      this.freeButtonImg.color = Color.white;
      this.flybyButtonImg.color = Color.white;
      this.InputsPanel.gameObject.SetActive(false);
      this.FOVPanel.gameObject.SetActive(true);
      this.TimePanel.gameObject.SetActive(GameManager.gameState == GameState.SinglePlayer);
      this.PivotPanel.gameObject.SetActive(false);
      this.CameraPanel.gameObject.SetActive(true);
      this.MovementPanel.gameObject.SetActive(false);
      this.camTarget.text = CameraControlUI.cam.followingUnit.UniqueName;
      this.pivotUp.text = "U";
    }
    else if (CameraControlUI.cam.currentState == CameraControlUI.cam.controlledState)
    {
      this.cockpitButtonImg.color = Color.white;
      this.orbitButtonImg.color = Color.white;
      this.chaseButtonImg.color = Color.white;
      this.controlledButtonImg.color = Color.green;
      this.freeButtonImg.color = Color.white;
      this.flybyButtonImg.color = Color.white;
      this.InputsPanel.gameObject.SetActive(true);
      this.FOVPanel.gameObject.SetActive(true);
      this.TimePanel.gameObject.SetActive(GameManager.gameState == GameState.SinglePlayer);
      this.PivotPanel.gameObject.SetActive(true);
      this.CameraPanel.gameObject.SetActive(true);
      this.MovementPanel.gameObject.SetActive(true);
      this.camTarget.text = CameraControlUI.cam.controlledState.IsLookingAt() ? CameraControlUI.cam.followingUnit.UniqueName : "-";
      this.pivotUp.text = CameraControlUI.cam.controlledState.GetPivotUp();
    }
    else if (CameraControlUI.cam.currentState == CameraControlUI.cam.freeState)
    {
      this.cockpitButtonImg.color = Color.white;
      this.orbitButtonImg.color = Color.white;
      this.chaseButtonImg.color = Color.white;
      this.controlledButtonImg.color = Color.white;
      this.freeButtonImg.color = Color.green;
      this.flybyButtonImg.color = Color.white;
      this.InputsPanel.gameObject.SetActive(true);
      this.FOVPanel.gameObject.SetActive(true);
      this.TimePanel.gameObject.SetActive(GameManager.gameState == GameState.SinglePlayer);
      this.PivotPanel.gameObject.SetActive(false);
      this.CameraPanel.gameObject.SetActive(false);
      this.MovementPanel.gameObject.SetActive(false);
      this.pivotParent.text = "-";
    }
    else if (CameraControlUI.cam.currentState == CameraControlUI.cam.TVState)
    {
      this.cockpitButtonImg.color = Color.white;
      this.orbitButtonImg.color = Color.white;
      this.chaseButtonImg.color = Color.white;
      this.controlledButtonImg.color = Color.white;
      this.freeButtonImg.color = Color.white;
      this.flybyButtonImg.color = Color.green;
      this.InputsPanel.gameObject.SetActive(false);
      this.FOVPanel.gameObject.SetActive(false);
      this.TimePanel.gameObject.SetActive(GameManager.gameState == GameState.SinglePlayer);
      this.PivotPanel.gameObject.SetActive(false);
      this.CameraPanel.gameObject.SetActive(false);
      this.MovementPanel.gameObject.SetActive(false);
    }
    else
    {
      this.cockpitButtonImg.color = Color.white;
      this.orbitButtonImg.color = Color.white;
      this.chaseButtonImg.color = Color.white;
      this.controlledButtonImg.color = Color.white;
      this.freeButtonImg.color = Color.white;
      this.flybyButtonImg.color = Color.white;
      this.InputsPanel.gameObject.SetActive(false);
      this.FOVPanel.gameObject.SetActive(true);
      this.TimePanel.gameObject.SetActive(GameManager.gameState == GameState.SinglePlayer);
      this.PivotPanel.gameObject.SetActive(false);
      this.CameraPanel.gameObject.SetActive(false);
      this.MovementPanel.gameObject.SetActive(false);
    }
    this.cockpitButton.interactable = (UnityEngine.Object) CameraControlUI.cam.followingUnit != (UnityEngine.Object) null && (UnityEngine.Object) CameraControlUI.cam.followingUnit.cockpitViewPoint != (UnityEngine.Object) null;
    this.chaseButton.interactable = (UnityEngine.Object) CameraControlUI.cam.followingUnit != (UnityEngine.Object) null;
    this.orbitButton.interactable = (UnityEngine.Object) CameraControlUI.cam.followingUnit != (UnityEngine.Object) null;
    this.flybyButton.interactable = (UnityEngine.Object) CameraControlUI.cam.followingUnit != (UnityEngine.Object) null && (UnityEngine.Object) CameraControlUI.cam.followingRB != (UnityEngine.Object) null && (double) CameraControlUI.cam.followingRB.velocity.magnitude > 0.0;
    this.GetValues();
  }
}
