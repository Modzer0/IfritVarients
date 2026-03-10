// Decompiled with JetBrains decompiler
// Type: SonicBoomManager
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System.Collections.Generic;
using UnityEngine;

#nullable disable
public static class SonicBoomManager
{
  private static List<SonicBoomManager.ManagedSonicBoom> managedBooms;
  private static int index;

  public static void RegisterAircraft(Aircraft aircraft)
  {
    if (SonicBoomManager.managedBooms == null)
      SonicBoomManager.managedBooms = new List<SonicBoomManager.ManagedSonicBoom>();
    foreach (SonicBoomManager.ManagedSonicBoom managedBoom in SonicBoomManager.managedBooms)
    {
      if ((Object) aircraft == (Object) managedBoom.aircraft)
        return;
    }
    SonicBoomManager.managedBooms.Add(new SonicBoomManager.ManagedSonicBoom(aircraft));
  }

  public static void ManageSonicBooms()
  {
    if (SonicBoomManager.managedBooms == null || SonicBoomManager.managedBooms.Count == 0)
      return;
    if (SonicBoomManager.index < 0)
      SonicBoomManager.index = SonicBoomManager.managedBooms.Count - 1;
    if (SonicBoomManager.index == -1)
      return;
    if (!SonicBoomManager.managedBooms[SonicBoomManager.index].Manage())
      SonicBoomManager.managedBooms.RemoveAt(SonicBoomManager.index);
    --SonicBoomManager.index;
  }

  private class ManagedSonicBoom
  {
    public readonly Aircraft aircraft;
    private AudioSource source;
    private GameObject sourceObject;
    private float lastSupersonic;

    public ManagedSonicBoom(Aircraft aircraft)
    {
      this.aircraft = aircraft;
      this.lastSupersonic = 1E+07f;
      this.sourceObject = new GameObject("Sonic Boom");
      this.sourceObject.transform.SetParent(Datum.origin);
      this.sourceObject.transform.position = aircraft.transform.position;
      this.source = this.sourceObject.AddComponent<AudioSource>();
      this.source.outputAudioMixerGroup = SoundManager.i.EffectsMixer;
      this.source.spatialBlend = 1f;
      this.source.minDistance = 1000f;
      this.source.dopplerLevel = 1f;
      this.source.spread = 20f;
      this.source.maxDistance = 5000f;
    }

    public bool Manage()
    {
      if ((Object) this.aircraft == (Object) null)
        return false;
      float num1 = Vector3.Distance(this.sourceObject.transform.position, SceneSingleton<CameraStateManager>.i.transform.position);
      float speedOfSound = LevelInfo.GetSpeedOfSound(this.aircraft.GlobalPosition().y);
      if ((double) this.aircraft.speed > (double) speedOfSound)
      {
        Vector3 rhs = SceneSingleton<CameraStateManager>.i.transform.position - this.aircraft.transform.position;
        if ((double) Vector3.Dot(this.aircraft.rb.velocity, rhs) > 0.0 && (double) rhs.magnitude < (double) num1)
        {
          this.lastSupersonic = Time.timeSinceLevelLoad;
          this.sourceObject.transform.position = this.aircraft.transform.position;
        }
      }
      if (((double) Time.timeSinceLevelLoad - (double) this.lastSupersonic) * (double) speedOfSound > (double) num1)
      {
        float num2 = 1000f / num1;
        this.source.PlayOneShot(GameAssets.i.sonicBoom, 1f);
        SceneSingleton<CameraStateManager>.i.ShakeCamera(Mathf.Clamp01(num2), 0.0f);
        this.lastSupersonic = 1E+07f;
      }
      return true;
    }
  }
}
