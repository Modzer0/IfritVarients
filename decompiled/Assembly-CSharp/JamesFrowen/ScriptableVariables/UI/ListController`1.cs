// Decompiled with JetBrains decompiler
// Type: JamesFrowen.ScriptableVariables.UI.ListController`1
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

#nullable disable
namespace JamesFrowen.ScriptableVariables.UI;

public abstract class ListController<T> : MonoBehaviour
{
  private static readonly ProfilerMarker updateListMarker = new ProfilerMarker("ListController UpdateList");
  private static readonly ProfilerMarker updateListValuesMarker = new ProfilerMarker("ListController UpdateList Values");
  private static readonly ProfilerMarker populateRowsMarker = new ProfilerMarker("ListController PopulateRows");
  private static readonly ProfilerMarker populateRowsResultMarker = new ProfilerMarker("ListController PopulateRows Result");
  [SerializeField]
  protected RectTransform _parent;
  [SerializeField]
  private int _maxRows = 100;
  [SerializeField]
  private GameObject _prefab;
  [SerializeField]
  private bool _findExisting;
  [SerializeField]
  private bool _includeInactive;
  [SerializeField]
  private bool _updateInViewOnly;
  [SerializeField]
  private ScrollRect _scrollView;
  private bool _hasInit;
  private readonly List<ListItem<T>> _rows = new List<ListItem<T>>();
  private readonly List<T> _values = new List<T>();
  private bool taskIsRunning;
  private int viewIndexTop;
  private int viewIndexBottom;

  private int ValueCountClamped => Math.Min(this._values.Count, this._maxRows);

  protected virtual void Awake()
  {
    this.CheckInit(true);
    if (!this._updateInViewOnly)
      return;
    this._scrollView.onValueChanged.AddListener(new UnityAction<Vector2>(this.ScrollViewChanged));
  }

  private void ScrollViewChanged(Vector2 scroll)
  {
    if (this.taskIsRunning || this._rows.Count <= 0 || this._values.Count <= 0)
      return;
    float height = this._scrollView.viewport.rect.height;
    float num1 = 1f - scroll.y;
    RectTransform parent = this._parent;
    float num2 = 0.0f;
    float num3 = 0.0f;
    VerticalLayoutGroup component;
    if (parent.TryGetComponent<VerticalLayoutGroup>(out component))
    {
      num2 = (float) (component.padding.top + component.padding.bottom);
      num3 = component.spacing;
    }
    float num4 = parent.sizeDelta.y - num2;
    float num5 = this._rows.First<ListItem<T>>().CalculateHeight(this._values.First<T>()) + num3;
    float num6 = (num4 - height) * num1;
    float num7 = num6 + height;
    int num8 = Mathf.FloorToInt(num6 / num5);
    int num9 = Mathf.CeilToInt(num7 / num5) + 1;
    Debug.Log((object) $"view:{height} content:{num4} {num1}%, {num6} -> {num7}, {num8} -> {num9}");
    int a1 = Mathf.Clamp(num8, 0, this.ValueCountClamped - 1);
    int a2 = Mathf.Clamp(num9, 0, this.ValueCountClamped - 1);
    int num10 = Mathf.Min(a1, this.viewIndexTop);
    int num11 = Mathf.Max(a2, this.viewIndexBottom);
    for (int index = num10; index <= num11; ++index)
    {
      bool visible = a1 <= index && index <= a2;
      this.ViewChanged(index, visible);
    }
    this.viewIndexTop = a1;
    this.viewIndexBottom = a2;
  }

  private void ViewChanged(int index, bool visible)
  {
    ListItem<T> row = this._rows[index];
    if (row.IsActive == visible)
      return;
    Debug.Log((object) $"{index}: {visible}");
    row.SetActive(visible);
    if (!visible)
      return;
    row.SetNewValue(this._values[index]);
  }

  private void CheckInit(bool setEmpty)
  {
    if (this._hasInit)
      return;
    this._hasInit = true;
    if (this._findExisting)
    {
      this._rows.AddRange((IEnumerable<ListItem<T>>) this.GetComponentsInChildren<ListItem<T>>(this._includeInactive));
      if (this._updateInViewOnly)
      {
        foreach (ListItem<T> row in this._rows)
          row.SetActive(false);
      }
    }
    if (!setEmpty)
      return;
    this.UpdateList((IReadOnlyList<T>) Array.Empty<T>());
  }

  protected virtual void OnValidate()
  {
  }

  public void UpdateList(IEnumerable<T> values)
  {
    using (ListController<T>.updateListMarker.Auto())
    {
      this._values.Clear();
      if (values != null)
        this._values.AddRange(values);
      if (this.taskIsRunning)
        return;
      this.UpdateListInner().Forget();
    }
  }

  public void UpdateList(IReadOnlyList<T> values)
  {
    using (ListController<T>.updateListMarker.Auto())
    {
      this._values.Clear();
      if (values != null)
        this._values.AddRange((IEnumerable<T>) values);
      if (this.taskIsRunning)
        return;
      this.UpdateListInner().Forget();
    }
  }

  public async UniTask UpdateListInner()
  {
    ListController<T> listController = this;
    listController.taskIsRunning = true;
    CancellationToken cancel = listController.destroyCancellationToken;
    try
    {
      listController.CheckInit(false);
      do
      {
        do
        {
          int createCount = Math.Min(listController._maxRows, listController._values.Count) - listController._rows.Count;
          if (createCount > 0)
          {
            await listController.CreateNewRows(createCount);
            if (cancel.IsCancellationRequested)
              goto label_2;
          }
          else
            goto label_9;
        }
        while (!listController._updateInViewOnly);
        listController.RebuildLayout();
        await UniTask.Yield();
      }
      while (!cancel.IsCancellationRequested);
      goto label_14;
label_2:
      cancel = new CancellationToken();
      return;
label_14:
      cancel = new CancellationToken();
      return;
    }
    finally
    {
      listController.taskIsRunning = false;
    }
label_9:
    using (ListController<T>.updateListValuesMarker.Auto())
    {
      if (listController._updateInViewOnly)
      {
        listController.ScrollViewChanged(listController._scrollView.normalizedPosition);
        cancel = new CancellationToken();
      }
      else
      {
        listController.UpdateRows();
        cancel = new CancellationToken();
      }
    }
  }

  private void UpdateRows()
  {
    for (int index = 0; index < this._rows.Count; ++index)
    {
      ListItem<T> row = this._rows[index];
      if (index < this.ValueCountClamped)
      {
        row.SetActive(true);
        row.SetNewValue(this._values[index]);
      }
      else
        row.SetActive(false);
    }
    this.RebuildLayout();
  }

  public void Redraw()
  {
    int valueCountClamped = this.ValueCountClamped;
    for (int index = 0; index < valueCountClamped; ++index)
      this._rows[index].Redraw();
  }

  private async UniTask CreateNewRows(int createCount)
  {
    AsyncInstantiateOperation<GameObject> op;
    using (ListController<T>.populateRowsMarker.Auto())
      op = UnityEngine.Object.InstantiateAsync<GameObject>(this._prefab, createCount, (Transform) this._parent);
    GameObject[] uniTask = await op.ToUniTask<GameObject>();
    using (ListController<T>.populateRowsResultMarker.Auto())
    {
      for (int index = 0; index < createCount; ++index)
      {
        ListItem<T> component = op.Result[index].GetComponent<ListItem<T>>();
        this._rows.Add(component);
        if (this._updateInViewOnly)
          component.SetActive(false);
      }
    }
    op = (AsyncInstantiateOperation<GameObject>) null;
  }

  protected virtual void RebuildLayout()
  {
  }
}
