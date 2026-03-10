// Decompiled with JetBrains decompiler
// Type: NuclearOption.Jobs.DetectionRequest
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

#nullable disable
namespace NuclearOption.Jobs;

public readonly struct DetectionRequest
{
  public readonly TargetDetector Detector;
  public readonly Unit target;
  private readonly Transform scanPoint;
  private readonly IRadarReturn radarReturn;
  private readonly float dist;
  private readonly float clutterFactor;

  public DetectionRequest(
    TargetDetector detector,
    Unit target,
    IRadarReturn radarReturn,
    float dist,
    float clutterFactor)
  {
    this.Detector = detector;
    this.target = target;
    this.radarReturn = radarReturn;
    this.dist = dist;
    this.clutterFactor = clutterFactor;
    this.scanPoint = detector.GetScanPoint();
  }

  public RaycastCommand GetRaycastCommand()
  {
    Vector3 position = this.scanPoint.position;
    Vector3 vector3 = this.target.transform.position + 0.4f * this.target.definition.height * Vector3.up;
    return new RaycastCommand(position, FastMath.NormalizedDirection(position, vector3), new QueryParameters(64 /*0x40*/, hitTriggers: QueryTriggerInteraction.Ignore), FastMath.Distance(position, vector3));
  }

  public void ProcessResult(RaycastHit hit)
  {
    if ((Object) this.target == (Object) null || this.target.disabled || (Object) this.Detector == (Object) null || (Object) hit.collider != (Object) null && FastMath.OutOfRange(hit.point.ToGlobalPosition(), this.target.GlobalPosition(), this.target.maxRadius * 1.5f))
      return;
    if (this.radarReturn != null)
    {
      Radar detector = this.Detector as Radar;
      if (!detector.CanSeeRadarReturn(this.radarReturn, this.dist, this.clutterFactor))
        return;
      detector.DetectTarget(this.target);
    }
    else
      this.Detector.DetectTarget(this.target);
  }
}
