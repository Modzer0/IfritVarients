// Decompiled with JetBrains decompiler
// Type: Capture
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Mirage;
using Mirage.Serialization;
using NuclearOption.DebugScripts;
using NuclearOption.Networking;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Profiling;
using UnityEngine;

#nullable disable
public class Capture : NetworkBehaviour
{
  private static readonly ProfilerMarker captureMarker = new ProfilerMarker("Airbase.Capture");
  private static readonly ProfilerMarker unitsMarker = new ProfilerMarker("Airbase.Capture.UpdateUnits");
  private static readonly ProfilerMarker changeMarker = new ProfilerMarker("Airbase.Capture.GetChange");
  private static readonly ProfilerMarker debugVisMarker = new ProfilerMarker("Airbase.Capture.Debug");
  [Tooltip("Drop in control if capturing and no units in range (and no faction in control). Effected by airbase defense")]
  [SerializeField]
  private float passiveDrop = 1f;
  [Tooltip("What balance capturing starts at when going from neutral to capturing")]
  [SerializeField]
  private float startingCapture = 0.1f;
  [Tooltip("How often to update balance. Numbers are still per second, so larger interval means bigger jumps in balance")]
  [SerializeField]
  private float checkInterval = 1f;
  private readonly Dictionary<FactionHQ, Capture.CapturingUnits> capturingFactions = new Dictionary<FactionHQ, Capture.CapturingUnits>();
  private ICapturable target;
  private float lastCaptureCheck;
  private bool capturable;
  private DebugText debugText;
  private static StringBuilder debugBuilder = new StringBuilder();
  protected Dictionary<PersistentID, float> captureCredit;
  [NonSerialized]
  private const int SYNC_VAR_COUNT = 1;
  [NonSerialized]
  private const int RPC_COUNT = 0;

  public float controlBalance
  {
    get => this.\u003CcontrolBalance\u003Ek__BackingField;
    private set => this.Network\u003CcontrolBalance\u003Ek__BackingField = value;
  }

  public FactionHQ capturingHQ { get; private set; }

  private void Awake()
  {
    this.target = this.GetComponent<ICapturable>();
    if (this.target != null)
      return;
    Debug.LogError((object) ("Capture could not find ICapturable on " + this.name));
  }

  public void ForceCapture(FactionHQ hq)
  {
    this.controlBalance = 1f;
    this.target.OnCapture(hq);
    this.capturingHQ = (FactionHQ) null;
    this.ReportTakingControl();
  }

  public void SetCapturable(bool capturable)
  {
    this.capturable = capturable;
    this.controlBalance = (UnityEngine.Object) this.target.CurrentHQ != (UnityEngine.Object) null ? 1f : 0.0f;
  }

  public void Update()
  {
    if (GameManager.gameState == GameState.Editor || !NetworkManagerNuclearOption.i.Server.Active || this.target.gridSquares == null)
      return;
    if ((double) Time.realtimeSinceStartup - (double) this.lastCaptureCheck > (double) this.checkInterval)
    {
      float timeDelta = (double) this.lastCaptureCheck == 0.0 ? this.checkInterval : Time.realtimeSinceStartup - this.lastCaptureCheck;
      this.lastCaptureCheck = Time.realtimeSinceStartup;
      if (this.capturable && !this.target.disabled)
      {
        if ((double) Time.timeSinceLevelLoad < 10.0)
          timeDelta *= 100f;
        this.CheckForCapture(timeDelta);
      }
    }
    if (!PlayerSettings.debugVis)
      return;
    this.debugVis();
  }

  private void debugVis()
  {
    using (Capture.debugVisMarker.Auto())
    {
      if ((UnityEngine.Object) this.debugText == (UnityEngine.Object) null)
        this.debugText = UnityEngine.Object.Instantiate<DebugText>(GameAssets.i.debugText, this.target.center.transform);
      this.debugText.Text.text = $"{(!((UnityEngine.Object) this.target.CurrentHQ != (UnityEngine.Object) null) ? (!((UnityEngine.Object) this.capturingHQ != (UnityEngine.Object) null) ? "no faction".AddColor(Color.white * 0.8f) : (this.capturingHQ.faction.factionName + " capturing").AddColor(this.capturingHQ.faction.color)) : (this.target.CurrentHQ.faction.factionName + " controlled").AddColor(this.target.CurrentHQ.faction.color))}\n{(!this.target.disabled ? (this.capturable ? MissionManagerDebugGui.CreateProgressBar(Capture.debugBuilder, this.controlBalance) : "Not Capturable") : "Disabled")}";
    }
  }

  private void CheckForCapture(float timeDelta)
  {
    using (Capture.captureMarker.Auto())
    {
      this.GetInRangeUnits();
      FactionHQ highestHQ;
      this.ApplyChange(this.GetChange(out highestHQ) * timeDelta, highestHQ);
    }
  }

