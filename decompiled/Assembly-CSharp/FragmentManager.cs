// Decompiled with JetBrains decompiler
// Type: FragmentManager
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
public class FragmentManager : MonoBehaviour
{
  [SerializeField]
  private float ejectionSpeed;
  [SerializeField]
  private Transform mainDebris;
  [SerializeField]
  private Transform castPoint;
  [SerializeField]
  private Transform mainCollapser;
  [SerializeField]
  private List<FragmentManager.Fragment> fragments;
  [SerializeField]
  private List<FragmentManager.CollapseDust> collapseDust = new List<FragmentManager.CollapseDust>();
  [SerializeField]
  private float mainDebrisAppearanceTime;
  [SerializeField]
  private float mainDebrisVerticalOffset;
  [SerializeField]
  private float mainDebrisRiseAmount;
  [SerializeField]
  private float toppleSpeed;
  private float time;
  private GlobalPosition mainDebrisTarget;
  private GlobalPosition mainDebrisOrigin;
  private Vector3 collapserVelocity;
  private Vector3 toppleAxis;
  [Tooltip("in seconds")]
  [SerializeField]
  private float cullDelay = 10f;

  private void OnEnable()
  {
    for (int index = 0; index < this.fragments.Count; ++index)
      this.fragments[index].rb.transform.SetParent((Transform) null);
    this.toppleAxis = UnityEngine.Random.insideUnitSphere;
    this.toppleAxis.y = 0.0f;
    this.toppleAxis = this.toppleAxis.normalized;
  }

  private void Start()
  {
    for (int index = 0; index < this.fragments.Count; ++index)
    {
      Vector3 force = UnityEngine.Random.insideUnitSphere * this.ejectionSpeed;
      this.fragments[index].rb.AddForce(force, ForceMode.VelocityChange);
      this.fragments[index].rb.AddTorque(force * 0.1f, ForceMode.VelocityChange);
      this.fragments[index].gameObject = this.fragments[index].rb.gameObject;
      DebrisManager.RegisterDebris(this.fragments[index].gameObject);
    }
    if ((UnityEngine.Object) this.mainDebris != (UnityEngine.Object) null)
    {
      if ((UnityEngine.Object) this.castPoint != (UnityEngine.Object) null)
      {
        RaycastHit hitInfo;
        if (Physics.Linecast(this.castPoint.position, this.castPoint.position - Vector3.up * 1000f, out hitInfo, 64 /*0x40*/))
        {
          this.mainDebrisTarget = hitInfo.point.ToGlobalPosition() + Vector3.up * this.mainDebrisVerticalOffset;
          this.mainDebris.rotation = Quaternion.LookRotation(this.transform.forward, hitInfo.normal);
          this.mainDebrisOrigin = this.mainDebrisTarget - this.mainDebris.up * this.mainDebrisRiseAmount;
          this.mainDebris.position = this.mainDebrisOrigin.ToLocalPosition();
        }
      }
      else
      {
        this.mainDebrisTarget = this.mainDebris.position.ToGlobalPosition();
        this.mainDebrisOrigin = this.mainDebrisTarget - Vector3.up * this.mainDebrisRiseAmount;
        this.mainDebris.position = this.mainDebrisOrigin.ToLocalPosition();
      }
    }
    this.StartSlowUpdateDelayed(this.cullDelay, 1f, new Action(this.FragmentCheck));
  }

  private void OnDestroy()
  {
    if (!((UnityEngine.Object) this.castPoint != (UnityEngine.Object) null))
      return;
    UnityEngine.Object.Destroy((UnityEngine.Object) this.castPoint.gameObject);
  }

  private void Update()
  {
    if ((double) this.time > (double) this.mainDebrisAppearanceTime && this.collapseDust.Count == 0)
      this.enabled = false;
    this.time += Time.deltaTime;
    if ((UnityEngine.Object) this.mainDebris != (UnityEngine.Object) null && (double) this.time < (double) this.mainDebrisAppearanceTime)
      this.mainDebris.transform.position = Vector3.Lerp(this.mainDebrisOrigin.ToLocalPosition(), this.mainDebrisTarget.ToLocalPosition(), this.time / this.mainDebrisAppearanceTime);
    if ((UnityEngine.Object) this.mainCollapser != (UnityEngine.Object) null)
    {
      this.collapserVelocity -= Vector3.up * 9f * Time.deltaTime;
      this.mainCollapser.transform.position += this.collapserVelocity * Time.deltaTime;
      this.mainCollapser.transform.Rotate(this.toppleAxis * this.toppleSpeed * Time.deltaTime, Space.World);
    }
    for (int index = this.collapseDust.Count - 1; index >= 0; --index)
    {
      if ((UnityEngine.Object) this.collapseDust[index].emitTransform == (UnityEngine.Object) null)
        this.collapseDust.RemoveAt(index);
      else if (!this.collapseDust[index].Emit(this.time))
      {
        if ((UnityEngine.Object) this.collapseDust[index].emitTransform.gameObject != (UnityEngine.Object) this.gameObject)
          UnityEngine.Object.Destroy((UnityEngine.Object) this.collapseDust[index].emitTransform.gameObject);
        this.collapseDust.RemoveAt(index);
      }
    }
  }

