// Decompiled with JetBrains decompiler
// Type: UnitDefinition
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;

#nullable disable
[CreateAssetMenu(fileName = "New Unit Definition", menuName = "ScriptableObjects/UnitDefinition", order = 6)]
public class UnitDefinition : ScriptableObject, INetworkDefinition, IHasJsonKey
{
  public TypeIdentity typeIdentity;
  public RoleIdentity roleIdentity;
  [Header("Important: dont change jsonKey after it is set")]
  [Tooltip("name used it look up to find unit, saved in mission json")]
  public string jsonKey;
  public string unitName;
  public string bogeyName;
  public string description;
  public string code;
  public float visibleRange;
  public float iconRange;
  public float radarSize;
  public Sprite friendlyIcon;
  public Sprite hostileIcon;
  public Sprite mapIcon;
  public bool mapOrient;
  public bool IsObstacle = true;
  public float iconSize;
  public float mapIconSize = 1f;
  public int captureCapacity;
  public float captureStrength;
  public float captureDefense;
  public float length;
  public float width;
  public float height;
  public float value;
  public float mass;
  public float manpower;
  public float armorTier;
  public float damageTolerance;
  public bool CanSlingLoad;
  public GameObject unitPrefab;
  public Vector3 spawnOffset;
  public bool disabled;
  public bool dontAutomaticallyAddToEncyclopedia;
  [Tooltip("Min Height above the ground when moving the unit in editor (can be negative)")]
  public float minEditorHeight;
  [Tooltip("Max Height above the ground when moving the unit in editor")]
  public float maxEditorHeight = 1000f;

  [field: NonSerialized]
  int? INetworkDefinition.LookupIndex { get; set; }

  string IHasJsonKey.JsonKey
  {
    get => this.jsonKey;
    set
    {
      if (!Application.isEditor)
        throw new Exception("JsonKey should only be set in UnityEditor");
      this.jsonKey = value;
    }
  }

  public float GetOpportunity(RoleIdentity role) => this.typeIdentity.ThreatPosedBy(role);

  public void CacheMass() => this.mass = this.unitPrefab.GetComponent<Unit>().GetPrefabMass();

  public float ThreatPosedBy(RoleIdentity role) => this.typeIdentity.ThreatPosedBy(role);
}
