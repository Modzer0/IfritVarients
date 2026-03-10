// Decompiled with JetBrains decompiler
// Type: NuclearOption.Networking.Lobbies.LobbyListFadeOverlay
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

#nullable disable
namespace NuclearOption.Networking.Lobbies;

public class LobbyListFadeOverlay : MonoBehaviour
{
  [SerializeField]
  private GameObject refreshingOverlay;
  [SerializeField]
  private CanvasGroup refreshingOverlayGroup;
  [SerializeField]
  private float fadeInTime;
  [SerializeField]
  private float fadeOutTime;
  private float alpha;
  private float targetAlpha;
  private float speed;

  public void Show()
  {
    this.refreshingOverlay.SetActive(true);
    this.targetAlpha = 1f;
    this.speed = (double) this.fadeInTime > 0.0 ? 1f / this.fadeInTime : 10000f;
    this.enabled = true;
  }

  public void Hide()
  {
    this.targetAlpha = 0.0f;
    this.speed = (double) this.fadeOutTime > 0.0 ? 1f / this.fadeInTime : 10000f;
  }

  private void LateUpdate()
  {
    this.alpha = Mathf.MoveTowards(this.alpha, this.targetAlpha, Time.deltaTime * this.speed);
    this.refreshingOverlayGroup.alpha = this.alpha;
    if ((double) this.alpha != (double) this.targetAlpha || (double) this.targetAlpha != 0.0)
      return;
    this.refreshingOverlay.SetActive(false);
    this.enabled = false;
  }
}
