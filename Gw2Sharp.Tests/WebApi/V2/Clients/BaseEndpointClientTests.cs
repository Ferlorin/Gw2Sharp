using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Gw2Sharp.Extensions;
using Gw2Sharp.Tests.Helpers;
using Gw2Sharp.WebApi;
using Gw2Sharp.WebApi.Caching;
using Gw2Sharp.WebApi.Http;
using Gw2Sharp.WebApi.V2;
using Gw2Sharp.WebApi.V2.Clients;
using Gw2Sharp.WebApi.V2.Models;
using NSubstitute;
using NSubstitute.Core;
using Xunit;

namespace Gw2Sharp.Tests.WebApi.V2.Clients
{
    public abstract class BaseEndpointClientTests<T> where T : IEndpointClient
    {
        protected BaseEndpointClientTests()
        {
            this.Client = this.CreateClient(this.CreateGw2Client());
        }

        public T Client { get; }

        protected virtual bool IsAuthenticated => false;

        protected abstract T CreateClient(IGw2Client gw2Client);


        private IGw2Client CreateGw2Client()
        {
            var cacheMethod = new NullCacheMethod();
            var httpClient = Substitute.For<IHttpClient>();
            var connection = new Connection(this.IsAuthenticated ? "12345678-1234-1234-1234-12345678901234567890-1234-1234-1234-123456789012" : null, Locale.English, cacheMethod: cacheMethod, httpClient: httpClient);
            return new Gw2Client(connection);
        }


        [Fact]
        public void InterfaceConsistencyTest()
        {
            Assert.Equal(this.CheckIfImplementsGenericInterface(this.Client, typeof(IPaginatedClient<>)), this.Client.IsPaginated);
            Assert.Equal(this.Client is IAuthenticatedClient, this.Client.IsAuthenticated);
            Assert.Equal(this.CheckIfImplementsGenericInterface(this.Client, typeof(ILocalizedClient<>)), this.Client.IsLocalized);
            Assert.Equal(this.CheckIfImplementsGenericInterface(this.Client, typeof(IBlobClient<>)), this.Client.HasBlobData);
            Assert.Equal(this.CheckIfImplementsGenericInterface(this.Client, typeof(IAllExpandableClient<>)), this.Client.IsAllExpandable);
            Assert.Equal(this.CheckIfImplementsGenericInterface(this.Client, typeof(IBulkExpandableClient<,>)), this.Client.IsBulkExpandable);
        }

        private bool CheckIfImplementsGenericInterface(IClient client, Type interfaceType) =>
            client.GetType().GetInterfaces()
                .Where(i => i.IsGenericType)
                .Any(i => i.GetGenericTypeDefinition() == interfaceType);


        #region Request Assertions

        protected virtual async Task AssertPaginatedDataAsync<TObject>(IPaginatedClient<TObject> client, string file)
            where TObject : IApiV2Object
        {
            var (data, expected) = this.GetTestData(file);

            ((IClientInternal)this.Client).Connection.HttpClient.RequestAsync(Arg.Any<IHttpRequest>(), CancellationToken.None).Returns(callInfo =>
            {
                this.AssertRequest(callInfo, client, "?page=2&page_size=100");
                this.AssertAuthenticatedRequest(callInfo, client);
                this.AssertLocalizedRequest(callInfo, client);
                this.AssertSchemaVersionRequest(callInfo, client);
                return new HttpResponse(data, HttpStatusCode.OK, null, null);
            });

            var actual = await client.PageAsync(2, 100);
            this.AssertJsonObject(expected.RootElement, actual);
        }

        protected virtual async Task AssertBlobDataAsync<TObject>(IBlobClient<TObject> client, string file)
            where TObject : IApiV2Object
        {
            var (data, expected) = this.GetTestData(file);
            ((IClientInternal)this.Client).Connection.HttpClient.RequestAsync(Arg.Any<IHttpRequest>(), CancellationToken.None).Returns(callInfo =>
            {
                this.AssertRequest(callInfo, client, string.Empty);
                this.AssertAuthenticatedRequest(callInfo, client);
                this.AssertLocalizedRequest(callInfo, client);
                this.AssertSchemaVersionRequest(callInfo, client);
                return new HttpResponse(data, HttpStatusCode.OK, null, null);
            });

            var actual = await client.GetAsync();
            this.AssertJsonObject(expected.RootElement, actual!);
        }

