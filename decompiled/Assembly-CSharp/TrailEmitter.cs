// Decompiled with JetBrains decompiler
// Type: TrailEmitter
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

#nullable disable
public class TrailEmitter : MonoBehaviour
{
  [SerializeField]
  private ParticleSystem trailSystem;
  [SerializeField]
  private float emitFrequency;
  [SerializeField]
  private float opacityVariation = 1f;
  [SerializeField]
  private float scaleVariation = 0.5f;
  [SerializeField]
  private float segmentLength = 30f;
  public Rigidbody rb;
  [SerializeField]
  private Transform emitTransform;
  private ParticleSystem.EmitParams emitParams;
  private float emitCounter;
  private float baseSize;
  public float opacity = 1f;
  [SerializeField]
  private float emitLifetime = 10f;
  private ParticleSystem.MainModule main;

  private void OnEnable()
  {
    this.main = this.trailSystem.main;
    this.baseSize = this.main.startSizeMultiplier;
    this.main.simulationSpace = ParticleSystemSimulationSpace.Custom;
    this.main.customSimulationSpace = Datum.origin;
  }

  public void StopTrail() => Object.Destroy((Object) this);

  private void FixedUpdate()
  {
    if (!((Object) this.rb != (Object) null))
      return;
    this.emitCounter += Time.deltaTime * this.rb.velocity.magnitude / this.segmentLength;
    this.emitLifetime -= Time.deltaTime;
    if ((double) this.emitCounter > 1.0)
    {
      this.main.startColor = (ParticleSystem.MinMaxGradient) new Color(1f, 1f, 1f, this.opacity * (float) (1.0 - (double) this.opacityVariation * (double) Random.value));
      this.main.startSizeMultiplier = this.baseSize * (float) (1.0 - (double) Random.value * (double) this.scaleVariation);
      this.emitParams.position = this.emitTransform.position.ToGlobalPosition().AsVector3();
      this.emitParams.velocity = this.rb.velocity + this.main.startSpeed.constant * new Vector3((float) Random.Range(-1, 1), (float) Random.Range(-1, 1), (float) Random.Range(-1, 1));
      this.trailSystem.Emit(this.emitParams, 1);
      this.emitCounter = 0.0f;
    }
    if ((double) this.emitLifetime > 0.0)
      return;
    Object.Destroy((Object) this);
  }
}
