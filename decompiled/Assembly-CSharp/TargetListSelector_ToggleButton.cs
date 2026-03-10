// Decompiled with JetBrains decompiler
// Type: TargetListSelector_ToggleButton
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

#nullable disable
public class TargetListSelector_ToggleButton : 
  MonoBehaviour,
  IPointerEnterHandler,
  IEventSystemHandler,
  IPointerClickHandler,
  IPointerExitHandler
{
  public Image image;
  public Text label;
  public bool sameFaction = true;
  public List<UnitDefinition> listUnitTypes = new List<UnitDefinition>();
  public System.Type[] listUnitTypeCache;
  public List<UnitDefinition> listDefinitions = new List<UnitDefinition>();
  public bool isActive = true;
  public bool status = true;
  public bool onFocus;
  [SerializeField]
  private Color colorIsOn = Color.green;
  [SerializeField]
  private Color colorIsOff = Color.grey;

  public event Action OnToggle;

  private void Awake()
  {
    this.listUnitTypeCache = new System.Type[this.listUnitTypes.Count];
    for (int index = 0; index < this.listUnitTypeCache.Length; ++index)
      this.listUnitTypeCache[index] = this.listUnitTypes[index].GetType();
  }

  private void Start() => this.SetColor();

  public void SetColor()
  {
    if ((UnityEngine.Object) this.image != (UnityEngine.Object) null)
      this.image.color = this.status ? this.colorIsOn : this.colorIsOff;
    if (!((UnityEngine.Object) this.label != (UnityEngine.Object) null))
      return;
    this.label.color = this.status ? this.colorIsOn : this.colorIsOff;
  }

  public void Toggle()
  {
    if (this.status)
      this.Set(false);
    else
      this.Set(true);
  }

  public void Set(bool arg)
  {
    this.status = arg;
    this.SetColor();
    SceneSingleton<TargetListSelector>.i.NeedUpdateIcons();
    Action onToggle = this.OnToggle;
    if (onToggle == null)
      return;
    onToggle();
  }

  public void OnPointerEnter(PointerEventData eventData) => this.onFocus = true;

  public void OnPointerClick(PointerEventData eventData)
  {
    if (!this.onFocus)
      return;
    if (eventData.button == PointerEventData.InputButton.Right)
    {
      SceneSingleton<TargetListSelector>.i.SetOnlyItem(this);
    }
    else
    {
      if (eventData.button != PointerEventData.InputButton.Left)
        return;
      this.Toggle();
    }
  }

  public void OnPointerExit(PointerEventData eventData) => this.onFocus = false;

  public bool CheckFactions(Unit u)
  {
    switch (DynamicMap.GetFactionMode(u.NetworkHQ))
    {
      case FactionMode.Friendly:
        if (this.sameFaction)
          return !this.status;
        break;
      case FactionMode.Enemy:
        if (!this.sameFaction)
          return !this.status;
        break;
    }
    return false;
  }

  public bool CheckUnitTypes(Unit u)
  {
    System.Type type = u.definition.GetType();
    for (int index = 0; index < this.listUnitTypes.Count; ++index)
    {
      if (this.listUnitTypeCache[index] == type)
        return !this.status;
    }
    return false;
  }

  public bool CheckDefinitions(Unit u)
  {
    UnitDefinition definition = u.definition;
    for (int index = 0; index < this.listDefinitions.Count; ++index)
    {
      if ((UnityEngine.Object) this.listDefinitions[index] == (UnityEngine.Object) definition)
        return !this.status;
    }
    return false;
  }
}
