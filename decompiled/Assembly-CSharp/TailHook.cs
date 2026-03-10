// Decompiled with JetBrains decompiler
// Type: TailHook
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;

#nullable disable
public class TailHook : MonoBehaviour
{
  [SerializeField]
  private Transform hinge;
  [SerializeField]
  private Transform hook;
  [SerializeField]
  private Transform castPoint;
  public Transform hookEnd;
  public UnitPart unitPart;
  [SerializeField]
  private float mass;
  [SerializeField]
  private float stowedAngle;
  [SerializeField]
  private float deployedAngle;
  private float deployedAmount;
  private bool deployed;
  private float hookLength;
  private Aircraft aircraft;
  private bool hooked;

  private void Awake()
  {
    this.hookLength = Vector3.Distance(this.hook.position, this.hookEnd.position);
  }

  public float GetMass() => this.mass;

  private void Start()
  {
    this.unitPart = this.gameObject.GetComponentInParent<UnitPart>();
    this.aircraft = this.unitPart.parentUnit as Aircraft;
    this.StartSlowUpdate(1f, new Action(this.CheckDeployConditions));
  }

  private void CheckDeployConditions()
  {
    if ((double) this.aircraft.radarAlt > 30.0 && (double) this.aircraft.radarAlt < 120.0 && (double) this.aircraft.speed < 100.0 && (double) this.aircraft.rb.velocity.y < 0.0 && !this.deployed)
    {
      if ((UnityEngine.Object) this.aircraft == (UnityEngine.Object) SceneSingleton<CombatHUD>.i.aircraft)
        SceneSingleton<AircraftActionsReport>.i.ReportText("Tail Hook Deployed", 4f);
      this.deployed = true;
      this.enabled = true;
    }
    if ((double) this.aircraft.speed <= 100.0 && (double) this.aircraft.speed >= 2.0 || !this.deployed)
      return;
    if ((UnityEngine.Object) this.aircraft == (UnityEngine.Object) SceneSingleton<CombatHUD>.i.aircraft)
      SceneSingleton<AircraftActionsReport>.i.ReportText("Tail Hook Retracted", 4f);
    this.deployed = false;
    if (!this.hooked)
      return;
    this.Unhook();
  }

  public void ApplyForce(Vector3 force)
  {
    if (PlayerSettings.debugVis)
    {
      GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(GameAssets.i.debugArrow, this.hookEnd);
      gameObject.transform.rotation = Quaternion.LookRotation(force);
      gameObject.transform.localScale = new Vector3(1f, 1f, force.magnitude);
      UnityEngine.Object.Destroy((UnityEngine.Object) gameObject, 0.1f);
    }
    this.deployedAmount -= Mathf.Clamp(TargetCalc.GetAngleOnAxis(this.castPoint.forward, force, this.castPoint.right) * 0.01f, -2f * Time.deltaTime, 2f * Time.deltaTime);
    this.deployedAmount = Mathf.Clamp(this.deployedAmount, 0.01f, 1f);
    this.unitPart.rb.AddForceAtPosition(force, this.hookEnd.position);
  }

  public void Unhook() => this.hooked = false;

  private void FixedUpdate()
  {
    if (!this.hooked)
    {
      this.deployedAmount += this.deployed ? 0.5f * Time.fixedDeltaTime : -0.5f * Time.fixedDeltaTime;
      this.deployedAmount = Mathf.Clamp01(this.deployedAmount);
      RaycastHit hitInfo;
      if (Physics.Linecast(this.hinge.position, this.hookEnd.position, out hitInfo) && (double) hitInfo.distance < (double) this.hookLength)
      {
        ArrestorGear component = hitInfo.collider.GetComponent<ArrestorGear>();
        if ((UnityEngine.Object) component != (UnityEngine.Object) null && component.Hook(this))
        {
          this.hooked = true;
          this.aircraft.ShakeAircraft(0.5f, 0.0f);
          if ((UnityEngine.Object) this.aircraft == (UnityEngine.Object) SceneSingleton<CombatHUD>.i.aircraft)
          {
            if (component.wireNumber == 1)
              SceneSingleton<AircraftActionsReport>.i.ReportText("Caught 1st Wire", 4f);
            else if (component.wireNumber == 2)
              SceneSingleton<AircraftActionsReport>.i.ReportText("Caught 2nd Wire", 4f);
            else if (component.wireNumber == 3)
              SceneSingleton<AircraftActionsReport>.i.ReportText("Caught 3rd Wire", 4f);
            else if (component.wireNumber == 4)
              SceneSingleton<AircraftActionsReport>.i.ReportText("Caught 4th Wire", 4f);
          }
        }
        this.deployedAmount -= 4f * Time.deltaTime;
        if ((UnityEngine.Object) hitInfo.collider.sharedMaterial != (UnityEngine.Object) GameAssets.i.terrainMaterial)
          this.aircraft.ThrowSparks(hitInfo.point, Vector3.zero);
      }
    }
    this.hook.localEulerAngles = new Vector3(Mathf.Lerp(this.stowedAngle, this.deployedAngle, this.deployedAmount), 0.0f, 0.0f);
    if ((double) this.deployedAmount != 0.0)
      return;
    this.enabled = false;
  }
}
