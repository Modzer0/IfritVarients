// Decompiled with JetBrains decompiler
// Type: CursorFlags
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;

#nullable disable
[Flags]
public enum CursorFlags
{
  None = 0,
  Pause = 1,
  Map = 2,
  SelectionMenu = 4,
  Dialogue = 8,
  NotInGame = 16, // 0x00000010
  Chat = 32, // 0x00000020
  EditorWindow = 64, // 0x00000040
  Loading = 128, // 0x00000080
  EmptyScene = 256, // 0x00000100
}
