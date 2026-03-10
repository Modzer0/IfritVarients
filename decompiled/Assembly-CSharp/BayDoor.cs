// Decompiled with JetBrains decompiler
// Type: BayDoor
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

#nullable disable
public class BayDoor : MonoBehaviour
{
  [SerializeField]
  protected float hingeAngle;
  [SerializeField]
  protected float openSpeed;
  [SerializeField]
  protected float closeSpeed;
  [SerializeField]
  protected AudioSource doorAudioSource;
  [SerializeField]
  protected AudioClip openStartSound;
  [SerializeField]
  protected AudioClip closeStartSound;
  private Vector3 baseAngle;
  protected float openTimer;
  protected float openAmount;
  protected float openAmountPrev;

  protected virtual void Awake()
  {
    this.baseAngle = this.transform.localEulerAngles;
    this.enabled = false;
  }

  public virtual void OpenDoor(float duration)
  {
    this.openTimer = duration;
    this.enabled = true;
  }

  protected virtual void Update()
  {
    this.openTimer -= Time.deltaTime;
    if ((double) this.openTimer > 0.0)
    {
      this.openAmount += this.openSpeed * Time.deltaTime;
      if ((Object) this.doorAudioSource != (Object) null && (Object) this.doorAudioSource.clip != (Object) this.openStartSound)
      {
        this.doorAudioSource.Stop();
        this.doorAudioSource.clip = this.openStartSound;
        this.doorAudioSource.Play();
      }
    }
    else
    {
      this.openAmount += -this.closeSpeed * Time.deltaTime;
      if ((Object) this.doorAudioSource != (Object) null && (Object) this.doorAudioSource.clip != (Object) this.closeStartSound)
      {
        this.doorAudioSource.Stop();
        this.doorAudioSource.clip = this.closeStartSound;
        this.doorAudioSource.Play();
      }
    }
    this.openAmount = Mathf.Clamp01(this.openAmount);
    if ((double) this.openAmount == (double) this.openAmountPrev)
      return;
    this.transform.localEulerAngles = new Vector3(this.baseAngle.x, this.baseAngle.y, this.openAmount * this.hingeAngle);
    this.openAmountPrev = this.openAmount;
    if ((double) this.openAmount > 0.0)
      return;
    this.enabled = false;
  }
}
