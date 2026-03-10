// Decompiled with JetBrains decompiler
// Type: NuclearOption.SavedMission.ObjectiveV2.ValueWrapperExtension
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.MissionEditorScripts;
using System;

#nullable disable
namespace NuclearOption.SavedMission.ObjectiveV2;

public static class ValueWrapperExtension
{
  public static void Setup<TWrapper, TValue>(
    this IDataField<TValue> field,
    string label,
    TValue value,
    Action<TValue> callback)
    where TWrapper : ValueWrapper<TValue>, new()
    where TValue : IEquatable<TValue>
  {
    field.Setup(label, (IValueWrapper<TValue>) ValueWrapper.FromCallback<TWrapper, TValue>(value, callback));
  }
}
