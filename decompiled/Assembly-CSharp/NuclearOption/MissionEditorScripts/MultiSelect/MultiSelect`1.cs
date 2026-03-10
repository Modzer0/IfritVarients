// Decompiled with JetBrains decompiler
// Type: NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect`1
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Mirage;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

#nullable disable
namespace NuclearOption.MissionEditorScripts.MultiSelect;

public class MultiSelect<TObject>
{
  private readonly List<TObject> _targets = new List<TObject>();
  public List<(object key, Action callback)> Changed = new List<(object, Action)>();

  public IReadOnlyList<TObject> Targets => (IReadOnlyList<TObject>) this._targets;

  public void AddAndInvokeChanged(object key, Action action)
  {
    action();
    this.Changed.Add((key, action));
  }

  public void RemoveChanged(object key)
  {
    for (int index = this.Changed.Count - 1; index >= 0; --index)
    {
      if (this.Changed[index].key == key)
        this.Changed.RemoveAt(index);
    }
  }

  private void TargetsChangedInvoke()
  {
    for (int index = 0; index < this.Changed.Count; ++index)
      this.Changed[index].callback();
  }

  public void ReplaceTargets(IReadOnlyList<TObject> targets)
  {
    if (targets != this._targets)
    {
      this._targets.Clear();
      this._targets.AddRange((IEnumerable<TObject>) targets);
    }
    this.TargetsChangedInvoke();
  }

  public void ReplaceTargets(TObject target)
  {
    this._targets.Clear();
    this._targets.Add(target);
    this.TargetsChangedInvoke();
  }

  public void AddTarget(TObject target)
  {
    this._targets.Add(target);
    this.TargetsChangedInvoke();
  }

  public void AddTargets(List<TObject> targets)
  {
    this._targets.AddRange((IEnumerable<TObject>) targets);
    this.TargetsChangedInvoke();
  }

  public void RemoveTarget(TObject target)
  {
    this._targets.Remove(target);
    this.TargetsChangedInvoke();
  }

  public void RemoveTargets(List<TObject> targets)
  {
    foreach (TObject target in targets)
      this._targets.Remove(target);
    this.TargetsChangedInvoke();
  }

  public Action<T> SetSameValueAction<T>(NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<TObject>.GetFieldRef<T> getField)
  {
    return (Action<T>) (value => this.SetSameValue<T>(getField, value));
  }

  public UnityAction<T> SetSameValueUnityAction<T>(NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<TObject>.GetFieldRef<T> getField)
  {
    return (UnityAction<T>) (value => this.SetSameValue<T>(getField, value));
  }

  public void SetSameValue<T>(NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<TObject>.GetFieldRef<T> getField, T value)
  {
    foreach (TObject target in this._targets)
    {
      // ISSUE: explicit reference operation
      ^getField(target) = value;
    }
  }

  public bool TryGetSameValue<T>(NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<TObject>.GetField<T> getField, out T sameValue)
  {
    return NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<TObject>.TryGetSameValue<T>((IReadOnlyList<TObject>) this._targets, getField, out sameValue);
  }

  public static bool TryGetSameValue<T>(
    IEnumerable<TObject> targetsEnumerable,
    NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<TObject>.GetField<T> getField,
    out T sameValue)
  {
    using (AutoPool<List<TObject>>.Wrapper wrapper = AutoPool<List<TObject>>.Take())
    {
      List<TObject> targets = wrapper.Item;
      targets.Clear();
      targets.AddRange(targetsEnumerable);
      return NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<TObject>.TryGetSameValue<T>((IReadOnlyList<TObject>) targets, getField, out sameValue);
    }
  }

  public static bool TryGetSameValue<T>(
    IReadOnlyList<TObject> targets,
    NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<TObject>.GetField<T> getField,
    out T sameValue)
  {
    sameValue = default (T);
    if (targets.Count == 0)
      return false;
    T x = getField(targets[0]);
    for (int index = 1; index < targets.Count; ++index)
    {
      T y = getField(targets[index]);
      if (!EqualityComparer<T>.Default.Equals(x, y))
        return false;
    }
    sameValue = x;
    return true;
  }

  public T GetSameValueOrDefault<T>(NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<TObject>.GetField<T> getField, T defaultValue)
  {
    T sameValue;
    return !this.TryGetSameValue<T>(getField, out sameValue) ? defaultValue : sameValue;
  }

