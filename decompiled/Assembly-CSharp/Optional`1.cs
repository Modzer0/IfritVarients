// Decompiled with JetBrains decompiler
// Type: Optional`1
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;

#nullable disable
[Serializable]
public struct Optional<T>(T initialValue)
{
  [SerializeField]
  private bool enabled = true;
  [SerializeField]
  private T value = initialValue;

  public void SetEnabled(bool enabled) => this.enabled = enabled;

  public bool Enabled => this.enabled;

  public T Value => this.value;
}
