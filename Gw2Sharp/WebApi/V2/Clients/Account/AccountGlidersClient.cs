using System;

namespace Gw2Sharp.WebApi.V2.Clients
{
    /// <summary>
    /// A client of the Guild Wars 2 API v2 account gliders endpoint.
    /// </summary>
    [EndpointPath("account/gliders")]
    public class AccountGlidersClient : BaseEndpointBlobClient<IApiV2ObjectList<int>>, IAccountGlidersClient
    {
        /// <summary>
        /// Creates a new <see cref="AccountGlidersClient"/> that is used for the API v2 account gliders endpoint.
        /// </summary>
        /// <param name="connection">The connection used to make requests, see <see cref="IConnection"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="connection"/> is <c>null</c>.</exception>
        public AccountGlidersClient(IConnection connection) :
            base(connection)
        { }
    }
}
