using System;
using System.Text;
using System.Web;
using System.Security.Cryptography;
using System.Globalization;

namespace Amundsen.Utilities
{
  /// <summary>
  /// Public Domain 2008 amundsen.com, inc.
  /// @author mike amundsen (mamund@yahoo.com)
  /// @version 1.0 (2008-07-03)
  /// </summary>
  public class CacheItem
  {
    private string _Key;

    public string Key
    {
      get { return _Key; }
      set { _Key = value; }
    }
    private string _ETag;

    public string ETag
    {
      get { return _ETag; }
      set { _ETag = value; }
    }
    private string _Payload;

    public string Payload
    {
      get { return _Payload; }
      set { _Payload = value; }
    }
    private DateTime _Expires;

    public DateTime Expires
    {
      get { return _Expires; }
      set { _Expires = value; }
    }
    private bool _ShowExpires;

    public bool ShowExpires
    {
      get { return _ShowExpires; }
      set { _ShowExpires = value; }
    }
    private DateTime _LastModified;

    public DateTime LastModified
    {
      get { return _LastModified; }
      set { _LastModified = value; }
    }

    public CacheItem(string key, string payload, string entityTag, DateTime expires, bool showExpires)
    {
      this.Key = key;
      this.ETag = entityTag;
      this.Payload = payload;
      this.Expires = expires;
      this.ShowExpires = showExpires;
      this.LastModified = DateTime.UtcNow;
    }
  }

  public class CacheService
  {
    public CacheItem GetItem(string key)
    {
      return (CacheItem)HttpRuntime.Cache.Get(key);
    }

    public CacheItem PutItem(string key, string payload, string entityTag, DateTime expires, bool showExpires)
    {
      return PutItem(new CacheItem(key, payload, entityTag, expires, showExpires));
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

    public string MD5BinHex(string data)
    {
      Encoding encoding = new ASCIIEncoding();
      byte[] bs = new MD5CryptoServiceProvider().ComputeHash(encoding.GetBytes(data));
      string hash = "";

      for (int i = 0; i < 16; i++)
        hash = String.Concat(hash, String.Format(CultureInfo.CurrentCulture,"{0:x02}", bs[i]));

      return hash;
    }

  }
}
