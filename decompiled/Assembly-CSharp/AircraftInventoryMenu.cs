// Decompiled with JetBrains decompiler
// Type: AircraftInventoryMenu
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Cysharp.Threading.Tasks;
using NuclearOption;
using NuclearOption.MissionEditorScripts.Buttons;
using NuclearOption.Networking;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#nullable disable
public class AircraftInventoryMenu : MonoBehaviour
{
  [SerializeField]
  private AircraftSelectionMenu selectionMenu;
  [SerializeField]
  private List<AircraftSelectionButton> listAircraftButtons = new List<AircraftSelectionButton>();
  [SerializeField]
  private HoverText hoverText;
  [SerializeField]
  private ShowHoverText requestButtonHoverText;
  [SerializeField]
  private ShowHoverText buyButtonHoverText;
  [SerializeField]
  private ShowHoverText sellButtonHoverText;
  [SerializeField]
  private ShowHoverText expandButtonHoverText;
  private AircraftDefinition selectedType;
  public GameObject buttonPrefab;
  public Transform buttonsContainer;
  [SerializeField]
  private TMP_Text requestLabel;
  [SerializeField]
  private TMP_Text buyLabel;
  [SerializeField]
  private TMP_Text sellLabel;
  [SerializeField]
  private TMP_Text expandLabel;
  [SerializeField]
  private Button requestButton;
  [SerializeField]
  private Button buyButton;
  [SerializeField]
  private Button sellButton;
  private bool checkedReserved;
  private bool hasReserved;
  private bool reserveCheckResult;
  [SerializeField]
  private GameObject reserveNotice;
  [SerializeField]
  private TMP_Text reserveNoticeText;
  private List<AircraftDefinition> sortedAircraftList = new List<AircraftDefinition>();
  [SerializeField]
  private AudioClip selectSound;
  [SerializeField]
  private AudioClip deselectSound;
  private Player localPlayer;

  public void Initialize(Player localPlayer)
  {
    this.localPlayer = localPlayer;
    this.checkedReserved = false;
    localPlayer.onReserveNotice += new Action<ReserveNotice>(this.Player_OnReserveNotice);
    if (this.buttonsContainer.childCount > 0 || this.listAircraftButtons.Count > 0)
    {
      foreach (Component component in this.buttonsContainer)
        UnityEngine.Object.Destroy((UnityEngine.Object) component.gameObject);
      this.listAircraftButtons.Clear();
      this.sortedAircraftList.Clear();
    }
    this.sortedAircraftList.AddRange((IEnumerable<AircraftDefinition>) Encyclopedia.i.aircraft);
    this.sortedAircraftList.Sort((Comparison<AircraftDefinition>) ((a, b) => a.value.CompareTo(b.value)));
    for (int index = 0; index < this.sortedAircraftList.Count; ++index)
    {
      AircraftDefinition sortedAircraft = this.sortedAircraftList[index];
      AircraftSelectionButton component = UnityEngine.Object.Instantiate<GameObject>(this.buttonPrefab, this.buttonsContainer).GetComponent<AircraftSelectionButton>();
      component.definition = sortedAircraft;
      this.listAircraftButtons.Add(component);
      component.Initialize(this, localPlayer, this.hoverText);
    }
    this.ToggleAircraftButtons();
  }

  public void Refresh()
  {
    foreach (AircraftSelectionButton listAircraftButton in this.listAircraftButtons)
    {
      listAircraftButton.Setup(this.localPlayer.OwnsAirframe(listAircraftButton.definition, true), this.selectionMenu.CanFlyAircraft(listAircraftButton.definition));
      listAircraftButton.SetActive((UnityEngine.Object) this.selectedType != (UnityEngine.Object) null && (UnityEngine.Object) this.selectedType == (UnityEngine.Object) listAircraftButton.definition);
    }
    this.UpdateSellButton();
    this.UpdateBuyButton();
    this.UpdateRequestButton();
    LayoutRebuilder.ForceRebuildLayoutImmediate(this.transform as RectTransform);
  }

