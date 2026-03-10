// Decompiled with JetBrains decompiler
// Type: PropStrikeDetector
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;

#nullable disable
public class PropStrikeDetector : MonoBehaviour
{
  [SerializeField]
  private float currentRadius;
  [SerializeField]
  private Collider propCollider;

  public event Action<float> OnStrike;

  private void OnTriggerStay(Collider other)
  {
    Transform transform = other.transform;
    float distance;
    if (!Physics.ComputePenetration(this.propCollider, this.transform.position, this.transform.rotation, other, transform.position, transform.rotation, out Vector3 _, out distance))
      return;
    Action<float> onStrike = this.OnStrike;
    if (onStrike != null)
      onStrike(distance);
    if ((double) distance >= (double) this.currentRadius)
      return;
    this.currentRadius = distance;
    this.transform.localScale = Vector3.one * distance / this.currentRadius;
  }
}
