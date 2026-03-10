// Decompiled with JetBrains decompiler
// Type: qol.WeaponLoading.Modifiers.Balance.RailcannonPodModifier
// Assembly: qol, Version=1.1.6.3, Culture=neutral, PublicKeyToken=null
// MVID: 3B6539EA-E38C-45EE-8FFF-FC58CC42BADC
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\BepInEx\plugins\qol_1.1.6.3b1.dll

using HarmonyLib;
using qol.Utilities;
using qol.WeaponLoading.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#nullable disable
namespace qol.WeaponLoading.Modifiers.Balance;

public class RailcannonPodModifier : IEntityModifier
{
  public string ModifierId => "RailcannonPod";

  public int Priority => 62;

  public IReadOnlyList<string> Dependencies => (IReadOnlyList<string>) Array.Empty<string>();

  public bool CanApply(ModificationContext context) => true;

  public void Apply(ModificationContext context)
  {
    GameObject duplicateMount = QOLPlugin.DuplicatePrefab("gun_57mm_pod", "gun_155mm_pod_P");
    if ((UnityEngine.Object) duplicateMount == (UnityEngine.Object) null)
    {
      context.Logger.LogError((object) $"[{this.ModifierId}] Failed to duplicate prefab gun_57mm_pod");
    }
    else
    {
      WeaponInfo info = QOLPlugin.DuplicateWeaponInfo("Railgun1", "Railgun1_P", (GameObject) null);
      if ((UnityEngine.Object) info == (UnityEngine.Object) null)
      {
        context.Logger.LogError((object) $"[{this.ModifierId}] Failed to duplicate weapon info Railgun1");
      }
      else
      {
        WeaponMount mount = QOLPlugin.DuplicateWeaponMount("gun_57mm_pod", "gun_155mm_pod_P", duplicateMount, info);
        if ((UnityEngine.Object) mount == (UnityEngine.Object) null)
        {
          context.Logger.LogError((object) $"[{this.ModifierId}] Failed to duplicate weapon mount");
        }
        else
        {
          GameObject original = PathLookup.Find("Destroyer1/Hull_CF/Hull_CFF/turret_F/cannon_F", false);
          if ((UnityEngine.Object) original == (UnityEngine.Object) null)
          {
            context.Logger.LogError((object) $"[{this.ModifierId}] Cannon source not found: Destroyer1/Hull_CF/Hull_CFF/turret_F/cannon_F");
          }
          else
          {
            GameObject gameObject1 = UnityEngine.Object.Instantiate<GameObject>(original);
            gameObject1.transform.SetParent(duplicateMount.transform);
            MeshRenderer component1 = gameObject1.GetComponent<MeshRenderer>();
            if ((UnityEngine.Object) component1 != (UnityEngine.Object) null)
              UnityEngine.Object.Destroy((UnityEngine.Object) component1);
            MeshFilter component2 = gameObject1.GetComponent<MeshFilter>();
            if ((UnityEngine.Object) component2 != (UnityEngine.Object) null)
              UnityEngine.Object.Destroy((UnityEngine.Object) component2);
            GameObject gameObject2 = PathLookup.Find("gun_155mm_pod_P/gunpod", false);
            if ((UnityEngine.Object) gameObject2 != (UnityEngine.Object) null)
              UnityEngine.Object.Destroy((UnityEngine.Object) gameObject2);
            gameObject1.name = "gunpod";
            Gun component3 = duplicateMount.GetComponent<Gun>();
            if ((UnityEngine.Object) component3 != (UnityEngine.Object) null)
            {
              Traverse traverse = Traverse.Create((object) component3);
              traverse.Field("velocityInherit").SetValue((object) null);
              traverse.Field("recoilSound").SetValue((object) null);
              traverse.Field("attachedUnit").SetValue((object) null);
            }
            Traverse.Create((object) gameObject1.AddComponent<SetGlobalParticles>()).Field("systems").SetValue((object) ((IEnumerable<ParticleSystem>) gameObject1.GetComponentsInChildren<ParticleSystem>()).ToList<ParticleSystem>());
            GameObject gameObject3 = PathLookup.Find("Laser_EW1", false);
            if ((UnityEngine.Object) gameObject3 != (UnityEngine.Object) null)
            {
              Laser component4 = gameObject3.GetComponent<Laser>();
              if ((UnityEngine.Object) component4 != (UnityEngine.Object) null && (UnityEngine.Object) component4.info != (UnityEngine.Object) null)
                info.weaponIcon = component4.info.weaponIcon;
            }
            QOLPlugin.AddMountToEncyclopedia(context.Encyclopedia, "gun_155mm_pod_P", mount);
            context.Logger.LogInfo((object) $"[{this.ModifierId}] Created gun_155mm_pod_P railcannon pod");
          }
        }
      }
    }
  }
}