  public void ToggleAircraftButtons()
  {
    this.buttonsContainer.gameObject.SetActive(!this.buttonsContainer.gameObject.activeSelf);
    if (this.buttonsContainer.gameObject.activeSelf)
    {
      this.expandLabel.transform.eulerAngles = new Vector3(0.0f, 0.0f, 180f);
      this.expandButtonHoverText.SetText("Hide aircraft buttons");
    }
    else
    {
      this.expandLabel.transform.eulerAngles = new Vector3(0.0f, 0.0f, 0.0f);
      this.expandButtonHoverText.SetText("Show aircraft buttons");
    }
  }

  public void UpdateSellButton()
  {
    if ((UnityEngine.Object) this.selectedType != (UnityEngine.Object) null && this.localPlayer.OwnsAirframe(this.selectedType, true))
    {
      if (this.localPlayer.PossessesReservedAirframe(this.selectedType))
      {
        this.sellLabel.text = "Return\n" + this.selectedType.unitName;
        this.sellButtonHoverText.SetText("Return " + this.selectedType.unitName);
      }
      else
      {
        this.sellLabel.text = "Sell\n" + this.selectedType.unitName;
        float valueInMillions = this.localPlayer.Allocation + this.selectedType.value;
        this.sellButtonHoverText.SetText($"{$"{$"{"Sell " + this.selectedType.unitName}\nFunds: {UnitConverter.ValueReading(this.localPlayer.Allocation)}"}\nValue: +{UnitConverter.ValueReading(this.selectedType.value)}"}\nRemain: {UnitConverter.ValueReading(valueInMillions)}");
      }
      this.sellButton.enabled = true;
      this.sellButton.interactable = true;
    }
    else
    {
      this.sellLabel.text = "Return / Sell";
      this.sellButtonHoverText.SetText("You do not own this type of aircraft");
      this.sellButton.enabled = false;
      this.sellButton.interactable = false;
    }
  }

  public void UpdateBuyButton()
  {
    this.buyLabel.text = $"Purchase for\n${this.selectedType.value}m";
    if ((UnityEngine.Object) this.selectedType != (UnityEngine.Object) null && (double) this.localPlayer.Allocation > (double) this.selectedType.value && this.selectionMenu.CanFlyAircraft(this.selectedType))
    {
      float valueInMillions = this.localPlayer.Allocation - this.selectedType.value;
      this.buyButtonHoverText.SetText($"{$"{$"{"Buy " + this.selectedType.unitName}\nFunds: {UnitConverter.ValueReading(this.localPlayer.Allocation)}"}\nCost: -{UnitConverter.ValueReading(this.selectedType.value)}"}\nRemain: {UnitConverter.ValueReading(valueInMillions)}");
      this.buyButton.enabled = true;
      this.buyButton.interactable = true;
    }
    else
    {
      if (!this.selectionMenu.CanFlyAircraft(this.selectedType))
        this.buyButtonHoverText.SetText("Aircraft not available at this airbase");
      else if ((double) this.localPlayer.Allocation < (double) this.selectedType.value)
        this.buyButtonHoverText.SetText("Insufficient funds for this aircraft");
      this.buyButton.enabled = false;
      this.buyButton.interactable = false;
    }
  }

  public async UniTask UpdateRequestButton()
  {
    await this.CheckReservingAirframe(this.selectedType);
    bool flag1 = this.localPlayer.OwnsAirframe(this.selectedType, true);
    bool flag2 = this.localPlayer.PossessesReservedAirframe();
    bool flag3 = this.localPlayer.PlayerRank >= this.selectedType.aircraftParameters.rankRequired;
    this.requestLabel.text = $"Requisition\n via Rank ({this.selectedType.aircraftParameters.rankRequired})";
    if (((!((UnityEngine.Object) this.selectedType != (UnityEngine.Object) null) || !this.selectionMenu.CanFlyAircraft(this.selectedType) || !this.checkedReserved || flag2 || this.hasReserved ? 0 : (!flag1 ? 1 : 0)) & (flag3 ? 1 : 0)) != 0 && this.reserveCheckResult)
    {
      this.requestButton.interactable = true;
      this.requestButton.enabled = true;
    }
    else
    {
      if (!this.selectionMenu.CanFlyAircraft(this.selectedType))
        this.requestButtonHoverText.SetText("Aircraft not available at this airbase");
      this.requestButton.enabled = false;
      this.requestButton.interactable = false;
    }
  }

