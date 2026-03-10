// Decompiled with JetBrains decompiler
// Type: MoveablePartAnimation
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;

#nullable disable
[CreateAssetMenu(fileName = "MoveablePartAnimation", menuName = "ScriptableObjects/MoveablePartAnimation", order = 9)]
[Serializable]
public class MoveablePartAnimation : ScriptableObject
{
  [SerializeField]
  private Vector3 rotationVector = Vector3.zero;
  [SerializeField]
  private float rotationSpeed;
  [SerializeField]
  private float rotationMax;
  private float rotationVariableSpeed;
  [SerializeField]
  private float rotationRandomSpeed;
  [SerializeField]
  private float windRotationEffect;
  [SerializeField]
  private bool windHeadingDependent;
  [SerializeField]
  private bool windRotationSpeedDependent;
  [SerializeField]
  private Vector3 translationVector = Vector3.zero;
  [SerializeField]
  private float translationSpeed;
  [SerializeField]
  private float translationMax;

  public virtual void Initialize(Transform transform)
  {
    this.rotationVariableSpeed = this.rotationSpeed + UnityEngine.Random.Range(-this.rotationRandomSpeed, this.rotationRandomSpeed);
    transform.localEulerAngles = this.rotationVector * UnityEngine.Random.Range(-this.rotationMax, this.rotationMax);
  }

  public virtual void Animate(Transform transform)
  {
    if (!((UnityEngine.Object) transform != (UnityEngine.Object) null) || !((UnityEngine.Object) NetworkSceneSingleton<LevelInfo>.i != (UnityEngine.Object) null))
      return;
    if (this.windHeadingDependent)
    {
      float target = 180f + NetworkSceneSingleton<LevelInfo>.i.GetWindHeading();
      if ((double) target > 360.0)
        target -= 360f;
      float num = Mathf.Clamp(Mathf.MoveTowardsAngle(transform.localEulerAngles.y, target, this.windRotationEffect * NetworkSceneSingleton<LevelInfo>.i.windVelocity.magnitude * Time.fixedDeltaTime), -this.rotationMax, this.rotationMax);
      transform.localEulerAngles = this.rotationVector * num;
    }
    else if (this.windRotationSpeedDependent)
    {
      this.rotationVariableSpeed = Mathf.Lerp(this.rotationVariableSpeed, this.windRotationEffect * NetworkSceneSingleton<LevelInfo>.i.windSpeed, 0.5f);
      this.rotationVariableSpeed = Mathf.Clamp(this.rotationVariableSpeed, 0.0f, this.rotationMax);
      this.rotationVariableSpeed = Mathf.Max(0.0f, this.rotationVariableSpeed);
      transform.localEulerAngles += this.rotationVector * this.rotationVariableSpeed * Time.deltaTime;
    }
    else
      transform.localEulerAngles += this.rotationVector * this.rotationVariableSpeed * Time.deltaTime;
  }
}
