// Decompiled with JetBrains decompiler
// Type: SlingloadHook
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

#nullable disable
public class SlingloadHook : Weapon
{
  public SlingloadHook.DeployState deployState;
  [SerializeField]
  public Transform winch;
  [SerializeField]
  public Transform hook;
  [SerializeField]
  private AudioSource winchAudioSource;
  [SerializeField]
  private AudioSource hookAudioSource;
  [SerializeField]
  private AudioClip hookSound;
  [SerializeField]
  private AudioClip unhookSound;
  [SerializeField]
  private AudioClip slingSnapSound;
  [SerializeField]
  private AudioClip winchStartSound;
  [SerializeField]
  private AudioClip winchStopSound;
  [SerializeField]
  private LineRenderer lineRenderer;
  [SerializeField]
  private float lineMaxLength = 20f;
  [SerializeField]
  private float reelingSpeed = 3f;
  [SerializeField]
  private float breakForce = 500000f;
  [SerializeField]
  private float breakAngle = 120f;
  private Aircraft aircraft;
  private float lastTransformSent;
  private float lineLength;
  private Unit suspendedUnit;
  private Rigidbody body;
  private ConfigurableJoint joint;
  public float loadForce;
  private Vector3 hookTargetPos;
  private bool loadInWater;

  public override void AttachToUnit(Unit unit)
  {
    base.AttachToUnit(unit);
    this.aircraft = this.attachedUnit as Aircraft;
    this.ammo = 1;
    this.body = this.GetComponentInParent<Rigidbody>();
    this.lineRenderer.enabled = false;
    this.lineRenderer.useWorldSpace = true;
  }

  public override int GetAmmoLoaded() => 1;

  public override int GetAmmoTotal() => 1;

  public override void Fire(
    Unit owner,
    Unit target,
    Vector3 inheritedVelocity,
    WeaponStation weaponStation,
    GlobalPosition aimpoint)
  {
    weaponStation.UpdateLastFired(0);
    this.lastFired = Time.timeSinceLevelLoad;
    this.SetState();
  }

  public void SetState()
  {
    switch (this.deployState)
    {
      case SlingloadHook.DeployState.Retracted:
        this.aircraft.SetSlingLoadAttachment((Unit) null, SlingloadHook.DeployState.Deployed);
        break;
      case SlingloadHook.DeployState.Deployed:
        this.aircraft.SetSlingLoadAttachment((Unit) null, SlingloadHook.DeployState.Retracting);
        this.StartWinchSound();
        break;
      case SlingloadHook.DeployState.Connected:
        if (!(this.suspendedUnit is PilotDismounted))
        {
          this.hookAudioSource.PlayOneShot(this.unhookSound, 0.3f);
          if (!GameManager.IsLocalAircraft(this.aircraft))
            break;
          SceneSingleton<AircraftActionsReport>.i.ReportText("Load released - Retracting", 4f);
          this.aircraft.SetSlingLoadAttachment(this.suspendedUnit, SlingloadHook.DeployState.Retracting);
          break;
        }
        if (GameManager.IsLocalAircraft(this.aircraft))
          SceneSingleton<AircraftActionsReport>.i.ReportText("Start recovering pilot", 4f);
        this.aircraft.SetSlingLoadAttachment(this.suspendedUnit, SlingloadHook.DeployState.RescuePilot);
        this.StartWinchSound();
        break;
      case SlingloadHook.DeployState.Retracting:
        this.aircraft.SetSlingLoadAttachment((Unit) null, SlingloadHook.DeployState.Deployed);
        this.lineRenderer.enabled = true;
        break;
      case SlingloadHook.DeployState.RescuePilot:
        this.hookAudioSource.PlayOneShot(this.unhookSound, 0.3f);
        if (!GameManager.IsLocalAircraft(this.aircraft))
          break;
        SceneSingleton<AircraftActionsReport>.i.ReportText("Stop pilot recovery - Retracting", 4f);
        this.aircraft.SetSlingLoadAttachment(this.suspendedUnit, SlingloadHook.DeployState.RescuePilot);
        break;
    }
  }

