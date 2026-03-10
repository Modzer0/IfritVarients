// Decompiled with JetBrains decompiler
// Type: MissileDefinition
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

#nullable disable
[CreateAssetMenu(fileName = "New Missile", menuName = "ScriptableObjects/MissileDefinition", order = 7)]
public class MissileDefinition : UnitDefinition
{
  private float? mass;

  public float GetMass()
  {
    if (!this.mass.HasValue)
      this.mass = new float?(this.unitPrefab.GetComponent<Missile>().GetMass());
    return this.mass.Value;
  }
}
