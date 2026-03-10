// Decompiled with JetBrains decompiler
// Type: UnitStorage
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Cysharp.Threading.Tasks;
using NuclearOption.Networking;
using NuclearOption.SavedMission;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

#nullable disable
public class UnitStorage : MonoBehaviour
{
  public UnitPart UnitPart;
  public float MassLimit;
  private float currentMass;
  private float volumeLimit;
  private float currentVolume;
  [SerializeField]
  private Vector3 dimensions;
  [SerializeField]
  private UnitInventory inventory;
  [SerializeField]
  private Transform deployTransform;
  [SerializeField]
  private Transform approachTransform;
  [SerializeField]
  private float deployClearance = 20f;
  [SerializeField]
  private float approachDistance;
  [SerializeField]
  private float doorsLastOpened;
  [SerializeField]
  private UnitStorage.Door[] doors;
  [SerializeField]
  private List<UnitDefinition> deployableTypes = new List<UnitDefinition>();
  private Unit lastDeployedUnit;
  private List<Unit> incoming = new List<Unit>();
  private List<UnitDefinition> deploying = new List<UnitDefinition>();
  private bool deployingUnits;
  private bool doorsClosed;
  [SerializeField]
  private bool deployRail;

  private void Awake()
  {
    this.doorsClosed = true;
    this.enabled = false;
    this.volumeLimit = this.dimensions.x * this.dimensions.y * this.dimensions.z;
    this.LinkWithDelay().Forget();
  }

  private async UniTask LinkWithDelay()
  {
    YieldAwaitable yieldAwaitable = UniTask.Yield();
    await yieldAwaitable;
    yieldAwaitable = UniTask.Yield();
    await yieldAwaitable;
    yieldAwaitable = UniTask.Yield();
    await yieldAwaitable;
    yieldAwaitable = UniTask.Yield();
    await yieldAwaitable;
    yieldAwaitable = UniTask.Yield();
    await yieldAwaitable;
    UnitInventory foundSavedInventory;
    if (!this.InventoryFoundInMission(out foundSavedInventory))
      return;
    this.inventory = foundSavedInventory;
    this.inventory.AttachToSavedUnit(this.UnitPart.parentUnit.SavedUnit);
  }

  private void UnitStorage_OnUnitInitialize()
  {
    this.UnitPart.parentUnit.onInitialize -= new Action(this.UnitStorage_OnUnitInitialize);
    if (!this.InventoryFoundInMission(out this.inventory))
      return;
    this.inventory.AttachToSavedUnit(this.UnitPart.parentUnit.SavedUnit);
  }

  public bool CanFit(UnitDefinition unitDefinition)
  {
    return (double) unitDefinition.length <= (double) this.dimensions.z && (double) unitDefinition.width <= (double) this.dimensions.x && (double) unitDefinition.height <= (double) this.dimensions.y;
  }

  public bool ContainsUnit(UnitDefinition unitDefinition)
  {
    if (this.inventory == null || this.inventory.StoredList == null)
      return false;
    foreach (StoredUnitCount stored in this.inventory.StoredList)
    {
      if (stored.UnitType == unitDefinition.unitPrefab.name)
        return true;
    }
    return false;
  }

  public void DeployUnits()
  {
    if (this.deployingUnits)
      return;
    this.DeployAllUnits().Forget();
  }

  public bool HasUnits() => this.inventory != null && this.inventory.StoredList.Count > 0;

