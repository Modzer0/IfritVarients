// Decompiled with JetBrains decompiler
// Type: MissionManagerDebugGui
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Cysharp.Threading.Tasks;
using Mirage;
using NuclearOption.Networking;
using NuclearOption.SavedMission;
using NuclearOption.SavedMission.ObjectiveV2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

#nullable disable
[ExecuteInEditMode]
public class MissionManagerDebugGui : MonoBehaviour
{
  [SerializeField]
  private Rect box = new Rect(20f, 60f, 300f, 800f);
  [SerializeField]
  private bool rightAlign;
  [SerializeField]
  private bool bottomAlign;
  [SerializeField]
  private Color normalColor = new Color(0.1f, 0.1f, 0.1f, 0.2f);
  [SerializeField]
  private Color loadingColor = new Color(0.5f, 0.1f, 0.1f, 0.6f);
  private string missionName = "";
  private StringBuilder builder = new StringBuilder();
  private Texture2D background;
  private GUIStyle backgroundStyle;

  private void Start()
  {
    this.SetBackgroundColor(this.normalColor);
    MissionManager.onMissionLoad += new Action<Mission>(this.MissionManager_onMissionLoad);
  }

  private void OnDestroy()
  {
    MissionManager.onMissionLoad -= new Action<Mission>(this.MissionManager_onMissionLoad);
    if (!((UnityEngine.Object) this.background != (UnityEngine.Object) null))
      return;
    UnityEngine.Object.Destroy((UnityEngine.Object) this.background);
  }

  private void OnValidate() => this.SetBackgroundColor(this.normalColor);

  private void MissionManager_onMissionLoad(Mission obj) => this.missionName = obj.Name;

  private void SetBackgroundColor(Color color)
  {
    if ((UnityEngine.Object) this.background == (UnityEngine.Object) null)
      this.background = new Texture2D(2, 2);
    Color[] colors = new Color[this.background.width * this.background.height];
    for (int index = 0; index < colors.Length; ++index)
      colors[index] = color;
    this.background.SetPixels(colors);
    this.background.Apply();
    if (this.backgroundStyle == null)
      this.backgroundStyle = new GUIStyle();
    this.backgroundStyle.normal.background = this.background;
  }

  private void OnGUI()
  {
    if (this.rightAlign)
      this.box.x = (float) Screen.width - this.box.x - this.box.width;
    if (this.bottomAlign)
      this.box.y = (float) Screen.height - this.box.y - this.box.height;
    GUILayout.BeginArea(this.box);
    using (new GUILayout.VerticalScope(this.backgroundStyle, Array.Empty<GUILayoutOption>()))
    {
      GUILayout.Label($"Is Running {MissionManager.IsRunning}");
      GUILayout.Label("Mission: " + (MissionManager.CurrentMission?.Name ?? "NULL"));
      if (MissionManager.IsRunning)
      {
        if (MissionManager.CurrentMission != null)
          this.DrawObjectives();
      }
    }
    GUILayout.EndArea();
  }

  private void DrawLoadUnloadButtons()
  {
    using (new GUILayout.HorizontalScope(Array.Empty<GUILayoutOption>()))
    {
      GUI.enabled = MissionManager.IsRunning;
      if (GUILayout.Button("Unload"))
        this.UnloadSlow().Forget();
      GUI.enabled = !MissionManager.IsRunning;
      if (GUILayout.Button("Load"))
        this.LoadSlow(this.missionName, GameState.Multiplayer).Forget();
      GUI.enabled = MissionManager.IsRunning;
      if (GUILayout.Button("Reload"))
        this.Reload(GameManager.gameState).Forget();
      GUI.enabled = true;
      if (GameManager.gameState != GameState.Editor)
      {
        if (GUILayout.Button("Open editor"))
          this.Reload(GameState.Editor).Forget();
      }
      else if (GUILayout.Button("Play Mission"))
        this.Reload(GameState.SinglePlayer).Forget();
    }
    using (new GUILayout.HorizontalScope(Array.Empty<GUILayoutOption>()))
    {
      GUILayout.Label(new GUIContent("mission"));
      this.missionName = GUILayout.TextField(this.missionName);
    }
  }

  private void DrawObjectives()
  {
    MissionRunner runner = MissionManager.Runner;
    this.DrawObjectives("No Faction", runner.ActiveObjectives.Where<Objective>((Func<Objective, bool>) (x => (UnityEngine.Object) x.FactionHQ == (UnityEngine.Object) null)));
    foreach (Faction faction in FactionRegistry.factions)
    {
      FactionHQ key = FactionRegistry.HQLookup[faction];
      List<Objective> objectiveList1;
      IReadOnlyList<Objective> objectiveList2 = runner.activeByFaction.TryGetValue(key, out objectiveList1) ? (IReadOnlyList<Objective>) objectiveList1 : (IReadOnlyList<Objective>) Array.Empty<Objective>();
      this.DrawObjectives(faction.factionName, (IEnumerable<Objective>) objectiveList2);
    }
  }

  private void DrawObjectives(string factionName, IEnumerable<Objective> objectives)
  {
    this.builder.AppendLine("Objective: " + factionName);
    bool flag = true;
    foreach (Objective objective in objectives)
    {
      flag = false;
      this.builder.AppendLine($" - {objective}");
      float completePercent = objective.CompletePercent;
      this.builder.Append("   ");
      MissionManagerDebugGui.AppendProgressBar(this.builder, completePercent);
      this.builder.Append("\n");
    }
    if (flag)
      this.builder.AppendLine("<none>");
    GUILayout.Label(this.builder.ToString());
    this.builder.Clear();
  }

  public static string CreateProgressBar(float percent, int blocks = 24)
  {
    StringBuilder builder = new StringBuilder();
    MissionManagerDebugGui.AppendProgressBar(builder, percent, blocks);
    return builder.ToString();
  }

  public static string CreateProgressBar(StringBuilder builder, float percent, int blocks = 24)
  {
    builder.Clear();
    MissionManagerDebugGui.AppendProgressBar(builder, percent, blocks);
    return builder.ToString();
  }

  public static void AppendProgressBar(StringBuilder builder, float percent, int blocks = 24)
  {
    int num = (int) ((double) percent * (double) blocks);
    builder.Append("[");
    for (int index = 0; index < num; ++index)
      builder.Append("#");
    for (int index = 0; index < blocks - num; ++index)
      builder.Append("_");
    builder.Append($"] {(int) ((double) percent * 100.0)}%");
  }

  private UniTask UnloadSlow() => throw new NotImplementedException();

  private UniTask LoadSlow(string missionName, GameState state)
  {
    throw new NotImplementedException();
  }

  private static void UnloadInScene()
  {
    MissionManager.SetNullMission();
    ServerObjectManager serverObjectManager = NetworkManagerNuclearOption.i.ServerObjectManager;
    foreach (Unit allUnit in UnitRegistry.allUnits)
      serverObjectManager.Destroy(allUnit.gameObject, false);
    GameManager.ResetGameResolution();
  }

  private void LoadInScene(string missionName, GameState state)
  {
    throw new NotImplementedException();
  }

  private async UniTask Reload(GameState state)
  {
    string mission = this.missionName;
    UniTask uniTask = this.UnloadSlow();
    await uniTask;
    this.missionName = mission;
    uniTask = this.LoadSlow(mission, state);
    await uniTask;
    mission = (string) null;
  }
}
