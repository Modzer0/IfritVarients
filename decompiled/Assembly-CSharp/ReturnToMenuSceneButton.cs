// Decompiled with JetBrains decompiler
// Type: ReturnToMenuSceneButton
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using JamesFrowen.ScriptableVariables.UI;
using NuclearOption.SceneLoading;
using UnityEngine.SceneManagement;

#nullable disable
public class ReturnToMenuSceneButton : ButtonController
{
  protected override void onClick() => SceneManager.LoadScene(MapLoader.MainMenu);
}
