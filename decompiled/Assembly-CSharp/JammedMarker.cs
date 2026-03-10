// Decompiled with JetBrains decompiler
// Type: JammedMarker
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;
using UnityEngine.UI;

#nullable disable
public class JammedMarker : MonoBehaviour
{
  [SerializeField]
  private GameObject vectorLinePrefab;
  private GameObject vectorLine;
  private Image vectorLineImage;
  private Radar radar;
  private Unit unit;
  private Unit jammedBy;
  private UnitMapIcon mapIcon;
  private UnitMapIcon jammedByIcon;

  public void Setup(UnitMapIcon unitIcon, Unit jammedBy, Radar radar)
  {
    this.unit = unitIcon.unit;
    this.mapIcon = unitIcon;
    this.radar = radar;
    this.transform.localScale = Vector3.one * (1f / SceneSingleton<DynamicMap>.i.mapImage.transform.localScale.x);
    this.transform.position = unitIcon.transform.position;
    this.jammedBy = jammedBy;
    if (DynamicMap.TryGetMapIcon(jammedBy, out this.jammedByIcon))
      this.AddVectorLine();
    this.unit.onJam += new Action<Unit.JamEventArgs>(this.JammedMarker_OnUnitJammed);
  }

  private void AddVectorLine()
  {
    this.vectorLine = UnityEngine.Object.Instantiate<GameObject>(this.vectorLinePrefab, SceneSingleton<DynamicMap>.i.iconLayer.transform);
    this.vectorLineImage = this.vectorLine.GetComponent<Image>();
    this.vectorLineImage.color = Color.yellow;
  }

  private void Remove()
  {
    if ((UnityEngine.Object) this.unit != (UnityEngine.Object) null)
      this.unit.onJam -= new Action<Unit.JamEventArgs>(this.JammedMarker_OnUnitJammed);
    UnityEngine.Object.Destroy((UnityEngine.Object) this.gameObject);
    UnityEngine.Object.Destroy((UnityEngine.Object) this.vectorLine);
  }

  private void JammedMarker_OnUnitJammed(Unit.JamEventArgs jam)
  {
    this.jammedBy = jam.jammingUnit;
    if (!DynamicMap.TryGetMapIcon(this.jammedBy, out this.jammedByIcon) || !((UnityEngine.Object) this.vectorLine == (UnityEngine.Object) null))
      return;
    this.AddVectorLine();
  }

  private void Update()
  {
    if ((UnityEngine.Object) this.unit == (UnityEngine.Object) null || (UnityEngine.Object) this.mapIcon == (UnityEngine.Object) null || this.unit.disabled || (UnityEngine.Object) this.radar == (UnityEngine.Object) null || (UnityEngine.Object) this.jammedBy == (UnityEngine.Object) null || this.jammedBy.disabled || !this.radar.IsJammed())
    {
      this.Remove();
    }
    else
    {
      this.transform.position = this.mapIcon.transform.position;
      this.transform.localScale = Vector3.one / SceneSingleton<DynamicMap>.i.mapImage.transform.localScale.x;
      if ((UnityEngine.Object) this.jammedByIcon != (UnityEngine.Object) null)
      {
        this.vectorLineImage.enabled = true;
        this.vectorLineImage.transform.position = this.transform.position;
        Vector3 vector3 = this.jammedByIcon.transform.position - this.transform.position;
        this.vectorLine.transform.eulerAngles = new Vector3(0.0f, 0.0f, (float) (-(double) Mathf.Atan2(vector3.x, vector3.y) * 57.295780181884766));
        this.vectorLine.transform.localScale = (Vector3.one + Vector3.up * vector3.magnitude) / SceneSingleton<DynamicMap>.i.iconLayer.transform.lossyScale.x;
      }
      else
      {
        if (!((UnityEngine.Object) this.vectorLineImage != (UnityEngine.Object) null))
          return;
        this.vectorLineImage.enabled = false;
      }
    }
  }
}
