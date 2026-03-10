// Decompiled with JetBrains decompiler
// Type: ReserveReport
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption;
using NuclearOption.Networking;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#nullable disable
public class ReserveReport : SceneSingleton<ReserveReport>
{
  [SerializeField]
  public TMP_Text noticeText;
  [SerializeField]
  private Image aircraftImage;
  [SerializeField]
  private AudioClip reserveAwardedClip;
  [SerializeField]
  private AudioClip reserveAffordedClip;
  [SerializeField]
  private AudioClip reserveCancelledClip;
  private float hideTimer;

  public void Initialize(Player localPlayer)
  {
    localPlayer.onReserveNotice += new Action<ReserveNotice>(this.ReserveReport_OnReserveNotice);
  }

  public void SetPosition(Vector3 screenPosition) => this.transform.position = screenPosition;

  private void ReserveReport_OnReserveNotice(ReserveNotice reserveNotice)
  {
    if (PlayerSettings.cinematicMode)
      return;
    if (reserveNotice.outcome == ReserveEvent.cancelledAfford)
    {
      this.ShowDisplay(reserveNotice);
      this.FlashAffordAircraft();
    }
    else if (reserveNotice.outcome == ReserveEvent.granted)
    {
      this.ShowDisplay(reserveNotice);
      this.FlashGrantedAircraft();
    }
    else
    {
      if (reserveNotice.outcome != ReserveEvent.cancelledRank)
        return;
      this.ShowDisplay(reserveNotice);
      this.FlashReserveCancelled();
    }
  }

  public void ShowDisplay(ReserveNotice reserveNotice)
  {
    this.gameObject.SetActive(true);
    this.enabled = true;
    this.hideTimer = 5f;
    this.aircraftImage.sprite = reserveNotice.aircraftDefinition.mapIcon;
  }

  public void FlashGrantedAircraft()
  {
    this.aircraftImage.color = Color.green;
    this.noticeText.color = Color.green;
    SoundManager.PlayInterfaceOneShot(this.reserveAwardedClip);
    this.noticeText.text = "+1";
  }

  public void FlashAffordAircraft()
  {
    this.aircraftImage.color = Color.white;
    this.noticeText.color = Color.white;
    SoundManager.PlayInterfaceOneShot(this.reserveAffordedClip);
    this.noticeText.text = "+$";
  }

  public void FlashReserveCancelled()
  {
    this.aircraftImage.color = Color.red;
    this.noticeText.color = Color.red;
    SoundManager.PlayInterfaceOneShot(this.reserveCancelledClip);
    this.noticeText.text = "x";
  }

  private void Update()
  {
    this.hideTimer -= Time.deltaTime;
    if ((double) this.hideTimer >= 0.0)
      return;
    this.enabled = false;
    this.gameObject.SetActive(false);
  }
}
