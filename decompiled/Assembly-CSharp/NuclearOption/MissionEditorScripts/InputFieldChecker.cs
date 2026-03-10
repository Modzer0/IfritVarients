// Decompiled with JetBrains decompiler
// Type: NuclearOption.MissionEditorScripts.InputFieldChecker
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

#nullable disable
namespace NuclearOption.MissionEditorScripts;

public class InputFieldChecker : MonoBehaviour
{
  public static bool InsideInputField;
  private EventSystem system;

  private void Update()
  {
    if ((Object) this.system == (Object) null)
      this.system = EventSystem.current;
    if ((Object) this.system == (Object) null)
      return;
    this.CheckInputAndFocus(this.system.currentSelectedGameObject);
  }

  private void CheckInputAndFocus(GameObject currentObject)
  {
    if ((Object) currentObject != (Object) null)
    {
      TMP_InputField component1;
      if (currentObject.TryGetComponent<TMP_InputField>(out component1))
      {
        InputFieldChecker.InsideInputField = component1.isFocused;
        return;
      }
      InputField component2;
      if (currentObject.TryGetComponent<InputField>(out component2))
      {
        InputFieldChecker.InsideInputField = component2.isFocused;
        return;
      }
    }
    InputFieldChecker.InsideInputField = false;
  }

  private void OnDestroy() => InputFieldChecker.InsideInputField = false;
}
