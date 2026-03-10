// Decompiled with JetBrains decompiler
// Type: LazySortedQueue`1
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;

#nullable disable
public class LazySortedQueue<T>
{
  private bool needsSorting;
  private readonly List<T> list = new List<T>();
  private readonly Comparison<T> comparison;

  public int Count => this.list.Count;

  public LazySortedQueue(Comparison<T> comparison)
  {
    this.comparison = (Comparison<T>) ((x, y) => comparison(y, x));
  }

  public void Enqueue(T item)
  {
    this.list.Add(item);
    this.needsSorting = true;
  }

  public bool TryDequeue(out T item)
  {
    if (this.list.Count == 0)
    {
      item = default (T);
      return false;
    }
    if (this.needsSorting)
    {
      this.list.Sort(this.comparison);
      this.needsSorting = false;
    }
    item = this.list[this.list.Count - 1];
    this.list.RemoveAt(this.list.Count - 1);
    return true;
  }
}
