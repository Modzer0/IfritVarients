// Decompiled with JetBrains decompiler
// Type: qol.PatchConfig.PatchConfigRegistry
// Assembly: qol, Version=1.1.6.3, Culture=neutral, PublicKeyToken=null
// MVID: 3B6539EA-E38C-45EE-8FFF-FC58CC42BADC
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\BepInEx\plugins\qol_1.1.6.3b1.dll

using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using System;
using System.Collections.Generic;
using System.Reflection;

#nullable disable
namespace qol.PatchConfig;

public static class PatchConfigRegistry
{
  private static ManualLogSource _logger;
  private static bool _initialized;
  private static BaseUnityPlugin _plugin;
  private static readonly Dictionary<Type, PatchConfigRegistry.PatchRegistration> _patches = new Dictionary<Type, PatchConfigRegistry.PatchRegistration>();

  public static void Initialize(BaseUnityPlugin plugin, ManualLogSource logger)
  {
    if (PatchConfigRegistry._initialized)
    {
      logger?.LogWarning((object) "PatchConfigRegistry already initialized");
    }
    else
    {
      PatchConfigRegistry._plugin = plugin;
      PatchConfigRegistry._logger = logger;
      PatchConfigRegistry._initialized = true;
      Assembly executingAssembly = Assembly.GetExecutingAssembly();
      int num1 = 0;
      int num2 = 0;
      foreach (Type type in executingAssembly.GetTypes())
      {
        PatchConfigAttribute customAttribute1 = type.GetCustomAttribute<PatchConfigAttribute>();
        if (customAttribute1 != null)
        {
          ConfigEntry<bool> configEntry1 = plugin.Config.Bind<bool>(customAttribute1.Category, customAttribute1.Key, customAttribute1.DefaultEnabled, customAttribute1.Description);
          PatchConfigRegistry.PatchRegistration patchRegistration = new PatchConfigRegistry.PatchRegistration()
          {
            Config = customAttribute1,
            EnabledEntry = configEntry1
          };
          foreach (FieldInfo field in type.GetFields(BindingFlags.Static | BindingFlags.Public))
          {
            PatchValueAttribute customAttribute2 = field.GetCustomAttribute<PatchValueAttribute>();
            if (customAttribute2 != null)
            {
              object defaultValue = field.GetValue((object) null);
              ConfigEntryBase configEntry2 = PatchConfigRegistry.CreateConfigEntry(customAttribute1.Category, customAttribute2.Key, defaultValue, customAttribute2.Description, field.FieldType);
              PatchConfigRegistry.ValueRegistration valueRegistration = new PatchConfigRegistry.ValueRegistration()
              {
                Field = field,
                Attribute = customAttribute2,
                DefaultValue = defaultValue,
                ConfigEntry = configEntry2
              };
              if (configEntry2 != null)
              {
                object configEntryValue = PatchConfigRegistry.GetConfigEntryValue(configEntry2);
                field.SetValue((object) null, configEntryValue);
              }
              patchRegistration.Values.Add(valueRegistration);
              ++num2;
            }
          }
          PatchConfigRegistry._patches[type] = patchRegistration;
          ++num1;
          PatchConfigRegistry._logger?.LogDebug((object) $"Registered patch: [{customAttribute1.Category}] {customAttribute1.Key} {$"(enabled={configEntry1.Value}, values={patchRegistration.Values.Count})"}");
        }
      }
      PatchConfigRegistry._logger?.LogInfo((object) $"PatchConfigRegistry initialized: {num1} patches, {num2} values");
    }
  }

  private static ConfigEntryBase CreateConfigEntry(
    string category,
    string key,
    object defaultValue,
    string description,
    Type valueType)
  {
    if (valueType == typeof (bool))
      return (ConfigEntryBase) PatchConfigRegistry._plugin.Config.Bind<bool>(category, key, (bool) defaultValue, description);
    if (valueType == typeof (int))
      return (ConfigEntryBase) PatchConfigRegistry._plugin.Config.Bind<int>(category, key, (int) defaultValue, new ConfigDescription(description, (AcceptableValueBase) null, Array.Empty<object>()));
    if (valueType == typeof (float))
      return (ConfigEntryBase) PatchConfigRegistry._plugin.Config.Bind<float>(category, key, (float) defaultValue, new ConfigDescription(description, (AcceptableValueBase) null, Array.Empty<object>()));
    if (valueType == typeof (string))
      return (ConfigEntryBase) PatchConfigRegistry._plugin.Config.Bind<string>(category, key, (string) defaultValue, description);
    if (valueType.IsEnum)
    {
      MethodInfo method = typeof (ConfigFile).GetMethod("Bind", new Type[4]
      {
        typeof (string),
        typeof (string),
        valueType,
        typeof (string)
      });
      if (method != (MethodInfo) null)
        return (ConfigEntryBase) method.MakeGenericMethod(valueType).Invoke((object) PatchConfigRegistry._plugin.Config, new object[4]
        {
          (object) category,
          (object) key,
          defaultValue,
          (object) description
        });
    }
    PatchConfigRegistry._logger?.LogWarning((object) $"Unsupported config type: {valueType} for {category}.{key}");
    return (ConfigEntryBase) null;
  }