  public string GetSameLabel<T>(
    NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<TObject>.GetField<T> getField,
    NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<TObject>.GetLabelString<T> getLabel)
  {
    T sameValue;
    return !this.TryGetSameValue<T>(getField, out sameValue) ? "-" : getLabel(sameValue);
  }

  public string GetSameLabel(NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<TObject>.GetField<string> getField)
  {
    string sameValue;
    return !this.TryGetSameValue<string>(getField, out sameValue) ? "-" : sameValue;
  }

  public void SetupSlider(
    Slider slider,
    TextMeshProUGUI text,
    NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<TObject>.GetFieldRef<float> getField,
    NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<TObject>.GetLabelString<float> getLabel)
  {
    this.SetupSlider<float>(slider, text, getField, (NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<TObject>.ModifyValue<float, float>) (v => v), (NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<TObject>.ModifyValue<float, float>) (v => v), getLabel);
  }

  public void SetupSlider<T>(
    Slider slider,
    TextMeshProUGUI text,
    NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<TObject>.GetFieldRef<T> getField,
    NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<TObject>.ModifyValue<T, float> fieldToSlider,
    NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<TObject>.ModifyValue<float, T> sliderToField,
    NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<TObject>.GetLabelString<T> getLabel)
  {
    slider.onValueChanged.AddListener((UnityAction<float>) (v =>
    {
      T obj = sliderToField(v);
      this.SetSameValue<T>(getField, obj);
      if (!((UnityEngine.Object) text != (UnityEngine.Object) null))
        return;
      text.text = getLabel(obj);
    }));
    this.AddAndInvokeChanged((object) slider, (Action) (() =>
    {
      T sameValue;
      slider.SetValueWithoutNotify(this.TryGetSameValue<T>(getField.Cast<TObject, T>(), out sameValue) ? fieldToSlider(sameValue) : 0.0f);
      if (!((UnityEngine.Object) text != (UnityEngine.Object) null))
        return;
      text.text = this.GetSameLabel<T>(getField.Cast<TObject, T>(), getLabel);
    }));
  }

  public void SetupDropdown(
    TMP_Dropdown dropdown,
    List<string> dropdownOptions,
    NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<TObject>.GetFieldRef<string> getField)
  {
    dropdown.options.Clear();
    dropdown.AddOptions(dropdownOptions);
    dropdown.onValueChanged.AddListener((UnityAction<int>) (index => this.SetSameValue<string>(getField, dropdownOptions[index])));
    string sameValue;
    this.AddAndInvokeChanged((object) dropdown, (Action) (() => dropdown.SetValueWithoutNotify(this.TryGetSameValue<string>(getField.Cast<TObject, string>(), out sameValue) ? dropdownOptions.IndexOf(sameValue) : -1)));
  }

  public void SetupDropdown<T>(
    TMP_Dropdown dropdown,
    NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<TObject>.GetFieldRef<T> getField,
    NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<TObject>.ModifyValue<T, int> fieldToDropdown,
    NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<TObject>.ModifyValue<int, T> dropdownToField)
  {
    dropdown.onValueChanged.AddListener((UnityAction<int>) (v => this.SetSameValue<T>(getField, dropdownToField(v))));
    T sameValue;
    this.AddAndInvokeChanged((object) dropdown, (Action) (() => dropdown.SetValueWithoutNotify(this.TryGetSameValue<T>(getField.Cast<TObject, T>(), out sameValue) ? fieldToDropdown(sameValue) : -1)));
  }

  public void SetupInputField(
    TMP_InputField inputField,
    NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<TObject>.GetFieldRef<string> getField)
  {
    this.SetupInputField<string>(inputField, getField, (NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<TObject>.ModifyValue<string, string>) (t => t), (NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<TObject>.ModifyValue<string, string>) (t => t));
  }

