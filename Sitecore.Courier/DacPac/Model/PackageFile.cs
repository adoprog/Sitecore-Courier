using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sitecore.Courier.Sql.Model
{
  public class PackageFile
  {
    public byte[] Content
    {
      get;
      set;
    }

    public string FileName
    {
      get;
      set;
    }

    public PackageFile()
    {
    }
  }
}
