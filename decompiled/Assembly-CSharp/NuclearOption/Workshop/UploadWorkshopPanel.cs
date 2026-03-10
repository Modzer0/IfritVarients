// Decompiled with JetBrains decompiler
// Type: NuclearOption.Workshop.UploadWorkshopPanel
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Cysharp.Threading.Tasks;
using NuclearOption.MissionEditorScripts;
using NuclearOption.ModScripts;
using SFB;
using Steamworks;
using System;
using System.IO;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

#nullable disable
namespace NuclearOption.Workshop;

public class UploadWorkshopPanel : WorkshopMenu.WorkshopPanel
{
  private static readonly ExtensionFilter[] extensions = new ExtensionFilter[2]
  {
    new ExtensionFilter("Image Files", new string[3]
    {
      "png",
      "jpg",
      "jpeg"
    }),
    new ExtensionFilter("All Files", new string[1]{ "*" })
  };
  [Header("References")]
  [SerializeField]
  private SteamWorkshop steamWorkshop;
  [Header("Controls")]
  [SerializeField]
  private Button backButton;
  [SerializeField]
  private Button nextButton;
  [SerializeField]
  private TextMeshProUGUI nextButtonText;
  [SerializeField]
  private Button openWorkshop;
  [Header("Pages")]
  [SerializeField]
  private int activePageIndex;
  private UploadWorkshopPanel.Page activePage;
  [SerializeField]
  private UploadWorkshopPanel.Page1 page1;
  [SerializeField]
  private UploadWorkshopPanel.Page2 page2;
  [SerializeField]
  private UploadWorkshopPanel.Page3 page3;
  [SerializeField]
  private UploadWorkshopPanel.Page4 page4;
  private PublishedFileId_t workshopId;

  private void OpenPage1()
  {
    this.EnablePage(1);
    this.page1.Enable();
    this.Update();
  }

  private void OpenPage2(ModType type)
  {
    this.EnablePage(2);
    this.page2.Enable(type);
    this.Update();
  }

  private void OpenPage3(ModType type, string itemName)
  {
    this.EnablePage(3);
    this.page3.Enable(type, itemName);
    this.Update();
  }

  private void OpenPage3(WorkshopUploadItem item, SteamWorkshopItem details)
  {
    this.EnablePage(3);
    this.page3.Enable(item, details);
    this.Update();
  }

  private void OpenPage4(WorkshopUploadItem item, string changeLog)
  {
    this.EnablePage(4);
    this.page4.Enable(item, changeLog);
    this.Update();
  }

  public void OpenWithItem(WorkshopUploadItem item, SteamWorkshopItem details)
  {
    this.OpenPage3(item, details);
  }

  private void Awake()
  {
    this.page1.Setup(this);
    this.page2.Setup(this);
    this.page3.Setup(this);
    this.page4.Setup(this);
    this.nextButton.onClick.AddListener(new UnityAction(this.GoNext));
    this.backButton.onClick.AddListener(new UnityAction(this.GoBack));
    this.openWorkshop.onClick.AddListener(new UnityAction(this.OpenItemWorkshop));
  }

  private void OnEnable() => this.OpenPage1();

  private void OnValidate() => this.EnablePage(this.activePageIndex);

  private void EnablePage(int index)
  {
    this.page1.SetActive(index == 1);
    this.page2.SetActive(index == 2);
    this.page3.SetActive(index == 3);
    this.page4.SetActive(index == 4);
    this.activePageIndex = index;
    switch (index)
    {
      case 1:
        this.activePage = (UploadWorkshopPanel.Page) this.page1;
        break;
      case 2:
        this.activePage = (UploadWorkshopPanel.Page) this.page2;
        break;
      case 3:
        this.activePage = (UploadWorkshopPanel.Page) this.page3;
        break;
      case 4:
        this.activePage = (UploadWorkshopPanel.Page) this.page4;
        break;
    }
    FixLayout.ForceRebuildAtEndOfFrame((RectTransform) this.transform);
  }

  private void Update() => this.activePage.Update();

  private void GoNext() => this.activePage.GoNext();

  private void GoBack() => this.EnablePage(this.activePageIndex - 1);

  private void OpenItemWorkshop()
  {
    if (!(this.workshopId != PublishedFileId_t.Invalid))
      return;
    SteamWorkshopItem.OpenSteamPage(this.workshopId);
  }

