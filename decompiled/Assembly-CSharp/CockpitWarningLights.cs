// Decompiled with JetBrains decompiler
// Type: CockpitWarningLights
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
public class CockpitWarningLights : MonoBehaviour
{
  [SerializeField]
  private Renderer[] lightRenderers;
  [SerializeField]
  private Aircraft aircraft;
  private MissileWarning missileWarning;
  private List<Missile> knownMissiles;

  private void OnEnable()
  {
    for (int index = 0; index < this.lightRenderers.Length; ++index)
      this.lightRenderers[index].enabled = false;
    if (this.knownMissiles != null)
      return;
    this.aircraft.onInitialize += new Action(this.CockpitWarningLights_OnInitialize);
    this.enabled = false;
  }

  private void CockpitWarningLights_OnInitialize()
  {
    if (GameManager.IsLocalAircraft(this.aircraft))
    {
      this.missileWarning = this.aircraft.GetMissileWarningSystem();
      this.knownMissiles = this.missileWarning.knownMissiles;
      this.missileWarning.onMissileWarning += new Action<MissileWarning.OnMissileWarning>(this.CockpitWarningLights_OnMissileWarning);
      this.aircraft.onDisableUnit += new Action<Unit>(this.CockpitWarningLights_OnDisable);
    }
    else
      UnityEngine.Object.Destroy((UnityEngine.Object) this);
  }

  private void CockpitWarningLights_OnDisable(Unit unit)
  {
    this.missileWarning.onMissileWarning -= new Action<MissileWarning.OnMissileWarning>(this.CockpitWarningLights_OnMissileWarning);
    this.enabled = false;
  }

  private void CockpitWarningLights_OnMissileWarning(MissileWarning.OnMissileWarning _)
  {
    this.enabled = true;
  }

  private void Update()
  {
    if (this.knownMissiles.Count > 0)
    {
      for (int index = 0; index < this.lightRenderers.Length; ++index)
        this.lightRenderers[index].enabled = (double) Mathf.Sin(Time.timeSinceLevelLoad * 20f) > 0.0;
    }
    else
    {
      this.enabled = false;
      for (int index = 0; index < this.lightRenderers.Length; ++index)
        this.lightRenderers[index].enabled = false;
    }
  }
}
