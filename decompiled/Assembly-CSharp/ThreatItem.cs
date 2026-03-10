// Decompiled with JetBrains decompiler
// Type: ThreatItem
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using UnityEngine;
using UnityEngine.UI;

#nullable disable
public class ThreatItem : MonoBehaviour
{
  [SerializeField]
  private Text text;
  [SerializeField]
  private GameObject vectorLinePrefab;
  private Missile missile;
  private GameObject notchLine;
  private GameObject vectorLine;
  private GameObject notchIndicator;
  private Image notchIndicatorBox;
  private Text notchIndicatorLabel;
  private Transform playerAircraftIconTransform;
  private Transform missileIconTransform;

  public void SetItem(PersistentID missileID)
  {
    Unit unit;
    this.missile = UnitRegistry.TryGetUnit(new PersistentID?(missileID), out unit) ? unit as Missile : (Missile) null;
    this.gameObject.SetActive(false);
  }

  public bool FoundIcon()
  {
    if ((Object) this.missileIconTransform != (Object) null)
      return true;
    UnitMapIcon mapIcon1;
    UnitMapIcon mapIcon2;
    if (!DynamicMap.TryGetMapIcon((Unit) SceneSingleton<CombatHUD>.i.aircraft, out mapIcon1) || !DynamicMap.TryGetMapIcon((Unit) this.missile, out mapIcon2))
      return false;
    this.playerAircraftIconTransform = mapIcon1.transform;
    this.missileIconTransform = mapIcon2.transform;
    if (this.missile.GetSeekerType() == "ARH" || this.missile.GetSeekerType() == "SARH")
    {
      this.notchLine = SceneSingleton<DynamicMap>.i.ShowNotchLine();
      this.notchIndicator = SceneSingleton<CombatHUD>.i.ShowNotchIndicator();
      this.notchIndicatorBox = this.notchIndicator.GetComponent<Image>();
      this.notchIndicatorLabel = this.notchIndicator.GetComponentInChildren<Text>();
    }
    this.vectorLine = Object.Instantiate<GameObject>(this.vectorLinePrefab, SceneSingleton<DynamicMap>.i.iconLayer.transform);
    this.vectorLine.SetActive(false);
    this.gameObject.SetActive(true);
    return true;
  }

  public void AnimateItem()
  {
    if (!this.FoundIcon())
    {
      this.gameObject.SetActive(false);
    }
    else
    {
      if (this.missile.seekerMode == Missile.SeekerMode.activeSearch)
      {
        this.text.color = Color.yellow;
        this.vectorLine.SetActive(false);
      }
      else
      {
        this.text.color = Color.red + Color.green * Mathf.Sin(Time.realtimeSinceStartup * 20f);
        this.AlignVectorLine();
      }
      this.text.text = $"Missile [{this.missile.GetSeekerType()}] {UnitConverter.DistanceReading(Vector3.Distance(SceneSingleton<CombatHUD>.i.aircraft.transform.position, this.missile.transform.position))}";
      if ((Object) this.notchLine == (Object) null || (Object) this.playerAircraftIconTransform == (Object) null)
        return;
      Vector3 evasionVector = this.missile.GetEvasionPoint() - SceneSingleton<CombatHUD>.i.aircraft.GlobalPosition();
      this.AlignNotchLine(evasionVector);
      this.AlignNotchIndicator(evasionVector);
    }
  }

  private void OnDisable()
  {
    if ((Object) this.notchLine != (Object) null)
      this.notchLine.SetActive(false);
    if (!((Object) this.vectorLine != (Object) null))
      return;
    this.vectorLine.SetActive(false);
  }

  private void OnEnable()
  {
    if (!((Object) this.notchLine != (Object) null))
      return;
    this.notchLine.SetActive(true);
  }

  private void OnDestroy()
  {
    if ((Object) this.notchLine != (Object) null)
      Object.Destroy((Object) this.notchLine);
    if ((Object) this.vectorLine != (Object) null)
      Object.Destroy((Object) this.vectorLine);
    if (!((Object) this.notchIndicator != (Object) null))
      return;
    Object.Destroy((Object) this.notchIndicator);
  }

