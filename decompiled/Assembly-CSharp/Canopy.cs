// Decompiled with JetBrains decompiler
// Type: Canopy
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

#nullable disable
public class Canopy : MonoBehaviour
{
  public Transform ejectionTransform;
  public AeroPart attachedPart;
  public float mass;
  public Collider ejectionCollider;
  [SerializeField]
  private Vector3 ejectionForce;
  [SerializeField]
  private Vector3 forcePosition;
  private Rigidbody rb;
  [SerializeField]
  private Canopy.CanopyHinge[] canopyHinges;
  [SerializeField]
  private AudioClip ejectSound;
  [SerializeField]
  private float fireTime;
  [SerializeField]
  private float openSpeed;
  [SerializeField]
  private float glassDamageThreshold;
  [SerializeField]
  private float glassDamageLimit;
  [SerializeField]
  private Renderer[] glassRenderers;
  private float openAmount;
  private bool firing;
  private bool opening;

  private void Awake()
  {
    this.enabled = false;
    if (!(this.attachedPart.parentUnit is Aircraft parentUnit))
      return;
    parentUnit.onInitialize += new Action(this.Canopy_OnInitialize);
  }

  public void OpenHinges()
  {
    this.enabled = true;
    this.opening = true;
  }

  private void Canopy_OnInitialize()
  {
    if (!GameManager.IsLocalAircraft(this.attachedPart.parentUnit))
      return;
    this.attachedPart.onApplyDamage += new Action<UnitPart.OnApplyDamage>(this.Canopy_OnApplyDamage);
  }

  private void Canopy_OnApplyDamage(UnitPart.OnApplyDamage e)
  {
    float f = 1f - Mathf.Clamp01((float) (((double) e.hitPoints - (double) this.glassDamageLimit) / ((double) this.glassDamageThreshold - (double) this.glassDamageLimit)));
    for (int index = 0; index < this.glassRenderers.Length; ++index)
    {
      if ((UnityEngine.Object) this.glassRenderers[index] != (UnityEngine.Object) null)
        this.glassRenderers[index].material.SetFloat("_Cracked_Amount", Mathf.Sqrt(f));
    }
  }

  private async UniTask EjectionSequence()
  {
    Canopy canopy = this;
    await UniTask.Yield(PlayerLoopTiming.FixedUpdate);
    Rigidbody rb = canopy.attachedPart.rb;
    Vector3 position = rb.transform.InverseTransformPoint(canopy.transform.position);
    if (canopy.firing || (UnityEngine.Object) canopy.ejectionTransform == (UnityEngine.Object) null)
      return;
    canopy.firing = true;
    if ((UnityEngine.Object) canopy.attachedPart.transform == (UnityEngine.Object) canopy.ejectionTransform)
    {
      canopy.attachedPart.BreakAllJoints();
      canopy.attachedPart.CreateRB(rb.velocity, canopy.attachedPart.transform.position);
      canopy.rb = canopy.attachedPart.rb;
      rb.drag = 0.1f;
    }
    else
    {
      canopy.ejectionTransform.SetParent((Transform) null);
      canopy.ejectionCollider.enabled = true;
      canopy.rb = canopy.ejectionTransform.gameObject.AddComponent<Rigidbody>();
      canopy.rb.mass = canopy.mass;
      canopy.rb.velocity = rb.GetPointVelocity(canopy.transform.position);
      canopy.rb.interpolation = RigidbodyInterpolation.Interpolate;
      canopy.rb.MovePosition(rb.transform.TransformPoint(position));
      canopy.transform.position = rb.transform.TransformPoint(position);
    }
    canopy.rb.angularDrag = 0.01f;
    canopy.rb.drag = 0.1f;
    AudioSource audioSource = canopy.ejectionTransform.gameObject.AddComponent<AudioSource>();
    audioSource.outputAudioMixerGroup = SoundManager.i.EffectsMixer;
    audioSource.bypassListenerEffects = true;
    audioSource.clip = canopy.ejectSound;
    audioSource.volume = 2f;
    audioSource.dopplerLevel = 0.0f;
    audioSource.minDistance = 50f;
    audioSource.maxDistance = 1000f;
    audioSource.spatialBlend = 1f;
    audioSource.rolloffMode = AudioRolloffMode.Linear;
    audioSource.Play();
    if ((UnityEngine.Object) canopy.ejectionTransform.GetComponent<UnitPart>() == (UnityEngine.Object) null)
      NetworkSceneSingleton<Spawner>.i.DestroyLocal(canopy.ejectionTransform.gameObject, 10f);
    canopy.enabled = true;
  }

  public void Eject() => this.EjectionSequence().Forget();

  private void FixedUpdate()
  {
    if (!this.firing)
      return;
    if ((double) this.fireTime <= 0.0)
      this.enabled = false;
    this.rb.AddForceAtPosition(this.rb.transform.forward * this.ejectionForce.z + this.rb.transform.up * this.ejectionForce.y, this.rb.transform.position + this.forcePosition);
    this.fireTime -= Time.deltaTime;
  }

  private void Update()
  {
    if (!this.opening)
      return;
    if ((double) this.openAmount >= 1.0)
      this.enabled = false;
    this.openAmount += this.openSpeed * Time.deltaTime;
    for (int index = 0; index < this.canopyHinges.Length; ++index)
      this.canopyHinges[index].Animate(this.openAmount);
  }

  [Serializable]
  private class CanopyHinge
  {
    [SerializeField]
    private Transform transform;
    [SerializeField]
    private float hingeAngle;

    public void Animate(float openAmount)
    {
      this.transform.localEulerAngles = Vector3.right * this.hingeAngle * openAmount;
    }
  }
}
