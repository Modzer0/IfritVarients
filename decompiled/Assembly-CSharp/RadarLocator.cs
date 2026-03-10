// Decompiled with JetBrains decompiler
// Type: RadarLocator
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;

#nullable disable
public class RadarLocator : MonoBehaviour
{
  [SerializeField]
  private Aircraft aircraft;
  [SerializeField]
  private UnitPart[] essentialParts;
  [SerializeField]
  private bool onlySurface;
  private float rewardAmount;
  private float rewardCount;
  private float rewardThreshold = 1f;

  private void Awake()
  {
    this.aircraft.onRadarWarning += new Action<Aircraft.OnRadarWarning>(this.RadarLocator_OnRadarWarning);
    foreach (UnitPart essentialPart in this.essentialParts)
      essentialPart.onParentDetached += new Action<UnitPart>(this.RadarLocator_OnPartDetached);
  }

  private void RadarLocator_OnRadarWarning(Aircraft.OnRadarWarning e)
  {
    if (!this.aircraft.IsServer || !((UnityEngine.Object) this.aircraft.NetworkHQ != (UnityEngine.Object) null))
      return;
    if (this.onlySurface)
    {
      TypeIdentity typeIdentity = e.emitter.definition.typeIdentity;
      if ((double) typeIdentity.air > 0.0 || (double) typeIdentity.missile > 0.0)
        return;
    }
    if ((UnityEngine.Object) this.aircraft.Player != (UnityEngine.Object) null && (UnityEngine.Object) e.emitter.NetworkHQ != (UnityEngine.Object) null && (UnityEngine.Object) e.emitter.NetworkHQ != (UnityEngine.Object) this.aircraft.NetworkHQ)
    {
      float num = 0.0f;
      if (!this.aircraft.NetworkHQ.trackingDatabase.ContainsKey(e.emitter.persistentID))
        num = 0.01f;
      else if (!this.aircraft.NetworkHQ.IsTargetPositionAccurate(e.emitter, 500f))
        num = 0.005f;
      this.rewardCount += num * Mathf.Sqrt(e.emitter.definition.value);
      this.rewardAmount += num * Mathf.Sqrt(e.emitter.definition.value);
      if ((double) this.rewardCount > (double) this.rewardThreshold)
      {
        this.aircraft.NetworkHQ.ReportReconAction(this.aircraft.Player, this.rewardAmount);
        this.rewardAmount = 0.0f;
        this.rewardCount = 0.0f;
      }
    }
    this.aircraft.NetworkHQ.CmdUpdateTrackingInfo(e.emitter.persistentID);
  }

  private void RadarLocator_OnPartDetached(UnitPart part)
  {
    this.aircraft.onRadarWarning -= new Action<Aircraft.OnRadarWarning>(this.RadarLocator_OnRadarWarning);
  }
}
