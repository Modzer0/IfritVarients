// Decompiled with JetBrains decompiler
// Type: NuclearOption.MissionEditorScripts.UnitSelection
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.SavedMission;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

#nullable disable
namespace NuclearOption.MissionEditorScripts;

[DefaultExecutionOrder(-10)]
public class UnitSelection : SceneSingleton<UnitSelection>
{
  public static RaycastHit[] hitCache = new RaycastHit[100];
  [SerializeField]
  private LayerMask selectLayer = (LayerMask) -8193;
  [SerializeField]
  private LayerMask placeLayer = (LayerMask) -8193;
  [SerializeField]
  public Transform placementTransform;
  [SerializeField]
  private Transform cursor;
  [SerializeField]
  private bool setPositionTextWidth;
  [SerializeField]
  private TextMeshProUGUI positionText;
  [SerializeField]
  private EditorHandle handle;
  private List<object> disallowSelectionKeys = new List<object>();
  private IPlacingMenu placingMenu;
  private SelectionDetails _selectionDetails;

  private bool placeMode => this.placingMenu != null;

  public SelectionDetails SelectionDetails
  {
    get => this._selectionDetails;
    private set
    {
      if (this._selectionDetails == value)
        return;
      if (this._selectionDetails is IDisposable selectionDetails)
        selectionDetails.Dispose();
      this._selectionDetails = value;
    }
  }

  public SingleSelectionDetails HoverDetails { get; private set; }

  public SingleSelectionDetails GetSingleOrFirstMultiSelect()
  {
    SingleSelectionDetails firstMultiSelect;
    switch (this.SelectionDetails)
    {
      case SingleSelectionDetails selectionDetails1:
        firstMultiSelect = selectionDetails1;
        break;
      case MultiSelectSelectionDetails selectionDetails2:
        firstMultiSelect = selectionDetails2.Items[0];
        break;
      default:
        firstMultiSelect = (SingleSelectionDetails) null;
        break;
    }
    return firstMultiSelect;
  }

  private bool SelectDisallowed => this.disallowSelectionKeys.Count > 0;

  public event UnitSelection.SelectionChanged<SelectionDetails> OnSelect;

  public event UnitSelection.SelectionChanged<SingleSelectionDetails> OnHover;

  protected override void Awake()
  {
    base.Awake();
    MissionManager.onMissionLoad += new Action<Mission>(this.MissionManager_onMissionLoad);
  }

  private void OnDestroy()
  {
    MissionManager.onMissionLoad -= new Action<Mission>(this.MissionManager_onMissionLoad);
    if (!(this.SelectionDetails is IDisposable selectionDetails))
      return;
    selectionDetails.Dispose();
  }

  private void MissionManager_onMissionLoad(Mission obj)
  {
    this.ClearSelection();
    this.ClearHover();
  }

  public void RefreshSelected() => this.RefreshSelectionInternal();

  public void SetSelection(IEditorSelectable value) => this.SetSelectionInternal(value);

  public void ClearIfSelected(IEditorSelectable value)
  {
    if (this.SelectionDetails == null)
      return;
    if (this.SelectionDetails is SingleSelectionDetails selectionDetails1)
    {
      if (selectionDetails1?.Source != value)
        return;
      this.ClearSelection();
    }
    else
    {
      if (!(this.SelectionDetails is MultiSelectSelectionDetails selectionDetails))
        return;
      selectionDetails.ClearIfSelected(value);
    }
  }

  public void ClearSelection(IEditorSelectable hint)
  {
    if (!(this.SelectionDetails is SingleSelectionDetails selectionDetails))
      Debug.LogError((object) "Clear with Hint only supported with SingleSelectionDetails");
    else if (selectionDetails?.Source == hint)
      this.ClearSelectionInternal();
    else
      Debug.LogError((object) "ClearSelection called but hint was not selected");
  }

  public void ClearSelection() => this.ClearSelectionInternal();

