// Decompiled with JetBrains decompiler
// Type: GridLabels
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using System.Runtime.CompilerServices;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.UI;

#nullable disable
public class GridLabels : MonoBehaviour
{
  private static readonly ProfilerMarker gridLabels_OnMapChangedMarker = new ProfilerMarker("GridLabels_OnMapChanged");
  private static readonly ProfilerMarker updateMinorGridLabelsMarker = new ProfilerMarker("UpdateMinorGridLabels");
  private static readonly ProfilerMarker showGridToolTipMarker = new ProfilerMarker("ShowGridToolTip");
  private static readonly ProfilerMarker showGridAircraftMarker = new ProfilerMarker("ShowGridAircraft");
  private static readonly ProfilerMarker maximizeMarker = new ProfilerMarker("Maximize");
  private static readonly ProfilerMarker showLabelsMarker = new ProfilerMarker("ShowLabels");
  [SerializeField]
  private Text gridToolTip;
  [SerializeField]
  private Text gridAircraft;
  [SerializeField]
  private Font defaultFont;
  [SerializeField]
  private Material defaultMaterial;
  [SerializeField]
  private int offsetX = 80000;
  [SerializeField]
  private int offsetY = 80000;
  private GameObject majorParent;
  private GameObject minorParent;
  private Text[] listHorizontal;
  private Text[] listHorizontalMinor;
  private Text[] listVertical;
  private Text[] listVerticalMinor;
  private static string[] verticalMinorStrings;
  private static string[] horizontalMinorStrings;
  private int previousMinorVertical = int.MinValue;
  private int previousMinorHorizontal = int.MinValue;
  private float previousMapInverseScale = (float) int.MinValue;
  private float previousPosFixTop = (float) int.MinValue;
  private float previousPosFixLeft = (float) int.MinValue;
  public GameObject gridImage_prefab;
  private GameObject[] gridImages;

  public bool LabelShown { get; private set; }

  public bool LabelEnabled { get; private set; } = true;

  public void SetupGrid(int gridSizeX, int gridSizeY, int off_X, int off_Y)
  {
    this.offsetX = off_X;
    this.offsetY = off_Y;
    GridLabels.MakeNumberMinorStrings(ref GridLabels.horizontalMinorStrings, gridSizeX);
    GridLabels.MakeLetterMinorStrings(ref GridLabels.verticalMinorStrings, gridSizeY);
    if ((UnityEngine.Object) this.majorParent == (UnityEngine.Object) null)
    {
      this.majorParent = new GameObject("MajorParent", new System.Type[1]
      {
        typeof (RectTransform)
      });
      this.majorParent.transform.SetParent(this.transform, false);
    }
    if ((UnityEngine.Object) this.minorParent == (UnityEngine.Object) null)
    {
      this.minorParent = new GameObject("MinorParent", new System.Type[1]
      {
        typeof (RectTransform)
      });
      this.minorParent.transform.SetParent(this.transform, false);
    }
    if (this.listHorizontal == null || this.listHorizontal.Length < gridSizeX)
      this.listHorizontal = new Text[gridSizeX];
    if (this.listVertical == null || this.listVertical.Length < gridSizeY)
      this.listVertical = new Text[gridSizeY];
    for (int index = 0; index < this.listHorizontal.Length; ++index)
    {
      if ((UnityEngine.Object) this.listHorizontal[index] == (UnityEngine.Object) null)
      {
        string name = $"Top_{index + 1}";
        string text = $"{index}";
        this.listHorizontal[index] = this.MakeGridText(name, text, this.majorParent.transform);
      }
      this.listHorizontal[index].gameObject.SetActive(index < gridSizeX);
    }
    this.listVertical = new Text[gridSizeY];
    for (int index = 0; index < this.listVertical.Length; ++index)
    {
      if ((UnityEngine.Object) this.listVertical[index] == (UnityEngine.Object) null)
      {
        string name = $"Left_{index + 1}";
        string text = $"{(ValueType) (char) (65 + index)}";
        this.listVertical[index] = this.MakeGridText(name, text, this.majorParent.transform);
      }
      this.listVertical[index].gameObject.SetActive(index < gridSizeY);
    }
    if (this.listHorizontalMinor == null)
    {
      this.listHorizontalMinor = new Text[10];
      this.listVerticalMinor = new Text[10];
      for (int index = 0; index < 10; ++index)
      {
        string name1 = $"TopMinor_{index + 1}";
        string text1 = $"{index}";
        this.listHorizontalMinor[index] = this.MakeGridText(name1, text1, this.minorParent.transform);
        string name2 = $"LeftMinor_{index + 1}";
        string text2 = $"{(ValueType) (char) (97 + index)}";
        this.listVerticalMinor[index] = this.MakeGridText(name2, text2, this.minorParent.transform);
      }
    }
    int num1 = gridSizeX / 4;
    int num2 = gridSizeY / 4;
    Vector2 vector2 = 439.24f * Vector2.one;
    int newSize = num1 * num2;
    if (this.gridImages == null)
      this.gridImages = new GameObject[newSize];
    for (int index = newSize; index < this.gridImages.Length; ++index)
      UnityEngine.Object.Destroy((UnityEngine.Object) this.gridImages[index]);
    if (this.gridImages.Length != newSize)
      Array.Resize<GameObject>(ref this.gridImages, newSize);
    for (int index1 = 0; index1 < num1; ++index1)
    {
      for (int index2 = 0; index2 < num2; ++index2)
      {
        ref GameObject local = ref this.gridImages[index1 * num2 + index2];
        if ((UnityEngine.Object) local == (UnityEngine.Object) null)
          local = UnityEngine.Object.Instantiate<GameObject>(this.gridImage_prefab, this.transform);
        local.name = $"mapGrid_{index1}_{index2}";
        local.transform.localPosition = new Vector3(((float) (-num1 / 2 + index1) + 0.5f) * vector2.x, ((float) (num2 / 2 - index2) - 0.5f) * vector2.y, 0.0f);
      }
    }
    this.LabelShown = false;
  }

