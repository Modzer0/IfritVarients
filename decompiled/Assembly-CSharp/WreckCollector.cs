// Decompiled with JetBrains decompiler
// Type: WreckCollector
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#nullable disable
public class WreckCollector : MonoBehaviour
{
  [SerializeField]
  private Unit attachedUnit;
  [SerializeField]
  private float range;
  [SerializeField]
  private int maxWrecks = 10;
  [SerializeField]
  private int currentWrecks;
  private Wreckage currentTarget;
  private Building nearestDepot;
  public List<GameObject> storedWrecks = new List<GameObject>();
  public WreckCollector.CollectorState currentState;
  private float defaultMass;

  private void Start()
  {
    this.attachedUnit.onInitialize += new Action(this.WreckCollector_OnInitialize);
  }

  private void WreckCollector_OnInitialize()
  {
    foreach (GameObject storedWreck in this.storedWrecks)
      storedWreck.SetActive(false);
    this.currentTarget = (Wreckage) null;
    this.nearestDepot = (Building) null;
    this.defaultMass = this.attachedUnit.rb.mass;
    this.currentState = WreckCollector.CollectorState.Wait;
    if (!this.attachedUnit.IsServer)
      return;
    this.StartSlowUpdate(1f, new Action(this.UpdateState));
  }

  private void UpdateState()
  {
    if (this.attachedUnit.disabled)
      return;
    Debug.Log((object) $"WRECK REMOVAL {this.attachedUnit.name}, ENTER LOOP");
    switch (this.currentState)
    {
      case WreckCollector.CollectorState.Wait:
        this.FindWreck();
        break;
      case WreckCollector.CollectorState.GoToWreck:
        this.ReachedWreckCheck();
        break;
      case WreckCollector.CollectorState.GoToDepot:
        this.ReachedDepotCheck();
        break;
    }
  }

  private void FindWreck()
  {
    List<Wreckage> list = BattlefieldGrid.GetWrecksInRangeEnumerable(this.transform.GlobalPosition(), 1000f).ToList<Wreckage>();
    if (list.Count == 0)
      return;
    float range = 1000f;
    for (int index = 0; index < list.Count; ++index)
    {
      Wreckage wreckage = list[index];
      if (FastMath.InRange(wreckage.transform.position, this.transform.position, range))
      {
        float num = Vector3.Distance(wreckage.transform.position, this.transform.position);
        if ((double) num < (double) range && (double) num > 2.0 * (double) this.range)
        {
          this.currentTarget = wreckage;
          range = num;
        }
      }
    }
    if ((UnityEngine.Object) this.currentTarget != (UnityEngine.Object) null)
    {
      if (!(this.attachedUnit is GroundVehicle attachedUnit))
        return;
      attachedUnit.UnitCommand.SetDestination(this.currentTarget.transform.GlobalPosition(), false);
      this.currentState = WreckCollector.CollectorState.GoToWreck;
      Debug.Log((object) $"WRECK {this.currentTarget.name} HAS BEEN FOUND BY {this.attachedUnit.name} DIST {range:F0}");
    }
    else
    {
      if (this.currentWrecks <= 0)
        return;
      Debug.Log((object) $"NO WRECK HAS BEEN FOUND BY {this.attachedUnit.name}, LOOK FOR DEPOT");
      this.FindDepot();
    }
  }

  private void FindDepot()
  {
    this.transform.GlobalPosition();
    this.nearestDepot = (Building) this.attachedUnit.NetworkHQ.GetNearestDepot(this.attachedUnit.transform.position, 10000f);
    if ((UnityEngine.Object) this.nearestDepot != (UnityEngine.Object) null && !this.nearestDepot.disabled)
    {
      if (!(this.attachedUnit is GroundVehicle attachedUnit))
        return;
      attachedUnit.UnitCommand.SetDestination(this.nearestDepot.transform.GlobalPosition(), false);
      this.currentState = WreckCollector.CollectorState.GoToDepot;
      Debug.Log((object) $"{this.attachedUnit.name} FOUND {this.nearestDepot.name}");
    }
    else
      this.currentState = WreckCollector.CollectorState.Stop;
  }

  private void ReachedWreckCheck()
  {
    if (this.currentState != WreckCollector.CollectorState.GoToWreck || !FastMath.InRange(this.currentTarget.transform.position, this.transform.position, this.range))
      return;
    this.CollectWreck(this.currentTarget);
  }

  private void CollectWreck(Wreckage wreck)
  {
    if (this.storedWrecks.Count > this.currentWrecks)
      this.storedWrecks[this.currentWrecks].SetActive(true);
    ++this.currentWrecks;
    this.attachedUnit.rb.mass += 1000f;
    this.currentTarget = (Wreckage) null;
    Debug.Log((object) $"WRECK {wreck.name} HAS BEEN COLLECTED BY {this.attachedUnit.name} TOTAL MASS {this.attachedUnit.rb.mass}");
    wreck.Deactivate();
    if (this.currentWrecks == this.maxWrecks)
      this.FindDepot();
    else
      this.currentState = WreckCollector.CollectorState.Wait;
  }

  private void ReachedDepotCheck()
  {
    if (this.currentState != WreckCollector.CollectorState.GoToDepot || !FastMath.InRange(this.nearestDepot.transform.position, this.transform.position, this.nearestDepot.maxRadius))
      return;
    this.EmptyWrecks();
  }

  private void EmptyWrecks()
  {
    Debug.Log((object) $"{this.attachedUnit.name} HAS REACHED {this.nearestDepot.name} UNLOADING {this.currentWrecks} WRECKS");
    foreach (GameObject storedWreck in this.storedWrecks)
      storedWreck.SetActive(false);
    this.RewardWreckCollection(this.currentWrecks);
    this.currentWrecks = 0;
    this.attachedUnit.rb.mass += this.defaultMass;
    this.currentTarget = (Wreckage) null;
    this.nearestDepot = (Building) null;
    this.currentState = WreckCollector.CollectorState.Wait;
  }

  private void RewardWreckCollection(int nbWrecks)
  {
    this.attachedUnit.NetworkHQ.AddScore(0.1f * (float) nbWrecks);
  }

  public enum CollectorState
  {
    Stop,
    Wait,
    GoToWreck,
    GoToDepot,
  }
}
