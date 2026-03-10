// Decompiled with JetBrains decompiler
// Type: NuclearOption.MissionEditorScripts.Buttons.SaveMissionButton
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using JamesFrowen.ScriptableVariables.UI;
using NuclearOption.SavedMission;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

#nullable disable
namespace NuclearOption.MissionEditorScripts.Buttons;

[RequireComponent(typeof (Button))]
public class SaveMissionButton : ButtonController
{
  [SerializeField]
  private InputField missionNameInput;
  [SerializeField]
  private Text saveMessage;
  [SerializeField]
  private MissionEditorPopupMenus popupMenus;

  protected override void Awake()
  {
    base.Awake();
    this.missionNameInput.onEndEdit.AddListener(new UnityAction<string>(this.OnNameChanged));
  }

  private void OnNameChanged(string name)
  {
    if (MissionSaveLoad.ValidateName(ref name, false))
      this.missionNameInput.SetTextWithoutNotify(name);
    MissionManager.CurrentMission.Name = name;
  }

  protected override void onClick()
  {
    MissionSaveLoad.SaveMission(MissionManager.CurrentMission);
    this.saveMessage.text = "Saved Mission: " + MissionManager.CurrentMission.Name;
    this.popupMenus.ShowSaveMenu();
  }
}
