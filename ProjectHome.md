This is a repository of various free/open-source code projects i've started.

Most are projects i created to speaking/training events. some are more extensive projects that explore interesting topics. a few are just my personal putterings.

### 2009-05-25 Amundsen.Utilities ###
  * Stand-alone version of HTTP utilities used in SDS/Azure projects
  * Includes clientTest project as an example
  * HTTPClient.cs
  * DateTimePrecise.cs
  * Xslt.cs
  * Caching.cs
  * Mimeparse.cs
  * Hashing.cs

### 2008-12-13 General update to sync all code to CTP Public SDS servers ###
  * SDS Proxy updated
  * Constants.cs uses new CTP server address
  * overall clean up of codebase for CTP release

### 2008-11-15 Download ZIP contains new Photo Demo project + JOIN, ORDERBY, and TAKE ###
  * Photo Demo shows storing images, JS and CSS in SDS (600 images, 80MB)
  * SDS-Proxy now supports JOIN, ORDERBY, and TAKE + metrics from SDS servers
  * SDSDeploy.EXE bug fix to invalid MIME-types on bulk uploads
  * updated Provision Client and SDS.EXE point to new SDS Server Cluster

### 2008-10-13 Download ZIP contains new Entity-Binding SSDS DataSet project ###
  * download your Entity from SSDS
  * generate XSD Schema for your Entity
  * use XSD.exe to generate CS class for strongly-typed DatSet
  * add new DS class to your project
  * enumerate strongly-typed dataset to console
  * mod to your taste

```
@echo off
rem 2008-10-13 (mca) : make typed datasets from SSDS entity

set d=..\entity-binding
xsd task.xsd sitka.xsd /dataset /n:Amundsen.SSDS.Binding /o:%d%\
set d=

rem eof
```

```
static void Main(string[] args)
{
  HttpClient c = new HttpClient();
  NewDataSet ds = new NewDataSet();
  string xml = string.Empty;
  int x = 0;

  try
  {
    // get data from SSDS (this URL doesn't require SSDS auth)
    xml = c.Execute("http://amundsen.com/ssds/tasks/");

    // load the data into the strongly-typed dataset
    using (MemoryStream ms = new MemoryStream(Encoding.ASCII.GetBytes(xml)))
    {
      ds.ReadXml(ms);
      ms.Close();
    }

    // enumerate the task table
    Console.WriteLine(ds.task.TableName);
    foreach (NewDataSet.taskRow r in ds.task)
    {
      Console.WriteLine(string.Format("{0}: {1}, {2}, {3}, {4}", 
        ++x, r.Id, r.name, r._is_completed, r.Version));
    }
  }
  catch (Exception ex)
  {
    Console.WriteLine(string.Format("ERROR: {0}",ex.Message));

  }

  // wait for user to mash the keyboard
  Console.ReadLine();
}
```

### 2008-09-08 Download ZIP contains several updates ###
  * SSDSDeploy.exe - bug fix for large file uploads
  * SSDS-Proxy - support for requesting Blob files and returning Authority lists
  * SSDS-Provisioning Client - support for blobs, authority lists, and better login UI

### 2008-08-18 Download ZIP contains new SSDSDeploy.exe console app ###
new SSDSDeploy.exe supports uploading binary files to your SSDS store
```
SSDS Deploy Console (1.0 - 2008-08-18)

POST single binary file:
/{a}/{c}/{e} "[c:][\folder\path\]file.ext" ["mime-type"] [y]
where:
	{a} = authority
	{c} = container
	{e} = entity
	 y  = overwrite existing entities

ex:	/my-auth/files/my-profile "c:\temp\profile.jpg" "image\jpeg" y

POST multiple files using wildcard:
/{a}/{c}/* "[c:][\folder\path\]*.*" [y]

ex:	/my-authority/my-container/* "c:\uploads\*.*" y
	or
ex:	/my-authority/my-container/* "c:\images\*.png" y
```

### 2008-08-10 Download ZIP contains updated Guestbook Demo App ###
includes support for RSS Feeds, improved refresh, and other cool stuff
### 2008-07-31 Download ZIP contains new Guestbook Demo App ###
check out the live [Guestbook Demo](http://amundsen.com/examples/ssds/guestbook-client/)
### 2008-07-21 Download ZIP updated w/ minor tweaks to client UI and server caching ###
### 2008-07-20 Updated Example code including new SSDS.EXE console app ###
The most recent update includes a console client for provisioning against SSDS. There is also a demo script that shows how to use the SSDS.EXE client to script multiple commands against the SSDS server.

```
@echo-off ssds-script.cmd
rem note: "get" is now optional
set authority=my-authority
ssds /%authority%
ssds /%authority%/fish post
ssds /%authority%/fish
ssds /%authority%/fish/ post fish-001.xml
ssds /%authority%/fish/ post fish-002.xml
ssds /%authority%/fish/ 
ssds /%authority%/fish/fish-001 
ssds /%authority%/fish/fish-002 delete
ssds /%authority%/fish/
set authority=
```