// Decompiled with JetBrains decompiler
// Type: NuclearOption.Networking.Lobbies.JoinLobbyOverlay
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

#nullable disable
namespace NuclearOption.Networking.Lobbies;

public class JoinLobbyOverlay : MonoBehaviour
{
  private static JoinLobbyOverlay Instance;
  [SerializeField]
  private GameObject overlay;
  [Space]
  [SerializeField]
  private TextMeshProUGUI titleText;
  [SerializeField]
  private TextMeshProUGUI bodyText;
  [Space]
  [SerializeField]
  private TextMeshProUGUI closeText;
  [SerializeField]
  private MaskableGraphic closeImage;
  [SerializeField]
  private MaskableGraphic closeBorderImage;
  [SerializeField]
  private Button closeButton;
  [Header("Title Color")]
  [SerializeField]
  private Color joiningTitleColor;
  [SerializeField]
  private Color failTitleColor;
  [Header("Close Color")]
  [SerializeField]
  private Color joiningCloseImageColor;
  [SerializeField]
  private Color joiningCloseBorderImageColor;
  [SerializeField]
  private Color joiningCloseTextColor;
  [Space]
  [SerializeField]
  private Color failCloseImageColor;
  [SerializeField]
  private Color failCloseBorderImageColor;
  [SerializeField]
  private Color failCloseTextColor;

  private void Awake()
  {
    this.overlay.SetActive(false);
    this.closeButton.onClick.AddListener(new UnityAction(JoinLobbyOverlay.Close));
  }

  private void OnDestroy() => JoinLobbyOverlay.Instance = (JoinLobbyOverlay) null;

  public static void Open(JoinProgress joinProgress)
  {
    if ((Object) JoinLobbyOverlay.Instance == (Object) null)
    {
      GameObject target = Object.Instantiate<GameObject>((GameObject) UnityEngine.Resources.Load("JoinLobbyOverlayCanvas"));
      JoinLobbyOverlay.Instance = target.GetComponent<JoinLobbyOverlay>();
      Object.DontDestroyOnLoad((Object) target);
    }
    JoinLobbyOverlay.Instance.SetValues(joinProgress);
  }

  public static void Close()
  {
    if ((Object) JoinLobbyOverlay.Instance == (Object) null)
      return;
    JoinLobbyOverlay.Instance.overlay.SetActive(false);
  }

  private void SetValues(JoinProgress joinProgress)
  {
    if (joinProgress.Join)
    {
      this.titleText.text = "Joining Server...";
      this.closeText.text = "Cancel";
      this.titleText.color = this.joiningTitleColor;
      this.closeImage.color = this.joiningCloseImageColor;
      this.closeBorderImage.color = this.joiningCloseBorderImageColor;
      this.closeText.color = this.joiningCloseTextColor;
    }
    else
    {
      this.titleText.text = "Connect Failed";
      this.closeText.text = "Close";
      this.titleText.color = this.failTitleColor;
      this.closeImage.color = this.failCloseImageColor;
      this.closeBorderImage.color = this.failCloseBorderImageColor;
      this.closeText.color = this.failCloseTextColor;
    }
    this.overlay.SetActive(true);
    this.bodyText.text = joinProgress.Body;
  }

  private void CloseButton()
  {
    this.overlay.SetActive(false);
    if (!NetworkManagerNuclearOption.i.Client.Active)
      return;
    NetworkManagerNuclearOption.i.Stop(true);
  }
}
