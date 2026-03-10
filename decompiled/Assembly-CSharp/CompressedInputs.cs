// Decompiled with JetBrains decompiler
// Type: CompressedInputs
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

#nullable disable
public struct CompressedInputs
{
  public CompressedFloat pitch;
  public CompressedFloat roll;
  public CompressedFloat yaw;
  public CompressedFloat throttle;
  public CompressedFloat brake;
  public CompressedFloat customAxis1;

  public CompressedInputs(ControlInputs inputs)
  {
    this.pitch = inputs.pitch.Compress();
    this.roll = inputs.roll.Compress();
    this.yaw = inputs.yaw.Compress();
    this.throttle = inputs.throttle.Compress();
    this.brake = inputs.brake.Compress();
    this.customAxis1 = inputs.customAxis1.Compress();
  }

  public bool Valid(bool logErrors)
  {
    return (1 & (NetworkFloatHelper.Validate(this.pitch, logErrors, "pitch") ? 1 : 0) & (NetworkFloatHelper.Validate(this.roll, logErrors, "roll") ? 1 : 0) & (NetworkFloatHelper.Validate(this.yaw, logErrors, "yaw") ? 1 : 0) & (NetworkFloatHelper.Validate(this.throttle, logErrors, "throttle") ? 1 : 0) & (NetworkFloatHelper.Validate(this.brake, logErrors, "brake") ? 1 : 0) & (NetworkFloatHelper.Validate(this.customAxis1, logErrors, "customAxis1") ? 1 : 0)) != 0;
  }
}
