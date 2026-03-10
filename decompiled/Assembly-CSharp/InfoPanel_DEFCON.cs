// Decompiled with JetBrains decompiler
// Type: InfoPanel_DEFCON
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

#nullable disable
public class InfoPanel_DEFCON : MonoBehaviour
{
  [FormerlySerializedAs("currentlevelIndicator")]
  public GameObject currentLevelIndicator;
  [FormerlySerializedAs("currentlevelLabel")]
  public Text currentLevelLabel;
  private float previousLevel;
  public Text levelTactical;
  public Text levelStrategic;
  public Text levelMAD;

  private void Start()
  {
    this.levelTactical.text = NetworkSceneSingleton<MissionManager>.i.tacticalThreshold.ToString("N0");
    this.levelStrategic.text = NetworkSceneSingleton<MissionManager>.i.strategicThreshold.ToString("N0");
    this.levelMAD.text = (5f * NetworkSceneSingleton<MissionManager>.i.strategicThreshold).ToString("N0");
    this.UpdateUI(0.0f);
  }

  private void Update()
  {
    float currentEscalation = NetworkSceneSingleton<MissionManager>.i.currentEscalation;
    if ((double) this.previousLevel == (double) currentEscalation)
      return;
    this.previousLevel = currentEscalation;
    this.UpdateUI(currentEscalation);
  }

  private void UpdateUI(float currentLevel)
  {
    float tacticalThreshold = NetworkSceneSingleton<MissionManager>.i.tacticalThreshold;
    float strategicThreshold = NetworkSceneSingleton<MissionManager>.i.strategicThreshold;
    this.currentLevelLabel.text = currentLevel.ToString("N0");
    this.currentLevelIndicator.transform.localPosition = new Vector3(FastMath.Map(currentLevel, 0.0f, tacticalThreshold, 0.0f, 120f) - 120f + FastMath.Map(currentLevel, tacticalThreshold, strategicThreshold, 0.0f, 120f) + FastMath.Map(currentLevel, strategicThreshold, strategicThreshold * 4f, 0.0f, 120f), this.currentLevelIndicator.transform.localPosition.y, this.currentLevelIndicator.transform.localPosition.z);
  }
}
