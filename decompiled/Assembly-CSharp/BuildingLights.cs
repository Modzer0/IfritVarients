// Decompiled with JetBrains decompiler
// Type: BuildingLights
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#nullable disable
public class BuildingLights : MonoBehaviour
{
  [SerializeField]
  private bool daylightToggle = true;
  [SerializeField]
  private Renderer[] renderers;
  [SerializeField]
  private Light[] lights;
  [SerializeField]
  private UnitPart[] dependentParts;

  private void Awake()
  {
  }

  private void Start()
  {
    if (this.dependentParts != null && ((IEnumerable<UnitPart>) this.dependentParts).Count<UnitPart>() > 0)
    {
      foreach (UnitPart dependentPart in this.dependentParts)
        dependentPart.onApplyDamage += new Action<UnitPart.OnApplyDamage>(this.BuildingLights_OnPartDamage);
    }
    if (!this.daylightToggle)
      return;
    NetworkSceneSingleton<LevelInfo>.i.onDaylightChange += new Action(this.BuildingLights_onDaylightChange);
    this.BuildingLights_onDaylightChange();
  }

  private void BuildingLights_OnPartDamage(UnitPart.OnApplyDamage e)
  {
    if ((double) e.hitPoints >= 50.0)
      return;
    if (this.renderers != null)
    {
      foreach (Renderer renderer in this.renderers)
        renderer.enabled = false;
    }
    if (this.lights != null)
    {
      foreach (Behaviour light in this.lights)
        light.enabled = false;
    }
    foreach (UnitPart dependentPart in this.dependentParts)
    {
      if ((UnityEngine.Object) dependentPart != (UnityEngine.Object) null)
        dependentPart.onApplyDamage -= new Action<UnitPart.OnApplyDamage>(this.BuildingLights_OnPartDamage);
    }
    if (this.daylightToggle)
      NetworkSceneSingleton<LevelInfo>.i.onDaylightChange -= new Action(this.BuildingLights_onDaylightChange);
    UnityEngine.Object.Destroy((UnityEngine.Object) this);
  }

  private void BuildingLights_onDaylightChange()
  {
    bool flag = !NetworkSceneSingleton<LevelInfo>.i.isDayLight;
    if (this.renderers != null)
    {
      foreach (Renderer renderer in this.renderers)
      {
        if ((UnityEngine.Object) renderer != (UnityEngine.Object) null)
          renderer.enabled = flag;
      }
    }
    if (this.lights == null)
      return;
    foreach (Light light in this.lights)
    {
      if ((UnityEngine.Object) light != (UnityEngine.Object) null)
        light.enabled = flag;
    }
  }
}
