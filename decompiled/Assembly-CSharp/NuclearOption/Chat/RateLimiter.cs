// Decompiled with JetBrains decompiler
// Type: NuclearOption.Chat.RateLimiter
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System.Collections.Generic;

#nullable disable
namespace NuclearOption.Chat;

public class RateLimiter
{
  private readonly int limit;
  private readonly float timeSpan;
  private readonly Queue<float> messages = new Queue<float>();

  public RateLimiter(int limit, float timeSpan)
  {
    this.limit = limit;
    this.timeSpan = timeSpan;
  }

  public void OnSend(float now) => this.messages.Enqueue(now);

  public bool ShouldLimit(float now)
  {
    while (this.messages.Count > 0 && (double) this.messages.Peek() < (double) now - (double) this.timeSpan)
    {
      double num = (double) this.messages.Dequeue();
    }
    return this.messages.Count >= this.limit;
  }
}
