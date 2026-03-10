// Decompiled with JetBrains decompiler
// Type: NuclearOption.MissionEditorScripts.Buttons.UnitPreviewGenerator
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System.Collections.Generic;
using UnityEngine;

#nullable disable
namespace NuclearOption.MissionEditorScripts.Buttons;

internal class UnitPreviewGenerator : MonoBehaviour
{
  [SerializeField]
  private Camera camera;
  [SerializeField]
  private RenderTexture renderTexture;
  [SerializeField]
  private Transform holder;
  [SerializeField]
  public float sizeMultiple = 1f;
  [SerializeField]
  public bool ignoreCache;
  [SerializeField]
  private bool preview;
  [SerializeField]
  private UnitDefinition target;
  private readonly Dictionary<UnitDefinition, Texture2D> previews = new Dictionary<UnitDefinition, Texture2D>();

  private void Awake() => this.camera.targetTexture = this.renderTexture;

  private void Update()
  {
    if (!this.preview)
      return;
    this.preview = false;
    this.Render(this.target);
  }

  private void OnDestroy()
  {
    foreach (Object @object in this.previews.Values)
      Object.Destroy(@object);
    this.previews.Clear();
  }

  public Texture2D GetSprite(UnitDefinition unit)
  {
    if (this.ignoreCache)
      return this.GenerateSprite(unit);
    Texture2D sprite;
    if (!this.previews.TryGetValue(unit, out sprite))
    {
      sprite = this.GenerateSprite(unit);
      this.previews.Add(unit, sprite);
    }
    return sprite;
  }

  private Texture2D GenerateSprite(UnitDefinition unit)
  {
    RenderTexture active = RenderTexture.active;
    RenderTexture.active = this.renderTexture;
    try
    {
      this.Render(unit);
      Texture2D sprite = new Texture2D(this.renderTexture.width, this.renderTexture.height);
      sprite.ReadPixels(new Rect(0.0f, 0.0f, (float) this.renderTexture.width, (float) this.renderTexture.height), 0, 0);
      sprite.Apply();
      return sprite;
    }
    finally
    {
      RenderTexture.active = active;
    }
  }

  private void Render(UnitDefinition unit)
  {
    using (BenchmarkScope.Create("Preview Render"))
    {
      this.camera.gameObject.SetActive(true);
      this.holder.gameObject.SetActive(true);
      GameObject target = Object.Instantiate<GameObject>(unit.unitPrefab, this.holder);
      UnitPreviewGenerator.SetLayerRecursively(target, this.holder.gameObject.layer);
      this.camera.orthographicSize = this.GetDistance(unit) * this.sizeMultiple;
      this.camera.Render();
      Object.Destroy((Object) target);
      this.camera.gameObject.SetActive(false);
      this.holder.gameObject.SetActive(false);
    }
  }

  private static void SetLayerRecursively(GameObject target, int layer)
  {
    foreach (Component componentsInChild in target.GetComponentsInChildren<Transform>())
      componentsInChild.gameObject.layer = layer;
  }

  private float GetDistance(UnitDefinition definition)
  {
    float num1 = (float) (((double) Mathf.Max(definition.length, definition.width * 0.7f) + (double) Mathf.Max(definition.width, definition.length * 0.7f)) * 0.5);
    if ((double) definition.height > (double) definition.length && (double) definition.height > (double) definition.width)
      num1 = definition.height * 1.2f;
    float num2 = Mathf.Clamp(Mathf.Pow(num1 * 0.1f, 0.2f), 0.6f, 1.5f);
    return num1 / num2;
  }
}
