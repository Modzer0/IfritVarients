// Decompiled with JetBrains decompiler
// Type: WindowsTTS
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Cysharp.Threading.Tasks;
using SpeechLib;
using System;
using System.Threading.Tasks;

#nullable disable
public class WindowsTTS
{
  private readonly SpVoice voice = (SpVoice) new SpVoiceClass();

  public void Speak(int speed, int volume, string text, bool asSsml)
  {
    this.voice.Rate = speed;
    this.voice.Volume = volume;
    SpeechVoiceSpeakFlags Flags = SpeechVoiceSpeakFlags.SVSFlagsAsync | SpeechVoiceSpeakFlags.SVSFPurgeBeforeSpeak;
    if (asSsml)
    {
      text = $"<speak version=\"1.0\" xmlns=\"http://www.w3.org/2001/10/synthesis\" xml:lang=\"en-US\">{text}</speak>";
      Flags |= SpeechVoiceSpeakFlags.SVSFParseSsml;
    }
    this.voice.Speak(text, Flags);
  }

  public bool IsPlaying() => !this.voice.WaitUntilDone(0);

  public static UniTask SpeakAsync(int speed, int volume, string text, bool asSsml)
  {
    return UniTask.RunOnThreadPool((Func<UniTask>) (async () =>
    {
      WindowsTTS tts = new WindowsTTS();
      tts.Speak(speed, volume, text, asSsml);
      while (tts.IsPlaying())
        await Task.Delay(100);
      tts = (WindowsTTS) null;
    }));
  }
}
