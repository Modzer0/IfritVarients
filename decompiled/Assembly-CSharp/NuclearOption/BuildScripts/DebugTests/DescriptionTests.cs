// Decompiled with JetBrains decompiler
// Type: NuclearOption.BuildScripts.DebugTests.DescriptionTests
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

#nullable disable
namespace NuclearOption.BuildScripts.DebugTests;

public static class DescriptionTests
{
  public static void RunAllTests()
  {
    DescriptionTests.TestShortString();
    DescriptionTests.TestVeryShortString();
    DescriptionTests.TestLongString();
    DescriptionTests.TestMaxLength();
    DescriptionTests.TestSpecialCharacters();
    DescriptionTests.TestJapaneseText();
    DescriptionTests.TestEmptyString();
  }

  private static void TestShortString()
  {
    string str = "This is a short description that should fit easily into a single chunk.";
    List<string> results = StringHelper.SplitStringByByteCount(str, true, (int) byte.MaxValue, 10);
    (bool pass, string failReason) = DescriptionTests.Validate(str, results);
    if (pass && results.Count == 2 && results[0] == str && results[1] == "")
      Debug.Log((object) "Test 1a Passed: Short string handled correctly. ✅");
    else
      Debug.LogError((object) $"Test 1a Failed: {failReason ?? "Custom check failed"}. Expected 2 chunks with first chunk containing '{str}' and second being empty. Got {results.Count} chunks. First value: '{(results.Count > 0 ? (object) results[0] : (object) "N/A")}'. ❌");
  }

  private static void TestVeryShortString()
  {
    string str = "Kinda short, so it fits in 1";
    List<string> results = StringHelper.SplitStringByByteCount(str, true, (int) byte.MaxValue, 10);
    (bool pass, string failReason) = DescriptionTests.Validate(str, results);
    if (pass && results.Count == 2 && results[0] == str && results[1] == "")
      Debug.Log((object) "Test 1b Passed: Short string handled correctly. ✅");
    else
      Debug.LogError((object) $"Test 1b Failed: {failReason ?? "Custom check failed"}. Expected 2 chunks with first chunk containing '{str}' and second being empty. Got {results.Count} chunks. First value: '{(results.Count > 0 ? (object) results[0] : (object) "N/A")}'. ❌");
  }

  private static void TestLongString()
  {
    string str = new string('a', 500);
    List<string> results = StringHelper.SplitStringByByteCount(str, true, (int) byte.MaxValue, 10);
    (bool pass, string failReason) = DescriptionTests.Validate(str, results);
    if (pass && results.Count == 3 && Encoding.UTF8.GetByteCount(results[0]) <= (int) byte.MaxValue && Encoding.UTF8.GetByteCount(results[1]) <= (int) byte.MaxValue && results[2] == "")
      Debug.Log((object) "Test 2 Passed: Long string split into multiple chunks correctly. ✅");
    else
      Debug.LogError((object) $"Test 2 Failed: {failReason ?? "Custom check failed"}. Expected 3 chunks. Got {results.Count} chunks. First length: {results[0].Length}, second length: {results[1].Length}, third length: {(results.Count > 2 ? (object) results[2].Length.ToString() : (object) "N/A")}. ❌");
  }

  private static void TestMaxLength()
  {
    string str1 = new string('a', 2000);
    List<string> stringList = StringHelper.SplitStringByByteCount(str1, true, (int) byte.MaxValue, 10);
    (bool pass, string failReason) = DescriptionTests.Validate(str1, stringList);
    string str2 = string.Join("", stringList.Take<string>(stringList.Count - 1));
    if (pass && str2.Length <= 1000)
      Debug.Log((object) "Test 3 Passed: Long string correctly truncated to max length. ✅");
    else
      Debug.LogError((object) $"Test 3 Failed: {failReason ?? "Custom check failed"}. Expected merged length to be {1000}. Got {str2.Length}. ❌");
  }

  private static void TestSpecialCharacters()
  {
    string str = "Hello, world! \uD83D\uDE80\uD83C\uDF0C✨ This text uses emojis, which have a different byte count.";
    List<string> stringList = StringHelper.SplitStringByByteCount(str, true, (int) byte.MaxValue, 10);
    (bool pass, string failReason) = DescriptionTests.Validate(str, stringList);
    int num = stringList.Sum<string>((Func<string, int>) (x => Encoding.UTF8.GetByteCount(x)));
    if (pass && stringList.Count > 1 && num <= Encoding.UTF8.GetByteCount(str) + 2 && stringList.Last<string>() == "")
      Debug.Log((object) "Test 4 Passed: String with special characters handled correctly. ✅");
    else
      Debug.LogError((object) $"Test 4 Failed: {failReason ?? "Custom check failed"}. Expected multiple chunks for special characters, ending with an empty string. Got {stringList.Count} chunks. First value: '{(stringList.Count > 0 ? (object) stringList[0] : (object) "N/A")}'. ❌");
  }

  private static void TestJapaneseText()
  {
    string str = "これは日本語のテキストであり、UTF-8エンコーディングでは文字ごとに複数のバイトが使用されます。このテストケースは、関数が正しくテキストを分割し、バイト制限を尊重していることを確認するためにあります。";
    (bool pass, string failReason) = DescriptionTests.Validate(str, StringHelper.SplitStringByByteCount(str, true, (int) byte.MaxValue, 10));
    if (pass)
      Debug.Log((object) "Test 5 Passed: Japanese text correctly split into chunks under the 255-byte limit. ✅");
    else
      Debug.LogError((object) $"Test 5 Failed: {failReason}. ❌");
  }

  private static void TestEmptyString()
  {
    string str = "";
    List<string> results = StringHelper.SplitStringByByteCount(str, true, (int) byte.MaxValue, 10);
    (bool pass, string failReason) = DescriptionTests.Validate(str, results);
    if (pass && results.Count == 1 && results[0] == "")
      Debug.Log((object) "Test 6 Passed: Empty string handled correctly. ✅");
    else
      Debug.LogError((object) $"Test 6 Failed: {failReason ?? "Custom check failed"}. Expected 1 empty chunk. Got {results.Count} chunks. First value: '{(results.Count > 0 ? (object) results[0] : (object) "N/A")}'. ❌");
  }

  private static (bool pass, string failReason) Validate(string text, List<string> results)
  {
    if (!results.All<string>((Func<string, bool>) (x => Encoding.UTF8.GetByteCount(x) <= (int) byte.MaxValue)))
      return (false, "At least one chunk exceeded the 255-byte limit.");
    if (results.Count > 10)
      return (false, $"Resulting chunks ({results.Count}) exceeded the maximum allowed chunks ({10}).");
    if (results.Count < 10 && results.Last<string>().Length > 0)
      return (false, "Last chunk should be empty if under max number of chunks");
    string str = string.Join("", (IEnumerable<string>) results);
    return !text.StartsWith(str) ? (false, $"Text did not start with merged result\ntext={text}\nresult={str}") : (true, (string) null);
  }
}
