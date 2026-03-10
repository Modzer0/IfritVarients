// Decompiled with JetBrains decompiler
// Type: AudioHelper
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

#nullable disable
public static class AudioHelper
{
  public static float LinearToDecibel(float linear)
  {
    return (double) linear <= 0.0 ? -80f : 20f * Mathf.Log10(linear);
  }
}
