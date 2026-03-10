// Decompiled with JetBrains decompiler
// Type: WeatherSet
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using JamesFrowen;
using UnityEngine;

#nullable disable
[NestedObjectProperties(CreateSelf = true, DrawScriptField = false, IndentLevel = 2)]
public class WeatherSet : ScriptableObject
{
  public string displayName;
  public float coverage;
  public Texture2D mask;
  public Texture2D particleSampler;
  public Texture2D[] cookies;
  public bool lightning;
}
