// Decompiled with JetBrains decompiler
// Type: Parachute
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;

#nullable disable
public class Parachute : MonoBehaviour
{
  [SerializeField]
  private Unit attachedUnit;
  [SerializeField]
  private GameObject canopy;
  [SerializeField]
  private GameObject lines;
  [SerializeField]
  private float canopyMass;
  [SerializeField]
  private float lineSpring;
  [SerializeField]
  private float lineSlackLength;
  [SerializeField]
  private float maxRadius;
  [SerializeField]
  private float maxDrag;
  [SerializeField]
  private float damping = 1f;
  [SerializeField]
  private float openDelayMin;
  [SerializeField]
  private float openDelayMax;
  [SerializeField]
  private float openAltitudeMin;
  [SerializeField]
  private float openAltitudeMax;
  [SerializeField]
  private float openSpeedMin;
  [SerializeField]
  private float openSpeedMax;
  [SerializeField]
  private Renderer parachuteRenderer;
  private Material parachuteMaterial;
  private Vector3 lineVector;
  private Vector3 groundNormal = Vector3.up;
  private float lineLength;
  private float lineTension;
  private float openAmount;
  private float canopyDisplacement;
  private Vector3 canopyVel;
  private Vector3 canopyForce;
  private Vector3 repelForce;
  [SerializeField]
  private AnimationCurve chuteDrag;
  [SerializeField]
  private AnimationCurve chuteScaleVertical;
  [SerializeField]
  private AnimationCurve chuteScaleHorizontal;
  [SerializeField]
  private AnimationCurve wrinkleStrength;
  private float landedTime;
  private float timeSinceSpawn;
  private static int id_wrinkleDisplacement = Shader.PropertyToID("_wrinkleDisplacement");
  private static int id_wrinkleStrength = Shader.PropertyToID("_wrinkleStrength");
  public Action onUnitLanded;

  public void SetAttachedUnit(Unit unit) => this.attachedUnit = unit;

  public void DeployChute()
  {
    this.parachuteMaterial = this.parachuteRenderer.material;
    this.repelForce = Vector3.zero;
    this.groundNormal = Vector3.zero;
    this.canopy.SetActive(true);
    this.lines.SetActive(true);
    this.canopy.transform.SetParent((Transform) null);
    this.canopyVel = this.attachedUnit.rb.velocity;
    this.canopy.transform.position = this.transform.position - this.attachedUnit.rb.velocity.normalized * 2f;
  }

  public void FixedUpdate()
  {
    this.timeSinceSpawn += Time.fixedDeltaTime;
    this.attachedUnit.CheckRadarAlt();
    if (!this.canopy.activeSelf)
    {
      float num = (UnityEngine.Object) this.attachedUnit.rb != (UnityEngine.Object) null ? this.attachedUnit.rb.velocity.magnitude : 0.0f;
      if ((double) this.attachedUnit.radarAlt > (double) this.openAltitudeMin && (double) this.attachedUnit.radarAlt < (double) this.openAltitudeMax && (double) this.timeSinceSpawn > (double) this.openDelayMin && (double) this.timeSinceSpawn < (double) this.openDelayMax && (double) num > (double) this.openSpeedMin && (double) num < (double) this.openSpeedMax)
      {
        this.DeployChute();
        if (this.attachedUnit is PilotDismounted attachedUnit)
          attachedUnit.DeployChute();
      }
      if (!this.attachedUnit.rb.isKinematic)
        return;
      UnityEngine.Object.Destroy((UnityEngine.Object) this.gameObject);
    }
    else
    {
      float num1 = GameAssets.i.airDensityAltitude.Evaluate(this.transform.position.GlobalY() * (1f / 1000f));
      this.lineVector = this.canopy.transform.position - this.transform.position;
      this.lineLength = this.lineVector.magnitude;
      this.lineTension = Mathf.Max(this.lineLength - this.lineSlackLength, 0.0f) * this.lineSpring;
      Vector3 vector3 = this.canopyVel - NetworkSceneSingleton<LevelInfo>.i.GetWind(this.transform.GlobalPosition());
      float magnitude = vector3.magnitude;
      this.canopyDisplacement += magnitude * Time.deltaTime;
      float num2 = this.maxDrag * this.chuteDrag.Evaluate(this.openAmount) * num1;
      float num3 = -this.wrinkleStrength.Evaluate(this.openAmount) / Mathf.Clamp(this.lineTension * 0.0005f, 0.5f, 2f);
      this.canopyForce = -Vector3.up * this.canopyMass * 9.81f - this.lineVector.normalized * this.lineTension - vector3 * num2;
      if ((double) this.landedTime == 0.0)
      {
        this.openAmount += Mathf.Min(magnitude * 0.03f, 1f) * Time.deltaTime;
        this.canopyForce -= this.repelForce;
        this.lineTension = Mathf.Min(this.lineTension, this.attachedUnit.rb.mass * 200f);
        this.attachedUnit.rb.AddForceAtPosition(this.lineVector.normalized * this.lineTension, this.transform.position);
        this.attachedUnit.rb.AddTorque(Vector3.Cross(-this.attachedUnit.transform.up, -this.lineVector.normalized * this.lineTension) * this.attachedUnit.rb.mass * (1f / 1000f) * this.damping, ForceMode.Force);
        this.attachedUnit.rb.angularDrag = 2f;
        this.canopy.transform.LookAt(this.transform.position, this.attachedUnit.transform.forward);
      }
      this.canopyVel += this.canopyForce / this.canopyMass * Time.deltaTime;
      this.canopy.transform.position += this.canopyVel * Time.deltaTime;
      this.parachuteMaterial.SetFloat("_wrinkleDisplacment", this.canopyDisplacement);
      this.parachuteMaterial.SetFloat("_wrinkleStrength", num3);
    }
  }

