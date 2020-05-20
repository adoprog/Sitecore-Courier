using System;
using Sitecore.Courier.Sql.Model;

namespace Sitecore.Courier.DacPac
{
  public interface ISqlWriter : IDisposable
  {
    void WriteItem(Item item);
  }
}
