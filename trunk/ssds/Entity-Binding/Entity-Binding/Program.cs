using System;
using System.IO;
using System.Text;
using Amundsen.Utilities;

// 2008-10-13 (mca) : simple example of strongly-typed datasets for SSDS
//
// assumptions:
// - use XSD.exe to generate .cs files for each entity + sitka.xsd
// - add generated entity class to this project
// - replace custom URL w/ direct all to SSDS servers (w/ auth)
//
// other cool stuff:
// - use filters/ordering on the dataset
// - use this dataset in a windows data-binding scenario
// - work out details of dataset changes (add,update,delete) and write-back to SSDS
// 
namespace Amundsen.SSDS.Binding
{
  class Program
  {
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
          Console.WriteLine(string.Format("{0}: {1}, {2}, {3}, {4}", ++x, r.Id, r.name, r._is_completed, r.Version));
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine(string.Format("ERROR: {0}",ex.Message));

      }

      // wait for user to mash the keyboard
      Console.ReadLine();
    }
  }
}
