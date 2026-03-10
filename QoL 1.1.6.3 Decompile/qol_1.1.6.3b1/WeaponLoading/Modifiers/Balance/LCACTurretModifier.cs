// Decompiled with JetBrains decompiler
// Type: qol.WeaponLoading.Modifiers.Balance.LCACTurretModifier
// Assembly: qol, Version=1.1.6.3, Culture=neutral, PublicKeyToken=null
// MVID: 3B6539EA-E38C-45EE-8FFF-FC58CC42BADC
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\BepInEx\plugins\qol_1.1.6.3b1.dll

using HarmonyLib;
using qol.Utilities;
using qol.WeaponLoading.Core;
using System;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
namespace qol.WeaponLoading.Modifiers.Balance;

public class LCACTurretModifier : IEntityModifier
{
  public string ModifierId => "LCACTurret";

  public int Priority => 69;

  public IReadOnlyList<string> Dependencies => (IReadOnlyList<string>) Array.Empty<string>();

  public bool CanApply(ModificationContext context) => true;

  public void Apply(ModificationContext context)
  {
    GameObject gameObject1 = PathLookup.Find("LandingCraft1", false);
    if ((UnityEngine.Object) gameObject1 == (UnityEngine.Object) null)
    {
      context.Logger.LogWarning((object) $"[{this.ModifierId}] LandingCraft1 not found");
    }
    else
    {
      Ship component1 = gameObject1.GetComponent<Ship>();
      GameObject gameObject2 = PathLookup.Find("LandingCraft1/hull_F/hull_FL/turret", false);
      GameObject gameObject3 = PathLookup.Find("LandingCraft1/hull_F/hull_FR/missile_turret", false);
      GameObject original1 = PathLookup.Find("LightTruck1_AA/AATurret", false);
      GameObject original2 = PathLookup.Find("LightTruck1_AT/ATTurret", false);
      if ((UnityEngine.Object) original1 == (UnityEngine.Object) null || (UnityEngine.Object) original2 == (UnityEngine.Object) null)
      {
        context.Logger.LogWarning((object) $"[{this.ModifierId}] LightTruck1_AA/AT turret sources not found");
      }
      else
      {
        GameObject gameObject4 = PathLookup.Find("LandingCraft1/hull_F/hull_FL", false);
        GameObject gameObject5 = PathLookup.Find("LandingCraft1/hull_F/hull_FR", false);
        if ((UnityEngine.Object) gameObject4 == (UnityEngine.Object) null || (UnityEngine.Object) gameObject5 == (UnityEngine.Object) null)
        {
          context.Logger.LogWarning((object) $"[{this.ModifierId}] LCAC hull FL/FR not found");
        }
        else
        {
          GameObject turretGO1 = UnityEngine.Object.Instantiate<GameObject>(original1);
          GameObject turretGO2 = UnityEngine.Object.Instantiate<GameObject>(original2);
          turretGO1.transform.SetParent(gameObject4.transform);
          turretGO2.transform.SetParent(gameObject5.transform);
          if ((UnityEngine.Object) gameObject2 != (UnityEngine.Object) null)
            turretGO1.transform.position = gameObject2.transform.position;
          if ((UnityEngine.Object) gameObject3 != (UnityEngine.Object) null)
            turretGO2.transform.position = gameObject3.transform.position;
          LCACTurretModifier.SetTurretUnitReferences(turretGO1, component1);
          LCACTurretModifier.SetTurretUnitReferences(turretGO2, component1);
          GameObject gameObject6 = PathLookup.Find("AssaultCarrier1/hull_RL/hull_RR/hull_wellDeck/hull_RRL/welldeck_door", false);
          if ((UnityEngine.Object) gameObject6 != (UnityEngine.Object) null)
          {
            BoxCollider component2 = gameObject6.GetComponent<BoxCollider>();
            if ((UnityEngine.Object) component2 != (UnityEngine.Object) null)
              UnityEngine.Object.Destroy((UnityEngine.Object) component2);
          }
          context.Logger.LogInfo((object) $"[{this.ModifierId}] Replaced LCAC turrets and fixed welldeck door");
        }
      }
    }
  }

  private static void SetTurretUnitReferences(GameObject turretGO, Ship parentShip)
  {
    Unit unit = (Unit) parentShip;
    Rigidbody component1 = parentShip.gameObject.GetComponent<Rigidbody>();
    UnitPart component2 = turretGO.GetComponent<UnitPart>();
    if ((UnityEngine.Object) component2 != (UnityEngine.Object) null)
      Traverse.Create((object) component2).Field("parentUnit").SetValue((object) unit);
    Turret component3 = turretGO.GetComponent<Turret>();
    if ((UnityEngine.Object) component3 != (UnityEngine.Object) null)
      Traverse.Create((object) component3).Field("attachedUnit").SetValue((object) unit);
    TargetDetector component4 = turretGO.GetComponent<TargetDetector>();
    if ((UnityEngine.Object) component4 != (UnityEngine.Object) null)
      Traverse.Create((object) component4).Field("attachedUnit").SetValue((object) unit);
    MissileLauncher componentInChildren1 = turretGO.GetComponentInChildren<MissileLauncher>();
    if ((UnityEngine.Object) componentInChildren1 != (UnityEngine.Object) null)
      Traverse.Create((object) componentInChildren1).Field("attachedUnit").SetValue((object) unit);
    Gun componentInChildren2 = turretGO.GetComponentInChildren<Gun>();
    if (!((UnityEngine.Object) componentInChildren2 != (UnityEngine.Object) null))
      return;
    Traverse.Create((object) componentInChildren2).Field("velocityInherit").SetValue((object) component1);
  }
}
