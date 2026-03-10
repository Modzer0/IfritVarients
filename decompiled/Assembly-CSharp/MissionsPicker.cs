// Decompiled with JetBrains decompiler
// Type: MissionsPicker
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Cysharp.Threading.Tasks;
using NuclearOption.SavedMission;
using System;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

#nullable disable
public class MissionsPicker : SceneSingleton<MissionsPicker>
{
  [Header("Mission Preview")]
  [SerializeField]
  private MissionSelectPanel missionSelect;
  [SerializeField]
  private MissionSelectGroupButtons missionSelectGroups;
  [SerializeField]
  private Text missionTitle;
  [SerializeField]
  private TextMeshProUGUI missionDescription;
  [SerializeField]
  private Image previewImage;
  [SerializeField]
  private Sprite defaultMissionImage;
  [Header("Buttons")]
  [SerializeField]
  private Button openUserFolder;
  [SerializeField]
  private TextMeshProUGUI confirmText;
  [SerializeField]
  private Button confirmButton;
  [Header("Failed to load")]
  [SerializeField]
  private GameObject failedOverlay;
  [SerializeField]
  private TextMeshProUGUI failedText;
  [SerializeField]
  private Button closeFailed;
  private Color normalColor;
  private Mission mission;
  private CancellationTokenSource cancelGetPreview;

  public Mission Mission => this.mission;

  public event Action<Mission> OnMissionSelect;

  public event Action<Mission> OnMissionConfirmed;

  public void ShowPicker() => this.gameObject.SetActive(true);

  public void HidePicker() => this.gameObject.SetActive(false);

  public void SetPickerFilter(MissionsPicker.Filter filter)
  {
    this.missionSelect.SetPickerFilter(filter);
    this.missionSelectGroups.SetPickerFilter(filter);
  }

  protected override void Awake()
  {
    base.Awake();
    this.normalColor = this.missionDescription.color;
  }

  private void OnEnable() => this.SelectMission(this.missionSelect.SelectedMission);

  private void Start()
  {
    PlayerSettings.LoadPrefs();
    this.openUserFolder.onClick.AddListener(new UnityAction(MissionGroup.UserGroup.OpenFolder));
    this.confirmButton.onClick.AddListener(new UnityAction(this.ConfirmPressed));
    this.closeFailed.onClick.AddListener(new UnityAction(this.CloseFailed));
    this.missionSelect.OnMissionSelecteed += new Action<MissionKey>(this.SelectMission);
    this.SelectMission(this.missionSelect.SelectedMission);
  }

  public void SelectMission(MissionKey item)
  {
    if (!item.IsValid())
    {
      this.missionTitle.text = "";
      this.missionDescription.text = "";
      this.confirmButton.interactable = false;
      this.previewImage.sprite = (Sprite) null;
      this.previewImage.color = new Color(0.2f, 0.2f, 0.2f);
    }
    else
    {
      this.missionTitle.text = item.Name;
      this.GetPreviewAsync(item).Forget();
      string error;
      if (item.TryLoad(out this.mission, out error))
      {
        Action<Mission> onMissionSelect = this.OnMissionSelect;
        if (onMissionSelect != null)
          onMissionSelect(this.Mission);
        this.missionDescription.text = this.Mission.missionSettings.description;
        this.missionDescription.color = this.normalColor;
        this.confirmButton.interactable = true;
      }
      else
      {
        Action<Mission> onMissionSelect = this.OnMissionSelect;
        if (onMissionSelect != null)
          onMissionSelect((Mission) null);
        this.missionDescription.text = error;
        this.missionDescription.color = Color.red;
        this.confirmButton.interactable = false;
        this.failedOverlay.SetActive(true);
        this.failedText.text = error;
      }
    }
  }

  public void SetMissionWithoutNotify(Mission mission) => this.mission = mission;

  private async UniTaskVoid GetPreviewAsync(MissionKey item)
  {
    this.cancelGetPreview?.Cancel();
    this.cancelGetPreview = new CancellationTokenSource();
    CancellationToken token = this.cancelGetPreview.Token;
    this.previewImage.color = new Color(0.2f, 0.2f, 0.2f);
    Sprite preview = await item.GetPreview(token);
    if (token.IsCancellationRequested)
    {
      token = new CancellationToken();
    }
    else
    {
      this.previewImage.color = Color.white;
      this.previewImage.sprite = preview ?? this.defaultMissionImage;
      token = new CancellationToken();
    }
  }

  private void CloseFailed() => this.failedOverlay.SetActive(false);

  private void ConfirmPressed() => this.OnMissionConfirmed(this.Mission);

  public struct Filter
  {
    public List<MissionGroup> DisallowedGroups;
    public List<MissionTag> RequiredTags;
  }
}
