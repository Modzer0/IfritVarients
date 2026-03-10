// Decompiled with JetBrains decompiler
// Type: qol.FieldModification.Core.FieldProcessingContext
// Assembly: qol, Version=1.1.6.3, Culture=neutral, PublicKeyToken=null
// MVID: 3B6539EA-E38C-45EE-8FFF-FC58CC42BADC
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\BepInEx\plugins\qol_1.1.6.3b1.dll

using BepInEx.Logging;
using System.Text.RegularExpressions;
using UnityEngine;

#nullable disable
namespace qol.FieldModification.Core;

public class FieldProcessingContext
{
  public object TargetObject { get; }

  public GameObject TargetGameObject { get; }

  public string Path { get; }

  public string ComponentPath { get; }

  public string FieldName { get; }

  public string Value { get; }

  public Match Match { get; }

  public ManualLogSource Logger { get; }

  public QOLPlugin Plugin { get; }

  public FieldProcessingContext(
    object targetObject,
    GameObject targetGameObject,
    string path,
    string componentPath,
    string fieldName,
    string value,
    Match match,
    ManualLogSource logger,
    QOLPlugin plugin)
  {
    this.TargetObject = targetObject;
    this.TargetGameObject = targetGameObject;
    this.Path = path;
    this.ComponentPath = componentPath;
    this.FieldName = fieldName;
    this.Value = value;
    this.Match = match;
    this.Logger = logger;
    this.Plugin = plugin;
  }
}
