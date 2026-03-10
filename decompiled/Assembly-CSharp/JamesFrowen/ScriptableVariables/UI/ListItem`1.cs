// Decompiled with JetBrains decompiler
// Type: JamesFrowen.ScriptableVariables.UI.ListItem`1
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

#nullable disable
namespace JamesFrowen.ScriptableVariables.UI;

public abstract class ListItem<T> : MonoBehaviour
{
  [SerializeField]
  private GameObject activeRoot;

  public T Value { get; private set; }

  public virtual bool IsActive
  {
    get
    {
      this.CheckActiveRoot();
      return this.activeRoot.activeSelf;
    }
  }

  protected virtual void Awake()
  {
  }

  private void CheckActiveRoot()
  {
    if (!((Object) this.activeRoot == (Object) null))
      return;
    this.activeRoot = this.gameObject;
  }

  public virtual void SetActive(bool active)
  {
    this.CheckActiveRoot();
    if (this.activeRoot.activeSelf == active)
      return;
    this.activeRoot.SetActive(active);
  }

  protected abstract void SetValue(T value);

  public virtual float CalculateHeight(T value) => ((RectTransform) this.transform).sizeDelta.y;

  public void SetNewValue(T value)
  {
    this.Value = value;
    this.SetValue(value);
  }

  public void Redraw() => this.SetValue(this.Value);
}
