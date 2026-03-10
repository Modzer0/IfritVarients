// Decompiled with JetBrains decompiler
// Type: OpticalLandingSystem
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
public class OpticalLandingSystem : MonoBehaviour
{
  [SerializeField]
  private Airbase airbase;
  [SerializeField]
  private GameObject meatball;
  [SerializeField]
  private GameObject datumLights;
  [SerializeField]
  private GameObject waveOffLights;
  [SerializeField]
  private GameObject redLight;
  [SerializeField]
  private GameObject whiteLight;
  [SerializeField]
  private Transform meatballPos;
  [SerializeField]
  private Transform meatballPosLow;
  [SerializeField]
  private Transform meatballPosHigh;
  [SerializeField]
  private float meatballMeasuredRange;
  [SerializeField]
  private UnitPart attachedPart;
  private Airbase.Runway runway;
  private readonly List<Aircraft> guidingAircraft = new List<Aircraft>();
  private Aircraft currentlyGuidingAircraft;
  private float errorTime;

  private void Start()
  {
    this.attachedPart.onApplyDamage += new Action<UnitPart.OnApplyDamage>(this.OpticalLandingSystem_OnPartDamage);
    this.runway = this.airbase.GetLandingRunway();
    this.runway.OnRegisterLanding += new Action<Aircraft>(this.OpticalLandingSystem_OnRegisterLanding);
    this.Shutdown();
  }

  private void OnDestroy()
  {
    this.attachedPart.onApplyDamage -= new Action<UnitPart.OnApplyDamage>(this.OpticalLandingSystem_OnPartDamage);
    this.runway.OnRegisterLanding -= new Action<Aircraft>(this.OpticalLandingSystem_OnRegisterLanding);
  }

  private void OpticalLandingSystem_OnPartDamage(UnitPart.OnApplyDamage e)
  {
    if ((double) e.hitPoints > 50.0)
      return;
    this.attachedPart.onApplyDamage -= new Action<UnitPart.OnApplyDamage>(this.OpticalLandingSystem_OnPartDamage);
    this.Shutdown();
    UnityEngine.Object.Destroy((UnityEngine.Object) this);
  }

  private void OpticalLandingSystem_OnRegisterLanding(Aircraft aircraft)
  {
    this.guidingAircraft.Insert(0, aircraft);
    this.enabled = true;
    this.meatball.SetActive(true);
    this.datumLights.SetActive(true);
  }

  private void Shutdown()
  {
    this.enabled = false;
    this.meatball.SetActive(false);
    this.redLight.SetActive(false);
    this.whiteLight.SetActive(false);
    this.datumLights.SetActive(false);
    this.waveOffLights.SetActive(false);
  }

  private async UniTask WaveOff()
  {
    this.meatball.SetActive(false);
    this.redLight.SetActive(false);
    this.whiteLight.SetActive(false);
    this.datumLights.SetActive(false);
    this.waveOffLights.SetActive(true);
    await UniTask.Delay(5000);
    this.waveOffLights.SetActive(false);
    this.meatball.SetActive(true);
    this.datumLights.SetActive(true);
  }

  private void Update()
  {
    if (this.waveOffLights.activeSelf)
      return;
    for (int index = this.guidingAircraft.Count - 1; index >= 0; --index)
    {
      if ((UnityEngine.Object) this.guidingAircraft[index] == (UnityEngine.Object) null || this.guidingAircraft[index].disabled || (double) Vector3.Dot((this.guidingAircraft[index].transform.position - this.transform.position).normalized, -this.transform.forward) < 0.800000011920929)
        this.guidingAircraft.RemoveAt(index);
    }
    if (this.guidingAircraft.Count == 0)
    {
      this.Shutdown();
    }
    else
    {
      Aircraft currentlyGuidingAircraft = this.currentlyGuidingAircraft;
      List<Aircraft> guidingAircraft1 = this.guidingAircraft;
      Aircraft aircraft = guidingAircraft1[guidingAircraft1.Count - 1];
      if ((UnityEngine.Object) currentlyGuidingAircraft != (UnityEngine.Object) aircraft)
      {
        List<Aircraft> guidingAircraft2 = this.guidingAircraft;
        this.currentlyGuidingAircraft = guidingAircraft2[guidingAircraft2.Count - 1];
        this.errorTime = 0.0f;
      }
      Vector3 vector3 = this.runway.Start.position - this.currentlyGuidingAircraft.transform.position;
      float magnitude = vector3.magnitude;
      Vector3 position1 = this.currentlyGuidingAircraft.transform.position;
      Vector3 position2 = this.runway.Start.position;
      Vector3 spawnOffset = this.currentlyGuidingAircraft.definition.spawnOffset;
      float num = Vector3.Dot(this.currentlyGuidingAircraft.rb.velocity - this.runway.GetVelocity(), vector3.normalized);
      float timeToTouchdown = Mathf.Min(magnitude / num, 30f);
      if (this.runway.GetVelocity() != Vector3.zero)
      {
        vector3 = this.runway.Start.position + this.runway.GetVelocity() * timeToTouchdown - this.currentlyGuidingAircraft.transform.position;
        magnitude = vector3.magnitude;
      }
      float t = (float) ((double) this.runway.GetGlideslopeError(this.currentlyGuidingAircraft, timeToTouchdown, false) * (1000.0 / (double) Mathf.Max(magnitude, 100f)) + (double) this.meatballMeasuredRange * 0.5) / this.meatballMeasuredRange;
      this.meatballPos.position = Vector3.Lerp(this.meatballPosLow.position, this.meatballPosHigh.position, t);
      if ((double) t < 0.20000000298023224)
      {
        this.whiteLight.SetActive(false);
        if ((double) t < 0.0)
          this.redLight.SetActive((double) Mathf.Sin(Time.timeSinceLevelLoad * 16f) > 0.0);
        else
          this.redLight.SetActive(true);
      }
      if ((double) t > 0.800000011920929)
      {
        this.redLight.SetActive(false);
        if ((double) t > 1.0)
          this.whiteLight.SetActive((double) Mathf.Sin(Time.timeSinceLevelLoad * 16f) > 0.0);
        else
          this.whiteLight.SetActive(true);
      }
      if ((double) t >= 0.20000000298023224 && (double) t <= 0.800000011920929)
      {
        this.meatball.SetActive(true);
        this.whiteLight.SetActive(false);
        this.redLight.SetActive(false);
      }
      if ((double) t < 0.0 || (double) t > 1.0)
      {
        this.errorTime += Time.deltaTime;
        if ((double) this.errorTime <= 7.0 || (double) timeToTouchdown >= 10.0)
          return;
        this.WaveOff().Forget();
        this.guidingAircraft.Remove(this.currentlyGuidingAircraft);
      }
      else
        this.errorTime = 0.0f;
    }
  }
}
