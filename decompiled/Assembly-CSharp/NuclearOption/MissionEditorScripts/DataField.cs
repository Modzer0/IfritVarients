// Decompiled with JetBrains decompiler
// Type: NuclearOption.MissionEditorScripts.DataField
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using TMPro;
using UnityEngine;
using UnityEngine.UI;

#nullable disable
namespace NuclearOption.MissionEditorScripts;

public abstract class DataField : MonoBehaviour
{
  [SerializeField]
  protected TextMeshProUGUI label;
  public LayoutElement LabelLayout;
  public LayoutElement FieldLayout;
  private bool interactable = true;
  private bool interactableSetup;

  public Color LabelColor
  {
    get => this.label.color;
    set => this.label.color = value;
  }

  public bool Interactable
  {
    get => this.interactable;
    set
    {
      this.interactableSetup = true;
      this.interactable = value;
      this.SetFieldInteractable(value);
    }
  }

  protected abstract void SetFieldInteractable(bool value);

  private void Awake()
  {
    if (!this.interactableSetup)
      this.Interactable = false;
    this.AwakeSetup();
  }

  protected abstract void AwakeSetup();

  public void HideLabel() => this.LabelLayout.gameObject.SetActive(false);
}
