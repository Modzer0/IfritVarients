// Decompiled with JetBrains decompiler
// Type: AoABracket
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using UnityEngine;
using UnityEngine.UI;

#nullable disable
public class AoABracket : HUDApp
{
  [SerializeField]
  private Image eBracket;
  [SerializeField]
  private float targetAoA;
  [SerializeField]
  private float AoARange;
  private ControlInputs inputs;
  private Aircraft aircraft;

  private void Awake()
  {
  }

  public override void Initialize(Aircraft aircraft)
  {
    this.aircraft = aircraft;
    this.inputs = aircraft.GetInputs();
  }

  private void OnDestroy()
  {
  }

  public override void Refresh()
  {
    if ((Object) this.aircraft == (Object) null)
      return;
    if (this.aircraft.gearState == LandingGear.GearState.LockedRetracted || (double) this.aircraft.radarAlt < 1.0 || (double) this.inputs.throttle > 0.699999988079071)
    {
      if (!this.eBracket.enabled)
        return;
      this.eBracket.enabled = false;
    }
    else
    {
      if (!this.eBracket.enabled)
        this.eBracket.enabled = true;
      float num = 1080f / SceneSingleton<CameraStateManager>.i.mainCamera.fieldOfView;
      this.eBracket.transform.localPosition = new Vector3(0.0f, -this.targetAoA * num, 0.0f);
      this.eBracket.transform.localScale = Vector3.one * this.AoARange * num;
    }
  }
}