  public void SetupInputField<T>(
    TMP_InputField inputField,
    NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<TObject>.GetFieldRef<T> getField,
    NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<TObject>.ModifyValue<T, string> fieldToInputField,
    NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<TObject>.ModifyValue<string, T> inputFieldToField)
  {
    inputField.onEndEdit.AddListener((UnityAction<string>) (v => this.SetSameValue<T>(getField, inputFieldToField(v))));
    T sameValue;
    this.AddAndInvokeChanged((object) inputField, (Action) (() => inputField.SetTextWithoutNotify(this.TryGetSameValue<T>(getField.Cast<TObject, T>(), out sameValue) ? fieldToInputField(sameValue) : "-")));
  }

  public void SetupToggle(
    Toggle toggle,
    GameObject toggleDifferentValue,
    NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<TObject>.GetFieldRef<bool> getField)
  {
    toggle.onValueChanged.AddListener((UnityAction<bool>) (value => this.SetSameValue<bool>(getField, value)));
    this.AddAndInvokeChanged((object) toggle, (Action) (() =>
    {
      bool sameValue1;
      bool sameValue2 = this.TryGetSameValue<bool>(getField.Cast<TObject, bool>(), out sameValue1);
      bool flag = sameValue2 && sameValue1;
      if ((UnityEngine.Object) toggleDifferentValue != (UnityEngine.Object) null)
        toggleDifferentValue.SetActive(!sameValue2);
      toggle.SetIsOnWithoutNotify(flag);
    }));
  }

  public bool Any(NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<TObject>.GetField<bool> getField)
  {
    foreach (TObject target in this._targets)
    {
      if (getField(target))
        return true;
    }
    return false;
  }

  public bool All(NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<TObject>.GetField<bool> getField)
  {
    foreach (TObject target in this._targets)
    {
      if (!getField(target))
        return false;
    }
    return true;
  }

  public bool AllTheSame(
    NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<TObject>.EqualityChecker<TObject> areEqual)
  {
    return this.AllTheSame<TObject>((NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<TObject>.GetField<TObject>) (x => x), areEqual);
  }

  public bool AllTheSame<T>(NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<TObject>.GetField<T> getField) where T : IEquatable<T>
  {
    return this.AllTheSame<T>(getField, (NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<TObject>.EqualityChecker<T>) ((first, other) => EqualityComparer<T>.Default.Equals(first, other)));
  }

  public bool AllTheSame<T>(
    NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<TObject>.GetField<IReadOnlyList<T>> getField)
    where T : IEquatable<T>
  {
    return this.AllTheSame<T>(getField, (NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<TObject>.EqualityChecker<T>) ((first, other) => EqualityComparer<T>.Default.Equals(first, other)));
  }

  public bool AllTheSame<T>(
    NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<TObject>.GetField<IReadOnlyList<T>> getField,
    NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<TObject>.EqualityChecker<T> areEqual)
  {
    return this.AllTheSame<IReadOnlyList<T>>(getField, (NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<TObject>.EqualityChecker<IReadOnlyList<T>>) ((firstList, otherList) =>
    {
      int? count1 = firstList?.Count;
      int? count2 = otherList?.Count;
      if (!(count1.GetValueOrDefault() == count2.GetValueOrDefault() & count1.HasValue == count2.HasValue))
        return false;
      for (int index = 0; index < firstList.Count; ++index)
      {
        if (!areEqual(firstList[index], otherList[index]))
          return false;
      }
      return true;
    }));
  }

  [Conditional("UNITY_ASSERTIONS")]
  private static void AssertNotReferenceEquals<T>(T a, T b)
  {
    if (typeof (T).IsValueType || (object) a != (object) b)
      return;
    UnityEngine.Debug.LogError((object) "2 lists had the same reference when using classes. Each target should have its own objects");
  }

  public bool AllTheSame<T>(
    NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<TObject>.GetField<T> getField,
    NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<TObject>.EqualityChecker<T> checker)
  {
    if (this.Targets.Count == 0)
      return false;
    if (this.Targets.Count == 1)
      return true;
    T first = getField(this.Targets[0]);
    for (int index = 1; index < this.Targets.Count; ++index)
    {
      T other = getField(this.Targets[index]);
      if (!checker(first, other))
        return false;
    }
    return true;
  }

  public delegate bool EqualityChecker<T>(T first, T other);

  public delegate T GetField<T>(TObject obj);

  public delegate ref T GetFieldRef<T>(TObject obj);

  public delegate TTo ModifyValue<TFrom, TTo>(TFrom from);

  public delegate string GetLabelString<T>(T value);
}
