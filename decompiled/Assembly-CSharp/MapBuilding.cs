// Decompiled with JetBrains decompiler
// Type: MapBuilding
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.Jobs;
using System;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
public class MapBuilding : MonoBehaviour, IDamageable
{
  [SerializeField]
  private MapBuilding.MoveablePart[] movingParts;
  [SerializeField]
  private ArmorProperties armorProperties;
  [SerializeField]
  private GameObject destroyedPrefab;
  [SerializeField]
  private GameObject powerlinePrefab;
  [SerializeField]
  private float radius;
  private List<GameObject> dependentObjects = new List<GameObject>();
  private MapBuildingSet buildingSet;
  private int index;
  private float hitPoints = 100f;
  private bool removeFromSet;

  public event Action onBuildingDestroy;

  public ArmorProperties GetArmorProperties() => this.armorProperties;

  public void RegisterDependentObject(GameObject dependentObject)
  {
    this.dependentObjects.Add(dependentObject);
  }

  public void TakeDamage(
    float pierceDamage,
    float blastDamage,
    float amountAffected,
    float fireDamage,
    float impactDamage,
    PersistentID dealerID)
  {
    if (this.buildingSet == null)
    {
      Debug.LogError((object) "MapBuilding.TakeDamage called but buildingSet was not set");
    }
    else
    {
      double num1 = (double) Mathf.Max(pierceDamage - this.armorProperties.pierceArmor, 0.0f) / (double) Mathf.Max(this.armorProperties.pierceTolerance, 0.01f);
      float num2 = Mathf.Max(blastDamage - this.armorProperties.blastArmor, 0.0f) * amountAffected / Mathf.Max(this.armorProperties.blastTolerance, 0.01f);
      float num3 = Mathf.Max(fireDamage - this.armorProperties.fireArmor, 0.0f) / Mathf.Max(this.armorProperties.fireTolerance, 0.01f);
      double num4 = (double) num2;
      this.hitPoints -= (float) (num1 + num4) + num3 + impactDamage;
      if ((double) this.hitPoints >= 0.0)
        return;
      this.buildingSet.DestroyBuilding(this.index);
    }
  }

  public void ApplyDamage(
    float pierceDamage,
    float blastDamage,
    float fireDamage,
    float impactDamage)
  {
  }

  public Unit GetUnit() => (Unit) null;

  public Transform GetTransform() => this.transform;

  public void TakeShockwave(Vector3 origin, float blastEffectScale, float blastPower)
  {
  }

  public void Detach(Vector3 velocity, Vector3 relativePos)
  {
  }

  public float GetMass() => 0.0f;

  public void Destruct()
  {
    if ((UnityEngine.Object) this.destroyedPrefab != (UnityEngine.Object) null && (double) Time.timeSinceLevelLoad > 10.0)
      this.SpawnDestroyedPrefab();
    UnityEngine.Object.Destroy((UnityEngine.Object) this.gameObject);
  }

  private void SpawnDestroyedPrefab()
  {
    GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.destroyedPrefab, this.transform);
    gameObject.transform.SetParent((Transform) null);
    foreach (GameObject dependentObject in this.dependentObjects)
    {
      if ((UnityEngine.Object) dependentObject != (UnityEngine.Object) null)
        UnityEngine.Object.Destroy((UnityEngine.Object) dependentObject);
    }
    GridSquare gridSquare;
    if (!BattlefieldGrid.TryGetGridSquare(this.transform.GlobalPosition(), out gridSquare))
      return;
    Obstacle obstacle1 = new Obstacle(gameObject.transform, this.radius, float.MaxValue);
    int index = gridSquare.obstacles.FindIndex((Predicate<Obstacle>) (obstacle => (UnityEngine.Object) obstacle.Transform == (UnityEngine.Object) this.transform));
    if (index != -1)
      gridSquare.obstacles[index] = obstacle1;
    else
      gridSquare.obstacles.Add(obstacle1);
  }

  public void SpawnPowerline(MapBuilding adjacentBuilding)
  {
    float z = Vector3.Distance(adjacentBuilding.transform.position, this.transform.position);
    if ((double) z > 1000.0 || (double) z < 200.0)
      return;
    GameObject dependentObject = UnityEngine.Object.Instantiate<GameObject>(this.powerlinePrefab, this.transform);
    dependentObject.transform.localPosition = Vector3.zero;
    dependentObject.transform.LookAt(adjacentBuilding.transform);
    dependentObject.transform.localScale = new Vector3(1f, 1f, z);
    adjacentBuilding.RegisterDependentObject(dependentObject);
  }

  public void AssignBuildingSet(MapBuildingSet buildingSet, int index)
  {
    this.buildingSet = buildingSet;
    foreach (MapBuilding.MoveablePart movingPart in this.movingParts)
      movingPart.Initialize();
    this.index = index;
    if (this.movingParts.Length == 0)
      return;
    buildingSet.animatedBuildings.Add(this);
  }

  public void OnDestroy() => this.removeFromSet = true;

  public PartResult MapBuilding_OnUpdate()
  {
    if (this.removeFromSet)
      return PartResult.Remove;
    foreach (MapBuilding.MoveablePart movingPart in this.movingParts)
      movingPart.Animate();
    return PartResult.None;
  }

  [Serializable]
  public class MoveablePart
  {
    [SerializeField]
    private Transform transform;
    [SerializeField]
    private MoveablePartAnimation animation;

    public void Initialize()
    {
      if ((UnityEngine.Object) this.animation == (UnityEngine.Object) null)
        return;
      this.animation.Initialize(this.transform);
    }

    public void Animate()
    {
      if ((UnityEngine.Object) this.animation == (UnityEngine.Object) null)
        return;
      this.animation.Animate(this.transform);
    }
  }
}
