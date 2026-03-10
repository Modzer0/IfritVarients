// Decompiled with JetBrains decompiler
// Type: NuclearOption.SavedMission.ObjectiveV2.ValueWrapperInt
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

#nullable disable
namespace NuclearOption.SavedMission.ObjectiveV2;

public class ValueWrapperInt : ValueWrapper<int>, IValueWrapper<float>, IValueWrapper
{
  float IValueWrapper<float>.Value => (float) this.Value;

  void IValueWrapper<float>.RegisterOnChange(
    object owner,
    ValueWrapper<float>.OnChangeDelegate callback)
  {
    this.RegisterOnChange(owner, (ValueWrapper<int>.OnChangeDelegate) (v => callback((float) v)));
  }

  void IValueWrapper<float>.SetValue(float value, object source, bool invokeOnChangeOnly)
  {
    this.SetValue((int) value, source, invokeOnChangeOnly);
  }

  public ValueWrapperInt()
  {
  }

  public ValueWrapperInt(int value)
    : base(value)
  {
  }
}
