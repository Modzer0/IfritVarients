// Decompiled with JetBrains decompiler
// Type: NuclearOption.MissionEditorScripts.UndoRedo
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;

#nullable disable
namespace NuclearOption.MissionEditorScripts;

public static class UndoRedo
{
  private static UndoRedo.RingBuffer<UndoRedo.Command> UndoStack;
  private static UndoRedo.RingBuffer<UndoRedo.Command> RedoStack;

  public static void Init(int stackSize)
  {
    UndoRedo.UndoStack = new UndoRedo.RingBuffer<UndoRedo.Command>(stackSize);
    UndoRedo.RedoStack = new UndoRedo.RingBuffer<UndoRedo.Command>(stackSize);
  }

  public static void Do(UndoRedo.Command command)
  {
    UndoRedo.UndoStack.Push(command);
    UndoRedo.RedoStack.Clear();
    command.Do();
  }

  public static void Undo(int count)
  {
    UndoRedo.Command command;
    for (int index = 0; index < count && UndoRedo.UndoStack.TryPop(out command); ++index)
      command.Undo();
  }

  public static void Redo(int count)
  {
    UndoRedo.Command command;
    for (int index = 0; index < count && UndoRedo.RedoStack.TryPop(out command); ++index)
      command.Do();
  }

  public static IEnumerable<string> GetUndoDescriptions()
  {
    return UndoRedo.GetDescriptions(UndoRedo.UndoStack);
  }

  public static IEnumerable<string> GetRedoDescriptions()
  {
    return UndoRedo.GetDescriptions(UndoRedo.RedoStack);
  }

  private static IEnumerable<string> GetDescriptions(UndoRedo.RingBuffer<UndoRedo.Command> buffer)
  {
    for (int index = (buffer.Head - 1 + buffer.Buffer.Length) % buffer.Buffer.Length; index != buffer.Tail; index = (index - 1 + buffer.Buffer.Length) % buffer.Buffer.Length)
      yield return buffer.Buffer[index].Description;
  }

  public class RingBuffer<T>
  {
    public T[] Buffer;
    public int Head;
    public int Tail;

    public RingBuffer(int capcity) => this.Buffer = new T[capcity];

    public void Push(T item)
    {
      this.Buffer[this.Head] = item;
      this.Head = (this.Head + 1) % this.Buffer.Length;
      if (this.Head != this.Tail)
        return;
      this.Tail = (this.Tail + 1) % this.Buffer.Length;
    }

    public bool TryPop(out T item)
    {
      if (this.Head == this.Tail)
      {
        item = default (T);
        return false;
      }
      item = this.Buffer[this.Tail];
      this.Tail = (this.Tail + 1) % this.Buffer.Length;
      return true;
    }

    public void Clear()
    {
      for (int index = 0; index < this.Buffer.Length; ++index)
        this.Buffer[index] = default (T);
      this.Head = 0;
      this.Tail = 0;
    }
  }

  public readonly struct Command
  {
    public readonly string Description;
    public readonly Action Do;
    public readonly Action Undo;
  }
}
