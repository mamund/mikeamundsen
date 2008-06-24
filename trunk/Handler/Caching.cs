using System;
using System.Text;
using System.Web;
using System.Security.Cryptography;

namespace amundsen.ssds
{
  public class CacheItem
  {
    public string Key;
    public string ETag;
    public string Payload;
    public DateTime Expires;
    public bool ShowExpires;
    public DateTime LastModified;

    public CacheItem(string key, string payload, string etag, DateTime expires, bool show_expires)
    {
      this.Key = key;
      this.ETag = etag;
      this.Payload = payload;
      this.Expires = expires;
      this.ShowExpires = show_expires;
      this.LastModified = DateTime.UtcNow;
    }
  }

  public class CacheService
  {
    public CacheItem GetItem(string key)
    {
      return (CacheItem)HttpRuntime.Cache.Get(key);
    }

    public CacheItem PutItem(string key, string payload, string etag, DateTime expires, bool show_expires)
    {
      return PutItem(new CacheItem(key, payload, etag, expires, show_expires));
    }
    public CacheItem PutItem(CacheItem item)
    {
      HttpRuntime.Cache.Insert(item.Key, item);
      return item;
    }

    public void RemoveItem(string key)
    {
      HttpRuntime.Cache.Remove(key);
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

  }
}
