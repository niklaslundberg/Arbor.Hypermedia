using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Xunit;
using Xunit.Abstractions;

namespace Arbor.Hypermedia.Tests
{
    public sealed class HyperMediaTest : IDisposable
    {
        private readonly TestServer _server;
        private readonly ITestOutputHelper _testOutputHelper;

        public HyperMediaTest(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;

            var webBuilder = new WebHostBuilder()
                .UseStartup<TestStartup>();

            _server = new TestServer(webBuilder);
        }

        public void Dispose() => _server.Dispose();

        [Fact]
        public async Task MakeRequestForTodo()
        {
            var client = _server.CreateClient();
            using var request = new HttpRequestMessage(HttpMethod.Get, "/todo/1");
            using var response = await client.SendAsync(request);

            _testOutputHelper.WriteLine(await response.Content.ReadAsStringAsync());

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task MakeRequestForDone()
        {
            var client = _server.CreateClient();
            using var request = new HttpRequestMessage(HttpMethod.Get, "/todo/2");
            using var response = await client.SendAsync(request);

            _testOutputHelper.WriteLine(await response.Content.ReadAsStringAsync());

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task MakeRequestForNonExisting()
        {
            var client = _server.CreateClient();
            using var request = new HttpRequestMessage(HttpMethod.Get, "/todo/blah");
            using var response = await client.SendAsync(request);

            _testOutputHelper.WriteLine(await response.Content.ReadAsStringAsync());

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}