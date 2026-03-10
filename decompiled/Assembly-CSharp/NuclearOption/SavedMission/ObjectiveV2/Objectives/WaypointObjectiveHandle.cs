// Decompiled with JetBrains decompiler
// Type: NuclearOption.SavedMission.ObjectiveV2.Objectives.WaypointObjectiveHandle
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.MissionEditorScripts;
using UnityEngine;
using UnityEngine.EventSystems;

#nullable disable
namespace NuclearOption.SavedMission.ObjectiveV2.Objectives;

public class WaypointObjectiveHandle : 
  MonoBehaviour,
  IPointerEnterHandler,
  IEventSystemHandler,
  IPointerExitHandler,
  IPointerClickHandler,
  IEditorSelectable
{
  private static readonly Vector3[] worldCorners = new Vector3[4];
  [SerializeField]
  private ObjectiveOverlay overlay;
  [SerializeField]
  private RectTransform selectRect;
  [SerializeField]
  private Color normalColor;
  [SerializeField]
  private Color hoverColor;
  [SerializeField]
  private Color selectColor;
  private Transform proxy;
  private Waypoint waypoint;
  private Objective objective;
  private bool _selected;

  public WaypointEditor Editor { get; private set; }

  public int Index { get; private set; }

  public bool Selected => this._selected;

  public bool Hover { get; private set; }

  public void SetSelected(bool selected)
  {
    this._selected = selected;
    this.overlay.SetRaycastTarget(!selected);
  }

  public Waypoint Waypoint => this.waypoint;

  public Transform GetProxy()
  {
    if ((Object) this.proxy == (Object) null)
      this.proxy = new GameObject("WaypointProxy").transform;
    this.proxy.position = this.waypoint.GlobalPosition.Value.ToLocalPosition();
    return this.proxy;
  }

  public Rect GetWorldRect()
  {
    this.selectRect.GetWorldCorners(WaypointObjectiveHandle.worldCorners);
    return new Rect(WaypointObjectiveHandle.worldCorners[0].x, WaypointObjectiveHandle.worldCorners[0].y, WaypointObjectiveHandle.worldCorners[2].x - WaypointObjectiveHandle.worldCorners[0].x, WaypointObjectiveHandle.worldCorners[2].y - WaypointObjectiveHandle.worldCorners[0].y);
  }

  public void SetWaypoint(
    WaypointEditor editor,
    int index,
    Waypoint waypoint,
    Objective objective)
  {
    if (this.waypoint != waypoint && this.Selected)
      SceneSingleton<UnitSelection>.i.ClearSelection((IEditorSelectable) this);
    this.Editor = editor;
    this.Index = index;
    this.waypoint = waypoint;
    this.objective = objective;
    this.gameObject.SetActive(true);
  }

  public void Hide()
  {
    this.Editor = (WaypointEditor) null;
    this.Index = 0;
    this.waypoint = (Waypoint) null;
    this.gameObject.SetActive(false);
    this.overlay.HideOverlay();
    if (!this.Selected)
      return;
    SceneSingleton<UnitSelection>.i.ClearSelection((IEditorSelectable) this);
  }

  public void DeleteWaypoint()
  {
    if (this.Editor == null)
      return;
    this.Editor.DeleteWaypoint(this.Index);
  }

  private void Update()
  {
    GlobalPosition globalPosition = SceneSingleton<CameraStateManager>.i.mainCamera.transform.position.ToGlobalPosition();
    this.overlay.UpdateOverlay(MissionPosition.ResultForPosition(this.waypoint.ToObjectivePosition(), globalPosition));
  }

  public void OnPointerEnter(PointerEventData eventData)
  {
    this.Hover = true;
    if (this.Selected)
      return;
    this.overlay.SetColor(this.hoverColor);
    SceneSingleton<UnitSelection>.i.SetHover((IEditorSelectable) this);
  }

  public void OnPointerExit(PointerEventData eventData)
  {
    this.Hover = false;
    if (this.Selected)
      return;
    this.overlay.SetColor(this.normalColor);
    SceneSingleton<UnitSelection>.i.ClearHover((IEditorSelectable) this);
  }

  public void OnPointerClick(PointerEventData eventData)
  {
    if (this.Selected)
      return;
    this.overlay.SetColor(this.selectColor);
    SceneSingleton<UnitSelection>.i.SetSelection((IEditorSelectable) this);
    this.SetSelected(true);
    SceneSingleton<UnitSelection>.i.OnSelect += new UnitSelection.SelectionChanged<SelectionDetails>(this.SelectionChanged);
  }

  private void SelectionChanged(SelectionDetails details)
  {
    if (details is WaypointSelectionDetails selectionDetails && (Object) selectionDetails.Handle == (Object) this)
      return;
    this.SetSelected(false);
    this.overlay.SetColor(this.normalColor);
    SceneSingleton<UnitSelection>.i.OnSelect -= new UnitSelection.SelectionChanged<SelectionDetails>(this.SelectionChanged);
  }

  private void OnDestroy()
  {
    if (this.Selected)
      SceneSingleton<UnitSelection>.i.ClearSelection((IEditorSelectable) this);
    if (!((Object) this.proxy != (Object) null))
      return;
    Object.Destroy((Object) this.proxy.gameObject);
  }

  public string GetDisplayName()
  {
    return $"{this.objective.SavedObjective.DisplayName} - {this.Waypoint.GlobalPosition}";
  }

  SingleSelectionDetails IEditorSelectable.CreateSelectionDetails()
  {
    return (SingleSelectionDetails) new WaypointSelectionDetails(this);
  }
}
