// Decompiled with JetBrains decompiler
// Type: NuclearOption.MissionEditorScripts.EditorHandle
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Cysharp.Threading.Tasks;
using NuclearOption.SavedMission.ObjectiveV2;
using RuntimeHandle;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

#nullable disable
namespace NuclearOption.MissionEditorScripts;

[DefaultExecutionOrder(10)]
public class EditorHandle : MonoBehaviour
{
  [SerializeField]
  private UnitSelection unitSelection;
  [SerializeField]
  private GameObject handlePrefab;
  [Header("Buttons")]
  [SerializeField]
  private GameObject holder;
  [Space]
  [SerializeField]
  private Button selectMode;
  [SerializeField]
  private Button positionMode;
  [SerializeField]
  private Button rotationMode;
  [Space]
  [SerializeField]
  private Button worldMode;
  [SerializeField]
  private Button localMode;
  [Space]
  [SerializeField]
  private GameObject groupHolder;
  [SerializeField]
  private Button groupCenterMode;
  [SerializeField]
  private Button groupLocalMode;
  [Space]
  [SerializeField]
  private Color activeColor;
  [SerializeField]
  private Color deactiveColor;
  [Tooltip("default mode")]
  [SerializeField]
  private HandleType type;
  [SerializeField]
  private HandleType typeLastSelected;
  [SerializeField]
  private HandleSpace space = HandleSpace.LOCAL;
  [SerializeField]
  private Bounds globalBounds = new Bounds(Vector3.zero, Vector3.one * 100000f);
  private RuntimeTransformHandle handle;
  private SelectionDetails selection;
  private IValueWrapper<GlobalPosition> positionWrapper;
  private IValueWrapper<Quaternion> rotationWrapper;
  private Transform proxyTransform;

  public bool MouseHoverOrInteract => this.handle.MouseHoverOrInteract;

  private void OnDrawGizmosSelected()
  {
    Gizmos.color = Color.red;
    Vector3 center = this.globalBounds.center;
    if ((Object) Datum.origin != (Object) null)
      center += Datum.originPosition;
    Gizmos.DrawWireCube(center, this.globalBounds.size);
  }

  private void Awake()
  {
    this.selectMode.onClick.AddListener((UnityAction) (() => this.SetMode(HandleType.NONE)));
    this.positionMode.onClick.AddListener((UnityAction) (() => this.SetMode(HandleType.POSITION)));
    this.rotationMode.onClick.AddListener((UnityAction) (() => this.SetMode(HandleType.ROTATION)));
    this.worldMode.onClick.AddListener((UnityAction) (() => this.SetSpace(HandleSpace.WORLD)));
    this.localMode.onClick.AddListener((UnityAction) (() => this.SetSpace(HandleSpace.LOCAL)));
    this.groupCenterMode.onClick.AddListener((UnityAction) (() => this.SetGroupRotate(GroupRotationMode.Center)));
    this.groupLocalMode.onClick.AddListener((UnityAction) (() => this.SetGroupRotate(GroupRotationMode.Local)));
    this.handle = Object.Instantiate<GameObject>(this.handlePrefab).GetComponent<RuntimeTransformHandle>();
    this.handle.endedDraggingHandle.AddListener(new UnityAction(this.OnDragEnd));
    this.proxyTransform = new GameObject("EditorHandleProxy").transform;
    this.handle.target = this.proxyTransform;
    this.unitSelection.OnSelect += new UnitSelection.SelectionChanged<SelectionDetails>(this.UnitSelection_OnSelect);
    this.UnitSelection_OnSelect(this.unitSelection.SelectionDetails);
    this.SetMode(this.type, true);
    this.SetSpace(this.space, true);
    this.SetGroupRotate(MultiSelectSelectionDetails.RotationMode);
    this.type = HandleType.NONE;
    this.RefreshHandle();
    this.RunEarlyUpdate().Forget();
  }

  private void OnDragEnd()
  {
    if (!(this.selection is MultiSelectSelectionDetails selection))
      return;
    selection.RecalculatePositionsAndRotation();
  }

  private void OnDestroy()
  {
    this.unitSelection.OnSelect -= new UnitSelection.SelectionChanged<SelectionDetails>(this.UnitSelection_OnSelect);
    this.ClearWrapper();
    if (!((Object) this.proxyTransform != (Object) null))
      return;
    Object.Destroy((Object) this.proxyTransform.gameObject);
  }

  private void UnitSelection_OnSelect(SelectionDetails selectionDetails)
  {
    if (this.selection == selectionDetails)
      return;
    this.selection = selectionDetails;
    this.ClearWrapper();
    bool flag1 = false;
    bool flag2 = false;
    if (this.selection != null)
    {
      this.positionWrapper = this.selection.PositionWrapper;
      this.rotationWrapper = this.selection.RotationWrapper;
      if (this.positionWrapper != null)
      {
        this.positionWrapper.RegisterOnChange((object) this, new ValueWrapper<GlobalPosition>.OnChangeDelegate(this.OnPositionChanged));
        this.OnPositionChanged(this.positionWrapper.Value);
      }
      if (this.rotationWrapper != null)
      {
        this.rotationWrapper.RegisterOnChange((object) this, new ValueWrapper<Quaternion>.OnChangeDelegate(this.OnRotationChanged));
        this.OnRotationChanged(this.rotationWrapper.Value);
      }
      flag1 = this.positionWrapper != null && this.selection.PositionHandleAllowed;
      flag2 = this.rotationWrapper != null && this.selection.RotationHandleAllowed;
    }
    this.positionMode.interactable = flag1;
    this.rotationMode.interactable = flag2;
    this.worldMode.interactable = flag1 | flag1;
    this.localMode.interactable = flag1 | flag1;
    this.type = this.typeLastSelected;
    if (this.type == HandleType.POSITION && !flag1)
      this.type = flag2 ? HandleType.ROTATION : HandleType.NONE;
    if (this.type == HandleType.ROTATION && !flag2)
      this.type = flag1 ? HandleType.POSITION : HandleType.NONE;
    this.RefreshHandle();
    this.groupHolder.SetActive(flag2 && this.selection is MultiSelectSelectionDetails);
    FixLayout.ForceRebuildRecursive(this.groupHolder.transform.parent.AsRectTransform());
  }