  private void AlignVectorLine()
  {
    if ((Object) this.missileIconTransform == (Object) null || (Object) this.playerAircraftIconTransform == (Object) null)
      return;
    this.vectorLine.SetActive(true);
    this.vectorLine.transform.position = this.playerAircraftIconTransform.position;
    Vector3 vector3 = this.missileIconTransform.position - this.playerAircraftIconTransform.position;
    this.vectorLine.transform.eulerAngles = new Vector3(0.0f, 0.0f, (float) (-(double) Mathf.Atan2(vector3.x, vector3.y) * 57.295780181884766));
    this.vectorLine.transform.localScale = (Vector3.one + Vector3.up * vector3.magnitude) / SceneSingleton<DynamicMap>.i.iconLayer.transform.lossyScale.x;
  }

  private void AlignNotchLine(Vector3 evasionVector)
  {
    Vector3 rhs = Vector3.Cross(evasionVector, SceneSingleton<CombatHUD>.i.aircraft.rb.velocity);
    Vector3 vector3 = Vector3.Cross(this.missile.GetEvasionPoint() - SceneSingleton<CombatHUD>.i.aircraft.GlobalPosition(), rhs);
    if ((double) Vector3.Dot(SceneSingleton<CombatHUD>.i.aircraft.transform.forward, vector3) < 0.0)
      vector3 *= -1f;
    vector3.y = 0.0f;
    Quaternion quaternion = Quaternion.LookRotation(vector3, Vector3.up);
    this.notchLine.transform.position = this.playerAircraftIconTransform.position;
    this.notchLine.transform.eulerAngles = new Vector3(0.0f, 0.0f, SceneSingleton<DynamicMap>.i.mapImage.transform.eulerAngles.z - quaternion.eulerAngles.y);
  }

  private void AlignNotchIndicator(Vector3 evasionVector)
  {
    if ((Object) this.notchIndicator == (Object) null)
      return;
    Vector3 to = new Vector3(evasionVector.x, 0.0f, evasionVector.z);
    Vector3 rhs1 = Vector3.Cross(evasionVector, SceneSingleton<CombatHUD>.i.aircraft.rb.velocity);
    Vector3 rhs2 = Vector3.Cross(evasionVector, rhs1);
    if ((double) Vector3.Dot(SceneSingleton<CombatHUD>.i.aircraft.transform.forward, rhs2) < 0.0)
      rhs2 *= -1f;
    Vector3 b = new Vector3(1f, 1f, 0.0f);
    GlobalPosition position = SceneSingleton<CombatHUD>.i.aircraft.GlobalPosition() + 1000f * rhs2;
    float num = Vector3.SignedAngle(evasionVector, to, Vector3.up);
    this.notchIndicator.transform.position = Vector3.Scale(SceneSingleton<CameraStateManager>.i.mainCamera.WorldToScreenPoint(position.ToLocalPosition()), b);
    this.notchIndicator.transform.eulerAngles = new Vector3(0.0f, 0.0f, num - SceneSingleton<CombatHUD>.i.aircraft.transform.eulerAngles.z);
    float distance = FastMath.Distance(SceneSingleton<CombatHUD>.i.aircraft.transform.position, this.missile.transform.position);
    Color color = Color.green;
    if (this.missile.seekerMode == Missile.SeekerMode.activeLock)
      color = Color.Lerp(Color.yellow, Color.red, Mathf.Sin(Time.timeSinceLevelLoad * 20f) + 0.5f);
    else if (this.missile.seekerMode == Missile.SeekerMode.activeSearch)
      color = Color.Lerp(Color.green, Color.yellow, Mathf.Sin(Time.timeSinceLevelLoad * 10f) + 0.5f);
    this.notchIndicatorBox.color = color;
    this.notchIndicatorLabel.text = $"[{this.missile.GetSeekerType()}] {UnitConverter.DistanceReading(distance)}";
    this.notchIndicatorLabel.color = color;
  }
}