  public void Update()
  {
    if ((UnityEngine.Object) this.attachedUnit == (UnityEngine.Object) null)
    {
      UnityEngine.Object.Destroy((UnityEngine.Object) this.gameObject);
    }
    else
    {
      if (!this.canopy.activeSelf)
        return;
      if ((double) this.attachedUnit.radarAlt < 2.0)
      {
        this.landedTime += Time.deltaTime;
        if ((double) this.groundNormal.magnitude == 0.0)
          this.CheckGroundNormal();
        if ((double) this.landedTime > 5.0)
        {
          Action onUnitLanded = this.onUnitLanded;
          if (onUnitLanded != null)
            onUnitLanded();
          UnityEngine.Object.Destroy((UnityEngine.Object) this.gameObject);
        }
        this.openAmount = Mathf.Lerp(this.openAmount, 0.0f, 0.5f * Time.deltaTime);
        this.maxDrag = Mathf.Lerp(this.maxDrag, 0.0f, Time.deltaTime);
        Vector3 vector3 = (this.canopy.transform.position - this.attachedUnit.transform.position + NetworkSceneSingleton<LevelInfo>.i.GetWind(this.transform.GlobalPosition()).normalized) with
        {
          y = 0.0f
        };
        Vector3 forward = Vector3.Cross(this.groundNormal, vector3.normalized);
        if (forward != Vector3.zero)
          this.canopy.transform.rotation = Quaternion.Slerp(this.canopy.transform.rotation, Quaternion.LookRotation(forward), 0.5f * Time.deltaTime);
        this.canopy.transform.position += (vector3.normalized - Vector3.up) * Time.deltaTime / (1f + this.landedTime);
      }
      float num = this.maxRadius * this.chuteScaleHorizontal.Evaluate(this.openAmount);
      this.canopy.transform.localScale = new Vector3(num, num, this.maxRadius * this.chuteScaleVertical.Evaluate(this.openAmount));
      this.lines.transform.localScale = new Vector3(num, num, this.lineLength);
      this.lines.transform.LookAt(this.canopy.transform.position, this.attachedUnit.transform.forward);
    }
  }

  public void CutCanopy()
  {
    this.canopy.SetActive(false);
    this.lines.SetActive(false);
  }

  public bool IsOpen() => this.canopy.activeSelf;

  public void CheckGroundNormal()
  {
    RaycastHit hitInfo;
    if (!Physics.Raycast(this.transform.position, -Vector3.up, out hitInfo, float.MaxValue, 2112))
      return;
    this.groundNormal = hitInfo.normal;
  }

  protected void OnDestroy()
  {
    if ((UnityEngine.Object) this.canopy != (UnityEngine.Object) null)
      UnityEngine.Object.Destroy((UnityEngine.Object) this.canopy);
    if (!((UnityEngine.Object) this.lines != (UnityEngine.Object) null))
      return;
    UnityEngine.Object.Destroy((UnityEngine.Object) this.lines);
  }

  public float GetCurrentRadius()
  {
    return this.maxRadius * this.chuteScaleHorizontal.Evaluate(this.openAmount);
  }

  public Vector3 GetCanopyPosition()
  {
    return !((UnityEngine.Object) this.canopy == (UnityEngine.Object) null) ? this.canopy.transform.position : Vector3.zero;
  }

  public void AddRepelForce(Vector3 force) => this.repelForce = force;
}
