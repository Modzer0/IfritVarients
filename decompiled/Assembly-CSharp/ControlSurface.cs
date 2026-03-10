// Decompiled with JetBrains decompiler
// Type: ControlSurface
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.Jobs;
using System;
using UnityEngine;

#nullable disable
public class ControlSurface : MonoBehaviour
{
  [SerializeField]
  private float pitchRange;
  [SerializeField]
  private float rollRange;
  [SerializeField]
  private float yawRange;
  [SerializeField]
  private float brakeRange;
  [SerializeField]
  private UnitPart attachedSurface;
  [SerializeField]
  private GameObject visibleMesh;
  [SerializeField]
  private bool flap;
  [SerializeField]
  private float servoSpeed = 20f;
  [Header("Split Surface")]
  [SerializeField]
  private float splitDrag;
  [SerializeField]
  private Transform splitUpper;
  [SerializeField]
  private Transform splitLower;
  [SerializeField]
  private float maxSplit;
  [SerializeField]
  private float yawSplitFactor;
  private Aircraft aircraft;
  private float splitAmount;
  private ControlInputs controlInputs;
  private PtrAllocation<ControlSurfaceFields> JobFields;
  private NuclearOption.Jobs.JobPart<ControlSurface, ControlSurfaceFields> JobPart;

  private void Awake()
  {
    double maxSplit = (double) this.maxSplit;
    if ((UnityEngine.Object) this.attachedSurface.parentUnit != (UnityEngine.Object) null && this.attachedSurface.parentUnit is Aircraft)
    {
      this.aircraft = this.attachedSurface.parentUnit as Aircraft;
      this.aircraft.onInitialize += new Action(this.ControlSurface_OnInitialize);
      this.controlInputs = this.aircraft.GetInputs();
    }
    this.JobPart = new NuclearOption.Jobs.JobPart<ControlSurface, ControlSurfaceFields>(this, this.GetOrCreateJobField());
    JobManager.Add(this.JobPart);
  }

  private void OnValidate()
  {
    if ((double) this.maxSplit <= 0.0)
      return;
    if ((UnityEngine.Object) this.splitUpper == (UnityEngine.Object) null)
      Debug.LogError((object) $"maxSplit was above zero but splitUpper was null on {this}");
    if (!((UnityEngine.Object) this.splitLower == (UnityEngine.Object) null))
      return;
    Debug.LogError((object) $"maxSplit was above zero but splitLower was null on {this}");
  }

  ~ControlSurface() => ControlSurface.DisposeJobFields(ref this.JobFields);

  private static void DisposeJobFields(ref PtrAllocation<ControlSurfaceFields> fields)
  {
    if (fields.IsCreated)
    {
      ref ControlSurfaceFields local = ref fields.Ref();
      if (local.visibleTransformLink.IsCreated)
        local.visibleTransformLink.RemoveRef();
      if (local.upperTransformLink.IsCreated)
        local.upperTransformLink.RemoveRef();
      if (local.lowerTransformLink.IsCreated)
        local.lowerTransformLink.RemoveRef();
    }
    fields.Dispose();
  }

  private Ptr<ControlSurfaceFields> GetOrCreateJobField()
  {
    if (!this.JobFields.IsCreated)
    {
      JobsAllocator<ControlSurfaceFields>.Allocate(ref this.JobFields);
      ref ControlSurfaceFields local = ref this.JobFields.Ref();
      local.pitchRange = this.pitchRange;
      local.rollRange = this.rollRange;
      local.yawRange = this.yawRange;
      local.brakeRange = this.brakeRange;
      local.flap = this.flap;
      local.servoSpeed = this.servoSpeed;
      local.splitDrag = this.splitDrag;
      local.maxSplit = this.maxSplit;
      local.yawSplitFactor = this.yawSplitFactor;
      local.restingRotation = this.visibleMesh.transform.localRotation;
      if ((double) this.maxSplit > 0.0)
        local.restingSplitRotation = this.splitUpper.transform.localRotation;
    }
    return (Ptr<ControlSurfaceFields>) this.JobFields;
  }

  public void UpdateJobFields()
  {
    ref ControlSurfaceFields local = ref this.JobFields.Ref();
    local.IsDetached = this.attachedSurface.IsDetached();
    local.gearState = this.aircraft.gearState;
    local.controlInputs = new ControlInputsBurst()
    {
      pitch = this.controlInputs.pitch,
      roll = this.controlInputs.roll,
      yaw = this.controlInputs.yaw,
      throttle = this.controlInputs.throttle,
      brake = this.controlInputs.brake,
      customAxis1 = this.controlInputs.customAxis1
    };
  }

  public bool GetJobTransforms(
    out Transform liftTransform,
    out Transform upperTransform,
    out Transform lowerTransform)
  {
    liftTransform = this.visibleMesh.transform;
    if ((double) this.maxSplit > 0.0)
    {
      upperTransform = this.splitUpper;
      lowerTransform = this.splitLower;
      return true;
    }
    upperTransform = (Transform) null;
    lowerTransform = (Transform) null;
    return false;
  }

  public void ApplyJobFields()
  {
    if (!this.JobFields.IsCreated)
      return;
    this.splitAmount = this.JobFields.Ref().splitAmount;
  }

  private void ControlSurface_OnInitialize()
  {
    if (!this.aircraft.LocalSim || !((UnityEngine.Object) this.splitUpper != (UnityEngine.Object) null) || !((UnityEngine.Object) NetworkSceneSingleton<LevelInfo>.i != (UnityEngine.Object) null))
      return;
    this.aircraft.AddControlSurface(this);
  }

  private void OnDestroy()
  {
    JobManager.Remove(ref this.JobPart);
    ControlSurface.DisposeJobFields(ref this.JobFields);
  }

  public void Aero()
  {
    if ((double) this.splitAmount == 0.0)
      return;
    Vector3 vector3 = this.attachedSurface.rb.velocity - this.aircraft.GetWindVelocity();
    float num = -1f * this.splitAmount * this.splitDrag * this.aircraft.airDensity * vector3.sqrMagnitude;
    this.attachedSurface.rb.AddForce(vector3.normalized * num);
  }
}
