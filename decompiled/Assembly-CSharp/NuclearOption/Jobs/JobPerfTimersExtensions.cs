// Decompiled with JetBrains decompiler
// Type: NuclearOption.Jobs.JobPerfTimersExtensions
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

#nullable disable
namespace NuclearOption.Jobs;

public static class JobPerfTimersExtensions
{
  public static unsafe Ptr<long> ControlAccessPtr(this Ptr<JobSharedFields> timer)
  {
    return (Ptr<long>) &timer.ptr->controlAccess;
  }

  public static unsafe Ptr<long> AeroAccessPtr(this Ptr<JobSharedFields> timer)
  {
    return (Ptr<long>) &timer.ptr->aeroAccess;
  }

  public static unsafe Ptr<long> VehicleAccessPtr(this Ptr<JobSharedFields> timer)
  {
    return (Ptr<long>) &timer.ptr->vehicleAccess;
  }

  public static unsafe Ptr<long> WaterAccessPtr(this Ptr<JobSharedFields> timer)
  {
    return (Ptr<long>) &timer.ptr->waterAccess;
  }
}
