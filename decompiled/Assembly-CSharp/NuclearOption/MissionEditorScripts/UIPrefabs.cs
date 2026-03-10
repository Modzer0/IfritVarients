// Decompiled with JetBrains decompiler
// Type: NuclearOption.MissionEditorScripts.UIPrefabs
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.SavedMission.ObjectiveV2.Objectives;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#nullable disable
namespace NuclearOption.MissionEditorScripts;

[CreateAssetMenu(menuName = "NuclearOption/UI Prefabs")]
public class UIPrefabs : ScriptableObject
{
  public TextMeshProUGUI TextPrefab;
  public RectTransform GroupBoxPrefab;
  public Vector3DataField VectorFieldPrefab;
  public FloatDataField FloatFieldPrefab;
  public StringDataField StringFieldPrefab;
  public BoolDataField BoolFieldPrefab;
  public ReferenceList ReferenceListPrefab;
  public ReferenceDataField ReferenceDataPrefab;
  public EmptyDataList DataListPrefab;
  public DropdownDataField Dropdown;
  public WaypointObjectiveHandle WaypointEditor;
  public OverrideDataField OverrideField;
  public FactionDataField FactionDataPrefab;
  public HorizontalLayoutGroup HorizontalGroupPrefab;
  public VerticalLayoutGroup VerticalGroupPrefab;
  public float GroupBoxPadding = 20f;
}
