// Decompiled with JetBrains decompiler
// Type: CargoRamp
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

#nullable disable
public class CargoRamp : BayDoor
{
  private CargoRamp.RampState rampState = CargoRamp.RampState.closing;
  [SerializeField]
  private AudioClip openStopSound;
  [SerializeField]
  private AudioClip closeStopSound;
  [SerializeField]
  private AeroPart rampPart;
  [SerializeField]
  private AeroPart attachedPart;
  [SerializeField]
  private AeroPart latchPart;
  [SerializeField]
  private float hingeSpring;
  [SerializeField]
  private float hingeDamp;
  [SerializeField]
  private float hingeStrength;
  [SerializeField]
  private CargoRamp.Strut[] struts;
  [SerializeField]
  private Transform rampLatch;
  [SerializeField]
  private Transform fuselageLatch;
  [SerializeField]
  private Transform rampSurfaceNormal;
  [SerializeField]
  private Renderer[] rampLightingRenderers;
  [SerializeField]
  private Light[] rampLights;
  private Joint latchJoint;
  private float defaultAngle;
  private float currentAngle;

  protected override void Awake()
  {
    this.rampPart.parentUnit.onInitialize += new Action(this.CargoRamp_OnSpawned);
  }

  private void Start()
  {
    if (this.rampPart.parentUnit.networked)
      return;
    this.CargoRamp_OnSpawned();
  }

  private async UniTask EnableLatch(bool report)
  {
    foreach (Renderer lightingRenderer in this.rampLightingRenderers)
      lightingRenderer.enabled = false;
    foreach (Behaviour rampLight in this.rampLights)
      rampLight.enabled = false;
    if (!this.rampPart.parentUnit.LocalSim && this.rampPart.parentUnit.networked)
      return;
    await UniTask.WaitForFixedUpdate();
    if (report && GameManager.IsLocalAircraft(this.rampPart.parentUnit))
      SceneSingleton<AircraftActionsReport>.i.ReportText("Locking Cargo Door", 3f);
    this.latchJoint = (Joint) this.rampPart.gameObject.AddComponent<FixedJoint>();
    this.latchJoint.connectedBody = this.latchPart.rb;
    this.latchJoint.breakForce = this.hingeStrength * 0.5f;
    this.latchJoint.breakTorque = this.hingeStrength * 0.5f;
    this.latchJoint.enableCollision = false;
  }

  public bool IsOpen()
  {
    return (double) Vector3.Dot(this.rampSurfaceNormal.forward, this.attachedPart.transform.up) > 0.0;
  }

  private void DisableLatch()
  {
    foreach (Renderer lightingRenderer in this.rampLightingRenderers)
      lightingRenderer.enabled = true;
    foreach (Behaviour rampLight in this.rampLights)
      rampLight.enabled = true;
    if (!this.rampPart.parentUnit.LocalSim && this.rampPart.parentUnit.networked || !((UnityEngine.Object) this.latchJoint != (UnityEngine.Object) null))
      return;
    if (GameManager.IsLocalAircraft(this.rampPart.parentUnit))
      SceneSingleton<AircraftActionsReport>.i.ReportText("Unlocking Cargo Door", 3f);
    UnityEngine.Object.Destroy((UnityEngine.Object) this.latchJoint);
  }

  private void CargoRamp_OnSpawned()
  {
    this.EnableLatch(false).Forget();
    this.rampPart.SetHingeJoint(0, this.attachedPart, this.hingeSpring, this.hingeDamp, this.currentAngle, this.hingeStrength, this.defaultAngle);
    foreach (CargoRamp.Strut strut in this.struts)
      strut.Animate();
    this.rampPart.onPartDetached += new Action<UnitPart>(this.CargoRamp_OnDetached);
    this.rampPart.parentUnit.onInitialize -= new Action(this.CargoRamp_OnSpawned);
    this.enabled = false;
  }

  public override void OpenDoor(float duration)
  {
    this.DisableLatch();
    this.enabled = true;
    this.openTimer = duration;
    this.rampState = CargoRamp.RampState.opening;
  }

