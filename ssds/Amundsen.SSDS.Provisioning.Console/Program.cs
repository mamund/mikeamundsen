using System;
using System.Text.RegularExpressions;

using Amundsen.Utilities;

namespace Amundsen.SSDS.Provisioning
{
  /// <summary>
  /// Public Domain 2008 amundsen.com, inc.
  /// @author mike amundsen (mamund@yahoo.com)
  /// @version 1.3 (2008-07-20)
  /// @version 1.2 (2008-07-24)
  /// @version 1.1 (2008-07-20)
  /// @version 1.0 (2008-07-18)
  /// </summary>
  class SsdsPC
  {
    static string ssdsUser = string.Empty;
    static string ssdsPassword = string.Empty;
    static string ssdsProxy = string.Empty;

    static string authority_regex = "^/([^/]*)$"; 
    static string container_regex = "^/([^/]*)/([^/]*)$"; 
    static string entity_regex = "^/([^/]*)/([^/]*)/([^/?]*)$"; 
    static string query_regex = "^/([^/]*)/([^/]*)/\\?(.*)$"; 

    static void Main(string[] args)
    {
      SsdsCommands ssds = new SsdsCommands();
      string uri = string.Empty;
      string cmd = string.Empty;
      string[] arglist = args;

      try
      {
        if (arglist.Length == 0)
        {
          ShowHelp();
          return;
        }

        HandleConfigSettings();
        ssds.UserName = ssdsUser;
        ssds.Password = ssdsPassword;
        ssds.ssdsProxy = ssdsProxy;

        uri = arglist[0];
        cmd = (args.Length == 1 ? "get" : (uri.IndexOf("?") == -1 ? arglist[1] : "get"));

        // authority command
        if (Regex.IsMatch(uri, authority_regex, RegexOptions.IgnoreCase))
        {
          ssds.Authorities(new string[]{cmd,uri.Replace("/","")});
          return;
        }

        // container command
        if (Regex.IsMatch(uri, container_regex, RegexOptions.IgnoreCase))
        {
          string[] elm = uri.Split('/');
          ssds.Containers(new string[] { cmd, elm[1], elm[2] });
          return;
        }

        // entity command
        if (Regex.IsMatch(uri, entity_regex, RegexOptions.IgnoreCase))
        {
          string[] elm = uri.Split('/');
          ssds.Entities(new string[] { cmd, elm[1], elm[2], elm[3], (arglist.Length == 3 ? arglist[2] : string.Empty) });
          return;
        }

        // query command
        if (Regex.IsMatch(uri, query_regex, RegexOptions.IgnoreCase))
        {
          string[] elm = uri.Split('/');
          ssds.Queries(new string[] { cmd, elm[1], elm[2], arglist[1] });
          return;
        }

        // failed to recognize command uri
        Console.Out.WriteLine("***ERROR: unable to parse command line!");
        ShowHelp();
        return;

      }
      catch (Exception ex)
      {
        Console.Out.WriteLine(string.Format("\n***ERROR: {0}\n",ex.Message));
      }
    }

    private static void ShowHelp()
    {
      Console.Out.WriteLine("\nSDS Provisioning Console (1.2 - 2008-07-24)\n");

      Console.Out.WriteLine("Authorities:");
      Console.Out.WriteLine("\t/{aid} [[g]get]\n\tex: /my-authority\n");
      Console.Out.WriteLine("\t/{aid} [p]post\n\tex: /my-new-authority p\n");

      Console.Out.WriteLine("Containers:");
      Console.Out.WriteLine("\t/{aid}/ [[g]get]\n\tex: /my-authority/\n");
      Console.Out.WriteLine("\t/{aid}/{cid} [[g]get]\n\tex: /my-authority/my-container\n");
      Console.Out.WriteLine("\t/{aid}/{cid} [p]ost\n\tex: /my-authority/my-new-container p \n");
      Console.Out.WriteLine("\t/{aid}/{cid} [d]elete\n\tex: /my-authority/my-container d\n");

      Console.Out.WriteLine("Entities:");
      Console.Out.WriteLine("\t/{aid}/{cid}/ [[g]et]\n\tex: /my-authority/my-container/\n");
      Console.Out.WriteLine("\t/{aid}/{cid}/{eid} [[g]get]\n\tex: /my-authority/my-container/id001\n");
      Console.Out.WriteLine("\t/{aid}/{cid}/ \"{xml}|{filename}\" [p]ost\n\tex: /my-authority/my-container/ c:\\new-data.xml p\n");
      Console.Out.WriteLine("\t/{aid}/{cid}/{eid} \"{xml|filename}\" [u]pdate|put\n\tex: /my-authority/my-container/id001 c:\\modified-data.xml u\n");
      Console.Out.WriteLine("\t/{aid}/{cid}/{eid} [d]elete\n\tex: /my-authority/my-container/id001 d\n");

      Console.Out.WriteLine("Queries:");
      Console.Out.WriteLine("\t/{aid}/{cid}/? \"{query}\" [[g]get]\n\tex: /my-authority/my-container/? \"from e in entities where e.Id>\\\"1\\\" $and$ e.Id<\\\"30\\\" select e\"\n");
    }

