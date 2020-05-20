using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sitecore.Courier.Sql.Model
{
  public class Item
  {
    public string Database
    {
      get;
      set;
    }

    public IEnumerable<Field> Fields
    {
      get;
      set;
    }

    public string Id
    {
      get;
      set;
    }

    public string Key
    {
      get;
      set;
    }

    public string Language
    {
      get;
      set;
    }

    public string MasterId
    {
      get;
      set;
    }

    public string Name
    {
      get;
      set;
    }

    public string ParentId
    {
      get;
      set;
    }

    public string SortOrder
    {
      get;
      set;
    }

    public string TemplateId
    {
      get;
      set;
    }

    public string Version
    {
      get;
      set;
    }

    public Item()
    {
      this.Fields = new List<Field>();
    }
  }
}
