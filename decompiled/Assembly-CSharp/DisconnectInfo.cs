// Decompiled with JetBrains decompiler
// Type: DisconnectInfo
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

#nullable disable
public class DisconnectInfo
{
  public string Message;
  public bool ShowReason;

  public DisconnectInfo(string message)
  {
    this.Message = message;
    this.ShowReason = true;
  }

  public static DisconnectInfo NoReason()
  {
    return new DisconnectInfo("") { ShowReason = false };
  }

  public void Merge(DisconnectInfo other) => this.Message = "\n" + other.Message;
}
