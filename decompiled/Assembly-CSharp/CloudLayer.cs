// Decompiled with JetBrains decompiler
// Type: CloudLayer
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

#nullable disable
public class CloudLayer : MonoBehaviour
{
  [SerializeField]
  private WeatherSet[] weatherSets;
  private WeatherSet currentWeatherSet;
  [SerializeField]
  private ParticleSystem cloudSystem;
  [SerializeField]
  private ParticleSystem distantCloudSystem;
  [SerializeField]
  private ParticleSystem flyThroughSystem;
  [SerializeField]
  private MeshRenderer cloudRenderer;
  [SerializeField]
  private float densityMapScale;
  [SerializeField]
  private float layerHeight;
  [SerializeField]
  private float layerThickness;
  [SerializeField]
  private float cloudSizeMin;
  [SerializeField]
  private float cloudSizeMax;
  [SerializeField]
  private float cloudSizeVariation;
  [SerializeField]
  private float cloudLifeMin;
  [SerializeField]
  private float cloudLifeMax;
  [SerializeField]
  private float cloudDrawDist;
  [SerializeField]
  private int generationRate;
  [SerializeField]
  private int maxParticles;
  [SerializeField]
  private Lightning lightning;
  private Material layerMaterial;
  private Material cloudMaterial;
  private Material distantCloudMaterial;
  private Material flyThroughMaterial;
  private Vector3 samplePoint;
  private ParticleSystem.EmitParams emitParams;
  private ParticleSystem.MainModule main;
  private ParticleSystem.MainModule distantMain;
  private ParticleSystem.EmissionModule flyThroughEmit;
  private ParticleSystem.MainModule flyThroughMain;
  private ParticleSystem.ShapeModule flyThroughShape;
  private float spawnOpacity;
  private float cloudDetail = -1f;
  private float sizeFactor = 1f;
  private Vector2 windDisplacement;
  private Vector3 camVelPrev;
  private static int id_cloudPattern = Shader.PropertyToID("_cloudPattern");
  private static int id_ScatterColor = Shader.PropertyToID("_ScatterColor");
  private static int id_SunDirection = Shader.PropertyToID("_SunDirection");
  private static int id_CloudsOriginOffset = Shader.PropertyToID("_CloudsOriginOffset");

  private void Start()
  {
    this.main = this.cloudSystem.main;
    this.main.simulationSpace = ParticleSystemSimulationSpace.Custom;
    this.main.customSimulationSpace = Datum.origin;
    this.flyThroughMain = this.flyThroughSystem.main;
    this.flyThroughEmit = this.flyThroughSystem.emission;
    this.flyThroughShape = this.flyThroughSystem.shape;
    this.flyThroughMain.simulationSpace = ParticleSystemSimulationSpace.Custom;
    this.flyThroughMain.customSimulationSpace = Datum.origin;
    this.distantMain = this.distantCloudSystem.main;
    this.distantMain.simulationSpace = ParticleSystemSimulationSpace.Custom;
    this.distantMain.customSimulationSpace = Datum.origin;
    if (GameManager.gameState == GameState.Editor)
    {
      this.main.useUnscaledTime = true;
      this.distantMain.useUnscaledTime = true;
      this.flyThroughMain.useUnscaledTime = true;
    }
    else
    {
      this.main.useUnscaledTime = false;
      this.distantMain.useUnscaledTime = false;
      this.flyThroughMain.useUnscaledTime = false;
    }
    this.layerMaterial = MaterialHelper.CloneMaterial((Renderer) this.cloudRenderer);
    this.cloudMaterial = this.cloudSystem.GetComponent<ParticleSystemRenderer>().material;
    this.distantCloudMaterial = this.distantCloudSystem.GetComponent<ParticleSystemRenderer>().material;
    this.flyThroughMaterial = this.flyThroughSystem.GetComponent<ParticleSystemRenderer>().material;
    this.UpdateWeatherSets();
    if (GameManager.IsHeadless)
      return;
    this.WeatherSlowUpdate().Forget();
  }

  public float GetCloudOcclusion()
  {
    return (double) SceneSingleton<CameraStateManager>.i.transform.position.y > (double) this.transform.position.y ? 0.0f : this.currentWeatherSet.coverage;
  }

  private async UniTask WeatherSlowUpdate()
  {
    CloudLayer cloudLayer = this;
    CancellationToken cancel = cloudLayer.destroyCancellationToken;
    await UniTask.Delay(1000, true);
    while (!cancel.IsCancellationRequested)
    {
      cloudLayer.UpdateWeatherSets();
      await UniTask.Delay(1000, true);
    }
    cancel = new CancellationToken();
  }

