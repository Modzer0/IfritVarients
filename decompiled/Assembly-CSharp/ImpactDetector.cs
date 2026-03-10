// Decompiled with JetBrains decompiler
// Type: ImpactDetector
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.Networking;
using UnityEngine;

#nullable disable
public class ImpactDetector : MonoBehaviour
{
  [SerializeField]
  private float gLimit;
  private Rigidbody rb;
  private IDamageable damageable;
  private Vector3 velocityPrev;
  private Vector3 accel;
  private float timeSinceSpawn;

  private void Awake()
  {
    this.damageable = this.gameObject.GetComponent<IDamageable>();
    if (!NetworkManagerNuclearOption.i.Server.Active || this.damageable == null)
      Object.Destroy((Object) this);
    this.rb = this.damageable.GetUnit().rb;
    this.gameObject.GetComponent<Unit>().CheckRadarAlt();
    if ((double) this.gameObject.GetComponent<Unit>().radarAlt >= 1.0)
      return;
    Object.Destroy((Object) this);
  }

  private void FixedUpdate()
  {
    this.timeSinceSpawn += Time.fixedDeltaTime;
    if (this.velocityPrev != Vector3.zero)
      this.accel = (this.rb.velocity - this.velocityPrev) / Time.fixedDeltaTime;
    this.velocityPrev = this.rb.velocity;
    if ((double) this.accel.sqrMagnitude > (double) this.gLimit * (double) this.gLimit * 82.80999755859375)
    {
      this.damageable.TakeDamage(0.0f, 0.0f, 1f, 0.0f, this.accel.magnitude, PersistentID.None);
      Object.Destroy((Object) this);
    }
    if ((double) this.timeSinceSpawn <= 5.0 || (double) Mathf.Abs(this.rb.velocity.y) >= 5.0 || (double) this.rb.velocity.sqrMagnitude >= 10.0)
      return;
    Object.Destroy((Object) this);
  }

  public void SetGLimit(float value) => this.gLimit = value;
}
