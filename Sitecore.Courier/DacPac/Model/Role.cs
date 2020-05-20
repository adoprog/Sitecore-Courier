using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sitecore.Courier.Sql.Model
{
  public class Role
  {
    public IEnumerable<string> Membership
    {
      get;
      set;
    }

    public string Name
    {
      get;
      set;
    }

    public Role()
    {
    }
  }
}
