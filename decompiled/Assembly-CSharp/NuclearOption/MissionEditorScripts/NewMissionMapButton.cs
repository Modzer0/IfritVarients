// Decompiled with JetBrains decompiler
// Type: NuclearOption.MissionEditorScripts.NewMissionMapButton
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.SceneLoading;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#nullable disable
namespace NuclearOption.MissionEditorScripts;

[Serializable]
public class NewMissionMapButton : MonoBehaviour
{
  [SerializeField]
  private MapDetails mapDetails;
  [SerializeField]
  private TextMeshProUGUI text;
  [SerializeField]
  private Image image;
  [SerializeField]
  private Button button;
  [SerializeField]
  private Outline outline;

  public Button Button => this.button;

  public void SetMap(MapDetails details)
  {
    this.mapDetails = details;
    this.text.text = details.MapName;
    this.image.sprite = details.MapImage;
  }

  internal void OnMapSelected(MapDetails selectedMap)
  {
    bool flag = (UnityEngine.Object) this.mapDetails == (UnityEngine.Object) selectedMap;
    this.image.color = flag ? Color.white : Color.gray;
    this.outline.enabled = flag;
  }
}
