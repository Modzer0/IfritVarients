// Decompiled with JetBrains decompiler
// Type: qol.CommandProcessing.Handlers.TransformHandler
// Assembly: qol, Version=1.1.6.3, Culture=neutral, PublicKeyToken=null
// MVID: 3B6539EA-E38C-45EE-8FFF-FC58CC42BADC
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\BepInEx\plugins\qol_1.1.6.3b1.dll

using qol.CommandProcessing.Core;
using qol.CommandProcessing.Helpers;
using qol.Utilities;
using System;
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;

#nullable disable
namespace qol.CommandProcessing.Handlers;

public class TransformHandler : ICommandHandler
{
  public static readonly Regex TransformPattern = new Regex("^(?<path>(?:\"[^\"]+\"|\\S+))\\s+transform\\s+(?<posX>-?\\d+\\.?\\d*)\\s+(?<posY>-?\\d+\\.?\\d*)\\s+(?<posZ>-?\\d+\\.?\\d*)\\s+(?<rotX>-?\\d+\\.?\\d*)\\s+(?<rotY>-?\\d+\\.?\\d*)\\s+(?<rotZ>-?\\d+\\.?\\d*)\\s+(?<scaleX>-?\\d+\\.?\\d*)\\s+(?<scaleY>-?\\d+\\.?\\d*)\\s+(?<scaleZ>-?\\d+\\.?\\d*)$", RegexOptions.Compiled);

  public Regex Pattern => TransformHandler.TransformPattern;

  public int Priority => 80 /*0x50*/;

  public void Handle(Match match, CommandContext context)
  {
    string path = StringHelpers.StripQuotes(match.Groups["path"].Value);
    Vector3 position = new Vector3(float.Parse(match.Groups["posX"].Value, (IFormatProvider) CultureInfo.InvariantCulture), float.Parse(match.Groups["posY"].Value, (IFormatProvider) CultureInfo.InvariantCulture), float.Parse(match.Groups["posZ"].Value, (IFormatProvider) CultureInfo.InvariantCulture));
    Vector3 euler = new Vector3(float.Parse(match.Groups["rotX"].Value, (IFormatProvider) CultureInfo.InvariantCulture), float.Parse(match.Groups["rotY"].Value, (IFormatProvider) CultureInfo.InvariantCulture), float.Parse(match.Groups["rotZ"].Value, (IFormatProvider) CultureInfo.InvariantCulture));
    Vector3 vector3 = new Vector3(float.Parse(match.Groups["scaleX"].Value, (IFormatProvider) CultureInfo.InvariantCulture), float.Parse(match.Groups["scaleY"].Value, (IFormatProvider) CultureInfo.InvariantCulture), float.Parse(match.Groups["scaleZ"].Value, (IFormatProvider) CultureInfo.InvariantCulture));
    GameObject gameObject = PathLookup.Find(path);
    if ((UnityEngine.Object) gameObject == (UnityEngine.Object) null)
    {
      context.Logger.LogError((object) ("Transform target not found: " + path));
    }
    else
    {
      gameObject.transform.SetPositionAndRotation(position, Quaternion.Euler(euler));
      gameObject.transform.localScale = vector3;
    }
  }
}
