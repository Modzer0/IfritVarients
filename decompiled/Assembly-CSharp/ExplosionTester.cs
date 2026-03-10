// Decompiled with JetBrains decompiler
// Type: ExplosionTester
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

#nullable disable
public class ExplosionTester : MonoBehaviour
{
  [SerializeField]
  private TMP_Text blastYieldDisplay;
  [SerializeField]
  private TMP_InputField xCoords;
  [SerializeField]
  private TMP_InputField yCoords;
  [SerializeField]
  private TMP_InputField zCoords;
  [SerializeField]
  private GameObject explosionPointPrefab;
  [SerializeField]
  private Slider yieldSlider;
  private GameObject explosionPoint;

  private void Awake()
  {
    this.xCoords.SetTextWithoutNotify($"{0}");
    this.yCoords.SetTextWithoutNotify($"{0}");
    this.zCoords.SetTextWithoutNotify($"{0}");
  }

  private void Start()
  {
    this.gameObject.SetActive(PlayerSettings.debugVis);
    if (!this.gameObject.activeSelf)
    {
      this.enabled = false;
    }
    else
    {
      this.explosionPoint = UnityEngine.Object.Instantiate<GameObject>(this.explosionPointPrefab, (Transform) null);
      this.explosionPoint.transform.position = Vector3.zero;
    }
  }

  public void UIInput()
  {
    this.blastYieldDisplay.text = $"{(ValueType) (float) ((double) this.yieldSlider.value * (double) this.yieldSlider.value):F2} kg TNT";
    this.explosionPoint.transform.position = new Vector3(float.Parse(this.xCoords.text), float.Parse(this.yCoords.text), float.Parse(this.zCoords.text));
  }

  private void Update()
  {
    if ((!Input.GetMouseButtonDown(0) ? 0 : (!EventSystem.current.IsPointerOverGameObject() ? 1 : 0)) == 0)
      return;
    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
    RaycastHit hitInfo;
    if (!Physics.Raycast(ray, out hitInfo, 100000f, -8193))
      return;
    this.explosionPoint.transform.position = hitInfo.point - ray.direction.normalized * 0.2f;
    this.xCoords.text = $"{this.explosionPoint.transform.position.x:F2}";
    this.yCoords.text = $"{this.explosionPoint.transform.position.y:F2}";
    this.zCoords.text = $"{this.explosionPoint.transform.position.z:F2}";
  }

  public void Detonate()
  {
    DamageEffects.BlastFrag(this.yieldSlider.value * this.yieldSlider.value, this.explosionPoint.transform.position, PersistentID.None, PersistentID.None);
  }
}