        protected virtual async Task AssertGetDataAsync<TObject, TId>(IBulkExpandableClient<TObject, TId> client, string file, string idName = "id")
            where TObject : IApiV2Object, IIdentifiable<TId>
        {
            var (data, expected) = this.GetTestData(file);
            var id = this.GetId<TId>(expected.RootElement.GetProperty(idName));

            ((IClientInternal)this.Client).Connection.HttpClient.RequestAsync(Arg.Any<IHttpRequest>(), CancellationToken.None).Returns(callInfo =>
            {
                string idString = id!.ToString()!;
                if (id is Guid)
                    idString = idString.ToUpperInvariant();

                this.AssertRequest(callInfo, client, $"/{idString}");
                this.AssertAuthenticatedRequest(callInfo, client);
                this.AssertLocalizedRequest(callInfo, client);
                this.AssertSchemaVersionRequest(callInfo, client);
                return new HttpResponse(data, HttpStatusCode.OK, null, null);
            });

            var actual = await client.GetAsync(id);
            this.AssertJsonObject(expected.RootElement, actual);
        }

        protected virtual async Task AssertAllDataAsync<TObject>(IAllExpandableClient<TObject> client, string file, string idsName = "ids")
            where TObject : IApiV2Object
        {
            var (data, expected) = this.GetTestData(file);
            ((IClientInternal)this.Client).Connection.HttpClient.RequestAsync(Arg.Any<IHttpRequest>(), CancellationToken.None).Returns(callInfo =>
            {
                this.AssertRequest(callInfo, client, $"?{idsName}=all");
                this.AssertAuthenticatedRequest(callInfo, client);
                this.AssertLocalizedRequest(callInfo, client);
                this.AssertSchemaVersionRequest(callInfo, client);
                return new HttpResponse(data, HttpStatusCode.OK, null, null);
            });

            var actual = await client.AllAsync();
            this.AssertJsonObject(expected.RootElement, actual);
        }

        protected virtual async Task AssertBulkDataAsync<TObject, TId>(IBulkExpandableClient<TObject, TId> client, string file, string idName = "id", string idsName = "ids")
            where TObject : IApiV2Object, IIdentifiable<TId>
        {
            var (data, expected) = this.GetTestData(file);
            var ids = this.GetIds<TId>(expected.RootElement.EnumerateArray().Select(i => i.GetProperty(idName)));

            ((IClientInternal)this.Client).Connection.HttpClient.RequestAsync(Arg.Any<IHttpRequest>(), CancellationToken.None).Returns(callInfo =>
            {
                var idStrings = ids.Select(i =>
                {
                    string idString = i!.ToString()!;
                    if (i is Guid)
                        idString = idString.ToUpperInvariant();
                    return idString;
                });

                this.AssertRequest(callInfo, client, $"?{idsName}={string.Join(",", idStrings)}");
                this.AssertAuthenticatedRequest(callInfo, client);
                this.AssertLocalizedRequest(callInfo, client);
                this.AssertSchemaVersionRequest(callInfo, client);
                return new HttpResponse(data, HttpStatusCode.OK, null, null);
            });

            var actual = await client.ManyAsync(ids);
            this.AssertJsonObject(expected.RootElement, actual);
        }

        protected virtual async Task AssertIdsDataAsync<TObject, TId>(IBulkExpandableClient<TObject, TId> client, string file)
            where TObject : IApiV2Object, IIdentifiable<TId>
        {
            var (data, expected) = this.GetTestData(file);

            ((IClientInternal)this.Client).Connection.HttpClient.RequestAsync(Arg.Any<IHttpRequest>(), CancellationToken.None).Returns(callInfo =>
            {
                this.AssertRequest(callInfo, client, string.Empty);
                this.AssertAuthenticatedRequest(callInfo, client);
                this.AssertLocalizedRequest(callInfo, client);
                this.AssertSchemaVersionRequest(callInfo, client);
                return new HttpResponse(data, HttpStatusCode.OK, null, null);
            });

            var actual = await client.IdsAsync();
            this.AssertJsonObject(expected.RootElement, actual);
        }

        protected virtual void AssertRequest(CallInfo callInfo, IEndpointClient client, string pathAndQuery)
        {
            // Format the URI to how it's supposed to be
            var uri = new Uri(Gw2WebApiV2Client.UrlBase, $"{client.EndpointPath}{pathAndQuery}");
            var parameterProperties = client.GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.GetCustomAttribute<EndpointPathParameterAttribute>() != null);
            if (parameterProperties.Any())
            {
                var builder = new UriBuilder(uri);
                foreach (var parameter in parameterProperties)
                {
                    var attr = parameter.GetCustomAttribute<EndpointPathParameterAttribute>();
                    object value = parameter.GetValue(client);
                    if (value == null)
                        continue;

                    string toAppend = $"{attr.ParameterName}={value}";
                    builder.Query = builder.Query != null && builder.Query.Length > 1
                        ? $"{builder.Query.Substring(1)}&{toAppend}"
                        : toAppend;
                }
                uri = builder.Uri;
            }