  private void UpdateWeatherSets()
  {
    this.currentWeatherSet = this.weatherSets[Mathf.Clamp(Mathf.FloorToInt(NetworkSceneSingleton<LevelInfo>.i.conditions * (float) this.weatherSets.Length), 0, this.weatherSets.Length - 1)];
    if ((Object) this.currentWeatherSet != (Object) null)
    {
      Transform transform;
      if (GameManager.gameState == GameState.Encyclopedia)
      {
        transform = Camera.main.transform;
      }
      else
      {
        if (!((Object) SceneSingleton<CameraStateManager>.i != (Object) null))
          return;
        transform = SceneSingleton<CameraStateManager>.i.transform;
      }
      this.layerMaterial.SetTexture(CloudLayer.id_cloudPattern, (Texture) this.currentWeatherSet.mask);
      Color color = NetworkSceneSingleton<LevelInfo>.i.GetSunColor() * NetworkSceneSingleton<LevelInfo>.i.GetDaylightFactor(transform.position);
      this.cloudMaterial.SetColor(CloudLayer.id_ScatterColor, color);
      this.distantCloudMaterial.SetColor(CloudLayer.id_ScatterColor, color);
      this.flyThroughMaterial.SetColor(CloudLayer.id_ScatterColor, color);
      Vector3 forward = NetworkSceneSingleton<LevelInfo>.i.sun.transform.forward;
      this.cloudMaterial.SetVector(CloudLayer.id_SunDirection, (Vector4) forward);
      this.distantCloudMaterial.SetVector(CloudLayer.id_SunDirection, (Vector4) forward);
      this.flyThroughMaterial.SetVector(CloudLayer.id_SunDirection, (Vector4) forward);
      Vector3 position1 = this.transform.position;
      Vector3 position2 = transform.position;
      this.distantCloudSystem.gameObject.SetActive((double) this.currentWeatherSet.coverage > 0.10000000149011612);
      this.sizeFactor = NetworkSceneSingleton<LevelInfo>.i.conditions * 1.8f;
      this.distantMain.startSize = new ParticleSystem.MinMaxCurve(1200f * this.sizeFactor, 1800f * this.sizeFactor);
    }
    if (this.currentWeatherSet.lightning && !this.lightning.gameObject.activeSelf)
      this.lightning.gameObject.SetActive(true);
    if (!this.currentWeatherSet.lightning && this.lightning.gameObject.activeSelf)
      this.lightning.gameObject.SetActive(false);
    if ((double) this.cloudDetail == (double) Mathf.Clamp(PlayerSettings.cloudDetail, 0.1f, 1f))
      return;
    this.cloudDetail = Mathf.Clamp(PlayerSettings.cloudDetail, 0.1f, 1f);
    this.cloudSystem.gameObject.GetComponent<ParticleSystemRenderer>().material.SetFloat("_OpacityBoost", 1f - PlayerSettings.cloudDetail);
    this.main.maxParticles = (int) ((double) this.maxParticles * (double) Mathf.Sqrt(this.cloudDetail));
  }

