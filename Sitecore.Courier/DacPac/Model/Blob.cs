using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sitecore.Courier.Sql.Model
{
  public class Blob
  {
    public byte[] Data
    {
      get;
      set;
    }

    public string Database
    {
      get;
      set;
    }

    public string Id
    {
      get;
      set;
    }

    public Blob()
    {
    }
  }
}
