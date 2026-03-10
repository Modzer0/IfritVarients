// Decompiled with JetBrains decompiler
// Type: ScatterType
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
[Serializable]
public class ScatterType
{
  public Mesh mesh;
  public Material material;
  public float maxDensity;
  public Texture2D densityMap;
  public float drawDistance;
  public bool orientToTerrain;
  public float angleVariation;
  public float heightVariation;
  public float minScale;
  public float maxScale;
  public float sampleRadius;
  public GameObject collider;
  public int maxColliders;
  public int samplePoints = 1;
  public bool destructible;
  [NonSerialized]
  public Dictionary<int, List<ScatterPoint>> sectors = new Dictionary<int, List<ScatterPoint>>();
  [NonSerialized]
  public List<RenderBatch> renderBatches = new List<RenderBatch>();
}
