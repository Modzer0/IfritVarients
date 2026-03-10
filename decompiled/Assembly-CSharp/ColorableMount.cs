// Decompiled with JetBrains decompiler
// Type: ColorableMount
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

#nullable disable
public class ColorableMount : MonoBehaviour
{
  [SerializeField]
  protected Renderer[] colorableRenderers;
  [SerializeField]
  protected Renderer[] skinnableRenderers;

  public void AttachToAircraft(Aircraft aircraft)
  {
    foreach (Renderer colorableRenderer in this.colorableRenderers)
      aircraft.weaponManager.RegisterColorable(colorableRenderer);
    foreach (Renderer skinnableRenderer in this.skinnableRenderers)
      aircraft.weaponManager.RegisterSkinnable(skinnableRenderer);
    Object.Destroy((Object) this);
  }
}
