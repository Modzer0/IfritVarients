// Decompiled with JetBrains decompiler
// Type: Lightning
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

#nullable disable
public class Lightning : MonoBehaviour
{
  [SerializeField]
  private ParticleSystem lightningSystem;
  private ParticleSystem.EmitParams emitParams;
  [SerializeField]
  private Light flashLight;
  private ParticleSystem.MainModule main;
  [SerializeField]
  private float flashIntensity;
  [SerializeField]
  private AudioClip[] thunderClips;
  private float strikeTimer = 10f;

  private void OnEnable()
  {
    this.main = this.lightningSystem.main;
    this.main.simulationSpace = ParticleSystemSimulationSpace.Custom;
    this.main.customSimulationSpace = Datum.origin;
    this.flashLight.transform.SetParent(Datum.origin);
    this.transform.localPosition = Vector3.zero;
    this.strikeTimer = (float) Random.Range(2, 20);
    this.flashLight.enabled = false;
  }

  private void GenerateStrike()
  {
    this.flashLight.enabled = true;
    Vector3 vector3_1 = SceneSingleton<CameraStateManager>.i.transform.position + SceneSingleton<CameraStateManager>.i.transform.forward * 10000f + new Vector3((float) Random.Range(-10000, 10000), 0.0f, (float) Random.Range(-10000, 10000));
    Vector3 vector3_2 = vector3_1.ToGlobalPosition().AsVector3() with
    {
      y = NetworkSceneSingleton<LevelInfo>.i.cloudHeight + 50f
    };
    Vector3 vector3_3 = vector3_1;
    int num = 0;
    for (int index = 30; num < index; ++num)
    {
      this.emitParams.position = vector3_2;
      vector3_3 += vector3_2;
      this.lightningSystem.Emit(this.emitParams, 1);
      vector3_2 += new Vector3((float) Random.Range(-40, 40), -60f, (float) Random.Range(-40, 40));
    }
    this.flashLight.transform.localPosition = vector3_3 / (float) num;
    this.flashLight.intensity = this.flashIntensity;
    if (FastMath.OutOfRange(vector3_1, SceneSingleton<CameraStateManager>.i.transform.position, 8000f))
      return;
    GameObject gameObject = new GameObject();
    gameObject.name = "Lightning Sound";
    gameObject.transform.SetParent(Datum.origin);
    gameObject.transform.position = new Vector3(vector3_1.x, SceneSingleton<CameraStateManager>.i.transform.position.y, vector3_1.z);
    AudioSource audioSource = gameObject.AddComponent<AudioSource>();
    audioSource.maxDistance = 8000f;
    audioSource.spatialBlend = 1f;
    audioSource.dopplerLevel = 0.0f;
    audioSource.spread = 20f;
    audioSource.rolloffMode = AudioRolloffMode.Linear;
    audioSource.clip = this.thunderClips[Mathf.FloorToInt((float) Random.Range(0, this.thunderClips.Length))];
    AudioLowPassFilter filter = gameObject.AddComponent<AudioLowPassFilter>();
    SceneSingleton<ExplosionAudioManager>.i.AddExplosionAudio(audioSource, filter, 0.0f);
    Object.Destroy((Object) gameObject, 35f);
  }

  private void Update()
  {
    if ((Object) NetworkSceneSingleton<LevelInfo>.i == (Object) null)
      return;
    this.flashLight.intensity -= this.flashIntensity * Time.deltaTime;
    if ((double) this.flashLight.intensity <= 0.0)
      this.flashLight.enabled = false;
    this.strikeTimer -= Time.deltaTime;
    if ((double) this.strikeTimer > 0.0)
      return;
    this.GenerateStrike();
    this.strikeTimer = (float) Random.Range(2, 20);
  }
}
