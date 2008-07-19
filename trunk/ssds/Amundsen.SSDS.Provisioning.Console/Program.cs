using System;
using System.Text.RegularExpressions;

using Amundsen.Utilities;

namespace Amundsen.SSDS.Provisioning
{
  /// <summary>
  /// Public Domain 2008 amundsen.com, inc.
  /// @author mike amundsen (mamund@yahoo.com)
  /// @version 1.0 (2008-07-18)
  /// </summary>
  class SsdsPC
  {
    static string ssdsUser = string.Empty;
    static string ssdsPassword = string.Empty;

    static string authority_regex = "^/([^/]*)$"; 
    static string container_regex = "^/([^/]*)/([^/]*)$"; 
    static string entity_regex = "^/([^/]*)/([^/]*)/([^/?]*)$"; 
    static string query_regex = "^/([^/]*)/([^/]*)/\\?(.*)$"; 

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

        string uri = args[1];

        // authority command
        if (Regex.IsMatch(uri, authority_regex, RegexOptions.IgnoreCase))
        {
          ssds.Authorities(new string[]{args[0],uri.Replace("/","")});
          return;
        }

        // container command
        if (Regex.IsMatch(uri, container_regex, RegexOptions.IgnoreCase))
        {
          string[] elm = uri.Split('/');
          ssds.Containers(new string[] { args[0], elm[1], elm[2] });
          return;
        }

        // entity command
        if (Regex.IsMatch(uri, entity_regex, RegexOptions.IgnoreCase))
        {
          string[] elm = uri.Split('/');
          ssds.Entities(new string[] { args[0], elm[1], elm[2], elm[3], (args.Length==3?args[2]:string.Empty)});
          return;
        }

        // query command
        if (Regex.IsMatch(uri, query_regex, RegexOptions.IgnoreCase))
        {
          string[] elm = uri.Split('/');
          ssds.Queries(new string[] { args[0], elm[1], elm[2], args[2] });
          return;
        }

        // failed to recognize command uri
        ShowHelp();
        return;

      }
      catch (Exception ex)
      {
        Console.WriteLine(string.Format("\n***ERROR: {0}\n",ex.Message));
      }
    }

    private static void ShowHelp()
    {
      Console.WriteLine("\nSSDS Provisioning Console (1.0 - 2008-07-18)\n");

      Console.WriteLine("Authorities:");
      Console.WriteLine("\t[g]et /{aid} \n\tex: a /my-authority\n");
      Console.WriteLine("\t[p]post /{aid} \n\tex: p /my-new-authority\n");

      Console.WriteLine("Containers:");
      Console.WriteLine("\t[g]et /{aid}/ \n\tex: g /my-authority/\n");
      Console.WriteLine("\t[g]et /{aid}/{cid} \n\tex: g /my-authority/my-container\n");
      Console.WriteLine("\t[p]ost /{aid}/{cid} \n\tex: p /my-authority/my-new-container\n");
      Console.WriteLine("\t[d]elete /{aid}/{cid} \n\tex: d /my-authority/my-container\n");

      Console.WriteLine("Entities:");
      Console.WriteLine("\t[g]et /{aid}/{cid}/ \n\tex: g /my-authority/my-container/\n");
      Console.WriteLine("\t[g]et /{aid}/{cid}/{eid} \n\tex: g /my-authority/my-container/id001\n");
      Console.WriteLine("\t[p]ost /{aid}/{cid}/ \"{xml}|{filename}\" \n\tex: p /my-authority/my-container/ c:\\new-data.xml\n");
      Console.WriteLine("\t[u]pdate /{aid}/{cid}/{eid} \"{xml|filename}\" \n\tex: u /my-authority/my-container/id001 c:\\modified-data.xml\n");
      Console.WriteLine("\t[d]elete /{aid}/{cid}/{eid} \n\tex: d /my-authority/my-container/id001\n");

      Console.WriteLine("Queries:");
      Console.WriteLine("\t[q]uery /{aid}/{cid}/? \"{query}\" \n\tex: q /my-authority/my-container/? \"from e in entities where e.Id>\\\"1\\\" $and$ e.Id<\\\"30\\\" select e\"\n");
    }

    private static void HandleConfigSettings()
    {
      WebUtility wu = new WebUtility();
      ssdsUser = wu.GetConfigSectionItem("ssdsSettings", "ssdsUser");
      ssdsPassword = wu.GetConfigSectionItem("ssdsSettings", "ssdsPassword");
    }
  }

  // the real fun starts here
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
      int cmd = 0;
      int authority = 1;

      // set auth header
      client.RequestHeaders.Add("authorization", "Basic "+h.Base64Encode(string.Format("{0}:{1}", this.UserName, this.Password)));

      switch (args[cmd].ToLower())
      {
        case "g":
        case "get":
          url = string.Format("http://{0}{1}", Constants.proxyRoot, args[authority]);
          Console.WriteLine(client.Execute(url, "get", Constants.SsdsType));
          break;

        case "p":
        case "post":
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
      int cmd = 0;
      int authority = 1;
      int container = 2;
      
      // set auth header
      client.RequestHeaders.Add("authorization", "Basic " + h.Base64Encode(string.Format("{0}:{1}", this.UserName, this.Password)));

      switch (args[cmd].ToLower())
      {
        case "g":
        case "get":
          Console.WriteLine(client.Execute(string.Format("http://{0}{1}/{2}", Constants.proxyRoot, args[authority], args[container]), "get", Constants.SsdsType));
          break;

        case "p":
        case "post":
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
      int cmd = 0;
      int authority = 1;
      int container = 2;
      int entity = 3;
      int doc = 4;
      
      // set auth header
      client.RequestHeaders.Add("authorization", "Basic " + h.Base64Encode(string.Format("{0}:{1}", this.UserName, this.Password)));

      switch (args[cmd].ToLower())
      {
        case "g":
        case "get":
          Console.WriteLine(client.Execute(string.Format("http://{0}{1}/{2}/{3}", Constants.proxyRoot, args[authority], args[container], args[entity]), "get", Constants.SsdsType));
          break;

        case "p":
        case "post":
          body = ResolveDocument(args[doc]);
          url = string.Format("http://{0}{1}/{2}/", Constants.proxyRoot, args[authority], args[container]);
          Console.WriteLine(client.Execute(url, "post", Constants.SsdsType, body));
          Console.WriteLine("Entity has been added.");
          break;

        case "u":
        case "update":
        case "put":
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

      // parse into valid query string using jscript library
      query_string = args[query].Replace("$and$", "%26%26").Replace("$or$", "||");
      url = string.Format("http://{0}{1}/{2}/?{3}", Constants.proxyRoot, args[authority], args[container], query_string);
      query_string = Microsoft.JScript.GlobalObject.encodeURIComponent(new Uri(url).Query);
      url = string.Format("http://{0}{1}/{2}/?{3}", Constants.proxyRoot, args[authority], args[container], query_string.Substring(3));

      // execute and show results
      Console.WriteLine(client.Execute(url, "get", Constants.SsdsType));
    }

    // return valid SSDS entity (from disk, if needed)
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