  public void ApplyState(Unit suspendedUnit, SlingloadHook.DeployState deployState)
  {
    if (deployState == this.deployState)
      return;
    this.deployState = deployState;
    this.suspendedUnit = suspendedUnit;
    this.enabled = deployState != 0;
    this.lineRenderer.enabled = this.enabled;
    switch (deployState)
    {
      case SlingloadHook.DeployState.Retracted:
        this.lineLength = 0.0f;
        this.UpdateRope();
        this.StopWinchSound();
        break;
      case SlingloadHook.DeployState.Deployed:
        this.StartWinchSound();
        break;
      case SlingloadHook.DeployState.Connected:
        this.StopWinchSound();
        if (this.attachedUnit.LocalSim)
          this.CreateJoint(suspendedUnit.rb, new Vector3(0.0f, 0.5f * suspendedUnit.definition.height, 0.0f));
        if (GameManager.IsLocalAircraft(this.attachedUnit))
          SceneSingleton<AircraftActionsReport>.i.ReportText("Load Connected", 4f);
        this.hookAudioSource.PlayOneShot(this.hookSound, 0.3f);
        this.lineLength = (this.winch.position - suspendedUnit.transform.position + new Vector3(0.0f, 0.5f * suspendedUnit.definition.height, 0.0f)).magnitude;
        break;
      case SlingloadHook.DeployState.Retracting:
        if (this.attachedUnit.LocalSim && (Object) this.joint != (Object) null)
          Object.Destroy((Object) this.joint);
        this.loadForce = 0.0f;
        this.StartWinchSound();
        break;
      case SlingloadHook.DeployState.RescuePilot:
        this.StartWinchSound();
        break;
    }
  }

  private void StartWinchSound()
  {
    if (this.winchAudioSource.isPlaying)
      return;
    this.winchAudioSource.Play();
    this.winchAudioSource.pitch = 1f;
    this.winchAudioSource.volume = 1f;
    this.winchAudioSource.PlayOneShot(this.winchStartSound);
  }

  private void StopWinchSound()
  {
    if (!this.winchAudioSource.isPlaying)
      return;
    this.winchAudioSource.Stop();
    this.winchAudioSource.pitch = 1f;
    this.winchAudioSource.volume = 1f;
    this.winchAudioSource.PlayOneShot(this.winchStopSound);
  }

  private void RetractingState()
  {
    this.lineLength -= this.reelingSpeed * Time.deltaTime;
    this.winchAudioSource.pitch = (float) (1.0 - 0.10000000149011612 * (double) this.lineLength / (double) this.lineMaxLength);
    if ((double) this.lineLength > 0.0)
      return;
    this.aircraft.SetSlingLoadAttachment((Unit) null, SlingloadHook.DeployState.Retracted);
    this.lineLength = 0.0f;
    this.deployState = SlingloadHook.DeployState.Retracted;
    this.lineRenderer.enabled = false;
    this.StopWinchSound();
  }

  private void DeployedState()
  {
    if ((double) this.lineLength < (double) this.lineMaxLength)
    {
      this.lineLength += this.reelingSpeed * Time.deltaTime;
      this.winchAudioSource.pitch = (float) (0.949999988079071 + 0.10000000149011612 * (double) this.lineLength / (double) this.lineMaxLength);
      if ((double) this.lineLength >= (double) this.lineMaxLength)
      {
        this.lineLength = this.lineMaxLength;
        this.StopWinchSound();
      }
    }
    if (!this.aircraft.LocalSim || this.aircraft.weaponManager.GetTargetList().Count == 0)
      return;
    Unit target = this.aircraft.weaponManager.GetTargetList()[0];
    if (!((Object) target != (Object) null) || !target.definition.CanSlingLoad || !FastMath.InRange(this.body.velocity, target.rb.velocity, 8f) || target.IsSlung())
      return;
    this.hookTargetPos = target.transform.position + 0.5f * target.definition.height * target.transform.up;
    if ((double) FastMath.Distance(this.hook.position, this.hookTargetPos) + (double) FastMath.Distance(this.winch.position, this.hookTargetPos) >= (double) this.lineLength)
      return;
    this.aircraft.SetSlingLoadAttachment(target, SlingloadHook.DeployState.Connected);
  }

