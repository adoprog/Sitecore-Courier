using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sitecore.Courier.Sql.Model
{
  public class Field
  {
    public FieldType FieldType
    {
      get;
      set;
    }

    public string Id
    {
      get;
      set;
    }

    public string Value
    {
      get;
      set;
    }

    public Field()
    {
    }
  }
}
