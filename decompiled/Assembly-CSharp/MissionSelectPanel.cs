// Decompiled with JetBrains decompiler
// Type: MissionSelectPanel
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.SavedMission;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#nullable disable
public class MissionSelectPanel : MonoBehaviour
{
  [SerializeField]
  private MissionSelectList missionSelectList;
  [SerializeField]
  private TagFilterList tagFilterList;
  private MissionGroup activeGroup;
  private readonly HashSet<MissionTag> enabledFilters = new HashSet<MissionTag>();
  private readonly List<MissionTag> allTags = new List<MissionTag>();
  private readonly List<(MissionKey key, MissionQuickLoad mission)> allMissions = new List<(MissionKey, MissionQuickLoad)>();
  private readonly List<(MissionKey key, MissionQuickLoad mission)> filteredMissions = new List<(MissionKey, MissionQuickLoad)>();
  private readonly List<MissionGroup> disallowedGroups = new List<MissionGroup>();
  private readonly HashSet<MissionTag> requiredTags = new HashSet<MissionTag>();

  public MissionKey SelectedMission { get; private set; }

  public event Action<MissionKey> OnMissionSelecteed;

  public void SetSelectedMission(MissionKey item)
  {
    this.SelectedMission = item;
    Action<MissionKey> missionSelecteed = this.OnMissionSelecteed;
    if (missionSelecteed != null)
      missionSelecteed(item);
    this.missionSelectList.Redraw();
  }

  public void SetActiveGroup(MissionGroup group)
  {
    this.activeGroup = group;
    this.allMissions.Clear();
    this.allMissions.AddRange(MissionSaveLoad.QuickLoadMany(group.GetMissions().Where<MissionKey>((Func<MissionKey, bool>) (x => !this.disallowedGroups.Contains(x.Group)))));
    this.allTags.Clear();
    this.allTags.AddRange((IEnumerable<MissionTag>) this.allMissions.SelectMany<(MissionKey, MissionQuickLoad), MissionTag>((Func<(MissionKey, MissionQuickLoad), IEnumerable<MissionTag>>) (x => (IEnumerable<MissionTag>) x.mission.missionSettings.Tags)).Concat<MissionTag>((IEnumerable<MissionTag>) MissionTag.InternalTags).Distinct<MissionTag>().OrderBy<MissionTag, int>((Func<MissionTag, int>) (x => x.SortOrder)));
    this.RefreshLists();
  }

  private void RefreshLists()
  {
    this.enabledFilters.UnionWith((IEnumerable<MissionTag>) this.requiredTags);
    this.filteredMissions.Clear();
    this.filteredMissions.AddRange(this.allMissions.Where<(MissionKey, MissionQuickLoad)>(new Func<(MissionKey, MissionQuickLoad), bool>(this.FilterByTags)));
    this.missionSelectList.UpdateList(this.filteredMissions.Select<(MissionKey, MissionQuickLoad), MissionSelectListItem.Item>((Func<(MissionKey, MissionQuickLoad), MissionSelectListItem.Item>) (x => new MissionSelectListItem.Item(this, x.key, x.mission))));
    this.tagFilterList.UpdateList(this.allTags.Select<MissionTag, (MissionTag, bool, int)>((Func<MissionTag, (MissionTag, bool, int)>) (x => (x, this.enabledFilters.Contains(x), this.CountMissions(x)))).Select<(MissionTag, bool, int), TagFilterListItem.Item>((Func<(MissionTag, bool, int), TagFilterListItem.Item>) (x => new TagFilterListItem.Item(x.tag, new Action<MissionTag>(this.TagFilterClicked), x.enabled, x.count))));
  }

  private bool FilterByTags((MissionKey key, MissionQuickLoad mission) item)
  {
    List<MissionTag> tags = item.mission.missionSettings.Tags;
    foreach (MissionTag enabledFilter in this.enabledFilters)
    {
      MissionTag filter = enabledFilter;
      if (!tags.Any<MissionTag>((Func<MissionTag, bool>) (x => x.Equals(filter))))
        return false;
    }
    return true;
  }

  private int CountMissions(MissionTag tag)
  {
    return this.filteredMissions.Count<(MissionKey, MissionQuickLoad)>((Func<(MissionKey, MissionQuickLoad), bool>) (x => x.mission.missionSettings.Tags.Any<MissionTag>((Func<MissionTag, bool>) (x => x.Equals(tag)))));
  }

  private void TagFilterClicked(MissionTag tag)
  {
    if (this.requiredTags.Contains(tag))
      return;
    this.EnableTag(tag, !this.enabledFilters.Contains(tag));
  }

  private void EnableTag(MissionTag tag, bool enable)
  {
    if (enable)
      this.enabledFilters.Add(tag);
    else
      this.enabledFilters.Remove(tag);
    this.RefreshLists();
  }

  public void SetPickerFilter(MissionsPicker.Filter filter)
  {
    if (filter.DisallowedGroups != null)
      this.disallowedGroups.AddRange((IEnumerable<MissionGroup>) filter.DisallowedGroups);
    if (filter.RequiredTags != null)
      this.requiredTags.UnionWith((IEnumerable<MissionTag>) filter.RequiredTags);
    if (this.activeGroup == null)
      return;
    this.SetActiveGroup(this.activeGroup);
  }
}
