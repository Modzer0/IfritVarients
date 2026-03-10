// Decompiled with JetBrains decompiler
// Type: NuclearOption.SavedMission.ObjectiveV2.ValueWrapperQuaternion
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

#nullable disable
namespace NuclearOption.SavedMission.ObjectiveV2;

public class ValueWrapperQuaternion : ValueWrapper<Quaternion>, IValueWrapper<Vector3>, IValueWrapper
{
  Vector3 IValueWrapper<Vector3>.Value => this.Value.eulerAngles;

  void IValueWrapper<Vector3>.RegisterOnChange(
    object owner,
    ValueWrapper<Vector3>.OnChangeDelegate callback)
  {
    this.RegisterOnChange(owner, (ValueWrapper<Quaternion>.OnChangeDelegate) (v => callback(v.eulerAngles)));
  }

  void IValueWrapper<Vector3>.SetValue(Vector3 value, object source, bool invokeOnChangeOnly)
  {
    this.SetValue(Quaternion.Euler(value), source, invokeOnChangeOnly);
  }

  public ValueWrapperQuaternion()
  {
  }

  public ValueWrapperQuaternion(Quaternion value)
    : base(value)
  {
  }
}
