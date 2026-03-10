// Decompiled with JetBrains decompiler
// Type: Heap`1
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

#nullable disable
public class Heap<T> where T : IHeapItem<T>
{
  private T[] items;
  private int currentItemCount;

  public Heap(int maxHeapSize) => this.items = new T[maxHeapSize];

  public void Add(T item)
  {
    item.HeapIndex = this.currentItemCount;
    this.items[this.currentItemCount] = item;
    this.SortUp(item);
    ++this.currentItemCount;
  }

  public void UpdateItem(T item) => this.SortUp(item);

  public int Count => this.currentItemCount;

  public bool Contains(T item) => object.Equals((object) this.items[item.HeapIndex], (object) item);

  public T RemoveFirst()
  {
    T obj = this.items[0];
    --this.currentItemCount;
    this.items[0] = this.items[this.currentItemCount];
    this.items[0].HeapIndex = 0;
    this.SortDown(this.items[0]);
    return obj;
  }

  private void SortDown(T item)
  {
    while (true)
    {
      int index1 = item.HeapIndex * 2 + 1;
      int index2 = item.HeapIndex * 2 + 2;
      if (index1 < this.currentItemCount)
      {
        int index3 = index1;
        if (index2 < this.currentItemCount && this.items[index1].CompareTo(this.items[index2]) < 0)
          index3 = index2;
        if (item.CompareTo(this.items[index3]) < 0)
          this.Swap(item, this.items[index3]);
        else
          goto label_6;
      }
      else
        break;
    }
    return;
label_6:;
  }

  private void SortUp(T item)
  {
    int index = (item.HeapIndex - 1) / 2;
    while (true)
    {
      T obj = this.items[index];
      if (item.CompareTo(obj) > 0)
      {
        this.Swap(item, obj);
        index = (item.HeapIndex - 1) / 2;
      }
      else
        break;
    }
  }

  private void Swap(T itemA, T itemB)
  {
    this.items[itemA.HeapIndex] = itemB;
    this.items[itemB.HeapIndex] = itemA;
    int heapIndex = itemA.HeapIndex;
    itemA.HeapIndex = itemB.HeapIndex;
    itemB.HeapIndex = heapIndex;
  }
}
