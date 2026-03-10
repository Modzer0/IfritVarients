// Decompiled with JetBrains decompiler
// Type: GameStateExtension
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

#nullable disable
public static class GameStateExtension
{
  public static bool IsSingleOrMultiplayer(this GameState state)
  {
    return state == GameState.SinglePlayer || state == GameState.Multiplayer;
  }
}
