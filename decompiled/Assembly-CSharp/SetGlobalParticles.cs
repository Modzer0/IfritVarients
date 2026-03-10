// Decompiled with JetBrains decompiler
// Type: SetGlobalParticles
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System.Collections.Generic;
using UnityEngine;

#nullable disable
public class SetGlobalParticles : MonoBehaviour
{
  [SerializeField]
  private List<ParticleSystem> systems = new List<ParticleSystem>();

  private void Start()
  {
    foreach (ParticleSystem system in this.systems)
    {
      ParticleSystem.MainModule main = system.main with
      {
        simulationSpace = ParticleSystemSimulationSpace.Custom,
        customSimulationSpace = Datum.origin
      };
    }
    Object.Destroy((Object) this);
  }
}
