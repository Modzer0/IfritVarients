// Decompiled with JetBrains decompiler
// Type: HUDTurretState
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#nullable disable
public class HUDTurretState : HUDWeaponState
{
  private HUDTurretState.HintState hintState = HUDTurretState.HintState.Neutral;
  [SerializeField]
  private Text hint;
  [SerializeField]
  private Transform boresight;
  [SerializeField]
  private GameObject turretCrosshairPrefab;
  private HUDTurretCrosshair[] crosshairs;
  private Image targetDesignator;
  private WeaponStation weaponStation;
  [SerializeField]
  private Gradient readinessGradient;

  public override void UpdateWeaponDisplay(Aircraft aircraft, List<Unit> targetList)
  {
    SceneSingleton<FlightHud>.i.velocityVector.color = new Color(0.0f, 1f, 0.0f, Mathf.Clamp01((float) ((double) Vector3.Distance(SceneSingleton<FlightHud>.i.velocityVector.transform.position, this.boresight.position) * 0.014999999664723873 - 0.15000000596046448)));
    float a = float.MaxValue;
    foreach (HUDTurretCrosshair crosshair in this.crosshairs)
    {
      Vector3 crosshairPosition;
      crosshair.Refresh(Camera.main, out crosshairPosition);
      a = Mathf.Min(a, Vector3.Distance(crosshairPosition, this.targetDesignator.transform.position));
    }
    HUDTurretState.HintState hintState = this.hintState;
    GlobalPosition knownPosition;
    if (targetList.Count > 0 && !targetList[0].disabled && aircraft.NetworkHQ.TryGetKnownPosition(targetList[0], out knownPosition))
    {
      double num = (double) FastMath.Distance(knownPosition, aircraft.GlobalPosition());
      if (num > (double) this.weaponInfo.targetRequirements.maxRange)
        hintState = HUDTurretState.HintState.OutOfRange;
      if (num < (double) this.weaponInfo.targetRequirements.maxRange * 0.5)
        hintState = HUDTurretState.HintState.Shoot;
    }
    else
      hintState = HUDTurretState.HintState.Neutral;
    if (hintState != this.hintState)
    {
      this.hintState = hintState;
      this.UpdateHint();
    }
    this.targetDesignator.color = new Color(0.0f, 1f, 0.0f, Mathf.Clamp01((float) ((double) a * 0.0099999997764825821 - 0.30000001192092896)));
  }

  public override void SetHUDWeaponState(
    Image targetDesignator,
    Aircraft aircraft,
    WeaponStation weaponStation)
  {
    this.weaponStation = weaponStation;
    this.weaponInfo = weaponStation.WeaponInfo;
    this.targetDesignator = targetDesignator;
    targetDesignator.transform.localScale = Vector3.one;
    SceneSingleton<FlightHud>.i.waterline.enabled = true;
    SceneSingleton<FlightHud>.i.velocityVector.transform.localScale = Vector3.one;
    this.crosshairs = new HUDTurretCrosshair[weaponStation.Turrets.Count];
    for (int index = 0; index < this.crosshairs.Length; ++index)
    {
      this.crosshairs[index] = Object.Instantiate<GameObject>(this.turretCrosshairPrefab, this.transform).GetComponent<HUDTurretCrosshair>();
      this.crosshairs[index].Initialize(weaponStation.Turrets[index]);
    }
  }

  private void UpdateHint()
  {
    switch (this.hintState)
    {
      case HUDTurretState.HintState.Shoot:
        this.hint.text = "SHOOT";
        break;
      case HUDTurretState.HintState.Neutral:
        this.hint.text = "";
        break;
      case HUDTurretState.HintState.OutOfRange:
        this.hint.text = "OUT OF RANGE";
        break;
    }
  }

  private enum HintState
  {
    Shoot,
    Neutral,
    OutOfRange,
  }
}
