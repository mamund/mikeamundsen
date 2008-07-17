using System;

using Amundsen.Utilities;

namespace Amundsen.SSDS.Provisioning
{
  /// <summary>
  /// Public Domain 2008 amundsen.com, inc.
  /// @author mike amundsen (mamund@yahoo.com)
  /// @version 1.0 (2008-07-17)
  /// </summary>
  class SsdsPC
  {
    static string ssdsUser = string.Empty;
    static string ssdsPassword = string.Empty;

    static void Main(string[] args)
    {
      SsdsCommands ssds = new SsdsCommands();

      try
      {
        if (args.Length == 0)
        {
          ShowHelp();
          return;
        }

        HandleConfigSettings();
        ssds.UserName = ssdsUser;
        ssds.Password = ssdsPassword;

        switch(args[0].ToLower())
        {
          case "a":
          case "authorities":
            ssds.Authorities(args);
            break;
          case "c":
          case "containers":
            ssds.Containers(args);
            break;
          case "e":
          case "entities":
            ssds.Entities(args);
            break;
          case "q":
          case "queries":
            ssds.Queries(args);
            break;
          default:
            ShowHelp();
            break;
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine(string.Format("\n***ERROR: {0}\n",ex.Message));
      }
    }

    private static void ShowHelp()
    {
      Console.WriteLine("\n**** Help:");
      Console.WriteLine("\t[a]uthorities [r]ead {aid}");
      Console.WriteLine("\t[a]uthorities [a]dd {aid}\n");

      Console.WriteLine("\t[c]ontainers [l]ist {aid}");
      Console.WriteLine("\t[c]ontainers [r]ead {aid} {cid}");
      Console.WriteLine("\t[c]ontainers [a]dd {aid} {cid}");
      Console.WriteLine("\t[c]ontainers [d]elete {aid} {cid}\n");

      Console.WriteLine("\t[e]ntities [l]ist {aid} {cid}");
      Console.WriteLine("\t[e]ntities [r]ead {aid} {cid} {eid}");
      Console.WriteLine("\t[e]ntities [a]dd {aid} {cid} \"{xml}|{filename}\"");
      Console.WriteLine("\t[e]ntities [u]pdate {aid} {cid} {eid} \"{xml|filename}\"");
      Console.WriteLine("\t[e]ntities [d]elete {aid} {cid} {eid}\n");

      Console.WriteLine("\t[q]ueries {aid} {cid} \"{query}\"\n");

      Console.WriteLine("\t[h]elp\n");
    }

    private static void HandleConfigSettings()
    {
      WebUtility wu = new WebUtility();
      ssdsUser = wu.GetConfigSectionItem("ssdsSettings", "ssdsUser");
      ssdsPassword = wu.GetConfigSectionItem("ssdsSettings", "ssdsPassword");
    }
  }

  class SsdsCommands
  {
    HttpClient client = new HttpClient();
    Hashing h = new Hashing();

    public string UserName = string.Empty;
    public string Password = string.Empty;

    public SsdsCommands() {}
    public SsdsCommands(string user,string pass)
    {
      this.UserName = user;
      this.Password = pass;
    }

    public void Authorities(string[] args)
    {
      string body = string.Empty;
      string url = string.Empty;
      int cmd = 1;
      int authority = 2;

      // set auth header
      client.RequestHeaders.Add("authorization", "Basic "+h.Base64Encode(string.Format("{0}:{1}", this.UserName, this.Password)));

      switch (args[cmd].ToLower())
      {
        case "r":
        case "read":
          url = string.Format("http://{0}{1}", Constants.proxyRoot, args[authority]);
          Console.WriteLine(client.Execute(url, "get", Constants.SsdsType));
          break;

        case "a":
        case "add":
          body = string.Format("<authority>{0}</authority>", args[authority]);
          url = string.Format("http://{0}", Constants.proxyRoot);
          Console.WriteLine(client.Execute(url, "post", Constants.SsdsType, body));
          Console.WriteLine(string.Format("Authority [{0}] has been added.", args[authority]));
          break;

        default:
          throw new ApplicationException("Invalid Authority Command [" + args[cmd] + "]");
      }
    }