  private Text MakeGridText(string name, string text, Transform parent)
  {
    GameObject gameObject = new GameObject();
    gameObject.transform.SetParent(parent, false);
    gameObject.name = name;
    Text text1 = gameObject.AddComponent<Text>();
    text1.text = text;
    text1.font = this.defaultFont;
    text1.material = this.defaultMaterial;
    text1.supportRichText = false;
    text1.fontSize = 24;
    text1.alignment = TextAnchor.MiddleCenter;
    text1.horizontalOverflow = HorizontalWrapMode.Overflow;
    text1.verticalOverflow = VerticalWrapMode.Overflow;
    return text1;
  }

  private void LateUpdate()
  {
    if (!SceneSingleton<MapOptions>.i.showGridLabels)
      return;
    if (DynamicMap.mapMaximized)
    {
      this.ShowGridToolTip();
      this.GridLabels_OnMapChanged(true);
    }
    if ((UnityEngine.Object) SceneSingleton<CombatHUD>.i.aircraft != (UnityEngine.Object) null)
    {
      if (!this.gridAircraft.enabled)
        this.gridAircraft.enabled = true;
      this.ShowGridAircraft();
    }
    else
    {
      if (!this.gridAircraft.enabled)
        return;
      this.gridAircraft.enabled = false;
    }
  }