  private void ConnectedState()
  {
    Vector3 to = this.winch.position - this.hook.position;
    float magnitude = to.magnitude;
    Vector3 force1 = Vector3.zero;
    if ((double) this.lineLength < (double) this.lineMaxLength)
    {
      if ((double) this.lineLength < (double) magnitude)
      {
        float num = Mathf.Clamp01(magnitude - this.lineLength) * Mathf.Min(this.suspendedUnit.rb.mass, 20000f);
        force1 = to.normalized * num;
      }
      this.winchAudioSource.pitch = Mathf.Min(this.reelingSpeed, magnitude - this.lineLength) / this.reelingSpeed;
      this.lineLength += this.reelingSpeed * Time.deltaTime;
    }
    if ((double) this.lineLength > (double) this.lineMaxLength)
    {
      this.lineLength = this.lineMaxLength;
      this.StopWinchSound();
    }
    if (!this.aircraft.LocalSim)
      return;
    this.loadForce = this.joint.currentForce.magnitude;
    if ((this.attachedUnit.disabled || this.suspendedUnit.disabled || Physics.Linecast(this.winch.position, Vector3.Lerp(this.winch.position, this.hook.position, 0.8f), out RaycastHit _, -8193) || (double) Vector3.Angle(this.attachedUnit.transform.up, to) > (double) this.breakAngle ? 1 : ((double) this.loadForce > (double) this.breakForce ? 1 : 0)) != 0)
    {
      this.BreakRope();
    }
    else
    {
      if (this.loadInWater)
      {
        Vector3 force2 = Vector3.up * this.suspendedUnit.rb.mass * 25f * Mathf.Clamp01(Datum.LocalSeaY - this.suspendedUnit.transform.position.y);
        Vector3 torque = Vector3.Cross(this.suspendedUnit.transform.up, Vector3.up) * this.suspendedUnit.rb.mass * 5f;
        this.suspendedUnit.rb.AddForce(force2);
        this.suspendedUnit.rb.AddTorque(torque, ForceMode.Force);
      }
      this.suspendedUnit.rb.AddForceAtPosition(force1, this.hook.position);
      this.body.AddForceAtPosition(-force1, this.winch.position);
    }
  }

