// Decompiled with JetBrains decompiler
// Type: NuclearOption.SavedMission.ObjectiveV2.ValueWrapper`1
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Triggers;
using System;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
namespace NuclearOption.SavedMission.ObjectiveV2;

public abstract class ValueWrapper<T> : ValueWrapper, IValueWrapper<T>, IValueWrapper where T : IEquatable<T>
{
  private readonly List<(object owner, ValueWrapper<T>.OnChangeDelegate callback)> callbacks = new List<(object, ValueWrapper<T>.OnChangeDelegate)>();
  private T _value;

  public T Value => this._value;

  public ref T GetValueRef() => ref this._value;

  public void RegisterOnChange(object owner, Action callback)
  {
    this.callbacks.Add((owner, (ValueWrapper<T>.OnChangeDelegate) (_ => callback())));
  }

  public void RegisterOnChange(object owner, ValueWrapper<T>.OnChangeDelegate callback)
  {
    this.callbacks.Add((owner, callback));
  }

  public void UnregisterOnChange(object owner)
  {
    for (int index = 0; index < this.callbacks.Count; ++index)
    {
      if (this.callbacks[index].owner == owner)
        this.callbacks.RemoveAt(index);
    }
  }

  public void RegisterOnChangeWithAutoDestroy(
    Component owner,
    ValueWrapper<T>.OnChangeDelegate callback)
  {
    this.callbacks.Add(((object) owner, callback));
    owner.OnDestroyAsync().ContinueWith((Action) (() => this.UnregisterOnChange((object) owner))).Forget();
  }

  public void RegisterOnChangeWithAutoDestroy(
    GameObject owner,
    ValueWrapper<T>.OnChangeDelegate callback)
  {
    this.callbacks.Add(((object) owner, callback));
    owner.OnDestroyAsync().ContinueWith((Action) (() => this.UnregisterOnChange((object) owner))).Forget();
  }

  public ValueWrapper()
  {
  }

  public ValueWrapper(T value) => this._value = value;

  public void SetValue(T value, object source, bool invokeOnChangeOnly = true)
  {
    if (invokeOnChangeOnly && EqualityComparer<T>.Default.Equals(value, this.Value))
      return;
    this._value = value;
    foreach ((object owner, ValueWrapper<T>.OnChangeDelegate callback) in this.callbacks.ToArray())
    {
      if (owner != source)
        callback(value);
    }
  }

  public override string ToString() => this.Value.ToString();

  public static implicit operator T(ValueWrapper<T> wrapper) => wrapper.Value;

  public delegate void OnChangeDelegate(T newValue) where T : IEquatable<T>;
}
