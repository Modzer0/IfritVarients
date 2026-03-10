// Decompiled with JetBrains decompiler
// Type: ScatterSaveLoad
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Mirage.Serialization;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
public static class ScatterSaveLoad
{
  public static void Load(TextAsset binaryData, ScatterType[] scatterTypes)
  {
    NetworkReader reader = new NetworkReader();
    reader.Reset(binaryData.bytes);
    reader.ReadInt32();
    int num1 = 13;
    foreach (ScatterType scatterType1 in scatterTypes)
    {
      ScatterType scatterType2 = scatterType1;
      if (scatterType2.sectors == null)
        scatterType2.sectors = new Dictionary<int, List<ScatterPoint>>();
      scatterType1.sectors.Clear();
      int num2 = reader.ReadInt32();
      num1 = num1 * 5 + num2;
      for (int index1 = 0; index1 < num2; ++index1)
      {
        int key = reader.ReadInt32();
        int num3 = num1 * 5 + key;
        int capacity = reader.ReadInt32();
        List<ScatterPoint> scatterPointList = new List<ScatterPoint>(capacity);
        num1 = num3 * 5 + capacity;
        scatterType1.sectors[key] = scatterPointList;
        for (int index2 = 0; index2 < capacity; ++index2)
        {
          Vector3 globalPos = reader.ReadVector3();
          Quaternion rotation = reader.ReadQuaternion();
          float scale = reader.ReadSingle();
          scatterPointList.Add(new ScatterPoint(globalPos, rotation, scale));
          num1 = ((num1 * 5 + scatterPointList[index2].globalPos.GetHashCode()) * 5 + scatterPointList[index2].rotation.GetHashCode()) * 5 + scatterPointList[index2].scale.GetHashCode();
        }
      }
    }
  }
}
