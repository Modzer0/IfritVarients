// Decompiled with JetBrains decompiler
// Type: HUDBombingState
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#nullable disable
public class HUDBombingState : HUDWeaponState
{
  [SerializeField]
  private Image alignmentBar;
  private float dropTime = 100f;
  [SerializeField]
  private Text dropCountdown;
  [SerializeField]
  private Text ccipFallTime;
  [SerializeField]
  private Text ccrpFallTime;
  [SerializeField]
  private Image upperMarker;
  [SerializeField]
  private Image lowerMarker;
  [SerializeField]
  private Image ccipPipper;
  [SerializeField]
  private Image ccipLine;
  [SerializeField]
  private Image ccrpCircle;
  private WeaponStation weaponStation;
  private float lastCCIPCheck;
  private GlobalPosition ccipImpactPoint;
  private GlobalPosition averageTargetPosition;
  private GlobalPosition lastAverageTargetPosition;
  private bool initialized;
  private Vector3 ccipImpactPointSmoothed;
  private List<GlobalPosition> simPoints = new List<GlobalPosition>();

  public override void UpdateWeaponDisplay(Aircraft aircraft, List<Unit> targetList)
  {
    Camera mainCamera = SceneSingleton<CameraStateManager>.i.mainCamera;
    if (!this.initialized)
    {
      this.ccipImpactPoint = aircraft.GlobalPosition() + aircraft.rb.velocity * 8f - Vector3.up * 4.905f * 8f * 8f;
      this.ccipImpactPointSmoothed = aircraft.GlobalPosition().AsVector3() + aircraft.rb.velocity * 8f - Vector3.up * 4.905f * 8f * 8f;
      this.initialized = true;
    }
    SceneSingleton<CombatHUD>.i.targetDesignator.color = Color.Lerp(Color.black, Color.green, Mathf.Clamp01((float) ((double) Vector3.Distance(SceneSingleton<FlightHud>.i.GetHUDCenter().position, SceneSingleton<CombatHUD>.i.targetDesignator.transform.position) * 0.014999999664723873 - 0.15000000596046448)));
    SceneSingleton<FlightHud>.i.velocityVector.color = new Color(0.0f, 1f, 0.0f, Mathf.Clamp01((float) ((double) Vector3.Distance(SceneSingleton<FlightHud>.i.velocityVector.transform.position, SceneSingleton<CombatHUD>.i.targetDesignator.transform.position) * 0.014999999664723873 - 0.15000000596046448)));
    SceneSingleton<CombatHUD>.i.targetDesignator.enabled = !aircraft.gearDeployed;
    this.transform.localPosition = Vector3.zero;
    Vector3 zero = Vector3.zero;
    int num1 = 0;
    foreach (Unit target in targetList)
    {
      GlobalPosition knownPosition;
      if (!target.disabled && aircraft.NetworkHQ.TryGetKnownPosition(target, out knownPosition))
      {
        zero += knownPosition.ToLocalPosition();
        ++num1;
      }
    }
    if (num1 > 0)
      zero /= (float) num1;
    this.averageTargetPosition = zero.ToGlobalPosition();
    this.averageTargetPosition += Vector3.up * this.weaponStation.WeaponInfo.airburstHeight;
    Vector3 forward = aircraft.transform.forward with
    {
      y = 0.0f
    };
    Vector3 vector3_1 = this.averageTargetPosition - aircraft.GlobalPosition();
    Vector3 vector3_2 = vector3_1 with { y = 0.0f };
    float num2 = Vector3.Dot(aircraft.transform.forward, Vector3.up);
    int num3 = this.weaponStation == null ? 0 : (this.weaponStation.Ammo > 0 ? 1 : 0);
    if (num3 != 0 && num1 > 0 && (double) num2 > -0.0099999997764825821 && (double) Vector3.Dot(mainCamera.transform.forward, aircraft.transform.forward) > 0.0 && (double) Vector3.Dot(forward, vector3_2) > 0.5 && (double) vector3_1.y < 0.0)
    {
      if (!this.alignmentBar.gameObject.activeSelf)
        this.alignmentBar.gameObject.SetActive(true);
      if (!this.dropCountdown.enabled)
      {
        this.dropCountdown.enabled = true;
        this.ccrpFallTime.enabled = true;
        this.ccrpCircle.enabled = true;
      }
      double initialHeight = -(double) vector3_1.y;
      float magnitude = vector3_2.magnitude;
      double y = (double) aircraft.rb.velocity.y;
      float num4 = Kinematics.FallTime((float) initialHeight, (float) y);
      float f = Vector3.Dot(aircraft.rb.velocity.normalized, vector3_2.normalized);
      if ((double) Mathf.Abs(f) < 1.0 / 1000.0)
        f = 1f / 1000f;
      this.dropTime = magnitude / (f * aircraft.rb.velocity.magnitude) - num4;
      this.dropCountdown.text = $"REL {this.dropTime:F1}";
      this.dropCountdown.enabled = (double) this.dropTime < 100.0 && (double) this.dropTime > -10.0;
      this.dropCountdown.transform.eulerAngles = Vector3.zero;
      this.ccrpFallTime.text = $"ToF {num4:F1}";
      float num5 = -vector3_1.y + Vector3.Project(vector3_2, aircraft.transform.forward).y;
      this.alignmentBar.transform.position = mainCamera.WorldToScreenPoint(this.averageTargetPosition.ToLocalPosition() + Vector3.up * num5) with
      {
        z = 0.0f
      };
      Vector3 screenPoint = mainCamera.WorldToScreenPoint(this.averageTargetPosition.ToLocalPosition() + Vector3.up * num5 * 0.9f) with
      {
        z = 0.0f
      };
      this.alignmentBar.transform.eulerAngles = new Vector3(0.0f, 0.0f, (float) (-(double) Mathf.Atan2(screenPoint.x - this.alignmentBar.transform.position.x, screenPoint.y - this.alignmentBar.transform.position.y) * 57.295780181884766 + 180.0));
      if (float.IsFinite(this.dropTime))
      {
        this.dropTime = Mathf.Clamp(this.dropTime, -10f, 10f);
        this.upperMarker.transform.localPosition = new Vector3(0.0f, this.dropTime * 15f, 0.0f);
        this.lowerMarker.transform.localPosition = new Vector3(0.0f, this.dropTime * -15f, 0.0f);
        this.ccrpCircle.fillAmount = Mathf.Clamp01(1f - Mathf.Abs(this.dropTime / 10f));
      }
      if ((double) this.dropTime > 0.0)
      {
        this.dropCountdown.gameObject.transform.localPosition = Vector3.right * 30f;
        this.dropCountdown.color = Color.yellow;
        this.ccrpCircle.color = Color.yellow;
        this.ccrpFallTime.color = Color.yellow;
      }
      if ((double) this.dropTime < 0.0)
      {
        this.dropCountdown.gameObject.transform.localPosition = -Vector3.right * 30f;
        this.dropCountdown.color = Color.red;
        this.ccrpCircle.color = Color.red;
        this.ccrpFallTime.color = Color.red;
      }
      if ((double) Mathf.Abs(this.dropTime) < 2.0)
      {
        this.dropCountdown.color = Color.green;
        this.ccrpCircle.color = Color.green;
        this.ccrpFallTime.color = Color.green;
      }
    }
    else
    {
      if (this.alignmentBar.gameObject.activeSelf)
        this.alignmentBar.gameObject.SetActive(false);
      if (this.dropCountdown.enabled)
      {
        this.dropCountdown.enabled = false;
        this.ccrpFallTime.enabled = false;
        this.ccrpCircle.enabled = false;
      }
    }
    if (num3 != 0 && !this.alignmentBar.gameObject.activeSelf && !aircraft.gearDeployed && (num1 == 0 || (double) Vector3.Dot(SceneSingleton<CameraStateManager>.i.transform.forward, vector3_1.normalized) > 0.40000000596046448))
    {
      this.CCIPTrajectory(aircraft);
      this.UpdatePipperPosition(aircraft);
    }
    else
    {
      this.ccipLine.enabled = false;
      this.ccipPipper.enabled = false;
      this.ccipFallTime.enabled = false;
      this.ccipImpactPoint = aircraft.GlobalPosition() + aircraft.rb.velocity * 8f - Vector3.up * 4.905f * 8f * 8f;
      this.ccipImpactPointSmoothed = aircraft.transform.position + aircraft.rb.velocity * 8f - Datum.origin.position;
    }
  }

