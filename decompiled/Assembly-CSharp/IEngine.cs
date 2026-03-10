// Decompiled with JetBrains decompiler
// Type: IEngine
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;

#nullable disable
public interface IEngine
{
  event Action OnEngineDisable;

  event Action OnEngineDamage;

  float GetThrust();

  float GetMaxThrust();

  float GetRPM();

  float GetRPMRatio();

  void SetInteriorSounds(bool useInteriorSound);

  Transform transform { get; }
}