  public void ReplaceSelection<T>(List<T> objects) where T : IEditorSelectable
  {
    if (objects.Count == 0)
      this.ClearSelection();
    else if (objects.Count == 1)
    {
      this.SetSelection((IEditorSelectable) objects[0]);
    }
    else
    {
      List<SingleSelectionDetails> safeList;
      if (!this.CreateSafeDetailsList<T>(objects, out safeList))
        return;
      this.ReplaceSelection(safeList);
    }
  }

  private bool CreateSafeDetailsList<T>(List<T> source, out List<SingleSelectionDetails> safeList) where T : IEditorSelectable
  {
    safeList = new List<SingleSelectionDetails>();
    System.Type type = (System.Type) null;
    foreach (T obj in source)
    {
      SingleSelectionDetails selectionDetails = obj.CreateSelectionDetails();
      if (type == (System.Type) null)
        type = selectionDetails.GetType();
      else if (selectionDetails.GetType() != type)
      {
        Debug.LogError((object) "ReplaceSelection failed because list of object did not create the same SelectionDetails type");
        safeList.Clear();
        return false;
      }
      safeList.Add(selectionDetails);
    }
    return true;
  }

  public void ReplaceSelection(List<SingleSelectionDetails> objects)
  {
    MultiSelectSelectionDetails details = new MultiSelectSelectionDetails();
    foreach (SingleSelectionDetails single in objects)
      details.Add(single);
    this.SetSelectionInternal((SelectionDetails) details);
  }

  public void ToggleInMultiSelection(IEditorSelectable selectable)
  {
    if (this.SelectionDetails == null)
      this.SetSelectionInternal(selectable);
    else if (this.SelectionDetails is SingleSelectionDetails selectionDetails2)
    {
      if (selectionDetails2.Source == selectable)
        this.ClearSelectionInternal();
      else
        this.AddToMultiSelection(selectable);
    }
    else if (this.SelectionDetails is MultiSelectSelectionDetails selectionDetails1)
    {
      if (selectionDetails1.Items.Any<SingleSelectionDetails>((Func<SingleSelectionDetails, bool>) (x => x.Source == selectable)))
      {
        selectionDetails1.Remove(selectable);
        this.RefreshSelected();
      }
      else
        this.AddToMultiSelection(selectable);
    }
    else
      Debug.LogError((object) $"Case for {this.SelectionDetails?.GetType()} not found");
  }

  public void AddToMultiSelection(IEditorSelectable selectable)
  {
    if (this.SelectionDetails == null)
    {
      this.SetSelectionInternal(selectable);
    }
    else
    {
      SingleSelectionDetails selectionDetails1 = selectable.CreateSelectionDetails();
      switch (this.SelectionDetails)
      {
        case SingleSelectionDetails single:
          if (selectionDetails1.GetType() == single.GetType())
          {
            MultiSelectSelectionDetails details = new MultiSelectSelectionDetails();
            details.Add(single);
            details.Add(selectionDetails1);
            this.SetSelectionInternal((SelectionDetails) details);
            break;
          }
          Debug.LogError((object) $"Can't multiselect {selectionDetails1.GetType().Name} with {single.GetType().Name}");
          break;
        case MultiSelectSelectionDetails selectionDetails2:
          if (selectionDetails1.GetType() == selectionDetails2.SelectionType)
          {
            selectionDetails2.Add(selectionDetails1);
            this.RefreshSelectionInternal();
            break;
          }
          Debug.LogError((object) $"Can't multiselect {selectionDetails1.GetType().Name} with {selectionDetails2.SelectionType.Name}");
          break;
        default:
          Debug.LogError((object) $"Case for {this.SelectionDetails?.GetType()} not found");
          break;
      }
    }
  }

