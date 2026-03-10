// Decompiled with JetBrains decompiler
// Type: GameTime
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using UnityEngine;
using UnityEngine.UI;

#nullable disable
public class GameTime : HUDApp
{
  [SerializeField]
  private Text gameTimeLabel;
  [SerializeField]
  private Text gameTimeValue;

  public override void Initialize(Aircraft aircraft)
  {
  }

  public override void RefreshSettings()
  {
    if (PlayerSettings.hudTime > 0)
    {
      this.gameTimeLabel.enabled = true;
      this.gameTimeValue.enabled = true;
    }
    else
    {
      this.gameTimeLabel.enabled = false;
      this.gameTimeValue.enabled = false;
    }
    base.RefreshSettings();
    this.gameTimeLabel.fontSize = this.fontSize;
    this.gameTimeValue.fontSize = this.fontSize;
  }

  public override void Refresh()
  {
    if (!((Object) NetworkSceneSingleton<LevelInfo>.i != (Object) null))
      return;
    this.gameTimeValue.text = UnitConverter.TimeOfDay(NetworkSceneSingleton<LevelInfo>.i.timeOfDay, false);
  }
}
