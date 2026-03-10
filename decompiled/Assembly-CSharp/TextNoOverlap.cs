// Decompiled with JetBrains decompiler
// Type: TextNoOverlap
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using UnityEngine;
using UnityEngine.UI;

#nullable disable
public class TextNoOverlap
{
  public readonly Text Text;
  public Vector2 TargetPosition;
  public Vector2 PreviousPosition;
  public Vector2 NudgeOffset;
  public bool AutomaticlalySetPosition;

  public TextNoOverlap(Text objectiveInfo) => this.Text = objectiveInfo;

  public void SetTarget(Vector2 target)
  {
    this.TargetPosition = target;
    if (!this.AutomaticlalySetPosition)
      return;
    this.Text.transform.position = (Vector3) target;
  }
}
