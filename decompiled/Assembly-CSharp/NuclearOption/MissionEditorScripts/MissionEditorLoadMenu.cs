// Decompiled with JetBrains decompiler
// Type: NuclearOption.MissionEditorScripts.MissionEditorLoadMenu
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Cysharp.Threading.Tasks;
using NuclearOption.SavedMission;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#nullable disable
namespace NuclearOption.MissionEditorScripts;

public class MissionEditorLoadMenu : MonoBehaviour
{
  [SerializeField]
  private MissionsPicker missionsPicker;
  [SerializeField]
  private GameObject loadingNotification;

  private void Awake()
  {
    this.missionsPicker.OnMissionConfirmed += new Action<Mission>(this.OnMissionConfirmed);
    List<MissionGroup> list = ((IEnumerable<MissionGroup>) MissionGroup.AllGroups).Except<MissionGroup>((IEnumerable<MissionGroup>) new List<MissionGroup>()
    {
      (MissionGroup) MissionGroup.Default,
      (MissionGroup) MissionGroup.User,
      (MissionGroup) MissionGroup.BuiltIn
    }).ToList<MissionGroup>();
    this.missionsPicker.SetPickerFilter(new MissionsPicker.Filter()
    {
      DisallowedGroups = list
    });
  }

  private void OnMissionConfirmed(Mission mission)
  {
    if ((UnityEngine.Object) this.loadingNotification != (UnityEngine.Object) null)
      this.loadingNotification.SetActive(true);
    MissionEditor.LoadEditor(mission).Forget();
  }
}