  private void Update()
  {
    if (GameManager.IsHeadless || (Object) SceneSingleton<CameraStateManager>.i == (Object) null)
      return;
    Transform transform = SceneSingleton<CameraStateManager>.i.transform;
    this.layerHeight = NetworkSceneSingleton<LevelInfo>.i.cloudHeight;
    Texture2D cookie1 = this.currentWeatherSet.cookies[Mathf.Clamp(Mathf.FloorToInt((float) (((double) transform.position.y - ((double) this.transform.position.y + 200.0)) * (double) this.currentWeatherSet.cookies.Length / 200.0)), 0, this.currentWeatherSet.cookies.Length - 1)];
    Texture2D cookie2 = this.currentWeatherSet.cookies[0];
    NetworkSceneSingleton<LevelInfo>.i.SetCookie(cookie1, (float) ((double) this.currentWeatherSet.cookies[0].width * (double) this.densityMapScale * 0.75), this.windDisplacement);
    this.windDisplacement += new Vector2(NetworkSceneSingleton<LevelInfo>.i.windVelocity.x, NetworkSceneSingleton<LevelInfo>.i.windVelocity.z) * Time.deltaTime;
    this.layerMaterial.SetVector(CloudLayer.id_CloudsOriginOffset, (Vector4) new Vector2(Datum.originPosition.x + this.windDisplacement.x, Datum.originPosition.z + this.windDisplacement.y));
    this.transform.position = new Vector3(0.0f, Datum.LocalSeaY + this.layerHeight, 0.0f);
    int num1 = 0;
    float cloudDrawDist = this.cloudDrawDist;
    this.samplePoint = transform.position + transform.forward * cloudDrawDist * 0.2f + SceneSingleton<CameraStateManager>.i.cameraVelocity * 5f;
    float num2 = Mathf.Clamp01((float) (1.0 + (double) (this.transform.position.y - transform.position.y) * 0.00025000001187436283));
    Vector3 position1 = Datum.origin.position;
    for (; (double) num1 < (double) this.generationRate * (double) num2 * (double) this.cloudDetail; ++num1)
    {
      Vector2 insideUnitCircle = Random.insideUnitCircle;
      this.samplePoint += new Vector3(insideUnitCircle.x * cloudDrawDist, 0.0f, insideUnitCircle.y * cloudDrawDist);
      GlobalPosition globalPosition = this.samplePoint.ToGlobalPosition();
      Color pixel = this.currentWeatherSet.particleSampler.GetPixel((int) (((double) globalPosition.x - (double) this.windDisplacement.x) / (double) this.densityMapScale % (double) cookie2.width), (int) (((double) globalPosition.z - (double) this.windDisplacement.y) / (double) this.densityMapScale % (double) cookie2.height));
      if ((double) pixel.r > 0.15000000596046448)
      {
        this.spawnOpacity = 1f - pixel.r;
        this.main.startColor = (ParticleSystem.MinMaxGradient) new Color(1f, 1f, 1f, pixel.r * 0.5f);
        this.samplePoint -= position1;
        this.samplePoint.y = this.layerHeight + Mathf.Sign(Random.value) * this.spawnOpacity * this.layerThickness;
        this.emitParams.position = this.samplePoint;
        this.emitParams.velocity = Vector3.right * NetworkSceneSingleton<LevelInfo>.i.windVelocity.x + Vector3.forward * NetworkSceneSingleton<LevelInfo>.i.windVelocity.y;
        float t = (float) (((double) Mathf.Abs(insideUnitCircle.x) + (double) Mathf.Abs(insideUnitCircle.y)) / ((double) cloudDrawDist * 1.2000000476837158));
        float num3 = Mathf.Lerp(this.cloudSizeMin, this.cloudSizeMax, t) * Mathf.Lerp(0.1f, 1.1f, this.sizeFactor);
        this.main.startLifetimeMultiplier = Mathf.Lerp(this.cloudLifeMin, this.cloudLifeMax, t);
        this.main.startSizeMultiplier = num3 * Random.Range(1f - this.cloudSizeVariation, 1f + this.cloudSizeVariation);
        this.cloudSystem.Emit(this.emitParams, 1);
      }
    }
    float num4 = this.transform.position.y + 50f;
    GlobalPosition globalPosition1 = transform.GlobalPosition();
    Vector3 cameraVelocity = SceneSingleton<CameraStateManager>.i.cameraVelocity;
    Vector3 vector3_1 = (cameraVelocity - this.camVelPrev) / Time.unscaledDeltaTime;
    Vector3 vector3_2 = SceneSingleton<CameraStateManager>.i.transform.forward * 30f;
    GlobalPosition position2 = globalPosition1 + vector3_2 + cameraVelocity * 0.8f;
    this.camVelPrev = cameraVelocity;
    float num5 = position2.ToLocalPosition().y - num4;
    float num6 = 1f;
    if ((double) num5 < -50.0)
      num6 = (float) (1.0 + ((double) num5 + 50.0) * 0.0099999997764825821);
    if ((double) num5 > 50.0)
      num6 = (float) (1.0 - ((double) num5 - 50.0) * 0.029999999329447746);
    if ((double) num6 > 0.0)
    {
      this.flyThroughSystem.transform.position = position2.ToLocalPosition();
      Color pixel = this.currentWeatherSet.particleSampler.GetPixel((int) (((double) position2.x - (double) this.windDisplacement.x) / (double) this.densityMapScale % (double) cookie2.width), (int) (((double) position2.z - (double) this.windDisplacement.y) / (double) this.densityMapScale % (double) cookie2.height));
      num6 *= pixel.r;
      if ((double) pixel.r > 0.0)
      {
        float magnitude = SceneSingleton<CameraStateManager>.i.cameraVelocity.magnitude;
        this.flyThroughShape.radius = (float) (50.0 * (1.0 + (double) magnitude * 0.004999999888241291));
        this.flyThroughMain.startColor = (ParticleSystem.MinMaxGradient) new Color(1f, 1f, 1f, num6 * num6);
        this.flyThroughEmit.rateOverTime = (ParticleSystem.MinMaxCurve) Mathf.Lerp(20f, 60f, magnitude * (3f / 1000f) * this.cloudDetail);
        this.flyThroughSystem.transform.LookAt(SceneSingleton<CameraStateManager>.i.transform.position);
        this.flyThroughMain.startLifetime = (ParticleSystem.MinMaxCurve) Mathf.Lerp(6f, 1.5f, magnitude * 0.005f);
        this.flyThroughMain.startSize = (ParticleSystem.MinMaxCurve) Mathf.Lerp(25f, 100f, magnitude * (3f / 1000f));
      }
    }
    if (this.flyThroughSystem.isPlaying)
    {
      if ((double) num6 > 0.0)
        return;
      this.flyThroughSystem.Stop();
    }
    else
    {
      if ((double) num6 <= 0.0)
        return;
      this.flyThroughSystem.Play();
    }
  }
}
