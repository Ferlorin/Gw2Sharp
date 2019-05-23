using System;

namespace Gw2Sharp.WebApi.V2.Clients
{
    /// <summary>
    /// A client of the Guild Wars 2 API v2 account outfits endpoint.
    /// </summary>
    [EndpointPath("account/outfits")]
    public class AccountOutfitsClient : BaseEndpointBlobClient<IApiV2ObjectList<int>>, IAccountOutfitsClient
    {
        /// <summary>
        /// Creates a new <see cref="AccountOutfitsClient"/> that is used for the API v2 account outfits endpoint.
        /// </summary>
        /// <param name="connection">The connection used to make requests, see <see cref="IConnection"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="connection"/> is <c>null</c>.</exception>
        public AccountOutfitsClient(IConnection connection) :
            base(connection)
        { }
    }
}
