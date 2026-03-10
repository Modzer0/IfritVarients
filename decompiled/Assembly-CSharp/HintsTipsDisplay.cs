// Decompiled with JetBrains decompiler
// Type: HintsTipsDisplay
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#nullable disable
public class HintsTipsDisplay : MonoBehaviour
{
  public Button leftButton;
  public Button rightButton;
  public Text hintType;
  public Text hintText;
  public TextAsset hintsCSV;
  private List<HintsTipsDisplay.HintTip> listHints = new List<HintsTipsDisplay.HintTip>();
  private int index;
  private float lastChange;
  [SerializeField]
  private float refreshTime = 5f;

  private void Awake()
  {
    this.ReadHints();
    this.Shuffle();
  }

  private void Start() => this.PickRandom();

  private void Update()
  {
    if ((double) Time.timeSinceLevelLoad <= (double) this.lastChange + (double) this.refreshTime)
      return;
    this.OnButtonClick(true);
  }

  public void PickRandom()
  {
    if (this.listHints.Count <= 0)
      return;
    this.index = UnityEngine.Random.Range(0, this.listHints.Count);
    this.DisplayHint(this.index);
  }

  public void Shuffle()
  {
    for (int index1 = 0; index1 < this.listHints.Count; ++index1)
    {
      int index2 = UnityEngine.Random.Range(index1, this.listHints.Count);
      HintsTipsDisplay.HintTip listHint = this.listHints[index1];
      this.listHints[index1] = this.listHints[index2];
      this.listHints[index2] = listHint;
    }
  }

  public void OnButtonClick(bool next)
  {
    this.index += next ? 1 : -1;
    if (this.index < 0)
      this.index = this.listHints.Count - 1;
    else if (this.index >= this.listHints.Count)
      this.index = 0;
    this.DisplayHint(this.index);
  }

  public void DisplayHint(int index)
  {
    this.hintType.text = this.listHints[index].type;
    this.hintText.text = this.listHints[index].text;
    this.lastChange = Time.timeSinceLevelLoad;
  }

  public void ReadHints()
  {
    string text = this.hintsCSV.text;
    string[] separator = new string[3]{ "\r\n", "\r", "\n" };
    foreach (string str in text.Split(separator, StringSplitOptions.RemoveEmptyEntries))
    {
      string[] strArray = str.Split(';', StringSplitOptions.None);
      ++this.index;
      this.listHints.Add(new HintsTipsDisplay.HintTip()
      {
        id = this.index,
        type = strArray[0],
        text = strArray[1]
      });
    }
  }

  private struct HintTip
  {
    public int id;
    public string type;
    public string text;
  }
}
