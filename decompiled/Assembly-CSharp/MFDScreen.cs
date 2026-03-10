// Decompiled with JetBrains decompiler
// Type: MFDScreen
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using UnityEngine;
using UnityEngine.UI;

#nullable disable
public class MFDScreen : MonoBehaviour
{
  public VirtualMFD virtualMFD;
  public Text label;
  public Image highlight;
  public bool isActive;
  public string shortName;
  public GameObject displayPanel;
  public bool aircraftOnly;

  private void Start() => this.AdaptScale();

  public void Setup(VirtualMFD mfd, string s)
  {
    this.virtualMFD = mfd;
    this.shortName = s;
    this.label.text = this.shortName;
  }

  private void AdaptScale()
  {
    if ((double) Screen.width / (double) Screen.height < 1.7000000476837158)
      this.transform.localScale = 0.79f * Vector3.one;
    else
      this.transform.localScale = Vector3.one;
  }

  public void CloseScreen(Vector3 posClose)
  {
    this.displayPanel.SetActive(false);
    this.transform.localPosition = posClose;
    this.highlight.enabled = false;
    this.isActive = false;
  }

  public void ShowScreen(Vector3 posShow)
  {
    this.displayPanel.SetActive(true);
    this.transform.localPosition = posShow;
    this.highlight.enabled = true;
    this.isActive = true;
  }
}
