// Decompiled with JetBrains decompiler
// Type: LoadingScreen
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;
using UnityEngine.UI;

#nullable disable
public class LoadingScreen : MonoBehaviour, IProgress<float>
{
  private static LoadingScreen i;
  [SerializeField]
  private Sprite[] images;
  [SerializeField]
  private Image loadingImage;
  [SerializeField]
  private Slider loadingProgress;
  private int activeCount;
  private float progressStart;
  private float progressEnd;

  public static LoadingScreen GetLoadingScreen()
  {
    if ((UnityEngine.Object) LoadingScreen.i == (UnityEngine.Object) null)
    {
      LoadingScreen.i = UnityEngine.Object.Instantiate<LoadingScreen>(GameAssets.i.loadingScreenPrefab);
      UnityEngine.Object.DontDestroyOnLoad((UnityEngine.Object) LoadingScreen.i.gameObject);
      LoadingScreen.i.gameObject.SetActive(false);
    }
    return LoadingScreen.i;
  }

  private void Awake()
  {
    this.loadingProgress.minValue = 0.0f;
    this.loadingProgress.maxValue = 1f;
  }

  public void ShowLoadingScreen(Sprite imageOverride = null)
  {
    if (GameManager.IsHeadless)
      return;
    ColorLog<LoadingScreen>.Info($"ShowLoadingScreen activeCount={this.activeCount}");
    ++this.activeCount;
    if (this.activeCount > 1)
      return;
    this.loadingImage.sprite = (UnityEngine.Object) imageOverride != (UnityEngine.Object) null ? imageOverride : this.images[UnityEngine.Random.Range(0, this.images.Length)];
    this.SetProgressRange(0.0f, 1f);
    this.loadingProgress.value = 0.0f;
    this.gameObject.SetActive(true);
  }

  public void HideLoadingScreen()
  {
    if (GameManager.IsHeadless)
      return;
    --this.activeCount;
    ColorLog<LoadingScreen>.Info($"HideLoadingScreen activeCount={this.activeCount}");
    if (this.activeCount >= 1)
      return;
    this.loadingProgress.value = 1f;
    this.gameObject.SetActive(false);
  }

  public void SetProgressRange(float start, float end)
  {
    this.progressStart = start;
    this.progressEnd = end;
  }

  void IProgress<float>.Report(float value)
  {
    if (GameManager.IsHeadless)
      return;
    this.loadingProgress.value = Mathf.Lerp(this.progressStart, this.progressEnd, value);
  }
}