  private void Opening()
  {
    this.currentAngle += Mathf.Clamp(this.hingeAngle - this.currentAngle, -this.openSpeed * Time.fixedDeltaTime, this.openSpeed * Time.fixedDeltaTime);
    this.rampPart.SetHingeJoint(0, this.attachedPart, this.hingeSpring, this.hingeDamp, this.currentAngle, this.hingeStrength, this.defaultAngle);
    if ((double) this.currentAngle == (double) this.hingeAngle)
    {
      this.rampState = CargoRamp.RampState.open;
      if (!((UnityEngine.Object) this.doorAudioSource != (UnityEngine.Object) null))
        return;
      this.doorAudioSource.Stop();
      this.doorAudioSource.clip = this.openStopSound;
      this.doorAudioSource.Play();
    }
    else
    {
      if (!((UnityEngine.Object) this.doorAudioSource != (UnityEngine.Object) null) || !((UnityEngine.Object) this.doorAudioSource.clip != (UnityEngine.Object) this.openStartSound))
        return;
      this.doorAudioSource.Stop();
      this.doorAudioSource.clip = this.openStartSound;
      this.doorAudioSource.Play();
    }
  }

  private void Open()
  {
    this.openTimer -= Time.fixedDeltaTime;
    if ((double) this.openTimer > 0.0)
      return;
    this.rampState = CargoRamp.RampState.closing;
  }

  private void CargoRamp_OnDetached(UnitPart unitPart)
  {
    foreach (CargoRamp.Strut strut in this.struts)
      strut.Remove();
    this.DisableLatch();
    foreach (Renderer lightingRenderer in this.rampLightingRenderers)
      lightingRenderer.enabled = false;
    foreach (Behaviour rampLight in this.rampLights)
      rampLight.enabled = false;
    if ((UnityEngine.Object) this.doorAudioSource != (UnityEngine.Object) null)
      this.doorAudioSource.Stop();
    UnityEngine.Object.Destroy((UnityEngine.Object) this);
  }

  private void Closing()
  {
    this.currentAngle += Mathf.Clamp(2f - this.currentAngle, -this.closeSpeed * Time.fixedDeltaTime, this.closeSpeed * Time.fixedDeltaTime);
    this.rampPart.SetHingeJoint(0, this.attachedPart, this.hingeSpring, this.hingeDamp, this.currentAngle, this.hingeStrength, this.defaultAngle);
    if ((double) Vector3.Dot(this.rampLatch.position - this.fuselageLatch.position, this.fuselageLatch.forward) > 0.0 && !this.rampPart.IsDetached() && !this.latchPart.IsDetached())
    {
      if ((UnityEngine.Object) this.doorAudioSource != (UnityEngine.Object) null)
      {
        this.doorAudioSource.Stop();
        this.doorAudioSource.clip = this.closeStopSound;
        this.doorAudioSource.Play();
      }
      this.EnableLatch(true).Forget();
      this.enabled = false;
    }
    else
    {
      if (!((UnityEngine.Object) this.doorAudioSource != (UnityEngine.Object) null) || !((UnityEngine.Object) this.doorAudioSource.clip != (UnityEngine.Object) this.closeStartSound))
        return;
      this.doorAudioSource.Stop();
      this.doorAudioSource.clip = this.closeStartSound;
      this.doorAudioSource.Play();
    }
  }

  protected override void Update()
  {
    foreach (CargoRamp.Strut strut in this.struts)
      strut.Animate();
    switch (this.rampState)
    {
      case CargoRamp.RampState.opening:
        this.Opening();
        break;
      case CargoRamp.RampState.open:
        this.Open();
        break;
      case CargoRamp.RampState.closing:
        this.Closing();
        break;
    }
  }

  private enum RampState
  {
    opening,
    open,
    closing,
  }

  [Serializable]
  private struct Strut
  {
    [SerializeField]
    private Transform transform;
    [SerializeField]
    private Transform target;
    [SerializeField]
    private float maxLength;

    public void Animate()
    {
      this.transform.LookAt(this.target);
      float z = FastMath.Distance(this.target.position, this.transform.position);
      if ((double) z > (double) this.maxLength)
        this.Remove();
      this.transform.localScale = new Vector3(1f, 1f, z);
    }

    public void Remove() => this.transform.gameObject.GetComponent<Renderer>().enabled = false;
  }
}
