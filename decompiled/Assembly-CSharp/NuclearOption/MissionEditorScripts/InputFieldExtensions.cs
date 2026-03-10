// Decompiled with JetBrains decompiler
// Type: NuclearOption.MissionEditorScripts.InputFieldExtensions
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using TMPro;

#nullable disable
namespace NuclearOption.MissionEditorScripts;

public static class InputFieldExtensions
{
  public static void SetIfNotFocus(
    this TMP_InputField inputField,
    string value,
    bool withoutNotify = true)
  {
    if (inputField.isFocused)
      return;
    if (withoutNotify)
      inputField.SetTextWithoutNotify(value);
    else
      inputField.text = value;
  }
}
