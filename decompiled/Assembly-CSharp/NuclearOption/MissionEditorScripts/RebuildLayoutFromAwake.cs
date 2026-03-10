// Decompiled with JetBrains decompiler
// Type: NuclearOption.MissionEditorScripts.RebuildLayoutFromAwake
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

#nullable disable
namespace NuclearOption.MissionEditorScripts;

public class RebuildLayoutFromAwake : MonoBehaviour
{
  [SerializeField]
  private RectTransform target;

  private void OnValidate()
  {
    if (!((Object) this.target == (Object) null))
      return;
    this.target = this.transform.AsRectTransform();
  }

  private void Awake()
  {
    if ((Object) this.target == (Object) null)
      this.target = this.transform.AsRectTransform();
    FixLayout.ForceRebuildAtEndOfFrame(this.target);
  }
}
