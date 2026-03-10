// Decompiled with JetBrains decompiler
// Type: ConvoyPurchaseOption
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.MissionEditorScripts.Buttons;
using NuclearOption.Networking;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#nullable disable
public class ConvoyPurchaseOption : MonoBehaviour
{
  [SerializeField]
  private TMP_Text text;
  [SerializeField]
  private Button button;
  [SerializeField]
  private ShowHoverText buttonHoverText;
  private ContributeToFaction contributeToFaction;
  private Player localPlayer;
  private Faction.ConvoyGroup convoyGroup;
  private float cost;
  private bool wasInteractable;

  public void Initialize(
    ContributeToFaction contributeToFaction,
    Player localPlayer,
    Faction.ConvoyGroup convoyGroup)
  {
    this.localPlayer = localPlayer;
    this.contributeToFaction = contributeToFaction;
    this.convoyGroup = convoyGroup;
    this.cost = convoyGroup.GetCost();
    this.text.text = $"{convoyGroup.Name} ({UnitConverter.ValueReading(this.cost)})";
    this.SetHoverText();
  }

  public void SetButtonHoverText(HoverText hoverText) => this.buttonHoverText.SetHover(hoverText);

  private void Update()
  {
    bool flag = (double) this.localPlayer.Allocation >= (double) this.cost;
    if (flag == this.wasInteractable)
      return;
    this.button.interactable = flag;
    this.text.color = flag ? Color.white : Color.gray;
    this.wasInteractable = flag;
  }

  private void OnDisable() => Object.Destroy((Object) this.gameObject);

  public void SelectConvoy() => this.contributeToFaction.SelectConvoy(this.convoyGroup);

  private void SetHoverText()
  {
    string text = "Spawn :";
    for (int index = 0; index < this.convoyGroup.Constituents.Count; ++index)
      text += $"\n{this.convoyGroup.Constituents[index].Count}x {this.convoyGroup.Constituents[index].Type.unitName}";
    this.buttonHoverText.SetText(text);
  }
}
