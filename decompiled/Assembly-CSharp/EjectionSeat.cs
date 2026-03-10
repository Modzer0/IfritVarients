// Decompiled with JetBrains decompiler
// Type: EjectionSeat
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

#nullable disable
public class EjectionSeat : MonoBehaviour
{
  private const float RAIL_TIME = 1.5f;
  [SerializeField]
  private Transform nozzle;
  [SerializeField]
  private float mass;
  [SerializeField]
  private float thrust;
  [SerializeField]
  private float duration;
  private Rigidbody cockpitRB;
  private Rigidbody pilotRB;
  private Rigidbody detachedRB;
  private UnitPart cockpitPart;
  [SerializeField]
  private ParticleSystem[] ejectParticles;
  [SerializeField]
  private Collider seatCollider;
  [SerializeField]
  private AudioClip fireSound;
  private PilotDismounted pilotDismounted;
  private Vector3 cockpitOffset;
  private float railPosition;
  private bool firing;

  public bool IsOnEjectionRail => this.firing && (double) this.railPosition < 1.5;

  public void LinkToPilot(PilotDismounted pilotDismounted)
  {
    this.pilotDismounted = pilotDismounted;
    this.pilotRB = pilotDismounted.rb;
    this.pilotRB.mass += this.mass;
  }

  public void Fire(UnitPart unitPart)
  {
    this.cockpitPart = unitPart;
    this.cockpitRB = unitPart.rb;
    this.firing = true;
    this.cockpitOffset = unitPart.transform.InverseTransformPoint(this.pilotRB.transform.position);
    this.FireEffects();
    this.FirePhysics().Forget();
  }

  private async UniTask FirePhysics()
  {
    EjectionSeat ejectionSeat = this;
    CancellationToken cancel = ejectionSeat.destroyCancellationToken;
    ejectionSeat.railPosition = 0.0f;
    float ejectTime = 0.0f;
    float speed = 0.0f;
    ejectionSeat.seatCollider.enabled = false;
    YieldAwaitable yieldAwaitable;
    while ((double) ejectionSeat.railPosition < 1.5)
    {
      speed += ejectionSeat.thrust / ejectionSeat.pilotRB.mass * Time.fixedDeltaTime;
      Vector3 position = ejectionSeat.cockpitPart.transform.TransformPoint(ejectionSeat.cockpitOffset) + ejectionSeat.nozzle.transform.up * ejectionSeat.railPosition;
      Quaternion rotation = ejectionSeat.cockpitPart.transform.rotation;
      ejectionSeat.pilotRB.Move(position, rotation);
      ejectionSeat.pilotRB.angularVelocity = ejectionSeat.cockpitRB.angularVelocity;
      ejectionSeat.pilotRB.AddForce(ejectionSeat.nozzle.transform.up * ejectionSeat.thrust);
      ejectionSeat.railPosition += speed * Time.fixedDeltaTime;
      ejectTime += Time.fixedDeltaTime;
      yieldAwaitable = UniTask.Yield(PlayerLoopTiming.FixedUpdate);
      await yieldAwaitable;
      if (cancel.IsCancellationRequested)
      {
        cancel = new CancellationToken();
        return;
      }
    }
    ejectionSeat.seatCollider.enabled = true;
    ejectionSeat.pilotDismounted.SetCollidable(true);
    while ((double) ejectTime < (double) ejectionSeat.duration)
    {
      ejectTime += Time.fixedDeltaTime;
      ejectionSeat.pilotRB.AddTorque(0.3f * ejectionSeat.pilotRB.mass * Vector3.Cross(ejectionSeat.nozzle.transform.up, Vector3.up), ForceMode.Force);
      ejectionSeat.pilotRB.AddForce(Vector3.RotateTowards(ejectionSeat.nozzle.transform.up, Vector3.up, 0.5f, 0.0f) * ejectionSeat.thrust);
      yieldAwaitable = UniTask.Yield(PlayerLoopTiming.FixedUpdate);
      await yieldAwaitable;
      if (cancel.IsCancellationRequested)
      {
        cancel = new CancellationToken();
        return;
      }
    }
    cancel = new CancellationToken();
  }

  private void LateUpdate()
  {
    if (!this.IsOnEjectionRail)
      return;
    Vector3 position = this.cockpitPart.transform.TransformPoint(this.cockpitOffset) + this.nozzle.transform.up * this.railPosition;
    Quaternion rotation = this.cockpitPart.transform.rotation;
    this.pilotRB.transform.SetPositionAndRotation(position, rotation);
    this.pilotRB.Move(position, rotation);
  }

  private void FireEffects()
  {
    AudioSource audioSource = this.gameObject.AddComponent<AudioSource>();
    audioSource.outputAudioMixerGroup = SoundManager.i.EffectsMixer;
    audioSource.bypassListenerEffects = true;
    audioSource.clip = this.fireSound;
    audioSource.volume = 2f;
    audioSource.dopplerLevel = 0.0f;
    audioSource.minDistance = 50f;
    audioSource.maxDistance = 1000f;
    audioSource.spatialBlend = 1f;
    audioSource.rolloffMode = AudioRolloffMode.Linear;
    audioSource.Play();
    Object.Destroy((Object) audioSource, 10f);
    foreach (ParticleSystem ejectParticle in this.ejectParticles)
      ejectParticle.Play();
  }

  public void Detach()
  {
    if ((Object) this.detachedRB != (Object) null || (Object) this.pilotRB == (Object) null)
      return;
    this.seatCollider.enabled = false;
    this.transform.SetParent((Transform) null);
    this.pilotRB.mass -= this.mass;
    this.detachedRB = this.gameObject.AddComponent<Rigidbody>();
    this.detachedRB.mass = this.mass;
    this.detachedRB.drag = 0.05f;
    this.detachedRB.angularDrag = 0.05f;
    this.detachedRB.interpolation = RigidbodyInterpolation.Interpolate;
    this.detachedRB.velocity = this.pilotRB.velocity;
    this.detachedRB.angularVelocity = this.pilotRB.angularVelocity;
    Object.Destroy((Object) this.gameObject, 20f);
  }

  public void BailOut(Rigidbody pilotRB, Rigidbody cockpit)
  {
    pilotRB.mass -= this.mass;
    this.transform.SetParent(cockpit.transform);
  }
}