  public void AddToMultiSelection<T>(List<T> objects) where T : IEditorSelectable
  {
    if (this.SelectionDetails == null)
    {
      this.ReplaceSelection<T>(objects);
    }
    else
    {
      List<SingleSelectionDetails> safeList;
      if (!this.CreateSafeDetailsList<T>(objects, out safeList))
        return;
      switch (this.SelectionDetails)
      {
        case SingleSelectionDetails single1:
          if (safeList[0].GetType() == single1.GetType())
          {
            MultiSelectSelectionDetails details = new MultiSelectSelectionDetails();
            details.Add(single1);
            foreach (SingleSelectionDetails single in safeList)
              details.Add(single);
            this.SetSelectionInternal((SelectionDetails) details);
            break;
          }
          Debug.LogError((object) $"Can't multiselect {safeList[0].GetType().Name} with {single1.GetType().Name}");
          break;
        case MultiSelectSelectionDetails selectionDetails:
          if (safeList[0].GetType().GetType() == selectionDetails.SelectionType)
          {
            foreach (SingleSelectionDetails single in safeList)
              selectionDetails.Add(single);
            this.RefreshSelectionInternal();
            break;
          }
          Debug.LogError((object) $"Can't multiselect {safeList[0].GetType().Name} with {selectionDetails.SelectionType.Name}");
          break;
        default:
          Debug.LogError((object) $"Case for {this.SelectionDetails?.GetType()} not found");
          break;
      }
    }
  }

  public void RemoveFromMultiSelection(IEditorSelectable obj)
  {
    if (this.SelectionDetails is MultiSelectSelectionDetails selectionDetails)
      selectionDetails.Remove(obj);
    else
      Debug.LogError((object) "Can't use RemoveFromMultiSelection when not using MultiSelectSelectionDetails");
  }

  public void RemoveFromMultiSelection<T>(List<T> objects, bool errorIfNotSeleted) where T : IEditorSelectable
  {
    if (this.SelectionDetails is MultiSelectSelectionDetails selectionDetails)
      selectionDetails.RemoveAll<T>(objects, errorIfNotSeleted);
    else
      Debug.LogError((object) "Can't use RemoveFromMultiSelection when not using MultiSelectSelectionDetails");
  }

  public void ReplaceMultiSelection(MultiSelectSelectionDetails hint, SingleSelectionDetails first)
  {
    if (this.SelectionDetails == hint)
      this.SetSelectionInternal((SelectionDetails) first);
    else
      Debug.LogError((object) "ReplaceMultiSelection called but hint was not selected");
  }

  public void ClearMultiSelection(MultiSelectSelectionDetails hint)
  {
    if (this.SelectionDetails == hint)
      this.ClearSelectionInternal();
    else
      Debug.LogError((object) "ClearMultiSelection called but hint was not selected");
  }

  private void RefreshSelectionInternal()
  {
    Faction faction;
    if (this.SelectionDetails != null && this.SelectionDetails.TryGetFaction(out faction))
      SceneSingleton<MissionEditor>.i.stickyFaction = (UnityEngine.Object) faction != (UnityEngine.Object) null ? faction.factionName : "";
    UnitSelection.SelectionChanged<SelectionDetails> onSelect = this.OnSelect;
    if (onSelect == null)
      return;
    onSelect(this.SelectionDetails);
  }

  private void SetSelectionInternal(IEditorSelectable selectable)
  {
    this.SetSelectionInternal((SelectionDetails) selectable.CreateSelectionDetails());
  }

  private void SetSelectionInternal(SelectionDetails details)
  {
    this.SelectionDetails = details;
    Faction faction;
    if (details.TryGetFaction(out faction))
      SceneSingleton<MissionEditor>.i.stickyFaction = (UnityEngine.Object) faction != (UnityEngine.Object) null ? faction.factionName : "";
    UnitSelection.SelectionChanged<SelectionDetails> onSelect = this.OnSelect;
    if (onSelect == null)
      return;
    onSelect(details);
  }

  private void ClearSelectionInternal()
  {
    this.SelectionDetails = (SelectionDetails) null;
    UnitSelection.SelectionChanged<SelectionDetails> onSelect = this.OnSelect;
    if (onSelect == null)
      return;
    onSelect((SelectionDetails) null);
  }