  private async UniTask CheckReservingAirframe(AircraftDefinition selectedType)
  {
    if ((UnityEngine.Object) selectedType == (UnityEngine.Object) null)
    {
      Debug.LogError((object) "Can't send RPC because selectedType is null");
    }
    else
    {
      ReserveNotice reserveNotice = await this.localPlayer.CmdCheckReservingAirframe(selectedType);
      if (reserveNotice.outcome == ReserveEvent.Invalid)
        Debug.LogError((object) "Server returned Invalid for CmdCheckReservingAirframe");
      else
        this.OnReserveCheck(reserveNotice);
    }
  }

  private void OnReserveCheck(ReserveNotice reserveNotice)
  {
    this.checkedReserved = true;
    this.hasReserved = reserveNotice.isReserving;
    if (reserveNotice.outcome == ReserveEvent.rejectedRank)
    {
      this.requestButtonHoverText.SetText($"Unable to requisition: Requires Rank {reserveNotice.aircraftDefinition.aircraftParameters.rankRequired}");
      this.reserveCheckResult = false;
    }
    else if (reserveNotice.outcome == ReserveEvent.rejectedPossessesReserved)
    {
      this.requestButtonHoverText.SetText("Unable to requisition: You must return your other requisitioned aircraft first.");
      this.reserveCheckResult = false;
    }
    else if (reserveNotice.outcome == ReserveEvent.rejectedAfford)
    {
      this.requestButtonHoverText.SetText("Unable to requisition: you can afford this aircraft.");
      this.reserveCheckResult = false;
    }
    else if (reserveNotice.outcome == ReserveEvent.rejectedDuplicate)
    {
      this.requestButtonHoverText.SetText("Unable to requisition: Request already in process.");
      this.reserveCheckResult = false;
    }
    else if (reserveNotice.outcome == ReserveEvent.rejectedOwned)
    {
      this.requestButtonHoverText.SetText("Unable to requisition: You already hold this aircraft.");
      this.reserveCheckResult = false;
    }
    else
    {
      this.requestButtonHoverText.SetText("Request this aircraft when available.");
      this.reserveCheckResult = true;
    }
  }

  private void Player_OnReserveNotice(ReserveNotice reserveNotice)
  {
    if (reserveNotice.outcome == ReserveEvent.granted || reserveNotice.outcome == ReserveEvent.cancelledAfford || reserveNotice.outcome == ReserveEvent.cancelledOwned || reserveNotice.outcome == ReserveEvent.cancelledRank || (UnityEngine.Object) this == (UnityEngine.Object) null)
      return;
    this.reserveNotice.SetActive(true);
    this.requestButton.interactable = false;
    this.hasReserved = true;
    string unitName = reserveNotice.aircraftDefinition.unitName;
    string str = unitName.EndsWith("ss") || unitName.EndsWith("ch") || unitName.EndsWith("sh") || unitName.EndsWith("x") ? unitName + "es" : unitName + "s";
    if (reserveNotice.queuePosition == 1)
      this.reserveNoticeText.text = $"No reserve {str} currently available. You are first in queue for this airframe";
    else if (reserveNotice.queuePosition > 1)
      this.reserveNoticeText.text = $"No reserve {str} currently available. You are number {reserveNotice.queuePosition} in queue for this airframe";
    else if (reserveNotice.queuePosition == -1)
      this.reserveNoticeText.text = "Already requested an airframe";
    else if (reserveNotice.queuePosition == -2)
    {
      this.reserveNoticeText.text = "Insufficient rank to request this airframe";
    }
    else
    {
      if (reserveNotice.queuePosition != -3)
        return;
      this.reserveNoticeText.text = "You already hold one of this airframe";
    }
  }

