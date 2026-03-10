// Decompiled with JetBrains decompiler
// Type: NuclearOption.DedicatedServer.MissionRotation
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using JamesFrowen.ScriptableVariables;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace NuclearOption.DedicatedServer;

public class MissionRotation
{
  private List<MissionOptions> allMissions;
  private readonly RotationType rotationType;
  private int _nextIndex;
  private List<MissionOptions> randomQueue;
  private MissionOptions NextQueued;
  private MissionOptions? NextOverride;

  public MissionRotation(MissionOptions[] missionRotation, RotationType rotationType)
  {
    this.allMissions = missionRotation == null || missionRotation.Length != 0 ? ((IEnumerable<MissionOptions>) missionRotation).ToList<MissionOptions>() : throw new ArgumentNullException(nameof (missionRotation), "MissionRotation must have atleast 1 mission");
    this.rotationType = rotationType;
    this._nextIndex = 0;
    if (rotationType == RotationType.RandomQueue)
    {
      this.randomQueue = new List<MissionOptions>(missionRotation.Length);
      for (int index = 0; index < missionRotation.Length; ++index)
        this.randomQueue.Add(missionRotation[index]);
      MissionRotation.Shuffle<MissionOptions>((IList<MissionOptions>) this.randomQueue, true);
    }
    this.NextQueued = this.GetNextMapInternal();
  }

  private static void Shuffle<T>(IList<T> list, bool firstShuffle = false)
  {
    if (list.Count == 1)
      return;
    for (int index1 = 0; index1 < list.Count - 1; ++index1)
    {
      int index2 = index1 != 0 || firstShuffle ? UnityEngine.Random.Range(index1, list.Count) : UnityEngine.Random.Range(0, list.Count - 1);
      T obj = list[index1];
      list[index1] = list[index2];
      list[index2] = obj;
    }
  }

  private MissionOptions GetNextMapInternal()
  {
    switch (this.rotationType)
    {
      case RotationType.Sequence:
        if (this._nextIndex >= this.allMissions.Count)
          this._nextIndex = 0;
        MissionOptions allMission = this.allMissions[this._nextIndex];
        this._nextIndex = (this._nextIndex + 1) % this.allMissions.Count;
        return allMission;
      case RotationType.RandomQueue:
        if (this._nextIndex >= this.randomQueue.Count)
        {
          MissionRotation.Shuffle<MissionOptions>((IList<MissionOptions>) this.randomQueue);
          this._nextIndex = 0;
        }
        MissionOptions random = this.randomQueue[this._nextIndex];
        ++this._nextIndex;
        return random;
      default:
        return this.allMissions.RandomElement<MissionOptions>();
    }
  }

  public MissionOptions GetNext()
  {
    if (this.NextOverride.HasValue)
    {
      MissionOptions next = this.NextOverride.Value;
      this.NextOverride = new MissionOptions?();
      return next;
    }
    MissionOptions nextQueued = this.NextQueued;
    this.NextQueued = this.GetNextMapInternal();
    return nextQueued;
  }

  public MissionOptions PeakNext() => this.NextOverride ?? this.NextQueued;

  public void OverrideNext(MissionOptions option)
  {
    this.NextOverride = new MissionOptions?(option);
  }

  public void RemoveBrokenMap(MissionKeySaveable key)
  {
    ColorLog<DedicatedServerManager>.Info($"Failed to load mission. Removing {key} from missionRotation");
    this.allMissions.RemoveAll((Predicate<MissionOptions>) (m => m.Key.Equals(key)));
    this.randomQueue?.RemoveAll((Predicate<MissionOptions>) (m => m.Key.Equals(key)));
    if (this._nextIndex > 0)
      --this._nextIndex;
    if (key.Equals((object) this.NextOverride.Value))
    {
      this.NextOverride = new MissionOptions?();
    }
    else
    {
      if (!key.Equals((object) this.NextQueued))
        return;
      this.NextQueued = this.GetNextMapInternal();
    }
  }
}
