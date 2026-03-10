// Decompiled with JetBrains decompiler
// Type: ContributeToFaction
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Cysharp.Threading.Tasks;
using NuclearOption.MissionEditorScripts.Buttons;
using NuclearOption.Networking;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#nullable disable
public class ContributeToFaction : MonoBehaviour
{
  private ContributeToFaction.ContributionType contributionType;
  [SerializeField]
  private AircraftSelectionMenu selectionMenu;
  [SerializeField]
  private TMP_Text contributePercentage;
  [SerializeField]
  private TMP_Text contributeValue;
  [SerializeField]
  private TMP_Text giveAirframeNumber;
  [SerializeField]
  private TMP_Text giveAirframeValue;
  [SerializeField]
  private TMP_Text giveVehicleValue;
  [SerializeField]
  private TMP_Text currentFunds;
  [SerializeField]
  private TMP_Text remainingFunds;
  [SerializeField]
  private Button fundsButton;
  [SerializeField]
  private Button aircraftButton;
  [SerializeField]
  private Button vehiclesButton;
  [SerializeField]
  private Slider contributeSlider;
  [SerializeField]
  private Slider giveAirframesSlider;
  [SerializeField]
  private Button contributeConfirm;
  [SerializeField]
  private Button giveAirframesConfirm;
  [SerializeField]
  private Button giveVehicleConfirm;
  [SerializeField]
  private TMP_Dropdown aircraftDropdown;
  [SerializeField]
  private GameObject convoySelectPrefab;
  [SerializeField]
  private Transform convoySelectBackground;
  [SerializeField]
  private HoverText hoverText;
  [SerializeField]
  private ShowHoverText convoyHoverText;
  private List<UnitDefinition> sortedUnitList;
  private Faction.ConvoyGroup selectedConvoy;
  private Player localPlayer;
  private FactionHQ localHq;
  private bool canSpawnConvoy;

  private void OnEnable() => this.ResetValues();

  public void SetContributeVehicles()
  {
    this.selectedConvoy = (Faction.ConvoyGroup) null;
    this.contributionType = ContributeToFaction.ContributionType.Vehicles;
    this.RefreshVehicleList();
    this.CheckValues();
  }

  public void SetContributeFunds()
  {
    this.contributionType = ContributeToFaction.ContributionType.Funds;
    this.CheckValues();
  }

  public void ResetValues()
  {
    if (!GameManager.GetLocalPlayer<Player>(out this.localPlayer))
    {
      Debug.LogError((object) "AircraftInventoryMenu should not be active without a local player");
    }
    else
    {
      this.localHq = this.localPlayer.HQ;
      if ((UnityEngine.Object) this.localHq == (UnityEngine.Object) null)
      {
        Debug.LogError((object) "AircraftInventoryMenu should not be active without a local faction");
      }
      else
      {
        this.CheckAllowedToSpawnConvoy().Forget();
        this.vehiclesButton.interactable = !this.localPlayer.HQ.preventDonation && this.canSpawnConvoy;
        this.fundsButton.interactable = (double) this.localPlayer.Allocation > 0.0;
        this.selectedConvoy = (Faction.ConvoyGroup) null;
        this.contributeSlider.value = 0.0f;
        this.giveAirframesSlider.value = 0.0f;
        this.contributionType = ContributeToFaction.ContributionType.None;
        this.CheckValues();
      }
    }
  }

  public void CheckValues()
  {
    if (this.contributionType == ContributeToFaction.ContributionType.Funds)
    {
      if ((double) this.localPlayer.Allocation > 0.0)
      {
        this.contributePercentage.text = $"{(ValueType) (float) (100.0 * (double) this.contributeSlider.value):F0}%";
        this.contributeValue.text = "-" + UnitConverter.ValueReading(this.localPlayer.Allocation * this.contributeSlider.value);
        this.contributeConfirm.interactable = (double) this.contributeSlider.value > 0.0;
      }
      else
      {
        this.contributePercentage.text = "0%";
        this.contributeValue.text = "$0";
        this.contributeConfirm.interactable = false;
      }
    }
    if (this.contributionType == ContributeToFaction.ContributionType.Vehicles)
    {
      float valueInMillions = this.selectedConvoy != null ? this.selectedConvoy.GetCost() : 0.0f;
      this.giveVehicleValue.text = "-" + UnitConverter.ValueReading(valueInMillions);
      this.giveVehicleConfirm.interactable = (double) valueInMillions > 0.0;
    }
    this.CalculateFunds();
  }

