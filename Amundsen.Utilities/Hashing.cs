using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;

namespace Amundsen.Utilities
{
  /// <summary>
  /// Public Domain 2008-2010 amundsen.com, inc.
  /// @author mike amundsen (mamund@yahoo.com)
  /// @version 1.0 (2008-07-03)
  /// </summary>
  public class Hashing
  {
    public string Base64Encode(string data)
    {
      try
      {
        byte[] encData_byte = new byte[data.Length];
        encData_byte = System.Text.Encoding.UTF8.GetBytes(data);
        string encodedData = Convert.ToBase64String(encData_byte);
        return encodedData;
      }
      catch (Exception e)
      {
        throw new Exception("Error in base64Encode" + e.Message);
      }
    }

    public string Base64Decode(string data)
    {
      try
      {
        System.Text.UTF8Encoding encoder = new System.Text.UTF8Encoding();
        System.Text.Decoder utf8Decode = encoder.GetDecoder();

        byte[] todecode_byte = Convert.FromBase64String(data);
        int charCount = utf8Decode.GetCharCount(todecode_byte, 0, todecode_byte.Length);
        char[] decoded_char = new char[charCount];
        utf8Decode.GetChars(todecode_byte, 0, todecode_byte.Length, decoded_char, 0);
        string result = new String(decoded_char);
        return result;
      }
      catch (Exception e)
      {
        throw new Exception("Error in base64Decode" + e.Message);
      }
    }

    public string MD5(string data)
    {
      return MD5(data, false);
    }
    public string MD5(string data, bool removeTail)
    {
      string rtn = Convert.ToBase64String(new System.Security.Cryptography.MD5CryptoServiceProvider().ComputeHash(System.Text.Encoding.Default.GetBytes(data)));
      if (removeTail)
        return rtn.Replace("=", "");
      else
        return rtn;
    }

    public string MD5BinHex(string val)
    {
      Encoding encoding = new ASCIIEncoding();
      byte[] bs = new MD5CryptoServiceProvider().ComputeHash(encoding.GetBytes(val));
      string hash = "";

      for (int i = 0; i < 16; i++)
        hash = String.Concat(hash, String.Format("{0:x02}", bs[i]));

      return hash;
    }

    public string SHA1(string data)
    {
      string rtn = string.Empty;
      SHA1 md = new SHA1CryptoServiceProvider();

      byte[] digest = md.ComputeHash(Encoding.Default.GetBytes(data));
      foreach (byte i in digest)
      {
        rtn += i.ToString("x2");
      }
      return rtn;

    }

    // hashing helper
    public string MacSha(string canonicalizedString, byte[] key)
    {
      byte[] dataToMAC = System.Text.Encoding.UTF8.GetBytes(canonicalizedString);

      using (HMACSHA256 hmacsha1 = new HMACSHA256(key))
      {
        return System.Convert.ToBase64String(hmacsha1.ComputeHash(dataToMAC));
      }
    }

  }
}
