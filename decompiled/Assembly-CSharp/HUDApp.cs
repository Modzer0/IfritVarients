// Decompiled with JetBrains decompiler
// Type: HUDApp
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

#nullable disable
public class HUDApp : MonoBehaviour
{
  [SerializeField]
  protected HUDApp.AppType type;
  protected int fontSize;
  protected Color fontColor;

  public virtual void Initialize(Aircraft aircraft)
  {
  }

  public virtual void Refresh()
  {
  }

  public virtual void RefreshSettings()
  {
    switch (this.type)
    {
      case HUDApp.AppType.HUD:
        this.fontSize = (int) PlayerSettings.hudTextSize;
        this.fontColor = new Color((float) (PlayerSettings.hudColorR / (int) byte.MaxValue), (float) (PlayerSettings.hudColorG / (int) byte.MaxValue), (float) (PlayerSettings.hudColorB / (int) byte.MaxValue));
        break;
      case HUDApp.AppType.HMD:
        this.fontSize = (int) PlayerSettings.hmdTextSize;
        this.fontColor = new Color((float) (PlayerSettings.hudColorR / (int) byte.MaxValue), (float) (PlayerSettings.hudColorG / (int) byte.MaxValue), (float) (PlayerSettings.hudColorB / (int) byte.MaxValue));
        break;
      case HUDApp.AppType.MFD:
        this.fontSize = (int) PlayerSettings.hudTextSize;
        break;
    }
  }

  protected enum AppType
  {
    HUD,
    HMD,
    MFD,
  }
}
