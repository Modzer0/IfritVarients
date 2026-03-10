// Decompiled with JetBrains decompiler
// Type: NuclearOption.SavedMission.ObjectiveV2.ValueWrapperOverride`1
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;

#nullable disable
namespace NuclearOption.SavedMission.ObjectiveV2;

public class ValueWrapperOverride<T> : ValueWrapper<Override<T>>, IValueWrapper<T>, IValueWrapper where T : IEquatable<T>
{
  T IValueWrapper<T>.Value => this.Value.Value;

  void IValueWrapper<T>.RegisterOnChange(object owner, ValueWrapper<T>.OnChangeDelegate callback)
  {
    this.RegisterOnChange(owner, (ValueWrapper<Override<T>>.OnChangeDelegate) (v => callback(v.Value)));
  }

  void IValueWrapper<T>.SetValue(T value, object source, bool invokeOnChangeOnly)
  {
    this.SetValue(new Override<T>(this.Value.IsOverride, value), source, invokeOnChangeOnly);
  }
}
