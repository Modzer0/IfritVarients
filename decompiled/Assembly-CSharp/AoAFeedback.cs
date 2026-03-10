// Decompiled with JetBrains decompiler
// Type: AoAFeedback
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

#nullable disable
public static class AoAFeedback
{
  private static Aircraft _aircraft;
  private static AudioSource _source;
  private static AircraftParameters.OnboardAoAEffects aoaEffects;
  private static float volume;
  private static float volumeSmoothed;
  private static float shake;
  private static float shakeSmoothed;
  private static float lastUpdate;

  private static void SetupAircraft(Aircraft aircraft)
  {
    AoAFeedback._aircraft = aircraft;
    AoAFeedback.lastUpdate = 0.0f;
    if ((Object) AoAFeedback._source != (Object) null)
      Object.Destroy((Object) AoAFeedback._source);
    if ((Object) aircraft == (Object) null)
      return;
    AoAFeedback.volumeSmoothed = 0.0f;
    AoAFeedback.shakeSmoothed = 0.0f;
    AoAFeedback._source = aircraft.cockpit.gameObject.AddComponent<AudioSource>();
    AoAFeedback._source.dopplerLevel = 0.0f;
    AoAFeedback._source.spatialBlend = 0.0f;
    AoAFeedback._source.minDistance = 4f;
    AoAFeedback._source.maxDistance = 5f;
    AoAFeedback._source.priority = 128 /*0x80*/;
    AoAFeedback._source.loop = true;
    AoAFeedback._source.volume = 0.0f;
    AoAFeedback._source.outputAudioMixerGroup = SoundManager.i.EffectsMixer;
    AoAFeedback._source.bypassListenerEffects = true;
    AoAFeedback.aoaEffects = aircraft.GetAircraftParameters().AoAEffects;
    AoAFeedback._source.clip = AoAFeedback.aoaEffects.AudioClip;
    AoAFeedback._source.Play();
  }

  public static void RunAoAFeedback(Aircraft aircraft)
  {
    if ((Object) AoAFeedback._aircraft != (Object) aircraft)
      AoAFeedback.SetupAircraft(aircraft);
    if ((double) Time.timeSinceLevelLoad - (double) AoAFeedback.lastUpdate < 0.10000000149011612)
    {
      AoAFeedback.volumeSmoothed = Mathf.Lerp(AoAFeedback.volumeSmoothed, AoAFeedback.volume, 8f * Time.fixedDeltaTime);
      AoAFeedback.shakeSmoothed = Mathf.Lerp(AoAFeedback.shakeSmoothed, AoAFeedback.shake, 8f * Time.fixedDeltaTime);
      AoAFeedback._source.volume = AoAFeedback.volumeSmoothed;
      SceneSingleton<CameraStateManager>.i.ShakeCamera(0.0f, AoAFeedback.shakeSmoothed);
    }
    else
    {
      if ((Object) aircraft == (Object) null)
        return;
      AoAFeedback.lastUpdate = Time.timeSinceLevelLoad;
      Vector3 direction = aircraft.cockpit.rb.velocity - NetworkSceneSingleton<LevelInfo>.i.GetWind(aircraft.cockpit.xform.GlobalPosition());
      Vector3 vector3 = aircraft.cockpit.xform.InverseTransformDirection(direction);
      double f = (double) Mathf.Atan2(vector3.y, vector3.z) * 57.295780181884766;
      float num1 = Mathf.Max(aircraft.speed - AoAFeedback.aoaEffects.OnsetSpeed, 0.0f) / (AoAFeedback.aoaEffects.FullVolumeSpeed - AoAFeedback.aoaEffects.OnsetSpeed);
      float num2 = Mathf.Max(Mathf.Abs((float) f) - AoAFeedback.aoaEffects.OnsetAlpha, 0.0f) / (AoAFeedback.aoaEffects.FullVolumeAlpha - AoAFeedback.aoaEffects.OnsetAlpha);
      AoAFeedback.volume = Mathf.Sqrt(num1 * num2);
      AoAFeedback.shake = AoAFeedback.aoaEffects.ShakeFactor * num1 * num2;
    }
  }
}
