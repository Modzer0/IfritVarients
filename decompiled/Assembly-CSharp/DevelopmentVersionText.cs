// Decompiled with JetBrains decompiler
// Type: DevelopmentVersionText
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System.IO;
using TMPro;
using UnityEngine;

#nullable disable
public class DevelopmentVersionText : MonoBehaviour
{
  public TextMeshProUGUI Text;

  private void Awake() => this.Text.text = DevelopmentVersionText.GetText();

  private static string GetText()
  {
    if (!Debug.isDebugBuild || Application.isEditor)
      return "";
    string path = Path.Combine(Application.dataPath, "..", "build-hash.txt");
    Debug.Log((object) ("Developer version file:" + path));
    if (File.Exists(path))
      return "Private Build - " + File.ReadAllText(path).Trim();
    Debug.LogWarning((object) "Developer version file not found");
    return "";
  }
}
