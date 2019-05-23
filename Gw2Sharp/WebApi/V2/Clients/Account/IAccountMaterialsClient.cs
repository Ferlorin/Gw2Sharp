using Gw2Sharp.WebApi.V2.Models;

namespace Gw2Sharp.WebApi.V2.Clients
{
    /// <summary>
    /// A client of the Guild Wars 2 API v2 account materials endpoint.
    /// </summary>
    public interface IAccountMaterialsClient :
        IAuthenticatedClient<IApiV2ObjectList<AccountMaterial>>,
        IBlobClient<IApiV2ObjectList<AccountMaterial>>
    {
    }
}