  public abstract class Page
  {
    [SerializeField]
    private GameObject holder;
    protected UploadWorkshopPanel panel;

    public void SetActive(bool active)
    {
      if (active && !this.holder.activeSelf)
      {
        this.holder.SetActive(true);
      }
      else
      {
        if (active || !this.holder.activeSelf)
          return;
        this.holder.SetActive(false);
      }
    }

    public void Setup(UploadWorkshopPanel panel)
    {
      this.panel = panel;
      this.SetActive(false);
    }

    public abstract void Update();

    public abstract void GoNext();
  }

  [Serializable]
  public class Page1 : UploadWorkshopPanel.Page
  {
    [SerializeField]
    private TMP_Dropdown modTypeDropdown;

    public void Enable()
    {
      this.panel.nextButton.interactable = false;
      this.PopulateTypeDropdown();
      this.panel.nextButtonText.text = "Next";
      this.panel.openWorkshop.gameObject.SetActive(false);
      this.panel.workshopId = PublishedFileId_t.Invalid;
    }

    private void PopulateTypeDropdown()
    {
      this.modTypeDropdown.ClearOptions();
      this.modTypeDropdown.options.Add(new TMP_Dropdown.OptionData()
      {
        text = ""
      });
      foreach (ModType modTypes in ModTypes.ModTypesArray)
        this.modTypeDropdown.options.Add(new TMP_Dropdown.OptionData(modTypes.Name));
      this.modTypeDropdown.SetValueWithoutNotify(0);
    }

    public override void Update()
    {
      this.panel.nextButton.interactable = this.modTypeDropdown.value != 0;
    }

    public override void GoNext()
    {
      int index = this.modTypeDropdown.value - 1;
      this.panel.OpenPage2(ModTypes.ModTypesArray[index]);
    }
  }

  [Serializable]
  public class Page2 : UploadWorkshopPanel.Page
  {
    [SerializeField]
    private TextMeshProUGUI typeLabel;
    [SerializeField]
    private TMP_Dropdown itemSelectDropdown;
    private ModType type;

    public void Enable(ModType type)
    {
      this.type = type;
      this.panel.nextButton.interactable = true;
      this.typeLabel.text = "Select " + type.Name;
      type.PopulateItemDropdown(this.itemSelectDropdown);
      this.panel.nextButtonText.text = "Next";
      this.panel.openWorkshop.gameObject.SetActive(false);
    }

    public override void Update()
    {
      this.panel.nextButton.interactable = this.itemSelectDropdown.value != 0;
    }

    public override void GoNext()
    {
      this.panel.OpenPage3(this.type, this.itemSelectDropdown.options[this.itemSelectDropdown.value].text);
    }
  }

  [Serializable]
  public class Page3 : UploadWorkshopPanel.Page
  {
    [SerializeField]
    private GameObject loadingOverlay;
    [Header("Main panel")]
    [SerializeField]
    private TMP_InputField missionNameInput;
    [SerializeField]
    private Toggle isPublicToggle;
    [SerializeField]
    private TMP_InputField descriptionInput;
    [SerializeField]
    private Button openFileSelectorButton;
    [SerializeField]
    private Image previewImage;
    [SerializeField]
    private TextMeshProUGUI needPreviewWarning;
    [SerializeField]
    private TMP_InputField changeLogInput;
    [Header("Details not found")]
    [SerializeField]
    private GameObject notFoundPanel;
    [SerializeField]
    private Button notFoundRetry;
    [SerializeField]
    private Button notFoundCreateNew;
    private bool needToDisposeImage;
    private bool eventAdded;
    private string imagePath;
    private CancellationTokenSource loadImageCancel;
    private WorkshopUploadItem item;

    public void Enable(WorkshopUploadItem item, SteamWorkshopItem details)
    {
      this.item = item;
      this.SetupAsync(details).Forget();
    }

    public void Enable(ModType type, string itemName)
    {
      if (!this.eventAdded)
      {
        this.eventAdded = true;
        this.openFileSelectorButton.onClick.AddListener(new UnityAction(this.OpenFileSelector));
        this.notFoundRetry.onClick.AddListener(new UnityAction(this.NotFoundRetry));
        this.notFoundCreateNew.onClick.AddListener(new UnityAction(this.NotFoundCreateNew));
      }
      this.loadingOverlay.SetActive(false);
      this.notFoundPanel.SetActive(false);
      this.item = type.GetItem(itemName);
      if (this.item == null)
        this.panel.GoBack();
      else
        UniTask.Void((Func<UniTaskVoid>) (async () =>
        {
          if (this.item.WorkshopId != PublishedFileId_t.Invalid)
            await this.TryLoadDetails();
          else
            await this.SetupAsync((SteamWorkshopItem) null);
        }));
    }

