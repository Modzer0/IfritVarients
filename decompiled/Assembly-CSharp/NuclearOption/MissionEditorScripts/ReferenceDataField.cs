// Decompiled with JetBrains decompiler
// Type: NuclearOption.MissionEditorScripts.ReferenceDataField
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Cysharp.Threading.Tasks;
using NuclearOption.SavedMission.ObjectiveV2;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

#nullable disable
namespace NuclearOption.MissionEditorScripts;

public class ReferenceDataField : DataField
{
  private const string NONE_DARK_TEXT = "<color=#444><size=80%><none></size></color>";
  private const string NONE_LIGHT_TEXT = "<color=#AAA><size=80%><none></size></color>";
  [SerializeField]
  private Button openButton;
  [SerializeField]
  private TextMeshProUGUI currentValueText;
  public ReferencePopup Popup;
  [SerializeField]
  private float paddingBetweenPopup;
  [Tooltip("should light text be used for <none> instead of dark")]
  [SerializeField]
  private bool useLightNone;
  private ISaveableReference current;
  private ReferenceList.ListWrapper options;
  private Action<ISaveableReference> setValue;
  private bool allowNone;
  private bool dropdownOpen;

  protected override void SetFieldInteractable(bool value) => this.openButton.interactable = value;

  protected override void AwakeSetup()
  {
    this.openButton.onClick.AddListener(new UnityAction(this.OpenPopup));
    this.Popup.Hide();
    UniTask.Void((Func<UniTaskVoid>) (async () =>
    {
      ReferenceDataField referenceDataField = this;
      CancellationToken cancel = referenceDataField.destroyCancellationToken;
      await UniTask.Yield();
      await UniTask.Yield();
      if (cancel.IsCancellationRequested)
      {
        cancel = new CancellationToken();
      }
      else
      {
        referenceDataField.Popup.transform.SetParent(referenceDataField.GetComponentInParent<Canvas>().transform, true);
        cancel = new CancellationToken();
      }
    }));
  }

  private void Update()
  {
    if (!this.Popup.Holder.activeSelf)
      return;
    this.UpdatePopupPosition();
  }

  private void OnValidate()
  {
    if (!((UnityEngine.Object) this.Popup != (UnityEngine.Object) null) || !this.Popup.Holder.activeSelf)
      return;
    this.UpdatePopupPosition();
  }

  private void UpdatePopupPosition()
  {
    RectTransform transform1 = (RectTransform) this.Popup.transform;
    RectTransform transform2 = (RectTransform) this.transform;
    float x = transform2.lossyScale.x;
    Vector3 vector3 = (Vector3) new Vector2(transform2.position.x + (transform2.rect.width / 2f + this.paddingBetweenPopup) * x, transform2.position.y);
    transform1.position = vector3;
  }

  private void OnDestroy()
  {
    if (!((UnityEngine.Object) this.Popup.Holder != (UnityEngine.Object) null))
      return;
    UnityEngine.Object.Destroy((UnityEngine.Object) this.Popup.Holder);
  }

  public void Setup<T>(string label, List<T> options, T current, Action<T> setValue) where T : class, ISaveableReference
  {
    this.Setup<T>(label, (IList) options, current, setValue);
  }

  public void Setup<T>(string label, IList options, T current, Action<T> setValue) where T : class, ISaveableReference
  {
    this.label.text = label;
    this.options = new ReferenceList.ListWrapper(options);
    this.setValue = (Action<ISaveableReference>) (t => setValue((T) t));
    this.allowNone = true;
    this.SetValue((ISaveableReference) current);
    this.Interactable = true;
  }

  public void SetupReadOnly(string label, ISaveableReference current)
  {
    this.label.text = label;
    this.options = (ReferenceList.ListWrapper) null;
    this.setValue = (Action<ISaveableReference>) null;
    this.SetValue(current);
    this.Interactable = false;
    this.Popup.Hide();
  }

  public void SetupReadOnly(string label, string text)
  {
    this.label.text = label;
    this.options = (ReferenceList.ListWrapper) null;
    this.setValue = (Action<ISaveableReference>) null;
    this.current = (ISaveableReference) null;
    this.currentValueText.text = text ?? (this.useLightNone ? "<color=#AAA><size=80%><none></size></color>" : "<color=#444><size=80%><none></size></color>");
    this.Interactable = false;
    this.Popup.Hide();
  }

  private void SetValue(ISaveableReference current)
  {
    this.current = current;
    this.currentValueText.text = current?.ToUIString(true) ?? (this.useLightNone ? "<color=#AAA><size=80%><none></size></color>" : "<color=#444><size=80%><none></size></color>");
  }

  private void OpenPopup()
  {
    if (this.dropdownOpen)
    {
      Debug.LogWarning((object) "Drop down already open");
    }
    else
    {
      this.dropdownOpen = true;
      this.Popup.ShowPickOption(this.current, this.allowNone, (GetAllOptions) (() => (IEnumerable<ISaveableReference>) this.options), (ReferenceToString) (o => o.ToUIString()), (PickOrCancel) ((pick, obj) =>
      {
        this.dropdownOpen = false;
        if (!pick)
          return;
        this.SetValue(obj);
        this.setValue(obj);
        SceneSingleton<MissionEditor>.i.CheckAutoSave();
      }));
    }
  }
}
