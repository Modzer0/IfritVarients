// Decompiled with JetBrains decompiler
// Type: NuclearOption.BuildScripts.DebugTests.SceneLoadTest
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Cysharp.Threading.Tasks;
using NuclearOption.MissionEditorScripts;
using NuclearOption.SavedMission;
using NuclearOption.SceneLoading;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

#nullable disable
namespace NuclearOption.BuildScripts.DebugTests;

public class SceneLoadTest
{
  private static bool IsRunning;
  private const int DELAY = 500;
  private const string MISSION_MAP_0 = "custom_airbase";
  private const string MISSION_MAP_1 = "NavalMap";

  private static void Log(string message)
  {
    Debug.Log((object) $"<color=red>{new string('-', message.Length)}</color>");
    Debug.Log((object) $"<color=red>[SceneLoadTest] {message}</color>");
    Debug.Log((object) $"<color=red>{new string('-', message.Length)}</color>");
  }

  private static void LogWarning(string message)
  {
    Debug.LogWarning((object) $"<color=red>{new string('-', message.Length)}</color>");
    Debug.LogWarning((object) $"<color=red>[SceneLoadTest] {message}</color>");
    Debug.LogWarning((object) $"<color=red>{new string('-', message.Length)}</color>");
  }

  private static void LogError(string message)
  {
    Debug.LogError((object) $"<color=red>{new string('-', message.Length)}</color>");
    Debug.LogError((object) $"<color=red>[SceneLoadTest] {message}</color>");
    Debug.LogError((object) $"<color=red>{new string('-', message.Length)}</color>");
  }

  private async UniTask Wait()
  {
    await UniTask.Delay(500, true);
    this.AssertNoNullUnits();
    if (!SceneLoadTest.IsRunning)
      throw new SceneLoadTest.StopRunException();
  }

  public async UniTask Run()
  {
    try
    {
      if (SceneLoadTest.IsRunning)
      {
        SceneLoadTest.LogWarning("Already running");
      }
      else
      {
        GameObject target = new GameObject("RunChecker");
        target.AddComponent<SceneLoadTest.RunChecker>();
        UnityEngine.Object.DontDestroyOnLoad((UnityEngine.Object) target);
        await this.RunInner();
      }
    }
    catch (SceneLoadTest.StopRunException ex)
    {
    }
  }

  public async UniTask RunInner()
  {
    await this.Wait();
    MapLoader mapLoader = ((IEnumerable<MapLoader>) Resources.FindObjectsOfTypeAll<MapLoader>()).First<MapLoader>();
    SceneLoadTest.Log("New map 0 from main menu");
    await SceneLoadTest.EditorNewMission(mapLoader.Maps[0]);
    UniTask uniTask = this.Wait();
    await uniTask;
    uniTask = SceneLoadTest.ExitEditor();
    await uniTask;
    SceneLoadTest.Log("New map 1 from main menu");
    uniTask = SceneLoadTest.EditorNewMission(mapLoader.Maps[1]);
    await uniTask;
    uniTask = this.Wait();
    await uniTask;
    SceneLoadTest.Log("New map 1->1 from Editor");
    uniTask = SceneLoadTest.EditorNewMission(mapLoader.Maps[1]);
    await uniTask;
    uniTask = this.Wait();
    await uniTask;
    SceneLoadTest.Log("New map 1->0 from Editor");
    uniTask = SceneLoadTest.EditorNewMission(mapLoader.Maps[0]);
    await uniTask;
    uniTask = this.Wait();
    await uniTask;
    uniTask = SceneLoadTest.ExitEditor();
    await uniTask;
    SceneLoadTest.Log("Load map 0 from Menu");
    uniTask = SceneLoadTest.EditorLoadMission("custom_airbase");
    await uniTask;
    uniTask = this.Wait();
    await uniTask;
    uniTask = SceneLoadTest.ExitEditor();
    await uniTask;
    SceneLoadTest.Log("Load map 1 from Menu");
    uniTask = SceneLoadTest.EditorLoadMission("NavalMap");
    await uniTask;
    uniTask = this.Wait();
    await uniTask;
    uniTask = SceneLoadTest.ExitEditor();
    await uniTask;
    SceneLoadTest.Log("Reload same mission map 0 START");
    uniTask = SceneLoadTest.EditorLoadMission("custom_airbase");
    await uniTask;
    uniTask = this.Wait();
    await uniTask;
    SceneLoadTest.Log("Reload same mission map 0 SECOND LOAD");
    uniTask = SceneLoadTest.EditorLoadMission("custom_airbase");
    await uniTask;
    uniTask = this.Wait();
    await uniTask;
    SceneLoadTest.Log("Load map 1 from Editor");
    uniTask = SceneLoadTest.EditorLoadMission("NavalMap");
    await uniTask;
    uniTask = this.Wait();
    await uniTask;
    SceneLoadTest.Log("Load map 0 from Editor");
    uniTask = SceneLoadTest.EditorLoadMission("custom_airbase");
    await uniTask;
    uniTask = this.Wait();
    await uniTask;
    uniTask = SceneLoadTest.ExitEditor();
    await uniTask;
    SceneLoadTest.Log("End");
    mapLoader = (MapLoader) null;
  }

  private void AssertNoNullUnits()
  {
    foreach (UnityEngine.Object allUnit in UnitRegistry.allUnits)
    {
      if (allUnit == (UnityEngine.Object) null)
        SceneLoadTest.LogError("Null unit found in UnitRegistry");
    }
  }

  private static async UniTask ExitEditor()
  {
    SceneLoadTest.Log("ExitEditor Start");
    await MissionEditor.ExitEditor();
    YieldAwaitable yieldAwaitable;
    while (!(SceneManager.GetActiveScene().path == MapLoader.MainMenu))
    {
      SceneLoadTest.Log("ExitEditor Yield");
      yieldAwaitable = UniTask.Yield();
      await yieldAwaitable;
    }
    yieldAwaitable = UniTask.Yield();
    await yieldAwaitable;
  }

  private static async UniTask EditorNewMission(MapDetails mapDetails)
  {
    await MissionEditor.LoadEditor(NewMissionConfig.DefaultMission() with
    {
      Map = MapKey.GameWorldPrefab(mapDetails.PrefabName)
    });
  }

  private static async UniTask EditorLoadMission(string mapName)
  {
    Mission mission;
    string error;
    if (MissionSaveLoad.TryLoad(new MissionKey(mapName, (MissionGroup) MissionGroup.User), out mission, out error))
      await MissionEditor.LoadEditor(mission);
    else
      Debug.LogError((object) error);
  }

  private class RunChecker : MonoBehaviour
  {
    private void Awake() => SceneLoadTest.IsRunning = true;

    private void OnDestroy() => SceneLoadTest.IsRunning = false;
  }

  [Serializable]
  public class StopRunException : Exception
  {
  }
}
