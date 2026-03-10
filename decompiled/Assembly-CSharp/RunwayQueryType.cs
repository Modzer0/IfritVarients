// Decompiled with JetBrains decompiler
// Type: RunwayQueryType
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;

#nullable disable
[Flags]
public enum RunwayQueryType
{
  Any = 0,
  Landing = 1,
  Takeoff = 2,
  LandingOrTakeoff = Takeoff | Landing, // 0x00000003
  Vertical = 8,
}