  public void CalculateFunds()
  {
    this.currentFunds.text = UnitConverter.ValueReading(this.localPlayer.Allocation) ?? "";
    float allocation = this.localPlayer.Allocation;
    if (this.contributionType == ContributeToFaction.ContributionType.Funds && (double) this.contributeSlider.value > 0.0)
      allocation -= this.localPlayer.Allocation * this.contributeSlider.value;
    if (this.contributionType == ContributeToFaction.ContributionType.Vehicles && this.selectedConvoy != null)
      allocation -= this.selectedConvoy.GetCost();
    this.remainingFunds.text = UnitConverter.ValueReading(allocation) ?? "";
  }

  public void CancelContribution()
  {
    this.contributionType = ContributeToFaction.ContributionType.None;
  }

  public void SelectConvoy(Faction.ConvoyGroup convoy)
  {
    this.selectedConvoy = convoy;
    this.CheckValues();
  }

  public void ContributeFunds()
  {
    if ((double) this.localPlayer.Allocation <= 0.0)
      return;
    this.localPlayer.CmdDonateFactionFunds(this.localPlayer.Allocation * this.contributeSlider.value);
  }

  public void PurchaseConvoy() => this.localPlayer.CmdPurchaseConvoy(this.selectedConvoy.Name);

  public void RefreshVehicleList()
  {
    List<Faction.ConvoyGroup> convoyGroups = this.localHq.faction.GetConvoyGroups();
    for (int index = 0; index < convoyGroups.Count; ++index)
    {
      Faction.ConvoyGroup convoyGroup = convoyGroups[index];
      ConvoyPurchaseOption component = UnityEngine.Object.Instantiate<GameObject>(this.convoySelectPrefab, this.convoySelectBackground).GetComponent<ConvoyPurchaseOption>();
      component.SetButtonHoverText(this.hoverText);
      component.Initialize(this, this.localPlayer, convoyGroup);
    }
  }

  public void RefreshAircraftList()
  {
    this.aircraftDropdown.ClearOptions();
    if (this.sortedUnitList == null)
      this.sortedUnitList = new List<UnitDefinition>();
    else
      this.sortedUnitList.Clear();
    this.sortedUnitList.AddRange((IEnumerable<UnitDefinition>) Encyclopedia.i.aircraft);
    this.sortedUnitList.Sort((Comparison<UnitDefinition>) ((a, b) => a.value.CompareTo(b.value)));
    for (int index = 0; index < this.sortedUnitList.Count; ++index)
      AddAircraftToDropdown($"{this.sortedUnitList[index].unitName} ({UnitConverter.ValueReading(this.sortedUnitList[index].value)})");
    this.aircraftDropdown.value = 0;
    this.aircraftDropdown.RefreshShownValue();

    void AddAircraftToDropdown(string text)
    {
      this.aircraftDropdown.options.Add(new TMP_Dropdown.OptionData(text));
    }
  }

  public async UniTask CheckAllowedToSpawnConvoy()
  {
    float delaySpawnConvoy = await this.localPlayer.CmdGetDelaySpawnConvoy();
    this.canSpawnConvoy = (double) delaySpawnConvoy <= 0.0;
    this.convoyHoverText.SetText(this.canSpawnConvoy ? "Contribute a group of vehicles to the faction" : $"Wait {delaySpawnConvoy:F0}s before next contribution of vehicles");
  }

  [Serializable]
  public enum ContributionType
  {
    None,
    Funds,
    Vehicles,
  }
}
