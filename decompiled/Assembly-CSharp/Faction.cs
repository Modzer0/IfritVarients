// Decompiled with JetBrains decompiler
// Type: Faction
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
[CreateAssetMenu(fileName = "New Faction", menuName = "ScriptableObjects/Faction", order = 7)]
public class Faction : ScriptableObject
{
  public string factionName;
  public string factionTag;
  public string factionExtendedName;
  public Sprite factionHeaderSprite;
  public Sprite factionGrayscaleLogo;
  public Sprite factionColorLogo;
  public Color color;
  [Tooltip("Color used when map icon is selected")]
  public Color selectedColor;
  public AudioClip theme;
  public AudioClip idleTheme;
  [SerializeField]
  private List<Faction.ConvoyGroup> convoyGroups = new List<Faction.ConvoyGroup>();

  public Faction.ConvoyGroup GetConvoyGroup(string name)
  {
    foreach (Faction.ConvoyGroup convoyGroup in this.convoyGroups)
    {
      if (convoyGroup.Name == name)
        return convoyGroup;
    }
    return (Faction.ConvoyGroup) null;
  }

  public List<Faction.ConvoyGroup> GetConvoyGroups() => this.convoyGroups;

  [Serializable]
  public class ConvoyUnit
  {
    public UnitDefinition Type;
    public int Count = 1;
  }

  [Serializable]
  public class ConvoyGroup
  {
    public string Name;
    public List<Faction.ConvoyUnit> Constituents = new List<Faction.ConvoyUnit>();

    public float GetCost()
    {
      float cost = 0.0f;
      foreach (Faction.ConvoyUnit constituent in this.Constituents)
        cost += constituent.Type.value * (float) constituent.Count;
      return cost;
    }
  }
}
