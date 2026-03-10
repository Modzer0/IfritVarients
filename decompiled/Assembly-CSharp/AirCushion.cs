// Decompiled with JetBrains decompiler
// Type: AirCushion
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;

#nullable disable
public class AirCushion : MonoBehaviour, IThrustSource
{
  [SerializeField]
  private Unit attachedUnit;
  [SerializeField]
  private float spring;
  [SerializeField]
  private float damp;
  [SerializeField]
  private float aligningStrength;
  [SerializeField]
  private float maxHeight;
  [SerializeField]
  private float sinkRate;
  [SerializeField]
  private float dragForward;
  [SerializeField]
  private float dragLateral;
  [SerializeField]
  private float steerAxisDamping;
  [SerializeField]
  private float conditionMin;
  [SerializeField]
  private Transform castTransform;
  [SerializeField]
  private UnitPart[] criticalParts;
  private float currentThrust;
  private float condition;
  private float inflatedSpring;
  private float inflatedDamp;
  private float lastSurfaceSample;
  private bool sinking;
  private bool deflating;
  private bool inflating;
  private bool overLand;
  private Vector3 thrustApplyOffset;
  private Plane surfacePlane;

  public float GetMaxThrust() => this.spring * this.maxHeight;

  public float GetThrust() => this.currentThrust;

  private void Awake()
  {
    this.CalcThrustOffset();
    this.attachedUnit.onDisableUnit += new Action<Unit>(this.AirCushion_OnUnitDisabled);
    foreach (UnitPart criticalPart in this.criticalParts)
      criticalPart.onApplyDamage += new Action<UnitPart.OnApplyDamage>(this.AirCushion_OnCriticalPartDamage);
    this.inflatedSpring = this.spring;
    this.inflatedDamp = this.damp;
  }

  private void AirCushion_OnCriticalPartDamage(UnitPart.OnApplyDamage e) => this.CalcThrustOffset();

  private void CalcThrustOffset()
  {
    float num1 = 0.0f;
    this.thrustApplyOffset = Vector3.zero;
    foreach (UnitPart criticalPart in this.criticalParts)
    {
      num1 += criticalPart.hitPoints;
      this.thrustApplyOffset += this.attachedUnit.transform.InverseTransformPoint(criticalPart.transform.position) * criticalPart.hitPoints;
    }
    int num2 = 100 * this.criticalParts.Length;
    this.condition = (float) (((double) Mathf.Max(num1 / (float) num2, 0.0f) - (double) this.conditionMin) / (1.0 - (double) this.conditionMin));
    this.thrustApplyOffset /= (float) (this.criticalParts.Length * 100);
    this.thrustApplyOffset.y = 0.0f;
  }

  public bool Landed() => this.overLand;

  public void Deflate()
  {
    this.deflating = true;
    this.inflating = false;
  }

  public void Inflate()
  {
    this.inflating = true;
    this.deflating = false;
  }

  private void AirCushion_OnUnitDisabled(Unit unit) => this.sinking = true;

  private void SampleSurface()
  {
    if ((double) Time.timeSinceLevelLoad - (double) this.lastSurfaceSample < 0.20000000298023224)
      return;
    this.lastSurfaceSample = Time.timeSinceLevelLoad;
    this.surfacePlane.SetNormalAndPosition(Vector3.up, Vector3.zero);
    ref Plane local1 = ref this.surfacePlane;
    GlobalPosition globalPosition = this.castTransform.position.ToGlobalPosition();
    Ray ray = new Ray(globalPosition.AsVector3(), -this.castTransform.up);
    float num1;
    ref float local2 = ref num1;
    int num2 = !local1.Raycast(ray, out local2) || (double) this.castTransform.position.y <= (double) Datum.LocalSeaY || (double) num1 <= 0.0 ? 0 : ((double) num1 < (double) this.maxHeight ? 1 : 0);
    RaycastHit hitInfo;
    bool flag = Physics.Linecast(this.castTransform.position, this.castTransform.position - this.castTransform.up * this.maxHeight, out hitInfo);
    float num3 = num2 != 0 ? num1 : this.maxHeight;
    this.overLand = flag && (double) hitInfo.distance < (double) num3;
    if (!this.overLand)
      return;
    ref Plane local3 = ref this.surfacePlane;
    Vector3 normal = hitInfo.normal;
    globalPosition = hitInfo.point.ToGlobalPosition();
    Vector3 inPoint = globalPosition.AsVector3();
    local3.SetNormalAndPosition(normal, inPoint);
  }

  private void FixedUpdate()
  {
    if (this.sinking || this.deflating)
    {
      this.spring -= this.sinkRate * Time.fixedDeltaTime;
      this.transform.localScale = new Vector3(1f, Mathf.Lerp(0.5f, 1f, this.spring / this.inflatedSpring), 1f);
      if ((double) this.spring <= 0.0)
      {
        this.spring = 0.0f;
        this.damp = 0.0f;
        this.deflating = false;
      }
    }
    if (this.inflating)
    {
      this.spring += this.sinkRate * Time.fixedDeltaTime;
      this.damp = this.inflatedDamp;
      this.transform.localScale = new Vector3(1f, Mathf.Lerp(0.5f, 1f, this.spring / this.inflatedSpring), 1f);
      if ((double) this.spring >= (double) this.inflatedSpring)
      {
        this.transform.localScale = Vector3.one;
        this.spring = this.inflatedSpring;
        this.inflating = false;
      }
    }
    if ((double) this.spring <= 0.0)
      return;
    this.SampleSurface();
    float enter;
    if (!this.surfacePlane.Raycast(new Ray(this.castTransform.position.ToGlobalPosition().AsVector3(), -this.castTransform.up), out enter) || (double) enter >= (double) this.maxHeight || (double) enter <= 0.0)
      return;
    Vector3 vector3_1 = this.castTransform.InverseTransformDirection(this.attachedUnit.rb.angularVelocity);
    float num1 = Mathf.Max(this.maxHeight - enter, 0.0f) * this.spring;
    float num2 = this.damp * Vector3.Dot(this.attachedUnit.rb.velocity, -this.surfacePlane.normal);
    float num3 = -vector3_1.y * this.steerAxisDamping * this.attachedUnit.rb.mass;
    float f1 = Vector3.Dot(this.attachedUnit.rb.velocity, this.castTransform.forward);
    float f2 = Vector3.Dot(this.attachedUnit.rb.velocity, this.castTransform.right);
    Vector3 vector3_2 = Mathf.Abs(f1) * f1 * this.castTransform.forward * this.dragForward + Mathf.Abs(f2) * f2 * this.castTransform.right * this.dragLateral;
    if (this.overLand)
      vector3_2 *= 2f;
    Vector3 vector3_3 = 50f * -Vector3.Cross(this.surfacePlane.normal, this.castTransform.up) - 15f * this.attachedUnit.rb.angularVelocity;
    this.currentThrust = Mathf.Max(num1 + num2, 0.0f);
    this.attachedUnit.rb.AddForceAtPosition((this.currentThrust * this.surfacePlane.normal - vector3_2) * this.condition, this.castTransform.TransformPoint(this.thrustApplyOffset));
    this.attachedUnit.rb.AddTorque((vector3_3 * this.attachedUnit.rb.mass * this.aligningStrength + this.castTransform.up * num3) * this.condition);
  }
}
