// Decompiled with JetBrains decompiler
// Type: NuclearOption.Networking.Lobbies.FlashErrorMessageModal
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using TMPro;
using UnityEngine;
using UnityEngine.UI;

#nullable disable
namespace NuclearOption.Networking.Lobbies;

public class FlashErrorMessageModal : MonoBehaviour
{
  public GameObject panel;
  public TextMeshProUGUI message;
  public Button cancelButton;
  public float HideTime;
}
