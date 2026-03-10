// Decompiled with JetBrains decompiler
// Type: NuclearOption.SavedMission.ObjectiveV2.Outcomes.ModifyEnvironmentOutcome
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.MissionEditorScripts;
using System;

#nullable disable
namespace NuclearOption.SavedMission.ObjectiveV2.Outcomes;

internal class ModifyEnvironmentOutcome : Outcome
{
  private static readonly MissionEnvironment defaultSettings = new MissionEnvironment();
  private ValueWrapperOverride<float> timeOfDay = new ValueWrapperOverride<float>();
  private ValueWrapperOverride<float> weather = new ValueWrapperOverride<float>();
  private ValueWrapperOverride<float> cloudAltitude = new ValueWrapperOverride<float>();
  private ValueWrapperOverride<float> windSpeed = new ValueWrapperOverride<float>();
  private ValueWrapperOverride<float> windTurbulence = new ValueWrapperOverride<float>();
  private ValueWrapperOverride<float> windHeading = new ValueWrapperOverride<float>();

  protected override void WriteOutcome(ReadWriteObjective writer)
  {
    writer.Override<float>(ref this.timeOfDay.GetValueRef(), new ReadWriteObjective.ReadWriteValue<float>(writer.Float));
    writer.Override<float>(ref this.weather.GetValueRef(), new ReadWriteObjective.ReadWriteValue<float>(writer.Float));
    writer.Override<float>(ref this.cloudAltitude.GetValueRef(), new ReadWriteObjective.ReadWriteValue<float>(writer.Float));
    writer.Override<float>(ref this.windSpeed.GetValueRef(), new ReadWriteObjective.ReadWriteValue<float>(writer.Float));
    writer.Override<float>(ref this.windTurbulence.GetValueRef(), new ReadWriteObjective.ReadWriteValue<float>(writer.Float));
    writer.Override<float>(ref this.windHeading.GetValueRef(), new ReadWriteObjective.ReadWriteValue<float>(writer.Float));
    if (writer.mode != ReadWriteObjective.Mode.Read)
      return;
    SetIfNoOverride<float>(this.timeOfDay, ModifyEnvironmentOutcome.defaultSettings.timeOfDay);
    SetIfNoOverride<float>(this.weather, ModifyEnvironmentOutcome.defaultSettings.weatherIntensity);
    SetIfNoOverride<float>(this.cloudAltitude, ModifyEnvironmentOutcome.defaultSettings.cloudAltitude);
    SetIfNoOverride<float>(this.windSpeed, ModifyEnvironmentOutcome.defaultSettings.windSpeed);
    SetIfNoOverride<float>(this.windTurbulence, ModifyEnvironmentOutcome.defaultSettings.windTurbulence);
    SetIfNoOverride<float>(this.windHeading, ModifyEnvironmentOutcome.defaultSettings.windHeading);

    static void SetIfNoOverride<T>(ValueWrapperOverride<T> wrapper, T value) where T : IEquatable<T>
    {
      if (wrapper.Value.IsOverride)
        return;
      wrapper.SetValue(new Override<T>(false, value), (object) null, true);
    }
  }

  public override void ReferenceDestroyed(ISaveableReference reference)
  {
  }

  public override void Complete(Objective completedObjective)
  {
    LevelInfo level = NetworkSceneSingleton<LevelInfo>.i;
    this.timeOfDay.IfOverride<float>((Action<float>) (value => level.SetTimeOfDay(value)));
    this.weather.IfOverride<float>((Action<float>) (value => level.Networkconditions = value));
    this.cloudAltitude.IfOverride<float>((Action<float>) (value => level.NetworkcloudHeight = value));
    this.windSpeed.IfOverride<float>((Action<float>) (value => level.NetworkwindSpeed = value));
    this.windTurbulence.IfOverride<float>((Action<float>) (value => level.NetworkwindTurbulence = value));
    this.windHeading.IfOverride<float>((Action<float>) (value => level.SetWindHeading(value)));
  }

  public override void DrawData(DataDrawer drawer)
  {
    FloatDataField floatDataField1 = drawer.DrawOverride<float, FloatDataField>("Time of Day", this.timeOfDay, drawer.Prefabs.FloatFieldPrefab).Item2;
    floatDataField1.SetSliderSettings(new FloatDataField.FloatSlider(0.0f, 24f));
    floatDataField1.SetSteps(new float?(0.1f));
    FloatDataField floatDataField2 = drawer.DrawOverride<float, FloatDataField>("Weather", this.weather, drawer.Prefabs.FloatFieldPrefab).Item2;
    floatDataField2.SetSliderSettings(new FloatDataField.FloatSlider(0.0f, 1f));
    floatDataField2.SetSteps(new float?(0.25f));
    FloatDataField floatDataField3 = drawer.DrawOverride<float, FloatDataField>("Cloud Height", this.cloudAltitude, drawer.Prefabs.FloatFieldPrefab).Item2;
    floatDataField3.SetSliderSettings(new FloatDataField.FloatSlider(500f, 4000f));
    floatDataField3.SetSteps(new float?(0.1f));
    FloatDataField floatDataField4 = drawer.DrawOverride<float, FloatDataField>("Wind Speed", this.windSpeed, drawer.Prefabs.FloatFieldPrefab).Item2;
    floatDataField4.SetSliderSettings(new FloatDataField.FloatSlider(0.0f, 20f));
    floatDataField4.SetSteps(new float?(0.1f));
    FloatDataField floatDataField5 = drawer.DrawOverride<float, FloatDataField>("Wind Turbulence", this.windTurbulence, drawer.Prefabs.FloatFieldPrefab).Item2;
    floatDataField5.SetSliderSettings(new FloatDataField.FloatSlider(0.0f, 1f));
    floatDataField5.SetSteps(new float?(1f / 1000f));
    FloatDataField floatDataField6 = drawer.DrawOverride<float, FloatDataField>("Wind Direction", this.windHeading, drawer.Prefabs.FloatFieldPrefab).Item2;
    floatDataField6.SetSliderSettings(new FloatDataField.FloatSlider(0.0f, 360f));
    floatDataField6.SetSteps(new float?(0.1f));
  }
}