  public void CCIPTrajectory(Aircraft aircraft)
  {
    if ((double) Time.timeSinceLevelLoad - (double) this.lastCCIPCheck < 0.10000000149011612)
      return;
    this.lastCCIPCheck = Time.timeSinceLevelLoad;
    this.simPoints.Clear();
    GlobalPosition position = aircraft.GlobalPosition() - Vector3.up * aircraft.definition.spawnOffset.y;
    Vector3 vector3 = aircraft.rb.velocity - Vector3.up * 9.81f * 0.25f;
    float num = 0.5f;
    for (int index1 = 0; index1 < 32 /*0x20*/; ++index1)
    {
      num += 0.04f;
      position += vector3 * num;
      this.simPoints.Add(position);
      if ((double) position.y < 0.0)
      {
        Ray ray = new Ray(position.ToLocalPosition() - vector3 * num, vector3 * num);
        float enter;
        if (this.simPoints.Count > 1 && Datum.WaterPlane().Raycast(ray, out enter))
        {
          List<GlobalPosition> simPoints1 = this.simPoints;
          int index2 = simPoints1.Count - 1;
          List<GlobalPosition> simPoints2 = this.simPoints;
          GlobalPosition globalPosition = simPoints2[simPoints2.Count - 2] + vector3.normalized * enter;
          simPoints1[index2] = globalPosition;
          break;
        }
        break;
      }
      vector3 -= (Vector3.up * 9.81f + vector3.normalized * 2E-05f * vector3.sqrMagnitude) * num;
    }
    if ((double) position.y > 0.0)
    {
      this.ccipPipper.enabled = false;
      this.ccipLine.enabled = false;
      this.ccipFallTime.enabled = false;
    }
    else if (this.SplitSearchTrajectory())
    {
      this.ccipPipper.enabled = true;
      this.ccipLine.enabled = true;
      this.ccipFallTime.enabled = true;
      this.ccipFallTime.text = $"ToF {Kinematics.FallTime(aircraft.radarAlt, aircraft.rb.velocity.y):F1}";
    }
    else
    {
      this.ccipPipper.enabled = false;
      this.ccipLine.enabled = false;
      this.ccipFallTime.enabled = false;
    }
  }

