// Decompiled with JetBrains decompiler
// Type: NuclearOption.Jobs.ControlSurfaceFields
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

#nullable disable
namespace NuclearOption.Jobs;

public struct ControlSurfaceFields
{
  public PtrRefCounter<IndexLink> visibleTransformLink;
  public PtrRefCounter<IndexLink> upperTransformLink;
  public PtrRefCounter<IndexLink> lowerTransformLink;
  public float pitchRange;
  public float rollRange;
  public float yawRange;
  public float brakeRange;
  public bool flap;
  public float servoSpeed;
  public float splitDrag;
  public float maxSplit;
  public float yawSplitFactor;
  public Quaternion restingRotation;
  public Quaternion restingSplitRotation;
  public bool IsDetached;
  public ControlInputsBurst controlInputs;
  public LandingGear.GearState gearState;
  public float currentPitch;
  public float currentRoll;
  public float currentYaw;
  public float splitAmount;
}