            Assert.Equal(uri, callInfo.ArgAt<IHttpRequest>(0).Url);
            var requestHeaders = callInfo.ArgAt<IHttpRequest>(0).RequestHeaders;
            Assert.Contains(new KeyValuePair<string, string>("Accept", "application/json"), requestHeaders);
            Assert.Contains(new KeyValuePair<string, string>("User-Agent", ((IClientInternal)client).Connection.UserAgent), requestHeaders);
        }

        protected virtual void AssertAuthenticatedRequest(CallInfo callInfo, IEndpointClient client)
        {
            var requestHeaders = callInfo.ArgAt<IHttpRequest>(0).RequestHeaders;
            if (client.IsAuthenticated)
                Assert.Contains(new KeyValuePair<string, string>("Authorization", $"Bearer {((IClientInternal)client).Connection.AccessToken}"), requestHeaders);
            else
                Assert.DoesNotContain(requestHeaders, h => h.Key == "Authorization");
        }

        protected virtual void AssertLocalizedRequest(CallInfo callInfo, IEndpointClient client)
        {
            var requestHeaders = callInfo.ArgAt<IHttpRequest>(0).RequestHeaders;
            Assert.Contains(new KeyValuePair<string, string>("Accept-Language", ((IClientInternal)client).Connection.LocaleString), requestHeaders);
        }

        protected virtual void AssertSchemaVersionRequest(CallInfo callInfo, IEndpointClient client)
        {
            var requestHeaders = callInfo.ArgAt<IHttpRequest>(0).RequestHeaders;
            if (!string.IsNullOrWhiteSpace(client.SchemaVersion))
                Assert.Contains(new KeyValuePair<string, string>("X-Schema-Version", client.SchemaVersion), requestHeaders);
            else
                Assert.DoesNotContain(requestHeaders, h => h.Key == "X-Schema-Version");
        }


