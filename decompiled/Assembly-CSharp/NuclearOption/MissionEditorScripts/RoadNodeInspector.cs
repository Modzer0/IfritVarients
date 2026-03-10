// Decompiled with JetBrains decompiler
// Type: NuclearOption.MissionEditorScripts.RoadNodeInspector
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using RoadPathfinding;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#nullable disable
namespace NuclearOption.MissionEditorScripts;

public class RoadNodeInspector : MonoBehaviour
{
  [SerializeField]
  private Text nodeName;
  [SerializeField]
  private Text NeighborsList;
  [SerializeField]
  private GameObject neighborHighlightEffect;

  public void DisplayNodeInfo(RoadPathfinding.Node node)
  {
    this.nodeName.text = "Path Node " + node.id.ToString();
    this.NeighborsList.text = "Neighbors: ";
    foreach (KeyValuePair<Road, RoadPathfinding.Node> keyValuePair in node.connectionsLookup)
    {
      Text neighborsList = this.NeighborsList;
      neighborsList.text = $"{neighborsList.text} \n Path Node {keyValuePair.Value.id.ToString()}";
      GameObject gameObject = Object.Instantiate<GameObject>(this.neighborHighlightEffect, Datum.origin);
      gameObject.transform.localPosition = keyValuePair.Value.position.AsVector3();
      Object.Destroy((Object) gameObject, 10f);
    }
  }
}
