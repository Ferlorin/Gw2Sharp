using System;
using System.Collections.Generic;
using System.Net;
using FluentAssertions;
using Gw2Sharp.WebApi.Http;
using Gw2Sharp.WebApi.V2.Models;
using Xunit;

namespace Gw2Sharp.Tests.WebApi.Http
{
    public class TooManyRequestsExceptionTests
    {
        [Fact]
        public void SerializableTest()
        {
            var request = new HttpRequest(new Uri("http://localhost"), new Dictionary<string, string> { { "hello", "tyria" } });
#if NET461
            var response = new HttpResponse<ErrorObject>(new ErrorObject { Text = "Error" }, (HttpStatusCode)429, null, null);
#else
            var response = new HttpResponse<ErrorObject>(new ErrorObject { Text = "Error" }, HttpStatusCode.TooManyRequests, null, null);
#endif
            var exception = new TooManyRequestsException(request, response);
            exception.Should().BeBinarySerializable();
        }
    }
}
