// Decompiled with JetBrains decompiler
// Type: otherTime
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;
using UnityEngine.UI;

#nullable disable
public class otherTime : HUDApp
{
  [SerializeField]
  private Text otherTimeLabel;
  [SerializeField]
  private Text otherTimeValue;

  public override void Initialize(Aircraft aircraft)
  {
  }

  public override void RefreshSettings()
  {
    if (PlayerSettings.hudTime > 0)
    {
      this.otherTimeLabel.enabled = true;
      this.otherTimeValue.enabled = true;
    }
    else
    {
      this.otherTimeLabel.enabled = false;
      this.otherTimeValue.enabled = false;
    }
    base.RefreshSettings();
    this.otherTimeLabel.fontSize = this.fontSize;
    this.otherTimeValue.fontSize = this.fontSize;
  }

  public override void Refresh()
  {
    if (PlayerSettings.hudTime == 1)
    {
      this.otherTimeLabel.text = "Mission";
      this.otherTimeValue.text = UnitConverter.TimeOfDay(NetworkSceneSingleton<MissionManager>.i.MissionTime / 3600f, true);
    }
    else
    {
      if (PlayerSettings.hudTime != 2)
        return;
      this.otherTimeLabel.text = "Local";
      DateTime now = DateTime.Now;
      this.otherTimeValue.text = $"{now.Hour:D2}:{now.Minute:D2}";
    }
  }
}