        protected (string, JsonDocument) GetTestData(string fileResourceName)
        {
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"Gw2Sharp.Tests.{fileResourceName}"))
            {
                if (stream == null)
                    throw new FileNotFoundException($"Resource {fileResourceName} does not exist");
                using (var reader = new StreamReader(stream))
                {
                    string data = reader.ReadToEnd();
                    return (data, JsonDocument.Parse(data));
                }
            }
        }

        protected TObject GetId<TObject>(JsonElement id) =>
            typeof(TObject) == typeof(Guid) ? (TObject)(object)id.GetGuid() : JsonSerializer.Deserialize<TObject>(id.GetRawText());

        protected IEnumerable<TObject> GetIds<TObject>(IEnumerable<JsonElement> ids) =>
            typeof(TObject) == typeof(Guid) ? ids.Select(i => i.GetGuid()).Cast<TObject>() : ids.Select(i => JsonSerializer.Deserialize<TObject>(i.GetRawText()));


        protected void AssertJsonObject(JsonElement expected, object actual)
        {
            switch (expected.ValueKind)
            {
                case JsonValueKind.Object:
                    this.AssertJsonObject(expected.EnumerateObject(), actual);
                    break;
                case JsonValueKind.Array:
                    this.AssertJsonObject(expected.EnumerateArray(), actual);
                    break;
                default:
                    this.AssertJsonValue(expected, actual);
                    break;
            }
        }

        protected void AssertJsonObject(JsonElement.ObjectEnumerator expected, object actual)
        {
            var type = actual.GetType();
            if (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(ReadOnlyDictionary<,>) || type.GetGenericTypeDefinition() == typeof(Dictionary<,>)))
            {
                // Dictionary
                var keyType = type.GetGenericArguments()[0];
                var dic = (dynamic)actual;
                foreach (var kvp in expected)
                {
                    dynamic key;
                    if (keyType == typeof(string))
                    {
                        key = kvp.Name;
                    }
                    else
                    {
                        string keyString = string.Concat(kvp.Name.Split('_').Select(s => string.Concat(
                            s[0].ToString().ToUpper(),
                            s.Substring(1))));
                        key = Convert.ChangeType(keyString, keyType);
                        if (key == null)
                            throw new InvalidOperationException($"Expected property '{keyString}' to exist in type {type.FullName}");
                    }
                    var actualValue = dic[key];
                    this.AssertJsonObject(kvp.Value, actualValue);
                }
            }
            else
            {
                // Specific object
                foreach (var kvp in expected)
                {
                    string key = string.Concat(kvp.Name.Split('_').Select(s => string.Concat(
                        s[0].ToString().ToUpper(),
                        s.Substring(1))));
                    var property = type.GetProperty(key);
                    if (property == null)
                        throw new InvalidOperationException($"Expected property '{key}' to exist in type {type.FullName}");
                    object actualValue = property.GetValue(actual);
                    this.AssertJsonObject(kvp.Value, actualValue);
                }
            }
        }

        protected void AssertJsonObject(JsonElement.ArrayEnumerator expected, object actual)
        {
            var actualList = (actual as IEnumerable)?.Cast<object>().ToList();
            if (actualList is null && actual.GetType().GetGenericTypeDefinition() == typeof(KeyValuePair<,>))
            {
                // Cast KeyValuePair to a list
                dynamic kvp = actual;
                actualList = new List<object> { kvp.Key, kvp.Value };
            }
            if (actualList is null)
                throw new InvalidOperationException($"Expected an object that's castable to an enumerable for {expected}");

            var expectedList = expected.ToList();
            for (int i = 0; i < expectedList.Count; i++)
                this.AssertJsonObject(expectedList[i], actualList[i]);
        }

        protected void AssertJsonValue(JsonElement expected, object actual)
        {
            bool switched = true;
            switch (actual)
            {
                case Guid guid:
                    Assert.Equal(new Guid(expected.GetString()), guid);
                    break;
                case DateTime dateTime:
                    Assert.Equal(expected.GetDateTime(), dateTime);
                    break;
                case DateTimeOffset dateTime:
                    Assert.Equal(expected.GetDateTimeOffset(), dateTime);
                    break;
                case TimeSpan timeSpan:
                    Assert.Equal(TimeSpan.FromSeconds(expected.GetInt32()), timeSpan);
                    break;
                case int @int:
                    if (expected.ValueKind == JsonValueKind.String)
                        Assert.Equal(int.Parse(expected.GetString()), @int);
                    else
                        Assert.Equal(expected.GetInt32(), @int);
                    break;
                case long @long:
                    if (expected.ValueKind == JsonValueKind.String)
                        Assert.Equal(long.Parse(expected.GetString()), @long);
                    else
                        Assert.Equal(expected.GetInt64(), @long);
                    break;
                case double @double:
                    Assert.Equal(expected.GetDouble(), @double, 10);
                    break;
                case bool @bool:
                    Assert.Equal(expected.GetBoolean(), @bool);
                    break;
                case string @string:
                    Assert.Equal(expected.GetString(), @string);
                    break;
                case null:
                    if (expected.ValueKind == JsonValueKind.String)
                        Assert.Equal(expected.GetString(), string.Empty);
                    break;
                default:
                    switched = false;
                    break;
            }

            if (!switched)
            {
                var typeInfo = actual!.GetType().GetTypeInfo();
                if (typeInfo.IsGenericType && typeInfo.GetGenericTypeDefinition() == typeof(ApiEnum<>))
                {
                    var enumType = typeInfo.GenericTypeArguments[0];
                    dynamic @enum = actual; // Just for easiness

                    Assert.Equal(expected.GetString(), @enum.RawValue);
                    if (@enum.IsUnknown)
                    {
                        var enumNames = Enum.GetNames(enumType).Select(x => x.Replace("_", ""));
                        Assert.True(enumNames.Contains((string)@enum.RawValue, StringComparer.OrdinalIgnoreCase), $"Expected '{expected}' to be a value in enumerator {@enum.Value.GetType().FullName}; detected value '{@enum.Value}'");
                    }
                    Assert.Equal(expected.GetString().ParseEnum(enumType), @enum.Value);

                    switched = true;
                }
            }

            if (!switched)
                Assert.Equal(expected.GetString(), actual!.ToString());
        }

        #endregion


        #region ArgumentNullException tests

        [Fact]
        public virtual void ArgumentNullConstructorTest()
        {
            AssertArguments.ThrowsWhenNullConstructor(
                this.Client.GetType(),
                new[] { typeof(IConnection), typeof(IGw2Client) },
                new object[] { new Connection(), new Gw2Client() },
                new[] { true, true });
        }

        #endregion
    }
}
