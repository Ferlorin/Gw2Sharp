using System;
using Gw2Sharp.WebApi.V2.Models;

namespace Gw2Sharp.WebApi.V2.Clients
{
    /// <summary>
    /// A client of the Guild Wars 2 API v2 account build storage endpoint.
    /// </summary>
    [EndpointPath("account/buildstorage")]
    [EndpointSchemaVersion("2019-12-19T00:00:00.000Z")]
    public class AccountBuildStorageClient : BaseEndpointBulkAllClient<AccountBuildStorageSlot, int>, IAccountBuildStorageClient
    {
        /// <summary>
        /// Creates a new <see cref="AccountBuildStorageClient"/> that is used for the API v2 account build storage endpoint.
        /// </summary>
        /// <param name="connection">The connection used to make requests, see <see cref="IConnection"/>.</param>
        /// <param name="gw2Client">The Guild Wars 2 client.</param>
        /// <exception cref="ArgumentNullException"><paramref name="connection"/> or <paramref name="gw2Client"/> is <see langword="null"/>.</exception>
        protected internal AccountBuildStorageClient(IConnection connection, IGw2Client gw2Client) :
            base(connection, gw2Client)
        { }
    }
}
