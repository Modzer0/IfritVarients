// Decompiled with JetBrains decompiler
// Type: NuclearOption.BuildScripts.DebugTests.LobbyPasswordTest
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.Networking.Lobbies;
using System;
using UnityEngine;

#nullable disable
namespace NuclearOption.BuildScripts.DebugTests;

public class LobbyPasswordTest
{
  public static void Run()
  {
    string str = "nuclear option";
    LobbyPassword lobbyPassword = new LobbyPassword(str);
    LobbyPassword.PasswordChallenge challenge1 = lobbyPassword.GenerateChallenge();
    LobbyPassword.PasswordChallenge challenge2 = lobbyPassword.GenerateChallenge();
    byte[] response1 = LobbyPassword.GenerateResponse(str, challenge1.GetNonceBytes());
    byte[] response2 = LobbyPassword.GenerateResponse(str, challenge2.GetNonceBytes());
    bool flag1 = challenge2.VerifyResponse(ArraySegment<byte>.op_Implicit(response2));
    bool flag2 = challenge1.VerifyResponse(ArraySegment<byte>.op_Implicit(response1));
    Debug.Log((object) $"[Case 1] Interleaved - Player B: {flag1}, Player A: {flag2}");
    if (!flag2)
      Debug.LogError((object) "BUG DETECTED: Player A failed because Player B overwritten the shared buffer!");
    LobbyPassword.PasswordChallenge challenge3 = lobbyPassword.GenerateChallenge();
    byte[] response3 = LobbyPassword.GenerateResponse(str, challenge3.GetNonceBytes());
    challenge3.VerifyResponse(ArraySegment<byte>.op_Implicit(response3));
    Debug.Log((object) $"[Case 2] Re-entry - Second attempt success: {challenge3.VerifyResponse(ArraySegment<byte>.op_Implicit(response3))} (Expected: False)");
    LobbyPassword.PasswordChallenge challenge4 = new LobbyPassword("very_long_password_here_12345").GenerateChallenge();
    byte[] response4 = LobbyPassword.GenerateResponse("very_long_password_here_12345", challenge4.GetNonceBytes());
    Debug.Log((object) $"[Case 3] Length check - Success: {challenge4.VerifyResponse(ArraySegment<byte>.op_Implicit(response4))}");
    LobbyPassword.PasswordChallenge challenge5 = lobbyPassword.GenerateChallenge();
    byte[] response5 = LobbyPassword.GenerateResponse(str, challenge5.GetNonceBytes());
    byte[] numArray = new byte[1024 /*0x0400*/];
    int num = 50;
    Buffer.BlockCopy((Array) response5, 0, (Array) numArray, num, response5.Length);
    ArraySegment<byte> clientResponse = new ArraySegment<byte>(numArray, num, response5.Length);
    Debug.Log((object) $"[Case 4] ArraySegment respect - Success: {challenge5.VerifyResponse(clientResponse)}");
  }
}
