// Decompiled with JetBrains decompiler
// Type: LiveryData
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.ModScripts;
using System;
using UnityEngine;

#nullable disable
[CreateAssetMenu(menuName = "ScriptableObjects/Aircraft/Livery")]
[CopyToModProject(CopyCreateAssetMenu = false)]
public class LiveryData : ScriptableObject
{
  public Texture2D Texture;
  public float Glossiness;
  public LiveryData.TextureColor[] Colors;

  [CopyToModProject]
  [Serializable]
  public struct TextureColor
  {
    public Color32 Color;
    public int Count;
  }
}