  public void GridLabels_OnMapChanged(bool _)
  {
    using (GridLabels.gridLabels_OnMapChangedMarker.Auto())
    {
      if (!SceneSingleton<MapOptions>.i.showGridLabels)
        return;
      if (DynamicMap.mapMaximized)
      {
        Transform transform = SceneSingleton<DynamicMap>.i.mapImage.transform;
        float current = 1f / transform.localScale.x;
        float num1 = (440f - transform.localPosition.y) * current;
        float num2 = (-440f - transform.localPosition.x) * current;
        bool changed = GridLabels.HasChanged(ref this.previousMapInverseScale, current) | GridLabels.HasChanged(ref this.previousPosFixTop, num1) | GridLabels.HasChanged(ref this.previousPosFixLeft, num2);
        if (changed)
        {
          for (int index = 0; index < this.listHorizontal.Length; ++index)
          {
            float x = ((float) -this.offsetX + (float) (5000.0 + (double) index * 10000.0)) * SceneSingleton<DynamicMap>.i.mapDisplayFactor;
            this.listHorizontal[index].transform.localScale = 0.5f * current * Vector3.one;
            this.listHorizontal[index].transform.localPosition = new Vector3(x, num1, 0.0f);
          }
        }
        if (changed)
        {
          for (int index = 0; index < this.listVertical.Length; ++index)
          {
            float y = ((float) this.offsetY - (float) (5000.0 + (double) index * 10000.0)) * SceneSingleton<DynamicMap>.i.mapDisplayFactor;
            this.listVertical[index].transform.localScale = 0.5f * current * Vector3.one;
            this.listVertical[index].transform.localPosition = new Vector3(num2, y, 0.0f);
          }
        }
        bool show = (double) current < 0.33000001311302185;
        if (show)
          this.UpdateMinorGridLabels(ref show, changed);
        if (this.minorParent.activeSelf == show)
          return;
        this.minorParent.SetActive(show);
      }
      else
      {
        if (!this.LabelShown)
          return;
        this.ShowLabels(false);
      }
    }
  }

  private void UpdateMinorGridLabels(ref bool show, bool changed)
  {
    using (GridLabels.updateMinorGridLabelsMarker.Auto())
    {
      Transform transform = SceneSingleton<DynamicMap>.i.mapImage.transform;
      float num1 = 1f / transform.localScale.x;
      Vector3 vector3_1 = (Input.mousePosition - transform.position) * (SceneSingleton<DynamicMap>.i.mapDimension / (900f * transform.lossyScale.x));
      Vector3 vector3_2 = new GlobalPosition(vector3_1.x, 0.0f, vector3_1.y).AsVector3();
      Vector3 vector3_3 = new Vector3((float) this.offsetX + vector3_2.x, 0.0f, (float) this.offsetY - vector3_2.z);
      int num2 = Mathf.FloorToInt(vector3_3.x / 10000f);
      int num3 = Mathf.FloorToInt(vector3_3.z / 10000f);
      if (!this.InBounds(num2, num3))
      {
        show = false;
      }
      else
      {
        changed |= GridLabels.HasChanged(ref this.previousMinorVertical, num3);
        changed |= GridLabels.HasChanged(ref this.previousMinorHorizontal, num2);
        if (!changed)
          return;
        float y1 = (420f - transform.localPosition.y) * num1;
        float x1 = (-420f - transform.localPosition.x) * num1;
        Vector3 vector3_4 = 0.5f * num1 * Vector3.one;
        for (int minor = 0; minor < 10; ++minor)
        {
          this.listHorizontalMinor[minor].text = this.GetHorizontalMinorLabel(num2, minor);
          this.listVerticalMinor[minor].text = this.GetVerticalMinorLabel(num3, minor);
          this.listHorizontalMinor[minor].transform.localScale = vector3_4;
          this.listVerticalMinor[minor].transform.localScale = vector3_4;
          float x2 = ((float) -this.offsetX + (float) (500.0 + (double) minor * 1000.0 + (double) num2 * 10000.0)) * SceneSingleton<DynamicMap>.i.mapDisplayFactor;
          float y2 = ((float) this.offsetY - (float) (500.0 + (double) minor * 1000.0 + (double) num3 * 10000.0)) * SceneSingleton<DynamicMap>.i.mapDisplayFactor;
          this.listHorizontalMinor[minor].transform.localPosition = new Vector3(x2, y1, 0.0f);
          this.listVerticalMinor[minor].transform.localPosition = new Vector3(x1, y2, 0.0f);
        }
      }
    }
  }

  private void ShowGridToolTip()
  {
    using (GridLabels.showGridToolTipMarker.Auto())
      this.gridToolTip.text = this.GetGridPosition(SceneSingleton<DynamicMap>.i.GetCursorCoordinates());
  }

  private void ShowGridAircraft()
  {
    using (GridLabels.showGridAircraftMarker.Auto())
    {
      string gridPosition = this.GetGridPosition(SceneSingleton<CombatHUD>.i.aircraft.GlobalPosition());
      this.gridAircraft.text = DynamicMap.mapMaximized ? "Current: " + gridPosition : gridPosition ?? "";
    }
  }

