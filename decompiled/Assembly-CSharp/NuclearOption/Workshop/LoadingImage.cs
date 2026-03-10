// Decompiled with JetBrains decompiler
// Type: NuclearOption.Workshop.LoadingImage
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using UnityEngine;
using UnityEngine.UI;

#nullable disable
namespace NuclearOption.Workshop;

public class LoadingImage : MonoBehaviour
{
  [SerializeField]
  private Image loadingImage;
  [SerializeField]
  private float fadeTime;
  [SerializeField]
  private CanvasGroup group;
  private float alpha;
  private bool active;

  private void Awake()
  {
    if (this.active)
      return;
    this.gameObject.SetActive(false);
  }

  private void Update()
  {
    if (this.active)
    {
      if ((double) this.alpha < 1.0)
        this.alpha += Time.deltaTime / this.fadeTime;
      else
        this.alpha = 1f;
    }
    else
      this.alpha -= Time.deltaTime / this.fadeTime;
    if ((double) this.alpha <= 0.0)
    {
      this.alpha = 0.0f;
      this.active = false;
      this.gameObject.SetActive(false);
    }
    else
    {
      this.group.alpha = this.alpha;
      this.loadingImage.transform.Rotate(new Vector3(0.0f, 0.0f, -100f * Time.deltaTime));
    }
  }

  public void SetActive(bool active)
  {
    this.active = active;
    if (!active)
      return;
    this.gameObject.SetActive(true);
  }
}