  private void UpdatePipperPosition(Aircraft aircraft)
  {
    Camera main = Camera.main;
    this.ccipImpactPointSmoothed = Vector3.Lerp(this.ccipImpactPointSmoothed, this.ccipImpactPoint.AsVector3() + aircraft.rb.velocity * 0.3f, 5f * Time.deltaTime);
    if ((double) Vector3.Dot((this.ccipImpactPointSmoothed + Datum.origin.position - main.transform.position).normalized, main.transform.forward) < 0.5)
    {
      this.ccipPipper.enabled = false;
      this.ccipLine.enabled = false;
    }
    else
    {
      Vector3 screenPoint = main.WorldToScreenPoint(this.ccipImpactPointSmoothed + Datum.origin.position) with
      {
        z = 0.0f
      };
      this.ccipPipper.transform.position = screenPoint;
      Vector3 position = SceneSingleton<FlightHud>.i.velocityVector.transform.position with
      {
        z = 0.0f
      };
      float num = 1080f / (float) Screen.height;
      Vector3 lhs = position - screenPoint;
      this.ccipLine.transform.position = screenPoint + lhs.normalized * 22f / num;
      Vector3 vector3 = position - lhs.normalized * 8f / num;
      float y = (this.ccipLine.transform.position - vector3).magnitude * num;
      float z = (float) (-(double) Mathf.Atan2(lhs.x, lhs.y) * 57.295780181884766);
      if ((double) Vector3.Dot(lhs, vector3 - this.ccipLine.transform.position) < 0.0)
        this.ccipLine.enabled = false;
      this.ccipLine.transform.eulerAngles = new Vector3(0.0f, 0.0f, z);
      this.ccipLine.transform.localScale = new Vector3(1f, y, 1f);
    }
  }

  private bool SplitSearchTrajectory()
  {
    List<GlobalPosition> simPoints1 = this.simPoints;
    this.ccipImpactPoint = simPoints1[simPoints1.Count - 1];
    int num1 = 0;
    int num2 = 0;
    int count = this.simPoints.Count;
    while (this.simPoints.Count > 2 && num1 < 20)
    {
      ++num1;
      int index1 = Mathf.FloorToInt((float) this.simPoints.Count * 0.5f);
      ++num2;
      RaycastHit hitInfo;
      if (Physics.Linecast(this.simPoints[0].ToLocalPosition(), this.simPoints[index1].ToLocalPosition(), out hitInfo, -8193))
      {
        this.ccipImpactPoint = hitInfo.point.ToGlobalPosition();
        for (int index2 = this.simPoints.Count - 1; index2 > index1; --index2)
          this.simPoints.RemoveAt(index2);
      }
      else
      {
        ++num2;
        Vector3 localPosition1 = this.simPoints[index1].ToLocalPosition();
        List<GlobalPosition> simPoints2 = this.simPoints;
        Vector3 localPosition2 = simPoints2[simPoints2.Count - 1].ToLocalPosition();
        RaycastHit raycastHit;
        ref RaycastHit local = ref raycastHit;
        if (Physics.Linecast(localPosition1, localPosition2, out local, -8193))
        {
          this.ccipImpactPoint = raycastHit.point.ToGlobalPosition();
          for (int index3 = index1 - 1; index3 >= 0; --index3)
            this.simPoints.RemoveAt(index3);
        }
        else
          break;
      }
    }
    return true;
  }

  public override void SetHUDWeaponState(
    Image targetDesignator,
    Aircraft aircraft,
    WeaponStation weaponStation)
  {
    this.weaponStation = weaponStation;
    this.weaponInfo = weaponStation.WeaponInfo;
    targetDesignator.color = Color.green;
    targetDesignator.transform.localScale = Vector3.one;
    SceneSingleton<CameraStateManager>.i.SetDesiredFoV(PlayerSettings.defaultFoV, 0.0f);
    SceneSingleton<FlightHud>.i.waterline.enabled = true;
    SceneSingleton<FlightHud>.i.velocityVector.transform.localScale = Vector3.one;
  }

  public override void HUDFixedUpdate(Aircraft aircraft, List<Unit> targetList)
  {
  }
}
