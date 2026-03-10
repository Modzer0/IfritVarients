// Decompiled with JetBrains decompiler
// Type: AllocationDisplay
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.Networking;
using TMPro;
using UnityEngine;

#nullable disable
public class AllocationDisplay : SceneSingleton<AllocationDisplay>
{
  [SerializeField]
  private TMP_Text allocationReadout;
  private float lastActivatedTime;
  private bool visible = true;

  protected override void Awake()
  {
    base.Awake();
    this.gameObject.SetActive(false);
    this.enabled = false;
  }

  public void SetVisible(bool visible)
  {
    this.visible = visible && PlayerSettings.cinematicMode;
    if (!visible)
    {
      this.gameObject.SetActive(false);
      this.enabled = false;
    }
    Debug.Log((object) $"Setting Allocation Display Visibility to {visible}");
  }

  public void Show(Player localPlayer, float change)
  {
    string str1 = $"<color=#{ColorUtility.ToHtmlStringRGBA((double) localPlayer.Allocation >= 0.0 ? Color.white : Color.red)}>";
    string str2 = (double) change >= 0.0 ? "+" : "-";
    string str3 = $"<color=#{ColorUtility.ToHtmlStringRGBA((double) change >= 0.0 ? Color.green + Color.white * 0.5f : Color.red + Color.green * 0.5f)}>";
    this.allocationReadout.text = $"{str1}{UnitConverter.ValueReading(localPlayer.Allocation)}</color> {str3}({str2}{UnitConverter.ValueReading(Mathf.Abs(change))})</color>";
    if (!this.visible)
      return;
    this.enabled = true;
    this.gameObject.SetActive(true);
    this.lastActivatedTime = Time.timeSinceLevelLoad;
    this.transform.position = SceneSingleton<GameplayUI>.i.topPanelTransform.position;
  }

  private void Update()
  {
    float num = Time.timeSinceLevelLoad - this.lastActivatedTime;
    if ((double) num <= 5.0)
      return;
    this.transform.position += Vector3.up * 50f * Time.deltaTime;
    if ((double) num <= 6.0)
      return;
    this.enabled = false;
    this.gameObject.SetActive(false);
  }
}
