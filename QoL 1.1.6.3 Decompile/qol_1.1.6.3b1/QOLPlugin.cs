// Decompiled with JetBrains decompiler
// Type: QOLPlugin
// Assembly: qol, Version=1.1.6.3, Culture=neutral, PublicKeyToken=null
// MVID: 3B6539EA-E38C-45EE-8FFF-FC58CC42BADC
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\BepInEx\plugins\qol_1.1.6.3b1.dll

using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using Mirage;
using Mirage.Serialization;
using NuclearOption.AddressableScripts;
using NuclearOption.Networking;
using NuclearOption.Networking.Lobbies;
using NuclearOption.NetworkTransforms;
using NuclearOption.SavedMission;
using qol.CommandProcessing.Core;
using qol.CommandProcessing.Handlers;
using qol.CommandProcessing.Processing;
using qol.FieldModification.Core;
using qol.FieldModification.Processors;
using qol.HarmonyPatches.Configs;
using qol.Multiplayer;
using qol.PatchConfig;
using qol.UI.Branding;
using qol.UI.Theming;
using qol.Utilities;
using qol.WeaponLoading.Core;
using qol.WeaponLoading.Modifiers.Balance;
using qol.WeaponLoading.Modifiers.Effects;
using qol.WeaponLoading.Modifiers.Materials;
using qol.WeaponLoading.Modifiers.Visual;
using qol.WeaponLoading.Modifiers.Weapons;
using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

#nullable disable
[BepInPlugin("com.offiry.qol", "qol", "1.1.6.3")]
[BepInDependency]
public class QOLPlugin : BaseUnityPlugin
{
  private CommandDispatcher _commandDispatcher;
  private CommandContext _commandContext;
  private ProcessingContext _processingContext;
  private FieldProcessorRegistry _fieldProcessorRegistry;
  private static StringBuilder diagnosticsBuffer = new StringBuilder();
  private static bool diagnosticsStarted = false;
  public static Dictionary<string, string> newAircraftName = new Dictionary<string, string>();
  public static string guid2 = "QoL";
  private static bool multiThreadingEnabled = false;
  private static bool notDedicatedServer = true;
  private static bool isBeta = false;
  private static Callback<LobbyDataUpdate_t> LobbyDataUpdateCallback;
  private static ManualLogSource logger;
  private static ManualLogSource mplogger;
  private static ManualLogSource kflogger;
  private string _pluginFolder;
  private Harmony harmony;
  private static ConfigEntry<bool> config_designations;
  private string primaryWhite = "#f5f5f5ff";
  private string secondaryWhite = "#f5f5f540";
  private string primaryGreen = "#58e187ff";
  private string neonGreen = "#98d793ff";
  public static string[] GLOCexcludedAircraft = new string[2]
  {
    "kestrel",
    "P_MiniDrone1"
  };
  public static Material newPilotMat;
  public static GameObject standardShockwave_GO;
  public static Material p_missiles1;
  public static string[] backgroundNames = new string[13]
  {
    "Cricket1b.jpg",
    "Compass2b.jpg",
    "Chicane1b.jpg",
    "Ibis2b.jpg",
    "Brawler1b.jpg",
    "Kestrel1b.jpg",
    "Revoker2b.jpg",
    "Vortex3b.jpg",
    "Ternion1b.jpg",
    "Ifrit1b.jpg",
    "Tarantula1b.jpg",
    "Medusa2b.jpg",
    "Darkreach1b.jpg"
  };
  public static Dictionary<string, string[]> aircraftDesignations = new Dictionary<string, string[]>()
  {
    {
      "CI-22",
      new string[2]{ "CI-22 Cricket", "CI-22" }
    },
    {
      "T/A-30",
      new string[2]{ "TA-30YH Compass", "TA-30YH" }
    },
    {
      "UH-90",
      new string[2]{ "UH-90K Ibis", "UH-90K" }
    },
    {
      "SAH-46",
      new string[2]{ "SAH-46L Chicane", "SAH-46L" }
    },
    {
      "A-19",
      new string[2]{ "A-19C Brawler", "A-19C" }
    },
    {
      "FS-12",
      new string[2]{ "FS-12V Revoker", "FS-12V" }
    },
    {
      "FS-20",
      new string[2]{ "FS-20B Vortex", "FS-20B" }
    },
    {
      "KR-67",
      new string[2]{ "KR-67A Ifrit", "KR-67A" }
    },
    {
      "VL-49",
      new string[2]{ "VL-49D Tarantula", "VL-49D" }
    },
    {
      "EW-25",
      new string[2]{ "EA-25B Medusa", "EA-25B" }
    },
    {
      "SFB-81",
      new string[2]{ "SFB-81 Darkreach", "SFB-81" }
    }
  };
  public static List<string> botNames = new List<string>();

  private void InitializeCommandDispatcher()
  {
    this._commandContext = new CommandContext(this, QOLPlugin.logger);
    this._commandDispatcher = new CommandDispatcher(QOLPlugin.logger);
    this._commandDispatcher.RegisterHandlers((ICommandHandler) new FieldModifyHandler(), (ICommandHandler) new HardpointSetHandler(), (ICommandHandler) new TransformHandler(), (ICommandHandler) new WeaponHandler(), (ICommandHandler) new MountModHandler(), (ICommandHandler) new AddComponentHandler(), (ICommandHandler) new LiveryNameHandler(), (ICommandHandler) new DefaultLoadoutHandler());
    this._processingContext = new ProcessingContext(QOLPlugin.logger, QOLPlugin.notDedicatedServer, QOLPlugin.multiThreadingEnabled, QOLPlugin.guid2, new Action<string>(this.ProcessCommandLine), new Func<Font>(this.GetBestFont), this.primaryWhite, this.primaryGreen, this.secondaryWhite);
  }

  private IEnumerator DelayedStart()
  {
    QOLPlugin qolPlugin = this;
    QOLPlugin.logger.LogInfo((object) "Initializing");
    if (QOLPlugin.notDedicatedServer)
      QOLPlugin.logger.LogInfo((object) "Loading client version");
    else
      QOLPlugin.logger.LogInfo((object) "Loading dedicated server version");
    if (QOLPlugin.notDedicatedServer)
      yield return (object) null;
    System.Type[] typeArray = new System.Type[4]
    {
      typeof (WeaponMount),
      typeof (AircraftParameters),
      typeof (HardpointSet),
      typeof (WeaponManager)
    };
    foreach (System.Type type in typeArray)
    {
      foreach (FieldInfo field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        ReflectionHelpers.GetField(type, field.Name);
    }
    qolPlugin.InitializeCommandDispatcher();
    if (QOLPlugin.notDedicatedServer)
      yield return (object) null;
    if (QOLPlugin.notDedicatedServer)
    {
      GameObject gameObject = PathLookup.Find("LoadingScreen");
      PathLookup.Find("LoadingScreen/background").GetComponent<Image>().GetComponent<AspectRatioFitter>().aspectRatio = 1.77777779f;
      LoadingScreen component = gameObject.GetComponent<LoadingScreen>();
      FieldInfo[] fields = component.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
      Sprite[] array = new Sprite[0];
      foreach (string backgroundName in QOLPlugin.backgroundNames)
      {
        Texture2D texture = BackgroundManager.LoadTextureFromResources(backgroundName);
        Sprite sprite = Sprite.Create(texture, new Rect(0.0f, 0.0f, (float) texture.width, (float) texture.height), new Vector2(0.5f, 0.5f));
        Array.Resize<Sprite>(ref array, array.Length + 1);
        array[array.Length - 1] = sprite;
      }
      fields[0].SetValue((object) component, (object) array);
    }
    if (QOLPlugin.notDedicatedServer)
      yield return (object) null;
    qolPlugin.ProcessConfigFileForModifications();
    if (QOLPlugin.notDedicatedServer)
      yield return (object) null;
    if (QOLPlugin.notDedicatedServer)
      ((MonoBehaviour) qolPlugin).StartCoroutine(qolPlugin.ModifyMenuOnceWhenReady());
  }

  public static string GetResourceHash()
  {
    return ConfigLoader.GetResourceHash(Assembly.GetExecutingAssembly());
  }

  private void ProcessConfigFileForModifications()
  {
    Assembly assembly = typeof (QOLPlugin).Assembly;
    if (!QOLPlugin.notDedicatedServer)
      StartProcessing();
    else
      QOLPlugin.ExecuteAfterFrame(new Action(StartProcessing));

    void StartProcessing()
    {
      try
      {
        string configContent = ConfigLoader.LoadEmbeddedConfig(assembly, QOLPlugin.logger);
        if (configContent == null)
          return;
        ((MonoBehaviour) this).StartCoroutine(!this._processingContext.MultiThreadingEnabled ? new SingleThreadProcessor(this._processingContext, "1.1.6.3").Process(configContent) : new ThreadedProcessor(this._processingContext).Process(configContent));
        QOLPlugin.logger.LogInfo((object) "Starting load process coroutine");
      }
      catch (Exception ex)
      {
        QOLPlugin.logger.LogError((object) $"Failed processing config: {ex}");
      }
    }
  }

  private void ProcessCommandLine(string line)
  {
    this._commandDispatcher.ProcessLine(line, this._commandContext);
  }

  public IEnumerator CreateConfigProcessor(string configContent)
  {
    if (this._commandDispatcher == null)
      this.InitializeCommandDispatcher();
    return new SingleThreadProcessor(this._processingContext, "1.1.6.3").Process(configContent);
  }

  public FieldProcessorRegistry FieldProcessorRegistry
  {
    get
    {
      if (this._fieldProcessorRegistry == null)
        this.InitializeFieldProcessorRegistry();
      return this._fieldProcessorRegistry;
    }
  }

  private void InitializeFieldProcessorRegistry()
  {
    this._fieldProcessorRegistry = new FieldProcessorRegistry();
    this._fieldProcessorRegistry.Register((IFieldProcessor) new VectorProcessor());
    this._fieldProcessorRegistry.Register((IFieldProcessor) new ColorProcessor());
    this._fieldProcessorRegistry.Register((IFieldProcessor) new ObjectProcessor());
    this._fieldProcessorRegistry.Register((IFieldProcessor) new AudioProcessor());
    this._fieldProcessorRegistry.Register((IFieldProcessor) new ActiveProcessor());
    this._fieldProcessorRegistry.Register((IFieldProcessor) new ShipProcessor());
    this._fieldProcessorRegistry.Register((IFieldProcessor) new PartProcessor());
    this._fieldProcessorRegistry.Register((IFieldProcessor) new VehicleProcessor());
    this._fieldProcessorRegistry.Register((IFieldProcessor) new AircraftProcessor());
    this._fieldProcessorRegistry.Register((IFieldProcessor) new MissileDefinitionProcessor());
    this._fieldProcessorRegistry.Register((IFieldProcessor) new WeaponInfoProcessor());
    this._fieldProcessorRegistry.Register((IFieldProcessor) new WeaponInfoEditProcessor());
    this._fieldProcessorRegistry.Register((IFieldProcessor) new StandardProcessor());
    QOLPlugin.logger.LogDebug((object) $"Initialized {this._fieldProcessorRegistry.Count} field processors");
  }

  private static string DiagnosticsPath
  {
    get => Path.Combine(Paths.BepInExRootPath, "LoadoutDiagnostics.txt");
  }

  private static void WriteDiagnostics(string message)
  {
    if (!QOLPlugin.diagnosticsStarted)
    {
      QOLPlugin.diagnosticsStarted = true;
      QOLPlugin.diagnosticsBuffer.Clear();
      QOLPlugin.diagnosticsBuffer.AppendLine("=".PadRight(80 /*0x50*/, '='));
      QOLPlugin.diagnosticsBuffer.AppendLine($"LOADOUT DIAGNOSTICS - {DateTime.Now}");
      QOLPlugin.diagnosticsBuffer.AppendLine("=".PadRight(80 /*0x50*/, '='));
      QOLPlugin.diagnosticsBuffer.AppendLine();
    }
    QOLPlugin.diagnosticsBuffer.AppendLine(message);
  }

  private static void FlushDiagnostics()
  {
    if (QOLPlugin.diagnosticsBuffer.Length <= 0)
      return;
    try
    {
      File.AppendAllText(QOLPlugin.DiagnosticsPath, QOLPlugin.diagnosticsBuffer.ToString());
      QOLPlugin.diagnosticsBuffer.Clear();
      QOLPlugin.logger.LogInfo((object) ("[Diagnostics] Written to " + QOLPlugin.DiagnosticsPath));
    }
    catch (Exception ex)
    {
      QOLPlugin.logger.LogError((object) ("[Diagnostics] Failed to write: " + ex.Message));
    }
  }

  [HarmonyPostfix]
  [HarmonyPatch(typeof (PilotDismounted), "Setup")]
  public static void PilotDismountedPatch_PostfixSetup(PilotDismounted __instance)
  {
    try
    {
      if ((UnityEngine.Object) __instance.NetworkHQ == (UnityEngine.Object) null || (UnityEngine.Object) __instance.NetworkHQ.faction == (UnityEngine.Object) null)
      {
        QOLPlugin.logger.LogWarning((object) "NetworkHQ or faction was null, exiting");
      }
      else
      {
        if (__instance.NetworkHQ.faction.name != "Primeva")
          return;
        if ((UnityEngine.Object) QOLPlugin.newPilotMat == (UnityEngine.Object) null)
        {
          QOLPlugin.logger.LogWarning((object) "New pilot material not initialized!");
        }
        else
        {
          foreach (SkinnedMeshRenderer componentsInChild in __instance.GetComponentsInChildren<SkinnedMeshRenderer>(true))
          {
            if ((UnityEngine.Object) componentsInChild.sharedMaterial != (UnityEngine.Object) null && componentsInChild.sharedMaterial.name == "pilot")
            {
              componentsInChild.sharedMaterial = QOLPlugin.newPilotMat;
              QOLPlugin.logger.LogInfo((object) ("Replaced pilot material on ejected pilot " + componentsInChild.gameObject.name));
            }
          }
        }
      }
    }
    catch (Exception ex)
    {
      QOLPlugin.logger.LogError((object) ("Error in PostfixSetup: " + ex.Message));
    }
  }

  public static bool IsModdedClient => true;

  private void InitializeMultiplayer()
  {
    CoroutineRunner.Initialize(QOLPlugin.mplogger);
    LobbyHelper.Initialize(QOLPlugin.mplogger);
    ModVerificationManager.Initialize(QOLPlugin.mplogger);
  }

  private static IEnumerator WaitForMemberDataCallback(ulong steamId, float maxWaitTime = 10f)
  {
    float startTime = Time.time;
    bool callbackReceived = false;
    while ((double) Time.time - (double) startTime < (double) maxWaitTime && !callbackReceived)
    {
      if (!ModVerificationManager.IsVerificationPending(steamId))
      {
        callbackReceived = true;
        break;
      }
      yield return (object) new WaitForSeconds(0.5f);
    }
    if (!callbackReceived)
      QOLPlugin.mplogger.LogError((object) $"[CallbackWait] Timeout waiting for member data callback for SteamID: {steamId}");
  }

  private void OnLobbyDataUpdated(LobbyDataUpdate_t callback)
  {
    QOLPlugin.mplogger.LogWarning((object) $"[LobbyDataCallback] Lobby data updated - LobbyID: {callback.m_ulSteamIDLobby}, MemberID: {callback.m_ulSteamIDMember}, Success: {callback.m_bSuccess}");
    CSteamID steamIDLobby = new CSteamID(callback.m_ulSteamIDLobby);
    CSteamID steamIDUser = new CSteamID(callback.m_ulSteamIDMember);
    if (callback.m_bSuccess == (byte) 1)
    {
      if (steamIDUser == SteamUser.GetSteamID())
      {
        string lobbyMemberData = SteamMatchmaking.GetLobbyMemberData(steamIDLobby, steamIDUser, "qol");
        QOLPlugin.mplogger.LogWarning((object) ("[LobbyDataCallback] Self member data updated - qol = " + lobbyMemberData));
        ModVerificationManager.OnMemberDataUpdated(steamIDUser.m_SteamID, true);
      }
      else if (steamIDUser.IsValid())
      {
        string lobbyMemberData = SteamMatchmaking.GetLobbyMemberData(steamIDLobby, steamIDUser, "qol");
        QOLPlugin.mplogger.LogWarning((object) $"[LobbyDataCallback] Player ({steamIDUser}) data updated - {"qol"} = {lobbyMemberData}");
      }
      else
      {
        string lobbyData = SteamMatchmaking.GetLobbyData(steamIDLobby, "qol");
        QOLPlugin.mplogger.LogWarning((object) ("[LobbyDataCallback] Lobby data updated - qol = " + lobbyData));
      }
    }
    else
    {
      QOLPlugin.mplogger.LogError((object) $"[LobbyDataCallback] Lobby data update FAILED - LobbyID: {steamIDLobby.m_SteamID}, MemberID: {steamIDUser.m_SteamID}");
      if (!(steamIDUser == SteamUser.GetSteamID()))
        return;
      ModVerificationManager.OnMemberDataUpdated(steamIDUser.m_SteamID, false);
    }
  }

  private void InitializeUI()
  {
    MenuThemer.Initialize(QOLPlugin.logger);
    VersionWatermark.Initialize(QOLPlugin.logger, QOLPlugin.isBeta, QOLPlugin.guid2);
    BackgroundManager.Initialize(QOLPlugin.logger, QOLPlugin.backgroundNames);
  }

  private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
  {
    this.Logger.LogInfo((object) $"OnSceneLoaded called to scene={scene} name={scene.name} mode={mode}");
    if ((UnityEngine.Object) this == (UnityEngine.Object) null)
    {
      this.Logger.LogError((object) "Plugin instance is null!");
    }
    else
    {
      if (QOLPlugin.notDedicatedServer)
      {
        ((MonoBehaviour) this).StartCoroutine(VersionWatermark.AddWhenReady(QOLPlugin.notDedicatedServer, new Func<Font>(this.GetBestFont)));
        ((MonoBehaviour) this).StartCoroutine(MenuThemer.ApplySceneTheme(QOLPlugin.notDedicatedServer));
        BackgroundManager.ReplaceBackgroundImage();
      }
      PathLookup.ClearCache();
    }
  }

  private IEnumerator ModifyMenuOnceWhenReady()
  {
    return MenuThemer.ApplyOneTimeTheme(QOLPlugin.notDedicatedServer);
  }

  private IEnumerator ModifyMenuWhenReady()
  {
    return MenuThemer.ApplySceneTheme(QOLPlugin.notDedicatedServer);
  }

  private Font GetBestFont()
  {
    foreach (Font bestFont in UnityEngine.Resources.FindObjectsOfTypeAll<Font>())
    {
      if (bestFont.name == "BrassMono-Regular" || bestFont.name == "Barlow-Regular")
        return bestFont;
    }
    return Font.CreateDynamicFontFromOSFont("Arial", 13) ?? UnityEngine.Resources.GetBuiltinResource<Font>("Arial.ttf");
  }

  public static void LoadCustomWeapons(Encyclopedia __instance)
  {
    if (true)
      UnityEngine.Debug.unityLogger.logEnabled = false;
    Shader shader = Shader.Find("Universal Render Pipeline/Lit");
    Assembly executingAssembly = Assembly.GetExecutingAssembly();
    QOLPlugin.p_missiles1 = new Material(shader);
    QOLPlugin.p_missiles1.mainTexture = (Texture) QOLPlugin.LoadTextureFromResource(executingAssembly.GetName().Name + ".Resources.P_Missiles1.p_missiles1_b.jpg");
    ModificationRegistry registry = new ModificationRegistry();
    ModificationContext context = new ModificationContext(__instance, registry, QOLPlugin.logger);
    IEntityModifier[] array = new IEntityModifier[39]
    {
      (IEntityModifier) new WeaponVariantModifier(),
      (IEntityModifier) new GlassMaterialModifier(),
      (IEntityModifier) new NavLightMaterialModifier(),
      (IEntityModifier) new ExplosionParticleModifier(),
      (IEntityModifier) new ParticleEmissionModifier(),
      (IEntityModifier) new ShipArmorModifier(),
      (IEntityModifier) new TargetDetectorModifier(),
      (IEntityModifier) new WaterSplashModifier(),
      (IEntityModifier) new MissileSmokeTrailModifier(),
      (IEntityModifier) new FlareEffectModifier(),
      (IEntityModifier) new RadarParametersModifier(),
      (IEntityModifier) new PropFanEfficiencyModifier(),
      (IEntityModifier) new CorvetteBalanceModifier(),
      (IEntityModifier) new ECMPodInfoModifier(),
      (IEntityModifier) new FireDamageModifier(),
      (IEntityModifier) new DecalSizeModifier(),
      (IEntityModifier) new AircraftSlingLoadModifier(),
      (IEntityModifier) new HeloRadarJammerModifier(),
      (IEntityModifier) new HorseMeshModifier(),
      (IEntityModifier) new RailgunParticleModifier(),
      (IEntityModifier) new MedusaAirbrakeModifier(),
      (IEntityModifier) new AnnexArrestorModifier(),
      (IEntityModifier) new ShardFireControlModifier(),
      (IEntityModifier) new MBTEjectionModifier(),
      (IEntityModifier) new EW1LaserModifier(),
      (IEntityModifier) new StorageTankExplosionModifier(),
      (IEntityModifier) new RailcannonPodModifier(),
      (IEntityModifier) new AttackHeloTurretModifier(),
      (IEntityModifier) new AssaultCarrierLaserTurretModifier(),
      (IEntityModifier) new CruiseMissileBoosterModifier(),
      (IEntityModifier) new AAM3QuadExtraModifier(),
      (IEntityModifier) new AAM4SeekerModifier(),
      (IEntityModifier) new ChicaneGunFixModifier(),
      (IEntityModifier) new LCACTurretModifier(),
      (IEntityModifier) new PylonRenameModifier(),
      (IEntityModifier) new UH1PilotSwapModifier(),
      (IEntityModifier) new MunitionsPalletRenameModifier(),
      (IEntityModifier) new WeaponIconModifier(),
      (IEntityModifier) new AircraftLoadoutModifier()
    };
    Array.Sort<IEntityModifier>(array, (Comparison<IEntityModifier>) ((a, b) => a.Priority.CompareTo(b.Priority)));
    foreach (IEntityModifier entityModifier in array)
    {
      if (entityModifier.CanApply(context))
        entityModifier.Apply(context);
    }
    PathLookup.ClearCache();
    QOLPlugin.logger.LogInfo((object) "[LoadCustomWeapons] All modifiers complete, PathLookup cache cleared");
    if (QOLPlugin.config_designations.Value)
    {
      foreach (Aircraft aircraft in UnityEngine.Resources.FindObjectsOfTypeAll<Aircraft>())
      {
        string[] strArray;
        if (QOLPlugin.aircraftDesignations.TryGetValue(aircraft.definition.code, out strArray))
        {
          QOLPlugin.newAircraftName[aircraft.definition.unitName] = strArray[0];
          aircraft.definition.unitName = strArray[0];
          aircraft.definition.code = strArray[1];
        }
      }
    }
    foreach (object obj in UnityEngine.Resources.FindObjectsOfTypeAll<Turret>())
    {
      Traverse traverse = Traverse.Create(obj);
      if ((double) (float) traverse.Field("targetAssessmentInterval").GetValue() < 1.0)
        traverse.Field("targetAssessmentInterval").SetValue((object) 1f);
    }
    MissileDefinition definition1 = PathLookup.Find("P_GLB1").GetComponent<Missile>().definition as MissileDefinition;
    MissileDefinition definition2 = PathLookup.Find("bomb_500_glide").GetComponent<Missile>().definition as MissileDefinition;
    definition1.dontAutomaticallyAddToEncyclopedia = false;
    definition1.disabled = false;
    definition2.dontAutomaticallyAddToEncyclopedia = false;
    definition2.disabled = false;
    GameObject duplicateMount1 = PathLookup.Find("AttackHelo1/cockpit_R/cockpit_F/turretMount/turret", false);
    if ((UnityEngine.Object) duplicateMount1 == (UnityEngine.Object) null)
    {
      context.Logger.LogWarning((object) "[WeaponLoading] AttackHelo1 turret not found");
    }
    else
    {
      duplicateMount1.transform.SetParent((Transform) null);
      duplicateMount1.name = "turret_30mm_laser";
      Traverse traverse1 = Traverse.Create((object) duplicateMount1.GetComponent<Turret>());
      traverse1.Field("attachedUnit").SetValue((object) null);
      traverse1.Field("firingCones").SetValue((object) new FiringCone[0]);
      Material material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
      material.mainTexture = (Texture) QOLPlugin.LoadTextureFromResource(Assembly.GetExecutingAssembly().GetName().Name + ".Resources.FBXMod.attackHelo1_turret.png");
      duplicateMount1.GetComponent<MeshRenderer>().sharedMaterials = new Material[1]
      {
        material
      };
      duplicateMount1.GetComponent<MeshRenderer>().enabled = false;
      GameObject gameObject1 = UnityEngine.Object.Instantiate<GameObject>(PathLookup.Find("Laser_EW1"));
      gameObject1.name = "Laser_EW1_turreted";
      WeaponStation[] weaponStationArray = traverse1.Field("weaponStations").GetValue<WeaponStation[]>();
      weaponStationArray[0].Weapons = new List<Weapon>()
      {
        (Weapon) gameObject1.GetComponent<Laser>()
      };
      traverse1.Field("weaponStations").SetValue((object) weaponStationArray);
      traverse1.Field("targetDetectors").SetValue((object) new List<TargetDetector>());
      GameObject gameObject2 = PathLookup.Find("turret_30mm_laser/gun", false);
      GameObject gameObject3 = PathLookup.Find("turret_30mm_laser/gun/barrel", false);
      gameObject1.transform.position = gameObject2.transform.position;
      gameObject1.transform.SetParent(gameObject2.transform);
      UnityEngine.Object.Destroy((UnityEngine.Object) gameObject2.GetComponent<Gun>());
      UnityEngine.Object.Destroy((UnityEngine.Object) PathLookup.Find("turret_30mm_laser/gun/barrel/muzzle"));
      WeaponInfo info1 = QOLPlugin.DuplicateWeaponInfo("Laser_EW1", "Laser_EW1_turret_info", (GameObject) null);
      gameObject1.GetComponent<Laser>().info = info1;
      WeaponMount mount1 = QOLPlugin.DuplicateWeaponMount("Laser_EW1", "Laser_EW1_turreted", duplicateMount1, info1);
      QOLPlugin.AddMountToEncyclopedia(__instance, "Laser_EW1_turreted", mount1);
      using (Stream manifestResourceStream = executingAssembly.GetManifestResourceStream(executingAssembly.GetName().Name + ".Resources.FBXMod.TurretLaserBarrel.bundle"))
      {
        using (MemoryStream destination = new MemoryStream())
        {
          manifestResourceStream.CopyTo((Stream) destination);
          AssetBundle assetBundle = AssetBundle.LoadFromMemory(destination.ToArray());
          GameObject[] gameObjectArray = assetBundle.LoadAllAssets<GameObject>();
          if (gameObjectArray.Length == 0)
          {
            assetBundle.Unload(false);
            return;
          }
          MeshFilter component = gameObjectArray[0].GetComponent<MeshFilter>();
          gameObject3.GetComponent<MeshFilter>().mesh = component.mesh;
          gameObject3.GetComponent<MeshRenderer>().sharedMaterials = new Material[3]
          {
            PathLookup.Find("EW1").GetComponent<MeshRenderer>().sharedMaterials[0],
            PathLookup.Find("EW1/fuselage_F/cockpit_R/cockpit_F/canopyFrame/canopy").GetComponent<MeshRenderer>().sharedMaterials[0],
            material
          };
          assetBundle.Unload(false);
        }
      }
      PathLookup.Find("CAS1/fuselage_F/cannon", false).name = "cannon1";
      PathLookup.Find("CAS1/fuselage_F/cannon", false).name = "cannon2";
      GameObject gameObject4 = new GameObject();
      gameObject4.transform.SetParent(PathLookup.Find("Fighter1/fuselage/cockpit/cheek_L/canard_L").transform);
      gameObject4.transform.position = PathLookup.Find("Fighter1/fuselage/cockpit/cheek_L/canard_L/canard_L_visible").transform.position;
      PathLookup.Find("Fighter1/fuselage/cockpit/cheek_L/canard_L/canard_L_visible").transform.SetParent(gameObject4.transform);
      GameObject gameObject5 = new GameObject();
      gameObject5.transform.SetParent(PathLookup.Find("Fighter1/fuselage/cockpit/cheek_R/canard_R").transform);
      gameObject5.transform.position = PathLookup.Find("Fighter1/fuselage/cockpit/cheek_R/canard_R/canard_R_visible").transform.position;
      PathLookup.Find("Fighter1/fuselage/cockpit/cheek_R/canard_R/canard_R_visible").transform.SetParent(gameObject5.transform);
      Material sharedMaterial1 = PathLookup.Find("gun_12.7mm_pod/gunpod").GetComponent<MeshRenderer>().sharedMaterial;
      Material sharedMaterial2 = PathLookup.Find("EW1").GetComponent<MeshRenderer>().sharedMaterials[1];
      using (Stream manifestResourceStream = executingAssembly.GetManifestResourceStream(executingAssembly.GetName().Name + ".Resources.P_Pods1.P_WeaponsPod1.bundle"))
      {
        using (MemoryStream destination = new MemoryStream())
        {
          manifestResourceStream.CopyTo((Stream) destination);
          AssetBundle assetBundle = AssetBundle.LoadFromMemory(destination.ToArray());
          if (assetBundle.LoadAllAssets().Length == 0)
          {
            assetBundle.Unload(false);
            return;
          }
          assetBundle.Unload(false);
        }
      }
      GameObject gameObject6 = PathLookup.Find("P_WeaponsPod1");
      gameObject6.transform.localScale = Vector3.one;
      List<UnityEngine.Renderer> rendererList = new List<UnityEngine.Renderer>();
      foreach (Transform componentsInChild in gameObject6.GetComponentsInChildren<Transform>())
      {
        if (componentsInChild.name.Contains("door"))
        {
          MeshRenderer component = componentsInChild.gameObject.GetComponent<MeshRenderer>();
          if ((UnityEngine.Object) component != (UnityEngine.Object) null)
          {
            QOLPlugin.logger.LogDebug((object) ("Setting up BayDoor for " + componentsInChild.name));
            rendererList.Add((UnityEngine.Renderer) component);
            component.sharedMaterials = new Material[2]
            {
              sharedMaterial1,
              sharedMaterial2
            };
            Traverse traverse2 = Traverse.Create((object) componentsInChild.gameObject.AddComponent<BayDoor>());
            traverse2.Field("hingeAngle").SetValue((object) -110f);
            traverse2.Field("openSpeed").SetValue((object) 5f);
            traverse2.Field("closeSpeed").SetValue((object) 2f);
          }
        }
        else
        {
          MeshRenderer component = componentsInChild.gameObject.GetComponent<MeshRenderer>();
          if ((UnityEngine.Object) component != (UnityEngine.Object) null)
          {
            rendererList.Add((UnityEngine.Renderer) component);
            component.sharedMaterials = new Material[2]
            {
              sharedMaterial1,
              sharedMaterial2
            };
          }
        }
      }
      Traverse.Create((object) gameObject6.AddComponent<ColorableMount>()).Field("colorableRenderers").SetValue((object) rendererList.ToArray());
      GameObject duplicateMount2 = QOLPlugin.DuplicatePrefab("P_WeaponsPod1", "P_AAM2_double_pod");
      GameObject gameObject7 = PathLookup.Find("P_AAM2_double_pod/hardpoint");
      GameObject gameObject8 = UnityEngine.Object.Instantiate<GameObject>(PathLookup.Find("AAM2_double_internal"));
      gameObject8.transform.SetParent(gameObject7.transform);
      gameObject8.transform.localPosition = Vector3.zero;
      gameObject8.name = "aams";
      BayDoor[] componentsInChildren1 = duplicateMount2.GetComponentsInChildren<BayDoor>();
      foreach (object componentsInChild in duplicateMount2.GetComponentsInChildren<MountedMissile>())
        Traverse.Create(componentsInChild).Field("bayDoors").SetValue((object) componentsInChildren1);
      WeaponMount mount2 = QOLPlugin.DuplicateWeaponMount("AAM2_double_internal", "P_AAM2_double_pod", duplicateMount2, QOLPlugin.GetMount("AAM2_double_internal").info);
      QOLPlugin.AddMountToEncyclopedia(__instance, "P_AAM2_double_pod", mount2);
      GameObject duplicateMount3 = QOLPlugin.DuplicatePrefab("P_WeaponsPod1", "P_AAM4_double_pod");
      GameObject gameObject9 = PathLookup.Find("P_AAM4_double_pod/hardpoint");
      GameObject gameObject10 = UnityEngine.Object.Instantiate<GameObject>(PathLookup.Find("AAM4_double_internal"));
      gameObject10.transform.SetParent(gameObject9.transform);
      gameObject10.transform.localPosition = Vector3.zero;
      gameObject10.name = "aams";
      BayDoor[] componentsInChildren2 = duplicateMount3.GetComponentsInChildren<BayDoor>();
      foreach (object componentsInChild in duplicateMount3.GetComponentsInChildren<MountedMissile>())
        Traverse.Create(componentsInChild).Field("bayDoors").SetValue((object) componentsInChildren2);
      WeaponMount mount3 = QOLPlugin.DuplicateWeaponMount("AAM4_double_internal", "P_AAM4_double_pod", duplicateMount3, QOLPlugin.GetMount("AAM4_double_internal").info);
      QOLPlugin.AddMountToEncyclopedia(__instance, "P_AAM4_double_pod", mount3);
      GameObject duplicateMount4 = QOLPlugin.DuplicatePrefab("P_WeaponsPod1", "P_AAM1_double_pod");
      GameObject gameObject11 = PathLookup.Find("P_AAM1_double_pod/hardpoint");
      GameObject gameObject12 = UnityEngine.Object.Instantiate<GameObject>(PathLookup.Find("AAM1_double_internal"));
      gameObject12.transform.SetParent(gameObject11.transform);
      gameObject12.transform.localPosition = Vector3.zero;
      gameObject12.name = "aams";
      BayDoor[] componentsInChildren3 = duplicateMount4.GetComponentsInChildren<BayDoor>();
      foreach (object componentsInChild in duplicateMount4.GetComponentsInChildren<MountedMissile>())
        Traverse.Create(componentsInChild).Field("bayDoors").SetValue((object) componentsInChildren3);
      WeaponMount mount4 = QOLPlugin.DuplicateWeaponMount("AAM1_double_internal", "P_AAM1_double_pod", duplicateMount4, QOLPlugin.GetMount("AAM1_double_internal").info);
      QOLPlugin.AddMountToEncyclopedia(__instance, "P_AAM1_double_pod", mount4);
      GameObject duplicateMount5 = QOLPlugin.DuplicatePrefab("P_WeaponsPod1", "P_AGM_heavy_single_pod");
      GameObject gameObject13 = PathLookup.Find("P_AGM_heavy_single_pod/hardpoint");
      GameObject gameObject14 = UnityEngine.Object.Instantiate<GameObject>(PathLookup.Find("AGM_heavy_internal"));
      gameObject14.transform.SetParent(gameObject13.transform);
      gameObject14.transform.localPosition = Vector3.zero;
      gameObject14.name = "agm";
      BayDoor[] componentsInChildren4 = duplicateMount5.GetComponentsInChildren<BayDoor>();
      foreach (object componentsInChild in duplicateMount5.GetComponentsInChildren<MountedMissile>())
        Traverse.Create(componentsInChild).Field("bayDoors").SetValue((object) componentsInChildren4);
      WeaponMount mount5 = QOLPlugin.DuplicateWeaponMount("AGM_heavy_internal", "P_AGM_heavy_single_pod", duplicateMount5, QOLPlugin.GetMount("AGM_heavy_internal").info);
      QOLPlugin.AddMountToEncyclopedia(__instance, "P_AGM_heavy_single_pod", mount5);
      GameObject duplicateMount6 = QOLPlugin.DuplicatePrefab("P_WeaponsPod1", "P_bomb_250_double_pod");
      GameObject gameObject15 = PathLookup.Find("P_bomb_250_double_pod/hardpoint");
      GameObject gameObject16 = UnityEngine.Object.Instantiate<GameObject>(PathLookup.Find("bomb_250_internalx2"));
      gameObject16.transform.SetParent(gameObject15.transform);
      gameObject16.transform.localPosition = Vector3.zero;
      gameObject16.name = "bombs";
      BayDoor[] componentsInChildren5 = duplicateMount6.GetComponentsInChildren<BayDoor>();
      foreach (object componentsInChild in duplicateMount6.GetComponentsInChildren<MountedMissile>())
        Traverse.Create(componentsInChild).Field("bayDoors").SetValue((object) componentsInChildren5);
      WeaponMount mount6 = QOLPlugin.DuplicateWeaponMount("bomb_250_internalx2", "P_bomb_250_double_pod", duplicateMount6, QOLPlugin.GetMount("bomb_250_internalx2").info);
      QOLPlugin.AddMountToEncyclopedia(__instance, "P_bomb_250_double_pod", mount6);
      GameObject duplicateMount7 = QOLPlugin.DuplicatePrefab("P_WeaponsPod1", "P_bomb_glide1_double_pod");
      GameObject gameObject17 = PathLookup.Find("P_bomb_glide1_double_pod/hardpoint");
      GameObject gameObject18 = UnityEngine.Object.Instantiate<GameObject>(PathLookup.Find("bomb_glide1_double_internal"));
      gameObject18.transform.SetParent(gameObject17.transform);
      gameObject18.transform.localPosition = Vector3.zero;
      gameObject18.name = "bombs";
      BayDoor[] componentsInChildren6 = duplicateMount7.GetComponentsInChildren<BayDoor>();
      foreach (object componentsInChild in duplicateMount7.GetComponentsInChildren<MountedMissile>())
        Traverse.Create(componentsInChild).Field("bayDoors").SetValue((object) componentsInChildren6);
      WeaponMount mount7 = QOLPlugin.DuplicateWeaponMount("bomb_glide1_double_internal", "P_bomb_glide1_double_pod", duplicateMount7, QOLPlugin.GetMount("bomb_glide1_double_internal").info);
      QOLPlugin.AddMountToEncyclopedia(__instance, "P_bomb_glide1_double_pod", mount7);
      GameObject duplicateMount8 = QOLPlugin.DuplicatePrefab("P_WeaponsPod1", "P_AGM1_triple_pod");
      GameObject gameObject19 = PathLookup.Find("P_AGM1_triple_pod/hardpoint");
      GameObject gameObject20 = UnityEngine.Object.Instantiate<GameObject>(PathLookup.Find("AGM1_triple"));
      gameObject20.transform.SetParent(gameObject19.transform);
      gameObject20.transform.localPosition = Vector3.zero;
      gameObject20.name = "aams";
      BayDoor[] componentsInChildren7 = duplicateMount8.GetComponentsInChildren<BayDoor>();
      foreach (object componentsInChild in duplicateMount8.GetComponentsInChildren<MountedMissile>())
        Traverse.Create(componentsInChild).Field("bayDoors").SetValue((object) componentsInChildren7);
      WeaponMount mount8 = QOLPlugin.DuplicateWeaponMount("AGM1_triple", "P_AGM1_triple_pod", duplicateMount8, QOLPlugin.GetMount("AGM1_triple").info);
      QOLPlugin.AddMountToEncyclopedia(__instance, "P_AGM1_triple_pod", mount8);
      GameObject duplicateMount9 = QOLPlugin.DuplicatePrefab("P_WeaponsPod1", "P_bomb_500_pod");
      GameObject gameObject21 = PathLookup.Find("P_bomb_500_pod/hardpoint");
      GameObject gameObject22 = UnityEngine.Object.Instantiate<GameObject>(PathLookup.Find("bomb_500_internal"));
      gameObject22.transform.SetParent(gameObject21.transform);
      gameObject22.transform.localPosition = Vector3.zero;
      gameObject22.name = "aams";
      BayDoor[] componentsInChildren8 = duplicateMount9.GetComponentsInChildren<BayDoor>();
      foreach (object componentsInChild in duplicateMount9.GetComponentsInChildren<MountedMissile>())
        Traverse.Create(componentsInChild).Field("bayDoors").SetValue((object) componentsInChildren8);
      WeaponMount mount9 = QOLPlugin.DuplicateWeaponMount("bomb_500_internal", "P_bomb_500_pod", duplicateMount9, QOLPlugin.GetMount("bomb_500_internal").info);
      QOLPlugin.AddMountToEncyclopedia(__instance, "P_bomb_500_pod", mount9);
      UnityEngine.Object.Destroy((UnityEngine.Object) PathLookup.Find("RocketPod1_triple_db/pod (2)"));
      GameObject gameObject23 = PathLookup.Find("gun_155mm_pod_P/recoil");
      Material sharedMaterial3 = gameObject23.GetComponent<MeshRenderer>().sharedMaterial;
      using (Stream manifestResourceStream = executingAssembly.GetManifestResourceStream(executingAssembly.GetName().Name + ".Resources.P_Pods1.CAS1_40mmRailcannon_barrel.bundle"))
      {
        using (MemoryStream destination = new MemoryStream())
        {
          manifestResourceStream.CopyTo((Stream) destination);
          AssetBundle assetBundle = AssetBundle.LoadFromMemory(destination.ToArray());
          GameObject[] gameObjectArray = assetBundle.LoadAllAssets<GameObject>();
          if (gameObjectArray.Length == 0)
          {
            assetBundle.Unload(false);
            return;
          }
          MeshFilter component = gameObjectArray[0].GetComponent<MeshFilter>();
          gameObject23.GetComponent<MeshFilter>().mesh = component.mesh;
          gameObject23.GetComponent<MeshRenderer>().sharedMaterials = new Material[1]
          {
            sharedMaterial3
          };
          assetBundle.Unload(false);
        }
      }
      WeaponInfo weaponInfo1 = QOLPlugin.DuplicateWeaponInfo("AShM2_info", "AShM2_VLS_info", PathLookup.Find("AShM2"));
      weaponInfo1.weaponName = "RGM-98";
      weaponInfo1.shortName = "RGM-98";
      weaponInfo1.targetRequirements.maxRange = 30000f;
      GameObject gameObject24 = PathLookup.Find("Darkreach/fuselage_F/cockpit/cockpit_int");
      GameObject gameObject25 = QOLPlugin.LoadFirstAssetFromBundle<GameObject>(QOLPlugin.GetBundlePath("FBXMod.DarkreachNew2.bundle"));
      if ((UnityEngine.Object) gameObject25 == (UnityEngine.Object) null)
        return;
      MeshFilter component1 = gameObject25.GetComponent<MeshFilter>();
      MeshRenderer component2 = gameObject25.GetComponent<MeshRenderer>();
      gameObject24.GetComponent<MeshFilter>().mesh = component1.mesh;
      Material[] destinationArray = new Material[3];
      Array.Copy((Array) gameObject24.GetComponent<MeshRenderer>().sharedMaterials, (Array) destinationArray, 2);
      destinationArray[2] = component2.sharedMaterials[2];
      gameObject24.GetComponent<MeshRenderer>().sharedMaterials = destinationArray;
      Texture2D texture2D = QOLPlugin.LoadTextureFromResource(executingAssembly.GetName().Name + ".Resources.FBXMod.mug_texture.png");
      if ((UnityEngine.Object) texture2D != (UnityEngine.Object) null)
      {
        gameObject24.GetComponent<MeshRenderer>().sharedMaterials[2].color = new Color(1f, 1f, 1f, 1f);
        gameObject24.GetComponent<MeshRenderer>().sharedMaterials[2].mainTexture = (Texture) texture2D;
      }
      PathLookup.Find("AssaultCarrier1/hull_L/hull_FR/tower_F1/tower_F2/missile_turret_F");
      GameObject duplicateMount10 = QOLPlugin.DuplicatePrefab("UGV1_grenadex1", "UGV1_mixed_x6");
      WeaponInfo weaponInfo2 = ResourceLookup.FindWeaponInfo("UGV1_grenadex1_info");
      WeaponMount mount10 = QOLPlugin.DuplicateWeaponMount("UGV1_grenadex1", "UGV1_mixed_x6", duplicateMount10, weaponInfo2, ResourceLookup.FindVehicleDefinition("UGV1_grenade"));
      GameObject gameObject26 = UnityEngine.Object.Instantiate<GameObject>(PathLookup.Find("UGV1_grenadex1/UGV1_grenade"));
      gameObject26.name = "UGV1_grenade_1";
      GameObject gameObject27 = UnityEngine.Object.Instantiate<GameObject>(PathLookup.Find("UGV1_AT_Px1/UGV1_SAM"));
      gameObject27.name = "UGV1_grenade_2";
      GameObject gameObject28 = UnityEngine.Object.Instantiate<GameObject>(PathLookup.Find("UGV1_AT_Px1/UGV1_SAM"));
      gameObject28.name = "UGV1_grenade_3";
      GameObject gameObject29 = UnityEngine.Object.Instantiate<GameObject>(PathLookup.Find("UGV1_SAMx1/UGV1_SAM"));
      gameObject29.name = "UGV1_SAM_1";
      GameObject gameObject30 = UnityEngine.Object.Instantiate<GameObject>(PathLookup.Find("UGV1_SAMx1/UGV1_SAM"));
      gameObject30.name = "UGV1_SAM_2";
      gameObject26.transform.SetParent(duplicateMount10.transform);
      gameObject27.transform.SetParent(duplicateMount10.transform);
      gameObject28.transform.SetParent(duplicateMount10.transform);
      gameObject29.transform.SetParent(duplicateMount10.transform);
      gameObject30.transform.SetParent(duplicateMount10.transform);
      QOLPlugin.AddMountToEncyclopedia(__instance, "UGV1_mixed_x6", mount10);
      GameObject duplicateMount11 = QOLPlugin.DuplicatePrefab("HLT-Rx1", "SAMTrailer1x1");
      WeaponInfo info2 = QOLPlugin.DuplicateWeaponInfo("HLT-R_info", "SAMTrailer1_info", (GameObject) null);
      WeaponMount mount11 = QOLPlugin.DuplicateWeaponMount("HLT-Rx1", "SAMTrailer1x1", duplicateMount11, info2, ((IEnumerable<VehicleDefinition>) UnityEngine.Resources.FindObjectsOfTypeAll<VehicleDefinition>()).FirstOrDefault<VehicleDefinition>((Func<VehicleDefinition, bool>) (vd => vd.name.Equals("SAMTrailer1", StringComparison.InvariantCultureIgnoreCase))));
      QOLPlugin.AddMountToEncyclopedia(__instance, "SAMTrailer1x1", mount11);
      GameObject duplicateMount12 = QOLPlugin.DuplicatePrefab("HLT-Rx1", "HLT-Mx1");
      WeaponInfo info3 = QOLPlugin.DuplicateWeaponInfo("HLT-R_info", "HLT-M_info", (GameObject) null);
      WeaponMount mount12 = QOLPlugin.DuplicateWeaponMount("HLT-Rx1", "HLT-Mx1", duplicateMount12, info3, ((IEnumerable<VehicleDefinition>) UnityEngine.Resources.FindObjectsOfTypeAll<VehicleDefinition>()).FirstOrDefault<VehicleDefinition>((Func<VehicleDefinition, bool>) (vd => vd.name.Equals("HLT-M", StringComparison.InvariantCultureIgnoreCase))));
      QOLPlugin.AddMountToEncyclopedia(__instance, "HLT-Mx1", mount12);
      GameObject duplicateMount13 = QOLPlugin.DuplicatePrefab("HLT-Rx1", "HLT-FCx1");
      WeaponInfo info4 = QOLPlugin.DuplicateWeaponInfo("HLT-R_info", "HLT-FC_info", (GameObject) null);
      WeaponMount mount13 = QOLPlugin.DuplicateWeaponMount("HLT-Rx1", "HLT-FCx1", duplicateMount13, info4, ((IEnumerable<VehicleDefinition>) UnityEngine.Resources.FindObjectsOfTypeAll<VehicleDefinition>()).FirstOrDefault<VehicleDefinition>((Func<VehicleDefinition, bool>) (vd => vd.name.Equals("HLT-FC", StringComparison.InvariantCultureIgnoreCase))));
      QOLPlugin.AddMountToEncyclopedia(__instance, "HLT-FCx1", mount13);
      GameObject duplicateMount14 = QOLPlugin.DuplicatePrefab("HLT-Rx1", "HLT-FTx1");
      WeaponInfo info5 = QOLPlugin.DuplicateWeaponInfo("HLT-R_info", "HLT-FT_info", (GameObject) null);
      WeaponMount mount14 = QOLPlugin.DuplicateWeaponMount("HLT-Rx1", "HLT-FTx1", duplicateMount14, info5, ((IEnumerable<VehicleDefinition>) UnityEngine.Resources.FindObjectsOfTypeAll<VehicleDefinition>()).FirstOrDefault<VehicleDefinition>((Func<VehicleDefinition, bool>) (vd => vd.name.Equals("HLT-FT", StringComparison.InvariantCultureIgnoreCase))));
      QOLPlugin.AddMountToEncyclopedia(__instance, "HLT-FTx1", mount14);
      GameObject duplicateMount15 = QOLPlugin.DuplicatePrefab("HLT-Rx1", "LaserTrailer1x1");
      WeaponInfo info6 = QOLPlugin.DuplicateWeaponInfo("HLT-R_info", "LaserTrailer1_info", (GameObject) null);
      WeaponMount mount15 = QOLPlugin.DuplicateWeaponMount("HLT-Rx1", "LaserTrailer1x1", duplicateMount15, info6, ((IEnumerable<VehicleDefinition>) UnityEngine.Resources.FindObjectsOfTypeAll<VehicleDefinition>()).FirstOrDefault<VehicleDefinition>((Func<VehicleDefinition, bool>) (vd => vd.name.Equals("LaserTrailer1", StringComparison.InvariantCultureIgnoreCase))));
      QOLPlugin.AddMountToEncyclopedia(__instance, "LaserTrailer1x1", mount15);
      GameObject duplicateMount16 = QOLPlugin.DuplicatePrefab("HLT-Rx1", "CRAMTrailer1x1");
      WeaponInfo info7 = QOLPlugin.DuplicateWeaponInfo("HLT-R_info", "CRAMTrailer1_info", (GameObject) null);
      WeaponMount mount16 = QOLPlugin.DuplicateWeaponMount("HLT-Rx1", "CRAMTrailer1x1", duplicateMount16, info7, ((IEnumerable<VehicleDefinition>) UnityEngine.Resources.FindObjectsOfTypeAll<VehicleDefinition>()).FirstOrDefault<VehicleDefinition>((Func<VehicleDefinition, bool>) (vd => vd.name.Equals("CRAMTrailer1", StringComparison.InvariantCultureIgnoreCase))));
      QOLPlugin.AddMountToEncyclopedia(__instance, "CRAMTrailer1x1", mount16);
      UnityEngine.Object.Instantiate<GameObject>(PathLookup.Find("SAMTrailer1")).transform.SetParent(PathLookup.Find("SAMTrailer1x1/HLT-R").transform);
      UnityEngine.Object.Instantiate<GameObject>(PathLookup.Find("HLT-M")).transform.SetParent(PathLookup.Find("HLT-Mx1/HLT-R").transform);
      UnityEngine.Object.Instantiate<GameObject>(PathLookup.Find("HLT-FT")).transform.SetParent(PathLookup.Find("HLT-FTx1/HLT-R").transform);
      UnityEngine.Object.Instantiate<GameObject>(PathLookup.Find("LaserTrailer1")).transform.SetParent(PathLookup.Find("LaserTrailer1x1/HLT-R").transform);
      UnityEngine.Object.Instantiate<GameObject>(PathLookup.Find("CRAMTrailer1")).transform.SetParent(PathLookup.Find("CRAMTrailer1x1/HLT-R").transform);
      UnityEngine.Object.Instantiate<GameObject>(PathLookup.Find("HLT-M")).transform.SetParent(PathLookup.Find("HLT-FCx1/HLT-R").transform);
      UnityEngine.Object.Destroy((UnityEngine.Object) PathLookup.Find("SAMTrailer1x1/HLT-R/upfit"));
      UnityEngine.Object.Destroy((UnityEngine.Object) PathLookup.Find("HLT-Mx1/HLT-R/upfit"));
      UnityEngine.Object.Destroy((UnityEngine.Object) PathLookup.Find("HLT-FTx1/HLT-R/upfit"));
      UnityEngine.Object.Destroy((UnityEngine.Object) PathLookup.Find("LaserTrailer1x1/HLT-R/upfit"));
      UnityEngine.Object.Destroy((UnityEngine.Object) PathLookup.Find("CRAMTrailer1x1/HLT-R/upfit"));
      UnityEngine.Object.Destroy((UnityEngine.Object) PathLookup.Find("HLT-FCx1/HLT-R/upfit"));
      UnityEngine.Object.Destroy((UnityEngine.Object) PathLookup.Find("SAMTrailer1x1/HLT-R/axle1"));
      UnityEngine.Object.Destroy((UnityEngine.Object) PathLookup.Find("HLT-Mx1/HLT-R/axle1"));
      UnityEngine.Object.Destroy((UnityEngine.Object) PathLookup.Find("HLT-FTx1/HLT-R/axle1"));
      UnityEngine.Object.Destroy((UnityEngine.Object) PathLookup.Find("LaserTrailer1x1/HLT-R/axle1"));
      UnityEngine.Object.Destroy((UnityEngine.Object) PathLookup.Find("CRAMTrailer1x1/HLT-R/axle1"));
      UnityEngine.Object.Destroy((UnityEngine.Object) PathLookup.Find("HLT-FCx1/HLT-R/axle1"));
      UnityEngine.Object.Destroy((UnityEngine.Object) PathLookup.Find("SAMTrailer1x1/HLT-R/axle2"));
      UnityEngine.Object.Destroy((UnityEngine.Object) PathLookup.Find("HLT-Mx1/HLT-R/axle2"));
      UnityEngine.Object.Destroy((UnityEngine.Object) PathLookup.Find("HLT-FTx1/HLT-R/axle2"));
      UnityEngine.Object.Destroy((UnityEngine.Object) PathLookup.Find("LaserTrailer1x1/HLT-R/axle2"));
      UnityEngine.Object.Destroy((UnityEngine.Object) PathLookup.Find("CRAMTrailer1x1/HLT-R/axle2"));
      UnityEngine.Object.Destroy((UnityEngine.Object) PathLookup.Find("HLT-FCx1/HLT-R/axle2"));
      UnityEngine.Object.Destroy((UnityEngine.Object) PathLookup.Find("SAMTrailer1x1/HLT-R/axle3"));
      UnityEngine.Object.Destroy((UnityEngine.Object) PathLookup.Find("HLT-Mx1/HLT-R/axle3"));
      UnityEngine.Object.Destroy((UnityEngine.Object) PathLookup.Find("HLT-FTx1/HLT-R/axle3"));
      UnityEngine.Object.Destroy((UnityEngine.Object) PathLookup.Find("LaserTrailer1x1/HLT-R/axle3"));
      UnityEngine.Object.Destroy((UnityEngine.Object) PathLookup.Find("CRAMTrailer1x1/HLT-R/axle3"));
      UnityEngine.Object.Destroy((UnityEngine.Object) PathLookup.Find("HLT-FCx1/HLT-R/axle3"));
      UnityEngine.Object.Destroy((UnityEngine.Object) PathLookup.Find("SAMTrailer1x1/HLT-R/axle4"));
      UnityEngine.Object.Destroy((UnityEngine.Object) PathLookup.Find("HLT-Mx1/HLT-R/axle4"));
      UnityEngine.Object.Destroy((UnityEngine.Object) PathLookup.Find("HLT-FTx1/HLT-R/axle4"));
      UnityEngine.Object.Destroy((UnityEngine.Object) PathLookup.Find("LaserTrailer1x1/HLT-R/axle4"));
      UnityEngine.Object.Destroy((UnityEngine.Object) PathLookup.Find("CRAMTrailer1x1/HLT-R/axle4"));
      UnityEngine.Object.Destroy((UnityEngine.Object) PathLookup.Find("HLT-FCx1/HLT-R/axle4"));
      UnityEngine.Object.Destroy((UnityEngine.Object) PathLookup.Find("SAMTrailer1x1/HLT-R").GetComponent<MeshRenderer>());
      UnityEngine.Object.Destroy((UnityEngine.Object) PathLookup.Find("HLT-Mx1/HLT-R").GetComponent<MeshRenderer>());
      UnityEngine.Object.Destroy((UnityEngine.Object) PathLookup.Find("HLT-FTx1/HLT-R").GetComponent<MeshRenderer>());
      UnityEngine.Object.Destroy((UnityEngine.Object) PathLookup.Find("LaserTrailer1x1/HLT-R").GetComponent<MeshRenderer>());
      UnityEngine.Object.Destroy((UnityEngine.Object) PathLookup.Find("CRAMTrailer1x1/HLT-R").GetComponent<MeshRenderer>());
      UnityEngine.Object.Destroy((UnityEngine.Object) PathLookup.Find("HLT-FCx1/HLT-R").GetComponent<MeshRenderer>());
      UnityEngine.Object.Destroy((UnityEngine.Object) duplicateMount11.GetComponentInChildren<NetworkIdentity>());
      UnityEngine.Object.Destroy((UnityEngine.Object) duplicateMount12.GetComponentInChildren<NetworkIdentity>());
      UnityEngine.Object.Destroy((UnityEngine.Object) duplicateMount14.GetComponentInChildren<NetworkIdentity>());
      UnityEngine.Object.Destroy((UnityEngine.Object) duplicateMount15.GetComponentInChildren<NetworkIdentity>());
      UnityEngine.Object.Destroy((UnityEngine.Object) duplicateMount16.GetComponentInChildren<NetworkIdentity>());
      UnityEngine.Object.Destroy((UnityEngine.Object) duplicateMount13.GetComponentInChildren<NetworkIdentity>());
      UnityEngine.Object.Destroy((UnityEngine.Object) duplicateMount11.GetComponentInChildren<Rigidbody>());
      UnityEngine.Object.Destroy((UnityEngine.Object) duplicateMount12.GetComponentInChildren<Rigidbody>());
      UnityEngine.Object.Destroy((UnityEngine.Object) duplicateMount14.GetComponentInChildren<Rigidbody>());
      UnityEngine.Object.Destroy((UnityEngine.Object) duplicateMount15.GetComponentInChildren<Rigidbody>());
      UnityEngine.Object.Destroy((UnityEngine.Object) duplicateMount16.GetComponentInChildren<Rigidbody>());
      UnityEngine.Object.Destroy((UnityEngine.Object) duplicateMount13.GetComponentInChildren<Rigidbody>());
      UnityEngine.Object.Destroy((UnityEngine.Object) duplicateMount11.GetComponentInChildren<GroundVehicle>());
      UnityEngine.Object.Destroy((UnityEngine.Object) duplicateMount12.GetComponentInChildren<GroundVehicle>());
      UnityEngine.Object.Destroy((UnityEngine.Object) duplicateMount14.GetComponentInChildren<GroundVehicle>());
      UnityEngine.Object.Destroy((UnityEngine.Object) duplicateMount15.GetComponentInChildren<GroundVehicle>());
      UnityEngine.Object.Destroy((UnityEngine.Object) duplicateMount16.GetComponentInChildren<GroundVehicle>());
      UnityEngine.Object.Destroy((UnityEngine.Object) duplicateMount13.GetComponentInChildren<GroundVehicle>());
      UnityEngine.Object.Destroy((UnityEngine.Object) duplicateMount11.GetComponentInChildren<Gun>());
      UnityEngine.Object.Destroy((UnityEngine.Object) duplicateMount12.GetComponentInChildren<Gun>());
      UnityEngine.Object.Destroy((UnityEngine.Object) duplicateMount14.GetComponentInChildren<Gun>());
      UnityEngine.Object.Destroy((UnityEngine.Object) duplicateMount15.GetComponentInChildren<Gun>());
      UnityEngine.Object.Destroy((UnityEngine.Object) duplicateMount16.GetComponentInChildren<Gun>());
      UnityEngine.Object.Destroy((UnityEngine.Object) duplicateMount13.GetComponentInChildren<Gun>());
      UnityEngine.Object.Destroy((UnityEngine.Object) duplicateMount11.GetComponentInChildren<Turret>());
      UnityEngine.Object.Destroy((UnityEngine.Object) duplicateMount12.GetComponentInChildren<Turret>());
      UnityEngine.Object.Destroy((UnityEngine.Object) duplicateMount14.GetComponentInChildren<Turret>());
      UnityEngine.Object.Destroy((UnityEngine.Object) duplicateMount15.GetComponentInChildren<Turret>());
      UnityEngine.Object.Destroy((UnityEngine.Object) duplicateMount16.GetComponentInChildren<Turret>());
      UnityEngine.Object.Destroy((UnityEngine.Object) duplicateMount13.GetComponentInChildren<Turret>());
      UnityEngine.Object.Destroy((UnityEngine.Object) duplicateMount11.GetComponentInChildren<Laser>());
      UnityEngine.Object.Destroy((UnityEngine.Object) duplicateMount12.GetComponentInChildren<Laser>());
      UnityEngine.Object.Destroy((UnityEngine.Object) duplicateMount14.GetComponentInChildren<Laser>());
      UnityEngine.Object.Destroy((UnityEngine.Object) duplicateMount15.GetComponentInChildren<Laser>());
      UnityEngine.Object.Destroy((UnityEngine.Object) duplicateMount16.GetComponentInChildren<Laser>());
      UnityEngine.Object.Destroy((UnityEngine.Object) duplicateMount13.GetComponentInChildren<Laser>());
      UnityEngine.Object.Destroy((UnityEngine.Object) duplicateMount11.GetComponentInChildren<TargetDetector>());
      UnityEngine.Object.Destroy((UnityEngine.Object) duplicateMount12.GetComponentInChildren<TargetDetector>());
      UnityEngine.Object.Destroy((UnityEngine.Object) duplicateMount14.GetComponentInChildren<TargetDetector>());
      UnityEngine.Object.Destroy((UnityEngine.Object) duplicateMount15.GetComponentInChildren<TargetDetector>());
      UnityEngine.Object.Destroy((UnityEngine.Object) duplicateMount16.GetComponentInChildren<TargetDetector>());
      UnityEngine.Object.Destroy((UnityEngine.Object) duplicateMount13.GetComponentInChildren<TargetDetector>());
      UnityEngine.Object.Destroy((UnityEngine.Object) duplicateMount11.GetComponentInChildren<MissileLauncher>());
      UnityEngine.Object.Destroy((UnityEngine.Object) duplicateMount12.GetComponentInChildren<MissileLauncher>());
      UnityEngine.Object.Destroy((UnityEngine.Object) duplicateMount14.GetComponentInChildren<MissileLauncher>());
      UnityEngine.Object.Destroy((UnityEngine.Object) duplicateMount15.GetComponentInChildren<MissileLauncher>());
      UnityEngine.Object.Destroy((UnityEngine.Object) duplicateMount16.GetComponentInChildren<MissileLauncher>());
      UnityEngine.Object.Destroy((UnityEngine.Object) duplicateMount13.GetComponentInChildren<MissileLauncher>());
      UnityEngine.Object.Destroy((UnityEngine.Object) duplicateMount11.GetComponentInChildren<GroundVehicleNetworkTransform>());
      UnityEngine.Object.Destroy((UnityEngine.Object) duplicateMount12.GetComponentInChildren<GroundVehicleNetworkTransform>());
      UnityEngine.Object.Destroy((UnityEngine.Object) duplicateMount14.GetComponentInChildren<GroundVehicleNetworkTransform>());
      UnityEngine.Object.Destroy((UnityEngine.Object) duplicateMount15.GetComponentInChildren<GroundVehicleNetworkTransform>());
      UnityEngine.Object.Destroy((UnityEngine.Object) duplicateMount16.GetComponentInChildren<GroundVehicleNetworkTransform>());
      UnityEngine.Object.Destroy((UnityEngine.Object) duplicateMount13.GetComponentInChildren<GroundVehicleNetworkTransform>());
      UnityEngine.Object.Destroy((UnityEngine.Object) duplicateMount11.GetComponentInChildren<UnitCommand>());
      UnityEngine.Object.Destroy((UnityEngine.Object) duplicateMount12.GetComponentInChildren<UnitCommand>());
      UnityEngine.Object.Destroy((UnityEngine.Object) duplicateMount14.GetComponentInChildren<UnitCommand>());
      UnityEngine.Object.Destroy((UnityEngine.Object) duplicateMount15.GetComponentInChildren<UnitCommand>());
      UnityEngine.Object.Destroy((UnityEngine.Object) duplicateMount16.GetComponentInChildren<UnitCommand>());
      UnityEngine.Object.Destroy((UnityEngine.Object) duplicateMount13.GetComponentInChildren<UnitCommand>());
      foreach (UnityEngine.Object componentsInChild in duplicateMount11.GetComponentsInChildren<BoxCollider>())
        UnityEngine.Object.Destroy(componentsInChild);
      foreach (UnityEngine.Object componentsInChild in duplicateMount12.GetComponentsInChildren<BoxCollider>())
        UnityEngine.Object.Destroy(componentsInChild);
      foreach (UnityEngine.Object componentsInChild in duplicateMount14.GetComponentsInChildren<BoxCollider>())
        UnityEngine.Object.Destroy(componentsInChild);
      foreach (UnityEngine.Object componentsInChild in duplicateMount15.GetComponentsInChildren<BoxCollider>())
        UnityEngine.Object.Destroy(componentsInChild);
      foreach (UnityEngine.Object componentsInChild in duplicateMount16.GetComponentsInChildren<BoxCollider>())
        UnityEngine.Object.Destroy(componentsInChild);
      foreach (UnityEngine.Object componentsInChild in duplicateMount13.GetComponentsInChildren<BoxCollider>())
        UnityEngine.Object.Destroy(componentsInChild);
      foreach (UnityEngine.Object componentsInChild in duplicateMount11.GetComponentsInChildren<MeshCollider>())
        UnityEngine.Object.Destroy(componentsInChild);
      foreach (UnityEngine.Object componentsInChild in duplicateMount12.GetComponentsInChildren<MeshCollider>())
        UnityEngine.Object.Destroy(componentsInChild);
      foreach (UnityEngine.Object componentsInChild in duplicateMount14.GetComponentsInChildren<MeshCollider>())
        UnityEngine.Object.Destroy(componentsInChild);
      foreach (UnityEngine.Object componentsInChild in duplicateMount15.GetComponentsInChildren<MeshCollider>())
        UnityEngine.Object.Destroy(componentsInChild);
      foreach (UnityEngine.Object componentsInChild in duplicateMount16.GetComponentsInChildren<MeshCollider>())
        UnityEngine.Object.Destroy(componentsInChild);
      foreach (UnityEngine.Object componentsInChild in duplicateMount13.GetComponentsInChildren<MeshCollider>())
        UnityEngine.Object.Destroy(componentsInChild);
      foreach (UnityEngine.Object componentsInChild in duplicateMount11.GetComponentsInChildren<UnitPart>())
        UnityEngine.Object.Destroy(componentsInChild);
      foreach (UnityEngine.Object componentsInChild in duplicateMount12.GetComponentsInChildren<UnitPart>())
        UnityEngine.Object.Destroy(componentsInChild);
      foreach (UnityEngine.Object componentsInChild in duplicateMount14.GetComponentsInChildren<UnitPart>())
        UnityEngine.Object.Destroy(componentsInChild);
      foreach (UnityEngine.Object componentsInChild in duplicateMount15.GetComponentsInChildren<UnitPart>())
        UnityEngine.Object.Destroy(componentsInChild);
      foreach (UnityEngine.Object componentsInChild in duplicateMount16.GetComponentsInChildren<UnitPart>())
        UnityEngine.Object.Destroy(componentsInChild);
      foreach (UnityEngine.Object componentsInChild in duplicateMount13.GetComponentsInChildren<UnitPart>())
        UnityEngine.Object.Destroy(componentsInChild);
      GameObject gameObject31 = PathLookup.Find("CruiseMissile1_cargox16");
      GameObject gameObject32 = UnityEngine.Object.Instantiate<GameObject>(PathLookup.Find("CruiseMissile1_internalx4"));
      gameObject32.transform.SetParent(gameObject31.transform);
      gameObject32.name = "temp1";
      GameObject gameObject33 = UnityEngine.Object.Instantiate<GameObject>(PathLookup.Find("CruiseMissile1_internalx4"));
      gameObject33.transform.SetParent(gameObject31.transform);
      gameObject33.name = "temp2";
      GameObject gameObject34 = UnityEngine.Object.Instantiate<GameObject>(PathLookup.Find("CruiseMissile1_internalx4"));
      gameObject34.transform.SetParent(gameObject31.transform);
      gameObject34.name = "temp3";
      foreach (object componentsInChild in gameObject31.GetComponentsInChildren<MountedMissile>())
      {
        Traverse traverse3 = Traverse.Create(componentsInChild);
        traverse3.Field("railDirection").SetValue((object) MountedMissile.RailDirection.Backward);
        traverse3.Field("railLength").SetValue((object) 12f);
        traverse3.Field("railSpeed").SetValue((object) 4f);
        traverse3.Field("railDelay").SetValue((object) 1f);
      }
      foreach (object componentsInChild in PathLookup.Find("AShM1_cargox4").GetComponentsInChildren<MountedMissile>())
      {
        Traverse traverse4 = Traverse.Create(componentsInChild);
        traverse4.Field("railDirection").SetValue((object) MountedMissile.RailDirection.Backward);
        traverse4.Field("railLength").SetValue((object) 8f);
        traverse4.Field("railSpeed").SetValue((object) 4f);
        traverse4.Field("railDelay").SetValue((object) 1f);
      }
      GameObject gameObject35 = PathLookup.Find("ammunitionBunker_destroyed");
      PathLookup.Find("ammunitionBunker_destroyed/fireball_large").name = "fireball_large_original";
      PathLookup.Find("ammunitionBunker_destroyed/AmmoCookoff").transform.SetParent(PathLookup.Find("fireball_large").transform);
      PathLookup.Find("fireball_large").transform.SetParent(gameObject35.transform);
      PathLookup.Find("ammunitionBunker_destroyed/fireball_large/AmmoCookoff").GetComponent<MushroomCloud>().enabled = false;
      PathLookup.Find("ammunitionBunker_destroyed/fireball_large/AmmoCookoff").GetComponent<Shockwave>().enabled = false;
      PathLookup.Find("ammunitionBunker_destroyed/fireball_large/AmmoCookoff/dust and smoke standing surface").transform.SetParent(gameObject35.transform);
      PathLookup.Find("ammunitionBunker_destroyed/fireball_large/AmmoCookoff/dust and smoke standing billboard").transform.SetParent(gameObject35.transform);
      PathLookup.Find("ammunitionBunker_destroyed/fireball_large/AmmoCookoff/persistentDust").transform.SetParent(gameObject35.transform);
      PathLookup.Find("ammunitionBunker_destroyed/fireball_large/AmmoCookoff/Dirt Stand-in").transform.SetParent(gameObject35.transform);
      PathLookup.Find("ammunitionBunker_destroyed/fireball_large/AmmoCookoff/groundDust").transform.SetParent(gameObject35.transform);
      UnityEngine.Object.Destroy((UnityEngine.Object) PathLookup.Find("ammunitionBunker_destroyed/fireball_large/AmmoCookoff/shockwaveDecal"));
      GameObject gameObject36 = PathLookup.Find("ammunitionBunker_destroyed/fireball_large/AmmoCookoff");
      SetGlobalParticles component3 = gameObject36.GetComponent<SetGlobalParticles>();
      ParticleSystem component4 = PathLookup.Find("fireball_medium/fireball_flames_slow").GetComponent<ParticleSystem>();
      Transform customSimulationSpace = component4.main.customSimulationSpace;
      foreach (ParticleSystem componentsInChild in gameObject36.GetComponentsInChildren<ParticleSystem>(true))
      {
        ParticleSystem.MainModule main = componentsInChild.main with
        {
          simulationSpace = ParticleSystemSimulationSpace.World,
          customSimulationSpace = Datum.origin
        };
        if (typeof (SetGlobalParticles).GetField("systems", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue((object) component3) is List<ParticleSystem> particleSystemList)
        {
          // ISSUE: explicit non-virtual call
          __nonvirtual (particleSystemList.Add(componentsInChild));
        }
        ParticleSystemRenderer component5 = componentsInChild.GetComponent<ParticleSystemRenderer>();
        ParticleSystemRenderer component6 = component4.GetComponent<ParticleSystemRenderer>();
        if ((UnityEngine.Object) component5 != (UnityEngine.Object) null && (UnityEngine.Object) component6 != (UnityEngine.Object) null)
        {
          component5.alignment = component6.alignment;
          component5.renderMode = component6.renderMode;
          if ((UnityEngine.Object) component5.material != (UnityEngine.Object) null && (UnityEngine.Object) component6.material != (UnityEngine.Object) null)
            component5.material.SetInt("_Cull", component6.material.GetInt("_Cull"));
        }
      }
      ParticleSystem.MainModule main1 = PathLookup.Find("ammunitionBunker_destroyed/fireball_large/AmmoCookoff/BurningClumpsEmitterRoof").GetComponent<ParticleSystem>().main with
      {
        startDelay = (ParticleSystem.MinMaxCurve) 0.5f,
        duration = 20f
      };
      PathLookup.Find("StorageTankExplosion/fireball_large/fireball_slow").GetComponent<ParticleSystem>().main.duration = 16f;
      ParticleSystem component7 = PathLookup.Find("ballisticMissile1/FireParticles")?.GetComponent<ParticleSystem>();
      if ((UnityEngine.Object) component7 != (UnityEngine.Object) null)
        component7.main.maxParticles = 25;
      ParticleSystem component8 = PathLookup.Find("ballisticMissile1_tacNuke/FireParticles")?.GetComponent<ParticleSystem>();
      if ((UnityEngine.Object) component8 != (UnityEngine.Object) null)
        component8.main.maxParticles = 25;
      float num1 = 2f;
      float num2 = 1.25f;
      ParticleSystem.MainModule main2 = PathLookup.Find("ammunitionBunker_destroyed/fireball_large/fireball_fast").GetComponent<ParticleSystem>().main;
      ParticleSystem.MainModule main3 = PathLookup.Find("ammunitionBunker_destroyed/fireball_large/fireball_flames2").GetComponent<ParticleSystem>().main;
      ParticleSystem.MainModule main4 = PathLookup.Find("ammunitionBunker_destroyed/fireball_large/streamers").GetComponent<ParticleSystem>().main;
      main2.duration = 1f * num2;
      main2.startSize = (ParticleSystem.MinMaxCurve) (30f * num1);
      main2.startLifetime = (ParticleSystem.MinMaxCurve) (3f * num1);
      main3.duration = 3f * num2;
      main3.startSize = (ParticleSystem.MinMaxCurve) (10f * num1);
      main3.startLifetime = (ParticleSystem.MinMaxCurve) (3f * num1);
      main4.startLifetimeMultiplier = 3f;
      main4.startSpeedMultiplier = 400f;
      main4.startSizeMultiplier = 10f;
      main4.gravityModifier = (ParticleSystem.MinMaxCurve) 0.2f;
      ParticleSystem.MainModule main5 = PathLookup.Find("fireball_medium/fireball_flames_fast").GetComponent<ParticleSystem>().main;
      ParticleSystem.MainModule main6 = PathLookup.Find("fireball_medium/fireball_flames_slow").GetComponent<ParticleSystem>().main;
      ParticleSystem.MainModule main7 = PathLookup.Find("fireball_medium/fireball_smoke").GetComponent<ParticleSystem>().main;
      ParticleSystem.MainModule main8 = PathLookup.Find("fireball_medium/streamers").GetComponent<ParticleSystem>().main;
      main5.duration = 0.5f * num2;
      main5.startSize = (ParticleSystem.MinMaxCurve) (16f * num1);
      main5.startLifetime = (ParticleSystem.MinMaxCurve) (3f * num1);
      main6.duration = 0.5f * num2;
      main6.startSize = (ParticleSystem.MinMaxCurve) (20f * num1);
      main6.startLifetime = (ParticleSystem.MinMaxCurve) (3.5f * num1);
      main7.duration = 30f;
      main7.startLifetime = (ParticleSystem.MinMaxCurve) 10f;
      main7.maxParticles = 200;
      main8.startLifetimeMultiplier = 2f;
      main8.startSpeedMultiplier = 300f;
      main8.startSizeMultiplier = 5f;
      main8.gravityModifier = (ParticleSystem.MinMaxCurve) 0.2f;
      float num3 = 2f;
      ParticleSystem.MainModule main9 = PathLookup.Find("fire_large/flames").GetComponent<ParticleSystem>().main;
      ParticleSystem.MainModule main10 = PathLookup.Find("fire_med/airFire1Flames").GetComponent<ParticleSystem>().main;
      ParticleSystem.MainModule main11 = PathLookup.Find("fire_small/engineFire1Flames").GetComponent<ParticleSystem>().main;
      ParticleSystem.MainModule main12 = PathLookup.Find("fire_tiny/engineFire1Flames").GetComponent<ParticleSystem>().main;
      main9.startSizeMultiplier = num3;
      main9.startLifetimeMultiplier = num3;
      main10.startSizeMultiplier = num3;
      main10.startLifetimeMultiplier = num3;
      main11.startSizeMultiplier = num3;
      main11.startLifetimeMultiplier = num3;
      main12.startSizeMultiplier = num3;
      main12.startLifetimeMultiplier = num3;
      WeaponInfo weaponInfo3 = ResourceLookup.FindWeaponInfo("info_AGM2");
      WeaponInfo weaponInfo4 = ResourceLookup.FindWeaponInfo("AAM3_info");
      GameObject duplicateMount17 = QOLPlugin.DuplicatePrefab("AGM1_rotaryLauncher_UtilityHelo1", "AGM2_rotaryLauncher_UtilityHelo1");
      GameObject duplicateMount18 = QOLPlugin.DuplicatePrefab("AGM1_rotaryLauncher_UtilityHelo1", "AAM3_rotaryLauncher_UtilityHelo1");
      UnityEngine.Object.Instantiate<GameObject>(PathLookup.Find("AGM2_rotaryLauncher_UtilityHelo1/pylon/agm1")).transform.SetParent(duplicateMount17.transform);
      UnityEngine.Object.Instantiate<GameObject>(PathLookup.Find("AGM2_rotaryLauncher_UtilityHelo1/pylon/agm1")).transform.SetParent(duplicateMount17.transform);
      UnityEngine.Object.Instantiate<GameObject>(PathLookup.Find("AGM2_rotaryLauncher_UtilityHelo1/pylon/agm1")).transform.SetParent(duplicateMount17.transform);
      UnityEngine.Object.Destroy((UnityEngine.Object) PathLookup.Find("AAM3_rotaryLauncher_UtilityHelo1/pylon/agm1"));
      UnityEngine.Object.Destroy((UnityEngine.Object) PathLookup.Find("AAM3_rotaryLauncher_UtilityHelo1/pylon/agm1 (1)"));
      UnityEngine.Object.Destroy((UnityEngine.Object) PathLookup.Find("AAM3_rotaryLauncher_UtilityHelo1/pylon/agm1 (2)"));
      WeaponMount mount17 = QOLPlugin.DuplicateWeaponMount("AGM1_rotaryLauncher_UtilityHelo1", "AGM2_rotaryLauncher_UtilityHelo1", duplicateMount17, weaponInfo3);
      WeaponMount mount18 = QOLPlugin.DuplicateWeaponMount("AGM1_rotaryLauncher_UtilityHelo1", "AAM3_rotaryLauncher_UtilityHelo1", duplicateMount18, weaponInfo4);
      ResourceLookup.FindWeaponMount("AGM1_rotaryLauncher_UtilityHelo1");
      QOLPlugin.AddMountToEncyclopedia(__instance, "AGM2_rotaryLauncher_UtilityHelo1", mount17);
      QOLPlugin.AddMountToEncyclopedia(__instance, "AAM3_rotaryLauncher_UtilityHelo1", mount18);
      WeaponManager component9 = PathLookup.Find("UtilityHelo1/fuelTank_F/fuselage_F/cockpit").GetComponent<WeaponManager>();
      FieldInfo field1 = typeof (Hardpoint).GetField("pylonOptions", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
      System.Type nestedType = typeof (Hardpoint).GetNestedType("HardpointPylon", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
      FieldInfo field2 = nestedType.GetField("cargo", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
      FieldInfo field3 = nestedType.GetField("mount", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
      FieldInfo field4 = nestedType.GetField("renderer", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
      MeshRenderer component10 = PathLookup.Find("UtilityHelo1/fuselage_L/slidingDoor_L").GetComponent<MeshRenderer>();
      MeshRenderer component11 = PathLookup.Find("UtilityHelo1/fuselage_L/AGMPanel_L").GetComponent<MeshRenderer>();
      UnityEngine.Object.Instantiate<GameObject>(PathLookup.Find("UtilityHelo1/fuselage_L"));
      object instance1 = Activator.CreateInstance(nestedType, true);
      field2.SetValue(instance1, (object) true);
      field3.SetValue(instance1, (object) null);
      field4.SetValue(instance1, (object) component10);
      object instance2 = Activator.CreateInstance(nestedType, true);
      field2.SetValue(instance2, (object) false);
      field3.SetValue(instance2, (object) null);
      field4.SetValue(instance2, (object) component11);
      Array instance3 = Array.CreateInstance(nestedType, 2);
      instance3.SetValue(instance1, 0);
      instance3.SetValue(instance2, 1);
      field1.SetValue((object) component9.hardpointSets[3].hardpoints[0], (object) instance3);
      MeshRenderer component12 = PathLookup.Find("trainer/wingroot2_L/wing1_L/pylon1L").GetComponent<MeshRenderer>();
      MeshRenderer component13 = PathLookup.Find("trainer/wingroot2_R/wing1_R/pylon1R").GetComponent<MeshRenderer>();
      MeshRenderer component14 = PathLookup.Find("trainer/wingroot2_L/wing1_L/wing2_L/pylon2L").GetComponent<MeshRenderer>();
      MeshRenderer component15 = PathLookup.Find("trainer/wingroot2_R/wing1_R/wing2_R/pylon2R").GetComponent<MeshRenderer>();
      MeshRenderer component16 = PathLookup.Find("trainer/wingroot2_L/wing1_L/wing2_L/pylon3L").GetComponent<MeshRenderer>();
      MeshRenderer component17 = PathLookup.Find("trainer/wingroot2_R/wing1_R/wing2_R/pylon3R").GetComponent<MeshRenderer>();
      WeaponManager component18 = PathLookup.Find("trainer/cockpit_R/cockpit_F").GetComponent<WeaponManager>();
      Array instance4 = Array.CreateInstance(nestedType, 1);
      object instance5 = Activator.CreateInstance(nestedType, true);
      field2.SetValue(instance5, (object) false);
      field3.SetValue(instance5, (object) null);
      field4.SetValue(instance5, (object) component12);
      instance4.SetValue(instance5, 0);
      field1.SetValue((object) component18.hardpointSets[1].hardpoints[0], (object) instance4);
      Array instance6 = Array.CreateInstance(nestedType, 1);
      object instance7 = Activator.CreateInstance(nestedType, true);
      field2.SetValue(instance7, (object) false);
      field3.SetValue(instance7, (object) null);
      field4.SetValue(instance7, (object) component13);
      instance6.SetValue(instance7, 0);
      field1.SetValue((object) component18.hardpointSets[1].hardpoints[1], (object) instance6);
      Array instance8 = Array.CreateInstance(nestedType, 1);
      object instance9 = Activator.CreateInstance(nestedType, true);
      field2.SetValue(instance9, (object) false);
      field3.SetValue(instance9, (object) null);
      field4.SetValue(instance9, (object) component14);
      instance8.SetValue(instance9, 0);
      field1.SetValue((object) component18.hardpointSets[2].hardpoints[0], (object) instance8);
      Array instance10 = Array.CreateInstance(nestedType, 1);
      object instance11 = Activator.CreateInstance(nestedType, true);
      field2.SetValue(instance11, (object) false);
      field3.SetValue(instance11, (object) null);
      field4.SetValue(instance11, (object) component15);
      instance10.SetValue(instance11, 0);
      field1.SetValue((object) component18.hardpointSets[2].hardpoints[1], (object) instance10);
      Array instance12 = Array.CreateInstance(nestedType, 1);
      object instance13 = Activator.CreateInstance(nestedType, true);
      field2.SetValue(instance13, (object) false);
      field3.SetValue(instance13, (object) null);
      field4.SetValue(instance13, (object) component16);
      instance12.SetValue(instance13, 0);
      field1.SetValue((object) component18.hardpointSets[3].hardpoints[0], (object) instance12);
      Array instance14 = Array.CreateInstance(nestedType, 1);
      object instance15 = Activator.CreateInstance(nestedType, true);
      field2.SetValue(instance15, (object) false);
      field3.SetValue(instance15, (object) null);
      field4.SetValue(instance15, (object) component17);
      instance14.SetValue(instance15, 0);
      field1.SetValue((object) component18.hardpointSets[3].hardpoints[1], (object) instance14);
      component18.hardpointSets[1].hardpoints[0].Pylon = (UnityEngine.Renderer) component12;
      component18.hardpointSets[1].hardpoints[1].Pylon = (UnityEngine.Renderer) component13;
      component18.hardpointSets[2].hardpoints[0].Pylon = (UnityEngine.Renderer) component14;
      component18.hardpointSets[2].hardpoints[1].Pylon = (UnityEngine.Renderer) component15;
      component18.hardpointSets[3].hardpoints[0].Pylon = (UnityEngine.Renderer) component16;
      component18.hardpointSets[3].hardpoints[1].Pylon = (UnityEngine.Renderer) component17;
      GameObject gameObject37 = QOLPlugin.LoadFirstAssetFromBundle<GameObject>(QOLPlugin.GetBundlePath("FBXMod.DarkreachNew.bundle"));
      if ((UnityEngine.Object) gameObject37 != (UnityEngine.Object) null)
      {
        MeshFilter[] componentsInChildren9 = gameObject37.GetComponentsInChildren<MeshFilter>();
        if (componentsInChildren9.Length >= 4)
        {
          PathLookup.Find("Darkreach/engine_L").GetComponent<MeshFilter>().mesh = componentsInChildren9[0].sharedMesh;
          PathLookup.Find("Darkreach/engine_R").GetComponent<MeshFilter>().mesh = componentsInChildren9[1].sharedMesh;
          PathLookup.Find("Darkreach/fuselage_R/nozzle_L").GetComponent<MeshFilter>().mesh = componentsInChildren9[2].sharedMesh;
          PathLookup.Find("Darkreach/fuselage_R/nozzle_R").GetComponent<MeshFilter>().mesh = componentsInChildren9[3].sharedMesh;
        }
      }
      GameObject gameObject38 = QOLPlugin.LoadFirstAssetFromBundle<GameObject>(QOLPlugin.GetBundlePath("FBXMod.CompassNew.bundle"));
      if ((UnityEngine.Object) gameObject38 != (UnityEngine.Object) null)
      {
        MeshFilter[] componentsInChildren10 = gameObject38.GetComponentsInChildren<MeshFilter>();
        if (componentsInChildren10.Length >= 2)
        {
          PathLookup.Find("trainer/engine_L/nozzle_L").GetComponent<MeshFilter>().mesh = componentsInChildren10[0].sharedMesh;
          PathLookup.Find("trainer/engine_R/nozzle_R").GetComponent<MeshFilter>().mesh = componentsInChildren10[1].sharedMesh;
        }
      }
      GameObject gameObject39 = QOLPlugin.LoadFirstAssetFromBundle<GameObject>(QOLPlugin.GetBundlePath("FBXMod.AttackHelo1NewWings.bundle"));
      if ((UnityEngine.Object) gameObject39 != (UnityEngine.Object) null)
      {
        MeshFilter[] componentsInChildren11 = gameObject39.GetComponentsInChildren<MeshFilter>();
        if (componentsInChildren11.Length >= 2)
        {
          PathLookup.Find("AttackHelo1/weaponbay_L/wing_L").GetComponent<MeshFilter>().mesh = componentsInChildren11[0].sharedMesh;
          PathLookup.Find("AttackHelo1/weaponbay_R/wing_R").GetComponent<MeshFilter>().mesh = componentsInChildren11[1].sharedMesh;
        }
      }
      GameObject gameObject40 = QOLPlugin.LoadFirstAssetFromBundle<GameObject>(QOLPlugin.GetBundlePath("FBXMod.Fighter1New.bundle"));
      if ((UnityEngine.Object) gameObject40 != (UnityEngine.Object) null)
      {
        MeshFilter[] componentsInChildren12 = gameObject40.GetComponentsInChildren<MeshFilter>();
        if (componentsInChildren12.Length >= 4)
        {
          PathLookup.Find("Fighter1/fuselage/wing1a_L/wing2_L").GetComponent<MeshFilter>().mesh = componentsInChildren12[0].sharedMesh;
          PathLookup.Find("Fighter1/fuselage/wing1a_L/wing2_L/wingtip_L").GetComponent<MeshFilter>().mesh = componentsInChildren12[2].sharedMesh;
          PathLookup.Find("Fighter1/fuselage/wing1a_R/wing2_R").GetComponent<MeshFilter>().mesh = componentsInChildren12[1].sharedMesh;
          PathLookup.Find("Fighter1/fuselage/wing1a_R/wing2_R/wingtip_R").GetComponent<MeshFilter>().mesh = componentsInChildren12[3].sharedMesh;
        }
      }
      GameObject gameObject41 = QOLPlugin.LoadFirstAssetFromBundle<GameObject>(QOLPlugin.GetBundlePath("FBXMod.Fighter1Nose.bundle"));
      if ((UnityEngine.Object) gameObject41 != (UnityEngine.Object) null)
      {
        MeshFilter[] componentsInChildren13 = gameObject41.GetComponentsInChildren<MeshFilter>();
        if (componentsInChildren13.Length >= 1)
          PathLookup.Find("Fighter1/fuselage/cockpit/nose").GetComponent<MeshFilter>().mesh = componentsInChildren13[0].sharedMesh;
      }
      GameObject gameObject42 = QOLPlugin.LoadFirstAssetFromBundle<GameObject>(QOLPlugin.GetBundlePath("FBXMod.Fighter1Nozzle.bundle"));
      if ((UnityEngine.Object) gameObject42 != (UnityEngine.Object) null)
      {
        GameObject gameObject43 = PathLookup.Find("Fighter1/tail/nozzle");
        Material sharedMaterial4 = gameObject43.GetComponent<MeshRenderer>().sharedMaterial;
        UnityEngine.Object.Destroy((UnityEngine.Object) gameObject43.GetComponent<MeshFilter>());
        UnityEngine.Object.Destroy((UnityEngine.Object) gameObject43.GetComponent<MeshRenderer>());
        SkinnedMeshRenderer skinnedMeshRenderer = gameObject43.AddComponent<SkinnedMeshRenderer>();
        skinnedMeshRenderer.sharedMesh = gameObject42.GetComponent<SkinnedMeshRenderer>().sharedMesh;
        skinnedMeshRenderer.sharedMaterial = sharedMaterial4;
      }
      GameObject gameObject44 = QOLPlugin.LoadFirstAssetFromBundle<GameObject>(QOLPlugin.GetBundlePath("FBXMod.SmallFighter1Nose.bundle"));
      if ((UnityEngine.Object) gameObject44 != (UnityEngine.Object) null)
      {
        MeshFilter[] componentsInChildren14 = gameObject44.GetComponentsInChildren<MeshFilter>();
        if (componentsInChildren14.Length >= 1)
        {
          GameObject gameObject45 = PathLookup.Find("SmallFighter1/liftFan/cockpit/nose");
          gameObject45.GetComponent<MeshFilter>().mesh = componentsInChildren14[0].sharedMesh;
          gameObject45.GetComponent<MeshRenderer>().sharedMaterials = new Material[2]
          {
            gameObject45.GetComponent<MeshRenderer>().sharedMaterials[0],
            PathLookup.Find("SmallFighter1/liftFan/cockpit/canopyHinge/canopyFrame/canopy").GetComponent<MeshRenderer>().material
          };
        }
      }
      GameObject gameObject46 = UnityEngine.Object.Instantiate<GameObject>(PathLookup.Find("Fighter1/fuselage/wing1a_L/wing2_L/pylon_wing_L"));
      gameObject46.transform.SetParent(PathLookup.Find("Fighter1/fuselage/wing1a_L/wing2_L/wingtip_L").transform);
      gameObject46.name = "pylon_wing_L2";
      GameObject gameObject47 = UnityEngine.Object.Instantiate<GameObject>(PathLookup.Find("Fighter1/fuselage/wing1a_R/wing2_R/pylon_wing_R"));
      gameObject47.transform.SetParent(PathLookup.Find("Fighter1/fuselage/wing1a_R/wing2_R/wingtip_R").transform);
      gameObject47.name = "pylon_wing_R2";
      gameObject46.transform.position = PathLookup.Find("Fighter1/fuselage/wing1a_L/wing2_L/wingtip_L/hardpoint_wingtip_L").transform.position;
      gameObject46.transform.rotation = PathLookup.Find("Fighter1/fuselage/wing1a_L/wing2_L/wingtip_L/hardpoint_wingtip_L").transform.rotation;
      gameObject47.transform.position = PathLookup.Find("Fighter1/fuselage/wing1a_R/wing2_R/wingtip_R/hardpoint_wingtip_R").transform.position;
      gameObject47.transform.rotation = PathLookup.Find("Fighter1/fuselage/wing1a_R/wing2_R/wingtip_R/hardpoint_wingtip_R").transform.rotation;
      UnitPart component19 = PathLookup.Find("Corvette1/bow1/magazine").GetComponent<UnitPart>();
      typeof (UnitPart).GetField("disintegrateObjects", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).SetValue((object) component19, (object) new GameObject[2]
      {
        PathLookup.Find("Corvette1/bow1/missileStation1"),
        PathLookup.Find("Corvette1/bow1/missileStation2")
      });
      GameObject gameObject48 = UnityEngine.Object.Instantiate<GameObject>(PathLookup.Find("Destroyer1/Hull_CF/Hull_CFF/R9missileStation1"));
      gameObject48.name = "RAMmissileStation1";
      gameObject48.transform.SetParent(PathLookup.Find("Destroyer1/Hull_CF/Hull_CFF").transform);
      gameObject48.transform.position = PathLookup.Find("Destroyer1/Hull_CF/Hull_CFF/R9missileStation1").transform.position;
      PathLookup.Find("Destroyer1/Hull_CF/Hull_CFF/R9missileStation1/launchParticles").SetActive(false);
      PathLookup.Find("Destroyer1/Hull_CF/Hull_CFF/RAMmissileStation1/launchParticles").SetActive(false);
      foreach (StandardLoadout standardLoadout in ResourceLookup.FindAircraftParameters("CAS1Parameters").StandardLoadouts)
        standardLoadout.loadout.weapons.Add(QOLPlugin.GetMount("TailHook_Trainer"));
      foreach (StandardLoadout standardLoadout in ResourceLookup.FindAircraftParameters("SFBParameters").StandardLoadouts)
        standardLoadout.loadout.weapons.Add(QOLPlugin.GetMount("P_PassiveJammer1"));
      foreach (Missile missile in UnityEngine.Resources.FindObjectsOfTypeAll<Missile>())
      {
        missile.ClearDirtyBits();
        missile.unitState = (Unit.UnitState) 0;
        missile.NetworkunitState = (Unit.UnitState) 0;
      }
      UnityEngine.Debug.unityLogger.logEnabled = true;
      PathLookup.ClearCache();
      if (QOLPlugin.notDedicatedServer)
        return;
      QOLPlugin qolPlugin = new QOLPlugin();
      try
      {
        Assembly assembly = typeof (QOLPlugin).Assembly;
        string name = ((IEnumerable<string>) assembly.GetManifestResourceNames()).FirstOrDefault<string>((Func<string, bool>) (n => n.EndsWith("commands.qol", StringComparison.OrdinalIgnoreCase)));
        if (name != null)
        {
          Stream manifestResourceStream = assembly.GetManifestResourceStream(name);
          if (manifestResourceStream != null)
          {
            string end = new StreamReader(manifestResourceStream).ReadToEnd();
            IEnumerator configProcessor = qolPlugin.CreateConfigProcessor(end);
            do
              ;
            while (configProcessor.MoveNext());
            QOLPlugin.logger.LogInfo((object) "Starting load process coroutine");
          }
          else
            QOLPlugin.logger.LogWarning((object) "Issue with ManifestResourceStream occured");
        }
        else
          QOLPlugin.logger.LogWarning((object) "No embedded and external file found");
      }
      catch (Exception ex)
      {
        QOLPlugin.logger.LogError((object) $"Failed processing config as dedicated, {ex}");
      }
    }
  }

  public static WeaponMount GetMount(string input) => ResourceLookup.FindWeaponMount(input);

  public static StandardLoadout CreateStandardLoadout(float fuelRatio)
  {
    StandardLoadout standardLoadout = new StandardLoadout();
    FieldInfo field = typeof (StandardLoadout).GetField("loadout", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
    System.Type fieldType = field.FieldType;
    object instance = Activator.CreateInstance(fieldType);
    field.SetValue((object) standardLoadout, instance);
    fieldType.GetField("weapons", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).SetValue(instance, (object) new List<WeaponMount>());
    standardLoadout.FuelRatio = fuelRatio;
    return standardLoadout;
  }

  public static GameObject DuplicatePrefab(string originalName, string newName)
  {
    GameObject original = PathLookup.Find(originalName);
    if ((UnityEngine.Object) original == (UnityEngine.Object) null)
    {
      QOLPlugin.logger.LogError((object) $"Prefab '{originalName}' not found!");
      return (GameObject) null;
    }
    GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(original);
    gameObject.name = newName;
    gameObject.transform.SetParent((Transform) null);
    gameObject.hideFlags = HideFlags.HideAndDontSave;
    gameObject.SetActive(false);
    NetworkIdentity componentInChildren = gameObject.GetComponentInChildren<NetworkIdentity>();
    if (!((UnityEngine.Object) componentInChildren != (UnityEngine.Object) null))
      return gameObject;
    Traverse traverse = Traverse.Create((object) componentInChildren);
    componentInChildren.PrefabHash = int.Parse(componentInChildren.PrefabHash.ToString().Substring(1));
    traverse.Field("_hasSpawned").SetValue((object) false);
    traverse.Method("NetworkReset", Array.Empty<object>()).GetValue();
    return gameObject;
  }

  public static WeaponInfo DuplicateWeaponInfo(
    string originalName,
    string newName,
    GameObject weapon)
  {
    QOLPlugin.logger.LogDebug((object) $"{originalName} {newName} {weapon?.ToString()}");
    WeaponInfo weaponInfo1 = ResourceLookup.FindWeaponInfo(originalName);
    if ((UnityEngine.Object) weaponInfo1 == (UnityEngine.Object) null)
    {
      QOLPlugin.logger.LogError((object) $"WeaponInfo '{originalName}' not found!");
      return (WeaponInfo) null;
    }
    WeaponInfo weaponInfo2 = UnityEngine.Object.Instantiate<WeaponInfo>(weaponInfo1);
    weaponInfo2.name = newName;
    weaponInfo2.weaponPrefab = weapon;
    if ((UnityEngine.Object) weapon != (UnityEngine.Object) null)
    {
      if ((UnityEngine.Object) weapon.GetComponent<Missile>() != (UnityEngine.Object) null)
        ReflectionHelpers.SetFieldValue((object) weapon.GetComponent<Missile>(), "info", (object) weaponInfo2);
      if ((UnityEngine.Object) weapon.GetComponentInChildren<JammingPod>() != (UnityEngine.Object) null)
      {
        ReflectionHelpers.SetFieldValue((object) weapon.GetComponentInChildren<JammingPod>(), "info", (object) weaponInfo2);
        weaponInfo2.weaponPrefab = (GameObject) null;
      }
    }
    return weaponInfo2;
  }

  public static MissileDefinition DuplicateMissileDefinition(
    string originalName,
    string newName,
    GameObject weapon)
  {
    MissileDefinition missileDefinition1 = ResourceLookup.FindMissileDefinition(originalName);
    Missile component = weapon.GetComponent<Missile>();
    if ((UnityEngine.Object) missileDefinition1 == (UnityEngine.Object) null)
    {
      QOLPlugin.logger.LogError((object) $"MissileDefinition '{originalName}' not found!");
      return (MissileDefinition) null;
    }
    if ((UnityEngine.Object) component == (UnityEngine.Object) null)
      QOLPlugin.logger.LogError((object) $"Missile '{originalName}' not found! GO returned '{missileDefinition1}'");
    MissileDefinition missileDefinition2 = UnityEngine.Object.Instantiate<MissileDefinition>(missileDefinition1);
    missileDefinition2.name = newName;
    ReflectionHelpers.SetFieldValue((object) component, "definition", (object) missileDefinition2);
    missileDefinition2.unitPrefab = weapon;
    missileDefinition2.jsonKey = newName;
    return missileDefinition2;
  }

  public static WeaponMount DuplicateWeaponMount(
    string originalName,
    string newName,
    GameObject duplicateMount,
    WeaponInfo info,
    VehicleDefinition vehicleDefinition = null)
  {
    WeaponMount weaponMount1 = ResourceLookup.FindWeaponMount(originalName);
    if ((UnityEngine.Object) weaponMount1 == (UnityEngine.Object) null)
    {
      QOLPlugin.logger.LogError((object) $"WeaponMount '{originalName}' not found!");
      return (WeaponMount) null;
    }
    WeaponMount weaponMount2 = UnityEngine.Object.Instantiate<WeaponMount>(weaponMount1);
    weaponMount2.name = newName;
    weaponMount2.jsonKey = newName;
    weaponMount2.dontAutomaticallyAddToEncyclopedia = false;
    MountedMissile[] componentsInChildren = duplicateMount.GetComponentsInChildren<MountedMissile>(true);
    QOLPlugin.logger.LogDebug((object) $"Found {componentsInChildren.Length} in {duplicateMount} {weaponMount2}");
    foreach (object target in componentsInChildren)
      ReflectionHelpers.SetFieldValue(target, nameof (info), (object) info);
    foreach (MountedCargo componentsInChild in duplicateMount.GetComponentsInChildren<MountedCargo>(true))
    {
      QOLPlugin.logger.LogDebug((object) $"MountedCargo {componentsInChild} found in {info} to {vehicleDefinition} on {duplicateMount}");
      ReflectionHelpers.SetFieldValue((object) componentsInChild, "cargo", (object) vehicleDefinition);
      ReflectionHelpers.SetFieldValue((object) componentsInChild, nameof (info), (object) info);
    }
    weaponMount2.info = info;
    weaponMount2.prefab = duplicateMount;
    return weaponMount2;
  }

  public static VehicleDefinition DuplicateVehicleDefinition(
    string originalName,
    string newName,
    GameObject vehicle)
  {
    VehicleDefinition vehicleDefinition1 = ResourceLookup.FindVehicleDefinition(originalName);
    GroundVehicle component = vehicle.GetComponent<GroundVehicle>();
    if ((UnityEngine.Object) vehicleDefinition1 == (UnityEngine.Object) null)
    {
      QOLPlugin.logger.LogError((object) $"VehicleDefinition '{originalName}' not found!");
      return (VehicleDefinition) null;
    }
    VehicleDefinition vehicleDefinition2 = UnityEngine.Object.Instantiate<VehicleDefinition>(vehicleDefinition1);
    vehicleDefinition2.name = newName;
    ReflectionHelpers.SetFieldValue((object) component, "definition", (object) vehicleDefinition2);
    vehicleDefinition2.unitPrefab = vehicle;
    vehicleDefinition2.jsonKey = newName;
    return vehicleDefinition2;
  }

  public static BuildingDefinition DuplicateBuildingDefinition(
    string originalName,
    string newName,
    GameObject building)
  {
    BuildingDefinition buildingDefinition1 = ResourceLookup.FindBuildingDefinition(originalName);
    Building component = building.GetComponent<Building>();
    if ((UnityEngine.Object) buildingDefinition1 == (UnityEngine.Object) null)
    {
      QOLPlugin.logger.LogError((object) $"BuildingDefinition '{originalName}' not found!");
      return (BuildingDefinition) null;
    }
    BuildingDefinition buildingDefinition2 = UnityEngine.Object.Instantiate<BuildingDefinition>(buildingDefinition1);
    buildingDefinition2.name = newName;
    ReflectionHelpers.SetFieldValue((object) component, "definition", (object) buildingDefinition2);
    buildingDefinition2.unitPrefab = building;
    buildingDefinition2.jsonKey = newName;
    return buildingDefinition2;
  }

  public static void AddMountToEncyclopedia(
    Encyclopedia __instance,
    string name,
    WeaponMount mount)
  {
    __instance.weaponMounts.Add(mount);
  }

  public static void AddMissileToEncyclopedia(
    Encyclopedia __instance,
    string name,
    MissileDefinition missile)
  {
    __instance.missiles.Add(missile);
  }

  public static void AddVehicleToEncyclopedia(
    Encyclopedia __instance,
    string name,
    VehicleDefinition vehicle)
  {
    __instance.vehicles.Add(vehicle);
  }

  public static void AddBuildingToEncyclopedia(
    Encyclopedia __instance,
    string name,
    BuildingDefinition building)
  {
    __instance.buildings.Add(building);
  }

  public static AircraftDefinition DuplicateAircraftDefinition(
    string originalName,
    string newName,
    GameObject aircraft)
  {
    AircraftDefinition aircraftDefinition1 = ResourceLookup.FindAircraftDefinition(originalName);
    Aircraft component = aircraft.GetComponent<Aircraft>();
    if ((UnityEngine.Object) aircraftDefinition1 == (UnityEngine.Object) null)
    {
      QOLPlugin.logger.LogError((object) $"AircraftDefinition '{originalName}' not found!");
      return (AircraftDefinition) null;
    }
    AircraftDefinition aircraftDefinition2 = UnityEngine.Object.Instantiate<AircraftDefinition>(aircraftDefinition1);
    aircraftDefinition2.name = newName;
    ReflectionHelpers.SetFieldValue((object) component, "definition", (object) aircraftDefinition2);
    aircraftDefinition2.unitPrefab = aircraft;
    aircraftDefinition2.jsonKey = newName;
    aircraftDefinition2.aircraftParameters = UnityEngine.Object.Instantiate<AircraftParameters>(aircraftDefinition1.aircraftParameters);
    aircraftDefinition2.aircraftParameters.name = newName + "_parameters";
    return aircraftDefinition2;
  }

  public static void AddAircraftToEncyclopedia(
    Encyclopedia __instance,
    string name,
    AircraftDefinition aircraft)
  {
    __instance.aircraft.Add(aircraft);
  }

  public static ShipDefinition DuplicateShipDefinition(
    string originalName,
    string newName,
    GameObject ship)
  {
    ShipDefinition shipDefinition1 = ResourceLookup.FindShipDefinition(originalName);
    Ship component = ship.GetComponent<Ship>();
    if ((UnityEngine.Object) shipDefinition1 == (UnityEngine.Object) null)
    {
      QOLPlugin.logger.LogError((object) $"ShipDefinition '{originalName}' not found!");
      return (ShipDefinition) null;
    }
    ShipDefinition shipDefinition2 = UnityEngine.Object.Instantiate<ShipDefinition>(shipDefinition1);
    shipDefinition2.name = newName;
    ReflectionHelpers.SetFieldValue((object) component, "definition", (object) shipDefinition2);
    shipDefinition2.unitPrefab = ship;
    shipDefinition2.jsonKey = newName;
    return shipDefinition2;
  }

  public static void AddShipToEncyclopedia(
    Encyclopedia __instance,
    string name,
    ShipDefinition ship)
  {
    __instance.ships.Add(ship);
  }

  public static Texture2D LoadTextureFromResource(string resourcePath)
  {
    using (Stream manifestResourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourcePath))
    {
      if (manifestResourceStream == null)
        return (Texture2D) null;
      using (MemoryStream destination = new MemoryStream())
      {
        manifestResourceStream.CopyTo((Stream) destination);
        Texture2D tex = new Texture2D(2, 2);
        tex.LoadImage(destination.ToArray());
        return tex;
      }
    }
  }

  public static bool WithAssetBundle(
    string resourcePath,
    Action<AssetBundle> action,
    bool logErrorOnMissing = false)
  {
    using (Stream manifestResourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourcePath))
    {
      if (manifestResourceStream == null)
      {
        if (logErrorOnMissing)
          QOLPlugin.logger.LogError((object) (resourcePath + " not found in embedded resources!"));
        return false;
      }
      using (MemoryStream destination = new MemoryStream())
      {
        manifestResourceStream.CopyTo((Stream) destination);
        AssetBundle assetBundle = AssetBundle.LoadFromMemory(destination.ToArray());
        try
        {
          action(assetBundle);
        }
        finally
        {
          assetBundle.Unload(false);
        }
        return true;
      }
    }
  }

  public static T LoadFirstAssetFromBundle<T>(string resourcePath, bool logErrorOnMissing = false) where T : UnityEngine.Object
  {
    T result = default (T);
    QOLPlugin.WithAssetBundle(resourcePath, (Action<AssetBundle>) (bundle => result = ((IEnumerable<T>) bundle.LoadAllAssets<T>()).FirstOrDefault<T>()), logErrorOnMissing);
    return result;
  }

  public static T LoadNamedAssetFromBundle<T>(
    string resourcePath,
    string assetName,
    bool logErrorOnMissing = false)
    where T : UnityEngine.Object
  {
    T result = default (T);
    QOLPlugin.WithAssetBundle(resourcePath, (Action<AssetBundle>) (bundle => result = ((IEnumerable<T>) bundle.LoadAllAssets<T>()).FirstOrDefault<T>((Func<T, bool>) (a => a.name.Equals(assetName, StringComparison.OrdinalIgnoreCase)))), logErrorOnMissing);
    return result;
  }

  public static string GetBundlePath(string relativePath)
  {
    return $"{Assembly.GetExecutingAssembly().GetName().Name}.Resources.{relativePath}";
  }

  public static void DisableParticleEmissions(params string[] paths)
  {
    foreach (string path in paths)
    {
      GameObject gameObject = PathLookup.Find(path, false);
      if ((UnityEngine.Object) gameObject != (UnityEngine.Object) null)
      {
        ParticleSystem component = gameObject.GetComponent<ParticleSystem>();
        if ((UnityEngine.Object) component != (UnityEngine.Object) null)
          component.emission.rateOverTime = (ParticleSystem.MinMaxCurve) 0.0f;
      }
    }
  }

  public static void DestroyAtPaths(params string[] paths)
  {
    foreach (string path in paths)
    {
      GameObject gameObject = PathLookup.Find(path, false);
      if ((UnityEngine.Object) gameObject != (UnityEngine.Object) null)
        UnityEngine.Object.Destroy((UnityEngine.Object) gameObject);
    }
  }

  private void ApplyHarmonyPatches()
  {
    QOLPlugin.config_designations = this.Config.Bind<bool>("Text", "designations", true, "Tweaks aircraft designations to add unique suffixes.");
    this.harmony.PatchAll(Assembly.GetExecutingAssembly());
  }

  private void Awake()
  {
    ((UnityEngine.Object) this).hideFlags = HideFlags.HideAndDontSave;
    QOLPlugin.logger = this.Logger;
    QOLPlugin.mplogger = Logger.CreateLogSource("P2082 MP");
    QOLPlugin.kflogger = Logger.CreateLogSource("P2082 KF");
    PathLookup.Initialize(QOLPlugin.logger);
    this._pluginFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
    this.GenerateBotNames();
    this.harmony = new Harmony("qol");
    PatchConfigRegistry.Initialize((BaseUnityPlugin) this, QOLPlugin.logger);
    this.ApplyHarmonyPatches();
    this.InitializeMultiplayer();
    this.InitializeUI();
    if (QOLPlugin.notDedicatedServer)
      ((MonoBehaviour) this).StartCoroutine(this.DelayedStart());
    if (!QOLPlugin.notDedicatedServer)
      return;
    SceneManager.sceneLoaded += new UnityAction<Scene, LoadSceneMode>(this.OnSceneLoaded);
  }

  public static async void ExecuteAfterFrame(Action action)
  {
    if (!QOLPlugin.notDedicatedServer)
      return;
    await Task.Delay(1);
    await Task.Yield();
    Action action1 = action;
    if (action1 == null)
      return;
    action1();
  }

  private void GenerateBotNames()
  {
    Assembly executingAssembly = Assembly.GetExecutingAssembly();
    using (Stream manifestResourceStream = executingAssembly.GetManifestResourceStream(executingAssembly.GetName().Name + ".Resources.botNames.txt"))
    {
      using (StreamReader streamReader = new StreamReader(manifestResourceStream))
      {
        string str;
        while ((str = streamReader.ReadLine()) != null)
          QOLPlugin.botNames.Add(str);
      }
    }
  }

  private static string FormatWithSignificantDecimals(float value, float relativeThreshold = 0.01f)
  {
    if ((double) Math.Abs(value) < 9.9999999747524271E-07)
      return "0";
    int digits = Math.Max(0, -(int) Math.Floor(Math.Log10((double) Math.Abs(value) * (double) relativeThreshold)));
    float d = (float) Math.Round((double) value, digits);
    return (double) Math.Abs(d - (float) Math.Truncate((double) d)) < (double) (Math.Abs(d) * relativeThreshold) ? ((int) d).ToString() : d.ToString().TrimEnd('0').TrimEnd('.');
  }

  private static string EnergyReading(float power)
  {
    if ((double) power < 1.0)
      return QOLPlugin.FormatWithSignificantDecimals(power * 1000f) + "W";
    return (double) power < 1000.0 ? QOLPlugin.FormatWithSignificantDecimals(power) + "kW" : QOLPlugin.FormatWithSignificantDecimals(power / 1000f) + "MW";
  }

  public static void DisplayMountInfo(AircraftSelectionMenu instance, WeaponMount mount)
  {
    Traverse traverse1 = Traverse.Create((object) instance);
    TMP_Text tmpText1 = traverse1.Field("weaponSeeker").GetValue<TMP_Text>();
    TMP_Text tmpText2 = traverse1.Field("weaponRange").GetValue<TMP_Text>();
    TMP_Text tmpText3 = traverse1.Field("weaponAP").GetValue<TMP_Text>();
    TMP_Text tmpText4 = traverse1.Field("weaponHE").GetValue<TMP_Text>();
    TMP_Text tmpText5 = traverse1.Field("weaponRCS").GetValue<TMP_Text>();
    TMP_Text tmpText6 = traverse1.Field("weaponCost").GetValue<TMP_Text>();
    GameObject gameObject1 = traverse1.Field("weaponInfoArea").GetValue<GameObject>();
    GameObject gameObject2 = traverse1.Field("weaponImageArea").GetValue<GameObject>();
    Image image = traverse1.Field("weaponImage").GetValue<Image>();
    TMP_Text tmpText7 = traverse1.Field("info").GetValue<TMP_Text>();
    Aircraft aircraft = traverse1.Field("previewAircraft").GetValue<Aircraft>();
    List<AircraftDefinition> aircraftDefinitionList = traverse1.Field("aircraftSelection").GetValue<List<AircraftDefinition>>();
    int index = traverse1.Field("selectionIndex").GetValue<int>();
    WeaponInfo info = mount?.info;
    if ((UnityEngine.Object) info == (UnityEngine.Object) null)
    {
      gameObject2.SetActive(false);
      gameObject1.SetActive(false);
      tmpText7.text = (UnityEngine.Object) aircraft != (UnityEngine.Object) null ? aircraftDefinitionList[index].aircraftParameters.aircraftDescription : "Nothing Selected";
    }
    else if ((UnityEngine.Object) info.weaponIcon == (UnityEngine.Object) null)
    {
      gameObject2.SetActive(false);
      gameObject1.SetActive(false);
      instance.DisplayInfo(mount?.info);
    }
    else
    {
      gameObject2.SetActive(true);
      image.sprite = info.weaponIcon;
      gameObject1.SetActive(true);
      if (info.cargo || info.troops)
      {
        tmpText1.text = "Cargo";
        tmpText2.text = "-";
        tmpText3.text = "-";
        tmpText4.text = "-";
        tmpText5.text = "M: " + UnitConverter.WeightReading(info.massPerRound);
        tmpText6.text = "C: " + UnitConverter.ValueReading(info.costPerRound);
      }
      else
      {
        tmpText1.text = (UnityEngine.Object) info.weaponPrefab != (UnityEngine.Object) null ? info.weaponPrefab.GetComponent<MissileSeeker>().GetSeekerType() : (info.energy ? "Laser" : "Gun");
        tmpText2.text = "R: " + UnitConverter.DistanceReading(info.targetRequirements.maxRange);
        tmpText3.text = (UnityEngine.Object) info.weaponPrefab != (UnityEngine.Object) null ? $"AP: {info.weaponPrefab.GetComponent<Missile>().GetPierce()}" : $"AP: {info.pierceDamage}";
        tmpText4.text = (UnityEngine.Object) info.weaponPrefab != (UnityEngine.Object) null ? "HE: " + UnitConverter.YieldReading(info.weaponPrefab.GetComponent<Missile>().GetYield()) : "HE: " + UnitConverter.YieldReading(info.blastDamage);
        tmpText5.text = (UnityEngine.Object) info.weaponPrefab != (UnityEngine.Object) null ? $"RCS: {info.weaponPrefab.GetComponent<Missile>().definition.radarSize}" : "-";
        tmpText6.text = (UnityEngine.Object) info.weaponPrefab != (UnityEngine.Object) null ? "C: " + UnitConverter.ValueReading(info.costPerRound) : "-";
      }
      instance.DisplayInfo(mount?.info);
      float emptyCost = mount.emptyCost;
      Weapon[] componentsInChildren = mount.prefab.GetComponentsInChildren<Weapon>();
      foreach (Weapon weapon in componentsInChildren)
        emptyCost += (float) weapon.GetFullAmmo() * ((IEnumerable<Weapon>) componentsInChildren).Last<Weapon>().info.costPerRound;
      tmpText6.text = "C: " + UnitConverter.ValueReading(emptyCost);
      if (info.jammer)
      {
        tmpText1.text = "Jammer";
        tmpText3.text = "-";
        tmpText4.text = "-";
      }
      if (info.sling)
      {
        tmpText1.text = "Sling";
        tmpText3.text = "-";
        tmpText4.text = "-";
      }
      if (info.energy && !info.jammer)
      {
        Laser componentInChildren = mount.prefab.GetComponentInChildren<Laser>();
        if ((UnityEngine.Object) componentInChildren != (UnityEngine.Object) null)
        {
          Traverse traverse2 = Traverse.Create((object) componentInChildren);
          tmpText3.text = "PWR: " + QOLPlugin.EnergyReading(traverse2.Field("power").GetValue<float>());
          tmpText4.text = $"ANG: {QOLPlugin.FormatWithSignificantDecimals(traverse2.Field("maxAngle").GetValue<float>())}°";
        }
      }
      if (info.troops)
        tmpText1.text = "Troops";
      if (info.gun)
      {
        Gun componentInChildren = mount.prefab.GetComponentInChildren<Gun>();
        if ((UnityEngine.Object) componentInChildren != (UnityEngine.Object) null)
        {
          Traverse traverse3 = Traverse.Create((object) componentInChildren);
          int num1 = traverse3.Field("magazineCapacity").GetValue<int>();
          int num2 = traverse3.Field("magazines").GetValue<int>();
          float num3 = traverse3.Field("fireRate").GetValue<float>();
          tmpText5.text = $"RD: {num1 * (1 + num2)}";
          tmpText6.text = "RPM: " + QOLPlugin.FormatWithSignificantDecimals(num3);
        }
      }
      gameObject2.SetActive(true);
      gameObject1.SetActive(true);
      tmpText7.text = info.description;
    }
  }

  [HarmonyPatch(typeof (Aircraft))]
  [qol.PatchConfig.PatchConfig("Text", "aircraftNames", true, "Gives AI aircraft random player names extracted from the Primeva 2082 Discord")]
  public class AircraftNamePatch
  {
    [HarmonyPrepare]
    private static bool Prepare()
    {
      return PatchConfigRegistry.IsEnabled(typeof (QOLPlugin.AircraftNamePatch));
    }

    [HarmonyPostfix]
    [HarmonyPatch("OnStartServer")]
    private static void Postfix(Aircraft __instance)
    {
      if (__instance.unitName.Contains("["))
        return;
      string botName = QOLPlugin.botNames[UnityEngine.Random.Range(0, QOLPlugin.botNames.Count)];
      QOLPlugin.logger.LogInfo((object) $"Renamed {__instance.NetworkunitName} to {botName} [{__instance.NetworkunitName}]");
      __instance.unitName = $"{botName} [{__instance.NetworkunitName}]";
    }
  }

  [HarmonyPatch(typeof (Aircraft))]
  [qol.PatchConfig.PatchConfig("Text", "aircraftLiveries", true, "Gives AI aircraft random liveries from the ones the host has")]
  public class AircraftLiveryPatch
  {
    [HarmonyPrepare]
    private static bool Prepare()
    {
      return PatchConfigRegistry.IsEnabled(typeof (QOLPlugin.AircraftLiveryPatch));
    }

    [HarmonyPostfix]
    [HarmonyPatch("OnStartServer")]
    private static void Postfix(Aircraft __instance)
    {
      if ((UnityEngine.Object) __instance == (UnityEngine.Object) null)
      {
        QOLPlugin.logger.LogWarning((object) "Aircraft instance is null in AircraftLiveryPatch");
      }
      else
      {
        if (!string.IsNullOrEmpty(__instance.unitName) && __instance.unitName.Contains("["))
          return;
        List<LiveryKey> liveryKeyList = new List<LiveryKey>();
        string aircraftFaction = (string) null;
        if ((UnityEngine.Object) __instance.NetworkHQ?.faction != (UnityEngine.Object) null)
          aircraftFaction = __instance.NetworkHQ.faction.name;
        AircraftParameters aircraftParameters = __instance.GetAircraftParameters();
        if ((UnityEngine.Object) aircraftParameters != (UnityEngine.Object) null && aircraftParameters.liveries != null)
        {
          for (int index = 0; index < aircraftParameters.liveries.Count; ++index)
          {
            AircraftParameters.Livery livery = aircraftParameters.liveries[index];
            if (livery != null && ValidFaction(livery.faction?.factionName))
              liveryKeyList.Add(new LiveryKey(index));
          }
        }
        if (ModFolders.AppDataSkins != null)
        {
          try
          {
            IEnumerable<LiveryMetaData> source = ModFolders.AppDataSkins.ListMetaData();
            if (source != null)
            {
              foreach (LiveryMetaData metaData in source.Where<LiveryMetaData>((Func<LiveryMetaData, bool>) (x => x.Aircraft == __instance.unitName && ValidFaction(x.Faction))))
                liveryKeyList.Add(new LiveryKey(metaData, false));
            }
          }
          catch (Exception ex)
          {
            QOLPlugin.logger.LogError((object) ("Error processing AppDataSkins: " + ex.Message));
          }
        }
        if (ModFolders.WorkshopSkins != null)
        {
          try
          {
            IEnumerable<LiveryMetaData> source = ModFolders.WorkshopSkins.ListMetaData();
            if (source != null)
            {
              foreach (LiveryMetaData metaData in source.Where<LiveryMetaData>((Func<LiveryMetaData, bool>) (x => x.Aircraft == __instance.unitName && ValidFaction(x.Faction))))
                liveryKeyList.Add(new LiveryKey(metaData, true));
            }
          }
          catch (Exception ex)
          {
            QOLPlugin.logger.LogError((object) ("Error processing WorkshopSkins: " + ex.Message));
          }
        }
        if (liveryKeyList.Count > 0)
        {
          try
          {
            int index = UnityEngine.Random.Range(0, liveryKeyList.Count);
            __instance.SetLiveryKey(liveryKeyList[index]);
          }
          catch (Exception ex)
          {
            QOLPlugin.logger.LogError((object) ("Error setting livery: " + ex.Message));
          }
        }
        else
          QOLPlugin.logger.LogWarning((object) ("No valid liveries found for aircraft: " + __instance.unitName));

        bool ValidFaction(string liveryFaction)
        {
          return string.IsNullOrEmpty(liveryFaction) || FactionHelper.EmptyOrNoFactionOrNeutral(liveryFaction) || string.IsNullOrEmpty(aircraftFaction) || FactionHelper.EmptyOrNoFactionOrNeutral(aircraftFaction) || liveryFaction == aircraftFaction;
        }
      }
    }
  }

  [HarmonyPatch(typeof (AircraftSelectionMenu))]
  public static class DisplayInfoPatch
  {
    [HarmonyPatch("DisplayInfo")]
    [HarmonyPrefix]
    private static bool Prefix(AircraftSelectionMenu __instance) => false;
  }

  [HarmonyPatch(typeof (AircraftSelectionMenu))]
  public static class DescriptionOnHoverPatch
  {
    [HarmonyPatch("Initialize")]
    [HarmonyPrefix]
    private static void Prefix(AircraftSelectionMenu __instance)
    {
      WeaponSelector component = Traverse.Create((object) __instance).Field("weaponSelectionPrefab").GetValue<GameObject>().GetComponent<WeaponSelector>();
      if ((UnityEngine.Object) component.GetComponent<QOLPlugin.WeaponSelectorDescriptionOnHover>() == (UnityEngine.Object) null)
      {
        component.gameObject.AddComponent<QOLPlugin.WeaponSelectorDescriptionOnHover>();
        component.gameObject.transform.Find("HardpointSetDropdown/Template/Scrollbar").GetComponent<Scrollbar>().direction = Scrollbar.Direction.BottomToTop;
      }
      component.GetComponent<QOLPlugin.WeaponSelectorDescriptionOnHover>().selectionMenu = __instance;
    }
  }

  [HarmonyPatch(typeof (Aircraft), "OnStartServer")]
  public static class AircraftSkillPatch
  {
    [HarmonyPostfix]
    public static void Postfix(Aircraft __instance)
    {
      __instance.skill = UnityEngine.Random.Range(5f, 15f);
      __instance.bravery = UnityEngine.Random.Range(0.5f, 1.5f);
    }
  }

  [HarmonyPatch(typeof (Pilot), "TogglePilotVisibility")]
  [qol.PatchConfig.PatchConfig("Graphics", "showCopilots", true, "Forces aircraft copilots and crew to be always visible, even in first-person view and the selection screen")]
  private class PilotVisibilityPatch
  {
    [HarmonyPrepare]
    private static bool Prepare()
    {
      return PatchConfigRegistry.IsEnabled(typeof (QOLPlugin.PilotVisibilityPatch));
    }

    private static bool Prefix(Pilot __instance, ref bool enabled)
    {
      if (!(__instance.currentState is PilotPlayerState))
        enabled = true;
      return true;
    }
  }

  [HarmonyPatch(typeof (GLOC), "OnEnable")]
  private class GLOCPatch
  {
    [HarmonyPostfix]
    private static void Postfix(GLOC __instance)
    {
      Pilot component = __instance.GetComponent<Pilot>();
      if ((UnityEngine.Object) component?.aircraft == (UnityEngine.Object) null || ((IEnumerable<string>) QOLPlugin.GLOCexcludedAircraft).Contains<string>(component.aircraft.name))
        return;
      FieldInfo field1 = typeof (GLOC).GetField("bloodPumpRate", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
      FieldInfo field2 = typeof (GLOC).GetField("staminaRecoveryRate", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
      field1?.SetValue((object) __instance, (object) 0.32f);
      field2?.SetValue((object) __instance, (object) 0.19f);
      if (!QOLPlugin.isBeta)
        return;
      FlyByWireTuner[] objectsOfTypeAll = UnityEngine.Resources.FindObjectsOfTypeAll<FlyByWireTuner>();
      if (objectsOfTypeAll == null)
        return;
      ((IEnumerable<FlyByWireTuner>) objectsOfTypeAll).FirstOrDefault<FlyByWireTuner>((Func<FlyByWireTuner, bool>) (c => c.gameObject.scene.IsValid()))?.gameObject?.SetActive(PlayerSettings.debugVis);
    }
  }

  [HarmonyPatch(typeof (AircraftSelectionMenu))]
  public static class LoadDefaults_Patch
  {
    [HarmonyPatch("LoadDefaults")]
    [HarmonyPrefix]
    public static bool Prefix(AircraftSelectionMenu __instance)
    {
      if ((UnityEngine.Object) __instance == (UnityEngine.Object) null)
      {
        QOLPlugin.logger.LogError((object) "LoadDefaults patch: Could not find AircraftSelectionMenu instance!");
        return true;
      }
      try
      {
        FieldInfo field1 = typeof (AircraftSelectionMenu).GetField("aircraftSelection", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        FieldInfo field2 = typeof (AircraftSelectionMenu).GetField("selectionIndex", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        FieldInfo field3 = typeof (AircraftSelectionMenu).GetField("liveryDropdown", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        FieldInfo field4 = typeof (AircraftSelectionMenu).GetField("fuelLevel", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        FieldInfo field5 = typeof (AircraftSelectionMenu).GetField("previewAircraft", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        FieldInfo field6 = typeof (AircraftSelectionMenu).GetField("weaponSelectors", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        Faction faction = (typeof (AircraftSelectionMenu).GetField("airbase", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).GetValue((object) __instance) as Airbase).CurrentHQ.faction;
        List<AircraftDefinition> aircraftDefinitionList = field1.GetValue((object) __instance) as List<AircraftDefinition>;
        int index1 = (int) field2.GetValue((object) __instance);
        AircraftSelectionMenu aircraftSelectionMenu1 = __instance;
        Slider slider = field4.GetValue((object) aircraftSelectionMenu1) as Slider;
        AircraftSelectionMenu aircraftSelectionMenu2 = __instance;
        TMP_Dropdown tmpDropdown = field3.GetValue((object) aircraftSelectionMenu2) as TMP_Dropdown;
        Aircraft aircraft = field5.GetValue((object) __instance) as Aircraft;
        List<WeaponSelector> weaponSelectorList = field6.GetValue((object) __instance) as List<WeaponSelector>;
        AircraftCustomization aircraftCustomization;
        if (!GameManager.aircraftCustomization.TryGetValue(aircraftDefinitionList[index1], out aircraftCustomization))
        {
          aircraftCustomization = new AircraftCustomization(aircraftDefinitionList[index1].aircraftParameters.loadouts[0], 0.5f, -1);
          if (AircraftDefaultsConfigs.FuelLevels.ContainsKey(aircraft.name))
          {
            QOLPlugin.logger.LogDebug((object) ("Setting up liveryselection for " + faction.factionName));
            int livery = faction.factionName == "Primeva" ? AircraftDefaultsConfigs.LiveryIndexPrimeva[aircraft.name] : AircraftDefaultsConfigs.LiveryIndexBDF[aircraft.name];
            aircraftCustomization = new AircraftCustomization(aircraftDefinitionList[index1].aircraftParameters.loadouts[1], AircraftDefaultsConfigs.FuelLevels[aircraft.name], livery);
          }
        }
        for (int index2 = 0; index2 < aircraftCustomization.loadout.weapons.Count; ++index2)
        {
          WeaponMount weapon = aircraftCustomization.loadout.weapons[index2];
          weaponSelectorList[index2].SetValue(weapon);
        }
        typeof (AircraftSelectionMenu).GetMethod("UpdateWeapons", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Invoke((object) __instance, new object[1]
        {
          (object) false
        });
        tmpDropdown.SetValueWithoutNotify(aircraftCustomization.livery);
        typeof (AircraftSelectionMenu).GetMethod("SelectLivery", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Invoke((object) __instance, (object[]) null);
        slider.SetValueWithoutNotify(aircraftCustomization.fuelLevel);
        typeof (AircraftSelectionMenu).GetMethod("ChangeFuelLevel", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Invoke((object) __instance, (object[]) null);
        return false;
      }
      catch (Exception ex)
      {
        QOLPlugin.logger.LogError((object) $"Error in LoadDefaults patch: {ex}");
        return true;
      }
    }
  }

  [HarmonyPatch]
  public class AircraftParameters_StandardLoadout_Fix
  {
    [HarmonyPrepare]
    private static bool Prepare() => false;

    [HarmonyPatch(typeof (AircraftParameters), "GetRandomStandardLoadout")]
    [HarmonyPrefix]
    public static bool GetRandomStandardLoadoutPrefix(ref StandardLoadout __result)
    {
      QOLPlugin.logger.LogInfo((object) "Ignored StandardLoadout");
      __result = (StandardLoadout) null;
      return false;
    }
  }

  [HarmonyPatch(typeof (Aircraft), "Rearm")]
  public static class RearmRepairPatch
  {
    [HarmonyPostfix]
    public static void Postfix(Aircraft __instance)
    {
      foreach (UnitPart componentsInChild in __instance.GetComponentsInChildren<AeroPart>())
        componentsInChild.Repair();
    }
  }

  [HarmonyPatch(typeof (Aircraft))]
  [HarmonyPatch("WaitRemoveAircraft")]
  private class WaitRemoveAircraftPatch
  {
    [HarmonyPrepare]
    private static bool Prepare() => false;

    private static bool Prefix(ref float delay)
    {
      float num = 2f;
      delay *= num;
      return true;
    }
  }

  [HarmonyPatch(typeof (Aircraft))]
  [HarmonyPatch]
  private class AIPilotSkillPatch
  {
    [HarmonyPrepare]
    private static bool Prepare() => false;

    [HarmonyPostfix]
    public static void Postfix(Aircraft __instance)
    {
      Traverse.Create((object) __instance).Field("skill").SetValue((object) 20f);
    }
  }

  [HarmonyPatch(typeof (CameraStateManager), "SetFollowingUnit")]
  public static class CameraStateManager_SetFollowingUnit_Patch
  {
    [HarmonyPrepare]
    private static bool Prepare() => false;

    [HarmonyPostfix]
    public static void Postfix(CameraStateManager __instance, Unit unit)
    {
      if (!((UnityEngine.Object) unit != (UnityEngine.Object) null) || !((UnityEngine.Object) unit.gameObject.GetComponent<PilotDismounted>() != (UnityEngine.Object) null))
        return;
      __instance.SwitchState((CameraBaseState) __instance.cockpitState);
    }
  }

  [HarmonyPatch(typeof (CameraOrbitState), "EnterState")]
  private class ThirdPersonHUDPatch
  {
    [HarmonyPrepare]
    private static bool Prepare() => false;

    [HarmonyPostfix]
    private static void Postfix(CameraOrbitState __instance)
    {
      if (!((UnityEngine.Object) SceneSingleton<CombatHUD>.i.aircraft != (UnityEngine.Object) null) || CameraStateManager.cameraMode != CameraMode.cockpit && CameraStateManager.cameraMode != CameraMode.orbit)
        return;
      FlightHud.EnableCanvas(true);
      DynamicMap.EnableCanvas(true);
    }
  }

  [HarmonyPatch(typeof (CameraCockpitState))]
  public static class CockpitFOVPatch
  {
    [HarmonyPrepare]
    private static bool Prepare() => false;

    [HarmonyPatch("EnterState")]
    [HarmonyPostfix]
    public static void EnterState_Postfix(CameraCockpitState __instance)
    {
      Traverse.Create((object) __instance).Field("FOVAdjustment").SetValue((object) 20f);
    }
  }

  [HarmonyPatch(typeof (CargoDeploymentSystem), "Initialize")]
  public class CDS_Patch
  {
    [HarmonyPrepare]
    private static bool Prepare() => false;

    private static bool Prefix(CargoDeploymentSystem __instance)
    {
      __instance.gameObject.SetActive(true);
      return true;
    }
  }

  [HarmonyPatch]
  public class MountSpawnChecker
  {
    [HarmonyPrepare]
    private static bool Prepare() => false;

    [HarmonyPrefix]
    [HarmonyPatch(typeof (MountedCargo), "AttachToHardpoint")]
    private static bool Prefix_0(
      MountedCargo __instance,
      Aircraft aircraft,
      Hardpoint hardpoint,
      WeaponMount mount)
    {
      QOLPlugin.logger.LogInfo((object) "Starting hardpoint check for cargo");
      QOLPlugin.logger.LogWarning((object) aircraft);
      QOLPlugin.logger.LogWarning((object) hardpoint);
      QOLPlugin.logger.LogWarning((object) mount);
      QOLPlugin.logger.LogWarning((object) hardpoint.part);
      QOLPlugin.logger.LogWarning((object) hardpoint.part.parentUnit);
      QOLPlugin.logger.LogWarning((object) __instance.cargo);
      QOLPlugin.logger.LogWarning((object) __instance.cargo.unitPrefab);
      QOLPlugin.logger.LogWarning((object) __instance.cargo.unitPrefab.GetComponent<Unit>());
      return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof (WeaponMount), "Initialize")]
    private static bool Prefix_1(WeaponMount __instance)
    {
      QOLPlugin.logger.LogInfo((object) "WeaponMount Init checking...");
      QOLPlugin.logger.LogWarning((object) ("cargo? " + __instance.Cargo.ToString()));
      QOLPlugin.logger.LogWarning((object) ("info = " + __instance.info?.ToString()));
      QOLPlugin.logger.LogWarning((object) ("prefab = " + __instance.prefab?.ToString()));
      if ((UnityEngine.Object) __instance.info != (UnityEngine.Object) null)
        QOLPlugin.logger.LogWarning((object) ("info.weaponPrefab: " + __instance.info.weaponPrefab?.ToString()));
      return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof (Hardpoint), "SpawnMount")]
    private static bool Prefix(
      Hardpoint __instance,
      Aircraft aircraft,
      WeaponMount weaponMount,
      ref GameObject __result,
      ref Transform ___transform,
      ref UnitPart ___part,
      ref GameObject ___spawnedPrefab)
    {
      QOLPlugin.logger.LogInfo((object) "=== Hardpoint.SpawnMount Diagnostic ===");
      bool flag1 = false;
      if ((UnityEngine.Object) ___transform == (UnityEngine.Object) null)
      {
        QOLPlugin.logger.LogError((object) "transform is null!");
        flag1 = true;
      }
      else
        QOLPlugin.logger.LogInfo((object) ("transform: " + ___transform.name));
      if ((UnityEngine.Object) ___part == (UnityEngine.Object) null)
      {
        QOLPlugin.logger.LogError((object) "part is null!");
        flag1 = true;
      }
      else
        QOLPlugin.logger.LogInfo((object) $"part: {___part.name}, detached: {___part.IsDetached()}");
      if ((UnityEngine.Object) ___spawnedPrefab != (UnityEngine.Object) null)
        QOLPlugin.logger.LogWarning((object) ("spawnedPrefab already exists: " + ___spawnedPrefab.name));
      if (__instance.BuiltInTurrets == null)
      {
        QOLPlugin.logger.LogError((object) "BuiltInTurrets array is null!");
        flag1 = true;
      }
      else
      {
        QOLPlugin.logger.LogInfo((object) $"BuiltInTurrets count: {__instance.BuiltInTurrets.Length}");
        for (int index = 0; index < __instance.BuiltInTurrets.Length; ++index)
        {
          if ((UnityEngine.Object) __instance.BuiltInTurrets[index] == (UnityEngine.Object) null)
          {
            QOLPlugin.logger.LogError((object) $"BuiltInTurrets[{index}] is null!");
            flag1 = true;
          }
        }
      }
      if (__instance.BuiltInWeapons == null)
      {
        QOLPlugin.logger.LogError((object) "BuiltInWeapons array is null!");
        flag1 = true;
      }
      else
      {
        QOLPlugin.logger.LogInfo((object) $"BuiltInWeapons count: {__instance.BuiltInWeapons.Length}");
        for (int index = 0; index < __instance.BuiltInWeapons.Length; ++index)
        {
          if ((UnityEngine.Object) __instance.BuiltInWeapons[index] == (UnityEngine.Object) null)
          {
            QOLPlugin.logger.LogError((object) $"BuiltInWeapons[{index}] is null!");
            flag1 = true;
          }
        }
      }
      bool flag2;
      if ((UnityEngine.Object) weaponMount == (UnityEngine.Object) null)
      {
        QOLPlugin.logger.LogError((object) "weaponMount parameter is null!");
        flag2 = true;
        __result = (GameObject) null;
        return false;
      }
      QOLPlugin.logger.LogInfo((object) ("weaponMount: " + weaponMount.mountName));
      if ((UnityEngine.Object) weaponMount.prefab == (UnityEngine.Object) null)
      {
        QOLPlugin.logger.LogError((object) "weaponMount.prefab is null!");
        flag2 = true;
        __result = (GameObject) null;
        return false;
      }
      QOLPlugin.logger.LogInfo((object) ("weaponMount.prefab: " + weaponMount.prefab.name));
      if ((UnityEngine.Object) aircraft == (UnityEngine.Object) null)
      {
        QOLPlugin.logger.LogError((object) "aircraft parameter is null!");
        flag1 = true;
      }
      else
        QOLPlugin.logger.LogInfo((object) ("aircraft: " + aircraft.name));
      if ((UnityEngine.Object) aircraft != (UnityEngine.Object) null && (UnityEngine.Object) aircraft.weaponManager == (UnityEngine.Object) null)
      {
        QOLPlugin.logger.LogError((object) "aircraft.weaponManager is null!");
        flag1 = true;
      }
      if (flag1)
      {
        QOLPlugin.logger.LogError((object) "Null reference detected");
        __result = (GameObject) null;
        return false;
      }
      QOLPlugin.logger.LogInfo((object) "proceeding with original method");
      return true;
    }
  }

  [HarmonyPatch(typeof (MountedCargo), "MountedCargo_OnPartDetached")]
  private class MountedCargo_OnPartDetached_Patch
  {
    [HarmonyPrepare]
    private static bool Prepare() => false;

    private static bool Prefix(MountedCargo __instance, UnitPart part) => false;
  }

  [HarmonyPatch(typeof (MountedCargo))]
  public class MountedCargo_RailPatch
  {
    [HarmonyPatch("RailLaunch")]
    [HarmonyPrefix]
    public static bool Prefix(MountedCargo __instance)
    {
      if (WeaponMountConfigs.CustomTrailerMounts.Contains<string>(__instance.gameObject.transform.parent.name))
      {
        Traverse traverse1 = Traverse.Create((object) __instance).Field("mountedPosition");
        Traverse traverse2 = Traverse.Create((object) __instance).Field("railVector");
        Vector3 vector3 = traverse1.GetValue<Vector3>() + traverse2.GetValue<Vector3>().normalized * 33f;
        __instance.transform.localPosition = vector3;
      }
      return true;
    }
  }

  [HarmonyPatch(typeof (Encyclopedia), "AfterLoad", new System.Type[] {})]
  public static class Encyclopedia_AfterLoad_Diagnostics
  {
    [HarmonyPrepare]
    private static bool Prepare() => true;

    [HarmonyPostfix]
    public static void Postfix(Encyclopedia __instance)
    {
      QOLPlugin.WriteDiagnostics("ENCYCLOPEDIA AIRCRAFT DATA");
      QOLPlugin.WriteDiagnostics("-".PadRight(60, '-'));
      List<AircraftDefinition> aircraft = __instance.aircraft;
      if (aircraft == null)
        throw new Exception("[LoadoutDiagnostics] Encyclopedia.aircraft is null!");
      QOLPlugin.WriteDiagnostics($"Found {aircraft.Count} aircraft definitions\n");
      foreach (AircraftDefinition aircraftDefinition in aircraft)
      {
        if (!((UnityEngine.Object) aircraftDefinition == (UnityEngine.Object) null))
        {
          string str = aircraftDefinition.jsonKey ?? aircraftDefinition.name ?? "unknown";
          GameObject unitPrefab = aircraftDefinition.unitPrefab;
          QOLPlugin.WriteDiagnostics("\nAIRCRAFT: " + str);
          if ((UnityEngine.Object) unitPrefab == (UnityEngine.Object) null)
          {
            QOLPlugin.WriteDiagnostics("  ERROR: unitPrefab is null");
          }
          else
          {
            QOLPlugin.WriteDiagnostics("  Prefab Name: " + unitPrefab.name);
            WeaponManager componentInChildren = unitPrefab.GetComponentInChildren<WeaponManager>();
            if ((UnityEngine.Object) componentInChildren == (UnityEngine.Object) null)
            {
              QOLPlugin.WriteDiagnostics("  No WeaponManager found");
            }
            else
            {
              HardpointSet[] hardpointSets = componentInChildren.hardpointSets;
              if (hardpointSets == null)
              {
                QOLPlugin.WriteDiagnostics("  No hardpointSets");
              }
              else
              {
                QOLPlugin.WriteDiagnostics($"  Hardpoint Count: {hardpointSets.Length}");
                for (int index = 0; index < hardpointSets.Length; ++index)
                {
                  HardpointSet hardpointSet = hardpointSets[index];
                  if (hardpointSet == null)
                  {
                    QOLPlugin.WriteDiagnostics($"    [{index}] NULL");
                  }
                  else
                  {
                    List<WeaponMount> weaponOptions = hardpointSet.weaponOptions;
                    // ISSUE: explicit non-virtual call
                    int count = weaponOptions != null ? __nonvirtual (weaponOptions.Count) : 0;
                    QOLPlugin.WriteDiagnostics($"    [{index}] {hardpointSet.name ?? "unnamed"} - {count} options:");
                    if (weaponOptions != null)
                    {
                      foreach (UnityEngine.Object @object in weaponOptions)
                        QOLPlugin.WriteDiagnostics("        - " + (@object?.name ?? "null"));
                    }
                  }
                }
                AircraftParameters aircraftParameters = aircraftDefinition.aircraftParameters;
                if (aircraftParameters?.loadouts != null && aircraftParameters.loadouts.Count > 0)
                {
                  QOLPlugin.WriteDiagnostics($"  Preset Loadouts ({aircraftParameters.loadouts.Count}):");
                  for (int index1 = 0; index1 < aircraftParameters.loadouts.Count; ++index1)
                  {
                    Loadout loadout = aircraftParameters.loadouts[index1];
                    if (loadout?.weapons != null)
                    {
                      QOLPlugin.WriteDiagnostics($"    Loadout[{index1}] ({loadout.weapons.Count} weapons):");
                      for (int index2 = 0; index2 < loadout.weapons.Count; ++index2)
                      {
                        WeaponMount weapon = loadout.weapons[index2];
                        QOLPlugin.WriteDiagnostics($"      [{index2}] {((UnityEngine.Object) weapon != (UnityEngine.Object) null ? (object) weapon.name : (object) "null")}");
                      }
                    }
                  }
                }
              }
            }
          }
        }
      }
      QOLPlugin.WriteDiagnostics("\n" + "-".PadRight(60, '-'));
      QOLPlugin.WriteDiagnostics("WEAPON LOOKUP KEYS");
      QOLPlugin.WriteDiagnostics("-".PadRight(60, '-'));
      List<string> stringList = Encyclopedia.WeaponLookup != null ? new List<string>((IEnumerable<string>) Encyclopedia.WeaponLookup.Keys) : throw new Exception("[LoadoutDiagnostics] Encyclopedia.WeaponLookup is null!");
      stringList.Sort();
      foreach (string str in stringList)
        QOLPlugin.WriteDiagnostics("  " + str);
      QOLPlugin.WriteDiagnostics($"\nTotal weapons in Encyclopedia: {Encyclopedia.WeaponLookup.Count}");
      QOLPlugin.FlushDiagnostics();
      QOLPlugin.logger.LogInfo((object) "[LoadoutDiagnostics] Encyclopedia data written successfully");
    }
  }

  [HarmonyPatch(typeof (SavedLoadout), "CreateLoadout", new System.Type[] {typeof (WeaponManager)})]
  public static class SavedLoadout_CreateLoadout_Diagnostics
  {
    [HarmonyPrepare]
    private static bool Prepare() => true;

    [HarmonyPrefix]
    public static void Prefix(SavedLoadout __instance, WeaponManager weaponManager)
    {
      try
      {
        string str1 = "Unknown";
        Aircraft aircraft = typeof (WeaponManager).GetField("aircraft", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue((object) weaponManager) as Aircraft;
        if ((UnityEngine.Object) aircraft != (UnityEngine.Object) null)
          str1 = aircraft.name?.Replace("(Clone)", "").Trim() ?? "Unknown";
        QOLPlugin.WriteDiagnostics("\n" + "=".PadRight(60, '='));
        QOLPlugin.WriteDiagnostics("SAVED LOADOUT -> CREATE LOADOUT");
        QOLPlugin.WriteDiagnostics("Aircraft: " + str1);
        QOLPlugin.WriteDiagnostics("-".PadRight(60, '-'));
        List<SavedLoadout.SelectedMount> selected = __instance.Selected;
        if (selected != null)
        {
          QOLPlugin.WriteDiagnostics($"SavedLoadout.Selected ({selected.Count} items):");
          for (int index = 0; index < selected.Count; ++index)
          {
            string str2 = selected[index].Key ?? "null";
            QOLPlugin.WriteDiagnostics($"  [{index}] Key=\"{str2}\"");
          }
        }
        else
          QOLPlugin.WriteDiagnostics("SavedLoadout.Selected is NULL");
        HardpointSet[] hardpointSets = weaponManager?.hardpointSets;
        if (hardpointSets != null)
        {
          QOLPlugin.WriteDiagnostics($"\nWeaponManager hardpointSets ({hardpointSets.Length} slots):");
          for (int index = 0; index < hardpointSets.Length; ++index)
          {
            HardpointSet hardpointSet = hardpointSets[index];
            if (hardpointSet == null)
            {
              QOLPlugin.WriteDiagnostics($"  [{index}] NULL");
            }
            else
            {
              List<WeaponMount> weaponOptions = hardpointSet.weaponOptions;
              // ISSUE: explicit non-virtual call
              int count = weaponOptions != null ? __nonvirtual (weaponOptions.Count) : 0;
              QOLPlugin.WriteDiagnostics($"  [{index}] {hardpointSet.name ?? "unnamed"} - {count} options");
            }
          }
        }
        QOLPlugin.WriteDiagnostics("-".PadRight(60, '-'));
        QOLPlugin.FlushDiagnostics();
      }
      catch (Exception ex)
      {
        QOLPlugin.WriteDiagnostics($"ERROR in CreateLoadout diagnostics: {ex}");
        QOLPlugin.FlushDiagnostics();
      }
    }
  }

  [HarmonyPatch(typeof (Aircraft))]
  public static class Aircraft_Loadout_Diagnostics
  {
    [HarmonyPrepare]
    private static bool Prepare() => true;

    [HarmonyPatch("OnStartServer")]
    [HarmonyPrefix]
    public static void OnStartServerPrefix(Aircraft __instance)
    {
      QOLPlugin.Aircraft_Loadout_Diagnostics.LogAircraftLoadout(__instance, "OnStartServer-PRE");
    }

    [HarmonyPatch("OnStartServer")]
    [HarmonyPostfix]
    public static void OnStartServerPostfix(Aircraft __instance)
    {
      QOLPlugin.Aircraft_Loadout_Diagnostics.LogAircraftLoadout(__instance, "OnStartServer-POST");
    }

    [HarmonyPatch("OnStartClient")]
    [HarmonyPrefix]
    public static void OnStartClientPrefix(Aircraft __instance)
    {
      QOLPlugin.Aircraft_Loadout_Diagnostics.LogAircraftLoadout(__instance, "OnStartClient-PRE");
    }

    [HarmonyPatch("OnStartClient")]
    [HarmonyPostfix]
    public static void OnStartClientPostfix(Aircraft __instance)
    {
      QOLPlugin.Aircraft_Loadout_Diagnostics.LogAircraftLoadout(__instance, "OnStartClient-POST");
    }

    private static void LogAircraftLoadout(Aircraft aircraft, string phase)
    {
      try
      {
        string str = aircraft.name?.Replace("(Clone)", "").Trim() ?? "Unknown";
        Loadout loadout = typeof (Aircraft).GetField("loadout", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue((object) aircraft) as Loadout;
        QOLPlugin.WriteDiagnostics($"\n[{phase}] {str}");
        if (loadout?.weapons == null)
        {
          QOLPlugin.WriteDiagnostics("  Loadout: NULL or no weapons");
        }
        else
        {
          QOLPlugin.WriteDiagnostics($"  Loadout ({loadout.weapons.Count} weapons):");
          for (int index = 0; index < loadout.weapons.Count; ++index)
          {
            WeaponMount weapon = loadout.weapons[index];
            QOLPlugin.WriteDiagnostics($"    [{index}] {((UnityEngine.Object) weapon != (UnityEngine.Object) null ? (object) weapon.name : (object) "null")}");
          }
        }
        WeaponManager componentInChildren = aircraft.GetComponentInChildren<WeaponManager>();
        if (componentInChildren?.hardpointSets != null)
          QOLPlugin.WriteDiagnostics($"  WeaponManager hardpoints: {componentInChildren.hardpointSets.Length}");
        QOLPlugin.FlushDiagnostics();
      }
      catch (Exception ex)
      {
        QOLPlugin.WriteDiagnostics("ERROR in Aircraft diagnostics: " + ex.Message);
        QOLPlugin.FlushDiagnostics();
      }
    }
  }

  [HarmonyPatch(typeof (Aircraft), "Awake")]
  public static class Aircraft_Awake_RuntimeDiagnostics
  {
    private static HashSet<string> loggedAircraft = new HashSet<string>();
    private static bool headerWritten = false;

    private static string RuntimeDiagnosticsPath
    {
      get => Path.Combine(Paths.BepInExRootPath, "LoadoutDiagnostics_Runtime.txt");
    }

    [HarmonyPrepare]
    private static bool Prepare() => true;

    [HarmonyPostfix]
    public static void Postfix(Aircraft __instance)
    {
      try
      {
        string str1 = __instance.name?.Replace("(Clone)", "").Trim() ?? "Unknown";
        if (QOLPlugin.Aircraft_Awake_RuntimeDiagnostics.loggedAircraft.Contains(str1))
          return;
        QOLPlugin.Aircraft_Awake_RuntimeDiagnostics.loggedAircraft.Add(str1);
        StringBuilder stringBuilder = new StringBuilder();
        if (!QOLPlugin.Aircraft_Awake_RuntimeDiagnostics.headerWritten)
        {
          stringBuilder.AppendLine("=".PadRight(80 /*0x50*/, '='));
          stringBuilder.AppendLine($"RUNTIME AIRCRAFT DIAGNOSTICS - {DateTime.Now}");
          stringBuilder.AppendLine("(Captured from actual spawned aircraft instances)");
          stringBuilder.AppendLine("=".PadRight(80 /*0x50*/, '='));
          stringBuilder.AppendLine();
          QOLPlugin.Aircraft_Awake_RuntimeDiagnostics.headerWritten = true;
        }
        WeaponManager componentInChildren = __instance.GetComponentInChildren<WeaponManager>();
        if ((UnityEngine.Object) componentInChildren == (UnityEngine.Object) null)
        {
          stringBuilder.AppendLine($"\nAIRCRAFT: {str1} - No WeaponManager");
          File.AppendAllText(QOLPlugin.Aircraft_Awake_RuntimeDiagnostics.RuntimeDiagnosticsPath, stringBuilder.ToString());
        }
        else
        {
          HardpointSet[] hardpointSets = componentInChildren.hardpointSets;
          stringBuilder.AppendLine("\nAIRCRAFT: " + str1);
          stringBuilder.AppendLine($"  Hardpoint Count: {(hardpointSets != null ? hardpointSets.Length : 0)}");
          if (hardpointSets != null)
          {
            for (int index = 0; index < hardpointSets.Length; ++index)
            {
              HardpointSet hardpointSet = hardpointSets[index];
              if (hardpointSet == null)
              {
                stringBuilder.AppendLine($"    [{index}] NULL");
              }
              else
              {
                List<WeaponMount> weaponOptions = hardpointSet.weaponOptions;
                // ISSUE: explicit non-virtual call
                int count = weaponOptions != null ? __nonvirtual (weaponOptions.Count) : 0;
                stringBuilder.AppendLine($"    [{index}] {hardpointSet.name ?? "unnamed"} - {count} options:");
                if (weaponOptions != null)
                {
                  foreach (UnityEngine.Object @object in weaponOptions)
                  {
                    string str2 = @object?.name ?? "null";
                    stringBuilder.AppendLine("        - " + str2);
                  }
                }
              }
            }
          }
          File.AppendAllText(QOLPlugin.Aircraft_Awake_RuntimeDiagnostics.RuntimeDiagnosticsPath, stringBuilder.ToString());
          QOLPlugin.logger.LogInfo((object) ("[LoadoutDiagnostics] Logged runtime config for " + str1));
        }
      }
      catch (Exception ex)
      {
        QOLPlugin.logger.LogError((object) ("[LoadoutDiagnostics] Runtime diagnostic error: " + ex.Message));
      }
    }
  }

  [HarmonyPatch(typeof (MainMenu), "Start")]
  public static class MainMenu_Start_ModdedDiagnostics
  {
    private static string ModdedDiagnosticsPath
    {
      get => Path.Combine(Paths.BepInExRootPath, "LoadoutDiagnostics_Modded.txt");
    }

    [HarmonyPrepare]
    private static bool Prepare() => true;

    [HarmonyPostfix]
    public static void Postfix()
    {
      StringBuilder stringBuilder = new StringBuilder();
      stringBuilder.AppendLine("=".PadRight(80 /*0x50*/, '='));
      stringBuilder.AppendLine($"MODDED LOADOUT DIAGNOSTICS - {DateTime.Now}");
      stringBuilder.AppendLine("(After commands.qol applied)");
      stringBuilder.AppendLine("=".PadRight(80 /*0x50*/, '='));
      stringBuilder.AppendLine();
      Encyclopedia encyclopedia = ((IEnumerable<Encyclopedia>) UnityEngine.Resources.FindObjectsOfTypeAll<Encyclopedia>()).FirstOrDefault<Encyclopedia>();
      if ((UnityEngine.Object) encyclopedia == (UnityEngine.Object) null)
      {
        stringBuilder.AppendLine("ERROR: Could not find Encyclopedia instance!");
        File.WriteAllText(QOLPlugin.MainMenu_Start_ModdedDiagnostics.ModdedDiagnosticsPath, stringBuilder.ToString());
      }
      else
      {
        stringBuilder.AppendLine($"Found {encyclopedia.aircraft.Count} aircraft definitions\n");
        foreach (AircraftDefinition aircraftDefinition in encyclopedia.aircraft)
        {
          if (!((UnityEngine.Object) aircraftDefinition == (UnityEngine.Object) null))
          {
            string str1 = aircraftDefinition.jsonKey ?? aircraftDefinition.name ?? "unknown";
            GameObject unitPrefab = aircraftDefinition.unitPrefab;
            stringBuilder.AppendLine("\nAIRCRAFT: " + str1);
            if ((UnityEngine.Object) unitPrefab == (UnityEngine.Object) null)
            {
              stringBuilder.AppendLine("  ERROR: unitPrefab is null");
            }
            else
            {
              stringBuilder.AppendLine("  Prefab Name: " + unitPrefab.name);
              WeaponManager componentInChildren = unitPrefab.GetComponentInChildren<WeaponManager>();
              if ((UnityEngine.Object) componentInChildren == (UnityEngine.Object) null)
              {
                stringBuilder.AppendLine("  No WeaponManager found");
              }
              else
              {
                HardpointSet[] hardpointSets = componentInChildren.hardpointSets;
                if (hardpointSets == null)
                {
                  stringBuilder.AppendLine("  No hardpointSets");
                }
                else
                {
                  stringBuilder.AppendLine($"  Hardpoint Count: {hardpointSets.Length}");
                  for (int index = 0; index < hardpointSets.Length; ++index)
                  {
                    HardpointSet hardpointSet = hardpointSets[index];
                    if (hardpointSet == null)
                    {
                      stringBuilder.AppendLine($"    [{index}] NULL");
                    }
                    else
                    {
                      List<WeaponMount> weaponOptions = hardpointSet.weaponOptions;
                      // ISSUE: explicit non-virtual call
                      int count = weaponOptions != null ? __nonvirtual (weaponOptions.Count) : 0;
                      stringBuilder.AppendLine($"    [{index}] {hardpointSet.name ?? "unnamed"} - {count} options:");
                      if (weaponOptions != null)
                      {
                        foreach (UnityEngine.Object @object in weaponOptions)
                        {
                          string str2 = @object?.name ?? "null";
                          stringBuilder.AppendLine("        - " + str2);
                        }
                      }
                    }
                  }
                  AircraftParameters aircraftParameters = aircraftDefinition.aircraftParameters;
                  if (aircraftParameters?.loadouts != null && aircraftParameters.loadouts.Count > 0)
                  {
                    stringBuilder.AppendLine($"  Preset Loadouts ({aircraftParameters.loadouts.Count}):");
                    for (int index1 = 0; index1 < aircraftParameters.loadouts.Count; ++index1)
                    {
                      Loadout loadout = aircraftParameters.loadouts[index1];
                      if (loadout?.weapons != null)
                      {
                        stringBuilder.AppendLine($"    Loadout[{index1}] ({loadout.weapons.Count} weapons):");
                        for (int index2 = 0; index2 < loadout.weapons.Count; ++index2)
                        {
                          WeaponMount weapon = loadout.weapons[index2];
                          stringBuilder.AppendLine($"      [{index2}] {((UnityEngine.Object) weapon != (UnityEngine.Object) null ? (object) weapon.name : (object) "null")}");
                        }
                      }
                    }
                  }
                }
              }
            }
          }
        }
        File.WriteAllText(QOLPlugin.MainMenu_Start_ModdedDiagnostics.ModdedDiagnosticsPath, stringBuilder.ToString());
        QOLPlugin.logger.LogInfo((object) ("[LoadoutDiagnostics] Modded config written to " + QOLPlugin.MainMenu_Start_ModdedDiagnostics.ModdedDiagnosticsPath));
      }
    }
  }

  [HarmonyPatch]
  public static class ShockwaveEffectsPatch
  {
    [HarmonyPrepare]
    private static bool Prepare() => false;

    [HarmonyPatch(typeof (Missile.Warhead), "Detonate")]
    [HarmonyPostfix]
    private static void Detonate_Postfix(
      Rigidbody rb,
      PersistentID ownerID,
      Vector3 position,
      Vector3 normal,
      bool armed,
      float blastYield,
      bool hitArmor,
      bool hitTerrain)
    {
      QOLPlugin.logger.LogInfo((object) "Patching warhead detonation");
      try
      {
        if (!armed || (double) blastYield <= 100.0)
          return;
        QOLPlugin.logger.LogInfo((object) "Warhead met arm and yield conditions");
        float yield = (float) ((double) blastYield * 9.9999999747524271E-07 * 2.0);
        GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(QOLPlugin.standardShockwave_GO);
        gameObject.transform.position = position;
        Shockwave shockwave = gameObject.AddComponent<Shockwave>();
        if (!((UnityEngine.Object) shockwave != (UnityEngine.Object) null))
          return;
        QOLPlugin.logger.LogInfo((object) "Got shockwave");
        FieldInfo field1 = typeof (Shockwave).GetField("yieldKilotons", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (field1 != (FieldInfo) null)
        {
          QOLPlugin.logger.LogInfo((object) $"Setting yield to {blastYield} as {yield}");
          field1.SetValue((object) shockwave, (object) yield);
        }
        FieldInfo field2 = typeof (Shockwave).GetField("vaporCloud", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (field2 != (FieldInfo) null)
        {
          QOLPlugin.logger.LogInfo((object) $"Setting {shockwave} vaporCloud to {gameObject}");
          field2.SetValue((object) shockwave, (object) gameObject);
        }
        FieldInfo field3 = typeof (Shockwave).GetField("vaporCloudAlpha", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (field3 != (FieldInfo) null)
          field3.SetValue((object) shockwave, field3.GetValue((object) PathLookup.Find("explosion_250kt").GetComponent<Shockwave>()));
        shockwave.SetOwner(ownerID, yield);
      }
      catch (Exception ex)
      {
        QOLPlugin.logger.LogError((object) ("Error in ShockwaveEffectsPatch: " + ex.Message));
      }
    }
  }

  [HarmonyPatch]
  public static class ShockwaveDistortionFadeout
  {
    [HarmonyPrepare]
    private static bool Prepare() => false;

    [HarmonyPatch(typeof (Shockwave), "Update")]
    [HarmonyPrefix]
    private static bool ShockwaveUpdate_Prefix(Shockwave __instance)
    {
      FieldInfo field1 = typeof (Shockwave).GetField("vaporCloud", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
      QOLPlugin.logger.LogInfo((object) $"vaporField is '{field1}'");
      GameObject gameObject = (GameObject) field1.GetValue((object) __instance);
      QOLPlugin.logger.LogInfo((object) $"vaporCloud is '{gameObject}' and Shockwave parent is '{__instance.gameObject.name}'");
      if (gameObject.GetComponent<MeshRenderer>().sharedMaterial.name.Contains("HeatDistortion"))
      {
        FieldInfo field2 = typeof (Shockwave).GetField("dustOpacity", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        QOLPlugin.logger.LogInfo((object) $"opacityField is '{field2}'");
        float a = (float) field2.GetValue((object) __instance);
        gameObject.GetComponent<MeshRenderer>().sharedMaterial.color = new Color(1f, 1f, 1f, Mathf.Min(a, 0.0f));
        QOLPlugin.logger.LogInfo((object) $"vaporCloud {gameObject} set opacity to {a}");
        if ((double) a <= 0.0)
        {
          UnityEngine.Object.Destroy((UnityEngine.Object) gameObject);
          UnityEngine.Object.Destroy((UnityEngine.Object) __instance);
        }
      }
      return true;
    }
  }

  [HarmonyPatch]
  [qol.PatchConfig.PatchConfig("Misc", "moon", true, "Fractures the moon")]
  public class MoonBreaker
  {
    [HarmonyPrepare]
    private static bool Prepare() => PatchConfigRegistry.IsEnabled(typeof (QOLPlugin.MoonBreaker));

    [HarmonyPatch(typeof (LevelInfo), "ApplyMapSettings")]
    [HarmonyPostfix]
    private static void ApplyMapSettingsPostfix(LevelInfo __instance)
    {
      QOLPlugin.MoonBreaker.ReplaceMoonMesh(__instance);
    }

    [HarmonyPatch(typeof (LevelInfo), "LoadFromMission")]
    [HarmonyPostfix]
    private static void LoadFromMissionPostfix(LevelInfo __instance)
    {
      QOLPlugin.MoonBreaker.ReplaceMoonMesh(__instance);
    }

    private static void ReplaceMoonMesh(LevelInfo __instance)
    {
      try
      {
        MeshFilter component1 = __instance.moonObject.GetComponent<MeshFilter>();
        if ((component1 != null ? (component1.mesh?.name?.Contains("ReplacedMoon").GetValueOrDefault() ? 1 : 0) : 0) != 0)
          return;
        FieldInfo field = typeof (LevelInfo).GetField("skyAnimData", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        LevelInfo.SkyAnimData skyAnimData = (LevelInfo.SkyAnimData) field.GetValue((object) __instance);
        skyAnimData.hazeAmount = 0.2f;
        field.SetValue((object) __instance, (object) skyAnimData);
        Mesh mesh = QOLPlugin.LoadFirstAssetFromBundle<Mesh>(QOLPlugin.GetBundlePath("FBXMod.Moon.bundle"));
        if ((UnityEngine.Object) mesh == (UnityEngine.Object) null)
          return;
        MeshFilter component2 = __instance.moonObject.GetComponent<MeshFilter>();
        if (!((UnityEngine.Object) component2 != (UnityEngine.Object) null))
          return;
        component2.mesh = mesh;
        mesh.name = "ReplacedMoon";
        QOLPlugin.logger.LogDebug((object) "Replaced moon mesh");
      }
      catch (Exception ex)
      {
        QOLPlugin.logger.LogError((object) $"Error replacing moon mesh: {ex}");
      }
    }
  }

  [HarmonyPatch]
  [qol.PatchConfig.PatchConfig("Graphics", "slimelights", true, "Forces slimelights to always stay on despite navlights toggle")]
  public static class NavLightsPatch2
  {
    [HarmonyPrepare]
    private static bool Prepare()
    {
      return PatchConfigRegistry.IsEnabled(typeof (QOLPlugin.NavLightsPatch2));
    }

    private static MethodBase TargetMethod()
    {
      return (MethodBase) AccessTools.Method(AccessTools.FirstInner(typeof (NavLights), (Func<System.Type, bool>) (inner => inner.Name.Contains("NavLight"))), "Toggle", new System.Type[1]
      {
        typeof (bool)
      }, (System.Type[]) null);
    }

    [HarmonyPrefix]
    public static bool Prefix_Toggle(object __instance, bool enabled)
    {
      FieldInfo fieldInfo1 = AccessTools.Field(__instance.GetType(), "renderer");
      FieldInfo fieldInfo2 = AccessTools.Field(__instance.GetType(), "litMaterial");
      FieldInfo fieldInfo3 = AccessTools.Field(__instance.GetType(), "objects");
      UnityEngine.Renderer renderer = (UnityEngine.Renderer) fieldInfo1?.GetValue(__instance);
      Material material = (Material) fieldInfo2?.GetValue(__instance);
      GameObject[] gameObjectArray = (GameObject[]) fieldInfo3?.GetValue(__instance);
      if (!((UnityEngine.Object) material != (UnityEngine.Object) null) || material.name.IndexOf("slime", StringComparison.OrdinalIgnoreCase) < 0)
        return true;
      if ((UnityEngine.Object) renderer != (UnityEngine.Object) null)
        renderer.material = material;
      if (gameObjectArray != null)
      {
        foreach (GameObject gameObject in gameObjectArray)
        {
          if ((UnityEngine.Object) gameObject != (UnityEngine.Object) null)
            gameObject.SetActive(enabled);
        }
      }
      return false;
    }
  }

  [HarmonyPatch(typeof (VaporEmitter))]
  [HarmonyPatch("Emit")]
  public static class VaporEmitter_SmoothAlpha
  {
    [HarmonyPrefix]
    public static void Prefix(
      VaporEmitter __instance,
      ref float alpha,
      float airspeed,
      Vector3 velocity,
      float altitude)
    {
      alpha *= 2f;
      alpha = Mathf.Clamp(alpha, -90f, 90f);
    }

    [HarmonyPostfix]
    public static void Postfix(
      VaporEmitter __instance,
      float alpha,
      float airspeed,
      Vector3 velocity,
      float altitude)
    {
      if ((bool) Traverse.Create((object) __instance).Field("contrail").GetValue())
        return;
      float num = __instance.alphaThresholdVelocity.Evaluate(airspeed);
      float t = Mathf.Clamp01((float) (((double) Mathf.Abs(alpha) - (double) num) / 10.0));
      ParticleSystem.MainModule main = __instance.particles.main;
      ParticleSystem.MinMaxGradient startColor = main.startColor;
      if (startColor.mode != ParticleSystemGradientMode.Color)
        return;
      Color color = startColor.color with
      {
        a = Mathf.Lerp(0.0f, __instance.opacity, t)
      };
      main.startColor = (ParticleSystem.MinMaxGradient) color;
    }
  }

  [HarmonyPatch(typeof (Turbojet))]
  public static class Turbojet_SplitThrottle_Patch
  {
    public static Dictionary<string, (float splitThrust, float maxModifier)> EngineSettings = new Dictionary<string, (float, float)>();
    private static Dictionary<Turbojet, float> originalThrottles = new Dictionary<Turbojet, float>();

    [HarmonyPrepare]
    private static bool Prepare() => false;

    public static void ConfigureEngineByPattern(
      string namePattern,
      float splitThrust,
      float maxModifier = 0.5f)
    {
      QOLPlugin.Turbojet_SplitThrottle_Patch.EngineSettings[namePattern.ToLower()] = (splitThrust, Mathf.Clamp(maxModifier, 0.0f, 1f));
    }

    [HarmonyPatch("FixedUpdate")]
    [HarmonyPrefix]
    public static void FixedUpdate_Prefix(Turbojet __instance)
    {
      string lower = __instance.gameObject.name.ToLower();
      (float, float) valueTuple = (0.0f, 0.5f);
      bool flag = false;
      foreach (KeyValuePair<string, (float splitThrust, float maxModifier)> engineSetting in QOLPlugin.Turbojet_SplitThrottle_Patch.EngineSettings)
      {
        if (lower.Contains(engineSetting.Key))
        {
          valueTuple = engineSetting.Value;
          flag = true;
          break;
        }
      }
      if (!flag || (double) Mathf.Abs(valueTuple.Item1) < 0.0099999997764825821)
        return;
      FieldInfo fieldInfo = AccessTools.Field(typeof (Turbojet), "controlInputs");
      if (fieldInfo == (FieldInfo) null || !(fieldInfo.GetValue((object) __instance) is ControlInputs controlInputs))
        return;
      float num = Mathf.Clamp(-controlInputs.yaw * valueTuple.Item1 * valueTuple.Item2, -valueTuple.Item2, valueTuple.Item2);
      float throttle = controlInputs.throttle;
      controlInputs.throttle = Mathf.Clamp01(throttle * (1f + num));
      QOLPlugin.Turbojet_SplitThrottle_Patch.originalThrottles[__instance] = throttle;
    }

    [HarmonyPatch("FixedUpdate")]
    [HarmonyPostfix]
    public static void FixedUpdate_Postfix(Turbojet __instance)
    {
      float num;
      if (!QOLPlugin.Turbojet_SplitThrottle_Patch.originalThrottles.TryGetValue(__instance, out num))
        return;
      FieldInfo fieldInfo = AccessTools.Field(typeof (Turbojet), "controlInputs");
      if (fieldInfo != (FieldInfo) null && fieldInfo.GetValue((object) __instance) is ControlInputs controlInputs)
        controlInputs.throttle = num;
      QOLPlugin.Turbojet_SplitThrottle_Patch.originalThrottles.Remove(__instance);
    }

    [HarmonyPatch(typeof (JetNozzle), "Thrust")]
    private static class JetNozzleBlendShapes
    {
      private static void Postfix(
        JetNozzle __instance,
        float thrustAmount,
        float rpmRatio,
        float thrustRatio,
        float throttle,
        bool allowAfterburner)
      {
        SkinnedMeshRenderer component = __instance?.GetComponent<SkinnedMeshRenderer>();
        if ((UnityEngine.Object) component == (UnityEngine.Object) null)
          return;
        float b = thrustRatio * 100f;
        float blendShapeWeight = component.GetBlendShapeWeight(0);
        if ((double) throttle > 0.89999997615814209 & allowAfterburner)
        {
          float t = 1f;
          b = Mathf.Lerp(thrustRatio * 100f, 0.0f, t);
        }
        float num = (double) blendShapeWeight < (double) b ? Mathf.Min(blendShapeWeight + 1f, b) : Mathf.Max(blendShapeWeight - 1f, b);
        component.SetBlendShapeWeight(0, num);
      }
    }
  }

  [HarmonyPatch(typeof (Turbojet))]
  [HarmonyPatch("SetParasiticLoss")]
  public static class ParasiticLossPatch
  {
    [HarmonyPrepare]
    private static bool Prepare() => false;

    private static bool Prefix(ref float loss)
    {
      loss *= 0.5f;
      return true;
    }
  }

  [HarmonyPatch(typeof (RotorShaft), "RotorShaft_OnSpawnedInPosition")]
  [qol.PatchConfig.PatchConfig("Misc", "neutralFold", true, "Prevent neutral aircraft from deploying foldables, such as rotors")]
  public static class RotorShaftPatche
  {
    [HarmonyPrepare]
    private static bool Prepare()
    {
      return PatchConfigRegistry.IsEnabled(typeof (QOLPlugin.RotorShaftPatche));
    }

    [HarmonyPrefix]
    private static bool PreventUnfoldForNeutral(RotorShaft __instance)
    {
      Aircraft aircraft = __instance.aircraft;
      return (UnityEngine.Object) aircraft == (UnityEngine.Object) null || !((UnityEngine.Object) aircraft.NetworkHQ == (UnityEngine.Object) null);
    }
  }

  [HarmonyPatch(typeof (ControlSurface))]
  [HarmonyPatch("UpdateJobFields")]
  private class CanardTest
  {
    [HarmonyPostfix]
    private static void Postfix(ControlSurface __instance)
    {
      if (!__instance.gameObject.name.Contains("canard"))
        return;
      Aircraft aircraft = (Aircraft) Traverse.Create((object) __instance).Field("aircraft").GetValue();
      if ((UnityEngine.Object) aircraft == (UnityEngine.Object) null || (double) aircraft.radarAlt < 2.0)
        return;
      Vector3 velocity = aircraft.cockpit.rb.velocity;
      Vector3 vector3 = aircraft.cockpit.transform.InverseTransformDirection(velocity);
      float num1 = Mathf.Clamp(Mathf.Atan2(-vector3.y, vector3.z) * 57.29578f, -50f, 50f) * 0.9f;
      Transform transform = __instance.transform.Find("New Game Object");
      if ((UnityEngine.Object) transform == (UnityEngine.Object) null)
        return;
      float x = transform.localRotation.eulerAngles.x;
      if ((double) x > 180.0)
        x -= 360f;
      float num2 = Mathf.Clamp(x, -50f, 50f);
      float num3 = Mathf.Clamp(num1 - num2, -1f, 1f);
      transform.localRotation = Quaternion.Euler(num2 + num3, 0.0f, 0.0f);
    }
  }

  [HarmonyPatch(typeof (RotorShaft))]
  [HarmonyPatch("Awake")]
  public static class RotorShaft_Awake_Patch
  {
    private static FieldInfo rotorsField;
    private static FieldInfo unfoldedField;
    private static FieldInfo startupProgressField;
    private static FieldInfo aircraftField;
    private static FieldInfo unitPartField;
    private static FieldInfo rotorHingesField;
    private static MethodInfo unfoldRotorsMethod;
    private static System.Type rotorHingeType;
    private static FieldInfo hingeHingeField;
    private static FieldInfo hingeFoldAngleField;
    private static FieldInfo hingeFoldSpeedField;
    private static FieldInfo hingeFoldAmountField;
    private static MethodInfo hingeInitializeMethod;
    private static MethodInfo hingeUnfoldMethod;

    [HarmonyPrepare]
    private static bool Prepare() => false;

    static RotorShaft_Awake_Patch()
    {
      System.Type type = typeof (RotorShaft);
      QOLPlugin.RotorShaft_Awake_Patch.rotorsField = type.GetField("rotors", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
      QOLPlugin.RotorShaft_Awake_Patch.unfoldedField = type.GetField("unfolded", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
      QOLPlugin.RotorShaft_Awake_Patch.startupProgressField = type.GetField("startupProgress", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
      QOLPlugin.RotorShaft_Awake_Patch.aircraftField = type.GetField("aircraft", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
      QOLPlugin.RotorShaft_Awake_Patch.unitPartField = type.GetField("unitPart", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
      QOLPlugin.RotorShaft_Awake_Patch.rotorHingesField = type.GetField("rotorHinges", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
      QOLPlugin.RotorShaft_Awake_Patch.unfoldRotorsMethod = type.GetMethod("UnfoldRotors", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
      QOLPlugin.RotorShaft_Awake_Patch.rotorHingeType = type.GetNestedType("RotorHinge", BindingFlags.NonPublic);
      if (QOLPlugin.RotorShaft_Awake_Patch.rotorHingeType != (System.Type) null)
      {
        QOLPlugin.RotorShaft_Awake_Patch.hingeHingeField = QOLPlugin.RotorShaft_Awake_Patch.rotorHingeType.GetField("hinge", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        QOLPlugin.RotorShaft_Awake_Patch.hingeFoldAngleField = QOLPlugin.RotorShaft_Awake_Patch.rotorHingeType.GetField("foldAngle", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        QOLPlugin.RotorShaft_Awake_Patch.hingeFoldSpeedField = QOLPlugin.RotorShaft_Awake_Patch.rotorHingeType.GetField("foldSpeed", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        QOLPlugin.RotorShaft_Awake_Patch.hingeFoldAmountField = QOLPlugin.RotorShaft_Awake_Patch.rotorHingeType.GetField("foldAmount", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        QOLPlugin.RotorShaft_Awake_Patch.hingeInitializeMethod = QOLPlugin.RotorShaft_Awake_Patch.rotorHingeType.GetMethod("Initialize", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        QOLPlugin.RotorShaft_Awake_Patch.hingeUnfoldMethod = QOLPlugin.RotorShaft_Awake_Patch.rotorHingeType.GetMethod("Unfold", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
      }
      else
        UnityEngine.Debug.LogError((object) "[RotorShaft Patch] Could not find RotorHinge nested type!");
    }

    public static bool Prefix(RotorShaft __instance)
    {
      try
      {
        if (QOLPlugin.RotorShaft_Awake_Patch.rotorsField?.GetValue((object) __instance) is SwashRotor[] swashRotorArray && swashRotorArray.Length != 0)
          return true;
        UnityEngine.Debug.LogWarning((object) ("[RotorShaft Patch] Initializing as hinge-only system on " + __instance.gameObject.name));
        QOLPlugin.RotorShaft_Awake_Patch.rotorsField?.SetValue((object) __instance, (object) new SwashRotor[0]);
        Aircraft aircraft = QOLPlugin.RotorShaft_Awake_Patch.aircraftField?.GetValue((object) __instance) as Aircraft;
        QOLPlugin.RotorShaft_Awake_Patch.unitPartField?.GetValue((object) __instance);
        Array rotorHinges = QOLPlugin.RotorShaft_Awake_Patch.rotorHingesField?.GetValue((object) __instance) as Array;
        if ((UnityEngine.Object) aircraft != (UnityEngine.Object) null)
        {
          if ((double) aircraft.radarAlt > (double) aircraft.definition.spawnOffset.y + 1.0)
          {
            QOLPlugin.RotorShaft_Awake_Patch.unfoldedField?.SetValue((object) __instance, (object) true);
            QOLPlugin.RotorShaft_Awake_Patch.startupProgressField?.SetValue((object) __instance, (object) 1f);
            if (rotorHinges != null && QOLPlugin.RotorShaft_Awake_Patch.rotorHingeType != (System.Type) null)
              QOLPlugin.RotorShaft_Awake_Patch.InitializeHinges(rotorHinges, 0.0f);
          }
          else
          {
            QOLPlugin.RotorShaft_Awake_Patch.unfoldedField?.SetValue((object) __instance, (object) false);
            QOLPlugin.RotorShaft_Awake_Patch.startupProgressField?.SetValue((object) __instance, (object) 0.0f);
            if (rotorHinges != null && QOLPlugin.RotorShaft_Awake_Patch.rotorHingeType != (System.Type) null)
              QOLPlugin.RotorShaft_Awake_Patch.InitializeHinges(rotorHinges, 1f);
            if (QOLPlugin.RotorShaft_Awake_Patch.unfoldRotorsMethod != (MethodInfo) null)
              QOLPlugin.RotorShaft_Awake_Patch.unfoldRotorsMethod.Invoke((object) __instance, (object[]) null);
          }
        }
        __instance.gameObject.AddComponent<QOLPlugin.RotorShaftSkipper>().Initialize(__instance);
        return false;
      }
      catch (Exception ex)
      {
        UnityEngine.Debug.LogError((object) $"[RotorShaft Patch] Error in prefix patch: {ex}");
        return true;
      }
    }

    private static void InitializeHinges(Array rotorHinges, float foldAmount)
    {
      if (QOLPlugin.RotorShaft_Awake_Patch.hingeInitializeMethod == (MethodInfo) null)
      {
        UnityEngine.Debug.LogWarning((object) "[RotorShaft Patch] hingeInitializeMethod not found, using manual initialization");
        QOLPlugin.RotorShaft_Awake_Patch.ManualInitializeHinges(rotorHinges, foldAmount);
      }
      else
      {
        float num = UnityEngine.Random.Range(0.8f, 1.2f);
        foreach (object rotorHinge in rotorHinges)
        {
          try
          {
            QOLPlugin.RotorShaft_Awake_Patch.hingeInitializeMethod.Invoke(rotorHinge, new object[2]
            {
              (object) num,
              (object) foldAmount
            });
          }
          catch (Exception ex)
          {
            UnityEngine.Debug.LogError((object) $"[RotorShaft Patch] Error calling Initialize on hinge: {ex}");
            QOLPlugin.RotorShaft_Awake_Patch.ManualInitializeHinges((Array) new object[1]
            {
              rotorHinge
            }, foldAmount);
          }
        }
      }
    }

    private static void ManualInitializeHinges(Array rotorHinges, float foldAmount)
    {
      float num = UnityEngine.Random.Range(0.8f, 1.2f);
      foreach (object rotorHinge in rotorHinges)
      {
        try
        {
          QOLPlugin.RotorShaft_Awake_Patch.hingeFoldSpeedField?.SetValue(rotorHinge, (object) num);
          QOLPlugin.RotorShaft_Awake_Patch.hingeFoldAmountField?.SetValue(rotorHinge, (object) foldAmount);
          Transform transform = QOLPlugin.RotorShaft_Awake_Patch.hingeHingeField?.GetValue(rotorHinge) as Transform;
          Vector3 vector3 = QOLPlugin.RotorShaft_Awake_Patch.hingeFoldAngleField?.GetValue(rotorHinge) as Vector3? ?? Vector3.zero;
          if ((UnityEngine.Object) transform != (UnityEngine.Object) null)
            transform.localEulerAngles = vector3 * Mathf.Clamp01(foldAmount);
        }
        catch (Exception ex)
        {
          UnityEngine.Debug.LogError((object) $"[RotorShaft Patch] Error in manual hinge initialization: {ex}");
        }
      }
    }
  }

  [HarmonyPatch(typeof (SavedLoadout), "CreateLoadout", new System.Type[] {typeof (WeaponManager)})]
  public static class SavedLoadout_CreateLoadout_Patch
  {
    [HarmonyPrefix]
    public static bool Prefix(
      SavedLoadout __instance,
      WeaponManager weaponManager,
      ref Loadout __result)
    {
      if ((UnityEngine.Object) weaponManager == (UnityEngine.Object) null)
      {
        QOLPlugin.logger.LogWarning((object) "[LoadoutPatch] WeaponManager is null, falling back to original");
        return true;
      }
      string aircraftName = QOLPlugin.SavedLoadout_CreateLoadout_Patch.GetAircraftName(weaponManager);
      Loadout loadout = new Loadout();
      loadout.weapons = new List<WeaponMount>();
      List<SavedLoadout.SelectedMount> selected = __instance.Selected;
      if (selected == null || selected.Count == 0)
      {
        QOLPlugin.logger.LogDebug((object) $"[LoadoutPatch] {aircraftName}: No selected weapons in saved loadout");
        __result = loadout;
        return false;
      }
      HardpointSet[] hardpointSets = weaponManager.hardpointSets;
      if (hardpointSets == null)
      {
        QOLPlugin.logger.LogWarning((object) $"[LoadoutPatch] {aircraftName}: No hardpoint sets on WeaponManager");
        __result = loadout;
        return false;
      }
      for (int index = 0; index < selected.Count; ++index)
      {
        string str = selected[index].Key;
        if (string.IsNullOrEmpty(str))
        {
          loadout.weapons.Add((WeaponMount) null);
        }
        else
        {
          string remappedKey;
          if (LoadoutRemapConfigs.TryGetRemappedKey(aircraftName, index, str, out remappedKey))
          {
            if (remappedKey == null)
            {
              QOLPlugin.logger.LogDebug((object) $"[LoadoutPatch] {aircraftName} hardpoint {index}: '{str}' remapped to empty slot");
              loadout.weapons.Add((WeaponMount) null);
              continue;
            }
            QOLPlugin.logger.LogDebug((object) $"[LoadoutPatch] {aircraftName} hardpoint {index}: '{str}' remapped to '{remappedKey}'");
            str = remappedKey;
          }
          WeaponMount weaponMount = QOLPlugin.SavedLoadout_CreateLoadout_Patch.TryGetWeaponMount(weaponManager, hardpointSets, index, str, aircraftName);
          loadout.weapons.Add(weaponMount);
        }
      }
      List<WeaponMount> weaponMountList = LoadoutAdapterConfigs.AdaptLoadout(aircraftName, loadout.weapons, (Func<string, WeaponMount>) (key => QOLPlugin.SavedLoadout_CreateLoadout_Patch.TryEncyclopediaLookup(key)));
      if (weaponMountList != null)
      {
        QOLPlugin.logger.LogInfo((object) $"[LoadoutPatch] {aircraftName}: Adapted vanilla {loadout.weapons.Count}-slot loadout to modded {weaponMountList.Count}-slot:");
        loadout.weapons = weaponMountList;
      }
      else
        QOLPlugin.logger.LogInfo((object) $"[LoadoutPatch] {aircraftName}: Built loadout with {loadout.weapons.Count} weapons (no adaptation needed):");
      for (int index = 0; index < loadout.weapons.Count; ++index)
      {
        WeaponMount weapon = loadout.weapons[index];
        QOLPlugin.logger.LogInfo((object) $"[LoadoutPatch]   [{index}] = {((UnityEngine.Object) weapon != (UnityEngine.Object) null ? (object) weapon.name : (object) "null")}");
      }
      __result = loadout;
      return false;
    }

    private static WeaponMount TryGetWeaponMount(
      WeaponManager manager,
      HardpointSet[] hardpointSets,
      int index,
      string weaponKey,
      string aircraftName)
    {
      if (index < hardpointSets.Length)
      {
        HardpointSet hardpointSet = hardpointSets[index];
        if (hardpointSet != null)
        {
          List<WeaponMount> weaponOptions = hardpointSet.weaponOptions;
          if (weaponOptions != null && weaponOptions.Count > 0)
          {
            foreach (WeaponMount weaponMount in weaponOptions)
            {
              if ((UnityEngine.Object) weaponMount != (UnityEngine.Object) null && weaponMount.name == weaponKey)
              {
                QOLPlugin.logger.LogDebug((object) $"[LoadoutPatch] {aircraftName} hardpoint {index}: Found '{weaponKey}' in weaponOptions");
                return weaponMount;
              }
            }
          }
        }
      }
      WeaponMount weaponMount1 = QOLPlugin.SavedLoadout_CreateLoadout_Patch.TryEncyclopediaLookup(weaponKey);
      if ((UnityEngine.Object) weaponMount1 != (UnityEngine.Object) null)
      {
        QOLPlugin.logger.LogDebug((object) $"[LoadoutPatch] {aircraftName} hardpoint {index}: Found '{weaponKey}' via Encyclopedia fallback");
        return weaponMount1;
      }
      if (index < hardpointSets.Length)
      {
        HardpointSet hardpointSet = hardpointSets[index];
        if (hardpointSet?.weaponOptions != null && hardpointSet.weaponOptions.Count > 0)
        {
          foreach (WeaponMount weaponOption in hardpointSet.weaponOptions)
          {
            if ((UnityEngine.Object) weaponOption != (UnityEngine.Object) null)
            {
              QOLPlugin.logger.LogWarning((object) $"[LoadoutPatch] {aircraftName} hardpoint {index}: '{weaponKey}' not found, using fallback '{weaponOption.name}'");
              return weaponOption;
            }
          }
          QOLPlugin.logger.LogWarning((object) $"[LoadoutPatch] {aircraftName} hardpoint {index}: '{weaponKey}' not found, weaponOptions has {hardpointSet.weaponOptions.Count} entries but all null");
        }
        else
          QOLPlugin.logger.LogWarning((object) $"[LoadoutPatch] {aircraftName} hardpoint {index}: '{weaponKey}' not found, weaponOptions is null or empty");
      }
      QOLPlugin.logger.LogWarning((object) $"[LoadoutPatch] {aircraftName} hardpoint {index}: Failed to find weapon '{weaponKey}' (not in options, Encyclopedia, or fallback)");
      return (WeaponMount) null;
    }

    private static WeaponMount TryEncyclopediaLookup(string weaponKey)
    {
      try
      {
        FieldInfo field = typeof (Encyclopedia).GetField("WeaponLookup", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        if (field == (FieldInfo) null)
        {
          PropertyInfo property = typeof (Encyclopedia).GetProperty("WeaponLookup", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
          WeaponMount weaponMount;
          return property != (PropertyInfo) null && property.GetValue((object) null) is Dictionary<string, WeaponMount> dictionary && dictionary.TryGetValue(weaponKey, out weaponMount) ? weaponMount : (WeaponMount) null;
        }
        if (field.GetValue((object) null) is Dictionary<string, WeaponMount> dictionary1)
        {
          WeaponMount weaponMount;
          if (dictionary1.TryGetValue(weaponKey, out weaponMount))
            return weaponMount;
        }
      }
      catch (Exception ex)
      {
        QOLPlugin.logger.LogError((object) ("[LoadoutPatch] Error accessing Encyclopedia.WeaponLookup: " + ex.Message));
      }
      return (WeaponMount) null;
    }

    private static string GetAircraftName(WeaponManager manager)
    {
      Aircraft aircraft = typeof (WeaponManager).GetField("aircraft", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue((object) manager) as Aircraft;
      if ((UnityEngine.Object) aircraft != (UnityEngine.Object) null)
        return aircraft.name?.Replace("(Clone)", "").Trim() ?? "Unknown";
      Aircraft componentInParent = manager.GetComponentInParent<Aircraft>();
      return (UnityEngine.Object) componentInParent != (UnityEngine.Object) null ? componentInParent.name?.Replace("(Clone)", "").Trim() ?? "Unknown" : manager.name?.Replace("(Clone)", "").Trim() ?? "Unknown";
    }
  }

  [HarmonyPatch(typeof (Aircraft))]
  public static class Aircraft_Loadout_Trace_Patch
  {
    [HarmonyPatch("Awake")]
    [HarmonyPostfix]
    public static void AwakePostfix(Aircraft __instance)
    {
      QOLPlugin.Aircraft_Loadout_Trace_Patch.LogLoadoutState(__instance, "Awake");
    }

    [HarmonyPatch("OnStartServer")]
    [HarmonyPrefix]
    public static void OnStartServerPrefix(Aircraft __instance)
    {
      QOLPlugin.Aircraft_Loadout_Trace_Patch.LogLoadoutState(__instance, "OnStartServer-PRE");
    }

    [HarmonyPatch("OnStartServer")]
    [HarmonyPostfix]
    public static void OnStartServerPostfix(Aircraft __instance)
    {
      QOLPlugin.Aircraft_Loadout_Trace_Patch.LogLoadoutState(__instance, "OnStartServer-POST");
    }

    [HarmonyPatch("OnStartClient")]
    [HarmonyPrefix]
    public static void OnStartClientPrefix(Aircraft __instance)
    {
      QOLPlugin.Aircraft_Loadout_Trace_Patch.LogLoadoutState(__instance, "OnStartClient-PRE");
    }

    [HarmonyPatch("OnStartClient")]
    [HarmonyPostfix]
    public static void OnStartClientPostfix(Aircraft __instance)
    {
      QOLPlugin.Aircraft_Loadout_Trace_Patch.LogLoadoutState(__instance, "OnStartClient-POST");
    }

    private static void LogLoadoutState(Aircraft aircraft, string phase)
    {
      try
      {
        Loadout loadout1 = typeof (Aircraft).GetField("loadout", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue((object) aircraft) as Loadout;
        Loadout loadout2 = typeof (Aircraft).GetProperty("Networkloadout", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue((object) aircraft) as Loadout;
        Loadout loadout3 = typeof (Aircraft).GetProperty("loadout", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue((object) aircraft) as Loadout;
        int? nullable1 = loadout1?.weapons?.Count;
        int num1 = nullable1 ?? -1;
        int? nullable2;
        if (loadout2 == null)
        {
          nullable1 = new int?();
          nullable2 = nullable1;
        }
        else
        {
          List<WeaponMount> weapons = loadout2.weapons;
          if (weapons == null)
          {
            nullable1 = new int?();
            nullable2 = nullable1;
          }
          else
          {
            // ISSUE: explicit non-virtual call
            nullable2 = new int?(__nonvirtual (weapons.Count));
          }
        }
        nullable1 = nullable2;
        int num2 = nullable1 ?? -1;
        int? nullable3;
        if (loadout3 == null)
        {
          nullable1 = new int?();
          nullable3 = nullable1;
        }
        else
        {
          List<WeaponMount> weapons = loadout3.weapons;
          if (weapons == null)
          {
            nullable1 = new int?();
            nullable3 = nullable1;
          }
          else
          {
            // ISSUE: explicit non-virtual call
            nullable3 = new int?(__nonvirtual (weapons.Count));
          }
        }
        nullable1 = nullable3;
        int num3 = nullable1 ?? -1;
        QOLPlugin.logger.LogInfo((object) $"[LoadoutTrace] {aircraft.name} @ {phase}: field={num1}, Network={num2}, public={num3}");
        Loadout loadout4 = loadout1 ?? loadout2 ?? loadout3;
        if (loadout4?.weapons == null)
          return;
        for (int index = 0; index < loadout4.weapons.Count; ++index)
        {
          WeaponMount weapon = loadout4.weapons[index];
          QOLPlugin.logger.LogInfo((object) $"[LoadoutTrace]   [{index}] = {((UnityEngine.Object) weapon != (UnityEngine.Object) null ? (object) weapon.name : (object) "null")}");
        }
      }
      catch (Exception ex)
      {
        QOLPlugin.logger.LogError((object) $"[LoadoutTrace] {aircraft.name} @ {phase}: Error - {ex.Message}");
      }
    }
  }

  [HarmonyPatch(typeof (Encyclopedia), "AfterLoad")]
  public class EncyclopediaPatch
  {
    [HarmonyPatch("AfterLoad", new System.Type[] {})]
    [HarmonyPrefix]
    private static bool Prefix(Encyclopedia __instance)
    {
      QOLPlugin.LoadCustomWeapons(__instance);
      return true;
    }
  }

  [HarmonyPatch(typeof (FlareEjector))]
  public class FlareEjectorAttachPatch
  {
    [HarmonyPostfix]
    [HarmonyPatch("Awake")]
    private static void Postfix(FlareEjector __instance)
    {
      if (!((UnityEngine.Object) __instance.aircraft == (UnityEngine.Object) null))
        return;
      QOLPlugin.logger.LogDebug((object) "Attempting to attach FlareEjector");
      AeroPart aeroPart = __instance.transform.root.GetComponent<AeroPart>();
      for (int index = 0; index < 10; ++index)
      {
        QOLPlugin.logger.LogDebug((object) aeroPart.name);
        PartJoint[] partJointArray = (PartJoint[]) Traverse.Create((object) aeroPart).Field("joints").GetValue();
        if (partJointArray.Length != 0)
          aeroPart = partJointArray[0].connectedPart as AeroPart;
        else
          break;
      }
      __instance.aircraft = aeroPart.gameObject.GetComponentInChildren<Aircraft>();
      foreach (FlareEjector.EjectionPoint ejectionPoint in (FlareEjector.EjectionPoint[]) Traverse.Create((object) __instance).Field("ejectionPoints").GetValue())
        ejectionPoint.part = (UnitPart) __instance.GetComponentInParent<AeroPart>();
      __instance.aircraft.countermeasureManager.RegisterCountermeasure((Countermeasure) __instance);
    }
  }

  [HarmonyPatch(typeof (ModLoader))]
  public static class ReadLiveryMetaNamePatch
  {
    public static MethodBase TargetMethod()
    {
      MethodInfo methodInfo = AccessTools.Method(typeof (ModLoader), "ReadMetaData", new System.Type[2]
      {
        typeof (PublishedFileId_t),
        typeof (string)
      }, (System.Type[]) null);
      if ((object) methodInfo == null)
        return (MethodBase) null;
      return (MethodBase) methodInfo.MakeGenericMethod(typeof (LiveryMetaData));
    }

    [HarmonyPostfix]
    public static void Postfix(ref LiveryMetaData? __result)
    {
      if (!__result.HasValue)
        return;
      LiveryMetaData liveryMetaData = __result.Value;
      liveryMetaData.Aircraft = CollectionExtensions.GetValueOrDefault<string, string>((IReadOnlyDictionary<string, string>) QOLPlugin.newAircraftName, liveryMetaData.Aircraft, liveryMetaData.Aircraft);
      __result = new LiveryMetaData?(liveryMetaData);
    }
  }

  [HarmonyPatch(typeof (Building))]
  [HarmonyPatch]
  private class BeefierBuildings
  {
    [HarmonyPostfix]
    public static void Postfix(Building __instance)
    {
      if (__instance.name.Contains("Helipad"))
        Traverse.Create((object) __instance).Field("hitpoints").SetValue((object) 500f);
      else if (__instance.name.Contains("revetment1"))
        Traverse.Create((object) __instance).Field("hitpoints").SetValue((object) 250f);
      else if (__instance.name.Contains("hangar_med"))
      {
        Traverse.Create((object) __instance).Field("hitpoints").SetValue((object) 250f);
      }
      else
      {
        if (!__instance.name.Contains("VehicleDepot1"))
          return;
        Traverse.Create((object) __instance).Field("hitpoints").SetValue((object) 250f);
      }
    }
  }

  [HarmonyPatch(typeof (Spawner))]
  [HarmonyPatch("SpawnVehicle", new System.Type[] {typeof (GameObject), typeof (GlobalPosition), typeof (Quaternion), typeof (Vector3), typeof (FactionHQ), typeof (string), typeof (float), typeof (bool), typeof (Player)})]
  private class SpawnVehicleParamsPatch
  {
    public static bool Prefix(
      Spawner __instance,
      ref GroundVehicle __result,
      ref GameObject prefab,
      ref GlobalPosition globalPosition,
      Quaternion rotation,
      Vector3 velocity,
      FactionHQ hq,
      string uniqueName,
      float skill,
      bool holdPosition,
      Player player)
    {
      GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(prefab);
      gameObject.SetActive(true);
      gameObject.transform.position = globalPosition.ToLocalPosition();
      gameObject.transform.rotation = rotation;
      GroundVehicle component = gameObject.GetComponent<GroundVehicle>();
      if ((UnityEngine.Object) component.rb == (UnityEngine.Object) null)
        typeof (GroundVehicle).GetField("rb", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).SetValue((object) component, (object) gameObject.GetComponent<Rigidbody>());
      component.rb.MovePosition(globalPosition.ToLocalPosition());
      component.rb.MoveRotation(rotation);
      component.rb.velocity = velocity;
      component.Networkowner = player;
      component.NetworkHQ = hq;
      component.NetworkUniqueName = uniqueName;
      component.NetworkstartPosition = globalPosition;
      component.NetworkstartRotation = rotation;
      component.NetworkunitName = component.definition.unitName;
      component.skill = skill;
      component.SetHoldPosition(holdPosition);
      __instance.ServerObjectManager.Spawn(gameObject);
      __result = component;
      return false;
    }
  }

  [HarmonyPatch(typeof (SARHSeeker))]
  public static class SARHSeekerImprovements
  {
    [HarmonyPostfix]
    [HarmonyPatch("Seek")]
    public static void Postfix(SARHSeeker __instance)
    {
      Traverse traverse = Traverse.Create((object) __instance);
      Unit unit = (Unit) traverse.Field("targetUnit").GetValue();
      if (!((UnityEngine.Object) unit != (UnityEngine.Object) null) || (double) unit.speed <= 1000.0)
        return;
      Vector3 vector3 = UnityEngine.Random.insideUnitSphere * 25f * Mathf.InverseLerp(1000f, 2500f, unit.speed);
      traverse.Field("knownPos").SetValue((object) new GlobalPosition(((GlobalPosition) traverse.Field("knownPos").GetValue()).AsVector3() + vector3));
      traverse.Method("SendTargetInfo", Array.Empty<object>()).GetValue();
    }
  }

  [HarmonyPatch(typeof (Spawner))]
  public static class MissileSpawnPatch
  {
    [HarmonyPrefix]
    [HarmonyPatch("SpawnMissile", new System.Type[] {typeof (MissileDefinition), typeof (Vector3), typeof (Quaternion), typeof (Vector3), typeof (Unit), typeof (Unit)})]
    public static bool Prefix1(
      MissileDefinition missile,
      Vector3 launchPosition,
      Quaternion rotation,
      Vector3 velocity,
      Unit target,
      Unit owner,
      Spawner __instance,
      out Missile __result)
    {
      if (!__instance.IsServer)
        throw new MethodInvocationException("[Server] function 'SpawnMissile' called when server not active");
      GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(missile.unitPrefab, launchPosition, rotation);
      gameObject.SetActive(true);
      gameObject.GetComponent<Rigidbody>().velocity = velocity;
      Missile component = gameObject.GetComponent<Missile>();
      component.NetworkHQ = (UnityEngine.Object) owner != (UnityEngine.Object) null ? owner.NetworkHQ : (FactionHQ) null;
      component.NetworkunitName = component.definition.unitName;
      component.NetworkownerID = (UnityEngine.Object) owner != (UnityEngine.Object) null ? owner.persistentID : PersistentID.None;
      component.SetTarget(target);
      component.NetworkstartPosition = launchPosition.ToGlobalPosition();
      component.NetworkstartOffsetFromOwner = launchPosition - owner.transform.position;
      component.NetworkstartingVelocity = velocity;
      component.NetworkstartRotation = rotation;
      __instance.ServerObjectManager.Spawn(gameObject);
      __result = component;
      return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch("SpawnMissile", new System.Type[] {typeof (GameObject), typeof (Vector3), typeof (Quaternion), typeof (Vector3), typeof (Unit), typeof (Unit)})]
    public static bool Prefix2(
      GameObject missile,
      Vector3 launchPosition,
      Quaternion rotation,
      Vector3 velocity,
      Unit target,
      Unit owner,
      Spawner __instance,
      out Missile __result)
    {
      if (!__instance.IsServer)
        throw new MethodInvocationException("[Server] function 'SpawnMissile' called when server not active");
      GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(missile, launchPosition, rotation);
      gameObject.SetActive(true);
      gameObject.GetComponent<Rigidbody>().velocity = velocity;
      Missile component = gameObject.GetComponent<Missile>();
      component.NetworkHQ = (UnityEngine.Object) owner != (UnityEngine.Object) null ? owner.NetworkHQ : (FactionHQ) null;
      component.NetworkunitName = component.definition.unitName;
      component.NetworkownerID = (UnityEngine.Object) owner != (UnityEngine.Object) null ? owner.persistentID : PersistentID.None;
      component.SetTarget(target);
      component.NetworkstartPosition = launchPosition.ToGlobalPosition();
      component.NetworkstartOffsetFromOwner = launchPosition - owner.transform.position;
      component.NetworkstartingVelocity = velocity;
      component.NetworkstartRotation = rotation;
      __instance.ServerObjectManager.Spawn(gameObject);
      __result = component;
      return false;
    }
  }

  [HarmonyPatch(typeof (ARHSeeker))]
  public class ARHSeeker_SurfaceTargetingPatch
  {
    internal static string[] affectedMissiles = new string[3]
    {
      "AAM2",
      "AAM4",
      "P_SAMRadar1"
    };
    internal static string[] reducedAccuracyMissiles = new string[1]
    {
      "AAM4"
    };

    [HarmonyPatch("DatalinkMode")]
    [HarmonyPostfix]
    private static void Postfix(ARHSeeker __instance)
    {
      if (!((IEnumerable<string>) QOLPlugin.ARHSeeker_SurfaceTargetingPatch.affectedMissiles).Contains<string>(__instance.gameObject.name))
        return;
      Traverse traverse = Traverse.Create((object) __instance);
      Unit unit = traverse.Field("targetUnit").GetValue<Unit>();
      if ((UnityEngine.Object) unit == (UnityEngine.Object) null || (double) unit.speed < 5.0)
        return;
      switch (unit)
      {
        case GroundVehicle _:
        case Ship _:
          traverse.Field("knownPos").GetValue<GlobalPosition>();
          Vector3 vector3_1 = traverse.Field("positionalErrorVector").GetValue<Vector3>();
          Vector3 vector3_2 = vector3_1;
          if (((IEnumerable<string>) QOLPlugin.ARHSeeker_SurfaceTargetingPatch.reducedAccuracyMissiles).Contains<string>(__instance.gameObject.name) && (double) vector3_1.magnitude > 25.0)
            vector3_2 = vector3_1.normalized * 25f;
          else if ((double) vector3_1.magnitude > 10.0)
            vector3_2 = vector3_1.normalized * 10f;
          traverse.Field("knownPos").SetValue((object) (unit.GlobalPosition() + vector3_2));
          break;
      }
    }
  }

  [HarmonyPatch(typeof (VLSBooster), "VLSBooster_OnInitialize")]
  private class VLSBoosterPatch
  {
    internal static string[] disableVLSed = new string[8]
    {
      "COIN",
      "trainer",
      "Fighter1",
      "SmallFighter1",
      "Multirole1",
      "EW1",
      "Darkreach",
      "CAS1"
    };
    internal static string[] compatibleMissiles = new string[4]
    {
      "CruiseMissile1",
      "AShM1",
      "AShM2",
      "P_SAMRadar1"
    };
    internal static string[] alwaysActiveMissiles = new string[1]
    {
      "P_HAsM1"
    };

    [HarmonyPrefix]
    private static bool Prefix(VLSBooster __instance)
    {
      if (SceneManager.GetActiveScene().name == "Encyclopedia")
        return false;
      Missile missile = Traverse.Create((object) __instance).Field("missile").GetValue<Missile>();
      if (!((IEnumerable<string>) QOLPlugin.VLSBoosterPatch.alwaysActiveMissiles).Contains<string>(missile.name) && ((IEnumerable<string>) QOLPlugin.VLSBoosterPatch.compatibleMissiles).Contains<string>(missile.name) && (UnityEngine.Object) missile.owner != (UnityEngine.Object) null && ((IEnumerable<string>) QOLPlugin.VLSBoosterPatch.disableVLSed).Contains<string>(missile.owner.name))
        return true;
      AccessTools.Method(typeof (VLSBooster), "Activate", (System.Type[]) null, (System.Type[]) null)?.Invoke((object) __instance, (object[]) null);
      return false;
    }

    [HarmonyPostfix]
    private static void Postfix(VLSBooster __instance)
    {
      AccessTools.Field(typeof (VLSBooster), "activated")?.SetValue((object) __instance, (object) true);
    }
  }

  [HarmonyPatch(typeof (VLSBooster), "Burnout")]
  private class VLSBoosterBurnoutPatch
  {
    [HarmonyPostfix]
    private static void Postfix(VLSBooster __instance)
    {
      (Traverse.Create((object) __instance).Field("rb").GetValue() as Rigidbody).drag = 1f;
    }
  }

  [HarmonyPatch(typeof (Missile))]
  private class MissileTangiblePatch
  {
    private static readonly HashSet<Missile> missilesWithActivatedBoosters = new HashSet<Missile>();

    [HarmonyPostfix]
    [HarmonyPatch(typeof (Missile), "StartMissile")]
    private static void StartMissilePostfix(Missile __instance)
    {
      if (!__instance.gameObject.name.Contains("(Clone)"))
        return;
      string str = __instance.gameObject.name.Replace("(Clone)", "").Trim();
      __instance.gameObject.name = str;
      if (string.IsNullOrEmpty(__instance.unitName) || !__instance.unitName.Contains("(Clone)"))
        return;
      __instance.unitName = __instance.unitName.Replace("(Clone)", "").Trim();
    }

    [HarmonyPatch("MotorThrust")]
    [HarmonyPostfix]
    private static void MotorThrustPostfix(Missile __instance)
    {
      if (QOLPlugin.MissileTangiblePatch.missilesWithActivatedBoosters.Contains(__instance))
        return;
      object obj = AccessTools.Field(typeof (Missile), "motor")?.GetValue((object) __instance);
      if (obj == null)
        return;
      double num = (double) (float) AccessTools.Field(obj.GetType(), "delayTimer")?.GetValue(obj);
      bool flag = (bool) AccessTools.Field(obj.GetType(), "activated")?.GetValue(obj);
      if (num <= 0.0 || flag)
        return;
      QOLPlugin.MissileTangiblePatch.ActivateVLSBoosters(__instance);
      QOLPlugin.MissileTangiblePatch.missilesWithActivatedBoosters.Add(__instance);
    }

    private static void ActivateVLSBoosters(Missile missile)
    {
      if ((UnityEngine.Object) missile.owner == (UnityEngine.Object) null || GameManager.gameState == GameState.Encyclopedia)
        return;
      foreach (VLSBooster componentsInChild in missile.GetComponentsInChildren<VLSBooster>())
      {
        if ((UnityEngine.Object) componentsInChild != (UnityEngine.Object) null && !QOLPlugin.MissileTangiblePatch.GetActivatedField(componentsInChild))
        {
          AccessTools.Method(typeof (VLSBooster), "Activate", (System.Type[]) null, (System.Type[]) null)?.Invoke((object) componentsInChild, (object[]) null);
          QOLPlugin.MissileTangiblePatch.SetActivatedField(componentsInChild, true);
        }
      }
    }

    private static bool GetActivatedField(VLSBooster instance)
    {
      return (bool) AccessTools.Field(typeof (VLSBooster), "activated")?.GetValue((object) instance);
    }

    private static void SetActivatedField(VLSBooster instance, bool value)
    {
      AccessTools.Field(typeof (VLSBooster), "activated")?.SetValue((object) instance, (object) value);
    }
  }

  [HarmonyPatch(typeof (ARHSeeker), "Seek")]
  private class ARHLoftReduction
  {
    [HarmonyPostfix]
    private static void Postfix(ARHSeeker __instance)
    {
      Traverse traverse = Traverse.Create((object) __instance);
      if ((double) traverse.Field("loftAmount").GetValue<float>() <= 0.0)
        return;
      GlobalPosition globalPosition = traverse.Field("knownPos").GetValue<GlobalPosition>();
      Missile unit = traverse.Field("missile").GetValue<Missile>();
      if ((double) Vector3.Distance(globalPosition.AsVector3(), unit.GlobalPosition().AsVector3()) >= (double) (float) traverse.Field("terminalRange").GetValue())
        return;
      traverse.Field("loftAmount").SetValue((object) 0.0f);
    }
  }

  [HarmonyPatch(typeof (Missile.Warhead))]
  [HarmonyPatch("Detonate")]
  private class ClusterMissileWarheadPatch
  {
    private static readonly FieldInfo airEffectField = typeof (Missile.Warhead).GetField("airEffect", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

    [HarmonyPrepare]
    private static bool Prepare() => false;

    private static void Prefix(
      Missile.Warhead __instance,
      Rigidbody rb,
      int ownerID,
      Vector3 position,
      Vector3 normal,
      bool armed,
      float blastYield,
      bool hitArmor,
      bool hitTerrain)
    {
      GameObject gameObject1 = (GameObject) QOLPlugin.ClusterMissileWarheadPatch.airEffectField?.GetValue((object) __instance);
      if (!armed || hitArmor || hitTerrain || !((UnityEngine.Object) gameObject1 != (UnityEngine.Object) null) || !gameObject1.name.Contains("bomb_125_1"))
        return;
      Missile component = rb.GetComponent<Missile>();
      if ((UnityEngine.Object) component == (UnityEngine.Object) null)
        return;
      Unit owner = component.owner;
      Unit target = (Unit) null;
      Vector3 velocity = rb.velocity;
      GlobalPosition aimpoint = new GlobalPosition(0.0f, 0.0f, 0.0f);
      GameObject original = ((IEnumerable<GameObject>) UnityEngine.Resources.FindObjectsOfTypeAll<GameObject>()).FirstOrDefault<GameObject>((Func<GameObject, bool>) (go => go.name.Equals("bomb_125_single", StringComparison.InvariantCultureIgnoreCase)));
      if ((UnityEngine.Object) original == (UnityEngine.Object) null)
      {
        QOLPlugin.logger.LogError((object) "Could not find bomb_125_single prefab!");
      }
      else
      {
        for (int index = 0; index < 10; ++index)
        {
          GameObject gameObject2 = UnityEngine.Object.Instantiate<GameObject>(original, position, rb.rotation);
          MountedMissile componentInChildren = gameObject2.GetComponentInChildren<MountedMissile>();
          if ((UnityEngine.Object) componentInChildren != (UnityEngine.Object) null)
          {
            componentInChildren.Fire(owner, target, velocity, (WeaponStation) null, aimpoint);
          }
          else
          {
            QOLPlugin.logger.LogWarning((object) "No MountedMissile component found in bomb mount hierarchy");
            UnityEngine.Object.Destroy((UnityEngine.Object) gameObject2);
          }
        }
      }
    }
  }

  [HarmonyPatch]
  public class MissileDelayedDestroyPatch
  {
    [HarmonyPrepare]
    private static bool Prepare() => false;

    [HarmonyTargetMethod]
    public static MethodBase TargetMethod()
    {
      return (MethodBase) AccessTools.Method(typeof (Missile), "DelayedDestroy", new System.Type[1]
      {
        typeof (float)
      }, (System.Type[]) null);
    }

    [HarmonyPrefix]
    public static bool Prefix(ref float delay)
    {
      delay *= 5f;
      return true;
    }
  }

  [HarmonyPatch(typeof (Missile))]
  [HarmonyPatch("MissedTarget")]
  private class Missile_MissedTarget_Patch
  {
    [HarmonyPrepare]
    private static bool Prepare() => false;

    private static bool Prefix(Missile __instance, ref bool __result)
    {
      __result = false;
      return false;
    }
  }

  [HarmonyPatch(typeof (OpticalSeekerCruiseMissile))]
  [HarmonyPatch("PreTerminalMode")]
  public static class OpticalSeekerCruiseMissile_PreTerminalMode_Patch
  {
    [HarmonyPrepare]
    private static bool Prepare() => false;

    private static bool Prefix(OpticalSeekerCruiseMissile __instance)
    {
      Traverse traverse = Traverse.Create((object) __instance);
      Missile missile = traverse.Field("missile").GetValue<Missile>();
      if ((double) Time.timeSinceLevelLoad - (double) traverse.Field("lastTerminalCheck").GetValue<float>() < 0.75)
        return false;
      traverse.Field("lastTerminalCheck").SetValue((object) Time.timeSinceLevelLoad);
      GlobalPosition globalPosition = traverse.Field("knownPos").GetValue<GlobalPosition>();
      GlobalPosition aimPoint = __instance.TerrainWaypoint(globalPosition);
      if (!traverse.Field("initialBoostMode").GetValue<bool>())
        missile.SetAimpoint(aimPoint, Vector3.zero);
      if ((double) missile.timeSinceSpawn > 2.0)
      {
        float range = traverse.Field("terminalRange").GetValue<float>();
        Unit unit = traverse.Field("targetUnit").GetValue<Unit>();
        if (FastMath.InRange(__instance.transform.GlobalPosition(), globalPosition, range))
        {
          if ((UnityEngine.Object) unit != (UnityEngine.Object) null && !unit.disabled)
          {
            traverse.Field("targetPart").SetValue((object) unit.GetRandomPart());
            traverse.Field("terminalMode").SetValue((object) true);
            return false;
          }
          missile.Detonate(Vector3.up, false, false);
        }
      }
      return false;
    }
  }

  [HarmonyPatch(typeof (NetworkManagerNuclearOption), "SpawnCharacter")]
  public static class ValidateModdedPlayersPatch
  {
    [HarmonyPrepare]
    private static bool Prepare() => false;

    [HarmonyPostfix]
    private static void Postfix(INetworkPlayer networkPlayer)
    {
      QOLPlugin.mplogger.LogWarning((object) "[ValidateModdedPlayersPatch] SpawnCharacter postfix called for network player");
      CoroutineRunner instance = CoroutineRunner.Instance;
      if ((UnityEngine.Object) instance == (UnityEngine.Object) null)
      {
        QOLPlugin.mplogger.LogError((object) "[ValidateModdedPlayersPatch] CoroutineRunner instance is null, cannot start validation");
      }
      else
      {
        QOLPlugin.mplogger.LogWarning((object) "[ValidateModdedPlayersPatch] Starting validation coroutine");
        instance.StartCoroutine(QOLPlugin.ValidateModdedPlayersPatch.ValidatePlayer(networkPlayer));
      }
    }

    private static IEnumerator ValidatePlayer(INetworkPlayer networkPlayer)
    {
      QOLPlugin.mplogger.LogWarning((object) "[ValidateModdedPlayersPatch] ValidatePlayer coroutine started");
      CSteamID lobbyId = LobbyHelper.GetCurrentLobby();
      QOLPlugin.mplogger.LogWarning((object) $"[ValidateModdedPlayersPatch] Current lobby ID: {lobbyId.m_SteamID}");
      if (lobbyId.m_SteamID == 0UL)
      {
        QOLPlugin.mplogger.LogWarning((object) "[ValidateModdedPlayersPatch] No active lobby for validation, aborting");
      }
      else
      {
        NetworkIdentity identity = networkPlayer?.Identity;
        if ((UnityEngine.Object) identity == (UnityEngine.Object) null)
        {
          QOLPlugin.mplogger.LogError((object) "[ValidateModdedPlayersPatch] NetworkIdentity not found for player");
        }
        else
        {
          QOLPlugin.mplogger.LogWarning((object) $"[ValidateModdedPlayersPatch] Found NetworkIdentity: {identity.NetId}");
          Player player = identity.GetComponent<Player>();
          if ((UnityEngine.Object) player == (UnityEngine.Object) null)
          {
            QOLPlugin.mplogger.LogError((object) "[ValidateModdedPlayersPatch] Player component not found");
          }
          else
          {
            QOLPlugin.mplogger.LogWarning((object) $"[ValidateModdedPlayersPatch] Found Player: {player.PlayerName} (SteamID: {player.SteamID})");
            if (player.IsLocalPlayer)
            {
              QOLPlugin.mplogger.LogWarning((object) ("[ValidateModdedPlayersPatch] Skipping validation for local player " + player.PlayerName));
            }
            else
            {
              QOLPlugin.mplogger.LogWarning((object) $"[ValidateModdedPlayersPatch] Validating remote player {player.PlayerName} ({player.SteamID})");
              yield return (object) new WaitForSeconds(5f);
              string str = SteamMatchmaking.GetLobbyMemberData(lobbyId, new CSteamID(player.SteamID), "qol");
              if (str == null)
              {
                QOLPlugin.mplogger.LogError((object) $"[ValidateModdedPlayersPatch] Player {player.PlayerName} not found in lobby or lobby invalid");
              }
              else
              {
                if (string.IsNullOrEmpty(str))
                {
                  str = "null";
                  QOLPlugin.mplogger.LogWarning((object) "[ValidateModdedPlayersPatch] Player mod status is NULL/empty");
                }
                else
                  QOLPlugin.mplogger.LogWarning((object) ("[ValidateModdedPlayersPatch] Player mod status retrieved: " + str));
                QOLPlugin.mplogger.LogWarning((object) ("[ValidateModdedPlayersPatch] Expected version: 1.1.6.3, Actual: " + str));
                if (str != "1.1.6.3")
                {
                  QOLPlugin.mplogger.LogWarning((object) ("[ValidateModdedPlayersPatch] Version mismatch detected! Kicking player " + player.PlayerName));
                  string[] strArray = new string[5]
                  {
                    new string('\n', 30),
                    "<color=red>",
                    QOLPlugin.guid2,
                    " Mod v1.1.6.3 is required.</color>\nNeed help? Visit the Primeva 2082 Discord: discord.gg/qqMwyr2qxR",
                    new string('\n', 31 /*0x1F*/)
                  };
                  player.KickReason(string.Concat(strArray), "<color=yellow>[P2082 MP Manager]</color>");
                  QOLPlugin.mplogger.LogWarning((object) ("[ValidateModdedPlayersPatch] Kick reason set for player " + player.PlayerName));
                  yield return (object) new WaitForSeconds(1f);
                  QOLPlugin.mplogger.LogWarning((object) ("[ValidateModdedPlayersPatch] Disconnecting player " + player.PlayerName));
                  networkPlayer.Disconnect();
                  QOLPlugin.mplogger.LogWarning((object) $"[ValidateModdedPlayersPatch] Player {player.PlayerName} kicked successfully");
                }
                else
                  QOLPlugin.mplogger.LogWarning((object) $"[ValidateModdedPlayersPatch] Player {player.PlayerName} validated successfully - versions match!");
              }
            }
          }
        }
      }
    }
  }

  [HarmonyPatch(typeof (SteamLobby), "TryJoinLobby")]
  public static class ModHandshakePatch
  {
    [HarmonyPrepare]
    private static bool Prepare() => false;

    [HarmonyPostfix]
    private static void Postfix()
    {
      if (!QOLPlugin.IsModdedClient)
        return;
      CoroutineRunner.Instance?.StartCoroutine(QOLPlugin.ModHandshakePatch.HandshakeProcess());
    }

    private static IEnumerator HandshakeProcess()
    {
      QOLPlugin.mplogger.LogWarning((object) "[ModHandshakePatch] HandshakeProcess coroutine started");
      int attempts = 0;
      CSteamID lobbyId;
      do
      {
        QOLPlugin.mplogger.LogWarning((object) $"[ModHandshakePatch] Handshake attempt {attempts + 1}/{8}");
        lobbyId = LobbyHelper.GetCurrentLobby();
        ++attempts;
        if (lobbyId.m_SteamID == 0UL)
        {
          QOLPlugin.mplogger.LogWarning((object) $"[ModHandshakePatch] Waiting for lobby connection ({attempts}/{8})");
          yield return (object) new WaitForSeconds(1f);
        }
        else
        {
          QOLPlugin.mplogger.LogWarning((object) $"[ModHandshakePatch] Lobby found: {lobbyId.m_SteamID}, setting member data");
          SteamMatchmaking.SetLobbyMemberData(lobbyId, "qol", "1.1.6.3");
          QOLPlugin.mplogger.LogWarning((object) "[ModHandshakePatch] Set member data: qol = 1.1.6.3");
          yield return (object) QOLPlugin.WaitForMemberDataCallback(SteamUser.GetSteamID().m_SteamID);
          string lobbyMemberData = SteamMatchmaking.GetLobbyMemberData(lobbyId, SteamUser.GetSteamID(), "qol");
          QOLPlugin.mplogger.LogWarning((object) ("[ModHandshakePatch] Post-callback member data verification: " + lobbyMemberData));
          if (!string.IsNullOrEmpty(lobbyMemberData) && lobbyMemberData == "1.1.6.3")
          {
            QOLPlugin.mplogger.LogWarning((object) ("[ModHandshakePatch] Member data verified successfully: qol = " + lobbyMemberData));
            break;
          }
          QOLPlugin.mplogger.LogWarning((object) "[ModHandshakePatch] Member data verification failed, retrying...");
          yield return (object) new WaitForSeconds(2f);
        }
      }
      while (attempts < 8);
      if (lobbyId.m_SteamID == 0UL)
      {
        QOLPlugin.mplogger.LogError((object) "[ModHandshakePatch] Failed to get lobby SteamID after all attempts");
      }
      else
      {
        QOLPlugin.mplogger.LogWarning((object) $"[ModHandshakePatch] Lobby connection established: {lobbyId.m_SteamID}");
        string lobbyData = SteamMatchmaking.GetLobbyData(lobbyId, "qol");
        bool flag = !string.IsNullOrEmpty(lobbyData);
        QOLPlugin.mplogger.LogWarning((object) $"[ModHandshakePatch] Server mod status check - HasMod: {flag}, Version: {lobbyData}");
        if (!flag)
        {
          QOLPlugin.mplogger.LogWarning((object) $"[ModHandshakePatch] Disconnecting from vanilla server ({lobbyId.m_SteamID})");
          NetworkManagerNuclearOption objectOfType = UnityEngine.Object.FindObjectOfType<NetworkManagerNuclearOption>();
          if ((UnityEngine.Object) objectOfType != (UnityEngine.Object) null)
          {
            QOLPlugin.mplogger.LogWarning((object) "[ModHandshakePatch] NetworkManager found, stopping connection");
            objectOfType.Stop(true);
            GameManager.SetDisconnectReason(new DisconnectInfo($"<color=red>{QOLPlugin.guid2} Mod is not supported on this server.</color>\nNeed help? Visit the Primeva 2082 Discord: discord.gg/qqMwyr2qxR"));
            QOLPlugin.mplogger.LogWarning((object) "[ModHandshakePatch] Disconnect reason set, disconnecting client");
            objectOfType.Client.Disconnect();
          }
          else
            QOLPlugin.mplogger.LogError((object) "[ModHandshakePatch] NetworkManager not found for disconnection");
          QOLPlugin.mplogger.LogWarning((object) "[ModHandshakePatch] Leaving Steam lobby");
          SteamMatchmaking.LeaveLobby(lobbyId);
          QOLPlugin.mplogger.LogWarning((object) "[ModHandshakePatch] Disconnected from vanilla server successfully");
        }
        else
          QOLPlugin.mplogger.LogWarning((object) "[ModHandshakePatch] Joined modded server successfully!");
      }
    }
  }

  [HarmonyPatch(typeof (NetworkManagerNuclearOption), "StartHostAsync")]
  public static class HostHandshakePatch
  {
    [HarmonyPrepare]
    private static bool Prepare() => false;

    [HarmonyPostfix]
    private static void Postfix(NetworkManagerNuclearOption __instance, HostOptions options)
    {
      QOLPlugin.mplogger.LogWarning((object) "[HostHandshakePatch] StartHostAsync postfix called");
      if (!QOLPlugin.IsModdedClient)
      {
        QOLPlugin.mplogger.LogWarning((object) "[HostHandshakePatch] Not a modded client, skipping host setup");
      }
      else
      {
        QOLPlugin.mplogger.LogWarning((object) "[HostHandshakePatch] Starting host setup process");
        __instance.StartCoroutine(QOLPlugin.HostHandshakePatch.HostSetupProcess());
      }
    }

    private static IEnumerator HostSetupProcess()
    {
      QOLPlugin.mplogger.LogWarning((object) "[HostHandshakePatch] HostSetupProcess coroutine started");
      yield return (object) new WaitForSeconds(5f);
      QOLPlugin.mplogger.LogWarning((object) "[HostHandshakePatch] Initial wait completed");
      int attempts = 0;
      CSteamID lobbyId;
      do
      {
        QOLPlugin.mplogger.LogWarning((object) $"[HostHandshakePatch] Host setup attempt {attempts + 1}/{5}");
        lobbyId = LobbyHelper.GetCurrentLobby();
        ++attempts;
        if (lobbyId.m_SteamID == 0UL)
          QOLPlugin.mplogger.LogWarning((object) $"[HostHandshakePatch] Waiting for lobby creation ({attempts}/{5})");
        else
          QOLPlugin.mplogger.LogWarning((object) $"[HostHandshakePatch] Lobby created successfully: {lobbyId.m_SteamID}");
        yield return (object) new WaitForSeconds(1f);
      }
      while (lobbyId.m_SteamID == 0UL && attempts < 5);
      if (lobbyId.m_SteamID == 0UL)
      {
        QOLPlugin.mplogger.LogError((object) "[HostHandshakePatch] Failed to get lobby SteamID after hosting attempts");
      }
      else
      {
        QOLPlugin.mplogger.LogWarning((object) $"[HostHandshakePatch] Setting up modded lobby ({lobbyId.m_SteamID})");
        QOLPlugin.mplogger.LogWarning((object) "[HostHandshakePatch] Setting lobby data: qol = 1.1.6.3");
        SteamMatchmaking.SetLobbyData(lobbyId, "qol", "1.1.6.3");
        QOLPlugin.mplogger.LogWarning((object) "[HostHandshakePatch] Setting member data: qol = 1.1.6.3");
        SteamMatchmaking.SetLobbyMemberData(lobbyId, "qol", "1.1.6.3");
        string str = SteamMatchmaking.GetLobbyData(lobbyId, "qol");
        if (string.IsNullOrEmpty(str))
          str = "null";
        QOLPlugin.mplogger.LogWarning((object) ("[HostHandshakePatch] Host data verification: qol = " + str));
        string lobbyData = SteamMatchmaking.GetLobbyData(lobbyId, "name");
        QOLPlugin.mplogger.LogWarning((object) ("[HostHandshakePatch] Current lobby name: " + lobbyData));
        if (!string.IsNullOrEmpty(lobbyData))
        {
          string pchValue = "[MOD] " + lobbyData;
          SteamMatchmaking.SetLobbyData(lobbyId, "name", pchValue);
          QOLPlugin.mplogger.LogWarning((object) $"[HostHandshakePatch] Lobby renamed to '{pchValue}'");
        }
        else
          QOLPlugin.mplogger.LogWarning((object) "[HostHandshakePatch] Current lobby name is empty, skipping rename");
        SteamMatchmaking.SetLobbyData(lobbyId, "modded_server", "1");
        QOLPlugin.mplogger.LogWarning((object) "[HostHandshakePatch] modded_server flag set to 1");
        QOLPlugin.mplogger.LogWarning((object) "[HostHandshakePatch] Host setup complete");
      }
    }
  }

  [HarmonyPatch(typeof (UnitConverter))]
  public static class YieldReadingPatch
  {
    [HarmonyPatch("YieldReading")]
    [HarmonyPostfix]
    private static void Postfix(float yield, ref string __result)
    {
      if ((double) yield > 1000000000.0)
        __result = QOLPlugin.FormatWithSignificantDecimals(yield * 1E-09f) + "Mt";
      else if ((double) yield > 1000000.0)
        __result = QOLPlugin.FormatWithSignificantDecimals(yield * 1E-06f) + "kt";
      else if (PlayerSettings.unitSystem == PlayerSettings.UnitSystem.Metric)
        __result = QOLPlugin.FormatWithSignificantDecimals(yield) + "kg";
      else
        __result = QOLPlugin.FormatWithSignificantDecimals(yield * 2.20462f) + "lb";
    }
  }

  [HarmonyPatch(typeof (UnitPart), "IsDetached")]
  private class UnitPart_Detached_Patch
  {
    [HarmonyPrepare]
    private static bool Prepare() => false;

    [HarmonyPrefix]
    private static bool Prefix(UnitPart __instance, ref bool __result)
    {
      __result = false;
      return false;
    }
  }

  [HarmonyPatch(typeof (UnitConverter))]
  [qol.PatchConfig.PatchConfig("Technical", "decimals", true, "Improves some decimal displays when values are low")]
  public static class UnitConverter_Patches
  {
    [HarmonyPrepare]
    private static bool Prepare()
    {
      return PatchConfigRegistry.IsEnabled(typeof (QOLPlugin.UnitConverter_Patches));
    }

    [HarmonyPatch("AltitudeReading")]
    [HarmonyPrefix]
    private static bool AltitudeReading_Prefix(float altitude, ref string __result)
    {
      if (PlayerSettings.unitSystem == PlayerSettings.UnitSystem.Metric)
      {
        __result = (double) altitude < 100.0 ? $"{altitude:F1}m" : $"{altitude:F0}m";
      }
      else
      {
        float num = altitude * 3.28084f;
        __result = (double) num < 100.0 ? $"{num:F1}ft" : $"{num:F0}ft";
      }
      return false;
    }

    [HarmonyPatch("ClimbRateReading")]
    [HarmonyPrefix]
    private static bool ClimbRateReading_Prefix(float speed, ref string __result)
    {
      string str = (double) speed > 0.5 ? "+" : "";
      if (PlayerSettings.unitSystem == PlayerSettings.UnitSystem.Metric)
      {
        float num = Math.Abs(speed);
        __result = (double) num >= 1.0 ? ((double) num >= 10.0 ? $"{str}{speed:F0}m/s" : $"{str}{speed:F1}m/s") : $"{str}{speed:F2}m/s";
      }
      else
      {
        float num = (float) ((double) speed * 3.2808399200439453 * 60.0);
        __result = $"{str}{num:F0}fpm";
      }
      return false;
    }
  }

  [HarmonyPatch]
  private static class VersionGetterPatch
  {
    private static void Postfix(ref string __result)
    {
      __result += "_qol-v1.1.6.3";
      QOLPlugin.mplogger.LogWarning((object) ("Updated game version to " + __result));
    }
  }

  [HarmonyPatch]
  private static class UnitPersistentIDPatch
  {
    [HarmonyPrepare]
    private static bool Prepare() => false;

    [HarmonyPostfix]
    private static void Postfix(Unit __instance)
    {
      UnityEngine.Debug.Log((object) $"[UnitPersistentIDPatch] Starting patch for Unit instance {__instance.GetInstanceID()}");
      try
      {
        FieldInfo field1 = typeof (Unit).GetField("persistentID", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (field1 == (FieldInfo) null)
        {
          UnityEngine.Debug.LogError((object) "[UnitPersistentIDPatch] Failed to find 'persistentID' field on Unit class");
          UnityEngine.Debug.Log((object) "[UnitPersistentIDPatch] All fields on Unit class:");
          foreach (FieldInfo field2 in typeof (Unit).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            UnityEngine.Debug.Log((object) $"  - {field2.Name} (Type: {field2.FieldType.Name}, Public: {field2.IsPublic})");
          return;
        }
        UnityEngine.Debug.Log((object) ("[UnitPersistentIDPatch] Found field 'persistentID' of type " + field1.FieldType.Name));
        SyncVarAttribute customAttribute1 = field1.GetCustomAttribute<SyncVarAttribute>();
        if (customAttribute1 == null)
        {
          UnityEngine.Debug.LogError((object) "[UnitPersistentIDPatch] No SyncVarAttribute found on persistentID field");
          UnityEngine.Debug.Log((object) "[UnitPersistentIDPatch] All attributes on persistentID field:");
          foreach (object customAttribute2 in field1.GetCustomAttributes(true))
            UnityEngine.Debug.Log((object) $"  - {customAttribute2.GetType().Name}: {customAttribute2}");
          return;
        }
        UnityEngine.Debug.Log((object) "[UnitPersistentIDPatch] Found SyncVarAttribute. Current state:");
        UnityEngine.Debug.Log((object) ("[UnitPersistentIDPatch]  - Attribute Type: " + customAttribute1.GetType().FullName));
        UnityEngine.Debug.Log((object) $"[UnitPersistentIDPatch]  - Attribute ToString: {customAttribute1}");
        PropertyInfo property1 = customAttribute1.GetType().GetProperty("initialOnly", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (property1 == (PropertyInfo) null)
        {
          UnityEngine.Debug.LogError((object) "[UnitPersistentIDPatch] 'initialOnly' property not found on SyncVarAttribute");
          UnityEngine.Debug.Log((object) "[UnitPersistentIDPatch] All properties on SyncVarAttribute:");
          foreach (PropertyInfo property2 in customAttribute1.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            UnityEngine.Debug.Log((object) $"  - {property2.Name} (Type: {property2.PropertyType.Name}, CanRead: {property2.CanRead}, CanWrite: {property2.CanWrite})");
          UnityEngine.Debug.Log((object) "[UnitPersistentIDPatch] All fields on SyncVarAttribute:");
          foreach (FieldInfo field3 in customAttribute1.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            UnityEngine.Debug.Log((object) $"  - {field3.Name} (Type: {field3.FieldType.Name}, Value: {field3.GetValue((object) customAttribute1)})");
          return;
        }
        UnityEngine.Debug.Log((object) ("[UnitPersistentIDPatch] Found 'initialOnly' property. Type: " + property1.PropertyType.Name));
        UnityEngine.Debug.Log((object) $"[UnitPersistentIDPatch]  - CanRead: {property1.CanRead}");
        UnityEngine.Debug.Log((object) $"[UnitPersistentIDPatch]  - CanWrite: {property1.CanWrite}");
        if (property1.CanRead)
        {
          try
          {
            UnityEngine.Debug.Log((object) $"[UnitPersistentIDPatch] Current initialOnly value: {property1.GetValue((object) customAttribute1)}");
          }
          catch (Exception ex)
          {
            UnityEngine.Debug.LogError((object) ("[UnitPersistentIDPatch] Failed to read current value: " + ex.Message));
          }
        }
        if (property1.CanWrite)
        {
          try
          {
            property1.SetValue((object) customAttribute1, (object) false);
            UnityEngine.Debug.Log((object) "[UnitPersistentIDPatch] Successfully set initialOnly to false");
            if (property1.CanRead)
              UnityEngine.Debug.Log((object) $"[UnitPersistentIDPatch] Verified new initialOnly value: {property1.GetValue((object) customAttribute1)}");
          }
          catch (Exception ex)
          {
            UnityEngine.Debug.LogError((object) ("[UnitPersistentIDPatch] Failed to set initialOnly to false: " + ex.Message));
            UnityEngine.Debug.LogError((object) $"[UnitPersistentIDPatch] Exception details: {ex}");
          }
        }
        else
        {
          UnityEngine.Debug.LogError((object) "[UnitPersistentIDPatch] 'initialOnly' property is read-only!");
          FieldInfo field4 = customAttribute1.GetType().GetField("<initialOnly>k__BackingField", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
          if (field4 != (FieldInfo) null)
          {
            UnityEngine.Debug.Log((object) "[UnitPersistentIDPatch] Found backing field, attempting to modify directly...");
            try
            {
              field4.SetValue((object) customAttribute1, (object) false);
              UnityEngine.Debug.Log((object) "[UnitPersistentIDPatch] Successfully modified backing field!");
            }
            catch (Exception ex)
            {
              UnityEngine.Debug.LogError((object) ("[UnitPersistentIDPatch] Failed to modify backing field: " + ex.Message));
            }
          }
        }
      }
      catch (Exception ex)
      {
        UnityEngine.Debug.LogError((object) ("[UnitPersistentIDPatch] Unexpected error: " + ex.Message));
        UnityEngine.Debug.LogError((object) ("[UnitPersistentIDPatch] Stack trace: " + ex.StackTrace));
      }
      UnityEngine.Debug.Log((object) "[UnitPersistentIDPatch] Patch completed");
    }
  }

  [HarmonyPatch(typeof (ServerObjectManager))]
  public class SpawnDebugPatch
  {
    [HarmonyPrepare]
    private static bool Prepare() => false;

    [HarmonyPrefix]
    [HarmonyPatch("Spawn", new System.Type[] {typeof (NetworkIdentity)})]
    private static void SpawnPrefix(NetworkIdentity identity)
    {
      if ((UnityEngine.Object) identity == (UnityEngine.Object) null)
        return;
      Missile component = identity.GetComponent<Missile>();
      if ((UnityEngine.Object) component == (UnityEngine.Object) null)
        return;
      UnityEngine.Debug.Log((object) "=== SPAWNING MISSILE ===");
      UnityEngine.Debug.Log((object) ("Name: " + component.name));
      UnityEngine.Debug.Log((object) $"NetId: {identity.NetId}");
      UnityEngine.Debug.Log((object) $"PrefabHash: 0x{identity.PrefabHash:X}");
      UnityEngine.Debug.Log((object) $"SceneId: 0x{identity.SceneId:X}");
      UnityEngine.Debug.Log((object) $"IsSceneObject: {identity.IsSceneObject}");
      UnityEngine.Debug.Log((object) $"IsPrefab: {identity.IsPrefab}");
      UnityEngine.Debug.Log((object) $"_hasSpawned: {ReflectionHelpers.GetFieldValue<bool>((object) identity, "_hasSpawned")}");
      if (component.persistentID.Id == 0U)
        return;
      UnityEngine.Debug.Log((object) $"In UnitRegistry: {UnitRegistry.persistentUnitLookup.ContainsKey(component.persistentID)} (ID: {component.persistentID.Id})");
    }
  }

  [HarmonyPatch(typeof (Unit))]
  public class UnitDebugPatch
  {
    private static FieldInfo persistentIDField = typeof (Unit).GetField("persistentID", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

    [HarmonyPrepare]
    private static bool Prepare() => false;

    [HarmonyPostfix]
    [HarmonyPatch("OnStartServer")]
    private static void OnStartServerPostfix(Unit __instance)
    {
      if (!(__instance is Missile missile))
        return;
      UnityEngine.Debug.Log((object) "=== After OnStartServer ===");
      UnityEngine.Debug.Log((object) ("Missile: " + missile.name));
      UnityEngine.Debug.Log((object) $"PersistentID assigned: {missile.persistentID.Id}");
      UnityEngine.Debug.Log((object) $"NetId: {missile.Identity?.NetId}");
      bool flag = UnitRegistry.persistentUnitLookup.ContainsKey(missile.persistentID);
      UnityEngine.Debug.Log((object) $"In UnitRegistry: {flag}");
      if (flag || missile.persistentID.Id == 0U)
        return;
      UnityEngine.Debug.LogWarning((object) $"Missile has PersistentID {missile.persistentID.Id} but not in UnitRegistry!");
    }

    [HarmonyPostfix]
    [HarmonyPatch("RegisterUnit")]
    private static void RegisterUnitPostfix(Unit __instance)
    {
      if (!(__instance is Missile missile))
        return;
      UnityEngine.Debug.Log((object) "=== After RegisterUnit ===");
      UnityEngine.Debug.Log((object) ("Missile: " + missile.name));
      UnityEngine.Debug.Log((object) $"PersistentID: {missile.persistentID.Id}");
      UnityEngine.Debug.Log((object) $"Now in UnitRegistry: {UnitRegistry.persistentUnitLookup.ContainsKey(missile.persistentID)}");
      if (string.IsNullOrEmpty(missile.UniqueName))
        return;
      UnityEngine.Debug.Log((object) $"In customIDLookup: {UnitRegistry.customIDLookup.ContainsKey(missile.UniqueName)}");
    }

    [HarmonyPatch]
    private static void SetPersistentIDPostfix(Unit __instance, PersistentID value)
    {
      if (!(__instance is Missile missile))
        return;
      PersistentID persistentId = (PersistentID) QOLPlugin.UnitDebugPatch.persistentIDField.GetValue((object) __instance);
      UnityEngine.Debug.Log((object) $"Missile {missile.name}: PersistentID changed from {persistentId.Id} to {value.Id}");
      UnityEngine.Debug.Log((object) $"Stack Trace: {new StackTrace()}");
    }
  }

  [HarmonyPatch(typeof (UnitRegistry))]
  public class UnitRegistryDebugPatch
  {
    [HarmonyPrepare]
    private static bool Prepare() => false;

    [HarmonyPostfix]
    [HarmonyPatch("RegisterUnit")]
    private static void RegisterUnitPostfix(Unit unit, PersistentID id)
    {
      if (!(unit is Missile missile))
        return;
      UnityEngine.Debug.Log((object) "=== UnitRegistry.RegisterUnit ===");
      UnityEngine.Debug.Log((object) ("Missile: " + missile.name));
      UnityEngine.Debug.Log((object) $"ID being registered: {id.Id}");
      UnityEngine.Debug.Log((object) $"Unit's current PersistentID: {missile.persistentID.Id}");
      if ((int) id.Id == (int) missile.persistentID.Id)
        return;
      UnityEngine.Debug.LogError((object) $"ID MISMATCH! Registry: {id.Id}, Unit: {missile.persistentID.Id}");
    }
  }

  [HarmonyPatch(typeof (Unit))]
  public static class UnitRegistrationFix
  {
    [HarmonyPrepare]
    private static bool Prepare() => false;

    [HarmonyPatch("DeserializeSyncVars")]
    [HarmonyPostfix]
    public static void OnDeserializeSyncVars(
      Unit __instance,
      NetworkReader reader,
      bool initialState)
    {
      try
      {
        if (__instance.IsServer || !(__instance is Missile))
          return;
        QOLPlugin.logger.LogInfo((object) $"RegistrationFix: Start, checking {__instance.unitName} ({__instance.name}) PID={__instance.persistentID.Id} NPID={__instance.NetworkpersistentID} NID={__instance.NetId}");
        if (__instance.persistentID.IsValid)
        {
          if (UnitRegistry.TryGetUnit(new PersistentID?(__instance.persistentID), out Unit _))
            return;
          QOLPlugin.logger.LogWarning((object) $"RegistrationFix: Registering {__instance.unitName} ({__instance.name}) ID={__instance.persistentID.Id}");
          UnitRegistry.RegisterUnit(__instance, UnitRegistry.GetNextIndex());
        }
        else
        {
          QOLPlugin.logger.LogWarning((object) $"RegistrationFix: {__instance.unitName} ({__instance.name}) ID={__instance.persistentID.Id} has invalid ID, using secondary method");
          UnitRegistry.RegisterUnit(__instance, UnitRegistry.GetNextIndex());
        }
      }
      catch (Exception ex)
      {
        QOLPlugin.logger.LogError((object) $"RegistrationFix: Error in DeserializeSyncVars: {ex}");
      }
    }

    [HarmonyPatch("RegisterUnit")]
    [HarmonyPostfix]
    public static void OnRegisterUnit(Unit __instance, float? updateInterval)
    {
      try
      {
        if (__instance.IsServer || !(__instance is Missile))
          return;
        QOLPlugin.logger.LogInfo((object) $"RegistrationFix: Missile RegisterUnit called for {__instance.unitName} ({__instance.name}) ID={__instance.persistentID.Id}, Valid={__instance.persistentID.IsValid}");
        if (__instance.persistentID.IsValid)
        {
          if (UnitRegistry.TryGetUnit(new PersistentID?(__instance.persistentID), out Unit _))
            return;
          QOLPlugin.logger.LogMessage((object) $"RegistrationFix: Late registration for {__instance.unitName} ({__instance.name}) ID={__instance.persistentID.Id}");
          UnitRegistry.RegisterUnit(__instance, UnitRegistry.GetNextIndex());
        }
        else
        {
          QOLPlugin.logger.LogMessage((object) $"RegistrationFix: Doublate registration for {__instance.unitName} ({__instance.name}) ID={__instance.persistentID.Id}");
          UnitRegistry.RegisterUnit(__instance, UnitRegistry.GetNextIndex());
        }
      }
      catch (Exception ex)
      {
        QOLPlugin.logger.LogError((object) $"[RegistrationFix] Error in RegisterUnit: {ex}");
      }
    }
  }

  [HarmonyPatch(typeof (Unit))]
  public static class UnitRegistrationFix3
  {
    [HarmonyPrepare]
    private static bool Prepare() => false;

    [HarmonyPostfix]
    [HarmonyPatch("RegisterUnit")]
    public static void FixMissileID(Unit __instance)
    {
      if (!(__instance is Missile missile) || missile.persistentID.Id != 0U || !((UnityEngine.Object) missile.Identity != (UnityEngine.Object) null) || missile.Identity.NetId == 0U)
        return;
      missile.persistentID = new PersistentID()
      {
        Id = missile.NetId
      };
      UnitRegistry.RegisterUnit((Unit) missile, missile.persistentID);
      QOLPlugin.logger.LogInfo((object) $"Registered {missile.unitName} with PID={missile.persistentID} NPID={missile.NetworkpersistentID} NID={missile.NetId}");
      Unit unit;
      UnitRegistry.TryGetUnit(new PersistentID?(missile.persistentID), out unit);
      QOLPlugin.logger.LogInfo((object) $"Checking {missile.persistentID} resulted in {unit}");
    }
  }

  [HarmonyPatch(typeof (NetworkIdentity))]
  public static class UnitRegistrationFix5
  {
    [HarmonyPrepare]
    private static bool Prepare() => false;

    [HarmonyPostfix]
    [HarmonyPatch("SetClientValues")]
    public static void SetPersistentID(
      NetworkIdentity __instance,
      ClientObjectManager clientObjectManager,
      SpawnMessage msg)
    {
      Missile component = __instance.GetComponent<Missile>();
      if ((UnityEngine.Object) component == (UnityEngine.Object) null || component.IsServer)
        return;
      component.NetworkpersistentID = new PersistentID()
      {
        Id = __instance.NetId
      };
      UnityEngine.Debug.Log((object) $"SetClientValues: NetId={__instance.NetId}, PersistentID={component.persistentID.Id}");
    }
  }

  [HarmonyPatch(typeof (Missile))]
  public class UnitRegistrationFix2
  {
    [HarmonyPrepare]
    private static bool Prepare() => false;

    [HarmonyPrefix]
    [HarmonyPatch("StartMissile")]
    private static void StartMissilePrefix(Missile __instance)
    {
      if (!__instance.IsServer && !__instance.IsHost || !((UnityEngine.Object) __instance.Identity != (UnityEngine.Object) null) || __instance.Identity.NetId != 0U)
        return;
      if (__instance.persistentID.Id == 0U)
      {
        PersistentID nextIndex = UnitRegistry.GetNextIndex();
        typeof (Unit).GetProperty("NetworkpersistentID")?.SetValue((object) __instance, (object) nextIndex);
      }
      ServerObjectManager serverObjectManager = __instance.Identity.ServerObjectManager;
      if (!((UnityEngine.Object) serverObjectManager != (UnityEngine.Object) null))
        return;
      serverObjectManager.Spawn(__instance.Identity);
    }
  }

  [HarmonyPatch(typeof (Missile))]
  public static class UnitRegistrationFix4
  {
    [HarmonyPrepare]
    private static bool Prepare() => false;

    [HarmonyPostfix]
    [HarmonyPatch("StartMissile")]
    public static void SetID(Missile __instance)
    {
      if (__instance.IsServer || __instance.persistentID.Id != 0U)
        return;
      __instance.NetworkpersistentID = new PersistentID()
      {
        Id = __instance.Identity.NetId
      };
      QOLPlugin.logger.LogInfo((object) $"StartMissile: __instance.NetworkpersistentID set to {__instance.NetworkpersistentID}");
    }
  }

  [HarmonyPatch(typeof (Missile))]
  public static class UnitRegistrationFix6
  {
    [HarmonyPrepare]
    private static bool Prepare() => false;

    [HarmonyPatch("Awake")]
    [HarmonyPrefix]
    private static bool Prefix1(Missile __instance)
    {
      QOLPlugin.logger.LogInfo((object) $"Awake {Time.frameCount}, {Time.renderedFrameCount}");
      return true;
    }

    [HarmonyPatch("OnEnable")]
    [HarmonyPrefix]
    private static bool Prefix2(Missile __instance)
    {
      QOLPlugin.logger.LogInfo((object) $"OnEnable {Time.frameCount}, {Time.renderedFrameCount}");
      return true;
    }
  }

  [HarmonyPatch(typeof (Missile))]
  [HarmonyPatch("StartMissile")]
  public static class MissileRegistrationFix
  {
    [HarmonyPrepare]
    private static bool Prepare() => false;

    private static void Postfix(Missile __instance)
    {
      try
      {
        QOLPlugin.logger.LogMessage((object) $" Starting {__instance.unitName} ({__instance.name})");
        QOLPlugin.logger.LogInfo((object) $" persistentID = {__instance.persistentID}");
        QOLPlugin.logger.LogInfo((object) $" IsServer = {__instance.IsServer}");
        QOLPlugin.logger.LogInfo((object) $" IsValid = {__instance.persistentID.IsValid}");
        if (!__instance.IsServer && __instance.persistentID.IsValid)
        {
          Unit unit;
          if (!UnitRegistry.TryGetUnit(new PersistentID?(__instance.persistentID), out unit))
          {
            QOLPlugin.logger.LogInfo((object) $" Registering custom missile with ID: {__instance.persistentID.Id}, Name: {__instance.unitName}");
            UnitRegistry.RegisterUnit((Unit) __instance, __instance.persistentID);
            if (UnitRegistry.TryGetUnit(new PersistentID?(__instance.persistentID), out unit))
              QOLPlugin.logger.LogInfo((object) $" Successfully registered missile with ID: {__instance.persistentID.Id}");
            else
              QOLPlugin.logger.LogError((object) $" Failed to register missile with ID: {__instance.persistentID.Id}");
          }
          else
            QOLPlugin.logger.LogInfo((object) $" Missile already exists with ID: {__instance.persistentID.Id}");
        }
        else if (__instance.IsServer)
        {
          QOLPlugin.logger.LogInfo((object) " Skipping register, running on server");
        }
        else
        {
          if (__instance.persistentID.IsValid)
            return;
          QOLPlugin.logger.LogInfo((object) $" Skipping register, invalid persistent ID: {__instance.persistentID.Id}");
        }
      }
      catch (Exception ex)
      {
        QOLPlugin.logger.LogError((object) $" Error in registration patch: {ex}");
      }
    }
  }

  [HarmonyPatch]
  public class PilotOnStartClient
  {
    [HarmonyPrepare]
    private static bool Prepare() => false;

    [HarmonyPostfix]
    [HarmonyPatch(typeof (Aircraft), "OnStartClient")]
    public static void PostfixOnStartClient(Aircraft __instance)
    {
      if ((UnityEngine.Object) __instance.NetworkHQ == (UnityEngine.Object) null)
        return;
      if (__instance.NetworkHQ.faction.name == null)
      {
        QOLPlugin.logger.LogWarning((object) "Faction for NetworkHQ was null, exiting");
      }
      else
      {
        if (__instance.NetworkHQ.faction.name != "Primeva")
          return;
        if ((UnityEngine.Object) QOLPlugin.newPilotMat == (UnityEngine.Object) null)
        {
          QOLPlugin.logger.LogWarning((object) "New pilot material not initialized!");
        }
        else
        {
          foreach (SkinnedMeshRenderer componentsInChild in __instance.GetComponentsInChildren<SkinnedMeshRenderer>(true))
          {
            if ((UnityEngine.Object) componentsInChild.sharedMaterial != (UnityEngine.Object) null && componentsInChild.sharedMaterial.name == "pilot")
            {
              componentsInChild.sharedMaterial = QOLPlugin.newPilotMat;
              QOLPlugin.logger.LogInfo((object) ("Replaced pilot material on " + componentsInChild.gameObject.name));
            }
          }
        }
      }
    }
  }

  [HarmonyPatch]
  public class PilotOnStartServer
  {
    [HarmonyPrepare]
    private static bool Prepare() => false;

    [HarmonyPostfix]
    [HarmonyPatch(typeof (Aircraft), "OnStartServer")]
    public static void PostfixAwake(Aircraft __instance)
    {
      if ((UnityEngine.Object) __instance.NetworkHQ == (UnityEngine.Object) null)
        return;
      if (__instance.NetworkHQ.faction.name == null)
      {
        QOLPlugin.logger.LogWarning((object) "Faction for NetworkHQ was null, exiting");
      }
      else
      {
        if (__instance.NetworkHQ.faction.name != "Primeva")
          return;
        if ((UnityEngine.Object) QOLPlugin.newPilotMat == (UnityEngine.Object) null)
        {
          QOLPlugin.logger.LogWarning((object) "New pilot material not initialized!");
        }
        else
        {
          foreach (SkinnedMeshRenderer componentsInChild in __instance.GetComponentsInChildren<SkinnedMeshRenderer>(true))
          {
            if ((UnityEngine.Object) componentsInChild.sharedMaterial != (UnityEngine.Object) null && componentsInChild.sharedMaterial.name == "pilot")
            {
              componentsInChild.sharedMaterial = QOLPlugin.newPilotMat;
              QOLPlugin.logger.LogInfo((object) ("Replaced pilot material on " + componentsInChild.gameObject.name));
            }
          }
        }
      }
    }
  }

  [HarmonyPatch]
  public class KillFeedLogger
  {
    internal static readonly List<QOLPlugin.ItemRelationship> _messageHistory = new List<QOLPlugin.ItemRelationship>();
    private static readonly Regex _logRegex = new Regex("<color=#(?<type1>[0-9a-fA-F]{8})>(?<item1>.*?)</color>\\s+(?<verb>.*?)\\s+<color=#(?<type2>[0-9a-fA-F]{8})>(?<item2>.*?)</color>");

    [HarmonyPrepare]
    private static bool Prepare() => false;

    [HarmonyPrefix]
    [HarmonyPatch(typeof (GameplayUI), "KillFeed")]
    public static bool PrefixKillFeed(GameplayUI __instance, ref string message)
    {
      Match match = QOLPlugin.KillFeedLogger._logRegex.Match(message);
      if (!match.Success)
        return true;
      string item1 = match.Groups["item1"].Value;
      string type1 = match.Groups["type1"].Value;
      string item2 = match.Groups["item2"].Value;
      string type2 = match.Groups["type2"].Value;
      QOLPlugin.ItemRelationship itemRelationship = QOLPlugin.KillFeedLogger._messageHistory.Find((Predicate<QOLPlugin.ItemRelationship>) (x => x.Item1 == item1 && x.Type1 == type1));
      if (itemRelationship == null)
      {
        itemRelationship = new QOLPlugin.ItemRelationship()
        {
          Item1 = item1,
          Type1 = type1
        };
        QOLPlugin.KillFeedLogger._messageHistory.Add(itemRelationship);
      }
      QOLPlugin.RelationshipCount relationshipCount = itemRelationship.Relationships.Find((Predicate<QOLPlugin.RelationshipCount>) (x => x.Item2 == item2 && x.Type2 == type2));
      if (relationshipCount == null)
      {
        relationshipCount = new QOLPlugin.RelationshipCount()
        {
          Item2 = item2,
          Type2 = type2,
          Count = 1
        };
        itemRelationship.Relationships.Add(relationshipCount);
      }
      else
        ++relationshipCount.Count;
      type1 = FactionConfigs.GetFactionName(type1);
      type2 = FactionConfigs.GetFactionName(type2);
      QOLPlugin.kflogger.LogMessage((object) $"{type1} {item1} >>> {type2} {item2} x{relationshipCount.Count}");
      if (relationshipCount.Count > 1)
        message += $" x{relationshipCount.Count}";
      return true;
    }
  }

  [HarmonyPatch(typeof (FactionHQ), "ReportReconAction")]
  public static class ReconBonusPatch
  {
    [HarmonyPrefix]
    public static bool Prefix(FactionHQ __instance, Player player, ref float totalValue)
    {
      totalValue *= 4f;
      return true;
    }
  }

  [HarmonyPatch(typeof (FactionHQ), "ReportJammingAction")]
  public static class JammingBonusPatch
  {
    [HarmonyPrefix]
    public static bool Prefix(
      FactionHQ __instance,
      Player player,
      Unit target,
      ref float totalJamValue)
    {
      totalJamValue *= 3f;
      return true;
    }
  }

  [HarmonyPatch(typeof (FactionHQ), "ReportSupplyAction")]
  public static class ResupplyBonusPatch
  {
    [HarmonyPrefix]
    public static bool Prefix(
      FactionHQ __instance,
      Player player,
      Unit target,
      ref float refillValue,
      bool singleUse)
    {
      refillValue *= 3f;
      return true;
    }
  }

  [HarmonyPatch(typeof (Player))]
  [HarmonyPatch]
  private class RankThresholdPatch
  {
    [HarmonyPrepare]
    private static bool Prepare() => false;

    [HarmonyPatch]
    public static void PostFix(Player __instance)
    {
      Traverse.Create((object) __instance).Field("rankThresholds").SetValue((object) RankConfigs.RankThresholds);
    }
  }

  [HarmonyPatch(typeof (KillTypeExtensions), "GetVerb")]
  [qol.PatchConfig.PatchConfig("Text", "killfeed", true, "Improves killfeed messages by randomly choosing between multiple verbs per kill type, including rare entries")]
  private class KillTypeExtensionsPatch
  {
    [HarmonyPrepare]
    private static bool Prepare()
    {
      return PatchConfigRegistry.IsEnabled(typeof (QOLPlugin.KillTypeExtensionsPatch));
    }

    [HarmonyPostfix]
    private static void Postfix(ref string __result, KillType killType, bool hasKiller)
    {
      try
      {
        System.Random random = new System.Random();
        (string[], string[]) valueTuple;
        if (!(hasKiller ? KillMessageConfigs.KillerMessages : KillMessageConfigs.NoKillerMessages).TryGetValue(killType, out valueTuple))
          return;
        string[] strArray = random.Next(50) == 0 ? valueTuple.Item2 : valueTuple.Item1;
        if (strArray.Length == 0)
          return;
        __result = strArray[random.Next(strArray.Length)];
      }
      catch (Exception ex)
      {
        QOLPlugin.logger.LogError((object) $"Error in kill message: {ex}");
      }
    }
  }

  [HarmonyPatch]
  public class CameraStatePatches
  {
    private const float ZOOM_SPEED_MULTIPLIER = 2f;
    private const float CAMERA_SPEED_MULTIPLIER = 2f;
    private const bool DISABLE_AUTO_ROTATION = true;

    [HarmonyPrepare]
    private static bool Prepare() => false;

    [HarmonyPatch(typeof (CameraEncyclopediaState), "EnterState")]
    [HarmonyPostfix]
    private static void PatchEncyclopediaFOV(
      CameraEncyclopediaState __instance,
      CameraStateManager cam)
    {
      cam.mainCamera.fieldOfView = PlayerSettings.defaultFoV;
    }

    [HarmonyPatch(typeof (CameraOrbitState), "EnterState")]
    [HarmonyPostfix]
    private static void PatchOrbitFOV(CameraOrbitState __instance, CameraStateManager cam)
    {
      cam.mainCamera.fieldOfView = PlayerSettings.defaultFoV;
      cam.SetDesiredFoV(PlayerSettings.defaultFoV, PlayerSettings.defaultFoV);
    }

    [HarmonyPatch(typeof (CameraSelectionState), "EnterState")]
    [HarmonyPostfix]
    private static void PatchSelectionFOV(CameraSelectionState __instance, CameraStateManager cam)
    {
      cam.mainCamera.fieldOfView = PlayerSettings.defaultFoV;
    }

    [HarmonyPatch(typeof (CameraCockpitState), "UpdateState")]
    [HarmonyPrefix]
    private static bool PatchCockpitZoomSpeed(CameraCockpitState __instance, CameraStateManager cam)
    {
      try
      {
        Traverse traverse = Traverse.Create((object) __instance);
        if (!DynamicMap.mapMaximized)
        {
          float num = Mathf.Clamp(traverse.Field("FOVAdjustment").GetValue<float>() + (float) ((double) GameManager.playerInput.GetAxis("Zoom View") * -1.0 * 2.0), 25f - cam.mainCamera.fieldOfView, 80f - cam.mainCamera.fieldOfView);
          traverse.Field("FOVAdjustment").SetValue((object) num);
        }
        return true;
      }
      catch (Exception ex)
      {
        UnityEngine.Debug.LogError((object) $"Error in cockpit zoom patch: {ex}");
        return true;
      }
    }
  }

  [HarmonyPatch(typeof (Altitude))]
  public static class Altitude_Patches
  {
    private static FieldInfo _radarAltField;
    private static FieldInfo _absAltField;
    private static FieldInfo _aircraftField;

    [HarmonyPrepare]
    private static bool Prepare()
    {
      QOLPlugin.Altitude_Patches._radarAltField = typeof (Altitude).GetField("radarAlt", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
      QOLPlugin.Altitude_Patches._absAltField = typeof (Altitude).GetField("absAlt", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
      QOLPlugin.Altitude_Patches._aircraftField = typeof (Altitude).GetField("aircraft", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
      return false;
    }

    [HarmonyPatch("Refresh")]
    [HarmonyPostfix]
    private static void Refresh_Postfix(Altitude __instance)
    {
      Aircraft aircraft = (Aircraft) QOLPlugin.Altitude_Patches._aircraftField.GetValue((object) __instance);
      UnityEngine.UI.Text text1 = (UnityEngine.UI.Text) QOLPlugin.Altitude_Patches._absAltField.GetValue((object) __instance);
      UnityEngine.UI.Text text2 = (UnityEngine.UI.Text) QOLPlugin.Altitude_Patches._radarAltField.GetValue((object) __instance);
      if ((UnityEngine.Object) aircraft == (UnityEngine.Object) null)
        return;
      text2.text = "RCS " + aircraft.RCS.ToString("F4");
      text1.text = $"ALT {UnitConverter.AltitudeReading(aircraft.transform.position.GlobalY())} | {UnitConverter.AltitudeReading(aircraft.radarAlt)}";
    }
  }

  [HarmonyPatch(typeof (HUDBoresightState))]
  [HarmonyPatch("UpdateWeaponDisplay")]
  [qol.PatchConfig.PatchConfig("Technical", "betterPip", true, "Show advanced pip with better calculation, lead and lag, range circle, and format")]
  internal static class SimpleDualPipPatch
  {
    [PatchValue("leadPipSize", "Size of the lead pip indicator")]
    public static float LeadPipSize = 15f;
    [PatchValue("lagPipSize", "Size of the lag pip indicator")]
    public static float LagPipSize = 50f;
    [PatchValue("boresightSize", "Size of the boresight indicator")]
    public static float BoresightSize = 15f;
    [PatchValue("rangeRingSize", "Size of the range ring around lag pip")]
    public static float RangeRingSize = 60f;
    private static Image projectedPosition;
    private static Image targetPosition;
    private static Image boresight;
    private static Image rangeRing;
    private static bool setupDone = false;
    private static FieldInfo projectedPositionField;
    private static FieldInfo targetPositionField;
    private static FieldInfo boresightField;
    private static FieldInfo lineField;
    private static FieldInfo weaponInfoField;
    private static FieldInfo targetVelField;
    private static FieldInfo targetAccelSmoothedField;
    private static Texture2D pipTex1;
    private static Texture2D pipTex2;
    private static Texture2D pipTex3;

    [HarmonyPrepare]
    private static bool Prepare()
    {
      return PatchConfigRegistry.IsEnabled(typeof (QOLPlugin.SimpleDualPipPatch));
    }

    [HarmonyPostfix]
    private static void ManagePips(
      HUDBoresightState __instance,
      Aircraft aircraft,
      List<Unit> targetList)
    {
      if (QOLPlugin.SimpleDualPipPatch.projectedPositionField == (FieldInfo) null)
        QOLPlugin.SimpleDualPipPatch.InitializeReflectionFields(__instance);
      QOLPlugin.SimpleDualPipPatch.projectedPosition = (Image) QOLPlugin.SimpleDualPipPatch.projectedPositionField.GetValue((object) __instance);
      QOLPlugin.SimpleDualPipPatch.targetPosition = (Image) QOLPlugin.SimpleDualPipPatch.targetPositionField.GetValue((object) __instance);
      QOLPlugin.SimpleDualPipPatch.boresight = (Image) QOLPlugin.SimpleDualPipPatch.boresightField.GetValue((object) __instance);
      Image image = (Image) QOLPlugin.SimpleDualPipPatch.lineField.GetValue((object) __instance);
      WeaponInfo weaponInfo = (WeaponInfo) QOLPlugin.SimpleDualPipPatch.weaponInfoField.GetValue((object) __instance);
      Vector3 vector3_1 = (Vector3) QOLPlugin.SimpleDualPipPatch.targetVelField.GetValue((object) __instance);
      Vector3 vector3_2 = (Vector3) QOLPlugin.SimpleDualPipPatch.targetAccelSmoothedField.GetValue((object) __instance);
      if ((UnityEngine.Object) QOLPlugin.SimpleDualPipPatch.pipTex1 == (UnityEngine.Object) null || (UnityEngine.Object) QOLPlugin.SimpleDualPipPatch.pipTex2 == (UnityEngine.Object) null || (UnityEngine.Object) QOLPlugin.SimpleDualPipPatch.pipTex3 == (UnityEngine.Object) null)
      {
        QOLPlugin.SimpleDualPipPatch.LoadTextures();
        QOLPlugin.SimpleDualPipPatch.setupDone = false;
      }
      if ((UnityEngine.Object) QOLPlugin.SimpleDualPipPatch.rangeRing == (UnityEngine.Object) null || (UnityEngine.Object) QOLPlugin.SimpleDualPipPatch.rangeRing.gameObject == (UnityEngine.Object) null)
      {
        QOLPlugin.SimpleDualPipPatch.CreateRangeRing(__instance, QOLPlugin.SimpleDualPipPatch.targetPosition);
        QOLPlugin.SimpleDualPipPatch.setupDone = false;
      }
      if (!QOLPlugin.SimpleDualPipPatch.setupDone)
        QOLPlugin.SimpleDualPipPatch.SetupPip();
      if ((UnityEngine.Object) QOLPlugin.SimpleDualPipPatch.rangeRing == (UnityEngine.Object) null)
        QOLPlugin.SimpleDualPipPatch.CreateRangeRing(__instance, QOLPlugin.SimpleDualPipPatch.targetPosition);
      QOLPlugin.SimpleDualPipPatch.projectedPosition.enabled = true;
      QOLPlugin.SimpleDualPipPatch.targetPosition.enabled = true;
      QOLPlugin.SimpleDualPipPatch.rangeRing.enabled = true;
      if (targetList.Count == 0 || (UnityEngine.Object) targetList[0] == (UnityEngine.Object) null || targetList[0].disabled)
      {
        QOLPlugin.SimpleDualPipPatch.projectedPosition.enabled = false;
        QOLPlugin.SimpleDualPipPatch.targetPosition.enabled = false;
        QOLPlugin.SimpleDualPipPatch.rangeRing.enabled = false;
      }
      else
      {
        float num1 = Vector3.Distance(aircraft.transform.position, targetList[0].transform.position);
        float maxRange = weaponInfo.targetRequirements.maxRange;
        if ((double) num1 > (double) maxRange)
        {
          QOLPlugin.SimpleDualPipPatch.projectedPosition.enabled = false;
          QOLPlugin.SimpleDualPipPatch.targetPosition.enabled = false;
          QOLPlugin.SimpleDualPipPatch.rangeRing.enabled = false;
        }
        else
        {
          float num2 = TargetCalc.TargetLeadTime(targetList[0], aircraft.gameObject, aircraft.rb, weaponInfo.muzzleVelocity, weaponInfo.dragCoef, 3);
          Vector3 vector3_3 = new Vector3();
          Vector3 position1 = targetList[0].transform.position + (vector3_1 - aircraft.rb.velocity) * num2 + (Vector3.up * 9.81f + vector3_2) * 0.5f * num2 * num2;
          Vector3 vector3_4 = Vector3.Scale(SceneSingleton<CameraStateManager>.i.mainCamera.WorldToScreenPoint(position1), new Vector3(1f, 1f, 0.0f));
          Vector3 position2 = QOLPlugin.SimpleDualPipPatch.targetPosition.transform.position;
          Vector3 vector3_5 = Vector3.Scale(QOLPlugin.SimpleDualPipPatch.boresight.transform.position - vector3_4 + position2, new Vector3(1f, 1f, 0.0f));
          QOLPlugin.SimpleDualPipPatch.projectedPosition.transform.position = vector3_4;
          QOLPlugin.SimpleDualPipPatch.targetPosition.transform.position = vector3_5;
          float num3 = Mathf.Clamp01(num1 / maxRange);
          QOLPlugin.SimpleDualPipPatch.rangeRing.fillAmount = num3;
          image.enabled = false;
        }
      }
    }

    private static void InitializeReflectionFields(HUDBoresightState instance)
    {
      System.Type type = typeof (HUDBoresightState);
      QOLPlugin.SimpleDualPipPatch.pipTex1 = ((IEnumerable<Texture2D>) UnityEngine.Resources.FindObjectsOfTypeAll<Texture2D>()).FirstOrDefault<Texture2D>((Func<Texture2D, bool>) (img => img.name.Equals("leadPipper", StringComparison.InvariantCultureIgnoreCase)));
      QOLPlugin.SimpleDualPipPatch.pipTex2 = ((IEnumerable<Texture2D>) UnityEngine.Resources.FindObjectsOfTypeAll<Texture2D>()).FirstOrDefault<Texture2D>((Func<Texture2D, bool>) (img => img.name.Equals("CCIP_pipper", StringComparison.InvariantCultureIgnoreCase)));
      QOLPlugin.SimpleDualPipPatch.pipTex3 = ((IEnumerable<Texture2D>) UnityEngine.Resources.FindObjectsOfTypeAll<Texture2D>()).FirstOrDefault<Texture2D>((Func<Texture2D, bool>) (img => img.name.Equals("CircleThick", StringComparison.InvariantCultureIgnoreCase)));
      QOLPlugin.SimpleDualPipPatch.projectedPositionField = type.GetField("projectedPosition", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
      QOLPlugin.SimpleDualPipPatch.targetPositionField = type.GetField("targetPosition", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
      QOLPlugin.SimpleDualPipPatch.boresightField = type.GetField("boresight", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
      QOLPlugin.SimpleDualPipPatch.lineField = type.GetField("line", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
      QOLPlugin.SimpleDualPipPatch.weaponInfoField = type.GetField("weaponInfo", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
      QOLPlugin.SimpleDualPipPatch.targetVelField = type.GetField("targetVel", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
      QOLPlugin.SimpleDualPipPatch.targetAccelSmoothedField = type.GetField("targetAccelSmoothed", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
      if (QOLPlugin.SimpleDualPipPatch.projectedPositionField == (FieldInfo) null)
        UnityEngine.Debug.LogError((object) "Could not find projectedPosition field!");
      if (QOLPlugin.SimpleDualPipPatch.targetPositionField == (FieldInfo) null)
        UnityEngine.Debug.LogError((object) "Could not find targetPosition field!");
      if (QOLPlugin.SimpleDualPipPatch.boresightField == (FieldInfo) null)
        UnityEngine.Debug.LogError((object) "Could not find boresight field!");
      if (QOLPlugin.SimpleDualPipPatch.lineField == (FieldInfo) null)
        UnityEngine.Debug.LogError((object) "Could not find line field!");
      if (QOLPlugin.SimpleDualPipPatch.weaponInfoField == (FieldInfo) null)
        UnityEngine.Debug.LogError((object) "Could not find weaponInfo field!");
      if (QOLPlugin.SimpleDualPipPatch.targetVelField == (FieldInfo) null)
        UnityEngine.Debug.LogError((object) "Could not find targetVel field!");
      if (!(QOLPlugin.SimpleDualPipPatch.targetAccelSmoothedField == (FieldInfo) null))
        return;
      UnityEngine.Debug.LogError((object) "Could not find targetAccelSmoothed field!");
    }

    private static void SetupPip()
    {
      QOLPlugin.SimpleDualPipPatch.projectedPosition.sprite = Sprite.Create(QOLPlugin.SimpleDualPipPatch.pipTex1, new Rect(0.0f, 0.0f, (float) QOLPlugin.SimpleDualPipPatch.pipTex1.width, (float) QOLPlugin.SimpleDualPipPatch.pipTex1.height), new Vector2(0.5f, 0.5f));
      QOLPlugin.SimpleDualPipPatch.targetPosition.sprite = Sprite.Create(QOLPlugin.SimpleDualPipPatch.pipTex2, new Rect(0.0f, 0.0f, (float) QOLPlugin.SimpleDualPipPatch.pipTex2.width, (float) QOLPlugin.SimpleDualPipPatch.pipTex2.height), new Vector2(0.5f, 0.5f));
      QOLPlugin.SimpleDualPipPatch.boresight.sprite = Sprite.Create(QOLPlugin.SimpleDualPipPatch.boresight.sprite.texture, new Rect((float) (QOLPlugin.SimpleDualPipPatch.boresight.sprite.texture.width / 4), (float) (QOLPlugin.SimpleDualPipPatch.boresight.sprite.texture.height / 4), (float) (QOLPlugin.SimpleDualPipPatch.boresight.sprite.texture.width / 2), (float) (QOLPlugin.SimpleDualPipPatch.boresight.sprite.texture.height / 2)), new Vector2(0.5f, 0.5f));
      QOLPlugin.SimpleDualPipPatch.projectedPosition.rectTransform.sizeDelta = new Vector2(QOLPlugin.SimpleDualPipPatch.LeadPipSize, QOLPlugin.SimpleDualPipPatch.LeadPipSize);
      QOLPlugin.SimpleDualPipPatch.targetPosition.rectTransform.sizeDelta = new Vector2(QOLPlugin.SimpleDualPipPatch.LagPipSize, QOLPlugin.SimpleDualPipPatch.LagPipSize);
      QOLPlugin.SimpleDualPipPatch.boresight.rectTransform.sizeDelta = new Vector2(QOLPlugin.SimpleDualPipPatch.BoresightSize, QOLPlugin.SimpleDualPipPatch.BoresightSize);
      QOLPlugin.SimpleDualPipPatch.setupDone = true;
    }

    private static void CreateRangeRing(HUDBoresightState instance, Image pip)
    {
      GameObject gameObject = new GameObject("RangeRing");
      gameObject.transform.SetParent(pip.transform);
      gameObject.transform.localPosition = Vector3.zero;
      gameObject.transform.localScale = Vector3.one;
      QOLPlugin.SimpleDualPipPatch.rangeRing = gameObject.AddComponent<Image>();
      QOLPlugin.SimpleDualPipPatch.rangeRing.sprite = Sprite.Create(QOLPlugin.SimpleDualPipPatch.pipTex3, new Rect(0.0f, 0.0f, (float) QOLPlugin.SimpleDualPipPatch.pipTex3.width, (float) QOLPlugin.SimpleDualPipPatch.pipTex3.height), new Vector2(0.5f, 0.5f));
      QOLPlugin.SimpleDualPipPatch.rangeRing.type = Image.Type.Filled;
      QOLPlugin.SimpleDualPipPatch.rangeRing.fillMethod = Image.FillMethod.Radial360;
      QOLPlugin.SimpleDualPipPatch.rangeRing.fillOrigin = 0;
      QOLPlugin.SimpleDualPipPatch.rangeRing.fillAmount = 0.0f;
      QOLPlugin.SimpleDualPipPatch.rangeRing.rectTransform.sizeDelta = new Vector2(QOLPlugin.SimpleDualPipPatch.RangeRingSize, QOLPlugin.SimpleDualPipPatch.RangeRingSize);
      QOLPlugin.SimpleDualPipPatch.rangeRing.transform.localEulerAngles = new Vector3(0.0f, 0.0f, 180f);
      QOLPlugin.SimpleDualPipPatch.rangeRing.color = pip.color;
      QOLPlugin.SimpleDualPipPatch.rangeRing.material = pip.material;
    }

    private static void LoadTextures()
    {
      QOLPlugin.SimpleDualPipPatch.pipTex1 = ((IEnumerable<Texture2D>) UnityEngine.Resources.FindObjectsOfTypeAll<Texture2D>()).FirstOrDefault<Texture2D>((Func<Texture2D, bool>) (img => img.name.Equals("leadPipper", StringComparison.InvariantCultureIgnoreCase)));
      QOLPlugin.SimpleDualPipPatch.pipTex2 = ((IEnumerable<Texture2D>) UnityEngine.Resources.FindObjectsOfTypeAll<Texture2D>()).FirstOrDefault<Texture2D>((Func<Texture2D, bool>) (img => img.name.Equals("CCIP_pipper", StringComparison.InvariantCultureIgnoreCase)));
      QOLPlugin.SimpleDualPipPatch.pipTex3 = ((IEnumerable<Texture2D>) UnityEngine.Resources.FindObjectsOfTypeAll<Texture2D>()).FirstOrDefault<Texture2D>((Func<Texture2D, bool>) (img => img.name.Equals("CircleThick", StringComparison.InvariantCultureIgnoreCase)));
    }
  }

  [HarmonyPatch]
  public static class MapToolTip_AircraftLinesPatch
  {
    private static List<Image> additionalLines = new List<Image>();
    private static FieldInfo line1Field = typeof (MapToolTip).GetField("line1", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
    private static FieldInfo iconField = typeof (MapToolTip).GetField("icon", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

    [HarmonyPrepare]
    private static bool Prepare() => false;

    static MapToolTip_AircraftLinesPatch()
    {
      if (QOLPlugin.MapToolTip_AircraftLinesPatch.line1Field == (FieldInfo) null)
        QOLPlugin.logger.LogError((object) "Failed to find line1 field in MapToolTip!");
      if (!(QOLPlugin.MapToolTip_AircraftLinesPatch.iconField == (FieldInfo) null))
        return;
      QOLPlugin.logger.LogError((object) "Failed to find icon field in MapToolTip!");
    }

    [HarmonyPatch(typeof (MapToolTip), "RefreshAirUnitTooltip")]
    [HarmonyPostfix]
    private static void RefreshAirUnitTooltip_Postfix(MapToolTip __instance, Unit unit)
    {
      QOLPlugin.logger.LogInfo((object) ("RefreshAirUnitTooltip_Postfix called for unit: " + unit?.name));
      Aircraft aircraft = unit as Aircraft;
      if ((UnityEngine.Object) aircraft == (UnityEngine.Object) null)
        QOLPlugin.logger.LogWarning((object) "Unit is not an aircraft");
      else if ((UnityEngine.Object) aircraft.weaponManager == (UnityEngine.Object) null)
      {
        QOLPlugin.logger.LogWarning((object) "Aircraft has no weapon manager");
      }
      else
      {
        Image line1 = (Image) QOLPlugin.MapToolTip_AircraftLinesPatch.line1Field?.GetValue((object) __instance);
        MapIcon mapIcon1 = (MapIcon) QOLPlugin.MapToolTip_AircraftLinesPatch.iconField?.GetValue((object) __instance);
        if ((UnityEngine.Object) line1 == (UnityEngine.Object) null)
        {
          QOLPlugin.logger.LogError((object) "line1 is null!");
        }
        else
        {
          if ((UnityEngine.Object) mapIcon1 == (UnityEngine.Object) null)
            QOLPlugin.logger.LogWarning((object) "icon is null - will use default distance calculation");
          List<Unit> targetList = aircraft.weaponManager.GetTargetList();
          if (targetList == null || targetList.Count == 0)
          {
            QOLPlugin.logger.LogInfo((object) "No targets found");
          }
          else
          {
            QOLPlugin.logger.LogInfo((object) $"Processing {targetList.Count} targets");
            QOLPlugin.MapToolTip_AircraftLinesPatch.EnsureEnoughLines(__instance, line1, targetList.Count);
            for (int index = 0; index < targetList.Count; ++index)
            {
              if ((UnityEngine.Object) targetList[index] == (UnityEngine.Object) null)
              {
                QOLPlugin.logger.LogWarning((object) $"Target {index} is null");
              }
              else
              {
                Image image = index == 0 ? line1 : QOLPlugin.MapToolTip_AircraftLinesPatch.additionalLines[index - 1];
                if ((UnityEngine.Object) image == (UnityEngine.Object) null)
                {
                  QOLPlugin.logger.LogError((object) $"Line {index} is null!");
                }
                else
                {
                  GlobalPosition knownPosition;
                  if (!unit.NetworkHQ.TryGetKnownPosition(targetList[index], out knownPosition))
                  {
                    QOLPlugin.logger.LogWarning((object) $"Could not get position for target {index}");
                  }
                  else
                  {
                    Vector3 vector3 = FastMath.Direction(unit.GlobalPosition(), knownPosition);
                    Vector3 to = new Vector3(vector3.x, 0.0f, vector3.z);
                    float num1 = 0.0f;
                    float num2 = Vector3.SignedAngle(Vector3.forward, to, Vector3.up);
                    UnitMapIcon mapIcon2;
                    if (DynamicMap.TryGetMapIcon(targetList[index], out mapIcon2) && (UnityEngine.Object) mapIcon1 != (UnityEngine.Object) null)
                    {
                      num1 = Vector3.Distance(mapIcon2.transform.localPosition, mapIcon1.transform.localPosition);
                      QOLPlugin.logger.LogInfo((object) $"Target {index} distance: {num1}");
                    }
                    else
                      QOLPlugin.logger.LogWarning((object) $"Could not find map icon for target {index}");
                    image.rectTransform.sizeDelta = new Vector2(1f, num1 * SceneSingleton<DynamicMap>.i.mapImage.transform.localScale.x);
                    image.rectTransform.localEulerAngles = new Vector3(0.0f, 0.0f, -num2);
                    Color color = Color.grey;
                    WeaponStation currentWeaponStation = aircraft.weaponManager.currentWeaponStation;
                    if (currentWeaponStation != null && (UnityEngine.Object) currentWeaponStation.WeaponInfo != (UnityEngine.Object) null)
                    {
                      if (currentWeaponStation.WeaponInfo.jammer)
                        color = Color.yellow;
                      else if ((double) currentWeaponStation.WeaponInfo.effectiveness.antiSurface > 0.0)
                        color = Color.red;
                      else if ((double) currentWeaponStation.WeaponInfo.effectiveness.antiAir > 0.0)
                        color = Color.cyan;
                      QOLPlugin.logger.LogInfo((object) $"Target {index} color set to {color}");
                    }
                    color.a = 0.6f;
                    image.color = color;
                    image.enabled = true;
                    QOLPlugin.logger.LogInfo((object) $"Enabled line for target {index}");
                  }
                }
              }
            }
          }
        }
      }
    }

    [HarmonyPatch(typeof (MapToolTip), "ResetAll")]
    [HarmonyPostfix]
    private static void ResetAll_Postfix(MapToolTip __instance)
    {
      QOLPlugin.logger.LogInfo((object) "ResetAll called");
      Image image = (Image) QOLPlugin.MapToolTip_AircraftLinesPatch.line1Field?.GetValue((object) __instance);
      foreach (Image additionalLine in QOLPlugin.MapToolTip_AircraftLinesPatch.additionalLines)
      {
        if ((UnityEngine.Object) additionalLine != (UnityEngine.Object) null)
        {
          additionalLine.enabled = false;
          additionalLine.rectTransform.sizeDelta = Vector2.zero;
        }
      }
    }

    private static void EnsureEnoughLines(MapToolTip instance, Image line1, int requiredCount)
    {
      QOLPlugin.logger.LogInfo((object) $"Ensuring we have {requiredCount} lines (currently have {QOLPlugin.MapToolTip_AircraftLinesPatch.additionalLines.Count + 1})");
      int num = requiredCount - 1 - QOLPlugin.MapToolTip_AircraftLinesPatch.additionalLines.Count;
      if (num > 0)
      {
        if ((UnityEngine.Object) line1 == (UnityEngine.Object) null)
        {
          QOLPlugin.logger.LogError((object) "Cannot create lines - line1 is null!");
          return;
        }
        if ((UnityEngine.Object) line1.transform.parent == (UnityEngine.Object) null)
        {
          QOLPlugin.logger.LogError((object) "Cannot create lines - line1 has no parent!");
          return;
        }
        QOLPlugin.logger.LogInfo((object) $"Creating {num} new lines");
        for (int index = 0; index < num; ++index)
        {
          Image image = UnityEngine.Object.Instantiate<Image>(line1, line1.transform.parent);
          image.name = $"TargetLine_{QOLPlugin.MapToolTip_AircraftLinesPatch.additionalLines.Count + 1}";
          QOLPlugin.MapToolTip_AircraftLinesPatch.additionalLines.Add(image);
          QOLPlugin.logger.LogInfo((object) ("Created new line: " + image.name));
        }
      }
      for (int index = requiredCount - 1; index < QOLPlugin.MapToolTip_AircraftLinesPatch.additionalLines.Count; ++index)
      {
        if ((UnityEngine.Object) QOLPlugin.MapToolTip_AircraftLinesPatch.additionalLines[index] != (UnityEngine.Object) null)
        {
          QOLPlugin.MapToolTip_AircraftLinesPatch.additionalLines[index].enabled = false;
          QOLPlugin.MapToolTip_AircraftLinesPatch.additionalLines[index].rectTransform.sizeDelta = Vector2.zero;
          QOLPlugin.logger.LogInfo((object) $"Disabled extra line {index}");
        }
      }
    }
  }

  [HarmonyPatch]
  public static class ExtendedShipTypesPatch
  {
    private static FieldInfo _listToolTipsField;

    [HarmonyPrepare]
    private static bool Prepare() => false;

    [HarmonyPatch(typeof (MapToolTip), "RefreshCarrierTooltip")]
    [HarmonyPrefix]
    public static bool RefreshCarrierTooltip_Prefix(Airbase airbase, MapToolTip __instance)
    {
      if (QOLPlugin.ExtendedShipTypesPatch._listToolTipsField == (FieldInfo) null)
      {
        QOLPlugin.ExtendedShipTypesPatch._listToolTipsField = typeof (MapToolTip).GetField("listToolTips", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (QOLPlugin.ExtendedShipTypesPatch._listToolTipsField == (FieldInfo) null)
        {
          QOLPlugin.logger.LogError((object) "Failed to find listToolTips field in MapToolTip class");
          return true;
        }
      }
      if (!(QOLPlugin.ExtendedShipTypesPatch._listToolTipsField.GetValue((object) __instance) is List<TooltipItem> tooltipItemList) || tooltipItemList.Count == 0)
        return true;
      TooltipItem tooltipItem = tooltipItemList[0];
      tooltipItem.gameObject.SetActive(true);
      int num = 0;
      if (airbase.AnyHangarsAvailable())
      {
        for (int index = 0; index < airbase.hangars.Count; ++index)
        {
          if (!airbase.hangars[index].Disabled && ShipTypeConfigs.CarrierTypeCodes.Contains(airbase.hangars[index].attachedUnit.definition.code))
            ++num;
        }
      }
      tooltipItem.Setup(GameAssets.i.hangarSprite_carrier, new Color?(num > 0 ? Color.white : Color.grey), (string) null, new int?(num));
      return false;
    }
  }

  [HarmonyPatch(typeof (GameplayUI))]
  public static class AlwaysShowVersionTagPatch
  {
    [HarmonyPatch("PauseGame")]
    [HarmonyPostfix]
    public static void Postfix(GameplayUI __instance) => __instance.gameplayCanvas.enabled = true;
  }

  [HarmonyPatch(typeof (HintsTipsDisplay))]
  public static class HintsTipsDisplayPatches
  {
    [HarmonyPatch("Awake")]
    [HarmonyPrefix]
    private static void AwakePrefix(HintsTipsDisplay __instance)
    {
      Assembly executingAssembly = Assembly.GetExecutingAssembly();
      using (Stream manifestResourceStream = executingAssembly.GetManifestResourceStream(executingAssembly.GetName().Name + ".Resources.hints2.csv"))
      {
        using (StreamReader streamReader = new StreamReader(manifestResourceStream))
        {
          string end = streamReader.ReadToEnd();
          __instance.hintsCSV = new TextAsset(end);
        }
      }
    }
  }

  [HarmonyPatch(typeof (DynamicMap), "Maximize")]
  [qol.PatchConfig.PatchConfig("Misc", "orbitHUD", false, "Show HUD and other cockpit information in third person")]
  private class MinimapMaxHUDPatch
  {
    [HarmonyPrepare]
    private static bool Prepare()
    {
      return PatchConfigRegistry.IsEnabled(typeof (QOLPlugin.MinimapMaxHUDPatch));
    }

    [HarmonyPostfix]
    private static void Postfix(DynamicMap __instance)
    {
      if (!((UnityEngine.Object) SceneSingleton<CombatHUD>.i.aircraft != (UnityEngine.Object) null) || CameraStateManager.cameraMode != CameraMode.cockpit && CameraStateManager.cameraMode != CameraMode.orbit)
        return;
      FlightHud.EnableCanvas(true);
      DynamicMap.EnableCanvas(false);
    }
  }

  [HarmonyPatch(typeof (DynamicMap), "Minimize")]
  [qol.PatchConfig.PatchConfig("Misc", "orbitHUD", false, "Show HUD and other cockpit information in third person")]
  private class MinimapMinHUDPatch
  {
    [HarmonyPrepare]
    private static bool Prepare()
    {
      return PatchConfigRegistry.IsEnabled(typeof (QOLPlugin.MinimapMinHUDPatch));
    }

    [HarmonyPostfix]
    private static void Postfix(DynamicMap __instance)
    {
      if (!((UnityEngine.Object) SceneSingleton<CombatHUD>.i.aircraft != (UnityEngine.Object) null) || CameraStateManager.cameraMode != CameraMode.cockpit && CameraStateManager.cameraMode != CameraMode.orbit)
        return;
      FlightHud.EnableCanvas(true);
      DynamicMap.EnableCanvas(true);
    }
  }

  [HarmonyPatch(typeof (DynamicMap))]
  [qol.PatchConfig.PatchConfig("Accessibility", "betterMap", true, "Forces map to display in black-and-white and without transparency, for better readability")]
  public class DynamicMapPatches
  {
    private static readonly FieldInfo _backgroundMinimizedField = typeof (DynamicMap).GetField("backgroundMinimized", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
    private static readonly FieldInfo _backgroundMaximizedField = typeof (DynamicMap).GetField("backgroundMaximized", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

    [HarmonyPrepare]
    private static bool Prepare()
    {
      return PatchConfigRegistry.IsEnabled(typeof (QOLPlugin.DynamicMapPatches));
    }

    [HarmonyPostfix]
    [HarmonyPatch("Awake")]
    private static void ModifyInitialColors(DynamicMap __instance)
    {
      QOLPlugin.DynamicMapPatches._backgroundMinimizedField?.SetValue((object) __instance, (object) Color.black);
      QOLPlugin.DynamicMapPatches._backgroundMaximizedField?.SetValue((object) __instance, (object) Color.black);
      __instance.mapBackground.color = Color.black;
      Image component = __instance.mapImage.GetComponent<Image>();
      if (!((UnityEngine.Object) component != (UnityEngine.Object) null))
        return;
      component.color = Color.white;
    }

    [HarmonyPostfix]
    [HarmonyPatch("Maximize")]
    private static void ForceBlackBackgroundOnMaximize(DynamicMap __instance)
    {
      __instance.mapBackground.color = Color.black;
    }

    [HarmonyPostfix]
    [HarmonyPatch("Minimize")]
    private static void ForceBlackBackgroundOnMinimize(DynamicMap __instance)
    {
      __instance.mapBackground.color = Color.black;
    }
  }

  [HarmonyPatch(typeof (GridLabels))]
  [qol.PatchConfig.PatchConfig("Accessibility", "betterMap", true, "Forces map to display in black-and-white and without transparency, for better readability")]
  public class GridLabelsPatches
  {
    private static readonly FieldInfo _gridImagesField = typeof (GridLabels).GetField("gridImages", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

    [HarmonyPrepare]
    private static bool Prepare()
    {
      return PatchConfigRegistry.IsEnabled(typeof (QOLPlugin.GridLabelsPatches));
    }

    [HarmonyPostfix]
    [HarmonyPatch("SetupGrid")]
    private static void MakeGridLinesWhite(GridLabels __instance)
    {
      GameObject[] gameObjectArray = (GameObject[]) QOLPlugin.GridLabelsPatches._gridImagesField.GetValue((object) __instance);
      if (gameObjectArray == null)
        return;
      foreach (GameObject gameObject in gameObjectArray)
      {
        if ((UnityEngine.Object) gameObject != (UnityEngine.Object) null)
        {
          Image component = gameObject.GetComponent<Image>();
          if ((UnityEngine.Object) component != (UnityEngine.Object) null)
            component.color = Color.white;
        }
      }
    }
  }

  [HarmonyPatch(typeof (CombatHUD))]
  [HarmonyPatch("ShowWeaponStation")]
  public class LaserGuidedWeaponPatch
  {
    private static bool wasLaserGuided;
    private static WeaponInfo weaponInfo;

    [HarmonyPrefix]
    private static void Prefix(WeaponStation weaponStation)
    {
      if (!((UnityEngine.Object) weaponStation?.WeaponInfo != (UnityEngine.Object) null) || !weaponStation.WeaponInfo.laserGuided)
        return;
      QOLPlugin.LaserGuidedWeaponPatch.weaponInfo = weaponStation.WeaponInfo;
      QOLPlugin.LaserGuidedWeaponPatch.wasLaserGuided = true;
      QOLPlugin.LaserGuidedWeaponPatch.weaponInfo.laserGuided = false;
    }

    [HarmonyPostfix]
    private static void Postfix()
    {
      if (!((UnityEngine.Object) QOLPlugin.LaserGuidedWeaponPatch.weaponInfo != (UnityEngine.Object) null) || !QOLPlugin.LaserGuidedWeaponPatch.wasLaserGuided)
        return;
      QOLPlugin.LaserGuidedWeaponPatch.weaponInfo.laserGuided = true;
      QOLPlugin.LaserGuidedWeaponPatch.weaponInfo = (WeaponInfo) null;
      QOLPlugin.LaserGuidedWeaponPatch.wasLaserGuided = false;
    }
  }

  [HarmonyPatch(typeof (HUDUnitMarker))]
  [qol.PatchConfig.PatchConfig("Accessibility", "betterJam", true, "Removes jamming wobble and improves visibility during jam effect")]
  public static class HUDUnitMarker_Patches
  {
    [HarmonyPrepare]
    private static bool Prepare()
    {
      return PatchConfigRegistry.IsEnabled(typeof (QOLPlugin.HUDUnitMarker_Patches));
    }

    [HarmonyPatch("JammingDistortion")]
    [HarmonyPrefix]
    private static void JammingDistortion_Prefix(
      HUDUnitMarker __instance,
      float jammingStrength,
      ref bool __runOriginal)
    {
      if ((bool) typeof (HUDUnitMarker).GetField("hidden", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).GetValue((object) __instance))
        return;
      if ((double) __instance.image.color.b < 1.0 && (double) __instance.image.color.b < 0.05000000074505806)
        __instance.image.color = __instance.image.color.WithAlpha(Mathf.Max(0.01f, (float) (1.0 - (double) jammingStrength * 2.0)));
      __runOriginal = false;
    }
  }

  [HarmonyPatch(typeof (UnitMapIcon))]
  [qol.PatchConfig.PatchConfig("Accessibility", "betterJam", true, "Removes jamming wobble and improves visibility during jam effect")]
  public static class UnitMapIcon_Patches
  {
    [HarmonyPrepare]
    private static bool Prepare()
    {
      return PatchConfigRegistry.IsEnabled(typeof (QOLPlugin.UnitMapIcon_Patches));
    }

    [HarmonyPatch("JammingDistortion")]
    [HarmonyPrefix]
    private static void JammingDistortion_Prefix(
      UnitMapIcon __instance,
      float jammingStrength,
      ref bool __runOriginal)
    {
      if ((double) __instance.iconImage.color.b < 1.0 && (double) __instance.iconImage.color.b < 0.05000000074505806)
        __instance.iconImage.color = __instance.iconImage.color.WithAlpha(Mathf.Max(0.01f, (float) (1.0 - (double) jammingStrength * 2.0)));
      __runOriginal = false;
    }
  }

  [HarmonyPatch]
  [qol.PatchConfig.PatchConfig("Technical", "hideLaserGuidedText", false, "Hides laser-guided display text")]
  public static class HUDLaserGuidedState_DisplayText_Patch
  {
    private static MethodInfo hideAllMethod = typeof (HUDLaserGuidedState).GetMethod("HideAll", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

    [HarmonyPrepare]
    private static bool Prepare()
    {
      return PatchConfigRegistry.IsEnabled(typeof (QOLPlugin.HUDLaserGuidedState_DisplayText_Patch));
    }

    [HarmonyTargetMethod]
    public static MethodBase TargetMethod()
    {
      return (MethodBase) typeof (HUDLaserGuidedState).GetMethod("DisplayText", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
    }

    [HarmonyPostfix]
    public static void Postfix(HUDLaserGuidedState __instance)
    {
      QOLPlugin.HUDLaserGuidedState_DisplayText_Patch.hideAllMethod?.Invoke((object) __instance, (object[]) null);
    }
  }

  [HarmonyPatch(typeof (Player))]
  [HarmonyPatch("ShowMap")]
  [qol.PatchConfig.PatchConfig("Misc", "disableMapShortcut", false, "Disables the map show shortcut")]
  private class ShowMapPatch
  {
    [HarmonyPrepare]
    private static bool Prepare() => PatchConfigRegistry.IsEnabled(typeof (QOLPlugin.ShowMapPatch));

    [HarmonyPrefix]
    private static bool Prefix() => false;
  }

  [HarmonyPatch(typeof (HUDLaserGuidedState), "UpdateWeaponDisplay")]
  private class LaserArcFixPatch
  {
    [HarmonyPrepare]
    private static bool Prepare() => false;

    [HarmonyPostfix]
    private static void UpdateWeaponDisplay_Postfix(HUDLaserGuidedState __instance)
    {
      try
      {
        FieldInfo fieldInfo1 = AccessTools.Field(typeof (HUDLaserGuidedState), "minAlignment");
        FieldInfo fieldInfo2 = AccessTools.Field(typeof (HUDLaserGuidedState), "currentMaxArc");
        FieldInfo fieldInfo3 = AccessTools.Field(typeof (HUDLaserGuidedState), "currentMaxArcPrev");
        typeof (HUDLaserGuidedState).GetField("innerCircle", BindingFlags.Instance | BindingFlags.NonPublic);
        typeof (HUDLaserGuidedState).GetField("innerCircle", BindingFlags.Instance | BindingFlags.NonPublic);
        if (fieldInfo1 != (FieldInfo) null && fieldInfo2 != (FieldInfo) null)
        {
          float num = (float) fieldInfo1.GetValue((object) __instance);
          fieldInfo2.SetValue((object) __instance, (object) num);
          fieldInfo3.SetValue((object) __instance, (object) num);
        }
        else
          QOLPlugin.logger.LogWarning((object) "LaserArcFix encountered null for alignment or arc");
      }
      catch (Exception ex)
      {
        QOLPlugin.logger.LogError((object) $"Error in LaserArcFix: {ex}");
      }
    }
  }

  [HarmonyPatch(typeof (HUDLaserGuidedState))]
  [HarmonyPatch("UpdateWeaponDisplay")]
  public static class HUDLaserGuidedState_UpdateWeaponDisplay_Patch
  {
    private static FieldInfo _innerCircleField = typeof (HUDLaserGuidedState).GetField("innerCircle", BindingFlags.Instance | BindingFlags.NonPublic);
    private static FieldInfo _currentMaxArcField = typeof (HUDLaserGuidedState).GetField("currentMaxArc", BindingFlags.Instance | BindingFlags.NonPublic);
    private static FieldInfo _currentMaxArcPrevField = typeof (HUDLaserGuidedState).GetField("currentMaxArcPrev", BindingFlags.Instance | BindingFlags.NonPublic);
    private static FieldInfo _fovPrevField = typeof (HUDLaserGuidedState).GetField("fovPrev", BindingFlags.Instance | BindingFlags.NonPublic);
    private static FieldInfo _camField = typeof (HUDLaserGuidedState).GetField("cam", BindingFlags.Instance | BindingFlags.NonPublic);
    private static FieldInfo _minAlignmentField = typeof (HUDLaserGuidedState).GetField("minAlignment", BindingFlags.Instance | BindingFlags.NonPublic);
    private static FieldInfo _outerCircleField = typeof (HUDLaserGuidedState).GetField("outerCircle", BindingFlags.Instance | BindingFlags.NonPublic);
    private static MethodInfo _displayTextMethod = typeof (HUDLaserGuidedState).GetMethod("DisplayText", BindingFlags.Instance | BindingFlags.NonPublic);

    [HarmonyPrepare]
    private static bool Prepare() => false;

    [HarmonyPostfix]
    public static void Postfix(HUDLaserGuidedState __instance)
    {
      try
      {
        Image image1 = (Image) QOLPlugin.HUDLaserGuidedState_UpdateWeaponDisplay_Patch._innerCircleField.GetValue((object) __instance);
        float num1 = (float) QOLPlugin.HUDLaserGuidedState_UpdateWeaponDisplay_Patch._minAlignmentField.GetValue((object) __instance);
        Image image2 = (Image) QOLPlugin.HUDLaserGuidedState_UpdateWeaponDisplay_Patch._outerCircleField.GetValue((object) __instance);
        Camera camera = (Camera) QOLPlugin.HUDLaserGuidedState_UpdateWeaponDisplay_Patch._camField.GetValue((object) __instance);
        float num2 = (float) QOLPlugin.HUDLaserGuidedState_UpdateWeaponDisplay_Patch._fovPrevField.GetValue((object) __instance);
        image1.fillAmount = 1f;
        QOLPlugin.HUDLaserGuidedState_UpdateWeaponDisplay_Patch._currentMaxArcField.SetValue((object) __instance, (object) num1);
        float num3 = num1;
        float num4 = (float) QOLPlugin.HUDLaserGuidedState_UpdateWeaponDisplay_Patch._currentMaxArcPrevField.GetValue((object) __instance);
        if ((double) camera.fieldOfView != (double) num2 || (double) num4 != (double) num3)
        {
          QOLPlugin.HUDLaserGuidedState_UpdateWeaponDisplay_Patch._fovPrevField.SetValue((object) __instance, (object) camera.fieldOfView);
          QOLPlugin.HUDLaserGuidedState_UpdateWeaponDisplay_Patch._currentMaxArcPrevField.SetValue((object) __instance, (object) num3);
          float num5 = (float) (50.0 / (double) camera.fieldOfView * ((double) num1 / 8.0));
          image2.transform.localScale = new Vector3(num5, num5, num5);
        }
        QOLPlugin.HUDLaserGuidedState_UpdateWeaponDisplay_Patch._displayTextMethod.Invoke((object) __instance, (object[]) null);
      }
      catch (Exception ex)
      {
        QOLPlugin.logger.LogError((object) $"Error in HUDLaserGuidedState patch: {ex}");
      }
    }
  }

  [HarmonyPatch(typeof (Ship))]
  [qol.PatchConfig.PatchConfig("Text", "shipNames", true, "Names ships after their linked airbase display name, if applicable")]
  public class ShipNamePatch
  {
    [HarmonyPrepare]
    private static bool Prepare()
    {
      return PatchConfigRegistry.IsEnabled(typeof (QOLPlugin.ShipNamePatch));
    }

    [HarmonyPostfix]
    [HarmonyPatch("Awake")]
    private static void Postfix(Ship __instance)
    {
      __instance.Identity.OnStartServer.AddListener((Action) (() =>
      {
        Airbase componentInChildren = __instance.gameObject.GetComponentInChildren<Airbase>();
        if ((UnityEngine.Object) componentInChildren == (UnityEngine.Object) null || !(__instance.unitName != componentInChildren.SavedAirbase.DisplayName) || __instance.unitName.Contains("["))
          return;
        QOLPlugin.logger.LogInfo((object) $"Renamed {__instance.unitName} to {componentInChildren.SavedAirbase.DisplayName} [{__instance.unitName}]");
        __instance.unitName = $"{componentInChildren.SavedAirbase.DisplayName} [{__instance.unitName}]";
      }));
    }
  }

  [HarmonyPatch(typeof (GroundVehicle))]
  public class GroundVehicle_IRSource
  {
    [HarmonyPatch("Awake")]
    [HarmonyPostfix]
    private static void Postfix(GroundVehicle __instance)
    {
      IRSource source = new IRSource(__instance.transform, 0.5f, false);
      __instance.AddIRSource(source);
    }
  }

  [HarmonyPatch(typeof (Ship))]
  public class Ship_IRSource
  {
    [HarmonyPatch("Awake")]
    [HarmonyPostfix]
    private static void Postfix(Ship __instance)
    {
      IRSource source = new IRSource(__instance.transform, 0.5f, false);
      __instance.AddIRSource(source);
    }
  }

  [HarmonyPatch(typeof (LandingCraftAI))]
  public class LandingCraftStuckPatch
  {
    private static Dictionary<LandingCraftAI, float> stuckTimers = new Dictionary<LandingCraftAI, float>();

    [HarmonyPatch("Update")]
    [HarmonyPostfix]
    private static void UpdatePostfix(LandingCraftAI __instance)
    {
      Traverse traverse = Traverse.Create((object) __instance);
      int num = (int) traverse.Field("state").GetValue<ShipAI.ShipAIState>();
      Ship ship = traverse.Field("ship").GetValue<Ship>();
      if ((double) ship.speed < 2.0)
      {
        if (!QOLPlugin.LandingCraftStuckPatch.stuckTimers.ContainsKey(__instance))
        {
          QOLPlugin.LandingCraftStuckPatch.stuckTimers[__instance] = Time.timeSinceLevelLoad;
        }
        else
        {
          if ((double) Time.timeSinceLevelLoad - (double) QOLPlugin.LandingCraftStuckPatch.stuckTimers[__instance] <= 20.0)
            return;
          traverse.Field("state").SetValue((object) ShipAI.ShipAIState.returning);
          traverse.Field("lastDestinationSelected").SetValue((object) -100f);
          QOLPlugin.LandingCraftStuckPatch.stuckTimers.Remove(__instance);
          QOLPlugin.logger.LogWarning((object) $"LandingCraft '{ship.unitName}' was stuck, changing state");
        }
      }
      else
        QOLPlugin.LandingCraftStuckPatch.stuckTimers.Remove(__instance);
    }
  }

  [HarmonyPatch(typeof (GroundVehicle), "OnStartServer")]
  public static class GroundVehicleSkillPatch
  {
    [HarmonyPrepare]
    private static bool Prepare() => false;

    [HarmonyPostfix]
    public static void Postfix(Aircraft __instance) => __instance.skill = UnityEngine.Random.Range(0.7f, 1.4f);
  }

  [HarmonyPatch(typeof (LandingCraftAI))]
  public class LandingCraftAI_Fix_Patch
  {
    private static readonly Dictionary<LandingCraftAI, float> _deploymentFinishTimes = new Dictionary<LandingCraftAI, float>();
    private static readonly Dictionary<LandingCraftAI, Traverse> _cachedTraverses = new Dictionary<LandingCraftAI, Traverse>();

    [HarmonyPrepare]
    private static bool Prepare() => false;

    [HarmonyPatch("Update")]
    [HarmonyPostfix]
    private static void Update_Postfix(LandingCraftAI __instance)
    {
      Traverse traverse;
      if (!QOLPlugin.LandingCraftAI_Fix_Patch._cachedTraverses.TryGetValue(__instance, out traverse))
      {
        traverse = Traverse.Create((object) __instance);
        QOLPlugin.LandingCraftAI_Fix_Patch._cachedTraverses[__instance] = traverse;
      }
      Ship ship = traverse.Field("ship").GetValue<Ship>();
      if ((UnityEngine.Object) ship == (UnityEngine.Object) null)
        return;
      AirCushion airCushion = traverse.Field("airCushion").GetValue<AirCushion>();
      UnitStorage unitStorage = traverse.Field("unitStorage").GetValue<UnitStorage>();
      if (QOLPlugin.LandingCraftAI_Fix_Patch.HandleHoldingState(__instance, traverse) || QOLPlugin.LandingCraftAI_Fix_Patch.HandleUnloadingState(__instance, unitStorage, airCushion) || QOLPlugin.LandingCraftAI_Fix_Patch.HandleLandingOrNavigatingState(__instance, ship, airCushion, traverse))
        return;
      QOLPlugin.LandingCraftAI_Fix_Patch._deploymentFinishTimes.Remove(__instance);
    }

    [HarmonyPatch("ReturnToShip")]
    [HarmonyPostfix]
    private static void ReturnToShip_Postfix(LandingCraftAI __instance)
    {
      Traverse traverse;
      if (__instance.state != ShipAI.ShipAIState.returning || !QOLPlugin.LandingCraftAI_Fix_Patch._cachedTraverses.TryGetValue(__instance, out traverse))
        return;
      Ship ship = traverse.Field("ship").GetValue<Ship>();
      if ((UnityEngine.Object) ship == (UnityEngine.Object) null || (double) ship.speed >= 2.0)
        return;
      __instance.state = ShipAI.ShipAIState.navigating;
      traverse.Field("lastDestinationSelected").SetValue((object) -100f);
      traverse.Field("inputs").Field("throttle").SetValue((object) 1f);
    }

    private static bool HandleHoldingState(LandingCraftAI instance, Traverse traverse)
    {
      if (instance.state != ShipAI.ShipAIState.holding)
        return false;
      instance.state = ShipAI.ShipAIState.landing;
      traverse.Field("inputs").Field("throttle").SetValue((object) 1f);
      return true;
    }

    private static bool HandleUnloadingState(
      LandingCraftAI instance,
      UnitStorage unitStorage,
      AirCushion airCushion)
    {
      if (instance.state != ShipAI.ShipAIState.unloading)
        return false;
      if ((UnityEngine.Object) unitStorage == (UnityEngine.Object) null || unitStorage.HasUnits())
      {
        QOLPlugin.LandingCraftAI_Fix_Patch._deploymentFinishTimes.Remove(instance);
        return true;
      }
      float num;
      if (!QOLPlugin.LandingCraftAI_Fix_Patch._deploymentFinishTimes.TryGetValue(instance, out num))
      {
        QOLPlugin.LandingCraftAI_Fix_Patch._deploymentFinishTimes[instance] = Time.timeSinceLevelLoad;
        return true;
      }
      if ((double) Time.timeSinceLevelLoad - (double) num <= 5.0)
        return true;
      instance.state = ShipAI.ShipAIState.returning;
      airCushion?.Inflate();
      QOLPlugin.LandingCraftAI_Fix_Patch._deploymentFinishTimes.Remove(instance);
      return true;
    }

    private static bool HandleLandingOrNavigatingState(
      LandingCraftAI instance,
      Ship ship,
      AirCushion airCushion,
      Traverse traverse)
    {
      if (instance.state != ShipAI.ShipAIState.landing && instance.state != ShipAI.ShipAIState.navigating || (double) ship.GlobalPosition().y <= 5.0)
        return false;
      instance.state = ShipAI.ShipAIState.unloading;
      airCushion?.Deflate();
      Traverse traverse1 = traverse.Method("WaitDeployUnits", Array.Empty<object>());
      if (traverse1.MethodExists())
        traverse1.GetValue();
      QOLPlugin.LandingCraftAI_Fix_Patch._deploymentFinishTimes.Remove(instance);
      return true;
    }
  }

  [HarmonyPatch(typeof (LandingGear))]
  [HarmonyPatch("BreakWheel")]
  public static class LandingGear_BreakWheel_Patch
  {
    [HarmonyPrepare]
    private static bool Prepare() => false;

    private static bool Prefix(LandingGear __instance)
    {
      FieldInfo field1 = typeof (LandingGear).GetField("compressionDistance", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
      FieldInfo field2 = typeof (LandingGear).GetField("maxCompression", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
      FieldInfo field3 = typeof (LandingGear).GetField("gearHinge", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
      if (field1 == (FieldInfo) null || field2 == (FieldInfo) null || field3 == (FieldInfo) null)
        return true;
      double num1 = (double) (float) field1.GetValue((object) __instance);
      float num2 = (float) field2.GetValue((object) __instance);
      Transform transform = (Transform) field3.GetValue((object) __instance);
      double num3 = (double) num2 * 2.0;
      return num1 > num3 || (double) transform.localEulerAngles.x > 20.0;
    }
  }

  [HarmonyPatch(typeof (LandingGear))]
  [HarmonyPatch("Awake")]
  public static class LandingGear_Awake_Patch
  {
    private static FieldInfo aircraftField = typeof (LandingGear).GetField("aircraft", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
    private static FieldInfo attachedPartField = typeof (LandingGear).GetField("attachedPart", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
    private static MethodInfo onPartDetachedMethod = typeof (LandingGear).GetMethod("LandingGear_OnPartDetached", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

    [HarmonyPrepare]
    private static bool Prepare() => false;

    [HarmonyPostfix]
    public static void Postfix(LandingGear __instance)
    {
      Aircraft aircraft1 = (Aircraft) QOLPlugin.LandingGear_Awake_Patch.aircraftField.GetValue((object) __instance);
      AeroPart component1 = (AeroPart) QOLPlugin.LandingGear_Awake_Patch.attachedPartField.GetValue((object) __instance);
      if ((UnityEngine.Object) aircraft1 == (UnityEngine.Object) null && (UnityEngine.Object) __instance.transform != (UnityEngine.Object) null)
      {
        Aircraft component2 = __instance.transform.root.GetComponent<Aircraft>();
        if ((UnityEngine.Object) component2 != (UnityEngine.Object) null)
        {
          QOLPlugin.LandingGear_Awake_Patch.aircraftField.SetValue((object) __instance, (object) component2);
          QOLPlugin.logger.LogInfo((object) ("[LandingGearPatch] Found Aircraft for " + __instance.name));
        }
      }
      if ((UnityEngine.Object) component1 == (UnityEngine.Object) null && (UnityEngine.Object) __instance.transform != (UnityEngine.Object) null)
      {
        for (Transform transform = __instance.transform; (UnityEngine.Object) transform != (UnityEngine.Object) null && (UnityEngine.Object) component1 == (UnityEngine.Object) null; transform = transform.parent)
          component1 = transform.GetComponent<AeroPart>();
        if ((UnityEngine.Object) component1 != (UnityEngine.Object) null)
        {
          QOLPlugin.LandingGear_Awake_Patch.attachedPartField.SetValue((object) __instance, (object) component1);
          QOLPlugin.logger.LogInfo((object) ("[LandingGearPatch] Found AeroPart for " + __instance.name));
        }
      }
      Aircraft aircraft2 = (Aircraft) QOLPlugin.LandingGear_Awake_Patch.aircraftField.GetValue((object) __instance);
      AeroPart aeroPart = (AeroPart) QOLPlugin.LandingGear_Awake_Patch.attachedPartField.GetValue((object) __instance);
      if (!((UnityEngine.Object) aircraft2 != (UnityEngine.Object) null))
        return;
      if (!((UnityEngine.Object) aeroPart != (UnityEngine.Object) null))
        return;
      try
      {
        FieldInfo field1 = typeof (Aircraft).GetField("onSetGear", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (field1 != (FieldInfo) null)
        {
          Action<Aircraft.OnSetGear> action = (Action<Aircraft.OnSetGear>) Delegate.Combine(Delegate.Remove((Delegate) field1.GetValue((object) aircraft2), QOLPlugin.LandingGear_Awake_Patch.onPartDetachedMethod.CreateDelegate(typeof (Action<Aircraft.OnSetGear>), (object) __instance)), QOLPlugin.LandingGear_Awake_Patch.onPartDetachedMethod.CreateDelegate(typeof (Action<Aircraft.OnSetGear>), (object) __instance));
          field1.SetValue((object) aircraft2, (object) action);
        }
        FieldInfo field2 = typeof (AeroPart).GetField("onParentDetached", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (field2 != (FieldInfo) null && QOLPlugin.LandingGear_Awake_Patch.onPartDetachedMethod != (MethodInfo) null)
        {
          Action<AeroPart> action = (Action<AeroPart>) Delegate.Combine(Delegate.Remove((Delegate) field2.GetValue((object) aeroPart), QOLPlugin.LandingGear_Awake_Patch.onPartDetachedMethod.CreateDelegate(typeof (Action<AeroPart>), (object) __instance)), QOLPlugin.LandingGear_Awake_Patch.onPartDetachedMethod.CreateDelegate(typeof (Action<AeroPart>), (object) __instance));
          field2.SetValue((object) aeroPart, (object) action);
        }
        QOLPlugin.logger.LogInfo((object) ("[LandingGearPatch] Successfully re-initialized events for " + __instance.name));
      }
      catch (Exception ex)
      {
        QOLPlugin.logger.LogWarning((object) $"[LandingGearPatch] Error re-initializing events for {__instance.name}: {ex.Message}");
      }
    }
  }

  [HarmonyPatch(typeof (Hangar), "Hangar_OnApplyDamage")]
  public static class Hangar_OnApplyDamage_Patch
  {
    public static bool Prefix(Hangar __instance, UnitPart.OnApplyDamage e)
    {
      Traverse traverse = Traverse.Create((object) __instance);
      if (!(traverse.Field("criticalPart").GetValue<UnitPart>() is ShipPart))
        return true;
      if ((double) e.hitPoints < -100.0)
        traverse.Field("selfDisabled").SetValue((object) true);
      return false;
    }
  }

  [HarmonyPatch]
  public static class GroundVehicle_SlingloadFire_Patch
  {
    [HarmonyPatch(typeof (GroundVehicle), "StowTurrets")]
    [HarmonyPrefix]
    public static bool Prefix(GroundVehicle __instance) => false;
  }

  [HarmonyPatch(typeof (CloudLayer), "Start")]
  public static class CloudLayer_Start_Patch
  {
    private static readonly FieldInfo _layerThicknessField = AccessTools.Field(typeof (CloudLayer), "layerThickness");

    [HarmonyPrepare]
    private static bool Prepare() => false;

    [HarmonyPostfix]
    public static void Postfix(CloudLayer __instance)
    {
      if (QOLPlugin.CloudLayer_Start_Patch._layerThicknessField == (FieldInfo) null)
      {
        QOLPlugin.logger.LogError((object) "[CloudLayer Patch] Could not find layerThickness field!");
      }
      else
      {
        QOLPlugin.CloudLayer_Start_Patch._layerThicknessField.SetValue((object) __instance, (object) 250f);
        double num = (double) (float) QOLPlugin.CloudLayer_Start_Patch._layerThicknessField.GetValue((object) __instance);
      }
    }
  }

  [HarmonyPatch(typeof (LevelInfo), "UpdateTimeOfDayLighting")]
  [qol.PatchConfig.PatchConfig("Graphics", "betterBloom", true, "Refactors the day and night bloom cycle for significantly smoother transitions and beautiful dawn and dusk periods")]
  public class LevelInfoBloomPatch
  {
    [HarmonyPrepare]
    private static bool Prepare()
    {
      return PatchConfigRegistry.IsEnabled(typeof (QOLPlugin.LevelInfoBloomPatch));
    }

    [HarmonyPostfix]
    private static void ModifyBloomThreshold(LevelInfo __instance)
    {
      try
      {
        FieldInfo field1 = typeof (LevelInfo).GetField("bloom", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        FieldInfo field2 = typeof (LevelInfo).GetField("ambientIntensity", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (field1 == (FieldInfo) null || field2 == (FieldInfo) null)
        {
          UnityEngine.Debug.LogError((object) "Could not find required fields via reflection");
        }
        else
        {
          object obj1 = field1.GetValue((object) __instance);
          float num1 = (float) field2.GetValue((object) __instance);
          if (obj1 == null)
            return;
          System.Type type = obj1.GetType();
          FieldInfo field3 = type.GetField("threshold");
          FieldInfo field4 = type.GetField("intensity");
          if (field3 == (FieldInfo) null || field4 == (FieldInfo) null)
          {
            UnityEngine.Debug.LogError((object) "Could not find Bloom fields");
          }
          else
          {
            object obj2 = field3.GetValue(obj1);
            object obj3 = field4.GetValue(obj1);
            PropertyInfo property = obj2.GetType().GetProperty("value");
            if (property == (PropertyInfo) null)
            {
              UnityEngine.Debug.LogError((object) "Could not find 'value' property on MinFloatParameter");
            }
            else
            {
              float num2 = 0.05f;
              float num3 = 0.1f;
              if ((double) num1 < (double) num2)
              {
                property.SetValue(obj2, (object) 0.3f);
                property.SetValue(obj3, (object) 3f);
              }
              else if ((double) num1 > (double) num3)
              {
                property.SetValue(obj2, (object) 1.3f);
                property.SetValue(obj3, (object) 0.5f);
              }
              else
              {
                float t = (float) (((double) num1 - (double) num2) / ((double) num3 - (double) num2));
                float num4 = Mathf.Lerp(1.3f, 0.3f, t);
                float num5 = Mathf.Lerp(0.5f, 3f, t);
                property.SetValue(obj2, (object) num4);
                property.SetValue(obj3, (object) num5);
              }
            }
          }
        }
      }
      catch (Exception ex)
      {
        UnityEngine.Debug.LogError((object) ("Error in bloom patch: " + ex.Message));
      }
    }
  }

  [HarmonyPatch(typeof (NightVision))]
  public static class NightVisionPatch
  {
    private static FieldInfo postProcessingField;
    private static FieldInfo colorAdjustmentsField;

    [HarmonyPrepare]
    private static bool Prepare() => false;

    static NightVisionPatch()
    {
      System.Type type = typeof (NightVision);
      QOLPlugin.NightVisionPatch.postProcessingField = type.GetField("postProcessing", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
      QOLPlugin.NightVisionPatch.colorAdjustmentsField = type.GetField("colorAdjustments", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
    }

    [HarmonyPatch("Start")]
    [HarmonyPostfix]
    private static void Start_Postfix(NightVision __instance)
    {
      ReflectionHelpers.SetFieldValue((object) __instance, "gainMin", (object) -3f);
      ReflectionHelpers.SetFieldValue((object) __instance, "gainMax", (object) 3f);
      ReflectionHelpers.SetFieldValue((object) __instance, "bloomThresholdMin", (object) 0.0f);
      ReflectionHelpers.SetFieldValue((object) __instance, "bloomThresholdMax", (object) 2f);
      Volume volume = (Volume) QOLPlugin.NightVisionPatch.postProcessingField.GetValue((object) __instance);
      if ((UnityEngine.Object) volume == (UnityEngine.Object) null || (UnityEngine.Object) volume.profile == (UnityEngine.Object) null)
        return;
      VolumeProfile profile = volume.profile;
      ColorAdjustments component1;
      if (profile.TryGet<ColorAdjustments>(out component1))
        component1.contrast.Override(25f);
      FilmGrain component2;
      if (profile.TryGet<FilmGrain>(out component2))
      {
        component2.intensity.Override(2f);
        component2.response.Override(2f);
      }
      ChromaticAberration component3;
      if (profile.TryGet<ChromaticAberration>(out component3))
        component3.intensity.Override(0.5f);
      Vignette component4;
      if (!profile.TryGet<Vignette>(out component4))
        return;
      component4.intensity.Override(0.3f);
    }
  }

  [HarmonyPatch(typeof (DamageParticles))]
  public static class DamageParticles_SlowUpdate_Patch
  {
    [HarmonyPatch("SlowUpdate")]
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> SlowUpdate_Transpiler(
      IEnumerable<CodeInstruction> instructions)
    {
      List<CodeInstruction> codeInstructionList = new List<CodeInstruction>(instructions);
      for (int index = 0; index < codeInstructionList.Count; ++index)
      {
        if (codeInstructionList[index].opcode == OpCodes.Ldsfld && codeInstructionList[index].operand.ToString().Contains("LocalSeaY"))
        {
          codeInstructionList[index] = new CodeInstruction(OpCodes.Ldc_R4, (object) -150f);
          break;
        }
      }
      return (IEnumerable<CodeInstruction>) codeInstructionList;
    }

    [HarmonyPatch("ParentObjectCulled")]
    [HarmonyPrefix]
    private static bool ParentObjectCulled_Prefix(DamageParticles __instance) => true;

    [HarmonyPatch("DetachTimer")]
    [HarmonyPrefix]
    private static bool DetachTimer_Prefix(DamageParticles __instance) => false;
  }

  [HarmonyPatch(typeof (LightAnimation))]
  [qol.PatchConfig.PatchConfig("Graphics", "brighterLights", true, "Significantly increases the brightness of dynamic lights and improves auto-exposure effects for nuclear weapons")]
  private class LightAnimation_Patch
  {
    [HarmonyPrepare]
    private static bool Prepare()
    {
      return PatchConfigRegistry.IsEnabled(typeof (QOLPlugin.LightAnimation_Patch));
    }

    [HarmonyPatch("OnEnable")]
    [HarmonyPrefix]
    private static bool OnEnablePrefix(LightAnimation __instance)
    {
      Traverse traverse = Traverse.Create((object) __instance);
      float num = traverse.Field("lightIntensity").GetValue<float>();
      traverse.Field("lightIntensity").SetValue((object) (float) ((double) num * 5.0));
      if ((double) num * 5.0 > 100000.0)
        ExposureController.RegisterBrightLight((Light) traverse.Field("animatedLight").GetValue());
      return false;
    }
  }

  [HarmonyPatch(typeof (Hardpoint), "SpawnMount")]
  public class HardpointSpawnPatch
  {
    private static bool Prefix(Hardpoint __instance, Aircraft aircraft, WeaponMount weaponMount)
    {
      if ((UnityEngine.Object) weaponMount.prefab != (UnityEngine.Object) null && (UnityEngine.Object) weaponMount.prefab.GetComponentInChildren<Gun>() != (UnityEngine.Object) null)
      {
        weaponMount.prefab.SetActive(true);
        weaponMount.prefab.transform.position = Vector3.zero;
      }
      return true;
    }

    private static void Postfix(
      Hardpoint __instance,
      Aircraft aircraft,
      WeaponMount weaponMount,
      GameObject ___spawnedPrefab)
    {
      if ((UnityEngine.Object) weaponMount.prefab.GetComponentInChildren<Gun>() != (UnityEngine.Object) null && !WeaponMountConfigs.ActiveTurretMounts.Contains<string>(weaponMount.name))
        weaponMount.prefab.SetActive(false);
      FuelTank componentInChildren1 = weaponMount.prefab.GetComponentInChildren<FuelTank>();
      if ((UnityEngine.Object) componentInChildren1 != (UnityEngine.Object) null)
      {
        ___spawnedPrefab.GetComponentInChildren<AeroPart>().parentUnit = (Unit) aircraft;
        ___spawnedPrefab.SetActive(true);
        AeroPart componentInChildren2 = ___spawnedPrefab.GetComponentInChildren<AeroPart>();
        if ((UnityEngine.Object) componentInChildren2 != (UnityEngine.Object) null && aircraft.LocalSim)
        {
          componentInChildren2.CreateRB(aircraft.rb.GetPointVelocity(__instance.transform.position), __instance.transform.position);
          componentInChildren2.CreateJoints();
        }
        Traverse.Create((object) componentInChildren1).Field("part").SetValue((object) componentInChildren2);
        QOLPlugin.logger.LogInfo((object) componentInChildren2);
        QOLPlugin.logger.LogInfo(Traverse.Create((object) componentInChildren1).Field("part").GetValue());
        Traverse traverse = Traverse.Create(Traverse.Create((object) componentInChildren1).Field("part").GetValue());
        Rigidbody rigidbody = traverse.Field("rb").GetValue<Rigidbody>();
        QOLPlugin.logger.LogInfo((object) Traverse.Create((object) componentInChildren1).Field("part").GetValue<AeroPart>().parentUnit);
        QOLPlugin.logger.LogInfo((object) rigidbody);
        QOLPlugin.logger.LogInfo(Traverse.Create((object) rigidbody).Property("mass", (object[]) null).GetValue());
        QOLPlugin.logger.LogInfo(traverse.Field("centerOfMass").GetValue());
        QOLPlugin.logger.LogInfo(Traverse.Create((object) componentInChildren2.parentUnit).Field("LocalSim").GetValue());
        QOLPlugin.logger.LogInfo(Traverse.Create((object) rigidbody).Property("centerOfMass", (object[]) null).GetValue());
        componentInChildren1.Refuel(aircraft.fuelLevel);
      }
      ___spawnedPrefab.SetActive(true);
    }
  }

  [HarmonyPatch(typeof (HUDTurretCrosshair))]
  [HarmonyPatch("Refresh")]
  public static class HUDTurretCrosshair_Refresh_Patch
  {
    public static bool Prefix(
      HUDTurretCrosshair __instance,
      Camera mainCamera,
      out Vector3 crosshairPosition)
    {
      crosshairPosition = Vector3.one * 10000f;
      Traverse traverse = Traverse.Create((object) __instance);
      if (traverse.Field("gun").GetValue() != null)
        return true;
      traverse.Field("crosshair").Field("enabled").SetValue((object) false);
      traverse.Field("readinessCircle").Field("enabled").SetValue((object) false);
      Turret turret = traverse.Field("turret").GetValue<Turret>();
      if ((UnityEngine.Object) turret != (UnityEngine.Object) null && (UnityEngine.Object) mainCamera != (UnityEngine.Object) null)
      {
        Vector3 direction = turret.GetDirection();
        if ((double) Vector3.Dot(mainCamera.transform.forward, direction - mainCamera.transform.position) > 0.0)
        {
          crosshairPosition = SceneSingleton<CameraStateManager>.i.mainCamera.WorldToScreenPoint(direction);
          crosshairPosition.z = 0.0f;
          __instance.transform.position = crosshairPosition;
          Image image = traverse.Field("circle").GetValue<Image>();
          if ((UnityEngine.Object) image != (UnityEngine.Object) null)
          {
            image.enabled = true;
            image.color = Color.green;
          }
        }
        else
        {
          Image image = traverse.Field("circle").GetValue<Image>();
          if ((UnityEngine.Object) image != (UnityEngine.Object) null)
            image.enabled = false;
        }
      }
      return false;
    }
  }

  [HarmonyPatch(typeof (Gun), "Fire")]
  public static class GunPatchForRailcannon
  {
    [HarmonyPrefix]
    private static bool Prefix(
      Gun __instance,
      Unit firingUnit,
      Unit target,
      Vector3 inheritedVelocity,
      WeaponStation weaponStation,
      GlobalPosition aimpoint)
    {
      PowerSupply component;
      if (!weaponStation.WeaponInfo.energy || !(firingUnit is Aircraft) || !firingUnit.gameObject.TryGetComponent<PowerSupply>(out component))
        return true;
      if ((double) component.GetCharge() < 0.60000002384185791)
        return false;
      double num = (double) component.DrawPower(15000f);
      return true;
    }
  }

  [HarmonyPatch(typeof (ChargeIndicator), "SetVisibility")]
  public static class PowerPatchForRailcannon
  {
    [HarmonyPostfix]
    private static void Postfix(ChargeIndicator __instance, bool visible)
    {
      __instance.enabled = true;
      __instance.gameObject.SetActive(true);
    }
  }

  [HarmonyPatch(typeof (Turret))]
  public class TurretTargetFilterPatch2
  {
    [HarmonyPatch("AssessTargetPriority")]
    [HarmonyPrefix]
    private static bool FilterTargets_Prefix(
      Turret __instance,
      Unit targetCandidate,
      ref float priorityThreshold)
    {
      if ((UnityEngine.Object) targetCandidate == (UnityEngine.Object) null || targetCandidate.disabled || !(targetCandidate is Missile missile))
        return true;
      WeaponInfo weaponInfo = missile.GetWeaponInfo();
      if (weaponInfo.overHorizon && missile.name.Contains("ballistic") && (double) missile.rb.velocity.y > 0.0)
      {
        if ((double) priorityThreshold < 0.0)
          priorityThreshold = 0.0f;
        Traverse.Create((object) __instance).Field("target").SetValue((object) null);
        return false;
      }
      if (weaponInfo.nuclear || weaponInfo.bomb || weaponInfo.overHorizon || missile.name.Contains("glide"))
        return true;
      Unit unit = Traverse.Create((object) missile).Field<Unit>("target").Value;
      if ((UnityEngine.Object) unit == (UnityEngine.Object) null)
      {
        if ((double) priorityThreshold < 0.0)
          priorityThreshold = 0.0f;
        Traverse.Create((object) __instance).Field("target").SetValue((object) null);
        return false;
      }
      if (!(unit is Missile))
        return true;
      if ((double) priorityThreshold < 0.0)
        priorityThreshold = 0.0f;
      Traverse.Create((object) __instance).Field("target").SetValue((object) null);
      return false;
    }

    [HarmonyPatch("AssessTargetPriority")]
    [HarmonyPostfix]
    private static void FilterTargets_Postfix(
      Turret __instance,
      Unit targetCandidate,
      float priorityThreshold)
    {
      if ((double) Time.timeSinceLevelLoad / 10.0 % 10.0 != 0.0 || !(targetCandidate is Aircraft aircraft))
        return;
      MissileWarning missileWarningSystem = aircraft.GetMissileWarningSystem();
      if (missileWarningSystem.knownMissiles.Count >= 2)
        return;
      List<Missile> knownMissiles = missileWarningSystem.knownMissiles;
      List<Missile> collection = (List<Missile>) Traverse.Create((object) missileWarningSystem).Field("unknownMissiles").GetValue();
      List<Missile> missileList1 = new List<Missile>(knownMissiles.Count + collection.Count);
      missileList1.AddRange((IEnumerable<Missile>) knownMissiles);
      missileList1.AddRange((IEnumerable<Missile>) collection);
      List<Missile> missileList2 = missileList1;
      if (missileList2.Count >= 4)
        return;
      foreach (Missile missile in missileList2)
      {
        if (missile.ownerID == __instance.GetAttachedUnit().persistentID)
          return;
      }
      Traverse.Create((object) __instance).Field("target").SetValue((object) targetCandidate);
    }
  }

  [HarmonyPatch(typeof (Gun), "AttachToHardpoint")]
  public class GunMountSpawnPatch
  {
    [HarmonyPrepare]
    private static bool Prepare() => false;

    private static bool Prefix(
      Gun __instance,
      Aircraft aircraft,
      Hardpoint hardpoint,
      WeaponMount mount)
    {
      QOLPlugin.logger.LogDebug((object) __instance);
      __instance.transform.root.gameObject.SetActive(true);
      return true;
    }
  }

  [HarmonyPatch(typeof (CombatAI), "InterceptViability")]
  public static class CombatAI_InterceptViability_Patch
  {
    [HarmonyPrepare]
    private static bool Prepare() => false;

    [HarmonyPrefix]
    public static bool Prefix(
      Unit target,
      Unit analyzer,
      WeaponStation weaponStation,
      float maxRange,
      float targetDist,
      float targetMaxSpeed)
    {
      return !weaponStation.WeaponInfo.gun || (double) maxRange >= (double) targetDist;
    }

    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
      int index = 0;
      foreach (CodeInstruction instruction in instructions)
      {
        if (index >= 26 && index <= 66)
        {
          ++index;
        }
        else
        {
          yield return instruction;
          ++index;
        }
      }
    }
  }

  [HarmonyPatch(typeof (LaserDesignator))]
  public class LaserDesignatorPatch
  {
    [HarmonyPrepare]
    private static bool Prepare() => false;

    [HarmonyPatch("Awake")]
    [HarmonyPostfix]
    private static void AwakePostfix(LaserDesignator __instance)
    {
      __instance.gameObject.AddComponent<LineRenderer>();
    }

    [HarmonyPatch("LaseTargets")]
    [HarmonyPostfix]
    private static void LaseTargetsPostfix(LaserDesignator __instance)
    {
      __instance.StartCoroutine(QOLPlugin.LaserDesignatorPatch.LaserUpdateCoroutine(__instance));
    }

    private static IEnumerator LaserUpdateCoroutine(LaserDesignator designator)
    {
      LineRenderer lineRenderer = designator.GetComponent<LineRenderer>();
      if ((UnityEngine.Object) lineRenderer == (UnityEngine.Object) null)
      {
        lineRenderer = designator.gameObject.AddComponent<LineRenderer>();
        lineRenderer.startWidth = 0.01f;
        lineRenderer.endWidth = 0.01f;
        lineRenderer.positionCount = 2;
        lineRenderer.useWorldSpace = true;
        lineRenderer.startColor = new Color(0.0f, 10f, 0.0f, 1f);
        lineRenderer.endColor = new Color(0.0f, 10f, 0.0f, 1f);
        lineRenderer.enabled = false;
        lineRenderer.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
      }
      while ((UnityEngine.Object) designator != (UnityEngine.Object) null && (UnityEngine.Object) designator.gameObject != (UnityEngine.Object) null)
      {
        yield return (object) null;
        List<Unit> lasedTargets = designator.GetLasedTargets();
        if (lasedTargets != null && lasedTargets.Count > 0)
        {
          lineRenderer.enabled = true;
          lineRenderer.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
          lineRenderer.positionCount = lasedTargets.Count * 2;
          for (int index = 0; index < lasedTargets.Count; ++index)
          {
            if ((UnityEngine.Object) lasedTargets[index] != (UnityEngine.Object) null && (UnityEngine.Object) lasedTargets[index].transform != (UnityEngine.Object) null)
            {
              lineRenderer.SetPosition(index * 2, designator.transform.position);
              lineRenderer.SetPosition(index * 2 + 1, lasedTargets[index].transform.position);
            }
          }
        }
        else if (lineRenderer.enabled)
          lineRenderer.enabled = false;
      }
    }

    [HarmonyPatch("OnDestroy")]
    [HarmonyPostfix]
    private static void OnDestroyPostfix(LaserDesignator __instance)
    {
      LineRenderer component = __instance.GetComponent<LineRenderer>();
      if (!((UnityEngine.Object) component != (UnityEngine.Object) null))
        return;
      UnityEngine.Object.Destroy((UnityEngine.Object) component);
    }
  }

  [HarmonyPatch(typeof (Turret))]
  public class TurretTargetFilterPatch
  {
    [HarmonyPrepare]
    private static bool Prepare() => false;

    [HarmonyPatch("AssessTargetPriority")]
    [HarmonyPrefix]
    private static bool FilterSameWeaponTypeTargets(
      Turret __instance,
      Unit targetCandidate,
      ref float priorityThreshold)
    {
      try
      {
        if ((UnityEngine.Object) targetCandidate == (UnityEngine.Object) null || targetCandidate.disabled)
          return true;
        WeaponStation[] weaponStations = __instance.GetWeaponStations();
        if (weaponStations == null || weaponStations.Length == 0)
          return true;
        string name = targetCandidate.name;
        if (string.IsNullOrEmpty(name))
          return true;
        foreach (WeaponStation weaponStation in weaponStations)
        {
          if (!((UnityEngine.Object) weaponStation.WeaponInfo.weaponPrefab == (UnityEngine.Object) null) && weaponStation.WeaponInfo.weaponPrefab.name == name)
          {
            priorityThreshold = Math.Max(priorityThreshold, 0.0f);
            typeof (Turret).GetField("target", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue((object) __instance, (object) null);
            return false;
          }
        }
        return true;
      }
      catch (Exception ex)
      {
        QOLPlugin.logger.LogError((object) $"Error in TurretTargetFilterPatch: {ex}");
        return true;
      }
    }
  }

  [HarmonyPatch(typeof (TargetCalc))]
  [HarmonyPatch("TargetLeadTime")]
  internal static class TargetLeadTimePatch
  {
    [HarmonyPrepare]
    private static bool Prepare() => false;

    [HarmonyPatch("TargetLeadTime")]
    [HarmonyPrefix]
    private static bool TargetLeadTimePrefix(
      Unit target,
      GameObject gun,
      Rigidbody muzzleRB,
      float muzzleVelocity,
      float dragCoef,
      int iterations,
      ref float __result)
    {
      if (iterations == 0)
      {
        __result = 0.0f;
        return false;
      }
      Vector3 vector3 = (UnityEngine.Object) target.rb != (UnityEngine.Object) null ? target.rb.velocity : Vector3.zero;
      float num1 = muzzleVelocity;
      if ((UnityEngine.Object) muzzleRB != (UnityEngine.Object) null)
        num1 += Vector3.Dot(muzzleRB.velocity, (target.transform.position - gun.transform.position).normalized);
      float num2 = Vector3.Distance(target.transform.position, gun.transform.position) / num1;
      for (int index = 0; index < iterations; ++index)
      {
        float num3 = Vector3.Distance(target.transform.position + vector3 * num2, gun.transform.position);
        num2 = (Mathf.Pow(2.71828f, dragCoef * num3 / num1) - 1f) / dragCoef;
        if (!float.IsFinite(num2) || (double) num2 > 120.0)
          __result = 120f;
      }
      __result = num2;
      return false;
    }
  }

  [HarmonyPatch(typeof (Hardpoint))]
  public static class HardpointPatches
  {
    [HarmonyPrepare]
    private static bool Prepare() => false;

    [HarmonyPatch("ModifyDrag")]
    [HarmonyPrefix]
    private static void Prefix_ModifyDrag(Hardpoint __instance, ref bool __runOriginal)
    {
      FieldInfo field = typeof (Hardpoint).GetField("hardpointSet", BindingFlags.Instance | BindingFlags.NonPublic);
      if (!(field != (FieldInfo) null))
        return;
      HardpointSet hardpointSet = (HardpointSet) field.GetValue((object) __instance);
      if (hardpointSet == null || !hardpointSet.name.Contains("Bay"))
        return;
      __runOriginal = false;
    }

    [HarmonyPatch("ModifyRCS")]
    [HarmonyPrefix]
    private static void Prefix_ModifyRCS(Hardpoint __instance, ref bool __runOriginal)
    {
      FieldInfo field = typeof (Hardpoint).GetField("hardpointSet", BindingFlags.Instance | BindingFlags.NonPublic);
      if (!(field != (FieldInfo) null))
        return;
      HardpointSet hardpointSet = (HardpointSet) field.GetValue((object) __instance);
      if (hardpointSet == null || !hardpointSet.name.Contains("Bay"))
        return;
      __runOriginal = false;
    }
  }

  [HarmonyPatch(typeof (Turret), "AimTurret", new System.Type[] {typeof (WeaponStation)})]
  private class Turret_AimTurret_Patch
  {
    [HarmonyPrepare]
    private static bool Prepare() => false;

    private static void Postfix(Turret __instance, ref bool __result)
    {
      Traverse traverse = Traverse.Create((object) __instance);
      Vector3 direction = traverse.Field("aimVector").GetValue<Vector3>();
      Vector3 normalized1 = direction.normalized;
      direction = __instance.GetDirection();
      Vector3 normalized2 = direction.normalized;
      float num = Vector3.Angle(normalized1, normalized2);
      __result = (double) num + (double) Mathf.Abs(traverse.Field("traverseError").GetValue<float>()) + (double) Mathf.Abs(traverse.Field("elevationError").GetValue<float>()) < 3.0;
      traverse.Field("onTarget").SetValue((object) __result);
    }
  }

  public static class NavLightsPatch
  {
    private static bool IsNightTime()
    {
      LevelInfo component = GameObject.Find("LevelInfo")?.GetComponent<LevelInfo>();
      if ((UnityEngine.Object) component == (UnityEngine.Object) null)
        return false;
      float timeOfDay = component.timeOfDay;
      return (double) timeOfDay < 6.0 || (double) timeOfDay >= 18.0;
    }

    public static bool Prefix_NavLight_OnSetGear(object __instance, Aircraft.OnSetGear e)
    {
      if (!QOLPlugin.NavLightsPatch.IsNightTime())
        return true;
      AccessTools.Method(__instance.GetType(), "Toggle", (System.Type[]) null, (System.Type[]) null)?.Invoke(__instance, new object[1]
      {
        (object) true
      });
      return false;
    }
  }

  public class RotorShaftSkipper : MonoBehaviour
  {
    private RotorShaft rotorShaft;
    private FieldInfo angularSpeedField;
    private FieldInfo angularSpeedRatioField;
    private FieldInfo conditionField;
    private FieldInfo detachedField;

    public void Initialize(RotorShaft rotorShaft)
    {
      this.rotorShaft = rotorShaft;
      System.Type type = typeof (RotorShaft);
      this.angularSpeedField = type.GetField("angularSpeed", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
      this.angularSpeedRatioField = type.GetField("angularSpeedRatio", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
      this.conditionField = type.GetField("condition", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
      this.detachedField = type.GetField("detached", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
      this.angularSpeedField?.SetValue((object) rotorShaft, (object) 0.0f);
      this.angularSpeedRatioField?.SetValue((object) rotorShaft, (object) 0.0f);
      this.conditionField?.SetValue((object) rotorShaft, (object) 0.0f);
      this.detachedField?.SetValue((object) rotorShaft, (object) true);
    }

    private void Update()
    {
    }

    private void FixedUpdate()
    {
    }
  }

  public class KillFeedCleanup
  {
    private static readonly WaitForSeconds _waitFifteenSeconds = new WaitForSeconds(15f);
    private static Dictionary<(string, string, string, string), int> _previousCounts = new Dictionary<(string, string, string, string), int>();

    public static IEnumerator CleanupStaleCountsCoroutine()
    {
      while (true)
      {
        yield return (object) QOLPlugin.KillFeedCleanup._waitFifteenSeconds;
        QOLPlugin.KillFeedCleanup.CleanupStaleCounts();
      }
    }

    private static void CleanupStaleCounts()
    {
      Dictionary<(string, string, string, string), int> dictionary = new Dictionary<(string, string, string, string), int>();
      foreach (QOLPlugin.ItemRelationship itemRelationship in QOLPlugin.KillFeedLogger._messageHistory)
      {
        foreach (QOLPlugin.RelationshipCount relationship in itemRelationship.Relationships)
        {
          if (relationship.Count > 0)
          {
            (string, string, string, string) key = (itemRelationship.Item1, itemRelationship.Type1, relationship.Item2, relationship.Type2);
            dictionary[key] = relationship.Count;
          }
        }
      }
      foreach (QOLPlugin.ItemRelationship itemRelationship in QOLPlugin.KillFeedLogger._messageHistory)
      {
        for (int index = itemRelationship.Relationships.Count - 1; index >= 0; --index)
        {
          QOLPlugin.RelationshipCount relationship = itemRelationship.Relationships[index];
          (string, string, string, string) key = (itemRelationship.Item1, itemRelationship.Type1, relationship.Item2, relationship.Type2);
          int num;
          if (QOLPlugin.KillFeedCleanup._previousCounts.TryGetValue(key, out num) && relationship.Count == num)
          {
            relationship.Count = 0;
            itemRelationship.Relationships.RemoveAt(index);
          }
        }
      }
      QOLPlugin.KillFeedCleanup._previousCounts = dictionary;
      QOLPlugin.KillFeedLogger._messageHistory.RemoveAll((Predicate<QOLPlugin.ItemRelationship>) (itemRel => itemRel.Relationships.Count == 0));
    }
  }

  public class RelationshipCount
  {
    public string Item2 { get; set; }

    public string Type2 { get; set; }

    public int Count { get; set; }
  }

  public class ItemRelationship
  {
    public string Item1 { get; set; }

    public string Type1 { get; set; }

    public List<QOLPlugin.RelationshipCount> Relationships { get; set; } = new List<QOLPlugin.RelationshipCount>();
  }

  public class WeaponSelectorDescriptionOnHover : 
    MonoBehaviour,
    IPointerEnterHandler,
    IEventSystemHandler,
    IPointerExitHandler
  {
    private TMP_Dropdown dropdown;
    private WeaponSelector weaponSelector;
    public AircraftSelectionMenu selectionMenu;

    private void Start()
    {
      this.weaponSelector = this.GetComponent<WeaponSelector>();
      this.dropdown = Traverse.Create((object) this.weaponSelector).Field("dropdown").GetValue<TMP_Dropdown>();
      if (!((UnityEngine.Object) this.dropdown.template != (UnityEngine.Object) null))
        return;
      Toggle componentInChildren = this.dropdown.template.GetComponentInChildren<Toggle>();
      if (!((UnityEngine.Object) componentInChildren != (UnityEngine.Object) null) || !((UnityEngine.Object) componentInChildren.GetComponent<QOLPlugin.WeaponDropdownDescriptionOnHover>() == (UnityEngine.Object) null))
        return;
      componentInChildren.gameObject.AddComponent<QOLPlugin.WeaponDropdownDescriptionOnHover>();
      QOLPlugin.WeaponDropdownDescriptionOnHover component = componentInChildren.gameObject.GetComponent<QOLPlugin.WeaponDropdownDescriptionOnHover>();
      component.weaponSelector = this.weaponSelector;
      component.selectionMenu = this.selectionMenu;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
      QOLPlugin.DisplayMountInfo(this.selectionMenu, this.weaponSelector?.GetValue());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
      QOLPlugin.DisplayMountInfo(this.selectionMenu, (WeaponMount) null);
    }
  }

  public class WeaponDropdownDescriptionOnHover : 
    MonoBehaviour,
    IPointerEnterHandler,
    IEventSystemHandler,
    IPointerExitHandler
  {
    private TMP_Dropdown dropdown;
    private TMP_Text itemLabel;
    public WeaponSelector weaponSelector;
    public AircraftSelectionMenu selectionMenu;

    private void Start()
    {
      this.dropdown = this.GetComponentInParent<TMP_Dropdown>();
      this.itemLabel = this.transform.Find("Item Label")?.GetComponent<TMP_Text>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
      if (!((UnityEngine.Object) this.dropdown != (UnityEngine.Object) null) || !((UnityEngine.Object) this.itemLabel != (UnityEngine.Object) null))
        return;
      string optionText = this.itemLabel.text;
      WeaponMount mount = (WeaponMount) null;
      if (optionText != "Empty")
      {
        WeaponMount weaponMount = Traverse.Create((object) this.weaponSelector).Field("getCache").GetValue<List<WeaponMount>>().Find((Predicate<WeaponMount>) (obj => obj.mountName == optionText));
        if ((UnityEngine.Object) weaponMount == (UnityEngine.Object) null)
          QOLPlugin.logger.LogError((object) ("Couldn't find mount for displayName " + optionText));
        mount = weaponMount;
      }
      if (!((UnityEngine.Object) mount != (UnityEngine.Object) null) || !((UnityEngine.Object) this.selectionMenu != (UnityEngine.Object) null))
        return;
      QOLPlugin.DisplayMountInfo(this.selectionMenu, mount);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
      QOLPlugin.DisplayMountInfo(this.selectionMenu, (WeaponMount) null);
    }
  }
}