    public void Containers(string[] args)
    {
      string body = string.Empty;
      string url = string.Empty;
      int cmd = 1;
      int authority = 2;
      int container = 3;
      
      // set auth header
      client.RequestHeaders.Add("authorization", "Basic " + h.Base64Encode(string.Format("{0}:{1}", this.UserName, this.Password)));

      switch (args[cmd].ToLower())
      {
        case "l":
        case "list":
          Console.WriteLine(client.Execute(string.Format("http://{0}{1}/", Constants.proxyRoot, args[authority]), "get", Constants.SsdsType));
          break;

        case "r":
        case "read":
          Console.WriteLine(client.Execute(string.Format("http://{0}{1}/{2}", Constants.proxyRoot, args[authority], args[container]), "get", Constants.SsdsType));
          break;

        case "a":
        case "add":
          body = string.Format("<container>{0}</container>", args[container]);
          url = string.Format("http://{0}{1}/", Constants.proxyRoot, args[authority] );
          Console.WriteLine(client.Execute(url, "post", Constants.SsdsType, body));
          Console.WriteLine(string.Format("Container [{0}] has been added to [{1}].", args[container], args[authority]));
          break;

        case "d":
        case "delete":
          url = string.Format("http://{0}{1}/{2}", Constants.proxyRoot, args[authority], args[container]);
          Console.WriteLine(client.Execute(url, "delete", Constants.SsdsType));
          Console.WriteLine(string.Format("Container [{0}] has been deleted.", args[container]));
          break;

        default:
          throw new ApplicationException("Invalid Container Command [" + args[cmd] + "]");
      }
    }

    public void Entities(string[] args)
    {
      string body = string.Empty;
      string url = string.Empty;
      string etag = string.Empty;
      int cmd = 1;
      int authority = 2;
      int container = 3;
      int entity = 4;
      int doc = 5;
      
      // set auth header
      client.RequestHeaders.Add("authorization", "Basic " + h.Base64Encode(string.Format("{0}:{1}", this.UserName, this.Password)));

      switch (args[cmd].ToLower())
      {
        case "l":
        case "list":
          Console.WriteLine(client.Execute(string.Format("http://{0}{1}/{2}/", Constants.proxyRoot, args[authority], args[container]), "get", Constants.SsdsType));
          break;

        case "r":
        case "read":
          Console.WriteLine(client.Execute(string.Format("http://{0}{1}/{2}/{3}", Constants.proxyRoot, args[authority], args[container], args[entity]), "get", Constants.SsdsType));
          break;

        case "a":
        case "add":
          body = ResolveDocument(args[entity]);
          url = string.Format("http://{0}{1}/{2}/", Constants.proxyRoot, args[authority], args[container]);
          Console.WriteLine(client.Execute(url, "post", Constants.SsdsType, body));
          Console.WriteLine(string.Format("Entity [{0}] has been added.", args[entity]));
          break;

        case "u":
        case "update":
          body = ResolveDocument(args[doc]);
          url = string.Format("http://{0}{1}/{2}/{3}", Constants.proxyRoot, args[authority], args[container], args[entity]);

          // get current record's etag value
          client.RequestHeaders.Add("cache-control", "no-cache");
          client.Execute(url, "head", Constants.SsdsType);
          etag = (client.ResponseHeaders["etag"] != null ? client.ResponseHeaders["etag"] : string.Empty);

          // update w/ the etag value
          client.RequestHeaders.Add("if-match", etag);
          Console.WriteLine(client.Execute(url, "put", Constants.SsdsType, body));
          Console.WriteLine(string.Format("Entity [{0}] has been updated.", args[entity]));
          break;

        case "d":
        case "delete":
          url = string.Format("http://{0}{1}/{2}/{3}", Constants.proxyRoot, args[authority], args[container], args[entity]);
          Console.WriteLine(client.Execute(url, "delete", Constants.SsdsType));
          Console.WriteLine(string.Format("Entity [{0}] has been deleted.", args[entity]));
          break;

        default:
          throw new ApplicationException("Invalid Entity Command [" + args[cmd] + "]");
      }
    }

    public void Queries(string[] args)
    {
      string query_string = string.Empty;
      string url = string.Empty;
      int authority = 1;
      int container = 2;
      int query = 3;

      // set auth header
      client.RequestHeaders.Add("authorization", "Basic " + h.Base64Encode(string.Format("{0}:{1}", this.UserName, this.Password)));

      // parse into valid query string
      query_string = args[query].Replace("$and$", "%26%26").Replace("$or$", "||");
      url = string.Format("http://{0}{1}/{2}/?{3}", Constants.proxyRoot, args[authority], args[container], query_string);
      query_string = Microsoft.JScript.GlobalObject.encodeURIComponent(new Uri(url).Query);
      url = string.Format("http://{0}{1}/{2}/?{3}", Constants.proxyRoot, args[authority], args[container], query_string.Substring(3));

      // execute and show results
      Console.WriteLine(client.Execute(url, "get", Constants.SsdsType));
    }

    private string ResolveDocument(string doc)
    {
      string rtn = string.Empty;
      if (doc.IndexOf(Constants.SitkaNS) != -1)
      {
        rtn = doc;
      }
      else
      {
        using (System.IO.StreamReader sr = new System.IO.StreamReader(doc))
        {
          rtn = sr.ReadToEnd();
          sr.Close();
        }
      }
      return rtn;
    }
  }
}
