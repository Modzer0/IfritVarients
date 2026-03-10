// Decompiled with JetBrains decompiler
// Type: ControlInputs
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

#nullable disable
public class ControlInputs
{
  public float pitch;
  public float roll;
  public float yaw;
  public float throttle;
  public float brake;
  public float customAxis1;

  public override string ToString()
  {
    return $"Inputs({this.pitch:0.00},{this.roll:0.00},{this.yaw:0.00},{this.throttle:0.00},{this.brake:0.00},{this.customAxis1:0.00})";
  }
}
