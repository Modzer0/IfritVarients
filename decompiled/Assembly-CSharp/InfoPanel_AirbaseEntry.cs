// Decompiled with JetBrains decompiler
// Type: InfoPanel_AirbaseEntry
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;
using UnityEngine.UI;

#nullable disable
internal class InfoPanel_AirbaseEntry : MonoBehaviour
{
  private Airbase airbase;
  private InfoPanel_Faction factionInfoPanel;
  [SerializeField]
  private Text airbaseName;
  [SerializeField]
  private Text helipad;
  [SerializeField]
  private Text revetment;
  [SerializeField]
  private Text medium;
  [SerializeField]
  private Text shelter;
  [SerializeField]
  private Text warheads;
  [SerializeField]
  private Text carrier;
  private float lastRefresh;
  private float refreshRate = 1f;

  private void Awake() => this.lastRefresh = Time.timeSinceLevelLoad;

  private void Update()
  {
    if ((double) Time.timeSinceLevelLoad <= (double) this.lastRefresh + (double) this.refreshRate)
      return;
    this.RefreshAirbase();
    this.lastRefresh = Time.timeSinceLevelLoad;
  }

  public void SetAirbase(Airbase a, InfoPanel_Faction panel)
  {
    this.airbase = a;
    this.airbaseName.text = this.airbase.SavedAirbase.DisplayName;
    this.factionInfoPanel = panel;
    this.RefreshAirbase();
    this.airbase.onLostControl += new Action(this.Airbase_onCapture);
  }

  private void Airbase_onCapture()
  {
    this.factionInfoPanel.listAirbases.Remove(this.airbase);
    this.airbase.onLostControl -= new Action(this.Airbase_onCapture);
    if (!((UnityEngine.Object) this != (UnityEngine.Object) null))
      return;
    UnityEngine.Object.Destroy((UnityEngine.Object) this.gameObject);
  }

  public Airbase GetAirbase() => this.airbase;

  public void RefreshAirbase()
  {
    int num1 = 0;
    int num2 = 0;
    int num3 = 0;
    int num4 = 0;
    int num5 = 0;
    int num6 = 0;
    for (int index = 0; index < this.airbase.hangars.Count; ++index)
    {
      if (!this.airbase.hangars[index].Disabled)
      {
        if (this.airbase.hangars[index].attachedUnit.definition.code == "HPAD")
          ++num1;
        else if (this.airbase.hangars[index].attachedUnit.definition.code == "REV")
          ++num2;
        else if (this.airbase.hangars[index].attachedUnit.definition.code == "HGR-M")
          ++num3;
        else if (this.airbase.hangars[index].attachedUnit.definition.code == "HGR-H")
          ++num4;
        else if (this.airbase.hangars[index].attachedUnit.definition.code == "SHP")
          ++num5;
      }
      num6 = this.airbase.GetWarheads();
    }
    if ((UnityEngine.Object) this.helipad != (UnityEngine.Object) null)
    {
      this.helipad.text = num1.ToString();
      this.helipad.color = num1 > 0 ? Color.green : Color.grey;
    }
    if ((UnityEngine.Object) this.revetment != (UnityEngine.Object) null)
    {
      this.revetment.text = num2.ToString();
      this.revetment.color = num2 > 0 ? Color.green : Color.grey;
    }
    if ((UnityEngine.Object) this.medium != (UnityEngine.Object) null)
    {
      this.medium.text = num3.ToString();
      this.medium.color = num3 > 0 ? Color.green : Color.grey;
    }
    if ((UnityEngine.Object) this.shelter != (UnityEngine.Object) null)
    {
      this.shelter.text = num4.ToString();
      this.shelter.color = num4 > 0 ? Color.green : Color.grey;
    }
    if ((UnityEngine.Object) this.carrier != (UnityEngine.Object) null)
    {
      this.carrier.text = num5.ToString();
      this.carrier.color = num5 > 0 ? Color.green : Color.grey;
    }
    if (!((UnityEngine.Object) this.warheads != (UnityEngine.Object) null))
      return;
    this.warheads.text = num6.ToString();
    this.warheads.color = num6 > 0 ? Color.green : Color.grey;
  }
}
