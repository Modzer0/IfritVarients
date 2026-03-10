// Decompiled with JetBrains decompiler
// Type: CargoDeploymentSystem
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

#nullable disable
public class CargoDeploymentSystem : MonoBehaviour
{
  [SerializeField]
  private Unit attachedUnit;
  [SerializeField]
  private List<Parachute> listParachutes;
  [SerializeField]
  private float radialStiffness = 100000f;
  [SerializeField]
  private Vector3 baseFrameDimensions;
  [SerializeField]
  private GameObject liftingFrame;
  private bool initialized;
  private bool chuteOpen;

  public void Initialize(Unit attachedUnit)
  {
    this.attachedUnit = attachedUnit;
    attachedUnit.onDisableUnit += new Action<Unit>(this.CargoDeploymentSystem_OnUnitDisabled);
    if (attachedUnit is GroundVehicle vehicle)
    {
      vehicle.SetWheelsLocked(true);
      this.StowTurrets(vehicle).Forget();
    }
    UnitDefinition definition = attachedUnit.definition;
    this.transform.localPosition = -definition.spawnOffset;
    this.liftingFrame.transform.localScale = new Vector3(definition.width * 1.1f / this.baseFrameDimensions.x, definition.height * 1.1f / this.baseFrameDimensions.y, definition.length * 1.1f / this.baseFrameDimensions.z);
    foreach (Parachute listParachute in this.listParachutes)
    {
      listParachute.transform.localScale = new Vector3(1f / this.liftingFrame.transform.localScale.x, 1f / this.liftingFrame.transform.localScale.y, 1f / this.liftingFrame.transform.localScale.z);
      listParachute.SetAttachedUnit(attachedUnit);
      listParachute.gameObject.SetActive(true);
      listParachute.onUnitLanded += new Action(this.CargoDeploymentSystem_OnUnitLanded);
    }
    this.initialized = true;
  }

  private async UniTask StowTurrets(GroundVehicle vehicle)
  {
    CancellationToken cancel = this.destroyCancellationToken;
    await UniTask.Delay(100);
    if (cancel.IsCancellationRequested)
    {
      cancel = new CancellationToken();
    }
    else
    {
      vehicle.StowTurrets(true);
      cancel = new CancellationToken();
    }
  }

  private void FixedUpdate()
  {
    for (int index1 = 0; index1 < this.listParachutes.Count; ++index1)
    {
      if (!((UnityEngine.Object) this.listParachutes[index1] == (UnityEngine.Object) null) && this.listParachutes[index1].IsOpen())
      {
        if (!this.chuteOpen)
          this.chuteOpen = true;
        Vector3 zero = Vector3.zero;
        for (int index2 = 0; index2 < this.listParachutes.Count; ++index2)
        {
          if (index1 != index2)
          {
            Vector3 vector3 = this.listParachutes[index1].GetCanopyPosition() - this.listParachutes[index2].GetCanopyPosition();
            float num = (float) (4.0 * ((double) this.listParachutes[index1].GetCurrentRadius() + (double) this.listParachutes[index2].GetCurrentRadius()));
            if ((double) vector3.magnitude < (double) num)
              zero -= this.radialStiffness * (num - vector3.magnitude) * vector3.normalized;
            if ((double) vector3.magnitude == 0.0)
              zero -= this.radialStiffness * this.listParachutes[index1].transform.right;
          }
        }
        this.listParachutes[index1].AddRepelForce(zero);
      }
    }
  }

  public void CargoDeploymentSystem_OnUnitDisabled(Unit unit)
  {
    UnityEngine.Object.Destroy((UnityEngine.Object) this.gameObject);
  }

  public void CargoDeploymentSystem_OnUnitLanded() => UnityEngine.Object.Destroy((UnityEngine.Object) this.gameObject);

  protected void OnDestroy()
  {
    if (!this.initialized)
      return;
    this.attachedUnit.onDisableUnit -= new Action<Unit>(this.CargoDeploymentSystem_OnUnitDisabled);
    foreach (Parachute listParachute in this.listParachutes)
      listParachute.onUnitLanded -= new Action(this.CargoDeploymentSystem_OnUnitLanded);
    if (this.attachedUnit is GroundVehicle attachedUnit)
    {
      attachedUnit.SetWheelsLocked(false);
      attachedUnit.StowTurrets(false);
    }
    if (this.attachedUnit.disabled || this.attachedUnit.enabled)
      return;
    this.attachedUnit.enabled = true;
  }
}
