// Decompiled with JetBrains decompiler
// Type: ObjectiveMarker
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.SavedMission.ObjectiveV2;
using UnityEngine;
using UnityEngine.UI;

#nullable disable
public class ObjectiveMarker : MapMarker
{
  private ObjectiveMarkerManager manager;
  [SerializeField]
  private Text objName;
  [SerializeField]
  private Sprite destroyObjective;
  [SerializeField]
  private Sprite waypointObjective;
  [SerializeField]
  private Sprite captureObjective;
  [SerializeField]
  private Sprite reconObjective;
  public bool shown;
  public bool masked;
  private MissionPosition.PositionResult posResult;

  public void SetObjective(
    ObjectiveMarkerManager objManager,
    MissionPosition.PositionResult positionResult)
  {
    this.manager = objManager;
    this.posResult = positionResult;
    Objective objective = positionResult.Objective;
    this.objName.text = objective.SavedObjective.DisplayName;
    this.gameObject.name = "OBJ_" + objective.SavedObjective.TypeName;
    this.SetSprite(positionResult);
    this.Show(true);
  }

  public void UpdateMarker(MissionPosition.PositionResult pos)
  {
    this.transform.eulerAngles = Vector3.zero;
    this.transform.localScale = Vector3.one * (1f / SceneSingleton<DynamicMap>.i.mapImage.transform.localScale.x) * 1f;
    Vector3 vector3 = pos.Position.AsVector3() * SceneSingleton<DynamicMap>.i.mapDisplayFactor;
    this.transform.localPosition = new Vector3(vector3.x, vector3.z, 0.0f);
    if (pos.Objective == this.posResult.Objective && pos.Position.Equals(this.posResult.Position))
      return;
    this.posResult = pos;
    this.objName.text = pos.Objective.SavedObjective.DisplayName;
    this.SetSprite(pos);
  }

  public override void Show(bool value)
  {
    this.markerImg.color = Color.white;
    this.markerImg.enabled = value;
    this.objName.enabled = value;
    this.shown = value;
    this.masked = false;
  }

  public override void Mask()
  {
    this.markerImg.color = (0.5f * this.markerImg.color) with
    {
      a = 0.5f
    };
    this.objName.enabled = false;
    this.masked = true;
  }

  public void SetSprite(MissionPosition.PositionResult positionResult)
  {
    Objective objective = positionResult.Objective;
    float y = 20f;
    if ((objective.SavedObjective.Type == ObjectiveType.ReachWaypoints || objective.SavedObjective.Type == ObjectiveType.ReachUnits) && (Object) this.markerImg.sprite != (Object) this.waypointObjective)
      this.markerImg.sprite = this.waypointObjective;
    else if (objective.SavedObjective.Type == ObjectiveType.DestroyUnits && (Object) this.markerImg.sprite != (Object) this.destroyObjective)
      this.markerImg.sprite = this.destroyObjective;
    else if (objective.SavedObjective.Type == ObjectiveType.SpotUnit && (Object) this.markerImg.sprite != (Object) this.reconObjective)
    {
      this.markerImg.sprite = this.reconObjective;
      y = 40f;
    }
    else if (objective.SavedObjective.Type == ObjectiveType.CaptureAirbase && (Object) this.markerImg.sprite != (Object) this.captureObjective)
    {
      this.markerImg.sprite = this.captureObjective;
      y = 40f;
    }
    this.markerImg.rectTransform.sizeDelta = y * Vector2.one;
    this.objName.rectTransform.localPosition = new Vector3(0.0f, y, 0.0f);
  }
}