  private async UniTask DeployAllUnits()
  {
    UnitStorage fromStorage = this;
    CancellationToken cancel = fromStorage.destroyCancellationToken;
    fromStorage.deployingUnits = true;
    if (fromStorage.inventory == null)
      cancel = new CancellationToken();
    else if (fromStorage.inventory.StoredList.Count == 0)
    {
      cancel = new CancellationToken();
    }
    else
    {
      while (fromStorage.DoorsNotClosed())
      {
        await UniTask.Delay(5000);
        if (cancel.IsCancellationRequested)
        {
          cancel = new CancellationToken();
          return;
        }
        if (fromStorage.UnitPart.parentUnit.disabled)
        {
          cancel = new CancellationToken();
          return;
        }
      }
      UniTask uniTask;
      while (fromStorage.CheckIncomingCount() > 0)
      {
        fromStorage.OpenDoors();
        uniTask = UniTask.Delay(5000);
        await uniTask;
        if (cancel.IsCancellationRequested)
        {
          cancel = new CancellationToken();
          return;
        }
        if (fromStorage.UnitPart.parentUnit.disabled)
        {
          cancel = new CancellationToken();
          return;
        }
      }
      fromStorage.deploying.Clear();
      for (int index1 = fromStorage.inventory.StoredList.Count - 1; index1 >= 0; --index1)
      {
        string unitType = fromStorage.inventory.StoredList[index1].UnitType;
        UnitDefinition unitDefinition = Encyclopedia.Lookup[unitType];
        int count = fromStorage.inventory.StoredList[index1].Count;
        if (fromStorage.deployableTypes.Count <= 0 || fromStorage.deployableTypes.Contains(unitDefinition))
        {
          for (int index2 = 0; index2 < count; ++index2)
            fromStorage.deploying.Add(unitDefinition);
          fromStorage.inventory.AddOrRemove(unitDefinition, -count);
        }
      }
      for (int i = fromStorage.deploying.Count - 1; i >= 0; --i)
      {
        UnitDefinition definition = fromStorage.deploying[i];
        while (!fromStorage.DoorsOpen() || (UnityEngine.Object) fromStorage.lastDeployedUnit != (UnityEngine.Object) null && !fromStorage.lastDeployedUnit.disabled && FastMath.InRange(fromStorage.lastDeployedUnit.transform.position, fromStorage.deployTransform.position, fromStorage.deployClearance))
        {
          fromStorage.OpenDoors();
          uniTask = UniTask.Delay(1000);
          await uniTask;
          if (cancel.IsCancellationRequested)
          {
            cancel = new CancellationToken();
            return;
          }
          if (fromStorage.UnitPart.parentUnit.disabled)
          {
            cancel = new CancellationToken();
            return;
          }
        }
        fromStorage.lastDeployedUnit = NetworkSceneSingleton<Spawner>.i.SpawnUnit(definition, fromStorage.deployTransform.position, fromStorage.deployTransform.rotation, fromStorage.UnitPart.parentUnit.rb.GetPointVelocity(fromStorage.deployTransform.position), fromStorage.UnitPart.parentUnit, (Player) null);
        UnitStorage component;
        if (fromStorage.lastDeployedUnit.gameObject.TryGetComponent<UnitStorage>(out component))
          component.TryFillFromStorage(fromStorage);
        fromStorage.enabled = true;
        if (fromStorage.lastDeployedUnit is GroundVehicle lastDeployedUnit1)
          lastDeployedUnit1.MoveFromDepot();
        if (fromStorage.lastDeployedUnit is Ship lastDeployedUnit2)
          lastDeployedUnit2.Launch();
        fromStorage.deploying.RemoveAt(i);
        definition = (UnitDefinition) null;
      }
      while ((UnityEngine.Object) fromStorage.lastDeployedUnit != (UnityEngine.Object) null && !fromStorage.lastDeployedUnit.disabled && FastMath.InRange(fromStorage.lastDeployedUnit.transform.position, fromStorage.deployTransform.position, fromStorage.deployClearance * 2f))
      {
        fromStorage.OpenDoors();
        uniTask = UniTask.Delay(1000);
        await uniTask;
        if (cancel.IsCancellationRequested)
        {
          cancel = new CancellationToken();
          return;
        }
        if (fromStorage.UnitPart.parentUnit.disabled)
        {
          cancel = new CancellationToken();
          return;
        }
      }
      fromStorage.deployingUnits = false;
      cancel = new CancellationToken();
    }
  }

  public bool DoorsOpen()
  {
    foreach (UnitStorage.Door door in this.doors)
    {
      if (!door.IsOpen())
        return false;
    }
    return true;
  }

  public bool DoorsNotClosed()
  {
    foreach (UnitStorage.Door door in this.doors)
    {
      if (door.IsNotClosed())
        return true;
    }
    return false;
  }

  public void OpenDoors()
  {
    this.doorsLastOpened = Time.timeSinceLevelLoad;
    if (!this.doorsClosed)
      return;
    this.MoveDoors().Forget();
  }

