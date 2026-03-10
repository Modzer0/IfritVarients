// Decompiled with JetBrains decompiler
// Type: AircraftSelectionButton
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.MissionEditorScripts.Buttons;
using NuclearOption.Networking;
using UnityEngine;
using UnityEngine.UI;

#nullable disable
public class AircraftSelectionButton : MonoBehaviour
{
  [SerializeField]
  private Button button;
  [SerializeField]
  private Image image;
  [SerializeField]
  private Image border;
  [SerializeField]
  private Image background;
  [SerializeField]
  private Text label;
  [SerializeField]
  private Text ownedValue;
  [SerializeField]
  private Text supplyValue;
  [SerializeField]
  private ShowHoverText buttonHoverText;
  public AircraftDefinition definition;
  [SerializeField]
  private AircraftSelectionMenu selectionMenu;
  [SerializeField]
  private AircraftInventoryMenu inventoryMenu;
  private float timeSinceLastUpdate;
  private bool isActive;
  private bool isOwned;
  private bool isAvailable;
  private Player localPlayer;

  public void Initialize(AircraftInventoryMenu inventory, Player localPlayer, HoverText hover)
  {
    this.localPlayer = localPlayer;
    this.inventoryMenu = inventory;
    this.label.text = this.definition.code;
    this.image.sprite = this.definition.mapIcon;
    this.buttonHoverText = this.gameObject.GetComponent<ShowHoverText>();
    this.buttonHoverText.SetHover(hover);
    this.SetHoverText();
  }

  public void Setup(bool owned, bool available = true)
  {
    this.button.enabled = true;
    this.button.interactable = true;
    this.isOwned = owned;
    this.isAvailable = available;
    this.SetColor();
    this.UpdateOwned();
  }

  public void SetColor()
  {
    Color white = Color.white;
    Color color = !this.isOwned || !this.isAvailable ? (!this.isOwned || this.isAvailable ? (this.isOwned || this.isAvailable ? Color.white : Color.grey) : Color.yellow) : Color.green;
    this.image.color = color;
    this.label.color = color;
    this.ownedValue.color = color;
    this.supplyValue.color = color;
    this.border.enabled = this.isActive;
    this.border.color = color;
    this.background.color = this.isActive ? Color.black : 0.45f * Color.gray;
  }

  public void UpdateOwned()
  {
    this.timeSinceLastUpdate = Time.timeSinceLevelLoad;
    int num = this.localPlayer.OwnedAirframeTypeCount(this.definition, true);
    if (num == 0)
    {
      this.ownedValue.text = "0";
      this.ownedValue.enabled = false;
    }
    else
    {
      this.ownedValue.text = $"{num}";
      this.ownedValue.enabled = true;
    }
    int unitSupply = this.localPlayer.HQ.GetUnitSupply((UnitDefinition) this.definition);
    if (unitSupply > 0)
    {
      this.supplyValue.text = $"{unitSupply}";
      this.supplyValue.enabled = true;
    }
    else
    {
      this.supplyValue.text = "0";
      this.supplyValue.enabled = false;
    }
    if (this.localPlayer.PossessesReservedAirframe(this.definition))
    {
      this.ownedValue.color = Color.yellow;
      this.ownedValue.text = "R";
    }
    this.SetHoverText();
  }

  public void SetHoverText()
  {
    string str = (this.definition.code ?? "") + $"\nOwned: {this.localPlayer.OwnedAirframeTypeCount(this.definition, true)}";
    if (this.localPlayer.PossessesReservedAirframe(this.definition))
      str += "(R)";
    int unitSupply = this.localPlayer.HQ.GetUnitSupply((UnitDefinition) this.definition);
    string text = str + $"\nFaction: {unitSupply}";
    if (!this.isAvailable)
      text += "\nNot available here";
    this.buttonHoverText.SetText(text);
  }

  private void Update()
  {
    if ((double) Time.timeSinceLevelLoad - (double) this.timeSinceLastUpdate <= 0.5)
      return;
    this.UpdateOwned();
  }

  public void OnClick()
  {
    if (!((Object) this.inventoryMenu != (Object) null))
      return;
    this.inventoryMenu.SetSelectedType(this.definition);
  }

  public void SetActive(bool state)
  {
    this.isActive = state;
    this.SetColor();
  }

  public bool CheckAvailable() => this.isAvailable;
}