  public List<GameObject> GetWreckageObjects()
  {
    List<GameObject> wreckageObjects = new List<GameObject>();
    for (int index = 0; index < this.fragments.Count; ++index)
      wreckageObjects.Add(this.fragments[index].rb.gameObject);
    return wreckageObjects;
  }

  public void ApplyExplosionForce(Building.RecentExplosion explosion)
  {
    float num1 = Mathf.Pow(explosion.yield, 0.3333f);
    foreach (FragmentManager.Fragment fragment in this.fragments)
    {
      Vector3 vector3 = fragment.rb.transform.position.ToGlobalPosition() - explosion.globalPosition;
      float num2 = Mathf.Max(vector3.magnitude / num1, 1f);
      float a = (float) (25000.0 / ((double) num2 * (double) num2 * (double) num2));
      Vector3 extents = fragment.rb.GetComponentInChildren<Collider>().bounds.extents;
      float num3 = Mathf.Min((float) ((double) Mathf.Pow((float) (((double) extents.x + (double) extents.y + (double) extents.z) / 3.0), 2f) * (double) Mathf.Min(a, num1 * 250f) * 25.0), explosion.yield * 200f, 60f * fragment.rb.mass);
      fragment.rb.AddForce(vector3.normalized * num3, ForceMode.Impulse);
    }
  }

  private void FragmentCheck()
  {
    for (int index = this.fragments.Count - 1; index >= 0; --index)
    {
      FragmentManager.Fragment fragment = this.fragments[index];
      if ((UnityEngine.Object) fragment.rb == (UnityEngine.Object) null)
        this.fragments.RemoveAt(index);
      else if ((double) fragment.rb.velocity.sqrMagnitude < 1.0 || (double) fragment.rb.position.y < (double) Datum.LocalSeaY - 100.0)
      {
        UnityEngine.Object.Destroy((UnityEngine.Object) fragment.rb);
        this.fragments.RemoveAt(index);
      }
    }
    if (this.fragments.Count != 0 || this.collapseDust.Count != 0)
      return;
    UnityEngine.Object.Destroy((UnityEngine.Object) this);
  }

  [Serializable]
  private class Fragment
  {
    public Rigidbody rb;
    [NonSerialized]
    public GameObject gameObject;
  }

  [Serializable]
  private class CollapseDust
  {
    public Transform emitTransform;
    [SerializeField]
    private string effectType;
    [SerializeField]
    private Rigidbody rb;
    [SerializeField]
    private float duration;
    [SerializeField]
    private float flatRadius;
    [SerializeField]
    private float size;
    [SerializeField]
    private float life;
    [SerializeField]
    private float emitSpeed;
    [SerializeField]
    private float emitInterval;
    private float lastEmitted;

    public bool Emit(float time)
    {
      if ((double) time > (double) this.duration)
        return false;
      if ((double) Time.timeSinceLevelLoad - (double) this.lastEmitted < (double) this.emitInterval)
        return true;
      this.lastEmitted = Time.timeSinceLevelLoad;
      Vector3 position;
      Quaternion rotation;
      this.emitTransform.GetPositionAndRotation(out position, out rotation);
      Vector3 vector3_1 = rotation * Vector3.right;
      Vector3 vector3_2 = rotation * Vector3.forward;
      Vector3 vector3_3 = (UnityEngine.Object) this.rb != (UnityEngine.Object) null ? this.rb.GetPointVelocity(position) : Vector3.zero;
      Vector3 vector3_4 = this.emitSpeed * UnityEngine.Random.insideUnitSphere;
      vector3_4.y = Mathf.Max(vector3_4.y, 0.0f);
      Vector2 vector2 = UnityEngine.Random.insideUnitCircle * this.flatRadius;
      SceneSingleton<ParticleEffectManager>.i.EmitParticles(this.effectType, 1, (position + vector3_1 * vector2.x + vector3_2 * vector2.y).ToGlobalPosition(), vector3_3 + vector3_4, 0.0f, this.life, 0.3f, this.size, 0.3f, 0.3f, 1f, 0.2f);
      return true;
    }
  }
}
