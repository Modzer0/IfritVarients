// Decompiled with JetBrains decompiler
// Type: VaporEffect
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

#nullable disable
public class VaporEffect : MonoBehaviour
{
  [SerializeField]
  private Aircraft aircraft;
  [SerializeField]
  private VaporEmitter[] emitters;
  private float alpha;
  private float airspeed;

  private void OnEnable()
  {
    for (int index = 0; index < this.emitters.Length; ++index)
      this.emitters[index].Initialize();
  }

  private void FixedUpdate()
  {
    if (!((Object) this.aircraft.rb != (Object) null) || (double) this.aircraft.displayDetail <= 0.5)
      return;
    this.airspeed = this.aircraft.speed;
    this.alpha = TargetCalc.GetAngleOnAxis(this.transform.forward, this.aircraft.rb.velocity, this.transform.right);
    for (int index = 0; index < this.emitters.Length; ++index)
      this.emitters[index].Emit(this.alpha, this.airspeed, this.aircraft.rb.velocity, this.aircraft.transform.GlobalPosition().y);
  }
}
