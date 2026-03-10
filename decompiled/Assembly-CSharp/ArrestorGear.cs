// Decompiled with JetBrains decompiler
// Type: ArrestorGear
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

#nullable disable
public class ArrestorGear : MonoBehaviour
{
  public int wireNumber;
  [SerializeField]
  private Transform leftSide;
  [SerializeField]
  private Transform rightSide;
  [SerializeField]
  private Transform cableCenter;
  [SerializeField]
  private Transform restCenter;
  [SerializeField]
  private Transform cableLeftSide;
  [SerializeField]
  private Transform cableRightSide;
  [SerializeField]
  private float damping;
  [SerializeField]
  private float spring;
  [SerializeField]
  private float maxExtension;
  private float relaxedLength;
  private float extensionPrev;
  private float adjustedDamping;
  private float adjustedSpring;
  private TailHook attachedHook;
  private float hookTime;

  private void Awake()
  {
    this.relaxedLength = Vector3.Distance(this.leftSide.position, this.rightSide.position);
    this.restCenter.position = (this.leftSide.position + this.rightSide.position) * 0.5f;
    this.cableCenter.position = this.restCenter.position;
    this.cableLeftSide.transform.LookAt(this.cableCenter.position);
    this.cableRightSide.transform.LookAt(this.cableCenter.position);
    this.cableLeftSide.localScale = new Vector3(1f, 1f, this.relaxedLength * 0.5f);
    this.cableRightSide.localScale = new Vector3(1f, 1f, this.relaxedLength * 0.5f);
    this.extensionPrev = 0.0f;
  }

  public bool Hook(TailHook tailHook)
  {
    if ((Object) this.attachedHook != (Object) null || (double) Mathf.Abs(Vector3.Dot(this.transform.forward, tailHook.unitPart.rb.velocity.normalized)) < 0.5)
      return false;
    float mass = tailHook.unitPart.parentUnit.GetMass();
    this.adjustedDamping = mass / 20000f * this.damping;
    this.adjustedSpring = mass / 20000f * this.spring;
    this.attachedHook = tailHook;
    this.enabled = true;
    return true;
  }

  public void Unhook()
  {
    this.hookTime = 0.0f;
    this.attachedHook.Unhook();
    this.attachedHook = (TailHook) null;
  }

  private void FixedUpdate()
  {
    if ((Object) this.attachedHook != (Object) null)
    {
      this.cableCenter.transform.position = this.attachedHook.hookEnd.position;
      float num1 = Vector3.Distance(this.attachedHook.hookEnd.position, this.leftSide.position) + Vector3.Distance(this.attachedHook.hookEnd.position, this.rightSide.position) - this.relaxedLength;
      float num2 = (num1 - this.extensionPrev) / Time.fixedDeltaTime;
      float num3 = (double) num2 > 0.0 ? num2 * this.adjustedDamping : 0.0f;
      this.attachedHook.ApplyForce((this.leftSide.position - this.attachedHook.hookEnd.position + (this.rightSide.position - this.attachedHook.hookEnd.position)).normalized * (num1 * this.adjustedSpring * Mathf.Clamp01(num2 * 0.1f) * Mathf.Clamp01(num1 * 0.03f) + num3 * Mathf.Clamp01(num1 * 0.05f)));
      this.extensionPrev = num1;
      this.hookTime += Time.fixedDeltaTime;
      if ((double) num2 < 1.0 && (double) this.hookTime > 1.0 || (double) num1 > (double) this.maxExtension || (double) this.hookTime > 5.0)
        this.Unhook();
    }
    else
    {
      this.cableCenter.position += Vector3.ClampMagnitude(this.restCenter.position - this.cableCenter.position, 4f * Time.deltaTime);
      if ((double) Vector3.Distance(this.cableCenter.position, this.restCenter.position) < 0.0099999997764825821)
      {
        this.cableCenter.position = this.restCenter.position;
        this.enabled = false;
      }
    }
    float z1 = Vector3.Distance(this.cableCenter.position, this.leftSide.position);
    float z2 = Vector3.Distance(this.cableCenter.position, this.rightSide.position);
    this.cableLeftSide.LookAt(this.cableCenter.transform.position);
    this.cableRightSide.LookAt(this.cableCenter.transform.position);
    this.cableLeftSide.localScale = new Vector3(1f, 1f, z1);
    this.cableRightSide.localScale = new Vector3(1f, 1f, z2);
  }
}
