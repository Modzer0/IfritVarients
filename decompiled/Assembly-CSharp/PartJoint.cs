// Decompiled with JetBrains decompiler
// Type: PartJoint
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;

#nullable disable
[Serializable]
public class PartJoint
{
  public UnitPart connectedPart;
  public UnitPart tensor;
  public int solverIterations = 6;
  public float breakForce;
  public float breakTorque;
  public Transform anchor;
  public AudioClip breakSound;
  [HideInInspector]
  public Joint joint;
}
