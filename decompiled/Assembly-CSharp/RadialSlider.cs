// Decompiled with JetBrains decompiler
// Type: RadialSlider
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

#nullable disable
public class RadialSlider : 
  MonoBehaviour,
  IPointerEnterHandler,
  IEventSystemHandler,
  IPointerExitHandler,
  IPointerDownHandler,
  IPointerUpHandler
{
  private bool focus;
  private bool grab;
  [SerializeField]
  private Image arrow;
  [SerializeField]
  private Image head;
  [SerializeField]
  private TextMeshProUGUI valueLabel;
  public float value;
  public UnityEvent OnValueChanged;

  private void Update()
  {
    if (!this.grab)
      return;
    this.SetValue(Vector3.SignedAngle(Vector3.up, (Input.mousePosition - this.transform.position).normalized, -Vector3.forward));
  }

  public void OnPointerEnter(PointerEventData pointer) => this.focus = true;

  public void OnPointerExit(PointerEventData pointer) => this.focus = false;

  public void OnPointerDown(PointerEventData pointer)
  {
    if (!this.focus)
      return;
    this.grab = true;
    this.arrow.color = Color.green;
    this.head.color = Color.green;
  }

  public void OnPointerUp(PointerEventData pointer)
  {
    if (!this.grab)
      return;
    this.grab = false;
    this.arrow.color = Color.white;
    this.head.color = Color.white;
    this.OnValueChanged?.Invoke();
  }

  public void SetValue(float input)
  {
    this.value = input;
    if ((double) this.value < 0.0)
      this.value += 360f;
    this.arrow.transform.localEulerAngles = new Vector3(0.0f, 0.0f, -this.value);
    this.valueLabel.text = $"{this.value:F0}";
    this.valueLabel.transform.eulerAngles = Vector3.zero;
  }
}
