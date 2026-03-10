// Decompiled with JetBrains decompiler
// Type: NuclearOption.AddressableScripts.ModLoader`2
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;

#nullable disable
namespace NuclearOption.AddressableScripts;

public abstract class ModLoader<TMeta, TData> : ModLoader where TMeta : struct, IMetaData
{
  public override Type AssetType => typeof (TData);

  public abstract IEnumerable<TMeta> ListMetaData();
}
