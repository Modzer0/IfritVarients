// Decompiled with JetBrains decompiler
// Type: NuclearOption.SavedMission.ObjectiveV2.ReadWriteObjective
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;

#nullable disable
namespace NuclearOption.SavedMission.ObjectiveV2;

public class ReadWriteObjective
{
  public readonly ReadWriteObjective.Mode mode;
  private readonly bool write;
  private readonly MissionLookups lookups;
  private ReadWriteObjective.Data data;

  private LoadErrors loadErrors => this.lookups.LoadErrors;

  public ReadWriteObjective(
    ReadWriteObjective.Mode mode,
    ref List<ObjectiveData> data,
    MissionLookups lookups)
  {
    if (mode == ReadWriteObjective.Mode.Read && lookups == null)
      throw new ArgumentNullException(nameof (lookups));
    if (data == null)
      data = new List<ObjectiveData>();
    this.data = new ReadWriteObjective.Data(data);
    this.lookups = lookups;
    this.mode = mode;
    this.write = mode == ReadWriteObjective.Mode.Write;
    if (mode != ReadWriteObjective.Mode.Write)
      return;
    data.Clear();
  }

  public void Reference<T>(ref T reference) where T : ISaveableReference
  {
    if (this.write)
    {
      this.data.Push(new ObjectiveData((object) reference != null ? reference.GetNameSavedCheckDestroyed<T>() : ""));
    }
    else
    {
      string key = (string) null;
      this.String(ref key, (string) null);
      if (string.IsNullOrEmpty(key))
      {
        reference = default (T);
      }
      else
      {
        Dictionary<string, T> lookup = this.FindLookup<T>();
        reference = lookup[key];
      }
    }
  }

  public void ReferenceList<T>(ref List<T> references, int? readCount = null) where T : ISaveableReference
  {
    if (this.write)
    {
      if (references == null)
        return;
      foreach (T reference in references)
        this.data.Push(new ObjectiveData(reference.GetNameSavedCheckDestroyed<T>()));
    }
    else
    {
      if (references == null)
        references = new List<T>();
      references.Clear();
      Dictionary<string, T> lookup = this.FindLookup<T>();
      ObjectiveData objectiveData;
      while (this.data.ReadLoop(ref readCount, out objectiveData))
      {
        string stringValue = objectiveData.StringValue;
        T obj;
        if (lookup.TryGetValue(stringValue, out obj))
          references.Add(obj);
        else
          this.loadErrors.Warn($"'{stringValue}' was not found in lookup for {typeof (T).Name}");
      }
    }
  }

  private void ReadMany(int? readCount, Action<ObjectiveData> callback)
  {
    int num = readCount ?? this.data.Remaining;
    for (int index = 0; index < num; ++index)
    {
      ObjectiveData objectiveData = this.data.Pop();
      callback(objectiveData);
    }
  }

  private Dictionary<string, T> FindLookup<T>() where T : ISaveableReference
  {
    System.Type type = typeof (T);
    if (type == typeof (Objective))
      return (Dictionary<string, T>) this.lookups.Objectives;
    if (type == typeof (Outcome))
      return (Dictionary<string, T>) this.lookups.Outcomes;
    if (type == typeof (SavedUnit))
      return (Dictionary<string, T>) this.lookups.SavedUnits;
    if (type == typeof (SavedAirbase))
      return (Dictionary<string, T>) this.lookups.Airbases;
    throw new ArgumentException($"Invalid type {type}");
  }

  public void DataList<T>(ref List<T> saveable, int? readCount = null) where T : ISaveableData, new()
  {
    if (this.write)
    {
      if (saveable == null)
        return;
      foreach (T obj in saveable)
        this.data.Push(obj.ToObjectiveData());
    }
    else
    {
      if (saveable == null)
        saveable = new List<T>();
      saveable.Clear();
      List<T> objList = saveable;
      ObjectiveData data;
      while (this.data.ReadLoop(ref readCount, out data))
      {
        T obj = new T();
        obj.FromObjectiveData(data, this.lookups);
        objList.Add(obj);
      }
    }
  }

  public void StringList(ref List<string> values, int? readCount = null)
  {
    if (this.write)
    {
      if (values == null)
        return;
      foreach (string str in values)
        this.data.Push(new ObjectiveData(str));
    }
    else
    {
      if (values == null)
        values = new List<string>();
      values.Clear();
      ObjectiveData objectiveData;
      while (this.data.ReadLoop(ref readCount, out objectiveData))
        values.Add(objectiveData.StringValue);
    }
  }

