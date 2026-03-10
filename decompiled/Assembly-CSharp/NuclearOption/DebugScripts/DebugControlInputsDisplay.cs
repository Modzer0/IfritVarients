// Decompiled with JetBrains decompiler
// Type: NuclearOption.DebugScripts.DebugControlInputsDisplay
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

#nullable disable
namespace NuclearOption.DebugScripts;

public class DebugControlInputsDisplay : MonoBehaviour
{
  [Header("Settings")]
  [SerializeField]
  private float width = 200f;
  [SerializeField]
  private float height = 20f;
  [SerializeField]
  private float spacing = 2f;
  [SerializeField]
  private Color positiveFillColor = Color.green;
  [SerializeField]
  private Color negativeFillColor = Color.red;
  [SerializeField]
  private Gradient percentColor;
  [Header("References")]
  [SerializeField]
  private GameObject holder;
  [SerializeField]
  private GameObject layout;
  [SerializeField]
  private DebugControlInputsDisplayRow template;
  private DebugControlInputsDisplayRow[] all;
  private ControlInputs controlInputs;
  private Pilot pilot;

  public void Setup(Pilot pilot)
  {
    this.controlInputs = pilot.aircraft.GetInputs();
    this.pilot = pilot;
    this.all = new DebugControlInputsDisplayRow[8];
    for (int index = 0; index < 8; ++index)
    {
      this.all[index] = Object.Instantiate<DebugControlInputsDisplayRow>(this.template, this.layout.transform);
      this.all[index].gameObject.SetActive(true);
    }
    this.template.gameObject.SetActive(false);
    this.all[0].Label.text = "Stuck Low Speed";
    this.all[1].Label.text = "Stuck Yaw Steering";
    this.all[2].Label.text = "Pitch";
    this.all[3].Label.text = "Roll";
    this.all[4].Label.text = "Yaw";
    this.all[5].Label.text = "Throttle";
    this.all[6].Label.text = "Brake";
    this.all[7].Label.text = "Custom Axis 1";
  }

  private void LateUpdate()
  {
    float yPosition = 0.0f;
    if (this.pilot.currentState is AIPilotTaxiState currentState)
    {
      this.all[0].gameObject.SetActive(true);
      this.all[1].gameObject.SetActive(true);
      this.UpdatePercent(this.all[0], currentState.stuckTimerSpeedPercent, ref yPosition);
      this.UpdatePercent(this.all[1], currentState.stuckTimerYawPercent, ref yPosition);
    }
    else
    {
      this.all[0].gameObject.SetActive(false);
      this.all[1].gameObject.SetActive(false);
    }
    this.UpdatePlusMinus(this.all[2], this.controlInputs.pitch, ref yPosition);
    this.UpdatePlusMinus(this.all[3], this.controlInputs.roll, ref yPosition);
    this.UpdatePlusMinus(this.all[4], this.controlInputs.yaw, ref yPosition);
    this.UpdatePlusMinus(this.all[5], this.controlInputs.throttle, ref yPosition);
    this.UpdatePlusMinus(this.all[6], this.controlInputs.brake, ref yPosition);
    this.UpdatePlusMinus(this.all[7], this.controlInputs.customAxis1, ref yPosition);
    ((RectTransform) this.layout.transform).sizeDelta = new Vector2(this.width, this.spacing - yPosition);
  }

  private void UpdatePlusMinus(DebugControlInputsDisplayRow row, float value, ref float yPosition)
  {
    value = Mathf.Clamp(value, -1.5f, 1.5f);
    ((RectTransform) row.transform).anchoredPosition = new Vector2(0.0f, yPosition);
    yPosition -= this.height + this.spacing;
    RectTransform rectTransform = row.Image.rectTransform;
    float x = Mathf.Abs(value) * (this.width / 2f);
    rectTransform.sizeDelta = new Vector2(x, this.height);
    rectTransform.anchoredPosition = (double) value <= 0.0 ? new Vector2(-x, 0.0f) : new Vector2(0.0f, 0.0f);
    row.Image.color = (double) value > 0.0 ? this.positiveFillColor : this.negativeFillColor;
  }

  private void UpdatePercent(DebugControlInputsDisplayRow row, float percent, ref float yPosition)
  {
    percent = Mathf.Clamp01(percent);
    ((RectTransform) row.transform).anchoredPosition = new Vector2(0.0f, yPosition);
    yPosition -= this.height + this.spacing;
    RectTransform rectTransform = row.Image.rectTransform;
    rectTransform.sizeDelta = new Vector2(percent * this.width, this.height);
    rectTransform.anchoredPosition = new Vector2((float) (-(double) this.width / 2.0), 0.0f);
    row.Image.color = this.percentColor.Evaluate(percent);
  }
}