    private void NotFoundRetry()
    {
      this.notFoundPanel.SetActive(false);
      this.TryLoadDetails().Forget();
    }

    private void NotFoundCreateNew()
    {
      this.notFoundPanel.SetActive(false);
      this.item.ClearSteamId();
      this.SetupAsync((SteamWorkshopItem) null).Forget();
    }

    private async UniTask TryLoadDetails()
    {
      this.loadingOverlay.SetActive(true);
      (bool flag, SteamWorkshopItem details) = await SteamWorkshop.GetDetails(this.item.WorkshopId);
      if (flag)
        await this.SetupAsync(details);
      else
        this.notFoundPanel.SetActive(true);
    }

    private async UniTask SetupAsync(SteamWorkshopItem details)
    {
      UploadWorkshopPanel.Page3 page3 = this;
      page3.loadingOverlay.SetActive(false);
      page3.notFoundPanel.SetActive(false);
      FixLayout.ForceRebuildAtEndOfFrame((RectTransform) page3.panel.transform);
      page3.panel.openWorkshop.gameObject.SetActive(page3.item.WorkshopId != PublishedFileId_t.Invalid);
      page3.panel.nextButton.interactable = true;
      page3.panel.nextButtonText.text = page3.item.WorkshopId != PublishedFileId_t.Invalid ? "Update Existing" : "Create New";
      page3.missionNameInput.text = page3.item.DisplayName;
      page3.missionNameInput.interactable = false;
      if (details != null)
      {
        page3.isPublicToggle.isOn = details.Public;
        page3.descriptionInput.text = details.Description;
      }
      else
      {
        page3.isPublicToggle.isOn = false;
        page3.descriptionInput.text = page3.item.Description;
      }
      page3.imagePath = (string) null;
      page3.changeLogInput.text = string.Empty;
      page3.changeLogInput.interactable = page3.item.WorkshopId != PublishedFileId_t.Invalid;
      page3.needPreviewWarning.text = page3.item.WorkshopId != PublishedFileId_t.Invalid ? "Loading..." : "Need Preview";
      if (details != null)
      {
        UniTask uniTask = details.SetPreviewImageAsync(page3.previewImage, ref page3.loadImageCancel);
        CancellationToken cancel = page3.loadImageCancel.Token;
        await uniTask;
        if (cancel.IsCancellationRequested)
          return;
        FixLayout.ForceRebuildRecursive((RectTransform) page3.panel.transform);
        page3.SetPreviewColor();
        page3.needPreviewWarning.text = (UnityEngine.Object) page3.previewImage.sprite != (UnityEngine.Object) null ? "" : "Failed to load";
        cancel = new CancellationToken();
      }
      else
        page3.SetPreviewTexture((Texture2D) null);
    }

    public void OpenFileSelector()
    {
      string[] strArray = StandaloneFileBrowser.OpenFilePanel("Select Image", "", UploadWorkshopPanel.extensions, false);
      if (strArray.Length == 0)
        return;
      bool flag1 = this.ValidSize(strArray[0]);
      this.loadImageCancel?.Cancel();
      this.loadImageCancel = (CancellationTokenSource) null;
      Texture2D texture;
      bool flag2 = UploadWorkshopPanel.Page3.TryLoadImage(strArray[0], out texture);
      this.imagePath = !(flag2 & flag1) ? (string) null : strArray[0];
      if (!flag2)
        this.needPreviewWarning.text = "Load failed";
      else if (!flag1)
        this.needPreviewWarning.text = "Preview must be under 1MB";
      else
        this.needPreviewWarning.text = "";
      this.SetPreviewTexture(flag2 ? texture : (Texture2D) null);
    }

    private bool ValidSize(string filePath)
    {
      return (double) new FileInfo(filePath).Length / 1048576.0 <= 1.0;
    }