  private void GetInRangeUnits()
  {
    using (Capture.unitsMarker.Auto())
    {
      this.capturingFactions.Clear();
      float captureRange = this.target.CaptureRange;
      Vector3 position = this.target.center.position;
      bool flag = true;
      foreach (GridSquare gridSquare in this.target.gridSquares)
      {
        foreach (Unit unit in gridSquare.units)
        {
          FactionHQ networkHq = unit.NetworkHQ;
          if (!((UnityEngine.Object) unit == (UnityEngine.Object) null) && !unit.disabled && !((UnityEngine.Object) networkHq == (UnityEngine.Object) null) && !FastMath.OutOfRange(unit.transform.position, position, captureRange))
          {
            float num1 = unit.CaptureStrength;
            float num2 = unit.CaptureDefense;
            if ((UnityEngine.Object) this.target.CurrentHQ != (UnityEngine.Object) unit.NetworkHQ)
              this.RecordCapture(unit.persistentID, unit.CaptureStrength);
            if ((double) num1 < 0.0)
              num1 = 0.0f;
            if ((double) num2 < 0.0)
              num2 = 0.0f;
            if ((double) num1 != 0.0 || (double) num2 != 0.0)
            {
              if ((double) num1 != 0.0)
                flag = false;
              Capture.CapturingUnits capturingUnits;
              this.capturingFactions.TryGetValue(networkHq, out capturingUnits);
              capturingUnits.Strength += num1;
              capturingUnits.Defense += num2;
              this.capturingFactions[networkHq] = capturingUnits;
            }
          }
        }
      }
      if (!flag)
        return;
      this.capturingFactions.Clear();
    }
  }

  private float GetChange(out FactionHQ highestHQ)
  {
    using (Capture.changeMarker.Auto())
    {
      if (this.capturingFactions.Count == 0)
      {
        highestHQ = (FactionHQ) null;
        return (UnityEngine.Object) this.capturingHQ != (UnityEngine.Object) null ? this.passiveDrop / this.target.CaptureDefense : 0.0f;
      }
      highestHQ = (FactionHQ) null;
      float num1 = 0.0f;
      float num2 = 0.0f;
      foreach (KeyValuePair<FactionHQ, Capture.CapturingUnits> capturingFaction in this.capturingFactions)
      {
        FactionHQ key = capturingFaction.Key;
        Capture.CapturingUnits capturingUnits = capturingFaction.Value;
        if ((UnityEngine.Object) this.target.CurrentHQ != (UnityEngine.Object) null)
        {
          if ((UnityEngine.Object) this.target.CurrentHQ == (UnityEngine.Object) key)
            num1 = capturingUnits.Strength;
          else
            num2 += capturingUnits.Strength;
        }
        else if ((UnityEngine.Object) this.capturingHQ != (UnityEngine.Object) null)
        {
          if ((UnityEngine.Object) this.capturingHQ == (UnityEngine.Object) key)
            num1 = capturingUnits.Strength;
          else
            num2 += capturingUnits.Strength;
        }
        else if ((double) capturingUnits.Strength > (double) num1)
        {
          num2 += num1;
          highestHQ = capturingFaction.Key;
          num1 = capturingUnits.Strength;
        }
        else
          num2 += capturingUnits.Strength;
      }
      float num3 = num1 - num2;
      float num4 = 0.0f;
      if ((UnityEngine.Object) this.target.CurrentHQ != (UnityEngine.Object) null)
      {
        Capture.CapturingUnits capturingUnits;
        this.capturingFactions.TryGetValue(this.target.CurrentHQ, out capturingUnits);
        num4 += capturingUnits.Defense;
      }
      else
      {
        foreach (Unit defenseUnit in this.target.GetDefenseUnits())
        {
          if (!((UnityEngine.Object) defenseUnit == (UnityEngine.Object) null) && !defenseUnit.disabled)
            num4 += defenseUnit.CaptureDefense;
        }
      }
      float num5 = this.target.CaptureDefense + num4;
      float change = num3 / num5;
      if ((double) this.controlBalance >= 1.0)
        ;
      return change;
    }
  }

  private void ApplyChange(float change, FactionHQ highestHQ)
  {
    float num = this.controlBalance + change;
    if ((double) num > 1.0)
      num = 1f;
    if ((double) num == (double) this.controlBalance)
      return;
    this.controlBalance = num;
    if ((double) change > 0.0)
      LogChange("increasing control", change);
    else
      LogChange("decreasing control", change);
    if ((UnityEngine.Object) this.target.CurrentHQ != (UnityEngine.Object) null)
    {
      if ((double) this.controlBalance > 0.0)
        return;
      this.controlBalance = 0.0f;
      this.target.OnCapture((FactionHQ) null);
    }
    else if ((UnityEngine.Object) this.capturingHQ != (UnityEngine.Object) null)
    {
      if ((double) this.controlBalance >= 1.0)
      {
        this.ForceCapture(this.capturingHQ);
      }
      else
      {
        if ((double) this.controlBalance > 0.0)
          return;
        LogChange("stop capture", change);
        this.controlBalance = 0.0f;
        this.capturingHQ = (FactionHQ) null;
      }
    }
    else
    {
      if ((double) change <= 0.0)
        return;
      this.capturingHQ = highestHQ;
      this.controlBalance = this.startingCapture;
      LogChange("start capture", change);
    }

    static void LogChange(string msg, float change)
    {
    }
  }

