// Decompiled with JetBrains decompiler
// Type: NuclearOption.SavedMission.ObjectiveV2.Objectives.IObjectiveList`1
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;

#nullable disable
namespace NuclearOption.SavedMission.ObjectiveV2.Objectives;

public interface IObjectiveList<T>
{
  float GetCompletePercent();

  void ReadNetworkData(List<int> data);

  List<int> UpdateNetworkList();

  bool UpdateAndCheck(CheckCallback<T> checkCallback);

  void ForeachNotComplete(Action<T> item);
}
