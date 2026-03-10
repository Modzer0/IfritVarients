// Decompiled with JetBrains decompiler
// Type: NuclearOption.SavedMission.ObjectiveV2.Outcomes.ChangeTypeHelper
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;

#nullable disable
namespace NuclearOption.SavedMission.ObjectiveV2.Outcomes;

public static class ChangeTypeHelper
{
  public static void ChangeValue(
    float value,
    ChangeType type,
    float oldValue,
    Action<float> setValue)
  {
    switch (type)
    {
      case ChangeType.Add:
        if ((double) value == 0.0)
          break;
        setValue(oldValue + value);
        break;
      case ChangeType.Subtract:
        if ((double) value == 0.0)
          break;
        setValue(oldValue - value);
        break;
      case ChangeType.Set:
        if ((double) value == (double) oldValue)
          break;
        setValue(value);
        break;
    }
  }
}
