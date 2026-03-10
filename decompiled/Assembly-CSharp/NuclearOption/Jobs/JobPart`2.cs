// Decompiled with JetBrains decompiler
// Type: NuclearOption.Jobs.JobPart`2
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
namespace NuclearOption.Jobs;

public class JobPart<TPart, TField> : IHasIndexInJob, IDisposable
  where TPart : MonoBehaviour
  where TField : unmanaged
{
  public readonly TPart Part;
  public readonly Ptr<TField> Field;
  public readonly List<PtrRefCounter<IndexLink>> links = new List<PtrRefCounter<IndexLink>>();

  public NullableIndex IndexInJob { get; set; }

  public JobPart(TPart part, Ptr<TField> field)
  {
    this.Part = part;
    this.Field = field;
  }

  public void Dispose()
  {
    foreach (PtrRefCounter<IndexLink> link in this.links)
      link.RemoveRef();
    this.links.Clear();
  }

  public override string ToString()
  {
    return !((UnityEngine.Object) this.Part != (UnityEngine.Object) null) ? "null" : $"{this.Part.GetInstanceID()} {this.Part}";
  }
}
