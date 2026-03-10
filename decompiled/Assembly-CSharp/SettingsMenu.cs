// Decompiled with JetBrains decompiler
// Type: SettingsMenu
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

#nullable disable
public class SettingsMenu : MonoBehaviour
{
  [SerializeField]
  private Transform parent;
  [SerializeField]
  private SettingsMenuButton defaultMenu;
  private SettingsMenuButton currentButton;
  private GameObject currentSubmenu;

  public void Start()
  {
    if ((Object) SceneSingleton<GameplayUI>.i != (Object) null)
    {
      FlightHud.EnableCanvas(false);
      SceneSingleton<GameplayUI>.i.gameplayCanvas.enabled = false;
    }
    PlayerSettings.LoadPrefs();
    this.OpenMenu(this.defaultMenu);
  }

  public void OpenMenu(SettingsMenuButton button)
  {
    if ((Object) this.currentButton == (Object) button)
      return;
    if ((Object) this.currentSubmenu != (Object) null)
      Object.Destroy((Object) this.currentSubmenu);
    this.currentSubmenu = Object.Instantiate<GameObject>(button.MenuPrefab, this.parent);
  }

  public void CloseSettingsMenu()
  {
    PlayerSettings.ApplyPrefs();
    Object.Destroy((Object) this.gameObject);
  }
}
