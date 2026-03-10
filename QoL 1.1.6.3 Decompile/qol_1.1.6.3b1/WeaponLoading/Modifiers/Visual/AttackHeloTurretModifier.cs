// Decompiled with JetBrains decompiler
// Type: qol.WeaponLoading.Modifiers.Visual.AttackHeloTurretModifier
// Assembly: qol, Version=1.1.6.3, Culture=neutral, PublicKeyToken=null
// MVID: 3B6539EA-E38C-45EE-8FFF-FC58CC42BADC
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\BepInEx\plugins\qol_1.1.6.3b1.dll

using HarmonyLib;
using qol.Utilities;
using qol.WeaponLoading.Core;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

#nullable disable
namespace qol.WeaponLoading.Modifiers.Visual;

public class AttackHeloTurretModifier : IEntityModifier
{
  public string ModifierId => "AttackHeloTurret";

  public int Priority => 63 /*0x3F*/;

  public IReadOnlyList<string> Dependencies => (IReadOnlyList<string>) Array.Empty<string>();

  public bool CanApply(ModificationContext context) => true;

  public void Apply(ModificationContext context)
  {
    GameObject gameObject1 = PathLookup.Find("turret_20mm_rotary/turret", false);
    GameObject gameObject2 = PathLookup.Find("turret_30mm_rotary/turret", false);
    GameObject gameObject3 = PathLookup.Find("turret_30mm_chaingun/turret", false);
    GameObject gameObject4 = PathLookup.Find("turret_20mm_rotary/turret/gun/barrel", false);
    GameObject gameObject5 = PathLookup.Find("turret_30mm_rotary/turret/gun/barrel", false);
    GameObject gameObject6 = PathLookup.Find("turret_30mm_chaingun/turret/elevationTransform/gun/barrel", false);
    Material material1 = new Material(context.LitShader);
    material1.mainTexture = (Texture) QOLPlugin.LoadTextureFromResource(Assembly.GetExecutingAssembly().GetName().Name + ".Resources.FBXMod.attackHelo1_turret.png");
    Material material2 = new Material(context.LitShader);
    material2.SetColor("_Color", Color.black);
    material2.color = new Color(0.1f, 0.1f, 0.1f, 1f);
    if ((UnityEngine.Object) gameObject1 != (UnityEngine.Object) null && (UnityEngine.Object) material1 != (UnityEngine.Object) null)
      gameObject1.GetComponent<MeshRenderer>().material = material1;
    if ((UnityEngine.Object) gameObject2 != (UnityEngine.Object) null && (UnityEngine.Object) material1 != (UnityEngine.Object) null)
      gameObject2.GetComponent<MeshRenderer>().material = material1;
    if ((UnityEngine.Object) gameObject3 != (UnityEngine.Object) null && (UnityEngine.Object) material1 != (UnityEngine.Object) null)
      gameObject3.GetComponent<MeshRenderer>().material = material1;
    if ((UnityEngine.Object) gameObject4 != (UnityEngine.Object) null)
    {
      gameObject4.GetComponent<MeshRenderer>().sharedMaterials = new Material[1]
      {
        material2
      };
      gameObject4.GetComponent<MeshRenderer>().enabled = true;
    }
    if ((UnityEngine.Object) gameObject5 != (UnityEngine.Object) null)
      gameObject5.GetComponent<MeshRenderer>().sharedMaterials = new Material[1]
      {
        material2
      };
    if ((UnityEngine.Object) gameObject6 != (UnityEngine.Object) null)
      gameObject6.GetComponent<MeshRenderer>().sharedMaterials = new Material[1]
      {
        material2
      };
    if ((UnityEngine.Object) gameObject4 != (UnityEngine.Object) null && (UnityEngine.Object) gameObject5 != (UnityEngine.Object) null)
      gameObject4.GetComponent<MeshFilter>().sharedMesh = gameObject5.GetComponent<MeshFilter>().sharedMesh;
    context.Logger.LogInfo((object) $"[{this.ModifierId}] Applied turret reskinning and mesh swap");
    Traverse traverse = Traverse.Create((object) gameObject2.GetComponentInChildren<Gun>());
    traverse.Field("fireSounds").SetValue((object) null);
    traverse.Field("spinTransform").SetValue((object) PathLookup.Find("turret_30mm_rotary/turret/gun/barrel").transform);
    GameObject gameObject7 = PathLookup.Find("turret_30mm_rotary/turret/gun");
    AudioSource component = gameObject7.GetComponent<AudioSource>();
    AudioSource audioSource = gameObject7.AddComponent<AudioSource>();
    audioSource.outputAudioMixerGroup = component.outputAudioMixerGroup;
    audioSource.mute = component.mute;
    audioSource.bypassEffects = component.bypassEffects;
    audioSource.bypassListenerEffects = component.bypassListenerEffects;
    audioSource.bypassReverbZones = component.bypassReverbZones;
    audioSource.playOnAwake = component.playOnAwake;
    audioSource.loop = true;
    audioSource.priority = component.priority;
    audioSource.volume = component.volume;
    audioSource.pitch = component.pitch;
    audioSource.panStereo = component.panStereo;
    audioSource.spatialBlend = component.spatialBlend;
    audioSource.reverbZoneMix = component.reverbZoneMix;
    audioSource.dopplerLevel = component.dopplerLevel;
    audioSource.spread = component.spread;
    audioSource.rolloffMode = component.rolloffMode;
    audioSource.minDistance = component.minDistance;
    audioSource.maxDistance = component.maxDistance;
    Traverse.Create((object) gameObject1.GetComponentInChildren<Gun>()).Field("spinTransform").SetValue((object) PathLookup.Find("turret_20mm_rotary/turret/gun/barrel").transform);
    ColorableMount colorableMount1 = PathLookup.Find("turret_20mm_rotary").AddComponent<ColorableMount>();
    ColorableMount colorableMount2 = PathLookup.Find("turret_30mm_rotary").AddComponent<ColorableMount>();
    ColorableMount colorableMount3 = PathLookup.Find("turret_30mm_chaingun").AddComponent<ColorableMount>();
    Traverse.Create((object) colorableMount1).Field("colorableRenderers").SetValue((object) new Renderer[1]
    {
      (Renderer) gameObject1.GetComponent<MeshRenderer>()
    });
    Traverse.Create((object) colorableMount2).Field("colorableRenderers").SetValue((object) new Renderer[1]
    {
      (Renderer) gameObject2.GetComponent<MeshRenderer>()
    });
    Traverse.Create((object) colorableMount3).Field("colorableRenderers").SetValue((object) new Renderer[1]
    {
      (Renderer) gameObject3.GetComponent<MeshRenderer>()
    });
    Traverse.Create((object) colorableMount1).Field("skinnableRenderers").SetValue((object) new Renderer[0]);
    Traverse.Create((object) colorableMount2).Field("skinnableRenderers").SetValue((object) new Renderer[0]);
    Traverse.Create((object) colorableMount3).Field("skinnableRenderers").SetValue((object) new Renderer[0]);
  }
}
