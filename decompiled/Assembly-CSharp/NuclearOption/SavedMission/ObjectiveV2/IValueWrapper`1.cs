// Decompiled with JetBrains decompiler
// Type: NuclearOption.SavedMission.ObjectiveV2.IValueWrapper`1
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;

#nullable disable
namespace NuclearOption.SavedMission.ObjectiveV2;

public interface IValueWrapper<T> : IValueWrapper where T : IEquatable<T>
{
  T Value { get; }

  void SetValue(T value, object source, bool invokeOnChangeOnly = true);

  void RegisterOnChange(object owner, ValueWrapper<T>.OnChangeDelegate callback);
}
