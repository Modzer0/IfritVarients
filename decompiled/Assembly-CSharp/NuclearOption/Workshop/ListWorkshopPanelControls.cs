// Decompiled with JetBrains decompiler
// Type: NuclearOption.Workshop.ListWorkshopPanelControls
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#nullable disable
namespace NuclearOption.Workshop;

public class ListWorkshopPanelControls : MonoBehaviour
{
  public GameObject Holder;
  public TMP_InputField FilterNameInput;
  [Space]
  public BetterToggleGroup OrderByToggleGroup;
  public Button ClearFilterButton;
  public Button RefreshButton;
}