  private static object GetConfigEntryValue(ConfigEntryBase entry)
  {
    return entry.GetType().GetProperty("Value")?.GetValue((object) entry);
  }

  public static bool IsEnabled(Type patchType)
  {
    if (!PatchConfigRegistry._initialized)
    {
      PatchConfigRegistry._logger?.LogWarning((object) ("PatchConfigRegistry not initialized when checking " + patchType.Name));
      return true;
    }
    PatchConfigRegistry.PatchRegistration patchRegistration;
    return !PatchConfigRegistry._patches.TryGetValue(patchType, out patchRegistration) || patchRegistration.EnabledEntry.Value;
  }

  public static void SetEnabled(Type patchType, bool enabled)
  {
    PatchConfigRegistry.PatchRegistration patchRegistration;
    if (!PatchConfigRegistry._patches.TryGetValue(patchType, out patchRegistration))
      return;
    patchRegistration.EnabledEntry.Value = enabled;
    PatchConfigRegistry._logger?.LogInfo((object) $"Patch {patchType.Name} enabled={enabled}");
  }

  public static IEnumerable<(Type Type, string Category, string Key, bool Enabled, string Description)> GetAllPatches()
  {
    foreach (KeyValuePair<Type, PatchConfigRegistry.PatchRegistration> patch in PatchConfigRegistry._patches)
      yield return (patch.Key, patch.Value.Config.Category, patch.Value.Config.Key, patch.Value.EnabledEntry.Value, patch.Value.Config.Description);
  }

  public static IEnumerable<(string Key, Type FieldType, object Value, string Description)> GetPatchValues(
    Type patchType)
  {
    PatchConfigRegistry.PatchRegistration patchRegistration;
    if (PatchConfigRegistry._patches.TryGetValue(patchType, out patchRegistration))
    {
      foreach (PatchConfigRegistry.ValueRegistration valueRegistration in patchRegistration.Values)
      {
        object obj = valueRegistration.Field.GetValue((object) null);
        yield return (valueRegistration.Attribute.Key, valueRegistration.Field.FieldType, obj, valueRegistration.Attribute.Description);
      }
    }
  }

  public static void ResetToDefaults()
  {
    foreach (PatchConfigRegistry.PatchRegistration patchRegistration in PatchConfigRegistry._patches.Values)
    {
      patchRegistration.EnabledEntry.Value = patchRegistration.Config.DefaultEnabled;
      foreach (PatchConfigRegistry.ValueRegistration valueRegistration in patchRegistration.Values)
      {
        if (valueRegistration.ConfigEntry != null)
        {
          object obj = Convert.ChangeType(valueRegistration.DefaultValue, valueRegistration.Field.FieldType);
          PatchConfigRegistry.SetConfigEntryValue(valueRegistration.ConfigEntry, obj);
          valueRegistration.Field.SetValue((object) null, valueRegistration.DefaultValue);
        }
      }
    }
    PatchConfigRegistry._plugin.Config.Save();
    PatchConfigRegistry._logger?.LogInfo((object) "PatchConfigRegistry reset to defaults");
  }

  private static void SetConfigEntryValue(ConfigEntryBase entry, object value)
  {
    entry.GetType().GetProperty("Value")?.SetValue((object) entry, value);
  }

  public static void ReloadConfig()
  {
    PatchConfigRegistry._plugin.Config.Reload();
    foreach (PatchConfigRegistry.PatchRegistration patchRegistration in PatchConfigRegistry._patches.Values)
    {
      foreach (PatchConfigRegistry.ValueRegistration valueRegistration in patchRegistration.Values)
      {
        if (valueRegistration.ConfigEntry != null)
        {
          object configEntryValue = PatchConfigRegistry.GetConfigEntryValue(valueRegistration.ConfigEntry);
          valueRegistration.Field.SetValue((object) null, configEntryValue);
        }
      }
    }
    PatchConfigRegistry._logger?.LogInfo((object) "PatchConfigRegistry reloaded from config");
  }

  private class PatchRegistration
  {
    public PatchConfigAttribute Config { get; set; }

    public ConfigEntry<bool> EnabledEntry { get; set; }

    public List<PatchConfigRegistry.ValueRegistration> Values { get; } = new List<PatchConfigRegistry.ValueRegistration>();
  }

  private class ValueRegistration
  {
    public FieldInfo Field { get; set; }

    public PatchValueAttribute Attribute { get; set; }

    public object DefaultValue { get; set; }

    public ConfigEntryBase ConfigEntry { get; set; }
  }
}
