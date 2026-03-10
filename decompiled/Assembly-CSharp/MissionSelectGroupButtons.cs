// Decompiled with JetBrains decompiler
// Type: MissionSelectGroupButtons
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.SavedMission;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

#nullable disable
public class MissionSelectGroupButtons : MonoBehaviour
{
  [SerializeField]
  private Button tabButtonPrefab;
  [SerializeField]
  private RectTransform buttonHolder;
  [SerializeField]
  private MissionSelectPanel panel;
  private Button activeButton;
  private readonly List<MissionGroup> disallowedGroups = new List<MissionGroup>();
  private readonly Dictionary<MissionGroup, Button> buttons = new Dictionary<MissionGroup, Button>();

  private void Awake()
  {
    MissionGroup.AllGroup all = MissionGroup.All;
    this.TabButtonOnClick(this.CreateTabButtons(all), (MissionGroup) all);
  }

  private Button CreateTabButtons(MissionGroup.AllGroup startingGroup)
  {
    Button tabButtons = (Button) null;
    foreach (MissionGroup allGroup in MissionGroup.AllGroups)
    {
      MissionGroup group = allGroup;
      Button button = Object.Instantiate<Button>(this.tabButtonPrefab, (Transform) this.buttonHolder);
      this.buttons[group] = button;
      button.GetComponentInChildren<TextMeshProUGUI>().text = group.Name;
      button.onClick.AddListener((UnityAction) (() => this.TabButtonOnClick(button, group)));
      if (group == startingGroup)
        tabButtons = button;
      if (this.disallowedGroups.Contains(group))
        button.gameObject.SetActive(false);
    }
    return tabButtons;
  }

  private void TabButtonOnClick(Button button, MissionGroup group)
  {
    MissionSelectGroupButtons.SetButtonColor(ref this.activeButton, button);
    this.panel.SetActiveGroup(group);
  }

  private static void SetButtonColor(ref Button activeButton, Button button)
  {
    if ((Object) activeButton != (Object) null)
      activeButton.interactable = true;
    activeButton = button;
    if (!((Object) activeButton != (Object) null))
      return;
    activeButton.interactable = false;
  }

  public void SetPickerFilter(MissionsPicker.Filter filter)
  {
    if (filter.DisallowedGroups == null)
      return;
    this.disallowedGroups.AddRange((IEnumerable<MissionGroup>) filter.DisallowedGroups);
    foreach (MissionGroup disallowedGroup in filter.DisallowedGroups)
    {
      Button button;
      if (this.buttons.TryGetValue(disallowedGroup, out button))
        button.gameObject.SetActive(false);
    }
  }
}
