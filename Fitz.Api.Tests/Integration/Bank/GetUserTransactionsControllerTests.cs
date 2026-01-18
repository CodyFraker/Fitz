using Fitz.Api.Tests;
using Fitz.Database;
using Fitz.Database.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Xunit;

namespace Fitz.Api.Tests.Integration.Bank
{
    public class GetUserTransactionsControllerTests : IClassFixture<WebApplicationFactory<Fitz.Api.Program>>
    {
        private readonly WebApplicationFactory<Fitz.Api.Program> _factory;
        private readonly HttpClient _client;
        private readonly ulong _testUserId = Users.Spy;

        public GetUserTransactionsControllerTests(WebApplicationFactory<Fitz.Api.Program> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<BotContext>));
                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }

                    services.AddDbContext<BotContext>(options =>
                    {
                        options.UseInMemoryDatabase("GetUserTransactionsTestDb");
                    });

                    Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");
                });
            });

            _client = _factory.CreateClient();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "test-token");
        }

        [Fact]
        public async Task GetUserTransactions_WithValidAuth_ReturnsOk()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<BotContext>();
            
            var transaction = new Transaction
            {
                Sender = _testUserId,
                Recipient = 987654321,
                Amount = 50,
                Reason = Reason.Bonus,
                Timestamp = DateTime.UtcNow
            };
            
            db.Transactions.Add(transaction);
            await db.SaveChangesAsync();

            var response = await _client.GetAsync($"/api/bank/transactions/{_testUserId}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<Fitz.Api.Models.Responses.ApiResponse<Fitz.Api.Models.Responses.TransactionsResponse>>(
                content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.NotNull(apiResponse);
            Assert.True(apiResponse.Success);
            Assert.NotNull(apiResponse.Data);
            Assert.Equal(1, apiResponse.Data.Transactions.Count);
            Assert.Equal(1, apiResponse.Data.TotalCount);
        }

        [Fact]
        public async Task GetUserTransactions_WithoutAuth_ReturnsUnauthorized()
        {
            var clientWithoutAuth = _factory.CreateClient();
            var response = await clientWithoutAuth.GetAsync($"/api/bank/transactions/{_testUserId}");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task GetUserTransactions_WithDifferentUserId_ReturnsForbidden()
        {
            var differentUserId = 999999999UL;
            var response = await _client.GetAsync($"/api/bank/transactions/{differentUserId}");

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task GetUserTransactions_WithPagination_ReturnsCorrectPage()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<BotContext>();
            
            for (int i = 1; i <= 15; i++)
            {
                var transaction = new Transaction
                {
                    Sender = _testUserId,
                    Recipient = (ulong)(100000000 + i),
                    Amount = 10 + i,
                    Reason = Reason.Bonus,
                    Timestamp = DateTime.UtcNow.AddMinutes(-i)
                };
                db.Transactions.Add(transaction);
            }
            
            await db.SaveChangesAsync();

            var response = await _client.GetAsync($"/api/bank/transactions/{_testUserId}?skip=5&take=5");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<Fitz.Api.Models.Responses.ApiResponse<Fitz.Api.Models.Responses.TransactionsResponse>>(
                content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.NotNull(apiResponse);
            Assert.True(apiResponse.Success);
            Assert.NotNull(apiResponse.Data);
            Assert.Equal(5, apiResponse.Data.Transactions.Count);
            Assert.Equal(15, apiResponse.Data.TotalCount);
            Assert.Equal(5, apiResponse.Data.Skip);
            Assert.Equal(5, apiResponse.Data.Take);
        }

        [Fact]
        public async Task GetUserTransactions_WithDefaultPagination_ReturnsFirstPage()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<BotContext>();
            
            for (int i = 1; i <= 25; i++)
            {
                var transaction = new Transaction
                {
                    Sender = _testUserId,
                    Recipient = (ulong)(100000000 + i),
                    Amount = 10 + i,
                    Reason = Reason.Donated,
                    Timestamp = DateTime.UtcNow.AddMinutes(-i)
                };
                db.Transactions.Add(transaction);
            }
            
            await db.SaveChangesAsync();

            var response = await _client.GetAsync($"/api/bank/transactions/{_testUserId}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<Fitz.Api.Models.Responses.ApiResponse<Fitz.Api.Models.Responses.TransactionsResponse>>(
                content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.NotNull(apiResponse);
            Assert.True(apiResponse.Success);
            Assert.NotNull(apiResponse.Data);
            Assert.Equal(10, apiResponse.Data.Transactions.Count);
            Assert.Equal(25, apiResponse.Data.TotalCount);
            Assert.Equal(0, apiResponse.Data.Skip);
            Assert.Equal(10, apiResponse.Data.Take);
        }

        [Fact]
        public async Task GetUserTransactions_WithInvalidTake_ReturnsBadRequest()
        {
            var response1 = await _client.GetAsync($"/api/bank/transactions/{_testUserId}?take=0");
            var response2 = await _client.GetAsync($"/api/bank/transactions/{_testUserId}?take=101");

            Assert.Equal(HttpStatusCode.BadRequest, response1.StatusCode);
            Assert.Equal(HttpStatusCode.BadRequest, response2.StatusCode);
        }

        [Fact]
        public async Task GetUserTransactions_WithInvalidSkip_ReturnsBadRequest()
        {
            var response = await _client.GetAsync($"/api/bank/transactions/{_testUserId}?skip=-1");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetUserTransactions_IncludesBothSentAndReceived()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<BotContext>();
            
            var sentTransaction = new Transaction
            {
                Sender = _testUserId,
                Recipient = 111111111,
                Amount = 50,
                Reason = Reason.Donated,
                Timestamp = DateTime.UtcNow
            };
            
            var receivedTransaction = new Transaction
            {
                Sender = 222222222,
                Recipient = _testUserId,
                Amount = 100,
                Reason = Reason.Bonus,
                Timestamp = DateTime.UtcNow.AddMinutes(-1)
            };
            
            db.Transactions.Add(sentTransaction);
            db.Transactions.Add(receivedTransaction);
            await db.SaveChangesAsync();

            var response = await _client.GetAsync($"/api/bank/transactions/{_testUserId}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<Fitz.Api.Models.Responses.ApiResponse<Fitz.Api.Models.Responses.TransactionsResponse>>(
                content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.NotNull(apiResponse);
            Assert.True(apiResponse.Success);
            Assert.NotNull(apiResponse.Data);
            Assert.Equal(2, apiResponse.Data.Transactions.Count);
            Assert.Equal(2, apiResponse.Data.TotalCount);
        }
    }
}