  private void ClearSelection() => this.UnitSelection_OnSelect((SelectionDetails) null);

  private void ClearWrapper()
  {
    this.positionWrapper?.UnregisterOnChange((object) this);
    this.rotationWrapper?.UnregisterOnChange((object) this);
    this.positionWrapper = (IValueWrapper<GlobalPosition>) null;
    this.rotationWrapper = (IValueWrapper<Quaternion>) null;
  }

  private void OnPositionChanged(GlobalPosition newValue)
  {
    this.proxyTransform.position = newValue.ToLocalPosition();
  }

  private void OnRotationChanged(Quaternion newValue) => this.proxyTransform.rotation = newValue;

  private void SetMode(HandleType value, bool forceRefresh = false)
  {
    if (this.type == value && !forceRefresh)
      return;
    this.type = value;
    this.typeLastSelected = value;
    this.RefreshHandle();
    this.selectMode.GetComponent<MaskableGraphic>().color = this.type == HandleType.NONE ? this.activeColor : this.deactiveColor;
    this.positionMode.GetComponent<MaskableGraphic>().color = this.type == HandleType.POSITION ? this.activeColor : this.deactiveColor;
    this.rotationMode.GetComponent<MaskableGraphic>().color = this.type == HandleType.ROTATION ? this.activeColor : this.deactiveColor;
  }

  private void SetSpace(HandleSpace value, bool forceRefresh = false)
  {
    if (this.space == value && !forceRefresh)
      return;
    this.space = value;
    this.RefreshHandle();
    this.worldMode.GetComponent<MaskableGraphic>().color = this.space == HandleSpace.WORLD ? this.activeColor : this.deactiveColor;
    this.localMode.GetComponent<MaskableGraphic>().color = this.space == HandleSpace.LOCAL ? this.activeColor : this.deactiveColor;
  }

  private void SetGroupRotate(GroupRotationMode mode)
  {
    MultiSelectSelectionDetails.RotationMode = mode;
    this.groupCenterMode.GetComponent<MaskableGraphic>().color = mode == GroupRotationMode.Center ? this.activeColor : this.deactiveColor;
    this.groupLocalMode.GetComponent<MaskableGraphic>().color = mode == GroupRotationMode.Local ? this.activeColor : this.deactiveColor;
    if (!(this.selection is MultiSelectSelectionDetails selection))
      return;
    selection.RecalculatePositionsAndRotation();
  }

  private void RefreshHandle()
  {
    this.holder.SetActive(this.selection != null);
    int num1 = this.handle.gameObject.activeSelf ? 1 : 0;
    bool flag = this.type != HandleType.NONE;
    int num2 = flag ? 1 : 0;
    if (num1 != num2)
    {
      this.handle.gameObject.SetActive(flag);
      if (flag)
        this.handle.ForceRefresh();
    }
    if (this.selection == null || this.type == HandleType.NONE)
      return;
    this.handle.type = this.type;
    this.handle.axes = HandleAxes.XYZ;
    this.handle.space = this.space;
    if (this.type != HandleType.POSITION)
      return;
    this.handle.axes = this.selection.AllowedPositionAxes;
  }

  private async UniTask RunEarlyUpdate()
  {
    EditorHandle editorHandle = this;
    CancellationToken cancel = editorHandle.destroyCancellationToken;
    YieldAwaitable yieldAwaitable = UniTask.Yield(PlayerLoopTiming.LastEarlyUpdate);
    await yieldAwaitable;
    while (!cancel.IsCancellationRequested)
    {
      editorHandle.EarlyUpdate();
      yieldAwaitable = UniTask.Yield(PlayerLoopTiming.LastEarlyUpdate);
      await yieldAwaitable;
    }
    cancel = new CancellationToken();
  }

  private void EarlyUpdate()
  {
    if (this.selection == null || !this.selection.IsDestroyed)
      return;
    this.ClearSelection();
  }

  private void Update()
  {
    if (this.type == HandleType.NONE)
      return;
    if (this.positionWrapper != null)
    {
      GlobalPosition globalPosition = this.proxyTransform.GlobalPosition();
      if (this.selection is UnitSelectionDetails selection)
        globalPosition = selection.ClampPosition(globalPosition);
      this.proxyTransform.position = this.ClampBounds(globalPosition).ToLocalPosition();
      this.positionWrapper.SetValue(this.proxyTransform.transform.GlobalPosition(), (object) this);
    }
    if (this.rotationWrapper != null)
      this.rotationWrapper.SetValue(this.proxyTransform.transform.rotation, (object) this);
    if (Input.GetKeyDown(KeyCode.G))
      this.SetMode(HandleType.POSITION);
    else if (Input.GetKeyDown(KeyCode.R))
      this.SetMode(HandleType.ROTATION);
    Physics.SyncTransforms();
  }

  private GlobalPosition ClampBounds(GlobalPosition pos) => pos;
}
