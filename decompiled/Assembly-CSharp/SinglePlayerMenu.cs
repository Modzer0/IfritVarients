// Decompiled with JetBrains decompiler
// Type: SinglePlayerMenu
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.Networking;
using NuclearOption.SavedMission;
using System;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
public class SinglePlayerMenu : MonoBehaviour
{
  [SerializeField]
  private MissionsPicker picker;

  private void Awake()
  {
    this.picker.OnMissionConfirmed += new Action<Mission>(this.StartMission);
    this.picker.SetPickerFilter(new MissionsPicker.Filter()
    {
      RequiredTags = new List<MissionTag>()
      {
        MissionTag.SinglePlayer
      }
    });
    this.picker.ShowPicker();
  }

  private void StartMission(Mission mission)
  {
    if (mission == null)
      throw new InvalidOperationException("Start Mission should not be called while mission is null");
    MissionManager.SetMission(mission, false);
    NetworkManagerNuclearOption.i.StartHost(new HostOptions(SocketType.Offline, GameState.SinglePlayer, mission.MapKey));
  }
}
