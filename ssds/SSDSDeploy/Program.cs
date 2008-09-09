using System;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Security.Permissions;
using Microsoft.Win32;
using System.Threading;

using Amundsen.Utilities;
using Amundsen.SSDS.Provisioning;

namespace Amundsen.SSDS.Deploy
{
  /// <summary>
  /// Public Domain 2008 amundsen.com, inc.
  /// @author mike amundsen (mamund@yahoo.com)
  /// @version 1.1 (2008-09-08)
  /// @version 1.0 (2008-08-18)
  /// </summary>
  class Program
  {
    static string ssdsUser = string.Empty;
    static string ssdsPassword = string.Empty;
    static string entity_regex = "^/([^/]*)/([^/]*)/([^/?]*)$";

    static void Main(string[] args)
    {
      Deploy d = new Deploy();
      string uri = string.Empty;
      string cmd = string.Empty;
      string[] arglist = args;

      Console.Out.WriteLine("\nSSDS Deploy Console (1.0 - 2008-08-18)\n");

      if (arglist.Length == 0)
      {
        ShowHelp();
        return;
      }

      try
      {
        HandleConfigSettings();
        d.UserName = ssdsUser;
        d.Password = ssdsPassword;

        uri = arglist[0];
        cmd = "post";
        string files = args[1];
        string ctype = (args.Length > 2 && args[2] != "y" ? args[2] : string.Empty);
        string overwrite = (args[args.Length-1]=="y"?"y":string.Empty);
        // process entity command
        if (Regex.IsMatch(uri, entity_regex, RegexOptions.IgnoreCase))
        {
          string[] elm = uri.Split('/');
          d.PostCollection(new string[] { cmd, elm[1], elm[2], elm[3], files, ctype, overwrite});
          return;
        }
      }
      catch (Exception ex)
      {
        Console.Out.WriteLine("ERROR: " + ex.Message);
      }
    }

    private static void ShowHelp()
    {
      Console.Out.WriteLine("POST single binary file:");

      Console.Out.WriteLine("/{a}/{c}/{e} \"[c:][\\folder\\path\\]file.ext\" [\"mime-type\"] [y]");
      Console.Out.WriteLine("where:\t{a} = authority");
      Console.Out.WriteLine("\t{c} = container");
      Console.Out.WriteLine("\t{e} = entity");
      Console.Out.WriteLine("\t y  = overwrite existing entities\n");

      Console.Out.WriteLine("ex:\t/my-auth/files/my-profile \"c:\\temp\\profile.jpg\" \"image\\jpeg\" y");
      
      Console.Out.WriteLine("\nPOST multiple files using wildcard:");
      Console.Out.WriteLine("/{a}/{c}/* \"[c:][\\folder\\path\\]*.*\" [y]");
      Console.Out.WriteLine("ex:\t/my-authority/my-container/* \"c:\\uploads\\*.*\" y");
      Console.Out.WriteLine("\tor");
      Console.Out.WriteLine("ex:\t/my-authority/my-container/* \"c:\\images\\*.png\" y");
    }

    private static void HandleConfigSettings()
    {
      WebUtility wu = new WebUtility();
      ssdsUser = wu.GetConfigSectionItem("ssdsSettings", "ssdsUser");
      ssdsPassword = wu.GetConfigSectionItem("ssdsSettings", "ssdsPassword");
    }
  }

  // *********************************************************
  // this class does the real work
  // *********************************************************
  class Deploy
  {
    public string UserName = string.Empty;
    public string Password = string.Empty;

