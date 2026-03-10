// Decompiled with JetBrains decompiler
// Type: AircraftActionsReport
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

#nullable disable
public class AircraftActionsReport : SceneSingleton<AircraftActionsReport>
{
  [SerializeField]
  private Image background;
  [SerializeField]
  private Text actionsText;
  private Aircraft aircraft;
  private int messageLines;

  public void ReportText(string report, float displayTime)
  {
    if ((UnityEngine.Object) this.actionsText == (UnityEngine.Object) null)
      return;
    if (this.actionsText.text.Length > 0)
      this.actionsText.text += "\n";
    this.actionsText.text += report;
    this.background.enabled = true;
    this.TrimMessages(displayTime).Forget();
  }

  public void Initialize(Aircraft aircraft)
  {
    if ((UnityEngine.Object) SceneSingleton<AircraftActionsReport>.i == (UnityEngine.Object) null)
      this.SetupSingleton();
    this.aircraft = aircraft;
    aircraft.onDisableUnit += new Action<Unit>(this.AircraftActionsReport_OnDisable);
    this.ClearMessages();
  }

  private void AircraftActionsReport_OnDisable(Unit unit)
  {
    this.aircraft.onDisableUnit -= new Action<Unit>(this.AircraftActionsReport_OnDisable);
    if (!((UnityEngine.Object) this != (UnityEngine.Object) null))
      return;
    UnityEngine.Object.Destroy((UnityEngine.Object) this.gameObject);
  }

  private void ClearMessages()
  {
    this.actionsText.text = "";
    this.messageLines = 0;
    this.background.enabled = false;
  }

  private async UniTask TrimMessages(float messageTime)
  {
    AircraftActionsReport aircraftActionsReport = this;
    aircraftActionsReport.background.enabled = true;
    ++aircraftActionsReport.messageLines;
    CancellationToken cancel = aircraftActionsReport.destroyCancellationToken;
    await UniTask.Delay((int) ((double) messageTime * 1000.0));
    if (cancel.IsCancellationRequested)
    {
      cancel = new CancellationToken();
    }
    else
    {
      if (aircraftActionsReport.messageLines > 1)
      {
        int num = aircraftActionsReport.actionsText.text.IndexOf("\n");
        string str = aircraftActionsReport.actionsText.text.Substring(num + 1);
        aircraftActionsReport.actionsText.text = str;
      }
      else
        aircraftActionsReport.actionsText.text = string.Empty;
      --aircraftActionsReport.messageLines;
      if (aircraftActionsReport.messageLines > 0)
      {
        cancel = new CancellationToken();
      }
      else
      {
        aircraftActionsReport.background.enabled = false;
        cancel = new CancellationToken();
      }
    }
  }
}