  public void SetHover(IEditorSelectable value) => this.SetHoverInternal(value);

  public void ClearHover(IEditorSelectable hint)
  {
    if (this.HoverDetails?.Source != hint)
      return;
    this.SetHoverInternal((IEditorSelectable) null);
  }

  private void ClearHover() => this.SetHoverInternal((IEditorSelectable) null);

  private void SetHoverInternal(IEditorSelectable selectable)
  {
    if (this.HoverDetails?.Source == selectable)
      return;
    SingleSelectionDetails selectionDetails = selectable?.CreateSelectionDetails();
    this.HoverDetails = selectionDetails;
    UnitSelection.SelectionChanged<SingleSelectionDetails> onHover = this.OnHover;
    if (onHover == null)
      return;
    onHover(selectionDetails);
  }

  private void Update()
  {
    if (DynamicMap.mapMaximized || (UnityEngine.Object) Camera.main == (UnityEngine.Object) null)
      return;
    this.CheckRaycast();
    if (!this.placeMode || !Input.GetKeyDown(KeyCode.Escape))
      return;
    this.placingMenu.CancelPlace();
  }

  private void CheckRaycast()
  {
    bool flag1 = EventSystem.current.IsPointerOverGameObject();
    bool flag2 = Input.GetMouseButtonDown(0) && !flag1;
    if (this.handle.MouseHoverOrInteract)
    {
      this.cursor.gameObject.SetActive(false);
    }
    else
    {
      bool flag3 = false;
      RaycastHit hit = new RaycastHit();
      if (!flag1)
        flag3 = this.DoRayCast(this.placeMode, out hit);
      this.cursor.gameObject.SetActive(flag3);
      if (!flag3)
      {
        if (this.placeMode)
          return;
        if (flag2)
          this.TryDeselect();
        else
          this.ClearHover();
      }
      else
      {
        this.MoveCursorTransforms(hit);
        if (this.placeMode)
        {
          if (!flag2)
            return;
          (bool placeMore, IEditorSelectable placedObject) = this.placingMenu.Place(Input.GetKey(KeyCode.LeftShift));
          if (placeMore)
            return;
          this.placingMenu = (IPlacingMenu) null;
          if (placedObject == null)
            return;
          this.SetSelection(placedObject);
        }
        else
        {
          IEditorSelectable selectable1;
          bool selectable2 = this.TryGetSelectable(hit, out selectable1);
          if (flag2)
          {
            if (Input.GetKey(KeyCode.LeftShift))
            {
              if (!selectable2)
                return;
              this.ToggleInMultiSelection(selectable1);
            }
            else if (selectable2)
              this.SetSelection(selectable1);
            else
              this.TryDeselect();
          }
          else if (selectable2)
          {
            this.SetHover(selectable1);
          }
          else
          {
            SingleSelectionDetails hoverDetails = this.HoverDetails;
            if ((hoverDetails != null ? (hoverDetails.AutoUnhover ? 1 : 0) : 0) == 0)
              return;
            this.ClearHover();
          }
        }
      }
    }
  }

  private bool DoRayCast(bool placeMode, out RaycastHit hit)
  {
    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
    LayerMask layerMask = placeMode ? this.placeLayer : this.selectLayer;
    bool flag = Physics.Raycast(ray, out hit, 100000f, (int) layerMask);
    float enter;
    if (Datum.WaterPlane().Raycast(ray, out enter) && (double) enter < 100000.0)
    {
      if (!flag || (double) enter < (double) hit.distance)
      {
        hit.point = Camera.main.transform.position + ray.direction * enter;
        hit.normal = Vector3.up;
      }
      flag = true;
    }
    return flag;
  }

