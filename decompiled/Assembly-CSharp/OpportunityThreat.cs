// Decompiled with JetBrains decompiler
// Type: OpportunityThreat
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

#nullable disable
public struct OpportunityThreat(float opportunity, float threat)
{
  public readonly float opportunity = opportunity;
  public readonly float threat = threat;

  public float GetCombinedScore() => this.opportunity * (this.threat + 1f);
}