  private async UniTask MoveDoors()
  {
    UnitStorage unitStorage = this;
    CancellationToken cancel = unitStorage.destroyCancellationToken;
    unitStorage.doorsClosed = false;
    while (!unitStorage.doorsClosed)
    {
      if (cancel.IsCancellationRequested)
      {
        cancel = new CancellationToken();
        return;
      }
      if (unitStorage.UnitPart.parentUnit.disabled)
      {
        cancel = new CancellationToken();
        return;
      }
      unitStorage.doorsClosed = true;
      foreach (UnitStorage.Door door in unitStorage.doors)
      {
        int target = (double) Time.timeSinceLevelLoad - (double) unitStorage.doorsLastOpened < 5.0 ? 1 : 0;
        door.Animate((float) target);
        if (target == 1 || !door.Finished())
          unitStorage.doorsClosed = false;
      }
      await UniTask.Yield();
    }
    unitStorage.doorsClosed = true;
    cancel = new CancellationToken();
  }

  public bool HasFinishedDeploying() => !this.deployingUnits;

  public Unit GetUnit() => this.UnitPart.parentUnit;

  public Transform GetDoorTransform()
  {
    return this.doors.Length != 0 ? this.doors[0].GetTransform() : this.deployTransform;
  }

  public GlobalPosition GetApproachTarget(Unit approachingUnit)
  {
    Vector3 vector3_1 = this.approachTransform.position + this.approachTransform.forward * this.approachDistance;
    float num = FastMath.Distance(this.approachTransform.GlobalPosition(), approachingUnit.GlobalPosition());
    Vector3 target = approachingUnit.transform.position - this.approachTransform.position;
    float t = Vector3.Dot(target.normalized, this.approachTransform.forward);
    Vector3 vector3_2 = Vector3.RotateTowards(this.approachTransform.forward * this.approachDistance, target, 1.57079637f, 0.0f);
    return Vector3.Lerp(vector3_1 + vector3_2, this.approachTransform.position + this.approachTransform.forward * num * 0.7f, t).ToGlobalPosition();
  }

  public void RegisterIncoming(Unit incomingUnit)
  {
    if (this.incoming.Contains(incomingUnit))
      return;
    this.incoming.Add(incomingUnit);
  }

  public int CheckIncomingCount()
  {
    for (int index = this.incoming.Count - 1; index >= 0; --index)
    {
      Unit unit = this.incoming[index];
      if ((UnityEngine.Object) unit == (UnityEngine.Object) null || unit.disabled || !FastMath.InRange(unit.transform.position, this.approachTransform.position, 1000f))
        this.incoming.RemoveAt(index);
    }
    return this.incoming.Count;
  }

  public void Transfer(UnitStorage fromInventory)
  {
    this.inventory.Transfer(fromInventory.inventory);
  }

  public void TryFillFromStorage(UnitStorage fromStorage)
  {
    UnitInventory inventory = fromStorage.inventory;
    for (int index = inventory.StoredList.Count - 1; index >= 0; --index)
    {
      StoredUnitCount stored = inventory.StoredList[index];
      UnitDefinition unitDefinition = Encyclopedia.Lookup[stored.UnitType];
      int numAllowed;
      if (this.CanStoreUnit(unitDefinition, stored.Count, out numAllowed) && numAllowed > 0)
      {
        fromStorage.AddOrRemoveUnit(unitDefinition, -numAllowed);
        this.AddOrRemoveUnit(unitDefinition, numAllowed);
      }
    }
  }

  public bool CanStoreUnit(UnitDefinition definition, int numberToStore, out int numAllowed)
  {
    numAllowed = 0;
    if (!this.CanFit(definition))
      return false;
    float num1 = this.MassLimit - this.currentMass;
    numAllowed = Mathf.Min(Mathf.FloorToInt(num1 / definition.mass), numberToStore);
    float num2 = this.volumeLimit - this.currentVolume;
    numAllowed = Mathf.Min(numAllowed, Mathf.FloorToInt(num2 / (definition.length + definition.width + definition.height)));
    return numAllowed > 0;
  }

  public void Store(Unit unit)
  {
    if (!unit.LocalSim)
      return;
    this.incoming.Remove(unit);
    this.inventory.AddOrRemove(unit.definition, 1);
    unit.NetworkunitState = Unit.UnitState.Returned;
    unit.Networkdisabled = true;
    UnityEngine.Object.Destroy((UnityEngine.Object) unit, 3f);
  }

  public List<StoredUnitCount> GetStoredList() => this.inventory?.StoredList;

