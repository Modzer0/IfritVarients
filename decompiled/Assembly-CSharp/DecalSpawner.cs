// Decompiled with JetBrains decompiler
// Type: DecalSpawner
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using UnityEngine.Rendering.Universal;

#nullable disable
public class DecalSpawner : MonoBehaviour
{
  private int layerMask = 64 /*0x40*/;
  private RaycastHit hit;
  [SerializeField]
  private float decalSize;
  [SerializeField]
  private Material decalMaterial;
  [SerializeField]
  private float fadeInTime;
  [SerializeField]
  private bool verticalProjection;

  private void Start()
  {
    if (Physics.Linecast(this.transform.position + Vector3.up * this.decalSize * 0.5f, this.transform.position - Vector3.up * this.decalSize * 0.5f, out this.hit, this.layerMask))
    {
      GameObject effect = Object.Instantiate<GameObject>(GameAssets.i.scorchMarkDecal, Datum.origin);
      DecalProjector component = effect.GetComponent<DecalProjector>();
      component.size = new Vector3(this.decalSize, this.decalSize, this.decalSize * 0.2f);
      effect.transform.rotation = this.verticalProjection ? Quaternion.LookRotation(Vector3.up) : Quaternion.LookRotation(-this.hit.normal);
      effect.transform.position = this.hit.point;
      component.material = this.decalMaterial;
      SceneSingleton<EffectManager>.i.AddEffect(effect);
      if ((double) this.fadeInTime > 0.0)
        this.DecalFadeIn(component).Forget();
    }
    if ((double) this.fadeInTime != 0.0)
      return;
    Object.Destroy((Object) this);
  }

  private async UniTask DecalFadeIn(DecalProjector decalProjector)
  {
    DecalSpawner decalSpawner = this;
    CancellationToken cancel = decalSpawner.destroyCancellationToken;
    float fadeInRate = 1f / decalSpawner.fadeInTime;
    for (decalProjector.fadeFactor = 0.0f; (double) decalProjector.fadeFactor < 1.0; decalProjector.fadeFactor += fadeInRate * Time.deltaTime)
    {
      await UniTask.Yield();
      if (cancel.IsCancellationRequested)
      {
        cancel = new CancellationToken();
        return;
      }
    }
    decalProjector.fadeFactor = 1f;
    Object.Destroy((Object) decalSpawner);
    cancel = new CancellationToken();
  }
}
