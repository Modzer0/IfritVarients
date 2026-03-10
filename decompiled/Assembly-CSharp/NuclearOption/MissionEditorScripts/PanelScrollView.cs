// Decompiled with JetBrains decompiler
// Type: NuclearOption.MissionEditorScripts.PanelScrollView
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Triggers;
using System;
using UnityEngine;
using UnityEngine.UI;

#nullable disable
namespace NuclearOption.MissionEditorScripts;

public class PanelScrollView : MonoBehaviour, ILayerRebuildRoot
{
  [SerializeField]
  private bool rebuildParent;
  [SerializeField]
  private RectTransform scrollParent;
  [SerializeField]
  private RectTransform scrollContent;
  [SerializeField]
  private float maxHeight;
  [SerializeField]
  private bool useMinWidth;
  [SerializeField]
  private float minWidth;
  private bool isRebuilding;
  private bool endOfFrameRequestRebuild;
  private RectTransform child;

  public T AddChild<T>(T prefab) where T : Component
  {
    T obj = UnityEngine.Object.Instantiate<T>(prefab, (Transform) this.scrollContent);
    this.child = (RectTransform) obj.transform;
    this.child.OnDestroyAsync().ContinueWith(new Action(this.ChildDestroyed)).Forget();
    return obj;
  }

  public GameObject AddChild(GameObject prefab)
  {
    GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(prefab, (Transform) this.scrollContent);
    this.child = (RectTransform) gameObject.transform;
    this.child.OnDestroyAsync().ContinueWith(new Action(this.ChildDestroyed)).Forget();
    return gameObject;
  }

  private void ChildDestroyed()
  {
    if (!((UnityEngine.Object) this != (UnityEngine.Object) null))
      return;
    UnityEngine.Object.Destroy((UnityEngine.Object) this.gameObject);
  }

  public void SetChild(RectTransform menu, bool rebuild = true)
  {
    this.child = menu;
    if (!rebuild)
      return;
    this.Rebuild();
  }

  void ILayerRebuildRoot.Rebuild()
  {
    if (!((UnityEngine.Object) this.child != (UnityEngine.Object) null))
      return;
    this.Rebuild();
  }

  void ILayerRebuildRoot.RebuildEndOfFrame()
  {
    if (this.endOfFrameRequestRebuild)
      return;
    this.endOfFrameRequestRebuild = true;
    this.DoRebuildEndOfFrame().Forget();
  }

  private async UniTaskVoid DoRebuildEndOfFrame()
  {
    await UniTask.Yield(PlayerLoopTiming.PostLateUpdate);
    this.endOfFrameRequestRebuild = false;
    if (!((UnityEngine.Object) this.child != (UnityEngine.Object) null))
      return;
    this.Rebuild();
  }

  public void Rebuild()
  {
    if (this.isRebuilding)
      return;
    this.isRebuilding = true;
    try
    {
      FixLayout.ForceRebuildRecursive(this.child);
      float num = LayoutUtility.GetMinWidth(this.child);
      if (this.useMinWidth)
        num = Mathf.Max(num, this.minWidth);
      float minHeight = LayoutUtility.GetMinHeight(this.child);
      this.scrollParent.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, num);
      DataDrawerHelper.SetRectHeight(this.scrollParent, Mathf.Min(minHeight, this.maxHeight));
      this.scrollContent.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, num);
      this.scrollContent.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, minHeight);
      if (!this.rebuildParent)
        return;
      FixLayout.ForceRebuildRecursive(this.transform.parent.AsRectTransform());
    }
    finally
    {
      this.isRebuilding = false;
    }
  }
}