  public void Float(ref float value, float defaultValue = 0.0f)
  {
    if (this.write)
    {
      this.data.Push(new ObjectiveData(value));
    }
    else
    {
      ObjectiveData objectiveData;
      value = this.data.TryPop(out objectiveData) ? objectiveData.FloatValue : defaultValue;
    }
  }

  public void Int(ref int value, int defaultValue = 0)
  {
    if (this.write)
    {
      this.data.Push(new ObjectiveData((float) value));
    }
    else
    {
      ObjectiveData objectiveData;
      value = this.data.TryPop(out objectiveData) ? (int) objectiveData.FloatValue : defaultValue;
    }
  }

  public void Enum<T>(ref T value, T defaultValue = default (T)) where T : unmanaged, System.Enum
  {
    int num = ReadWriteObjective.UnsafeCast<T, int>(value);
    int defaultValue1 = ReadWriteObjective.UnsafeCast<T, int>(defaultValue);
    this.Int(ref num, defaultValue1);
    value = ReadWriteObjective.UnsafeCast<int, T>(num);
    SaveHelper.ValidateEnum<T>(value);
  }

  private static unsafe TResult UnsafeCast<TFrom, TResult>(TFrom value)
    where TFrom : unmanaged
    where TResult : unmanaged
  {
    return *(TResult*) &value;
  }

  public void Bool(ref bool value, bool defaultValue = false)
  {
    if (this.write)
    {
      this.data.Push(new ObjectiveData(value ? 1f : 0.0f));
    }
    else
    {
      ObjectiveData objectiveData;
      value = this.data.TryPop(out objectiveData) ? (double) objectiveData.FloatValue == 1.0 : defaultValue;
    }
  }

  public void String(ref string value, string defaultValue = "")
  {
    if (this.write)
    {
      this.data.Push(new ObjectiveData(value));
    }
    else
    {
      ObjectiveData objectiveData;
      value = this.data.TryPop(out objectiveData) ? objectiveData.StringValue : defaultValue;
    }
  }

  public void Vector3(ref UnityEngine.Vector3 value, UnityEngine.Vector3 defaultValue)
  {
    if (this.write)
    {
      this.data.Push(new ObjectiveData(value));
    }
    else
    {
      ObjectiveData objectiveData;
      value = this.data.TryPop(out objectiveData) ? objectiveData.VectorValue : defaultValue;
    }
  }

  public void Override<T>(
    ref NuclearOption.SavedMission.ObjectiveV2.Override<T> @override,
    ReadWriteObjective.ReadWriteValue<T> readWriteValue)
    where T : IEquatable<T>
  {
    if (this.write)
    {
      this.data.Push(new ObjectiveData(@override.IsOverride ? 1f : 0.0f));
      if (!@override.IsOverride)
        return;
      T obj = @override.Value;
      readWriteValue(ref obj);
    }
    else
    {
      bool isOverride = (double) this.data.PopOrDefault().FloatValue == 1.0;
      T obj = default (T);
      if (isOverride)
        readWriteValue(ref obj);
      @override = new NuclearOption.SavedMission.ObjectiveV2.Override<T>(isOverride, obj);
    }
  }

  public delegate void ReadWriteValue<T>(ref T value, T defaultValue = null);

  private class Data
  {
    private readonly List<ObjectiveData> data;
    private int index;

    public int Remaining => this.data.Count - this.index;

    public Data(List<ObjectiveData> data) => this.data = data;

    public void Push(ObjectiveData data) => this.data.Add(data);

    public ObjectiveData Pop()
    {
      ObjectiveData objectiveData = this.data[this.index];
      ++this.index;
      return objectiveData;
    }

    public ObjectiveData PopOrDefault(ObjectiveData @default = default (ObjectiveData))
    {
      ObjectiveData objectiveData;
      return !this.TryPop(out objectiveData) ? @default : objectiveData;
    }

    public bool TryPop(out ObjectiveData value)
    {
      if (this.index < this.data.Count)
      {
        value = this.Pop();
        return true;
      }
      value = new ObjectiveData();
      return false;
    }

    public bool ReadLoop(ref int? readCount, out ObjectiveData value)
    {
      int num = readCount ?? this.Remaining;
      if (num > 0)
      {
        value = this.Pop();
        readCount = new int?(num - 1);
        return true;
      }
      value = new ObjectiveData();
      return false;
    }
  }

  public enum Mode
  {
    Read,
    Write,
  }
}