  public string GetGridPosition(GlobalPosition globalPosition)
  {
    Vector3 vector3 = new Vector3((float) this.offsetX + globalPosition.x, 0.0f, (float) this.offsetY - globalPosition.z);
    int num1 = Mathf.FloorToInt(vector3.x / 10000f);
    int minor1 = Mathf.FloorToInt((float) (((double) vector3.x - 10000.0 * (double) num1) / 1000.0));
    int num2 = Mathf.FloorToInt(vector3.z / 10000f);
    int minor2 = Mathf.FloorToInt((float) (((double) vector3.z - 10000.0 * (double) num2) / 1000.0));
    if (!this.InBounds(num1, num2))
      return "";
    string horizontalMinorLabel = this.GetHorizontalMinorLabel(num1, minor1);
    return this.GetVerticalMinorLabel(num2, minor2) + horizontalMinorLabel;
  }

  public void Maximize(bool maximized)
  {
    using (GridLabels.maximizeMarker.Auto())
    {
      this.majorParent.SetActive(maximized);
      this.minorParent.SetActive(maximized);
      this.gridToolTip.enabled = maximized;
      this.LabelShown = maximized;
      if (maximized)
      {
        this.gridToolTip.transform.SetParent(SceneSingleton<DynamicMap>.i.mapImage.transform.parent, false);
        this.gridToolTip.transform.localPosition = new Vector3(430f, -390f, 0.0f);
        this.gridToolTip.transform.localEulerAngles = Vector3.zero;
        this.gridAircraft.transform.SetParent(SceneSingleton<DynamicMap>.i.mapImage.transform.parent, false);
        this.gridAircraft.transform.localPosition = new Vector3(430f, -420f, 0.0f);
        this.gridAircraft.transform.localEulerAngles = Vector3.zero;
      }
      else
      {
        this.gridAircraft.transform.SetParent(SceneSingleton<DynamicMap>.i.hudMapAnchor.transform, false);
        this.gridAircraft.transform.localPosition = new Vector3(140f, -130f, 0.0f);
        this.gridAircraft.transform.localEulerAngles = Vector3.zero;
      }
    }
  }

  public void ShowLabels(bool show)
  {
    using (GridLabels.showLabelsMarker.Auto())
    {
      this.majorParent.SetActive(show);
      this.minorParent.SetActive(show);
      this.gridToolTip.enabled = show;
      this.gridAircraft.enabled = show;
      this.LabelEnabled = show;
    }
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool InBounds(int x, int y)
  {
    return x >= 0 && x < this.listHorizontal.Length && y >= 0 && y < this.listVertical.Length;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public string GetHorizontalMinorLabel(int major, int minor)
  {
    if (0 > major || major >= this.listHorizontal.Length)
      return "";
    int index = major * 10 + minor;
    return GridLabels.horizontalMinorStrings[index];
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public string GetVerticalMinorLabel(int major, int minor)
  {
    if (0 > major || major >= this.listVertical.Length)
      return "";
    int index = major * 10 + minor;
    return GridLabels.verticalMinorStrings[index];
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static bool HasChanged(ref int previous, int current)
  {
    int num = previous != current ? 1 : 0;
    previous = current;
    return num != 0;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static bool HasChanged(ref float previous, float current)
  {
    int num = (double) previous != (double) current ? 1 : 0;
    previous = current;
    return num != 0;
  }

  private static void MakeNumberMinorStrings(ref string[] array, int gridSize)
  {
    if (array != null && array.Length > gridSize * 10)
      return;
    array = new string[gridSize * 10];
    for (int index = 0; index < array.Length; ++index)
    {
      int num1 = index / 10;
      int num2 = index % 10;
      array[index] = $"{num1}{num2}";
    }
  }

  private static void MakeLetterMinorStrings(ref string[] array, int gridSize)
  {
    if (array != null && array.Length > gridSize * 10)
      return;
    array = new string[gridSize * 10];
    for (int index = 0; index < array.Length; ++index)
    {
      char ch1 = (char) (65 + index / 10);
      char ch2 = (char) (97 + index % 10);
      array[index] = $"{ch1}{ch2}";
    }
  }
}
