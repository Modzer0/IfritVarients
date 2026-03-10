// Decompiled with JetBrains decompiler
// Type: DamageEffect
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;

#nullable disable
[Serializable]
public class DamageEffect
{
  public GameObject prefab;
  public float threshold;
  [NonSerialized]
  public GameObject instance;
  [NonSerialized]
  public bool triggered;
}
