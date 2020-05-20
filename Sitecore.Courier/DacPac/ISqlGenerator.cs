using Sitecore.Courier.Sql.Model;

namespace Sitecore.Courier.DacPac
{
  public interface ISqlGenerator
  {
    string GenerateAddBlobStatements(Blob blob);

    string GenerateAddItemStatements(Item any);

    string GenerateAddRoleStatements(Role role);

    string GenerateAppendStatements();

    string GeneratePrependStatements();
  }
}
