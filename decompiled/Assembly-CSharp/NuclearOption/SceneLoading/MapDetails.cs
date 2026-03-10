// Decompiled with JetBrains decompiler
// Type: NuclearOption.SceneLoading.MapDetails
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

#nullable disable
namespace NuclearOption.SceneLoading;

[CreateAssetMenu(fileName = "MapDetails", menuName = "ScriptableObjects/MapDetails", order = 997)]
public class MapDetails : ScriptableObject
{
  public string PrefabName;
  public string MapName;
  public Sprite MapImage;
}
