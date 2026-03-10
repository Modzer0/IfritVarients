// Decompiled with JetBrains decompiler
// Type: VehicleFormation
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System.Collections.Generic;
using UnityEngine;

#nullable disable
public class VehicleFormation
{
  public GroundVehicle leader;
  public Dictionary<GroundVehicle, int> members = new Dictionary<GroundVehicle, int>();
  private int formationWidth = 1;
  private int formationLength = 1;
  private float formationSpacing = 10f;

  public VehicleFormation(GroundVehicle leader)
  {
    this.members.Add(leader, 0);
    this.leader = leader;
  }

  public void RemoveVehicle(GroundVehicle vehicle)
  {
    if (!this.members.ContainsKey(vehicle))
      return;
    this.members.Remove(vehicle);
    this.CalcDimensions();
  }

  public void AddVehicle(GroundVehicle vehicle)
  {
    if (this.members.ContainsKey(vehicle))
      return;
    this.members.Add(vehicle, this.members.Count);
    this.CalcDimensions();
  }

  public void CalcDimensions()
  {
    this.formationWidth = this.members.Count;
    this.formationLength = 1;
    if (this.members.Count < 4)
      return;
    this.formationWidth = Mathf.FloorToInt(Mathf.Sqrt((float) this.members.Count));
    this.formationLength = Mathf.CeilToInt((float) (this.members.Count / this.formationWidth));
  }

  public GlobalPosition? GetTargetPos(GroundVehicle vehicle)
  {
    GlobalPosition? targetPos = new GlobalPosition?();
    if ((Object) vehicle != (Object) this.leader)
    {
      int member = this.members[vehicle];
      int count = this.members.Count;
      int num1 = member % this.formationWidth;
      int num2 = Mathf.FloorToInt((float) (member / this.formationLength));
      float num3 = (float) (((double) num1 - (double) this.formationWidth * 0.5) * (double) this.formationSpacing * 2.0);
      float num4 = (float) num2 * this.formationSpacing;
      targetPos = new GlobalPosition?(this.leader.GlobalPosition() + -this.leader.transform.forward * num4 + this.leader.transform.right * num3);
    }
    return targetPos;
  }
}
