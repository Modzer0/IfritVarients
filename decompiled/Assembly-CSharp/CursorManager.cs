// Decompiled with JetBrains decompiler
// Type: CursorManager
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

#nullable disable
public static class CursorManager
{
  private static bool visible;
  private static CursorFlags flags;
  private static bool forceHidden;
  private static bool setup;

  private static void Setup() => CursorManager.setup = true;

  private static void Application_focusChanged(bool focus)
  {
    if (!Application.isPlaying || !focus || CursorManager.visible)
      return;
    CursorManager.Visible = true;
    CursorManager.Refresh();
  }

  public static CursorFlags GetFlags() => CursorManager.flags;

  public static bool GetFlag(CursorFlags flag) => (CursorManager.flags & flag) != 0;

  public static void SetFlag(CursorFlags flag, bool value)
  {
    int flags = (int) CursorManager.flags;
    if (value)
      CursorManager.flags |= flag;
    else
      CursorManager.flags &= ~flag;
    CursorManager.Refresh();
  }

  public static void ForceHidden(bool hidden)
  {
    CursorManager.forceHidden = hidden;
    CursorManager.Refresh();
  }

  public static void Refresh()
  {
    if (!CursorManager.setup)
      CursorManager.Setup();
    if (CursorManager.forceHidden)
      CursorManager.Visible = false;
    else
      CursorManager.Visible = CursorManager.flags != 0;
  }

  public static bool Visible
  {
    get => CursorManager.visible;
    private set
    {
      if (CursorManager.visible == value)
        return;
      CursorManager.visible = value;
      Cursor.visible = value;
      CursorManager.SetLockState();
    }
  }

  private static void SetLockState()
  {
    Cursor.lockState = CursorManager.visible ? CursorLockMode.None : CursorLockMode.Locked;
  }
}