    // post a collection of files
    public void PostCollection(string[] args)
    {
      string cmd = args[0];
      string authority = args[1];
      string container = args[2];
      string entity = args[3];
      string diskFile = args[4];
      string contentType = args[5];
      string overwrite = args[6].ToLower();

      string[] fileSet = null;
      string folder = string.Empty;
      bool defaultEntity = (entity == "*" || entity==string.Empty);

      // build set of files to upload
      if (diskFile.IndexOf("*") != -1)
      {
        if (diskFile.IndexOf(@"\") == -1 || diskFile.IndexOf(":") == -1)
        {
          folder = string.Format(@"{0}\", Directory.GetCurrentDirectory());
        }
        else
        {
          folder = diskFile.Substring(0, diskFile.LastIndexOf(@"\")+1);
          diskFile = diskFile.Replace(folder, "");
        }
        fileSet = Directory.GetFiles(folder, diskFile);
      }
      else
      {
        fileSet = new string[] { diskFile };
      }

      // now upload each file in the set
      for (int i = 0; i < fileSet.Length; i++)
      {
        try
        {
          PostFile(new string[] { cmd, authority, container, (defaultEntity?new FileInfo(fileSet[i]).Name.ToLower():entity), fileSet[i], contentType, overwrite });
        }
        catch (Exception ex)
        {
          Console.WriteLine("ERROR: " + ex.Message);
        }
      }
    }

    // post a single file
    public void PostFile(string[] args)
    {
      string cmd = args[0];
      string authority = args[1];
      string container = args[2];
      string entity = args[3];
      string diskFile = args[4];
      string contentType = args[5];
      string overwrite = args[6];

      string uri = string.Format("https://{0}.{1}{2}", authority, Constants.SsdsRoot, container);

      if (cmd == "post" || cmd == "p")
      {
        Console.Out.WriteLine(string.Format("{0}/{1}",uri,entity));

        if (overwrite == "y")
        {
          try
          {
            HttpClient client = new HttpClient(this.UserName, this.Password);
            client.Execute(string.Format("{0}/{1}", uri, entity), "delete", Constants.SsdsType);
          }
          catch (Exception ex)
          {
            // ignore this
          }
        }
        using (FileStream fs = File.OpenRead(diskFile))
        {
          try
          {
            IAsyncResult res = null;
            byte[] buffer = new byte[64 * 1024]; // 64k 
            int bytesRead = -1;

            HttpWebRequest wr = GetRequestObject(uri, fs, contentType, diskFile, entity, this.UserName, this.Password);

            using (Stream rs = wr.GetRequestStream())
            {
              while (true)
              {
                bytesRead = fs.Read(buffer, 0, buffer.Length);
                if (bytesRead <= 0)
                {
                  break;
                }
                rs.Write(buffer, 0, bytesRead);
                Console.Write(".");

                if (res == null)
                {
                  res = wr.BeginGetResponse(new AsyncCallback(uploadCallback), wr);
                  ThreadPool.RegisterWaitForSingleObject(
                    res.AsyncWaitHandle,
                    new WaitOrTimerCallback(timeoutCallback),
                    wr,
                    (30 * 1000),
                    true);
                }
              }
            }
          }
          catch (WebException ex)
          {
            string msg = ex.Message;

            if (ex.Response != null)
            {
              using (HttpWebResponse er = (HttpWebResponse)ex.Response)
              {
                msg = string.Format("{0}/{1}", (int)er.StatusCode, er.StatusDescription);
              }
            }
            throw new ApplicationException(msg);
          }
          finally
          {
            fs.Close();
          }
        }
      }
      else
      {
        throw new ArgumentException("Unknown command [" + cmd + "]");
      }
    }

    private HttpWebRequest GetRequestObject(string uri, FileStream fs,string contentType,string diskFile,string entity, string user, string pass)
    {
      Hashing h = new Hashing();
      HttpWebRequest wr = (HttpWebRequest)HttpWebRequest.Create(uri);
      wr.Method = "post";
      wr.ContentLength = fs.Length;
      wr.ContentType = (contentType == string.Empty ? GetMimeType(diskFile) : contentType);
      wr.Timeout = (30 * 1000); // 30-second timeout
      wr.AllowWriteStreamBuffering = false;
      wr.Headers.Add("authorization", "Basic " + h.Base64Encode(string.Format("{0}:{1}", user, pass)));
      wr.Headers.Add("slug", entity);

      return wr;
    }

    // handle timeout error
    private static void timeoutCallback(object obj, bool timedOut)
    {
      if (timedOut)
      {
        HttpWebRequest req = (HttpWebRequest)obj;

        if (req != null)
        {
          req.Abort();
        }
        throw new Exception("Upload timed-out");
      }
    }
  
    // handle async callback
    private void uploadCallback(IAsyncResult res)
    {
      HttpWebRequest req = (HttpWebRequest)res.AsyncState;
      if (req != null)
      {
        using (HttpWebResponse response = (HttpWebResponse)req.EndGetResponse(res))
        {
          try
          {
            Console.Out.WriteLine(" {0}/{1} ({2})", (int)response.StatusCode, response.StatusDescription, response.Headers["ETag"]);
          }
          catch (Exception ex) 
          {
            // ignore this one
          }
        }
      }
    }

    // lookup the mime-type from the registry
    private string GetMimeType(string file)
    {
      string rtn = string.Empty;
      string subKey = @"MIME\Database\Content Type";

      RegistryPermission perm = new RegistryPermission(RegistryPermissionAccess.Read, "\\HKEY_CLASSES_ROOT");
      RegistryKey root = Registry.ClassesRoot;
      RegistryKey key = root.OpenSubKey(subKey);

      FileInfo fi = new FileInfo(file);
      string ext = fi.Extension;

      foreach(string name in key.GetSubKeyNames())
      {
        RegistryKey k = root.OpenSubKey(string.Format(@"{0}\{1}", subKey, name));
        if (k.GetValue("Extension") != null && k.GetValue("Extension").ToString().ToLower() == ext)
        {
          rtn = name;
          break;
        }
      }

      // failed to find it!
      if(rtn==string.Empty)
      {
        rtn = "application/octet-stream";
      }
      
      return rtn;
    }
  }
}
