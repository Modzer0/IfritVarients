// Decompiled with JetBrains decompiler
// Type: RenderBatch
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

#nullable disable
public struct RenderBatch(Matrix4x4[] array, int count)
{
  public readonly Matrix4x4[] Array = array;
  public readonly int Count = count;
}
