// Decompiled with JetBrains decompiler
// Type: NuclearOption.Workshop.WorkshopListItem
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Cysharp.Threading.Tasks;
using JamesFrowen.ScriptableVariables.UI;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

#nullable disable
namespace NuclearOption.Workshop;

public class WorkshopListItem : 
  ListItem<SteamWorkshopItem>,
  IPointerClickHandler,
  IEventSystemHandler,
  IPointerEnterHandler,
  IPointerExitHandler
{
  [Header("UI")]
  [SerializeField]
  private TextMeshProUGUI nameText;
  [SerializeField]
  private GameObject installBadge;
  [SerializeField]
  private GameObject ownerBadge;
  [SerializeField]
  private Image previewImage;
  [SerializeField]
  private float startScale;
  [SerializeField]
  private float hoverScale;
  [SerializeField]
  private float hoverSeconds;
  private WorkshopMenu workshopMenu;
  private CancellationTokenSource cancellationTokenSource;
  private float scale;
  private bool hover;

  protected override void Awake() => this.workshopMenu = this.GetComponentInParent<WorkshopMenu>();

  private void Update()
  {
    float target = this.hover ? this.hoverScale : 1f;
    if ((double) this.scale == (double) target)
      return;
    this.scale = Mathf.MoveTowards(this.scale, target, Time.deltaTime / this.hoverSeconds);
    this.transform.localScale = Vector3.one * this.scale;
  }

  private void OnDisable() => this.nameText.text = "";

  protected override void SetValue(SteamWorkshopItem value)
  {
    if (this.nameText.text != value.Name)
      this.scale = this.startScale;
    this.nameText.text = value.Name;
    this.installBadge.SetActive(value.Subscribed);
    this.ownerBadge.SetActive(value.IsOwner());
    value.SetPreviewImageAsync(this.previewImage, ref this.cancellationTokenSource).Forget();
  }

  public void OpenDetails() => this.workshopMenu.OpenItemDetailsPanel(this.Value);

  void IPointerClickHandler.OnPointerClick(PointerEventData eventData) => this.OpenDetails();

  void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData) => this.hover = true;

  void IPointerExitHandler.OnPointerExit(PointerEventData eventData) => this.hover = false;
}
