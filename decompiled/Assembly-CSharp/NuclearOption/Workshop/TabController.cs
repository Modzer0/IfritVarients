// Decompiled with JetBrains decompiler
// Type: NuclearOption.Workshop.TabController
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

#nullable disable
namespace NuclearOption.Workshop;

public class TabController : MonoBehaviour
{
  [SerializeField]
  private Button buttonTemplate;
  [SerializeField]
  private List<string> options;
  [SerializeField]
  private List<Button> allButtons;
  [SerializeField]
  private int buttonWidth;
  [SerializeField]
  private Color activeColor;
  [SerializeField]
  private Color inactiveColor;
  [SerializeField]
  private int activeIndex;
  private bool hasSetup;

  public event TabController.TabChangedDelegate TabChanged;

  private void Awake()
  {
    for (int i = 0; i < this.allButtons.Count; ++i)
      this.SetupNewButton(i);
  }

  private void SetupNewButton(int i)
  {
    Button button = this.allButtons[i];
    button.name = $"Tab button {i}";
    button.onClick.AddListener((UnityAction) (() => this.OnClick(button)));
    RectTransform transform = (RectTransform) button.transform;
    transform.anchoredPosition = new Vector2((float) (this.buttonWidth * i), transform.anchoredPosition.y);
  }

  private void OnValidate()
  {
    for (int index = 0; index < this.allButtons.Count; ++index)
    {
      if (index < this.options.Count)
      {
        Button allButton = this.allButtons[index];
        if (!((Object) allButton == (Object) null))
          allButton.GetComponentInChildren<TextMeshProUGUI>().text = this.options[index];
      }
    }
  }

  public void Start()
  {
    if (this.hasSetup)
      return;
    this.Setup();
  }

  public void Setup(List<string> options, int active)
  {
    this.options.Clear();
    this.options.AddRange((IEnumerable<string>) options);
    this.activeIndex = active;
    this.Setup();
  }

  private void Setup()
  {
    this.hasSetup = true;
    while (this.allButtons.Count < this.options.Count)
    {
      this.allButtons.Add(Object.Instantiate<Button>(this.buttonTemplate, this.transform));
      this.SetupNewButton(this.allButtons.Count - 1);
    }
    for (int index = 0; index < this.allButtons.Count; ++index)
    {
      Button allButton = this.allButtons[index];
      if (index < this.options.Count)
      {
        allButton.gameObject.SetActive(true);
        allButton.GetComponentInChildren<TextMeshProUGUI>().text = this.options[index];
      }
      else
        allButton.gameObject.SetActive(false);
    }
    this.SetActiveButton(this.activeIndex);
    TabController.TabChangedDelegate tabChanged = this.TabChanged;
    if (tabChanged == null)
      return;
    tabChanged(this.options[this.activeIndex], this.activeIndex);
  }

  private void OnClick(Button clicked)
  {
    int index = this.allButtons.IndexOf(clicked);
    this.SetActiveButton(index);
    TabController.TabChangedDelegate tabChanged = this.TabChanged;
    if (tabChanged == null)
      return;
    tabChanged(this.options[index], index);
  }

  private void SetActiveButton(int index)
  {
    this.activeIndex = index;
    for (int index1 = this.options.Count - 1; index1 >= 0; --index1)
      this.allButtons[index1].transform.SetSiblingIndex(this.options.Count - 1 - index1);
    for (int index2 = 0; index2 < this.options.Count; ++index2)
    {
      Button allButton = this.allButtons[index2];
      if (this.activeIndex == index2)
      {
        allButton.image.color = this.activeColor;
        allButton.transform.SetAsLastSibling();
      }
      else
        allButton.image.color = this.inactiveColor;
    }
  }

  public delegate void TabChangedDelegate(string label, int index);
}