  private RaycastHit MoveCursorTransforms(RaycastHit hit)
  {
    Vector3 point = hit.point;
    point.y = Mathf.Max(point.y, Datum.LocalSeaY);
    this.placementTransform.position = point;
    this.placementTransform.rotation = Quaternion.FromToRotation(this.transform.up, hit.normal);
    float num = Vector3.Distance(SceneSingleton<CameraStateManager>.i.transform.position, hit.point);
    this.cursor.transform.position = point + this.placementTransform.up * 0.2f * num * 0.025f;
    this.cursor.transform.localScale = new Vector3(num * 0.025f, 1f, num * 0.025f);
    this.positionText.text = point.ToGlobalPosition().AsVector3().ToString("0.0");
    if (this.setPositionTextWidth)
      this.positionText.SetRectWidth(this.positionText.preferredWidth);
    this.placingMenu?.MoveCursor(this.placementTransform);
    return hit;
  }

  private bool TryGetSelectable(RaycastHit hit, out IEditorSelectable selectable)
  {
    if (this.SelectDisallowed)
    {
      selectable = (IEditorSelectable) null;
      return false;
    }
    Collider collider = hit.collider;
    if ((UnityEngine.Object) collider == (UnityEngine.Object) null)
    {
      selectable = (IEditorSelectable) null;
      return false;
    }
    selectable = collider.GetComponentInParent<IEditorSelectable>();
    if (selectable != null)
      return true;
    IDamageable component;
    if (collider.TryGetComponent<IDamageable>(out component))
    {
      selectable = (IEditorSelectable) component.GetUnit();
      if (selectable != null)
        return true;
    }
    return false;
  }

  private void TryDeselect()
  {
    if (this.SelectionDetails == null)
      return;
    if (this.handle.MouseHoverOrInteract)
      Debug.LogWarning((object) "Mouse over handle, no de-select");
    else if (this.SelectionDetails is SingleSelectionDetails selectionDetails1)
    {
      if (!UnitSelection.TryDeselect(selectionDetails1))
        return;
      this.ClearSelection();
    }
    else
    {
      if (!(this.SelectionDetails is MultiSelectSelectionDetails selectionDetails) || !selectionDetails.Items.All<SingleSelectionDetails>(new Func<SingleSelectionDetails, bool>(UnitSelection.TryDeselect)))
        return;
      this.ClearSelection();
    }
  }

  private static bool TryDeselect(SingleSelectionDetails details)
  {
    if (details is UnitSelectionDetails selectionDetails1)
      return CheckDistanceClear(selectionDetails1.Unit.transform.position);
    if (details is WaypointSelectionDetails selectionDetails2)
    {
      Rect worldRect = selectionDetails2.Handle.GetWorldRect();
      return !new Rect(worldRect.x - 150f, worldRect.y - 150f, worldRect.width + 300f, worldRect.height + 300f).Contains(Input.mousePosition);
    }
    return details.PositionWrapper == null || CheckDistanceClear(details.PositionWrapper.Value.ToLocalPosition());

    static bool CheckDistanceClear(Vector3 position)
    {
      return (double) Vector3.Distance(Input.mousePosition, SceneSingleton<CameraStateManager>.i.mainCamera.WorldToScreenPoint(position) with
      {
        z = 0.0f
      }) > 200.0;
    }
  }

  public void StartPlaceUnit(IPlacingMenu placingMenu)
  {
    this.placingMenu = placingMenu;
    placingMenu.MoveCursor(this.placementTransform);
    this.ClearSelection();
  }

  public void StopPlacingUnit(IPlacingMenu placingMenu) => this.placingMenu = (IPlacingMenu) null;

  public Vector3 GetPlacementUpAxis(UnitDefinition placingDefinition)
  {
    Vector3 up = Vector3.up;
    if (!(placingDefinition is BuildingDefinition))
      up = this.placementTransform.up;
    return up;
  }

  public static void DisallowSelection(object key, bool disallow)
  {
    if (disallow)
      SceneSingleton<UnitSelection>.i.disallowSelectionKeys.Add(key);
    else
      SceneSingleton<UnitSelection>.i.disallowSelectionKeys.Remove(key);
  }

  public delegate void SelectionChanged<T>(T details) where T : SelectionDetails;
}
