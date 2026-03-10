// Decompiled with JetBrains decompiler
// Type: GLOC
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

#nullable disable
public class GLOC : MonoBehaviour
{
  [SerializeField]
  private float bloodPumpRate = 0.28f;
  private float stamina = 1f;
  private float staminaRecoveryRate = 0.18f;
  private float bloodPressure = 1f;
  private bool conscious = true;
  private ColorAdjustments colorAdjustments;
  private Vignette vignette;
  private Image blackoutImage;

  private void OnEnable()
  {
    if (!SceneSingleton<CameraStateManager>.i.GetPostProcessVolume().profile.TryGet<ColorAdjustments>(out this.colorAdjustments))
      throw new NullReferenceException("colorAdjustments");
    this.blackoutImage = SceneSingleton<CameraStateManager>.i.GetPostProcessVolume().profile.TryGet<Vignette>(out this.vignette) ? SceneSingleton<CameraStateManager>.i.GetBlackoutImage() : throw new NullReferenceException("colorAdjustments");
    SceneSingleton<CameraStateManager>.i.onSwitchCamera += new Action(this.GLOC_OnSwitchCamera);
  }

  public void ResetGLOC()
  {
    this.blackoutImage.color = Color.clear;
    this.colorAdjustments.saturation.value = 0.0f;
    this.vignette.intensity.value = 0.4f;
  }

  public float SimulateGLOC(float gForce)
  {
    gForce = Mathf.Abs(gForce);
    if (this.conscious)
    {
      float num = this.bloodPumpRate - gForce * 0.04f;
      if ((double) this.bloodPressure < 0.550000011920929)
        num += this.stamina * 0.25f;
      if ((double) this.bloodPressure < 0.60000002384185791 && !this.blackoutImage.enabled)
        this.blackoutImage.enabled = true;
      else if ((double) this.bloodPressure >= 0.60000002384185791 && this.blackoutImage.enabled)
        this.blackoutImage.enabled = false;
      this.stamina += (this.staminaRecoveryRate - gForce * 0.04f) * Time.deltaTime;
      this.stamina = Mathf.Clamp01(this.stamina);
      this.bloodPressure += num * Time.deltaTime;
      this.bloodPressure = Mathf.Clamp(this.bloodPressure, 0.0f, 1f);
      if ((double) this.bloodPressure < 0.20000000298023224)
      {
        this.LOC().Forget();
        this.conscious = false;
      }
    }
    if (CameraStateManager.cameraMode == CameraMode.cockpit)
    {
      float t1 = (float) (((double) this.bloodPressure - 0.20000000298023224) / 0.40000000596046448);
      float t2 = (float) (((double) this.bloodPressure - 0.30000001192092896) / 0.40000000596046448);
      this.blackoutImage.color = Color.Lerp(Color.black, Color.clear, t1);
      this.colorAdjustments.saturation.value = Mathf.Lerp(-100f, 0.0f, t2);
      this.vignette.intensity.value = Mathf.Lerp(1f, 0.4f, t1);
      AudioMixerVolume.SetMasterAudioFilterStrength(Mathf.Lerp(250f, 11000f, Mathf.Clamp01(t1)) + 11000f * Mathf.Clamp01(t2));
    }
    return this.bloodPressure;
  }

  private void GLOC_OnSwitchCamera()
  {
    if (CameraStateManager.cameraMode == CameraMode.cockpit)
      return;
    this.blackoutImage.color = Color.clear;
    this.colorAdjustments.saturation.value = 0.0f;
    this.vignette.intensity.value = 0.4f;
    AudioMixerVolume.SetMasterAudioFilterStrength(22000f);
  }

  private void OnDestroy()
  {
    SceneSingleton<CameraStateManager>.i.onSwitchCamera -= new Action(this.GLOC_OnSwitchCamera);
    this.blackoutImage.color = Color.clear;
    this.colorAdjustments.saturation.value = 0.0f;
    this.vignette.intensity.value = 0.4f;
  }

  private async UniTask LOC()
  {
    this.blackoutImage.enabled = true;
    this.blackoutImage.color = new Color(0.0f, 0.0f, 0.0f, 1f);
    await UniTask.Delay((int) ((double) UnityEngine.Random.Range(3f, 6f) * 1000.0));
    this.bloodPressure = 0.2f;
    this.conscious = true;
  }
}