  private void RescuePilotState()
  {
    Vector3 to = this.winch.position - this.hook.position;
    float magnitude = to.magnitude;
    if ((double) magnitude < 1.0 && this.aircraft.IsServer && (Object) this.suspendedUnit != (Object) null)
    {
      this.lineLength = 1f;
      this.RescuePilot(this.suspendedUnit as PilotDismounted);
    }
    else
    {
      if (!this.aircraft.LocalSim)
        return;
      if ((Object) this.suspendedUnit == (Object) null)
      {
        this.aircraft.SetSlingLoadAttachment((Unit) null, SlingloadHook.DeployState.Retracting);
      }
      else
      {
        this.loadForce = this.joint.currentForce.magnitude;
        if ((double) this.lineLength > 4.0)
        {
          bool flag = this.attachedUnit.disabled || this.suspendedUnit.disabled || (double) Vector3.Angle(this.attachedUnit.transform.up, to) > (double) this.breakAngle || (double) this.loadForce > (double) this.breakForce;
          RaycastHit hitInfo;
          IDamageable component;
          if (Physics.Linecast(Vector3.Lerp(this.winch.position, this.hook.position, 0.1f), Vector3.Lerp(this.winch.position, this.hook.position, 0.9f), out hitInfo, -8193) && (!hitInfo.collider.gameObject.TryGetComponent<IDamageable>(out component) || (Object) component.GetUnit() != (Object) this.aircraft && (Object) component.GetUnit() != (Object) this.suspendedUnit))
            flag = true;
          if (flag)
          {
            this.BreakRope();
            return;
          }
        }
        float num = 20f;
        Vector3 vector3_1 = new Vector3(to.normalized.x, 0.0f, to.normalized.z);
        Vector3 vector3_2 = Vector3.zero;
        if ((double) this.suspendedUnit.rb.velocity.y > (double) this.attachedUnit.rb.velocity.y + 3.0)
          num /= 2f;
        Vector3 vector3_3 = num * this.suspendedUnit.rb.mass * to.normalized;
        if ((double) this.GetRopeAngle() > 5.0)
        {
          Vector3 vector3_4 = new Vector3(this.attachedUnit.rb.velocity.x - this.suspendedUnit.rb.velocity.x, 0.0f, this.attachedUnit.rb.velocity.z - this.suspendedUnit.rb.velocity.z);
          vector3_2 = 10f * this.suspendedUnit.rb.mass * vector3_1 + 0.1f * this.suspendedUnit.rb.mass * vector3_4 / Time.fixedDeltaTime;
        }
        Vector3 torque = this.suspendedUnit.rb.mass * 4f * Vector3.Cross(this.suspendedUnit.transform.up, Vector3.up);
        if (this.loadInWater)
          this.suspendedUnit.rb.AddForce(this.suspendedUnit.rb.mass * 25f * Mathf.Clamp01(Datum.LocalSeaY - this.suspendedUnit.transform.position.y) * Vector3.up);
        Vector3 force = vector3_3 + vector3_2;
        this.suspendedUnit.rb.AddForceAtPosition(force, this.hook.position);
        this.body.AddForceAtPosition(-force, this.winch.position);
        this.suspendedUnit.rb.AddTorque(torque, ForceMode.Force);
        this.lineLength = magnitude;
        this.winchAudioSource.pitch = (float) (0.60000002384185791 - (double) this.lineLength / (double) this.lineMaxLength * 0.10000000149011612);
      }
    }
  }

  public void FixedUpdate()
  {
    switch (this.deployState)
    {
      case SlingloadHook.DeployState.Deployed:
        this.DeployedState();
        break;
      case SlingloadHook.DeployState.Connected:
        this.ConnectedState();
        break;
      case SlingloadHook.DeployState.Retracting:
        this.RetractingState();
        break;
      case SlingloadHook.DeployState.RescuePilot:
        this.RescuePilotState();
        break;
    }
    if (!((Object) this.suspendedUnit != (Object) null) || !this.aircraft.LocalSim || this.aircraft.IsServer || (double) Time.timeSinceLevelLoad - (double) this.lastTransformSent <= 0.10000000149011612)
      return;
    this.lastTransformSent = Time.timeSinceLevelLoad;
    this.aircraft.CmdSendSlungTransform(this.aircraft.transform.InverseTransformPoint(this.suspendedUnit.transform.position), this.suspendedUnit.transform.rotation);
  }

  private void Update()
  {
    this.UpdateRope();
    if ((Object) this.suspendedUnit == (Object) null)
      return;
    if (!this.loadInWater)
    {
      if ((double) this.suspendedUnit.transform.position.y >= (double) Datum.LocalSeaY)
        return;
      this.LoadEnterWater();
    }
    else
    {
      if ((double) this.suspendedUnit.transform.position.y <= (double) Datum.LocalSeaY)
        return;
      this.LoadExitWater();
    }
  }

  private void LoadEnterWater()
  {
    this.loadInWater = true;
    this.suspendedUnit.rb.drag = 0.1f;
    this.suspendedUnit.rb.angularDrag = 0.2f;
  }

