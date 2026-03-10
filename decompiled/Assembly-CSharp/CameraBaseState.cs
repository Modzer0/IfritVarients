// Decompiled with JetBrains decompiler
// Type: CameraBaseState
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

#nullable disable
public abstract class CameraBaseState
{
  public abstract void EnterState(CameraStateManager cam);

  public abstract void LeaveState(CameraStateManager cam);

  public abstract void UpdateState(CameraStateManager cam);

  public abstract void FixedUpdateState(CameraStateManager cam);
}
