// Decompiled with JetBrains decompiler
// Type: WaterEffect
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;

#nullable disable
public class WaterEffect : MonoBehaviour
{
  [SerializeField]
  private float lifetime = 120f;
  [SerializeField]
  private bool snapToDatum = true;
  [SerializeField]
  private Unit unit;

  private void Start()
  {
    this.transform.SetParent(Datum.origin);
    if (this.snapToDatum)
      this.transform.position = new Vector3(this.transform.position.x, Datum.LocalSeaY, this.transform.position.z);
    this.transform.localEulerAngles = new Vector3(0.0f, this.transform.localEulerAngles.y, 0.0f);
    if ((UnityEngine.Object) this.unit != (UnityEngine.Object) null)
      this.unit.onDisableUnit += new Action<Unit>(this.WaterEffect_OnUnitDisable);
    else
      UnityEngine.Object.Destroy((UnityEngine.Object) this.gameObject, this.lifetime);
    this.enabled = false;
  }

  private void WaterEffect_OnUnitDisable(Unit unit)
  {
    unit.onDisableUnit -= new Action<Unit>(this.WaterEffect_OnUnitDisable);
    if (!((UnityEngine.Object) this != (UnityEngine.Object) null))
      return;
    UnityEngine.Object.Destroy((UnityEngine.Object) this.gameObject, this.lifetime);
  }
}
