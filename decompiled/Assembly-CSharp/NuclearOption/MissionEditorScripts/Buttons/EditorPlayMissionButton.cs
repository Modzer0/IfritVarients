// Decompiled with JetBrains decompiler
// Type: NuclearOption.MissionEditorScripts.Buttons.EditorPlayMissionButton
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Cysharp.Threading.Tasks;
using JamesFrowen.ScriptableVariables.UI;

#nullable disable
namespace NuclearOption.MissionEditorScripts.Buttons;

public class EditorPlayMissionButton : ButtonController
{
  protected override void onClick() => MissionEditor.PlayFromEditor().Forget();
}
