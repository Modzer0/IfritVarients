// Decompiled with JetBrains decompiler
// Type: ObjectiveOverlay
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using UnityEngine;
using UnityEngine.UI;

#nullable disable
public class ObjectiveOverlay : MonoBehaviour
{
  [SerializeField]
  private Image objectivePointer;
  [SerializeField]
  private Image objectiveDot;
  [SerializeField]
  private Image sizeIndicator;
  [SerializeField]
  private Text objectiveInfo;
  [SerializeField]
  private Transform pointerTail;
  private Color baseColor = Color.green;
  private bool hidden;
  public TextNoOverlap TextNoOverlap;

  private void Awake() => this.TextNoOverlap = new TextNoOverlap(this.objectiveInfo);

  public void Initialize(Transform iconLayer)
  {
    this.objectivePointer.transform.SetParent(iconLayer);
    this.objectiveInfo.transform.SetParent(iconLayer);
    this.hidden = false;
  }

  public void UpdateOverlay(MissionPosition.PositionResult result)
  {
    this.hidden = false;
    this.objectivePointer.enabled = true;
    this.objectiveInfo.enabled = true;
    Vector3 screenPoint = SceneSingleton<CameraStateManager>.i.mainCamera.WorldToScreenPoint(result.Position.ToLocalPosition());
    Vector3 vector3_1 = new Vector3((float) Screen.width * 0.5f, (float) Screen.height * 0.5f, 0.0f);
    Vector3 vector3_2 = vector3_1;
    Vector3 a = screenPoint - vector3_1;
    if ((double) a.z < 0.0)
      a *= -1f;
    Vector3 vector3_3 = Vector3.Scale(a, new Vector3(1f, 1f, 0.0f));
    float f = Mathf.Atan2(vector3_3.y, vector3_3.x);
    float num1 = Mathf.Tan(f);
    double num2 = (double) Vector3.Angle(SceneSingleton<CameraStateManager>.i.transform.forward, result.Direction);
    if (num2 > 90.0 || (double) Mathf.Abs(vector3_3.x) > (double) Screen.width * 0.5 || (double) Mathf.Abs(vector3_3.y) > (double) Screen.height * 0.5)
    {
      vector3_3 = (double) vector3_3.x <= 0.0 ? new Vector3(-vector3_2.x, -vector3_2.x * num1, 0.0f) : new Vector3(vector3_2.x, vector3_2.x * num1, 0.0f);
      if ((double) vector3_3.y > (double) vector3_2.y)
        vector3_3 = new Vector3(vector3_2.y / num1, vector3_2.y, 0.0f);
      else if ((double) vector3_3.y < -(double) vector3_2.y)
        vector3_3 = new Vector3(-vector3_2.y / num1, -vector3_2.y, 0.0f);
      this.sizeIndicator.enabled = false;
    }
    else
      this.sizeIndicator.enabled = true;
    Vector3 vector3_4 = vector3_3 + vector3_1;
    this.objectivePointer.transform.position = vector3_4;
    this.objectiveDot.transform.position = vector3_4;
    this.objectivePointer.transform.localEulerAngles = new Vector3(0.0f, 0.0f, (float) ((double) f * 57.295780181884766 - 90.0));
    if (num2 > 10.0)
    {
      this.objectivePointer.enabled = true;
      this.objectiveDot.enabled = false;
      this.TextNoOverlap.SetTarget((Vector2) this.pointerTail.position);
    }
    else
    {
      this.objectivePointer.enabled = false;
      this.objectiveDot.enabled = true;
      this.TextNoOverlap.SetTarget((Vector2) (this.objectiveDot.transform.position - Vector3.up * 25f));
    }
    float num3 = 0.0f;
    if (result.Range.HasValue)
      num3 = result.Range.Value;
    float num4 = (float) (1.0 / ((double) result.Distance != 0.0 ? (double) result.Distance : 0.0099999997764825821));
    this.sizeIndicator.transform.localScale = Vector3.one * (85f * num3 * num4);
    float num5 = (float) ((double) num3 * 20.0 * (double) num4 - 0.5);
    this.sizeIndicator.transform.localEulerAngles = Vector3.forward * num5 * 3f;
    this.sizeIndicator.color = this.baseColor * Mathf.Clamp01(num5);
    this.sizeIndicator.transform.position = this.objectivePointer.transform.position;
    this.objectiveInfo.text = $"{(result.Objective != null ? result.Objective.SavedObjective.DisplayName : "Waypoint")} {UnitConverter.DistanceReading(result.Distance)}";
    this.objectiveInfo.fontSize = (int) PlayerSettings.overlayTextSize;
  }

  public void HideOverlay()
  {
    if (this.hidden)
      return;
    this.hidden = true;
    this.objectivePointer.enabled = false;
    this.objectiveDot.enabled = false;
    this.objectiveInfo.enabled = false;
    this.sizeIndicator.enabled = false;
  }

  public void SetColor(Color color)
  {
    this.baseColor = color;
    this.objectivePointer.color = color;
    this.objectiveDot.color = color;
    this.sizeIndicator.color = color;
    this.objectiveInfo.color = color;
  }

  public void SetRaycastTarget(bool enabled)
  {
    this.objectivePointer.raycastTarget = enabled;
    this.objectiveDot.raycastTarget = enabled;
    this.sizeIndicator.raycastTarget = enabled;
    this.objectiveInfo.raycastTarget = enabled;
  }
}
