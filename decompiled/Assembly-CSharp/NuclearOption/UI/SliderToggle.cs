// Decompiled with JetBrains decompiler
// Type: NuclearOption.UI.SliderToggle
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

#nullable disable
namespace NuclearOption.UI;

[AddComponentMenu("UI/SliderToggle", 530)]
[RequireComponent(typeof (RectTransform))]
public class SliderToggle : BaseToggle
{
  [Header("Animation")]
  [SerializeField]
  private RectTransform handle;
  [SerializeField]
  private Vector2 onPosition;
  [SerializeField]
  private Vector2 offPosition;
  [SerializeField]
  private float moveDuration;
  [SerializeField]
  private Image background;
  [SerializeField]
  private Color onColor;
  [SerializeField]
  private Color offColor;
  [SerializeField]
  private float colorDuration;
  private bool isRunning;
  private bool targetOn;
  private float moveLerp;
  private float colorLerp;

  protected override void PlayEffect(bool instant)
  {
    if ((Object) this.handle == (Object) null || (Object) this.background == (Object) null)
      return;
    if (instant)
    {
      this.handle.anchoredPosition = this.isOn ? this.onPosition : this.offPosition;
      this.background.color = this.isOn ? this.onColor : this.offColor;
    }
    else
    {
      this.targetOn = this.m_IsOn;
      if (this.isRunning)
        return;
      this.TransitionTask().Forget();
    }
  }

  private async UniTask TransitionTask()
  {
    SliderToggle sliderToggle = this;
    sliderToggle.isRunning = true;
    CancellationToken cancel = sliderToggle.destroyCancellationToken;
    while (!cancel.IsCancellationRequested)
    {
      float unscaledDeltaTime = Time.unscaledDeltaTime;
      SliderToggle.MoveTowards(ref sliderToggle.moveLerp, sliderToggle.moveDuration, unscaledDeltaTime, sliderToggle.targetOn);
      SliderToggle.MoveTowards(ref sliderToggle.colorLerp, sliderToggle.colorDuration, unscaledDeltaTime, sliderToggle.targetOn);
      sliderToggle.handle.anchoredPosition = Vector2.Lerp(sliderToggle.offPosition, sliderToggle.onPosition, SliderToggle.EaseInOut(sliderToggle.moveLerp));
      sliderToggle.background.color = Color.Lerp(sliderToggle.offColor, sliderToggle.onColor, SliderToggle.EaseInOut(sliderToggle.colorLerp));
      if ((double) sliderToggle.moveLerp < 1.0 || (double) sliderToggle.colorLerp < 1.0)
        await UniTask.Yield();
      else
        break;
    }
    sliderToggle.isRunning = false;
    cancel = new CancellationToken();
  }

  private static void MoveTowards(ref float value, float duration, float dt, bool target)
  {
    float num = (double) duration > 0.0 ? dt / duration : 1f;
    if (!target)
      num *= -1f;
    value = Mathf.Clamp01(value + num);
  }

  public static float EaseInOut(float t)
  {
    return (float) ((double) t * (double) t * (3.0 - 2.0 * (double) t));
  }
}
