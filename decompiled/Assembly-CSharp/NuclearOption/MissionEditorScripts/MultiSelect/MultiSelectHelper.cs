// Decompiled with JetBrains decompiler
// Type: NuclearOption.MissionEditorScripts.MultiSelect.MultiSelectHelper
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

#nullable disable
namespace NuclearOption.MissionEditorScripts.MultiSelect;

public static class MultiSelectHelper
{
  public static NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<TObject>.GetField<T> Cast<TObject, T>(
    this NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<TObject>.GetFieldRef<T> getFieldRef)
  {
    // ISSUE: explicit reference operation
    return (NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<TObject>.GetField<T>) (x => ^getFieldRef(x));
  }
}
