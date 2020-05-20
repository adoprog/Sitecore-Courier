using System;
using Sitecore.Update;

namespace Sitecore.Courier.DacPac
{
  public class Converter : IDisposable
  {
    public DiffInfo Diff
    {
      get;
    }

    public ISqlWriter Writer
    {
      get;
    }

    public Converter(DiffInfo diff, ISqlWriter writer)
    {
      this.Diff = diff;
      this.Writer = writer;
    }

    public void Dispose()
    {
      this.Diff.Dispose();
      this.Writer.Dispose();
    }
  }
}
