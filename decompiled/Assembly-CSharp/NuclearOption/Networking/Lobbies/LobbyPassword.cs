// Decompiled with JetBrains decompiler
// Type: NuclearOption.Networking.Lobbies.LobbyPassword
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using System.Security.Cryptography;
using System.Text;

#nullable disable
namespace NuclearOption.Networking.Lobbies;

public class LobbyPassword
{
  private const int NONCE_LENGTH = 32 /*0x20*/;
  private byte[] combinedBytes;

  public LobbyPassword(string lobbyPassword)
  {
    this.combinedBytes = LobbyPassword.GenerateCombineByteArray(lobbyPassword);
  }

  public LobbyPassword.PasswordChallenge GenerateChallenge()
  {
    return new LobbyPassword.PasswordChallenge(this.combinedBytes);
  }

  public static bool TestShortPassword(LobbyInstance lobby, string password)
  {
    string shortPassword;
    return !lobby.IsPasswordProtected(out shortPassword) || shortPassword == LobbyPassword.GetShortPassword(password);
  }

  public static string GetShortPassword(string password)
  {
    using (SHA256 shA256 = SHA256.Create())
    {
      byte[] bytes = Encoding.UTF8.GetBytes(password);
      return $"{(int) shA256.ComputeHash(bytes)[0] & 63 /*0x3F*/:x1}";
    }
  }

  private static byte[] GenerateCombineByteArray(string password)
  {
    byte[] bytes = Encoding.UTF8.GetBytes(password);
    byte[] dst = new byte[bytes.Length + 32 /*0x20*/];
    Buffer.BlockCopy((Array) bytes, 0, (Array) dst, 0, bytes.Length);
    return dst;
  }

  public static byte[] GenerateResponse(string password, ArraySegment<byte> nonceBytes)
  {
    return LobbyPassword.HashPasswordNonce(LobbyPassword.GenerateCombineByteArray(password), nonceBytes);
  }

  private static byte[] HashPasswordNonce(byte[] combinedBytes, ArraySegment<byte> nonceBytes)
  {
    using (SHA256 shA256 = SHA256.Create())
    {
      Buffer.BlockCopy((Array) nonceBytes.Array, nonceBytes.Offset, (Array) combinedBytes, combinedBytes.Length - 32 /*0x20*/, 32 /*0x20*/);
      return shA256.ComputeHash(combinedBytes);
    }
  }

  public class PasswordChallenge
  {
    private readonly byte[] combinedBytes;
    private readonly byte[] nonce;
    private bool hasChallenged;

    public PasswordChallenge(byte[] combinedBytes)
    {
      this.combinedBytes = combinedBytes;
      this.nonce = LobbyPassword.PasswordChallenge.GenerateNonceBytes();
    }

    public ArraySegment<byte> GetNonceBytes() => ArraySegment<byte>.op_Implicit(this.nonce);

    public bool VerifyResponse(ArraySegment<byte> clientResponse)
    {
      if (this.hasChallenged)
        return false;
      this.hasChallenged = true;
      byte[] numArray = LobbyPassword.HashPasswordNonce(this.combinedBytes, ArraySegment<byte>.op_Implicit(this.nonce));
      int offset = clientResponse.Offset;
      int count = clientResponse.Count;
      byte[] array = clientResponse.Array;
      if (count != numArray.Length)
        return false;
      for (int index = 0; index < count; ++index)
      {
        if ((int) array[offset + index] != (int) numArray[index])
          return false;
      }
      return true;
    }

    private static byte[] GenerateNonceBytes()
    {
      byte[] data = new byte[32 /*0x20*/];
      using (RandomNumberGenerator randomNumberGenerator = RandomNumberGenerator.Create())
        randomNumberGenerator.GetBytes(data);
      return data;
    }
  }
}