    private static void HandleConfigSettings()
    {
      WebUtility wu = new WebUtility();
      ssdsUser = wu.GetConfigSectionItem("ssdsSettings", "ssdsUser");
      ssdsPassword = wu.GetConfigSectionItem("ssdsSettings", "ssdsPassword");
      ssdsProxy = wu.GetConfigSectionItem("ssdsSettings", "ssdsProxy");
    }
  }

  // the real fun starts here
  class SsdsCommands
  {
    HttpClient client = new HttpClient();
    Hashing h = new Hashing();

    public string UserName = string.Empty;
    public string Password = string.Empty;
    public string ssdsProxy = string.Empty;

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
          url = string.Format("{0}{1}", this.ssdsProxy, args[authority]);
          Console.Out.WriteLine(client.Execute(url, "get", Constants.SsdsType));
          break;

        case "p":
        case "post":
          body = string.Format("<authority>{0}</authority>", args[authority]);
          url = string.Format("{0}", this.ssdsProxy);
          Console.Out.WriteLine(client.Execute(url, "post", Constants.SsdsType, body));
          Console.Out.WriteLine(string.Format("Authority [{0}] has been added.", (client.ResponseHeaders["location"]!=null?client.ResponseHeaders["location"]:args[authority])));
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
          Console.Out.WriteLine(client.Execute(string.Format("{0}{1}/{2}", this.ssdsProxy, args[authority], args[container]), "get", Constants.SsdsType));
          break;

        case "p":
        case "post":
          body = string.Format("<container>{0}</container>", args[container]);
          url = string.Format("{0}{1}/", this.ssdsProxy, args[authority]);
          Console.Out.WriteLine(client.Execute(url, "post", Constants.SsdsType, body));
          Console.Out.WriteLine(string.Format("Container [{0}] has been added.", (client.ResponseHeaders["location"]!=null?client.ResponseHeaders["location"]:args[container])));
          break;

        case "d":
        case "delete":
          url = string.Format("{0}{1}/{2}", this.ssdsProxy, args[authority], args[container]);
          Console.Out.WriteLine(client.Execute(url, "delete", Constants.SsdsType));
          Console.Out.WriteLine(string.Format("Container [{0}] has been deleted.", args[container]));
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
          Console.Out.WriteLine(client.Execute(string.Format("{0}{1}/{2}/{3}", this.ssdsProxy, args[authority], args[container], args[entity]), "get", Constants.SsdsType));
          break;

        case "p":
        case "post":
          body = ResolveDocument(args[doc]);
          url = string.Format("{0}{1}/{2}/", this.ssdsProxy, args[authority], args[container]);
          Console.Out.WriteLine(client.Execute(url, "post", Constants.SsdsType, body));
          Console.Out.WriteLine(string.Format("Entity [{0}] has been added.",(client.ResponseHeaders["location"]!=null?client.ResponseHeaders["location"]:string.Empty)));
          break;

        case "u":
        case "update":
        case "put":
          body = ResolveDocument(args[doc]);
          url = string.Format("{0}{1}/{2}/{3}", this.ssdsProxy, args[authority], args[container], args[entity]);

          // get current record's etag value
          client.RequestHeaders.Add("cache-control", "no-cache");
          client.Execute(url, "head", Constants.SsdsType);
          etag = (client.ResponseHeaders["etag"] != null ? client.ResponseHeaders["etag"] : string.Empty);

          // update w/ the etag value
          client.RequestHeaders.Add("if-match", etag);
          Console.Out.WriteLine(client.Execute(url, "put", Constants.SsdsType, body));
          Console.Out.WriteLine(string.Format("Entity [{0}] has been updated.", args[entity]));
          break;

        case "d":
        case "delete":
          url = string.Format("{0}{1}/{2}/{3}", this.ssdsProxy, args[authority], args[container], args[entity]);
          Console.Out.WriteLine(client.Execute(url, "delete", Constants.SsdsType));
          Console.Out.WriteLine(string.Format("Entity [{0}] has been deleted.", args[entity]));
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

      // parse into valid query url
      // note using jscript library to handle proper encoding for SSDS
      url = string.Format("{0}{1}/{2}/?{3}", this.ssdsProxy, args[authority], args[container], args[query].Replace("$and$", "%26%26").Replace("$or$", "||"));
      query_string = Microsoft.JScript.GlobalObject.encodeURIComponent(new Uri(url).Query);
      url = string.Format("{0}{1}/{2}/?{3}", this.ssdsProxy, args[authority], args[container], query_string.Substring(3));

      // execute and show results
      Console.Out.WriteLine(client.Execute(url, "get", Constants.SsdsType));
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