  public void RecordCapture(PersistentID lastCapturedBy, float captureAmount)
  {
    if (this.captureCredit == null)
      this.captureCredit = new Dictionary<PersistentID, float>();
    float num;
    this.captureCredit.TryGetValue(lastCapturedBy, out num);
    this.captureCredit[lastCapturedBy] = num + captureAmount;
  }

  public virtual void ReportTakingControl()
  {
    if (!(this.target is Airbase target) || this.captureCredit == null)
      return;
    float num1 = 0.0f;
    float f = 0.0f;
    for (int index = 0; index < target.buildings.Count; ++index)
    {
      if (!target.buildings[index].disabled)
      {
        f += target.buildings[index].definition.value;
        if ((UnityEngine.Object) target.buildings[index].GetComponent<WarheadStorage>() != (UnityEngine.Object) null)
          f += 28f * (float) target.buildings[index].GetComponent<WarheadStorage>().number;
      }
    }
    float num2 = (float) (25.0 + 2.0 * (double) Mathf.Sqrt(f));
    foreach (KeyValuePair<PersistentID, float> keyValuePair in this.captureCredit)
    {
      PersistentUnit persistentUnit;
      if (UnitRegistry.TryGetPersistentUnit(keyValuePair.Key, out persistentUnit) && !((UnityEngine.Object) persistentUnit.GetHQ() != (UnityEngine.Object) target.CurrentHQ))
        num1 += keyValuePair.Value;
    }
    Debug.Log((object) $"AIRBASE {target.SavedAirbase.DisplayName} TOTAL CAPTURE {num1}");
    Dictionary<Player, float> dictionary = new Dictionary<Player, float>();
    foreach (KeyValuePair<PersistentID, float> keyValuePair in this.captureCredit)
    {
      PersistentUnit persistentUnit;
      if (UnitRegistry.TryGetPersistentUnit(keyValuePair.Key, out persistentUnit))
      {
        float num3 = keyValuePair.Value / num1;
        if ((double) num3 >= 0.0099999997764825821)
        {
          FactionHQ hq = persistentUnit.GetHQ();
          if (!((UnityEngine.Object) hq != (UnityEngine.Object) target.CurrentHQ))
          {
            float score = num2 * num3;
            float num4 = score * hq.killReward;
            hq.AddScore(score);
            hq.AddFunds(num4 * hq.playerTaxRate);
            if ((UnityEngine.Object) persistentUnit.player != (UnityEngine.Object) null)
            {
              if (!dictionary.ContainsKey(persistentUnit.player))
                dictionary.Add(persistentUnit.player, 0.0f);
              dictionary[persistentUnit.player] += num3;
            }
          }
        }
      }
    }
    foreach (KeyValuePair<Player, float> keyValuePair in dictionary)
      keyValuePair.Key.HQ.ReportCaptureLocationAction(keyValuePair.Key, target, keyValuePair.Value * num2);
    this.captureCredit = (Dictionary<PersistentID, float>) null;
  }

  private void MirageProcessed()
  {
  }

  public float Network\u003CcontrolBalance\u003Ek__BackingField
  {
    get => this.\u003CcontrolBalance\u003Ek__BackingField;
    set
    {
      if (this.SyncVarEqual<float>(value, this.\u003CcontrolBalance\u003Ek__BackingField))
        return;
      float balanceKBackingField = this.\u003CcontrolBalance\u003Ek__BackingField;
      this.\u003CcontrolBalance\u003Ek__BackingField = value;
      this.SetDirtyBit(1UL);
    }
  }

  public override bool SerializeSyncVars(NetworkWriter writer, bool initialize)
  {
    ulong syncVarDirtyBits = this.SyncVarDirtyBits;
    bool flag = base.SerializeSyncVars(writer, initialize);
    if (initialize)
    {
      // ISSUE: reference to a compiler-generated field
      writer.WriteSingleConverter(this.\u003CcontrolBalance\u003Ek__BackingField);
      return true;
    }
    writer.Write(syncVarDirtyBits, 1);
    if (((long) syncVarDirtyBits & 1L) != 0L)
    {
      // ISSUE: reference to a compiler-generated field
      writer.WriteSingleConverter(this.\u003CcontrolBalance\u003Ek__BackingField);
      flag = true;
    }
    return flag;
  }

  public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
  {
    base.DeserializeSyncVars(reader, initialState);
    if (initialState)
    {
      // ISSUE: reference to a compiler-generated field
      this.\u003CcontrolBalance\u003Ek__BackingField = reader.ReadSingleConverter();
    }
    else
    {
      ulong dirtyBit = reader.Read(1);
      this.SetDeserializeMask(dirtyBit, 0);
      if (((long) dirtyBit & 1L) == 0L)
        return;
      // ISSUE: reference to a compiler-generated field
      this.\u003CcontrolBalance\u003Ek__BackingField = reader.ReadSingleConverter();
    }
  }

  protected override int GetRpcCount() => 0;

  private struct CapturingUnits
  {
    public float Strength;
    public float Defense;
  }
}