    private void SetPreviewTexture(Texture2D newTexture)
    {
      if (this.needToDisposeImage)
      {
        Sprite sprite = this.previewImage.sprite;
        this.previewImage.sprite = (Sprite) null;
        UnityEngine.Object.Destroy((UnityEngine.Object) sprite.texture);
        UnityEngine.Object.Destroy((UnityEngine.Object) sprite);
        this.needToDisposeImage = false;
      }
      if ((UnityEngine.Object) newTexture != (UnityEngine.Object) null)
      {
        this.previewImage.sprite = Sprite.Create(newTexture, new Rect(0.0f, 0.0f, (float) newTexture.width, (float) newTexture.height), new Vector2(0.5f, 0.5f));
        this.needToDisposeImage = true;
      }
      else
        this.previewImage.sprite = (Sprite) null;
      this.SetPreviewColor();
    }

    private void SetPreviewColor()
    {
      this.previewImage.color = (UnityEngine.Object) this.previewImage.sprite != (UnityEngine.Object) null ? Color.white : new Color(0.2f, 0.2f, 0.2f);
    }

    private static bool TryLoadImage(string path, out Texture2D texture)
    {
      texture = (Texture2D) null;
      if (string.IsNullOrEmpty(path) || !File.Exists(path))
        return false;
      texture = new Texture2D(0, 0);
      byte[] data = File.ReadAllBytes(path);
      return texture.LoadImage(data);
    }

    public override void Update() => this.panel.nextButton.interactable = this.CanUpload();

    private bool CanUpload()
    {
      return !string.IsNullOrEmpty(this.missionNameInput.text) && !string.IsNullOrEmpty(this.descriptionInput.text) && (!(this.item.WorkshopId == PublishedFileId_t.Invalid) || !string.IsNullOrEmpty(this.imagePath));
    }

    public override void GoNext()
    {
      this.item.Description = this.descriptionInput.text;
      this.item.ImagePath = this.imagePath;
      this.item.Public = this.isPublicToggle.isOn;
      this.panel.OpenPage4(this.item, this.changeLogInput.text);
    }
  }

  [Serializable]
  public class Page4 : UploadWorkshopPanel.Page
  {
    [SerializeField]
    private TextMeshProUGUI uploadingLabel;
    [SerializeField]
    private TextMeshProUGUI uploadingProgressOverall;
    [SerializeField]
    private TextMeshProUGUI uploadingProgressPart;

    public void Enable(WorkshopUploadItem item, string changeLog)
    {
      this.panel.backButton.interactable = false;
      this.panel.nextButton.interactable = false;
      this.panel.openWorkshop.gameObject.SetActive(item.WorkshopId != PublishedFileId_t.Invalid);
      this.UploadAsync(item, changeLog).Forget();
      this.uploadingLabel.text = "Uploading";
    }

    public override void Update()
    {
    }

    private async UniTask UploadAsync(WorkshopUploadItem item, string changeLog)
    {
      UploadWorkshopPanel.Page4 page4 = this;
      UploadWorkshopPanel.Page4.UploadProgress uploadProgress = new UploadWorkshopPanel.Page4.UploadProgress(page4.uploadingProgressOverall, page4.uploadingProgressPart);
      SteamWorkshopItem steamItem = item.ToSteamItem();
      UniTask<(bool, EResult)> task = page4.panel.steamWorkshop.CreateOrUpdateItem(steamItem, changeLog, (IWorkshopCreateCallbacks) item, (IProgress<(float, float)>) uploadProgress);
      while (task.Status == UniTaskStatus.Pending)
        await UniTask.Yield();
      (bool flag, EResult eResult) = await task;
      page4.uploadingLabel.text = flag ? "Success" : "Failed: " + eResult.Nicify();
      page4.panel.openWorkshop.gameObject.SetActive(item.WorkshopId != PublishedFileId_t.Invalid);
      FixLayout.ForceRebuildRecursive((RectTransform) page4.panel.transform);
      task = new UniTask<(bool, EResult)>();
    }

    public override void GoNext() => throw new NotSupportedException();

    internal class UploadProgress : IProgress<(float overall, float part)>
    {
      private readonly TextMeshProUGUI overall;
      private readonly TextMeshProUGUI part;

      public UploadProgress(TextMeshProUGUI overall, TextMeshProUGUI part)
      {
        this.overall = overall;
        this.part = part;
      }

      public void Report((float overall, float part) value)
      {
        this.overall.text = MissionManagerDebugGui.CreateProgressBar(value.overall, 40);
        this.part.text = MissionManagerDebugGui.CreateProgressBar(value.part, 20);
      }
    }
  }
}
