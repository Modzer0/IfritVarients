// Decompiled with JetBrains decompiler
// Type: PIDTuner
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using UnityEngine;
using UnityEngine.UI;

#nullable disable
public class PIDTuner : MonoBehaviour
{
  [SerializeField]
  private Slider pSlider;
  [SerializeField]
  private Slider iSlider;
  [SerializeField]
  private Slider dSlider;
  [SerializeField]
  private Text pValue;
  [SerializeField]
  private Text iValue;
  [SerializeField]
  private Text dValue;
  private PID pid;

  private void OnEnable()
  {
    this.pSlider.SetValueWithoutNotify(0.1f);
    this.iSlider.SetValueWithoutNotify(0.01f);
    this.dSlider.SetValueWithoutNotify(0.1f);
    this.UpdateValues();
  }

  public void AttachTuner(PID pid) => this.pid = pid;

  public void UpdateValues()
  {
    this.pValue.text = this.pSlider.value.ToString("F2");
    this.iValue.text = this.iSlider.value.ToString("F2");
    this.dValue.text = this.dSlider.value.ToString("F2");
    if (this.pid == null)
      return;
    this.pid.SetValues(this.pSlider.value, this.iSlider.value, this.dSlider.value);
  }
}
