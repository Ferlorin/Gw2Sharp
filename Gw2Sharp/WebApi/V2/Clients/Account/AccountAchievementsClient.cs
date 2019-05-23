using System;
using Gw2Sharp.WebApi.V2.Models;

namespace Gw2Sharp.WebApi.V2.Clients
{
    /// <summary>
    /// A client of the Guild Wars 2 API v2 account achievements endpoint.
    /// </summary>
    [EndpointPath("account/achievements")]
    public class AccountAchievementsClient : BaseEndpointBlobClient<IApiV2ObjectList<AccountAchievement>>, IAccountAchievementsClient
    {
        /// <summary>
        /// Creates a new <see cref="AccountAchievementsClient"/> that is used for the API v2 account achievements endpoint.
        /// </summary>
        /// <param name="connection">The connection used to make requests, see <see cref="IConnection"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="connection"/> is <c>null</c>.</exception>
        public AccountAchievementsClient(IConnection connection) :
            base(connection)
        { }
    }
}
