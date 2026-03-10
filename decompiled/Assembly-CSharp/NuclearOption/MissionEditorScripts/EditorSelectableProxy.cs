// Decompiled with JetBrains decompiler
// Type: NuclearOption.MissionEditorScripts.EditorSelectableProxy
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

#nullable disable
namespace NuclearOption.MissionEditorScripts;

public class EditorSelectableProxy : MonoBehaviour, IEditorSelectable
{
  private IEditorSelectable flagOwner;

  public static void Add(IEditorSelectable flagOwner, GameObject addTo)
  {
    addTo.AddComponent<EditorSelectableProxy>().flagOwner = flagOwner;
  }

  public SingleSelectionDetails CreateSelectionDetails() => this.flagOwner.CreateSelectionDetails();
}
