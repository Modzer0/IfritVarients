// Decompiled with JetBrains decompiler
// Type: CompressedFloat
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System.Runtime.InteropServices;

#nullable disable
[StructLayout(LayoutKind.Explicit, Size = 2)]
public struct CompressedFloat
{
  public const float FLOAT_TO_HALF_MIN = -65504f;
  public const float FLOAT_TO_HALF_MAX = 65504f;
  [FieldOffset(0)]
  public ushort Value;
}
