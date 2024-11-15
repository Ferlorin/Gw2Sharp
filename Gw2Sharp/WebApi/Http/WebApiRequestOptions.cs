using System;
using System.Collections.Generic;
using System.Linq;
using Gw2Sharp.Extensions;
using Gw2Sharp.WebApi.V2.Clients;

namespace Gw2Sharp.WebApi.Http
{
    /// <summary>
    /// A web request.
    /// </summary>
    public class WebApiRequestOptions
    {
        /// <summary>
        /// The web API base url.
        /// </summary>
        public string BaseUrl { get; set; } = string.Empty;

        /// <summary>
        /// The web API endpoint path.
        /// </summary>
        public string EndpointPath { get; set; } = string.Empty;

        /// <summary>
        /// The path suffix, a.k.a. used when an additional path string is appended to the endpoint path.
        /// </summary>
        public string PathSuffix { get; set; } = string.Empty;

#pragma warning disable CA2227 // Collection properties should be read only
        /// <summary>
        /// The web API endpoint query parameters.
        /// </summary>
        public IDictionary<string, string> EndpointQuery { get; set; } = new Dictionary<string, string>();
#pragma warning restore CA2227 // Collection properties should be read only

        /// <summary>
        /// The name of the query parameter that acts as the id.
        /// </summary>
        public string BulkQueryParameterIdName { get; set; } = EndpointBulkIdNameAttribute.DEFAULT_PARAM_ID;

        /// <summary>
        /// The name of the query parameter that acts as the ids.
        /// </summary>
        public string BulkQueryParameterIdsName { get; set; } = EndpointBulkIdNameAttribute.DEFAULT_PARAM_IDS;

        /// <summary>
        /// The name of the property in the object returned by the API that acts as the id.
        /// </summary>
        public string BulkObjectIdName { get; set; } = EndpointBulkIdNameAttribute.DEFAULT_OBJECT_ID;

        /// <summary>
        /// The URL.
        /// </summary>
        public Uri Url
        {
            get
            {
                var builder = new UriBuilder(this.BaseUrl);
                builder.Path += this.EndpointPath;
                if (!string.IsNullOrEmpty(this.PathSuffix))
                    builder.Path += !this.PathSuffix.StartsWith("/", StringComparison.InvariantCulture) ? "/" : string.Empty + this.PathSuffix;
                builder.Query = string.Join("&", this.EndpointQuery.Select(x => $"{Uri.EscapeDataString(x.Key)}{(x.Value != null ? $"={Uri.EscapeDataString(x.Value)}" : "")}"));
                return builder.Uri;
            }
        }

        /// <summary>
        /// The URL without the authentication parameter.
        /// </summary>
        public Uri UrlWithoutAuth
        {
            get
            {
                var builder = new UriBuilder(this.BaseUrl);
                builder.Path += this.EndpointPath;
                if (!string.IsNullOrEmpty(this.PathSuffix))
                    builder.Path += !this.PathSuffix.StartsWith("/", StringComparison.InvariantCulture) ? "/" : string.Empty + this.PathSuffix;
                builder.Query = string.Join("&", this.EndpointQuery.Where(x => x.Key != "access_token").Select(x => $"{Uri.EscapeDataString(x.Key)}{(x.Value != null ? $"={Uri.EscapeDataString(x.Value)}" : "")}"));
                return builder.Uri;
            }
        }

#pragma warning disable CA2227 // Collection properties should be read only
        /// <summary>
        /// The request headers to use in the web request.
        /// </summary>
        public IDictionary<string, string> RequestHeaders { get; set; } = new Dictionary<string, string>();
#pragma warning restore CA2227 // Collection properties should be read only


        /// <summary>
        /// Performs a deep clone of the request.
        /// </summary>
        /// <returns>The cloned request.</returns>
        public WebApiRequestOptions DeepCopy() => new WebApiRequestOptions
        {
            BaseUrl = this.BaseUrl,
            EndpointPath = this.EndpointPath,
            PathSuffix = this.PathSuffix,
            EndpointQuery = this.EndpointQuery.ShallowCopy(),
            BulkQueryParameterIdName = this.BulkQueryParameterIdName,
            BulkQueryParameterIdsName = this.BulkQueryParameterIdsName,
            BulkObjectIdName = this.BulkObjectIdName,
            RequestHeaders = this.RequestHeaders.ShallowCopy()
        };
    }
}
