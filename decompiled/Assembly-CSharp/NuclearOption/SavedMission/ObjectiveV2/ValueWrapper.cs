// Decompiled with JetBrains decompiler
// Type: NuclearOption.SavedMission.ObjectiveV2.ValueWrapper
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;

#nullable disable
namespace NuclearOption.SavedMission.ObjectiveV2;

public abstract class ValueWrapper
{
  public static TWrapper FromCallback<TWrapper, TValue>(TValue value, Action<TValue> onSet)
    where TWrapper : ValueWrapper<TValue>, new()
    where TValue : IEquatable<TValue>
  {
    return ValueWrapper.FromCallback<TWrapper, TValue>((object) onSet, value, onSet);
  }

  public static TWrapper FromCallback<TWrapper, TValue>(
    object owner,
    TValue value,
    Action<TValue> onSet)
    where TWrapper : ValueWrapper<TValue>, new()
    where TValue : IEquatable<TValue>
  {
    TWrapper wrapper = new TWrapper();
    wrapper.SetValue(value, (object) null, true);
    wrapper.RegisterOnChange(owner, new ValueWrapper<TValue>.OnChangeDelegate(onSet.Invoke));
    return wrapper;
  }
}