  private void LoadExitWater()
  {
    this.loadInWater = false;
    this.suspendedUnit.rb.drag = 0.02f;
    this.suspendedUnit.rb.angularDrag = 0.1f;
  }

  private void BreakRope()
  {
    this.hookAudioSource.PlayOneShot(this.slingSnapSound);
    if (GameManager.IsLocalAircraft(this.aircraft))
      SceneSingleton<AircraftActionsReport>.i.ReportText("Load Lost - Retracting", 4f);
    this.aircraft.SetSlingLoadAttachment(this.suspendedUnit, SlingloadHook.DeployState.Retracting);
  }

  private void RescuePilot(PilotDismounted rescuedPilot)
  {
    if (this.aircraft.IsServer)
      rescuedPilot.Capture(this.attachedUnit);
    if (!GameManager.IsLocalAircraft(this.aircraft))
      return;
    SceneSingleton<AircraftActionsReport>.i.ReportText("Pilot rescued", 4f);
  }

  private void CreateJoint(Rigidbody target, Vector3 attachPoint)
  {
    this.joint = this.body.gameObject.AddComponent<ConfigurableJoint>();
    this.joint.autoConfigureConnectedAnchor = false;
    this.joint.anchor = this.winch.localPosition;
    this.joint.connectedAnchor = attachPoint;
    this.joint.connectedBody = target;
    SoftJointLimit softJointLimit = new SoftJointLimit();
    SoftJointLimitSpring jointLimitSpring = new SoftJointLimitSpring();
    softJointLimit.limit = this.lineMaxLength;
    jointLimitSpring.spring = this.suspendedUnit.rb.mass * 100f;
    jointLimitSpring.damper = 1000f;
    this.joint.linearLimit = softJointLimit;
    this.joint.linearLimitSpring = jointLimitSpring;
    this.joint.xMotion = ConfigurableJointMotion.Limited;
    this.joint.yMotion = ConfigurableJointMotion.Limited;
    this.joint.zMotion = ConfigurableJointMotion.Limited;
    this.joint.angularXMotion = ConfigurableJointMotion.Free;
    this.joint.angularYMotion = ConfigurableJointMotion.Free;
    this.joint.angularZMotion = ConfigurableJointMotion.Free;
  }

  private void UpdateRope()
  {
    if (this.deployState == SlingloadHook.DeployState.Connected || this.deployState == SlingloadHook.DeployState.RescuePilot)
    {
      if ((Object) this.suspendedUnit != (Object) null)
      {
        this.hookTargetPos = this.suspendedUnit.transform.position + 0.5f * this.suspendedUnit.definition.height * this.suspendedUnit.transform.up;
        this.hook.position = this.hookTargetPos;
      }
    }
    else
    {
      float num = this.lineLength;
      if ((double) num > (double) this.attachedUnit.radarAlt)
        num = this.attachedUnit.radarAlt;
      this.hook.position = this.winch.position + Vector3.down * num;
    }
    for (int index = 0; index < 10; ++index)
      this.lineRenderer.SetPosition(index, Vector3.Lerp(this.winch.position, this.hook.position, (float) index * 0.111f));
  }

  public float GetLineLength() => this.lineLength;

  public float GetLineMaxLength() => this.lineMaxLength;

  public Unit GetSuspendedUnit() => this.suspendedUnit;

  public float GetRopeFactor()
  {
    return Mathf.Clamp01(this.joint.currentForce.magnitude / this.breakForce);
  }

  public float GetRopeAngle()
  {
    return Vector3.Angle(-this.attachedUnit.transform.up, this.suspendedUnit.transform.position - this.attachedUnit.transform.position);
  }

  public enum DeployState : byte
  {
    Retracted,
    Deployed,
    Connected,
    Retracting,
    RescuePilot,
  }
}
