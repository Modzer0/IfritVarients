// Decompiled with JetBrains decompiler
// Type: qol.FieldModification.Core.FieldProcessorRegistry
// Assembly: qol, Version=1.1.6.3, Culture=neutral, PublicKeyToken=null
// MVID: 3B6539EA-E38C-45EE-8FFF-FC58CC42BADC
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\BepInEx\plugins\qol_1.1.6.3b1.dll

using System;
using System.Collections.Generic;

#nullable disable
namespace qol.FieldModification.Core;

public class FieldProcessorRegistry
{
  private readonly Dictionary<string, IFieldProcessor> _processors = new Dictionary<string, IFieldProcessor>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);

  public void Register(IFieldProcessor processor)
  {
    if (this._processors.ContainsKey(processor.Operation))
      throw new ArgumentException($"Processor for operation '{processor.Operation}' is already registered.");
    this._processors[processor.Operation] = processor;
  }

  public bool TryGetProcessor(string operation, out IFieldProcessor processor)
  {
    return this._processors.TryGetValue(operation, out processor);
  }

  public IFieldProcessor GetProcessor(string operation)
  {
    IFieldProcessor processor;
    if (!this._processors.TryGetValue(operation, out processor))
      throw new KeyNotFoundException($"No processor registered for operation '{operation}'.");
    return processor;
  }

  public IEnumerable<string> RegisteredOperations => (IEnumerable<string>) this._processors.Keys;

  public int Count => this._processors.Count;
}
