// Decompiled with JetBrains decompiler
// Type: NuclearOption.SavedMission.ObjectiveV2.ObjectiveData
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;

#nullable disable
namespace NuclearOption.SavedMission.ObjectiveV2;

[Serializable]
public struct ObjectiveData
{
  public string StringValue;
  public float FloatValue;
  public Vector3 VectorValue;

  public ObjectiveData(string value)
  {
    this.StringValue = value;
    this.FloatValue = 0.0f;
    this.VectorValue = new Vector3();
  }

  public ObjectiveData(float value)
  {
    this.StringValue = (string) null;
    this.FloatValue = value;
    this.VectorValue = new Vector3();
  }

  public ObjectiveData(Vector3 value)
  {
    this.StringValue = (string) null;
    this.FloatValue = 0.0f;
    this.VectorValue = value;
  }

  public ObjectiveData(string stringValue, float floatValue)
  {
    this.StringValue = stringValue;
    this.FloatValue = floatValue;
    this.VectorValue = new Vector3();
  }

  public ObjectiveData(string stringValue, Vector3 vectorValue)
  {
    this.StringValue = stringValue;
    this.FloatValue = 0.0f;
    this.VectorValue = vectorValue;
  }

  public ObjectiveData(float floatValue, Vector3 vectorValue)
  {
    this.StringValue = (string) null;
    this.FloatValue = floatValue;
    this.VectorValue = vectorValue;
  }
}
