// Decompiled with JetBrains decompiler
// Type: Cockpit
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
public class Cockpit : MonoBehaviour
{
  [SerializeField]
  private Renderer tacScreenRender;
  [SerializeField]
  private GameObject tacScreenUIPrefab;
  [SerializeField]
  private Aircraft aircraft;
  private TacScreen tacScreen;
  [SerializeField]
  private GameObject[] engineSources;
  private List<IEngine> engineStates = new List<IEngine>();
  [SerializeField]
  private Cockpit.Joystick[] joysticks;
  [SerializeField]
  private Cockpit.Throttle[] throttles;
  private ControlInputs inputs;

  private void Awake()
  {
    this.aircraft.onInitialize += new Action(this.Cockpit_OnAircraftInitialize);
    for (int index = 0; index < this.engineSources.Length; ++index)
      this.engineStates.Add(this.engineSources[index].GetComponent<IEngine>());
    this.inputs = this.aircraft.GetInputs();
  }

  private void Update()
  {
    foreach (Cockpit.Joystick joystick in this.joysticks)
      joystick.Animate(this.inputs);
    foreach (Cockpit.Throttle throttle in this.throttles)
      throttle.Animate(this.inputs);
  }

  public List<IEngine> GetEngineStates() => this.engineStates;

  private void Cockpit_OnAircraftInitialize()
  {
    if ((UnityEngine.Object) SceneSingleton<CombatHUD>.i != (UnityEngine.Object) null && (UnityEngine.Object) SceneSingleton<CombatHUD>.i.aircraft != (UnityEngine.Object) null && (UnityEngine.Object) SceneSingleton<CombatHUD>.i.aircraft == (UnityEngine.Object) this.aircraft)
    {
      this.tacScreen = UnityEngine.Object.Instantiate<GameObject>(this.tacScreenUIPrefab, this.transform).GetComponent<TacScreen>();
      this.tacScreen.Initialize(this.aircraft, this);
      this.aircraft.onDisableUnit += new Action<Unit>(this.Cockpit_OnAircraftDisable);
      this.enabled = true;
    }
    else
      this.enabled = false;
  }

  private void Cockpit_OnAircraftDisable(Unit unit)
  {
    UnityEngine.Object.Destroy((UnityEngine.Object) this.tacScreen.gameObject);
  }

  [Serializable]
  private class Joystick
  {
    [SerializeField]
    private Transform transform;
    [SerializeField]
    private float range;

    public void Animate(ControlInputs inputs)
    {
      this.transform.localEulerAngles = new Vector3(inputs.pitch * this.range, 0.0f, -inputs.roll * this.range);
    }
  }

  [Serializable]
  private class Throttle
  {
    [SerializeField]
    private bool rotation;
    [SerializeField]
    private bool motion;
    [SerializeField]
    private Transform transform;
    [SerializeField]
    private float range;

    public void Animate(ControlInputs inputs)
    {
      if (this.rotation)
        this.transform.localEulerAngles = new Vector3(inputs.throttle * this.range, 0.0f, 0.0f);
      if (!this.motion)
        return;
      this.transform.localPosition = new Vector3(0.0f, 0.0f, inputs.throttle * this.range);
    }
  }
}