  public void SellAirframe()
  {
    if ((UnityEngine.Object) this.selectedType == (UnityEngine.Object) null)
      return;
    if (this.localPlayer.OwnsAirframe(this.selectedType, false))
      this.localPlayer.CmdSellAirframe(this.selectedType);
    else if (this.localPlayer.OwnsAirframe(this.selectedType, true))
      this.localPlayer.CmdReturnAirframe(this.selectedType);
    SoundManager.PlayInterfaceOneShot(this.deselectSound);
    this.SetSelectedType(this.selectedType);
  }

  public void BuyAirframe()
  {
    if ((UnityEngine.Object) this.selectedType == (UnityEngine.Object) null)
      return;
    this.localPlayer.CmdPurchaseAirframe(this.selectedType);
    SoundManager.PlayInterfaceOneShot(this.selectSound);
    this.SetSelectedType(this.selectedType);
  }

  public void RequestAirframe()
  {
    if ((UnityEngine.Object) this.selectedType == (UnityEngine.Object) null)
      return;
    this.localPlayer.CmdRequestReserveAirframe(this.selectedType);
    this.reserveCheckResult = false;
    this.hasReserved = true;
    this.requestButton.interactable = false;
    this.SetSelectedType(this.selectedType);
  }

  private void Update()
  {
  }

  public void PreviousAircraft()
  {
    int index = this.listAircraftButtons.FindIndex((Predicate<AircraftSelectionButton>) (_x => (UnityEngine.Object) _x.definition == (UnityEngine.Object) this.selectedType));
    bool flag = false;
    AircraftDefinition definition = this.selectedType;
    while (!flag)
    {
      --index;
      if (index < 0)
        index = this.listAircraftButtons.Count - 1;
      definition = this.listAircraftButtons[index].definition;
      flag = this.selectionMenu.CanFlyAircraft(definition);
      if (flag || (UnityEngine.Object) definition == (UnityEngine.Object) this.selectedType)
        break;
    }
    this.SetSelectedType(definition);
  }

  public void NextAircraft()
  {
    int index = this.listAircraftButtons.FindIndex((Predicate<AircraftSelectionButton>) (_x => (UnityEngine.Object) _x.definition == (UnityEngine.Object) this.selectedType));
    bool flag = false;
    AircraftDefinition definition = this.selectedType;
    while (!flag)
    {
      ++index;
      if (index > this.listAircraftButtons.Count - 1)
        index = 0;
      definition = this.listAircraftButtons[index].definition;
      flag = this.selectionMenu.CanFlyAircraft(definition);
      if (flag || (UnityEngine.Object) definition == (UnityEngine.Object) this.selectedType)
        break;
    }
    this.SetSelectedType(definition);
  }

  public void OnAircraftButtonClick(AircraftSelectionButton button)
  {
    if (!((UnityEngine.Object) button.definition != (UnityEngine.Object) this.selectedType))
      return;
    this.SetSelectedType(button.definition);
  }

  public void SetSelectedType(AircraftDefinition definition)
  {
    this.selectedType = definition;
    this.checkedReserved = false;
    if ((UnityEngine.Object) this.selectedType != (UnityEngine.Object) this.selectionMenu.GetSelectedType() && this.selectionMenu.CanFlyAircraft(this.selectedType))
      this.selectionMenu.SetSelectedType(this.selectedType);
    this.Refresh();
  }

  public void OnDestroy()
  {
    if (this.localPlayer == null)
      return;
    this.localPlayer.onReserveNotice -= new Action<ReserveNotice>(this.Player_OnReserveNotice);
  }
}
