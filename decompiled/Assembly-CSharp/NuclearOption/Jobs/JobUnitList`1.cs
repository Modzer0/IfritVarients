// Decompiled with JetBrains decompiler
// Type: NuclearOption.Jobs.JobUnitList`1
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
namespace NuclearOption.Jobs;

public class JobUnitList<T> where T : IHasIndexInJob
{
  private readonly List<T> _items = new List<T>();
  public readonly List<T> PendingAdd = new List<T>();
  public readonly List<int> PendingRemove = new List<int>();
  private bool removeNeedsSorting;

  public int CountAfterPending
  {
    get => this._items.Count + this.PendingAdd.Count - this.PendingRemove.Count;
  }

  public bool PendingChangesAdd => this.PendingAdd.Count > 0;

  public bool PendingChangesRemove => this.PendingRemove.Count > 0;

  public void Add(T item) => this.PendingAdd.Add(item);

  private bool Contains(T item)
  {
    foreach (T obj in this.PendingAdd)
    {
      if (obj.Equals((object) item))
        return true;
    }
    foreach (T obj in this._items)
    {
      if (obj.Equals((object) item))
        return true;
    }
    return false;
  }

  public void Remove(T item)
  {
    NullableIndex indexInJob = item.IndexInJob;
    if (indexInJob.HasValue)
    {
      this.PendingRemove.Add(indexInJob.Index);
      this.removeNeedsSorting = true;
    }
    else
    {
      if (this.PendingAdd.Remove(item))
        return;
      Debug.LogError((object) "Item being removed was not in item or pending");
    }
  }

  public void FullClear()
  {
    foreach (T obj in this._items)
    {
      if ((object) obj != null && obj is IDisposable disposable)
        disposable.Dispose();
    }
    foreach (T obj in this.PendingAdd)
    {
      if ((object) obj != null && obj is IDisposable disposable)
        disposable.Dispose();
    }
    this._items.Clear();
    this.PendingAdd.Clear();
    this.PendingRemove.Clear();
  }

  public bool ProcessNextAdded(out T add, out int addIndex)
  {
    if (this.PendingAdd.Count == 0)
    {
      add = default (T);
      addIndex = 0;
      return false;
    }
    int index = this.PendingAdd.Count - 1;
    add = this.PendingAdd[index];
    this.PendingAdd.RemoveAt(index);
    addIndex = this._items.Count;
    this._items.Add(add);
    add.IndexInJob = new NullableIndex(addIndex);
    return true;
  }

  public bool ProcessNextRemoved(out int removeIndex, out T removedItem)
  {
    if (this.PendingRemove.Count == 0)
    {
      removeIndex = 0;
      removedItem = default (T);
      return false;
    }
    if (this.removeNeedsSorting)
    {
      this.PendingRemove.Sort();
      this.removeNeedsSorting = false;
    }
    int index = this.PendingRemove.Count - 1;
    removeIndex = this.PendingRemove[index];
    this.PendingRemove.RemoveAt(index);
    this.RemoveAtSwap(removeIndex, out removedItem);
    return true;
  }

  private void RemoveAtSwap(int removeIndex, out T removedItem)
  {
    removedItem = this._items[removeIndex];
    int index = this._items.Count - 1;
    if (index != removeIndex)
    {
      T obj = this._items[index];
      this._items[removeIndex] = obj;
      obj.IndexInJob = new NullableIndex(removeIndex);
    }
    this._items.RemoveAt(index);
  }
}