  public void AddOrRemoveUnitEditor(
    Mission mission,
    SavedUnit savedUnit,
    UnitDefinition unitDefinition,
    int change)
  {
    if (GameManager.gameState != GameState.Editor)
      Debug.LogError((object) "AddOrRemoveUnitEditor should only be used in editor");
    else
      this.GetOrCreateStorageList(mission, savedUnit).AddOrRemove(unitDefinition, change);
  }

  public void AddOrRemoveUnit(UnitDefinition unitDefinition, int amount)
  {
    if (GameManager.gameState == GameState.Editor)
    {
      Debug.LogError((object) "AddOrRemoveUnit should not be used in editor");
    }
    else
    {
      this.inventory.AddOrRemove(unitDefinition, amount);
      this.currentMass += (float) amount * unitDefinition.mass;
      this.currentVolume += (float) amount * (unitDefinition.length * unitDefinition.height * unitDefinition.width);
    }
  }

  public bool InventoryFoundInMission(out UnitInventory foundSavedInventory)
  {
    foundSavedInventory = (UnitInventory) null;
    Mission currentMission = MissionManager.CurrentMission;
    if (currentMission == null)
      return false;
    foreach (UnitInventory unitInventory in currentMission.unitInventories)
    {
      if (unitInventory.AttachedUnitUniqueName == this.UnitPart.parentUnit.UniqueName)
      {
        foundSavedInventory = unitInventory;
        break;
      }
    }
    return foundSavedInventory != null;
  }

  public UnitInventory GetOrCreateStorageList(Mission mission, SavedUnit owner)
  {
    UnitInventory inventory = this.inventory;
    if ((inventory != null ? (inventory.StoredList.Count > 0 ? 1 : 0) : 0) != 0)
      return this.inventory;
    this.inventory = new UnitInventory();
    this.inventory.AttachToSavedUnit(owner);
    mission.unitInventories.Add(this.inventory);
    return this.inventory;
  }

  private void FixedUpdate()
  {
    if (!this.deployRail || (UnityEngine.Object) this.lastDeployedUnit == (UnityEngine.Object) null || !FastMath.InRange(this.lastDeployedUnit.transform.position, this.deployTransform.position, this.deployClearance))
    {
      this.enabled = false;
    }
    else
    {
      this.lastDeployedUnit.rb.AddForce(-(this.lastDeployedUnit.transform.position - (this.deployTransform.position + Vector3.Project(this.lastDeployedUnit.transform.position - this.deployTransform.position, this.deployTransform.forward)) - 0.2f * this.deployTransform.forward) * this.lastDeployedUnit.rb.mass * 10f);
      this.lastDeployedUnit.rb.AddTorque(Vector3.up * -TargetCalc.GetAngleOnAxis(this.deployTransform.forward, this.lastDeployedUnit.transform.forward, Vector3.up) * this.lastDeployedUnit.rb.mass * 40f);
    }
  }

  [Serializable]
  private class Door
  {
    [SerializeField]
    private Transform transform;
    [SerializeField]
    private bool move;
    [SerializeField]
    private bool rotate;
    [SerializeField]
    private float openSpeed;
    [SerializeField]
    private Vector3 positionClosed;
    [SerializeField]
    private Vector3 positionOpen;
    [SerializeField]
    private Vector3 rotationClosed;
    [SerializeField]
    private Vector3 rotationOpen;
    private float currentPosition;

    public bool IsOpen() => (double) this.currentPosition == 1.0;

    public bool IsNotClosed() => (double) this.currentPosition > 0.0;

    public Transform GetTransform() => this.transform;

    public void Animate(float target)
    {
      this.currentPosition += Mathf.Clamp(target - this.currentPosition, -this.openSpeed * Time.deltaTime, this.openSpeed * Time.deltaTime);
      this.currentPosition = Mathf.Clamp01(this.currentPosition);
      if (this.move)
        this.transform.localPosition = Vector3.Lerp(this.positionClosed, this.positionOpen, this.currentPosition);
      if (!this.rotate)
        return;
      this.transform.localEulerAngles = Vector3.Lerp(this.rotationClosed, this.rotationOpen, this.currentPosition);
    }

    public bool Finished()
    {
      return (double) this.currentPosition == 1.0 || (double) this.currentPosition == 0.0;
    }
  }
}
